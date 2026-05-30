using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace SkiaSharp.Benchmarks;

// Microbenchmark for the lock-striped HandleDictionary (experiment #4101 / PR #4102).
//
// Each BenchmarkDotNet job runs in its OWN process with SKIASHARP_HANDLE_SHARDS pinned, so
// HandleDictionary.ShardCount (resolved once at static init) takes that job's value. This lets us
// compare the original single-lock design (Shards=1) against N-way striping running the exact same
// code path, with thread count as a parameter to locate the scaling knee.
//
// Two workloads:
//   ChurnDistinct - every thread constructs + disposes DISTINCT handles. Their registrations and
//                   removals hash onto different shards, so striping can run them in parallel. This
//                   is the case striping is meant to help: concurrent create/teardown of unrelated
//                   objects (e.g. many threads decoding images or building paths).
//   SharedLookup  - every thread repeatedly resolves a small SHARED set of already-registered
//                   handles (the dedup / cache-hit path under the upgradeable-read lock). A handful
//                   of handles map to a handful of shards, so striping cannot spread the contention.
//                   Included as an honest counter-signal: it shows where striping does NOT help (and
//                   where the extra locks can even bounce cache lines).
//
// The wrapper (BenchObject) is non-owning and backed by a synthetic handle, so no native memory is
// touched: the benchmark isolates registry lock contention rather than Skia allocation cost. A real
// workload dilutes the win with native ctor/dtor cost, so treat these numbers as the upper bound on
// striping's benefit, not a typical application speedup.
[MemoryDiagnoser(false)]
[Config(typeof(Config))]
public class HandleDictionaryBenchmark
{
	// Registry operations per measured invocation, split evenly across the worker threads. Constant
	// (required by OperationsPerInvoke) so ns/op is directly comparable across thread counts.
	private const int TotalOps = 1 << 20;

	public sealed class Config : ManualConfig
	{
		public Config()
		{
			foreach (var shards in new[] { 1, 2, 4, 8, 16 }) {
				AddJob(Job.Default
					.WithEnvironmentVariable("SKIASHARP_HANDLE_SHARDS", shards.ToString())
					.WithWarmupCount(3)
					.WithIterationCount(7)
					.WithId($"Shards={shards}"));
			}
		}
	}

	// A non-owning wrapper over a synthetic handle: DisposeNative is a no-op, so the only work it
	// performs is registering with / deregistering from HandleDictionary.
	private sealed class BenchObject : SKObject
	{
		public BenchObject(IntPtr handle, bool owns)
			: base(handle, owns)
		{
		}

		protected override void DisposeNative()
		{
			// no native object backs this synthetic handle
		}
	}

	private static readonly Func<IntPtr, bool, BenchObject> Factory = (h, owns) => new BenchObject(h, owns);

	[Params(1, 4, 8, 12, 16)]
	public int Threads;

	private IntPtr[] churnHandles;       // TotalOps distinct handles for the churn workload
	private IntPtr[] sharedHandles;      // small shared set for the lookup workload
	private BenchObject[] sharedObjects; // strong refs so the shared wrappers stay alive in the registry
	private long handleSeed = 0x6000_0000_0000_0000L;

	// Synthetic handle values well away from any real native pointer range.
	private IntPtr NextHandle() => new IntPtr(Interlocked.Add(ref handleSeed, 0x1000));

	[GlobalSetup]
	public void Setup()
	{
		churnHandles = new IntPtr[TotalOps];
		for (var i = 0; i < TotalOps; i++)
			churnHandles[i] = NextHandle();

		const int sharedCount = 8;
		sharedHandles = new IntPtr[sharedCount];
		sharedObjects = new BenchObject[sharedCount];
		for (var i = 0; i < sharedCount; i++) {
			sharedHandles[i] = NextHandle();
			sharedObjects[i] = HandleDictionary.GetOrAddObject(sharedHandles[i], false, false, Factory);
		}
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		if (sharedObjects == null)
			return;
		foreach (var o in sharedObjects)
			o?.Dispose();
	}

	// Runs `work` on `Threads` dedicated threads, each handling a contiguous slice of [0, TotalOps),
	// released together so the measured window is the parallel section (not thread spin-up).
	private void RunParallel(Action<int, int> work)
	{
		var threads = new Thread[Threads];
		var start = new ManualResetEventSlim(false);
		var chunk = TotalOps / Threads;

		for (var t = 0; t < Threads; t++) {
			var lo = t * chunk;
			var hi = (t == Threads - 1) ? TotalOps : lo + chunk;
			threads[t] = new Thread(() => {
				start.Wait();
				work(lo, hi);
			}) { IsBackground = true };
			threads[t].Start();
		}

		start.Set();
		foreach (var th in threads)
			th.Join();

		start.Dispose();
	}

	// Striping's target case: distinct handles construct + dispose in parallel across shards.
	[Benchmark(OperationsPerInvoke = TotalOps)]
	public void ChurnDistinct()
	{
		RunParallel((lo, hi) => {
			for (var i = lo; i < hi; i++) {
				var obj = HandleDictionary.GetOrAddObject(churnHandles[i], false, false, Factory);
				obj.Dispose();
			}
		});
	}

	// Counter-signal: all threads resolve a few shared handles via the dedup path; contention cannot
	// be spread because only a few shards are touched. The returned instances are the live shared
	// wrappers and must NOT be disposed.
	[Benchmark(OperationsPerInvoke = TotalOps)]
	public void SharedLookup()
	{
		var shared = sharedHandles;
		var mask = shared.Length - 1;
		RunParallel((lo, hi) => {
			for (var i = lo; i < hi; i++)
				_ = HandleDictionary.GetOrAddObject(shared[i & mask], false, false, Factory);
		});
	}
}

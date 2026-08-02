using System;
using System.Collections.Generic;
using System.Threading;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// Measures the managed overhead HandleDictionary imposes on EVERY native-object wrap.
//
// GetObject / GetOrAddObject / GetInstance are on the hot path of every refcounted native object
// returned to managed code (paint.Shader, image.ColorSpace, canvas fetches, factory results, ...).
// Each call first decides whether the wrapper type skips global registration. That decision is a
// compile-time invariant, but the original code re-computed it with a reflection call
// (Type.IsAssignableFrom) on every single call. The fix caches the result per type in a generic
// static holder (HandleDictionary.SkipObjectRegistration<T>.Value).
//
// This benchmark faithfully reproduces the common "wrapper already registered" branch of
// GetInstance<T>: the type-check followed by the read-locked dictionary lookup. Old performs the
// reflection check (current shipped behaviour, copied verbatim); New performs the cached-bool read.
// Everything after the check is identical, so the Ratio is an honest measure of the change's effect
// on the real managed lookup path — not a synthetic isolated micro-loop.
[MemoryDiagnoser]
public class HandleDictionaryTypeCheckBenchmark
{
	// The real predicate: "does this wrapper type skip global registration?" (implements
	// ISKSkipObjectRegistration). For the overwhelmingly common wrapper type this is FALSE, so the
	// check falls through to the locked dictionary lookup — that is the path this benchmark measures.
	private static readonly Type SkipObjectRegistrationType = typeof (ISKSkipObjectRegistration);

	private readonly ReaderWriterLockSlim instancesLock = new (LockRecursionPolicy.NoRecursion);
	private readonly Dictionary<IntPtr, WeakReference> instances = new ();

	private object target;      // strong root so the WeakReference stays alive
	private IntPtr handle;

	// How many wrapped objects a caller touches in one unit of work (e.g. per draw / per frame).
	[Params(1, 100)]
	public int N { get; set; }

	[GlobalSetup]
	public void GlobalSetup()
	{
		handle = (IntPtr)0x1234;
		target = new object ();
		instances[handle] = new WeakReference (target);
	}

	// Old: current shipped behaviour — reflection type-check on every call, then the locked lookup.
	[Benchmark(Baseline = true)]
	public int Old()
	{
		var found = 0;
		for (var i = 0; i < N; i++)
		{
			if (SkipObjectRegistrationType.IsAssignableFrom (typeof (SampleObject)))
				continue;
			if (LookupOld (handle) != null)
				found++;
		}
		return found;
	}

	// New: cached type-check (a plain static-field read), then the identical locked lookup.
	[Benchmark]
	public int New()
	{
		var found = 0;
		for (var i = 0; i < N; i++)
		{
			if (SkipCache<SampleObject>.Value)
				continue;
			if (LookupNew (handle) != null)
				found++;
		}
		return found;
	}

	private object LookupOld (IntPtr h)
	{
		instancesLock.EnterReadLock ();
		try
		{
			if (instances.TryGetValue (h, out var weak) && weak.IsAlive)
				return weak.Target;
			return null;
		}
		finally
		{
			instancesLock.ExitReadLock ();
		}
	}

	private object LookupNew (IntPtr h) => LookupOld (h);

	private sealed class SampleObject
	{
		public IntPtr Handle => IntPtr.Zero;
	}

	private static class SkipCache<T>
	{
		internal static readonly bool Value =
			typeof (ISKSkipObjectRegistration).IsAssignableFrom (typeof (T));
	}
}

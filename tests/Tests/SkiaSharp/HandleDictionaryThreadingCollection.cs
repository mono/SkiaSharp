using Xunit;

namespace SkiaSharp.Tests
{
	// The dedicated white-box concurrency/lifecycle tests for HandleDictionary/SKObject drive their own
	// deterministic interleavings (via gates and barriers on dedicated OS threads) and then join those
	// threads against tight deadlines. A failed join is the signal for a genuine product deadlock.
	//
	// Under xUnit's default parallelism EVERY test class is its own collection and runs concurrently, and
	// EVERY SKObject create/dispose/lookup across the whole suite contends on the single global
	// HandleDictionary lock. On Windows that lock is a Win32 CRITICAL_SECTION (the #1383 STA-pump fix) with
	// no reader concurrency, so the entire parallel suite serializes through one mutex. On a small/slow CI
	// agent this can starve a parked dispose thread of CPU/lock ownership for >10s and trip these tight
	// deadlines — a false "deadlock" that has nothing to do with the code under test.
	//
	// Placing all of these classes in one DisableParallelization collection makes them run sequentially in
	// xUnit's non-parallel phase, AFTER the parallel phase has drained, so nothing else is hammering the
	// global lock while their deadlines are ticking. This does NOT weaken coverage: each test produces its
	// own interleaving, and the rest of the suite still exercises the global lock in parallel during the
	// parallel phase.
	//
	// SKManagedStreamConcurrencyTest joins this collection for the same reason: its gated "public Dispose
	// while a native lazy read is in-flight" tests park a real OS thread inside a managed Stream.Read() and
	// then race a public Dispose against codec teardown under a 30s join deadline. Running it in the
	// drained non-parallel phase removes both the global-lock contention above AND any thread-pool
	// starvation, so the parked reader and the racing disposer always get CPU before the deadline.
	[CollectionDefinition (HandleDictionaryThreadingCollection.Name, DisableParallelization = true)]
	public sealed class HandleDictionaryThreadingCollection
	{
		public const string Name = "HandleDictionary threading (serialized)";
	}
}

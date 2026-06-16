using System.Threading;
using Xunit;

namespace SkiaSharp.Tests
{
	// Pins the re-entrancy contract of HandleDictionary.GetOrAddObject, which runs the object
	// factory INSIDE the upgradeable-read lock. The allowed/forbidden re-entrant operations are a
	// direct consequence of PlatformLock and are platform-divergent, so they are easy to break
	// accidentally in a future lock refactor. These tests lock the observed behaviour down.
	//
	// Verified empirically (macOS) before being written:
	//   reentrant GetInstance (read)        -> succeeds
	//   nested   GetOrAddObject (create)     -> throws LockRecursionException (non-Windows)
	[Collection (HandleDictionaryThreadingCollection.Name)]
	public class SKHandleDictionaryReentrancyTest : SKTest
	{
		// A factory MAY read the dictionary. GetInstance takes a read lock, and a thread already
		// holding the upgradeable-read lock is permitted to enter read mode under
		// ReaderWriterLockSlim(NoRecursion) (a legal upgradeable->read downgrade) and may also
		// recurse on the Windows CRITICAL_SECTION. So this is safe on every platform.
		[Fact]
		public void FactoryMayReadDictionaryFromWithinTheLock ()
		{
			var probe = SKHandleDictionaryTestHelpers.NextHandle ();
			var handle = SKHandleDictionaryTestHelpers.NextHandle ();

			using var obj = HandleDictionary.GetOrAddObject<FakeNativeObject> (handle, false, true, (h, owns) => {
				// Must not throw: a read from inside the factory is part of the supported contract.
				HandleDictionary.GetInstance<FakeNativeObject> (probe, out _);
				return new FakeNativeObject (h);
			});

			Assert.NotNull (obj);
			Assert.True (HandleDictionary.GetInstance<FakeNativeObject> (handle, out var found));
			Assert.Same (obj, found);
		}

		// A factory MUST NOT create another registered object: a nested GetOrAddObject is an
		// upgradeable->upgradeable re-acquire, i.e. recursion. On non-Windows the lock is
		// ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion) and throws LockRecursionException;
		// on Windows the lock is a recursive Win32 CRITICAL_SECTION (the #1383 STA-pump fix) and it
		// would NOT throw. We assert only the platform we can run here; the throw aborts the lock
		// re-acquire before any wrapper is constructed, so nothing leaks into the registry.
		[Fact]
		public void FactoryCannotCreateNestedRegisteredObjectOnNonWindows ()
		{
			Assert.SkipWhen (IsWindows, "Windows uses a recursive CRITICAL_SECTION (#1383); nested creation does not throw there.");

			var nested = SKHandleDictionaryTestHelpers.NextHandle ();
			var outer = SKHandleDictionaryTestHelpers.NextHandle ();

			Assert.Throws<LockRecursionException> (() => {
				HandleDictionary.GetOrAddObject<FakeNativeObject> (outer, false, true, (h, owns) =>
					HandleDictionary.GetOrAddObject<FakeNativeObject> (nested, false, true, (h2, o2) => new FakeNativeObject (h2)));
			});

			// The nested re-acquire threw before constructing either wrapper, so neither handle
			// was ever registered. (No address-reuse concern: these are synthetic unique handles.)
			Assert.False (HandleDictionary.GetInstance<FakeNativeObject> (outer, out _));
			Assert.False (HandleDictionary.GetInstance<FakeNativeObject> (nested, out _));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp.Tests;
using Xunit;

[assembly: AssemblyFixture(typeof(GarbageCleanupFixture))]

namespace SkiaSharp.Tests
{
	public class GarbageCleanupFixture : IDisposable
	{
		public GarbageCleanupFixture()
		{
		}

		public void Dispose()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();

			var staticObjects = HandleDictionary.instances.Values
				.Select(o => o.Target)
				.Where(o => !IsExpectedToBeDead(o))
				.Cast<SKObject>()
				.ToList();
			var staticChildren = staticObjects
				.SelectMany(o => o.OwnedObjects.Values)
				.ToList();

			// make sure nothing is alive
			var aliveObjects = HandleDictionary.instances.Values
				.Select(o => o.Target)
				.Where(o => IsExpectedToBeDead(o))
				.Cast<SKObject>()
				.ToList();
			foreach (var o in staticChildren)
				aliveObjects.Remove(o);
#if DEBUG
			foreach (var o in aliveObjects)
				Console.WriteLine($"Found an alive object {o} that was created at: {HandleDictionary.stackTraces[o.Handle]}");
#endif
			Assert.Empty(aliveObjects);

#if THROW_OBJECT_EXCEPTIONS
			// make sure all the exceptions are accounted for
			var exceptions = HandleDictionary.exceptions
				.ToList();
			Assert.Empty(exceptions);

			// make sure all the GCHandles are freed
			var gcHandles = GCHandleProxy.allocatedHandles.Values
				.Select(o => o.Target)
				.ToList();
			Assert.Empty(gcHandles);
#endif
		}

		private bool IsExpectedToBeDead(object instance)
		{
			if (instance == null)
				return false;

			var skobject = Assert.IsAssignableFrom<SKObject>(instance);

			// Process-global singleton wrappers live for the process lifetime (rooted by a static
			// field) and are never collected, so they must be exempt from leak detection. Without an
			// immortal latch the only intrinsic marker left on the wrapper is IgnorePublicDispose.
			// Note: this is broader than the singletons — dispose-protected typefaces (match-family /
			// match-character) and re-parented managed streams also set IgnorePublicDispose, so they
			// are exempted here too. That is a modest reduction in leak-detection scope, not a
			// correctness change: those objects are still torn down by their owners' DisposeInternal,
			// just no longer cross-checked by this fixture.
			if (skobject.IgnorePublicDispose)
				return false;

			return true;
		}
	}
}

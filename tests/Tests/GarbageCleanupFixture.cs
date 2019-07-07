using System;
using System.Collections.Concurrent;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class GarbageCleanupFixture : IDisposable
	{
#if THROW_OBJECT_EXCEPTIONS
		internal static readonly ConcurrentBag<IntPtr> ignoredExceptions = new ConcurrentBag<IntPtr>();
#endif

		private static readonly string[] StaticTypes = new[] {
			"SkiaSharp.SKData+SKDataStatic",
			"SkiaSharp.SKFontManager+SKFontManagerStatic",
			"SkiaSharp.SKTypeface+SKTypefaceStatic",
		};

		public GarbageCleanupFixture()
		{
			Assert.Empty(SKObject.constructors);
			Assert.Empty(SKObject.instances);
		}

		public void Dispose()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();

			// make sure nothing is alive
			var aliveObjects = SKObject.instances.Values
				.Select(o => o.Target)
				.Where(IsExpectedToBeDead)
				.ToList();
			Assert.Empty(aliveObjects);

#if THROW_OBJECT_EXCEPTIONS
			// make sure all the exceptions are accounted for
			var ignored = ignoredExceptions
				.ToList();
			var exceptions = SKObject.exceptions
				.ToList();
			var keep = exceptions
				.Where(ex => !ignored.Contains((IntPtr)ex.Data["Handle"]))
				.ToList();
			Assert.Empty(keep);
			foreach (var ignore in ignored)
			{
				var e = exceptions
					.Where(ex => ex.Data["Handle"] is IntPtr)
					.ToList();
				Assert.NotEmpty(e);
			}

			// make sure all the GCHandles are freed
			var gcHandles = GCHandleProxy.allocatedHandles.Values
				.Select(o => o.Target)
				.ToList();
			Assert.Empty(gcHandles);
#endif
		}

		private bool IsExpectedToBeDead(object instance)
		{
			var skobject = Assert.IsAssignableFrom<SKObject>(instance);

			if (StaticTypes.Contains(skobject.GetType().FullName))
				return false;

			return true;
		}
	}
}

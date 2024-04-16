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
		private static readonly string[] StaticTypes = new[] {
			"SkiaSharp.SKData+SKDataStatic",
			"SkiaSharp.SKFontManager+SKFontManagerStatic",
			"SkiaSharp.SKFontStyle+SKFontStyleStatic",
			"SkiaSharp.SKTypeface+SKTypefaceStatic",
			"SkiaSharp.SKColorSpace+SKColorSpaceStatic",
			"SkiaSharp.SKBlender+SKBlenderStatic",
		};

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

			if (StaticTypes.Contains(skobject.GetType().FullName))
				return false;

			return true;
		}
	}
}

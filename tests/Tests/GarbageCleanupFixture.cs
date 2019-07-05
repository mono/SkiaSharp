using System;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class GarbageCleanupFixture : IDisposable
	{
		private static readonly string[] StaticTypes = new[] {
			"SkiaSharp.SKData+SKDataStatic",
			"SkiaSharp.SKFontManager+SKFontManagerStatic",
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

			//Assert.NotEmpty(SKObject.constructors);

			var aliveObjects = SKObject.instances.Values
				.Select(o => o.Target)
				.Where(o => o != null)
				.Where(IsExpectToBeDead)
				.ToList();

			//Assert.Equal(0, aliveObjects.Count);
			Assert.Empty(aliveObjects);
		}

		private bool IsExpectToBeDead(object instance)
		{
			var skobject = Assert.IsAssignableFrom<SKObject>(instance);

			if (StaticTypes.Contains(skobject.GetType().FullName))
				return false;

			//// TODO: remove this once the tests are all fixed
			//if (skobject.GetType().Name.Contains("Stream"))
			//	return false;

			return true;
		}
	}
}

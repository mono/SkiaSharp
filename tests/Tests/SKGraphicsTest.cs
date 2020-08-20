using Xunit;

namespace SkiaSharp.Tests
{
	public class SKGraphicsTest : SKTest
	{
		public SKGraphicsTest()
		{
			SKGraphics.Init();
		}

		[SkippableFact]
		public unsafe void GetFontCacheLimitIsNotZero()
		{
			var limit = SKGraphics.GetFontCacheLimit();

			Assert.NotEqual(0, limit);
		}

		[SkippableFact]
		public unsafe void GetFontCacheLimitUpdatesAndReturnsPrevious()
		{
			var limit = SKGraphics.GetFontCacheLimit();
			Assert.NotEqual(0, limit);

			var oldLimit = SKGraphics.SetFontCacheLimit(limit + 1);
			Assert.Equal(limit, oldLimit);

			var newLimit = SKGraphics.SetFontCacheLimit(limit);
			Assert.Equal(limit + 1, newLimit);
		}
	}
}

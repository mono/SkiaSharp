using Android.Graphics;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.Views.Android.Tests
{
	public abstract class AndroidTests : SKTest
	{
		protected static void ValidateTestBitmap(Bitmap bmp, byte alpha = 255)
		{
			Assert.NotNull(bmp);
			Assert.Equal(40, bmp.Width);
			Assert.Equal(40, bmp.Height);

			Assert.Equal(Get(SKColors.Red), (uint)bmp.GetPixel(10, 10));
			Assert.Equal(Get(SKColors.Green), (uint)bmp.GetPixel(30, 10));
			Assert.Equal(Get(SKColors.Blue), (uint)bmp.GetPixel(10, 30));
			Assert.Equal(Get(SKColors.Yellow), (uint)bmp.GetPixel(30, 30));

			uint Get(SKColor color) =>
				alpha == 0
					? 0
					: (uint)color.WithAlpha(alpha);
		}
	}
}

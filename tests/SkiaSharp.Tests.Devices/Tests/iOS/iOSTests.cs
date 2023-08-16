using CoreGraphics;
using SkiaSharp.Tests;
using UIKit;
using Xunit;

namespace SkiaSharp.Views.iOS.Tests
{
	public abstract class iOSTests : SKTest
	{
		protected static void ValidateTestBitmap(UIImage uiImage, byte alpha = 255)
		{
			var cgImage = uiImage.CGImage;

			ValidateTestBitmap(cgImage, alpha);
		}

		protected static void ValidateTestBitmap(CGImage cgImage, byte alpha = 255)
		{
			Assert.NotNull(cgImage);
			Assert.Equal(40, cgImage.Width);
			Assert.Equal(40, cgImage.Height);

			Assert.Equal(Get(SKColors.Red), cgImage.GetPixel(10, 10));
			Assert.Equal(Get(SKColors.Green), cgImage.GetPixel(30, 10));
			Assert.Equal(Get(SKColors.Blue), cgImage.GetPixel(10, 30));
			Assert.Equal(Get(SKColors.Yellow), cgImage.GetPixel(30, 30));

			SKColor Get(SKColor color) =>
				alpha == 0
					? SKColor.Empty
					: color.WithAlpha(alpha);
		}
	}
}

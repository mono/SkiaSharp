using CoreGraphics;

namespace SkiaSharp.Views.iOS.Tests
{
	public static class TestExtensions
	{
		public static SKColor GetPixel(this CGImage cgImage, int x, int y)
		{
			var data = cgImage.DataProvider.CopyData();

			var bytesPerPixel = cgImage.BitsPerPixel / cgImage.BitsPerComponent;

			var offset = (y * cgImage.BytesPerRow) + (x * bytesPerPixel);

			var a = data[offset + 3];
			var r = data[offset + 0];
			var g = data[offset + 1];
			var b = data[offset + 2];

			if (a == 0)
				return SKColor.Empty;

			return (SKColor)new SKColorF((float)r / a, (float)g / a, (float)b / a, a / 255f);
		}
	}
}

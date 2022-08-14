using Xunit;

namespace SkiaSharp.Views.iOS.Tests
{
	public class iOSExtensionsTests : iOSTests
	{
		[SkippableTheory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(255)]
		public void PixelBackedImageToUIImage(byte alpha)
		{
			using var bitmap = CreateTestBitmap(alpha);
			using var image = SKImage.FromBitmap(bitmap);

			using var iosBitmap = image.ToUIImage();

			ValidateTestBitmap(iosBitmap, alpha);
		}

		[SkippableTheory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(255)]
		public void BitmapToUIImage(byte alpha)
		{
			using var bitmap = CreateTestBitmap(alpha);

			using var uiImage = bitmap.ToUIImage();

			ValidateTestBitmap(uiImage, alpha);
		}

		[SkippableTheory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(255)]
		public void PixmapToUIImage(byte alpha)
		{
			using var bitmap = CreateTestBitmap(alpha);
			using var pixmap = bitmap.PeekPixels();

			using var uiImage = pixmap.ToUIImage();

			ValidateTestBitmap(uiImage, alpha);
		}

		[SkippableTheory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(255)]
		public void EncodedDataBackedImageToUIImage(byte alpha)
		{
			using var bitmap = CreateTestBitmap(alpha);
			using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
			using var image = SKImage.FromEncodedData(data);

			using var uiImage = image.ToUIImage();

			ValidateTestBitmap(uiImage, alpha);
		}
	}
}

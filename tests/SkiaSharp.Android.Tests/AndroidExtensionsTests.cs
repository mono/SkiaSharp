using Xunit;

namespace SkiaSharp.Views.Android.Tests
{
	public class AndroidExtensionsTests : AndroidTests
	{
		[SkippableTheory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(255)]
		public void PixelBackedImageToBitmap(byte alpha)
		{
			using var bitmap = CreateTestBitmap(alpha);
			using var image = SKImage.FromBitmap(bitmap);

			using var androidBitmap = image.ToBitmap();

			ValidateTestBitmap(androidBitmap, alpha);

			androidBitmap.Recycle();
		}

		[SkippableTheory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(255)]
		public void BitmapToBitmap(byte alpha)
		{
			using var bitmap = CreateTestBitmap(alpha);

			using var androidBitmap = bitmap.ToBitmap();

			ValidateTestBitmap(androidBitmap, alpha);

			androidBitmap.Recycle();
		}

		[SkippableTheory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(255)]
		public void PixmapToBitmap(byte alpha)
		{
			using var bitmap = CreateTestBitmap(alpha);
			using var pixmap = bitmap.PeekPixels();

			using var androidBitmap = pixmap.ToBitmap();

			ValidateTestBitmap(androidBitmap, alpha);

			androidBitmap.Recycle();
		}

		[SkippableTheory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(255)]
		public void EncodedDataBackedImageToBitmap(byte alpha)
		{
			using var bitmap = CreateTestBitmap(alpha);
			using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
			using var image = SKImage.FromEncodedData(data);

			using var androidBitmap = image.ToBitmap();

			ValidateTestBitmap(androidBitmap, alpha);

			androidBitmap.Recycle();
		}
	}
}

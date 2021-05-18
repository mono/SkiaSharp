using System.Threading.Tasks;
using Android.App;
using SkiaSharp.Views.Android.Tests;
using Xunit;

namespace SkiaSharp.Views.Forms.Tests
{
	public class SKImageSourceHandlerTests : AndroidTests
	{
		private readonly SKImageSourceHandler handler;

		public SKImageSourceHandlerTests()
		{
			handler = new SKImageSourceHandler();
		}

		[SkippableTheory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(255)]
		public async Task PixelBackedImageToBitmap(byte alpha)
		{
			using var bitmap = CreateTestBitmap(alpha);
			using var image = SKImage.FromBitmap(bitmap);

			var source = (SKImageImageSource)image;

			using var androidBitmap = await handler.LoadImageAsync(source, Application.Context);

			ValidateTestBitmap(androidBitmap, alpha);

			androidBitmap.Recycle();
		}

		[SkippableTheory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(255)]
		public async Task BitmapToBitmap(byte alpha)
		{
			using var bitmap = CreateTestBitmap(alpha);

			var source = (SKBitmapImageSource)bitmap;

			using var androidBitmap = await handler.LoadImageAsync(source, Application.Context);

			ValidateTestBitmap(androidBitmap, alpha);

			androidBitmap.Recycle();
		}

		[SkippableTheory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(255)]
		public async Task PixmapToBitmap(byte alpha)
		{
			using var bitmap = CreateTestBitmap(alpha);
			using var pixmap = bitmap.PeekPixels();

			var source = (SKPixmapImageSource)pixmap;

			using var androidBitmap = await handler.LoadImageAsync(source, Application.Context);

			ValidateTestBitmap(androidBitmap, alpha);

			androidBitmap.Recycle();
		}

		[SkippableTheory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(100)]
		[InlineData(255)]
		public async Task EncodedDataBackedImageToBitmap(byte alpha)
		{
			using var bitmap = CreateTestBitmap(alpha);
			using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
			using var image = SKImage.FromEncodedData(data);

			var source = (SKImageImageSource)image;

			using var androidBitmap = await handler.LoadImageAsync(source, Application.Context);

			ValidateTestBitmap(androidBitmap, alpha);

			androidBitmap.Recycle();
		}
	}
}

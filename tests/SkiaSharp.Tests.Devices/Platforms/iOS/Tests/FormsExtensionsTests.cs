using System.Threading.Tasks;
using SkiaSharp.Views.iOS.Tests;
using Xunit;

namespace SkiaSharp.Views.Maui.Controls.Compatibility.Tests
{
	public class SKImageSourceHandlerTests : iOSTests
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

			using var uiImage = await handler.LoadImageAsync(source);

			ValidateTestBitmap(uiImage, alpha);
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

			using var uiImage = await handler.LoadImageAsync(source);

			ValidateTestBitmap(uiImage, alpha);
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

			using var uiImage = await handler.LoadImageAsync(source);

			ValidateTestBitmap(uiImage, alpha);
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

			using var uiImage = await handler.LoadImageAsync(source);

			ValidateTestBitmap(uiImage, alpha);
		}
	}
}

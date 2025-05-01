using System;
using CoreGraphics;
using Foundation;
using Metal;
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

		[SkippableTheory]
		[InlineData(0, 0, 0, 0)]
		[InlineData(5, 5, 5, 5)]
		[InlineData(1, 2, 3, 4)]
		[InlineData(1, 1, 0, 0)]
		[InlineData(0, 0, 1, 1)]
		[InlineData(100, 100, 100, 100)]
		public void SKRectToCGRect(int x, int y, int w, int h)
		{
			var initial = SKRect.Create(x, y, w, h);
			var expected = new CGRect(x, y, w, h);

			var actual = initial.ToRect();

			Assert.Equal(expected, actual);
		}

		[SkippableTheory]
		[InlineData(0, 0, 0, 0)]
		[InlineData(5, 5, 5, 5)]
		[InlineData(1, 2, 3, 4)]
		[InlineData(1, 1, 0, 0)]
		[InlineData(0, 0, 1, 1)]
		[InlineData(100, 100, 100, 100)]
		public void CGRectToSKRect(int x, int y, int w, int h)
		{
			var initial = new CGRect(x, y, w, h);
			var expected = SKRect.Create(x, y, w, h);

			var actual = initial.ToSKRect();

			Assert.Equal(expected, actual);
		}

		[SkippableFact]
		public void GRContextDisposeDoesNotCrash()
		{
			var device = MTLDevice.SystemDefault!;
			Skip.If(device == null, "Metal is not supported on this device.");

			using var commandQueue = device.CreateCommandQueue();
			using var backendContext = new GRMtlBackendContext()
			{
				Device = device,
				Queue = commandQueue,
			};

			using var context = GRContext.CreateMetal(backendContext);
		}
	}
}

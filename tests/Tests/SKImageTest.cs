using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKImageTest : SKTest
	{
		[SkippableFact]
		public void TestLazyImage()
		{
			var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));
			Assert.NotNull(data);

			var image = SKImage.FromEncodedData(data);
			Assert.NotNull(image);

			Assert.True(image.IsLazyGenerated);
		}

		[SkippableFact]
		public void TestNotLazyImage()
		{
			var bitmap = CreateTestBitmap();
			Assert.NotNull(bitmap);

			var image = SKImage.FromBitmap(bitmap);
			Assert.NotNull(image);

			Assert.False(image.IsLazyGenerated);
		}

		[SkippableFact]
		public void DecodingJpegImagePreservesColorSpace()
		{
			var path = Path.Combine(PathToImages, "baboon.jpg");

			var image = SKImage.FromEncodedData(path);

			Assert.NotNull(image.ColorSpace);
		}

		[SkippableFact]
		public void DecodingPngImagePreservesColorSpace()
		{
			var path = Path.Combine(PathToImages, "color-wheel.png");

			var image = SKImage.FromEncodedData(path);

			Assert.NotNull(image.ColorSpace);
		}

		[SkippableFact]
		public void TestFromPixelCopyIntPtr()
		{
			using (var bmp = CreateTestBitmap())
			using (var image = SKImage.FromPixelCopy(bmp.Info, bmp.GetPixels(out var length)))
			{
				ValidateTestPixmap(image.PeekPixels());
			}
		}

		[SkippableFact]
		public void TestFromPixelCopyByteArray()
		{
			using (var bmp = CreateTestBitmap())
			{
				var px = bmp.GetPixels(out var length);
				var dst = new byte[(int)length];
				Marshal.Copy(px, dst, 0, (int)length);
				using (var image = SKImage.FromPixelCopy(bmp.Info, dst))
				{
					ValidateTestPixmap(image.PeekPixels());
				}
			}
		}

		[SkippableFact]
		public void TestFromPixelCopyStream()
		{
			using (var bmp = CreateTestBitmap())
			{
				var px = bmp.GetPixels(out var length);
				var dst = new byte[(int)length];
				Marshal.Copy(px, dst, 0, (int)length);
				using (var stream = new MemoryStream(dst))
				using (var image = SKImage.FromPixelCopy(bmp.Info, stream))
				{
					ValidateTestPixmap(image.PeekPixels());
				}
			}
		}

		[SkippableFact]
		public void SupportsNonASCIICharactersInPath()
		{
			var fileName = Path.Combine(PathToImages, "上田雅美.jpg");

			using (var image = SKImage.FromEncodedData(fileName))
			{
				Assert.NotNull(image);
			}
		}

		[SkippableFact]
		public void TestImageManagedBytesDecodeDrawsCorrectly()
		{
			var path = Path.Combine(PathToImages, "color-wheel.png");

			var managed = File.ReadAllBytes(path);
			using (var image = SKImage.FromEncodedData(managed))
			using (var surface = SKSurface.Create(new SKImageInfo(200, 200)))
			{
				var canvas = surface.Canvas;
				canvas.Clear(SKColors.White);
				canvas.DrawImage(image, 0, 0);

				using (var snap = surface.Snapshot())
				using (var bmp = SKBitmap.FromImage(snap))
				{
					Assert.Equal(new SKColor(2, 255, 42), bmp.GetPixel(20, 20));
					Assert.Equal(new SKColor(1, 83, 255), bmp.GetPixel(108, 20));
					Assert.Equal(new SKColor(255, 166, 1), bmp.GetPixel(20, 108));
					Assert.Equal(new SKColor(255, 1, 214), bmp.GetPixel(108, 108));
				}
			}
		}

		[SkippableFact]
		public void TestImageManagedStreamDecodeDrawsCorrectly()
		{
			var path = Path.Combine(PathToImages, "color-wheel.png");

			using (var managed = new FileStream(path, FileMode.Open))
			using (var image = SKImage.FromEncodedData(managed))
			using (var surface = SKSurface.Create(new SKImageInfo(200, 200)))
			{
				var canvas = surface.Canvas;
				canvas.Clear(SKColors.White);
				canvas.DrawImage(image, 0, 0);

				using (var snap = surface.Snapshot())
				using (var bmp = SKBitmap.FromImage(snap))
				{
					Assert.Equal(new SKColor(2, 255, 42), bmp.GetPixel(20, 20));
					Assert.Equal(new SKColor(1, 83, 255), bmp.GetPixel(108, 20));
					Assert.Equal(new SKColor(255, 166, 1), bmp.GetPixel(20, 108));
					Assert.Equal(new SKColor(255, 1, 214), bmp.GetPixel(108, 108));
				}
			}
		}

		[SkippableFact]
		public void TestImageFileDecodeDrawsCorrectly()
		{
			var path = Path.Combine(PathToImages, "color-wheel.png");

			using (var image = SKImage.FromEncodedData(path))
			using (var surface = SKSurface.Create(new SKImageInfo(200, 200)))
			{
				var canvas = surface.Canvas;
				canvas.Clear(SKColors.White);
				canvas.DrawImage(image, 0, 0);

				using (var snap = surface.Snapshot())
				using (var bmp = SKBitmap.FromImage(snap))
				{
					Assert.Equal(new SKColor(2, 255, 42), bmp.GetPixel(20, 20));
					Assert.Equal(new SKColor(1, 83, 255), bmp.GetPixel(108, 20));
					Assert.Equal(new SKColor(255, 166, 1), bmp.GetPixel(20, 108));
					Assert.Equal(new SKColor(255, 1, 214), bmp.GetPixel(108, 108));
				}
			}
		}

		[SkippableFact]
		public void TestImageDataDecodeDrawsCorrectly()
		{
			var path = Path.Combine(PathToImages, "color-wheel.png");

			using (var data = SKData.Create(path))
			using (var image = SKImage.FromEncodedData(data))
			using (var surface = SKSurface.Create(new SKImageInfo(200, 200)))
			{
				var canvas = surface.Canvas;
				canvas.Clear(SKColors.White);
				canvas.DrawImage(image, 0, 0);

				using (var snap = surface.Snapshot())
				using (var bmp = SKBitmap.FromImage(snap))
				{
					Assert.Equal(new SKColor(2, 255, 42), bmp.GetPixel(20, 20));
					Assert.Equal(new SKColor(1, 83, 255), bmp.GetPixel(108, 20));
					Assert.Equal(new SKColor(255, 166, 1), bmp.GetPixel(20, 108));
					Assert.Equal(new SKColor(255, 1, 214), bmp.GetPixel(108, 108));
				}
			}
		}

		[Obsolete]
		[SkippableFact]
		public void EncodeWithSimpleSerializer()
		{
			var bitmap = CreateTestBitmap();

			bool encoded = false;
			var serializer = SKPixelSerializer.Create(pixmap =>
			{
				encoded = true;
				return pixmap.Encode(SKEncodedImageFormat.Jpeg, 100);
			});

			var image = SKImage.FromBitmap(bitmap);
			var data = image.Encode(serializer);

			var codec = SKCodec.Create(data);

			Assert.Equal(SKEncodedImageFormat.Jpeg, codec.EncodedFormat);

			Assert.True(encoded);
		}

		[Obsolete]
		[SkippableFact]
		public void EncodeWithSerializer()
		{
			var bitmap = CreateTestBitmap();

			var serializer = new TestSerializer();

			var image = SKImage.FromBitmap(bitmap);
			var data = image.Encode(serializer);

			Assert.NotNull(data);

			Assert.Equal(1, serializer.DidEncode);
			Assert.Equal(0, serializer.DidUseEncodedData);

			Assert.Equal(data.ToArray(), bitmap.Bytes);
		}

		[Obsolete]
		private class TestSerializer : SKPixelSerializer
		{
			public int DidEncode { get; set; }

			public int DidUseEncodedData { get; set; }

			public SKImageInfo EncodedInfo { get; set; }

			protected override SKData OnEncode(SKPixmap pixmap)
			{
				DidEncode++;

				EncodedInfo = pixmap.Info;

				return SKData.CreateCopy(pixmap.GetPixels(), (ulong)pixmap.Info.BytesSize);
			}

			protected override bool OnUseEncodedData(IntPtr data, IntPtr length)
			{
				DidUseEncodedData++;

				return false;
			}
		}
	}
}

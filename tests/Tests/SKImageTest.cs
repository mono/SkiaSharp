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
		public void ToRasterImageReturnsSameRaster()
		{
			using var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));
			using var image = SKImage.FromEncodedData(data);

			Assert.True(image.IsLazyGenerated);
			Assert.Null(image.PeekPixels());
			Assert.Equal(image, image.ToRasterImage());
		}

		[SkippableFact]
		public void LazyRasterCanReadToNonLazy()
		{
			using var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));
			using var image = SKImage.FromEncodedData(data);
			Assert.True(image.IsLazyGenerated);

			var info = new SKImageInfo(image.Width, image.Height);
			using var copy = SKImage.Create(info);
			using var pix = copy.PeekPixels();

			Assert.True(image.ReadPixels(pix));
			Assert.False(copy.IsLazyGenerated);
			Assert.NotNull(copy.PeekPixels());
		}

		[SkippableFact]
		public void ToRasterImageTrueFalseReturnsNonLazy()
		{
			using var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));
			using var image = SKImage.FromEncodedData(data);

			Assert.True(image.IsLazyGenerated);
			Assert.Null(image.PeekPixels());

			using var nonLazy = image.ToRasterImage(true);
			Assert.NotEqual(image, nonLazy);
			Assert.False(nonLazy.IsLazyGenerated);
			Assert.NotNull(nonLazy.PeekPixels());
			Assert.Equal(nonLazy, nonLazy.ToRasterImage());
		}

		[SkippableFact]
		public void ToRasterImageTrueTrueReturnsNonLazy()
		{
			using var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));
			using var image = SKImage.FromEncodedData(data);

			Assert.True(image.IsLazyGenerated);
			Assert.Null(image.PeekPixels());

			using var nonLazy = image.ToRasterImage(true);
			Assert.NotEqual(image, nonLazy);
			Assert.False(nonLazy.IsLazyGenerated);
			Assert.NotNull(nonLazy.PeekPixels());
			Assert.Equal(nonLazy, nonLazy.ToRasterImage(true));
		}

		[SkippableFact]
		public void ImmutableBitmapsAreNotCopied()
		{
			// create "pixel data"
			var pixelData = new SKBitmap(100, 100);
			pixelData.Erase(SKColors.Red);

			// create text bitmap
			var bitmap = new SKBitmap();
			bitmap.InstallPixels(pixelData.PeekPixels());

			// mark it as immutable
			bitmap.SetImmutable();

			// create an image
			var image = SKImage.FromBitmap(bitmap);
			Assert.Equal(SKColors.Red, image.PeekPixels().GetPixelColor(50, 50));

			// modify the "pixel data"
			pixelData.Erase(SKColors.Blue);

			// ensure that the pixels are modified
			Assert.Equal(SKColors.Blue, image.PeekPixels().GetPixelColor(50, 50));
		}

		[SkippableFact]
		public void MutableBitmapsAreCopied()
		{
			var bitmap = new SKBitmap(100, 100);
			bitmap.Erase(SKColors.Red);

			var image = SKImage.FromBitmap(bitmap);
			Assert.Equal(SKColors.Red, image.PeekPixels().GetPixelColor(50, 50));

			bitmap.Erase(SKColors.Blue);
			Assert.Equal(SKColors.Red, image.PeekPixels().GetPixelColor(50, 50));
		}

		[SkippableFact]
		public void ReleaseImagePixelsWasInvoked()
		{
			bool released = false;

			var onRelease = new SKImageRasterReleaseDelegate((addr, ctx) =>
			{
				Marshal.FreeCoTaskMem(addr);
				released = true;
				Assert.Equal("RELEASING!", ctx);
			});

			var info = new SKImageInfo(1, 1);
			var pixels = Marshal.AllocCoTaskMem(info.BytesSize);

			using (var pixmap = new SKPixmap(info, pixels))
			using (var image = SKImage.FromPixels(pixmap, onRelease, "RELEASING!"))
			{
				Assert.False(image.IsTextureBacked);
				var raster = image.ToRasterImage();
				Assert.Same(image, raster);
			}

			Assert.True(released, "The SKImageRasterReleaseDelegate was not called.");
		}

		[SkippableFact]
		public void DoesNotCrashWhenDecodingInvalidPath()
		{
			var path = Path.Combine(PathToImages, "file-does-not-exist.png");

			Assert.Null(SKImage.FromEncodedData(path));
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

			using (var managed = new FileStream(path, FileMode.Open, FileAccess.Read))
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

		[SkippableFact]
		public void CanScalePixels()
		{
			var srcInfo = new SKImageInfo(200, 200);
			var dstInfo = new SKImageInfo(100, 100);

			var srcSurface = SKSurface.Create(srcInfo);
			var dstBmp = new SKBitmap(dstInfo);

			using (var paint = new SKPaint { Color = SKColors.Green })
			{
				srcSurface.Canvas.Clear(SKColors.Blue);
				srcSurface.Canvas.DrawRect(new SKRect(0, 0, 100, 200), paint);
			}

			var srcImage = srcSurface.Snapshot();
			var srcPix = srcImage.PeekPixels();
			var dstPix = dstBmp.PeekPixels();

			Assert.Equal(SKColors.Green, srcPix.GetPixelColor(75, 75));
			Assert.Equal(SKColors.Blue, srcPix.GetPixelColor(175, 175));

			Assert.True(srcImage.ScalePixels(dstPix, SKFilterQuality.High));

			Assert.Equal(SKColors.Green, dstBmp.GetPixel(25, 25));
			Assert.Equal(SKColors.Blue, dstBmp.GetPixel(75, 75));
		}

		[SkippableFact]
		public unsafe void DataInstanceIsCorrectlyDisposedWhenPassed()
		{
			var released = false;

			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "baboon.jpg"));
			fixed (byte* b = bytes)
			{
				var input = SKData.Create((IntPtr)b, bytes.Length, (_, __) => released = true);
				Assert.Equal(1, input.GetReferenceCount());

				var image = SKImage.FromEncodedData(input);
				Assert.Equal(3, input.GetReferenceCount());
				Assert.False(released, "Data was disposed too soon.");

				image.Dispose();
				Assert.Equal(1, input.GetReferenceCount());
				Assert.False(released, "Data was disposed too soon.");

				input.Dispose();
				Assert.True(released, "Data was not disposed.");
			}
		}

		[SkippableFact]
		public unsafe void EncodedDataReturnsTheSameInstanceAsTheInput()
		{
			var released = false;

			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "baboon.jpg"));
			fixed (byte* b = bytes)
			{
				var input = SKData.Create((IntPtr)b, bytes.Length, (_, __) => released = true);
				var image = SKImage.FromEncodedData(input);

				var encoded = image.EncodedData;
				Assert.Same(input, encoded);
				Assert.Equal(3, input.GetReferenceCount());
				Assert.False(released, "Data was disposed too soon.");

				image.Dispose();
				Assert.Equal(1, input.GetReferenceCount());
				Assert.False(released, "Data was disposed too soon.");

				input.Dispose();
				Assert.True(released, "Data was not disposed.");
			}
		}

		[SkippableFact]
		public unsafe void EncodeReturnTheSameInstanceIfItWasUsedToConstruct()
		{
			var released = false;

			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "baboon.jpg"));
			fixed (byte* b = bytes)
			{
				var input = SKData.Create((IntPtr)b, bytes.Length, (_, __) => released = true);
				var image = SKImage.FromEncodedData(input);

				var result = image.Encode();
				Assert.Same(input, result);
				Assert.Equal(3, input.GetReferenceCount());
				Assert.False(released, "Data was disposed too soon.");

				image.Dispose();
				Assert.Equal(1, input.GetReferenceCount());
				Assert.False(released, "Data was disposed too soon.");

				input.Dispose();
				Assert.True(released, "Data was not disposed.");
			}
		}

		[SkippableFact]
		public unsafe void DataCanBeResurrectedFromImage()
		{
			var released = false;

			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "baboon.jpg"));
			var gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);

			using (var image = DoWork(out var dataHandle))
			{
				Assert.Equal(2, dataHandle.GetReferenceCount(false));
				Assert.False(released, "Data was disposed too soon.");

				ResurrectData(image);
			}

			Assert.True(released, "Data was not disposed.");

			gch.Free();

			SKImage DoWork(out IntPtr handle)
			{
				var input = SKData.Create(gch.AddrOfPinnedObject(), bytes.Length, (_, __) => released = true);
				handle = input.Handle;

				var img = SKImage.FromEncodedData(input);
				Assert.Equal(3, input.GetReferenceCount());
				Assert.False(released, "Data was disposed too soon.");

				var result = img.Encode();
				Assert.Same(input, result);
				Assert.Equal(3, input.GetReferenceCount());
				Assert.False(released, "Data was disposed too soon.");

				result.Dispose();
				Assert.Equal(2, handle.GetReferenceCount(false));
				Assert.False(released, "Data was disposed too soon.");

				return img;
			}

			void ResurrectData(SKImage img)
			{
				var encoded = img.EncodedData;
				Assert.NotNull(encoded);
				Assert.Equal(3, encoded.GetReferenceCount());

				var handle = encoded.Handle;

				encoded.Dispose();
				Assert.Equal(2, handle.GetReferenceCount(false));
				Assert.False(released, "Data was disposed too soon.");

			}
		}

		[SkippableFact]
		public unsafe void DataOutLivesImageUntilFinalizersRun()
		{
			var released = false;

			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "baboon.jpg"));
			var gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);

			var dataHandle = DoWork();

			Assert.Equal(1, dataHandle.GetReferenceCount(false));
			Assert.False(released, "Data was disposed too soon.");

			CollectGarbage();

			Assert.True(released, "Data was not disposed.");

			gch.Free();

			IntPtr DoWork()
			{
				var input = SKData.Create(gch.AddrOfPinnedObject(), bytes.Length, (_, __) => released = true);
				var handle = input.Handle;

				var img = SKImage.FromEncodedData(input);
				Assert.Equal(3, input.GetReferenceCount());
				Assert.False(released, "Data was disposed too soon.");

				img.Dispose();
				Assert.Equal(1, handle.GetReferenceCount(false));
				Assert.False(released, "Data was disposed too soon.");

				return handle;
			}
		}

		[SkippableFact]
		public unsafe void EncodeAndEncodedDataDoNotAdjustCountsWhenUsedTogether()
		{
			var released = false;

			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "baboon.jpg"));
			fixed (byte* b = bytes)
			{
				var input = SKData.Create((IntPtr)b, bytes.Length, (_, __) => released = true);
				var image = SKImage.FromEncodedData(input);

				var encoded1 = image.EncodedData;
				Assert.Same(input, encoded1);
				Assert.Equal(3, input.GetReferenceCount());
				Assert.False(released, "Data was disposed too soon.");

				var result = image.Encode();
				Assert.Same(input, result);
				Assert.Equal(3, input.GetReferenceCount());
				Assert.False(released, "Data was disposed too soon.");

				var encoded2 = image.EncodedData;
				Assert.Same(input, encoded2);
				Assert.Equal(3, input.GetReferenceCount());
				Assert.False(released, "Data was disposed too soon.");

				image.Dispose();
				Assert.Equal(1, input.GetReferenceCount());
				Assert.False(released, "Data was disposed too soon.");

				input.Dispose();
				Assert.True(released, "Data was not disposed.");
			}
		}

		[SkippableFact]
		public void EncodingDoesNotKeepReference()
		{
			var bitmap = CreateTestBitmap();
			var image = SKImage.FromBitmap(bitmap);

			Assert.Null(image.EncodedData);

			var result = image.Encode();
			Assert.NotNull(result);
			Assert.Equal(1, result.GetReferenceCount());
		}

		[SkippableFact]
		public void DataCreatedByImageExpiresAfterFinalizers()
		{
			VerifyImmediateFinalizers();

			var bitmap = CreateTestBitmap();
			var image = SKImage.FromBitmap(bitmap);

			var handle = DoEncode();

			CollectGarbage();

			Assert.False(SKObject.GetInstance<SKData>(handle, out _));

			IntPtr DoEncode()
			{
				var result = image.Encode();
				Assert.NotNull(result);
				Assert.Equal(1, result.GetReferenceCount());

				return result.Handle;
			}
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void RasterImageIsValidAlways()
		{
			using var image = SKImage.FromEncodedData(Path.Combine(PathToImages, "baboon.jpg"));

			Assert.True(image.IsValid(null));

			using var ctx = CreateGlContext();
			ctx.MakeCurrent();
			using var grContext = GRContext.CreateGl();

			Assert.True(image.IsValid(grContext));
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void TextureImageIsValidOnContext()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();
			using var grContext = GRContext.CreateGl();

			using var image = SKImage.FromEncodedData(Path.Combine(PathToImages, "baboon.jpg"));
			using var texture = image.ToTextureImage(grContext);

			Assert.True(texture.IsValid(grContext));
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void RasterImageCanBecomeTexture()
		{
			using var image = SKImage.FromEncodedData(Path.Combine(PathToImages, "baboon.jpg"));

			using var ctx = CreateGlContext();
			ctx.MakeCurrent();
			using var grContext = GRContext.CreateGl();

			Assert.False(image.IsTextureBacked);

			using var texture = image.ToTextureImage(grContext);

			Assert.NotNull(texture);
			Assert.True(texture.IsTextureBacked);
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void TextureImageCanBecomeRaster()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();
			using var grContext = GRContext.CreateGl();

			using var image = SKImage.FromEncodedData(Path.Combine(PathToImages, "baboon.jpg"));
			using var texture = image.ToTextureImage(grContext);

			using var raster = texture.ToRasterImage();

			Assert.NotNull(raster);
			Assert.False(raster.IsTextureBacked);
			Assert.False(raster.IsLazyGenerated);
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void DecodingWithDataAndDrawingOnGPUCreatesCorrectImage()
		{
			var info = new SKImageInfo(120, 120);
			var path = Path.Combine(PathToImages, "vimeo_icon_dark.png");

			using (var ctx = CreateGlContext())
			{
				ctx.MakeCurrent();

				using (var grContext = GRContext.CreateGl())
				using (var surface = SKSurface.Create(grContext, true, info))
				{
					var canvas = surface.Canvas;

					canvas.Clear(SKColors.Crimson);

					using (var data = SKData.Create(path))
					using (var image = SKImage.FromEncodedData(data))
					{
						canvas.DrawImage(image, 0, 0);
					}

					using (var bmp = new SKBitmap(info))
					{
						surface.ReadPixels(info, bmp.GetPixels(), info.RowBytes, 0, 0);

						Assert.Equal(SKColors.Crimson, bmp.GetPixel(3, 3));
						Assert.Equal(SKColors.Crimson, bmp.GetPixel(70, 50));
						Assert.Equal(new SKColor(23, 35, 34), bmp.GetPixel(40, 40));
					}
				}
			}
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void DecodingWithBitmapAndDrawingOnGPUCreatesCorrectImage()
		{
			var info = new SKImageInfo(120, 120);
			var path = Path.Combine(PathToImages, "vimeo_icon_dark.png");

			using (var ctx = CreateGlContext())
			{
				ctx.MakeCurrent();

				using (var grContext = GRContext.CreateGl())
				using (var surface = SKSurface.Create(grContext, true, info))
				{
					var canvas = surface.Canvas;

					canvas.Clear(SKColors.Crimson);

					using (var bitmap = SKBitmap.Decode(path))
					using (var image = SKImage.FromBitmap(bitmap))
					{
						canvas.DrawImage(image, 0, 0);
					}

					using (var bmp = new SKBitmap(info))
					{
						surface.ReadPixels(info, bmp.GetPixels(), info.RowBytes, 0, 0);

						Assert.Equal(SKColors.Crimson, bmp.GetPixel(3, 3));
						Assert.Equal(SKColors.Crimson, bmp.GetPixel(70, 50));
						Assert.Equal(new SKColor(23, 35, 34), bmp.GetPixel(40, 40));
					}
				}
			}
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void DecodingWithPathAndDrawingOnGPUCreatesCorrectImage()
		{
			var info = new SKImageInfo(120, 120);
			var path = Path.Combine(PathToImages, "vimeo_icon_dark.png");

			using (var ctx = CreateGlContext())
			{
				ctx.MakeCurrent();

				using (var grContext = GRContext.CreateGl())
				using (var surface = SKSurface.Create(grContext, true, info))
				{
					var canvas = surface.Canvas;

					canvas.Clear(SKColors.Crimson);

					using (var image = SKImage.FromEncodedData(path))
					{
						canvas.DrawImage(image, 0, 0);
					}

					using (var bmp = new SKBitmap(info))
					{
						surface.ReadPixels(info, bmp.GetPixels(), info.RowBytes, 0, 0);

						Assert.Equal(SKColors.Crimson, bmp.GetPixel(3, 3));
						Assert.Equal(SKColors.Crimson, bmp.GetPixel(70, 50));
						Assert.Equal(new SKColor(23, 35, 34), bmp.GetPixel(40, 40));
					}
				}
			}
		}

		[SkippableFact]
		public void DecodingWithDataCreatesCorrectImage()
		{
			var info = new SKImageInfo(120, 120);
			var path = Path.Combine(PathToImages, "vimeo_icon_dark.png");

			using (var bmp = new SKBitmap(info))
			using (var canvas = new SKCanvas(bmp))
			{
				canvas.Clear(SKColors.Crimson);

				using (var data = SKData.Create(path))
				using (var image = SKImage.FromEncodedData(data))
				{
					canvas.DrawImage(image, 0, 0);
				}

				Assert.Equal(SKColors.Crimson, bmp.GetPixel(3, 3));
				Assert.Equal(SKColors.Crimson, bmp.GetPixel(70, 50));
				Assert.Equal(new SKColor(23, 35, 34), bmp.GetPixel(40, 40));
			}
		}

		[SkippableFact]
		public void DecodingWithBitmapCreatesCorrectImage()
		{
			var info = new SKImageInfo(120, 120);
			var path = Path.Combine(PathToImages, "vimeo_icon_dark.png");

			using (var bmp = new SKBitmap(info))
			using (var canvas = new SKCanvas(bmp))
			{
				canvas.Clear(SKColors.Crimson);

				using (var bitmap = SKBitmap.Decode(path))
				using (var image = SKImage.FromBitmap(bitmap))
				{
					canvas.DrawImage(image, 0, 0);
				}

				Assert.Equal(SKColors.Crimson, bmp.GetPixel(3, 3));
				Assert.Equal(SKColors.Crimson, bmp.GetPixel(70, 50));
				Assert.Equal(new SKColor(23, 35, 34), bmp.GetPixel(40, 40));
			}
		}

		[SkippableFact]
		public void DecodingWithPathCreatesCorrectImage()
		{
			var info = new SKImageInfo(120, 120);
			var path = Path.Combine(PathToImages, "vimeo_icon_dark.png");

			using (var bmp = new SKBitmap(info))
			using (var canvas = new SKCanvas(bmp))
			{
				canvas.Clear(SKColors.Crimson);

				using (var image = SKImage.FromEncodedData(path))
				{
					canvas.DrawImage(image, 0, 0);
				}

				Assert.Equal(SKColors.Crimson, bmp.GetPixel(3, 3));
				Assert.Equal(SKColors.Crimson, bmp.GetPixel(70, 50));
				Assert.Equal(new SKColor(23, 35, 34), bmp.GetPixel(40, 40));
			}
		}

		[SkippableTheory]
		[InlineData(0f, 0f, 0f, 0f)]
		[InlineData(0f, 0f, 1f, 1f)]
		[InlineData(0.5f, 0.5f, 1f, 1f)]
		public void SubsetEncodesSubset(float xRatio, float yRatio, float wRatio, float hRatio)
		{
			var path = Path.Combine(PathToImages, "baboon.jpg");
			using var image = SKImage.FromEncodedData(path);
			var width = image.Width;
			var height = image.Height;

			var rect = new SKRectI(0, 0, width, height);
			var subset = image;
			if (xRatio != 0 || yRatio != 0 || wRatio != 0 || hRatio != 0)
			{
				var floatingRect = new SKRect(width * xRatio, height * yRatio, width * wRatio, height * hRatio);
				rect = SKRectI.Floor(floatingRect);
				subset = image.Subset(rect);
			}

			using var encoded = subset.Encode();
			using var img2 = SKImage.FromEncodedData(encoded);

			Assert.Equal(rect.Width, img2.Width);
			Assert.Equal(rect.Height, img2.Height);

			var subsetPixels = GetPixels(subset);
			var img2Pixels = GetPixels(img2);

			Assert.Equal(subsetPixels, img2Pixels);

			static SKColor[] GetPixels(SKImage image)
			{
				using var bmp = new SKBitmap(image.Width, image.Height);
				using var cnv = new SKCanvas(bmp);
				cnv.DrawImage(image, 0, 0);
				return bmp.Pixels;
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

		[SkippableTheory]
		[InlineData("osm-liberty.png", 30, 240, 0xFF725A50)]
		[InlineData("testimage.png", 1040, 340, 0xFF0059FF)]
		public void CanDecodePotentiallyCorruptPngFiles(string filename, int x, int y, uint color)
		{
			var path = Path.Combine(PathToImages, filename);

			using var image = SKImage.FromEncodedData(path);
			using var actualImage = image.ToRasterImage(true);
			using var pixmap = actualImage.PeekPixels();

			Assert.Equal((SKColor)color, pixmap.GetPixelColor(x, y));
		}
	}
}

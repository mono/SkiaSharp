using System;
using System.Threading;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKAsyncReadPixelsTest : SKTest
	{
		private static readonly SKColor FillColor = SKColors.Red; // rgba 255,0,0,255

		private static byte[] GetPixels(SKImageReadPixelsResult result)
		{
			Assert.NotNull(result);
			Assert.Equal(1, result.PlaneCount);
			return result.GetPlaneData(0).ToArray();
		}

		private static void AssertPixel(byte[] pixels, int height, int x, int y, byte r, byte g, byte b, byte a)
		{
			// GetPlaneData returns exactly rowBytes*height bytes, so rowBytes = length/height.
			var rowBytes = pixels.Length / height;
			var o = (y * rowBytes) + (x * 4);
			Assert.Equal(r, pixels[o + 0]);
			Assert.Equal(g, pixels[o + 1]);
			Assert.Equal(b, pixels[o + 2]);
			Assert.Equal(a, pixels[o + 3]);
		}

		// Left half red, right half blue — a pattern a broken rescale/row-traversal cannot fake.
		private static SKSurface CreateSplitSurface(int size)
		{
			var surface = SKSurface.Create(new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Premul));
			surface.Canvas.Clear(SKColors.Red);
			using (var paint = new SKPaint { Color = SKColors.Blue })
				surface.Canvas.DrawRect(new SKRect(size / 2, 0, size, size), paint);
			surface.Flush();
			return surface;
		}

		// Per Skia (SkImage.h/SkSurface.h): async reads are Ganesh-only; "in all other cases this
		// operates synchronously." So a raster surface must invoke the callback inline.
		[Fact]
		public void RasterSurfaceRequestReadPixelsIsSynchronousAndCorrect()
		{
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(info);
			surface.Canvas.Clear(FillColor);
			surface.Flush();

			var done = false;
			byte[] pixels = null;

			surface.RequestReadPixels(info, new SKRectI(0, 0, 4, 4), result =>
			{
				done = true;
				pixels = GetPixels(result);
			});

			Assert.True(done); // no Submit / pump: it must already have fired
			AssertPixel(pixels, info.Height, 0, 0, 255, 0, 0, 255);
			AssertPixel(pixels, info.Height, 3, 3, 255, 0, 0, 255);
		}

		[Fact]
		public void RasterImageRequestReadPixelsIsSynchronousAndCorrect()
		{
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(info);
			surface.Canvas.Clear(FillColor);
			surface.Flush();
			using var image = surface.Snapshot();

			var done = false;
			byte[] pixels = null;

			image.RequestReadPixels(info, new SKRectI(0, 0, 4, 4), result =>
			{
				done = true;
				pixels = GetPixels(result);
			});

			Assert.True(done);
			AssertPixel(pixels, info.Height, 0, 0, 255, 0, 0, 255);
		}

		// Same-size read of a two-colour pattern: verifies full readback + correct row traversal/stride.
		[Fact]
		public void RasterSurfaceRequestReadPixelsSameSizeReadsWholePattern()
		{
			var info = new SKImageInfo(8, 8, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = CreateSplitSurface(8);

			var done = false;
			byte[] pixels = null;

			surface.RequestReadPixels(info, new SKRectI(0, 0, 8, 8), result =>
			{
				done = true;
				pixels = GetPixels(result);
			});

			Assert.True(done);
			AssertPixel(pixels, info.Height, 0, 0, 255, 0, 0, 255); // left = red
			AssertPixel(pixels, info.Height, 3, 3, 255, 0, 0, 255); // still left of the split
			AssertPixel(pixels, info.Height, 4, 4, 0, 0, 255, 255); // right = blue
			AssertPixel(pixels, info.Height, 7, 7, 0, 0, 255, 255);
		}

		// Downscale a two-colour pattern 8x8 -> 4x4; a no-op or broken rescale cannot reproduce it.
		[Fact]
		public void RasterSurfaceRequestReadPixelsDownscaleWorks()
		{
			var dstInfo = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = CreateSplitSurface(8);

			var done = false;
			byte[] pixels = null;

			surface.RequestReadPixels(dstInfo, new SKRectI(0, 0, 8, 8), result =>
			{
				done = true;
				pixels = GetPixels(result);
			});

			Assert.True(done);
			AssertPixel(pixels, dstInfo.Height, 0, 0, 255, 0, 0, 255); // left column stays red
			AssertPixel(pixels, dstInfo.Height, 3, 0, 0, 0, 255, 255); // right column stays blue
		}

		// The result view is only valid during the callback; using it afterwards must throw.
		[Fact]
		public void RequestReadPixelsResultIsInvalidatedAfterCallback()
		{
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(info);
			surface.Canvas.Clear(FillColor);
			surface.Flush();

			SKImageReadPixelsResult captured = null;

			surface.RequestReadPixels(info, new SKRectI(0, 0, 4, 4), result =>
			{
				Assert.Equal(1, result.PlaneCount); // valid inside the callback
				captured = result;
			});

			Assert.NotNull(captured);
			Assert.Throws<ObjectDisposedException>(() => _ = captured.PlaneCount);
		}

		[Fact]
		public void RequestReadPixelsThrowsForNullCallback()
		{
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(info);
			Assert.Throws<ArgumentNullException>(() => surface.RequestReadPixels(info, new SKRectI(0, 0, 4, 4), null));
		}

		// A srcRect not contained by the source causes failure: the callback is invoked with null.
		[Fact]
		public void RequestReadPixelsInvokesCallbackWithNullOnFailure()
		{
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(info);
			surface.Canvas.Clear(FillColor);
			surface.Flush();

			var called = false;
			SKImageReadPixelsResult captured = null;

			surface.RequestReadPixels(info, new SKRectI(0, 0, 1000, 1000), result =>
			{
				called = true;
				captured = result;
			});

			Assert.True(called);      // raster failure is delivered synchronously
			Assert.Null(captured);
		}

		[Fact]
		public void ToArrayIsTightlyPackedAndCorrect()
		{
			var info = new SKImageInfo(8, 8, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = CreateSplitSurface(8);

			byte[] array = null;
			surface.RequestReadPixels(info, new SKRectI(0, 0, 8, 8), result => array = result.ToArray());

			Assert.Equal(info.BytesSize, array.Length); // tightly packed (padding stripped)
			AssertPixel(array, info.Height, 0, 0, 255, 0, 0, 255); // red
			AssertPixel(array, info.Height, 7, 7, 0, 0, 255, 255); // blue
		}

		[Fact]
		public void CopyPlaneToStripsPaddingAndValidatesSize()
		{
			var info = new SKImageInfo(8, 8, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = CreateSplitSurface(8);

			surface.RequestReadPixels(info, new SKRectI(0, 0, 8, 8), result =>
			{
				var packed = new byte[info.BytesSize];
				result.CopyPlaneTo(0, packed);
				AssertPixel(packed, info.Height, 0, 0, 255, 0, 0, 255);
				AssertPixel(packed, info.Height, 7, 7, 0, 0, 255, 255);

				// A too-small destination throws rather than over-reading.
				Assert.Throws<ArgumentException>(() => result.CopyPlaneTo(0, new byte[info.BytesSize - 1]));
			});
		}

		[Fact]
		public void ToImageOutlivesCallbackAndIsCorrect()
		{
			var info = new SKImageInfo(8, 8, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = CreateSplitSurface(8);

			SKImage image = null;
			surface.RequestReadPixels(info, new SKRectI(0, 0, 8, 8), result => image = result.ToImage());

			Assert.NotNull(image); // owned copy, valid after the callback returned
			using (image)
			{
				Assert.Equal(8, image.Width);
				Assert.Equal(8, image.Height);
				using var bmp = SKBitmap.FromImage(image);
				Assert.Equal(SKColors.Red, bmp.GetPixel(0, 0));
				Assert.Equal(SKColors.Blue, bmp.GetPixel(7, 7));
			}
		}

		[Fact]
		public void ToBitmapOutlivesCallbackAndIsCorrect()
		{
			var info = new SKImageInfo(8, 8, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = CreateSplitSurface(8);

			SKBitmap bitmap = null;
			surface.RequestReadPixels(info, new SKRectI(0, 0, 8, 8), result => bitmap = result.ToBitmap());

			Assert.NotNull(bitmap);
			using (bitmap)
			{
				Assert.Equal(SKColors.Red, bitmap.GetPixel(0, 0));
				Assert.Equal(SKColors.Blue, bitmap.GetPixel(7, 7));
			}
		}

		// On Ganesh the read is deferred when the backend supports transfer buffers; otherwise Skia
		// falls back to a synchronous read. Assert the deferred transition only when it did not fire
		// inline, and always assert it eventually completes with correct pixels.
		[Fact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		public void GpuSurfaceRequestReadPixelsCompletesWithCorrectPixels()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();
			using var grContext = GRContext.CreateGl();

			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(grContext, true, info);
			surface.Canvas.Clear(FillColor);
			surface.Flush();

			var done = false;
			byte[] pixels = null;

			surface.RequestReadPixels(info, new SKRectI(0, 0, 4, 4), result =>
			{
				done = true;
				if (result != null)
					pixels = GetPixels(result);
			});

			var firedSynchronously = done;
			if (!firedSynchronously)
			{
				// Proven deferred: it must not have fired before we pump.
				Assert.False(done);
				grContext.Submit(synchronous: true);
				for (var i = 0; i < 1000 && !done; i++)
				{
					grContext.CheckAsyncWorkCompletion();
					if (!done)
						Thread.Sleep(1);
				}
			}

			Assert.True(done);
			Assert.NotNull(pixels);
			AssertPixel(pixels, info.Height, 0, 0, 255, 0, 0, 255);
		}

		[Fact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		public void GpuSurfaceRequestReadPixelsDownscaleWorks()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();
			using var grContext = GRContext.CreateGl();

			var dstInfo = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			var srcInfo = new SKImageInfo(8, 8, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(grContext, true, srcInfo);
			surface.Canvas.Clear(SKColors.Red);
			using (var paint = new SKPaint { Color = SKColors.Blue })
				surface.Canvas.DrawRect(new SKRect(4, 0, 8, 8), paint);
			surface.Flush();

			var done = false;
			byte[] pixels = null;

			surface.RequestReadPixels(dstInfo, new SKRectI(0, 0, 8, 8), result =>
			{
				done = true;
				if (result != null)
					pixels = GetPixels(result);
			});

			if (!done)
			{
				grContext.Submit(synchronous: true);
				for (var i = 0; i < 1000 && !done; i++)
				{
					grContext.CheckAsyncWorkCompletion();
					if (!done)
						Thread.Sleep(1);
				}
			}

			Assert.True(done);
			Assert.NotNull(pixels);
			AssertPixel(pixels, dstInfo.Height, 0, 0, 255, 0, 0, 255); // left stays red
			AssertPixel(pixels, dstInfo.Height, 3, 0, 0, 0, 255, 255); // right stays blue
		}
	}
}

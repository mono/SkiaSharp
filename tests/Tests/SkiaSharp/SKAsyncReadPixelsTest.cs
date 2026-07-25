using System;
using System.Threading;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKAsyncReadPixelsTest : SKTest
	{
		private static readonly SKColor FillColor = SKColors.Red; // (255, 0, 0, 255)

		private static void AssertIsFillColor(byte[] pixels)
		{
			Assert.Equal(255, pixels[0]); // R
			Assert.Equal(0, pixels[1]);   // G
			Assert.Equal(0, pixels[2]);   // B
			Assert.Equal(255, pixels[3]); // A
		}

		private static byte[] CopyFirstPlane(SKImageReadPixelsResult result, int rowCount)
		{
			Assert.NotNull(result);
			Assert.Equal(1, result.PlaneCount);
			var rowBytes = result.GetPlaneRowBytes(0);
			var pixels = new byte[rowBytes * rowCount];
			result.CopyPlaneTo(0, pixels, rowCount);
			return pixels;
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
				pixels = CopyFirstPlane(result, info.Height);
			});

			// No Submit / CheckAsyncWorkCompletion pump: it must already have fired.
			Assert.True(done);
			AssertIsFillColor(pixels);
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
				pixels = CopyFirstPlane(result, info.Height);
			});

			Assert.True(done);
			AssertIsFillColor(pixels);
		}

		// Reading a smaller destination than the source rectangle exercises the rescale path.
		[Fact]
		public void RasterSurfaceRequestReadPixelsDownscaleWorks()
		{
			var srcInfo = new SKImageInfo(8, 8, SKColorType.Rgba8888, SKAlphaType.Premul);
			var dstInfo = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(srcInfo);
			surface.Canvas.Clear(FillColor);
			surface.Flush();

			var done = false;
			byte[] pixels = null;

			surface.RequestReadPixels(dstInfo, new SKRectI(0, 0, 8, 8), result =>
			{
				done = true;
				pixels = CopyFirstPlane(result, dstInfo.Height);
			});

			Assert.True(done);
			// A solid fill downscales to the same solid color.
			AssertIsFillColor(pixels);
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
		public void RasterSurfaceRequestReadPixelsWholeImageOverloadWorks()
		{
			// The (info, callback) overload reads at 1:1 (srcRect == info size), so no rescale happens.
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(info);
			surface.Canvas.Clear(FillColor);
			surface.Flush();

			var done = false;
			byte[] pixels = null;

			surface.RequestReadPixels(info, result =>
			{
				done = true;
				pixels = CopyFirstPlane(result, info.Height);
			});

			Assert.True(done);
			AssertIsFillColor(pixels);
		}

		[Fact]
		public void RequestReadPixelsThrowsForNullCallback()
		{
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(info);
			Assert.Throws<ArgumentNullException>(() => surface.RequestReadPixels(info, new SKRectI(0, 0, 4, 4), null));
		}

		// On Ganesh the read is genuinely asynchronous: the callback must NOT fire during the Request
		// call. It only fires once work is submitted and CheckAsyncWorkCompletion is pumped. That
		// false -> (pump) -> true transition is the proof of asynchrony.
		[Fact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		public void GpuSurfaceRequestReadPixelsIsAsynchronousAndCorrect()
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
					pixels = CopyFirstPlane(result, info.Height);
			});

			// PROVE ASYNC: the callback is deferred on the GPU backend.
			Assert.False(done);

			// Drive the work to completion.
			grContext.Submit(synchronous: true);
			for (var i = 0; i < 1000 && !done; i++)
			{
				grContext.CheckAsyncWorkCompletion();
				if (!done)
					Thread.Sleep(1);
			}

			Assert.True(done);
			Assert.NotNull(pixels);
			AssertIsFillColor(pixels);
		}

		[Fact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		public void GpuSurfaceRequestReadPixelsDownscaleWorks()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();
			using var grContext = GRContext.CreateGl();

			var srcInfo = new SKImageInfo(8, 8, SKColorType.Rgba8888, SKAlphaType.Premul);
			var dstInfo = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(grContext, true, srcInfo);
			surface.Canvas.Clear(FillColor);
			surface.Flush();

			var done = false;
			byte[] pixels = null;

			surface.RequestReadPixels(dstInfo, new SKRectI(0, 0, 8, 8), result =>
			{
				done = true;
				if (result != null)
					pixels = CopyFirstPlane(result, dstInfo.Height);
			});

			grContext.Submit(synchronous: true);
			for (var i = 0; i < 1000 && !done; i++)
			{
				grContext.CheckAsyncWorkCompletion();
				if (!done)
					Thread.Sleep(1);
			}

			Assert.True(done);
			Assert.NotNull(pixels);
			AssertIsFillColor(pixels);
		}
	}
}

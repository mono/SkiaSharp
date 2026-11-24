using System;
using System.Threading;
using Xunit;

namespace SkiaSharp.Tests
{
	[Collection(Visual.GpuRenderingCollection.Name)]
	public class SKAsyncReadPixelsTest : SKTest
	{
		private const int SrcSize = 8;
		private const int OffsetX = 2;
		private const int OffsetY = 1;
		private const int DstSize = 4; // srcRect = (2,1)..(6,5), read 1:1 (no rescale)

		// A distinct colour per pixel so any stride/offset/padding mistake is caught.
		private static SKColor PatternColor(int x, int y) => new SKColor((byte)x, (byte)y, (byte)((x * 16) + y), 255);

		private static SKBitmap CreatePatternBitmap(int size)
		{
			var bmp = new SKBitmap(new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Premul));
			for (var y = 0; y < size; y++)
				for (var x = 0; x < size; x++)
					bmp.SetPixel(x, y, PatternColor(x, y));
			return bmp;
		}

		// Left half red, right half blue — a two-colour pattern for the rescale tests.
		private static SKSurface CreateSplitSurface(int size, GRContext context = null)
		{
			var info = new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Premul);
			var surface = context == null ? SKSurface.Create(info) : SKSurface.Create(context, true, info);
			surface.Canvas.Clear(SKColors.Red);
			using (var paint = new SKPaint { Color = SKColors.Blue })
				surface.Canvas.DrawRect(new SKRect(size / 2, 0, size, size), paint);
			surface.Flush();
			return surface;
		}

		private static void AssertMatchesPattern(byte[] pixels, int rowBytes, int width, int height, int offsetX, int offsetY)
		{
			for (var oy = 0; oy < height; oy++)
			{
				for (var ox = 0; ox < width; ox++)
				{
					var expected = PatternColor(offsetX + ox, offsetY + oy);
					var o = (oy * rowBytes) + (ox * 4);
					Assert.Equal(expected.Red, pixels[o + 0]);
					Assert.Equal(expected.Green, pixels[o + 1]);
					Assert.Equal(expected.Blue, pixels[o + 2]);
					Assert.Equal(expected.Alpha, pixels[o + 3]);
				}
			}
		}

		private static void AssertBitmapMatchesPattern(SKBitmap bmp, int width, int height, int offsetX, int offsetY)
		{
			for (var oy = 0; oy < height; oy++)
				for (var ox = 0; ox < width; ox++)
					Assert.Equal(PatternColor(offsetX + ox, offsetY + oy), bmp.GetPixel(ox, oy));
		}

		private static SKImageInfo DstInfo => new SKImageInfo(DstSize, DstSize, SKColorType.Rgba8888, SKAlphaType.Premul);
		private static SKRectI OffsetRect => new SKRectI(OffsetX, OffsetY, OffsetX + DstSize, OffsetY + DstSize);

		// ---- Deterministic padding-strip unit test (real GPU/raster RGBA reads are never padded) ----

		[Fact]
		public void CopyRowsStripsSourcePadding()
		{
			const int width = 4, height = 3, bpp = 4;
			const int packed = width * bpp;    // 16
			const int srcStride = packed + 4;  // 20 -> 4 bytes of padding per row

			var src = new byte[srcStride * height];
			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					var o = (y * srcStride) + (x * bpp);
					src[o + 0] = (byte)x;
					src[o + 1] = (byte)y;
					src[o + 2] = (byte)((x * 16) + y);
					src[o + 3] = 255;
				}
				for (var p = packed; p < srcStride; p++)
					src[(y * srcStride) + p] = 0xEE; // poison the padding
			}

			var dst = new byte[packed * height];
			SKImageReadPixelsResult.CopyRows(src, srcStride, dst, packed, height);

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					var o = (y * packed) + (x * bpp);
					Assert.Equal((byte)x, dst[o + 0]);
					Assert.Equal((byte)y, dst[o + 1]);
					Assert.Equal((byte)((x * 16) + y), dst[o + 2]);
					Assert.Equal((byte)255, dst[o + 3]);
				}
			}
			Assert.DoesNotContain((byte)0xEE, dst); // padding did not leak through
		}

		[Fact]
		public void CopyRowsWithoutPaddingIsExact()
		{
			const int stride = 8, height = 3;
			var src = new byte[stride * height];
			for (var i = 0; i < src.Length; i++)
				src[i] = (byte)(i + 1);

			var dst = new byte[stride * height];
			SKImageReadPixelsResult.CopyRows(src, stride, dst, stride, height);

			Assert.Equal(src, dst);
		}

		// ---- Per-pixel correctness with an OFFSET srcRect (via a raster SKImage; read is synchronous) ----

		[Fact]
		public void GetPlaneDataMatchesPatternAtOffset()
		{
			using var bmp = CreatePatternBitmap(SrcSize);
			using var image = SKImage.FromBitmap(bmp);

			var done = false;
			byte[] raw = null;
			var rowBytes = 0;
			image.RequestReadPixels(DstInfo, OffsetRect, r =>
			{
				done = true;
				Assert.Equal(1, r.PlaneCount);
				rowBytes = r.GetPlaneRowBytes(0);
				Assert.True(rowBytes >= DstInfo.RowBytes); // stride is at least the packed width
				raw = r.GetPlaneData(0).ToArray();         // raw view: length includes any padding
			});

			Assert.True(done);
			AssertMatchesPattern(raw, rowBytes, DstSize, DstSize, OffsetX, OffsetY);
		}

		[Fact]
		public void CopyPlaneToMatchesPatternAtOffset()
		{
			using var bmp = CreatePatternBitmap(SrcSize);
			using var image = SKImage.FromBitmap(bmp);

			var done = false;
			byte[] packed = null;
			image.RequestReadPixels(DstInfo, OffsetRect, r =>
			{
				done = true;
				packed = new byte[DstInfo.BytesSize];
				r.CopyPlaneTo(0, packed);
			});

			Assert.True(done);
			AssertMatchesPattern(packed, DstInfo.RowBytes, DstSize, DstSize, OffsetX, OffsetY);
		}

		[Fact]
		public void ToArrayMatchesPatternAtOffsetAndIsTightlyPacked()
		{
			using var bmp = CreatePatternBitmap(SrcSize);
			using var image = SKImage.FromBitmap(bmp);

			var done = false;
			byte[] packed = null;
			image.RequestReadPixels(DstInfo, OffsetRect, r =>
			{
				done = true;
				packed = r.ToArray();
			});

			Assert.True(done);
			Assert.Equal(DstInfo.BytesSize, packed.Length);
			AssertMatchesPattern(packed, DstInfo.RowBytes, DstSize, DstSize, OffsetX, OffsetY);
		}

		[Fact]
		public void ToImageMatchesPatternAtOffsetAndOutlivesCallback()
		{
			using var bmp = CreatePatternBitmap(SrcSize);
			using var image = SKImage.FromBitmap(bmp);

			SKImage result = null;
			image.RequestReadPixels(DstInfo, OffsetRect, r => result = r.ToImage());

			Assert.NotNull(result);
			using (result)
			{
				Assert.Equal(DstSize, result.Width);
				Assert.Equal(DstSize, result.Height);
				using var outBmp = SKBitmap.FromImage(result);
				AssertBitmapMatchesPattern(outBmp, DstSize, DstSize, OffsetX, OffsetY);
			}
		}

		[Fact]
		public void ToBitmapMatchesPatternAtOffsetAndOutlivesCallback()
		{
			using var bmp = CreatePatternBitmap(SrcSize);
			using var image = SKImage.FromBitmap(bmp);

			SKBitmap result = null;
			image.RequestReadPixels(DstInfo, OffsetRect, r => result = r.ToBitmap());

			Assert.NotNull(result);
			using (result)
				AssertBitmapMatchesPattern(result, DstSize, DstSize, OffsetX, OffsetY);
		}

		// ---- CopyPlaneTo destination-size edge cases ----

		[Fact]
		public void CopyPlaneToExactDestinationSucceeds()
		{
			using var bmp = CreatePatternBitmap(SrcSize);
			using var image = SKImage.FromBitmap(bmp);

			image.RequestReadPixels(DstInfo, OffsetRect, r =>
			{
				var exact = new byte[DstInfo.BytesSize];
				r.CopyPlaneTo(0, exact);
				AssertMatchesPattern(exact, DstInfo.RowBytes, DstSize, DstSize, OffsetX, OffsetY);
			});
		}

		[Fact]
		public void CopyPlaneToTooSmallDestinationThrows()
		{
			using var bmp = CreatePatternBitmap(SrcSize);
			using var image = SKImage.FromBitmap(bmp);

			image.RequestReadPixels(DstInfo, OffsetRect, r =>
			{
				Assert.Throws<ArgumentException>(() => r.CopyPlaneTo(0, new byte[DstInfo.BytesSize - 1]));
				Assert.Throws<ArgumentException>(() => r.CopyPlaneTo(0, Span<byte>.Empty));
			});
		}

		[Fact]
		public void CopyPlaneToOversizedDestinationCopiesPrefixAndLeavesTail()
		{
			using var bmp = CreatePatternBitmap(SrcSize);
			using var image = SKImage.FromBitmap(bmp);

			image.RequestReadPixels(DstInfo, OffsetRect, r =>
			{
				const int tail = 8;
				var big = new byte[DstInfo.BytesSize + tail];
				for (var i = 0; i < big.Length; i++)
					big[i] = 0xAB;

				r.CopyPlaneTo(0, big);

				AssertMatchesPattern(big, DstInfo.RowBytes, DstSize, DstSize, OffsetX, OffsetY);
				for (var i = DstInfo.BytesSize; i < big.Length; i++)
					Assert.Equal(0xAB, big[i]); // trailing bytes untouched
			});
		}

		[Fact]
		public void AccessorsThrowForInvalidPlaneIndex()
		{
			using var bmp = CreatePatternBitmap(SrcSize);
			using var image = SKImage.FromBitmap(bmp);

			image.RequestReadPixels(DstInfo, OffsetRect, r =>
			{
				foreach (var bad in new[] { -1, 1, 99 })
				{
					Assert.Throws<ArgumentOutOfRangeException>(() => r.GetPlaneRowBytes(bad));
					Assert.Throws<ArgumentOutOfRangeException>(() => { r.GetPlaneData(bad); });
					Assert.Throws<ArgumentOutOfRangeException>(() => r.CopyPlaneTo(bad, new byte[DstInfo.BytesSize]));
					Assert.Throws<ArgumentOutOfRangeException>(() => r.ToArray(bad));
				}
			});
		}

		// ---- Behaviour: raster is synchronous; failure/null; argument validation ----

		[Fact]
		public void RasterSurfaceRequestReadPixelsIsSynchronous()
		{
			var info = new SKImageInfo(SrcSize, SrcSize, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = CreateSplitSurface(SrcSize);

			var done = false;
			byte[] pixels = null;
			surface.RequestReadPixels(info, new SKRectI(0, 0, SrcSize, SrcSize), r =>
			{
				done = true;
				pixels = r.ToArray();
			});

			Assert.True(done); // no pump: raster fires inline
			// left half red, right half blue
			Assert.Equal(255, pixels[0]);                    // (0,0).R
			Assert.Equal(0, pixels[2]);                      // (0,0).B
			var right = (0 * info.RowBytes) + (7 * 4);
			Assert.Equal(0, pixels[right + 0]);              // (7,0).R
			Assert.Equal(255, pixels[right + 2]);            // (7,0).B
		}

		[Fact]
		public void RequestReadPixelsThrowsForNullCallback()
		{
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(info);
			using var image = SKImage.FromBitmap(CreatePatternBitmap(4));

			Assert.Throws<ArgumentNullException>(() => surface.RequestReadPixels(info, new SKRectI(0, 0, 4, 4), null));
			Assert.Throws<ArgumentNullException>(() => image.RequestReadPixels(info, new SKRectI(0, 0, 4, 4), null));
		}

		[Fact]
		public void RequestReadPixelsInvokesCallbackWithNullOnFailure()
		{
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(info);
			surface.Canvas.Clear(SKColors.Red);
			surface.Flush();

			var called = false;
			SKImageReadPixelsResult captured = null;
			surface.RequestReadPixels(info, new SKRectI(0, 0, 1000, 1000), r =>
			{
				called = true;
				captured = r;
			});

			Assert.True(called);    // raster failure delivered synchronously
			Assert.Null(captured);
		}

		[Fact]
		public void RasterSurfaceRequestReadPixelsDownscaleWorks()
		{
			var dstInfo = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = CreateSplitSurface(8);

			var done = false;
			byte[] pixels = null;
			surface.RequestReadPixels(dstInfo, new SKRectI(0, 0, 8, 8), r =>
			{
				done = true;
				pixels = r.ToArray();
			});

			Assert.True(done);
			Assert.Equal(255, pixels[0]);                          // (0,0) red (left)
			var right = (3 * 4);
			Assert.Equal(255, pixels[right + 2]);                  // (3,0) blue (right)
		}

		// ---- Using the result after the callback returned must throw cleanly, never crash ----

		[Fact]
		public void AllMembersThrowAfterCallbackReturns()
		{
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(info);
			surface.Canvas.Clear(SKColors.Red);
			surface.Flush();

			SKImageReadPixelsResult escaped = null;
			surface.RequestReadPixels(info, new SKRectI(0, 0, 4, 4), r => escaped = r);

			Assert.NotNull(escaped);
			Assert.Throws<ObjectDisposedException>(() => { _ = escaped.PlaneCount; });
			Assert.Throws<ObjectDisposedException>(() => escaped.GetPlaneRowBytes(0));
			Assert.Throws<ObjectDisposedException>(() => { escaped.GetPlaneData(0); });
			Assert.Throws<ObjectDisposedException>(() => escaped.CopyPlaneTo(0, new byte[info.BytesSize]));
			Assert.Throws<ObjectDisposedException>(() => escaped.ToArray());
			Assert.Throws<ObjectDisposedException>(() => escaped.ToImage());
			Assert.Throws<ObjectDisposedException>(() => escaped.ToBitmap());

			// Dispose is idempotent / safe to call again.
			escaped.Dispose();
			escaped.Dispose();
		}

		// ---- Ganesh GPU: genuinely deferred (when the backend supports it) + correct pixels ----

		[Fact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		public void GpuSurfaceRequestReadPixelsCompletesWithCorrectPixels()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();
			using var grContext = GRContext.CreateGl();

			var info = new SKImageInfo(8, 8, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = CreateSplitSurface(8, grContext);

			var done = false;
			byte[] pixels = null;
			surface.RequestReadPixels(info, new SKRectI(0, 0, 8, 8), r =>
			{
				done = true;
				if (r != null)
					pixels = r.ToArray();
			});

			var firedSynchronously = done;
			if (!firedSynchronously)
			{
				Assert.False(done); // proven deferred: must not fire before we pump
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
			Assert.Equal(255, pixels[0]);              // (0,0) red
			var right = (7 * 4);
			Assert.Equal(255, pixels[right + 2]);      // (7,0) blue
		}

		[Fact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		public void GpuSurfaceRequestReadPixelsDownscaleWorks()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();
			using var grContext = GRContext.CreateGl();

			var dstInfo = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = CreateSplitSurface(8, grContext);

			var done = false;
			byte[] pixels = null;
			surface.RequestReadPixels(dstInfo, new SKRectI(0, 0, 8, 8), r =>
			{
				done = true;
				if (r != null)
					pixels = r.ToArray();
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
			Assert.Equal(255, pixels[0]);              // (0,0) red
			Assert.Equal(255, pixels[(3 * 4) + 2]);    // (3,0) blue
		}
	}
}

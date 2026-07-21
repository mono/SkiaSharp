using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKSurfaceTest : SKTest
	{
		private void DrawGpuSurface(Action<SKSurface, SKImageInfo> draw)
		{
			using (var ctx = CreateGlContext())
			{
				ctx.MakeCurrent();

				var info = new SKImageInfo(100, 100);

				using (var grContext = GRContext.CreateGl())
				using (var surface = SKSurface.Create(grContext, true, info))
				{
					Assert.NotNull(surface);

					draw(surface, info);
				}
			}
		}

		private void DrawGpuTexture(Action<SKSurface, GRBackendTexture> draw)
		{
			using (var ctx = CreateGlContext())
			{
				ctx.MakeCurrent();

				// create the texture
				var textureInfo = ctx.CreateTexture(new SKSizeI(100, 100));
				var texture = new GRBackendTexture(100, 100, false, textureInfo);

				// create the surface
				using (var grContext = GRContext.CreateGl())
				using (var surface = SKSurface.Create(grContext, texture, SKColorType.Rgba8888))
				{
					Assert.NotNull(surface);

					draw(surface, texture);
				}

				// clean up
				ctx.DestroyTexture(textureInfo.Id);
			}
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GpuSurfaceHasCanvas()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			using var grContext = GRContext.CreateGl();
			using var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100));

			var canvas = surface.Canvas;
			Assert.NotNull(canvas);
		}

		[Fact]
		public void RasterSurfaceHasCanvas()
		{
			using var surface = SKSurface.Create(new SKImageInfo(100, 100));

			var canvas = surface.Canvas;
			Assert.NotNull(canvas);
		}

		[Fact]
		public void CanvasReturnsSameInstanceAcrossCalls()
		{
			using var surface = SKSurface.Create(new SKImageInfo(100, 100));

			var canvas1 = surface.Canvas;
			var canvas2 = surface.Canvas;

			// Skia caches the canvas for the surface's lifetime, so the managed
			// wrapper is cached too and the same instance is returned each call.
			Assert.NotNull(canvas1);
			Assert.Same(canvas1, canvas2);
			Assert.Equal(canvas1.Handle, canvas2.Handle);
		}

		[Fact]
		public void CanvasIsRefetchedAfterExplicitCanvasDispose()
		{
			using var surface = SKSurface.Create(new SKImageInfo(100, 100));

			var canvas1 = surface.Canvas;
			Assert.NotNull(canvas1);

			// Disposing the surface-owned canvas is an anti-pattern, but it must not
			// leave the cache holding a dead wrapper: the next access re-fetches a
			// fresh, usable canvas wrapping the still-alive native canvas.
			canvas1.Dispose();
			Assert.Equal(IntPtr.Zero, canvas1.Handle);

			var canvas2 = surface.Canvas;
			Assert.NotNull(canvas2);
			Assert.NotEqual(IntPtr.Zero, canvas2.Handle);
			Assert.NotSame(canvas1, canvas2);

			// The re-fetched canvas is fully functional.
			canvas2.Clear(SKColors.Red);
		}

		[Fact]
		public void SimpleSurfaceIsUnknownPixelGeometry()
		{
			var info = new SKImageInfo(100, 100);
			using (var surface = SKSurface.Create(info))
			{
				Assert.NotNull(surface);
				Assert.NotNull(surface.SurfaceProperties);
			}
		}

		[Fact]
		public void SimpleSurfaceWithPropertiesIsCorrect()
		{
			var info = new SKImageInfo(100, 100);
			var props = new SKSurfaceProperties(SKSurfacePropsFlags.UseDeviceIndependentFonts, SKPixelGeometry.RgbVertical);
			using (var surface = SKSurface.Create(info, props))
			{
				Assert.NotNull(surface);

				Assert.Equal(SKPixelGeometry.RgbVertical, surface.SurfaceProperties.PixelGeometry);
				Assert.Equal(SKSurfacePropsFlags.UseDeviceIndependentFonts, surface.SurfaceProperties.Flags);

				Assert.Equal(props.PixelGeometry, surface.SurfaceProperties.PixelGeometry);
				Assert.Equal(props.Flags, surface.SurfaceProperties.Flags);
			}
		}

		[Fact]
		public void Snapshot()
		{
			var info = new SKImageInfo(100, 100);
			using var surface = SKSurface.Create(info);
			Assert.NotNull(surface);

			var image = surface.Snapshot();
			Assert.NotNull(image);

			Assert.Equal(info.Width, image.Width);
			Assert.Equal(info.Height, image.Height);
		}

		[Fact]
		public void SnapshotReturnsSameInstance()
		{
			var info = new SKImageInfo(100, 100);
			using var surface = SKSurface.Create(info);
			Assert.NotNull(surface);

			var image1 = surface.Snapshot();
			Assert.NotNull(image1);
			var image2 = surface.Snapshot();
			Assert.NotNull(image2);

			Assert.Equal(image1, image2);
		}

		[Fact]
		public void SnapshotWithBoundsReturnsSameInstance()
		{
			var info = new SKImageInfo(100, 100);
			using var surface = SKSurface.Create(info);
			Assert.NotNull(surface);

			var image1 = surface.Snapshot();
			Assert.NotNull(image1);
			var image2 = surface.Snapshot(info.Rect);
			Assert.NotNull(image2);

			Assert.Equal(image1, image2);
		}

		[Fact]
		public void SnapshotWithBoundsReturnsDifferentInstance()
		{
			var info = new SKImageInfo(100, 100);
			using var surface = SKSurface.Create(info);
			Assert.NotNull(surface);

			var image1 = surface.Snapshot();
			Assert.NotNull(image1);
			var image2 = surface.Snapshot(new SKRectI(10, 10, 90, 90));
			Assert.NotNull(image2);

			Assert.NotEqual(image1, image2);
		}

		[Fact]
		public void SnapshotWithSameBoundsReturnsSameInstance()
		{
			var info = new SKImageInfo(100, 100);
			using var surface = SKSurface.Create(info);
			Assert.NotNull(surface);

			var image1 = surface.Snapshot(new SKRectI(10, 10, 90, 90));
			Assert.NotNull(image1);
			var image2 = surface.Snapshot(new SKRectI(10, 10, 90, 90));
			Assert.NotNull(image2);

			Assert.NotEqual(image1, image2);
		}

		[Fact]
		public void SnapshotWithDifferentBoundsReturnsDifferentInstance()
		{
			var info = new SKImageInfo(100, 100);
			using var surface = SKSurface.Create(info);
			Assert.NotNull(surface);

			var image1 = surface.Snapshot(new SKRectI(10, 20, 90, 90));
			Assert.NotNull(image1);
			var image2 = surface.Snapshot(new SKRectI(10, 10, 80, 90));
			Assert.NotNull(image2);

			Assert.NotEqual(image1, image2);
		}

		[Obsolete]
		[Fact]
		public void SimpleSurfaceWithPropsIsCorrect()
		{
			var info = new SKImageInfo(100, 100);
			var props = new SKSurfaceProperties(SKSurfacePropsFlags.UseDeviceIndependentFonts, SKPixelGeometry.RgbVertical);
			using (var surface = SKSurface.Create(info, props))
			{
				Assert.NotNull(surface);

				Assert.Equal(SKPixelGeometry.RgbVertical, surface.SurfaceProperties.PixelGeometry);
				Assert.Equal(SKSurfacePropsFlags.UseDeviceIndependentFonts, surface.SurfaceProperties.Flags);

				Assert.Equal(props.PixelGeometry, surface.SurfaceProperties.PixelGeometry);
				Assert.Equal(props.Flags, surface.SurfaceProperties.Flags);
			}
		}

		[Fact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void CanCreateSimpleSurface()
		{
			var info = new SKImageInfo(100, 100);
			using (var surface = SKSurface.Create(info))
			{
				Assert.NotNull(surface);
			}
		}

		[Fact]
		public void CanCreateSurfaceFromExistingMemory()
		{
			var info = new SKImageInfo(100, 100);
			var props = new SKSurfaceProperties(SKPixelGeometry.Unknown);

			var memory = Marshal.AllocCoTaskMem(info.BytesSize);

			using (var surface = SKSurface.Create(info, memory, info.RowBytes, props))
			{
				Assert.NotNull(surface);
			}

			Marshal.FreeCoTaskMem(memory);
		}

		[Fact]
		public void CanCreateSurfaceFromExistingMemoryUsingReleaseDelegate()
		{
			var hasReleased = false;

			var info = new SKImageInfo(100, 100);
			var props = new SKSurfaceProperties(SKPixelGeometry.Unknown);

			var memory = Marshal.AllocCoTaskMem(info.BytesSize);

			using (var surface = SKSurface.Create(info, memory, info.RowBytes, OnRelease, "Hello", props))
			{
				Assert.NotNull(surface);
			}

			Assert.True(hasReleased);

			void OnRelease(IntPtr address, object context)
			{
				Marshal.FreeCoTaskMem(memory);
				hasReleased = true;

				Assert.Equal("Hello", context);
			}
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GpuBackendSurfaceIsCreated()
		{
			DrawGpuSurface((surface, info) =>
			{
				Assert.NotNull(surface);

				var canvas = surface.Canvas;
				Assert.NotNull(canvas);

				canvas.Clear(SKColors.Transparent);
			});
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GpuTextureSurfaceIsCreated()
		{
			DrawGpuTexture((surface, texture) =>
			{
				Assert.NotNull(surface);

				var canvas = surface.Canvas;
				Assert.NotNull(canvas);

				canvas.Clear(SKColors.Transparent);
			});
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GpuTextureSurfaceCanBeRead()
		{
			DrawGpuTexture((surface, texture) =>
			{
				var canvas = surface.Canvas;

				canvas.Clear(SKColors.Red);
				canvas.Flush();

				using (var image = surface.Snapshot())
				{
					Assert.True(image.IsTextureBacked);

					using (var raster = image.ToRasterImage())
					{
						Assert.False(raster.IsTextureBacked);

						using (var bmp = SKBitmap.FromImage(raster))
						{
							Assert.Equal(SKColors.Red, bmp.GetPixel(0, 0));
						}
					}
				}
			});
		}

		[Fact]
		public void CanDrawSurfaceWithSamplingOptions()
		{
			// Create source surface with red fill (50x50)
			using var sourceSurface = SKSurface.Create(new SKImageInfo(50, 50));
			sourceSurface.Canvas.Clear(SKColors.Red);

			// Create destination surface (100x100)
			using var destSurface = SKSurface.Create(new SKImageInfo(100, 100));
			destSurface.Canvas.Clear(SKColors.Blue);

			// Draw source surface at (25, 25) with sampling options
			// Source occupies pixels 25-74 in both dimensions
			var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);
			sourceSurface.Draw(destSurface.Canvas, 25, 25, sampling, null);

			// Verify red pixel at center of drawn area (50, 50)
			using var snapshot = destSurface.Snapshot();
			using var pixmap = snapshot.PeekPixels();
			Assert.NotNull(pixmap);
			Assert.Equal(SKColors.Red, pixmap.GetPixelColor(50, 50));
		}

		[Fact]
		public void CanDrawSurfaceOnCanvasWithSamplingOptions()
		{
			// Create source surface with green fill (50x50)
			using var sourceSurface = SKSurface.Create(new SKImageInfo(50, 50));
			sourceSurface.Canvas.Clear(SKColors.Green);

			// Create destination surface (100x100)
			using var destSurface = SKSurface.Create(new SKImageInfo(100, 100));
			destSurface.Canvas.Clear(SKColors.White);

			// Draw using Canvas.DrawSurface at (10, 10) with sampling options
			// Source occupies pixels 10-59 in both dimensions
			var sampling = new SKSamplingOptions(SKFilterMode.Linear);
			destSurface.Canvas.DrawSurface(sourceSurface, new SKPoint(10, 10), sampling, null);

			// Verify green pixel at center of drawn area (30, 30)
			using var snapshot = destSurface.Snapshot();
			using var pixmap = snapshot.PeekPixels();
			Assert.NotNull(pixmap);
			Assert.Equal(SKColors.Green, pixmap.GetPixelColor(30, 30));
		}
	}
}

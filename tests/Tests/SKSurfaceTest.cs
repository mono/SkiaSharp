#if NET_STANDARD
#else
#define SYSTEM_DRAWING
#endif

using System;
using System.Runtime.InteropServices;
using Xunit;

#if SYSTEM_DRAWING
using System.Drawing;
#endif

namespace SkiaSharp.Tests
{
	public class SKSurfaceTest : SKTest
	{
		private void DrawGpuSurface(Action<SKSurface, SKImageInfo> draw)
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			var info = new SKImageInfo(100, 100);

			using var grContext = GRContext.CreateGl();
			using var surface = SKSurface.Create(grContext, true, info);

			Assert.NotNull(surface);

			draw(surface, info);
		}

		private void DrawGpuTexture(Action<SKSurface, GRBackendTexture> draw)
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			// create the texture
			var textureInfo = ctx.CreateTexture(new SKSizeI(100, 100));
			var texture = new GRBackendTexture(100, 100, false, textureInfo);

			// create the surface
			using (var grContext = GRContext.CreateGl())
			using (var surface = SKSurface.CreateAsRenderTarget(grContext, texture, SKColorType.Rgba8888))
			{
				Assert.NotNull(surface);

				draw(surface, texture);
			}

			// clean up
			ctx.DestroyTexture(textureInfo.Id);
		}

		[SkippableFact]
		public void SimpleSurfaceIsUnknownPixelGeometry()
		{
			var info = new SKImageInfo(100, 100);
			using var surface = SKSurface.Create(info);

			Assert.NotNull(surface);
			Assert.NotNull(surface.SurfaceProperties);
		}

		[SkippableFact]
		public void SimpleSurfaceWithPropertiesIsCorrect()
		{
			var info = new SKImageInfo(100, 100);
			var props = new SKSurfaceProperties(SKSurfacePropsFlags.UseDeviceIndependentFonts, SKPixelGeometry.RgbVertical);
			using var surface = SKSurface.Create(info, props);

			Assert.NotNull(surface);

			Assert.Equal(SKPixelGeometry.RgbVertical, surface.SurfaceProperties.PixelGeometry);
			Assert.Equal(SKSurfacePropsFlags.UseDeviceIndependentFonts, surface.SurfaceProperties.Flags);

			Assert.Equal(props.PixelGeometry, surface.SurfaceProperties.PixelGeometry);
			Assert.Equal(props.Flags, surface.SurfaceProperties.Flags);
		}

		[SkippableFact]
		public void CanCreateSimpleSurface()
		{
			var info = new SKImageInfo(100, 100);
			using var surface = SKSurface.Create(info);

			Assert.NotNull(surface);
		}

		[SkippableFact]
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

		[SkippableFact]
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

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
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

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
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

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void GpuTextureSurfaceCanBeRead()
		{
			DrawGpuTexture((surface, texture) =>
			{
				var canvas = surface.Canvas;

				canvas.Clear(SKColors.Red);
				canvas.Flush();

				using var image = surface.Snapshot();
				Assert.True(image.IsTextureBacked);

				using var raster = image.ToRasterImage();
				Assert.False(raster.IsTextureBacked);

				using var bmp = SKBitmap.FromImage(raster);
				Assert.Equal(SKColors.Red, bmp.GetPixel(0, 0));
			});
		}

#if SYSTEM_DRAWING
		private void DrawBitmap(Action<SKSurface, BitmapData> draw)
		{
			using var bitmap = new Bitmap(100, 100, PixelFormat.Format32bppPArgb);
			var data = bitmap.LockBits(new Rectangle(0, 0, 100, 100), ImageLockMode.WriteOnly, bitmap.PixelFormat);

			var info = new SKImageInfo(100, 100, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
			using (var surface = SKSurface.Create(info, data.Scan0, data.Stride))
			{
				Assert.NotNull(surface);

				draw(surface, data);
			}

			bitmap.UnlockBits(data);
		}

		[SkippableFact]
		public void SurfaceCanvasReturnTheSameInstance()
		{
			DrawBitmap((surface, data) =>
			{
				var skcanvas1 = surface.Canvas;
				var skcanvas2 = surface.Canvas;

				Assert.NotNull(skcanvas1);
				Assert.NotNull(skcanvas2);

				Assert.Equal(skcanvas1, skcanvas2);
				Assert.True(skcanvas1 == skcanvas2);

				Assert.Same(skcanvas1, skcanvas2);
			});
		}

		[SkippableFact]
		public void SecondSurfaceWasCreatedDifferent()
		{
			DrawBitmap((surface, data) =>
			{
				var info = new SKImageInfo(100, 100, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
				var surface2 = SKSurface.Create(info, data.Scan0, data.Stride);

				Assert.NotNull(surface2);

				Assert.NotEqual(surface, surface2);
				Assert.NotEqual(surface.Handle, surface2.Handle);

				surface2.Dispose();
			});
		}
#endif
	}
}

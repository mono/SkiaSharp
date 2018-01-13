#if NET_STANDARD
#else
#define SYSTEM_DRAWING
#endif

using System;
using Xunit;

#if SYSTEM_DRAWING
using System.Drawing;
using System.Drawing.Imaging;
#endif

namespace SkiaSharp.Tests
{
	public class SKSurfaceTest : SKTest
	{
		private const int width = 100;
		private const int height = 100;

		private void DrawGpuSurface(Action<SKSurface, SKImageInfo> draw)
		{
			using (var ctx = CreateGlContext())
			{
				ctx.MakeCurrent();

				var info = new SKImageInfo(100, 100);

				using (var grContext = GRContext.Create(GRBackend.OpenGL))
				using (var surface = SKSurface.Create(grContext, true, info))
				{
					Assert.NotNull(surface);

					draw(surface, info);
				}
			}
		}

		private void DrawGpuTexture(Action<SKSurface, GRGlBackendTextureDesc> draw)
		{
			using (var ctx = CreateGlContext())
			{
				ctx.MakeCurrent();

				// create the texture
				var textureInfo = ctx.CreateTexture(new SKSizeI(100, 100));
				var textureDesc = new GRGlBackendTextureDesc
				{
					Width = 100,
					Height = 100,
					Config = GRPixelConfig.Rgba8888,
					Flags = GRBackendTextureDescFlags.RenderTarget,
					Origin = GRSurfaceOrigin.TopLeft,
					SampleCount = 0,
					TextureHandle = textureInfo,
				};

				// create the surface
				using (var grContext = GRContext.Create(GRBackend.OpenGL))
				using (var surface = SKSurface.CreateAsRenderTarget(grContext, textureDesc))
				{
					Assert.NotNull(surface);

					draw(surface, textureDesc);
				}

				// clean up
				ctx.DestroyTexture(textureInfo.Id);
			}
		}

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

		[SkippableFact]
		public void GpuTextureSurfaceIsCreated()
		{
			DrawGpuTexture((surface, desc) =>
			{
				Assert.NotNull(surface);

				var canvas = surface.Canvas;
				Assert.NotNull(canvas);

				canvas.Clear(SKColors.Transparent);
			});
		}

		[SkippableFact]
		public void GpuTextureSurfaceCanBeRead()
		{
			DrawGpuTexture((surface, desc) =>
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

#if SYSTEM_DRAWING
		private void DrawBitmap(Action<SKSurface, BitmapData> draw)
		{
			using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb))
			{
				var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

				using (var surface = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, data.Scan0, data.Stride))
				{
					Assert.NotNull(surface);

					draw(surface, data);
				}

				bitmap.UnlockBits(data);
			}
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
				var surface2 = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, data.Scan0, data.Stride);

				Assert.NotNull(surface2);

				Assert.NotEqual(surface, surface2);
				Assert.NotEqual(surface.Handle, surface2.Handle);

				surface2.Dispose();
			});
		}
#endif
	}
}

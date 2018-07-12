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

		[Obsolete]
		private void DrawGpuTextureWithDesc(Action<SKSurface, GRGlBackendTextureDesc> draw)
		{
			using (var ctx = CreateGlContext())
			{
				ctx.MakeCurrent();

				// create the texture
				var textureInfo = ctx.CreateTexture(new SKSizeI(100, 100));
				// this is a new field that was added to the struct
				textureInfo.Format = 0;
				var textureDesc = new GRGlBackendTextureDesc
				{
					Width = 100,
					Height = 100,
					Config = GRPixelConfig.Rgba8888,
					Flags = GRBackendTextureDescFlags.RenderTarget,
					Origin = GRSurfaceOrigin.BottomLeft,
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

		private void DrawGpuTexture(Action<SKSurface, GRBackendTexture> draw)
		{
			using (var ctx = CreateGlContext())
			{
				ctx.MakeCurrent();

				// create the texture
				var textureInfo = ctx.CreateTexture(new SKSizeI(100, 100));
				var texture = new GRBackendTexture(100, 100, false, textureInfo);

				// create the surface
				using (var grContext = GRContext.Create(GRBackend.OpenGL))
				using (var surface = SKSurface.CreateAsRenderTarget(grContext, texture, SKColorType.Rgba8888))
				{
					Assert.NotNull(surface);

					draw(surface, texture);
				}

				// clean up
				ctx.DestroyTexture(textureInfo.Id);
			}
		}

		[Trait(Category, GpuCategory)]
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

		[Trait(Category, GpuCategory)]
		[SkippableFact]
		[Obsolete]
		public void GpuTextureSurfaceIsCreatedWithDesc()
		{
			DrawGpuTextureWithDesc((surface, desc) =>
			{
				Assert.NotNull(surface);

				var canvas = surface.Canvas;
				Assert.NotNull(canvas);

				canvas.Clear(SKColors.Transparent);
			});
		}

		[Trait(Category, GpuCategory)]
		[SkippableFact]
		[Obsolete]
		public void GpuTextureSurfaceCanBeReadWithDesc()
		{
			DrawGpuTextureWithDesc((surface, desc) =>
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

		[Trait(Category, GpuCategory)]
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

		[Trait(Category, GpuCategory)]
		[SkippableFact]
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

#if SYSTEM_DRAWING
		private void DrawBitmap(Action<SKSurface, BitmapData> draw)
		{
			using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb))
			{
				var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

				var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
				using (var surface = SKSurface.Create(info, data.Scan0, data.Stride))
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
				var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
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

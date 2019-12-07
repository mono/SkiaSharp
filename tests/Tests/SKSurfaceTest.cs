#if NET_STANDARD
#else
#define SYSTEM_DRAWING
#endif

using System;
using System.Runtime.InteropServices;
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

				using (var grContext = GRContext.CreateGl())
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
				using (var grContext = GRContext.CreateGl())
				using (var surface = SKSurface.CreateAsRenderTarget(grContext, textureDesc))
				{
					Assert.NotNull(surface);

					draw(surface, textureDesc);
				}

				// clean up
				ctx.DestroyTexture(textureInfo.Id);
			}
		}

		[Obsolete]
		[StructLayout(LayoutKind.Sequential)]
		public struct GRTextureInfoOld
		{
			public uint Target;
			public uint Id;
		}

		[Obsolete]
		private void DrawGpuTextureWithOldDesc(Action<SKSurface, GRBackendTextureDesc> draw)
		{
			using (var ctx = CreateGlContext())
			{
				ctx.MakeCurrent();

				// create the texture
				var textureInfo = ctx.CreateTexture(new SKSizeI(100, 100));

				var oldInfo = new GRTextureInfoOld
				{
					Id = textureInfo.Id,
					Target = textureInfo.Target
				};
				var textureHandle = GCHandle.Alloc(oldInfo, GCHandleType.Pinned);
				var textureDesc = new GRBackendTextureDesc
				{
					Width = 100,
					Height = 100,
					Config = GRPixelConfig.Rgba8888,
					Flags = GRBackendTextureDescFlags.RenderTarget,
					Origin = GRSurfaceOrigin.BottomLeft,
					SampleCount = 0,
					TextureHandle = textureHandle.AddrOfPinnedObject(),
				};

				// create the surface
				using (var grContext = GRContext.CreateGl())
				using (var surface = SKSurface.CreateAsRenderTarget(grContext, textureDesc))
				{
					Assert.NotNull(surface);

					draw(surface, textureDesc);
				}

				// clean up
				textureHandle.Free();
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
				using (var grContext = GRContext.CreateGl())
				using (var surface = SKSurface.CreateAsRenderTarget(grContext, texture, SKColorType.Rgba8888))
				{
					Assert.NotNull(surface);

					draw(surface, texture);
				}

				// clean up
				ctx.DestroyTexture(textureInfo.Id);
			}
		}

		[SkippableFact]
		public void SimpleSurfaceIsUnknownPixelGeometry()
		{
			var info = new SKImageInfo(100, 100);
			using (var surface = SKSurface.Create(info))
			{
				Assert.NotNull(surface);
				Assert.NotNull(surface.SurfaceProperties);
			}
		}

		[SkippableFact]
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

		[Obsolete]
		[SkippableFact]
		public void SimpleSurfaceWithPropsIsCorrect()
		{
			var info = new SKImageInfo(100, 100);
			var props = new SKSurfaceProps
			{
				Flags = SKSurfacePropsFlags.UseDeviceIndependentFonts,
				PixelGeometry = SKPixelGeometry.RgbVertical
			};
			using (var surface = SKSurface.Create(info, props))
			{
				Assert.NotNull(surface);

				Assert.Equal(SKPixelGeometry.RgbVertical, surface.SurfaceProperties.PixelGeometry);
				Assert.Equal(SKSurfacePropsFlags.UseDeviceIndependentFonts, surface.SurfaceProperties.Flags);

				Assert.Equal(props.PixelGeometry, surface.SurfaceProperties.PixelGeometry);
				Assert.Equal(props.Flags, surface.SurfaceProperties.Flags);

				Assert.Equal(props.PixelGeometry, surface.SurfaceProps.PixelGeometry);
				Assert.Equal(props.Flags, surface.SurfaceProps.Flags);
			}
		}

		[SkippableFact]
		public void CanCreateSimpleSurface()
		{
			var info = new SKImageInfo(100, 100);
			using (var surface = SKSurface.Create(info))
			{
				Assert.NotNull(surface);
			}
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

		[Obsolete]
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void CanConvertFromPointerToDescToTextureWithNewInfo()
		{
			// the custom struct to contain the info
			var oldInfo = new GRGlTextureInfo
			{
				Id = 123,
				Target = 456,
				Format = 789
			};

			// pin it for the native code
			var textureHandle = GCHandle.Alloc(oldInfo, GCHandleType.Pinned);

			// use the very old desc
			var textureDesc = new GRBackendTextureDesc
			{
				Width = 100,
				Height = 100,
				Config = GRPixelConfig.Rgba8888,
				Flags = GRBackendTextureDescFlags.RenderTarget,
				Origin = GRSurfaceOrigin.BottomLeft,
				SampleCount = 246,
				TextureHandle = textureHandle.AddrOfPinnedObject(),
			};

			// create the new texture
			var texture = new GRBackendTexture(textureDesc);

			// free up all resourcess
			textureHandle.Free();

			// make sure we kept the information
			Assert.Equal(100, texture.Width);
			Assert.Equal(100, texture.Height);
			var newInfo = texture.GetGlTextureInfo();
			Assert.Equal(oldInfo.Id, newInfo.Id);
			Assert.Equal(oldInfo.Target, newInfo.Target);
			Assert.Equal(GRPixelConfig.Rgba8888.ToGlSizedFormat(), newInfo.Format);
		}

		[Obsolete]
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void CanConvertFromPointerToDescToTexture()
		{
			// the custom struct to contain the info
			var oldInfo = new GRTextureInfoOld
			{
				Id = 123,
				Target = 456
			};

			// pin it for the native code
			var textureHandle = GCHandle.Alloc(oldInfo, GCHandleType.Pinned);

			// use the very old desc
			var textureDesc = new GRBackendTextureDesc
			{
				Width = 100,
				Height = 100,
				Config = GRPixelConfig.Rgba8888,
				Flags = GRBackendTextureDescFlags.RenderTarget,
				Origin = GRSurfaceOrigin.BottomLeft,
				SampleCount = 246,
				TextureHandle = textureHandle.AddrOfPinnedObject(),
			};

			// create the new texture
			var texture = new GRBackendTexture(textureDesc);

			// free up all resourcess
			textureHandle.Free();

			// make sure we kept the information
			Assert.Equal(100, texture.Width);
			Assert.Equal(100, texture.Height);
			var newInfo = texture.GetGlTextureInfo();
			Assert.Equal(oldInfo.Id, newInfo.Id);
			Assert.Equal(oldInfo.Target, newInfo.Target);
			Assert.Equal(GRPixelConfig.Rgba8888.ToGlSizedFormat(), newInfo.Format);
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

		[Obsolete]
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
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

		[Obsolete]
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
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
						Assert.NotSame(image, raster);

						using (var bmp = SKBitmap.FromImage(raster))
						{
							Assert.Equal(SKColors.Red, bmp.GetPixel(0, 0));
						}
					}
				}
			});
		}

		[Obsolete]
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void GpuTextureSurfaceIsCreatedWithOldDesc()
		{
			DrawGpuTextureWithOldDesc((surface, desc) =>
			{
				Assert.NotNull(surface);

				var canvas = surface.Canvas;
				Assert.NotNull(canvas);

				canvas.Clear(SKColors.Transparent);
			});
		}

		[Obsolete]
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void GpuTextureSurfaceCanBeReadWithOldDesc()
		{
			DrawGpuTextureWithOldDesc((surface, desc) =>
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

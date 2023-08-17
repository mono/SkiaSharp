﻿using System;
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

		[SkippableFact]
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

		[SkippableFact]
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

		[SkippableFact]
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

		[SkippableFact]
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

		[SkippableFact]
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

		[SkippableFact]
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
		[SkippableFact]
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

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
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

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
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

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
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
	}
}

#if SYSTEM_DRAWING

using System;
using System.Drawing;
using System.Drawing.Imaging;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SystemDrawingTest : SKTest
	{
		private void DrawBitmap(Action<SKSurface, BitmapData> draw)
		{
			using (var bitmap = new Bitmap(100, 100, PixelFormat.Format32bppPArgb))
			{
				var data = bitmap.LockBits(new Rectangle(0, 0, 100, 100), ImageLockMode.WriteOnly, bitmap.PixelFormat);

				var info = new SKImageInfo(100, 100, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
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
				var info = new SKImageInfo(100, 100, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
				var surface2 = SKSurface.Create(info, data.Scan0, data.Stride);

				Assert.NotNull(surface2);

				Assert.NotEqual(surface, surface2);
				Assert.NotEqual(surface.Handle, surface2.Handle);

				surface2.Dispose();
			});
		}
	}
}

#endif

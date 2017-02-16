using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKSurfaceTest : SKTest, IDisposable
	{
		protected const int width = 100;
		protected const int height = 100;

		protected Bitmap bitmap;

		public SKSurfaceTest()
		{
			bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
		}

		public void Dispose()
		{
			bitmap.Dispose();
			bitmap = null;
		}

		public void Draw(Action<SKSurface> draw)
		{
			var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

			using (var surface = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, data.Scan0, data.Stride))
			{
				draw(surface);
			}

			bitmap.UnlockBits(data);
		}

		[Test]
		public void SurfaceCanvasReturnTheSameInstance()
		{
			Draw(surface =>
			{
				var skcanvas1 = surface.Canvas;
				var skcanvas2 = surface.Canvas;

				Assert.NotNull(skcanvas1);
				Assert.NotNull(skcanvas2);

				Assert.AreEqual(skcanvas1, skcanvas2);
				Assert.True(skcanvas1 == skcanvas2);

				Assert.AreSame(skcanvas1, skcanvas2);
			});
		}

		[Test]
		public void SecondSurfaceWasCreatedDifferent()
		{
			var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

			var surface1 = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, data.Scan0, data.Stride);
			var surface2 = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, data.Scan0, data.Stride);

			Assert.NotNull(surface1);
			Assert.NotNull(surface2);

			Assert.AreNotEqual(surface1, surface2);
			Assert.AreNotEqual(surface1.Handle, surface2.Handle);

			surface1.Dispose();
			surface2.Dispose();

			bitmap.UnlockBits(data);
		}

		[Test]
		public void SurfaceWasCreated()
		{
			var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

			var surface = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, data.Scan0, data.Stride);

			Assert.NotNull(surface);
			Assert.AreNotEqual(IntPtr.Zero, surface.Handle);

			surface.Dispose();

			Assert.AreEqual(IntPtr.Zero, surface.Handle);

			bitmap.UnlockBits(data);
		}
	}
}

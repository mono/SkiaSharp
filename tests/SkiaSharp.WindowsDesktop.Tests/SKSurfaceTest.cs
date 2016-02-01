using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkiaSharp.WindowsDesktop.Tests
{
	[TestFixture]
	public class SKSurfaceTest : SKTest
	{
		[Test]
		public void SurfaceCanvasReturnTheSameInstance()
		{
			Draw(surface =>
			{
				var skcanvas1 = surface.Canvas;
				var skcanvas2 = surface.Canvas;

				Assert.IsNotNull(skcanvas1);
				Assert.IsNotNull(skcanvas2);

				Assert.AreEqual(skcanvas1, skcanvas2);
				Assert.IsTrue(skcanvas1 == skcanvas2);

				Assert.AreSame(skcanvas1, skcanvas2);
			});
		}

		[Test]
		public void SecondSurfaceWasCreatedDifferent()
		{
			var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

			var surface1 = SKSurface.Create(width, height, SKColorType.Bgra_8888, SKAlphaType.Premul, data.Scan0, width * 4);
			var surface2 = SKSurface.Create(width, height, SKColorType.Bgra_8888, SKAlphaType.Premul, data.Scan0, width * 4);

			Assert.IsNotNull(surface1);
			Assert.IsNotNull(surface2);

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

			var surface = SKSurface.Create(width, height, SKColorType.Bgra_8888, SKAlphaType.Premul, data.Scan0, width * 4);

			Assert.IsNotNull(surface);
			Assert.AreNotEqual(IntPtr.Zero, surface.Handle);

			surface.Dispose();

			Assert.AreEqual(IntPtr.Zero, surface.Handle);

			bitmap.UnlockBits(data);
		}
	}
}

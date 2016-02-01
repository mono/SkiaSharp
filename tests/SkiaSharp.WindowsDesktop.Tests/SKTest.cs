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
	public abstract class SKTest
	{
		protected const int width = 100;
		protected const int height = 100;

		protected Bitmap bitmap;

		[SetUp]
		public void Setup()
		{
			bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
		}

		[TearDown]
		public void TearDown()
		{
			bitmap.Dispose();
			bitmap = null;
		}
		
		public void Draw(Action<SKSurface> draw)
		{
			var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

			using (var surface = SKSurface.Create(width, height, SKColorType.Bgra_8888, SKAlphaType.Premul, data.Scan0, width * 4))
			{
				draw(surface);
			}

			bitmap.UnlockBits(data);
		}
	}
}

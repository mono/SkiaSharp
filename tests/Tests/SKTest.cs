using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace SkiaSharp.Tests
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

			using (var surface = SKSurface.Create(width, height, SKColorType.N_32, SKAlphaType.Premul, data.Scan0, data.Stride))
			{
				draw(surface);
			}

			bitmap.UnlockBits(data);
		}
	}
}

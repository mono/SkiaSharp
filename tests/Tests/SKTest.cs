using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	[TestFixture]
	public abstract class SKTest
	{
		protected const string PathToFonts = "fonts";
		protected const string PathToImages = "images";

		protected const int width = 100;
		protected const int height = 100;

		protected static bool IsUnix => Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix;
		protected static bool IsWindows => !IsUnix;

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

			using (var surface = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, data.Scan0, data.Stride))
			{
				draw(surface);
			}

			bitmap.UnlockBits(data);
		}
	}
}

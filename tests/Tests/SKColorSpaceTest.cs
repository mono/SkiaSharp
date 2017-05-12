using System;
using System.IO;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKColorSpaceTest : SKTest
	{
		[Test]
		public void CanCreateSrgb()
		{
			var colorspace = SKColorSpace.CreateSrgb();

			Assert.IsNotNull(colorspace);
			Assert.IsTrue(SKColorSpace.Equal(colorspace, colorspace));
		}

		[Test]
		public void ImageInfoHasColorSpace()
		{
			var colorspace = SKColorSpace.CreateSrgb();

			var info = new SKImageInfo(100, 100, SKImageInfo.PlatformColorType, SKAlphaType.Premul, colorspace);
			Assert.AreEqual(colorspace, info.ColorSpace);

			var image = SKImage.Create(info);
			Assert.AreEqual(colorspace, image.PeekPixels().ColorSpace);
		}

		[Test]
		public void SrgbColorSpaceIsCloseToSrgb()
		{
			var colorspace = SKColorSpace.CreateSrgb();

			Assert.IsTrue(colorspace.GammaIsCloseToSrgb);
		}
	}
}

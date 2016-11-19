using System;
using System.IO;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	[TestFixture]
	public class SKSvgTest : SKTest
	{
		[Test]
		public void LoadSvgCanvasSize()
		{
			var path = Path.Combine (PathToImages, "logos.svg");

			var svg = new SKSvg();
			svg.Load(path);

			Assert.AreEqual(new SKSize(300, 300), svg.CanvasSize);
		}

		[Test]
		public void LoadSvgCustomCanvasSize()
		{
			var path = Path.Combine (PathToImages, "logos.svg");

			var svg = new SKSvg(new SKSize(150, 150));
			svg.Load(path);

			Assert.AreEqual(new SKSize(150, 150), svg.CanvasSize);
		}

		[Test]
		public void SvgLoadsToBitmap()
		{
			var path = Path.Combine (PathToImages, "logos.svg");
			var background = (SKColor)0xfff8f8f8;

			var svg = new SKSvg();
			svg.Load(path);

			var bmp = new SKBitmap((int)svg.CanvasSize.Width, (int)svg.CanvasSize.Height);
			var canvas = new SKCanvas(bmp);
			canvas.DrawPicture(svg.Picture);
			canvas.Flush();

			Assert.AreEqual(background, bmp.GetPixel(0,0));
		}
	}
}

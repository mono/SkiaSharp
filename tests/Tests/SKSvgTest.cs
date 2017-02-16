using System;
using System.IO;
using NUnit.Framework;
using System.Xml.Linq;

namespace SkiaSharp.Tests
{
	public class SKSvgTest : SKTest
	{
		[Test]
		public void LoadSvgCanvasSize()
		{
			var path = Path.Combine(PathToImages, "logos.svg");

			var svg = new SKSvg();
			svg.Load(path);

			Assert.AreEqual(new SKSize(300, 300), svg.CanvasSize);
		}

		[Test]
		public void LoadSvgCustomCanvasSize()
		{
			var path = Path.Combine(PathToImages, "logos.svg");

			var svg = new SKSvg(new SKSize(150, 150));
			svg.Load(path);

			Assert.AreEqual(new SKSize(150, 150), svg.CanvasSize);
		}

		[Test]
		public void SvgLoadsToBitmap()
		{
			var path = Path.Combine(PathToImages, "logos.svg");
			var background = (SKColor)0xfff8f8f8;

			var svg = new SKSvg();
			svg.Load(path);

			var bmp = new SKBitmap((int)svg.CanvasSize.Width, (int)svg.CanvasSize.Height);
			var canvas = new SKCanvas(bmp);
			canvas.DrawPicture(svg.Picture);
			canvas.Flush();

			Assert.AreEqual(background, bmp.GetPixel(0, 0));
		}

		[Test]
		public void SvgCanvasCreatesValidDrawing()
		{
			using (var stream = new MemoryStream())
			{
				// draw the SVG
				using (var skStream = new SKManagedWStream(stream, false))
				using (var writer = new SKXmlStreamWriter(skStream))
				using (var canvas = SKSvgCanvas.Create(SKRect.Create(200, 150), writer))
				{
					var rectPaint = new SKPaint { Color = SKColors.Blue, Style = SKPaintStyle.Fill };
					canvas.DrawRect(SKRect.Create(50, 70, 100, 30), rectPaint);

					var circlePaint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Fill };
					canvas.DrawOval(SKRect.Create(50, 70, 100, 30), circlePaint);

					skStream.Flush();
				}

				// reset the sream
				stream.Position = 0;

				// read the SVG
				var xdoc = XDocument.Load(stream);
				var svg = xdoc.Root;

				var ns = (XNamespace)"http://www.w3.org/2000/svg";

				Assert.AreEqual(ns, svg.GetDefaultNamespace());
				Assert.AreEqual("200", svg.Attribute("width").Value);
				Assert.AreEqual("150", svg.Attribute("height").Value);

				var rect = svg.Element(ns + "rect");
				Assert.AreEqual("rgb(0,0,255)", rect.Attribute("fill").Value);
				Assert.AreEqual("50", rect.Attribute("x").Value);
				Assert.AreEqual("70", rect.Attribute("y").Value);
				Assert.AreEqual("100", rect.Attribute("width").Value);
				Assert.AreEqual("30", rect.Attribute("height").Value);

				var ellipse = svg.Element(ns + "ellipse");
				Assert.AreEqual("rgb(255,0,0)", ellipse.Attribute("fill").Value);
				Assert.AreEqual("100", ellipse.Attribute("cx").Value);
				Assert.AreEqual("85", ellipse.Attribute("cy").Value);
				Assert.AreEqual("50", ellipse.Attribute("rx").Value);
				Assert.AreEqual("15", ellipse.Attribute("ry").Value);
			}
		}
	}
}

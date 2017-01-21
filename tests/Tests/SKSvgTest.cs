using System;
using System.IO;
using Xunit;
using System.Xml.Linq;

namespace SkiaSharp.Tests
{
	public class SKSvgTest : SKTest
	{
		[Fact]
		public void LoadSvgCanvasSize()
		{
			var path = Path.Combine(PathToImages, "logos.svg");

			var svg = new SKSvg();
			svg.Load(path);

			Assert.Equal(new SKSize(300, 300), svg.CanvasSize);
		}

		[Fact]
		public void LoadSvgCustomCanvasSize()
		{
			var path = Path.Combine(PathToImages, "logos.svg");

			var svg = new SKSvg(new SKSize(150, 150));
			svg.Load(path);

			Assert.Equal(new SKSize(150, 150), svg.CanvasSize);
		}

		[Fact]
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

			Assert.Equal(background, bmp.GetPixel(0, 0));
		}

		[Fact]
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

				Assert.Equal(ns, svg.GetDefaultNamespace());
				Assert.Equal("200", svg.Attribute("width").Value);
				Assert.Equal("150", svg.Attribute("height").Value);

				var rect = svg.Element(ns + "rect");
				Assert.Equal("rgb(0,0,255)", rect.Attribute("fill").Value);
				Assert.Equal("50", rect.Attribute("x").Value);
				Assert.Equal("70", rect.Attribute("y").Value);
				Assert.Equal("100", rect.Attribute("width").Value);
				Assert.Equal("30", rect.Attribute("height").Value);

				var ellipse = svg.Element(ns + "ellipse");
				Assert.Equal("rgb(255,0,0)", ellipse.Attribute("fill").Value);
				Assert.Equal("100", ellipse.Attribute("cx").Value);
				Assert.Equal("85", ellipse.Attribute("cy").Value);
				Assert.Equal("50", ellipse.Attribute("rx").Value);
				Assert.Equal("15", ellipse.Attribute("ry").Value);
			}
		}
	}
}

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;
using Xunit.Categories;

namespace SkiaSharp.Tests
{
	public class SKCanvasTest : SKTest
	{
		[Category(GpuCategory)]
		[SkippableFact]
		public void CanvasCanRestoreOnGpu()
		{
			using (var ctx = CreateGlContext())
			{
				ctx.MakeCurrent();

				using (var grContext = GRContext.Create(GRBackend.OpenGL))
				using (var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100)))
				{
					var canvas = surface.Canvas;

					Assert.Equal(SKMatrix.MakeIdentity(), canvas.TotalMatrix);

					using (new SKAutoCanvasRestore(canvas))
					{
						canvas.Translate(10, 10);
						Assert.Equal(SKMatrix.MakeTranslation(10, 10), canvas.TotalMatrix);
					}

					Assert.Equal(SKMatrix.MakeIdentity(), canvas.TotalMatrix);
				}
			}
		}

		[SkippableFact]
		public void DrawTextBlobIsTheSameAsDrawText()
		{
			var info = new SKImageInfo(300, 300);
			var text = "SkiaSharp";

			byte[] textPixels;
			using (var bmp = new SKBitmap(info))
			using (var canvas = new SKCanvas(bmp))
			using (var paint = new SKPaint())
			{
				paint.TextSize = 50;
				paint.TextAlign = SKTextAlign.Center;

				canvas.Clear(SKColors.White);
				canvas.DrawText(text, 150, 175, paint);

				textPixels = bmp.Bytes;
			}

			byte[] glyphsPixels;
			using (var bmp = new SKBitmap(info))
			using (var canvas = new SKCanvas(bmp))
			using (var paint = new SKPaint())
			{
				ushort[] glyphs;
				using (var glyphsp = new SKPaint())
					glyphs = glyphsp.GetGlyphs(text);

				paint.TextSize = 50;
				paint.TextAlign = SKTextAlign.Center;
				paint.TextEncoding = SKTextEncoding.GlyphId;

				canvas.Clear(SKColors.White);
				using (var builder = new SKTextBlobBuilder())
				{
					builder.AddRun(paint, 0, 0, glyphs);
					canvas.DrawText(builder.Build(), 150, 175, paint);
				}

				glyphsPixels = bmp.Bytes;
			}

			Assert.Equal(textPixels, glyphsPixels);
		}

		[SkippableFact]
		public void CanDrawText()
		{
			using (var bmp = new SKBitmap(new SKImageInfo(300, 300)))
			using (var canvas = new SKCanvas(bmp))
			using (var paint = new SKPaint())
			{
				canvas.DrawText("text", 150, 175, paint);
			}
		}

		[SkippableFact]
		public void CanDrawEmptyText()
		{
			using (var bmp = new SKBitmap(new SKImageInfo(300, 300)))
			using (var canvas = new SKCanvas(bmp))
			using (var paint = new SKPaint())
			{
				canvas.DrawText("", 150, 175, paint);
			}
		}

		[SkippableFact]
		public void CanDrawNullPointerZeroLengthText()
		{
			using (var bmp = new SKBitmap(new SKImageInfo(300, 300)))
			using (var canvas = new SKCanvas(bmp))
			using (var paint = new SKPaint())
			{
				canvas.DrawText(IntPtr.Zero, 0, 150, 175, paint);
			}
		}

		[SkippableFact]
		public void ThrowsOnDrawNullPointerText()
		{
			using (var bmp = new SKBitmap(new SKImageInfo(300, 300)))
			using (var canvas = new SKCanvas(bmp))
			using (var paint = new SKPaint())
			{
				Assert.Throws<ArgumentNullException>(() => canvas.DrawText(IntPtr.Zero, 123, 150, 175, paint));
			}
		}

		[SkippableFact]
		public void CanvasCanClipRoundRect()
		{
			using (var canvas = new SKNWayCanvas(100, 100))
			{
				canvas.ClipRoundRect(new SKRoundRect(new SKRect(10, 10, 50, 50), 5, 5));
			}
		}

		[SkippableFact]
		public void CanvasCanDrawRoundRect()
		{
			using (var canvas = new SKNWayCanvas(100, 100))
			using (var paint = new SKPaint())
			{
				canvas.DrawRoundRect(new SKRoundRect(new SKRect(10, 10, 50, 50), 5, 5), paint);
			}
		}

		[SkippableFact]
		public void NWayCanvasCanBeConstructed()
		{
			using (var canvas = new SKNWayCanvas(100, 100))
			{
				Assert.NotNull(canvas);
			}
		}

		[SkippableFact]
		public void NWayCanvasDrawsToMultipleCanvases()
		{
			using (var firstBitmap = new SKBitmap(new SKImageInfo(100, 100)))
			using (var first = new SKCanvas(firstBitmap))
			using (var secondBitmap = new SKBitmap(new SKImageInfo(100, 100)))
			using (var second = new SKCanvas(secondBitmap))
			{
				first.Clear(SKColors.Red);
				second.Clear(SKColors.Green);

				using (var canvas = new SKNWayCanvas(100, 100))
				{
					canvas.AddCanvas(first);
					canvas.AddCanvas(second);

					canvas.Clear(SKColors.Blue);

					Assert.Equal(SKColors.Blue, firstBitmap.GetPixel(0, 0));
					Assert.Equal(SKColors.Blue, secondBitmap.GetPixel(0, 0));

					canvas.Clear(SKColors.Orange);
				}

				Assert.Equal(SKColors.Orange, firstBitmap.GetPixel(0, 0));
				Assert.Equal(SKColors.Orange, secondBitmap.GetPixel(0, 0));
			}
		}

		[SkippableFact]
		public void OverdrawCanvasDrawsProperly()
		{
			using (var bitmap = new SKBitmap(new SKImageInfo(100, 100)))
			using (var canvas = new SKCanvas(bitmap))
			using (var overdraw = new SKOverdrawCanvas(canvas))
			{
				bitmap.Erase(SKColors.Transparent);

				overdraw.DrawRect(SKRect.Create(10, 10, 30, 30), new SKPaint());
				overdraw.DrawRect(SKRect.Create(20, 20, 30, 30), new SKPaint());

				Assert.Equal(0, bitmap.GetPixel(5, 5).Alpha);
				Assert.Equal(1, bitmap.GetPixel(15, 15).Alpha);
				Assert.Equal(2, bitmap.GetPixel(25, 25).Alpha);
				Assert.Equal(1, bitmap.GetPixel(45, 45).Alpha);
			}
		}

		[SkippableFact]
		public void TotalMatrixIsCorrect()
		{
			using (var bitmap = new SKBitmap(new SKImageInfo(100, 100)))
			using (var canvas = new SKCanvas(bitmap))
			{
				canvas.Translate(10, 20);
				Assert.Equal(SKMatrix.MakeTranslation(10, 20).Values, canvas.TotalMatrix.Values);

				canvas.Translate(10, 20);
				Assert.Equal(SKMatrix.MakeTranslation(20, 40).Values, canvas.TotalMatrix.Values);
			}
		}

		[SkippableFact]
		public void SvgCanvasSavesFile()
		{
			var stream = new MemoryStream();

			using (var wstream = new SKManagedWStream(stream))
			using (var writer = new SKXmlStreamWriter(wstream))
			using (var svg = SKSvgCanvas.Create(SKRect.Create(100, 100), writer))
			{
				var paint = new SKPaint
				{
					Color = SKColors.Red,
					Style = SKPaintStyle.Fill
				};
				svg.DrawRect(SKRect.Create(10, 10, 80, 80), paint);
			}

			stream.Position = 0;

			using (var reader = new StreamReader(stream))
			{
				var xml = reader.ReadToEnd();
				var xdoc = XDocument.Parse(xml);

				var svg = xdoc.Root;
				var ns = svg.Name.Namespace;

				Assert.Equal(ns + "svg", svg.Name);
				Assert.Equal("100", svg.Attribute("width")?.Value);
				Assert.Equal("100", svg.Attribute("height")?.Value);

				var rect = svg.Element(ns + "rect");
				Assert.Equal(ns + "rect", rect.Name);
				Assert.Equal("rgb(255,0,0)", rect.Attribute("fill")?.Value);
				Assert.Equal("none", rect.Attribute("stroke")?.Value);
				Assert.Equal("10", rect.Attribute("x")?.Value);
				Assert.Equal("10", rect.Attribute("y")?.Value);
				Assert.Equal("80", rect.Attribute("width")?.Value);
				Assert.Equal("80", rect.Attribute("height")?.Value);
			}
		}
	}
}

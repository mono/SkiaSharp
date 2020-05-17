using System;
using System.IO;
using System.Xml.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKCanvasTest : SKTest
	{
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void CanvasCanRestoreOnGpu()
		{
			using (var ctx = CreateGlContext())
			{
				ctx.MakeCurrent();

				using (var grContext = GRContext.CreateGl())
				using (var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100)))
				{
					var canvas = surface.Canvas;

					Assert.Equal(SKMatrix.Identity, canvas.TotalMatrix);

					using (new SKAutoCanvasRestore(canvas))
					{
						canvas.Translate(10, 10);
						Assert.Equal(SKMatrix.CreateTranslation(10, 10), canvas.TotalMatrix);
					}

					Assert.Equal(SKMatrix.Identity, canvas.TotalMatrix);
				}
			}
		}

		[SkippableTheory]
		[InlineData(SKTextAlign.Left)]
		//[InlineData(SKTextAlign.Center)]
		//[InlineData(SKTextAlign.Right)]
		public void DrawTextBlobIsTheSameAsDrawText(SKTextAlign align)
		{
			var info = new SKImageInfo(300, 300);
			var text = "SkiaSharp";

			byte[] textPixels;
			using (var bmp = new SKBitmap(info))
			using (var canvas = new SKCanvas(bmp))
			using (var paint = new SKPaint())
			{
				paint.TextSize = 50;
				paint.TextAlign = align;

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
				paint.TextAlign = align;
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
		public void CanDrawRoundRectDifference()
		{
			using var outer = new SKRoundRect(SKRect.Create(50, 50, 200, 200), 20);
			using var inner = new SKRoundRect(SKRect.Create(100, 100, 100, 100), 20);

			using var paint = new SKPaint();

			SKColor[] diff;
			using (var bmp = new SKBitmap(new SKImageInfo(300, 300)))
			using (var canvas = new SKCanvas(bmp))
			{
				canvas.Clear(SKColors.Transparent);
				canvas.DrawRoundRectDifference(outer, inner, paint);

				diff = bmp.Pixels;
			}

			SKColor[] paths;
			using (var bmp = new SKBitmap(new SKImageInfo(300, 300)))
			using (var canvas = new SKCanvas(bmp))
			using (var path = new SKPath())
			{
				canvas.Clear(SKColors.Transparent);

				path.AddRoundRect(outer);
				path.AddRoundRect(inner);
				path.FillType = SKPathFillType.EvenOdd;

				canvas.DrawPath(path, paint);

				paths = bmp.Pixels;
			}

			Assert.Equal(paths, diff);
		}

		[SkippableFact]
		public void DrawAtlasThrowsOnMismatchingArgs()
		{
			using var bmp = new SKBitmap(new SKImageInfo(300, 300));
			using var img = SKImage.FromBitmap(bmp);
			using var canvas = new SKCanvas(bmp);

			using var paint = new SKPaint();
			var sprites = new[] { SKRect.Empty, SKRect.Empty };
			var transforms = new[] { SKRotationScaleMatrix.Empty };

			Assert.Throws<ArgumentException>("transforms", () => canvas.DrawAtlas(img, sprites, transforms, paint));
		}

		[SkippableFact]
		public void CanDrawPatch()
		{
			var cubics = new SKPoint[12] {
				// top points
				new SKPoint(100, 100), new SKPoint(150, 50), new SKPoint(250, 150), new SKPoint(300, 100),
				// right points
				new SKPoint(250, 150), new SKPoint(350, 250),
				// bottom points
				new SKPoint(300, 300), new SKPoint(250, 250), new SKPoint(150, 350), new SKPoint(100, 300),
				// left points
				new SKPoint(50, 250), new SKPoint(150, 150)
			};

			var baboon = SKImage.FromEncodedData(Path.Combine(PathToImages, "baboon.jpg"));
			var tex = new SKPoint[4] {
				new SKPoint(0, 0),
				new SKPoint(baboon.Width, 0),
				new SKPoint(baboon.Width, baboon.Height),
				new SKPoint(0, baboon.Height),
			};

			using var bmp = new SKBitmap(new SKImageInfo(400, 400));
			using var canvas = new SKCanvas(bmp);
			using var paint = new SKPaint
			{
				IsAntialias = true,
				Shader = baboon.ToShader(),
			};

			canvas.DrawPatch(cubics, null, tex, paint);
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

		[Obsolete]
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

		[Obsolete]
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
		public void DrawAtlasSupportsTransforms()
		{
			var target = new SKRect(50, 50, 80, 90);
			var rec = new[]
			{
				(Scale: 1, Degrees:   0, TX:  10, TY: 10), // just translate
				(Scale: 2, Degrees:   0, TX: 110, TY: 10), // scale + translate
				(Scale: 1, Degrees:  30, TX: 210, TY: 10), // rotate + translate
				(Scale: 2, Degrees: -30, TX: 310, TY: 30), // scale + rotate + translate
			};

			var N = rec.Length;
			var xform = new SKRotationScaleMatrix[N];
			var tex = new SKRect[N];
			var colors = new SKColor[N];

			for (var i = 0; i < N; ++i)
			{
				xform[i] = Apply(rec[i]);
				tex[i] = target;
				colors[i] = 0x80FF0000 + (uint)(i * 40 * 256);
			}

			using var atlas = CreateAtlas(target);

			using var bitmap = new SKBitmap(new SKImageInfo(640, 480));
			using var canvas = new SKCanvas(bitmap);

			using var paint = new SKPaint
			{
				FilterQuality = SKFilterQuality.Low,
				IsAntialias = true
			};

			canvas.Clear(SKColors.White);
			canvas.DrawAtlas(atlas, tex, xform, paint);
			canvas.Translate(0, 100);
			canvas.DrawAtlas(atlas, tex, xform, colors, SKBlendMode.SrcIn, paint);

			Assert.Equal(SKColors.Blue, bitmap.GetPixel(32, 41));
			Assert.Equal(SKColors.Blue, bitmap.GetPixel(156, 77));
			Assert.Equal(SKColors.Blue, bitmap.GetPixel(201, 45));
			Assert.Equal(SKColors.Blue, bitmap.GetPixel(374, 80));

			Assert.Equal(0xFF7F7FFF, bitmap.GetPixel(32, 141));
			Assert.Equal(0xFF7F7FFF, bitmap.GetPixel(156, 177));
			Assert.Equal(0xFF7F7FFF, bitmap.GetPixel(201, 145));
			Assert.Equal(0xFF7F7FFF, bitmap.GetPixel(374, 180));

			static SKRotationScaleMatrix Apply((float Scale, float Degrees, float TX, float TY) rec)
			{
				var rad = SKMatrix.DegreesToRadians * rec.Degrees;
				return new SKRotationScaleMatrix(
					rec.Scale * (float)Math.Cos(rad),
					rec.Scale * (float)Math.Sin(rad),
					rec.TX,
					rec.TY);
			}

			static SKImage CreateAtlas(SKRect target)
			{
				var info = new SKImageInfo(100, 100);

				using var surface = SKSurface.Create(info);
				using var canvas = surface.Canvas;

				// draw red everywhere, but we don't expect to see it in the draw, testing the notion
				// that drawAtlas draws a subset-region of the atlas.
				canvas.Clear(SKColors.Red);

				using var paint = new SKPaint
				{
					BlendMode = SKBlendMode.Clear
				};

				// zero out a place (with a 1-pixel border) to land our drawing.
				var r = target;
				r.Inflate(1, 1);
				canvas.DrawRect(r, paint);

				paint.BlendMode = SKBlendMode.SrcOver;
				paint.Color = SKColors.Blue;
				paint.IsAntialias = true;
				canvas.DrawOval(target, paint);

				return surface.Snapshot();
			}
		}

		[SkippableFact]
		public void TotalMatrixIsCorrect()
		{
			using (var bitmap = new SKBitmap(new SKImageInfo(100, 100)))
			using (var canvas = new SKCanvas(bitmap))
			{
				canvas.Translate(10, 20);
				Assert.Equal(SKMatrix.CreateTranslation(10, 20).Values, canvas.TotalMatrix.Values);

				canvas.Translate(10, 20);
				Assert.Equal(SKMatrix.CreateTranslation(20, 40).Values, canvas.TotalMatrix.Values);
			}
		}

		[SkippableFact]
		public void SvgCanvasSavesFile()
		{
			var stream = new MemoryStream();

			using (var svg = SKSvgCanvas.Create(SKRect.Create(100, 100), stream))
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
				Assert.Equal("red", rect.Attribute("fill")?.Value);
				Assert.Null(rect.Attribute("stroke")?.Value);
				Assert.Equal("10", rect.Attribute("x")?.Value);
				Assert.Equal("10", rect.Attribute("y")?.Value);
				Assert.Equal("80", rect.Attribute("width")?.Value);
				Assert.Equal("80", rect.Attribute("height")?.Value);
			}
		}

		[Obsolete]
		[SkippableFact]
		public void SvgCanvasSavesFileUsingWriter()
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
				Assert.Equal("red", rect.Attribute("fill")?.Value);
				Assert.Null(rect.Attribute("stroke")?.Value);
				Assert.Equal("10", rect.Attribute("x")?.Value);
				Assert.Equal("10", rect.Attribute("y")?.Value);
				Assert.Equal("80", rect.Attribute("width")?.Value);
				Assert.Equal("80", rect.Attribute("height")?.Value);
			}
		}

		[SkippableTheory]
		[InlineData(SKTextAlign.Left, 300)]
		[InlineData(SKTextAlign.Center, 162)]
		[InlineData(SKTextAlign.Right, 23)]
		public void TextAlignMovesTextPosition(SKTextAlign align, int offset)
		{
			var font = Path.Combine(PathToFonts, "segoeui.ttf");
			using var tf = SKTypeface.FromFile(font);

			using var bitmap = new SKBitmap(600, 200);
			using var canvas = new SKCanvas(bitmap);

			canvas.Clear(SKColors.White);

			using var paint = new SKPaint();
			paint.Typeface = tf;
			paint.IsAntialias = true;
			paint.TextSize = 64;
			paint.Color = SKColors.Black;
			paint.TextAlign = align;

			canvas.DrawText("SkiaSharp", 300, 100, paint);

			SaveBitmap(bitmap, $"output-{align}.png");

			AssertTextAlign(bitmap, offset, 0);
		}

		[SkippableTheory]
		[InlineData(SKTextAlign.Left, 300)]
		//[InlineData(SKTextAlign.Center, 162)]
		//[InlineData(SKTextAlign.Right, 23)]
		public void TextAlignMovesTextBlobPosition(SKTextAlign align, int offset)
		{
			var font = Path.Combine(PathToFonts, "segoeui.ttf");
			using var tf = SKTypeface.FromFile(font);

			using var bitmap = new SKBitmap(600, 200);
			using var canvas = new SKCanvas(bitmap);

			canvas.Clear(SKColors.White);

			using var paint = new SKPaint();
			paint.Typeface = tf;
			paint.IsAntialias = true;
			paint.TextSize = 64;
			paint.Color = SKColors.Black;
			paint.TextAlign = align;

			var glyphs = paint.GetGlyphs("SkiaSharp");
			using var blobBuilder = new SKTextBlobBuilder();
			blobBuilder.AddRun(paint, 0, 0, glyphs);
			using var blob = blobBuilder.Build();

			canvas.DrawText(blob, 300, 100, paint);

			SaveBitmap(bitmap, $"output-b-{align}.png");

			AssertTextAlign(bitmap, offset, 0);
		}

		[SkippableTheory]
		[InlineData(SKTextAlign.Left, 300)]
		[InlineData(SKTextAlign.Center, 300)]
		[InlineData(SKTextAlign.Right, 300)]
		public void TextAlignMovesHorizontalTextBlobPosition(SKTextAlign align, int offset)
		{
			var font = Path.Combine(PathToFonts, "segoeui.ttf");
			using var tf = SKTypeface.FromFile(font);

			using var bitmap = new SKBitmap(600, 200);
			using var canvas = new SKCanvas(bitmap);

			canvas.Clear(SKColors.White);

			using var paint = new SKPaint();
			paint.Typeface = tf;
			paint.IsAntialias = true;
			paint.TextSize = 64;
			paint.Color = SKColors.Black;
			paint.TextAlign = align;

			var glyphs = paint.GetGlyphs("SkiaSharp");
			var widths = paint.GetGlyphWidths("SkiaSharp");
			var positions = new float[widths.Length];
			for (var i = 1; i < positions.Length; i++)
			{
				positions[i] = positions[i - 1] + widths[i - 1];
			}

			using var blobBuilder = new SKTextBlobBuilder();
			var run = blobBuilder.AllocateHorizontalRun(paint, glyphs.Length, 0);
			run.SetGlyphs(glyphs);
			run.SetPositions(positions);
			using var blob = blobBuilder.Build();

			canvas.DrawText(blob, 300, 100, paint);

			SaveBitmap(bitmap, $"output-h-{align}.png");

			AssertTextAlign(bitmap, offset, 0);
		}

		[SkippableTheory]
		[InlineData(SKTextAlign.Left, 300)]
		[InlineData(SKTextAlign.Center, 300)]
		[InlineData(SKTextAlign.Right, 300)]
		public void TextAlignMovesPositionedTextBlobPosition(SKTextAlign align, int offset)
		{
			var font = Path.Combine(PathToFonts, "segoeui.ttf");
			using var tf = SKTypeface.FromFile(font);

			using var bitmap = new SKBitmap(600, 200);
			using var canvas = new SKCanvas(bitmap);

			canvas.Clear(SKColors.White);

			using var paint = new SKPaint();
			paint.Typeface = tf;
			paint.IsAntialias = true;
			paint.TextSize = 64;
			paint.Color = SKColors.Black;
			paint.TextAlign = align;

			var glyphs = paint.GetGlyphs("SkiaSharp");
			var widths = paint.GetGlyphWidths("SkiaSharp");
			var positions = new SKPoint[widths.Length];
			for (var i = 1; i < positions.Length; i++)
			{
				positions[i] = new SKPoint(positions[i - 1].X + widths[i - 1], 0);
			}

			using var blobBuilder = new SKTextBlobBuilder();
			var run = blobBuilder.AllocatePositionedRun(paint, glyphs.Length);
			run.SetGlyphs(glyphs);
			run.SetPositions(positions);
			using var blob = blobBuilder.Build();

			canvas.DrawText(blob, 300, 100, paint);

			SaveBitmap(bitmap, $"output-p-{align}.png");

			AssertTextAlign(bitmap, offset, 0);
		}

		[SkippableTheory]
		[InlineData(SKTextAlign.Left, 300, 300)]
		//[InlineData(SKTextAlign.Center, 300, 162)]
		//[InlineData(SKTextAlign.Right, 300, 23)]
		public void TextAlignMovesMixedTextBlobPosition(SKTextAlign align, int offsetPositioned, int offsetDefault)
		{
			var font = Path.Combine(PathToFonts, "segoeui.ttf");
			using var tf = SKTypeface.FromFile(font);

			using var bitmap = new SKBitmap(600, 300);
			using var canvas = new SKCanvas(bitmap);

			canvas.Clear(SKColors.White);

			using var paint = new SKPaint();
			paint.Typeface = tf;
			paint.IsAntialias = true;
			paint.TextSize = 64;
			paint.Color = SKColors.Black;
			paint.TextAlign = align;

			var glyphs = paint.GetGlyphs("SkiaSharp");
			var widths = paint.GetGlyphWidths("SkiaSharp");
			var positions = new SKPoint[widths.Length];
			for (var i = 1; i < positions.Length; i++)
			{
				positions[i] = new SKPoint(positions[i - 1].X + widths[i - 1], 0);
			}

			using var blobBuilder = new SKTextBlobBuilder();
			var run = blobBuilder.AllocatePositionedRun(paint, glyphs.Length);
			run.SetGlyphs(glyphs);
			run.SetPositions(positions);
			blobBuilder.AddRun(paint, 0, 100, glyphs);
			using var blob = blobBuilder.Build();

			canvas.DrawText(blob, 300, 100, paint);

			SaveBitmap(bitmap, $"output-m-{align}.png");

			AssertTextAlign(bitmap, offsetPositioned, 0);
			AssertTextAlign(bitmap, offsetDefault, 100);
		}

		private static void AssertTextAlign(SKBitmap bitmap, int x, int y)
		{
			// [S]kia[S]har[p]

			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 6, y + 66));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 28, y + 87));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 28, y + 66));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 6, y + 87));

			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 120, y + 66));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 142, y + 87));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 142, y + 66));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 120, y + 87));

			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 246, y + 70));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 246, y + 113));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 271, y + 83));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 258, y + 83));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 258, y + 113));
		}
	}
}

using System;
using SkiaSharp;
using System.Linq;
using System.IO;
using System.Reflection;

namespace SkiaSharp
{
	public class Demos
	{
		#pragma warning disable 414
		static readonly SKColor XamLtBlue = new SKColor (0x34, 0x98, 0xdb);
		static readonly SKColor XamGreen = new SKColor (0x77, 0xd0, 0x65);
		static readonly SKColor XamDkBlue = new SKColor (0x2c, 0x3e, 0x50);
		static readonly SKColor XamPurple = new SKColor (0xb4, 0x55, 0xb6);
		#pragma warning restore 414

		public static void DrawXamagon (SKCanvas canvas, int width, int height)
		{
			// Width 41.6587026 => 144.34135
			// Height 56 => 147

			var paddingFactor = .6f;
			var imageLeft = 41.6587026f;
			var imageRight = 144.34135f;
			var imageTop = 56f;
			var imageBottom = 147f;

			var imageWidth = imageRight - imageLeft;

			var scale = (((float)height > width ? width : height) / imageWidth) * paddingFactor;

			var translateX =  (imageLeft + imageRight) / -2 + width/scale * 1/2;
			var translateY =  (imageBottom + imageTop) / -2 + height/scale * 1/2;

			canvas.Scale (scale, scale);
			canvas.Translate (translateX , translateY);


			using (var paint = new SKPaint ()) {
				paint.IsAntialias = true;
				paint.Color = SKColors.White;
				canvas.DrawPaint (paint);

				paint.StrokeCap = SKStrokeCap.Round;
				var t = paint.StrokeCap;


				using (var path = new SKPath ()) {
					path.MoveTo (71.4311121f, 56f);
					path.CubicTo (68.6763107f, 56.0058575f, 65.9796704f, 57.5737917f, 64.5928855f, 59.965729f);
					path.LineTo (43.0238921f, 97.5342563f);
					path.CubicTo (41.6587026f, 99.9325978f, 41.6587026f, 103.067402f, 43.0238921f, 105.465744f);
					path.LineTo (64.5928855f, 143.034271f);
					path.CubicTo (65.9798162f, 145.426228f, 68.6763107f, 146.994582f, 71.4311121f, 147f);
					path.LineTo (114.568946f, 147f);
					path.CubicTo (117.323748f, 146.994143f, 120.020241f, 145.426228f, 121.407172f, 143.034271f);
					path.LineTo (142.976161f, 105.465744f);
					path.CubicTo (144.34135f, 103.067402f, 144.341209f, 99.9325978f, 142.976161f, 97.5342563f);
					path.LineTo (121.407172f, 59.965729f);
					path.CubicTo (120.020241f, 57.5737917f, 117.323748f, 56.0054182f, 114.568946f, 56f);
					path.LineTo (71.4311121f, 56f);
					path.Close ();

					paint.Color = XamDkBlue;
					canvas.DrawPath (path, paint);

				}

				using (var path = new SKPath ()) {

					path.MoveTo (71.8225901f, 77.9780432f);
					path.CubicTo (71.8818491f, 77.9721857f, 71.9440029f, 77.9721857f, 72.0034464f, 77.9780432f);
					path.LineTo (79.444074f, 77.9780432f);
					path.CubicTo (79.773437f, 77.9848769f, 80.0929203f, 78.1757336f, 80.2573978f, 78.4623994f);
					path.LineTo (92.8795281f, 101.015639f);
					path.CubicTo (92.9430615f, 101.127146f, 92.9839987f, 101.251384f, 92.9995323f, 101.378901f);
					path.CubicTo (93.0150756f, 101.251354f, 93.055974f, 101.127107f, 93.1195365f, 101.015639f);
					path.LineTo (105.711456f, 78.4623994f);
					path.CubicTo (105.881153f, 78.167045f, 106.215602f, 77.975134f, 106.554853f, 77.9780432f);
					path.LineTo (113.995483f, 77.9780432f);
					path.CubicTo (114.654359f, 77.9839007f, 115.147775f, 78.8160066f, 114.839019f, 79.4008677f);
					path.LineTo (102.518299f, 101.500005f);
					path.LineTo (114.839019f, 123.568869f);
					path.CubicTo (115.176999f, 124.157088f, 114.671442f, 125.027775f, 113.995483f, 125.021957f);
					path.LineTo (106.554853f, 125.021957f);
					path.CubicTo (106.209673f, 125.019028f, 105.873247f, 124.81384f, 105.711456f, 124.507327f);
					path.LineTo (93.1195365f, 101.954088f);
					path.CubicTo (93.0560031f, 101.84258f, 93.0150659f, 101.718333f, 92.9995323f, 101.590825f);
					path.CubicTo (92.983989f, 101.718363f, 92.9430906f, 101.842629f, 92.8795281f, 101.954088f);
					path.LineTo (80.2573978f, 124.507327f);
					path.CubicTo (80.1004103f, 124.805171f, 79.7792269f, 125.008397f, 79.444074f, 125.021957f);
					path.LineTo (72.0034464f, 125.021957f);
					path.CubicTo (71.3274867f, 125.027814f, 70.8220664f, 124.157088f, 71.1600463f, 123.568869f);
					path.LineTo (83.4807624f, 101.500005f);
					path.LineTo (71.1600463f, 79.400867f);
					path.CubicTo (70.8647037f, 78.86725f, 71.2250368f, 78.0919422f, 71.8225901f, 77.9780432f);
					path.LineTo (71.8225901f, 77.9780432f);
					path.Close ();

					paint.Color = SKColors.White;
					canvas.DrawPath (path, paint);
				}
			}
		}

		// https://fiddle.skia.org/c/3523e95a9f8b96228b6b4bdd5409cb94
		public static void TextSample (SKCanvas canvas, int width, int height)
		{
			canvas.DrawColor (SKColors.White);

			using (var paint = new SKPaint ()) {
				paint.TextSize = 64.0f;
				paint.IsAntialias = true;
				paint.Color = new SKColor (0x42, 0x81, 0xA4);
				paint.IsStroke = false;

				canvas.DrawText ("Skia", width / 2f, 64.0f, paint);
			}

			using (var paint = new SKPaint ()) {
				paint.TextSize = 64.0f;
				paint.IsAntialias = true;
				paint.Color = new SKColor (0x9C, 0xAF, 0xB7);
				paint.IsStroke = true;
				paint.StrokeWidth = 3;
				paint.TextAlign = SKTextAlign.Center;

				canvas.DrawText ("Skia", width / 2f, 144.0f, paint);
			}

			using (var paint = new SKPaint ()) {
				paint.TextSize = 64.0f;
				paint.IsAntialias = true;
				paint.Color = new SKColor (0xE6, 0xB8, 0x9C);
				paint.TextScaleX = 1.5f;
				paint.TextAlign = SKTextAlign.Right;

				canvas.DrawText ("Skia", width / 2f, 224.0f, paint);
			}
		}

		public static void DrawGradient (SKCanvas canvas, int width, int height)
		{
			var ltColor = SKColors.White;
			var dkColor = SKColors.Black;

			using (var paint = new SKPaint ()) {
				paint.IsAntialias = true;
				using (var shader = SKShader.CreateLinearGradient (
					new SKPoint (0, 0),
					new SKPoint (0, height),
					new [] {ltColor, dkColor},
					null,
					SKShaderTileMode.Clamp)) {

					paint.Shader = shader;
					canvas.DrawPaint (paint);
				}
			}

			// Center and Scale the Surface
			var scale = (width < height ? width : height) / (240f);
			canvas.Translate (width/2f, height/2f);
			canvas.Scale (scale, scale);
			canvas.Translate (-128, -128);

			using (var paint = new SKPaint ()) {
				paint.IsAntialias = true;
				using (var shader = SKShader.CreateTwoPointConicalGradient (
					new SKPoint (115.2f, 102.4f), 
					25.6f,
					new SKPoint (102.4f, 102.4f),
					128.0f,
					new [] {ltColor, dkColor},
					null,
					SKShaderTileMode.Clamp
				)) {
					paint.Shader = shader;

					canvas.DrawOval (new SKRect (51.2f, 51.2f, 204.8f, 204.8f), paint);
				}
			}
		}

		public static void UnicodeText (SKCanvas canvas, int width, int height)
		{
			// stops at 0x530 on Android
			string text = "\u03A3 and \u0750";

			using (var paint = new SKPaint ())
			using (var tf = SKTypeface.FromFamilyName ("Tahoma")) {
				canvas.DrawColor (SKColors.White);

				paint.IsAntialias = true;
				paint.TextSize = 60;
				paint.Typeface = tf;
				canvas.DrawText (text, 50, 100, paint);
			}


			using (var paint = new SKPaint ())
			using (var tf = SKTypeface.FromFamilyName ("Times New Roman")) {
				paint.Color = XamDkBlue;

				paint.IsAntialias = true;
				paint.TextSize = 60;
				paint.Typeface = tf;
				canvas.DrawText (text, 50, 200, paint);
			}
		}

		public static void FilledHeptagram (SKCanvas canvas, int width, int height)
		{
			var size = ((float)height > width ? width : height) * 0.75f;
			var R = 0.45f * size;
			var TAU = 6.2831853f;

			using (var path = new SKPath ()) {
				path.MoveTo (R, 0.0f);
				for (int i = 1; i < 7; ++i) {
					var theta = 3f * i * TAU / 7f;
					path.LineTo (R * (float)Math.Cos (theta), R * (float)Math.Sin (theta));
				}
				path.Close ();

				using (var paint = new SKPaint ()) {
					paint.IsAntialias = true;
					canvas.Clear (SKColors.White);
					canvas.Translate (width / 2f, height / 2f);
					canvas.DrawPath (path, paint);
				}
			}
		}

		public static void ManageDrawMatrix (SKCanvas canvas, int width, int height)
		{
			var size = ((float)height > width ? width : height) * 0.5f;
			var center = new SKPoint ((width - size) / 2f, (height - size) / 2f);

			// draw these at specific locations
			var leftRect = SKRect.Create (center.X - size / 2f, center.Y, size, size);
			var rightRect = SKRect.Create (center.X + size / 2f, center.Y, size, size);

			// draw this at the current location / transformation
			var rotatedRect = SKRect.Create (0f, 0f, size, size);

			using (var paint = new SKPaint ()) {
				paint.IsAntialias = true;
				canvas.Clear (XamPurple);

				// draw
				paint.Color = XamDkBlue;
				canvas.DrawRect (leftRect, paint);

				// save
				canvas.Save();

				// transform
				canvas.Translate (width / 2f, center.Y);
				canvas.RotateDegrees (45);

				// draw
				paint.Color = XamGreen;
				canvas.DrawRect (rotatedRect, paint);

				// undo transform / restore
				canvas.Restore();

				// draw
				paint.Color = XamLtBlue;
				canvas.DrawRect (rightRect, paint);
			}
		}

		public static void CustomFonts (SKCanvas canvas, int width, int height)
		{
			string text = "\u03A3 and \u0750";

			using (var paint = new SKPaint ()) {
				canvas.Clear (SKColors.White);
				paint.IsAntialias = true;

				using (var tf = SKTypeface.FromFile (CustomFontPath)) {
					paint.Color = XamGreen;
					paint.TextSize = 40;
					paint.Typeface = tf;

					canvas.DrawText (text, 50, 50, paint);
				}

				using (var stream = new SKFileStream (CustomFontPath))
				using (var tf = SKTypeface.FromStream (stream)) {
					paint.Color = XamDkBlue;
					paint.TextSize = 40;
					paint.Typeface = tf;

					canvas.DrawText (text, 50, 100, paint);
				}

				var assembly = typeof(Demos).GetTypeInfo ().Assembly;
				var fontName = assembly.GetName ().Name + ".embedded-font.ttf";

				using (var resource = assembly.GetManifestResourceStream (fontName))
				using (var memory = new MemoryStream ()) {
					resource.CopyTo (memory);
					var bytes = memory.ToArray ();
					using (var stream = new SKMemoryStream (bytes))
					using (var tf = SKTypeface.FromStream (stream)) {
						paint.Color = XamLtBlue;
						paint.TextSize = 40;
						paint.Typeface = tf;

						canvas.DrawText (text, 50, 150, paint);
					}
				}

				using (var resource = assembly.GetManifestResourceStream (fontName))
				using (var stream = new SKManagedStream (resource))
				using (var tf = SKTypeface.FromStream (stream)) {
					paint.Color = XamPurple;
					paint.TextSize = 40;
					paint.Typeface = tf;

					canvas.DrawText (text, 50, 200, paint);
				}
			}
		}

		public static void Xfermode (SKCanvas canvas, int width, int height)
		{
			var modes = Enum.GetValues (typeof(SKXferMode)).Cast<SKXferMode> ().ToArray ();

			var cols = width < height ? 3 : 5;
			var rows = (modes.Length - 1) / cols + 1;

			var w = (float)width / cols;
			var h = (float)height / rows;
			var rect = SKRect.Create (w, h);
			var srcPoints = new[] {
				new SKPoint (0.0f, 0.0f),
				new SKPoint (w, 0.0f)
			};
			var srcColors = new [] {
				SKColors.Magenta.WithAlpha (0),
				SKColors.Magenta
			};
			var dstPoints = new [] {
				new SKPoint (0.0f, 0.0f),
				new SKPoint (0.0f, h)
			};
			var dstColors = new [] {
				SKColors.Cyan.WithAlpha (0),
				SKColors.Cyan
			};

			using (var text = new SKPaint ())
			using (var stroke = new SKPaint ())
			using (var src = new SKPaint ())
			using (var dst = new SKPaint ())
			using (var srcShader = SKShader.CreateLinearGradient (srcPoints [0], srcPoints [1], srcColors, null, SKShaderTileMode.Clamp))
			using (var dstShader = SKShader.CreateLinearGradient (dstPoints [0], dstPoints [1], dstColors, null, SKShaderTileMode.Clamp)) {
				text.TextSize = 12.0f;
				text.IsAntialias = true;
				text.TextAlign = SKTextAlign.Center;
				stroke.IsStroke = true;
				src.Shader = srcShader;
				dst.Shader = dstShader;

				canvas.Clear (SKColors.White);

				for (var i = 0; i < modes.Length; ++i) {
					using (new SKAutoCanvasRestore (canvas, true)) {
						canvas.Translate (w * (i / rows), h * (i % rows));

						canvas.ClipRect (rect);
						canvas.DrawColor (SKColors.LightGray);

						canvas.SaveLayer (null);
						canvas.Clear (SKColors.Transparent);
						canvas.DrawPaint (dst);

						src.XferMode = modes [i];
						canvas.DrawPaint (src);
						canvas.DrawRect (rect, stroke);

						var desc = modes [i].ToString ();
						canvas.DrawText (desc, w / 2f, h / 2f, text);
					}
				}
			}
		}

		public static void BitmapShader (SKCanvas canvas, int width, int height)
		{
			var assembly = typeof(Demos).GetTypeInfo ().Assembly;
			var imageName = assembly.GetName ().Name + ".color-wheel.png";

			// load the image from the embedded resource stream
			using (var resource = assembly.GetManifestResourceStream (imageName))
			using (var stream = new SKManagedStream(resource)) 
			using (var source = SKBitmap.Decode (stream)) {
				// create the shader and paint
				//SkMatrix matrix;
				//matrix.setScale(0.75f, 0.75f);
				//matrix.preRotate(30.0f);
				var matrix = SKMatrix.MakeRotation (30.0f);
				using (var shader = SKShader.CreateBitmap (source, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, matrix))
				using (var paint = new SKPaint ()) {
					paint.IsAntialias = true;
					paint.Shader = shader;

					// tile the bitmap
					canvas.Clear (SKColors.White);
					canvas.DrawPaint (paint);
				}
			}
		}

		public static void BitmapShaderManipulated (SKCanvas canvas, int width, int height)
		{
			var assembly = typeof(Demos).GetTypeInfo ().Assembly;
			var imageName = assembly.GetName ().Name + ".color-wheel.png";

			// load the image from the embedded resource stream
			using (var resource = assembly.GetManifestResourceStream (imageName))
			using (var stream = new SKManagedStream(resource))
			using (var source = SKBitmap.Decode (stream)) {
				// invert the pixels
				var pixels = source.Pixels;
				for (int i = 0; i < pixels.Length; i++) {
					pixels[i] = new SKColor (
						(byte)(255 - pixels [i].Red), 
						(byte)(255 - pixels [i].Green), 
						(byte)(255 - pixels [i].Blue), 
						pixels [i].Alpha);
				}
				source.Pixels = pixels;

				using (var shader = SKShader.CreateBitmap (source, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat))
				using (var paint = new SKPaint ()) {
					paint.IsAntialias = true;
					paint.Shader = shader;

					// tile the bitmap
					canvas.Clear (SKColors.White);
					canvas.DrawPaint (paint);
				}
			}
		}

		public static void BitmapDecoder(SKCanvas canvas, int width, int height)
		{
			var assembly = typeof(Demos).GetTypeInfo().Assembly;
			var imageName = assembly.GetName().Name + ".color-wheel.png";

			canvas.Clear(SKColors.White);

			// load the embedded resource stream
			using (var resource = assembly.GetManifestResourceStream(imageName))
			using (var stream = new SKManagedStream(resource))
			using (var decoder = new SKImageDecoder(stream))
			using (var paint = new SKPaint())
			using (var tf = SKTypeface.FromFamilyName("Arial"))
			{
				paint.IsAntialias = true;
				paint.TextSize = 14;
				paint.Typeface = tf;
				paint.Color = SKColors.Black;

				// read / set decoder settings
				decoder.DitherImage = true;
				decoder.PreferQualityOverSpeed = true;
				decoder.SampleSize = 2;

				// decode the image
				using (var bitmap = new SKBitmap())
				{
					var result = decoder.Decode(stream, bitmap);
					if (result != SKImageDecoderResult.Failure)
					{
						var info = bitmap.Info;
						var x = 25;
						var y = 25;

						canvas.DrawBitmap(bitmap, x, y);
						x += info.Width + 25;
						y += 14;

						canvas.DrawText(string.Format("Result: {0}", result), x, y, paint);
						y += 20;

						canvas.DrawText(string.Format("Size: {0}px x {1}px", bitmap.Width, bitmap.Height), x, y, paint);
						y += 20;

						canvas.DrawText(string.Format("Pixels: {0} @ {1}b/px", bitmap.Pixels.Length, bitmap.BytesPerPixel), x, y, paint);
					}
				}
			}
		}

		public static void SweepGradientShader(SKCanvas canvas, int width, int height)
		{
			var colors = new[] { SKColors.Cyan, SKColors.Magenta, SKColors.Yellow, SKColors.Cyan };
			var center = new SKPoint(width / 2f, height / 2f);

			using (var shader = SKShader.CreateSweepGradient(center, colors, null))
			using (var paint = new SKPaint())
			{
				paint.Shader = shader;
				canvas.DrawPaint(paint);
			}
		}

		public static void FractalPerlinNoiseShader(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			using (var shader = SKShader.CreatePerlinNoiseFractalNoise(0.05f, 0.05f, 4, 0.0f))
			using (var paint = new SKPaint())
			{
				paint.Shader = shader;
				canvas.DrawPaint(paint);
			}
		}

		public static void TurbulencePerlinNoiseShader(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			using (var shader = SKShader.CreatePerlinNoiseTurbulence(0.05f, 0.05f, 4, 0.0f))
			using (var paint = new SKPaint())
			{
				paint.Shader = shader;
				canvas.DrawPaint(paint);
			}
		}

		public static void ComposeShader(SKCanvas canvas, int width, int height)
		{
			var colors = new [] { SKColors.Blue, SKColors.Yellow };
			var center = new SKPoint(width / 2f, height / 2f);

			using (var shader1 = SKShader.CreateRadialGradient(center, 180.0f, colors, null, SKShaderTileMode.Clamp))
			using (var shader2 = SKShader.CreatePerlinNoiseTurbulence(0.025f, 0.025f, 2, 0.0f))
			using (var shader = SKShader.CreateCompose(shader1, shader2))
			using (var paint = new SKPaint())
			{
				paint.Shader = shader;
				canvas.DrawPaint(paint);
			}
		}

		public static void BlurMaskFilter(SKCanvas canvas, int width, int height)
		{
			canvas.DrawColor(SKColors.White);

			using (var paint = new SKPaint())
			using (var filter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5.0f))
			{
				paint.IsAntialias = true;
				paint.TextSize = 120;
				paint.TextAlign = SKTextAlign.Center;
				paint.MaskFilter = filter;

				canvas.DrawText("Skia", width / 2f, height / 2f, paint);
			}
		}

		public static void EmbossMaskFilter(SKCanvas canvas, int width, int height)
		{
			canvas.DrawColor(SKColors.White);

			SKPoint3 direction = new SKPoint3(1.0f, 1.0f, 1.0f);

			using (var paint = new SKPaint())
			using (var filter = SKMaskFilter.CreateEmboss(2.0f, direction, 0.3f, 0.1f))
			{
				paint.IsAntialias = true;
				paint.TextSize = 120;
				paint.TextAlign = SKTextAlign.Center;
				paint.MaskFilter = filter;

				canvas.DrawText("Skia", width / 2f, height / 2f, paint);
			}
		}

		public static void ColorMatrixColorFilter(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			var assembly = typeof(Demos).GetTypeInfo().Assembly;
			var imageName = assembly.GetName().Name + ".baboon.png";

			// load the image from the embedded resource stream
			using (var resource = assembly.GetManifestResourceStream(imageName))
			using (var stream = new SKManagedStream(resource))
			using (var bitmap = SKBitmap.Decode(stream))
			{
				var f = new Action<SKRect, float[]>((rect, colorMatrix) =>
				{
					using (var cf = SKColorFilter.CreateColorMatrix(colorMatrix))
					using (var paint = new SKPaint())
					{
						paint.ColorFilter = cf;

						canvas.DrawBitmap(bitmap, rect, paint);
					}
				});

				var colorMatrix1 = new float[20] {
					0f, 1f, 0f, 0f, 0f,
					0f, 0f, 1f, 0f, 0f,
					1f, 0f, 0f, 0f, 0f,
					0f, 0f, 0f, 1f, 0f
				};
				var grayscale = new float[20] {
					0.21f, 0.72f, 0.07f, 0.0f, 0.0f,
					0.21f, 0.72f, 0.07f, 0.0f, 0.0f,
					0.21f, 0.72f, 0.07f, 0.0f, 0.0f,
					0.0f,  0.0f,  0.0f,  1.0f, 0.0f
				};
				var colorMatrix3 = new float[20] {
					-1f,  1f,  1f, 0f, 0f,
					 1f, -1f,  1f, 0f, 0f,
					 1f,  1f, -1f, 0f, 0f,
					 0f,  0f,  0f, 1f, 0f
				};
				var colorMatrix4 = new float[20] {
					0.0f, 0.5f, 0.5f, 0f, 0f,
					0.5f, 0.0f, 0.5f, 0f, 0f,
					0.5f, 0.5f, 0.0f, 0f, 0f,
					0.0f, 0.0f, 0.0f, 1f, 0f
				};
				var highContrast = new float[20] {
					4.0f, 0.0f, 0.0f, 0.0f, -4.0f * 255f / (4.0f - 1f),
					0.0f, 4.0f, 0.0f, 0.0f, -4.0f * 255f / (4.0f - 1f),
					0.0f, 0.0f, 4.0f, 0.0f, -4.0f * 255f / (4.0f - 1f),
					0.0f, 0.0f, 0.0f, 1.0f, 0.0f
				};
				var colorMatrix6 = new float[20] {
					0f, 0f, 1f, 0f, 0f,
					1f, 0f, 0f, 0f, 0f,
					0f, 1f, 0f, 0f, 0f,
					0f, 0f, 0f, 1f, 0f
				};
				var sepia = new float[20] {
					0.393f, 0.769f, 0.189f, 0.0f, 0.0f,
					0.349f, 0.686f, 0.168f, 0.0f, 0.0f,
					0.272f, 0.534f, 0.131f, 0.0f, 0.0f,
					0.0f,   0.0f,   0.0f,   1.0f, 0.0f
				};
				var inverter = new float[20] {
					-1f,  0f,  0f, 0f, 255f,
					0f, -1f,  0f, 0f, 255f,
					0f,  0f, -1f, 0f, 255f,
					0f,  0f,  0f, 1f, 0f
				};

				var matices = new[] {
					colorMatrix1, grayscale, highContrast, sepia,
					colorMatrix3, colorMatrix4, colorMatrix6, inverter
				};

				var cols = width < height ? 2 : 4;
				var rows = (matices.Length - 1) / cols + 1;
				var w = (float)width / cols;
				var h = (float)height / rows;

				for (int y = 0; y < rows; y++)
				{
					for (int x = 0; x < cols; x++)
					{
						f(SKRect.Create(x * w, y * h, w, h), matices[y * cols + x]);
					}
				}
			}
		}

		public static void ColorTableColorFilter(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			var assembly = typeof(Demos).GetTypeInfo().Assembly;
			var imageName = assembly.GetName().Name + ".baboon.png";

			var ct = new byte[256];
			for (int i = 0; i < 256; ++i)
			{
				var x = (i - 96) * 255 / 64;
				ct[i] = x < 0 ? (byte)0 : x > 255 ? (byte)255 : (byte)x;
			}

			// load the image from the embedded resource stream
			using (var resource = assembly.GetManifestResourceStream(imageName))
			using (var stream = new SKManagedStream(resource))
			using (var bitmap = SKBitmap.Decode(stream))
			using (var cf = SKColorFilter.CreateTable(null, ct, ct, ct))
			using (var paint = new SKPaint())
			{
				paint.ColorFilter = cf;

				canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
			}
		}

		public static void LumaColorFilter(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			var assembly = typeof(Demos).GetTypeInfo().Assembly;
			var imageName = assembly.GetName().Name + ".baboon.png";
			
			// load the image from the embedded resource stream
			using (var resource = assembly.GetManifestResourceStream(imageName))
			using (var stream = new SKManagedStream(resource))
			using (var bitmap = SKBitmap.Decode(stream))
			using (var cf = SKColorFilter.CreateLumaColor())
			using (var paint = new SKPaint())
			{
				paint.ColorFilter = cf;

				canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
			}
		}

		public static void XferModeColorFilter(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			var assembly = typeof(Demos).GetTypeInfo().Assembly;
			var imageName = assembly.GetName().Name + ".baboon.png";

			// load the image from the embedded resource stream
			using (var resource = assembly.GetManifestResourceStream(imageName))
			using (var stream = new SKManagedStream(resource))
			using (var bitmap = SKBitmap.Decode(stream))
			using (var cf = SKColorFilter.CreateXferMode(SKColors.Red, SKXferMode.ColorDodge))
			using (var paint = new SKPaint())
			{
				paint.ColorFilter = cf;

				canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
			}
		}

		public static void ErodeImageFilter(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			var assembly = typeof(Demos).GetTypeInfo().Assembly;
			var imageName = assembly.GetName().Name + ".baboon.png";

			// load the image from the embedded resource stream
			using (var resource = assembly.GetManifestResourceStream(imageName))
			using (var stream = new SKManagedStream(resource))
			using (var bitmap = SKBitmap.Decode(stream))
			using (var filter = SKImageFilter.CreateErode(5, 5))
			using (var paint = new SKPaint())
			{
				paint.ImageFilter = filter;

				canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
			}
		}

		public static void DilateImageFilter(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			var assembly = typeof(Demos).GetTypeInfo().Assembly;
			var imageName = assembly.GetName().Name + ".baboon.png";

			// load the image from the embedded resource stream
			using (var resource = assembly.GetManifestResourceStream(imageName))
			using (var stream = new SKManagedStream(resource))
			using (var bitmap = SKBitmap.Decode(stream))
			using (var filter = SKImageFilter.CreateDilate(5, 5))
			using (var paint = new SKPaint())
			{
				paint.ImageFilter = filter;

				canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
			}
		}

		public static void BlurImageFilter(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			var assembly = typeof(Demos).GetTypeInfo().Assembly;
			var imageName = assembly.GetName().Name + ".baboon.png";

			// load the image from the embedded resource stream
			using (var resource = assembly.GetManifestResourceStream(imageName))
			using (var stream = new SKManagedStream(resource))
			using (var bitmap = SKBitmap.Decode(stream))
			using (var filter = SKImageFilter.CreateBlur(5, 5))
			using (var paint = new SKPaint())
			{
				paint.ImageFilter = filter;

				canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
			}
		}

		public static void MagnifierImageFilter(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			var assembly = typeof(Demos).GetTypeInfo().Assembly;
			var imageName = assembly.GetName().Name + ".baboon.png";

			var size = width > height ? height : width;
			var small = size / 5;

			// load the image from the embedded resource stream
			using (var resource = assembly.GetManifestResourceStream(imageName))
			using (var stream = new SKManagedStream(resource))
			using (var bitmap = SKBitmap.Decode(stream))
			using (var filter = SKImageFilter.CreateMagnifier(SKRect.Create(small*2, small*2, small*3, small*3), small))
			using (var paint = new SKPaint())
			{
				paint.ImageFilter = filter;

				canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
			}
		}

		public static void ChainedImageFilter(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			var assembly = typeof(Demos).GetTypeInfo().Assembly;
			var imageName = assembly.GetName().Name + ".baboon.png";

			var size = width > height ? height : width;
			var small = size / 5;

			// load the image from the embedded resource stream
			using (var resource = assembly.GetManifestResourceStream(imageName))
			using (var stream = new SKManagedStream(resource))
			using (var bitmap = SKBitmap.Decode(stream))
			using (var filterMag = SKImageFilter.CreateMagnifier(SKRect.Create(small*2, small*2, small*3, small*3), small))
			using (var filterBlur = SKImageFilter.CreateBlur(5, 5, filterMag))
			using (var paint = new SKPaint())
			{
				paint.ImageFilter = filterBlur;

				canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
			}
		}

		[Flags]
		public enum Platform
		{
			iOS = 1,
			Android = 2,
			OSX = 4,
			WindowsDesktop = 8,
			All = 0xFFFF,
		}

		class Sample 
		{
			public string Title { get; set; }
			public Action <SKCanvas, int, int> Method { get; set; }
			public Platform Platform { get; set; }
		}

		public static string [] SamplesForPlatform (Platform platform)
		{
			return sampleList.Where (s => 0 != (s.Platform & platform)).Select (s => s.Title).ToArray ();
		}

		public static Action <SKCanvas, int, int> MethodForSample (string title)
		{
			return sampleList.Where (s => s.Title == title).First ().Method;
		}

		public static string CustomFontPath { get; set; }

		static Sample [] sampleList = new Sample[] {
			new Sample {Title="Xamagon", Method = DrawXamagon, Platform = Platform.All},
			new Sample {Title="Text Sample", Method = TextSample, Platform = Platform.All},
			new Sample {Title="Gradient Sample", Method = DrawGradient, Platform = Platform.All},
			new Sample {Title="Unicode Text", Method = UnicodeText, Platform = Platform.All},
			new Sample {Title="Filled Heptagram", Method = FilledHeptagram, Platform = Platform.All},
			new Sample {Title="Manage Draw Matrix", Method = ManageDrawMatrix, Platform = Platform.All},
			new Sample {Title="Custom Fonts", Method = CustomFonts, Platform = Platform.All},
			new Sample {Title="Xfermode", Method = Xfermode, Platform = Platform.All},
			new Sample {Title="Bitmap Shader", Method = BitmapShader, Platform = Platform.All},
			new Sample {Title="Bitmap Shader (Manipulated)", Method = BitmapShaderManipulated, Platform = Platform.All},
			new Sample {Title="Bitmap Decoder", Method = BitmapDecoder, Platform = Platform.All},
			new Sample {Title="Sweep Gradient Shader", Method = SweepGradientShader, Platform = Platform.All},
			new Sample {Title="Fractal Perlin Noise Shader", Method = FractalPerlinNoiseShader, Platform = Platform.All},
			new Sample {Title="Turbulence Perlin Noise Shader", Method = TurbulencePerlinNoiseShader, Platform = Platform.All},
			new Sample {Title="Compose Shader", Method = ComposeShader, Platform = Platform.All},
			new Sample {Title="Blur Mask Filter", Method = BlurMaskFilter, Platform = Platform.All},
			new Sample {Title="Emboss Mask Filter", Method = EmbossMaskFilter, Platform = Platform.All},
			new Sample {Title="Color Matrix Color Filter", Method = ColorMatrixColorFilter, Platform = Platform.All},
			new Sample {Title="Color Table Color Filter", Method = ColorTableColorFilter, Platform = Platform.All},
			new Sample {Title="Luma Color Filter", Method = LumaColorFilter, Platform = Platform.All},
			new Sample {Title="XferMode Color Filter", Method = XferModeColorFilter, Platform = Platform.All},
			new Sample {Title="Erode Image Filter", Method = ErodeImageFilter, Platform = Platform.All},
			new Sample {Title="Dilate Image Filter", Method = DilateImageFilter, Platform = Platform.All},
			new Sample {Title="Blur Image Filter", Method = BlurImageFilter, Platform = Platform.All},
			new Sample {Title="Magnifier Image Filter", Method = MagnifierImageFilter, Platform = Platform.All},
			new Sample {Title="Chained Image Filter", Method = ChainedImageFilter, Platform = Platform.All},
		};
	}
}


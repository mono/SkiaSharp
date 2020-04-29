using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.Uno;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SkiaSharpSample
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			SKCanvas canvas = e.Surface.Canvas;
			int width = e.Info.Width;
			int height = e.Info.Height;

			// Sample1(e);

			// DrawMatrixSample(canvas, width, height);
			// DrawShader(canvas, width, height);
			// PathBoundsSample(canvas, width, height);
			// PathEffectsSample(canvas, width, height);
			// DrawVerticesSample(canvas, width, height);
			// FractalPerlinNoiseShaderSample(canvas, width, height);
			// FilledHeptagramSample(canvas, width, height);
			// PathConicToQuadsSample(canvas, width, height);
			// SweepGradientShaderSample(canvas, width, height);
			// XamagonSample(canvas, width, height);
			// TurbulencePerlinNoiseShaderSample(canvas, width, height);
			// ComposeShaderSample(canvas, width, height);
			BlurMaskFilterSample(canvas, width, height);
		}

		void BlurMaskFilterSample(SKCanvas canvas, int width, int height)
		{
			canvas.DrawColor(SKColors.White);

			using (var paint = new SKPaint())
			using (var filter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5.0f))
			{
				paint.IsAntialias = true;
				paint.TextSize = 120;
				paint.TextAlign = SKTextAlign.Center;
				paint.MaskFilter = filter;

				canvas.DrawText("SkiaSharp", width / 2f, height / 4f, paint);
			}

			// scale up so we don't have to change the values in the method
			var scaling = 3;
			canvas.Scale(scaling, scaling);
			var rect = SKRect.Create(width / (3f * scaling), height / (2f * scaling), width / (3f * scaling), height / (4f * scaling));

			DrawInnerBlurRectangle(canvas, rect);
		}

		private void DrawInnerBlurRectangle(SKCanvas canvas, SKRect rect)
		{
			// create the rounded rectangle
			var roundedRect = new SKPath();
			roundedRect.AddRoundRect(rect, 10, 10);

			// draw the white background
			var p = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = SKColors.White
			};
			canvas.DrawPath(roundedRect, p);

			using (new SKAutoCanvasRestore(canvas))
			{
				// clip the canvas to stop the blur from appearing outside
				canvas.ClipPath(roundedRect, SKClipOperation.Intersect, true);

				// draw the wide blur all around
				p.Color = SKColors.Black;
				p.Style = SKPaintStyle.Stroke;
				p.StrokeWidth = 2;
				p.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2);
				canvas.Translate(0.5f, 1.5f);
				canvas.DrawPath(roundedRect, p);

				// draw the narrow blur at the top
				p.StrokeWidth = 1;
				p.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 1);
				canvas.DrawPath(roundedRect, p);
			}

			// draw the border
			p.StrokeWidth = 2;
			p.MaskFilter = null;
			p.Color = SKColors.Green;
			canvas.DrawPath(roundedRect, p);
		}

		void ComposeShaderSample(SKCanvas canvas, int width, int height)
		{
			var colors = new[] { SKColors.Blue, SKColors.Yellow };
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

		void TurbulencePerlinNoiseShaderSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			using (var shader = SKShader.CreatePerlinNoiseTurbulence(0.05f, 0.05f, 4, 0.0f))
			using (var paint = new SKPaint())
			{
				paint.Shader = shader;
				canvas.DrawPaint(paint);
			}
		}

		private void XamagonSample(SKCanvas canvas, int width, int height)
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

			var translateX = (imageLeft + imageRight) / -2 + width / scale * 1 / 2;
			var translateY = (imageBottom + imageTop) / -2 + height / scale * 1 / 2;

			canvas.Scale(scale, scale);
			canvas.Translate(translateX, translateY);

			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;
				paint.Color = SKColors.White;
				paint.StrokeCap = SKStrokeCap.Round;

				canvas.DrawPaint(paint);

				using (var path = new SKPath())
				{
					path.MoveTo(71.4311121f, 56f);
					path.CubicTo(68.6763107f, 56.0058575f, 65.9796704f, 57.5737917f, 64.5928855f, 59.965729f);
					path.LineTo(43.0238921f, 97.5342563f);
					path.CubicTo(41.6587026f, 99.9325978f, 41.6587026f, 103.067402f, 43.0238921f, 105.465744f);
					path.LineTo(64.5928855f, 143.034271f);
					path.CubicTo(65.9798162f, 145.426228f, 68.6763107f, 146.994582f, 71.4311121f, 147f);
					path.LineTo(114.568946f, 147f);
					path.CubicTo(117.323748f, 146.994143f, 120.020241f, 145.426228f, 121.407172f, 143.034271f);
					path.LineTo(142.976161f, 105.465744f);
					path.CubicTo(144.34135f, 103.067402f, 144.341209f, 99.9325978f, 142.976161f, 97.5342563f);
					path.LineTo(121.407172f, 59.965729f);
					path.CubicTo(120.020241f, 57.5737917f, 117.323748f, 56.0054182f, 114.568946f, 56f);
					path.LineTo(71.4311121f, 56f);
					path.Close();

					paint.Color = SKColors.DarkBlue;
					canvas.DrawPath(path, paint);
				}

				using (var path = new SKPath())
				{
					path.MoveTo(71.8225901f, 77.9780432f);
					path.CubicTo(71.8818491f, 77.9721857f, 71.9440029f, 77.9721857f, 72.0034464f, 77.9780432f);
					path.LineTo(79.444074f, 77.9780432f);
					path.CubicTo(79.773437f, 77.9848769f, 80.0929203f, 78.1757336f, 80.2573978f, 78.4623994f);
					path.LineTo(92.8795281f, 101.015639f);
					path.CubicTo(92.9430615f, 101.127146f, 92.9839987f, 101.251384f, 92.9995323f, 101.378901f);
					path.CubicTo(93.0150756f, 101.251354f, 93.055974f, 101.127107f, 93.1195365f, 101.015639f);
					path.LineTo(105.711456f, 78.4623994f);
					path.CubicTo(105.881153f, 78.167045f, 106.215602f, 77.975134f, 106.554853f, 77.9780432f);
					path.LineTo(113.995483f, 77.9780432f);
					path.CubicTo(114.654359f, 77.9839007f, 115.147775f, 78.8160066f, 114.839019f, 79.4008677f);
					path.LineTo(102.518299f, 101.500005f);
					path.LineTo(114.839019f, 123.568869f);
					path.CubicTo(115.176999f, 124.157088f, 114.671442f, 125.027775f, 113.995483f, 125.021957f);
					path.LineTo(106.554853f, 125.021957f);
					path.CubicTo(106.209673f, 125.019028f, 105.873247f, 124.81384f, 105.711456f, 124.507327f);
					path.LineTo(93.1195365f, 101.954088f);
					path.CubicTo(93.0560031f, 101.84258f, 93.0150659f, 101.718333f, 92.9995323f, 101.590825f);
					path.CubicTo(92.983989f, 101.718363f, 92.9430906f, 101.842629f, 92.8795281f, 101.954088f);
					path.LineTo(80.2573978f, 124.507327f);
					path.CubicTo(80.1004103f, 124.805171f, 79.7792269f, 125.008397f, 79.444074f, 125.021957f);
					path.LineTo(72.0034464f, 125.021957f);
					path.CubicTo(71.3274867f, 125.027814f, 70.8220664f, 124.157088f, 71.1600463f, 123.568869f);
					path.LineTo(83.4807624f, 101.500005f);
					path.LineTo(71.1600463f, 79.400867f);
					path.CubicTo(70.8647037f, 78.86725f, 71.2250368f, 78.0919422f, 71.8225901f, 77.9780432f);
					path.LineTo(71.8225901f, 77.9780432f);
					path.Close();

					paint.Color = SKColors.White;
					canvas.DrawPath(path, paint);
				}
			}
		}

		void SweepGradientShaderSample(SKCanvas canvas, int width, int height)
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

		void PathConicToQuadsSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);
			canvas.Scale(2);

			var points = new[]
			{
				new SKPoint(10, 10),
				new SKPoint(50, 20),
				new SKPoint(100, 150)
			};

			using (var paint = new SKPaint())
			using (var textPaint = new SKPaint())
			{
				paint.Style = SKPaintStyle.Stroke;
				paint.StrokeWidth = 5;
				paint.IsAntialias = true;
				paint.StrokeCap = SKStrokeCap.Round;

				textPaint.IsAntialias = true;

				using (var path = new SKPath())
				{
					// create a conic path
					path.MoveTo(points[0]);
					path.ConicTo(points[1], points[2], 10);

					// draw the conic-based path
					paint.Color = SKColors.DarkBlue;
					canvas.DrawPath(path, paint);

					// get the quads from the conic points
					var quads = SKPath.ConvertConicToQuads(points[0], points[1], points[2], 10, out var pts, 2);

					// move the points on a bit
					for (var i = 0; i < pts.Length; i++)
						pts[i].Offset(120, 0);
					// draw the quad-based path
					using (var quadsPath = new SKPath())
					{
						quadsPath.MoveTo(pts[0].X, pts[0].Y);
						for (var i = 0; i < quads; i++)
						{
							var idx = i * 2;
							quadsPath.CubicTo(
								pts[idx].X, pts[idx].Y,
								pts[idx + 1].X, pts[idx + 1].Y,
								pts[idx + 2].X, pts[idx + 2].Y);
						}

						paint.Color = SKColors.Purple;
						canvas.DrawPath(quadsPath, paint);
					}

					// move the points on a bit
					for (var i = 0; i < pts.Length; i++)
						pts[i].Offset(120, 0);
					// draw the dots
					paint.Color = SKColors.Green;
					canvas.DrawPoints(SKPointMode.Points, pts, paint);
				}
			}
		}

		void FilledHeptagramSample(SKCanvas canvas, int width, int height)
		{
			var size = ((float)height > width ? width : height) * 0.75f;
			var R = 0.45f * size;
			var TAU = 6.2831853f;

			using (var path = new SKPath())
			{
				path.MoveTo(R, 0.0f);
				for (var i = 1; i < 7; ++i)
				{
					var theta = 3f * i * TAU / 7f;
					path.LineTo(R * (float)Math.Cos(theta), R * (float)Math.Sin(theta));
				}
				path.Close();

				using (var paint = new SKPaint())
				{
					paint.IsAntialias = true;
					canvas.Clear(SKColors.White);
					canvas.Translate(width / 2f, height / 2f);
					canvas.DrawPath(path, paint);
				}
			}
		}

		void FractalPerlinNoiseShaderSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			using (var shader = SKShader.CreatePerlinNoiseFractalNoise(0.05f, 0.05f, 4, 0.0f))
			using (var paint = new SKPaint())
			{
				paint.Shader = shader;
				canvas.DrawPaint(paint);
			}
		}

		void DrawVerticesSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);
			var paint = new SKPaint
			{
				IsAntialias = true
			};

			var vertices = new[] { new SKPoint(110, 20), new SKPoint(160, 200), new SKPoint(10, 200) };
			var colors = new[] { SKColors.Red, SKColors.Green, SKColors.Blue };

			canvas.DrawVertices(SKVertexMode.Triangles, vertices, colors, paint);
		}

		void PathEffectsSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			var step = height / 4;

			using (var paint = new SKPaint())
			using (var effect = SKPathEffect.CreateDash(new[] { 15f, 5f }, 0))
			{
				paint.IsStroke = true;
				paint.StrokeWidth = 4;
				paint.PathEffect = effect;
				canvas.DrawLine(10, step, width - 10 - 10, step, paint);
			}

			using (var paint = new SKPaint())
			using (var effect = SKPathEffect.CreateDiscrete(10, 10))
			{
				paint.IsStroke = true;
				paint.StrokeWidth = 4;
				paint.PathEffect = effect;
				canvas.DrawLine(10, step * 2, width - 10 - 10, step * 2, paint);
			}

			using (var paint = new SKPaint())
			using (var dashEffect = SKPathEffect.CreateDash(new[] { 15f, 5f }, 0))
			using (var discreteEffect = SKPathEffect.CreateDiscrete(10, 10))
			using (var effect = SKPathEffect.CreateCompose(dashEffect, discreteEffect))
			{
				paint.IsStroke = true;
				paint.StrokeWidth = 4;
				paint.PathEffect = effect;
				canvas.DrawLine(10, step * 3, width - 10 - 10, step * 3, paint);
			}
		}

		private void PathBoundsSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);
			canvas.Scale(2, 2);

			using (var paint = new SKPaint())
			using (var textPaint = new SKPaint())
			{
				paint.Style = SKPaintStyle.Stroke;
				paint.StrokeWidth = 1;
				paint.IsAntialias = true;
				paint.StrokeCap = SKStrokeCap.Round;

				textPaint.IsAntialias = true;

				using (var path = new SKPath())
				{
					path.MoveTo(-6.2157825e-7f, -25.814698f);
					path.RCubicTo(-34.64102137842175f, 19.9999998f, 0f, 40f, 0f, 40f);
					path.Offset(50, 35);

					// draw using GetBounds
					paint.Color = SKColors.LightBlue;
					canvas.DrawPath(path, paint);

					path.GetBounds(out var rect);

					paint.Color = SKColors.DarkBlue;
					canvas.DrawRect(rect, paint);

					canvas.DrawText("Bounds", rect.Left, rect.Bottom + paint.TextSize + 10, textPaint);

					// move for next curve
					path.Offset(100, 0);

					// draw using GetTightBounds
					paint.Color = SKColors.LightBlue;
					canvas.DrawPath(path, paint);

					path.GetTightBounds(out rect);

					paint.Color = SKColors.DarkBlue;
					canvas.DrawRect(rect, paint);

					canvas.DrawText("TightBounds", rect.Left, rect.Bottom + paint.TextSize + 10, textPaint);
				}
			}
		}

		private static void DrawMatrixSample(SKCanvas canvas, int width, int height)
		{
			var size = ((float)height > width ? width : height) * 0.5f;
			var center = new SKPoint((width - size) / 2f, (height - size) / 2f);

			// draw these at specific locations
			var leftRect = SKRect.Create(center.X - size / 2f, center.Y, size, size);
			var rightRect = SKRect.Create(center.X + size / 2f, center.Y, size, size);

			// draw this at the current location / transformation
			var rotatedRect = SKRect.Create(0f, 0f, size, size);

			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;
				canvas.Clear(SKColors.Purple);

				// draw
				paint.Color = SKColors.DarkBlue;
				canvas.DrawRect(leftRect, paint);

				// save
				canvas.Save();

				// transform
				canvas.Translate(width / 2f, center.Y);
				canvas.RotateDegrees(45);

				// draw
				paint.Color = SKColors.Green;
				canvas.DrawRoundRect(rotatedRect, 10, 10, paint);

				// undo transform / restore
				canvas.Restore();

				// draw
				paint.Color = SKColors.LightBlue;
				canvas.DrawRect(rightRect, paint);
			}
		}

		private void DrawShader(SKCanvas canvas, int width, int height)
		{
			var ltColor = SKColors.White;
			var dkColor = SKColors.Black;

			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;
				using (var shader = SKShader.CreateLinearGradient(
					new SKPoint(0, 0),
					new SKPoint(0, height),
					new[] { ltColor, dkColor },
					null,
					SKShaderTileMode.Clamp))
				{

					paint.Shader = shader;
					canvas.DrawPaint(paint);
				}
			}

			// Center and Scale the Surface
			var scale = (width < height ? width : height) / (240f);
			canvas.Translate(width / 2f, height / 2f);
			canvas.Scale(scale, scale);
			canvas.Translate(-128, -128);

			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;
				using (var shader = SKShader.CreateTwoPointConicalGradient(
					new SKPoint(115.2f, 102.4f),
					25.6f,
					new SKPoint(102.4f, 102.4f),
					128.0f,
					new[] { ltColor, dkColor },
					null,
					SKShaderTileMode.Clamp
				))
				{
					paint.Shader = shader;

					canvas.DrawOval(new SKRect(51.2f, 51.2f, 204.8f, 204.8f), paint);
				}
			}
		}

		private static void Sample1(SKPaintSurfaceEventArgs e)
		{
			Console.WriteLine($"AlphaType:{e.Info.AlphaType} BitsPerPixel:{e.Info.BitsPerPixel} ColorSpace:{e.Info.ColorSpace} ColorType:{e.Info.ColorType}");
			Console.WriteLine($"SKTypeface.Default:{SKTypeface.Default} FamilyName:{SKTypeface.Default.FamilyName} FontSlant:{SKTypeface.Default.FontSlant}");

			// the the canvas and properties
			var canvas = e.Surface.Canvas;

			// get the screen density for scaling
			//var display = DisplayInformation.GetForCurrentView();
			//var scale = display.LogicalDpi / 96.0f;
			var scale = 1.0f;
			var scaledSize = new SKSize(e.Info.Width / scale, e.Info.Height / scale);

			// handle the device screen density 
			canvas.Scale(scale);

			// make sure the canvas is blank
			canvas.Clear(SKColors.Red);

			// draw some text
			var paint = new SKPaint
			{
				Color = SKColors.Blue,
				IsAntialias = true,
				Typeface = SKTypeface.Default,
				Style = SKPaintStyle.Fill,
				TextAlign = SKTextAlign.Center,
				TextSize = 24
			};

			canvas.DrawLine(0, 0, scaledSize.Width, scaledSize.Height, paint);

			var coord = new SKPoint(scaledSize.Width / 2, (scaledSize.Height + paint.TextSize) / 2);
			canvas.DrawText("Hello from SkiaSharp", coord, paint);

			canvas.Flush();
		}
	}
}

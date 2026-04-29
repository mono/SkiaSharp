using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class WideGamutP3Sample : CanvasSampleBase
{
	private int viewIndex;
	private float intensity = 1.0f;

	private static readonly string[] Views = { "Color Swatches", "Gradient Comparison", "Gamut Map" };

	public override string Title => "Wide-Gamut P3";

	public override DateOnly? DateAdded => new DateOnly(2026, 4, 27);

	public override string Description =>
		"Compare Display P3 wide-gamut colors with sRGB. P3 reds, greens, and blues are more vivid than sRGB can represent.";

	public override string Category => SampleManager.Shaders;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("view", "View", Views, viewIndex),
		new SliderControl("intensity", "P3 Intensity", 0.5f, 1.0f, intensity, 0.01f,
			Description: "How far into P3-exclusive gamut to push colors."),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "view": viewIndex = (int)value; break;
			case "intensity": intensity = (float)value; break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(new SKColor(0xFF1A1A2E));

		switch (viewIndex)
		{
			case 0: DrawSwatches(canvas, width, height); break;
			case 1: DrawGradientComparison(canvas, width, height); break;
			case 2: DrawGamutMap(canvas, width, height); break;
		}
	}

	private void DrawSwatches(SKCanvas canvas, int width, int height)
	{
		var margin = 20f;
		var headerH = 60f;

		// Title
		DrawTitle(canvas, width, "Display P3 vs sRGB Color Swatches");

		// Define P3-exclusive colors — values > 1.0 in sRGB are outside sRGB gamut
		var swatches = new (string Label, SKColorF SrgbColor, SKColorF P3Color)[]
		{
			("Red",     new SKColorF(1.0f, 0.0f, 0.0f),   new SKColorF(intensity, 0.0f, 0.0f)),
			("Green",   new SKColorF(0.0f, 1.0f, 0.0f),   new SKColorF(0.0f, intensity, 0.0f)),
			("Blue",    new SKColorF(0.0f, 0.0f, 1.0f),   new SKColorF(0.0f, 0.0f, intensity)),
			("Orange",  new SKColorF(1.0f, 0.5f, 0.0f),   new SKColorF(intensity, 0.35f, 0.0f)),
			("Magenta", new SKColorF(1.0f, 0.0f, 0.5f),   new SKColorF(intensity, 0.0f, 0.4f)),
			("Cyan",    new SKColorF(0.0f, 0.8f, 1.0f),   new SKColorF(0.0f, 0.7f * intensity, intensity)),
		};

		var swatchW = (width - margin * 2) / swatches.Length;
		var swatchH = (height - headerH - margin * 4) / 2f;

		using var srgbCS = SKColorSpace.CreateSrgb();
		using var p3CS = SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Srgb, SKColorSpaceXyz.DisplayP3);

		using var labelFont = new SKFont { Size = 12 };
		using var labelPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
		using var smallFont = new SKFont { Size = 10 };

		for (var i = 0; i < swatches.Length; i++)
		{
			var x = margin + i * swatchW;
			var swatch = swatches[i];

			// sRGB swatch (top row)
			var srgbRect = new SKRect(x + 4, headerH, x + swatchW - 4, headerH + swatchH - 4);
			using (var paint = new SKPaint { IsAntialias = true })
			{
				paint.SetColor(swatch.SrgbColor, srgbCS);
				canvas.DrawRoundRect(srgbRect, 8, 8, paint);
			}

			// P3 swatch (bottom row)
			var p3Rect = new SKRect(x + 4, headerH + swatchH + 20, x + swatchW - 4, headerH + swatchH * 2 + 16);
			using (var paint = new SKPaint { IsAntialias = true })
			{
				paint.SetColor(swatch.P3Color, p3CS);
				canvas.DrawRoundRect(p3Rect, 8, 8, paint);
			}

			// Labels
			var labelW = labelFont.MeasureText(swatch.Label);
			canvas.DrawText(swatch.Label, x + swatchW / 2f - labelW / 2f, headerH + swatchH + 12, labelFont, labelPaint);
		}

		// Row labels
		labelPaint.Color = new SKColor(180, 180, 180);
		canvas.DrawText("sRGB", margin, headerH - 6, smallFont, labelPaint);
		canvas.DrawText("Display P3", margin, headerH + swatchH + 32, smallFont, labelPaint);
	}

	private void DrawGradientComparison(SKCanvas canvas, int width, int height)
	{
		DrawTitle(canvas, width, "Gradient: sRGB vs Display P3");

		var margin = 20f;
		var gradH = (height - 100) / 2f - 20f;
		var y1 = 70f;
		var y2 = y1 + gradH + 40f;

		using var srgbCS = SKColorSpace.CreateSrgb();
		using var p3CS = SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Srgb, SKColorSpaceXyz.DisplayP3);

		// sRGB gradient
		DrawGradientRow(canvas, "sRGB Gradient", margin, y1, width - margin * 2, gradH, srgbCS);

		// P3 gradient
		DrawGradientRow(canvas, "Display P3 Gradient", margin, y2, width - margin * 2, gradH, p3CS);
	}

	private void DrawGradientRow(SKCanvas canvas, string label, float x, float y, float w, float h, SKColorSpace cs)
	{
		using var labelFont = new SKFont { Size = 13 };
		using var labelPaint = new SKPaint { Color = new SKColor(180, 180, 180), IsAntialias = true };
		canvas.DrawText(label, x, y - 6, labelFont, labelPaint);

		var rect = new SKRect(x, y, x + w, y + h);

		// Draw a gradient from deep red → green → blue in the given color space
		// We create an offscreen surface in the target color space for accurate rendering
		var info = new SKImageInfo((int)w, (int)h, SKColorType.RgbaF16, SKAlphaType.Premul, cs);
		using var surface = SKSurface.Create(info);
		if (surface == null)
		{
			// Fallback: draw with plain gradient in sRGB
			DrawFallbackGradient(canvas, rect);
			return;
		}

		var offCanvas = surface.Canvas;
		offCanvas.Clear(SKColors.Transparent);

		// Create gradient colors as SKColorF — these will be interpreted in the surface's color space
		using var paint = new SKPaint { IsAntialias = true };
		using var shader = SKShader.CreateLinearGradient(
			new SKPoint(0, 0),
			new SKPoint(w, 0),
			new SKColor[]
			{
				new(0xFFFF0000), // red
				new(0xFFFF8800), // orange
				new(0xFFFFFF00), // yellow
				new(0xFF00FF00), // green
				new(0xFF0088FF), // blue
				new(0xFFFF00FF), // magenta
			},
			null,
			SKShaderTileMode.Clamp);

		paint.Shader = shader;
		offCanvas.DrawRect(0, 0, w, h, paint);

		using var image = surface.Snapshot();
		canvas.DrawImage(image, rect);

		// Border
		using var borderPaint = new SKPaint
		{
			IsStroke = true,
			StrokeWidth = 1,
			Color = new SKColor(255, 255, 255, 60),
			IsAntialias = true,
		};
		canvas.DrawRoundRect(rect, 4, 4, borderPaint);
	}

	private static void DrawFallbackGradient(SKCanvas canvas, SKRect rect)
	{
		using var shader = SKShader.CreateLinearGradient(
			new SKPoint(rect.Left, rect.Top),
			new SKPoint(rect.Right, rect.Top),
			new SKColor[] { SKColors.Red, SKColors.Green, SKColors.Blue },
			null,
			SKShaderTileMode.Clamp);

		using var paint = new SKPaint { Shader = shader };
		canvas.DrawRoundRect(rect, 4, 4, paint);
	}

	private void DrawGamutMap(SKCanvas canvas, int width, int height)
	{
		DrawTitle(canvas, width, "CIE Gamut Map — sRGB vs Display P3");

		var cx = width / 2f;
		var cy = height / 2f + 10;
		var size = Math.Min(width, height) * 0.38f;

		// Draw a simplified CIE-like diagram showing sRGB and P3 gamut triangles
		// These are approximate chromaticity coordinates mapped to canvas

		// CIE horseshoe background (approximated as a rounded triangle)
		DrawCIEBackground(canvas, cx, cy, size);

		// sRGB triangle (approximate chromaticity)
		var srgbR = MapChromaticity(0.64f, 0.33f, cx, cy, size);
		var srgbG = MapChromaticity(0.30f, 0.60f, cx, cy, size);
		var srgbB = MapChromaticity(0.15f, 0.06f, cx, cy, size);

		using var srgbBuilder = new SKPathBuilder();
		srgbBuilder.MoveTo(srgbR);
		srgbBuilder.LineTo(srgbG);
		srgbBuilder.LineTo(srgbB);
		srgbBuilder.Close();
		using var srgbPath = srgbBuilder.Detach();

		// P3 triangle (wider gamut)
		var p3R = MapChromaticity(0.68f, 0.32f, cx, cy, size);
		var p3G = MapChromaticity(0.265f, 0.69f, cx, cy, size);
		var p3B = MapChromaticity(0.15f, 0.06f, cx, cy, size);

		using var p3Builder = new SKPathBuilder();
		p3Builder.MoveTo(p3R);
		p3Builder.LineTo(p3G);
		p3Builder.LineTo(p3B);
		p3Builder.Close();
		using var p3Path = p3Builder.Detach();

		// Fill P3 triangle (subtle)
		using (var p3Fill = new SKPaint { Color = new SKColor(100, 200, 255, 30), IsAntialias = true })
			canvas.DrawPath(p3Path, p3Fill);

		// Fill sRGB triangle (subtle)
		using (var srgbFill = new SKPaint { Color = new SKColor(255, 255, 255, 25), IsAntialias = true })
			canvas.DrawPath(srgbPath, srgbFill);

		// Draw P3 outline
		using (var p3Stroke = new SKPaint
		{
			IsStroke = true, StrokeWidth = 2.5f,
			Color = new SKColor(0xFF5DADE2), IsAntialias = true,
		})
			canvas.DrawPath(p3Path, p3Stroke);

		// Draw sRGB outline
		using (var srgbStroke = new SKPaint
		{
			IsStroke = true, StrokeWidth = 2.5f,
			Color = new SKColor(0xFFE74C3C), IsAntialias = true,
			PathEffect = SKPathEffect.CreateDash(new[] { 8f, 4f }, 0),
		})
			canvas.DrawPath(srgbPath, srgbStroke);

		// Vertex labels
		using var vertFont = new SKFont { Size = 11 };
		using var vertPaint = new SKPaint { IsAntialias = true };

		vertPaint.Color = new SKColor(0xFFE74C3C);
		canvas.DrawText("sRGB R", srgbR.X + 6, srgbR.Y + 4, vertFont, vertPaint);
		canvas.DrawText("G", srgbG.X - 4, srgbG.Y - 8, vertFont, vertPaint);
		canvas.DrawText("B", srgbB.X - 14, srgbB.Y + 16, vertFont, vertPaint);

		vertPaint.Color = new SKColor(0xFF5DADE2);
		canvas.DrawText("P3 R", p3R.X + 6, p3R.Y - 8, vertFont, vertPaint);
		canvas.DrawText("G", p3G.X + 8, p3G.Y - 4, vertFont, vertPaint);

		// White point
		var wp = MapChromaticity(0.3127f, 0.3290f, cx, cy, size);
		using (var wpPaint = new SKPaint { Color = SKColors.White, IsAntialias = true })
			canvas.DrawCircle(wp, 4, wpPaint);
		canvas.DrawText("D65", wp.X + 6, wp.Y - 4, vertFont, new SKPaint { Color = SKColors.White, IsAntialias = true });

		// Legend
		var legendY = height - 40f;
		using var legendFont = new SKFont { Size = 12 };

		using (var legendPaint = new SKPaint { Color = new SKColor(0xFFE74C3C), IsAntialias = true })
			canvas.DrawText("— — sRGB", 20, legendY, legendFont, legendPaint);

		using (var legendPaint = new SKPaint { Color = new SKColor(0xFF5DADE2), IsAntialias = true })
			canvas.DrawText("——— Display P3", 120, legendY, legendFont, legendPaint);
	}

	private static void DrawCIEBackground(SKCanvas canvas, float cx, float cy, float size)
	{
		// Approximate the CIE horseshoe shape with a filled path in dark grey
		using var bgBuilder = new SKPathBuilder();

		// Simplified spectral locus points (chromaticity coords mapped to canvas)
		var points = new (float x, float y)[]
		{
			(0.17f, 0.005f), (0.17f, 0.05f), (0.14f, 0.08f),
			(0.08f, 0.15f), (0.05f, 0.25f), (0.03f, 0.35f),
			(0.02f, 0.45f), (0.01f, 0.55f), (0.01f, 0.60f),
			(0.07f, 0.68f), (0.15f, 0.75f), (0.22f, 0.71f),
			(0.30f, 0.60f), (0.40f, 0.52f), (0.50f, 0.44f),
			(0.58f, 0.38f), (0.63f, 0.34f), (0.69f, 0.31f),
			(0.73f, 0.27f),
		};

		var mapped = new SKPoint[points.Length];
		for (var i = 0; i < points.Length; i++)
			mapped[i] = MapChromaticity(points[i].x, points[i].y, cx, cy, size);

		bgBuilder.MoveTo(mapped[0]);
		for (var i = 1; i < mapped.Length; i++)
			bgBuilder.LineTo(mapped[i]);
		bgBuilder.Close();
		using var bgPath = bgBuilder.Detach();

		using var bgPaint = new SKPaint { Color = new SKColor(40, 40, 60, 100), IsAntialias = true };
		canvas.DrawPath(bgPath, bgPaint);

		using var outlinePaint = new SKPaint
		{
			IsStroke = true,
			StrokeWidth = 1,
			Color = new SKColor(100, 100, 120, 80),
			IsAntialias = true,
		};
		canvas.DrawPath(bgPath, outlinePaint);
	}

	private static SKPoint MapChromaticity(float cx, float cy, float centerX, float centerY, float scale)
	{
		// Map CIE xy chromaticity (roughly [0,0.8] × [0,0.9]) to canvas coordinates
		var x = centerX + (cx - 0.35f) * scale * 3f;
		var y = centerY - (cy - 0.35f) * scale * 3f;
		return new SKPoint(x, y);
	}

	private static void DrawTitle(SKCanvas canvas, int width, string text)
	{
		using var titleFont = new SKFont { Size = 16 };
		using var titlePaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
		var tw = titleFont.MeasureText(text);
		canvas.DrawText(text, (width - tw) / 2f, 30, titleFont, titlePaint);
	}
}

using System;
using System.IO;
using System.Threading.Tasks;
using Svg.Skia;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class HeroImageSample : CanvasSampleBase
{
	private float curveIntensity = 1f;
	private float glowSize = 1f;
	private int colorScheme;

	private SKSvg? skiaSharpSvg;

	private static readonly string[] ColorSchemes = { "Default", "Cool Blue", "Warm Sunset", "Monochrome" };

	public override string Title => "Hero Image";

	public override DateOnly? DateAdded => new DateOnly(2026, 4, 29);

	public override string Description =>
		"Generate a stylized SkiaSharp banner with flowing Bézier curves, gradient text, and a frosted glass logo card.";

	public override IReadOnlyList<string> ApiTags =>
	[
		"SKSvg", "SKImage", "SKFontVariationPositionCoordinate",
		"SKMaskFilter", "SKMaskFilter.CreateBlur",
		"SKImageFilter", "SKImageFilter.CreateBlur",
		"SKShader", "SKShader.CreateRadialGradient", "SKShader.CreateLinearGradient",
		"SKPathBuilder", "SKPath", "SKRoundRect",
		"SKCanvas.DrawPicture", "SKCanvas.ClipRoundRect",
		"SKCanvas.DrawPath", "SKCanvas.DrawCircle", "SKCanvas.DrawRect",
		"SKCanvas.DrawRoundRect", "SKCanvas.DrawText", "SKCanvas.DrawImage",
		"SKCanvas", "SKPaint", "SKFont", "SKTypeface",
	];

	public override string Category => SampleManager.General;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("colorScheme", "Color Scheme", ColorSchemes, colorScheme),
		new SliderControl("curveIntensity", "Curve Intensity", 0f, 2f, curveIntensity, 0.1f),
		new SliderControl("glowSize", "Glow Size", 0f, 2f, glowSize, 0.1f),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "colorScheme":
				colorScheme = (int)value;
				break;
			case "curveIntensity":
				curveIntensity = (float)value;
				break;
			case "glowSize":
				glowSize = (float)value;
				break;
		}
	}

	protected override Task OnInit()
	{
		skiaSharpSvg = new SKSvg();
		using (var stream = SampleMedia.Images.SkiaSharpLogoSvg)
			skiaSharpSvg.Load(stream);

		return base.OnInit();
	}

	protected override void OnDestroy()
	{
		skiaSharpSvg?.Dispose();
		skiaSharpSvg = null;
		base.OnDestroy();
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		var colors = GetColorScheme();

		DrawBackground(canvas, width, height, colors);
		DrawDecorativeCurves(canvas, width, height, colors);
		DrawCenterGlow(canvas, width, height, colors);

		// Frosted glass card with logo
		float scale = Math.Min(width, height) / 1080f;
		var skiaCard = GetCardRect(width * 0.16f, height * 0.46f, 280 * scale, 280 * scale);

		using var bgSnapshot = canvas.Surface?.Snapshot();
		if (bgSnapshot != null)
		{
			DrawFrostedCard(canvas, bgSnapshot, skiaCard);
		}

		DrawSkiaSharpIcon(canvas, skiaCard);
		DrawText(canvas, width, height, skiaCard);
	}

	private static SKRect GetCardRect(float cx, float cy, float w, float h)
		=> new(cx - w / 2, cy - h / 2, cx + w / 2, cy + h / 2);

	private (SKColor bg1, SKColor bg2, SKColor accent1, SKColor accent2, SKColor accent3, SKColor accent4, SKColor accent5) GetColorScheme()
	{
		return colorScheme switch
		{
			1 => (new SKColor(0x0A, 0x14, 0x28), new SKColor(0x0C, 0x1A, 0x38),
			      new SKColor(0x15, 0x9B, 0xFF), new SKColor(0x3B, 0x82, 0xF6),
			      new SKColor(0x67, 0xE5, 0xAD), new SKColor(0x06, 0xB6, 0xD4),
			      new SKColor(0x81, 0x8C, 0xF8)),
			2 => (new SKColor(0x1A, 0x0A, 0x14), new SKColor(0x28, 0x0C, 0x10),
			      new SKColor(0xFF, 0x6B, 0x35), new SKColor(0xF6, 0x3B, 0x5C),
			      new SKColor(0xFF, 0xC1, 0x07), new SKColor(0xE5, 0x67, 0x67),
			      new SKColor(0xF8, 0x81, 0xCF)),
			3 => (new SKColor(0x12, 0x12, 0x12), new SKColor(0x1A, 0x1A, 0x1A),
			      new SKColor(0xAA, 0xAA, 0xAA), new SKColor(0x88, 0x88, 0x88),
			      new SKColor(0xCC, 0xCC, 0xCC), new SKColor(0x66, 0x66, 0x66),
			      new SKColor(0x99, 0x99, 0x99)),
			_ => (new SKColor(0x0B, 0x10, 0x26), new SKColor(0x12, 0x0C, 0x30),
			      new SKColor(0x2F, 0x45, 0x9D), new SKColor(0xD9, 0x57, 0x3F),
			      new SKColor(0x7B, 0xA7, 0x45), new SKColor(0x7A, 0x67, 0xF8),
			      new SKColor(0x15, 0x9B, 0xFF)),
		};
	}

	private void DrawBackground(SKCanvas canvas, int width, int height,
		(SKColor bg1, SKColor bg2, SKColor accent1, SKColor accent2, SKColor accent3, SKColor accent4, SKColor accent5) colors)
	{
		var bgColors = new SKColor[] { colors.bg1, colors.bg2, colors.bg1 };
		using var bgShader = SKShader.CreateRadialGradient(
			new SKPoint(width * 0.5f, height * 0.45f),
			width * 0.8f,
			bgColors, new float[] { 0f, 0.5f, 1f },
			SKShaderTileMode.Clamp);
		using var bgPaint = new SKPaint { Shader = bgShader };
		canvas.DrawRect(0, 0, width, height, bgPaint);

		using var dotPaint = new SKPaint
		{
			Color = new SKColor(255, 255, 255, 8),
			IsAntialias = true
		};
		var rng = new Random(42);
		for (int i = 0; i < 150; i++)
		{
			float x = (float)(rng.NextDouble() * width);
			float y = (float)(rng.NextDouble() * height);
			float r = (float)(rng.NextDouble() * 1.5 + 0.3);
			canvas.DrawCircle(x, y, r, dotPaint);
		}
	}

	private void DrawDecorativeCurves(SKCanvas canvas, int width, int height,
		(SKColor bg1, SKColor bg2, SKColor accent1, SKColor accent2, SKColor accent3, SKColor accent4, SKColor accent5) colors)
	{
		var intensity = curveIntensity;
		var curves = new[]
		{
			(Color: colors.accent1.WithAlpha(50), StrokeWidth: 3f,
			 P0: new SKPoint(-100, height * 0.85f),
			 C1: new SKPoint(width * 0.2f, height * (0.5f - 0.2f * intensity)),
			 C2: new SKPoint(width * 0.6f, height * (0.5f + 0.4f * intensity)),
			 P3: new SKPoint(width + 100, height * 0.15f)),

			(Color: colors.accent2.WithAlpha(40), StrokeWidth: 2.5f,
			 P0: new SKPoint(-50, height * 0.7f),
			 C1: new SKPoint(width * 0.3f, height * (0.5f - 0.4f * intensity)),
			 C2: new SKPoint(width * 0.7f, height * (0.5f + 0.45f * intensity)),
			 P3: new SKPoint(width + 50, height * 0.25f)),

			(Color: colors.accent3.WithAlpha(40), StrokeWidth: 2f,
			 P0: new SKPoint(-80, height * 0.95f),
			 C1: new SKPoint(width * 0.15f, height * 0.5f),
			 C2: new SKPoint(width * 0.5f, height * (0.5f + 0.2f * intensity)),
			 P3: new SKPoint(width + 80, height * 0.05f)),

			(Color: colors.accent4.WithAlpha(35), StrokeWidth: 2f,
			 P0: new SKPoint(width * 0.1f, -50),
			 C1: new SKPoint(width * 0.35f, height * (0.5f + 0.3f * intensity)),
			 C2: new SKPoint(width * 0.65f, height * (0.5f - 0.3f * intensity)),
			 P3: new SKPoint(width * 0.9f, height + 50)),

			(Color: colors.accent5.WithAlpha(30), StrokeWidth: 1.8f,
			 P0: new SKPoint(-100, height * 0.4f),
			 C1: new SKPoint(width * 0.25f, height * (0.5f - 0.45f * intensity)),
			 C2: new SKPoint(width * 0.75f, height * (0.5f + 0.35f * intensity)),
			 P3: new SKPoint(width + 100, height * 0.5f)),
		};

		foreach (var curve in curves)
		{
			using var builder = new SKPathBuilder();
			builder.MoveTo(curve.P0);
			builder.CubicTo(curve.C1, curve.C2, curve.P3);
			using var path = builder.Detach();

			using var paint = new SKPaint
			{
				Style = SKPaintStyle.Stroke,
				Color = curve.Color,
				StrokeWidth = curve.StrokeWidth,
				IsAntialias = true,
				StrokeCap = SKStrokeCap.Round,
			};
			canvas.DrawPath(path, paint);

			using var glowPaint = new SKPaint
			{
				Style = SKPaintStyle.Stroke,
				Color = curve.Color.WithAlpha((byte)(curve.Color.Alpha / 2)),
				StrokeWidth = curve.StrokeWidth * 6 * glowSize,
				IsAntialias = true,
				StrokeCap = SKStrokeCap.Round,
				MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 12 * glowSize),
			};
			canvas.DrawPath(path, glowPaint);
		}
	}

	private void DrawCenterGlow(SKCanvas canvas, int width, int height,
		(SKColor bg1, SKColor bg2, SKColor accent1, SKColor accent2, SKColor accent3, SKColor accent4, SKColor accent5) colors)
	{
		using var glowShader = SKShader.CreateRadialGradient(
			new SKPoint(width * 0.5f, height * 0.46f),
			Math.Min(width, height) * 0.4f * glowSize,
			new SKColor[] { colors.accent1.WithAlpha(30), new SKColor(0, 0, 0, 0) },
			new float[] { 0f, 1f },
			SKShaderTileMode.Clamp);
		using var glowPaint = new SKPaint { Shader = glowShader };
		canvas.DrawRect(0, 0, width, height, glowPaint);
	}

	private void DrawText(SKCanvas canvas, int width, int height, SKRect logoCard)
	{
		float scale = Math.Min(width, height) / 1080f;
		var baseTypeface = SampleMedia.Fonts.Default;

		// Create a bold variant using variable font weight axis
		var boldPosition = new[]
		{
			new SKFontVariationPositionCoordinate { Axis = SKFourByteTag.Parse("wght"), Value = 700 },
		};
		using var boldTypeface = baseTypeface.Clone(boldPosition) ?? baseTypeface;

		// Layout: [margin | logo | gap | text | margin]
		float leftMargin = logoCard.Left;
		float textAreaLeft = logoCard.Right + leftMargin;
		float textAreaRight = width - leftMargin;
		float textCenterX = (textAreaLeft + textAreaRight) / 2;

		// "SkiaSharp" in large bold gradient text
		using var titleFont = new SKFont(boldTypeface, 170 * scale);
		var titleText = "SkiaSharp";
		float titleWidth = titleFont.MeasureText(titleText);
		float titleX = textCenterX - titleWidth / 2;
		float titleY = height * 0.54f;

		using var titleShader = SKShader.CreateLinearGradient(
			new SKPoint(titleX, titleY - 140 * scale),
			new SKPoint(titleX + titleWidth, titleY),
			new SKColor[]
			{
				new(0x15, 0x9B, 0xFF),
				new(0x67, 0xE5, 0xAD),
				new(0x7B, 0xA7, 0x45),
			},
			new float[] { 0f, 0.5f, 1f },
			SKShaderTileMode.Clamp);
		using var titlePaint = new SKPaint
		{
			Shader = titleShader,
			IsAntialias = true,
		};
		canvas.DrawText(titleText, titleX, titleY, SKTextAlign.Left, titleFont, titlePaint);
	}

	private static void DrawFrostedCard(SKCanvas canvas, SKImage bgSnapshot, SKRect cardRect)
	{
		float cornerRadius = 24;
		var roundRect = new SKRoundRect(cardRect, cornerRadius, cornerRadius);

		canvas.Save();
		canvas.ClipRoundRect(roundRect, antialias: true);

		using var blurPaint = new SKPaint
		{
			ImageFilter = SKImageFilter.CreateBlur(20, 20),
			IsAntialias = true,
		};
		canvas.DrawImage(bgSnapshot, 0, 0, blurPaint);

		using var glassPaint = new SKPaint
		{
			Color = new SKColor(255, 255, 255, 12),
			IsAntialias = true,
		};
		canvas.DrawRoundRect(roundRect, glassPaint);

		using var innerGradient = SKShader.CreateLinearGradient(
			new SKPoint(cardRect.Left, cardRect.Top),
			new SKPoint(cardRect.Left, cardRect.Bottom),
			new SKColor[] { new(255, 255, 255, 10), new(255, 255, 255, 0) },
			new float[] { 0f, 1f },
			SKShaderTileMode.Clamp);
		using var innerPaint = new SKPaint { Shader = innerGradient, IsAntialias = true };
		canvas.DrawRoundRect(roundRect, innerPaint);

		canvas.Restore();

		using var borderPaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = new SKColor(255, 255, 255, 30),
			StrokeWidth = 1f,
			IsAntialias = true,
		};
		canvas.DrawRoundRect(roundRect, borderPaint);
	}

	private void DrawSkiaSharpIcon(SKCanvas canvas, SKRect cardRect)
	{
		DrawSvgInCard(canvas, skiaSharpSvg, cardRect);
	}

	private static void DrawSvgInCard(SKCanvas canvas, SKSvg? svg, SKRect cardRect)
	{
		if (svg?.Picture == null) return;

		var svgBounds = svg.Picture.CullRect;
		if (svgBounds.IsEmpty) return;

		float padding = cardRect.Width * 0.12f;
		float availW = cardRect.Width - padding * 2;
		float availH = cardRect.Height - padding * 2;
		float scale = Math.Min(availW / svgBounds.Width, availH / svgBounds.Height);

		float drawW = svgBounds.Width * scale;
		float drawH = svgBounds.Height * scale;
		float drawX = cardRect.MidX - drawW / 2;
		float drawY = cardRect.MidY - drawH / 2;

		canvas.Save();
		canvas.Translate(drawX - svgBounds.Left * scale, drawY - svgBounds.Top * scale);
		canvas.Scale(scale);
		canvas.DrawPicture(svg.Picture);
		canvas.Restore();
	}
}

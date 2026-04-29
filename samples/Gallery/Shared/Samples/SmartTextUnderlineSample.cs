using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class SmartTextUnderlineSample : CanvasSampleBase
{
	private float textSize = 64f;
	private float underlineThickness = 2.5f;
	private float underlineOffset = 4f;
	private float gapPadding = 2f;
	private int textIndex;
	private int colorSchemeIndex;
	private bool showNaive = true;

	private static readonly string[] TextOptions =
	{
		"Skipping g's & y's",
		"Typography playground",
		"Quagmire & Gryphon",
		"Sixty Jumping Dogs",
		"Happy people enjoy quiet pajamas",
	};

	private static readonly string[] ColorSchemes = { "Classic Blue", "Warm Coral", "Forest Green", "Midnight" };
	private static readonly SKColor[] TextColors = { new(0xFF1A5276), new(0xFFCB4335), new(0xFF196F3D), new(0xFFF0F0F0) };
	private static readonly SKColor[] UnderlineColors = { new(0xFF3498DB), new(0xFFE74C3C), new(0xFF27AE60), new(0xFF5DADE2) };
	private static readonly SKColor[] BgColors = { new(0xFFF8F9FA), new(0xFFFDF2F0), new(0xFFF0F7F0), new(0xFF1C2833) };

	public override string Title => "Smart Underline";

	public override DateOnly? DateAdded => new DateOnly(2026, 4, 27);

	public override string Description =>
		"Text underlines that intelligently break around descenders using GetIntercepts().";

	public override string Category => SampleManager.Text;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("text", "Text", TextOptions, textIndex),
		new PickerControl("color", "Color Scheme", ColorSchemes, colorSchemeIndex),
		new SliderControl("textSize", "Text Size", 32, 96, textSize, 2),
		new SliderControl("thickness", "Underline Thickness", 1f, 6f, underlineThickness, 0.5f),
		new SliderControl("offset", "Underline Offset", 0f, 12f, underlineOffset, 0.5f),
		new SliderControl("gapPad", "Gap Padding", 0f, 8f, gapPadding, 0.5f),
		new ToggleControl("showNaive", "Show Naïve Underline", showNaive),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "text": textIndex = (int)value; break;
			case "color": colorSchemeIndex = (int)value; break;
			case "textSize": textSize = (float)value; break;
			case "thickness": underlineThickness = (float)value; break;
			case "offset": underlineOffset = (float)value; break;
			case "gapPad": gapPadding = (float)value; break;
			case "showNaive": showNaive = (bool)value; break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		var bgColor = BgColors[colorSchemeIndex];
		canvas.Clear(bgColor);

		var text = TextOptions[textIndex];
		var textColor = TextColors[colorSchemeIndex];
		var underlineColor = UnderlineColors[colorSchemeIndex];

		using var font = new SKFont(SKTypeface.Default, textSize)
		{
			Subpixel = true,
		};
		var metrics = font.Metrics;

		// Center the text horizontally, place it in upper-center area
		var textWidth = font.MeasureText(text);
		var x = (width - textWidth) / 2f;
		var baseY = height * 0.35f;

		// --- Draw the naïve underline version (for comparison) ---
		if (showNaive)
		{
			DrawLabel(canvas, "Naïve underline (no descender gaps):", 12, baseY - textSize * 0.8f, bgColor);

			using var naiveTextPaint = new SKPaint { Color = textColor, IsAntialias = true };
			canvas.DrawText(text, x, baseY, font, naiveTextPaint);

			var naiveUnderlineY = baseY + underlineOffset;
			using var naiveLinePaint = new SKPaint
			{
				Color = underlineColor.WithAlpha(120),
				IsAntialias = true,
				IsStroke = true,
				StrokeWidth = underlineThickness,
				StrokeCap = SKStrokeCap.Round,
			};
			canvas.DrawLine(x, naiveUnderlineY, x + textWidth, naiveUnderlineY, naiveLinePaint);
		}

		// --- Draw the smart underline version ---
		var smartY = showNaive ? height * 0.65f : height * 0.45f;
		DrawLabel(canvas, "Smart underline (gaps around descenders):", 12, smartY - textSize * 0.8f, bgColor);

		using var textPaint = new SKPaint { Color = textColor, IsAntialias = true };
		canvas.DrawText(text, x, smartY, font, textPaint);

		// Build a text blob so we can use GetIntercepts
		using var blob = SKTextBlob.Create(text, font, new SKPoint(x, smartY));
		if (blob == null)
			return;

		// The underline sits just below the baseline
		var underlineY = smartY + underlineOffset;
		var upperBound = underlineY - underlineThickness / 2f;
		var lowerBound = underlineY + underlineThickness / 2f;

		// Get intercept pairs where descenders cross the underline region
		var intercepts = blob.GetIntercepts(upperBound, lowerBound);

		// Draw the underline as segments with gaps
		using var linePaint = new SKPaint
		{
			Color = underlineColor,
			IsAntialias = true,
			IsStroke = true,
			StrokeWidth = underlineThickness,
			StrokeCap = SKStrokeCap.Round,
		};

		var lineStart = x;
		var lineEnd = x + textWidth;

		if (intercepts.Length == 0)
		{
			// No descenders crossing — draw a single line
			canvas.DrawLine(lineStart, underlineY, lineEnd, underlineY, linePaint);
		}
		else
		{
			// Draw segments between intercept pairs
			var segStart = lineStart;
			for (var i = 0; i < intercepts.Length; i += 2)
			{
				var gapLeft = intercepts[i] - gapPadding;
				var gapRight = (i + 1 < intercepts.Length)
					? intercepts[i + 1] + gapPadding
					: intercepts[i] + gapPadding;

				if (segStart < gapLeft)
					canvas.DrawLine(segStart, underlineY, gapLeft, underlineY, linePaint);

				segStart = gapRight;
			}

			// Draw remaining segment after last gap
			if (segStart < lineEnd)
				canvas.DrawLine(segStart, underlineY, lineEnd, underlineY, linePaint);
		}

		// Draw info footer
		DrawFooter(canvas, width, height, intercepts.Length / 2, bgColor);
	}

	private static void DrawLabel(SKCanvas canvas, string text, float x, float y, SKColor bg)
	{
		var isDark = bg.Red < 100 && bg.Green < 100 && bg.Blue < 100;
		using var labelFont = new SKFont { Size = 13 };
		using var labelPaint = new SKPaint
		{
			Color = isDark ? new SKColor(180, 180, 180) : new SKColor(100, 100, 100),
			IsAntialias = true,
		};
		canvas.DrawText(text, x, y, labelFont, labelPaint);
	}

	private static void DrawFooter(SKCanvas canvas, int width, int height, int gapCount, SKColor bg)
	{
		var isDark = bg.Red < 100 && bg.Green < 100 && bg.Blue < 100;
		using var footerFont = new SKFont { Size = 12 };
		using var footerPaint = new SKPaint
		{
			Color = isDark ? new SKColor(140, 140, 140) : new SKColor(130, 130, 130),
			IsAntialias = true,
		};
		var info = $"GetIntercepts() found {gapCount} descender crossing{(gapCount != 1 ? "s" : "")}";
		var tw = footerFont.MeasureText(info);
		canvas.DrawText(info, (width - tw) / 2f, height - 16, footerFont, footerPaint);
	}
}

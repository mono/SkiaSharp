using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class TextLabSample : CanvasSampleBase
{
	private int alignIndex = 1;
	private float textSize = 48f;
	private bool showBounds;
	private bool showMetrics;
	private bool stroke;
	private int textIndex;
	private int fontIndex; // 0 = Embedded, 1 = Default

	private SKTypeface? embeddedTypeface;

	private static readonly string[] AlignOptions = { "Left", "Center", "Right" };
	private static readonly string[] TextOptions = { "SkiaSharp", "Hello World!", "The Quick Brown Fox", "0123456789", "AaBbCcDd" };
	private static readonly string[] FontOptions = { "Embedded", "Default" };

	public override string Title => "Text Lab";

	public override string Description =>
		"Explore text rendering with font selection, alignment, size, and metric visualization.";

	public override string Category => SampleCategories.Text;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("text", "Text", TextOptions, textIndex),
		new PickerControl("font", "Font", FontOptions, fontIndex),
		new SliderControl("textSize", "Text Size", 12, 120, textSize),
		new PickerControl("align", "Alignment", AlignOptions, alignIndex),
		new ToggleControl("stroke", "Stroke Only", stroke),
		new ToggleControl("showBounds", "Show Bounds", showBounds),
		new ToggleControl("showMetrics", "Show Metrics", showMetrics),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "text": textIndex = (int)value; break;
			case "font": fontIndex = (int)value; break;
			case "align": alignIndex = (int)value; break;
			case "textSize": textSize = (float)value; break;
			case "showBounds": showBounds = (bool)value; break;
			case "showMetrics": showMetrics = (bool)value; break;
			case "stroke": stroke = (bool)value; break;
		}
	}

	private SKTypeface GetTypeface()
	{
		if (fontIndex == 0)
		{
			if (embeddedTypeface == null)
			{
				using var stream = SampleMedia.Fonts.EmbeddedFont;
				if (stream != null)
					embeddedTypeface = SKTypeface.FromStream(stream);
			}
			return embeddedTypeface ?? SKTypeface.Default;
		}
		return SKTypeface.Default;
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var text = TextOptions[textIndex];
		var textAlign = alignIndex switch
		{
			0 => SKTextAlign.Left,
			2 => SKTextAlign.Right,
			_ => SKTextAlign.Center,
		};

		// Anchor X depends on alignment
		var x = alignIndex switch
		{
			0 => 20f,
			2 => width - 20f,
			_ => width / 2f,
		};

		var typeface = GetTypeface();

		using var font = new SKFont(typeface, textSize);
		using var paint = new SKPaint
		{
			IsAntialias = true,
			Color = new SKColor(0xFF4281A4),
		};

		if (stroke)
		{
			paint.IsStroke = true;
			paint.StrokeWidth = 2;
		}

		// Center text vertically
		var metrics = font.Metrics;
		var y = height / 2f - (metrics.Ascent + metrics.Descent) / 2f;

		canvas.DrawText(text, x, y, textAlign, font, paint);

		if (showBounds)
		{
			var textWidth = font.MeasureText(text, out var bounds);

			// Position bounds at the actual draw location
			var offsetX = alignIndex switch
			{
				1 => x - textWidth / 2f,
				2 => x - textWidth,
				_ => x,
			};
			bounds.Offset(offsetX, y);

			using var boundsPaint = new SKPaint
			{
				IsStroke = true,
				StrokeWidth = 1,
				Color = SKColors.Red,
				IsAntialias = true,
			};
			canvas.DrawRect(bounds, boundsPaint);
		}

		if (showMetrics)
		{
			using var dashEffect = SKPathEffect.CreateDash(new[] { 6f, 4f }, 0);
			using var linePaint = new SKPaint
			{
				IsStroke = true,
				StrokeWidth = 1,
				IsAntialias = true,
				PathEffect = dashEffect,
			};

			// Baseline (blue)
			linePaint.Color = SKColors.Blue;
			canvas.DrawLine(0, y, width, y, linePaint);

			// Ascent line (green)
			linePaint.Color = SKColors.Green;
			canvas.DrawLine(0, y + metrics.Ascent, width, y + metrics.Ascent, linePaint);

			// Descent line (orange)
			linePaint.Color = SKColors.Orange;
			canvas.DrawLine(0, y + metrics.Descent, width, y + metrics.Descent, linePaint);

			// Leading line (purple) — top of next line
			if (metrics.Leading != 0)
			{
				linePaint.Color = SKColors.Purple;
				canvas.DrawLine(0, y + metrics.Descent + metrics.Leading,
					width, y + metrics.Descent + metrics.Leading, linePaint);
			}

			// Labels
			using var labelFont = new SKFont(SampleMedia.Fonts.Default, 12);
			using var labelPaint = new SKPaint
			{
				IsAntialias = true,
			};
			labelPaint.Color = SKColors.Blue;
			canvas.DrawText("baseline", 4, y - 4, labelFont, labelPaint);
			labelPaint.Color = SKColors.Green;
			canvas.DrawText("ascent", 4, y + metrics.Ascent - 4, labelFont, labelPaint);
			labelPaint.Color = SKColors.Orange;
			canvas.DrawText("descent", 4, y + metrics.Descent - 4, labelFont, labelPaint);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		embeddedTypeface?.Dispose();
		embeddedTypeface = null;
	}
}

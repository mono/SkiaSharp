using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class TextLabSample : InteractiveSampleBase
{
	private int alignIndex = 1;
	private float textSize = 48f;
	private bool showBounds;
	private bool showMetrics;
	private bool stroke;
	private int textIndex;

	private static readonly string[] AlignOptions = { "Left", "Center", "Right" };
	private static readonly string[] TextOptions = { "SkiaSharp", "Hello World!", "The Quick Brown Fox", "0123456789", "AaBbCcDd" };

	public override string Title => "Text Lab";

	public override string Description =>
		"Explore text rendering with alignment, metrics, and measurement visualization.";

	public override string Category => SampleCategories.Text;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("text", "Text", TextOptions, textIndex),
		new PickerControl("align", "Alignment", AlignOptions, alignIndex),
		new SliderControl("textSize", "Text Size", 12, 120, textSize),
		new ToggleControl("showBounds", "Show Bounds", showBounds),
		new ToggleControl("showMetrics", "Show Metrics", showMetrics),
		new ToggleControl("stroke", "Stroke Only", stroke),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "text": textIndex = (int)value; break;
			case "align": alignIndex = (int)value; break;
			case "textSize": textSize = (float)value; break;
			case "showBounds": showBounds = (bool)value; break;
			case "showMetrics": showMetrics = (bool)value; break;
			case "stroke": stroke = (bool)value; break;
		}
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

		using var paint = new SKPaint
		{
			TextSize = textSize,
			IsAntialias = true,
			Color = new SKColor(0xFF4281A4),
			TextAlign = textAlign,
		};

		if (stroke)
		{
			paint.IsStroke = true;
			paint.StrokeWidth = 2;
		}

		// Center text vertically
		var metrics = paint.FontMetrics;
		var y = height / 2f - (metrics.Ascent + metrics.Descent) / 2f;

		canvas.DrawText(text, x, y, paint);

		if (showBounds)
		{
			var bounds = new SKRect();
			var textWidth = paint.MeasureText(text, ref bounds);

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
			using var linePaint = new SKPaint
			{
				IsStroke = true,
				StrokeWidth = 1,
				IsAntialias = true,
				PathEffect = SKPathEffect.CreateDash(new[] { 6f, 4f }, 0),
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
			using var labelPaint = new SKPaint
			{
				TextSize = 12,
				IsAntialias = true,
			};
			labelPaint.Color = SKColors.Blue;
			canvas.DrawText("baseline", 4, y - 4, labelPaint);
			labelPaint.Color = SKColors.Green;
			canvas.DrawText("ascent", 4, y + metrics.Ascent - 4, labelPaint);
			labelPaint.Color = SKColors.Orange;
			canvas.DrawText("descent", 4, y + metrics.Descent - 4, labelPaint);
		}
	}
}

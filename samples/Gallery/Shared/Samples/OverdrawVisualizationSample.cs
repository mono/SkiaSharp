using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class OverdrawVisualizationSample : CanvasSampleBase
{
	private bool visualizeOverdraw = true;

	public override string Title => "Overdraw Visualization";

	public override DateOnly? DateAdded => new DateOnly(2026, 6, 23);

	public override string Description =>
		"Visualize overdraw (how many times each pixel is drawn) using a color-coded heat map. " +
		"Useful for identifying rendering performance bottlenecks.";

	public override IReadOnlyList<string> ApiTags =>
	[
		"SKColorFilter", "SKColorFilter.CreateOverdraw",
		"SKCanvas", "SKCanvas.DrawRect", "SKCanvas.DrawCircle",
		"SKPaint", "SKColor",
	];

	public override string Category => SampleManager.ImageFilters;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SwitchControl("visualize", "Show Overdraw", visualizeOverdraw,
			Description: "Toggle between normal rendering and overdraw visualization."),
	];

	public override void HandleControlValueChanged(string id, object value)
	{
		switch (id)
		{
			case "visualize":
				visualizeOverdraw = (bool)value;
				Refresh();
				break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		// Standard overdraw color palette - heat map from transparent to red
		var overdrawColors = new SKColor[]
		{
			new SKColor(0x00000000), // 0 draws - transparent
			new SKColor(0x5A3F0099), // 1 draw  - blue
			new SKColor(0x5A2D8F0F), // 2 draws - green
			new SKColor(0x5ABFBF00), // 3 draws - yellow
			new SKColor(0x5ABF6600), // 4 draws - orange
			new SKColor(0x5AFF0000)  // 5+ draws - red
		};

		using var paint = new SKPaint
		{
			IsAntialias = true
		};

		if (visualizeOverdraw)
		{
			// Apply overdraw visualization filter
			using var overdrawFilter = SKColorFilter.CreateOverdraw(overdrawColors);
			paint.ColorFilter = overdrawFilter;
		}
		else
		{
			// Normal rendering - use a visible color
			paint.Color = new SKColor(0xFF4285F4);
		}

		// Draw overlapping shapes to demonstrate overdraw
		var centerX = width / 2f;
		var centerY = height / 2f;
		var size = Math.Min(width, height) * 0.6f;

		// Draw multiple overlapping rectangles
		for (int i = 0; i < 8; i++)
		{
			var offset = i * size / 16f;
			var rect = new SKRect(
				centerX - size / 2 + offset,
				centerY - size / 2 + offset,
				centerX + size / 2 - offset,
				centerY + size / 2 - offset
			);
			canvas.DrawRect(rect, paint);
		}

		// Draw overlapping circles
		var circleRadius = size / 6f;
		var positions = new (float x, float y)[]
		{
			(centerX - size / 4, centerY - size / 4),
			(centerX + size / 4, centerY - size / 4),
			(centerX - size / 4, centerY + size / 4),
			(centerX + size / 4, centerY + size / 4),
			(centerX, centerY),
		};

		foreach (var (x, y) in positions)
		{
			canvas.DrawCircle(x, y, circleRadius, paint);
		}

		// Draw legend if visualizing overdraw
		if (visualizeOverdraw)
		{
			DrawLegend(canvas, width, height, overdrawColors);
		}
	}

	private void DrawLegend(SKCanvas canvas, int width, int height, SKColor[] colors)
	{
		var padding = 10;
		var boxSize = 20;
		var spacing = 5;
		var x = padding;
		var y = height - padding - boxSize;

		using var textPaint = new SKPaint
		{
			IsAntialias = true,
			Color = SKColors.Black,
			TextSize = 14
		};

		using var boxPaint = new SKPaint
		{
			IsAntialias = true
		};

		var labels = new[] { "0x", "1x", "2x", "3x", "4x", "5+x" };
		
		for (int i = 0; i < colors.Length; i++)
		{
			boxPaint.Color = colors[i];
			canvas.DrawRect(x, y, boxSize, boxSize, boxPaint);
			canvas.DrawText(labels[i], x + boxSize + spacing, y + boxSize - 5, textPaint);
			x += boxSize + spacing + 30;
		}
	}
}

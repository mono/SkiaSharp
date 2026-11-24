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
		"Demonstrates overdraw visualization using SKOverdrawCanvas (tracks draw counts in alpha) " +
		"and SKColorFilter.CreateOverdraw() (converts alpha to color heat map).";

	public override IReadOnlyList<string> ApiTags =>
	[
		"SKColorFilter", "SKColorFilter.CreateOverdraw",
		"SKOverdrawCanvas",
		"SKCanvas", "SKCanvas.DrawRect", "SKCanvas.DrawCircle",
		"SKPaint", "SKColor",
	];

	public override string Category => SampleManager.ImageFilters;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new ToggleControl("visualize", "Side-by-Side", visualizeOverdraw,
			Description: "Show normal rendering vs overdraw visualization side-by-side."),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "visualize":
				visualizeOverdraw = (bool)value;
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
			IsAntialias = true,
			Color = new SKColor(0xFF4285F4) // Blue for normal rendering
		};

		if (visualizeOverdraw)
		{
			// Side-by-side comparison
			var halfWidth = width / 2;

			// Left side: Normal rendering
			canvas.Save();
			canvas.ClipRect(new SKRect(0, 0, halfWidth, height));
			DrawShapes(canvas, halfWidth, height, paint);
			canvas.Restore();

			// Right side: Overdraw visualization
			canvas.Save();
			canvas.Translate(halfWidth, 0);
			canvas.ClipRect(new SKRect(0, 0, halfWidth, height));
		
			// Create offscreen surface to capture overdraw counts in alpha channel
			// (Required: SKOverdrawCanvas writes to alpha, then we apply the color filter)
			var info = new SKImageInfo(halfWidth, height);
			using var overdrawSurface = SKSurface.Create(info);
			var offscreenCanvas = overdrawSurface.Canvas;
			offscreenCanvas.Clear(SKColors.Transparent);

			// Wrap in SKOverdrawCanvas to track draw counts
			using var overdrawCanvas = new SKOverdrawCanvas(offscreenCanvas);
			paint.Color = SKColors.White;
			DrawShapes(overdrawCanvas, halfWidth, height, paint);

			// Apply color filter to convert alpha counts to colors
			using var overdrawImage = overdrawSurface.Snapshot();
			using var overdrawFilter = SKColorFilter.CreateOverdraw(overdrawColors);
			using var filterPaint = new SKPaint { ColorFilter = overdrawFilter };
			canvas.DrawImage(overdrawImage, 0, 0, SKSamplingOptions.Default, filterPaint);
			canvas.Restore();

			// Draw divider
			using var dividerPaint = new SKPaint { Color = SKColors.Gray, StrokeWidth = 2 };
			canvas.DrawLine(halfWidth, 0, halfWidth, height, dividerPaint);

			// Draw labels
			DrawLabel(canvas, "Normal", halfWidth / 2, 20);
			DrawLabel(canvas, "Overdraw", halfWidth + halfWidth / 2, 20);

			// Draw legend at bottom
			DrawLegend(canvas, width, height, overdrawColors);
		}
		else
		{
			// Single view: just show normal rendering
			DrawShapes(canvas, width, height, paint);
		}
	}

	private void DrawShapes(SKCanvas canvas, int width, int height, SKPaint paint)
	{
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
	}

	private void DrawLabel(SKCanvas canvas, string text, float x, float y)
	{
		using var font = new SKFont(SampleMedia.Fonts.Default, 16) { Embolden = true };
		using var textPaint = new SKPaint
		{
			IsAntialias = true,
			Color = SKColors.Black
		};
		canvas.DrawText(text, x, y, SKTextAlign.Center, font, textPaint);
	}

	private void DrawLegend(SKCanvas canvas, int width, int height, SKColor[] colors)
	{
		var padding = 10;
		var boxSize = 20;
		var spacing = 5;
		var x = padding;
		var y = height - padding - boxSize;

		using var font = new SKFont(SampleMedia.Fonts.Default, 14);
		using var textPaint = new SKPaint
		{
			IsAntialias = true,
			Color = SKColors.Black
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
			canvas.DrawText(labels[i], x + boxSize + spacing, y + boxSize - 5, SKTextAlign.Left, font, textPaint);
			x += boxSize + spacing + 30;
		}
	}
}

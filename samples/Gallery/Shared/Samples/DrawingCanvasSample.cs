using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class DrawingCanvasSample : InteractiveSampleBase
{
	private float brushSize = 5f;
	private int colorIndex;
	private bool eraser;

	private SKPoint panStart;
	private SKPoint lastPoint;
	private SKPath? currentPath;
	private readonly List<SKPath> paths = new();
	private readonly List<SKColor> colors = new();
	private readonly List<float> sizes = new();

	private static readonly string[] ColorNames = { "Black", "Red", "Blue", "Green", "Orange", "Purple" };
	private static readonly SKColor[] ColorValues =
	{
		SKColors.Black, SKColors.Red, SKColors.Blue,
		SKColors.Green, SKColors.Orange, SKColors.Purple,
	};

	public override string Title => "Drawing Canvas";

	public override string Description =>
		"Freehand drawing with customizable brush size and color.";

	public override string Category => SampleCategories.Showcases;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("brushSize", "Brush Size", 1, 50, brushSize),
		new PickerControl("color", "Color", ColorNames, colorIndex),
		new ToggleControl("eraser", "Eraser Mode", eraser),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "brushSize": brushSize = (float)value; break;
			case "color": colorIndex = (int)value; break;
			case "eraser": eraser = (bool)value; break;
		}
	}

	public override void Pan(GestureState state, SKPoint translation)
	{
		// Do NOT call base.Pan — this sample draws in screen coordinates and
		// must not apply the matrix-based pan that SampleBase provides.
		// Blazor dispatches: Started = absolute start position,
		// Running = cumulative delta from start (current - panStartPoint).
		if (!IsInitialized)
			return;

		switch (state)
		{
			case GestureState.Started:
				panStart = translation;
				lastPoint = panStart;
				currentPath = new SKPath();
				currentPath.MoveTo(panStart);
				paths.Add(currentPath);
				colors.Add(eraser ? SKColors.White : ColorValues[colorIndex]);
				sizes.Add(brushSize);
				Refresh();
				break;

			case GestureState.Running:
				if (currentPath != null)
				{
					var point = new SKPoint(panStart.X + translation.X, panStart.Y + translation.Y);
					if (SKPoint.Distance(point, lastPoint) > 0.5f)
					{
						currentPath.LineTo(point);
						lastPoint = point;
						Refresh();
					}
				}
				break;

			case GestureState.Completed:
			case GestureState.Canceled:
				currentPath = null;
				Refresh();
				break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		// Reset matrix so strokes draw in screen coordinates
		canvas.ResetMatrix();
		canvas.Clear(SKColors.White);

		for (int i = 0; i < paths.Count; i++)
		{
			using var paint = new SKPaint
			{
				Color = colors[i],
				StrokeWidth = sizes[i],
				IsStroke = true,
				StrokeCap = SKStrokeCap.Round,
				StrokeJoin = SKStrokeJoin.Round,
				IsAntialias = true,
			};
			canvas.DrawPath(paths[i], paint);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		foreach (var path in paths)
			path.Dispose();
		paths.Clear();
		colors.Clear();
		sizes.Clear();
		currentPath = null;
	}
}

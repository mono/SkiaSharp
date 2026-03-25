using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class PathBuilderSample : InteractiveSampleBase
{
	private int shapeIndex;
	private bool showBounds;
	private bool showTightBounds;
	private int fillIndex;

	private static readonly string[] ShapeNames = { "Star", "Bezier Curve", "Spiral" };
	private static readonly string[] FillNames = { "Winding", "EvenOdd" };

	public override string Title => "Path Builder";

	public override string Category => SampleCategories.Paths;

	public override string Description => "Build geometric paths and visualize bounds and fill rules.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("shape", "Shape", ShapeNames, shapeIndex),
		new ToggleControl("showBounds", "Show Bounds", showBounds),
		new ToggleControl("showTightBounds", "Show Tight Bounds", showTightBounds),
		new PickerControl("fill", "Fill Rule", FillNames, fillIndex),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "shape":
				shapeIndex = (int)value;
				break;
			case "showBounds":
				showBounds = (bool)value;
				break;
			case "showTightBounds":
				showTightBounds = (bool)value;
				break;
			case "fill":
				fillIndex = (int)value;
				break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var cx = width / 2f;
		var cy = height / 2f;
		var radius = Math.Min(width, height) * 0.35f;

		using var path = shapeIndex switch
		{
			1 => CreateBezierCurve(cx, cy, radius),
			2 => CreateSpiral(cx, cy, radius, 7),
			_ => CreateStar(cx, cy, radius, 0.4f, 7),
		};

		path.FillType = fillIndex == 1 ? SKPathFillType.EvenOdd : SKPathFillType.Winding;

		using var fillPaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Fill,
			Color = new SKColor(59, 130, 246, 100),
		};
		canvas.DrawPath(path, fillPaint);

		using var strokePaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			Color = new SKColor(59, 130, 246),
			StrokeWidth = 2,
		};
		canvas.DrawPath(path, strokePaint);

		if (showBounds && path.GetBounds(out var bounds))
		{
			using var boundsPaint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				Color = SKColors.Green,
				StrokeWidth = 1,
				PathEffect = SKPathEffect.CreateDash(new float[] { 6, 4 }, 0),
			};
			canvas.DrawRect(bounds, boundsPaint);
		}


		if (showTightBounds && path.GetTightBounds(out var tight))
		{
			using var tightPaint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				Color = SKColors.Red,
				StrokeWidth = 1,
				PathEffect = SKPathEffect.CreateDash(new float[] { 6, 4 }, 0),
			};
			canvas.DrawRect(tight, tightPaint);
		}
	}

	private static SKPath CreateStar(float cx, float cy, float outerR, float innerRatio, int n)
	{
		var path = new SKPath();
		var innerR = outerR * innerRatio;
		var totalPoints = n * 2;

		for (var i = 0; i < totalPoints; i++)
		{
			var r = i % 2 == 0 ? outerR : innerR;
			var angle = (float)(Math.PI * 2 * i / totalPoints - Math.PI / 2);
			var x = cx + r * MathF.Cos(angle);
			var y = cy + r * MathF.Sin(angle);

			if (i == 0)
				path.MoveTo(x, y);
			else
				path.LineTo(x, y);
		}

		path.Close();
		return path;
	}

	private static SKPath CreateBezierCurve(float cx, float cy, float radius)
	{
		// A shape where control points extend well beyond the path,
		// creating a clear difference between bounds and tight bounds.
		var path = new SKPath();
		var r = radius * 0.6f;
		var cpExtend = radius * 1.4f;

		// Start at top
		path.MoveTo(cx, cy - r);

		// Right curve with control points extending far right
		path.CubicTo(
			cx + cpExtend, cy - cpExtend,
			cx + cpExtend, cy + cpExtend,
			cx, cy + r);

		// Left curve with control points extending far left
		path.CubicTo(
			cx - cpExtend, cy + cpExtend,
			cx - cpExtend, cy - cpExtend,
			cx, cy - r);

		path.Close();
		return path;
	}

	private static SKPath CreateSpiral(float cx, float cy, float maxRadius, int turns)
	{
		var path = new SKPath();
		var steps = turns * 60;

		for (var i = 0; i <= steps; i++)
		{
			var t = (float)i / steps;
			var angle = t * turns * MathF.PI * 2;
			var r = t * maxRadius;
			var x = cx + r * MathF.Cos(angle);
			var y = cy + r * MathF.Sin(angle);

			if (i == 0)
				path.MoveTo(x, y);
			else
				path.LineTo(x, y);
		}

		return path;
	}
}

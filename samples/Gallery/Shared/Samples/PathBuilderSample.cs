using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class PathBuilderSample : InteractiveSampleBase
{
	private int shapeIndex;
	private float points = 7f;
	private float innerRadius = 0.4f;
	private bool showBounds;
	private bool showTightBounds;
	private int fillIndex;

	private static readonly string[] ShapeNames = { "Star", "Polygon", "Spiral" };
	private static readonly string[] FillNames = { "Winding", "EvenOdd" };

	public override string Title => "Path Builder";

	public override string Category => SampleCategories.Paths;

	public override string Description => "Build geometric paths and visualize bounds and fill rules.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("shape", "Shape", ShapeNames, shapeIndex),
		new SliderControl("points", "Points", 3, 12, points, 1),
		new SliderControl("innerRadius", "Inner Radius", 0.1f, 0.9f, innerRadius, 0.05f),
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
			case "points":
				points = (float)value;
				break;
			case "innerRadius":
				innerRadius = (float)value;
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
		var n = (int)points;

		using var path = shapeIndex switch
		{
			1 => CreatePolygon(cx, cy, radius, n),
			2 => CreateSpiral(cx, cy, radius, n),
			_ => CreateStar(cx, cy, radius, innerRadius, n),
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

	private static SKPath CreatePolygon(float cx, float cy, float radius, int n)
	{
		var path = new SKPath();

		for (var i = 0; i < n; i++)
		{
			var angle = (float)(Math.PI * 2 * i / n - Math.PI / 2);
			var x = cx + radius * MathF.Cos(angle);
			var y = cy + radius * MathF.Sin(angle);

			if (i == 0)
				path.MoveTo(x, y);
			else
				path.LineTo(x, y);
		}

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

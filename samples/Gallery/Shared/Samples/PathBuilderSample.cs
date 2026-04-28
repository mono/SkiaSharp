using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class PathBuilderSample : CanvasSampleBase
{
	private int shapeIndex;
	private bool showBounds;
	private bool showTightBounds;
	private int fillIndex;
	private bool showControlPoints = true;

	private static readonly string[] ShapeNames = { "Star", "Bezier Curve", "Spiral" };
	private static readonly string[] FillNames = { "Winding", "EvenOdd" };

	public override string Title => "Path Builder";

	public override string Category => SampleCategories.Paths;

	public override string Description => "Build star, Bézier, and spiral paths with bounds visualization and fill rule controls.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("shape", "Shape", ShapeNames, shapeIndex),
		new PickerControl("fill", "Fill Rule", FillNames, fillIndex),
		new ToggleControl("showBounds", "Show Bounds", showBounds),
		new ToggleControl("showTightBounds", "Show Tight Bounds", showTightBounds),
		new ToggleControl("showControlPoints", "Show Control Points", showControlPoints),
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
			case "showControlPoints":
				showControlPoints = (bool)value;
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

		// Draw control points and connectors for bezier curve
		if (showControlPoints && shapeIndex == 1)
		{
			DrawBezierControlPoints(canvas, cx, cy, radius);
		}

		if (showBounds && path.GetBounds(out var bounds))
		{
			using var dashEffect = SKPathEffect.CreateDash(new float[] { 6, 4 }, 0);
			using var boundsPaint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				Color = SKColors.Green,
				StrokeWidth = 1,
				PathEffect = dashEffect,
			};
			canvas.DrawRect(bounds, boundsPaint);
		}


		if (showTightBounds && path.GetTightBounds(out var tight))
		{
			using var tightDash = SKPathEffect.CreateDash(new float[] { 6, 4 }, 0);
			using var tightPaint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				Color = SKColors.Red,
				StrokeWidth = 1,
				PathEffect = tightDash,
			};
			canvas.DrawRect(tight, tightPaint);
		}
	}

	private static void DrawBezierControlPoints(SKCanvas canvas, float cx, float cy, float radius)
	{
		var r = radius * 0.6f;
		var cpExtend = radius * 1.4f;

		// On-curve points (anchor points)
		var p0 = new SKPoint(cx, cy - r);  // top
		var p3 = new SKPoint(cx, cy + r);  // bottom

		// Control points for right curve
		var cp1 = new SKPoint(cx + cpExtend, cy - cpExtend);
		var cp2 = new SKPoint(cx + cpExtend, cy + cpExtend);

		// Control points for left curve
		var cp3 = new SKPoint(cx - cpExtend, cy + cpExtend);
		var cp4 = new SKPoint(cx - cpExtend, cy - cpExtend);

		// Draw connector lines (control point handles)
		using var dashEffect = SKPathEffect.CreateDash(new float[] { 4, 3 }, 0);
		using var linePaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			Color = new SKColor(255, 100, 100, 150),
			StrokeWidth = 1,
			PathEffect = dashEffect,
		};
		// Right curve handles
		canvas.DrawLine(p0, cp1, linePaint);
		canvas.DrawLine(cp2, p3, linePaint);
		// Left curve handles
		canvas.DrawLine(p3, cp3, linePaint);
		canvas.DrawLine(cp4, p0, linePaint);

		// Draw control points (hollow circles)
		using var cpPaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			Color = new SKColor(255, 80, 80),
			StrokeWidth = 2,
		};
		float cpRadius = 5;
		canvas.DrawCircle(cp1, cpRadius, cpPaint);
		canvas.DrawCircle(cp2, cpRadius, cpPaint);
		canvas.DrawCircle(cp3, cpRadius, cpPaint);
		canvas.DrawCircle(cp4, cpRadius, cpPaint);

		// Draw anchor points (filled circles)
		using var anchorPaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Fill,
			Color = new SKColor(59, 130, 246),
		};
		float anchorRadius = 6;
		canvas.DrawCircle(p0, anchorRadius, anchorPaint);
		canvas.DrawCircle(p3, anchorRadius, anchorPaint);

		// Labels
		using var labelFont = new SKFont(SampleMedia.Fonts.Default, 11);
		using var labelPaint = new SKPaint
		{
			IsAntialias = true,
			Color = new SKColor(120, 120, 120),
		};
		canvas.DrawText("CP1", cp1.X + 8, cp1.Y - 4, SKTextAlign.Left, labelFont, labelPaint);
		canvas.DrawText("CP2", cp2.X + 8, cp2.Y + 12, SKTextAlign.Left, labelFont, labelPaint);
		canvas.DrawText("CP3", cp3.X - 8, cp3.Y + 12, SKTextAlign.Right, labelFont, labelPaint);
		canvas.DrawText("CP4", cp4.X - 8, cp4.Y - 4, SKTextAlign.Right, labelFont, labelPaint);
		canvas.DrawText("P0", p0.X + 10, p0.Y, SKTextAlign.Left, labelFont, labelPaint);
		canvas.DrawText("P1", p3.X + 10, p3.Y, SKTextAlign.Left, labelFont, labelPaint);
	}

	private static SKPath CreateStar(float cx, float cy, float outerR, float innerRatio, int n)
	{
		using var builder = new SKPathBuilder();
		var innerR = outerR * innerRatio;
		var totalPoints = n * 2;

		for (var i = 0; i < totalPoints; i++)
		{
			var r = i % 2 == 0 ? outerR : innerR;
			var angle = (float)(Math.PI * 2 * i / totalPoints - Math.PI / 2);
			var x = cx + r * MathF.Cos(angle);
			var y = cy + r * MathF.Sin(angle);

			if (i == 0)
				builder.MoveTo(x, y);
			else
				builder.LineTo(x, y);
		}

		builder.Close();
		return builder.Detach();
	}

	private static SKPath CreateBezierCurve(float cx, float cy, float radius)
	{
		// A shape where control points extend well beyond the path,
		// creating a clear difference between bounds and tight bounds.
		using var builder = new SKPathBuilder();
		var r = radius * 0.6f;
		var cpExtend = radius * 1.4f;

		// Start at top
		builder.MoveTo(cx, cy - r);

		// Right curve with control points extending far right
		builder.CubicTo(
			cx + cpExtend, cy - cpExtend,
			cx + cpExtend, cy + cpExtend,
			cx, cy + r);

		// Left curve with control points extending far left
		builder.CubicTo(
			cx - cpExtend, cy + cpExtend,
			cx - cpExtend, cy - cpExtend,
			cx, cy - r);

		builder.Close();
		return builder.Detach();
	}

	private static SKPath CreateSpiral(float cx, float cy, float maxRadius, int turns)
	{
		using var builder = new SKPathBuilder();
		var steps = turns * 60;

		for (var i = 0; i <= steps; i++)
		{
			var t = (float)i / steps;
			var angle = t * turns * MathF.PI * 2;
			var r = t * maxRadius;
			var x = cx + r * MathF.Cos(angle);
			var y = cy + r * MathF.Sin(angle);

			if (i == 0)
				builder.MoveTo(x, y);
			else
				builder.LineTo(x, y);
		}

		return builder.Detach();
	}
}

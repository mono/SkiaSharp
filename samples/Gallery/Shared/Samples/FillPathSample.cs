using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class FillPathSample : CanvasSampleBase
{
	private int shapeType;
	private float strokeWidth = 8f;
	private int capType;
	private int joinType;
	private float resScale = 1f;
	private bool showOriginal = true;

	private static readonly string[] ShapeTypes = { "Star", "Rounded Rect", "Circle", "Spiral", "Arrow" };
	private static readonly string[] CapTypes = { "Butt", "Round", "Square" };
	private static readonly string[] JoinTypes = { "Miter", "Round", "Bevel" };

	public override string Title => "Fill Path";

	public override DateOnly? DateAdded => new DateOnly(2026, 4, 27);

	public override string Description =>
	"Compute the filled outline of a stroked path using SKPaint.GetFillPath, with adjustable stroke parameters.";

	public override string Category => SampleManager.Paths;

	public override IReadOnlyList<SampleControl> Controls =>
	[
	new PickerControl("shape", "Shape", ShapeTypes, shapeType),
new SliderControl("strokeWidth", "Stroke Width", 1, 40, strokeWidth),
new PickerControl("cap", "Stroke Cap", CapTypes, capType),
new PickerControl("join", "Stroke Join", JoinTypes, joinType),
new SliderControl("resScale", "Resolution Scale", 0.25f, 4f, resScale, 0.25f),
new ToggleControl("showOriginal", "Show Original Path", showOriginal),
];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "shape":
				shapeType = (int)value;
				break;
			case "strokeWidth":
				strokeWidth = (float)value;
				break;
			case "cap":
				capType = (int)value;
				break;
			case "join":
				joinType = (int)value;
				break;
			case "resScale":
				resScale = (float)value;
				break;
			case "showOriginal":
				showOriginal = (bool)value;
				break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var cx = width / 2f;
		var cy = height / 2f;
		var size = Math.Min(width, height) * 0.3f;

		using var srcPath = CreateShape(shapeType, cx, cy, size);

		using var paint = new SKPaint
		{
			IsStroke = true,
			StrokeWidth = strokeWidth,
			StrokeCap = (SKStrokeCap)capType,
			StrokeJoin = (SKStrokeJoin)joinType,
			IsAntialias = true,
		};

		var cullRect = new SKRect(0, 0, width, height);
		using var fillPath = paint.GetFillPath(srcPath, cullRect, resScale);

		if (fillPath != null)
		{
			using var fillPaint = new SKPaint
			{
				Color = SKColors.CornflowerBlue.WithAlpha(100),
				IsAntialias = true,
			};
			canvas.DrawPath(fillPath, fillPaint);

			using var outlinePaint = new SKPaint
			{
				Color = SKColors.CornflowerBlue,
				IsStroke = true,
				StrokeWidth = 1,
				IsAntialias = true,
			};
			canvas.DrawPath(fillPath, outlinePaint);
		}

		if (showOriginal)
		{
			using var originalPaint = new SKPaint
			{
				Color = SKColors.Red,
				IsStroke = true,
				StrokeWidth = 2,
				IsAntialias = true,
			};
			canvas.DrawPath(srcPath, originalPaint);
		}
	}

	private static SKPath CreateShape(int type, float cx, float cy, float size)
	{
		return type switch
		{
			0 => CreateStarPath(cx, cy, size, size * 0.45f, 5),
			1 => CreateRoundedRectPath(cx, cy, size),
			2 => CreateCirclePath(cx, cy, size),
			3 => CreateSpiralPath(cx, cy, size),
			4 => CreateArrowPath(cx, cy, size),
			_ => CreateStarPath(cx, cy, size, size * 0.45f, 5),
		};
	}

	private static SKPath CreateRoundedRectPath(float cx, float cy, float size)
	{
		using var builder = new SKPathBuilder();
		var rect = new SKRect(cx - size, cy - size * 0.6f, cx + size, cy + size * 0.6f);
		var rrect = new SKRoundRect(rect, size * 0.2f);
		builder.AddRoundRect(rrect);
		return builder.Detach();
	}

	private static SKPath CreateCirclePath(float cx, float cy, float size)
	{
		using var builder = new SKPathBuilder();
		builder.AddCircle(cx, cy, size);
		return builder.Detach();
	}

	private static SKPath CreateSpiralPath(float cx, float cy, float size)
	{
		using var builder = new SKPathBuilder();
		var turns = 4;
		var pointsPerTurn = 60;
		var totalPoints = turns * pointsPerTurn;

		for (var i = 0; i < totalPoints; i++)
		{
			var t = (float)i / totalPoints;
			var angle = (float)(t * turns * 2 * Math.PI);
			var radius = size * t;
			var x = cx + radius * (float)Math.Cos(angle);
			var y = cy + radius * (float)Math.Sin(angle);

			if (i == 0)
				builder.MoveTo(x, y);
			else
				builder.LineTo(x, y);
		}

		return builder.Detach();
	}

	private static SKPath CreateArrowPath(float cx, float cy, float size)
	{
		using var builder = new SKPathBuilder();
		var shaftWidth = size * 0.3f;
		var headWidth = size * 0.8f;

		builder.MoveTo(cx - size, cy + shaftWidth / 2);
		builder.LineTo(cx - size, cy - shaftWidth / 2);
		builder.LineTo(cx, cy - shaftWidth / 2);
		builder.LineTo(cx, cy - headWidth / 2);
		builder.LineTo(cx + size * 0.7f, cy);
		builder.LineTo(cx, cy + headWidth / 2);
		builder.LineTo(cx, cy + shaftWidth / 2);
		builder.Close();

		return builder.Detach();
	}

	private static SKPath CreateStarPath(float cx, float cy, float outerRadius, float innerRadius, int points)
	{
		using var builder = new SKPathBuilder();
		var totalPoints = points * 2;
		for (var i = 0; i < totalPoints; i++)
		{
			var angle = (float)(i * Math.PI / points - Math.PI / 2);
			var radius = i % 2 == 0 ? outerRadius : innerRadius;
			var x = cx + radius * (float)Math.Cos(angle);
			var y = cy + radius * (float)Math.Sin(angle);

			if (i == 0)
				builder.MoveTo(x, y);
			else
				builder.LineTo(x, y);
		}
		builder.Close();
		return builder.Detach();
	}
}

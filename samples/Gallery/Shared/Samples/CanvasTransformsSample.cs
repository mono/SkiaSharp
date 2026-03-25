using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class CanvasTransformsSample : InteractiveSampleBase
{
	private float translateX;
	private float translateY;
	private float rotate;
	private float scaleX = 1f;
	private float scaleY = 1f;
	private bool showGrid = true;

	public override string Title => "Canvas Transforms";

	public override string Category => SampleCategories.General;

	public override string Description => "Visualize 2D canvas transformations: translate, rotate, and scale.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("translateX", "Translate X", -200, 200, translateX),
		new SliderControl("translateY", "Translate Y", -200, 200, translateY),
		new SliderControl("rotate", "Rotate", -180, 180, rotate),
		new SliderControl("scaleX", "Scale X", 0.1f, 3, scaleX, 0.1f),
		new SliderControl("scaleY", "Scale Y", 0.1f, 3, scaleY, 0.1f),
		new ToggleControl("showGrid", "Show Grid", showGrid),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "translateX":
				translateX = (float)value;
				break;
			case "translateY":
				translateY = (float)value;
				break;
			case "rotate":
				rotate = (float)value;
				break;
			case "scaleX":
				scaleX = (float)value;
				break;
			case "scaleY":
				scaleY = (float)value;
				break;
			case "showGrid":
				showGrid = (bool)value;
				break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var cx = width / 2f;
		var cy = height / 2f;

		if (showGrid)
			DrawGrid(canvas, width, height);

		canvas.Save();

		// Apply transforms around center
		canvas.Translate(cx + translateX, cy + translateY);
		canvas.RotateDegrees(rotate);
		canvas.Scale(scaleX, scaleY);

		// Draw a colored rounded rectangle centered at origin
		var rectSize = Math.Min(width, height) * 0.25f;
		var rect = new SKRect(-rectSize, -rectSize, rectSize, rectSize);

		using var fillPaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Fill,
			Color = new SKColor(59, 130, 246, 180),
		};
		canvas.DrawRoundRect(rect, 15, 15, fillPaint);

		using var strokePaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			Color = new SKColor(30, 64, 175),
			StrokeWidth = 2,
		};
		canvas.DrawRoundRect(rect, 15, 15, strokePaint);

		// Draw axes on the rectangle
		using var axisPaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Red,
			StrokeWidth = 2,
		};
		canvas.DrawLine(-rectSize * 1.2f, 0, rectSize * 1.2f, 0, axisPaint);
		axisPaint.Color = SKColors.Green;
		canvas.DrawLine(0, -rectSize * 1.2f, 0, rectSize * 1.2f, axisPaint);

		canvas.Restore();

		// Draw matrix values in the top-left corner
		DrawMatrixInfo(canvas);
	}

	private static void DrawGrid(SKCanvas canvas, int width, int height)
	{
		using var gridPaint = new SKPaint
		{
			IsAntialias = false,
			Style = SKPaintStyle.Stroke,
			Color = new SKColor(200, 200, 200),
			StrokeWidth = 1,
		};

		var spacing = 40f;
		for (var x = spacing; x < width; x += spacing)
			canvas.DrawLine(x, 0, x, height, gridPaint);
		for (var y = spacing; y < height; y += spacing)
			canvas.DrawLine(0, y, width, y, gridPaint);

		// Draw center crosshair
		gridPaint.Color = new SKColor(150, 150, 150);
		gridPaint.StrokeWidth = 1;
		gridPaint.PathEffect = SKPathEffect.CreateDash(new float[] { 4, 4 }, 0);
		canvas.DrawLine(width / 2f, 0, width / 2f, height, gridPaint);
		canvas.DrawLine(0, height / 2f, width, height / 2f, gridPaint);
	}

	private void DrawMatrixInfo(SKCanvas canvas)
	{
		using var bgPaint = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = new SKColor(0, 0, 0, 160),
		};
		canvas.DrawRect(5, 5, 190, 80, bgPaint);

		using var textPaint = new SKPaint
		{
			IsAntialias = true,
			Color = SKColors.White,
		};
		using var font = new SKFont(SKTypeface.Default, 12);

		canvas.DrawText($"Translate: ({translateX:F0}, {translateY:F0})", 10, 22, font, textPaint);
		canvas.DrawText($"Rotate: {rotate:F1}°", 10, 40, font, textPaint);
		canvas.DrawText($"Scale: ({scaleX:F1}, {scaleY:F1})", 10, 58, font, textPaint);
		canvas.DrawText($"Matrix: [{scaleX * MathF.Cos(rotate * MathF.PI / 180):F2}, ...]", 10, 76, font, textPaint);
	}
}

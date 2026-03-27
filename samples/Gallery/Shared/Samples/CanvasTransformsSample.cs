using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class CanvasTransformsSample : CanvasSampleBase
{
	private float translateX;
	private float translateY;
	private float rotate;
	private float scaleX = 1f;
	private float scaleY = 1f;
	private bool showGrid = true;

	public override string Title => "2D Transforms";

	public override string Category => SampleCategories.General;

	public override string Description => "Visualize 2D canvas transformations — translate, rotate, and scale with live matrix display.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("rotate", "Rotate", -180, 180, rotate),
		new SliderControl("scaleX", "Scale X", 0.1f, 3, scaleX, 0.1f),
		new SliderControl("scaleY", "Scale Y", 0.1f, 3, scaleY, 0.1f),
		new SliderControl("translateX", "Translate X", -200, 200, translateX),
		new SliderControl("translateY", "Translate Y", -200, 200, translateY),
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
		var rectSize = Math.Min(width, height) * 0.3f;
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
		using var dashEffect = SKPathEffect.CreateDash(new float[] { 4, 4 }, 0);
		gridPaint.PathEffect = dashEffect;
		canvas.DrawLine(width / 2f, 0, width / 2f, height, gridPaint);
		canvas.DrawLine(0, height / 2f, width, height / 2f, gridPaint);
	}

	private void DrawMatrixInfo(SKCanvas canvas)
	{
		// Build the combined transform matrix
		var rad = rotate * MathF.PI / 180f;
		var cos = MathF.Cos(rad);
		var sin = MathF.Sin(rad);

		// The actual matrix: Translate(cx+tx,cy+ty) * Rotate * Scale
		var m = SKMatrix.Identity;
		m.ScaleX = scaleX * cos;
		m.SkewX = -scaleY * sin;
		m.TransX = translateX + canvas.DeviceClipBounds.Width / 2f;
		m.SkewY = scaleX * sin;
		m.ScaleY = scaleY * cos;
		m.TransY = translateY + canvas.DeviceClipBounds.Height / 2f;

		var boxW = 230f;
		var boxH = 100f;

		using var bgPaint = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = new SKColor(0, 0, 0, 180),
		};
		canvas.DrawRoundRect(new SKRoundRect(new SKRect(5, 5, 5 + boxW, 5 + boxH), 6, 6), bgPaint);

		using var textPaint = new SKPaint
		{
			IsAntialias = true,
			Color = SKColors.White,
		};
		using var font = new SKFont(SKTypeface.Default, 14);
		using var headerFont = new SKFont(SKTypeface.Default, 13);

		canvas.DrawText("Transform Matrix:", 10, 22, headerFont, textPaint);

		var vals = new[]
		{
			m.ScaleX, m.SkewX, m.TransX,
			m.SkewY, m.ScaleY, m.TransY,
			m.Persp0, m.Persp1, m.Persp2,
		};

		var startY = 40f;
		var lineH = 20f;
		for (var row = 0; row < 3; row++)
		{
			var line = $"│ {vals[row * 3],8:F2}  {vals[row * 3 + 1],8:F2}  {vals[row * 3 + 2],8:F2} │";
			canvas.DrawText(line, 12, startY + row * lineH, font, textPaint);
		}
	}
}

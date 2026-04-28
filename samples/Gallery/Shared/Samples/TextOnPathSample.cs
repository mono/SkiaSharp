using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class TextOnPathSample : CanvasSampleBase
{
	private int pathShapeIndex;
	private float textOffset;
	private bool warpGlyphs = true;
	private float textSize = 36f;
	private int textIndex;

	private static readonly string[] PathShapes = { "Circle", "Wave", "Heart" };
	private static readonly string[] TextOptions = { "SkiaSharp", "Hello World!", "Paths are fun!" };

	public override string Title => "Text on Path";

	public override string Category => SampleCategories.Text;

	public override string Description =>
		"Draw text along circle, wave, and heart-shaped paths with adjustable offset and size.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("text", "Text", TextOptions, textIndex),
		new PickerControl("path", "Path Shape", PathShapes, pathShapeIndex),
		new SliderControl("textSize", "Text Size", 12, 60, textSize),
		new SliderControl("offset", "Offset", 0, 100, textOffset, 0.5f),
		new ToggleControl("warp", "Warp Glyphs", warpGlyphs),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "text":
				textIndex = (int)value;
				break;
			case "path":
				pathShapeIndex = (int)value;
				break;
			case "offset":
				textOffset = (float)value;
				break;
			case "warp":
				warpGlyphs = (bool)value;
				break;
			case "textSize":
				textSize = (float)value;
				break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.SkyBlue);

		var cx = width / 2f;
		var cy = height / 2f;
		var size = Math.Min(width, height) * 0.35f;
		var text = TextOptions[textIndex];

		using var path = pathShapeIndex switch
		{
			1 => CreateWavePath(cx, cy, size, width),
			2 => CreateHeartPath(cx, cy, size),
			_ => CreateCirclePath(cx, cy, size),
		};

		// Draw the path
		using var pathPaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Blue,
			StrokeWidth = 2,
		};
		canvas.DrawPath(path, pathPaint);

		// Compute offset from slider (0-100 maps to 0-pathLength)
		using var measure = new SKPathMeasure(path);
		var hOffset = textOffset / 100f * measure.Length;

		// Draw text on the path
		using var textPaint = new SKPaint
		{
			IsAntialias = true,
			Color = SKColors.Black,
			Style = SKPaintStyle.Fill,
		};
		using var font = new SKFont(SKTypeface.Default, textSize);

		canvas.DrawTextOnPath(text, path, new SKPoint(hOffset, 0), warpGlyphs, SKTextAlign.Left, font, textPaint);

		// Draw labels
		using var labelPaint = new SKPaint { IsAntialias = true, Color = SKColors.White };
		using var labelFont = new SKFont(SKTypeface.Default, 14);
		canvas.DrawText($"Shape: {PathShapes[pathShapeIndex]}  Warp: {(warpGlyphs ? "on" : "off")}  Offset: {textOffset:F2}", 10, 20, labelFont, labelPaint);
	}

	private static SKPath CreateCirclePath(float cx, float cy, float radius)
	{
		using var builder = new SKPathBuilder();
		builder.AddCircle(cx, cy, radius);
		return builder.Detach();
	}

	private static SKPath CreateWavePath(float cx, float cy, float amplitude, float width)
	{
		using var builder = new SKPathBuilder();
		var startX = cx - width * 0.4f;
		var endX = cx + width * 0.4f;
		builder.MoveTo(startX, cy);

		var segments = 4;
		var segWidth = (endX - startX) / segments;
		for (var i = 0; i < segments; i++)
		{
			var x0 = startX + i * segWidth;
			var x1 = x0 + segWidth;
			var sign = i % 2 == 0 ? -1f : 1f;
			builder.CubicTo(
				x0 + segWidth * 0.33f, cy + sign * amplitude,
				x0 + segWidth * 0.66f, cy + sign * amplitude,
				x1, cy);
		}

		return builder.Detach();
	}

	private static SKPath CreateHeartPath(float cx, float cy, float size)
	{
		using var builder = new SKPathBuilder();
		// Heart shape using cubic Bézier curves
		builder.MoveTo(cx, cy + size * 0.4f);
		builder.CubicTo(cx + size * 0.6f, cy - size * 0.1f,
						 cx + size * 0.9f, cy - size * 0.6f,
						 cx, cy - size * 0.3f);
		builder.CubicTo(cx - size * 0.9f, cy - size * 0.6f,
						 cx - size * 0.6f, cy - size * 0.1f,
						 cx, cy + size * 0.4f);
		builder.Close();
		return builder.Detach();
	}
}

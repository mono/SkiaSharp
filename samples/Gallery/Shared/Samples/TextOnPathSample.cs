using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class TextOnPathSample : AnimatedInteractiveSampleBase
{
	private float animationPhase;
	private int pathShapeIndex;
	private float textOffset;
	private bool warpGlyphs = true;
	private float textSize = 24f;

	private static readonly string[] PathShapes = { "Circle", "Wave", "Heart" };

	public override string Title => "Text on Path";

	public override string Category => SampleCategories.Text;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("path", "Path Shape", PathShapes, pathShapeIndex),
		new SliderControl("offset", "Text Offset", 0, 1, textOffset, 0.01f),
		new ToggleControl("warp", "Warp Glyphs", warpGlyphs),
		new SliderControl("textSize", "Text Size", 12, 60, textSize),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
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

	protected override async Task OnUpdate(CancellationToken token)
	{
		await Task.Delay(30, token);
		animationPhase += 0.005f;
		if (animationPhase > 1f)
			animationPhase -= 1f;
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.SkyBlue);

		var cx = width / 2f;
		var cy = height / 2f;
		var size = Math.Min(width, height) * 0.35f;
		var text = "The quick brown fox jumps over the lazy dog!";

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

		// Compute animated + manual offset
		using var measure = new SKPathMeasure(path);
		var totalOffset = (animationPhase + textOffset) * measure.Length;

		// Draw text on the path
		using var textPaint = new SKPaint
		{
			IsAntialias = true,
			Color = SKColors.Black,
			Style = SKPaintStyle.Fill,
		};
		using var font = new SKFont(SKTypeface.Default, textSize);

		canvas.DrawTextOnPath(text, path, new SKPoint(totalOffset, 0), warpGlyphs, SKTextAlign.Left, font, textPaint);

		// Draw labels
		using var labelPaint = new SKPaint { IsAntialias = true, Color = SKColors.White };
		using var labelFont = new SKFont(SKTypeface.Default, 14);
		canvas.DrawText($"Shape: {PathShapes[pathShapeIndex]}  Warp: {(warpGlyphs ? "on" : "off")}  Offset: {textOffset:F2}", 10, 20, labelFont, labelPaint);
	}

	private static SKPath CreateCirclePath(float cx, float cy, float radius)
	{
		var path = new SKPath();
		path.AddCircle(cx, cy, radius);
		return path;
	}

	private static SKPath CreateWavePath(float cx, float cy, float amplitude, float width)
	{
		var path = new SKPath();
		var startX = cx - width * 0.4f;
		var endX = cx + width * 0.4f;
		path.MoveTo(startX, cy);

		var segments = 4;
		var segWidth = (endX - startX) / segments;
		for (var i = 0; i < segments; i++)
		{
			var x0 = startX + i * segWidth;
			var x1 = x0 + segWidth;
			var sign = i % 2 == 0 ? -1f : 1f;
			path.CubicTo(
				x0 + segWidth * 0.33f, cy + sign * amplitude,
				x0 + segWidth * 0.66f, cy + sign * amplitude,
				x1, cy);
		}

		return path;
	}

	private static SKPath CreateHeartPath(float cx, float cy, float size)
	{
		var path = new SKPath();
		// Heart shape using cubic Bézier curves
		path.MoveTo(cx, cy + size * 0.4f);
		path.CubicTo(cx + size * 0.6f, cy - size * 0.1f,
					  cx + size * 0.9f, cy - size * 0.6f,
					  cx, cy - size * 0.3f);
		path.CubicTo(cx - size * 0.9f, cy - size * 0.6f,
					  cx - size * 0.6f, cy - size * 0.1f,
					  cx, cy + size * 0.4f);
		path.Close();
		return path;
	}
}

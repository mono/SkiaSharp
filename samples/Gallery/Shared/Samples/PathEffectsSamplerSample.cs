using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class PathEffectsSamplerSample : CanvasSampleBase
{
	private float param = 12f;
	private float strokeWidth = 3f;
	private int baseShapeIndex;

	private static readonly string[] BaseShapes = { "Star", "Rounded Rect", "Wave" };

	public override string Title => "Path Effects Sampler";

	public override DateOnly? DateAdded => new DateOnly(2026, 4, 27);

	public override string Description =>
		"All 6 path effects side by side on the same base path: corner, dash, discrete, 1D stamp, 2D tile, and compose.";

	public override string Category => SampleManager.Paths;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("shape", "Base Shape", BaseShapes, baseShapeIndex),
		new SliderControl("param", "Effect Parameter", 2f, 30f, param, 1f,
			Description: "Primary parameter controlling the effect intensity."),
		new SliderControl("strokeWidth", "Stroke Width", 1f, 8f, strokeWidth, 0.5f),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "shape": baseShapeIndex = (int)value; break;
			case "param": param = (float)value; break;
			case "strokeWidth": strokeWidth = (float)value; break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(new SKColor(0xFFF5F5F5));

		// Layout: 3 columns × 2 rows
		var cols = 3;
		var rows = 2;
		var cellW = width / (float)cols;
		var cellH = height / (float)rows;
		var margin = 16f;

		var effects = new (string Name, Func<float, float, SKPathEffect?> Create)[]
		{
			("Corner", (p, _) => SKPathEffect.CreateCorner(Math.Max(1, p))),
			("Dash", (p, _) => SKPathEffect.CreateDash(new[] { p, p / 2f }, 0)),
			("Discrete", (p, _) => SKPathEffect.CreateDiscrete(Math.Max(1, p), p * 0.3f)),
			("1D Stamp", (p, _) => Create1DStamp(p)),
			("2D Tile", (p, cw) => Create2DTile(p, cw)),
			("Compose", (p, _) => CreateCompose(p)),
		};

		for (var i = 0; i < effects.Length; i++)
		{
			var col = i % cols;
			var row = i / cols;
			var cx = col * cellW + cellW / 2f;
			var cy = row * cellH + cellH / 2f;
			var size = Math.Min(cellW, cellH) / 2f - margin;

			canvas.Save();
			canvas.ClipRect(new SKRect(col * cellW, row * cellH, (col + 1) * cellW, (row + 1) * cellH));

			// Draw cell background
			using (var bgPaint = new SKPaint { Color = SKColors.White })
			{
				var cellRect = SKRect.Inflate(
					new SKRect(col * cellW + 4, row * cellH + 4, (col + 1) * cellW - 4, (row + 1) * cellH - 4),
					0, 0);
				canvas.DrawRoundRect(cellRect, 8, 8, bgPaint);
			}

			// Draw label
			using (var labelFont = new SKFont { Size = 13 })
			using (var labelPaint = new SKPaint { Color = new SKColor(0xFF555555), IsAntialias = true })
			{
				var labelW = labelFont.MeasureText(effects[i].Name);
				canvas.DrawText(effects[i].Name, cx - labelW / 2f, row * cellH + 22, labelFont, labelPaint);
			}

			// Create the base path
			using var path = CreateBasePath(cx, cy + 8, size);

			// Create and apply path effect
			using var effect = effects[i].Create(param, cellW);

			using var paint = new SKPaint
			{
				IsStroke = true,
				StrokeWidth = strokeWidth,
				Color = new SKColor(0xFF3B82F6),
				IsAntialias = true,
				PathEffect = effect,
				StrokeCap = SKStrokeCap.Round,
				StrokeJoin = SKStrokeJoin.Round,
			};

			// For 2D effects, fill instead of stroke
			if (i == 4) // 2D Tile
			{
				paint.IsStroke = false;
				paint.Color = new SKColor(0xFF3B82F6);
			}

			canvas.DrawPath(path, paint);
			canvas.Restore();
		}

		// Draw grid lines
		using var gridPaint = new SKPaint
		{
			Color = new SKColor(0xFFDDDDDD),
			IsStroke = true,
			StrokeWidth = 1,
		};
		for (var c = 1; c < cols; c++)
			canvas.DrawLine(c * cellW, 0, c * cellW, height, gridPaint);
		canvas.DrawLine(0, cellH, width, cellH, gridPaint);
	}

	private SKPath CreateBasePath(float cx, float cy, float size)
	{
		return baseShapeIndex switch
		{
			1 => CreateRoundedRectPath(cx, cy, size),
			2 => CreateWavePath(cx, cy, size),
			_ => CreateStarPath(cx, cy, size),
		};
	}

	private static SKPath CreateStarPath(float cx, float cy, float size)
	{
		using var builder = new SKPathBuilder();
		var outerR = size;
		var innerR = size * 0.45f;
		var points = 5;
		var angle = -Math.PI / 2;
		var step = Math.PI / points;

		builder.MoveTo(
			cx + (float)(outerR * Math.Cos(angle)),
			cy + (float)(outerR * Math.Sin(angle)));

		for (var i = 1; i < 2 * points; i++)
		{
			angle += step;
			var r = (i % 2 == 0) ? outerR : innerR;
			builder.LineTo(
				cx + (float)(r * Math.Cos(angle)),
				cy + (float)(r * Math.Sin(angle)));
		}

		builder.Close();
		return builder.Detach();
	}

	private static SKPath CreateRoundedRectPath(float cx, float cy, float size)
	{
		using var builder = new SKPathBuilder();
		var rect = new SKRect(cx - size, cy - size * 0.7f, cx + size, cy + size * 0.7f);
		builder.AddRoundRect(rect, size * 0.2f, size * 0.2f);
		return builder.Detach();
	}

	private static SKPath CreateWavePath(float cx, float cy, float size)
	{
		using var builder = new SKPathBuilder();
		var startX = cx - size;
		builder.MoveTo(startX, cy);

		var segments = 4;
		var segW = (size * 2) / segments;
		for (var i = 0; i < segments; i++)
		{
			var x0 = startX + i * segW;
			var x1 = x0 + segW;
			var sign = (i % 2 == 0) ? -1f : 1f;
			builder.CubicTo(
				x0 + segW * 0.33f, cy + sign * size * 0.6f,
				x0 + segW * 0.66f, cy + sign * size * 0.6f,
				x1, cy);
		}

		return builder.Detach();
	}

	private static SKPathEffect? Create1DStamp(float param)
	{
		// Stamp a small diamond shape along the path
		using var builder = new SKPathBuilder();
		var s = Math.Max(2, param * 0.4f);
		builder.MoveTo(0, -s);
		builder.LineTo(s, 0);
		builder.LineTo(0, s);
		builder.LineTo(-s, 0);
		builder.Close();

		using var stampPath = builder.Detach();
		var advance = Math.Max(s * 2 + 2, param);
		return SKPathEffect.Create1DPath(stampPath, advance, 0, SKPath1DPathEffectStyle.Rotate);
	}

	private static SKPathEffect? Create2DTile(float param, float cellW)
	{
		// Tile a small circle pattern across the path fill
		using var builder = new SKPathBuilder();
		var r = Math.Max(1, param * 0.2f);
		builder.AddCircle(0, 0, r);

		using var tilePath = builder.Detach();
		var spacing = Math.Max(r * 3, param);
		var matrix = SKMatrix.CreateScale(spacing, spacing);
		return SKPathEffect.Create2DPath(matrix, tilePath);
	}

	private static SKPathEffect CreateCompose(float param)
	{
		using var dash = SKPathEffect.CreateDash(new[] { param, param * 0.5f }, 0);
		using var corner = SKPathEffect.CreateCorner(Math.Max(1, param * 0.5f));
		return SKPathEffect.CreateCompose(dash, corner);
	}
}

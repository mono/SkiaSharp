using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class PathEffectsLabSample : CanvasSampleBase
{
	private int effectType;
	private float interval = 15f;
	private float phase;
	private float strokeWidth = 4f;

	private static readonly string[] EffectTypes = { "Dash", "Discrete", "Corner", "Compose" };

	public override string Title => "Path Effects Lab";

	public override string Description =>
		"Apply dash, discrete, corner, and composed path effects with live parameter tuning.";

	public override string Category => SampleCategories.PathEffects;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("effect", "Effect Type", EffectTypes, effectType),
		new SliderControl("strokeWidth", "Stroke Width", 1, 20, strokeWidth),
		new SliderControl("interval", "Interval/Length", 1, 50, interval, 1),
		new SliderControl("phase", "Phase/Deviation", 0, 30, phase, 0.5f),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "effect": effectType = (int)value; break;
			case "interval": interval = (float)value; break;
			case "phase": phase = (float)value; break;
			case "strokeWidth": strokeWidth = (float)value; break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var cx = width / 2f;
		var cy = height / 2f;
		var size = Math.Min(width, height) * 0.35f;

		using var path = CreateStarPath(cx, cy, size, size * 0.45f, 5);
		using var effect = CreatePathEffect();
		using var paint = new SKPaint
		{
			IsStroke = true,
			StrokeWidth = strokeWidth,
			Color = new SKColor(0xFF3B82F6),
			IsAntialias = true,
			PathEffect = effect,
			StrokeCap = SKStrokeCap.Round,
		};

		canvas.DrawPath(path, paint);
	}

	private SKPathEffect? CreatePathEffect()
	{
		return effectType switch
		{
			0 => SKPathEffect.CreateDash(new[] { interval, interval / 2f }, phase),
			1 => SKPathEffect.CreateDiscrete(Math.Max(1, interval), phase),
			2 => SKPathEffect.CreateCorner(Math.Max(1, interval)),
			3 => CreateComposeEffect(),
			_ => null,
		};
	}

	private SKPathEffect CreateComposeEffect()
	{
		using var dash = SKPathEffect.CreateDash(new[] { interval, interval / 2f }, phase);
		using var discrete = SKPathEffect.CreateDiscrete(Math.Max(1, interval), phase);
		return SKPathEffect.CreateCompose(dash, discrete);
	}

	private static SKPath CreateStarPath(float cx, float cy, float outerRadius, float innerRadius, int points)
	{
		using var builder = new SKPathBuilder();
		var angle = -Math.PI / 2;
		var step = Math.PI / points;

		builder.MoveTo(
			cx + (float)(outerRadius * Math.Cos(angle)),
			cy + (float)(outerRadius * Math.Sin(angle)));

		for (int i = 1; i < 2 * points; i++)
		{
			angle += step;
			var r = (i % 2 == 0) ? outerRadius : innerRadius;
			builder.LineTo(
				cx + (float)(r * Math.Cos(angle)),
				cy + (float)(r * Math.Sin(angle)));
		}

		builder.Close();
		return builder.Detach();
	}
}

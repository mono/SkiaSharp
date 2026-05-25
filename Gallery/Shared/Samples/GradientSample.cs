using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class GradientSample : CanvasSampleBase
{
	private int gradientType;
	private float angle = 45f;
	private int tileMode;

	private static readonly string[] GradientTypes = { "Linear", "Radial", "Sweep", "Two-Point Conical" };
	private static readonly string[] TileModes = { "Clamp", "Repeat", "Mirror" };

	private static readonly SKColor[] GradientColors = { new(0xFF3B82F6), new(0xFF8B5CF6), new(0xFFEC4899) };

	public override string Title => "Gradient";

	public override string Category => SampleCategories.Shaders;

	public override string Description =>
		"Create linear, radial, sweep, and conical gradients with adjustable angle and tile modes.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("gradientType", "Gradient Type", GradientTypes, gradientType),
		new PickerControl("tileMode", "Tile Mode", TileModes, tileMode),
		new SliderControl("angle", "Angle", 0, 360, angle, 1),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "gradientType":
				gradientType = (int)value;
				break;
			case "angle":
				angle = (float)value;
				break;
			case "tileMode":
				tileMode = (int)value;
				break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var cx = width / 2f;
		var cy = height / 2f;
		var size = Math.Min(width, height) * 0.7f;
		var rect = new SKRect(cx - size / 2, cy - size / 2, cx + size / 2, cy + size / 2);

		var mode = tileMode switch
		{
			1 => SKShaderTileMode.Repeat,
			2 => SKShaderTileMode.Mirror,
			_ => SKShaderTileMode.Clamp,
		};

		using var paint = new SKPaint { IsAntialias = true };
		using var shader = CreateGradientShader(rect, mode);
		paint.Shader = shader;
		canvas.DrawRoundRect(rect, 20, 20, paint);
	}

	private SKShader CreateGradientShader(SKRect rect, SKShaderTileMode mode)
	{
		var cx = rect.MidX;
		var cy = rect.MidY;
		var radius = Math.Min(rect.Width, rect.Height) / 2f;
		var rad = angle * MathF.PI / 180f;

		return gradientType switch
		{
			1 => SKShader.CreateRadialGradient(
				new SKPoint(cx, cy), radius, GradientColors, null, mode),
			2 => SKShader.CreateSweepGradient(
				new SKPoint(cx, cy), GradientColors),
			3 => SKShader.CreateTwoPointConicalGradient(
				new SKPoint(cx, cy), radius * 0.1f,
				new SKPoint(cx, cy), radius,
				GradientColors, null, mode),
			_ => SKShader.CreateLinearGradient(
				new SKPoint(cx + MathF.Cos(rad) * radius, cy + MathF.Sin(rad) * radius),
				new SKPoint(cx - MathF.Cos(rad) * radius, cy - MathF.Sin(rad) * radius),
				GradientColors, null, mode),
		};
	}
}

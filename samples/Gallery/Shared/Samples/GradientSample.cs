using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class GradientSample : CanvasSampleBase
{
	private int gradientType;
	private float angle = 45f;
	private int tileMode;
	private int colorSpaceIndex;

	private static readonly string[] GradientTypes = { "Linear", "Radial", "Sweep", "Two-Point Conical" };
	private static readonly string[] TileModes = { "Clamp", "Repeat", "Mirror" };
	private static readonly string[] ColorSpaceNames =
	{
		"Destination (sRGB)", "sRGB Linear", "Lab", "OKLab", "LCH", "OKLCH", "Srgb", "HSL", "HWB"
	};
	private static readonly SKGradientInterpolationColorSpace[] ColorSpaces =
	{
		SKGradientInterpolationColorSpace.Destination,
		SKGradientInterpolationColorSpace.SrgbLinear,
		SKGradientInterpolationColorSpace.Lab,
		SKGradientInterpolationColorSpace.OKLab,
		SKGradientInterpolationColorSpace.LCH,
		SKGradientInterpolationColorSpace.OKLCH,
		SKGradientInterpolationColorSpace.Srgb,
		SKGradientInterpolationColorSpace.HSL,
		SKGradientInterpolationColorSpace.HWB,
	};

	private static readonly SKColorF[] GradientColors = { new(0.231f, 0.510f, 0.965f), new(0.545f, 0.361f, 0.965f), new(0.925f, 0.282f, 0.600f) };

	public override string Title => "Gradient";

	public override string Category => SampleCategories.Shaders;

	public override string Description =>
		"Create linear, radial, sweep, and conical gradients with adjustable angle, tile mode, and interpolation color space.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("gradientType", "Gradient Type", GradientTypes, gradientType),
		new PickerControl("tileMode", "Tile Mode", TileModes, tileMode),
		new PickerControl("colorSpaceIndex", "Interpolation Color Space", ColorSpaceNames, colorSpaceIndex),
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
			case "colorSpaceIndex":
				colorSpaceIndex = (int)value;
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

		using var colorspace = SKColorSpace.CreateSrgb();

		var interpolation = new SKGradientInterpolation
		{
			ColorSpace = ColorSpaces[colorSpaceIndex],
			HueMethod = SKGradientInterpolationHueMethod.Shorter,
		};

		return gradientType switch
		{
			1 => SKShader.CreateRadialGradient(
				new SKPoint(cx, cy), radius, GradientColors, colorspace, null, mode, interpolation),
			2 => SKShader.CreateSweepGradient(
				new SKPoint(cx, cy), GradientColors, colorspace, null,
				SKShaderTileMode.Clamp, 0, 360, interpolation),
			3 => SKShader.CreateTwoPointConicalGradient(
				new SKPoint(cx, cy), radius * 0.1f,
				new SKPoint(cx, cy), radius,
				GradientColors, colorspace, null, mode, interpolation),
			_ => SKShader.CreateLinearGradient(
				new SKPoint(cx + MathF.Cos(rad) * radius, cy + MathF.Sin(rad) * radius),
				new SKPoint(cx - MathF.Cos(rad) * radius, cy - MathF.Sin(rad) * radius),
				GradientColors, colorspace, null, mode, interpolation),
		};
	}
}

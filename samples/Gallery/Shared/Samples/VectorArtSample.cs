using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class VectorArtSample : CanvasSampleBase
{
	private float _rotation;
	private float _scale = 1f;
	private int _themeIndex;

	private static readonly string[] Themes = { "Original", "Mono", "Neon", "Midnight" };

	public override string Title => "Vector Art";

	public override string Description => "Render complex Bézier path artwork with rotation, scale, and color theme controls.";

	public override string Category => SampleCategories.Paths;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("theme", "Theme", Themes, _themeIndex),
		new SliderControl("scale", "Scale", 0.5f, 2f, _scale, 0.1f),
		new SliderControl("rotation", "Rotation", 0, 360, _rotation),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "rotation":
				_rotation = (float)value;
				break;
			case "scale":
				_scale = (float)value;
				break;
			case "theme":
				_themeIndex = (int)value;
				break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		var (bgColor, outerColor, innerColor) = _themeIndex switch
		{
			1 => (SKColors.White, new SKColor(0xFF888888), SKColors.White),
			2 => (new SKColor(0xFF1A1A2E), new SKColor(0xFF00FF41), new SKColor(0xFFFF00FF)),
			3 => (new SKColor(0xFF0D0D1A), new SKColor(0xFF7B2FBE), new SKColor(0xFFE040FB)),
			_ => (SKColors.White, SampleMedia.Colors.XamarinDarkBlue, SKColors.White),
		};

		canvas.Clear(bgColor);

		// Original design coordinates
		var imageLeft = 41.6587026f;
		var imageRight = 144.34135f;
		var imageTop = 56f;
		var imageBottom = 147f;

		var imageWidth = imageRight - imageLeft;

		var paddingFactor = .6f;
		var baseScale = (((float)height > width ? width : height) / imageWidth) * paddingFactor;

		var translateX = (imageLeft + imageRight) / -2 + width / baseScale * 1 / 2;
		var translateY = (imageBottom + imageTop) / -2 + height / baseScale * 1 / 2;

		canvas.Save();

		// Apply user transforms around center
		canvas.Translate(width / 2f, height / 2f);
		canvas.RotateDegrees(_rotation);
		canvas.Scale(_scale);
		canvas.Translate(-width / 2f, -height / 2f);

		canvas.Scale(baseScale, baseScale);
		canvas.Translate(translateX, translateY);

		using var paint = new SKPaint();
		paint.IsAntialias = true;
		paint.StrokeCap = SKStrokeCap.Round;

		// Outer hexagon shape using cubic Bézier curves
		{
			using var builder = new SKPathBuilder();
			builder.MoveTo(71.4311121f, 56f);
			builder.CubicTo(68.6763107f, 56.0058575f, 65.9796704f, 57.5737917f, 64.5928855f, 59.965729f);
			builder.LineTo(43.0238921f, 97.5342563f);
			builder.CubicTo(41.6587026f, 99.9325978f, 41.6587026f, 103.067402f, 43.0238921f, 105.465744f);
			builder.LineTo(64.5928855f, 143.034271f);
			builder.CubicTo(65.9798162f, 145.426228f, 68.6763107f, 146.994582f, 71.4311121f, 147f);
			builder.LineTo(114.568946f, 147f);
			builder.CubicTo(117.323748f, 146.994143f, 120.020241f, 145.426228f, 121.407172f, 143.034271f);
			builder.LineTo(142.976161f, 105.465744f);
			builder.CubicTo(144.34135f, 103.067402f, 144.341209f, 99.9325978f, 142.976161f, 97.5342563f);
			builder.LineTo(121.407172f, 59.965729f);
			builder.CubicTo(120.020241f, 57.5737917f, 117.323748f, 56.0054182f, 114.568946f, 56f);
			builder.LineTo(71.4311121f, 56f);
			builder.Close();

			using var path = builder.Detach();
			paint.Color = outerColor;
			canvas.DrawPath(path, paint);
		}

		// Inner letter shape (X/V chevron paths)
		{
			using var builder = new SKPathBuilder();
			builder.MoveTo(71.8225901f, 77.9780432f);
			builder.CubicTo(71.8818491f, 77.9721857f, 71.9440029f, 77.9721857f, 72.0034464f, 77.9780432f);
			builder.LineTo(79.444074f, 77.9780432f);
			builder.CubicTo(79.773437f, 77.9848769f, 80.0929203f, 78.1757336f, 80.2573978f, 78.4623994f);
			builder.LineTo(92.8795281f, 101.015639f);
			builder.CubicTo(92.9430615f, 101.127146f, 92.9839987f, 101.251384f, 92.9995323f, 101.378901f);
			builder.CubicTo(93.0150756f, 101.251354f, 93.055974f, 101.127107f, 93.1195365f, 101.015639f);
			builder.LineTo(105.711456f, 78.4623994f);
			builder.CubicTo(105.881153f, 78.167045f, 106.215602f, 77.975134f, 106.554853f, 77.9780432f);
			builder.LineTo(113.995483f, 77.9780432f);
			builder.CubicTo(114.654359f, 77.9839007f, 115.147775f, 78.8160066f, 114.839019f, 79.4008677f);
			builder.LineTo(102.518299f, 101.500005f);
			builder.LineTo(114.839019f, 123.568869f);
			builder.CubicTo(115.176999f, 124.157088f, 114.671442f, 125.027775f, 113.995483f, 125.021957f);
			builder.LineTo(106.554853f, 125.021957f);
			builder.CubicTo(106.209673f, 125.019028f, 105.873247f, 124.81384f, 105.711456f, 124.507327f);
			builder.LineTo(93.1195365f, 101.954088f);
			builder.CubicTo(93.0560031f, 101.84258f, 93.0150659f, 101.718333f, 92.9995323f, 101.590825f);
			builder.CubicTo(92.983989f, 101.718363f, 92.9430906f, 101.842629f, 92.8795281f, 101.954088f);
			builder.LineTo(80.2573978f, 124.507327f);
			builder.CubicTo(80.1004103f, 124.805171f, 79.7792269f, 125.008397f, 79.444074f, 125.021957f);
			builder.LineTo(72.0034464f, 125.021957f);
			builder.CubicTo(71.3274867f, 125.027814f, 70.8220664f, 124.157088f, 71.1600463f, 123.568869f);
			builder.LineTo(83.4807624f, 101.500005f);
			builder.LineTo(71.1600463f, 79.400867f);
			builder.CubicTo(70.8647037f, 78.86725f, 71.2250368f, 78.0919422f, 71.8225901f, 77.9780432f);
			builder.LineTo(71.8225901f, 77.9780432f);
			builder.Close();

			using var path = builder.Detach();
			paint.Color = innerColor;
			canvas.DrawPath(path, paint);
		}

		canvas.Restore();
	}
}

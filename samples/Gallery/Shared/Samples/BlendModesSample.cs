using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class BlendModesSample : CanvasSampleBase
{
	private int modeIndex;
	private float srcOpacity = 1f;
	private float dstOpacity = 1f;

	private static readonly string[] BlendModeNames = Enum.GetNames(typeof(SKBlendMode));

	public override string Title => "Blend Modes";

	public override string Description =>
		"Explore all SkiaSharp blend modes with adjustable source and destination opacity.";

	public override string Category => SampleCategories.Shaders;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("mode", "Blend Mode", BlendModeNames, modeIndex),
		new SliderControl("srcOpacity", "Source Opacity", 0, 1, srcOpacity, 0.05f),
		new SliderControl("dstOpacity", "Dest Opacity", 0, 1, dstOpacity, 0.05f),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "mode": modeIndex = (int)value; break;
			case "srcOpacity": srcOpacity = (float)value; break;
			case "dstOpacity": dstOpacity = (float)value; break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var mode = (SKBlendMode)modeIndex;
		var cx = width / 2f;
		var cy = height / 2f;
		var radius = Math.Min(width, height) * 0.25f;
		var offset = radius * 0.5f;

		// Draw in an isolated layer so the blend mode only affects dst+src
		canvas.SaveLayer(null);
		canvas.Clear(SKColors.Transparent);

		// Destination circle (cyan)
		using (var dstPaint = new SKPaint
		{
			Color = SKColors.Cyan.WithAlpha((byte)(dstOpacity * 255)),
			IsAntialias = true,
		})
		{
			canvas.DrawCircle(cx - offset, cy, radius, dstPaint);
		}

		// Source circle (magenta) with blend mode
		using (var srcPaint = new SKPaint
		{
			Color = SKColors.Magenta.WithAlpha((byte)(srcOpacity * 255)),
			IsAntialias = true,
			BlendMode = mode,
		})
		{
			canvas.DrawCircle(cx + offset, cy, radius, srcPaint);
		}

		canvas.Restore();

		// Draw blend mode name
		using var textFont = new SKFont { Size = Math.Min(width, height) * 0.06f };
		using var textPaint = new SKPaint
		{
			Color = SKColors.Black,
			IsAntialias = true,
		};
		canvas.DrawText(mode.ToString(), cx, cy + radius + textFont.Size * 2, SKTextAlign.Center, textFont, textPaint);
	}
}

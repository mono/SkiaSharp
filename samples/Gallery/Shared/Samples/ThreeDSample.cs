using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class ThreeDSample : AnimatedInteractiveSampleBase
{
	private SKMatrix44 rotationMatrix;
	private int axisIndex;
	private float speed = 5f;
	private bool showShadow = true;

	private static readonly string[] Axes = { "Y", "X", "Z", "XY" };

	public override string Title => "3D Rotation (ortho)";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("axis", "Rotation Axis", Axes, axisIndex),
		new SliderControl("speed", "Speed (°/frame)", 1, 20, speed, 1),
		new ToggleControl("shadow", "Show Shadow", showShadow),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "axis":
				axisIndex = (int)value;
				break;
			case "speed":
				speed = (float)value;
				break;
			case "shadow":
				showShadow = (bool)value;
				break;
		}
	}

	protected override async Task OnInit()
	{
		rotationMatrix = SKMatrix44.CreateIdentity();
		await base.OnInit();
	}

	protected override async Task OnUpdate(CancellationToken token)
	{
		await Task.Delay(25, token);

		var (rx, ry, rz) = axisIndex switch
		{
			1 => (1f, 0f, 0f),
			2 => (0f, 0f, 1f),
			3 => (1f, 1f, 0f),
			_ => (0f, 1f, 0f),
		};
		var step = SKMatrix44.CreateRotationDegrees(rx, ry, rz, speed);
		rotationMatrix.PostConcat(step);
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		var length = Math.Min(width / 6, height / 6);
		var rect = new SKRect(-length, -length, length, length);
		var side = rotationMatrix.MapPoint(new SKPoint(1, 0)).X > 0;

		canvas.Clear(SampleMedia.Colors.XamarinLightBlue);
		canvas.Translate(width / 2, height / 2);

		var matrix = rotationMatrix.Matrix;
		canvas.Concat(ref matrix);

		using var paint = new SKPaint
		{
			Color = side ? SampleMedia.Colors.XamarinPurple : SampleMedia.Colors.XamarinGreen,
			Style = SKPaintStyle.Fill,
			IsAntialias = true,
		};
		canvas.DrawRoundRect(rect, 30, 30, paint);

		if (showShadow)
		{
			using var shadow = SKShader.CreateLinearGradient(
				new SKPoint(0, 0), new SKPoint(0, length * 2),
				new[] { paint.Color.WithAlpha(127), paint.Color.WithAlpha(0) },
				null,
				SKShaderTileMode.Clamp);
			using var shadowPaint = new SKPaint
			{
				Shader = shadow,
				Style = SKPaintStyle.Fill,
				IsAntialias = true,
			};
			var shadowRect = rect;
			shadowRect.Offset(0, length * 2 + 5);
			canvas.DrawRoundRect(shadowRect, 30, 30, shadowPaint);
		}
	}
}

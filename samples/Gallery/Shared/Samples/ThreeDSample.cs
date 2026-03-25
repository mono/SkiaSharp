using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class ThreeDSample : AnimatedInteractiveSampleBase
{
	private SKMatrix44 rotationMatrix;
	private float speedX;
	private float speedY = 5f;
	private float speedZ;
	private int projectionIndex;
	private bool showShadow = true;

	private static readonly string[] Projections = { "Orthographic", "Perspective" };

	public override string Title => "3D Rotation";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("speedX", "X Axis Speed", -15, 15, speedX, 1),
		new SliderControl("speedY", "Y Axis Speed", -15, 15, speedY, 1),
		new SliderControl("speedZ", "Z Axis Speed", -15, 15, speedZ, 1),
		new PickerControl("projection", "Projection", Projections, projectionIndex),
		new ToggleControl("shadow", "Show Shadow", showShadow),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "speedX":
				speedX = (float)value;
				break;
			case "speedY":
				speedY = (float)value;
				break;
			case "speedZ":
				speedZ = (float)value;
				break;
			case "projection":
				projectionIndex = (int)value;
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

		var magnitude = MathF.Sqrt(speedX * speedX + speedY * speedY + speedZ * speedZ);
		if (magnitude > 0.01f)
		{
			var step = SKMatrix44.CreateRotationDegrees(speedX / magnitude, speedY / magnitude, speedZ / magnitude, magnitude);
			rotationMatrix.PostConcat(step);
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		var length = Math.Min(width / 6, height / 6);
		var rect = new SKRect(-length, -length, length, length);
		var side = rotationMatrix.MapPoint(new SKPoint(1, 0)).X > 0;

		canvas.Clear(SampleMedia.Colors.XamarinLightBlue);
		canvas.Translate(width / 2, height / 2);

		if (projectionIndex == 1)
		{
			// Perspective projection
			var persp = SKMatrix.Identity;
			persp.Persp2 = 1f / 800f;
			canvas.Concat(ref persp);
		}

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

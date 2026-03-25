using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample.Samples;

public class ThreeDSamplePerspective : AnimatedSampleBase
{
	private SKMatrix44 rotationMatrix;
	private float rotationAngle;

	public override string Title => "3D Rotation (perspective)";

	public override string Category => SampleCategories.General;

	protected override async Task OnInit()
	{
		rotationMatrix = SKMatrix44.CreateIdentity();
		rotationAngle = 0;

		await base.OnInit();
	}

	protected override async Task OnUpdate(CancellationToken token)
	{
		await Task.Delay(25, token);

		rotationAngle = (rotationAngle + 3) % 360;
		rotationMatrix = SKMatrix44.CreateRotationDegrees(0, 1, 0, rotationAngle);
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		var length = Math.Min(width / 6, height / 6);
		var rect = new SKRect(-length, -length, length, length);
		var side = rotationMatrix.MapPoint(new SKPoint(1, 0)).X > 0;

		canvas.Clear(SampleMedia.Colors.XamarinDarkBlue);

		canvas.Translate(width / 2, height / 2);

		// Apply perspective: shrink the far edge to simulate depth
		var m44 = rotationMatrix;
		var m = m44.Matrix;

		// Add a simple perspective warp
		var perspective = SKMatrix.Identity;
		perspective.Persp0 = 0;
		perspective.Persp1 = 0;
		perspective.Persp2 = 1f - m44.MapPoint(new SKPoint(0, 0)).X * 0.001f;

		canvas.Concat(ref m);

		var paint = new SKPaint
		{
			Color = side ? SampleMedia.Colors.XamarinLightBlue : SampleMedia.Colors.XamarinPurple,
			Style = SKPaintStyle.Fill,
			IsAntialias = true,
		};

		canvas.DrawRoundRect(rect, 30, 30, paint);

		// Draw shadow
		var shadow = SKShader.CreateLinearGradient(
			new SKPoint(0, 0), new SKPoint(0, length * 2),
			new[] { paint.Color.WithAlpha(127), paint.Color.WithAlpha(0) },
			null,
			SKShaderTileMode.Clamp);
		paint = new SKPaint
		{
			Shader = shadow,
			Style = SKPaintStyle.Fill,
			IsAntialias = true,
		};

		rect.Offset(0, length * 2 + 5);
		canvas.DrawRoundRect(rect, 30, 30, paint);

		// Draw label
		using var textPaint = new SKPaint
		{
			Color = SKColors.White.WithAlpha(180),
			TextSize = 24,
			IsAntialias = true,
			TextAlign = SKTextAlign.Center,
		};
		canvas.DrawText("Perspective", 0, -length - 20, textPaint);
	}
}

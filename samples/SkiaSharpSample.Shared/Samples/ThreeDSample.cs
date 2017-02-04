using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class ThreeDSample : AnimatedSampleBase
	{
		private float rotation;

		[Preserve]
		public ThreeDSample()
		{
		}

		public override string Title => "3D Rotation";

		protected override async Task OnInit()
		{
			rotation = 30;

			await base.OnInit();
		}

		protected override async Task OnUpdate(CancellationToken token, TaskScheduler mainScheduler)
		{
			await Task.Delay(25, token);

			rotation = (rotation + 5) % 360;
		}

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Translate(width / 2, height / 2);

			var length = Math.Min(width / 6, height / 6);
			var rect = new SKRect(-length, -length, length, length);
			var side = rotation > 90 && rotation < 270;

			canvas.Clear(SampleMedia.Colors.XamarinLightBlue);

			var view = new SK3dView();
			view.RotateYDegrees(rotation);
			view.ApplyToCanvas(canvas);

			var paint = new SKPaint
			{
				Color = side ? SampleMedia.Colors.XamarinPurple : SampleMedia.Colors.XamarinGreen,
				Style = SKPaintStyle.Fill,
				IsAntialias = true
			};

			canvas.DrawRoundRect(rect, 30, 30, paint);

			var shadow = SKShader.CreateLinearGradient(
				new SKPoint(0, 0), new SKPoint(0, length * 2),
				new[] { paint.Color.WithAlpha(127), paint.Color.WithAlpha(0) },
				null,
				SKShaderTileMode.Clamp);
			paint = new SKPaint
			{
				Shader = shadow,
				Style = SKPaintStyle.Fill,
				IsAntialias = true
			};

			rect.Offset(0, length * 2 + 5);
			canvas.DrawRoundRect(rect, 30, 30, paint);
		}
	}
}

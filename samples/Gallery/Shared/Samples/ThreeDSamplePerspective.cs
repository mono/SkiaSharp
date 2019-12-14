using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class ThreeDSamplePerspective : AnimatedSampleBase
	{
		private SK3dView rotationView;
		
		[Preserve]
		public ThreeDSamplePerspective()
		{
		}

		public override string Title => "3D Rotation (perspective)";

		protected override async Task OnInit()
		{
			// create the base and step 3D rotation matrices (around the y-axis)
			rotationView = new SK3dView();
			rotationView.RotateYDegrees(30);

			await base.OnInit();
		}

		protected override async Task OnUpdate(CancellationToken token)
		{
			await Task.Delay(25, token);

			// step the rotation matrix
			rotationView.RotateYDegrees(5);
		}

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			// get the 2D equivalent of the 3D matrix
			var rotationMatrix = rotationView.Matrix;

			// get the properties of the rectangle
			var length = Math.Min(width / 6, height / 6);
			var rect = new SKRect(-length, -length, length, length);
			var side = rotationMatrix.MapPoint(new SKPoint(1, 0)).X > 0;

			canvas.Clear(SampleMedia.Colors.XamarinLightBlue);

			// first do 2D translation to the center of the screen
			canvas.Translate(width / 2, height / 2);

			// then apply the 3D rotation
			canvas.Concat(ref rotationMatrix);

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

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class GradientSample : SampleBase
	{
		[Preserve]
		public GradientSample()
		{
		}

		public override string Title => "Gradient";

		public override SampleCategories Category => SampleCategories.Shaders;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			var ltColor = SKColors.White;
			var dkColor = SKColors.Black;

			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;
				using (var shader = SKShader.CreateLinearGradient(
					new SKPoint(0, 0),
					new SKPoint(0, height),
					new[] { ltColor, dkColor },
					null,
					SKShaderTileMode.Clamp))
				{

					paint.Shader = shader;
					canvas.DrawPaint(paint);
				}
			}

			// Center and Scale the Surface
			var scale = (width < height ? width : height) / (240f);
			canvas.Translate(width / 2f, height / 2f);
			canvas.Scale(scale, scale);
			canvas.Translate(-128, -128);

			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;
				using (var shader = SKShader.CreateTwoPointConicalGradient(
					new SKPoint(115.2f, 102.4f),
					25.6f,
					new SKPoint(102.4f, 102.4f),
					128.0f,
					new[] { ltColor, dkColor },
					null,
					SKShaderTileMode.Clamp
				))
				{
					paint.Shader = shader;

					canvas.DrawOval(new SKRect(51.2f, 51.2f, 204.8f, 204.8f), paint);
				}
			}
		}
	}
}

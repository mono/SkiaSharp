using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	public class SweepGradientShaderSample : SampleBase
	{
		public SweepGradientShaderSample()
		{
		}

		public override string Title => "Sweep Gradient Shader";

		public override string Category => SampleCategories.Shaders;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			var colors = new[] { SKColors.Cyan, SKColors.Magenta, SKColors.Yellow, SKColors.Cyan };
			var center = new SKPoint(width / 2f, height / 2f);

			using (var shader = SKShader.CreateSweepGradient(center, colors, null))
			using (var paint = new SKPaint())
			{
				paint.Shader = shader;
				canvas.DrawPaint(paint);
			}
		}
	}
}

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class ComposeShaderSample : SampleBase
	{
		[Preserve]
		public ComposeShaderSample()
		{
		}

		public override string Title => "Compose Shader";

		public override SampleCategories Category => SampleCategories.Shaders;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			var colors = new[] { SKColors.Blue, SKColors.Yellow };
			var center = new SKPoint(width / 2f, height / 2f);

			using (var shader1 = SKShader.CreateRadialGradient(center, 180.0f, colors, null, SKShaderTileMode.Clamp))
			using (var shader2 = SKShader.CreatePerlinNoiseTurbulence(0.025f, 0.025f, 2, 0.0f))
			using (var shader = SKShader.CreateCompose(shader1, shader2))
			using (var paint = new SKPaint())
			{
				paint.Shader = shader;
				canvas.DrawPaint(paint);
			}
		}
	}
}

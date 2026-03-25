using SkiaSharp;

namespace SkiaSharpSample.Samples;

public class FractalPerlinNoiseShaderSample : SampleBase
{
	public FractalPerlinNoiseShaderSample()
	{
	}

	public override string Title => "Fractal Perlin Noise Shader";

	public override string Category => SampleCategories.Shaders;

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		using (var shader = SKShader.CreatePerlinNoiseFractalNoise(0.05f, 0.05f, 4, 0.0f))
		using (var paint = new SKPaint())
		{
			paint.Shader = shader;
			canvas.DrawPaint(paint);
		}
	}
}

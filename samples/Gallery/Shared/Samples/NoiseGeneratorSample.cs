using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class NoiseGeneratorSample : InteractiveSampleBase
{
	private int noiseType;
	private float freqX = 0.02f;
	private float freqY = 0.02f;
	private float octaves = 4f;
	private bool tiled;

	private static readonly string[] NoiseTypes = { "Fractal Perlin", "Turbulence" };

	public override string Title => "Noise Generator";

	public override string Description =>
		"Generate procedural noise textures with adjustable frequency and octaves.";

	public override string Category => SampleCategories.Shaders;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("type", "Noise Type", NoiseTypes, noiseType),
		new SliderControl("freqX", "Frequency X", 0.001f, 0.1f, freqX, 0.001f),
		new SliderControl("freqY", "Frequency Y", 0.001f, 0.1f, freqY, 0.001f),
		new SliderControl("octaves", "Octaves", 1, 8, octaves, 1),
		new ToggleControl("tiled", "Tile Mode", tiled),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "type": noiseType = (int)value; break;
			case "freqX": freqX = (float)value; break;
			case "freqY": freqY = (float)value; break;
			case "octaves": octaves = (float)value; break;
			case "tiled": tiled = (bool)value; break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var numOctaves = (int)octaves;

		SKShader shader;
		if (tiled)
		{
			var tileSize = new SKPointI(width, height);
			shader = noiseType == 0
				? SKShader.CreatePerlinNoiseFractalNoise(freqX, freqY, numOctaves, 0f, tileSize)
				: SKShader.CreatePerlinNoiseTurbulence(freqX, freqY, numOctaves, 0f, tileSize);
		}
		else
		{
			shader = noiseType == 0
				? SKShader.CreatePerlinNoiseFractalNoise(freqX, freqY, numOctaves, 0f)
				: SKShader.CreatePerlinNoiseTurbulence(freqX, freqY, numOctaves, 0f);
		}

		using (shader)
		using (var paint = new SKPaint { Shader = shader })
		{
			canvas.DrawPaint(paint);
		}
	}
}

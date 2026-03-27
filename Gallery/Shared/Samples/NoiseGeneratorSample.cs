using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class NoiseGeneratorSample : CanvasSampleBase
{
	private int noiseType;
	private float freqX = 0.02f;
	private float freqY = 0.02f;
	private float octaves = 4f;
	private float seed;
	private float offsetX;
	private float offsetY;
	private bool tiled;

	private static readonly string[] NoiseTypes = { "Fractal Perlin", "Turbulence" };

	public override string Title => "Noise Generator";

	public override string Description =>
		"Generate procedural Perlin noise textures with adjustable frequency, octaves, seed, and offset.";

	public override string Category => SampleCategories.Shaders;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("type", "Noise Type", NoiseTypes, noiseType),
		new ToggleControl("tiled", "Tile Mode", tiled),
		new SliderControl("freqX", "Frequency X", 0.001f, 0.100f, freqX, 0.001f),
		new SliderControl("freqY", "Frequency Y", 0.001f, 0.100f, freqY, 0.001f),
		new SliderControl("octaves", "Octaves", 1, 8, octaves, 1),
		new SliderControl("seed", "Seed", 0, 100, seed, 1),
		new SliderControl("offsetX", "Offset X", -500, 500, offsetX, 10),
		new SliderControl("offsetY", "Offset Y", -500, 500, offsetY, 10),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "type": noiseType = (int)value; break;
			case "freqX": freqX = (float)value; break;
			case "freqY": freqY = (float)value; break;
			case "octaves": octaves = (float)value; break;
			case "seed": seed = (float)value; break;
			case "offsetX": offsetX = (float)value; break;
			case "offsetY": offsetY = (float)value; break;
			case "tiled": tiled = (bool)value; break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var numOctaves = (int)octaves;

		SKShader baseShader;
		if (tiled)
		{
			var tileSize = new SKPointI(width, height);
			baseShader = noiseType == 0
				? SKShader.CreatePerlinNoiseFractalNoise(freqX, freqY, numOctaves, seed, tileSize)
				: SKShader.CreatePerlinNoiseTurbulence(freqX, freqY, numOctaves, seed, tileSize);
		}
		else
		{
			baseShader = noiseType == 0
				? SKShader.CreatePerlinNoiseFractalNoise(freqX, freqY, numOctaves, seed)
				: SKShader.CreatePerlinNoiseTurbulence(freqX, freqY, numOctaves, seed);
		}

		// Apply offset via local matrix translation
		var shader = (offsetX != 0 || offsetY != 0)
			? baseShader.WithLocalMatrix(SKMatrix.CreateTranslation(offsetX, offsetY))
			: baseShader;

		using (baseShader)
		using (shader != baseShader ? shader : null)
		using (var paint = new SKPaint { Shader = shader })
		{
			canvas.DrawPaint(paint);
		}
	}
}

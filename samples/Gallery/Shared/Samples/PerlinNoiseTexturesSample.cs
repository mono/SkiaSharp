using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class PerlinNoiseTexturesSample : CanvasSampleBase
{
	private float frequency = 0.015f;
	private float octaves = 4f;
	private float seed;
	private int presetIndex;

	private static readonly string[] Presets = { "Raw Comparison", "Marble", "Wood Grain", "Clouds", "Fire" };

	public override string Title => "Perlin Noise Textures";

	public override DateOnly? DateAdded => new DateOnly(2026, 4, 27);

	public override string Description =>
		"Procedural textures using Perlin noise — fractal vs turbulence side by side, with color presets for marble, wood, cloud, and fire effects.";

	public override string Category => SampleManager.Shaders;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("preset", "Texture Preset", Presets, presetIndex),
		new SliderControl("frequency", "Frequency", 0.002f, 0.06f, frequency, 0.001f),
		new SliderControl("octaves", "Octaves", 1, 8, octaves, 1),
		new SliderControl("seed", "Seed", 0, 100, seed, 1),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "preset": presetIndex = (int)value; break;
			case "frequency": frequency = (float)value; break;
			case "octaves": octaves = (float)value; break;
			case "seed": seed = (float)value; break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var numOctaves = (int)octaves;

		if (presetIndex == 0)
		{
			// Side-by-side raw comparison
			DrawRawComparison(canvas, width, height, numOctaves);
		}
		else
		{
			// Full-canvas textured preset
			DrawTexturePreset(canvas, width, height, numOctaves);
		}
	}

	private void DrawRawComparison(SKCanvas canvas, int width, int height, int numOctaves)
	{
		var halfW = width / 2f;
		var panelH = height - 40f;

		// Left panel: Fractal Noise
		using var fractalShader = SKShader.CreatePerlinNoiseFractalNoise(
			frequency, frequency, numOctaves, seed);
		using var fractalPaint = new SKPaint { Shader = fractalShader };
		canvas.Save();
		canvas.ClipRect(new SKRect(0, 30, halfW - 2, height));
		canvas.DrawPaint(fractalPaint);
		canvas.Restore();

		// Right panel: Turbulence
		using var turbShader = SKShader.CreatePerlinNoiseTurbulence(
			frequency, frequency, numOctaves, seed);
		using var turbPaint = new SKPaint { Shader = turbShader };
		canvas.Save();
		canvas.ClipRect(new SKRect(halfW + 2, 30, width, height));
		canvas.DrawPaint(turbPaint);
		canvas.Restore();

		// Divider
		using var divPaint = new SKPaint
		{
			Color = new SKColor(0, 0, 0, 80),
			IsStroke = true,
			StrokeWidth = 2,
		};
		canvas.DrawLine(halfW, 30, halfW, height, divPaint);

		// Labels
		using var labelFont = new SKFont { Size = 14 };
		using var labelPaint = new SKPaint { IsAntialias = true };

		// Background strip for labels
		using var stripPaint = new SKPaint { Color = new SKColor(255, 255, 255, 220) };
		canvas.DrawRect(0, 0, width, 30, stripPaint);

		labelPaint.Color = new SKColor(0xFF2C3E50);
		var fractalLabel = "Fractal Noise";
		var turbLabel = "Turbulence";
		var flw = labelFont.MeasureText(fractalLabel);
		var tlw = labelFont.MeasureText(turbLabel);
		canvas.DrawText(fractalLabel, halfW / 2f - flw / 2f, 21, labelFont, labelPaint);
		canvas.DrawText(turbLabel, halfW + halfW / 2f - tlw / 2f, 21, labelFont, labelPaint);
	}

	private void DrawTexturePreset(SKCanvas canvas, int width, int height, int numOctaves)
	{
		// Use turbulence as base for all texture presets (it looks more natural)
		using var noiseShader = SKShader.CreatePerlinNoiseTurbulence(
			frequency, frequency, numOctaves, seed);

		// Apply a color filter to transform the noise into the desired texture
		using var colorFilter = CreatePresetColorFilter();

		using var paint = new SKPaint
		{
			Shader = noiseShader,
			ColorFilter = colorFilter,
		};
		canvas.DrawPaint(paint);

		// Draw preset label
		using var bgPaint = new SKPaint { Color = new SKColor(0, 0, 0, 120) };
		canvas.DrawRoundRect(new SKRect(8, 8, 170, 36), 6, 6, bgPaint);

		using var labelFont = new SKFont { Size = 14 };
		using var labelPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
		canvas.DrawText(Presets[presetIndex], 16, 28, labelFont, labelPaint);
	}

	private SKColorFilter? CreatePresetColorFilter()
	{
		return presetIndex switch
		{
			1 => CreateMarbleFilter(),
			2 => CreateWoodFilter(),
			3 => CreateCloudFilter(),
			4 => CreateFireFilter(),
			_ => null,
		};
	}

	private static SKColorFilter CreateMarbleFilter()
	{
		// Map noise to blue-white marble tones
		// Using a color matrix to shift towards blue/grey
		return SKColorFilter.CreateColorMatrix(new float[]
		{
			0.4f, 0.3f, 0.3f, 0, 0.1f,   // R
			0.4f, 0.3f, 0.3f, 0, 0.1f,   // G
			0.5f, 0.4f, 0.4f, 0, 0.2f,   // B
			0,    0,    0,    1, 0,        // A
		});
	}

	private static SKColorFilter CreateWoodFilter()
	{
		// Map noise to brown wood tones
		return SKColorFilter.CreateColorMatrix(new float[]
		{
			0.6f, 0.3f, 0.1f, 0, 0.25f,  // R — warm brown
			0.3f, 0.2f, 0.1f, 0, 0.12f,  // G
			0.1f, 0.1f, 0.05f, 0, 0.04f, // B
			0,    0,    0,     1, 0,       // A
		});
	}

	private static SKColorFilter CreateCloudFilter()
	{
		// Map noise to sky-blue and white clouds
		return SKColorFilter.CreateColorMatrix(new float[]
		{
			0.4f, 0.3f, 0.3f, 0, 0.35f,  // R
			0.4f, 0.4f, 0.4f, 0, 0.45f,  // G
			0.3f, 0.3f, 0.5f, 0, 0.55f,  // B — push blue
			0,    0,    0,    1, 0,        // A
		});
	}

	private static SKColorFilter CreateFireFilter()
	{
		// Map noise to orange-red fire tones
		return SKColorFilter.CreateColorMatrix(new float[]
		{
			1.2f, 0.5f, 0.2f, 0, 0.1f,   // R — hot
			0.5f, 0.3f, 0.1f, 0, 0.0f,   // G — warm
			0.0f, 0.0f, 0.1f, 0, 0.0f,   // B — almost none
			0,    0,    0,    1, 0,        // A
		});
	}
}

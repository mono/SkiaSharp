using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class PhotoLabSample : InteractiveSampleBase
{
	private int preset;
	private float blurSigma;
	private float morphology;
	private bool magnifier;

	private static readonly string[] Presets =
		{ "Normal", "Grayscale", "Sepia", "Invert", "High Contrast", "Color Dodge" };

	public override string Title => "Photo Lab";

	public override string Description =>
		"Apply color filters, blur, morphology, and magnifier effects to an image.";

	public override string Category => SampleCategories.ImageFilters;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("preset", "Preset", Presets, preset),
		new SliderControl("blurSigma", "Blur Sigma", 0, 30, blurSigma),
		new SliderControl("morphology", "Morphology", -10, 10, morphology, 1),
		new ToggleControl("magnifier", "Show Magnifier", magnifier),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "preset": preset = (int)value; break;
			case "blurSigma": blurSigma = (float)value; break;
			case "morphology": morphology = (float)value; break;
			case "magnifier": magnifier = (bool)value; break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		using var stream = new SKManagedStream(SampleMedia.Images.Baboon);
		using var bitmap = SKBitmap.Decode(stream);

		var colorFilter = CreatePresetColorFilter();
		var imageFilters = new List<SKImageFilter>();

		try
		{
			SKImageFilter lastFilter = null;

			if (blurSigma > 0)
			{
				var f = SKImageFilter.CreateBlur(blurSigma, blurSigma, lastFilter);
				imageFilters.Add(f);
				lastFilter = f;
			}

			var m = (int)morphology;
			if (m < 0)
			{
				var f = SKImageFilter.CreateErode(-m, -m, lastFilter);
				imageFilters.Add(f);
				lastFilter = f;
			}
			else if (m > 0)
			{
				var f = SKImageFilter.CreateDilate(m, m, lastFilter);
				imageFilters.Add(f);
				lastFilter = f;
			}

			if (magnifier)
			{
				var size = Math.Min(width, height) / 3f;
				var f = SKImageFilter.CreateMagnifier(
					SKRect.Create(width / 2f - size / 2, height / 2f - size / 2, size, size),
					size / 2, size / 10, SKSamplingOptions.Default, lastFilter);
				imageFilters.Add(f);
				lastFilter = f;
			}

			using var paint = new SKPaint
			{
				ColorFilter = colorFilter,
				ImageFilter = lastFilter,
			};
			canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
		}
		finally
		{
			foreach (var f in imageFilters)
				f.Dispose();
			colorFilter?.Dispose();
		}
	}

	private SKColorFilter CreatePresetColorFilter()
	{
		return preset switch
		{
			1 => SKColorFilter.CreateColorMatrix(new float[]
			{
				0.21f, 0.72f, 0.07f, 0, 0,
				0.21f, 0.72f, 0.07f, 0, 0,
				0.21f, 0.72f, 0.07f, 0, 0,
				0,     0,     0,     1, 0,
			}),
			2 => SKColorFilter.CreateColorMatrix(new float[]
			{
				0.393f, 0.769f, 0.189f, 0, 0,
				0.349f, 0.686f, 0.168f, 0, 0,
				0.272f, 0.534f, 0.131f, 0, 0,
				0,      0,      0,      1, 0,
			}),
			3 => SKColorFilter.CreateColorMatrix(new float[]
			{
				-1,  0,  0, 0, 1,
				 0, -1,  0, 0, 1,
				 0,  0, -1, 0, 1,
				 0,  0,  0, 1, 0,
			}),
			4 => SKColorFilter.CreateHighContrast(
				false, SKHighContrastConfigInvertStyle.NoInvert, 0.5f),
			5 => SKColorFilter.CreateBlendMode(
				new SKColor(255, 200, 100), SKBlendMode.ColorDodge),
			_ => null,
		};
	}
}

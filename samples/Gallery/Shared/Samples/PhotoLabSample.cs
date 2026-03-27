using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class PhotoLabSample : CanvasSampleBase
{
	// Effect state
	private bool colorEnabled = true;
	private int colorPreset;
	private bool blurEnabled;
	private float blurSigma = 5f;
	private bool morphEnabled;
	private float morphRadius = 3f;
	private int morphType; // 0=Dilate, 1=Erode
	private bool magnifierEnabled;
	private float magnifierZoom = 5f;
	private bool contrastEnabled;
	private float contrastAmount = 0.3f;
	private SKBitmap? cachedBitmap;

	private static readonly string[] ColorPresets =
		{ "Grayscale", "Sepia", "Invert", "Warm", "Cool", "Color Dodge" };
	private static readonly string[] MorphTypes = { "Dilate", "Erode" };

	public override string Title => "Photo Lab";

	public override string Description =>
		"Composable image effect stack — color filters, blur, morphology, magnifier, and high contrast.";

	public override string Category => SampleCategories.ImageFilters;

	protected override Task OnInit()
	{
		using var stream = new SKManagedStream(SampleMedia.Images.Baboon);
		cachedBitmap = SKBitmap.Decode(stream);
		return base.OnInit();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		cachedBitmap?.Dispose();
		cachedBitmap = null;
	}

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new GroupControl("color", "Color Filter", colorEnabled,
		[
			new PickerControl("preset", "Preset", ColorPresets, colorPreset),
		], Description: "Apply color transformations like grayscale, sepia, or inversion."),
		new GroupControl("contrast", "High Contrast", contrastEnabled,
		[
			new SliderControl("amount", "Amount", 0, 1, contrastAmount, 0.05f),
		], Description: "Increase contrast for improved readability."),
		new GroupControl("blur", "Gaussian Blur", blurEnabled,
		[
			new SliderControl("sigma", "Sigma", 0.5f, 30, blurSigma),
		], Description: "Apply a Gaussian blur with configurable radius."),
		new GroupControl("morph", "Morphology", morphEnabled,
		[
			new PickerControl("type", "Type", MorphTypes, morphType),
			new SliderControl("radius", "Radius", 1, 10, morphRadius, 1),
		], Description: "Expand (Dilate) or shrink (Erode) bright regions."),
		new GroupControl("magnifier", "Magnifier", magnifierEnabled,
		[
			new SliderControl("zoom", "Zoom", 2, 15, magnifierZoom),
		], Description: "Lens magnification effect applied to the center of the image."),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			// Group toggles
			case "color": colorEnabled = (bool)value; break;
			case "blur": blurEnabled = (bool)value; break;
			case "morph": morphEnabled = (bool)value; break;
			case "magnifier": magnifierEnabled = (bool)value; break;
			case "contrast": contrastEnabled = (bool)value; break;

			// Color filter children
			case "color.preset": colorPreset = (int)value; break;

			// Blur children
			case "blur.sigma": blurSigma = (float)value; break;

			// Morphology children
			case "morph.type": morphType = (int)value; break;
			case "morph.radius": morphRadius = (float)value; break;

			// Magnifier children
			case "magnifier.zoom": magnifierZoom = (float)value; break;

			// Contrast children
			case "contrast.amount": contrastAmount = (float)value; break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		if (cachedBitmap == null) return;

		SKColorFilter? colorFilter = null;
		var imageFilters = new List<SKImageFilter>();

		try
		{
			// Build color filter
			if (colorEnabled)
				colorFilter = CreateColorFilter();

			// Build image filter chain
			SKImageFilter? lastFilter = null;

			if (blurEnabled && blurSigma > 0)
			{
				var f = SKImageFilter.CreateBlur(blurSigma, blurSigma, lastFilter);
				imageFilters.Add(f);
				lastFilter = f;
			}

			if (morphEnabled)
			{
				var r = (int)morphRadius;
				var f = morphType == 1
					? SKImageFilter.CreateErode(r, r, lastFilter)
					: SKImageFilter.CreateDilate(r, r, lastFilter);
				imageFilters.Add(f);
				lastFilter = f;
			}

			if (magnifierEnabled)
			{
				// Lens covers center 40% of the canvas
				var lensW = width * 0.4f;
				var lensH = height * 0.4f;
				var lensBounds = SKRect.Create(
					width / 2f - lensW / 2f,
					height / 2f - lensH / 2f,
					lensW, lensH);
				var f = SKImageFilter.CreateMagnifier(
					lensBounds, magnifierZoom, 10f, SKSamplingOptions.Default, lastFilter);
				imageFilters.Add(f);
				lastFilter = f;
			}

			if (contrastEnabled)
			{
				var hcFilter = SKColorFilter.CreateHighContrast(
					false, SKHighContrastConfigInvertStyle.NoInvert, contrastAmount);
				if (colorFilter != null)
				{
					var composed = SKColorFilter.CreateCompose(hcFilter, colorFilter);
					colorFilter.Dispose();
					hcFilter.Dispose();
					colorFilter = composed;
				}
				else
				{
					colorFilter = hcFilter;
				}
			}

			using var paint = new SKPaint
			{
				ColorFilter = colorFilter,
				ImageFilter = lastFilter,
			};
			canvas.DrawBitmap(cachedBitmap, SKRect.Create(width, height), paint);
		}
		finally
		{
			foreach (var f in imageFilters)
				f.Dispose();
			colorFilter?.Dispose();
		}
	}

	private SKColorFilter? CreateColorFilter() => colorPreset switch
	{
		0 => SKColorFilter.CreateColorMatrix(new float[] // Grayscale
		{
			0.21f, 0.72f, 0.07f, 0, 0,
			0.21f, 0.72f, 0.07f, 0, 0,
			0.21f, 0.72f, 0.07f, 0, 0,
			0,     0,     0,     1, 0,
		}),
		1 => SKColorFilter.CreateColorMatrix(new float[] // Sepia
		{
			0.393f, 0.769f, 0.189f, 0, 0,
			0.349f, 0.686f, 0.168f, 0, 0,
			0.272f, 0.534f, 0.131f, 0, 0,
			0,      0,      0,      1, 0,
		}),
		2 => SKColorFilter.CreateColorMatrix(new float[] // Invert
		{
			-1,  0,  0, 0, 1,
			 0, -1,  0, 0, 1,
			 0,  0, -1, 0, 1,
			 0,  0,  0, 1, 0,
		}),
		3 => SKColorFilter.CreateColorMatrix(new float[] // Warm
		{
			1.2f, 0,    0,    0, 0,
			0,    1.0f, 0,    0, 0,
			0,    0,    0.8f, 0, 0,
			0,    0,    0,    1, 0,
		}),
		4 => SKColorFilter.CreateColorMatrix(new float[] // Cool
		{
			0.8f, 0,    0,    0, 0,
			0,    1.0f, 0,    0, 0,
			0,    0,    1.2f, 0, 0,
			0,    0,    0,    1, 0,
		}),
		5 => SKColorFilter.CreateBlendMode(
			new SKColor(255, 200, 100), SKBlendMode.ColorDodge),
		_ => null,
	};
}

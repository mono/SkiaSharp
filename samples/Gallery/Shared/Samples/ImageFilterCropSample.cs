using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class ImageFilterCropSample : CanvasSampleBase
{
	private float cropLeft = 50f;
	private float cropTop = 50f;
	private float cropRight = 250f;
	private float cropBottom = 250f;
	private int tileModeIndex = 0;
	private bool useBlur = false;
	private bool enableCrop = true;
	private SKBitmap? cachedBitmap;

	private readonly SKShaderTileMode[] tileModes =
	[
		SKShaderTileMode.Decal,
		SKShaderTileMode.Clamp,
		SKShaderTileMode.Repeat,
		SKShaderTileMode.Mirror,
	];

	public override string Title => "Crop Image Filter";

	public override DateOnly? DateAdded => new DateOnly(2026, 6, 24);

	public override string Category => SampleManager.ImageFilters;

	public override string Description =>
		"Crop image filter output to a rectangle with different tile modes. " +
		"Demonstrates the modern replacement for the deprecated cropRect parameter pattern.";

	public override IReadOnlyList<string> ApiTags =>
	[
		"SKImageFilter", "SKImageFilter.CreateCrop", "SKImageFilter.CreateBlur",
		"SKShaderTileMode", "SKManagedStream", "SKBitmap",
		"SKCanvas.DrawBitmap", "SKCanvas", "SKPaint",
	];

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new ToggleControl("enableCrop", "Enable Crop", enableCrop),
		new SliderControl("cropLeft", "Crop Left", 0, 300, cropLeft, 10),
		new SliderControl("cropTop", "Crop Top", 0, 300, cropTop, 10),
		new SliderControl("cropRight", "Crop Right", 0, 300, cropRight, 10),
		new SliderControl("cropBottom", "Crop Bottom", 0, 300, cropBottom, 10),
		new PickerControl("tileMode", "Tile Mode", tileModes.Select(m => m.ToString()).ToArray(), tileModeIndex),
		new ToggleControl("useBlur", "Compose with Blur", useBlur),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "enableCrop":
				enableCrop = (bool)value;
				break;
			case "cropLeft":
				cropLeft = (float)value;
				break;
			case "cropTop":
				cropTop = (float)value;
				break;
			case "cropRight":
				cropRight = (float)value;
				break;
			case "cropBottom":
				cropBottom = (float)value;
				break;
			case "tileMode":
				tileModeIndex = (int)value;
				break;
			case "useBlur":
				useBlur = (bool)value;
				break;
		}
	}

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

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.LightGray);

		if (cachedBitmap == null) return;

		// Calculate the destination rect to fit the bitmap in the canvas
		var destRect = SKRect.Create(width, height);

		// Create the crop rect (in canvas coordinates, where the filter operates)
		var cropRect = new SKRect(cropLeft, cropTop, cropRight, cropBottom);
		var tileMode = tileModes[tileModeIndex];

		// Apply the crop filter only when enabled. A null filter is the correct
		// no-op here: it draws the full, uncropped image. (CreateEmpty() is NOT
		// used for the disabled state because an empty filter discards all
		// content and would render nothing instead of the original image.)
		using var inputFilter = (enableCrop && useBlur) ? SKImageFilter.CreateBlur(5, 5) : null;
		using var cropFilter = enableCrop ? SKImageFilter.CreateCrop(cropRect, tileMode, inputFilter) : null;

		using var paint = new SKPaint();
		paint.ImageFilter = cropFilter;
		canvas.DrawBitmap(cachedBitmap, destRect, SKSamplingOptions.Default, paint);

		// Draw a red outline showing the crop rect bounds.
		// The crop image filter operates in canvas coordinates, so the outline
		// is drawn with the raw crop rect (no scaling) to match the filter exactly.
		using var outlinePaint = new SKPaint
		{
			Color = SKColors.Red,
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 3,
			IsAntialias = true,
		};

		canvas.DrawRect(cropRect, outlinePaint);
	}
}

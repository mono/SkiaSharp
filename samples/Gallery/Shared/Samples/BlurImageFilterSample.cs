using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class BlurImageFilterSample : CanvasSampleBase
{
	private float sigmaX = 5f;
	private float sigmaY = 5f;
	private SKBitmap? cachedBitmap;

	public override string Title => "Blur Image Filter";

	public override string Category => SampleCategories.ImageFilters;

	public override string Description =>
		"Apply Gaussian blur with independent horizontal and vertical sigma controls.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("sigmaX", "Sigma X", 0, 50, sigmaX, 0.5f),
		new SliderControl("sigmaY", "Sigma Y", 0, 50, sigmaY, 0.5f),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "sigmaX":
				sigmaX = (float)value;
				break;
			case "sigmaY":
				sigmaY = (float)value;
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
		canvas.Clear(SKColors.White);

		if (cachedBitmap == null) return;

		using var filter = SKImageFilter.CreateBlur(sigmaX, sigmaY);
		using var paint = new SKPaint();
		paint.ImageFilter = filter;

		canvas.DrawBitmap(cachedBitmap, SKRect.Create(width, height), paint);
	}
}

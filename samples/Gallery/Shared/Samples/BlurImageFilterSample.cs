using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class BlurImageFilterSample : InteractiveSampleBase
{
	private float sigmaX = 5f;
	private float sigmaY = 5f;

	public override string Title => "Blur Image Filter";

	public override string Category => SampleCategories.ImageFilters;

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

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		using var stream = new SKManagedStream(SampleMedia.Images.Baboon);
		using var bitmap = SKBitmap.Decode(stream);
		using var filter = SKImageFilter.CreateBlur(sigmaX, sigmaY);
		using var paint = new SKPaint();
		paint.ImageFilter = filter;

		canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
	}
}

using SkiaSharp;

namespace SkiaSharpSample.Samples;

public class MagnifierImageFilterSample : SampleBase
{
	public MagnifierImageFilterSample()
	{
	}

	public override string Title => "Magnifier Image Filter";

	public override string Category => SampleCategories.ImageFilters;

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var size = width > height ? height : width;
		var small = size / 5;

		using (var stream = new SKManagedStream(SampleMedia.Images.Baboon))
		using (var bitmap = SKBitmap.Decode(stream))
		using (var filter = SKImageFilter.CreateMagnifier(SKRect.Create(small * 2, small * 2, small * 3, small * 3), small, small, SKSamplingOptions.Default))
		using (var paint = new SKPaint())
		{
			paint.ImageFilter = filter;

			canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
		}
	}
}

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	public class ChainedImageFilterSample : SampleBase
	{
		public ChainedImageFilterSample()
		{
		}

		public override string Title => "Chained Image Filter";

		public override string Category => SampleCategories.ImageFilters;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			var size = width > height ? height : width;
			var small = size / 5;

			using (var stream = new SKManagedStream(SampleMedia.Images.Baboon))
			using (var bitmap = SKBitmap.Decode(stream))
			using (var filterMag = SKImageFilter.CreateMagnifier(SKRect.Create(small * 2, small * 2, small * 3, small * 3), small, small, SKSamplingOptions.Default))
			using (var filterBlur = SKImageFilter.CreateBlur(5, 5, filterMag))
			using (var paint = new SKPaint())
			{
				paint.ImageFilter = filterBlur;

				canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
			}
		}
	}
}

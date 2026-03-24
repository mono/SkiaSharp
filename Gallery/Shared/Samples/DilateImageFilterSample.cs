using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class DilateImageFilterSample : SampleBase
	{
		[Preserve]
		public DilateImageFilterSample()
		{
		}

		public override string Title => "Dilate Image Filter";

		public override SampleCategories Category => SampleCategories.ImageFilters;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			using (var stream = new SKManagedStream(SampleMedia.Images.Baboon))
			using (var bitmap = SKBitmap.Decode(stream))
			using (var filter = SKImageFilter.CreateDilate(5, 5))
			using (var paint = new SKPaint())
			{
				paint.ImageFilter = filter;

				canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
			}
		}
	}
}

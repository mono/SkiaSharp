using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class HighContrastColorFilterSample : SampleBase
	{
		private bool grayscale;

		[Preserve]
		public HighContrastColorFilterSample()
		{
		}

		public override string Title => "High Contrast Color Filter";

		public override SampleCategories Category => SampleCategories.ColorFilters;

		protected override void OnTapped()
		{
			base.OnTapped();

			grayscale = !grayscale;

			Refresh();
		}

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			// load the image from the embedded resource stream
			using (var stream = new SKManagedStream(SampleMedia.Images.Baboon))
			using (var bitmap = SKBitmap.Decode(stream))
			using (var cf = SKColorFilter.CreateHighContrast(grayscale, SKHighContrastConfigInvertStyle.InvertBrightness, 0.1f))
			using (var paint = new SKPaint())
			{
				paint.ColorFilter = cf;

				canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
			}
		}
	}
}

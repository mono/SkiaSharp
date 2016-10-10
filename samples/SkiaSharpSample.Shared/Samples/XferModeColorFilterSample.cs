using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class XferModeColorFilterSample : SampleBase
	{
		[Preserve]
		public XferModeColorFilterSample()
		{
		}

		public override string Title => "Xfer Mode Color Filter";

		public override SampleCategories Category => SampleCategories.ColorFilters;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			// load the image from the embedded resource stream
			using (var stream = new SKManagedStream(SampleMedia.Images.Baboon))
			using (var bitmap = SKBitmap.Decode(stream))
			using (var cf = SKColorFilter.CreateXferMode(SKColors.Red, SKXferMode.ColorDodge))
			using (var paint = new SKPaint())
			{
				paint.ColorFilter = cf;

				canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
			}
		}
	}
}

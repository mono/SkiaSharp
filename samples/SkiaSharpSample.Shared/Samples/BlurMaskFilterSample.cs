using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class BlurMaskFilterSample : SampleBase
	{
		[Preserve]
		public BlurMaskFilterSample()
		{
		}

		public override string Title => "Blur Mask Filter";

		public override SampleCategories Category => SampleCategories.MaskFilters;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.DrawColor(SKColors.White);

			using (var paint = new SKPaint())
			using (var filter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5.0f))
			{
				paint.IsAntialias = true;
				paint.TextSize = 120;
				paint.TextAlign = SKTextAlign.Center;
				paint.MaskFilter = filter;

				canvas.DrawText("SkiaSharp", width / 2f, height / 2f, paint);
			}
		}
	}
}

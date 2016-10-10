using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class EmbossMaskFilterSample : SampleBase
	{
		[Preserve]
		public EmbossMaskFilterSample()
		{
		}

		public override string Title => "Emboss Mask Filter";

		public override SampleCategories Category => SampleCategories.MaskFilters;

		public override SampleBackends SupportedBackends => SampleBackends.All | ~SampleBackends.OpenGL;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.DrawColor(SKColors.White);

			SKPoint3 direction = new SKPoint3(1.0f, 1.0f, 1.0f);

			using (var paint = new SKPaint())
			using (var filter = SKMaskFilter.CreateEmboss(2.0f, direction, 0.3f, 0.1f))
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

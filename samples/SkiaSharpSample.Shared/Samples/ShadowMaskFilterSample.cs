using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class ShadowMaskFilterSample : SampleBase
	{
		[Preserve]
		public ShadowMaskFilterSample()
		{
		}

		public override string Title => "Shadow Mask Filter";

		public override SampleCategories Category => SampleCategories.MaskFilters;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.DrawColor(SKColors.White);

			SKPoint3 lightPos = new SKPoint3(-700, -700, 2800);
			float lightWidth = 2800;
			float ambientAlpha = 0.25f;
			float spotAlpha = 0.25f;

			using (var paint = new SKPaint())
			using (var filter = SKMaskFilter.CreateShadow(2.0f, lightPos, lightWidth, ambientAlpha, spotAlpha))
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

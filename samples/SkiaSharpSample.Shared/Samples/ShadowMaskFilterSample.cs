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

			var rect = SKRect.Create(width / 3, height / 3, width / 3, height / 3);

			float occluderHeight = 2.0f;
			SKPoint3 lightPos = new SKPoint3(0, 0, 50);
			float lightWidth = 500;
			float ambientAlpha = 0.75f;
			float spotAlpha = 0.75f;

			using (var paint = new SKPaint())
			using (var filter = SKMaskFilter.CreateShadow(occluderHeight, lightPos, lightWidth, ambientAlpha, spotAlpha))
			{
				paint.IsAntialias = true;
				paint.Color = SKColors.Black;
				paint.MaskFilter = filter;

				// draw the shadow
				canvas.DrawRect(rect, paint);

				paint.Color = SKColors.DarkBlue;
				paint.MaskFilter = null;

				// draw the rectangle
				canvas.DrawRect(rect, paint);
			}
		}
	}
}

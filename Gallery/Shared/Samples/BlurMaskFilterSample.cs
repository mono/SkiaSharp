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

				canvas.DrawText("SkiaSharp", width / 2f, height / 4f, paint);
			}

			// scale up so we don't have to change the values in the method
			var scaling = 3;
			canvas.Scale(scaling, scaling);
			var rect = SKRect.Create(width / (3f * scaling), height / (2f * scaling), width / (3f * scaling), height / (4f * scaling));

			DrawInnerBlurRectangle(canvas, rect);
		}

		private void DrawInnerBlurRectangle(SKCanvas canvas, SKRect rect)
		{
			// create the rounded rectangle
			var roundedRect = new SKPath();
			roundedRect.AddRoundRect(rect, 10, 10);

			// draw the white background
			var p = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = SKColors.White
			};
			canvas.DrawPath(roundedRect, p);

			using (new SKAutoCanvasRestore(canvas))
			{
				// clip the canvas to stop the blur from appearing outside
				canvas.ClipPath(roundedRect, SKClipOperation.Intersect, true);

				// draw the wide blur all around
				p.Color = SKColors.Black;
				p.Style = SKPaintStyle.Stroke;
				p.StrokeWidth = 2;
				p.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2);
				canvas.Translate(0.5f, 1.5f);
				canvas.DrawPath(roundedRect, p);

				// draw the narrow blur at the top
				p.StrokeWidth = 1;
				p.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 1);
				canvas.DrawPath(roundedRect, p);
			}

			// draw the border
			p.StrokeWidth = 2;
			p.MaskFilter = null;
			p.Color = SampleMedia.Colors.XamarinGreen;
			canvas.DrawPath(roundedRect, p);
		}
	}
}

using System;

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class BitmapAnnotationSample : SampleBase
	{
		[Preserve]
		public BitmapAnnotationSample()
		{
		}

		public override string Title => "Bitmap Annotation";

		public override SampleCategories Category => SampleCategories.BitmapDecoding;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			// decode the bitmap
			var desiredInfo = new SKImageInfo(386, 395, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
			using (var stream = new SKManagedStream(SampleMedia.Images.BabyTux))
			using (var bitmap = SKBitmap.Decode(stream, desiredInfo))
			{
				// draw directly on the bitmap
				using (var annotationCanvas = new SKCanvas(bitmap))
				using (var paint = new SKPaint())
				{
					paint.StrokeWidth = 3;
					paint.Color = SampleMedia.Colors.XamarinLightBlue;
					paint.Style = SKPaintStyle.Stroke;

					var face = SKRectI.Create(100, 50, 190, 170);
					annotationCanvas.DrawRect(face, paint);
				}

				// draw the modified bitmap to the screen
				canvas.DrawBitmap(bitmap, 10, 10);
			}
		}
	}
}

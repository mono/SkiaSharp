using System;

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class DngDecoderSample : SampleBase
	{
		[Preserve]
		public DngDecoderSample()
		{
		}

		public override string Title => "Adobe DNG Decoder";

		public override SampleCategories Category => SampleCategories.BitmapDecoding;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			// load the embedded resource stream
			using (var stream = new SKManagedStream(SampleMedia.Images.AdobeDng))
			using (var codec = SKCodec.Create(stream))
			using (var paint = new SKPaint())
			using (var tf = SKTypeface.FromFamilyName("Arial"))
			{
				var info = codec.Info;

				paint.IsAntialias = true;
				paint.TextSize = 14;
				paint.Typeface = tf;
				paint.Color = SKColors.Black;

				// decode the image
				using (var bitmap = new SKBitmap(info.Width, info.Height, info.ColorType, info.IsOpaque ? SKAlphaType.Opaque : SKAlphaType.Premul))
				{
					IntPtr length;
					var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels(out length));
					if (result == SKCodecResult.Success || result == SKCodecResult.IncompleteInput)
					{
						var x = 25;
						var y = 25;

						canvas.DrawBitmap(bitmap, SKRect.Create(x, y, bitmap.Width / 2, bitmap.Height / 2));
						x += bitmap.Width / 2 + 25;
						y += 14;

						canvas.DrawText(string.Format("Result: {0}", result), x, y, paint);
						y += 20;

						canvas.DrawText(string.Format("Size: {0}px x {1}px", bitmap.Width, bitmap.Height), x, y, paint);
						y += 20;

						canvas.DrawText(string.Format("Pixels: {0} @ {1}b/px", bitmap.Pixels.Length, bitmap.BytesPerPixel), x, y, paint);
					}
				}
			}
		}
	}
}

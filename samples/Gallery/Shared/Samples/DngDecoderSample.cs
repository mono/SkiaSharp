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
			using var codec = SKCodec.Create(SampleMedia.Images.AdobeDng);

			if (codec == null)
			{
				using var errorPaint = new SKPaint
				{
					TextSize = 60.0f,
					IsAntialias = true,
					Color = 0xFF9CAFB7,
					StrokeWidth = 3,
					TextAlign = SKTextAlign.Center
				};

				canvas.DrawText("Oops! No DNG support!", width / 2f, height / 3, errorPaint);

				return;
			}

			var info = codec.Info;

			using var tf = SKTypeface.FromFamilyName("Arial");
			using var paint = new SKPaint
			{
				IsAntialias = true,
				TextSize = 14,
				Typeface = tf,
				Color = SKColors.Black
			};

			// decode the image
			using var bitmap = new SKBitmap(info.Width, info.Height, info.ColorType, info.IsOpaque ? SKAlphaType.Opaque : SKAlphaType.Premul);
			var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels());
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

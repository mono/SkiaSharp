using System;

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class BitmapSubsetDecoderSample : SampleBase
	{
		[Preserve]
		public BitmapSubsetDecoderSample()
		{
		}

		public override string Title => "Bitmap Decoder (Subset)";

		public override SampleCategories Category => SampleCategories.BitmapDecoding;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			var paint = new SKPaint
			{
				IsStroke = true,
				StrokeWidth = 3,
				Color = SampleMedia.Colors.XamarinLightBlue
			};

			using (var stream = new SKManagedStream(SampleMedia.Images.BabyTux))
			using (var bigBitmap = SKBitmap.Decode(stream))
			{
				canvas.DrawBitmap(bigBitmap, 10, 10);
			}

			using (var stream = new SKManagedStream(SampleMedia.Images.BabyTux))
			using (var codec = SKCodec.Create(stream))
			{
				var info = codec.Info;
				var subset = SKRectI.Create(100, 50, 190, 170);
				var options = new SKCodecOptions(subset);
				using (var bitmap = new SKBitmap(subset.Width, subset.Height, info.ColorType, SKAlphaType.Premul))
				{
					var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels(), options);
					if (result == SKCodecResult.Success || result == SKCodecResult.IncompleteInput)
					{
						canvas.DrawBitmap(bitmap, info.Width + 20, subset.Top + 10);
					}
				}
			}
		}
	}
}

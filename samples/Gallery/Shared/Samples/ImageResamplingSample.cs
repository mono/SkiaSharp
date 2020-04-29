using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class ImageResamplingSample : AnimatedSampleBase
	{
		private bool useSkiaNative = false;

		[Preserve]
		public ImageResamplingSample()
		{
		}

		public override string Title => "Image Resampling";

		public override SampleCategories Category => SampleCategories.Xtras;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.Black);

			using (var stream = new SKManagedStream(SampleMedia.Images.Baboon))
			using (var bitmap = SKBitmap.Decode(stream))
			using (var hardPaint = new SKPaint
			{
				FilterQuality = SKFilterQuality.None,
				IsAntialias = false
			})
			using (var softPaint = new SKPaint
			{
				TextSize = 16,
				Color = SKColors.White,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				FilterQuality = SKFilterQuality.High,
				TextAlign = SKTextAlign.Center
			})
			{
				var dstWidth = width * 0.75f;
				var dstHeight = height * 0.75f;
				var dstRect = SKRect.Create((width - dstWidth) / 2f, 20 + (height - dstHeight - 20) / 2f, dstWidth, dstHeight);

				if (useSkiaNative)
				{
					canvas.DrawBitmap(bitmap, dstRect, softPaint);
					canvas.DrawText("Skia resize", width / 2f, 20, softPaint);
				}
				else
				{
					var options = new SKImageSamplingOptions
					{
						OutputOffset = dstRect.Location,
						ParallelOptions = new ParallelOptions()
					};

					using (var resampled = bitmap.ResampledTo(dstRect.Width, dstRect.Height, options))
					{
						// Make sure to convert the positions to integers to avoid Skia doing another sub-pixel resize!
						canvas.DrawBitmap(resampled, (int)dstRect.Left, (int)dstRect.Top, hardPaint);
						canvas.DrawText("SkiaSharp resample", width / 2f, 20, softPaint);
					}
				}
			}
		}

		protected override async Task OnUpdate(CancellationToken token)
		{
			await Task.Delay(1000, token);
			useSkiaNative = !useSkiaNative;
		}
	}
}

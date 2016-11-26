using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class DecodeGifFramesSample : SampleBase
	{
		private int currentFrame = 0;
		private SKCodec codec = null;
		private SKImageInfo info = SKImageInfo.Empty;
		private SKBitmap bitmap = null;
		private CancellationTokenSource cts;
		private SKCodecFrameInfo[] frames;
		private TaskScheduler scheduler;

		[Preserve]
		public DecodeGifFramesSample()
		{
		}

		public override string Title => "Decode Gif Frames";

		public override SampleCategories Category => SampleCategories.BitmapDecoding;

		protected override async Task OnInit()
		{
			await base.OnInit();

			var stream = new SKManagedStream(SampleMedia.Images.AnimatedHeartGif, true);
			codec = SKCodec.Create(stream);
			frames = codec.FrameInfo;

			info = codec.Info;
			info = new SKImageInfo(info.Width, info.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

			bitmap = new SKBitmap(info);

			cts = new CancellationTokenSource();

			scheduler = TaskScheduler.Current;
			var loop = Task.Run(async () =>
			{
				while (!cts.IsCancellationRequested)
				{
					var duration = frames[currentFrame].Duration;
					if (duration <= 0)
						duration = 100;

					await Task.Delay(duration, cts.Token);

					// next frame
					currentFrame++;
					if (currentFrame >= frames.Length)
						currentFrame = 0;

					new Task(Refresh).Start(scheduler);
				}
			}, cts.Token);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			codec?.Dispose();
			codec = null;

			cts.Cancel();
		}

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.Black);

			var opts = new SKCodecOptions(currentFrame, false);
			if (codec?.GetPixels(info, bitmap.GetPixels(), opts) == SKCodecResult.Success)
			{
				canvas.DrawBitmap(bitmap, 0, 0);
			}
		}
	}
}

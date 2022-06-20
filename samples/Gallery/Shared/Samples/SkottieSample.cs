using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Skottie;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class SkottieSample : AnimatedSampleBase
	{
		private Animation _animation;
		private Stopwatch _watch = new Stopwatch();

		[Preserve]
		public SkottieSample()
		{
		}

		public override string Title => "Skottie";

		public override SampleCategories Category => SampleCategories.General;

		protected override async Task OnInit()
		{
			_animation = Animation.Create(SampleMedia.Images.LottieLogo);
			_animation.Seek(0, null);

			_watch.Start();

			await base.OnInit();
		}

		protected override async Task OnUpdate(CancellationToken token)
		{
			if (_animation == null)
				return;

			await Task.Delay(25, token);

			_animation.SeekFrameTime(_watch.Elapsed);

			if (_watch.Elapsed > _animation.Duration)
				_watch.Restart();
		}

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			if (_animation == null)
				return;

			canvas.Clear(SKColors.White);

			_animation.Render(canvas, new SKRect(0, 0, width, height));
		}
	}
}

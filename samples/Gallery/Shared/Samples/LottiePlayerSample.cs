using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Skottie;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class LottiePlayerSample : CanvasSampleBase
{
	public override bool IsAnimated => true;

	private Animation? _animation;
	private readonly Stopwatch _watch = new Stopwatch();
	private bool _playing = true;
	private float _speed = 1f;

	public override string Title => "Lottie Player";

	public override DateOnly? DateAdded => new DateOnly(2026, 3, 27);

	public override string Description => "Play Lottie/Skottie animations with playback speed control.";

	public override string Category => SampleManager.General;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("speed", "Speed", 0.25f, 4f, _speed, 0.25f),
		new ToggleControl("playing", "Playing", _playing),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "playing":
				_playing = (bool)value;
				if (_playing && !_watch.IsRunning)
					_watch.Start();
				else if (!_playing)
					_watch.Stop();
				break;
			case "speed":
				_speed = (float)value;
				break;
		}
	}

	protected override async Task OnInit()
	{
		_animation = Animation.Create(SampleMedia.Images.LottieLogo);
		if (_animation == null) return;

		_animation.Seek(0, null);

		_watch.Start();

		await base.OnInit();
	}

	protected override async Task OnUpdate(CancellationToken token)
	{
		if (_animation == null)
			return;

		await Task.Delay(25, token);

		if (_playing)
		{
			var elapsed = TimeSpan.FromTicks((long)(_watch.Elapsed.Ticks * _speed));
			_animation.SeekFrameTime(elapsed);

			if (elapsed > _animation.Duration)
				_watch.Restart();
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		if (_animation == null)
			return;

		canvas.Clear(SKColors.White);

		_animation.Render(canvas, new SKRect(0, 0, width, height));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_animation?.Dispose();
		_animation = null;
	}
}

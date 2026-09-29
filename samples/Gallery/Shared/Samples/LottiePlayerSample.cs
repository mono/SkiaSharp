using System;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Skottie;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class LottiePlayerSample : CanvasSampleBase
{
	public override bool IsAnimated => true;

	protected override TimeSpan AnimationInterval => TimeSpan.FromMilliseconds(25);

	private Animation? _animation;
	private TimeSpan _position;
	private bool _playing = true;
	private float _speed = 1f;

	public override string Title => "Lottie Player";

	public override DateOnly? DateAdded => new DateOnly(2026, 3, 27);

	public override string Description => "Play Lottie/Skottie animations with playback speed control.";

	public override IReadOnlyList<string> ApiTags =>
	[
		"Animation", "Animation.Render", "Animation.SeekFrameTime",
		"SKCanvas", "SKRect",
	];

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
		_position = TimeSpan.Zero;

		await base.OnInit();
	}

	protected override bool OnUpdate(TimeSpan elapsed)
	{
		if (!_playing || _animation == null)
			return false;
		var duration = _animation.Duration;
		if (duration <= TimeSpan.Zero)
			return false;
		_position = TimeSpan.FromTicks((_position.Ticks + (long)(elapsed.Ticks * _speed)) % duration.Ticks);
		_animation.SeekFrameTime(_position);
		return true;
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

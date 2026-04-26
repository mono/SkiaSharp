using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class GifPlayerSample : CanvasSampleBase
{
	public override bool IsAnimated => true;

	private int currentFrame;
	private bool playing = true;
	private float speed = 1f;
	private SKCodec? codec;
	private SKImageInfo info;
	private SKBitmap? bitmap;
	private SKCodecFrameInfo[]? frames;

	public override string Title => "GIF Player";

	public override string Category => SampleCategories.BitmapDecoding;

	public override string Description => "Play animated GIFs frame-by-frame with adjustable playback speed.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("speed", "Speed", 0.25f, 4, speed, 0.25f),
		new ToggleControl("playing", "Playing", playing),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "playing":
				playing = (bool)value;
				break;
			case "speed":
				speed = (float)value;
				break;
		}
	}

	protected override async Task OnInit()
	{
		var stream = new SKManagedStream(SampleMedia.Images.AnimatedHeartGif, true);
		codec = SKCodec.Create(stream);
		if (codec == null) return;

		frames = codec.FrameInfo;

		info = codec.Info;
		info = new SKImageInfo(info.Width, info.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

		bitmap = new SKBitmap(info);

		await base.OnInit();
	}

	protected override async Task OnUpdate(CancellationToken token)
	{
		if (frames == null || frames.Length == 0)
		{
			await Task.Delay(100, token);
			return;
		}

		var duration = frames[currentFrame].Duration;
		if (duration <= 0)
			duration = 100;

		// Apply speed multiplier
		var adjustedDuration = (int)Math.Max(10, duration / speed);
		await Task.Delay(adjustedDuration, token);

		if (playing)
		{
			currentFrame++;
			if (currentFrame >= frames.Length)
				currentFrame = 0;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		codec?.Dispose();
		codec = null;
		bitmap?.Dispose();
		bitmap = null;
		frames = null;
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.Black);

		if (codec == null || bitmap == null || frames == null)
			return;

		var opts = new SKCodecOptions(currentFrame);
		if (codec.GetPixels(info, bitmap.GetPixels(), opts) == SKCodecResult.Success)
		{
			bitmap.NotifyPixelsChanged();

			// Center and scale the GIF
			var scale = Math.Min((float)width / info.Width, (float)height / info.Height);
			var scaledW = info.Width * scale;
			var scaledH = info.Height * scale;
			var destRect = SKRect.Create((width - scaledW) / 2, (height - scaledH) / 2, scaledW, scaledH);

			canvas.DrawBitmap(bitmap, destRect);
		}

		// Draw frame info
		using var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.White };
		using var font = new SKFont(SKTypeface.Default, 14);
		var statusText = playing ? "▶" : "⏸";
		canvas.DrawText($"{statusText} Frame {currentFrame + 1}/{frames.Length}  Speed: {speed:F2}x", 10, height - 10, font, textPaint);
	}
}

using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class AnimatedWebpEncoderSample : CanvasSampleBase
{
	private float _quality = 80;
	private int _compressionIndex;

	private SKData? _encodedData;
	private SKCodec? _codec;
	private int _currentFrame;
	private SKBitmap? _currentBitmap;
	private int _frameDurationMs;
	private SKTypeface? _typeface;

	private static readonly string[] CompressionOptions = { "Lossy", "Lossless" };

	private const string Text = "SkiaSharp";
	private const int FrameWidth = 480;
	private const int FrameHeight = 240;
	private const int FrameDuration = 120;

	// gradient colours for a loopable sky-like background
	private static readonly SKColor TopStart = new SKColor(20, 30, 80);
	private static readonly SKColor BottomStart = new SKColor(60, 90, 160);
	private static readonly SKColor TopEnd = new SKColor(40, 50, 120);
	private static readonly SKColor BottomEnd = new SKColor(80, 120, 200);

	public override string Title => "Animated WebP Encoder";

	public override string Description => "Encode an animated WebP with letter-by-letter reveal, blink, and fade-out, then play it back.";

	public override string Category => SampleManager.BitmapDecoding;

	public override DateOnly? DateAdded => new DateOnly(2026, 5, 5);

	public override bool IsAnimated => _codec != null && _codec.FrameCount > 1;

	public override byte[]? DownloadBytes => _encodedData?.ToArray();
	public override string DownloadFileName => "SkiaSharp-Animation.webp";
	public override string DownloadMimeType => "image/webp";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("quality", "Quality", 0, 100, _quality, 5),
		new PickerControl("compression", "Compression", CompressionOptions, _compressionIndex),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "quality":
				_quality = (float)value;
				break;
			case "compression":
				_compressionIndex = (int)value;
				break;
		}

		RebuildAnimation();
	}

	protected override Task OnInit()
	{
		using var fontStream = SampleMedia.Fonts.Nabla;
		_typeface = SKTypeface.FromStream(fontStream);
		RebuildAnimation();
		return base.OnInit();
	}

	protected override async Task OnUpdate(CancellationToken token)
	{
		await Task.Delay(Math.Max(16, _frameDurationMs), token);
		if (_codec != null && _codec.FrameCount > 0)
			_currentFrame = (_currentFrame + 1) % _codec.FrameCount;
	}

	private void RebuildAnimation()
	{
		DisposeEncoded();

		// Phase 1: letters appear one by one  (Text.Length frames)
		// Phase 2: full text visible           (4 frames)
		// Phase 3: blink off                   (2 frames)
		// Phase 4: blink on                    (2 frames)
		// Phase 5: blink off                   (2 frames)
		// Phase 6: blink on                    (2 frames)
		// Phase 7: letters disappear one by one(Text.Length frames)
		// Phase 8: empty pause                 (4 frames)  <- makes loop seamless

		var letterCount = Text.Length;
		var totalFrames =
			letterCount +    // appear
			4 +              // hold
			2 + 2 + 2 + 2 + // blink x2
			letterCount +    // disappear
			4;               // pause

		var frames = new SKWebpEncoderFrame[totalFrames];
		var bitmaps = new SKBitmap[totalFrames];
		var pixmaps = new SKPixmap[totalFrames];

		_frameDurationMs = FrameDuration;

		try
		{
			for (var i = 0; i < totalFrames; i++)
			{
				bitmaps[i] = new SKBitmap(FrameWidth, FrameHeight);
				using var canvas = new SKCanvas(bitmaps[i]);
				DrawAnimationFrame(canvas, FrameWidth, FrameHeight, i, totalFrames, letterCount, _typeface);
				pixmaps[i] = bitmaps[i].PeekPixels();
				frames[i] = new SKWebpEncoderFrame(pixmaps[i], TimeSpan.FromMilliseconds(FrameDuration));
			}

			var compression = _compressionIndex == 1
				? SKWebpEncoderCompression.Lossless
				: SKWebpEncoderCompression.Lossy;
			var options = new SKWebpEncoderOptions(compression, _quality);
			_encodedData = SKWebpEncoder.EncodeAnimated(frames, options);

			if (_encodedData != null)
			{
				_codec = SKCodec.Create(_encodedData);
				_currentBitmap = new SKBitmap(_codec!.Info);
				_currentFrame = 0;
			}
		}
		finally
		{
			for (var i = 0; i < totalFrames; i++)
			{
				pixmaps[i]?.Dispose();
				bitmaps[i]?.Dispose();
			}
		}

		Refresh();
	}

	private static void DrawAnimationFrame(
		SKCanvas canvas, int w, int h, int frame, int totalFrames, int letterCount, SKTypeface? typeface)
	{
		// interpolate background gradient across the timeline for subtle motion
		var t = (float)frame / Math.Max(1, totalFrames - 1);
		var top = LerpColor(TopStart, TopEnd, t);
		var bottom = LerpColor(BottomStart, BottomEnd, t);

		using var bgPaint = new SKPaint();
		bgPaint.Shader = SKShader.CreateLinearGradient(
			new SKPoint(0, 0), new SKPoint(0, h),
			new[] { top, bottom }, null, SKShaderTileMode.Clamp);
		canvas.DrawRect(0, 0, w, h, bgPaint);

		// draw subtle star-like dots that drift
		DrawStars(canvas, w, h, t);

		// determine which letters are visible
		var (visibleStart, visibleCount) = ComputeVisibleRange(frame, letterCount);
		if (visibleCount <= 0)
			return;

		var fontSize = h * 0.28f;
		using var font = new SKFont(typeface, fontSize);
		using var textPaint = new SKPaint
		{
			Color = SKColors.White,
			IsAntialias = true,
		};

		// measure full text to keep letters in stable positions
		var totalWidth = font.MeasureText(Text, textPaint);
		var startX = (w - totalWidth) / 2f;
		var y = h / 2f + fontSize * 0.35f;

		// advance past invisible leading letters
		var x = startX;
		for (var i = 0; i < visibleStart; i++)
			x += font.MeasureText(Text[i].ToString(), textPaint);

		// draw each visible letter with a slight glow
		for (var i = visibleStart; i < visibleStart + visibleCount; i++)
		{
			var ch = Text[i].ToString();
			var charW = font.MeasureText(ch, textPaint);

			// glow
			using var glowPaint = new SKPaint
			{
				Color = new SKColor(140, 180, 255, 90),
				IsAntialias = true,
				MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6),
			};
			canvas.DrawText(ch, x, y, font, glowPaint);

			// letter
			canvas.DrawText(ch, x, y, font, textPaint);
			x += charW;
		}
	}

	private static (int start, int count) ComputeVisibleRange(int frame, int letterCount)
	{
		var phase = 0;

		// Phase 1: appear (from first to last)
		if (frame < letterCount)
			return (0, frame + 1);
		phase = frame - letterCount;

		// Phase 2: hold (4)
		if (phase < 4)
			return (0, letterCount);
		phase -= 4;

		// Phase 3-6: blink (off 2, on 2, off 2, on 2)
		if (phase < 2) return (0, 0);            // blink off
		phase -= 2;
		if (phase < 2) return (0, letterCount);  // blink on
		phase -= 2;
		if (phase < 2) return (0, 0);            // blink off
		phase -= 2;
		if (phase < 2) return (0, letterCount);  // blink on
		phase -= 2;

		// Phase 7: disappear (first letter goes first)
		if (phase < letterCount)
			return (phase + 1, letterCount - phase - 1);
		phase -= letterCount;

		// Phase 8: empty pause
		return (0, 0);
	}

	private static void DrawStars(SKCanvas canvas, int w, int h, float t)
	{
		// deterministic "random" star positions
		var seed = 42;
		using var starPaint = new SKPaint
		{
			Color = new SKColor(255, 255, 255, 60),
			IsAntialias = true,
		};

		for (var i = 0; i < 30; i++)
		{
			seed = (seed * 1103515245 + 12345) & 0x7fffffff;
			var sx = (seed % w) + t * 12f;
			sx %= w;
			seed = (seed * 1103515245 + 12345) & 0x7fffffff;
			var sy = seed % h;
			seed = (seed * 1103515245 + 12345) & 0x7fffffff;
			var sr = 1f + (seed % 20) / 20f;
			canvas.DrawCircle(sx, sy, sr, starPaint);
		}
	}

	private static SKColor LerpColor(SKColor a, SKColor b, float t)
	{
		return new SKColor(
			(byte)(a.Red + (b.Red - a.Red) * t),
			(byte)(a.Green + (b.Green - a.Green) * t),
			(byte)(a.Blue + (b.Blue - a.Blue) * t),
			(byte)(a.Alpha + (b.Alpha - a.Alpha) * t));
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.Black);

		if (_codec == null || _encodedData == null || _currentBitmap == null)
		{
			using var errorPaint = new SKPaint { Color = SKColors.Red, IsAntialias = true };
			using var errorFont = new SKFont(SampleMedia.Fonts.Default, 18);
			canvas.DrawText("Encoding failed", width / 2f, height / 2f, SKTextAlign.Center, errorFont, errorPaint);
			return;
		}

		var frameIndex = Math.Clamp(_currentFrame, 0, Math.Max(0, _codec.FrameCount - 1));
		var opts = new SKCodecOptions(frameIndex);
		_codec.GetPixels(_currentBitmap.Info, _currentBitmap.GetPixels(), opts);

		var scale = Math.Min((float)width / _currentBitmap.Width, (float)height / _currentBitmap.Height);
		scale = Math.Min(scale, 2f);
		var dw = _currentBitmap.Width * scale;
		var dh = _currentBitmap.Height * scale;
		var destRect = new SKRect(
			(width - dw) / 2f, (height - dh) / 2f,
			(width + dw) / 2f, (height + dh) / 2f);

		canvas.DrawBitmap(_currentBitmap, destRect);

		// info bar
		using var infoPaint = new SKPaint { Color = new SKColor(200, 200, 200), IsAntialias = true };
		using var infoFont = new SKFont(SampleMedia.Fonts.Default, 12);
		var infoText = $"Frames: {_codec.FrameCount}  |  {_encodedData.Size:N0} bytes  |  Frame {frameIndex + 1}/{_codec.FrameCount}";
		canvas.DrawText(infoText, width / 2f, destRect.Bottom + 18, SKTextAlign.Center, infoFont, infoPaint);
	}

	private void DisposeEncoded()
	{
		_currentBitmap?.Dispose();
		_currentBitmap = null;
		_codec?.Dispose();
		_codec = null;
		_encodedData?.Dispose();
		_encodedData = null;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DisposeEncoded();
		_typeface?.Dispose();
		_typeface = null;
	}
}

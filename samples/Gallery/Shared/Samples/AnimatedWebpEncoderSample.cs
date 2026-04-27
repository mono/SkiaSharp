using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class AnimatedWebpEncoderSample : CanvasSampleBase
{
	private int _frameCount = 4;
	private float _quality = 80;
	private int _compressionIndex;
	private int _frameDuration = 250;

	private SKData? _encodedData;
	private SKCodec? _codec;
	private int _currentFrame;
	private SKBitmap? _currentBitmap;

	private static readonly string[] CompressionOptions = { "Lossy", "Lossless" };

	private static readonly SKColor[] FrameColors =
	{
		SKColors.Red, SKColors.Orange, SKColors.Yellow, SKColors.Green,
		SKColors.Cyan, SKColors.Blue, SKColors.Purple, SKColors.Magenta,
	};

	public override string Title => "Animated WebP Encoder";

	public override string Description => "Encode multiple frames as an animated WebP, then decode and play it back.";

	public override string Category => SampleCategories.General;

	public override bool IsAnimated => _codec != null && _codec.FrameCount > 1;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("frames", "Frame Count", 2, 8, _frameCount, 1),
		new SliderControl("duration", "Frame Duration (ms)", 50, 1000, _frameDuration, 50),
		new SliderControl("quality", "Quality", 0, 100, _quality, 5),
		new PickerControl("compression", "Compression", CompressionOptions, _compressionIndex),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "frames":
				_frameCount = (int)(float)value;
				break;
			case "duration":
				_frameDuration = (int)(float)value;
				break;
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
		RebuildAnimation();
		return base.OnInit();
	}

	protected override async Task OnUpdate(CancellationToken token)
	{
		await Task.Delay(Math.Max(16, _frameDuration), token);
		if (_codec != null && _codec.FrameCount > 0)
			_currentFrame = (_currentFrame + 1) % _codec.FrameCount;
	}

	private void RebuildAnimation()
	{
		_currentFrame = 0;
		_currentBitmap?.Dispose();
		_currentBitmap = null;
		_codec?.Dispose();
		_codec = null;
		_encodedData?.Dispose();
		_encodedData = null;

		var size = 200;
		var count = Math.Clamp(_frameCount, 2, FrameColors.Length);

		var frames = new SKWebpEncoderFrame[count];
		var bitmaps = new SKBitmap[count];
		var pixmaps = new SKPixmap[count];

		try
		{
			for (var i = 0; i < count; i++)
			{
				bitmaps[i] = new SKBitmap(size, size);
				using var canvas = new SKCanvas(bitmaps[i]);
				DrawFrame(canvas, size, size, i, count);
				pixmaps[i] = bitmaps[i].PeekPixels();
				frames[i] = new SKWebpEncoderFrame(pixmaps[i], _frameDuration);
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
			}
		}
		finally
		{
			for (var i = 0; i < count; i++)
			{
				pixmaps[i]?.Dispose();
				bitmaps[i]?.Dispose();
			}
		}

		Refresh();
	}

	private static void DrawFrame(SKCanvas canvas, int width, int height, int frameIndex, int totalFrames)
	{
		var color = FrameColors[frameIndex % FrameColors.Length];
		canvas.Clear(SKColors.White);

		using var paint = new SKPaint
		{
			Color = color,
			IsAntialias = true,
			Style = SKPaintStyle.Fill,
		};

		var cx = width / 2f;
		var cy = height / 2f;
		var radius = Math.Min(width, height) * 0.35f;
		canvas.DrawCircle(cx, cy, radius, paint);

		using var textPaint = new SKPaint
		{
			Color = SKColors.White,
			IsAntialias = true,
		};
		using var font = new SKFont { Size = radius * 0.6f };
		var text = $"{frameIndex + 1}";
		canvas.DrawText(text, cx, cy + font.Size * 0.35f, SKTextAlign.Center, font, textPaint);
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		if (_codec == null || _encodedData == null || _currentBitmap == null)
		{
			using var errorPaint = new SKPaint { Color = SKColors.Red, IsAntialias = true };
			using var errorFont = new SKFont { Size = 18 };
			canvas.DrawText("Encoding failed", width / 2f, height / 2f, SKTextAlign.Center, errorFont, errorPaint);
			return;
		}

		var frameIndex = Math.Clamp(_currentFrame, 0, Math.Max(0, _codec.FrameCount - 1));
		var opts = new SKCodecOptions(frameIndex);
		_codec.GetPixels(_currentBitmap.Info, _currentBitmap.GetPixels(), opts);

		var scale = Math.Min((width - 20f) / _currentBitmap.Width, (height - 80f) / _currentBitmap.Height);
		scale = Math.Min(scale, 2f);
		var dw = _currentBitmap.Width * scale;
		var dh = _currentBitmap.Height * scale;
		var destRect = new SKRect(
			(width - dw) / 2f, 10,
			(width + dw) / 2f, 10 + dh);

		canvas.DrawBitmap(_currentBitmap, destRect);

		using var infoPaint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
		using var infoFont = new SKFont { Size = 14 };
		var infoY = destRect.Bottom + 24;
		var infoText = $"Frames: {_codec.FrameCount}  |  Size: {_encodedData.Size:N0} bytes  |  Frame: {frameIndex + 1}/{_codec.FrameCount}";
		canvas.DrawText(infoText, width / 2f, infoY, SKTextAlign.Center, infoFont, infoPaint);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_currentBitmap?.Dispose();
		_currentBitmap = null;
		_codec?.Dispose();
		_codec = null;
		_encodedData?.Dispose();
		_encodedData = null;
	}
}

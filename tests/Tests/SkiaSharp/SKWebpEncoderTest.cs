using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests;

public class SKWebpEncoderTest : SKTest
{
	private static SKBitmap CreateColorBitmap(SKColor color, int width = 40, int height = 40)
	{
		var bmp = new SKBitmap(width, height);
		bmp.Erase(color);
		return bmp;
	}

	[SkippableFact]
	public void EncodeAnimatedWebpReturnsData()
	{
		using var bmp1 = CreateColorBitmap(SKColors.Red);
		using var bmp2 = CreateColorBitmap(SKColors.Blue);
		using var pix1 = bmp1.PeekPixels();
		using var pix2 = bmp2.PeekPixels();

		SKWebpEncoderFrame[] frames =
		[
			new(pix1, TimeSpan.FromMilliseconds(100)),
			new(pix2, TimeSpan.FromMilliseconds(200)),
		];

		var data = SKWebpEncoder.EncodeAnimated(frames, SKWebpEncoderOptions.Default);

		Assert.NotNull(data);
		Assert.True(data.Size > 0);
	}

	[SkippableFact]
	public void EncodeAnimatedWebpToStream()
	{
		using var bmp1 = CreateColorBitmap(SKColors.Red);
		using var bmp2 = CreateColorBitmap(SKColors.Green);
		using var pix1 = bmp1.PeekPixels();
		using var pix2 = bmp2.PeekPixels();

		SKWebpEncoderFrame[] frames =
		[
			new(pix1, TimeSpan.FromMilliseconds(100)),
			new(pix2, TimeSpan.FromMilliseconds(200)),
		];

		using var stream = new MemoryStream();
		var result = SKWebpEncoder.EncodeAnimated(stream, frames, SKWebpEncoderOptions.Default);

		Assert.True(result);
		Assert.True(stream.Length > 0);
	}

	[SkippableFact]
	public void EncodeAnimatedWebpWithWStream()
	{
		using var bmp1 = CreateColorBitmap(SKColors.Red);
		using var bmp2 = CreateColorBitmap(SKColors.Blue);
		using var pix1 = bmp1.PeekPixels();
		using var pix2 = bmp2.PeekPixels();

		SKWebpEncoderFrame[] frames =
		[
			new(pix1, TimeSpan.FromMilliseconds(100)),
			new(pix2, TimeSpan.FromMilliseconds(200)),
		];

		using var wstream = new SKDynamicMemoryWStream();
		var result = SKWebpEncoder.EncodeAnimated(wstream, frames, SKWebpEncoderOptions.Default);

		Assert.True(result);

		using var data = wstream.DetachAsData();
		Assert.NotNull(data);
		Assert.True(data.Size > 0);
	}

	[SkippableFact]
	public void EncodeAnimatedWebpRoundTrip()
	{
		using var bmp1 = CreateColorBitmap(SKColors.Red);
		using var bmp2 = CreateColorBitmap(SKColors.Blue);
		using var pix1 = bmp1.PeekPixels();
		using var pix2 = bmp2.PeekPixels();

		SKWebpEncoderFrame[] frames =
		[
			new(pix1, TimeSpan.FromMilliseconds(150)),
			new(pix2, TimeSpan.FromMilliseconds(250)),
		];

		var data = SKWebpEncoder.EncodeAnimated(frames, SKWebpEncoderOptions.Default);
		Assert.NotNull(data);

		using var codec = SKCodec.Create(data);
		Assert.NotNull(codec);
		Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
		Assert.True(codec.FrameCount >= 2, $"Expected at least 2 frames, got {codec.FrameCount}");

		var frameInfo = codec.FrameInfo;
		Assert.True(frameInfo.Length >= 2, $"Expected at least 2 frame infos, got {frameInfo.Length}");
		Assert.Equal(150, frameInfo[0].Duration);
		Assert.Equal(250, frameInfo[1].Duration);
	}

	[SkippableFact]
	public void EncodeAnimatedWebpWithLosslessCompression()
	{
		using var bmp1 = CreateColorBitmap(SKColors.Red);
		using var bmp2 = CreateColorBitmap(SKColors.Green);
		using var pix1 = bmp1.PeekPixels();
		using var pix2 = bmp2.PeekPixels();

		SKWebpEncoderFrame[] frames =
		[
			new(pix1, TimeSpan.FromMilliseconds(100)),
			new(pix2, TimeSpan.FromMilliseconds(100)),
		];

		var options = new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossless, 75);
		var data = SKWebpEncoder.EncodeAnimated(frames, options);

		Assert.NotNull(data);

		using var codec = SKCodec.Create(data);
		Assert.NotNull(codec);
		Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
		Assert.True(codec.FrameCount >= 2, $"Expected at least 2 frames, got {codec.FrameCount}");
	}

	[SkippableFact]
	public void EncodeAnimatedWebpWithNullWStreamThrows()
	{
		using var bmp = CreateColorBitmap(SKColors.Red);
		using var pix = bmp.PeekPixels();

		SKWebpEncoderFrame[] frames = [new(pix, TimeSpan.FromMilliseconds(100))];

		Assert.Throws<ArgumentNullException>(() =>
			SKWebpEncoder.EncodeAnimated((SKWStream)null!, frames, SKWebpEncoderOptions.Default));
	}

	[SkippableFact]
	public void EncodeAnimatedWebpWithNullStreamThrows()
	{
		using var bmp = CreateColorBitmap(SKColors.Red);
		using var pix = bmp.PeekPixels();

		SKWebpEncoderFrame[] frames = [new(pix, TimeSpan.FromMilliseconds(100))];

		Assert.Throws<ArgumentNullException>(() =>
			SKWebpEncoder.EncodeAnimated((Stream)null!, frames, SKWebpEncoderOptions.Default));
	}

	[SkippableFact]
	public void EncodeAnimatedWebpFrameWithNullPixmapThrows()
	{
		Assert.Throws<ArgumentNullException>(() =>
			new SKWebpEncoderFrame((SKPixmap)null!, TimeSpan.FromMilliseconds(100)));
	}

	[SkippableFact]
	public void EncodeAnimatedWebpSingleFrame()
	{
		using var bmp = CreateColorBitmap(SKColors.Red);
		using var pix = bmp.PeekPixels();

		SKWebpEncoderFrame[] frames = [new(pix, TimeSpan.FromMilliseconds(500))];

		var data = SKWebpEncoder.EncodeAnimated(frames, SKWebpEncoderOptions.Default);
		Assert.NotNull(data);

		using var codec = SKCodec.Create(data);
		Assert.NotNull(codec);
		Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
	}

	[SkippableFact]
	public void EncodeAnimatedWebpFrameDurationPreserved()
	{
		using var bmp = CreateColorBitmap(SKColors.Red);
		using var pix = bmp.PeekPixels();

		var frame = new SKWebpEncoderFrame(pix, TimeSpan.FromMilliseconds(300));
		Assert.Equal(TimeSpan.FromMilliseconds(300), frame.Duration);
		Assert.Same(pix, frame.Pixmap);
	}

	[SkippableFact]
	public void EncodeAnimatedWebpPreservesImageDimensions()
	{
		using var bmp1 = CreateColorBitmap(SKColors.Red);
		using var bmp2 = CreateColorBitmap(SKColors.Blue);
		using var pix1 = bmp1.PeekPixels();
		using var pix2 = bmp2.PeekPixels();

		SKWebpEncoderFrame[] frames =
		[
			new(pix1, TimeSpan.FromMilliseconds(100)),
			new(pix2, TimeSpan.FromMilliseconds(100)),
		];

		var data = SKWebpEncoder.EncodeAnimated(frames, SKWebpEncoderOptions.Default);
		Assert.NotNull(data);

		using var codec = SKCodec.Create(data);
		Assert.NotNull(codec);
		Assert.Equal(bmp1.Width, codec.Info.Width);
		Assert.Equal(bmp1.Height, codec.Info.Height);
	}

	// Single-frame encoding via SKWebpEncoder

	[SkippableFact]
	public void EncodeSingleFrameReturnsData()
	{
		using var bmp = CreateColorBitmap(SKColors.Green);
		using var pix = bmp.PeekPixels();

		var data = SKWebpEncoder.Encode(pix, SKWebpEncoderOptions.Default);

		Assert.NotNull(data);
		Assert.True(data.Size > 0);

		using var codec = SKCodec.Create(data);
		Assert.NotNull(codec);
		Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
	}

	[SkippableFact]
	public void EncodeSingleFrameToStream()
	{
		using var bmp = CreateColorBitmap(SKColors.Blue);
		using var pix = bmp.PeekPixels();

		using var stream = new MemoryStream();
		var result = SKWebpEncoder.Encode(stream, pix, SKWebpEncoderOptions.Default);

		Assert.True(result);
		Assert.True(stream.Length > 0);
	}

	[SkippableFact]
	public void EncodeSingleFrameWithWStream()
	{
		using var bmp = CreateColorBitmap(SKColors.Red);
		using var pix = bmp.PeekPixels();

		using var wstream = new SKDynamicMemoryWStream();
		var result = SKWebpEncoder.Encode(wstream, pix, SKWebpEncoderOptions.Default);

		Assert.True(result);

		using var data = wstream.DetachAsData();
		Assert.NotNull(data);
		Assert.True(data.Size > 0);
	}

	[SkippableFact]
	public void EncodeSingleFrameWithNullPixmapThrows()
	{
		Assert.Throws<ArgumentNullException>(() =>
			SKWebpEncoder.Encode((SKPixmap)null!, SKWebpEncoderOptions.Default));
	}

	[SkippableFact]
	public void EncodeSingleFrameWithNullWStreamThrows()
	{
		using var bmp = CreateColorBitmap(SKColors.Red);
		using var pix = bmp.PeekPixels();

		Assert.Throws<ArgumentNullException>(() =>
			SKWebpEncoder.Encode((SKWStream)null!, pix, SKWebpEncoderOptions.Default));
	}

	[SkippableFact]
	public void EncodeSingleFrameWithNullStreamThrows()
	{
		using var bmp = CreateColorBitmap(SKColors.Red);
		using var pix = bmp.PeekPixels();

		Assert.Throws<ArgumentNullException>(() =>
			SKWebpEncoder.Encode((Stream)null!, pix, SKWebpEncoderOptions.Default));
	}

	// SKWebpEncoderFrame constructor overloads

	[SkippableFact]
	public void FrameFromBitmapEncodes()
	{
		using var bmp1 = CreateColorBitmap(SKColors.Red);
		using var bmp2 = CreateColorBitmap(SKColors.Blue);

		SKWebpEncoderFrame[] frames =
		[
			new(bmp1, TimeSpan.FromMilliseconds(100)),
			new(bmp2, TimeSpan.FromMilliseconds(200)),
		];

		var data = SKWebpEncoder.EncodeAnimated(frames, SKWebpEncoderOptions.Default);
		Assert.NotNull(data);

		using var codec = SKCodec.Create(data);
		Assert.NotNull(codec);
		Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
		Assert.True(codec.FrameCount >= 2);
	}

	[SkippableFact]
	public void FrameFromImageEncodes()
	{
		using var bmp1 = CreateColorBitmap(SKColors.Green);
		using var bmp2 = CreateColorBitmap(SKColors.Yellow);
		using var img1 = SKImage.FromBitmap(bmp1);
		using var img2 = SKImage.FromBitmap(bmp2);

		SKWebpEncoderFrame[] frames =
		[
			new(img1, TimeSpan.FromMilliseconds(150)),
			new(img2, TimeSpan.FromMilliseconds(250)),
		];

		var data = SKWebpEncoder.EncodeAnimated(frames, SKWebpEncoderOptions.Default);
		Assert.NotNull(data);

		using var codec = SKCodec.Create(data);
		Assert.NotNull(codec);
		Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
		Assert.True(codec.FrameCount >= 2);
	}

	[SkippableFact]
	public void FrameFromNullBitmapThrows()
	{
		Assert.Throws<ArgumentNullException>(() =>
			new SKWebpEncoderFrame((SKBitmap)null!, TimeSpan.FromMilliseconds(100)));
	}

	[SkippableFact]
	public void FrameFromNullImageThrows()
	{
		Assert.Throws<ArgumentNullException>(() =>
			new SKWebpEncoderFrame((SKImage)null!, TimeSpan.FromMilliseconds(100)));
	}

	// Full round-trip: encode → decode → validate pixels and durations

	[SkippableFact]
	public void FullRoundTripValidatesPixelsAndDurations()
	{
		var size = 40;
		SKColor[] colors = [SKColors.Red, SKColors.Green, SKColors.Blue];
		int[] durationsMs = [100, 200, 300];

		// encode
		var srcBitmaps = new SKBitmap[colors.Length];
		var frames = new SKWebpEncoderFrame[colors.Length];
		for (var i = 0; i < colors.Length; i++)
		{
			srcBitmaps[i] = CreateColorBitmap(colors[i], size, size);
			frames[i] = new SKWebpEncoderFrame(srcBitmaps[i], TimeSpan.FromMilliseconds(durationsMs[i]));
		}

		var options = new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossless, 75);
		var data = SKWebpEncoder.EncodeAnimated(frames, options);
		Assert.NotNull(data);

		// decode
		using var codec = SKCodec.Create(data);
		Assert.NotNull(codec);
		Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
		Assert.Equal(colors.Length, codec.FrameCount);
		Assert.Equal(size, codec.Info.Width);
		Assert.Equal(size, codec.Info.Height);

		// validate each frame's duration and pixel content
		var frameInfos = codec.FrameInfo;
		Assert.Equal(colors.Length, frameInfos.Length);

		for (var i = 0; i < colors.Length; i++)
		{
			// duration
			Assert.Equal(durationsMs[i], frameInfos[i].Duration);

			// decode frame pixels
			var info = new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var decoded = new SKBitmap(info);
			var result = codec.GetPixels(info, decoded.GetPixels(), new SKCodecOptions(i));
			Assert.Equal(SKCodecResult.Success, result);

			// compare center pixel to expected color (lossless should be exact)
			var actual = decoded.GetPixel(size / 2, size / 2);
			Assert.Equal(colors[i].Red, actual.Red);
			Assert.Equal(colors[i].Green, actual.Green);
			Assert.Equal(colors[i].Blue, actual.Blue);
		}

		// cleanup source bitmaps
		foreach (var bmp in srcBitmaps)
			bmp.Dispose();
	}

	[SkippableFact]
	public void LossyRoundTripPreservesApproximateColors()
	{
		var size = 40;
		SKColor[] colors = [SKColors.Red, SKColors.Blue];
		int[] durationsMs = [150, 250];

		var frames = new SKWebpEncoderFrame[colors.Length];
		var srcBitmaps = new SKBitmap[colors.Length];
		for (var i = 0; i < colors.Length; i++)
		{
			srcBitmaps[i] = CreateColorBitmap(colors[i], size, size);
			frames[i] = new SKWebpEncoderFrame(srcBitmaps[i], TimeSpan.FromMilliseconds(durationsMs[i]));
		}

		var options = new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, 100);
		var data = SKWebpEncoder.EncodeAnimated(frames, options);
		Assert.NotNull(data);

		using var codec = SKCodec.Create(data);
		Assert.NotNull(codec);
		Assert.Equal(colors.Length, codec.FrameCount);

		var frameInfos = codec.FrameInfo;
		for (var i = 0; i < colors.Length; i++)
		{
			Assert.Equal(durationsMs[i], frameInfos[i].Duration);

			var info = new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var decoded = new SKBitmap(info);
			codec.GetPixels(info, decoded.GetPixels(), new SKCodecOptions(i));

			// lossy at q100 should be close but not necessarily exact
			var actual = decoded.GetPixel(size / 2, size / 2);
			Assert.InRange(actual.Red, (byte)Math.Max(0, colors[i].Red - 10), (byte)Math.Min(255, colors[i].Red + 10));
			Assert.InRange(actual.Green, (byte)Math.Max(0, colors[i].Green - 10), (byte)Math.Min(255, colors[i].Green + 10));
			Assert.InRange(actual.Blue, (byte)Math.Max(0, colors[i].Blue - 10), (byte)Math.Min(255, colors[i].Blue + 10));
		}

		foreach (var bmp in srcBitmaps)
			bmp.Dispose();
	}
}

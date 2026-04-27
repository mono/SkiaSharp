using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
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

			var frames = new SKWebpEncoderFrame[]
			{
				new SKWebpEncoderFrame(pix1, 100),
				new SKWebpEncoderFrame(pix2, 200),
			};

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

			var frames = new SKWebpEncoderFrame[]
			{
				new SKWebpEncoderFrame(pix1, 100),
				new SKWebpEncoderFrame(pix2, 200),
			};

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

			var frames = new SKWebpEncoderFrame[]
			{
				new SKWebpEncoderFrame(pix1, 100),
				new SKWebpEncoderFrame(pix2, 200),
			};

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

			var frames = new SKWebpEncoderFrame[]
			{
				new SKWebpEncoderFrame(pix1, 150),
				new SKWebpEncoderFrame(pix2, 250),
			};

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

			var frames = new SKWebpEncoderFrame[]
			{
				new SKWebpEncoderFrame(pix1, 100),
				new SKWebpEncoderFrame(pix2, 100),
			};

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

			var frames = new SKWebpEncoderFrame[]
			{
				new SKWebpEncoderFrame(pix, 100),
			};

			Assert.Throws<ArgumentNullException>(() =>
				SKWebpEncoder.EncodeAnimated((SKWStream)null!, frames, SKWebpEncoderOptions.Default));
		}

		[SkippableFact]
		public void EncodeAnimatedWebpWithNullStreamThrows()
		{
			using var bmp = CreateColorBitmap(SKColors.Red);
			using var pix = bmp.PeekPixels();

			var frames = new SKWebpEncoderFrame[]
			{
				new SKWebpEncoderFrame(pix, 100),
			};

			Assert.Throws<ArgumentNullException>(() =>
				SKWebpEncoder.EncodeAnimated((Stream)null!, frames, SKWebpEncoderOptions.Default));
		}

		[SkippableFact]
		public void EncodeAnimatedWebpFrameWithNullPixmapThrows()
		{
			Assert.Throws<ArgumentNullException>(() =>
				new SKWebpEncoderFrame(null!, 100));
		}

		[SkippableFact]
		public void EncodeAnimatedWebpSingleFrame()
		{
			using var bmp = CreateColorBitmap(SKColors.Red);
			using var pix = bmp.PeekPixels();

			var frames = new SKWebpEncoderFrame[]
			{
				new SKWebpEncoderFrame(pix, 500),
			};

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

			var frame = new SKWebpEncoderFrame(pix, 300);
			Assert.Equal(300, frame.Duration);
			Assert.Same(pix, frame.Pixmap);
		}

		[SkippableFact]
		public void EncodeAnimatedWebpPreservesImageDimensions()
		{
			using var bmp1 = CreateColorBitmap(SKColors.Red);
			using var bmp2 = CreateColorBitmap(SKColors.Blue);
			using var pix1 = bmp1.PeekPixels();
			using var pix2 = bmp2.PeekPixels();

			var frames = new SKWebpEncoderFrame[]
			{
				new SKWebpEncoderFrame(pix1, 100),
				new SKWebpEncoderFrame(pix2, 100),
			};

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
	}
}

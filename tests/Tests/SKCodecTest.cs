using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKCodecTest : SKTest
	{
		[Test]
		public void MinBufferedBytesNeededHasAValue ()
		{
			Assert.True (SKCodec.MinBufferedBytesNeeded > 0);
		}

		[Test]
		public void CanCreateStreamCodec ()
		{
			var stream = new SKFileStream (Path.Combine (PathToImages, "color-wheel.png"));
			using (var codec = SKCodec.Create (stream)) {
				Assert.AreEqual (SKEncodedImageFormat.Png, codec.EncodedFormat);
				Assert.AreEqual (128, codec.Info.Width);
				Assert.AreEqual (128, codec.Info.Height);
				Assert.AreEqual (SKAlphaType.Unpremul, codec.Info.AlphaType);
				Assert.AreEqual (SKImageInfo.PlatformColorType, codec.Info.ColorType);
			}
		}
		
		[Test]
		public void GetGifFrames ()
		{
			const int FrameCount = 16;

			var stream = new SKFileStream (Path.Combine (PathToImages, "animated-heart.gif"));
			using (var codec = SKCodec.Create (stream)) {
				Assert.AreEqual (-1, codec.RepetitionCount);

				var frameInfos = codec.FrameInfo;
				Assert.AreEqual (FrameCount, frameInfos.Length);

				Assert.AreEqual (-1, frameInfos [0].RequiredFrame);

				var cachedFrames = new SKBitmap [FrameCount];
				var info = new SKImageInfo (codec.Info.Width, codec.Info.Height);

				var decode = new Action<SKBitmap, bool, int> ((bm, cached, index) => {
					if (cached) {
						var requiredFrame = frameInfos [index].RequiredFrame;
						if (requiredFrame != -1) {
							Assert.True (cachedFrames [requiredFrame].CopyTo (bm));
						}
					}
					var opts = new SKCodecOptions (index, cached);
					var result = codec.GetPixels (info, bm.GetPixels (), opts);
					Assert.AreEqual (SKCodecResult.Success, result);
				});

				for (var i = 0; i < FrameCount; i++) {
					var cachedFrame = cachedFrames [i] = new SKBitmap (info);
					decode (cachedFrame, true, i);

					var uncachedFrame = new SKBitmap (info);
					decode (uncachedFrame, false, i);

					Assert.AreEqual (cachedFrame.Bytes, uncachedFrame.Bytes);
				}
			}
		}

		[Test]
		public void GetEncodedInfo ()
		{
			var stream = new SKFileStream (Path.Combine (PathToImages, "color-wheel.png"));
			using (var codec = SKCodec.Create (stream)) {
				Assert.AreEqual (SKEncodedInfoColor.Rgba, codec.EncodedInfo.Color);
				Assert.AreEqual (SKEncodedInfoAlpha.Unpremul, codec.EncodedInfo.Alpha);
				Assert.AreEqual (8, codec.EncodedInfo.BitsPerComponent);
			}
		}

		[Test]
		public void CanGetPixels ()
		{
			var stream = new SKFileStream (Path.Combine (PathToImages, "baboon.png"));
			using (var codec = SKCodec.Create (stream)) {
				var pixels = codec.Pixels;
				Assert.AreEqual (codec.Info.BytesSize, pixels.Length);
			}
		}

		[Test]
		public void DecodeImageScanlines ()
		{
			var path = Path.Combine (PathToImages, "CMYK.jpg");
			var imageHeight = 516;

			var fileData = File.ReadAllBytes (path);
			var correctBitmap = SKBitmap.Decode (path);

			var stream = new SKFileStream (path);
			using (var codec = SKCodec.Create (stream)) {
				var info = new SKImageInfo (codec.Info.Width, codec.Info.Height);
				using (var scanlineBitmap = new SKBitmap (info)) {
					scanlineBitmap.Erase (SKColors.Fuchsia);

					var result = codec.StartScanlineDecode (info);
					Assert.AreEqual (SKCodecResult.Success, result);

					Assert.AreEqual (SKCodecScanlineOrder.TopDown, codec.ScanlineOrder);
					Assert.AreEqual (0, codec.NextScanline);

					// only decode every second line
					for	(int y = 0; y < info.Height; y += 2) {
						Assert.AreEqual (1, codec.GetScanlines (scanlineBitmap.GetAddr (0, y), 1, info.RowBytes));
						Assert.AreEqual (y + 1, codec.NextScanline);
						if (codec.SkipScanlines (1))
							Assert.AreEqual (y + 2, codec.NextScanline);
						else
							Assert.AreEqual (imageHeight, codec.NextScanline); // reached the end
					}

					Assert.False (codec.SkipScanlines (1));
					Assert.AreEqual (imageHeight, codec.NextScanline);

					for (var x = 0; x < info.Width; x++) {
						for (var y = 0; y < info.Height; y++) {
							if (y % 2 == 0)
								Assert.AreEqual (correctBitmap.GetPixel (x, y), scanlineBitmap.GetPixel (x, y));
							else
								Assert.AreEqual (SKColors.Fuchsia, scanlineBitmap.GetPixel (x, y));
						}
					}
				}
			}
		}

		[Test]
		public void DecodePartialImage ()
		{
			// read the data here, so we can fake a throttle/download
			var path = Path.Combine (PathToImages, "baboon.png");
			var fileData = File.ReadAllBytes (path);
			SKColor[] correctBytes;
			using (var bitmap = SKBitmap.Decode (path)) {
				correctBytes = bitmap.Pixels;
			}

			int offset = 0;
			int maxCount = 1024 * 4;

			// the "download" stream needs some data
			var downloadStream = new MemoryStream ();
			downloadStream.Write (fileData, offset, maxCount);
			downloadStream.Position -= maxCount;
			offset += maxCount;

			using (var codec = SKCodec.Create (new SKManagedStream (downloadStream))) {
				var info = new SKImageInfo (codec.Info.Width, codec.Info.Height);

				// the bitmap to be decoded
				using (var incremental = new SKBitmap (info)) {

					// start decoding
					IntPtr length;
					var pixels = incremental.GetPixels (out length);
					var result = codec.StartIncrementalDecode (info, pixels, info.RowBytes);

					// make sure the start was successful
					Assert.AreEqual (SKCodecResult.Success, result);
					result = SKCodecResult.IncompleteInput;

					while (result == SKCodecResult.IncompleteInput) {
						// decode the rest
						int rowsDecoded = 0;
						result = codec.IncrementalDecode (out rowsDecoded);

						// write some more data to the stream
						maxCount = Math.Min (maxCount, fileData.Length - offset);
						downloadStream.Write (fileData, offset, maxCount);
						downloadStream.Position -= maxCount;
						offset += maxCount;
					}

					// compare to original
					Assert.AreEqual (correctBytes, incremental.Pixels);
				}
			}
		}

		[Test]
		public void BitmapDecodesCorrectly ()
		{
			byte[] codecPixels;
			byte[] bitmapPixels;

			using (var codec = SKCodec.Create (new SKFileStream (Path.Combine (PathToImages, "baboon.png")))) {
				codecPixels = codec.Pixels;
			}

			using (var bitmap = SKBitmap.Decode (Path.Combine (PathToImages, "baboon.png"))) {
				bitmapPixels = bitmap.Bytes;
			}

			Assert.AreEqual (codecPixels, bitmapPixels);
		}

		[Test]
		public void BitmapDecodesCorrectlyWithManagedStream ()
		{
			byte[] codecPixels;
			byte[] bitmapPixels;

			var stream = File.OpenRead (Path.Combine(PathToImages, "baboon.png"));
			using (var codec = SKCodec.Create (new SKManagedStream (stream))) {
				codecPixels = codec.Pixels;
			}

			using (var bitmap = SKBitmap.Decode (Path.Combine (PathToImages, "baboon.png"))) {
				bitmapPixels = bitmap.Bytes;
			}

			Assert.AreEqual (codecPixels, bitmapPixels);
		}
	}
}

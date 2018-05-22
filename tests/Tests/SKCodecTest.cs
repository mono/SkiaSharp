using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKCodecTest : SKTest
	{
		[SkippableFact]
		public void MinBufferedBytesNeededHasAValue ()
		{
			Assert.True (SKCodec.MinBufferedBytesNeeded > 0);
		}

		[SkippableFact]
		public void CanCreateStreamCodec ()
		{
			var stream = new SKFileStream (Path.Combine (PathToImages, "color-wheel.png"));
			using (var codec = SKCodec.Create (stream)) {
				Assert.Equal (SKEncodedImageFormat.Png, codec.EncodedFormat);
				Assert.Equal (128, codec.Info.Width);
				Assert.Equal (128, codec.Info.Height);
				Assert.Equal (SKAlphaType.Unpremul, codec.Info.AlphaType);
				Assert.Equal (SKImageInfo.PlatformColorType, codec.Info.ColorType);
			}
		}
		
		[SkippableFact]
		public void GetGifFrames ()
		{
			const int FrameCount = 16;

			var stream = new SKFileStream (Path.Combine (PathToImages, "animated-heart.gif"));
			using (var codec = SKCodec.Create (stream)) {
				Assert.Equal (-1, codec.RepetitionCount);

				var frameInfos = codec.FrameInfo;
				Assert.Equal (FrameCount, frameInfos.Length);

				Assert.Equal (-1, frameInfos [0].RequiredFrame);

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
					Assert.Equal (SKCodecResult.Success, result);
				});

				for (var i = 0; i < FrameCount; i++) {
					var cachedFrame = cachedFrames [i] = new SKBitmap (info);
					decode (cachedFrame, true, i);

					var uncachedFrame = new SKBitmap (info);
					decode (uncachedFrame, false, i);

					Assert.Equal (cachedFrame.Bytes, uncachedFrame.Bytes);
				}
			}
		}

		[SkippableFact]
		public void GetEncodedInfo ()
		{
			var stream = new SKFileStream (Path.Combine (PathToImages, "color-wheel.png"));
			using (var codec = SKCodec.Create (stream)) {
				Assert.Equal (SKEncodedInfoColor.Rgba, codec.EncodedInfo.Color);
				Assert.Equal (SKEncodedInfoAlpha.Unpremul, codec.EncodedInfo.Alpha);
				Assert.Equal (8, codec.EncodedInfo.BitsPerComponent);
			}
		}

		[SkippableFact]
		public void CanGetPixels ()
		{
			var stream = new SKFileStream (Path.Combine (PathToImages, "baboon.png"));
			using (var codec = SKCodec.Create (stream)) {
				var pixels = codec.Pixels;
				Assert.Equal (codec.Info.BytesSize, pixels.Length);
			}
		}

		[SkippableFact]
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
					Assert.Equal (SKCodecResult.Success, result);

					Assert.Equal (SKCodecScanlineOrder.TopDown, codec.ScanlineOrder);
					Assert.Equal (0, codec.NextScanline);

					// only decode every second line
					for	(int y = 0; y < info.Height; y += 2) {
						Assert.Equal (1, codec.GetScanlines (scanlineBitmap.GetAddr (0, y), 1, info.RowBytes));
						Assert.Equal (y + 1, codec.NextScanline);
						if (codec.SkipScanlines (1))
							Assert.Equal (y + 2, codec.NextScanline);
						else
							Assert.Equal (imageHeight, codec.NextScanline); // reached the end
					}

					Assert.False (codec.SkipScanlines (1));
					Assert.Equal (imageHeight, codec.NextScanline);

					for (var x = 0; x < info.Width; x++) {
						for (var y = 0; y < info.Height; y++) {
							if (y % 2 == 0)
								Assert.Equal (correctBitmap.GetPixel (x, y), scanlineBitmap.GetPixel (x, y));
							else
								Assert.Equal (SKColors.Fuchsia, scanlineBitmap.GetPixel (x, y));
						}
					}
				}
			}
		}

		[SkippableFact]
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
					Assert.Equal (SKCodecResult.Success, result);
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
					Assert.Equal (correctBytes, incremental.Pixels);
				}
			}
		}

		[SkippableFact]
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

			Assert.Equal (codecPixels, bitmapPixels);
		}

		[SkippableFact]
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

			Assert.Equal (codecPixels, bitmapPixels);
		}
	
		[SkippableFact (Skip = "This keeps breaking CI for some reason.")]
		public async Task DownloadedStream ()
		{
			var httpClient = new HttpClient ();
			using (var stream = await httpClient.GetStreamAsync (new Uri ("http://www.gstatic.com/webp/gallery/2.webp")))
			using (var bitmap = SKBitmap.Decode (stream))
				Assert.NotNull (bitmap);
		}
	
		[SkippableFact]
		public void ReadOnlyStream ()
		{
			using (var stream = File.OpenRead (Path.Combine (PathToImages, "baboon.png")))
			using (var nonSeekable = new NonSeekableReadOnlyStream (stream))
			using (var bitmap = SKBitmap.Decode (nonSeekable))
				Assert.NotNull (bitmap);
		}
	}
}

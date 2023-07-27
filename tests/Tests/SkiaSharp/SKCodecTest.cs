using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKCodecTest : SKTest
	{
		[SkippableFact]
		public void MinBufferedBytesNeededHasAValue()
		{
			Assert.True(SKCodec.MinBufferedBytesNeeded > 0);
		}

		[SkippableFact]
		public unsafe void ImageCanBeDecodedManyTimes()
		{
			var codec = SKCodec.Create(Path.Combine(PathToImages, "color-wheel.png"));

			for (var i = 0; i < 1000; ++i)
			{
				Assert.Equal(SKCodecResult.Success, codec.GetPixels(out _));
			}
		}

		[SkippableTheory]
		[InlineData("P8211052.JPG", SKEncodedOrigin.LeftBottom)]
		[InlineData("PA010741.JPG", SKEncodedOrigin.LeftBottom)]
		public void CodecCanLoadCorrectOrigin(string image, SKEncodedOrigin origin)
		{
			var codec = SKCodec.Create(Path.Combine(PathToImages, image));

			Assert.Equal(origin, codec.EncodedOrigin);
		}

		[SkippableFact]
		public unsafe void ReleaseDataWasInvokedOnlyAfterTheCodecWasFinished()
		{
			var path = Path.Combine(PathToImages, "color-wheel.png");
			var bytes = File.ReadAllBytes(path);

			var released = false;

			fixed (byte* b = bytes)
			{
				var data = SKData.Create((IntPtr)b, bytes.Length, (addr, ctx) => released = true);

				var codec = SKCodec.Create(data);
				Assert.NotEqual(SKImageInfo.Empty, codec.Info);

				data.Dispose();
				Assert.False(released, "The SKDataReleaseDelegate was called too soon.");

				codec.Dispose();
				Assert.True(released, "The SKDataReleaseDelegate was not called at all.");
			}
		}

		[SkippableFact]
		public unsafe void StreamLosesOwnershipToCodecButIsNotForgotten()
		{
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));
			var stream = new SKMemoryStream(bytes);
			var handle = stream.Handle;

			Assert.True(stream.OwnsHandle);
			Assert.True(SKObject.GetInstance<SKMemoryStream>(handle, out _));

			var codec = SKCodec.Create(stream);
			Assert.False(stream.OwnsHandle);

			stream.Dispose();
			Assert.True(SKObject.GetInstance<SKMemoryStream>(handle, out _));

			Assert.Equal(SKCodecResult.Success, codec.GetPixels(out var pixels));
			Assert.NotEmpty(pixels);
		}

		[SkippableFact]
		public unsafe void StreamLosesOwnershipAndCanBeDisposedButIsNotActually()
		{
			var path = Path.Combine(PathToImages, "color-wheel.png");
			var stream = new SKMemoryStream(File.ReadAllBytes(path));
			var handle = stream.Handle;

			Assert.True(stream.OwnsHandle);
			Assert.False(stream.IgnorePublicDispose);
			Assert.True(SKObject.GetInstance<SKMemoryStream>(handle, out _));

			var codec = SKCodec.Create(stream);
			Assert.False(stream.OwnsHandle);
			Assert.True(stream.IgnorePublicDispose);

			stream.Dispose();
			Assert.True(SKObject.GetInstance<SKMemoryStream>(handle, out var inst));
			Assert.Same(stream, inst);

			Assert.Equal(SKCodecResult.Success, codec.GetPixels(out var pixels));
			Assert.NotEmpty(pixels);

			codec.Dispose();
			Assert.False(SKObject.GetInstance<SKMemoryStream>(handle, out _));
		}

		[SkippableFact]
		public unsafe void InvalidStreamIsDisposedImmediately()
		{
			var stream = CreateTestSKStream();
			var handle = stream.Handle;

			Assert.True(stream.OwnsHandle);
			Assert.False(stream.IgnorePublicDispose);
			Assert.True(SKObject.GetInstance<SKStream>(handle, out _));

			Assert.Null(SKCodec.Create(stream));

			Assert.False(stream.OwnsHandle);
			Assert.True(stream.IgnorePublicDispose);
			Assert.False(SKObject.GetInstance<SKStream>(handle, out _));
		}

		[SkippableFact]
		public unsafe void StreamLosesOwnershipAndCanBeGarbageCollected()
		{
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			DoWork(out var codecH, out var streamH);

			CollectGarbage();

			Assert.False(SKObject.GetInstance<SKMemoryStream>(streamH, out _));
			Assert.False(SKObject.GetInstance<SKCodec>(codecH, out _));

			void DoWork(out IntPtr codecHandle, out IntPtr streamHandle)
			{
				var codec = CreateCodec(out streamHandle);
				codecHandle = codec.Handle;

				CollectGarbage();

				Assert.Equal(SKCodecResult.Success, codec.GetPixels(out var pixels));
				Assert.NotEmpty(pixels);

				Assert.True(SKObject.GetInstance<SKMemoryStream>(streamHandle, out var stream));
				Assert.False(stream.OwnsHandle);
				Assert.True(stream.IgnorePublicDispose);
			}

			SKCodec CreateCodec(out IntPtr streamHandle)
			{
				var stream = new SKMemoryStream(bytes);
				streamHandle = stream.Handle;

				Assert.True(stream.OwnsHandle);
				Assert.False(stream.IgnorePublicDispose);
				Assert.True(SKObject.GetInstance<SKMemoryStream>(streamHandle, out _));

				return SKCodec.Create(stream);
			}
		}

		[SkippableFact]
		public void CanCreateStreamCodec()
		{
			var stream = new SKFileStream(Path.Combine(PathToImages, "color-wheel.png"));
			Assert.True(stream.IsValid);

			using var codec = SKCodec.Create(stream);

			Assert.Equal(SKEncodedImageFormat.Png, codec.EncodedFormat);
			Assert.Equal(128, codec.Info.Width);
			Assert.Equal(128, codec.Info.Height);
			Assert.Equal(SKAlphaType.Unpremul, codec.Info.AlphaType);
			Assert.Equal(SKImageInfo.PlatformColorType, codec.Info.ColorType);
		}

		[SkippableFact]
		public void CanCreateStreamCodecWithResult()
		{
			var stream = new SKFileStream(Path.Combine(PathToImages, "color-wheel.png"));
			Assert.True(stream.IsValid);

			using var codec = SKCodec.Create(stream, out var result);

			Assert.Equal(SKCodecResult.Success, result);
			Assert.Equal(SKEncodedImageFormat.Png, codec.EncodedFormat);
			Assert.Equal(128, codec.Info.Width);
			Assert.Equal(128, codec.Info.Height);
			Assert.Equal(SKAlphaType.Unpremul, codec.Info.AlphaType);
			Assert.Equal(SKImageInfo.PlatformColorType, codec.Info.ColorType);
		}

		[SkippableFact]
		public void GetGifFrames()
		{
			const int FrameCount = 16;

			var stream = new SKFileStream(Path.Combine(PathToImages, "animated-heart.gif"));
			using (var codec = SKCodec.Create(stream))
			{
				Assert.Equal(-1, codec.RepetitionCount);

				var frameInfos = codec.FrameInfo;
				Assert.Equal(FrameCount, frameInfos.Length);

				Assert.Equal(-1, frameInfos[0].RequiredFrame);

				var cachedFrames = new SKBitmap[FrameCount];
				var info = new SKImageInfo(codec.Info.Width, codec.Info.Height);

				var decode = new Action<SKBitmap, int, int>((bm, cachedIndex, index) =>
				{
					var decodeInfo = info;
					if (index > 0)
					{
						decodeInfo = info.WithAlphaType(frameInfos[index].AlphaType);
					}
					Assert.True(bm.TryAllocPixels(decodeInfo));
					if (cachedIndex != -1)
					{
						Assert.True(cachedFrames[cachedIndex].CopyTo(bm));
					}
					var opts = new SKCodecOptions(index, cachedIndex);
					var result = codec.GetPixels(decodeInfo, bm.GetPixels(), opts);
					if (cachedIndex != -1 && frameInfos[cachedIndex].DisposalMethod == SKCodecAnimationDisposalMethod.RestorePrevious)
					{
						Assert.Equal(SKCodecResult.InvalidParameters, result);
					}
					Assert.Equal(SKCodecResult.Success, result);
				});

				for (var i = 0; i < FrameCount; i++)
				{
					var cachedFrame = cachedFrames[i] = new SKBitmap();
					decode(cachedFrame, -1, i);

					var uncachedFrame = new SKBitmap();
					decode(uncachedFrame, frameInfos[i].RequiredFrame, i);

					var cachedBytes = cachedFrame.Bytes;
					var uncachedBytes = uncachedFrame.Bytes;
					Assert.Equal(cachedBytes, uncachedBytes);
				}
			}
		}

		[SkippableFact]
		public void GetSingleGifFrame()
		{
			var stream = new SKFileStream(Path.Combine(PathToImages, "animated-heart.gif"));
			using (var codec = SKCodec.Create(stream))
			{
				var frameInfos = codec.FrameInfo;
				for (var i = 0; i < frameInfos.Length; i++)
				{
					Assert.True(codec.GetFrameInfo(i, out var info));
					Assert.Equal(frameInfos[i], info);
				}
			}
		}

		[SkippableFact]
		public void GetEncodedInfo()
		{
			var stream = new SKFileStream(Path.Combine(PathToImages, "color-wheel.png"));
			using (var codec = SKCodec.Create(stream))
			{
				Assert.Equal(SKImageInfo.PlatformColorType, codec.Info.ColorType);
				Assert.Equal(SKAlphaType.Unpremul, codec.Info.AlphaType);
				Assert.Equal(32, codec.Info.BitsPerPixel);
			}
		}

		[SkippableFact]
		public void CanGetPixels()
		{
			var stream = new SKFileStream(Path.Combine(PathToImages, "baboon.png"));
			using (var codec = SKCodec.Create(stream))
			{
				var pixels = codec.Pixels;
				Assert.Equal(codec.Info.BytesSize, pixels.Length);
			}
		}

		[SkippableFact]
		public void DecodeImageScanlines()
		{
			var path = Path.Combine(PathToImages, "CMYK.jpg");
			var imageHeight = 516;

			var fileData = File.ReadAllBytes(path);
			var correctBitmap = SKBitmap.Decode(path);

			var stream = new SKFileStream(path);
			using (var codec = SKCodec.Create(stream))
			{
				var info = new SKImageInfo(codec.Info.Width, codec.Info.Height);
				using (var scanlineBitmap = new SKBitmap(info))
				{
					scanlineBitmap.Erase(SKColors.Fuchsia);

					var result = codec.StartScanlineDecode(info);
					Assert.Equal(SKCodecResult.Success, result);

					Assert.Equal(SKCodecScanlineOrder.TopDown, codec.ScanlineOrder);
					Assert.Equal(0, codec.NextScanline);

					// only decode every second line
					for (int y = 0; y < info.Height; y += 2)
					{
						Assert.Equal(1, codec.GetScanlines(scanlineBitmap.GetAddress(0, y), 1, info.RowBytes));
						Assert.Equal(y + 1, codec.NextScanline);
						if (codec.SkipScanlines(1))
							Assert.Equal(y + 2, codec.NextScanline);
						else
							Assert.Equal(imageHeight, codec.NextScanline); // reached the end
					}

					Assert.False(codec.SkipScanlines(1));
					Assert.Equal(imageHeight, codec.NextScanline);

					for (var x = 0; x < info.Width; x++)
					{
						for (var y = 0; y < info.Height; y++)
						{
							if (y % 2 == 0)
								Assert.Equal(correctBitmap.GetPixel(x, y), scanlineBitmap.GetPixel(x, y));
							else
								Assert.Equal(SKColors.Fuchsia, scanlineBitmap.GetPixel(x, y));
						}
					}
				}
			}
		}

		[SkippableFact]
		public void DecodePartialImage()
		{
			// read the data here, so we can fake a throttle/download
			var path = Path.Combine(PathToImages, "baboon.png");
			var fileData = File.ReadAllBytes(path);
			SKColor[] correctBytes;
			using (var bitmap = SKBitmap.Decode(path))
			{
				correctBytes = bitmap.Pixels;
			}

			int offset = 0;
			int maxCount = 1024 * 4;

			// the "download" stream needs some data
			var downloadStream = new MemoryStream();
			downloadStream.Write(fileData, offset, maxCount);
			downloadStream.Position -= maxCount;
			offset += maxCount;

			using (var codec = SKCodec.Create(new SKManagedStream(downloadStream)))
			{
				var info = new SKImageInfo(codec.Info.Width, codec.Info.Height);

				// the bitmap to be decoded
				using (var incremental = new SKBitmap(info))
				{

					// start decoding
					IntPtr length;
					var pixels = incremental.GetPixels(out length);
					var result = codec.StartIncrementalDecode(info, pixels, info.RowBytes);

					// make sure the start was successful
					Assert.Equal(SKCodecResult.Success, result);
					result = SKCodecResult.IncompleteInput;

					while (result == SKCodecResult.IncompleteInput)
					{
						// decode the rest
						int rowsDecoded = 0;
						result = codec.IncrementalDecode(out rowsDecoded);

						// write some more data to the stream
						maxCount = Math.Min(maxCount, fileData.Length - offset);
						downloadStream.Write(fileData, offset, maxCount);
						downloadStream.Position -= maxCount;
						offset += maxCount;
					}

					// compare to original
					Assert.Equal(correctBytes, incremental.Pixels);
				}
			}
		}

		[SkippableFact]
		public void BitmapDecodesCorrectly()
		{
			byte[] codecPixels;
			byte[] bitmapPixels;

			using (var codec = SKCodec.Create(new SKFileStream(Path.Combine(PathToImages, "baboon.png"))))
			{
				codecPixels = codec.Pixels;
			}

			using (var bitmap = SKBitmap.Decode(Path.Combine(PathToImages, "baboon.png")))
			{
				bitmapPixels = bitmap.Bytes;
			}

			Assert.Equal(codecPixels, bitmapPixels);
		}

		[SkippableFact]
		public void BitmapDecodesCorrectlyWithManagedStream()
		{
			byte[] codecPixels;
			byte[] bitmapPixels;

			var stream = File.OpenRead(Path.Combine(PathToImages, "baboon.png"));
			using (var codec = SKCodec.Create(new SKManagedStream(stream)))
			{
				codecPixels = codec.Pixels;
			}

			using (var bitmap = SKBitmap.Decode(Path.Combine(PathToImages, "baboon.png")))
			{
				bitmapPixels = bitmap.Bytes;
			}

			Assert.Equal(codecPixels, bitmapPixels);
		}

		[SkippableFact]
		public void CanReadManagedStream()
		{
			using (var stream = File.OpenRead(Path.Combine(PathToImages, "baboon.png")))
			using (var codec = SKCodec.Create(stream))
				Assert.NotNull(codec);
		}

		[SkippableFact(Skip = "This keeps breaking CI for some reason.")]
		public async Task DownloadedStream()
		{
			var httpClient = new HttpClient();
			using (var stream = await httpClient.GetStreamAsync(new Uri("http://www.gstatic.com/webp/gallery/2.webp")))
			using (var bitmap = SKBitmap.Decode(stream))
				Assert.NotNull(bitmap);
		}

		[SkippableFact]
		public void ReadOnlyStream()
		{
			using (var stream = File.OpenRead(Path.Combine(PathToImages, "baboon.png")))
			using (var nonSeekable = new NonSeekableReadOnlyStream(stream))
			using (var bitmap = SKBitmap.Decode(nonSeekable))
				Assert.NotNull(bitmap);
		}

		[SkippableTheory]
		[InlineData("CMYK.jpg")]
		[InlineData("baboon.png")]
		[InlineData("color-wheel.png")]
		public void CanDecodePath(string image)
		{
			var path = Path.Combine(PathToImages, image);

			using var codec = SKCodec.Create(path);
			Assert.NotNull(codec);

			Assert.Equal(SKCodecResult.Success, codec.GetPixels(out var pixels));
			Assert.NotEmpty(pixels);
		}

		[SkippableTheory]
		[InlineData("CMYK.jpg")]
		[InlineData("baboon.png")]
		[InlineData("color-wheel.png")]
		public void CanDecodeData(string image)
		{
			var path = Path.Combine(PathToImages, image);

			using var data = SKData.Create(path);
			Assert.NotNull(data);

			using var codec = SKCodec.Create(data);
			Assert.NotNull(codec);

			Assert.Equal(SKCodecResult.Success, codec.GetPixels(out var pixels));
			Assert.NotEmpty(pixels);
		}
	}
}

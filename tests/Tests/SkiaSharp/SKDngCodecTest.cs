using System;
using System.Buffers.Binary;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKDngCodecTest : SKTest
	{
		[Theory]
		[InlineData("sample_1mp.dng", 1, SKEncodedImageFormat.Dng,
			0xff6e8dabu, 0xff6888a6u, 0xffb2c3dau, 0xff7b835eu, 0xff001403u)]
		[InlineData("sample_1mp_rotated.dng", 8, SKEncodedImageFormat.Dng,
			0xff6e8dabu, 0xff6888a6u, 0xffb2c3dau, 0xff7b835eu, 0xff001403u)]
		[InlineData("dng_with_preview.dng", 1, SKEncodedImageFormat.Jpeg,
			0xff718fa9u, 0xff6789a5u, 0xffb1c3d7u, 0xff7d845au, 0xff001300u)]
		public void DngDecodesWithExpectedBoundsAndColors(
			string filename, ushort orientation, SKEncodedImageFormat format,
			uint upperLeft, uint nearUpperLeft, uint upperMiddle, uint center, uint lowerRight)
		{
			var path = Path.Combine(PathToImages, filename);
			Assert.Equal(orientation, ReadTiffOrientation(path));

			using (var codec = SKCodec.Create(path))
				AssertDecodedPixels(codec, format, upperLeft, nearUpperLeft, upperMiddle, center, lowerRight);

			using var data = SKData.Create(path);
			using (var dataCodec = SKCodec.Create(data))
				AssertDecodedPixels(dataCodec, format, upperLeft, nearUpperLeft, upperMiddle, center, lowerRight);

			if (format == SKEncodedImageFormat.Dng)
			{
				using var stream = new NonSeekableReadOnlyStream(File.OpenRead(path));
				using var bufferedCodec = SKCodec.Create(stream);
				AssertDecodedPixels(bufferedCodec, format, upperLeft, nearUpperLeft, upperMiddle, center, lowerRight);
			}
		}

		private static void AssertDecodedPixels(
			SKCodec codec, SKEncodedImageFormat format,
			uint upperLeft, uint nearUpperLeft, uint upperMiddle, uint center, uint lowerRight)
		{
			Assert.NotNull(codec);
			Assert.Equal(format, codec.EncodedFormat);
			// RAW reports TopLeft even for the fixture whose TIFF orientation is 8.
			Assert.Equal(SKEncodedOrigin.TopLeft, codec.EncodedOrigin);
			Assert.Equal(600, codec.Info.Width);
			Assert.Equal(338, codec.Info.Height);

			var info = new SKImageInfo(600, 338, SKColorType.Rgba8888, SKAlphaType.Opaque);
			using var bitmap = new SKBitmap(info);
			Assert.Equal(SKCodecResult.Success, codec.GetPixels(info, bitmap.GetPixels()));
			Assert.Equal(new SKColor(upperLeft), bitmap.GetPixel(0, 0));
			Assert.Equal(new SKColor(nearUpperLeft), bitmap.GetPixel(5, 5));
			Assert.Equal(new SKColor(upperMiddle), bitmap.GetPixel(150, 84));
			Assert.Equal(new SKColor(center), bitmap.GetPixel(300, 169));
			Assert.Equal(new SKColor(lowerRight), bitmap.GetPixel(599, 337));
		}

		private static ushort ReadTiffOrientation(string path)
		{
			var data = File.ReadAllBytes(path);
			Assert.Equal((ushort)0x4949, BinaryPrimitives.ReadUInt16LittleEndian(data));
			Assert.Equal((ushort)42, BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(2)));
			var ifdOffset = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(4)));
			var count = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(ifdOffset));
			for (var i = 0; i < count; i++)
			{
				var entry = data.AsSpan(ifdOffset + 2 + i * 12);
				if (BinaryPrimitives.ReadUInt16LittleEndian(entry) == 0x0112)
				{
					Assert.Equal((ushort)3, BinaryPrimitives.ReadUInt16LittleEndian(entry.Slice(2)));
					Assert.Equal(1u, BinaryPrimitives.ReadUInt32LittleEndian(entry.Slice(4)));
					return BinaryPrimitives.ReadUInt16LittleEndian(entry.Slice(8));
				}
			}

			throw new InvalidDataException($"Missing TIFF orientation in {path}.");
		}
	}
}

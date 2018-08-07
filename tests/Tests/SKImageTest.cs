﻿using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKImageTest : SKTest
	{
		[SkippableFact]
		public void TestLazyImage()
		{
			var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));
			Assert.NotNull(data);

			var image = SKImage.FromEncodedData(data);
			Assert.NotNull(image);

			Assert.True(image.IsLazyGenerated);
		}

		[SkippableFact]
		public void TestNotLazyImage()
		{
			var bitmap = CreateTestBitmap();
			Assert.NotNull(bitmap);

			var image = SKImage.FromBitmap(bitmap);
			Assert.NotNull(image);

			Assert.False(image.IsLazyGenerated);
		}

		[SkippableFact]
		public void EncodeWithSimpleSerializer()
		{
			var bitmap = CreateTestBitmap();

			bool encoded = false;
			var serializer = SKPixelSerializer.Create(pixmap =>
			{
				encoded = true;
				return pixmap.Encode(SKEncodedImageFormat.Jpeg, 100);
			});

			var image = SKImage.FromBitmap(bitmap);
			var data = image.Encode(serializer);

			var codec = SKCodec.Create(data);

			Assert.Equal(SKEncodedImageFormat.Jpeg, codec.EncodedFormat);

			Assert.True(encoded);
		}

		[SkippableFact]
		public void EncodeWithSerializer()
		{
			var bitmap = CreateTestBitmap();

			var serializer = new TestSerializer();

			var image = SKImage.FromBitmap(bitmap);
			var data = image.Encode(serializer);

			Assert.NotNull(data);

			Assert.Equal(1, serializer.DidEncode);
			Assert.Equal(0, serializer.DidUseEncodedData);

			Assert.Equal(data.ToArray(), bitmap.Bytes);
		}

		private class TestSerializer : SKPixelSerializer
		{
			public int DidEncode { get; set; }

			public int DidUseEncodedData { get; set; }

			public SKImageInfo EncodedInfo { get; set; }

			protected override SKData OnEncode(SKPixmap pixmap)
			{
				DidEncode++;

				EncodedInfo = pixmap.Info;

				return SKData.CreateCopy(pixmap.GetPixels(), (ulong)pixmap.Info.BytesSize);
			}

			protected override bool OnUseEncodedData(IntPtr data, IntPtr length)
			{
				DidUseEncodedData++;

				return false;
			}
		}
	}
}

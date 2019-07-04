﻿using System;
using System.IO;
using System.Linq;
using Xunit;
using System.Collections.Generic;

namespace SkiaSharp.Tests
{
	public class SKManagedStreamTest : SKTest
	{
		[SkippableFact]
		public void DotNetStreamIsCollected()
		{
			var managed = CreateTestStream();
			var stream = new SKManagedStream(managed, true);

			Assert.Equal(0, managed.Position);

			stream.Dispose();

			Assert.Throws<ObjectDisposedException>(() => managed.Position);
		}

		[SkippableFact]
		public void DotNetStreamIsNotCollected()
		{
			var managed = CreateTestStream();
			var stream = new SKManagedStream(managed, false);

			Assert.Equal(0, managed.Position);

			stream.Dispose();

			Assert.Equal(0, managed.Position);
		}

		[SkippableFact]
		public unsafe void StreamLosesOwnershipToCodecAndCanBeForgotten()
		{
			var managed = new MemoryStream(File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png")));
			var stream = new SKManagedStream(managed, true);
			var handle = stream.Handle;

			Assert.True(stream.OwnsHandle);
			Assert.True(SKObject.GetInstance<SKManagedStream>(handle, out _));

			var codec = SKCodec.Create(stream);
			Assert.False(stream.OwnsHandle);

			stream.Dispose();
			Assert.False(SKObject.GetInstance<SKManagedStream>(handle, out _));

			Assert.Equal(SKCodecResult.Success, codec.GetPixels(out var pixels));
			Assert.NotEmpty(pixels);
		}

		[SkippableFact]
		public unsafe void StreamThatHasLostOwnershipIsDisposed()
		{
			var managed = new MemoryStream(File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png")));
			var stream = new SKManagedStream(managed, true);
			var handle = stream.Handle;

			Assert.True(stream.OwnsHandle);
			Assert.True(SKObject.GetInstance<SKManagedStream>(handle, out _));

			var codec = SKCodec.Create(stream);
			Assert.False(stream.OwnsHandle);

			codec.Dispose();
			Assert.False(SKObject.GetInstance<SKManagedStream>(handle, out _));
		}

		[SkippableFact]
		public void StreamIsCollectedEvenWhenNotPropertyDisposed()
		{
			var handle = DoWork();

			CollectGarbage();

			var exists = SKObject.GetInstance<SKManagedStream>(handle, out _);
			Assert.False(exists);

			IntPtr DoWork()
			{
				var managed = CreateTestStream();
				var stream = new SKManagedStream(managed, true);
				return stream.Handle;
			}
		}

		[SkippableFact]
		public void ManagedStreamReadsByteCorrectly()
		{
			var data = new byte[1024];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(i % byte.MaxValue);
			}

			var stream = new MemoryStream(data);
			var skManagedStream = new SKManagedStream(stream);

			skManagedStream.Rewind();
			Assert.Equal(0, stream.Position);
			Assert.Equal(0, skManagedStream.Position);

			for (int i = 0; i < data.Length; i++)
			{
				skManagedStream.Position = i;

				Assert.Equal(i, stream.Position);
				Assert.Equal(i, skManagedStream.Position);

				Assert.Equal((byte)(i % byte.MaxValue), data[i]);
				Assert.Equal((byte)(i % byte.MaxValue), skManagedStream.ReadByte());

				Assert.Equal(i + 1, stream.Position);
				Assert.Equal(i + 1, skManagedStream.Position);
			}
		}

		[SkippableFact]
		public void ManagedStreamReadsChunkCorrectly()
		{
			var data = new byte[1024];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(i % byte.MaxValue);
			}

			var stream = new MemoryStream(data);
			var skManagedStream = new SKManagedStream(stream);

			skManagedStream.Rewind();
			Assert.Equal(0, stream.Position);
			Assert.Equal(0, skManagedStream.Position);

			var buffer = new byte[data.Length / 2];
			skManagedStream.Read(buffer, buffer.Length);

			Assert.Equal(data.Take(buffer.Length), buffer);
		}

		[SkippableFact]
		public void ManagedStreamReadsOffsetChunkCorrectly()
		{
			var data = new byte[1024];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(i % byte.MaxValue);
			}

			var stream = new MemoryStream(data);
			var skManagedStream = new SKManagedStream(stream);

			var offset = 768;

			skManagedStream.Position = offset;

			var buffer = new byte[data.Length];
			var taken = skManagedStream.Read(buffer, buffer.Length);

			Assert.Equal(data.Length - offset, taken);

			var resultData = data.Skip(offset).Take(buffer.Length).ToArray();
			resultData = resultData.Concat(Enumerable.Repeat<byte>(0, offset)).ToArray();
			Assert.Equal(resultData, buffer);
		}

		[SkippableFact]
		public void ManagedStreamIsNotCollectedPrematurely()
		{
			using (var stream = new SKDynamicMemoryWStream())
			using (SKDocument document = SKDocument.CreatePdf(stream, new SKDocumentPdfMetadata()))
			{
				var paintList = new List<SKPaint>();

				for (int index = 0; index < 10; index++)
				{
					Stream fontStream = File.OpenRead(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
					SKTypeface typeface = SKTypeface.FromStream(fontStream);

					SKPaint paint = new SKPaint
					{
						Typeface = typeface
					};
					paintList.Add(paint);
				}

				using (SKCanvas pageCanvas = document.BeginPage(792, 842))
				{
					foreach (var paint in paintList)
					{
						for (int i = 0; i < 100; i++)
							pageCanvas.DrawText("Text", 0, 5 * i, paint);
					}

					document.EndPage();
				}

				CollectGarbage();

				document.Close();
			}
		}
	}
}

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

namespace SkiaSharp.Tests
{
	public class SKManagedStreamTest : SKTest
	{
		[Test]
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
			Assert.AreEqual(0, stream.Position);
			Assert.AreEqual(0, skManagedStream.Position);

			for (int i = 0; i < data.Length; i++)
			{
				skManagedStream.Position = i;

				Assert.AreEqual(i, stream.Position);
				Assert.AreEqual(i, skManagedStream.Position);

				Assert.AreEqual((byte)(i % byte.MaxValue), data[i]);
				Assert.AreEqual((byte)(i % byte.MaxValue), skManagedStream.ReadByte());

				Assert.AreEqual(i + 1, stream.Position);
				Assert.AreEqual(i + 1, skManagedStream.Position);
			}
		}

		[Test]
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
			Assert.AreEqual(0, stream.Position);
			Assert.AreEqual(0, skManagedStream.Position);

			var buffer = new byte[data.Length / 2];
			skManagedStream.Read(buffer, buffer.Length);

			Assert.AreEqual(data.Take(buffer.Length), buffer);
		}

		[Test]
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

			Assert.AreEqual(data.Length - offset, taken);

			var resultData = data.Skip(offset).Take(buffer.Length).ToArray();
			resultData = resultData.Concat(Enumerable.Repeat<byte>(0, offset)).ToArray();
			Assert.AreEqual(resultData, buffer);
		}

		[Test]
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

				GC.Collect();
				document.Close();
			}
		}
	}
}

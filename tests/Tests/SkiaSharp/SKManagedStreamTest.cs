using System;
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
			var dotnet = CreateTestStream();
			var stream = new SKManagedStream(dotnet, true);

			Assert.Equal(0, dotnet.Position);

			stream.Dispose();

			Assert.Throws<ObjectDisposedException>(() => dotnet.Position);
		}

		[SkippableFact]
		public void DotNetStreamIsNotCollected()
		{
			var dotnet = CreateTestStream();
			var stream = new SKManagedStream(dotnet, false);

			Assert.Equal(0, dotnet.Position);

			stream.Dispose();

			Assert.Equal(0, dotnet.Position);
		}

		[SkippableFact]
		public unsafe void StreamLosesOwnershipToCodecButIsNotForgotten()
		{
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));
			var dotnet = new MemoryStream(bytes);
			var stream = new SKManagedStream(dotnet, true);
			var handle = stream.Handle;

			Assert.True(stream.OwnsHandle);
			Assert.True(SKObject.GetInstance<SKManagedStream>(handle, out _));

			var codec = SKCodec.Create(stream);
			Assert.False(stream.OwnsHandle);

			stream.Dispose();
			Assert.True(SKObject.GetInstance<SKManagedStream>(handle, out _));

			Assert.Equal(SKCodecResult.Success, codec.GetPixels(out var pixels));
			Assert.NotEmpty(pixels);
		}

		[SkippableFact]
		public unsafe void StreamThatHasLostOwnershipIsDisposed()
		{
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));
			var dotnet = new MemoryStream(bytes);
			var stream = new SKManagedStream(dotnet, true);
			var handle = stream.Handle;

			Assert.True(stream.OwnsHandle);
			Assert.True(SKObject.GetInstance<SKManagedStream>(handle, out _));

			var codec = SKCodec.Create(stream);
			Assert.False(stream.OwnsHandle);

			codec.Dispose();
			Assert.False(SKObject.GetInstance<SKManagedStream>(handle, out _));
		}

		[SkippableFact]
		public void StreamIsCollectedEvenWhenNotProperlyDisposed()
		{
			SkipOnMono();

			var handle = DoWork();

			CollectGarbage();

			var exists = SKObject.GetInstance<SKManagedStream>(handle, out _);
			Assert.False(exists);

			IntPtr DoWork()
			{
				var dotnet = CreateTestStream();
				var stream = new SKManagedStream(dotnet, true);
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

		[SkippableTheory]
		[InlineData(1024, 0, 0, 0)]
		[InlineData(1024, 1, 1, 1)]
		[InlineData(1024, 10, 10, 10)]
		[InlineData(1024, 100, 100, 100)]
		[InlineData(1024, 1000, 1000, 1000)]
		[InlineData(1024, 10000, 1024, 1024)]
		public void ReadIsCorrect(int dataSize, int readSize, int finalPos, int expectedReadSize)
		{
			var data = new byte[dataSize];
			for (var i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(i % byte.MaxValue);
			}

			var stream = new MemoryStream(data);
			var managedStream = new SKManagedStream(stream);

			var buffer = new byte[dataSize * 2];

			var actualReadSize = managedStream.Read(buffer, readSize);

			Assert.Equal(expectedReadSize, actualReadSize);
			Assert.Equal(finalPos, stream.Position);
			Assert.Equal(finalPos, managedStream.Position);
			Assert.Equal(data.Take(readSize), buffer.Take(actualReadSize));
			Assert.All(buffer.Skip(actualReadSize), i => Assert.Equal(0, i));
		}

		[SkippableTheory]
		[InlineData(1024, 0, 0, 0)]
		[InlineData(1024, 1, 1, 1)]
		[InlineData(1024, 10, 10, 10)]
		[InlineData(1024, 100, 100, 100)]
		[InlineData(1024, 1000, 1000, 1000)]
		[InlineData(1024, 10000, 1024, 1024)]
		public void SkipIsCorrect(int dataSize, int readSize, int finalPos, int expectedReadSize)
		{
			var data = new byte[dataSize];
			for (var i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(i % byte.MaxValue);
			}

			var stream = new MemoryStream(data);
			var managedStream = new SKManagedStream(stream);

			var actualReadSize = managedStream.Skip(readSize);

			Assert.Equal(expectedReadSize, actualReadSize);
			Assert.Equal(finalPos, stream.Position);
			Assert.Equal(finalPos, managedStream.Position);
		}

		[SkippableTheory]
		[InlineData(1024, 0, 0, 0)]
		[InlineData(1024, 1, 1, 1)]
		[InlineData(1024, 10, 10, 10)]
		[InlineData(1024, 100, 100, 100)]
		[InlineData(1024, 1000, 1000, 1000)]
		[InlineData(1024, 10000, 1024, 1024)]
		public void SkipNonSeekableIsCorrect(int dataSize, int readSize, int finalPos, int expectedReadSize)
		{
			var data = new byte[dataSize];
			for (var i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(i % byte.MaxValue);
			}

			var stream = new MemoryStream(data);
			var nonSeekable = new NonSeekableReadOnlyStream(stream);
			var managedStream = new SKManagedStream(nonSeekable);

			var actualReadSize = managedStream.Skip(readSize);

			Assert.Equal(expectedReadSize, actualReadSize);
			Assert.Equal(finalPos, stream.Position);
		}

		[SkippableFact]
		public void SkipOffsetChunkCorrectly()
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

			var taken = skManagedStream.Skip(data.Length);

			Assert.Equal(data.Length - offset, taken);
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
			using (var document = CreateDocument(out var handle))
			{
				var paintList = new List<(SKFont Font, SKPaint Paint)>();

				for (var index = 0; index < 10; index++)
				{
					var fontStream = File.OpenRead(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
					var typeface = SKTypeface.FromStream(fontStream);

					var font = new SKFont
					{
						Typeface = typeface
					};
					var paint = new SKPaint
					{
					};
					paintList.Add((font, paint));
				}

				using (var pageCanvas = document.BeginPage(792, 842))
				{
					foreach (var pair in paintList)
					{
						for (var i = 0; i < 100; i++)
							pageCanvas.DrawText("Text", 0, 5 * i, pair.Font, pair.Paint);
					}

					document.EndPage();
				}

				CollectGarbage();

				Assert.True(SKObject.GetInstance<SKDynamicMemoryWStream>(handle, out _));

				document.Close();
			}

			SKDocument CreateDocument(out IntPtr streamHandle)
			{
				var stream = new SKDynamicMemoryWStream();
				streamHandle = stream.Handle;

				return SKDocument.CreatePdf(stream, new SKDocumentPdfMetadata());
			}
		}

		[SkippableFact]
		public unsafe void StreamLosesOwnershipAndCanBeDisposedButIsNotActually()
		{
			var path = Path.Combine(PathToImages, "color-wheel.png");
			var bytes = File.ReadAllBytes(path);
			var stream = new SKManagedStream(new MemoryStream(bytes), true);
			var handle = stream.Handle;

			Assert.True(stream.OwnsHandle);
			Assert.False(stream.IgnorePublicDispose);
			Assert.True(SKObject.GetInstance<SKManagedStream>(handle, out _));

			var codec = SKCodec.Create(stream);
			Assert.False(stream.OwnsHandle);
			Assert.True(stream.IgnorePublicDispose);

			stream.Dispose();
			Assert.True(SKObject.GetInstance<SKManagedStream>(handle, out var inst));
			Assert.Same(stream, inst);

			Assert.Equal(SKCodecResult.Success, codec.GetPixels(out var pixels));
			Assert.NotEmpty(pixels);

			codec.Dispose();
			Assert.False(SKObject.GetInstance<SKManagedStream>(handle, out _));
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
			SkipOnMono();

			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			DoWork(out var codecH, out var streamH);

			CollectGarbage();

			Assert.False(SKObject.GetInstance<SKManagedStream>(streamH, out _));
			Assert.False(SKObject.GetInstance<SKCodec>(codecH, out _));

			void DoWork(out IntPtr codecHandle, out IntPtr streamHandle)
			{
				var codec = CreateCodec(out streamHandle);
				codecHandle = codec.Handle;

				CollectGarbage();

				Assert.Equal(SKCodecResult.Success, codec.GetPixels(out var pixels));
				Assert.NotEmpty(pixels);

				Assert.True(SKObject.GetInstance<SKManagedStream>(streamHandle, out var stream));
				Assert.False(stream.OwnsHandle);
				Assert.True(stream.IgnorePublicDispose);
			}

			SKCodec CreateCodec(out IntPtr streamHandle)
			{
				var stream = new SKManagedStream(new MemoryStream(bytes), true);
				streamHandle = stream.Handle;

				Assert.True(stream.OwnsHandle);
				Assert.False(stream.IgnorePublicDispose);
				Assert.True(SKObject.GetInstance<SKManagedStream>(streamHandle, out _));

				return SKCodec.Create(stream);
			}
		}

		[SkippableFact]
		public void StreamCanBeDuplicatedAndBothRemainReadable()
		{
			var dotnet = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			var stream = new SKManagedStream(dotnet, true);
			Assert.Equal(1, stream.ReadByte());
			Assert.Equal(2, stream.ReadByte());
			Assert.Equal(3, stream.ReadByte());

			var dupe = stream.Duplicate();
			Assert.NotSame(stream, dupe);
			// duplicate starts at position 0
			Assert.Equal(1, dupe.ReadByte());
			Assert.Equal(2, dupe.ReadByte());
			Assert.Equal(3, dupe.ReadByte());
			// original remains readable at its previous position
			Assert.Equal(4, stream.ReadByte());
			Assert.Equal(5, stream.ReadByte());

			dupe.Dispose();
			stream.Dispose();
		}

		[SkippableFact]
		public void StreamCanBeForkedAndBothRemainReadable()
		{
			var dotnet = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			var stream = new SKManagedStream(dotnet, true);
			Assert.Equal(1, stream.ReadByte());
			Assert.Equal(2, stream.ReadByte());

			var dupe = stream.Fork();
			Assert.NotSame(stream, dupe);
			// fork starts at the same position as the original
			Assert.Equal(3, dupe.ReadByte());
			Assert.Equal(4, dupe.ReadByte());
			// original remains readable at its previous position
			Assert.Equal(3, stream.ReadByte());
			Assert.Equal(4, stream.ReadByte());

			dupe.Dispose();
			stream.Dispose();
		}

		[SkippableFact]
		public void StreamCanBeDuplicatedMultipleTimes()
		{
			var dotnet = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			var stream = new SKManagedStream(dotnet, true);
			Assert.Equal(1, stream.ReadByte());
			Assert.Equal(2, stream.ReadByte());

			var dupe1 = stream.Duplicate();
			var dupe2 = stream.Duplicate();

			Assert.Equal(1, dupe1.ReadByte());
			Assert.Equal(1, dupe2.ReadByte());
			// original still works
			Assert.Equal(3, stream.ReadByte());

			dupe1.Dispose();
			dupe2.Dispose();
			stream.Dispose();
		}

		[SkippableFact]
		public void StreamCanBeDuplicatedMultipleTimesWithChildDisposed()
		{
			var dotnet = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			var stream = new SKManagedStream(dotnet, true);
			Assert.Equal(1, stream.ReadByte());
			Assert.Equal(2, stream.ReadByte());

			var dupe1 = stream.Duplicate();
			Assert.Equal(1, dupe1.ReadByte());
			Assert.Equal(2, dupe1.ReadByte());

			dupe1.Dispose();

			// can still duplicate after child is disposed
			var dupe2 = stream.Duplicate();
			Assert.Equal(1, dupe2.ReadByte());
			Assert.Equal(2, dupe2.ReadByte());

			// can duplicate again — no limit
			var dupe3 = stream.Duplicate();
			Assert.Equal(1, dupe3.ReadByte());

			dupe2.Dispose();
			dupe3.Dispose();
			stream.Dispose();
		}

		[SkippableFact]
		public void DuplicatesAreIndependentAfterParentDisposed()
		{
			var dotnet = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			var stream = new SKManagedStream(dotnet, true);
			Assert.Equal(1, stream.ReadByte());
			Assert.Equal(2, stream.ReadByte());

			var dupe1 = stream.Duplicate();
			Assert.Equal(1, dupe1.ReadByte());
			Assert.Equal(2, dupe1.ReadByte());

			// disposing parent does not affect the independent duplicate
			stream.Dispose();

			Assert.Equal(3, dupe1.ReadByte());

			var dupe2 = dupe1.Duplicate();
			Assert.Equal(1, dupe2.ReadByte());
			Assert.Equal(2, dupe2.ReadByte());

			dupe1.Dispose();
			dupe2.Dispose();

			// the original .NET stream should be disposed since disposeManagedStream was true
			Assert.Throws<ObjectDisposedException>(() => dotnet.Position);
		}

		[SkippableFact]
		public void DuplicateStreamIsDisposed()
		{
			var dotnet = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			var stream = new SKManagedStream(dotnet, true);
			Assert.Equal(1, stream.ReadByte());
			Assert.Equal(2, stream.ReadByte());

			var dupe1 = stream.Duplicate();
			Assert.Equal(1, dupe1.ReadByte());
			Assert.Equal(2, dupe1.ReadByte());

			stream.Dispose();

			var dupe2 = dupe1.Duplicate();
			var handle = dupe2.Handle;
			Assert.Equal(1, dupe2.ReadByte());
			Assert.Equal(2, dupe2.ReadByte());

			Assert.True(SKObject.GetInstance<SKStream>(handle, out _));

			dupe2.Dispose();
			Assert.False(SKObject.GetInstance<SKStream>(handle, out _));

			dupe1.Dispose();
		}

		[SkippableFact]
		public void DuplicateStreamIsCollected()
		{
			SkipOnMono();

			var handle = DoWork();

			CollectGarbage();

			Assert.False(SKObject.GetInstance<SKStream>(handle, out _));

			IntPtr DoWork()
			{
				var dotnet = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
				var stream = new SKManagedStream(dotnet, true);
				Assert.Equal(1, stream.ReadByte());
				Assert.Equal(2, stream.ReadByte());

				var dupe1 = stream.Duplicate();
				Assert.Equal(1, dupe1.ReadByte());
				Assert.Equal(2, dupe1.ReadByte());

				stream.Dispose();

				var dupe2 = dupe1.Duplicate();
				Assert.Equal(1, dupe2.ReadByte());
				Assert.Equal(2, dupe2.ReadByte());

				return dupe2.Handle;
			}
		}

		[SkippableFact]
		public void MultipleDuplicatesAreIndependent()
		{
			var dotnet = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			var stream = new SKManagedStream(dotnet, true);
			Assert.Equal(1, stream.ReadByte());
			Assert.Equal(2, stream.ReadByte());

			var dupe1 = stream.Duplicate();
			Assert.Equal(1, dupe1.ReadByte());
			Assert.Equal(2, dupe1.ReadByte());

			// can still duplicate the original
			var dupe2 = stream.Duplicate();
			Assert.Equal(1, dupe2.ReadByte());
			Assert.Equal(2, dupe2.ReadByte());

			// can also duplicate the duplicate
			var dupe3 = dupe1.Duplicate();
			Assert.Equal(1, dupe3.ReadByte());
			Assert.Equal(2, dupe3.ReadByte());

			// disposing any one does not affect the others
			dupe1.Dispose();
			Assert.Equal(3, stream.ReadByte());
			Assert.Equal(3, dupe2.ReadByte());
			Assert.Equal(3, dupe3.ReadByte());

			dupe2.Dispose();
			dupe3.Dispose();
			stream.Dispose();
		}

		[SkippableFact]
		public void NonSeekableStreamDuplicateAndForkReturnNull()
		{
			var dotnet = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			var nonSeekable = new NonSeekableReadOnlyStream(dotnet);
			var stream = new SKManagedStream(nonSeekable);

			Assert.Null(stream.Duplicate());
			Assert.Null(stream.Fork());

			stream.Dispose();
		}

		[SkippableFact]
		public void DotNetStreamIsClosedWhenSKManagedStreamIsDisposed()
		{
			var dotnet = CreateTestStream();
			var stream = new SKManagedStream(dotnet, true);
			Assert.Equal(0, stream.Position);

			var dupe = stream.Duplicate();
			Assert.Equal(0, dupe.Position);

			// the snapshot has been taken, so the .NET stream is closed when
			// the SKManagedStream is disposed (the duplicate is independent)
			stream.Dispose();
			Assert.Throws<ObjectDisposedException>(() => dotnet.Position);

			// the duplicate is still usable after the parent is disposed
			Assert.Equal(0, dupe.Position);

			dupe.Dispose();
		}
	}
}

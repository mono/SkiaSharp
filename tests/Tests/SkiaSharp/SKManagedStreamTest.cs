using System;
using System.IO;
using System.Linq;
using Xunit;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

			// Failed Create has no new native owner, so RevokeOwnership(null) runs
			// DisposeInternal(): the wrapper is genuinely disposed and deregistered,
			// not merely public-dispose-guarded. Hence IsDisposed (not IgnorePublicDispose).
			Assert.False(stream.OwnsHandle);
			Assert.True(stream.IsDisposed);
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

		[SkippableFact]
		public unsafe void StreamLosesOwnershipButManagedStreamStaysOpenUntilOwnerDisposed()
		{
			var path = Path.Combine(PathToImages, "color-wheel.png");
			var bytes = File.ReadAllBytes(path);
			var dotnetStream = new MemoryStream(bytes);
			var stream = new SKManagedStream(dotnetStream, true);
			var handle = stream.Handle;

			Assert.True(stream.OwnsHandle);
			Assert.False(stream.IsDisposed);
			Assert.True(SKObject.GetInstance<SKManagedStream>(handle, out _));

			var codec = SKCodec.Create(stream);

			// The codec reads the managed stream LAZILY (on GetPixels), so the
			// wrapper must NOT be disposed at ownership transfer: doing so would
			// close the underlying .NET stream and crash the later managed read.
			Assert.False(stream.OwnsHandle);
			Assert.False(stream.IsDisposed);
			Assert.True(stream.IgnorePublicDispose);
			Assert.True(SKObject.GetInstance<SKManagedStream>(handle, out _));
			Assert.True(dotnetStream.CanRead);

			// A public Dispose() is ignored while the codec owns the native stream.
			stream.Dispose();
			Assert.False(stream.IsDisposed);
			Assert.True(dotnetStream.CanRead);

			// The lazy managed read must succeed — this is the regression guard.
			Assert.Equal(SKCodecResult.Success, codec.GetPixels(out var pixels));
			Assert.NotEmpty(pixels);

			// Disposing the owner tears down the wrapper and closes the .NET stream
			// (disposeManagedStream: true), now that nothing reads it any more.
			codec.Dispose();
			Assert.True(stream.IsDisposed);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(handle, stream);
			Assert.False(dotnetStream.CanRead);
		}

		// The SKObject.Owned write-stream path (used by SKSvgCanvas.Create(Stream) and
		// SKDocument.Create*(Stream)) is the mirror image of the codec/typeface read path.
		// There the internally-created SKManagedWStream is registered as an OwnsHandle child,
		// so it is torn down in DisposeManaged() — AFTER DisposeNative(). That ordering is
		// load-bearing for write streams: the native object (e.g. the SVG canvas footer
		// writer, or the PDF document serializer) flushes its final bytes into the managed
		// stream as it is destroyed, so the .NET stream MUST still be open at DisposeNative
		// time. These tests lock in that guarantee and prove writes triggered AFTER Create
		// (lazy writes) reach the underlying .NET stream without crossing a closed boundary.

		[SkippableFact]
		public void SvgCanvasWritesLazilyAndKeepsManagedStreamOpenUntilOwnerDisposed()
		{
			var tracking = new LifecycleTrackingStream();

			var svg = SKSvgCanvas.Create(SKRect.Create(100, 100), tracking);

			// Create must NOT dispose or close the caller's stream.
			Assert.False(tracking.IsDisposed);
			Assert.True(tracking.CanWrite);

			// Drawing happens AFTER Create returns: this is a lazy write through the
			// still-alive owned wstream. It must not crash or hit a closed stream.
			using (var paint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Fill })
			{
				svg.DrawRect(SKRect.Create(10, 10, 80, 80), paint);
			}
			Assert.False(tracking.IsDisposed);
			Assert.True(tracking.CanWrite);

			// Disposing the owner runs DisposeNative() (native canvas flushes the SVG
			// footer into the managed stream) BEFORE the owned wstream's DisposeManaged().
			// The final native flush must therefore land in a still-open .NET stream.
			svg.Dispose();

			// disposeManagedStream defaults to false for the Stream overload, so the
			// caller's stream stays open after the canvas is gone.
			Assert.False(tracking.IsDisposed);
			Assert.True(tracking.BytesWritten > 0);
			Assert.False(tracking.WroteAfterClose);

			tracking.Position = 0;
			using var reader = new StreamReader(tracking);
			var xml = reader.ReadToEnd();
			Assert.Contains("<svg", xml);
			Assert.Contains("rect", xml);
		}

		[SkippableFact]
		public void PdfDocumentWritesLazilyAndKeepsManagedStreamOpenUntilOwnerDisposed()
		{
			var tracking = new LifecycleTrackingStream();

			var document = SKDocument.CreatePdf(tracking);
			Assert.NotNull(document);

			// Create must NOT dispose or close the caller's stream.
			Assert.False(tracking.IsDisposed);
			Assert.True(tracking.CanWrite);

			// Page content is produced AFTER Create — a lazy write into the owned wstream.
			var canvas = document.BeginPage(100, 100);
			using (var paint = new SKPaint { Color = SKColors.Blue, Style = SKPaintStyle.Fill })
			{
				canvas.DrawRect(SKRect.Create(10, 10, 80, 80), paint);
			}
			document.EndPage();

			// Close() serializes the PDF body into the managed stream. The stream must
			// still be open here — Close runs before any disposal of the owned wstream.
			document.Close();
			Assert.False(tracking.IsDisposed);
			Assert.True(tracking.CanWrite);
			Assert.True(tracking.BytesWritten > 0);

			// Disposing the owner tears down the owned wstream in DisposeManaged(), after
			// DisposeNative(). With disposeManagedStream: false the .NET stream survives.
			document.Dispose();
			Assert.False(tracking.IsDisposed);
			Assert.False(tracking.WroteAfterClose);

			var header = new byte[5];
			tracking.Position = 0;
			Assert.Equal(5, tracking.Read(header, 0, 5));
			Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(header));
		}

		// ---- fromNative destroy-callback re-entrancy (mirrors the SKDrawable invariant) ----
		// SKManagedStream / SKManagedWStream own a native object whose destruction triggers a
		// managed destroy proxy (DelegateProxies.*stream*) that flips `fromNative` to 1 and calls
		// Dispose() re-entrantly. Under the lock-paired SKObject.Dispose, DisposeNative() runs
		// OUTSIDE the lock, so the synchronous native destroy can re-enter Dispose() on the
		// same thread; the re-entrant call re-acquires the lock fresh and no-ops on isDisposed==1.
		// These tests pin the single-free + deregistration + fromNative-flip invariants.

		[SkippableFact]
		public void DisposingManagedStreamFiresNativeDestroyCallback()
		{
			var dotnet = CreateTestStream();
			var stream = new SKManagedStream(dotnet, true);
			var handle = stream.Handle;

			try
			{
				Assert.NotEqual(IntPtr.Zero, handle);
				Assert.Equal(0, stream.fromNative);
				Assert.True(HandleDictionary.GetInstance<SKManagedStream>(handle, out var live));
				Assert.Same(stream, live);
			}
			finally
			{
				stream.Dispose();
			}

			Assert.Equal(1, stream.fromNative);
			Assert.True(stream.IsDisposed);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(handle, stream);
		}

		[SkippableFact]
		public void DisposingManagedWStreamFiresNativeDestroyCallback()
		{
			var dotnet = new MemoryStream();
			var stream = new SKManagedWStream(dotnet, true);
			var handle = stream.Handle;

			try
			{
				Assert.NotEqual(IntPtr.Zero, handle);
				Assert.Equal(0, stream.fromNative);
				Assert.True(HandleDictionary.GetInstance<SKManagedWStream>(handle, out var live));
				Assert.Same(stream, live);
			}
			finally
			{
				stream.Dispose();
			}

			Assert.Equal(1, stream.fromNative);
			Assert.True(stream.IsDisposed);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedWStream>(handle, stream);
		}

		[SkippableFact]
		public void DisposingManagedStreamTwiceIsNoOp()
		{
			var stream = new SKManagedStream(CreateTestStream(), true);
			var handle = stream.Handle;

			stream.Dispose();
			Assert.Equal(1, stream.fromNative);
			Assert.True(stream.IsDisposed);

			stream.Dispose();
			Assert.Equal(1, stream.fromNative);
			Assert.True(stream.IsDisposed);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(handle, stream);
		}

		[SkippableFact]
		public void ConcurrentDisposeOfSameManagedStreamIsIdempotent()
		{
			SkipOnPlatform(IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			// Many threads racing Dispose() on the SAME wrapper must funnel through the single
			// isDisposed CAS: one cleanup, native freed once, destroy callback flips fromNative once.
			var stream = new SKManagedStream(CreateTestStream(), true);
			var handle = stream.Handle;

			SKHandleDictionaryTestHelpers.RunWithTimeout(
				() => Parallel.For(0, 64, _ => stream.Dispose()),
				deadlockMessage: "Concurrent Dispose() of the same managed stream deadlocked.");

			Assert.Equal(1, stream.fromNative);
			Assert.True(stream.IsDisposed);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(handle, stream);
		}

		[SkippableFact]
		public void ConcurrentDisposeOfManyManagedStreamsIsSafe()
		{
			SkipOnPlatform(IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			// Mobile interpreter runtimes (iOS / Mac Catalyst / Android) schedule the thread pool too
			// sparsely to push 128 work items through the native dispose path before the timeout, so the
			// stress is sized down there. The work is driven through dedicated threads (RunConcurrent),
			// not Parallel.For: all N threads rendezvous at a barrier and then Dispose() simultaneously,
			// which is both deterministic (no pool dependency) and a stronger test of the registry's
			// concurrent-deregister path than pool-scheduled work items.
			var count = (IsAndroid || IsIOS || IsMacCatalyst) ? 16 : 128;
			var streams = new List<SKManagedStream>(count);
			for (var i = 0; i < count; i++)
				streams.Add(new SKManagedStream(CreateTestStream(), true));

			SKHandleDictionaryTestHelpers.RunConcurrent(
				count,
				i => streams[i].Dispose(),
				deadlockMessage: "Concurrent Dispose() of many managed streams deadlocked.");

			foreach (var stream in streams)
			{
				Assert.Equal(1, stream.fromNative);
				Assert.True(stream.IsDisposed);
			}
		}

		// After SKCodec.Create reparents the managed stream (OwnsHandle=false, IgnorePublicDispose
		// latched under the lock), the user STILL holds the wrapper and may call Dispose() on it
		// from another thread. The PR's lock-paired public Dispose must IGNORE that call so the codec
		// keeps reading through a live stream and remains its sole disposer. Before the LAZY-reparent
		// fix an eager public close here tore the native stream out from under an in-flight native read
		// -> use-after-free / host crash. The existing concurrency tests only race Dispose() against
		// itself on an OWNED stream; these two pin the reparented supported concurrency: (1) an ignored
		// public Dispose racing a lazy native read, and (2) an ignored public Dispose racing the codec's
		// owner-teardown (which must still be the single, exactly-once disposer of the wrapper).

		[SkippableFact]
		public unsafe void PublicDisposeWhileCodecReadIsInFlightIsIgnored()
		{
			SkipOnPlatform(IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			// DETERMINISTIC version of the race: park the native lazy read INSIDE the .NET
			// Stream.Read() callback, fire the (reparented) wrapper's public Dispose() while the
			// native read is provably in-flight, then release the read. The public Dispose must be
			// ignored (IgnorePublicDispose latched), so the in-flight native read completes against a
			// live stream and GetPixels succeeds. This is the exact use-after-free the LAZY-reparent
			// fix prevents — an eager public close here would close the .NET stream mid-read.
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			using var readEntered = new ManualResetEventSlim(false);
			using var releaseRead = new ManualResetEventSlim(false);

			var dotnet = new GatedCountingStream(bytes);
			var stream = new SKManagedStream(dotnet, true);
			var handle = stream.Handle;
			var codec = SKCodec.Create(stream);
			Task reader = null;
			try
			{
				Assert.False(stream.OwnsHandle);
				Assert.True(stream.IgnorePublicDispose);

				// Arm only AFTER Create() so the gate trips on a GetPixels read, not header parsing.
				dotnet.Arm(readEntered, releaseRead);

				var result = SKCodecResult.InternalError;
				reader = Task.Run(() => result = codec.GetPixels(out _));

				Assert.True(readEntered.Wait(TimeSpan.FromSeconds(30)), "Native lazy read never entered the managed stream.");

				// The native read is parked inside Read(); a public Dispose now must be a no-op.
				stream.Dispose();
				Assert.False(stream.IsDisposed);

				releaseRead.Set();
				Assert.True(reader.Wait(TimeSpan.FromSeconds(30)), "GetPixels did not complete after the read was released.");

				// If the ignored Dispose had wrongly closed the .NET stream, the parked inner.Read would
				// have thrown and GetPixels would not be Success; DisposeCount would also not be 0.
				Assert.Equal(SKCodecResult.Success, result);
				Assert.False(stream.IsDisposed);
				Assert.Equal(0, dotnet.DisposeCount);
			}
			finally
			{
				// Drain the reader before tearing the codec down so no background decode runs against a
				// disposed codec. Swallow here so a primary in-try assertion failure is not masked.
				releaseRead.Set();
				try { reader?.Wait(TimeSpan.FromSeconds(30)); } catch { }
				codec.Dispose();
			}

			// The codec is the sole disposer; the underlying .NET stream was closed exactly once.
			Assert.True(stream.IsDisposed);
			Assert.Equal(1, dotnet.DisposeCount);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(handle, stream);
		}

		[SkippableFact]
		public unsafe void PublicDisposeAroundLazyCodecReadIsIgnoredStress()
		{
			SkipOnPlatform(IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			// STRESS version: barrier-synchronised public Dispose vs a full GetPixels, repeated many
			// times to shake out interleavings beyond the single overlap the deterministic gate pins.
			// (The barrier only co-starts the two operations; it does NOT guarantee Dispose overlaps an
			// in-flight Read — that exact overlap is proven deterministically by the gated test above.)
			const int iterations = 100;
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			for (var i = 0; i < iterations; i++)
			{
				var stream = new SKManagedStream(new MemoryStream(bytes), true);
				var handle = stream.Handle;
				var codec = SKCodec.Create(stream);
				try
				{
					Assert.False(stream.OwnsHandle);
					Assert.True(stream.IgnorePublicDispose);

					var result = SKCodecResult.InternalError;
					using (var barrier = new Barrier(2))
					{
						// Dedicated threads (not Task.Run) so the Barrier(2) always has both participants
						// present even under full-suite load; see RunConcurrent rationale.
						Exception readError = null, disposeError = null;
						var read = new Thread (() => { try { barrier.SignalAndWait (); result = codec.GetPixels (out _); } catch (Exception ex) { readError = ex; } }) { IsBackground = true };
						var dispose = new Thread (() => { try { barrier.SignalAndWait (); stream.Dispose (); } catch (Exception ex) { disposeError = ex; } }) { IsBackground = true };
						read.Start ();
						dispose.Start ();
						var completed = read.Join (TimeSpan.FromSeconds (30));
						completed &= dispose.Join (TimeSpan.FromSeconds (30));
						Assert.True (completed, "Dispose/GetPixels race did not complete in time.");
						if (readError != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture (readError).Throw ();
						if (disposeError != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture (disposeError).Throw ();
					}

					// The lazy native read saw a live stream; the racing public Dispose was a no-op.
					Assert.Equal(SKCodecResult.Success, result);
					Assert.False(stream.IsDisposed);
					Assert.True(HandleDictionary.GetInstance<SKManagedStream>(handle, out _));
				}
				finally
				{
					codec.Dispose();
				}

				// The codec is the sole disposer; teardown ran after the race.
				Assert.Equal(1, stream.fromNative);
				Assert.True(stream.IsDisposed);
				SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(handle, stream);
			}
		}

		[SkippableFact]
		public unsafe void PublicDisposeRacingCodecTeardownClosesManagedStreamExactlyOnce()
		{
			SkipOnPlatform(IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			// Ignored public Dispose (no-ops on IgnorePublicDispose) racing the codec's owner-teardown
			// (the sole disposer, via DisposeUnownedManaged -> child.DisposeInternal). The underlying
			// .NET stream must be closed EXACTLY once — proven by DisposeCount. fromNative is only a
			// one-way latch (Interlocked.Exchange), so fromNative==1 asserts the native destroy callback
			// was OBSERVED, not that it fired exactly once; DisposeCount carries the exactly-once proof.
			const int iterations = 200;
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			for (var i = 0; i < iterations; i++)
			{
				var dotnet = new GatedCountingStream(bytes);
				var stream = new SKManagedStream(dotnet, true);
				var handle = stream.Handle;
				var codec = SKCodec.Create(stream);
				try
				{
					Assert.False(stream.OwnsHandle);
					Assert.True(stream.IgnorePublicDispose);

					using (var barrier = new Barrier(2))
					{
						var teardownThreadId = 0;
						// Dedicated threads (not Task.Run) so the Barrier(2) always has both participants.
						Exception disposeError = null, teardownError = null;
						var dispose = new Thread (() => { try { barrier.SignalAndWait (); stream.Dispose (); } catch (Exception ex) { disposeError = ex; } }) { IsBackground = true };
						var teardown = new Thread (() => { try { barrier.SignalAndWait (); teardownThreadId = Environment.CurrentManagedThreadId; codec.Dispose (); } catch (Exception ex) { teardownError = ex; } }) { IsBackground = true };
						dispose.Start ();
						teardown.Start ();
						var completed = dispose.Join (TimeSpan.FromSeconds (30));
						completed &= teardown.Join (TimeSpan.FromSeconds (30));
						Assert.True (completed, "Dispose/teardown race did not complete in time.");
						if (disposeError != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture (disposeError).Throw ();
						if (teardownError != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture (teardownError).Throw ();

						// The ignored public Dispose never closes the stream; the codec owner-teardown is the
						// real disposer. Proven by the closing thread being the teardown task, not the public
						// Dispose task — DisposeCount==1 alone could not distinguish which path won.
						Assert.Equal (teardownThreadId, dotnet.FirstDisposeThreadId);

						// Keep both wrappers rooted across the race assertion so a GC + finalizer
						// can never flip FirstDisposeThreadId from the finalizer thread mid-assert.
						GC.KeepAlive (stream);
						GC.KeepAlive (dotnet);
					}
				}
				finally
				{
					// Idempotent: redundant if the racing thread already tore the codec down.
					codec.Dispose();
				}

				// Ignored public Dispose + single owner teardown => one .NET stream close, one latch flip.
				Assert.Equal(1, dotnet.DisposeCount);
				Assert.Equal(1, stream.fromNative);
				Assert.True(stream.IsDisposed);
				SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(handle, stream);
			}
		}

		[SkippableFact]
		public unsafe void NonSeekableStreamReparentedToCodecTearsDownNestedStreamExactlyOnce()
		{
			// A NON-SEEKABLE stream routes SKCodec.Create through SKFrontBufferedManagedStream, which
			// holds a private *inner* SKManagedStream wrapping the user's .NET stream. Only the OUTER
			// front-buffered wrapper is reparented onto the codec (OwnsHandle=false, IgnorePublicDispose
			// =true); the inner SKManagedStream is UNTOUCHED (still owns its handle, still forwards public
			// Dispose). The user's racing fb.Dispose() must be a no-op, and codec teardown must walk
			// fb -> inner -> .NET stream and close the .NET stream EXACTLY once. We supply the inner
			// explicitly (public ctor) so both wrappers' deregistration is asserted without reflection.
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			var dotnet = new NonSeekableGatedCountingStream(bytes);
			var inner = new SKManagedStream(dotnet, true);
			var innerHandle = inner.Handle;
			var fb = new SKFrontBufferedManagedStream(inner, SKCodec.MinBufferedBytesNeeded, true);
			var fbHandle = fb.Handle;
			var codec = SKCodec.Create(fb);
			try
			{
				Assert.NotNull(codec);

				// Only the outer front-buffered wrapper is reparented; the inner stays a normal owner.
				Assert.False(fb.OwnsHandle);
				Assert.True(fb.IgnorePublicDispose);
				Assert.True(inner.OwnsHandle);
				Assert.False(inner.IgnorePublicDispose);

				Assert.Equal(SKCodecResult.Success, codec.GetPixels(out _));

				// The user still holds fb and (wrongly) disposes it: it is reparented, so this is a no-op.
				fb.Dispose();
				Assert.False(fb.IsDisposed);
				Assert.False(inner.IsDisposed);
				Assert.Equal(0, dotnet.DisposeCount);
			}
			finally
			{
				codec.Dispose();
			}

			// Codec teardown is the sole disposer: fb -> inner -> .NET stream, closed exactly once.
			Assert.True(fb.IsDisposed);
			Assert.True(inner.IsDisposed);
			Assert.Equal(1, dotnet.DisposeCount);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKFrontBufferedManagedStream>(fbHandle, fb);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(innerHandle, inner);
		}

		[SkippableFact]
		public unsafe void PublicDisposeWhileFrontBufferedCodecReadIsInFlightIsIgnored()
		{
			SkipOnPlatform(IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			// DETERMINISTIC in-flight-read race for the NESTED non-seekable graph. Park the native lazy
			// read inside the underlying .NET Stream.Read() (reached through fb's front buffer -> inner
			// SKManagedStream), fire the reparented fb's public Dispose() while that read is provably
			// in-flight, then release. The ignored public Dispose must not close anything mid-read, so
			// GetPixels succeeds and the .NET stream is closed exactly once by codec teardown.
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			using var readEntered = new ManualResetEventSlim(false);
			using var releaseRead = new ManualResetEventSlim(false);

			var dotnet = new NonSeekableGatedCountingStream(bytes);
			var fb = new SKFrontBufferedManagedStream(dotnet, SKCodec.MinBufferedBytesNeeded, true);
			var fbHandle = fb.Handle;
			var codec = SKCodec.Create(fb);
			Task reader = null;
			try
			{
				Assert.False(fb.OwnsHandle);
				Assert.True(fb.IgnorePublicDispose);

				// Arm only AFTER Create() so the gate trips on a GetPixels read past the front buffer, not
				// on the header bytes buffered during format detection.
				dotnet.Arm(readEntered, releaseRead);

				var result = SKCodecResult.InternalError;
				reader = Task.Run(() => result = codec.GetPixels(out _));

				Assert.True(readEntered.Wait(TimeSpan.FromSeconds(30)), "Native lazy read never reached the underlying non-seekable stream.");

				// The native read is parked inside the underlying Read(); a public Dispose now must no-op.
				fb.Dispose();
				Assert.False(fb.IsDisposed);
				Assert.Equal(0, dotnet.DisposeCount);

				releaseRead.Set();
				Assert.True(reader.Wait(TimeSpan.FromSeconds(30)), "GetPixels did not complete after the read was released.");

				Assert.Equal(SKCodecResult.Success, result);
				Assert.False(fb.IsDisposed);
				Assert.Equal(0, dotnet.DisposeCount);
			}
			finally
			{
				releaseRead.Set();
				try { reader?.Wait(TimeSpan.FromSeconds(30)); } catch { }
				codec.Dispose();
			}

			// The codec is the sole disposer; the underlying .NET stream was closed exactly once.
			Assert.True(fb.IsDisposed);
			Assert.Equal(1, dotnet.DisposeCount);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKFrontBufferedManagedStream>(fbHandle, fb);
		}

		[SkippableFact]
		public unsafe void InvalidNonSeekableStreamFailedCodecCreateClosesNestedStreamExactlyOnce()
		{
			// FAILURE path for the nested non-seekable graph: when the codec cannot be created, Create
			// still transfers ownership to a null native object, which disposes the front-buffered
			// wrapper immediately. That teardown must walk fb -> inner -> .NET stream and close it
			// exactly once, and both wrappers must deregister — no leak, no double-free.
			var bytes = new byte[256];
			for (var i = 0; i < bytes.Length; i++)
				bytes[i] = (byte)(i + 1);

			var dotnet = new NonSeekableGatedCountingStream(bytes);
			var inner = new SKManagedStream(dotnet, true);
			var innerHandle = inner.Handle;
			var fb = new SKFrontBufferedManagedStream(inner, SKCodec.MinBufferedBytesNeeded, true);
			var fbHandle = fb.Handle;

			var codec = SKCodec.Create(fb, out var result);

			Assert.Null(codec);
			Assert.NotEqual(SKCodecResult.Success, result);

			// The failed Create disposed fb immediately, which tore down the whole nested chain once.
			Assert.True(fb.IsDisposed);
			Assert.True(inner.IsDisposed);
			Assert.Equal(1, dotnet.DisposeCount);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKFrontBufferedManagedStream>(fbHandle, fb);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(innerHandle, inner);
		}

		[SkippableFact]
		public unsafe void InvalidSeekableManagedStreamFailedCodecCreateClosesStreamExactlyOnce()
		{
			// FAILURE path for the FLAT seekable graph (no front-buffering): a seekable managed Stream
			// of invalid bytes is wrapped in a single SKManagedStream. When the codec cannot be created,
			// Create transfers ownership to a null native object, so RevokeOwnership(null) disposes the
			// wrapper immediately. That teardown must close the .NET stream EXACTLY once and deregister
			// the wrapper — no leak, no double-free. Complements the non-seekable/front-buffered failure
			// test above, which exercises a different (nested) teardown graph.
			var bytes = new byte[256];
			for (var i = 0; i < bytes.Length; i++)
				bytes[i] = (byte)(i + 1);

			var dotnet = new GatedCountingStream(bytes);
			var stream = new SKManagedStream(dotnet, true);
			var handle = stream.Handle;

			Assert.True(stream.OwnsHandle);
			Assert.True(SKObject.GetInstance<SKManagedStream>(handle, out _));

			var codec = SKCodec.Create(stream, out var result);

			Assert.Null(codec);
			Assert.NotEqual(SKCodecResult.Success, result);

			// Failed Create disposed the flat wrapper, closing the .NET stream exactly once.
			Assert.True(stream.IsDisposed);
			Assert.Equal(1, dotnet.DisposeCount);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(handle, stream);
		}

		[SkippableFact]
		public unsafe void FrontBufferedCodecWithoutOwnershipDoesNotCloseUnderlyingStream()
		{
			// disposeUnderlyingStream:false — the user keeps ownership of the .NET stream. The nested
			// SKManagedStream is still disposed by codec teardown, but it must NOT close the user's .NET
			// stream. We then close it ourselves to prove the count is driven solely by the user, not the
			// codec chain.
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			var dotnet = new NonSeekableGatedCountingStream(bytes);
			var fb = new SKFrontBufferedManagedStream(dotnet, SKCodec.MinBufferedBytesNeeded, false);
			var fbHandle = fb.Handle;
			var codec = SKCodec.Create(fb);
			try
			{
				Assert.NotNull(codec);
				Assert.False(fb.OwnsHandle);
				Assert.True(fb.IgnorePublicDispose);
				Assert.Equal(SKCodecResult.Success, codec.GetPixels(out _));
			}
			finally
			{
				codec.Dispose();
			}

			// Codec teardown disposed the wrappers but, lacking ownership, left the .NET stream open.
			Assert.True(fb.IsDisposed);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKFrontBufferedManagedStream>(fbHandle, fb);
			Assert.Equal(0, dotnet.DisposeCount);

			// The user is the only owner: closing now is the first and only close.
			dotnet.Dispose();
			Assert.Equal(1, dotnet.DisposeCount);
		}

		// A seekable .NET Stream over a byte buffer that (a) counts how many times it is disposed and
		// (b) can park the FIRST read after Arm() until released — used to drive a deterministic
		// "public Dispose while the native lazy read is in-flight" race and to count exact teardown.
		private sealed class GatedCountingStream : Stream
		{
			private readonly MemoryStream inner;
			private ManualResetEventSlim readEntered;
			private ManualResetEventSlim releaseRead;
			private int gateArmed;
			private int disposeCount;
			private int firstDisposeThreadId;

			public GatedCountingStream(byte[] bytes) => inner = new MemoryStream(bytes, writable: false);

			public int DisposeCount => Volatile.Read(ref disposeCount);

			// Managed thread id of whoever closed the .NET stream first. Lets a test prove the codec
			// owner-teardown — not the ignored public Dispose — was the actual disposer.
			public int FirstDisposeThreadId => Volatile.Read(ref firstDisposeThreadId);

			public void Arm(ManualResetEventSlim entered, ManualResetEventSlim release)
			{
				readEntered = entered;
				releaseRead = release;
				Volatile.Write(ref gateArmed, 1);
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (Interlocked.CompareExchange(ref gateArmed, 0, 1) == 1)
				{
					readEntered.Set();
					releaseRead.Wait();
				}
				return inner.Read(buffer, offset, count);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (Interlocked.Increment(ref disposeCount) == 1)
						Volatile.Write(ref firstDisposeThreadId, Environment.CurrentManagedThreadId);
					// Actually close the backing buffer: a premature close during an in-flight read
					// then surfaces as an ObjectDisposedException from the parked inner.Read.
					inner.Dispose();
				}
				base.Dispose(disposing);
			}

			public override bool CanRead => true;
			public override bool CanSeek => true;
			public override bool CanWrite => false;
			public override long Length => inner.Length;
			public override long Position { get => inner.Position; set => inner.Position = value; }
			public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
			public override void Flush() => inner.Flush();
			public override void SetLength(long value) => throw new NotSupportedException();
			public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
		}

		// A NON-SEEKABLE .NET Stream over a byte buffer that (a) counts how many times it is disposed,
		// (b) records the disposing thread id, and (c) can park the first read after Arm() until
		// released. Non-seekable so SKCodec.Create routes it through the SKFrontBufferedManagedStream
		// (nested SKManagedStream) path — a different teardown graph from the flat seekable wrapper.
		private sealed class NonSeekableGatedCountingStream : Stream
		{
			private readonly MemoryStream inner;
			private ManualResetEventSlim readEntered;
			private ManualResetEventSlim releaseRead;
			private int gateArmed;
			private int disposeCount;
			private int firstDisposeThreadId;

			public NonSeekableGatedCountingStream(byte[] bytes) => inner = new MemoryStream(bytes, writable: false);

			public int DisposeCount => Volatile.Read(ref disposeCount);

			public int FirstDisposeThreadId => Volatile.Read(ref firstDisposeThreadId);

			public void Arm(ManualResetEventSlim entered, ManualResetEventSlim release)
			{
				readEntered = entered;
				releaseRead = release;
				Volatile.Write(ref gateArmed, 1);
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (Interlocked.CompareExchange(ref gateArmed, 0, 1) == 1)
				{
					readEntered.Set();
					releaseRead.Wait();
				}
				return inner.Read(buffer, offset, count);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (Interlocked.Increment(ref disposeCount) == 1)
						Volatile.Write(ref firstDisposeThreadId, Environment.CurrentManagedThreadId);
					inner.Dispose();
				}
				base.Dispose(disposing);
			}

			public override bool CanRead => true;
			public override bool CanSeek => false;
			public override bool CanWrite => false;
			public override long Length => throw new NotSupportedException();
			public override long Position { get => inner.Position; set => throw new NotSupportedException(); }
			public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
			public override void Flush() => throw new NotSupportedException();
			public override void SetLength(long value) => throw new NotSupportedException();
			public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
		}

		// A MemoryStream that records disposal and rejects (rather than silently swallowing)
		// any write that arrives after the stream has been closed — so a premature close in
		// the owned-wstream teardown ordering would surface as WroteAfterClose / an exception.
		private sealed class LifecycleTrackingStream : Stream
		{
			private readonly MemoryStream inner = new MemoryStream();
			private bool closed;

			public bool IsDisposed => closed;
			public long BytesWritten { get; private set; }
			public bool WroteAfterClose { get; private set; }

			public override bool CanRead => !closed && inner.CanRead;
			public override bool CanSeek => !closed && inner.CanSeek;
			public override bool CanWrite => !closed && inner.CanWrite;
			public override long Length => inner.Length;

			public override long Position
			{
				get => inner.Position;
				set => inner.Position = value;
			}

			public override void Flush() => inner.Flush();

			public override int Read(byte[] buffer, int offset, int count) =>
				inner.Read(buffer, offset, count);

			public override long Seek(long offset, SeekOrigin origin) =>
				inner.Seek(offset, origin);

			public override void SetLength(long value) => inner.SetLength(value);

			public override void Write(byte[] buffer, int offset, int count)
			{
				if (closed)
				{
					WroteAfterClose = true;
					throw new ObjectDisposedException(nameof(LifecycleTrackingStream));
				}

				BytesWritten += count;
				inner.Write(buffer, offset, count);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
					closed = true;

				// keep the underlying buffer readable for post-dispose assertions
				base.Dispose(disposing);
			}
		}
	}
}

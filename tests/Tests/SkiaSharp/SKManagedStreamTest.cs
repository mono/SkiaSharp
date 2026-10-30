using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests
{
	public class SKManagedStreamTest : SKTest
	{
		[Fact]
		public void DotNetStreamIsCollected()
		{
			var dotnet = CreateTestStream();
			var stream = new SKManagedStream(dotnet, true);

			Assert.Equal(0, dotnet.Position);

			stream.Dispose();

			Assert.Throws<ObjectDisposedException>(() => dotnet.Position);
		}

		[Fact]
		public void DotNetStreamIsNotCollected()
		{
			var dotnet = CreateTestStream();
			var stream = new SKManagedStream(dotnet, false);

			Assert.Equal(0, dotnet.Position);

			stream.Dispose();

			Assert.Equal(0, dotnet.Position);
		}

		[Fact]
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

		[Fact]
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
			SKHandleDictionaryTestHelpers.AssertDeregistered(handle, stream);
		}

		[Fact]
		public void StreamIsCollectedEvenWhenNotProperlyDisposed()
		{
			SkipOnMono();

			DoWork(out var handle, out var weakStream);

			CollectGarbage();

			SKHandleDictionaryTestHelpers.AssertCollected(handle, weakStream);

			[MethodImpl(MethodImplOptions.NoInlining)]
			void DoWork(out IntPtr streamHandle, out WeakReference streamRef)
			{
				var dotnet = CreateTestStream();
				var stream = new SKManagedStream(dotnet, true);
				SKHandleDictionaryTestHelpers.Track(stream, out streamHandle, out streamRef);
			}
		}

		[Fact]
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

		[Theory]
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

			var buffer = new byte[Math.Max(dataSize * 2, readSize)];

			var actualReadSize = managedStream.Read(buffer, readSize);

			Assert.Equal(expectedReadSize, actualReadSize);
			Assert.Equal(finalPos, stream.Position);
			Assert.Equal(finalPos, managedStream.Position);
			Assert.Equal(data.Take(readSize), buffer.Take(actualReadSize));
			Assert.All(buffer.Skip(actualReadSize), i => Assert.Equal(0, i));
		}

		[Theory]
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

		[Theory]
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

		[Fact]
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

		[Fact]
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

		[Fact]
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

		[Fact]
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
							pageCanvas.DrawText("Text", 0, 5 * i, SKTextAlign.Left, pair.Font, pair.Paint);
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

		[Fact]
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
			SKHandleDictionaryTestHelpers.AssertDeregistered(handle, stream);
		}

		[Fact]
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
			SKHandleDictionaryTestHelpers.AssertDeregistered(handle, stream);
		}

		[Fact]
		public unsafe void StreamLosesOwnershipAndCanBeGarbageCollected()
		{
			SkipOnMono();

			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			DoWork(out var codecH, out var weakCodec, out var streamH, out var weakStream);

			CollectGarbage();

			SKHandleDictionaryTestHelpers.AssertCollected(streamH, weakStream);
			SKHandleDictionaryTestHelpers.AssertCollected(codecH, weakCodec);

			[MethodImpl(MethodImplOptions.NoInlining)]
			void DoWork(out IntPtr codecHandle, out WeakReference codecRef, out IntPtr streamHandle, out WeakReference streamRef)
			{
				var codec = CreateCodec(out streamHandle, out streamRef);
				SKHandleDictionaryTestHelpers.Track(codec, out codecHandle, out codecRef);

				CollectGarbage();

				Assert.Equal(SKCodecResult.Success, codec.GetPixels(out var pixels));
				Assert.NotEmpty(pixels);

				Assert.True(SKObject.GetInstance<SKManagedStream>(streamHandle, out var stream));
				Assert.False(stream.OwnsHandle);
				Assert.True(stream.IgnorePublicDispose);

				GC.KeepAlive(codec);
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			SKCodec CreateCodec(out IntPtr streamHandle, out WeakReference streamRef)
			{
				var stream = new SKManagedStream(new MemoryStream(bytes), true);
				SKHandleDictionaryTestHelpers.Track(stream, out streamHandle, out streamRef);

				Assert.True(stream.OwnsHandle);
				Assert.False(stream.IgnorePublicDispose);
				Assert.True(SKObject.GetInstance<SKManagedStream>(streamHandle, out _));

				return SKCodec.Create(stream);
			}
		}

		[Fact]
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

		[Fact]
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

		[Fact]
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

		[Fact]
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

		[Fact]
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

		[Fact]
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
			SKHandleDictionaryTestHelpers.AssertDeregistered(handle, dupe2);

			dupe1.Dispose();
		}

		[Fact]
		public void DuplicateStreamIsCollected()
		{
			SkipOnMono();

			DoWork(out var handle, out var weakDupe);

			CollectGarbage();

			SKHandleDictionaryTestHelpers.AssertCollected(handle, weakDupe);

			[MethodImpl(MethodImplOptions.NoInlining)]
			void DoWork(out IntPtr dupeHandle, out WeakReference dupeRef)
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

				SKHandleDictionaryTestHelpers.Track(dupe2, out dupeHandle, out dupeRef);
			}
		}

		[Fact]
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

		[Fact]
		public void NonSeekableStreamDuplicateAndForkReturnNull()
		{
			var dotnet = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			var nonSeekable = new NonSeekableReadOnlyStream(dotnet);
			var stream = new SKManagedStream(nonSeekable);

			Assert.Null(stream.Duplicate());
			Assert.Null(stream.Fork());

			stream.Dispose();
		}

		[Fact]
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

		[Fact]
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

		[Fact]
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

		[Fact]
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

		[Fact]
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

		[Fact]
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

		[Fact]
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

		[Fact]
		public unsafe void ThrowingManagedStreamCallbacksDoNotCrashAndTerminateLoops()
		{
			using var stream = new ThrowingAbstractManagedStream();

			Assert.True(stream.IsAtEnd);
			Assert.False(stream.HasPosition);
			Assert.False(stream.HasLength);
			// Public reads and metadata queries remain native-backed and contain callback errors.
			Assert.Equal(IntPtr.Zero, SkiaApi.sk_stream_get_position(stream.Handle));
			Assert.Equal(IntPtr.Zero, SkiaApi.sk_stream_get_length(stream.Handle));
			Assert.Equal(0, stream.Position);
			Assert.Equal(0, stream.Length);
			Assert.Equal(0L, stream.Position64);
			Assert.Equal(0L, stream.Length64);
			Assert.False(stream.Rewind());
			Assert.False(stream.Seek(4));
			Assert.False(stream.Move(4));
			Assert.Null(stream.Duplicate());
			Assert.Null(stream.Fork());

			var buf = new byte[16];
			Assert.Equal(0, stream.Read(buf, buf.Length));
			fixed (byte* p = buf)
			{
				Assert.Equal(IntPtr.Zero, SkiaApi.sk_stream_read(stream.Handle, p, (IntPtr)buf.Length));
				Assert.Equal(0, stream.Peek((IntPtr)p, buf.Length));
			}

			// The explicit-length native factory contains callback errors.
			using var data = SKData.Create(stream, 1);
			Assert.Null(data);
			Assert.True(stream.IsAtEnd);

			// The lengthless factory invokes managed overrides without crossing reverse P/Invoke.
			Assert.Throws<InvalidOperationException>(() => SKData.Create((SKStream)stream));
		}

		[Fact]
		public void ThrowingOrDisposedUnderlyingStream_DoesNotAbortProcessInNativeCallbacks()
		{
			var ms = new MemoryStream(new byte[] { 1, 2, 3, 4 });
			using var managed = new SKManagedStream(ms);
			ms.Dispose();

			Assert.Equal(0, managed.Read(new byte[4], 4));
			Assert.Equal(0, managed.Skip(4));
			Assert.True(managed.IsAtEnd);
			Assert.False(managed.Rewind());
			Assert.False(managed.Seek(0));
			Assert.False(managed.Move(1));
			Assert.Equal(0, managed.Position);
			Assert.Equal(0, managed.Length);
			Assert.Null(managed.Duplicate());
			Assert.Null(managed.Fork());
		}

		[Fact]
		public void FailedForkAfterCachedSnapshotReleasesNativeDuplicateAndSnapshotReference()
		{
			var ms = new MemoryStream(new byte[] { 1, 2, 3, 4 });
			using var managed = new SKManagedStream(ms);
			using (var dupe = managed.Duplicate())
			{
				Assert.NotNull(dupe);
				Assert.NotNull(managed.snapshotData);
				Assert.Equal(2, ((ISKReferenceCounted)managed.snapshotData).GetReferenceCount());
			}

			var snapshot = managed.snapshotData;
			var snapshotHandle = snapshot.Handle;
			Assert.Equal(1, ((ISKReferenceCounted)snapshot).GetReferenceCount());

			// Disposing the underlying MemoryStream makes Position throw ObjectDisposedException
			// after OnDuplicate() has already allocated a native SkMemoryStream from cached snapshotData.
			ms.Dispose();
			Assert.Null(managed.Fork());
			Assert.Equal(1, ((ISKReferenceCounted)snapshot).GetReferenceCount());

			managed.Dispose();
			Assert.True(snapshot.IsDisposed);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKData>(snapshotHandle, snapshot);

			// Also verify with a custom seekable stream whose Position getter throws after snapshot caching.
			using var failingPosStream = new FailingPositionStream(new byte[] { 1, 2, 3, 4 });
			using var managedWithFailingPos = new SKManagedStream(failingPosStream);
			using (var dupe2 = managedWithFailingPos.Duplicate())
			{
				Assert.NotNull(dupe2);
			}

			var snapshot2 = managedWithFailingPos.snapshotData;
			Assert.NotNull(snapshot2);
			Assert.Equal(1, ((ISKReferenceCounted)snapshot2).GetReferenceCount());

			failingPosStream.ThrowOnPosition = true;
			Assert.Null(managedWithFailingPos.Fork());
			Assert.Equal(1, ((ISKReferenceCounted)snapshot2).GetReferenceCount());

			// Also verify SKAbstractManagedStream.OnFork when OnCreateNew succeeds and OnGetPosition throws.
			using var abstractForkStream = new ThrowingPositionOnForkAbstractStream();
			Assert.Null(abstractForkStream.Fork());
		}

		[Fact]
		public void ThrowingDisposeInManagedStreamClearsHandleAndDeregistersOnNativeDestroy()
		{
			var inner = new ThrowingDisposeStream(new byte[] { 1, 2, 3, 4 });
			var stream = new SKManagedStream(inner, disposeManagedStream: true);
			var handle = stream.Handle;

			Assert.Null(SKCodec.Create(stream));
			Assert.Equal(IntPtr.Zero, stream.Handle);
			Assert.True(stream.IsDisposed);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(handle, stream);
		}

		private sealed class ThrowingDisposeStream : MemoryStream
		{
			public ThrowingDisposeStream(byte[] buffer)
				: base(buffer)
			{
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing)
					throw new InvalidOperationException();
			}
		}

		private sealed class ThrowingPositionOnForkAbstractStream : SKAbstractManagedStream
		{
			protected internal override IntPtr OnRead(IntPtr buffer, IntPtr size) => IntPtr.Zero;
			protected internal override IntPtr OnPeek(IntPtr buffer, IntPtr size) => IntPtr.Zero;
			protected internal override bool OnIsAtEnd() => true;
			protected internal override bool OnHasPosition() => true;
			protected internal override bool OnHasLength() => true;
			protected internal override bool OnRewind() => true;
			protected internal override IntPtr OnGetPosition() => throw new InvalidOperationException();
			protected internal override IntPtr OnGetLength() => IntPtr.Zero;
			protected internal override bool OnSeek(IntPtr position) => true;
			protected internal override bool OnMove(int offset) => true;
			protected internal override IntPtr OnCreateNew() => SkiaApi.sk_memorystream_new();
		}

		private sealed class FailingPositionStream : MemoryStream
		{
			public FailingPositionStream(byte[] buffer)
				: base(buffer)
			{
			}

			public bool ThrowOnPosition { get; set; }

			public override long Position
			{
				get => ThrowOnPosition ? throw new InvalidOperationException() : base.Position;
				set => base.Position = value;
			}
		}

		[Fact]
		public void DisposedMemoryStreamInSKManagedStreamTerminatesSKDataCreate()
		{
			var dotnet = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			using var stream = new SKManagedStream(dotnet);
			var length = stream.Length;
			dotnet.Dispose();

			using var data = SKData.Create(stream, length);
			Assert.Null(data);
			Assert.True(stream.IsAtEnd);
		}

		[Fact]
		public unsafe void RecoverableManagedStreamErrorStateIsSetPreservedAndCleared()
		{
			using var stream = new RecoverableAbstractManagedStream();
			var buf = new byte[4];

			Assert.False(stream.IsAtEnd);

			// Read exception sets error state so IsAtEnd returns true even though OnIsAtEnd returns false
			stream.ThrowOnRead = true;
			Assert.Equal(0, stream.Read(buf, buf.Length));
			Assert.True(stream.IsAtEnd);

			// Failed Rewind/Seek/Move (returning false) preserves error state
			stream.RepositionResult = false;
			Assert.False(stream.Rewind());
			Assert.True(stream.IsAtEnd);
			Assert.False(stream.Seek(0));
			Assert.True(stream.IsAtEnd);
			Assert.False(stream.Move(0));
			Assert.True(stream.IsAtEnd);

			// Successful Rewind clears error state
			stream.RepositionResult = true;
			Assert.True(stream.Rewind());
			Assert.False(stream.IsAtEnd);

			// Peek exception sets error state, and successful Seek clears it
			stream.ThrowOnPeek = true;
			fixed (byte* p = buf)
			{
				Assert.Equal(0, stream.Peek((IntPtr)p, buf.Length));
			}
			Assert.True(stream.IsAtEnd);
			Assert.True(stream.Seek(0));
			Assert.False(stream.IsAtEnd);

			// Read exception sets error state again, and successful Move clears it
			Assert.Equal(0, stream.Read(buf, buf.Length));
			Assert.True(stream.IsAtEnd);
			Assert.True(stream.Move(0));
			Assert.False(stream.IsAtEnd);
		}

		private sealed class RecoverableAbstractManagedStream : SKAbstractManagedStream
		{
			public bool ThrowOnRead { get; set; }
			public bool ThrowOnPeek { get; set; }
			public bool RepositionResult { get; set; } = true;

			protected internal override IntPtr OnRead(IntPtr buffer, IntPtr size) =>
				ThrowOnRead ? throw new InvalidOperationException() : IntPtr.Zero;

			protected internal override IntPtr OnPeek(IntPtr buffer, IntPtr size) =>
				ThrowOnPeek ? throw new InvalidOperationException() : IntPtr.Zero;

			protected internal override bool OnIsAtEnd() => false;
			protected internal override bool OnHasPosition() => true;
			protected internal override bool OnHasLength() => true;
			protected internal override bool OnRewind() => RepositionResult;
			protected internal override IntPtr OnGetPosition() => IntPtr.Zero;
			protected internal override IntPtr OnGetLength() => (IntPtr)16;
			protected internal override bool OnSeek(IntPtr position) => RepositionResult;
			protected internal override bool OnMove(int offset) => RepositionResult;
			protected internal override IntPtr OnCreateNew() => IntPtr.Zero;
		}

		private sealed class ThrowingAbstractManagedStream : SKAbstractManagedStream
		{
			protected internal override IntPtr OnRead(IntPtr buffer, IntPtr size) =>
				throw new InvalidOperationException();

			protected internal override IntPtr OnPeek(IntPtr buffer, IntPtr size) =>
				throw new InvalidOperationException();

			protected internal override bool OnIsAtEnd() =>
				throw new InvalidOperationException();

			protected internal override bool OnHasPosition() =>
				throw new InvalidOperationException();

			protected internal override bool OnHasLength() =>
				throw new InvalidOperationException();

			protected internal override bool OnRewind() =>
				throw new InvalidOperationException();

			protected internal override IntPtr OnGetPosition() =>
				throw new InvalidOperationException();

			protected internal override IntPtr OnGetLength() =>
				throw new InvalidOperationException();

			protected internal override bool OnSeek(IntPtr position) =>
				throw new InvalidOperationException();

			protected internal override bool OnMove(int offset) =>
				throw new InvalidOperationException();

			protected internal override IntPtr OnCreateNew() =>
				throw new InvalidOperationException();

			protected internal override IntPtr OnDuplicate() =>
				throw new InvalidOperationException();

			protected internal override IntPtr OnFork() =>
				throw new InvalidOperationException();
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

		[Theory]
		[InlineData(true, 1)]
		[InlineData(true, 7)]
		[InlineData(true, 64)]
		[InlineData(false, 1)]
		[InlineData(false, 7)]
		[InlineData(false, 64)]
		public void ShortReadStream_ReadsFullRequestedCount(bool canSeek, int maxBytesPerRead)
		{
			var data = CreateSequentialBytes(1024);
			using var inner = new ConfigurableTestStream(data, canSeek: canSeek, maxBytesPerRead: maxBytesPerRead);
			using var managed = new SKManagedStream(inner);

			var buffer = new byte[500];
			var bytesRead = managed.Read(buffer, buffer.Length);

			Assert.Equal(500, bytesRead);
			Assert.Equal(data.Take(500), buffer);
			Assert.False(managed.IsAtEnd);

			var rest = new byte[600];
			var restRead = managed.Read(rest, rest.Length);

			Assert.Equal(524, restRead);
			Assert.Equal(data.Skip(500), rest.Take(524));
			Assert.True(managed.IsAtEnd);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(7)]
		[InlineData(64)]
		public unsafe void ShortReadStream_PeekReadsFullRequestedCountAndRestoresState(int maxBytesPerRead)
		{
			var data = CreateSequentialBytes(256);
			using var inner = new ConfigurableTestStream(data, canSeek: true, maxBytesPerRead: maxBytesPerRead);
			using var managed = new SKManagedStream(inner);

			var peekBuffer = new byte[100];
			fixed (byte* p = peekBuffer)
			{
				var peeked = managed.Peek((IntPtr)p, peekBuffer.Length);
				Assert.Equal(100, peeked);
			}

			Assert.Equal(0, inner.Position);
			Assert.Equal(0, managed.Position);
			Assert.False(managed.IsAtEnd);
			Assert.Equal(data.Take(100), peekBuffer);
		}

		[Fact]
		public unsafe void PeekReachingEof_DoesNotLatchIsAtEnd()
		{
			var data = CreateSequentialBytes(16);
			using var inner = new ConfigurableTestStream(data, canSeek: true, maxBytesPerRead: 5);
			using var managed = new SKManagedStream(inner);

			var peekBuffer = new byte[32];
			fixed (byte* p = peekBuffer)
			{
				var peeked = managed.Peek((IntPtr)p, peekBuffer.Length);
				Assert.Equal(16, peeked);
			}

			Assert.Equal(0, managed.Position);
			Assert.False(managed.IsAtEnd);

			var readBuffer = new byte[16];
			var read = managed.Read(readBuffer, readBuffer.Length);
			Assert.Equal(16, read);
			Assert.Equal(data, readBuffer);
			Assert.True(managed.IsAtEnd);
		}

		[Theory]
		[InlineData(true, 1)]
		[InlineData(true, 7)]
		[InlineData(false, 1)]
		[InlineData(false, 7)]
		public void ShortReadStream_SkipAdvancesFullRequestedCount(bool canSeek, int maxBytesPerRead)
		{
			var data = CreateSequentialBytes(256);
			using var inner = new ConfigurableTestStream(data, canSeek: canSeek, maxBytesPerRead: maxBytesPerRead);
			using var managed = new SKManagedStream(inner);

			var skipped = managed.Skip(100);
			Assert.Equal(100, skipped);
			Assert.False(managed.IsAtEnd);

			var buffer = new byte[50];
			var read = managed.Read(buffer, buffer.Length);
			Assert.Equal(50, read);
			Assert.Equal(data.Skip(100).Take(50), buffer);
		}

		[Fact]
		public void NonSeekableStream_IsAtEndRemainsFalseUntilActualEof()
		{
			var data = CreateSequentialBytes(100);
			using var inner = new ConfigurableTestStream(data, canSeek: false);
			using var managed = new SKManagedStream(inner);

			var chunk = new byte[25];
			Assert.Equal(25, managed.Read(chunk, 25));
			Assert.False(managed.IsAtEnd);

			Assert.Equal(25, managed.Read(chunk, 25));
			Assert.False(managed.IsAtEnd);

			Assert.Equal(25, managed.Read(chunk, 25));
			Assert.False(managed.IsAtEnd);

			// Exact final chunk: non-seekable stream has not returned 0 yet, or if a subsequent read returns 0 it is at end
			Assert.Equal(25, managed.Read(chunk, 25));
			Assert.Equal(0, managed.Read(chunk, 25));
			Assert.True(managed.IsAtEnd);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(40)]
		[InlineData(99)]
		public void EarlyEndingSeekableStream_SetsIsAtEndWhenReadHitsEof(int earlyEnd)
		{
			var data = CreateSequentialBytes(100);
			using var inner = new ConfigurableTestStream(data, canSeek: true, maxBytesPerRead: 7, earlyEndPosition: earlyEnd);
			using var managed = new SKManagedStream(inner);

			var buffer = new byte[100];
			var read = managed.Read(buffer, buffer.Length);

			Assert.Equal(earlyEnd, read);
			Assert.Equal(data.Take(earlyEnd), buffer.Take(earlyEnd));
			Assert.True(managed.IsAtEnd);
		}

		[Theory]
		[InlineData(int.MaxValue)]
		[InlineData(8)]
		public void EarlyEndingSeekableStream_WebpExifHeaderDoesNotInfiniteLoop(int maxBytesPerRead)
		{
			// Construct a RIFF WEBP header with a VP8X chunk that sets EXIF_FLAG (0x08).
			// In SkWebpCodec::get_header_from_stream, EXIF_FLAG triggers:
			//   do { bytes_read = stream->read(buffer, 4096); } while (!stream->isAtEnd());
			var webpBytes = new byte[64];
			"RIFF"u8.CopyTo(webpBytes.AsSpan(0, 4));
			BitConverter.GetBytes(100u).CopyTo(webpBytes, 4);
			"WEBP"u8.CopyTo(webpBytes.AsSpan(8, 4));
			"VP8X"u8.CopyTo(webpBytes.AsSpan(12, 4));
			BitConverter.GetBytes(10u).CopyTo(webpBytes, 16);
			webpBytes[20] = 0x08; // EXIF_FLAG

			using var inner = new ConfigurableTestStream(
				webpBytes,
				canSeek: true,
				maxBytesPerRead: maxBytesPerRead,
				earlyEndPosition: 35,
				reportedLength: 64,
				maxZeroReadsAfterEof: 10);

			using var codec = SKCodec.Create(inner, out _);

			Assert.False(inner.InfiniteLoopDetected);
		}

		[Fact]
		public void RewindSeekAndMove_ResetIsAtEndAfterEarlyEof()
		{
			var data = CreateSequentialBytes(100);
			using var inner = new ConfigurableTestStream(data, canSeek: true, earlyEndPosition: 40);
			using var managed = new SKManagedStream(inner);

			var buffer = new byte[50];
			Assert.Equal(40, managed.Read(buffer, 50));
			Assert.True(managed.IsAtEnd);

			Assert.True(managed.Rewind());
			Assert.Equal(0, managed.Position);
			Assert.False(managed.IsAtEnd);

			Assert.Equal(40, managed.Read(buffer, 50));
			Assert.True(managed.IsAtEnd);

			Assert.True(managed.Seek(10));
			Assert.Equal(10, managed.Position);
			Assert.False(managed.IsAtEnd);

			Assert.Equal(30, managed.Read(buffer, 50));
			Assert.True(managed.IsAtEnd);

			Assert.True(managed.Move(-20));
			Assert.Equal(20, managed.Position);
			Assert.False(managed.IsAtEnd);
		}

		[Fact]
		public void SeekAndMove_OutOfBoundsClampsToStreamBounds()
		{
			var data = CreateSequentialBytes(100);
			using var inner = new ConfigurableTestStream(data, canSeek: true);
			using var managed = new SKManagedStream(inner);

			Assert.True(managed.Seek(50));
			Assert.Equal(50, managed.Position);

			// Moving before beginning clamps to 0 without attempting negative Position
			Assert.True(managed.Move(-100));
			Assert.False(inner.NegativePositionAttempted);
			Assert.Equal(0, managed.Position);

			Assert.True(managed.Move(int.MinValue));
			Assert.False(inner.NegativePositionAttempted);
			Assert.Equal(0, managed.Position);

			// Moving past end clamps to Length
			Assert.True(managed.Move(500));
			Assert.Equal(100, managed.Position);
			Assert.True(managed.IsAtEnd);

			Assert.True(managed.Seek(10));
			Assert.False(managed.IsAtEnd);
			Assert.True(managed.Move(int.MaxValue));
			Assert.Equal(100, managed.Position);
			Assert.True(managed.IsAtEnd);

			// Positive move when Position is near long.MaxValue saturates and clamps to Length instead of wrapping negative
			inner.Position = long.MaxValue;
			Assert.True(managed.Move(1));
			Assert.Equal(100, inner.Position);
			Assert.Equal(100, managed.Position);
			Assert.True(managed.IsAtEnd);

			// Seeking past end clamps to Length
			Assert.True(managed.Seek(500));
			Assert.Equal(100, managed.Position);
			Assert.True(managed.IsAtEnd);
		}

		[Fact]
		public void LargeSkipAndRead_UseBoundedBufferAllocation()
		{
			var data = CreateSequentialBytes(128);
			using var inner = new ConfigurableTestStream(data, canSeek: false);
			using var managed = new SKManagedStream(inner);

			var skipped = managed.Skip(10_000_000);
			Assert.Equal(128, skipped);
			Assert.True(inner.MaxRequestedReadCount <= SKData.CopyBufferSize);
			Assert.True(managed.IsAtEnd);
		}

		[Theory]
		[InlineData((long)int.MaxValue + 1)]
		[InlineData((long)uint.MaxValue)]
		public void NativeLargeSkipPreservesUnsignedRequestWithBoundedBuffer(long requested)
		{
			var data = CreateSequentialBytes(128);
			using var inner = new ConfigurableTestStream(data, canSeek: false);
			using var managed = new SKManagedStream(inner);

			var skipped = SkiaApi.sk_stream_skip(managed.Handle, Utils.ToNativeSize((ulong)requested));

			Assert.Equal((ulong)data.Length, Utils.FromNativeSize(skipped));
			Assert.InRange(inner.MaxRequestedReadCount, 1, SKData.CopyBufferSize);
			Assert.True(managed.IsAtEnd);
		}

		[Theory]
		[InlineData(-5)]
		[InlineData(int.MaxValue)]
		public unsafe void HostileStream_InvalidReadReturnValues_DoNotOverrunBuffer(int returnMode)
		{
			using var hostile = new HostileReadReturnStream(returnMode);
			using var managed = new SKManagedStream(hostile);

			var guardBuffer = new byte[32];
			for (var i = 0; i < guardBuffer.Length; i++)
				guardBuffer[i] = 0x55;

			fixed (byte* p = guardBuffer)
			{
				// Request only 8 bytes into the 32-byte guard buffer; bytes [8..31] must remain untouched (0x55)
				var read = managed.Read((IntPtr)p, 8);
				Assert.True(read <= 8);
			}

			for (var i = 8; i < guardBuffer.Length; i++)
				Assert.Equal(0x55, guardBuffer[i]);
			Assert.True(managed.IsAtEnd);
		}

		[Theory]
		[InlineData(7, -1, 100)]
		[InlineData(7, 45, 45)]
		[InlineData(int.MaxValue, 45, 45)]
		public void DuplicateAndFork_WorkWithShortReadsAndEarlyEndingSeekableStream(int maxBytesPerRead, int earlyEnd, int expectedLength)
		{
			var data = CreateSequentialBytes(100);
			using var inner = new ConfigurableTestStream(data, canSeek: true, maxBytesPerRead: maxBytesPerRead, earlyEndPosition: earlyEnd);
			using var managed = new SKManagedStream(inner);

			var initialRead = new byte[10];
			Assert.Equal(10, managed.Read(initialRead, 10));

			if (earlyEnd >= 0)
			{
				var zeroReads = inner.ZeroReadCount;
				using var incompleteDuplicate = managed.Duplicate();
				Assert.Null(incompleteDuplicate);
				Assert.Null(managed.snapshotData);
				Assert.Equal(10, inner.Position);
				Assert.Equal(zeroReads + 1, inner.ZeroReadCount);
				Assert.False(managed.IsAtEnd);

				using var incompleteFork = managed.Fork();
				Assert.Null(incompleteFork);
				Assert.Null(managed.snapshotData);
				Assert.Equal(10, inner.Position);
				Assert.Equal(zeroReads + 2, inner.ZeroReadCount);
				Assert.False(managed.IsAtEnd);
				return;
			}

			using var dupe = managed.Duplicate();
			Assert.NotNull(dupe);
			Assert.Equal(0, dupe.Position);
			Assert.Equal(expectedLength, dupe.Length);

			var dupeBytes = new byte[100];
			Assert.Equal(expectedLength, dupe.Read(dupeBytes, 100));
			Assert.Equal(data.Take(expectedLength), dupeBytes.Take(expectedLength));

			using var forked = managed.Fork();
			Assert.NotNull(forked);
			Assert.Equal(10, forked.Position);

			var forkBytes = new byte[100];
			Assert.Equal(expectedLength - 10, forked.Read(forkBytes, 100));
			Assert.Equal(data.Skip(10).Take(expectedLength - 10), forkBytes.Take(expectedLength - 10));
		}

		[Fact]
		public void UnknownLengthSnapshotReadsToEofAndPreservesSourcePosition()
		{
			var data = CreateSequentialBytes(100);
			using var inner = new ConfigurableTestStream(data, maxBytesPerRead: 7, earlyEndPosition: 45, reportedLength: -1);
			inner.Position = 10;
			using var managed = new SKManagedStream(inner);

			using var duplicate = managed.Duplicate();
			Assert.NotNull(duplicate);
			Assert.Equal(45, duplicate.Length);
			var actual = new byte[45];
			Assert.Equal(actual.Length, duplicate.Read(actual, actual.Length));
			Assert.Equal(data.Take(45), actual);
			Assert.Equal(10, inner.Position);

			using var fork = managed.Fork();
			Assert.NotNull(fork);
			Assert.Equal(10, fork.Position);
			Assert.Equal(10, inner.Position);

			inner.Position = 95;
			Assert.Null(managed.Fork());
			Assert.Equal(95, inner.Position);
		}

		[Theory]
		[InlineData("color-wheel.png", true, 1)]
		[InlineData("color-wheel.png", true, 7)]
		[InlineData("color-wheel.png", false, 7)]
		[InlineData("color-wheel.png", false, int.MaxValue)]
		[InlineData("baboon.jpg", true, 7)]
		[InlineData("baboon.jpg", false, 7)]
		[InlineData("animated-heart.gif", true, 7)]
		[InlineData("animated-heart.gif", false, 7)]
		[InlineData("tomato.bmp", true, 7)]
		[InlineData("tomato.bmp", false, 7)]
		public void CodecAndBitmapDecode_PermutationsSucceed(string imageName, bool canSeek, int maxBytesPerRead)
		{
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, imageName));
			using var expectedBitmap = SKBitmap.Decode(bytes);
			Assert.NotNull(expectedBitmap);

			using var inner = new ConfigurableTestStream(bytes, canSeek: canSeek, maxBytesPerRead: maxBytesPerRead);
			using var actualBitmap = SKBitmap.Decode(inner);

			Assert.NotNull(actualBitmap);
			Assert.Equal(expectedBitmap.Width, actualBitmap.Width);
			Assert.Equal(expectedBitmap.Height, actualBitmap.Height);
			Assert.Equal(expectedBitmap.Bytes, actualBitmap.Bytes);
		}

		[Theory]
		[InlineData(true, 1)]
		[InlineData(true, 7)]
		[InlineData(true, int.MaxValue)]
		[InlineData(false, 7)]
		[InlineData(false, int.MaxValue)]
		public void WebpDecode_PermutationsSucceed(bool canSeek, int maxBytesPerRead)
		{
			using var sourceBitmap = new SKBitmap(16, 16);
			using (var canvas = new SKCanvas(sourceBitmap))
			{
				canvas.Clear(SKColors.CornflowerBlue);
			}
			using var encoded = sourceBitmap.Encode(SKEncodedImageFormat.Webp, 90);
			var bytes = encoded.ToArray();

			using var inner = new ConfigurableTestStream(bytes, canSeek: canSeek, maxBytesPerRead: maxBytesPerRead);
			using var decoded = SKBitmap.Decode(inner);

			Assert.NotNull(decoded);
			Assert.Equal(16, decoded.Width);
			Assert.Equal(16, decoded.Height);
		}

		[Theory]
		[InlineData(true, 1)]
		[InlineData(true, 7)]
		[InlineData(true, int.MaxValue)]
		[InlineData(false, 7)]
		[InlineData(false, int.MaxValue)]
		public void PictureDeserialize_PermutationsSucceed(bool canSeek, int maxBytesPerRead)
		{
			using var recorder = new SKPictureRecorder();
			using (var canvas = recorder.BeginRecording(SKRect.Create(50, 50)))
			using (var paint = new SKPaint { Color = SKColors.Green })
			{
				canvas.DrawRect(SKRect.Create(5, 5, 40, 40), paint);
			}
			using var picture = recorder.EndRecording();
			using var data = picture.Serialize();
			var bytes = data.ToArray();

			using var inner = new ConfigurableTestStream(bytes, canSeek: canSeek, maxBytesPerRead: maxBytesPerRead);
			using var deserialized = SKPicture.Deserialize(inner);

			Assert.NotNull(deserialized);
			Assert.Equal(SKRect.Create(50, 50), deserialized.CullRect);
		}

		[Theory]
		[InlineData(true, 7)]
		[InlineData(false, 7)]
		public void TypefaceFromStream_PermutationsSucceed(bool canSeek, int maxBytesPerRead)
		{
			var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
			using var inner = new ConfigurableTestStream(bytes, canSeek: canSeek, maxBytesPerRead: maxBytesPerRead);
			using var typeface = SKTypeface.FromStream(inner);

			Assert.NotNull(typeface);
			Assert.True(typeface.GlyphCount > 0);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public unsafe void EmptyStream_AllOperationsBehaveCorrectly(bool canSeek)
		{
			using var inner = new ConfigurableTestStream(Array.Empty<byte>(), canSeek: canSeek);
			using var managed = new SKManagedStream(inner);

			if (canSeek)
			{
				Assert.True(managed.IsAtEnd);
				Assert.Equal(0, managed.Length);
				Assert.Equal(0, managed.Position);
			}
			else
			{
				Assert.False(managed.IsAtEnd);
			}

			var buf = new byte[8];
			fixed (byte* p = buf)
			{
				Assert.Equal(0, managed.Peek((IntPtr)p, buf.Length));
			}

			Assert.Equal(0, managed.Read(buf, buf.Length));
			Assert.True(managed.IsAtEnd);
			Assert.Equal(0, managed.Skip(16));
			Assert.True(managed.IsAtEnd);

			if (canSeek)
			{
				Assert.True(managed.Rewind());
				Assert.True(managed.IsAtEnd);
				Assert.True(managed.Seek(100));
				Assert.Equal(0, managed.Position);
				Assert.True(managed.Move(-50));
				Assert.Equal(0, managed.Position);

				using var dupe = managed.Duplicate();
				Assert.NotNull(dupe);
				Assert.Equal(0, dupe.Length);
				Assert.True(dupe.IsAtEnd);

				using var fork = managed.Fork();
				Assert.NotNull(fork);
				Assert.Equal(0, fork.Length);
				Assert.True(fork.IsAtEnd);
			}
			else
			{
				Assert.False(managed.Rewind());
				Assert.False(managed.Seek(0));
				Assert.False(managed.Move(0));
				Assert.Null(managed.Duplicate());
				Assert.Null(managed.Fork());
			}

			using (var codecStream = new ConfigurableTestStream(Array.Empty<byte>(), canSeek: canSeek))
				Assert.Null(SKCodec.Create(codecStream));

			using (var picStream = new ConfigurableTestStream(Array.Empty<byte>(), canSeek: canSeek))
				Assert.Null(SKPicture.Deserialize(picStream));

			using (var fontStream = new ConfigurableTestStream(Array.Empty<byte>(), canSeek: canSeek))
				Assert.Null(SKTypeface.FromStream(fontStream));
		}

		[Fact]
		public void EofLatch_ClearsWhenUnderlyingSeekableStreamIsRepositionedOrExtended()
		{
			using var ms = new MemoryStream();
			using var managed = new SKManagedStream(ms);

			var smallBuf = new byte[16];
			Assert.Equal(0, managed.Read(smallBuf, smallBuf.Length));
			Assert.True(managed.IsAtEnd);

			// Extend the underlying MemoryStream and reposition it directly (outside SKManagedStream)
			var payload = CreateSequentialBytes(200_000);
			ms.Write(payload, 0, payload.Length);
			ms.Position = 0;

			Assert.False(managed.IsAtEnd);

			var largeBuf = new byte[payload.Length];
			var read = managed.Read(largeBuf, largeBuf.Length);
			Assert.Equal(payload.Length, read);
			Assert.Equal(payload, largeBuf);
			Assert.True(managed.IsAtEnd);

			// Also verify an early-ending stream where Position (20) < Length (100) latches IsAtEnd,
			// and moving inner.Position externally clears the early-EOF latch in OnIsAtEnd().
			var earlyData = CreateSequentialBytes(100);
			using var earlyInner = new ConfigurableTestStream(earlyData, canSeek: true, earlyEndPosition: 20);
			using var earlyManaged = new SKManagedStream(earlyInner);

			var earlyBuf = new byte[50];
			Assert.Equal(20, earlyManaged.Read(earlyBuf, earlyBuf.Length));
			Assert.Equal(20, earlyInner.Position);
			Assert.True(earlyManaged.IsAtEnd);

			earlyInner.Position = 10;
			Assert.False(earlyManaged.IsAtEnd);
			Assert.Equal(10, earlyManaged.Read(earlyBuf, earlyBuf.Length));
			Assert.True(earlyManaged.IsAtEnd);
		}

		[Fact]
		public void UnknownLengthSeekableStream_ReportsHasLengthFalseAndReadsAllBytes()
		{
			var data = CreateSequentialBytes(64);
			using var inner = new ConfigurableTestStream(data, canSeek: true, maxBytesPerRead: 5, reportedLength: -1);
			using var managed = new SKManagedStream(inner);

			Assert.False(managed.HasLength);
			Assert.Equal(0, managed.Length);
			Assert.False(managed.IsAtEnd);

			var actual = new byte[data.Length];
			Assert.Equal(data.Length, managed.Read(actual, actual.Length));
			Assert.Equal(data, actual);
			Assert.Equal(0, managed.Read(new byte[1], 1));
			Assert.True(managed.IsAtEnd);
		}

		private static byte[] CreateSequentialBytes(int length)
		{
			var data = new byte[length];
			for (var i = 0; i < data.Length; i++)
				data[i] = (byte)((i * 31 + 17) & 0xFF);
			return data;
		}
	}
}

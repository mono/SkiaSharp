using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKDataTest : SKTest
	{
		private readonly static byte[] OddData = new byte[] { 1, 3, 5, 7, 9 };

		[Fact]
		public void EmptyDataIsNotDisposed()
		{
			var empty = SKData.Empty;
			Assert.True(SKObject.GetInstance<SKData>(empty.Handle, out _));

			empty.Dispose();
			Assert.True(SKObject.GetInstance<SKData>(empty.Handle, out _));
		}

		[Fact]
		public void EmptyAndZeroLengthSameObject()
		{
			var empty = SKData.Empty;
			var zero = SKData.Create(0);

			Assert.Same(empty, zero);
		}

		[Fact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void ValidDataProperties()
		{
			var data = SKData.CreateCopy(OddData);

			Assert.Equal(OddData.Length, data.Size);
			Assert.Equal(OddData, data.ToArray());
		}

		[Fact]
		public void CreateCopyRejectsLengthLargerThanArray()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => SKData.CreateCopy(OddData, (ulong)OddData.Length + 1));
		}

		[Theory]
		[InlineData(null, new byte[] { 0 })]
		[InlineData("", new byte[] { 0 })]
		[InlineData("A", new byte[] { 65, 0 })]
		[InlineData("ABC", new byte[] { 65, 66, 67, 0 })]
		[InlineData("https://example.com/path/to/a/document?query=annotation#destination", null)]
		[InlineData("上田", new byte[] { 63, 63, 0 })]
		[InlineData("a😀b", new byte[] { 97, 63, 63, 98, 0 })]
		[InlineData("embedded\0null", new byte[] { 101, 109, 98, 101, 100, 100, 101, 100, 0, 110, 117, 108, 108, 0 })]
		public void Issue5426_FromCStringIncludesNullTerminator(string str, byte[] expected)
		{
			expected ??= System.Text.Encoding.ASCII.GetBytes((str ?? string.Empty) + '\0');

			using var data = SKData.FromCString(str);

			Assert.Equal(expected.Length, data.Size);
			Assert.Equal(expected, data.ToArray());
		}

		[Fact]
		public void AsStreamReturnsCorrectStreamData()
		{
			var data = SKData.CreateCopy(OddData);

			var stream = data.AsStream();

			var buffer = new byte[5];
			stream.Read(buffer, 0, 5);

			Assert.Equal(OddData, buffer);
		}

		[Fact]
		public void CanWriteToAsStream()
		{
			var data = SKData.Create(5);

			var stream = data.AsStream();
			stream.Write(OddData, 0, 5);

			Assert.Equal(OddData, data.ToArray());
		}

		[Fact]
		public void CanCopyToAsStream()
		{
			var data = SKData.Create(5);

			var stream = data.AsStream();
			var ms = new MemoryStream(OddData);
			ms.CopyTo(stream);

			Assert.Equal(OddData, data.ToArray());
		}

		[Theory]
		[InlineData(null, 0, 0, 0)]
		[InlineData("", 0, 0, 0)]
		[InlineData("H", 1, 1, 2)]
		[InlineData("Hello World!", 12, 12, 13)]
		[InlineData("Hello World!!", 13, 13, 14)]
		[InlineData("上田雅美", 4, 12, 13)]
		public void StringsAreConvertedWithNullTerminator(string str, int length, int byteLength, int terminatedLength)
		{
			Assert.Equal(length, str?.Length ?? 0);

			var bytes = StringUtilities.GetEncodedText(str, SKTextEncoding.Utf8);
			Assert.Equal(byteLength, bytes.Length);

			bytes = StringUtilities.GetEncodedText(str, SKTextEncoding.Utf8, true);
			Assert.Equal(terminatedLength, bytes.Length);
		}

		[Fact]
		public void DataCanBeCreatedFromStream()
		{
			using var stream = new SKFileStream(Path.Combine(PathToImages, "baboon.jpg"));
			Assert.True(stream.IsValid);

			using var data = SKData.Create(stream);

			Assert.NotNull(data);
			Assert.True(data.Size > 0);
		}

		[Fact]
		public void DataCanBeCreatedFromManagedStream()
		{
			using var managed = File.OpenRead(Path.Combine(PathToImages, "baboon.jpg"));
			using var stream = new SKManagedStream(managed);
			using var data = SKData.Create(stream);

			Assert.NotNull(data);
			Assert.True(data.Size > 0);
		}

		[Fact]
		public void DataCanBeCreatedFromFile()
		{
			var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));

			Assert.NotNull(data);
			Assert.True(data.Size > 0);
		}

		[Fact]
		public void DataCanBeCreatedFromNonASCIIFile()
		{
			var data = SKData.Create(Path.Combine(PathToImages, "上田雅美.jpg"));

			Assert.NotNull(data);
			Assert.True(data.Size > 0);
		}

		[Fact]
		public void NoDelegateDataCanBeCreated()
		{
			var memory = Marshal.AllocCoTaskMem(10);

			using (var data = SKData.Create(memory, 10))
			{
				Assert.Equal(memory, data.Data);
				Assert.Equal(10, data.Size);
			}

			Marshal.FreeCoTaskMem(memory);
		}

		[Fact]
		public void ReleaseDataWasInvoked()
		{
			bool released = false;

			var onRelease = new SKDataReleaseDelegate((addr, ctx) =>
			{
				Marshal.FreeCoTaskMem(addr);
				released = true;
				Assert.Equal("RELEASING!", ctx);
			});

			var memory = Marshal.AllocCoTaskMem(10);

			using (var data = SKData.Create(memory, 10, onRelease, "RELEASING!"))
			{
				Assert.Equal(memory, data.Data);
				Assert.Equal(10, data.Size);
			}

			Assert.True(released, "The SKDataReleaseDelegate was not called.");
		}

		[Fact]
		public void CanCreateFromNonSeekable()
		{
			using var stream = File.OpenRead(Path.Combine(PathToImages, "baboon.png"));
			using var nonSeekable = new NonSeekableReadOnlyStream(stream);
			using var data = SKData.Create(nonSeekable);

			Assert.NotNull(data);
		}

		[Fact]
		public void CanCreateFromPartiallyReadStream()
		{
			using var stream = File.OpenRead(Path.Combine(PathToImages, "baboon.png"));

			stream.Position = 10;

			using var data = SKData.Create(stream);

			Assert.NotNull(data);
			Assert.Equal(stream.Length - 10, data.Size);
		}

		[Fact]
		public void CanCreateFromPartiallyReadNonSeekable()
		{
			using var stream = File.OpenRead(Path.Combine(PathToImages, "baboon.png"));
			stream.Position = 10;

			using var nonSeekable = new NonSeekableReadOnlyStream(stream);
			using var data = SKData.Create(nonSeekable);

			Assert.NotNull(data);
			Assert.Equal(stream.Length - 10, data.Size);
		}

		[Fact(Skip = "Doesn't work as it relies on memory being overwritten by an external process.")]
		public void DataDisposedReturnsInvalidStream()
		{
			// create data
			var data = SKData.CreateCopy(OddData);

			// get the stream
			var stream = data.AsStream();

			// nuke the data
			data.Dispose();
			Assert.Equal(IntPtr.Zero, data.Handle);

			// read the stream
			var buffer = new byte[OddData.Length];
			stream.Read(buffer, 0, buffer.Length);

			// since the data was nuked, they will differ
			Assert.NotEqual(OddData, buffer);
		}

		// Reference implementation: the previous (buffered) SaveTo path — copy native memory into a
		// pooled managed buffer with Marshal.Copy, then write that buffer to the stream. Used as the
		// oracle to prove the current (span-based) SaveTo is byte-for-byte identical.
		private static unsafe void SaveToBuffered(SKData data, Stream target)
		{
			const int CopyBufferSize = 81920;
			var ptr = data.Data;
			var total = data.Size;
			var buffer = new byte[CopyBufferSize];
			for (var left = total; left > 0;)
			{
				var copyCount = (int)Math.Min(CopyBufferSize, left);
				Marshal.Copy(ptr, buffer, 0, copyCount);
				left -= copyCount;
				ptr += copyCount;
				target.Write(buffer, 0, copyCount);
			}
			GC.KeepAlive(data);
		}

		[Theory]
		// Sizes chosen to cross the 81920-byte internal chunk boundary: empty, sub-chunk, exactly one
		// chunk, one chunk + 1, and several chunks + a partial tail.
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(81919)]
		[InlineData(81920)]
		[InlineData(81921)]
		[InlineData(200000)]
		public void SaveToMatchesBufferedReference(int size)
		{
			var bytes = new byte[size];
			for (var i = 0; i < size; i++)
				bytes[i] = (byte)(i * 31 + 7);

			using var data = SKData.CreateCopy(bytes);

			using var actual = new MemoryStream();
			data.SaveTo(actual);

			using var expected = new MemoryStream();
			SaveToBuffered(data, expected);

			// The written stream must equal both the buffered-reference output and the source bytes.
			Assert.Equal(expected.ToArray(), actual.ToArray());
			Assert.Equal(bytes, actual.ToArray());
			Assert.Equal(size, actual.Length);
		}

		[Theory]
		[InlineData(true, 1)]
		[InlineData(true, 7)]
		[InlineData(false, 1)]
		[InlineData(false, 7)]
		public void CreateFromStream_HandlesShortReads(bool canSeek, int maxBytesPerRead)
		{
			var bytes = new byte[256];
			for (var i = 0; i < bytes.Length; i++)
				bytes[i] = (byte)(i + 1);

			using var stream = new ConfigurableTestStream(bytes, canSeek: canSeek, maxBytesPerRead: maxBytesPerRead);
			using var data = SKData.Create(stream);

			Assert.NotNull(data);
			Assert.Equal(bytes.Length, data.Size);
			Assert.Equal(bytes, data.ToArray());
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(65535)]
		[InlineData(65536)]
		[InlineData(65537)]
		[InlineData(196608)]
		[InlineData(2097152)]
		[InlineData(3145728)]
		[InlineData(10485760)]
		public void CreateFromUnknownLengthStreamMatchesBufferedReference(int size)
		{
			var bytes = new byte[size];
			for (var i = 0; i < bytes.Length; i++)
				bytes[i] = (byte)(i * 31 + 7);

			foreach (var maxRead in new[] { int.MaxValue, 997 })
			{
				using var actualStream = new ConfigurableTestStream(bytes, canSeek: false, maxBytesPerRead: maxRead);
				using var expectedStream = new ConfigurableTestStream(bytes, canSeek: false, maxBytesPerRead: maxRead);
				using var actual = SKData.Create(actualStream);
				using var expected = CreateFromUnknownLengthBuffered(expectedStream);

				Assert.NotNull(actual);
				Assert.Equal(expected.Size, actual.Size);
				Assert.Equal(expected.ToArray(), actual.ToArray());
				Assert.Equal(bytes, actual.ToArray());
				Assert.True(actualStream.CanRead);
				Assert.Equal(1, actualStream.ZeroReadCount);
			}
		}

		[Fact]
		public void CreateFromUnknownLengthStreamPreservesOffsetAndEarlyEnd()
		{
			var bytes = new byte[3 * 1024 * 1024];
			new Random(42).NextBytes(bytes);
			using var inner = new ConfigurableTestStream(bytes, maxBytesPerRead: 997, earlyEndPosition: 2 * 1024 * 1024 + 17);
			inner.Position = 7;
			using var stream = new NonSeekableReadOnlyStream(inner);
			using var data = SKData.Create(stream);

			Assert.Equal(2 * 1024 * 1024 + 10, data.Size);
			Assert.True(data.AsSpan().SequenceEqual(bytes.AsSpan(7, (int)data.Size)));
			Assert.Equal(2 * 1024 * 1024 + 17, inner.Position);
		}

		[Fact]
		public void CreateFromUnknownLengthStreamDoesNotRetainPooledBuffers()
		{
			var bytes = new byte[2 * 1024 * 1024];
			bytes.AsSpan().Fill(123);
			using var firstStream = new ConfigurableTestStream(bytes, canSeek: false);
			using var first = SKData.Create(firstStream);

			bytes.AsSpan().Fill(234);
			using var secondStream = new ConfigurableTestStream(bytes, canSeek: false);
			using var second = SKData.Create(secondStream);

			Assert.Equal(bytes, second.ToArray());
			bytes.AsSpan().Fill(123);
			Assert.Equal(bytes, first.ToArray());
		}

		[Fact]
		public void CreateFromUnknownLengthSeekableStreamReadsToEnd()
		{
			var bytes = new byte[2 * 1024 * 1024 + 17];
			new Random(42).NextBytes(bytes);
			using var stream = new ConfigurableTestStream(bytes, maxBytesPerRead: 997, reportedLength: -1);
			using var data = SKData.Create(stream);

			Assert.Equal(bytes, data.ToArray());
			Assert.Equal(bytes.Length, stream.Position);
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(int.MaxValue)]
		public void CreateFromUnknownLengthStreamRejectsInvalidCountsAfterFullChunks(int invalidReturn)
		{
			using var stream = new FailingAfterReadStream(196608, invalidReturn);
			Assert.Throws<IOException>(() => SKData.Create(stream));
		}

		[Fact]
		public void CreateFromUnknownLengthStreamPropagatesExceptionAfterFullChunks()
		{
			using var stream = new FailingAfterReadStream(196608);
			Assert.Throws<IOException>(() => SKData.Create(stream));
		}

		private static SKData CreateFromUnknownLengthBuffered(Stream stream)
		{
			using var memory = new SKDynamicMemoryWStream();
			using var buffer = Utils.RentArray<byte>(SKData.CopyBufferSize);
			while (true)
			{
				var readLen = stream.Read(buffer.Span);
				if ((uint)readLen > (uint)buffer.Length)
					throw new IOException("The stream returned an invalid byte count.");
				if (readLen == 0)
					break;
				if (!memory.Write(buffer.Array, readLen))
					return null;
			}
			return memory.DetachAsData();
		}

		private sealed class FailingAfterReadStream : MemoryStream
		{
			private readonly int? invalidReturn;

			public FailingAfterReadStream(int length, int? invalidReturn = null)
				: base(new byte[length], writable: false)
			{
				this.invalidReturn = invalidReturn;
			}

			public override bool CanSeek => false;

			public override int Read(byte[] buffer, int offset, int count) =>
				Position == Length ? Fail(count) : base.Read(buffer, offset, count);

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
			public override int Read(Span<byte> buffer) =>
				Position == Length ? Fail(buffer.Length) : base.Read(buffer);
#endif

			private int Fail(int count) =>
				invalidReturn.HasValue
					? invalidReturn == int.MaxValue ? count + 1 : invalidReturn.Value
					: throw new IOException("Simulated failure after reading full chunks.");
		}

		[Fact]
		public void CreateFromSeekableStream_HandlesEarlyEndAndPositionPastLength()
		{
			var bytes = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };
			using (var earlyStream = new ConfigurableTestStream(bytes, canSeek: true, maxBytesPerRead: 3, earlyEndPosition: 5))
			{
				using var data = SKData.Create(earlyStream);
				Assert.NotNull(data);
				Assert.Equal(new byte[] { 10, 20, 30, 40, 50 }, data.ToArray());
			}

			using (var pastEndStream = new MemoryStream(bytes))
			{
				pastEndStream.Position = 20;
				using var data = SKData.Create(pastEndStream);
				Assert.NotNull(data);
				Assert.Equal(0, data.Size);
			}

			using (var offsetEarlyStream = new ConfigurableTestStream(bytes, canSeek: true, maxBytesPerRead: 2, earlyEndPosition: 6))
			{
				offsetEarlyStream.Position = 2;
				using var data = SKData.Create(offsetEarlyStream);
				Assert.NotNull(data);
				Assert.Equal(new byte[] { 30, 40, 50, 60 }, data.ToArray());
			}
		}

		[Fact]
		public void CreateRejectsNegativeSizesAndLengths()
		{
			using var ms = new MemoryStream(OddData);
			using var skStream = new SKMemoryStream(OddData);

			Assert.Throws<ArgumentOutOfRangeException>(() => SKData.Create(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => SKData.Create(-1L));
			Assert.Throws<ArgumentOutOfRangeException>(() => SKData.Create(ms, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => SKData.Create(ms, -1L));
			Assert.Throws<ArgumentOutOfRangeException>(() => SKData.Create(skStream, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => SKData.Create(skStream, -1L));
		}

		[Fact]
		public void CreateFromSKStream_HandlesPartiallyConsumedAndNonSeekableStreams()
		{
			using (var ms = new MemoryStream(OddData))
			using (var skStream = new SKManagedStream(ms))
			{
				Assert.Equal(1, skStream.ReadByte());
				Assert.Equal(3, skStream.ReadByte());

				using var data = SKData.Create((SKStream)skStream);
				Assert.NotNull(data);
				Assert.Equal(new byte[] { 5, 7, 9 }, data.ToArray());
			}

			using (var inner = new ConfigurableTestStream(OddData, canSeek: false))
			using (var skStream = new SKManagedStream(inner))
			{
				using var data = SKData.Create((SKStream)skStream);
				Assert.NotNull(data);
				Assert.Equal(OddData, data.ToArray());
			}
		}

		[Fact]
		public void CreateFromSKStream_EarlyEndingAndFrontBufferedSKStreams_DoNotLoseConsumedBytes()
		{
			var bytes = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };

			using (var inner = new ConfigurableTestStream(bytes, canSeek: true, earlyEndPosition: 5))
			using (var skStream = new SKManagedStream(inner))
			{
				using var data = SKData.Create((SKStream)skStream);
				Assert.NotNull(data);
				Assert.Equal(new byte[] { 10, 20, 30, 40, 50 }, data.ToArray());
			}

			using (var ms = new MemoryStream(bytes))
			using (var fb = new SKFrontBufferedManagedStream(ms, 4, disposeUnderlyingStream: false))
			{
				Assert.Equal(10, fb.ReadByte());
				Assert.Equal(20, fb.ReadByte());

				using var data = SKData.Create((SKStream)fb);
				Assert.NotNull(data);
				Assert.Equal(new byte[] { 30, 40, 50, 60, 70, 80 }, data.ToArray());
			}
		}

		[Fact]
		public void CreateFromStream_WhenReadThrows_DisposesAllocatedDataImmediately()
		{
			using var throwingStream = new ThrowingReadStream();
			Assert.Throws<IOException>(() => SKData.Create(throwingStream, 1024));
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(int.MaxValue)]
		public void CreateFromStream_WhenStreamReturnsInvalidByteCount_ThrowsIOException(int invalidReturn)
		{
			using (var hostile = new HostileReadReturnStream(invalidReturn))
			{
				Assert.Throws<IOException>(() => SKData.Create(hostile));
			}

			using (var hostile = new HostileReadReturnStream(invalidReturn))
			{
				Assert.Throws<IOException>(() => SKData.Create(hostile, 32));
			}

			using (var hostile = new HostileReadReturnStream(invalidReturn))
			using (var nonSeekable = new NonSeekableReadOnlyStream(hostile))
			{
				Assert.Throws<IOException>(() => SKData.Create(nonSeekable));
			}
		}
	}
}

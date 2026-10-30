using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKStreamTest : SKTest
	{
		[Fact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void CanWriteTextToStream()
		{
			using (var stream = new SKDynamicMemoryWStream())
			{
				Assert.NotNull(stream);
				Assert.Equal(0, stream.BytesWritten);

				stream.WriteText("Hello");

				Assert.Equal(5, stream.BytesWritten);
			}
		}

		[Fact]
		public void SupportsNonASCIICharactersInPath()
		{
			var path = Path.Combine(PathToImages, "上田雅美.jpg");

			using (var stream = new SKFileStream(path))
			{
				Assert.NotNull(stream);
				Assert.True(stream.Length > 0);
				Assert.True(stream.IsValid);
			}
		}

		[Fact]
		public void WriteableFileStreamSelectCorrectStreamForASCIIPath()
		{
			var path = Path.Combine(PathToImages, Guid.NewGuid().ToString("D") + ".jpg");

			using (var stream = SKFileWStream.OpenStream(path))
			{
				Assert.IsType<SKFileWStream>(stream);
			}
		}

		[Fact]
		public void WriteableFileStreamSelectCorrectStreamForNonASCIIPath()
		{
			var path = Path.Combine(PathToImages, Guid.NewGuid().ToString("D") + "-上田雅美.jpg");

			using (var stream = SKFileWStream.OpenStream(path))
			{
				Assert.IsType<SKFileWStream>(stream);
				Assert.True(((SKFileWStream)stream).IsValid);
			}
		}

		[Fact]
		public void FileStreamSelectCorrectStreamForASCIIPath()
		{
			var path = Path.Combine(PathToImages, "baboon.jpg");

			using (var stream = SKFileStream.OpenStream(path))
			{
				Assert.IsType<SKFileStream>(stream);
			}
		}

		[Fact]
		public void FileStreamSelectCorrectStreamForNonASCIIPath()
		{
			var path = Path.Combine(PathToImages, "上田雅美.jpg");

			using (var stream = SKFileStream.OpenStream(path))
			{
				Assert.IsType<SKFileStream>(stream);
				Assert.True(((SKFileStream)stream).IsValid);
			}
		}

		[Fact]
		public void FileStreamForMissingFile()
		{
			var path = Path.Combine(PathToImages, "missing-image.png");

			Assert.Null(SKFileStream.OpenStream(path));

			var stream = new SKFileStream(path);

			Assert.Equal(0, stream.Length);
			Assert.False(stream.IsValid);
		}

		[Fact]
		public void MemoryStreamSupports64BitLengthPositionAndSeek()
		{
			using var stream = new SKMemoryStream(new byte[] { 10, 20, 30, 40, 50 });

			Assert.Equal(5L, stream.Length64);
			Assert.Equal(5, stream.Length);
			Assert.Equal(0L, stream.Position64);
			Assert.True(stream.Seek(2L));
			Assert.Equal(2L, stream.Position64);
			Assert.Equal(2, stream.Position);
			Assert.Equal(30, stream.ReadByte());
			Assert.True(stream.Move(-1));
			Assert.Equal(2L, stream.Position64);

			stream.Position64 = 4;
			Assert.Equal(50, stream.ReadByte());
			Assert.True(stream.Seek(100L));
			Assert.Equal(5L, stream.Position64);
			stream.Position = 1;
			Assert.Equal(1L, stream.Position64);
			Assert.True(stream.Seek(0));
			Assert.Equal(0L, stream.Position64);
		}

		[Theory]
		[InlineData(0L)]
		[InlineData((long)int.MaxValue)]
		[InlineData((long)int.MaxValue + 1)]
		[InlineData((long)uint.MaxValue)]
		public void Stream64MembersPreserveNativeUnsignedValues(long value)
		{
			using var stream = new SizeReportingStream((ulong)value);

			Assert.Equal(value, stream.Length64);
			Assert.True(stream.Seek(value));
			Assert.Equal(value, stream.Position64);
			stream.Position64 = value;
			Assert.Equal(value, stream.Position64);
			Assert.Equal(2, stream.SeekCalls);

			if (value <= int.MaxValue)
			{
				Assert.Equal((int)value, stream.Length);
				Assert.Equal((int)value, stream.Position);
			}
			else
			{
				Assert.Throws<OverflowException>(() => stream.Length);
				Assert.Throws<OverflowException>(() => stream.Position);
			}
		}

		[Fact]
		public void Stream64MembersHandlePlatformLimitsWithoutAllocatingPayload()
		{
			using var stream = new SizeReportingStream(IntPtr.Size == 8 ? (ulong)long.MaxValue : uint.MaxValue);

			if (IntPtr.Size == 8)
			{
				Assert.Equal(long.MaxValue, stream.Length64);
				Assert.True(stream.Seek(long.MaxValue));
				Assert.Equal(long.MaxValue, stream.Position64);

				stream.ReportedLength = (ulong)long.MaxValue + 1;
				Assert.Throws<OverflowException>(() => stream.Length64);
				Assert.Throws<OverflowException>(() => stream.Length);
				stream.ReportedPosition = ulong.MaxValue;
				Assert.Throws<OverflowException>(() => stream.Position64);
				Assert.Throws<OverflowException>(() => stream.Position);
			}
			else
			{
				Assert.Equal((long)uint.MaxValue, stream.Length64);
				Assert.Throws<ArgumentOutOfRangeException>("position", () => stream.Seek((long)uint.MaxValue + 1));
				Assert.Throws<ArgumentOutOfRangeException>("position", () => stream.Position64 = long.MaxValue);
				Assert.Equal(0, stream.SeekCalls);
			}
		}

		[Theory]
		[InlineData(-1L)]
		[InlineData(long.MinValue)]
		public void Stream64SeekRejectsNegativePositionsBeforeNativeEntry(long position)
		{
			using var stream = new SizeReportingStream(10);

			Assert.Throws<ArgumentOutOfRangeException>("position", () => stream.Seek(position));
			Assert.Throws<ArgumentOutOfRangeException>("position", () => stream.Position64 = position);
			Assert.Equal(0, stream.SeekCalls);
			Assert.Equal(0L, stream.Position64);
		}

		[Fact]
		public void Stream64MembersPreserveUnsupportedCapabilityResults()
		{
			using var inner = new MemoryStream(new byte[] { 1, 2, 3 });
			using var nonSeekable = new NonSeekableReadOnlyStream(inner);
			using var stream = new SKManagedStream(nonSeekable);

			Assert.False(stream.HasLength);
			Assert.False(stream.HasPosition);
			Assert.Equal(0L, stream.Length64);
			Assert.Equal(0L, stream.Position64);
			Assert.False(stream.Seek(0L));
			stream.Position64 = 1;
			Assert.Equal(0L, stream.Position64);
		}

		[Theory]
		[InlineData((long)int.MaxValue + 1)]
		[InlineData((long)uint.MaxValue)]
		public void ManagedStream64MembersPreserveUnsignedNativePositions(long position)
		{
			using var inner = new ConfigurableTestStream(Array.Empty<byte>(), reportedLength: uint.MaxValue);
			inner.Position = position;
			using var stream = new SKManagedStream(inner);

			// Check callbacks directly before exercising the public native round trip.
			Assert.Equal((ulong)uint.MaxValue, Utils.FromNativeSize(stream.OnGetLength()));
			Assert.Equal((ulong)position, Utils.FromNativeSize(stream.OnGetPosition()));
			Assert.True(stream.OnSeek(Utils.ToNativeSize((ulong)position)));
			Assert.Equal(position, inner.Position);
			Assert.False(inner.NegativePositionAttempted);
			Assert.Equal((ulong)uint.MaxValue, Utils.FromNativeSize(SkiaApi.sk_stream_get_length(stream.Handle)));
			Assert.Equal((ulong)position, Utils.FromNativeSize(SkiaApi.sk_stream_get_position(stream.Handle)));

			Assert.Equal((long)uint.MaxValue, stream.Length64);
			Assert.Equal(position, stream.Position64);
			Assert.True(stream.Seek(position));
			Assert.Equal(position, inner.Position);
			stream.Position64 = position - 1;
			Assert.Equal(position - 1, inner.Position);
			Assert.Equal(position - 1, stream.Position64);
			Assert.False(inner.NegativePositionAttempted);
			Assert.Equal(0, inner.TotalReadCalls);
		}

		[Fact]
		public void ManagedStreamRejectsUnrepresentableNativeSeekWithoutChangingPosition()
		{
			using var inner = new ConfigurableTestStream(Array.Empty<byte>(), reportedLength: long.MaxValue);
			inner.Position = 5;
			using var stream = new SKManagedStream(inner);

			if (IntPtr.Size == 8)
			{
				Assert.False(stream.OnSeek(new IntPtr(long.MinValue)));
				Assert.False(stream.OnSeek(new IntPtr(-1)));
				Assert.Equal(5, inner.Position);
				Assert.False(inner.NegativePositionAttempted);
			}
			else
			{
				Assert.Throws<OverflowException>(() => stream.OnGetLength());
				Assert.Throws<OverflowException>(() => stream.Length64);
				inner.Position = (long)uint.MaxValue + 1;
				Assert.Throws<OverflowException>(() => stream.OnGetPosition());
				Assert.Throws<OverflowException>(() => stream.Position64);
			}
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(int.MinValue)]
		public void NegativeStreamArgumentsAreRejectedBeforeNativeCallbacks(int value)
		{
			using var stream = new SizeReportingStream(10);

			Assert.Throws<ArgumentOutOfRangeException>("size", () => stream.Read(new byte[1], value));
			Assert.Throws<ArgumentOutOfRangeException>("size", () => stream.Read(IntPtr.Zero, value));
			Assert.Throws<ArgumentOutOfRangeException>("size", () => stream.Peek(IntPtr.Zero, value));
			Assert.Throws<ArgumentOutOfRangeException>("size", () => stream.Skip(value));
			Assert.Throws<ArgumentOutOfRangeException>("position", () => stream.Seek(value));
			Assert.Throws<ArgumentOutOfRangeException>("position", () => stream.Position = value);
			Assert.Equal(0, stream.ReadCalls);
			Assert.Equal(0, stream.PeekCalls);
			Assert.Equal(0, stream.SeekCalls);
		}

		[Fact]
		public void ArrayReadRejectsOversizedCountBeforeNativeCallback()
		{
			using var stream = new SizeReportingStream(10);

			Assert.Throws<ArgumentOutOfRangeException>("size", () => stream.Read(new byte[1], 2));
			Assert.Equal(0, stream.ReadCalls);
		}

		[Fact]
		public void NullArrayReadStillSkipsBytes()
		{
			using var stream = new SKMemoryStream(new byte[] { 10, 20, 30 });

			Assert.Equal(2, stream.Read((byte[])null, 2));
			Assert.Equal(2L, stream.Position64);
			Assert.Equal(30, stream.ReadByte());
		}

		private sealed class SizeReportingStream : SKAbstractManagedStream
		{
			public SizeReportingStream(ulong length) => ReportedLength = length;

			public ulong ReportedLength { get; set; }
			public ulong ReportedPosition { get; set; }
			public int SeekCalls { get; private set; }
			public int ReadCalls { get; private set; }
			public int PeekCalls { get; private set; }

			protected internal override IntPtr OnGetLength() => ToNativeSize(ReportedLength);
			protected internal override IntPtr OnGetPosition() => ToNativeSize(ReportedPosition);
			protected internal override bool OnHasLength() => true;
			protected internal override bool OnHasPosition() => true;
			protected internal override bool OnIsAtEnd() => ReportedPosition >= ReportedLength;
			protected internal override IntPtr OnCreateNew() => IntPtr.Zero;
			protected internal override bool OnMove(int offset) => false;

			protected internal override bool OnRewind() => OnSeek(IntPtr.Zero);

			protected internal override IntPtr OnRead(IntPtr buffer, IntPtr size)
			{
				ReadCalls++;
				return IntPtr.Zero;
			}

			protected internal override IntPtr OnPeek(IntPtr buffer, IntPtr size)
			{
				PeekCalls++;
				return IntPtr.Zero;
			}

			protected internal override bool OnSeek(IntPtr position)
			{
				SeekCalls++;
				ReportedPosition = IntPtr.Size == 8 ? (ulong)position.ToInt64() : (uint)position.ToInt32();
				return true;
			}

			private static IntPtr ToNativeSize(ulong value) =>
				IntPtr.Size == 8 ? new IntPtr((long)value) : new IntPtr((int)(uint)value);
		}

		[Fact]
		public void GarbageCollectionCollectsStreams()
		{
			SkipOnMono();

			var path = Path.Combine(PathToImages, "baboon.jpg");

			var weak = DoWork();

			CollectGarbage();

			Assert.False(weak.IsAlive);
			Assert.Null(weak.Target);

			WeakReference DoWork()
			{
				var stream = new SKFileStream(path);
				return new WeakReference(stream);
			}
		}

		[Fact]
		public void MemoryStreamCanBeDuplicated()
		{
			var stream = new SKMemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			Assert.Equal(1, stream.ReadByte());
			Assert.Equal(2, stream.ReadByte());
			Assert.Equal(3, stream.ReadByte());

			var dupe = stream.Duplicate();
			Assert.NotSame(stream, dupe);
			Assert.IsType<SKStreamImplementation>(dupe);
			Assert.Equal(1, dupe.ReadByte());
			Assert.Equal(4, stream.ReadByte());
			Assert.Equal(2, dupe.ReadByte());
			Assert.Equal(5, stream.ReadByte());
			Assert.Equal(3, dupe.ReadByte());
		}

		[Fact]
		public void MemoryStreamCanBeForked()
		{
			var stream = new SKMemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			Assert.Equal(1, stream.ReadByte());
			Assert.Equal(2, stream.ReadByte());

			var dupe = stream.Fork();
			Assert.NotSame(stream, dupe);
			Assert.IsType<SKStreamImplementation>(dupe);
			Assert.Equal(3, dupe.ReadByte());
			Assert.Equal(3, stream.ReadByte());
			Assert.Equal(4, dupe.ReadByte());
			Assert.Equal(4, stream.ReadByte());
		}

		[Fact]
		public void MemoryStreamGetDataReturnsBackingData()
		{
			var bytes = new byte[] { 10, 20, 30, 40, 50 };
			using var data = SKData.CreateCopy(bytes);
			using var stream = new SKMemoryStream(data);

			using var result = stream.GetData();

			Assert.NotNull(result);
			Assert.Equal(bytes.Length, (int)result.Size);
			Assert.Equal(bytes, result.ToArray());
		}

		[Fact]
		public void MemoryStreamGetDataFromByteArrayReturnsData()
		{
			var bytes = new byte[] { 1, 2, 3 };
			using var stream = new SKMemoryStream(bytes);

			using var result = stream.GetData();

			Assert.NotNull(result);
			Assert.Equal(bytes.Length, (int)result.Size);
			Assert.Equal(bytes, result.ToArray());
		}

		[Fact]
		public void MemoryStreamGetDataIsZeroCopy()
		{
			var bytes = new byte[] { 100, 200 };
			using var data = SKData.CreateCopy(bytes);
			using var stream = new SKMemoryStream(data);

			using var result1 = stream.GetData();
			using var result2 = stream.GetData();

			Assert.NotNull(result1);
			Assert.NotNull(result2);
			Assert.Equal(result1.Data, result2.Data);
		}

		[Fact]
		public void FileStreamGetDataReturnsNull()
		{
			var path = Path.Combine(PathToImages, "baboon.jpg");
			using var stream = new SKFileStream(path);

			var result = stream.GetData();

			Assert.Null(result);
		}

		[Fact]
		public void EmptyMemoryStreamGetDataReturnsEmptyData()
		{
			using var stream = new SKMemoryStream();

			using var result = stream.GetData();

			Assert.NotNull(result);
			Assert.Equal(0, (int)result.Size);
		}

		[Fact]
		public void GetDataInteropRoundTrip()
		{
			var bytes = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
			using var data = SKData.CreateCopy(bytes);
			using var stream = new SKMemoryStream(data);

			using var result = stream.GetData();

			Assert.NotNull(result);
			Assert.Equal(bytes, result.ToArray());
		}

		[Fact]
		public void ManagedStreamGetDataReturnsNull()
		{
			using var memoryStream = new MemoryStream(new byte[] { 1, 2, 3 });
			using var stream = new SKManagedStream(memoryStream);

			var result = stream.GetData();

			Assert.Null(result);
		}

		[Fact]
		public void ManagedStreamGetDataReturnsNullForNonSeekableStream()
		{
			using var inner = new MemoryStream(new byte[] { 10, 20, 30 });
			using var nonSeekable = new NonSeekableReadOnlyStream(inner);
			using var stream = new SKManagedStream(nonSeekable);

			var result = stream.GetData();

			Assert.Null(result);
		}

		[Fact]
		public void ManagedStreamGetDataReturnsNullEvenAfterRead()
		{
			using var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			using var stream = new SKManagedStream(memoryStream);

			// read some data first, then ask for getData
			stream.ReadByte();
			stream.ReadByte();

			var result = stream.GetData();

			Assert.Null(result);
		}

		[Fact]
		public void GetDataReturnsSameManagedObject()
		{
			var bytes = new byte[] { 10, 20, 30, 40 };
			using var data = SKData.CreateCopy(bytes);
			using var stream = new SKMemoryStream(data);

			var result = stream.GetData();

			// GetOrAddObject finds the existing C# wrapper and returns it
			Assert.Same(data, result);
		}

		[Fact]
		public void GetDataSurvivesStreamDisposal()
		{
			var bytes = new byte[] { 10, 20, 30, 40 };
			using var data = SKData.CreateCopy(bytes);
			var stream = new SKMemoryStream(data);

			var result = stream.GetData();
			Assert.NotNull(result);

			// dispose stream — data survives because SkData is ref-counted
			stream.Dispose();

			Assert.Equal(bytes.Length, (int)data.Size);
			Assert.Equal(bytes, data.ToArray());
		}

		[Fact]
		public void GetDataSurvivesNativeCreatedStreamDisposal()
		{
			var bytes = new byte[] { 0xCA, 0xFE, 0xBA, 0xBE };

			// byte[] constructor: native side creates the SkData internally
			var stream = new SKMemoryStream(bytes);

			var result = stream.GetData();
			Assert.NotNull(result);

			// dispose the stream — result holds its own native ref
			stream.Dispose();

			// result survives because ref-counted SkData keeps it alive
			Assert.Equal(bytes.Length, (int)result.Size);
			Assert.Equal(bytes, result.ToArray());

			result.Dispose();
		}
	}
}

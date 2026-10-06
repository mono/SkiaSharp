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
	}
}

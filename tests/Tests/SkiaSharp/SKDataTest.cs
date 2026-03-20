using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKDataTest : SKTest
	{
		private readonly static byte[] OddData = new byte[] { 1, 3, 5, 7, 9 };

		[SkippableFact]
		public void EmptyDataIsNotDisposed()
		{
			var empty = SKData.Empty;
			Assert.True(SKObject.GetInstance<SKData>(empty.Handle, out _));

			empty.Dispose();
			Assert.True(SKObject.GetInstance<SKData>(empty.Handle, out _));
		}

		[SkippableFact]
		public void EmptyAndZeroLengthSameObject()
		{
			var empty = SKData.Empty;
			var zero = SKData.Create(0);

			Assert.Same(empty, zero);
		}

		[SkippableFact]
		public void ValidDataProperties()
		{
			var data = SKData.CreateCopy(OddData);

			Assert.Equal(OddData.Length, data.Size);
			Assert.Equal(OddData, data.ToArray());
		}

		[SkippableFact]
		public void ValidDataPropertiesWithSpan()
		{
			var data = SKData.CreateCopy(new ReadOnlySpan<byte>(OddData));

			Assert.Equal(OddData.Length, data.Size);
			Assert.Equal(OddData, data.ToArray());
		}

		[SkippableFact]
		public void AsStreamReturnsCorrectStreamData()
		{
			var data = SKData.CreateCopy(OddData);

			var stream = data.AsStream();

			var buffer = new byte[5];
			stream.Read(buffer, 0, 5);

			Assert.Equal(OddData, buffer);
		}

		[SkippableFact]
		public void CanWriteToAsStream()
		{
			var data = SKData.Create(5);

			var stream = data.AsStream();
			stream.Write(OddData, 0, 5);

			Assert.Equal(OddData, data.ToArray());
		}

		[SkippableFact]
		public void CanCopyToAsStream()
		{
			var data = SKData.Create(5);

			var stream = data.AsStream();
			var ms = new MemoryStream(OddData);
			ms.CopyTo(stream);

			Assert.Equal(OddData, data.ToArray());
		}

		[SkippableTheory]
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

		[SkippableFact]
		public void DataCanBeCreatedFromStream()
		{
			using var stream = new SKFileStream(Path.Combine(PathToImages, "baboon.jpg"));
			Assert.True(stream.IsValid);

			using var data = SKData.Create(stream);

			Assert.NotNull(data);
			Assert.True(data.Size > 0);
		}

		[SkippableFact]
		public void DataCanBeCreatedFromManagedStream()
		{
			using var managed = File.OpenRead(Path.Combine(PathToImages, "baboon.jpg"));
			using var stream = new SKManagedStream(managed);
			using var data = SKData.Create(stream);

			Assert.NotNull(data);
			Assert.True(data.Size > 0);
		}

		[SkippableFact]
		public void DataCanBeCreatedFromFile()
		{
			var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));

			Assert.NotNull(data);
			Assert.True(data.Size > 0);
		}

		[SkippableFact]
		public void DataCanBeCreatedFromNonASCIIFile()
		{
			var data = SKData.Create(Path.Combine(PathToImages, "上田雅美.jpg"));

			Assert.NotNull(data);
			Assert.True(data.Size > 0);
		}

		[SkippableFact]
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

		[SkippableFact]
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

		[SkippableFact]
		public void CanCreateFromNonSeekable()
		{
			using var stream = File.OpenRead(Path.Combine(PathToImages, "baboon.png"));
			using var nonSeekable = new NonSeekableReadOnlyStream(stream);
			using var data = SKData.Create(nonSeekable);

			Assert.NotNull(data);
		}

		[SkippableFact]
		public void CanCreateFromPartiallyReadStream()
		{
			using var stream = File.OpenRead(Path.Combine(PathToImages, "baboon.png"));

			stream.Position = 10;

			using var data = SKData.Create(stream);

			Assert.NotNull(data);
			Assert.Equal(stream.Length - 10, data.Size);
		}

		[SkippableFact]
		public void CanCreateFromPartiallyReadNonSeekable()
		{
			using var stream = File.OpenRead(Path.Combine(PathToImages, "baboon.png"));
			stream.Position = 10;

			using var nonSeekable = new NonSeekableReadOnlyStream(stream);
			using var data = SKData.Create(nonSeekable);

			Assert.NotNull(data);
			Assert.Equal(stream.Length - 10, data.Size);
		}

		[SkippableFact(Skip = "Doesn't work as it relies on memory being overwritten by an external process.")]
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
	}
}

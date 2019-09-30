using System;
using System.Runtime.InteropServices;
using Xunit;
using System.IO;

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
		public void ValidDataProperties()
		{
			var data = SKData.CreateCopy(OddData);

			Assert.Equal(OddData.Length, data.Size);
			Assert.Equal(OddData, data.ToArray());
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

			var onRelease = new SKDataReleaseDelegate((addr, ctx) => {
				Marshal.FreeCoTaskMem(addr);
				released = true;
				Assert.Equal("RELEASING!", ctx);
			});

			var memory = Marshal.AllocCoTaskMem(10);

			using (var data = SKData.Create(memory, 10, onRelease, "RELEASING!")) {
				Assert.Equal(memory, data.Data);
				Assert.Equal(10, data.Size);
			}

			Assert.True(released, "The SKDataReleaseDelegate was not called.");
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

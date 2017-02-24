using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKDataTest : SKTest
	{
		private readonly static byte[] OddData = new byte[] { 1, 3, 5, 7, 9 };

		[Test]
		public void ValidDataProperties()
		{
			var data = SKData.CreateCopy(OddData);

			Assert.AreEqual(OddData.Length, data.Size);
			Assert.AreEqual(OddData, data.ToArray());
		}

		[Test]
		public void ReleaseDataWasInvoked()
		{
			bool released = false;

			var onRelease = new SKDataReleaseDelegate((addr, ctx) => {
				Marshal.FreeCoTaskMem(addr);
				released = true;
				Assert.AreEqual("RELEASING!", ctx);
			});

			var memory = Marshal.AllocCoTaskMem(10);

			using (var data = SKData.Create(memory, 10, onRelease, "RELEASING!")) {
				Assert.AreEqual(memory, data.Data);
				Assert.AreEqual(10, data.Size);
			}

			Assert.True(released, "The SKDataReleaseDelegate was not called.");
		}

		[Test]
		[Ignore("Doesn't work as it relies on memory being overwritten by an external process.")]
		public void DataDisposedReturnsInvalidStream()
		{
			// create data
			var data = SKData.CreateCopy(OddData);

			// get the stream
			var stream = data.AsStream();

			// nuke the data
			data.Dispose();
			Assert.AreEqual(IntPtr.Zero, data.Handle);

			// read the stream
			var buffer = new byte[OddData.Length];
			stream.Read(buffer, 0, buffer.Length);

			// since the data was nuked, they will differ
			Assert.AreNotEqual(OddData, buffer);
		}
	}
}

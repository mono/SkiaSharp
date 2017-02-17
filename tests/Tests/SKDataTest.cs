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
			var data = new SKData(OddData);

			Assert.AreEqual(OddData.Length, data.Size);
			Assert.AreEqual(OddData, data.ToArray());
		}

		[Test]
		[Ignore("Doesn't work as it relies on memory being overwritten by an external process.")]
		public void DataDisposedReturnsInvalidStream()
		{
			// create data
			var data = new SKData(OddData);

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

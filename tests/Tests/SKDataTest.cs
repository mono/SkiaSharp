using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	[TestFixture]
	public class SKDataTest : SKTest
	{
		private readonly static byte[] OddData = new byte[] { 1, 3, 5, 7, 9 };

		[Test]
		public void ValidDataProperties()
		{
			var data = new SKData(OddData);

			Assert.AreEqual(OddData.Length, data.Size);
			CollectionAssert.AreEqual(OddData, data.ToArray());
		}
	}
}

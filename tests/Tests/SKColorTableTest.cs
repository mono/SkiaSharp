using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKColorTableTest : SKTest
	{
		private readonly static SKColor[] Colors = new SKColor[] 
		{ 
			SKColors.Black, 
			SKColors.White, 
			SKColors.Red, 
			SKColors.Green, 
			SKColors.Blue 
		};

		[Test]
		public void MembersRetrieveColors()
		{
			var colorTable = new SKColorTable(Colors);

			Assert.AreEqual(Colors.Length, colorTable.Count);

			Assert.AreEqual(Colors, colorTable.Colors);

			Assert.AreEqual(Colors[0], colorTable[0]);
			Assert.AreEqual(Colors[1], colorTable[1]);
			Assert.AreEqual(Colors[2], colorTable[2]);
			Assert.AreEqual(Colors[3], colorTable[3]);
			Assert.AreEqual(Colors[4], colorTable[4]);
		}

		[Test]
		public void IndexerOutOfRangeBelow()
		{
			var colorTable = new SKColorTable(Colors);

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var color = colorTable[-1];
			});
		}

		[Test]
		public void IndexerOutOfRangeAbove()
		{
			var colorTable = new SKColorTable(Colors);

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var color = colorTable[5];
			});
		}
	}
}

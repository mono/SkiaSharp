using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKColorTableTest : SKTest
	{
		private readonly static SKColor[] Colors = new SKColor[]
		{
			(SKColor)0x33008200,
			SKColors.Black,
			SKColors.White,
			SKColors.Red,
			SKColors.Green,
			SKColors.Blue
		};

		private readonly static SKPMColor[] PMColors = new SKPMColor[]
		{
			(SKPMColor)(SKColor)0x33008200,
			(SKPMColor)SKColors.Black,
			(SKPMColor)SKColors.White,
			(SKPMColor)SKColors.Red,
			(SKPMColor)SKColors.Green,
			(SKPMColor)SKColors.Blue
		};

		[Test]
		public void MembersRetrieveSingleColorWithAlpha()
		{
			var c = (SKColor)0x33008200;
			var pm = SKPMColor.PreMultiply(c);
			var upm = SKPMColor.UnPreMultiply(pm);

			Assert.AreEqual(new SKColor(0x33008200), c);
			Assert.AreEqual(new SKPMColor(0x33001A00), pm);
			Assert.AreEqual(new SKColor(0x33008200), upm);

			var ctContents = new [] { pm };
			var ct = new SKColorTable(ctContents, 1);

			Assert.AreEqual(1, ct.Count);

			Assert.AreEqual(new SKPMColor(0x33001A00), ct[0]);
		}

		[Test]
		public void MembersRetrieveColors()
		{
			var colorTable = new SKColorTable(Colors);

			Assert.AreEqual(Colors.Length, colorTable.Count);

			Assert.AreEqual(PMColors, colorTable.Colors);
			Assert.AreEqual(Colors, colorTable.UnPreMultipledColors);

			Assert.AreEqual(PMColors[0], colorTable[0]);
			Assert.AreEqual(PMColors[1], colorTable[1]);
			Assert.AreEqual(PMColors[2], colorTable[2]);
			Assert.AreEqual(PMColors[3], colorTable[3]);
			Assert.AreEqual(PMColors[4], colorTable[4]);
			Assert.AreEqual(PMColors[5], colorTable[5]);

			Assert.AreNotEqual(Colors[0], colorTable[0]);
			Assert.AreNotEqual(Colors[1], colorTable[1]);
			Assert.AreNotEqual(Colors[2], colorTable[2]);
			Assert.AreNotEqual(Colors[3], colorTable[3]);
			Assert.AreNotEqual(Colors[4], colorTable[4]);
			Assert.AreNotEqual(Colors[5], colorTable[5]);
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

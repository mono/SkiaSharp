using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

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

		[Obsolete]
		[SkippableFact]
		public void MembersRetrieveSingleColorWithAlpha()
		{
			var c = (SKColor)0x33008200;
			var pm = SKPMColor.PreMultiply(c);
			var upm = SKPMColor.UnPreMultiply(pm);

			Assert.Equal(new SKColor(0x33008200), c);
			Assert.Equal(new SKPMColor(0x33001A00), pm);
			Assert.Equal(new SKColor(0x33008200), upm);

			var ctContents = new[] { pm };
			var ct = new SKColorTable(ctContents, 1);

			Assert.Equal(1, ct.Count);

			Assert.Equal(new SKPMColor(0x33001A00), ct[0]);
		}

		[Obsolete]
		[SkippableFact]
		public void MembersRetrieveColors()
		{
			var colorTable = new SKColorTable(Colors);

			Assert.Equal(Colors.Length, colorTable.Count);

			Assert.Equal(PMColors, colorTable.Colors);
			Assert.Equal(Colors, colorTable.UnPreMultipledColors);

			Assert.Equal(PMColors[0], colorTable[0]);
			Assert.Equal(PMColors[1], colorTable[1]);
			Assert.Equal(PMColors[2], colorTable[2]);
			Assert.Equal(PMColors[3], colorTable[3]);
			Assert.Equal(PMColors[4], colorTable[4]);
			Assert.Equal(PMColors[5], colorTable[5]);
		}

		[Obsolete]
		[SkippableFact]
		public void IndexerOutOfRangeBelow()
		{
			var colorTable = new SKColorTable(Colors);

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var color = colorTable[-1];
			});
		}

		[Obsolete]
		[SkippableFact]
		public void IndexerOutOfRangeAbove()
		{
			var colorTable = new SKColorTable(Colors);

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var color = colorTable[250];
			});
		}
	}
}

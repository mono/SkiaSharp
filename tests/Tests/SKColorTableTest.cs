using System;
using System.IO;
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

			var ctContents = new[] { pm };
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
				var color = colorTable[250];
			});
		}

		[Test]
		public void Index8ImageHasColorTable()
		{
			var path = Path.Combine(PathToImages, "index8.png");

			var codec = SKCodec.Create(new SKFileStream(path));

			// It appears I can't use Unpremul as the alpha type, as the color table is not premultiplied then
			// https://groups.google.com/forum/#!topic/skia-discuss/mNUxQon5OMY
			var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Index8, SKAlphaType.Premul);

			var bitmap = SKBitmap.Decode(codec, info);

			var colorTable = bitmap.ColorTable;

			Assert.IsNotNull(colorTable);

			Assert.AreEqual((SKPMColor)0x000000, colorTable[0]);
			Assert.AreEqual((SKColor)0x000000, colorTable.GetUnPreMultipliedColor(0));

			if (IsWindows || IsLinux) {
				Assert.AreEqual((SKPMColor)0xFFA4C639, colorTable[255]);
			} else {
				Assert.AreEqual((SKPMColor)0xFF39C6A4, colorTable[255]);
			}
			Assert.AreEqual((SKColor)0xFFA4C639, colorTable.GetUnPreMultipliedColor(255));

			if (IsWindows || IsLinux) {
				Assert.AreEqual((SKPMColor)0x7E51621C, colorTable[140]);
			} else {
				Assert.AreEqual((SKPMColor)0x7E1C6251, colorTable[140]);
			}
			Assert.AreEqual((SKColor)0x7EA4C639, colorTable.GetUnPreMultipliedColor(140));

			if (IsWindows || IsLinux) {
				Assert.AreEqual((SKPMColor)0x7E51621C, bitmap.GetIndex8Color(182, 348));
			} else {
				Assert.AreEqual((SKPMColor)0x7E1C6251, bitmap.GetIndex8Color(182, 348));
			}
			Assert.AreEqual((SKColor)0x7EA4C639, bitmap.GetPixel(182, 348));
		}
	}
}

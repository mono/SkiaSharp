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

		[SkippableFact]
		public void CreateIndex8Bitmap()
		{
			var info = new SKImageInfo(320, 240, SKColorType.Index8, SKAlphaType.Opaque);
			var ct = new SKColorTable(Colors);
			var bitmap = new SKBitmap(info, ct);
			Assert.NotNull(bitmap);
			Assert.Equal(ct, bitmap.ColorTable);
		}

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

		[SkippableFact]
		public void IndexerOutOfRangeBelow()
		{
			var colorTable = new SKColorTable(Colors);

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var color = colorTable[-1];
			});
		}

		[SkippableFact]
		public void IndexerOutOfRangeAbove()
		{
			var colorTable = new SKColorTable(Colors);

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var color = colorTable[250];
			});
		}

		[SkippableFact]
		public void Index8ImageHasColorTable()
		{
			var path = Path.Combine(PathToImages, "index8.png");

			var codec = SKCodec.Create(new SKFileStream(path));

			// It appears I can't use Unpremul as the alpha type, as the color table is not premultiplied then
			// https://groups.google.com/forum/#!topic/skia-discuss/mNUxQon5OMY
			var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Index8, SKAlphaType.Premul);

			var bitmap = SKBitmap.Decode(codec, info);

			var colorTable = bitmap.ColorTable;

			Assert.NotNull(colorTable);

			Assert.Equal((SKPMColor)0x000000, colorTable[0]);
			Assert.Equal((SKColor)0x000000, colorTable.GetUnPreMultipliedColor(0));

			if (IsWindows || IsLinux) {
				Assert.Equal((SKPMColor)0xFFA4C639, colorTable[255]);
			} else {
				Assert.Equal((SKPMColor)0xFF39C6A4, colorTable[255]);
			}
			Assert.Equal((SKColor)0xFFA4C639, colorTable.GetUnPreMultipliedColor(255));

			if (IsWindows || IsLinux) {
				Assert.Equal((SKPMColor)0x7E51621C, colorTable[140]);
			} else {
				Assert.Equal((SKPMColor)0x7E1C6251, colorTable[140]);
			}
			Assert.Equal((SKColor)0x7EA4C639, colorTable.GetUnPreMultipliedColor(140));

			if (IsWindows || IsLinux) {
				Assert.Equal((SKPMColor)0x7E51621C, bitmap.GetIndex8Color(182, 348));
			} else {
				Assert.Equal((SKPMColor)0x7E1C6251, bitmap.GetIndex8Color(182, 348));
			}
			Assert.Equal((SKColor)0x7EA4C639, bitmap.GetPixel(182, 348));
		}

		[SkippableFact]
		public void Index8ImageCanChangeColorTable()
		{
			var path = Path.Combine(PathToImages, "index8.png");

			var codec = SKCodec.Create(new SKFileStream(path));
			var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Index8, SKAlphaType.Premul);
			var bitmap = SKBitmap.Decode(codec, info);

			var colorTable = bitmap.ColorTable;
			Assert.Equal((SKColor)0xFFA4C639, colorTable.GetUnPreMultipliedColor(255));

			var invertedColors = colorTable.Colors.Select(c =>
			{
				var c2 = (SKColor)c;
				var inv = new SKColor((byte)(255 - c2.Red), (byte)(255 - c2.Green), (byte)(255 - c2.Blue), c2.Alpha);
				return (SKPMColor)inv;
			}).ToArray();

			colorTable = new SKColorTable(invertedColors);
			bitmap.SetColorTable(colorTable);

			Assert.Equal((SKColor)0xFF5B39C6, colorTable.GetUnPreMultipliedColor(255));
		}
	}
}

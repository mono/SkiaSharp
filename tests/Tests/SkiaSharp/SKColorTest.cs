using System;
using System.Collections.Generic;
using Xunit;

using SKOtherColor = System.Tuple<float, float, float>;
using ToOtherColor = System.Tuple<SkiaSharp.SKColor, System.Tuple<float, float, float>, string>;

namespace SkiaSharp.Tests
{
	public class SKColorTest : SKTest
	{
		private const int Precision = 2;

		[SkippableFact]
		public void ColorWithComponent()
		{
			var color = new SKColor();
			Assert.Equal(0, color.Red);
			Assert.Equal(0, color.Green);
			Assert.Equal(0, color.Blue);
			Assert.Equal(0, color.Alpha);

			var red = color.WithRed(255);
			Assert.Equal(255, red.Red);
			Assert.Equal(0, red.Green);
			Assert.Equal(0, red.Blue);
			Assert.Equal(0, red.Alpha);

			var green = color.WithGreen(255);
			Assert.Equal(0, green.Red);
			Assert.Equal(255, green.Green);
			Assert.Equal(0, green.Blue);
			Assert.Equal(0, green.Alpha);

			var blue = color.WithBlue(255);
			Assert.Equal(0, blue.Red);
			Assert.Equal(0, blue.Green);
			Assert.Equal(255, blue.Blue);
			Assert.Equal(0, blue.Alpha);

			var alpha = color.WithAlpha(255);
			Assert.Equal(0, alpha.Red);
			Assert.Equal(0, alpha.Green);
			Assert.Equal(0, alpha.Blue);
			Assert.Equal(255, alpha.Alpha);
		}

		[SkippableFact]
		public void ColorRgbToHsl()
		{
			var tuples = new List<ToOtherColor> {
				new ToOtherColor(new SKColor(000, 000, 000), new SKOtherColor(000f, 000.0f, 000.0f), "Black"),
				new ToOtherColor(new SKColor(255, 000, 000), new SKOtherColor(000f, 100.0f, 050.0f), "Red"),
				new ToOtherColor(new SKColor(255, 255, 000), new SKOtherColor(060f, 100.0f, 050.0f), "Yellow"),
				new ToOtherColor(new SKColor(255, 255, 255), new SKOtherColor(000f, 000.0f, 100.0f), "White"),
				new ToOtherColor(new SKColor(128, 128, 128), new SKOtherColor(000f, 000.0f, 050.2f), "Gray"),
				new ToOtherColor(new SKColor(128, 128, 000), new SKOtherColor(060f, 100.0f, 025.1f), "Olive"),
				new ToOtherColor(new SKColor(000, 128, 000), new SKOtherColor(120f, 100.0f, 025.1f), "Green"),
				new ToOtherColor(new SKColor(000, 000, 128), new SKOtherColor(240f, 100.0f, 025.1f), "Navy"),
			};

			foreach (var item in tuples)
			{
				// values
				SKColor rgb = item.Item1;
				SKOtherColor other = item.Item2;

				// to HSL
				float h, s, l;
				rgb.ToHsl(out h, out s, out l);

				Assert.Equal((double)other.Item1, (double)h, Precision);
				Assert.Equal((double)other.Item2, (double)s, Precision);
				Assert.Equal((double)other.Item3, (double)l, Precision);

				// to RGB
				SKColor back = SKColor.FromHsl(other.Item1, other.Item2, other.Item3);

				Assert.Equal(rgb.Red, back.Red);
				Assert.Equal(rgb.Green, back.Green);
				Assert.Equal(rgb.Blue, back.Blue);
				Assert.Equal(rgb.Alpha, back.Alpha);
			}
		}

		[SkippableFact]
		public void ColorRgbToHsv()
		{
			var tuples = new List<ToOtherColor> {
				new ToOtherColor(new SKColor(000, 000, 000), new SKOtherColor(000f, 000.0f, 000.0f), "Black"),
				new ToOtherColor(new SKColor(255, 000, 000), new SKOtherColor(000f, 100.0f, 100.0f), "Red"),
				new ToOtherColor(new SKColor(255, 255, 000), new SKOtherColor(060f, 100.0f, 100.0f), "Yellow"),
				new ToOtherColor(new SKColor(255, 255, 255), new SKOtherColor(000f, 000.0f, 100.0f), "White"),
				new ToOtherColor(new SKColor(128, 128, 128), new SKOtherColor(000f, 000.0f, 050.2f), "Gray"),
				new ToOtherColor(new SKColor(128, 128, 000), new SKOtherColor(060f, 100.0f, 050.2f), "Olive"),
				new ToOtherColor(new SKColor(000, 128, 000), new SKOtherColor(120f, 100.0f, 050.2f), "Green"),
				new ToOtherColor(new SKColor(000, 000, 128), new SKOtherColor(240f, 100.0f, 050.2f), "Navy"),
			};

			foreach (var item in tuples)
			{
				// values
				SKColor rgb = item.Item1;
				SKOtherColor other = item.Item2;

				// to HSV
				float h, s, v;
				rgb.ToHsv(out h, out s, out v);

				Assert.Equal((double)other.Item1, (double)h, Precision);
				Assert.Equal((double)other.Item2, (double)s, Precision);
				Assert.Equal((double)other.Item3, (double)v, Precision);

				// to RGB
				SKColor back = SKColor.FromHsv(other.Item1, other.Item2, other.Item3);

				Assert.Equal(rgb.Red, back.Red);
				Assert.Equal(rgb.Green, back.Green);
				Assert.Equal(rgb.Blue, back.Blue);
				Assert.Equal(rgb.Alpha, back.Alpha);
			}
		}

		[SkippableFact]
		public void HexToColor()
		{
			var tuples = new List<Tuple<string, SKColor>> {
				new Tuple<string, SKColor>("#ABC", (SKColor)0xFFAABBCC),
				new Tuple<string, SKColor>("#ABCD", (SKColor)0xAABBCCDD),
				new Tuple<string, SKColor>("#ABCDEF", (SKColor)0xFFABCDEF),
				new Tuple<string, SKColor>("#AAABACAD", (SKColor)0xAAABACAD),
				new Tuple<string, SKColor>("#A1C", (SKColor)0xFFAA11CC),
				new Tuple<string, SKColor>("#A2C3", (SKColor)0xAA22CC33),
				new Tuple<string, SKColor>("#A4C5E6", (SKColor)0xFFA4C5E6),
				new Tuple<string, SKColor>("#A7A8A9A0", (SKColor)0xA7A8A9A0),
				new Tuple<string, SKColor>("ABC", (SKColor)0xFFAABBCC),
				new Tuple<string, SKColor>("ABCD", (SKColor)0xAABBCCDD),
				new Tuple<string, SKColor>("ABCDEF", (SKColor)0xFFABCDEF),
				new Tuple<string, SKColor>("AAABACAD", (SKColor)0xAAABACAD),
				new Tuple<string, SKColor>("A1C", (SKColor)0xFFAA11CC),
				new Tuple<string, SKColor>("A2C3", (SKColor)0xAA22CC33),
				new Tuple<string, SKColor>("A4C5E6", (SKColor)0xFFA4C5E6),
				new Tuple<string, SKColor>("A7A8A9A0", (SKColor)0xA7A8A9A0),
			};

			foreach (var item in tuples)
			{
				// values
				string hex = item.Item1;
				SKColor other = item.Item2;

				SKColor color = SKColor.Parse(hex);

				Assert.Equal(other, color);
			}
		}

		[SkippableFact]
		public void InvalidHexToColor()
		{
			var tuples = new List<string> {
				"#ABCDE",
				"#123456ug",
				"12sd",
				"11111111111111",
			};

			foreach (var item in tuples)
			{
				// values
				string hex = item;

				SKColor color;
				var result = SKColor.TryParse(hex, out color);

				Assert.False(result, hex);
			}
		}

		[SkippableFact]
		public void PremultipliedColorsHaveCorrectBitShift()
		{
			var isARGB =
#if __ANDROID__
				false;
#else
				IsWindows || IsLinux;
#endif

			var color = (SKColor)0x12345678;

			Assert.Equal(new SKColor(0x34, 0x56, 0x78, 0x12), color);

			SKPMColor pmcolor;
			if (isARGB) {
				pmcolor = (SKPMColor)0x12345678;
			} else {
				pmcolor = (SKPMColor)0x12785634;
			}

			Assert.Equal(0x12, color.Alpha);
			Assert.Equal(0x12, pmcolor.Alpha);

			Assert.Equal(0x34, color.Red);
			Assert.Equal(0x34, pmcolor.Red);

			Assert.Equal(0x56, color.Green);
			Assert.Equal(0x56, pmcolor.Green);

			Assert.Equal(0x78, color.Blue);
			Assert.Equal(0x78, pmcolor.Blue);

			if (isARGB) {
				// ARGB
				Assert.Equal(24, SKImageInfo.PlatformColorAlphaShift);
				Assert.Equal(16, SKImageInfo.PlatformColorRedShift);
				Assert.Equal(8, SKImageInfo.PlatformColorGreenShift);
				Assert.Equal(0, SKImageInfo.PlatformColorBlueShift);
			} else {
				// ABGR
				Assert.Equal(24, SKImageInfo.PlatformColorAlphaShift);
				Assert.Equal(0, SKImageInfo.PlatformColorRedShift);
				Assert.Equal(8, SKImageInfo.PlatformColorGreenShift);
				Assert.Equal(16, SKImageInfo.PlatformColorBlueShift);
			}
		}

		[SkippableFact]
		public void MakeSureColorsAreNotBroken()
		{
			var color = new SKColor(100, 0, 0, 100);

			var paint = new SKPaint();
			paint.Color = color;

			Assert.Equal(color, paint.Color);
		}

		[SkippableFact]
		public void GetHashCodeIsConsistent()
		{
			var color1 = new SKColor(100, 0, 0, 100);
			var color2 = new SKColor(100, 0, 0, 100);

			Assert.Equal(color1.GetHashCode(), color2.GetHashCode());
		}

		[Obsolete]
		[SkippableFact]
		public void CanPreMultiplyArrays()
		{
			var colors = new SKColor[] { 0x33008200, 0x33008200, 0x33008200, 0x33008200, 0x33008200 };
			var pmcolors = new SKPMColor[] { 0x33001A00, 0x33001A00, 0x33001A00, 0x33001A00, 0x33001A00 };

			var pm = SKPMColor.PreMultiply(colors);

			Assert.Equal(pmcolors, pm);
		}

		[Obsolete]
		[SkippableFact]
		public void CanUnPreultiplyArrays()
		{
			var colors = new SKColor[] { 0x33008200, 0x33008200, 0x33008200, 0x33008200, 0x33008200 };
			var pmcolors = new SKPMColor[] { 0x33001A00, 0x33001A00, 0x33001A00, 0x33001A00, 0x33001A00 };

			var upm = SKPMColor.UnPreMultiply(pmcolors);

			Assert.Equal(colors, upm);
		}
	}
}

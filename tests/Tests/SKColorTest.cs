using System;
using System.Collections.Generic;
using NUnit.Framework;

using SKOtherColor = System.Tuple<float, float, float>;
using ToOtherColor = System.Tuple<SkiaSharp.SKColor, System.Tuple<float, float, float>, string>;

namespace SkiaSharp.Tests
{
	[TestFixture]
	public class SKColorTest : SKTest
	{
		private const float EPSILON = 0.01f;

		[Test]
		public void ColorWithComponent()
		{
			var color = new SKColor();
			Assert.AreEqual(0, color.Red);
			Assert.AreEqual(0, color.Green);
			Assert.AreEqual(0, color.Blue);
			Assert.AreEqual(0, color.Alpha);

			var red = color.WithRed(255);
			Assert.AreEqual(255, red.Red);
			Assert.AreEqual(0, red.Green);
			Assert.AreEqual(0, red.Blue);
			Assert.AreEqual(0, red.Alpha);

			var green = color.WithGreen(255);
			Assert.AreEqual(0, green.Red);
			Assert.AreEqual(255, green.Green);
			Assert.AreEqual(0, green.Blue);
			Assert.AreEqual(0, green.Alpha);

			var blue = color.WithBlue(255);
			Assert.AreEqual(0, blue.Red);
			Assert.AreEqual(0, blue.Green);
			Assert.AreEqual(255, blue.Blue);
			Assert.AreEqual(0, blue.Alpha);

			var alpha = color.WithAlpha(255);
			Assert.AreEqual(0, alpha.Red);
			Assert.AreEqual(0, alpha.Green);
			Assert.AreEqual(0, alpha.Blue);
			Assert.AreEqual(255, alpha.Alpha);
		}

		[Test]
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

				Assert.AreEqual(other.Item1, h, EPSILON, item.Item3 + " H");
				Assert.AreEqual(other.Item2, s, EPSILON, item.Item3 + " S");
				Assert.AreEqual(other.Item3, l, EPSILON, item.Item3 + " L");

				// to RGB
				SKColor back = SKColor.FromHsl(other.Item1, other.Item2, other.Item3);

				Assert.AreEqual(rgb.Red, back.Red, item.Item3 + " R");
				Assert.AreEqual(rgb.Green, back.Green, item.Item3 + " G");
				Assert.AreEqual(rgb.Blue, back.Blue, item.Item3 + " B");
				Assert.AreEqual(rgb.Alpha, back.Alpha, item.Item3 + " A");
			}
		}

		[Test]
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

				Assert.AreEqual(other.Item1, h, EPSILON, item.Item3 + " H");
				Assert.AreEqual(other.Item2, s, EPSILON, item.Item3 + " S");
				Assert.AreEqual(other.Item3, v, EPSILON, item.Item3 + " V");

				// to RGB
				SKColor back = SKColor.FromHsv(other.Item1, other.Item2, other.Item3);

				Assert.AreEqual(rgb.Red, back.Red, item.Item3 + " R");
				Assert.AreEqual(rgb.Green, back.Green, item.Item3 + " G");
				Assert.AreEqual(rgb.Blue, back.Blue, item.Item3 + " B");
				Assert.AreEqual(rgb.Alpha, back.Alpha, item.Item3 + " A");
			}
		}
	}
}

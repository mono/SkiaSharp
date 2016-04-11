using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	[TestFixture]
	public class SKColorTest : SKTest
	{
        private void AssertEquivalent(byte r, byte g, byte b, float hue, float saturation, float value)
		{
			SKColor color = new SKColor (r, g, b);
			float checkh, checks, checkv;
			SKColor.ToHSV (color, out checkh, out checks, out checkv);
			// Allow a small delta for rounding errors.
			Assert.AreEqual (hue, checkh, 0.01);
			Assert.AreEqual (saturation, checks, 0.01);
			Assert.AreEqual (value, checkv, 0.01);

			color = SKColor.FromHSV (hue, saturation, value);
			// Allow a small delta for rounding errors.
            Assert.AreEqual (r, color.Red, 1);
            Assert.AreEqual (g, color.Green, 1);
            Assert.AreEqual (b, color.Blue, 1);
        }

		[Test]
        public void TestRGBHSVConversions()
        {
            AssertEquivalent (0x00, 0x00, 0x00, 0, 0, 0);
            AssertEquivalent (0xff, 0xff, 0xff, 0, 0, 1);
            AssertEquivalent (0xff, 0x00, 0x00, 0, 1, 1);
            AssertEquivalent (0x00, 0xff, 0x00, 120, 1, 1);
            AssertEquivalent (0x00, 0x00, 0xff, 240, 1, 1);
            AssertEquivalent (0x00, 0xff, 0xff, 180, 1, 1);
            AssertEquivalent (0xff, 0xff, 0x00, 60, 1, 1);
            AssertEquivalent (0x80, 0x80, 0x80, 0, 0, 0.5f);
            AssertEquivalent (0xc0, 0xc0, 0xc0, 0, 0, 0.75f);
			AssertEquivalent (0x40, 0x80, 0x40, 120, 0.5f, 0.5f);
        }
	}
}

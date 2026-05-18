using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBColorTest : HBTest
	{
		// Construction

		[SkippableFact]
		public void ConstructorFromComponentsRoundtrips ()
		{
			var color = new HBColor (0xAA, 0xBB, 0xCC, 0xFF);

			Assert.Equal ((byte)0xAA, color.Red);
			Assert.Equal ((byte)0xBB, color.Green);
			Assert.Equal ((byte)0xCC, color.Blue);
			Assert.Equal ((byte)0xFF, color.Alpha);
		}

		[SkippableFact]
		public void ConstructorFromRawValuePreservesLayout ()
		{
			// hb_color_t layout: 0xBBGGRRAA
			var color = new HBColor (0xCCBBAA_FFu);

			Assert.Equal ((byte)0xAA, color.Red);
			Assert.Equal ((byte)0xBB, color.Green);
			Assert.Equal ((byte)0xCC, color.Blue);
			Assert.Equal ((byte)0xFF, color.Alpha);
			Assert.Equal (0xCCBBAA_FFu, color.Value);
		}

		[SkippableFact]
		public void ComponentConstructorProducesCorrectRawValue ()
		{
			var color = new HBColor (0xAA, 0xBB, 0xCC, 0xFF);

			// Verify raw uint layout: 0xBBGGRRAA
			Assert.Equal (0xCCBBAA_FFu, color.Value);
		}

		// Well-known colors

		[SkippableFact]
		public void OpaqueBlack ()
		{
			var color = new HBColor (0, 0, 0, 255);

			Assert.Equal ((byte)0, color.Red);
			Assert.Equal ((byte)0, color.Green);
			Assert.Equal ((byte)0, color.Blue);
			Assert.Equal ((byte)255, color.Alpha);
			Assert.Equal (0x000000_FFu, color.Value);
		}

		[SkippableFact]
		public void OpaqueWhite ()
		{
			var color = new HBColor (255, 255, 255, 255);

			Assert.Equal ((byte)255, color.Red);
			Assert.Equal ((byte)255, color.Green);
			Assert.Equal ((byte)255, color.Blue);
			Assert.Equal ((byte)255, color.Alpha);
			Assert.Equal (0xFFFFFF_FFu, color.Value);
		}

		[SkippableFact]
		public void TransparentBlack ()
		{
			var color = new HBColor (0, 0, 0, 0);

			Assert.Equal ((byte)0, color.Red);
			Assert.Equal ((byte)0, color.Green);
			Assert.Equal ((byte)0, color.Blue);
			Assert.Equal ((byte)0, color.Alpha);
			Assert.Equal (0x00000000u, color.Value);
		}

		[SkippableFact]
		public void PureRedIsDistinctFromPureBlue ()
		{
			var red = new HBColor (255, 0, 0, 255);
			var blue = new HBColor (0, 0, 255, 255);

			Assert.NotEqual (red.Value, blue.Value);
			Assert.Equal ((byte)255, red.Red);
			Assert.Equal ((byte)0, red.Blue);
			Assert.Equal ((byte)0, blue.Red);
			Assert.Equal ((byte)255, blue.Blue);
		}

		// Operators

		[SkippableFact]
		public void ImplicitConversionToUint ()
		{
			var color = new HBColor (0xAA, 0xBB, 0xCC, 0xFF);
			uint value = color;

			Assert.Equal (color.Value, value);
		}

		[SkippableFact]
		public void ExplicitConversionFromUint ()
		{
			uint raw = 0xCCBBAA_FFu;
			var color = (HBColor)raw;

			Assert.Equal ((byte)0xAA, color.Red);
			Assert.Equal ((byte)0xBB, color.Green);
			Assert.Equal ((byte)0xCC, color.Blue);
			Assert.Equal ((byte)0xFF, color.Alpha);
		}

		// Equality

		[SkippableFact]
		public void EqualColorsAreEqual ()
		{
			var a = new HBColor (10, 20, 30, 40);
			var b = new HBColor (10, 20, 30, 40);

			Assert.True (a == b);
			Assert.False (a != b);
			Assert.True (a.Equals (b));
			Assert.True (a.Equals ((object)b));
			Assert.Equal (a.GetHashCode (), b.GetHashCode ());
		}

		[SkippableFact]
		public void DifferentColorsAreNotEqual ()
		{
			var a = new HBColor (10, 20, 30, 40);
			var b = new HBColor (10, 20, 30, 41);

			Assert.False (a == b);
			Assert.True (a != b);
			Assert.False (a.Equals (b));
		}

		[SkippableFact]
		public void NotEqualToNull ()
		{
			var color = new HBColor (1, 2, 3, 4);

			Assert.False (color.Equals (null));
		}

		// ToString

		[SkippableFact]
		public void ToStringFormatsAsARGBHex ()
		{
			var color = new HBColor (0xAA, 0xBB, 0xCC, 0xFF);

			Assert.Equal ("#FFAABBCC", color.ToString ());
		}

		[SkippableFact]
		public void ToStringPadsWithZeros ()
		{
			var color = new HBColor (0, 0, 0, 0);

			Assert.Equal ("#00000000", color.ToString ());
		}
	}
}

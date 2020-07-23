using System.Collections.Generic;
using Xunit;

using SKOtherColor = System.Tuple<float, float, float>;

namespace SkiaSharp.Tests
{
	public class SKColorFTest : SKTest
	{
		private const int Precision = 2;

		[SkippableFact]
		public void MakeSureColorsAreNotBroken()
		{
			var color = new SKColorF(0.4f, 0, 0, 0.6f);

			var paint = new SKPaint();
			paint.ColorF = color;

			Assert.Equal(color, paint.ColorF);
			Assert.Equal((SKColor)color, paint.Color);
		}

		[SkippableFact]
		public void MakeSureColorsWithColorSpacesWork()
		{
			var color = new SKColorF(0.3f, 0, 0, 0.6f);

			var paint = new SKPaint();
			paint.SetColor(color, SKColorSpace.CreateSrgbLinear());

			Assert.NotEqual(color, paint.ColorF);
		}

		[SkippableFact]
		public void ColorWithComponent()
		{
			var color = new SKColorF();
			Assert.Equal(0, color.Red);
			Assert.Equal(0, color.Green);
			Assert.Equal(0, color.Blue);
			Assert.Equal(0, color.Alpha);

			var red = color.WithRed(0.5f);
			Assert.Equal(0.5f, red.Red);
			Assert.Equal(0, red.Green);
			Assert.Equal(0, red.Blue);
			Assert.Equal(0, red.Alpha);

			var green = color.WithGreen(0.5f);
			Assert.Equal(0, green.Red);
			Assert.Equal(0.5f, green.Green);
			Assert.Equal(0, green.Blue);
			Assert.Equal(0, green.Alpha);

			var blue = color.WithBlue(0.5f);
			Assert.Equal(0, blue.Red);
			Assert.Equal(0, blue.Green);
			Assert.Equal(0.5f, blue.Blue);
			Assert.Equal(0, blue.Alpha);

			var alpha = color.WithAlpha(0.5f);
			Assert.Equal(0, alpha.Red);
			Assert.Equal(0, alpha.Green);
			Assert.Equal(0, alpha.Blue);
			Assert.Equal(0.5f, alpha.Alpha);
		}

		public static IEnumerable<object[]> GetBasicColorFToColorConversionData()
		{
			// B/W
			yield return new object[] { SKColors.Black, new SKColorF(0, 0, 0, 1) };
			yield return new object[] { SKColors.White, new SKColorF(1, 1, 1, 1) };
			// R/G/B
			yield return new object[] { SKColors.Red, new SKColorF(1, 0, 0, 1) };
			yield return new object[] { SKColors.Lime, new SKColorF(0, 1, 0, 1) };
			yield return new object[] { SKColors.Blue, new SKColorF(0, 0, 1, 1) };
			// Empty
			yield return new object[] { new SKColor(0, 0, 0, 0), new SKColorF(0, 0, 0, 0) };
			yield return new object[] { new SKColor(0), new SKColorF(0, 0, 0, 0) };
			yield return new object[] { new SKColor(), new SKColorF() };
		}

		[SkippableTheory]
		[MemberData(nameof(GetBasicColorFToColorConversionData))]
		public void BasicColorFToColorConversion(SKColor color, SKColorF colorf)
		{
			var fromF = (SKColor)colorf;

			Assert.Equal(color, fromF);
		}

		[SkippableTheory]
		[MemberData(nameof(GetBasicColorFToColorConversionData))]
		public void BasicColorToColorFConversion(SKColor color, SKColorF colorf)
		{
			var toF = (SKColorF)color;

			Assert.Equal(colorf, toF);
		}

		public static IEnumerable<object[]> GetColorRgbToHslData()
		{
			yield return new object[] { new SKColorF(0.0f, 0.0f, 0.0f), new SKOtherColor(000f, 000.0f, 000.0f) };
			yield return new object[] { new SKColorF(1.0f, 0.0f, 0.0f), new SKOtherColor(000f, 100.0f, 050.0f) };
			yield return new object[] { new SKColorF(1.0f, 1.0f, 0.0f), new SKOtherColor(060f, 100.0f, 050.0f) };
			yield return new object[] { new SKColorF(1.0f, 1.0f, 1.0f), new SKOtherColor(000f, 000.0f, 100.0f) };
			yield return new object[] { new SKColorF(0.5f, 0.5f, 0.5f), new SKOtherColor(000f, 000.0f, 050.0f) };
			yield return new object[] { new SKColorF(0.5f, 0.5f, 0.0f), new SKOtherColor(060f, 100.0f, 025.0f) };
			yield return new object[] { new SKColorF(0.0f, 0.5f, 0.0f), new SKOtherColor(120f, 100.0f, 025.0f) };
			yield return new object[] { new SKColorF(0.0f, 0.0f, 0.5f), new SKOtherColor(240f, 100.0f, 025.0f) };
		}

		[SkippableTheory]
		[MemberData(nameof(GetColorRgbToHslData))]
		public void ColorRgbToHsl(SKColorF rgb, SKOtherColor other)
		{
			rgb.ToHsl(out var h, out var s, out var l);

			Assert.Equal(other.Item1, h, Precision);
			Assert.Equal(other.Item2, s, Precision);
			Assert.Equal(other.Item3, l, Precision);
		}

		[SkippableTheory]
		[MemberData(nameof(GetColorRgbToHslData))]
		public void ColorHsltoRgb(SKColorF rgb, SKOtherColor other)
		{
			var back = SKColorF.FromHsl(other.Item1, other.Item2, other.Item3);

			Assert.Equal(rgb.Red, back.Red, Precision);
			Assert.Equal(rgb.Green, back.Green, Precision);
			Assert.Equal(rgb.Blue, back.Blue, Precision);
			Assert.Equal(rgb.Alpha, back.Alpha, Precision);
		}

		public static IEnumerable<object[]> GetColorRgbToHsvData()
		{
			yield return new object[] { new SKColorF(0.0f, 0.0f, 0.0f), new SKOtherColor(000f, 000.0f, 000.0f) };
			yield return new object[] { new SKColorF(1.0f, 0.0f, 0.0f), new SKOtherColor(000f, 100.0f, 100.0f) };
			yield return new object[] { new SKColorF(1.0f, 1.0f, 0.0f), new SKOtherColor(060f, 100.0f, 100.0f) };
			yield return new object[] { new SKColorF(1.0f, 1.0f, 1.0f), new SKOtherColor(000f, 000.0f, 100.0f) };
			yield return new object[] { new SKColorF(0.5f, 0.5f, 0.5f), new SKOtherColor(000f, 000.0f, 050.0f) };
			yield return new object[] { new SKColorF(0.5f, 0.5f, 0.0f), new SKOtherColor(060f, 100.0f, 050.0f) };
			yield return new object[] { new SKColorF(0.0f, 0.5f, 0.0f), new SKOtherColor(120f, 100.0f, 050.0f) };
			yield return new object[] { new SKColorF(0.0f, 0.0f, 0.5f), new SKOtherColor(240f, 100.0f, 050.0f) };
		}

		[SkippableTheory]
		[MemberData(nameof(GetColorRgbToHsvData))]
		public void ColorRgbToHsv(SKColorF rgb, SKOtherColor other)
		{
			rgb.ToHsv(out var h, out var s, out var v);

			Assert.Equal(other.Item1, h, Precision);
			Assert.Equal(other.Item2, s, Precision);
			Assert.Equal(other.Item3, v, Precision);
		}

		[SkippableTheory]
		[MemberData(nameof(GetColorRgbToHsvData))]
		public void ColorHsvtoRgb(SKColorF rgb, SKOtherColor other)
		{
			var back = SKColorF.FromHsv(other.Item1, other.Item2, other.Item3);

			Assert.Equal(rgb.Red, back.Red, Precision);
			Assert.Equal(rgb.Green, back.Green, Precision);
			Assert.Equal(rgb.Blue, back.Blue, Precision);
			Assert.Equal(rgb.Alpha, back.Alpha, Precision);
		}

		public static IEnumerable<object[]> GetClampData()
		{
			// base
			yield return new object[] { new SKColorF(0, 0, 0, 0), new SKColorF(0, 0, 0, 0) };
			yield return new object[] { new SKColorF(0, 1, 0, 1), new SKColorF(0, 1, 0, 1) };
			yield return new object[] { new SKColorF(0, 0, 1, 1), new SKColorF(0, 0, 1, 1) };
			yield return new object[] { new SKColorF(1, 0, 0, 1), new SKColorF(1, 0, 0, 1) };
			yield return new object[] { new SKColorF(0, 1, 0, 1), new SKColorF(0, 1, 0, 1) };
			yield return new object[] { new SKColorF(1, 1, 1, 1), new SKColorF(1, 1, 1, 1) };
			// below
			yield return new object[] { new SKColorF(0, 0, 0, 0), new SKColorF(0, 0, 0, 0) };
			yield return new object[] { new SKColorF(0, -0.5f, 0, 1), new SKColorF(0, 0, 0, 1) };
			yield return new object[] { new SKColorF(0, 0, -0.5f, 1), new SKColorF(0, 0, 0, 1) };
			yield return new object[] { new SKColorF(-0.5f, 0, 0, 1), new SKColorF(0, 0, 0, 1) };
			yield return new object[] { new SKColorF(0, -0.5f, 0, 1), new SKColorF(0, 0, 0, 1) };
			yield return new object[] { new SKColorF(-0.5f, -0.5f, -0.5f, -0.5f), new SKColorF(0, 0, 0, 0) };
			// above
			yield return new object[] { new SKColorF(0, 0, 0, 0), new SKColorF(0, 0, 0, 0) };
			yield return new object[] { new SKColorF(0, 1.5f, 0, 1), new SKColorF(0, 1, 0, 1) };
			yield return new object[] { new SKColorF(0, 0, 1.5f, 1), new SKColorF(0, 0, 1, 1) };
			yield return new object[] { new SKColorF(1.5f, 0, 0, 1), new SKColorF(1, 0, 0, 1) };
			yield return new object[] { new SKColorF(0, 1.5f, 0, 1), new SKColorF(0, 1, 0, 1) };
			yield return new object[] { new SKColorF(1.5f, 1.5f, 1.5f, 1.5f), new SKColorF(1, 1, 1, 1) };
		}

		[SkippableTheory]
		[MemberData(nameof(GetClampData))]
		public void Clamp(SKColorF before, SKColorF after)
		{
			var clamp = before.Clamp();

			Assert.Equal(after, clamp);
		}

	}
}

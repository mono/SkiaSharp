using System.IO;

using Xunit;

using SkiaSharp.HarfBuzz;

namespace HarfBuzzSharp.Tests
{
	public class ColorExtensionsTest : HBTest
	{
		// HBColor -> SKColor

		[SkippableFact]
		public void ToSKColorPreservesChannels ()
		{
			var hb = new HBColor (0xAA, 0xBB, 0xCC, 0xFF);
			var sk = hb.ToSKColor ();

			Assert.Equal ((byte)0xAA, sk.Red);
			Assert.Equal ((byte)0xBB, sk.Green);
			Assert.Equal ((byte)0xCC, sk.Blue);
			Assert.Equal ((byte)0xFF, sk.Alpha);
		}

		[SkippableFact]
		public void ToSKColorWithOpaqueBlack ()
		{
			var hb = new HBColor (0, 0, 0, 255);
			var sk = hb.ToSKColor ();

			Assert.Equal (SkiaSharp.SKColors.Black, sk);
		}

		[SkippableFact]
		public void ToSKColorWithOpaqueWhite ()
		{
			var hb = new HBColor (255, 255, 255, 255);
			var sk = hb.ToSKColor ();

			Assert.Equal (SkiaSharp.SKColors.White, sk);
		}

		[SkippableFact]
		public void ToSKColorWithTransparent ()
		{
			var hb = new HBColor (0, 0, 0, 0);
			var sk = hb.ToSKColor ();

			Assert.Equal ((byte)0, sk.Alpha);
		}

		[SkippableFact]
		public void ToSKColorWorksWithRealPaletteColors ()
		{
			using var blob = Blob.FromFile (Path.Combine (PathToFonts, "test_glyphs-COLRv1.ttf"));
			using var face = new Face (blob, 0);
			var colors = face.GetPaletteColors (0);
			Assert.NotEmpty (colors);

			foreach (var hbColor in colors) {
				var skColor = hbColor.ToSKColor ();

				Assert.Equal (hbColor.Red, skColor.Red);
				Assert.Equal (hbColor.Green, skColor.Green);
				Assert.Equal (hbColor.Blue, skColor.Blue);
				Assert.Equal (hbColor.Alpha, skColor.Alpha);
			}
		}

		// HBColor -> SKColorF

		[SkippableFact]
		public void ToSKColorFNormalizesChannels ()
		{
			var hb = new HBColor (255, 128, 0, 204);
			var skF = hb.ToSKColorF ();

			Assert.Equal (1.0f, skF.Red, 0.01f);
			Assert.Equal (128 / 255f, skF.Green, 0.01f);
			Assert.Equal (0.0f, skF.Blue, 0.01f);
			Assert.Equal (204 / 255f, skF.Alpha, 0.01f);
		}

		[SkippableFact]
		public void ToSKColorFOpaqueWhiteIsAllOnes ()
		{
			var hb = new HBColor (255, 255, 255, 255);
			var skF = hb.ToSKColorF ();

			Assert.Equal (1.0f, skF.Red, 0.001f);
			Assert.Equal (1.0f, skF.Green, 0.001f);
			Assert.Equal (1.0f, skF.Blue, 0.001f);
			Assert.Equal (1.0f, skF.Alpha, 0.001f);
		}

		[SkippableFact]
		public void ToSKColorFBlackIsAllZeros ()
		{
			var hb = new HBColor (0, 0, 0, 0);
			var skF = hb.ToSKColorF ();

			Assert.Equal (0.0f, skF.Red);
			Assert.Equal (0.0f, skF.Green);
			Assert.Equal (0.0f, skF.Blue);
			Assert.Equal (0.0f, skF.Alpha);
		}

		// SKColor -> HBColor

		[SkippableFact]
		public void SKColorToHBColorRoundtrips ()
		{
			var original = new SkiaSharp.SKColor (0xAA, 0xBB, 0xCC, 0xFF);
			var hbColor = original.ToHBColor ();
			var converted = hbColor.ToSKColor ();

			Assert.Equal (original, converted);
		}

		[SkippableFact]
		public void SKColorToHBColorPreservesChannels ()
		{
			var sk = new SkiaSharp.SKColor (10, 20, 30, 40);
			var hb = sk.ToHBColor ();

			Assert.Equal ((byte)10, hb.Red);
			Assert.Equal ((byte)20, hb.Green);
			Assert.Equal ((byte)30, hb.Blue);
			Assert.Equal ((byte)40, hb.Alpha);
		}

		// SKColorF -> HBColor

		[SkippableFact]
		public void SKColorFToHBColorConvertsCorrectly ()
		{
			var skF = new SkiaSharp.SKColorF (1.0f, 0.5f, 0.0f, 0.8f);
			var hb = skF.ToHBColor ();

			Assert.Equal ((byte)255, hb.Red);
			Assert.Equal ((byte)128, hb.Green);
			Assert.Equal ((byte)0, hb.Blue);
			Assert.Equal ((byte)204, hb.Alpha);
		}

		[SkippableFact]
		public void SKColorFToHBColorClampsNegativeValues ()
		{
			var skF = new SkiaSharp.SKColorF (-0.5f, -1.0f, 0.0f, 1.0f);
			var hb = skF.ToHBColor ();

			Assert.Equal ((byte)0, hb.Red);
			Assert.Equal ((byte)0, hb.Green);
			Assert.Equal ((byte)0, hb.Blue);
			Assert.Equal ((byte)255, hb.Alpha);
		}

		[SkippableFact]
		public void SKColorFToHBColorClampsOverflowValues ()
		{
			var skF = new SkiaSharp.SKColorF (2.0f, 1.5f, 1.0f, 0.0f);
			var hb = skF.ToHBColor ();

			Assert.Equal ((byte)255, hb.Red);
			Assert.Equal ((byte)255, hb.Green);
			Assert.Equal ((byte)255, hb.Blue);
			Assert.Equal ((byte)0, hb.Alpha);
		}

		[SkippableFact]
		public void SKColorFToHBColorRoundtrips ()
		{
			// Due to 8-bit quantization, use values that map cleanly
			var original = new SkiaSharp.SKColorF (1.0f, 0.0f, 1.0f, 1.0f);
			var hb = original.ToHBColor ();
			var converted = hb.ToSKColorF ();

			Assert.Equal (original.Red, converted.Red, 0.01f);
			Assert.Equal (original.Green, converted.Green, 0.01f);
			Assert.Equal (original.Blue, converted.Blue, 0.01f);
			Assert.Equal (original.Alpha, converted.Alpha, 0.01f);
		}

		// Batch: ToSKColors

		[SkippableFact]
		public void ToSKColorsConvertsArray ()
		{
			var hbColors = new[] {
				new HBColor (255, 0, 0, 255),
				new HBColor (0, 255, 0, 255),
				new HBColor (0, 0, 255, 128),
			};
			var skColors = hbColors.ToSKColors ();

			Assert.Equal (3, skColors.Length);

			Assert.Equal ((byte)255, skColors[0].Red);
			Assert.Equal ((byte)0, skColors[0].Green);
			Assert.Equal ((byte)0, skColors[0].Blue);

			Assert.Equal ((byte)0, skColors[1].Red);
			Assert.Equal ((byte)255, skColors[1].Green);
			Assert.Equal ((byte)0, skColors[1].Blue);

			Assert.Equal ((byte)0, skColors[2].Red);
			Assert.Equal ((byte)0, skColors[2].Green);
			Assert.Equal ((byte)255, skColors[2].Blue);
			Assert.Equal ((byte)128, skColors[2].Alpha);
		}

		[SkippableFact]
		public void ToSKColorsWithNullReturnsNull ()
		{
			HBColor[] hbColors = null;
			var result = hbColors.ToSKColors ();

			Assert.Null (result);
		}

		[SkippableFact]
		public void ToSKColorsWithEmptyReturnsEmpty ()
		{
			var hbColors = System.Array.Empty<HBColor> ();
			var result = hbColors.ToSKColors ();

			Assert.Empty (result);
		}
	}
}

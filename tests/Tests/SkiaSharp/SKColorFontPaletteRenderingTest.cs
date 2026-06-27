using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests;

// Verifies COLR/CPAL palette selection (SKTypeface.Clone(paletteIndex) and per-entry
// SKFontArguments.PaletteOverrides) actually changes what is rendered.
//
// This works on FreeType (Linux/Android/WASM) and DirectWrite (Windows) via the platform COLR
// engines. On the CoreText backend (macOS/iOS/Mac Catalyst) palette selection used to be silently
// ignored: CoreText always renders CPAL palette 0. The fix bakes the requested palette into
// palette 0 of an in-memory copy of the font and rebuilds the CTFont from it. See the CPAL-baking
// code in externals/skia/src/ports/SkTypeface_mac_ct.cpp (skdata_with_baked_palette).
//
// The test font is a COLR**v0** font (layered solid colors). CoreText renders COLRv0 in color, so
// the baked palette is honored and these tests run (and pass) on every backend including CoreText.
public class SKColorFontPaletteRenderingTest : SKTest
{
	// COLRv0 test font (CC0). Derived from Skia's resources/fonts/colr.ttf (a font Skia exercises
	// across every backend, including DirectWrite) by extending its CPAL to three palettes. Two of
	// its color glyphs are used here:
	//   U+2662 (BLACK DIAMOND SUIT) - a large diamond painted with CPAL entry 0.
	//   U+1F600 (GRINNING FACE)     - a face whose dominant layer is painted with CPAL entry 2.
	// The three palettes give those two entries distinct, easy-to-detect colors:
	//   palette 0: entry0 = red,   entry2 = yellow
	//   palette 1: entry0 = blue,  entry2 = cyan
	//   palette 2: entry0 = lime,  entry2 = magenta
	private static string ColrV0FontPath =>
		Path.Combine (PathToFonts, "colr-v0-palettes.ttf");

	private const int DiamondCodepoint = 0x2662;
	private const int FaceCodepoint = 0x1F600;

	private static readonly string ColorText =
		char.ConvertFromUtf32 (DiamondCodepoint) + char.ConvertFromUtf32 (FaceCodepoint);

	// Expected dominant colors per palette: entry0 (rendered by the diamond) and entry2 (rendered
	// by the grinning face). These are the CPAL colors baked into the font above.
	public static IEnumerable<object[]> Palettes ()
	{
		yield return new object[] { 0, SKColors.Red, SKColors.Yellow };
		yield return new object[] { 1, SKColors.Blue, SKColors.Cyan };
		yield return new object[] { 2, SKColors.Lime, SKColors.Magenta };
	}

	private static SKBitmap Render (SKTypeface typeface, float fontSize = 64, int width = 220, int height = 96)
	{
		var bmp = new SKBitmap (new SKImageInfo (width, height));
		using var canvas = new SKCanvas (bmp);
		using var font = new SKFont (typeface, fontSize);
		using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };

		canvas.Clear (SKColors.White);
		canvas.DrawText (ColorText, 10, height - 22, SKTextAlign.Left, font, paint);
		canvas.Flush ();
		return bmp;
	}

	private static bool PixelsDiffer (SKBitmap a, SKBitmap b)
	{
		var pa = a.Bytes;
		var pb = b.Bytes;
		if (pa.Length != pb.Length)
			return true;
		for (int i = 0; i < pa.Length; i++)
			if (pa[i] != pb[i])
				return true;
		return false;
	}

	// Counts opaque pixels whose color is within tolerance of the target (order-independent: uses
	// SKColor channel accessors, not the raw memory layout).
	private static int CountColor (SKBitmap bmp, SKColor target, int tolerance = 40)
	{
		int count = 0;
		for (int y = 0; y < bmp.Height; y++)
		{
			for (int x = 0; x < bmp.Width; x++)
			{
				var c = bmp.GetPixel (x, y);
				if (c.Alpha < 200)
					continue;
				if (Math.Abs (c.Red - target.Red) <= tolerance &&
					Math.Abs (c.Green - target.Green) <= tolerance &&
					Math.Abs (c.Blue - target.Blue) <= tolerance)
					count++;
			}
		}
		return count;
	}

	[Fact]
	public void DifferentPaletteIndexProducesDifferentRendering ()
	{
		using var typeface = SKTypeface.FromFile (ColrV0FontPath);
		Assert.NotNull (typeface);

		using var palette0 = typeface.Clone (0);
		using var palette1 = typeface.Clone (1);
		Assert.NotNull (palette0);
		Assert.NotNull (palette1);

		using var pixels0 = Render (palette0);
		using var pixels1 = Render (palette1);

		Assert.True (PixelsDiffer (pixels0, pixels1),
			"Cloning with a different CPAL palette index must change the rendered colors.");
	}

	[Fact]
	public void AllPalettesProduceDistinctRenderings ()
	{
		using var typeface = SKTypeface.FromFile (ColrV0FontPath);
		Assert.NotNull (typeface);

		using var palette0 = typeface.Clone (0);
		using var palette1 = typeface.Clone (1);
		using var palette2 = typeface.Clone (2);

		using var pixels0 = Render (palette0);
		using var pixels1 = Render (palette1);
		using var pixels2 = Render (palette2);

		Assert.True (PixelsDiffer (pixels0, pixels1), "Palette 0 and 1 should render differently.");
		Assert.True (PixelsDiffer (pixels1, pixels2), "Palette 1 and 2 should render differently.");
		Assert.True (PixelsDiffer (pixels0, pixels2), "Palette 0 and 2 should render differently.");
	}

	[Theory]
	[MemberData (nameof (Palettes))]
	public void PaletteSelectionUsesExpectedColors (int index, SKColor entry0, SKColor entry2)
	{
		using var typeface = SKTypeface.FromFile (ColrV0FontPath);
		Assert.NotNull (typeface);

		using var tf = typeface.Clone (index);
		using var bmp = Render (tf);

		Assert.True (CountColor (bmp, entry0) >= 50,
			$"Palette {index} should paint the diamond (CPAL entry 0) with {entry0}.");
		Assert.True (CountColor (bmp, entry2) >= 50,
			$"Palette {index} should paint the face (CPAL entry 2) with {entry2}.");
	}

	[Fact]
	public void SamePaletteIndexRendersIdentically ()
	{
		using var typeface = SKTypeface.FromFile (ColrV0FontPath);
		Assert.NotNull (typeface);

		using var cloneA = typeface.Clone (1);
		using var cloneB = typeface.Clone (1);

		using var pixelsA = Render (cloneA);
		using var pixelsB = Render (cloneB);

		Assert.False (PixelsDiffer (pixelsA, pixelsB),
			"Two clones with the same palette index should render identically.");
	}

	[Fact]
	public void OutOfRangePaletteIndexFallsBackToBasePalette ()
	{
		using var typeface = SKTypeface.FromFile (ColrV0FontPath);
		Assert.NotNull (typeface);

		using var palette0 = typeface.Clone (0);
		// The font has 3 palettes; an out-of-range index falls back to palette 0
		// (CSS Fonts 4 / FreeType semantics).
		using var paletteOutOfRange = typeface.Clone (999);

		using var pixels0 = Render (palette0);
		using var pixelsOob = Render (paletteOutOfRange);

		Assert.False (PixelsDiffer (pixels0, pixelsOob),
			"An out-of-range palette index should fall back to the base palette (index 0).");
	}

	[Fact]
	public void NoOpPaletteOverrideMatchesBasePalette ()
	{
		using var typeface = SKTypeface.FromFile (ColrV0FontPath);
		Assert.NotNull (typeface);

		using var palette0 = typeface.Clone (0);
		using var pixels0 = Render (palette0);

		// Overriding entry 0 with the value it already has in palette 0 (red) is a no-op.
		var overrides = new[]
		{
			new SKFontPaletteOverride { Index = 0, Color = (uint)SKColors.Red },
		};
		var args = new SKFontArguments { PaletteIndex = 0, PaletteOverrides = overrides };
		using var overridden = typeface.Clone (args);
		using var pixelsOverridden = Render (overridden);

		Assert.False (PixelsDiffer (pixels0, pixelsOverridden),
			"A palette override equal to the existing color must not change the rendering.");
	}

	[Fact]
	public void PaletteOverrideChangesEntryColor ()
	{
		using var typeface = SKTypeface.FromFile (ColrV0FontPath);
		Assert.NotNull (typeface);

		using var palette0 = typeface.Clone (0);
		using var basePixels = Render (palette0);

		// In palette 0 the diamond (CPAL entry 0) is red. Override entry 0 to cyan.
		var overrides = new[]
		{
			new SKFontPaletteOverride { Index = 0, Color = (uint)SKColors.Cyan },
		};
		var args = new SKFontArguments { PaletteIndex = 0, PaletteOverrides = overrides };
		using var overridden = typeface.Clone (args);
		Assert.NotNull (overridden);
		using var overriddenPixels = Render (overridden);

		Assert.True (PixelsDiffer (basePixels, overriddenPixels),
			"A per-entry palette override must change the rendered colors.");
		Assert.True (CountColor (overriddenPixels, SKColors.Cyan) >= 50,
			"The overridden diamond (CPAL entry 0) should now be cyan.");
		Assert.True (CountColor (overriddenPixels, SKColors.Red) < 50,
			"The original red of the diamond should be gone after the override.");
	}
}

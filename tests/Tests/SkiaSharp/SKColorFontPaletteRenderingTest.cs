using System.Collections.Generic;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests;

// Verifies COLR/CPAL palette selection (SKTypeface.Clone(paletteIndex) and per-entry
// SKFontArguments.PaletteOverrides) actually changes what is rendered.
//
// This works on FreeType (Linux) and DirectWrite (Windows) via Skia's own COLR engine. On the
// CoreText backend (macOS/iOS) palette selection used to be silently ignored: CoreText always
// renders CPAL palette 0. The fix bakes the requested palette into palette 0 of an in-memory
// copy of the font and rebuilds the CTFont from it. See the CPAL-baking code in
// externals/skia/src/ports/SkTypeface_mac_ct.cpp (skdata_with_baked_palette).
//
// IMPORTANT: these palette tests use a COLR**v0** font. CoreText renders COLRv0 (layered solid
// colors) in color, so the baked palette is honored. CoreText does NOT paint COLRv1 (gradients);
// it falls back to a monochrome outline, so palette selection cannot be observed there. The
// COLRv1 case is covered by a separate test that is skipped on CoreText.
public class SKColorFontPaletteRenderingTest : SKTest
{
	// Self-contained COLRv0 test font (generated, CC0). Glyph 'A' (U+0041) is a COLRv0 glyph with
	// two layers: an outer rectangle painted with CPAL entry 0 and an inner rectangle painted with
	// CPAL entry 1. It has 3 palettes:
	//   palette 0: entry0 = red,     entry1 = green
	//   palette 1: entry0 = blue,    entry1 = yellow
	//   palette 2: entry0 = magenta, entry1 = cyan
	private static string ColrV0FontPath =>
		Path.Combine (PathToFonts, "colr-v0-palettes.ttf");

	// Official Skia COLRv1 test font (gradients). 3 distinct palettes. Used only to document that
	// COLRv1 palette selection works off-CoreText.
	private static string ColrV1FontPath =>
		Path.Combine (PathToFonts, "test_glyphs-COLRv1.ttf");

	private const string ColorText = "A";

	private static SKBitmap Render (SKTypeface typeface, float fontSize = 64, int width = 96, int height = 96)
	{
		var bmp = new SKBitmap (new SKImageInfo (width, height));
		using var canvas = new SKCanvas (bmp);
		using var font = new SKFont (typeface, fontSize);
		using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };

		canvas.Clear (SKColors.White);
		canvas.DrawText (ColorText, 16, height - 20, SKTextAlign.Left, font, paint);
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
	private static int CountColor (SKBitmap bmp, SKColor target, int tolerance = 32)
	{
		int count = 0;
		for (int y = 0; y < bmp.Height; y++)
		{
			for (int x = 0; x < bmp.Width; x++)
			{
				var c = bmp.GetPixel (x, y);
				if (c.Alpha < 200)
					continue;
				if (System.Math.Abs (c.Red - target.Red) <= tolerance &&
					System.Math.Abs (c.Green - target.Green) <= tolerance &&
					System.Math.Abs (c.Blue - target.Blue) <= tolerance)
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

	[Fact]
	public void PaletteSelectionUsesExpectedColors ()
	{
		using var typeface = SKTypeface.FromFile (ColrV0FontPath);
		Assert.NotNull (typeface);

		// Each palette paints the outer layer with entry 0 and the inner layer with entry 1.
		var expected = new[]
		{
			(index: 0, a: SKColors.Red,     b: SKColors.Lime),
			(index: 1, a: SKColors.Blue,    b: SKColors.Yellow),
			(index: 2, a: SKColors.Magenta, b: SKColors.Cyan),
		};

		foreach (var e in expected)
		{
			using var tf = typeface.Clone (e.index);
			using var bmp = Render (tf);

			Assert.True (CountColor (bmp, e.a) >= 20,
				$"Palette {e.index} should paint its outer layer with {e.a}.");
			Assert.True (CountColor (bmp, e.b) >= 20,
				$"Palette {e.index} should paint its inner layer with {e.b}.");
		}
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

		// Override the outer layer (entry 0) of palette 0 from red to blue.
		var overrides = new[]
		{
			new SKFontPaletteOverride { Index = 0, Color = (uint)SKColors.Blue },
		};
		var args = new SKFontArguments { PaletteIndex = 0, PaletteOverrides = overrides };
		using var overridden = typeface.Clone (args);
		Assert.NotNull (overridden);
		using var overriddenPixels = Render (overridden);

		Assert.True (PixelsDiffer (basePixels, overriddenPixels),
			"A per-entry palette override must change the rendered colors.");
		Assert.True (CountColor (overriddenPixels, SKColors.Blue) >= 20,
			"The overridden outer layer should now be blue.");
		Assert.True (CountColor (overriddenPixels, SKColors.Red) < 20,
			"The original red of the outer layer should be gone after the override.");
	}

	[Fact]
	public void ColrV1PaletteSelectionIsUnsupportedOnCoreText ()
	{
		// CoreText does not paint COLRv1 (gradients); it renders a monochrome outline, so palette
		// selection cannot take effect. FreeType/DirectWrite render COLRv1 with the selected
		// palette. This test documents and guards that platform difference.
		Assert.SkipWhen (IsMac || IsMacCatalyst || IsIOS,
			"CoreText does not render COLRv1; palette selection cannot be observed.");

		using var typeface = SKTypeface.FromFile (ColrV1FontPath);
		Assert.NotNull (typeface);

		var sb = new System.Text.StringBuilder ();
		foreach (var cp in new[] { 0xF0100, 0xF0101, 0xF0102 })
			sb.Append (char.ConvertFromUtf32 (cp));
		var text = sb.ToString ();

		SKBitmap RenderText (SKTypeface tf)
		{
			var bmp = new SKBitmap (new SKImageInfo (320, 80));
			using var canvas = new SKCanvas (bmp);
			using var font = new SKFont (tf, 40);
			using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
			canvas.Clear (SKColors.White);
			canvas.DrawText (text, 10, 55, SKTextAlign.Left, font, paint);
			canvas.Flush ();
			return bmp;
		}

		using var palette0 = typeface.Clone (0);
		using var palette1 = typeface.Clone (1);
		using var pixels0 = RenderText (palette0);
		using var pixels1 = RenderText (palette1);

		Assert.True (PixelsDiffer (pixels0, pixels1),
			"COLRv1 palette selection must change the rendered colors on FreeType/DirectWrite.");
	}
}

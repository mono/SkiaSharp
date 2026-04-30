using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests;

public class SKVariableFontRenderingTest : SKTest
{
	private static string DistortableFontPath =>
		Path.Combine (PathToFonts, "Distortable.ttf");

	private static byte[] RenderTextToBitmapBytes (SKTypeface typeface, string text, float fontSize, int width = 256, int height = 64)
	{
		using var bmp = new SKBitmap (new SKImageInfo (width, height));
		using var canvas = new SKCanvas (bmp);
		using var font = new SKFont (typeface, fontSize);
		using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };

		canvas.Clear (SKColors.White);
		canvas.DrawText (text, 10, height / 2 + fontSize / 3, font, paint);
		canvas.Flush ();

		return bmp.Bytes;
	}

	private static int CountDifferingPixelBytes (byte[] a, byte[] b)
	{
		if (a.Length != b.Length)
			return Math.Max (a.Length, b.Length);

		int count = 0;
		for (int i = 0; i < a.Length; i++)
		{
			if (a[i] != b[i])
				count++;
		}

		return count;
	}

	private static bool PixelsDiffer (byte[] a, byte[] b) =>
		CountDifferingPixelBytes (a, b) > 0;

	[SkippableFact]
	public void DifferentWeightVariationProducesDifferentRendering ()
	{
		using var baseTypeface = SKTypeface.FromFile (DistortableFontPath);
		Assert.NotNull (baseTypeface);

		var axes = baseTypeface.VariationDesignParameters;
		Assert.NotEmpty (axes);

		// Clone with min weight
		var minPosition = new[] {
			new SKFontVariationPositionCoordinate { Axis = axes[0].Tag, Value = axes[0].Min }
		};
		using var minTypeface = baseTypeface.Clone (minPosition);
		Assert.NotNull (minTypeface);

		// Clone with max weight
		var maxPosition = new[] {
			new SKFontVariationPositionCoordinate { Axis = axes[0].Tag, Value = axes[0].Max }
		};
		using var maxTypeface = baseTypeface.Clone (maxPosition);
		Assert.NotNull (maxTypeface);

		var text = "Hello Variable";

		var minPixels = RenderTextToBitmapBytes (minTypeface, text, 32);
		var maxPixels = RenderTextToBitmapBytes (maxTypeface, text, 32);

		Assert.True (PixelsDiffer (minPixels, maxPixels),
			"Rendering with min and max variation values should produce different pixel output.");
	}

	[SkippableFact]
	public void VariationPositionIsPreservedAfterClone ()
	{
		using var baseTypeface = SKTypeface.FromFile (DistortableFontPath);
		Assert.NotNull (baseTypeface);

		var axes = baseTypeface.VariationDesignParameters;
		Assert.NotEmpty (axes);

		var targetValue = axes[0].Min;
		var position = new[] {
			new SKFontVariationPositionCoordinate { Axis = axes[0].Tag, Value = targetValue }
		};
		using var cloned = baseTypeface.Clone (position);
		Assert.NotNull (cloned);

		// Verify the cloned typeface reports the expected variation position
		var clonedPosition = cloned.VariationDesignPosition;
		Assert.NotEmpty (clonedPosition);
		Assert.Equal (targetValue, clonedPosition[0].Value);

		// And render with it to confirm it produces distinct output from the max
		var maxPosition = new[] {
			new SKFontVariationPositionCoordinate { Axis = axes[0].Tag, Value = axes[0].Max }
		};
		using var maxTypeface = baseTypeface.Clone (maxPosition);

		var minPixels = RenderTextToBitmapBytes (cloned, "Hello Variable", 32);
		var maxPixels = RenderTextToBitmapBytes (maxTypeface, "Hello Variable", 32);

		Assert.True (PixelsDiffer (minPixels, maxPixels),
			"Cloned typeface at min should render differently from max.");
	}

	[SkippableFact]
	public void IncreasingVariationChangesInkCoverage ()
	{
		using var baseTypeface = SKTypeface.FromFile (DistortableFontPath);
		Assert.NotNull (baseTypeface);

		var axes = baseTypeface.VariationDesignParameters;
		Assert.NotEmpty (axes);

		var axis = axes[0];

		// Sum all pixel intensity deltas from white to measure total ink coverage
		long SumInkIntensity (byte[] pixels)
		{
			// Each pixel is 4 bytes (BGRA). Sum (255 - channel) for color channels.
			long total = 0;
			for (int i = 0; i < pixels.Length; i += 4)
			{
				total += (255 - pixels[i]) + (255 - pixels[i + 1]) + (255 - pixels[i + 2]);
			}
			return total;
		}

		var minPos = new[] {
			new SKFontVariationPositionCoordinate { Axis = axis.Tag, Value = axis.Min }
		};
		using var minTypeface = baseTypeface.Clone (minPos);
		var minPixels = RenderTextToBitmapBytes (minTypeface, "Hello Variable", 32);

		var maxPos = new[] {
			new SKFontVariationPositionCoordinate { Axis = axis.Tag, Value = axis.Max }
		};
		using var maxTypeface = baseTypeface.Clone (maxPos);
		var maxPixels = RenderTextToBitmapBytes (maxTypeface, "Hello Variable", 32);

		var minInk = SumInkIntensity (minPixels);
		var maxInk = SumInkIntensity (maxPixels);

		// Different variation values should produce measurably different ink coverage
		Assert.NotEqual (minInk, maxInk);
	}

	[SkippableFact]
	public void ClonedVariationPreservesPositionInRendering ()
	{
		using var baseTypeface = SKTypeface.FromFile (DistortableFontPath);
		Assert.NotNull (baseTypeface);

		var axes = baseTypeface.VariationDesignParameters;
		Assert.NotEmpty (axes);

		var midValue = (axes[0].Min + axes[0].Max) / 2;
		var position = new[] {
			new SKFontVariationPositionCoordinate { Axis = axes[0].Tag, Value = midValue }
		};

		// Clone twice with the same value
		using var clone1 = baseTypeface.Clone (position);
		using var clone2 = baseTypeface.Clone (position);
		Assert.NotNull (clone1);
		Assert.NotNull (clone2);

		var text = "Consistent";
		var pixels1 = RenderTextToBitmapBytes (clone1, text, 32);
		var pixels2 = RenderTextToBitmapBytes (clone2, text, 32);

		Assert.False (PixelsDiffer (pixels1, pixels2),
			"Two clones with the same variation value should render identically.");
	}

	[SkippableFact]
	public void MultipleIntermediateValuesProduceDistinctRenderings ()
	{
		using var baseTypeface = SKTypeface.FromFile (DistortableFontPath);
		Assert.NotNull (baseTypeface);

		var axes = baseTypeface.VariationDesignParameters;
		Assert.NotEmpty (axes);

		var axis = axes[0];
		var step = (axis.Max - axis.Min) / 4;

		// Pick 3 distinct points along the axis
		var values = new[] { axis.Min, axis.Min + step * 2, axis.Max };
		var renderings = new byte[values.Length][];

		for (int i = 0; i < values.Length; i++)
		{
			var pos = new[] {
				new SKFontVariationPositionCoordinate { Axis = axis.Tag, Value = values[i] }
			};
			using var typeface = baseTypeface.Clone (pos);
			Assert.NotNull (typeface);
			renderings[i] = RenderTextToBitmapBytes (typeface, "Variation", 36);
		}

		// Each rendering should differ from the others
		Assert.True (PixelsDiffer (renderings[0], renderings[1]),
			"Min and mid variation should render differently.");
		Assert.True (PixelsDiffer (renderings[1], renderings[2]),
			"Mid and max variation should render differently.");
		Assert.True (PixelsDiffer (renderings[0], renderings[2]),
			"Min and max variation should render differently.");
	}

	[SkippableFact]
	public void StaticFontRenderingUnaffectedByVariationAttempt ()
	{
		using var staticTypeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "content-font.ttf"));
		Assert.NotNull (staticTypeface);

		// Verify this is a static font with no axes
		var axes = staticTypeface.VariationDesignParameters;
		Assert.Empty (axes);

		// Render with the static font
		var text = "Static";
		var pixels = RenderTextToBitmapBytes (staticTypeface, text, 32);

		// Attempting to clone a static font with variation should return a usable typeface
		var position = new[] {
			new SKFontVariationPositionCoordinate { Axis = SKFourByteTag.Parse ("wght"), Value = 700 }
		};
		using var cloned = staticTypeface.Clone (position);
		Assert.NotNull (cloned);

		var clonedPixels = RenderTextToBitmapBytes (cloned, text, 32);
		// For a static font, the output should be the same regardless of variation
		Assert.False (PixelsDiffer (pixels, clonedPixels),
			"Static font rendering should not change with variation parameters.");
	}

	[SkippableFact]
	public void VariationDoesNotAffectGlyphCount ()
	{
		using var baseTypeface = SKTypeface.FromFile (DistortableFontPath);
		Assert.NotNull (baseTypeface);

		var axes = baseTypeface.VariationDesignParameters;
		Assert.NotEmpty (axes);

		var minPos = new[] {
			new SKFontVariationPositionCoordinate { Axis = axes[0].Tag, Value = axes[0].Min }
		};
		using var minTypeface = baseTypeface.Clone (minPos);

		var maxPos = new[] {
			new SKFontVariationPositionCoordinate { Axis = axes[0].Tag, Value = axes[0].Max }
		};
		using var maxTypeface = baseTypeface.Clone (maxPos);

		using var minFont = new SKFont (minTypeface, 32);
		using var maxFont = new SKFont (maxTypeface, 32);

		var text = "Hello";

		// Glyph count should be the same regardless of variation
		Assert.Equal (minFont.CountGlyphs (text), maxFont.CountGlyphs (text));
	}
}

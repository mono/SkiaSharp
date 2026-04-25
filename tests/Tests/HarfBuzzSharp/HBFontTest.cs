using System;
using System.Buffers;
using System.IO;
using System.Text;
using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HbFontTest : HBTest
	{
		// Variable font helpers
		private (Face face, Font font) CreateVariableFontPair ()
		{
			using var blob = Blob.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			var face = new Face (blob, 0);
			var font = new Font (face);
			return (face, font);
		}

		// US2: Set Font Variation Values

		[SkippableFact]
		public void CanSetVariations ()
		{
			var (face, font) = CreateVariableFontPair ();
			using (face)
			using (font) {
				var axes = face.VariationAxisInfos;
				Assert.NotEmpty (axes);

				var variations = new Variation[] {
					new Variation { Tag = axes[0].Tag, Value = axes[0].DefaultValue }
				};
				font.SetVariations (variations);
			}
		}

		[SkippableFact]
		public void CanSetMultipleVariationsSimultaneously ()
		{
			var (face, font) = CreateVariableFontPair ();
			using (face)
			using (font) {
				var axes = face.VariationAxisInfos;
				if (axes.Length < 1) return;

				// Set all axes to their min values
				var variations = new Variation[axes.Length];
				for (int i = 0; i < axes.Length; i++) {
					variations[i] = new Variation { Tag = axes[i].Tag, Value = axes[i].MinValue };
				}
				font.SetVariations (variations);
			}
		}

		[SkippableFact]
		public void CanSetVarCoordsDesign ()
		{
			var (face, font) = CreateVariableFontPair ();
			using (face)
			using (font) {
				var axisCount = face.VariationAxisCount;
				Assert.True (axisCount > 0);

				var coords = new float[axisCount];
				var axes = face.VariationAxisInfos;
				for (int i = 0; i < axisCount; i++)
					coords[i] = axes[i].DefaultValue;

				font.SetVariationCoordsDesign (coords);
			}
		}

		[SkippableFact]
		public void SetVariationsOnStaticFontDoesNotThrow ()
		{
			using var face = new Face (Blob, 0);
			using var font = new Font (face);
			var variations = new Variation[] {
				new Variation { Tag = Tag.Parse ("wght"), Value = 400 }
			};
			font.SetVariations (variations); // Should not throw
		}

		// US3: Named Instances

		[SkippableFact]
		public void CanSetVarNamedInstance ()
		{
			var (face, font) = CreateVariableFontPair ();
			using (face)
			using (font) {
				var count = face.NamedInstanceCount;
				if (count == 0)
					return;
				font.SetVariationNamedInstance (0); // Should not throw
			}
		}

		// US4: Normalized Coordinates

		[SkippableFact]
		public void CanSetAndGetNormalizedCoords ()
		{
			var (face, font) = CreateVariableFontPair ();
			using (face)
			using (font) {
				var axisCount = face.VariationAxisCount;
				Assert.True (axisCount > 0);

				// Set normalized coords (HarfBuzz uses 16.16 fixed point: 16384 = 1.0)
				var coords = new int[axisCount];
				coords[0] = 8192; // 0.5 in normalized space
				font.SetVariationCoordsNormalized (coords);

				var result = font.VariationCoordsNormalized;
				Assert.Equal (axisCount, result.Length);
				Assert.Equal (8192, result[0]);
			}
		}

		[SkippableFact]
		public void SpanGetVariationCoordsNormalizedMatchesProperty ()
		{
			var (face, font) = CreateVariableFontPair ();
			using (face)
			using (font) {
				var axisCount = face.VariationAxisCount;
				Assert.True (axisCount > 0);

				var coords = new int[axisCount];
				coords[0] = 8192;
				font.SetVariationCoordsNormalized (coords);

				var propertyResult = font.VariationCoordsNormalized;
				var spanBuffer = new int[axisCount];
				var written = font.GetVariationCoordsNormalized (spanBuffer);

				Assert.Equal (propertyResult.Length, written);
				for (int i = 0; i < propertyResult.Length; i++)
					Assert.Equal (propertyResult[i], spanBuffer[i]);
			}
		}

		[SkippableFact]
		public void SpanGetVariationCoordsNormalizedReturnsTotalLengthWhenBufferSmall ()
		{
			var (face, font) = CreateVariableFontPair ();
			using (face)
			using (font) {
				var axisCount = face.VariationAxisCount;
				Assert.True (axisCount > 0);

				var coords = new int[axisCount];
				coords[0] = 4096;
				font.SetVariationCoordsNormalized (coords);

				// Pass an empty buffer — should return total length without crashing
				var emptyBuffer = new int[0];
				var totalLength = font.GetVariationCoordsNormalized (emptyBuffer);
				Assert.Equal (axisCount, totalLength);
			}
		}

		[SkippableFact]
		public void SetVariationCoordsDesignAffectsNormalizedCoords ()
		{
			var (face, font) = CreateVariableFontPair ();
			using (face)
			using (font) {
				var axes = face.VariationAxisInfos;
				Assert.NotEmpty (axes);

				// Set design coords to min value
				var designCoords = new float[axes.Length];
				designCoords[0] = axes[0].MinValue;
				font.SetVariationCoordsDesign (designCoords);

				// Normalized coords should now be non-zero (unless min == default)
				var normalized = font.VariationCoordsNormalized;
				Assert.Equal (axes.Length, normalized.Length);

				// Now set to max and verify it's different
				designCoords[0] = axes[0].MaxValue;
				font.SetVariationCoordsDesign (designCoords);
				var normalized2 = font.VariationCoordsNormalized;

				if (axes[0].MinValue != axes[0].MaxValue)
					Assert.NotEqual (normalized[0], normalized2[0]);
			}
		}

		[SkippableFact]
		public void NegativeInstanceIndexThrowsForSetVariationNamedInstance ()
		{
			var (face, font) = CreateVariableFontPair ();
			using (face)
			using (font) {
				Assert.Throws<ArgumentOutOfRangeException> (() => font.SetVariationNamedInstance (-1));
			}
		}

		[SkippableFact]
		public void NormalizedCoordsAreEmptyForStaticFont ()
		{
			using var face = new Face (Blob, 0);
			using var font = new Font (face);
			var coords = font.VariationCoordsNormalized;
			Assert.Empty (coords);
		}

		[SkippableFact]
		public void SpanNormalizedCoordsReturnsZeroForStaticFont ()
		{
			using var face = new Face (Blob, 0);
			using var font = new Font (face);
			var buffer = new int[4];
			var length = font.GetVariationCoordsNormalized (buffer);
			Assert.Equal (0, length);
		}

		[SkippableFact]
		public void ShouldHaveDefaultSupportedShapers()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(new[] { "ot" }, font.SupportedShapers);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphByUnicode()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.True(font.TryGetGlyph('A', out var glyph));
				Assert.Equal(42u, glyph);
			}
		}

		[SkippableFact]
		public void ShouldHaveDefaultScale()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.GetScale(out var xScale, out var yScale);
				Assert.Equal((2048, 2048), (xScale, yScale));
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphOrigin()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.TryGetHorizontalGlyphOrigin(49, out var xOrigin, out var yOrigin);
				Assert.Equal((0, 0), (xOrigin, yOrigin));
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphOrigin()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.True(font.TryGetVerticalGlyphOrigin(49, out var xOrigin, out var yOrigin));
				Assert.Equal((557, 1991), (xOrigin, yOrigin));
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphAdvance()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(1114, font.GetHorizontalGlyphAdvance(49));
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphAdvance()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(-2961, font.GetVerticalGlyphAdvance(49));
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphAdvances()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var advances = font.GetHorizontalGlyphAdvances(new[] { 49u, 50u, 51u });
				var expected = new [] { 1114, 514, 602 };

				Assert.Equal(expected, advances);
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphAdvances()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var advances = font.GetVerticalGlyphAdvances(new[] { 49u, 50u, 51u });
				var expected = new [] { -2961, -2961, -2961 };

				Assert.Equal(expected, advances);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphName()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.TryGetGlyphName(49, out var name);

				Assert.Equal("H", name);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphFromName()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.TryGetGlyphFromName("H", out var glyph);

				Assert.Equal(49u, glyph);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphFromString()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.True(font.TryGetGlyphFromString("H", out var glyph));
				Assert.Equal(49u, glyph);
			}
		}

		[SkippableFact]
		public void ShouldConvertGlyphToString()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var s = font.GlyphToString(49);

				Assert.Equal("H", s);
			}
		}

		[SkippableFact]
		public void GlyphToStringIsCorrectWithDelegate()
		{
			// get an array and fill it with things
			var pool = ArrayPool<byte>.Shared;
			var buffer = pool.Rent(Font.NameBufferLength);
			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = (byte)i;
			pool.Return(buffer);

			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var s = font.GlyphToString(49);

				Assert.Equal("H", s);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphContourPointForOrigin()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.False(font.TryGetGlyphContourPointForOrigin(49, 0, Direction.LeftToRight, out _, out _));
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphContourPoint()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.False(font.TryGetGlyphContourPoint(49, 0, out _, out _));
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphAdvanceForDirection()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.GetGlyphAdvanceForDirection(49, Direction.LeftToRight, out var x, out _);

				Assert.Equal(1114, x);
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphKerning()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var kerning = font.GetHorizontalGlyphKerning(49, 50);

				Assert.Equal(0, kerning);
			}
		}

		[Theory]
		[InlineData(OpenTypeMetricsTag.UnderlineOffset, -60)]
		[InlineData(OpenTypeMetricsTag.UnderlineSize, 9)]
		[InlineData(OpenTypeMetricsTag.StrikeoutOffset, 1049)]
		[InlineData(OpenTypeMetricsTag.StrikeoutSize, 209)]
		public void ShouldGetOpenTypeMetrics(OpenTypeMetricsTag tag, int expected)
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var result = font.OpenTypeMetrics.TryGetPosition(tag, out var position);

				Assert.True(result);

				Assert.Equal(expected, position);
			}
		}
	}
}

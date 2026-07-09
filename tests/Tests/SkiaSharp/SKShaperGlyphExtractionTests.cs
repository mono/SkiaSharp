using System.IO;
using HarfBuzzSharp;
using SkiaSharp.Tests;
using Xunit;
using Buffer = HarfBuzzSharp.Buffer;

namespace SkiaSharp.HarfBuzz.Tests
{
	// Guards the SKShaper.Shape glyph-extraction optimization: reading the shaping results through the
	// zero-copy buffer.GetGlyphInfoSpan()/GetGlyphPositionSpan() instead of the allocating
	// buffer.GlyphInfos/GlyphPositions array getters must produce a byte-for-byte identical
	// SKShaper.Result. These tests pin that equivalence so the two accessors can never silently drift.
	public class SKShaperGlyphExtractionTests : SKTest
	{
		// The zero-copy span accessors must expose exactly the same glyph data as the array getters —
		// this is the invariant the SKShaper.Shape substitution relies on.
		[Fact]
		public void GlyphSpanAccessorsMatchArrayAccessors()
		{
			using var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf"));
			using var shaper = new SKShaper(tf);
			using var font = new SKFont { Size = 64, Typeface = tf };
			using var buffer = new Buffer();

			buffer.AddUtf8("متن متن متن");
			buffer.GuessSegmentProperties();

			// Shape in place using the shaper's own font (this is what SKShaper.Shape does internally).
			shaper.Shape(buffer, font);

			var infoArray = buffer.GlyphInfos;
			var infoSpan = buffer.GetGlyphInfoSpan();
			var posArray = buffer.GlyphPositions;
			var posSpan = buffer.GetGlyphPositionSpan();

			Assert.True(infoArray.Length > 0);
			Assert.Equal(infoArray.Length, infoSpan.Length);
			Assert.Equal(posArray.Length, posSpan.Length);

			for (var i = 0; i < infoArray.Length; i++)
			{
				Assert.Equal(infoArray[i], infoSpan[i]);
				Assert.Equal(posArray[i], posSpan[i]);
			}
		}

		// The shipped SKShaper.Shape(Buffer, ...) result must equal, bit-for-bit, an independent oracle
		// computed from the array getters (the pre-optimization data source). Passes trivially before the
		// change (array vs array), must still pass after it (span vs array — proving the data is identical),
		// and turns red if the extraction ever diverges.
		[Theory]
		[InlineData("content-font.ttf", "متن", 0f, 0f)]
		[InlineData("content-font.ttf", "متن", 100f, 200f)]
		[InlineData("content-font.ttf", "متن متن متن", 12.5f, -7.25f)]
		[InlineData("content-font.ttf", " متن ", 3f, 3f)]
		[InlineData("segoeui.ttf", "SkiaSharp", 0f, 0f)]
		[InlineData("segoeui.ttf", "SkiaSharp shaping!", 300f, 100f)]
		public void ShapeResultMatchesArrayGetterOracle(string fontFile, string text, float xOffset, float yOffset)
		{
			using var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, fontFile));
			using var shaper = new SKShaper(tf);
			using var font = new SKFont { Size = 64, Typeface = tf };
			using var buffer = new Buffer();

			buffer.AddUtf8(text);
			buffer.GuessSegmentProperties();

			// Subject: the shipped extraction (reads the glyph spans after the optimization).
			var result = shaper.Shape(buffer, xOffset, yOffset, font);

			// Oracle: recompute from the SAME now-shaped buffer via the array getters, mirroring the exact
			// arithmetic in SKShaper.Shape so any behavioural divergence is caught.
			var info = buffer.GlyphInfos;
			var pos = buffer.GlyphPositions;
			var len = buffer.Length;

			const int fontSizeScale = 512; // SKShaper.FONT_SIZE_SCALE (internal)
			var textSizeY = font.Size / fontSizeScale;
			var textSizeX = textSizeY * font.ScaleX;

			var expectedCodepoints = new uint[len];
			var expectedClusters = new uint[len];
			var expectedPoints = new SKPoint[len];
			var x = xOffset;
			var y = yOffset;
			for (var i = 0; i < len; i++)
			{
				expectedCodepoints[i] = info[i].Codepoint;
				expectedClusters[i] = info[i].Cluster;
				expectedPoints[i] = new SKPoint(
					x + pos[i].XOffset * textSizeX,
					y - pos[i].YOffset * textSizeY);
				x += pos[i].XAdvance * textSizeX;
				y += pos[i].YAdvance * textSizeY;
			}
			var expectedWidth = x - xOffset;

			Assert.True(len > 0);
			Assert.Equal(expectedCodepoints, result.Codepoints);
			Assert.Equal(expectedClusters, result.Clusters);
			Assert.Equal(expectedPoints, result.Points);
			Assert.Equal(expectedWidth, result.Width);
		}

		// The empty-input boundary must remain an empty result (unchanged by the extraction change).
		[Fact]
		public void EmptyTextReturnsEmptyResult()
		{
			using var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf"));
			using var shaper = new SKShaper(tf);
			using var font = new SKFont { Size = 64, Typeface = tf };

			var result = shaper.Shape(string.Empty, font);

			Assert.Empty(result.Codepoints);
			Assert.Empty(result.Clusters);
			Assert.Empty(result.Points);
			Assert.Equal(0f, result.Width);
		}
	}
}

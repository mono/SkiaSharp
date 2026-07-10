using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKFourByteTagTest : SKTest
	{
		[Fact]
		public void FourByteTagParseProducesCorrectValue ()
		{
			var tag = SKFourByteTag.Parse ("wght");
			Assert.Equal (0x77676874u, (uint)tag);
		}

		[Fact]
		public void FourByteTagToStringRoundTrips ()
		{
			var tag = SKFourByteTag.Parse ("wght");
			Assert.Equal ("wght", tag.ToString ());
		}

		[Fact]
		public void FourByteTagCharConstructorMatchesParse ()
		{
			var fromParse = SKFourByteTag.Parse ("wdth");
			var fromChars = new SKFourByteTag ('w', 'd', 't', 'h');
			Assert.Equal (fromParse, fromChars);
		}

		[Fact]
		public void FourByteTagImplicitConversionRoundTrips ()
		{
			SKFourByteTag tag = SKFourByteTag.Parse ("slnt");
			uint rawValue = tag;
			SKFourByteTag back = rawValue;
			Assert.Equal (tag, back);
			Assert.Equal ("slnt", back.ToString ());
		}

		[Fact]
		public void FourByteTagEqualityWorks ()
		{
			var a = SKFourByteTag.Parse ("wght");
			var b = SKFourByteTag.Parse ("wght");
			var c = SKFourByteTag.Parse ("wdth");

			Assert.Equal (a, b);
			Assert.True (a == b);
			Assert.False (a != b);
			Assert.NotEqual (a, c);
			Assert.True (a != c);
			Assert.False (a == c);
		}

		[Fact]
		public void FourByteTagDefaultIsZero ()
		{
			var tag = default(SKFourByteTag);
			Assert.Equal (0u, (uint)tag);
		}

		[Fact]
		public void FourByteTagParseEmptyReturnsZero ()
		{
			var tag = SKFourByteTag.Parse ("");
			Assert.Equal (0u, (uint)tag);
		}

		[Fact]
		public void FourByteTagParseNullReturnsZero ()
		{
			var tag = SKFourByteTag.Parse (null);
			Assert.Equal (0u, (uint)tag);
		}

		[Fact]
		public void FourByteTagParseShortStringPadsWithSpaces ()
		{
			// "ab" should become "ab  " (padded with spaces)
			var tag = SKFourByteTag.Parse ("ab");
			Assert.Equal ("ab  ", tag.ToString ());
		}

		[Fact]
		public void FourByteTagParseLongStringTruncates ()
		{
			// Only first 4 characters used
			var tag = SKFourByteTag.Parse ("abcdef");
			Assert.Equal ("abcd", tag.ToString ());
		}

		[Fact]
		public void FourByteTagKnownTagValues ()
		{
			// Well-known OpenType tag values
			Assert.Equal (0x77676874u, (uint)SKFourByteTag.Parse ("wght"));
			Assert.Equal (0x77647468u, (uint)SKFourByteTag.Parse ("wdth"));
			Assert.Equal (0x736C6E74u, (uint)SKFourByteTag.Parse ("slnt"));
			Assert.Equal (0x6F70737Au, (uint)SKFourByteTag.Parse ("opsz"));
			Assert.Equal (0x6974616Cu, (uint)SKFourByteTag.Parse ("ital"));
		}

		[Fact]
		public void FourByteTagGetHashCodeConsistent ()
		{
			var a = SKFourByteTag.Parse ("wght");
			var b = SKFourByteTag.Parse ("wght");
			Assert.Equal (a.GetHashCode (), b.GetHashCode ());
		}

		[Fact]
		public void FourByteTagEqualsObjectWorks ()
		{
			var tag = SKFourByteTag.Parse ("wght");
			Assert.True (tag.Equals ((object)SKFourByteTag.Parse ("wght")));
			Assert.False (tag.Equals ((object)SKFourByteTag.Parse ("wdth")));
			Assert.False (tag.Equals ("wght")); // wrong type
		}

		// Expected values are precomputed constants rather than the output of a runtime reference
		// implementation, so the oracle can never silently drift along with the code under test.
		// Each expected value is the big-endian packing of the low byte of the first four UTF-16
		// code units, padding any missing trailing slots with spaces (0x20) and treating
		// null/empty as zero.
		[Theory]
		[InlineData (null, 0x00000000u)]
		[InlineData ("", 0x00000000u)]
		[InlineData (" ", 0x20202020u)]
		[InlineData ("a", 0x61202020u)]
		[InlineData ("ab", 0x61622020u)]
		[InlineData ("abc", 0x61626320u)]
		[InlineData ("abcd", 0x61626364u)]
		[InlineData ("abcde", 0x61626364u)]
		[InlineData ("abcdef", 0x61626364u)]
		[InlineData ("wght", 0x77676874u)]
		[InlineData ("wdth", 0x77647468u)]
		[InlineData ("slnt", 0x736C6E74u)]
		[InlineData ("opsz", 0x6F70737Au)]
		[InlineData ("ital", 0x6974616Cu)]
		[InlineData ("GSUB", 0x47535542u)]
		[InlineData ("GPOS", 0x47504F53u)]
		[InlineData ("OS/2", 0x4F532F32u)]
		[InlineData ("    ", 0x20202020u)]
		[InlineData ("\0\0\0\0", 0x00000000u)]
		[InlineData ("\t\r\n ", 0x090D0A20u)]
		// characters above 0xFF: only the low byte is kept
		[InlineData ("\u0100\u0141\u2764\uFFFF", 0x004164FFu)]
		[InlineData ("\u00e9\u00e9\u00ffz", 0xE9E9FF7Au)]
		[InlineData ("\uABCDwgh", 0xCD776768u)]
		// surrogate code units are still just char code units to the algorithm
		[InlineData ("\uD83D\uDE00xy", 0x3D007879u)]
		public void FourByteTagParseProducesExpectedValue (string input, uint expected)
		{
			// Both the string overload and the span overload must produce the same constant.
			Assert.Equal (expected, (uint)SKFourByteTag.Parse (input));
			Assert.Equal (expected, (uint)SKFourByteTag.Parse (input.AsSpan ()));
		}

		[Fact]
		public void FourByteTagParseSpanFromSlicedStringMatches ()
		{
			// A span carved out of a larger string (no substring allocation) must parse the same
			// as the equivalent standalone string.
			const string source = "xxwghtyy";
			var slice = source.AsSpan (2, 4);
			Assert.Equal ((uint)SKFourByteTag.Parse ("wght"), (uint)SKFourByteTag.Parse (slice));
		}

		[Fact]
		public void FourByteTagParseEmptySpanReturnsZero ()
		{
			Assert.Equal (0u, (uint)SKFourByteTag.Parse (ReadOnlySpan<char>.Empty));
			Assert.Equal (0u, (uint)SKFourByteTag.Parse (default (ReadOnlySpan<char>)));
		}

		[Fact]
		public void FourByteTagSurvivesNativeRoundTrip ()
		{
			// Create a typeface, clone with a specific tag, read it back
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			var wght = SKFourByteTag.Parse ("wght");
			var position = new[] {
				new SKFontVariationPositionCoordinate { Axis = wght, Value = 1.5f }
			};

			using var cloned = typeface.Clone (position);
			Assert.NotNull (cloned);

			// The tag should survive the round-trip through C API
			var readBack = cloned.VariationDesignPosition;
			Assert.Single (readBack);
			Assert.Equal (wght, readBack[0].Axis);
			Assert.Equal ("wght", readBack[0].Axis.ToString ());
			Assert.Equal (1.5f, readBack[0].Value);
		}

	}
}

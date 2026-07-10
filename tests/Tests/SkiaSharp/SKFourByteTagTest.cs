using System;
using System.Collections.Generic;
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

		// Reference implementation: the ORIGINAL char[4]-scratch algorithm, kept verbatim as the
		// equivalence oracle so the managed rewrite is proven bit-for-bit identical.
		private static uint ReferenceParse (string tag)
		{
			if (string.IsNullOrEmpty (tag))
				return 0;

			var realTag = new char[4];
			var len = Math.Min (4, tag.Length);
			var i = 0;
			for (; i < len; i++)
				realTag[i] = tag[i];
			for (; i < 4; i++)
				realTag[i] = ' ';

			return (uint)(((byte)realTag[0] << 24) | ((byte)realTag[1] << 16) | ((byte)realTag[2] << 8) | (byte)realTag[3]);
		}

		public static IEnumerable<object[]> EquivalenceInputs ()
		{
			var inputs = new[]
			{
				null, "", " ", "a", "ab", "abc", "abcd", "abcde", "abcdef",
				"wght", "wdth", "slnt", "opsz", "ital", "GSUB", "GPOS", "OS/2",
				"    ", "\0\0\0\0", "\t\r\n ",
				// characters above 0xFF: only the low byte is kept, exactly like the old code
				"\u0100\u0141\u2764\uFFFF", "é\u00e9\u00ffz", "\uABCDwght"[..4],
				// surrogate-ish high code units (still just char code units to the algorithm)
				"\uD83D\uDE00xy",
			};
			foreach (var s in inputs)
				yield return new object[] { s };
		}

		[Theory]
		[MemberData (nameof (EquivalenceInputs))]
		public void FourByteTagParseMatchesReference (string input)
		{
			var expected = ReferenceParse (input);
			var actual = (uint)SKFourByteTag.Parse (input);
			Assert.Equal (expected, actual);
		}

		[Theory]
		[MemberData (nameof (EquivalenceInputs))]
		public void FourByteTagParseSpanOverloadMatchesStringOverload (string input)
		{
			var fromString = (uint)SKFourByteTag.Parse (input);
			var fromSpan = (uint)SKFourByteTag.Parse (input.AsSpan ());
			Assert.Equal (fromString, fromSpan);
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

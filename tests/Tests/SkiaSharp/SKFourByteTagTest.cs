using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKFourByteTagTest : SKTest
	{
		[SkippableFact]
		public void FourByteTagParseProducesCorrectValue ()
		{
			var tag = SKFourByteTag.Parse ("wght");
			Assert.Equal (0x77676874u, (uint)tag);
		}

		[SkippableFact]
		public void FourByteTagToStringRoundTrips ()
		{
			var tag = SKFourByteTag.Parse ("wght");
			Assert.Equal ("wght", tag.ToString ());
		}

		[SkippableFact]
		public void FourByteTagCharConstructorMatchesParse ()
		{
			var fromParse = SKFourByteTag.Parse ("wdth");
			var fromChars = new SKFourByteTag ('w', 'd', 't', 'h');
			Assert.Equal (fromParse, fromChars);
		}

		[SkippableFact]
		public void FourByteTagImplicitConversionRoundTrips ()
		{
			SKFourByteTag tag = SKFourByteTag.Parse ("slnt");
			uint rawValue = tag;
			SKFourByteTag back = rawValue;
			Assert.Equal (tag, back);
			Assert.Equal ("slnt", back.ToString ());
		}

		[SkippableFact]
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

		[SkippableFact]
		public void FourByteTagDefaultIsZero ()
		{
			var tag = default(SKFourByteTag);
			Assert.Equal (0u, (uint)tag);
		}

		[SkippableFact]
		public void FourByteTagParseEmptyReturnsZero ()
		{
			var tag = SKFourByteTag.Parse ("");
			Assert.Equal (0u, (uint)tag);
		}

		[SkippableFact]
		public void FourByteTagParseNullReturnsZero ()
		{
			var tag = SKFourByteTag.Parse (null);
			Assert.Equal (0u, (uint)tag);
		}

		[SkippableFact]
		public void FourByteTagParseShortStringPadsWithSpaces ()
		{
			// "ab" should become "ab  " (padded with spaces)
			var tag = SKFourByteTag.Parse ("ab");
			Assert.Equal ("ab  ", tag.ToString ());
		}

		[SkippableFact]
		public void FourByteTagParseLongStringTruncates ()
		{
			// Only first 4 characters used
			var tag = SKFourByteTag.Parse ("abcdef");
			Assert.Equal ("abcd", tag.ToString ());
		}

		[SkippableFact]
		public void FourByteTagKnownTagValues ()
		{
			// Well-known OpenType tag values
			Assert.Equal (0x77676874u, (uint)SKFourByteTag.Parse ("wght"));
			Assert.Equal (0x77647468u, (uint)SKFourByteTag.Parse ("wdth"));
			Assert.Equal (0x736C6E74u, (uint)SKFourByteTag.Parse ("slnt"));
			Assert.Equal (0x6F70737Au, (uint)SKFourByteTag.Parse ("opsz"));
			Assert.Equal (0x6974616Cu, (uint)SKFourByteTag.Parse ("ital"));
		}

		[SkippableFact]
		public void FourByteTagGetHashCodeConsistent ()
		{
			var a = SKFourByteTag.Parse ("wght");
			var b = SKFourByteTag.Parse ("wght");
			Assert.Equal (a.GetHashCode (), b.GetHashCode ());
		}

		[SkippableFact]
		public void FourByteTagEqualsObjectWorks ()
		{
			var tag = SKFourByteTag.Parse ("wght");
			Assert.True (tag.Equals ((object)SKFourByteTag.Parse ("wght")));
			Assert.False (tag.Equals ((object)SKFourByteTag.Parse ("wdth")));
			Assert.False (tag.Equals ("wght")); // wrong type
		}

		[SkippableFact]
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

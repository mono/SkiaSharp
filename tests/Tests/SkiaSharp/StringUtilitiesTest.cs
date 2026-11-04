using System.Text;
using Xunit;

namespace SkiaSharp.Tests
{
	public class StringUtilitiesTest : SKTest
	{
		private static Encoding GetEncoding (SKTextEncoding encoding) =>
			encoding switch {
				SKTextEncoding.Utf8 => Encoding.UTF8,
				SKTextEncoding.Utf16 => Encoding.Unicode,
				SKTextEncoding.Utf32 => Encoding.UTF32,
				_ => throw new System.ArgumentOutOfRangeException (nameof (encoding)),
			};

		// The original (pre-optimization) semantics: append a single '\0' char to a
		// non-empty string, then encode. This is the behaviour the fast path must match
		// byte-for-byte.
		private static byte[] Reference (string text, SKTextEncoding encoding, bool addNull)
		{
			if (!string.IsNullOrEmpty (text) && addNull)
				text += "\0";
			if (string.IsNullOrEmpty (text))
				return new byte[0];
			return GetEncoding (encoding).GetBytes (text);
		}

		[Theory]
		[InlineData (SKTextEncoding.Utf8)]
		[InlineData (SKTextEncoding.Utf16)]
		[InlineData (SKTextEncoding.Utf32)]
		public void AddNullMatchesReferenceAcrossInputs (SKTextEncoding encoding)
		{
			var inputs = new[] {
				null, "", "A", "Hello", "Arial", "Segoe UI Symbol",
				"https://example.com/annotation/link",
				"café", "naïve", "日本語", "😀", "a😀b",
				"\u0000embedded", "trailing\u0000", "mixed \u0000 null",
				"\uD83D\uDE00", // valid surrogate pair
				"\uD83D",       // lone high surrogate
				"\uDE00",       // lone low surrogate
				"x\uD83Dy",     // isolated surrogate in the middle
				"\uFEFFbom",
				new string ('z', 1000),
			};

			foreach (var text in inputs) {
				foreach (var addNull in new[] { true, false }) {
					var expected = Reference (text, encoding, addNull);
					var actual = StringUtilities.GetEncodedText (text, encoding, addNull);
					Assert.Equal (expected, actual);
				}
			}
		}

		[Theory]
		[InlineData (SKTextEncoding.Utf8, 1)]
		[InlineData (SKTextEncoding.Utf16, 2)]
		[InlineData (SKTextEncoding.Utf32, 4)]
		public void AddNullAppendsExactlyOneNullCharacter (SKTextEncoding encoding, int nullByteCount)
		{
			var withNull = StringUtilities.GetEncodedText ("Arial", encoding, true);
			var withoutNull = StringUtilities.GetEncodedText ("Arial", encoding, false);

			// The only difference is the trailing null character.
			Assert.Equal (withoutNull.Length + nullByteCount, withNull.Length);
			for (var i = 0; i < withoutNull.Length; i++)
				Assert.Equal (withoutNull[i], withNull[i]);
			for (var i = withoutNull.Length; i < withNull.Length; i++)
				Assert.Equal (0, withNull[i]);
		}

		[Theory]
		[InlineData (SKTextEncoding.Utf8)]
		[InlineData (SKTextEncoding.Utf16)]
		[InlineData (SKTextEncoding.Utf32)]
		public void EmptyAndNullNeverAppendNull (SKTextEncoding encoding)
		{
			Assert.Empty (StringUtilities.GetEncodedText (null, encoding, true));
			Assert.Empty (StringUtilities.GetEncodedText ("", encoding, true));
		}

		[Theory]
		[InlineData ("A", SKTextEncoding.Utf8, 'A')]
		[InlineData ("A", SKTextEncoding.Utf16, 'A')]
		[InlineData ("A", SKTextEncoding.Utf32, 'A')]
		[InlineData ("ä", SKTextEncoding.Utf16, 'ä')]
		[InlineData ("ä", SKTextEncoding.Utf32, 'ä')]
		[InlineData ("年", SKTextEncoding.Utf16, '年')]
		[InlineData ("年", SKTextEncoding.Utf32, '年')]
		[InlineData ("🚀", SKTextEncoding.Utf32, 0x1F680)]
		public void GetUnicodeCharacterCodeReturnsExpectedCodePoint (string character, SKTextEncoding encoding, int expected)
		{
			var actual = StringUtilities.GetUnicodeCharacterCode (character, encoding);
			Assert.Equal (expected, actual);
		}

		[Theory]
		[InlineData (SKTextEncoding.Utf8)]
		[InlineData (SKTextEncoding.Utf16)]
		[InlineData (SKTextEncoding.Utf32)]
		public void GetUnicodeCharacterCodeThrowsOnNull (SKTextEncoding encoding)
		{
			var ex = Assert.Throws<System.ArgumentNullException> (() => StringUtilities.GetUnicodeCharacterCode (null, encoding));
			Assert.Equal ("character", ex.ParamName);
		}

		[Theory]
		[InlineData ("", SKTextEncoding.Utf8)]
		[InlineData ("", SKTextEncoding.Utf16)]
		[InlineData ("", SKTextEncoding.Utf32)]
		[InlineData ("AB", SKTextEncoding.Utf8)]
		[InlineData ("AB", SKTextEncoding.Utf16)]
		[InlineData ("AB", SKTextEncoding.Utf32)]
		[InlineData ("ä", SKTextEncoding.Utf8)]
		[InlineData ("年", SKTextEncoding.Utf8)]
		[InlineData ("🚀", SKTextEncoding.Utf8)]
		[InlineData ("🚀", SKTextEncoding.Utf16)]
		[InlineData ("🚀A", SKTextEncoding.Utf32)]
		public void GetUnicodeCharacterCodeThrowsOnMultipleOrWideCharacters (string character, SKTextEncoding encoding)
		{
			var ex = Assert.Throws<System.ArgumentException> (() => StringUtilities.GetUnicodeCharacterCode (character, encoding));
			Assert.Equal ("character", ex.ParamName);
		}

		[Theory]
		[InlineData (SKTextEncoding.GlyphId)]
		[InlineData ((SKTextEncoding)999)]
		public void GetUnicodeCharacterCodeThrowsOnUnsupportedEncoding (SKTextEncoding encoding)
		{
			var ex = Assert.Throws<System.ArgumentOutOfRangeException> (() => StringUtilities.GetUnicodeCharacterCode ("A", encoding));
			Assert.Equal ("encoding", ex.ParamName);
		}

		[Theory]
		[InlineData (SKTextEncoding.Utf8)]
		[InlineData (SKTextEncoding.Utf16)]
		[InlineData (SKTextEncoding.Utf32)]
		public void GetStringThrowsOnNullByteArray (SKTextEncoding encoding)
		{
			var ex1 = Assert.Throws<System.ArgumentNullException> (() => StringUtilities.GetString ((byte[])null, encoding));
			Assert.Equal ("data", ex1.ParamName);

			var ex2 = Assert.Throws<System.ArgumentNullException> (() => StringUtilities.GetString ((byte[])null, 0, 0, encoding));
			Assert.Equal ("data", ex2.ParamName);
		}
	}
}

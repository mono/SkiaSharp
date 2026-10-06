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
	}
}

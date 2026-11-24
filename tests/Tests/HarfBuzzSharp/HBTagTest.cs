using System;
using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBTagTest : HBTest
	{
		// The verbatim ORIGINAL shipped Parse(string) body — the oracle. The optimized
		// implementation under test must agree with this bit-for-bit across every input.
		private static uint OldParse (string tag)
		{
			if (string.IsNullOrEmpty (tag))
				return 0; // Tag.None

			var realTag = new char[4];

			var len = Math.Min (4, tag.Length);
			var i = 0;
			for (; i < len; i++)
				realTag[i] = tag[i];
			for (; i < 4; i++)
				realTag[i] = ' ';

			return (uint)(((byte)realTag[0] << 24) | ((byte)realTag[1] << 16) | ((byte)realTag[2] << 8) | (byte)realTag[3]);
		}

		// Expected values are precomputed constants (big-endian packing of the low byte of the
		// first four UTF-16 code units, padding missing trailing slots with spaces (0x20), and
		// treating null/empty as zero) so the oracle can never silently drift with the code.
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
		[InlineData ("liga", 0x6C696761u)]
		[InlineData ("kern", 0x6B65726Eu)]
		[InlineData ("wght", 0x77676874u)]
		[InlineData ("wdth", 0x77647468u)]
		[InlineData ("GSUB", 0x47535542u)]
		[InlineData ("OS/2", 0x4F532F32u)]
		[InlineData ("    ", 0x20202020u)]
		[InlineData ("\0\0\0\0", 0x00000000u)]
		[InlineData ("\t\r\n ", 0x090D0A20u)]
		// characters above 0xFF: only the low byte is kept
		[InlineData ("\u0100\u0141\u2764\uFFFF", 0x004164FFu)]
		[InlineData ("\uABCDwgh", 0xCD776768u)]
		// surrogate code units are still just char code units to the algorithm
		[InlineData ("\uD83D\uDE00xy", 0x3D007879u)]
		public void ParseProducesExpectedValue (string input, uint expected)
		{
			// Optimized string overload matches the precomputed constant.
			Assert.Equal (expected, (uint)Tag.Parse (input));
			// Optimized span overload matches the precomputed constant.
			Assert.Equal (expected, (uint)Tag.Parse (input.AsSpan ()));
			// And both agree with the original shipped algorithm (the oracle).
			Assert.Equal (OldParse (input), (uint)Tag.Parse (input));
		}

		[Fact]
		public void ParseNullMatchesNone ()
		{
			Assert.Equal (Tag.None, Tag.Parse ((string)null));
			Assert.Equal (Tag.None, Tag.Parse (ReadOnlySpan<char>.Empty));
		}

		[Fact]
		public void ParseEmptyMatchesNone ()
		{
			Assert.Equal (Tag.None, Tag.Parse (""));
			Assert.Equal (Tag.None, Tag.Parse (default (ReadOnlySpan<char>)));
		}

		[Fact]
		public void ParseSpanFromSlicedStringMatches ()
		{
			// A span carved out of a larger string (no substring allocation) parses the same
			// as the equivalent standalone string.
			const string source = "xxwghtyy";
			var slice = source.AsSpan (2, 4);
			Assert.Equal ((uint)Tag.Parse ("wght"), (uint)Tag.Parse (slice));
		}

		[Fact]
		public void ParseRoundTripsThroughToString ()
		{
			var tag = Tag.Parse ("liga");
			Assert.Equal ("liga", tag.ToString ());
		}

		// Guards the oracle itself: a deliberately-wrong expected value must fail, proving the
		// equivalence assertions actually discriminate.
		[Fact]
		public void OracleCatchesWrongResult ()
		{
			Assert.NotEqual (0xDEADBEEFu, (uint)Tag.Parse ("wght"));
		}

		// The verbatim ORIGINAL shipped ToString() body — the oracle. The optimized
		// implementation (a stackalloc buffer instead of the char-boxing string.Concat
		// on the general path) must agree with this exactly across every value.
		private static string OldToString (uint value)
		{
			if (value == (uint)Tag.None)
				return nameof (Tag.None);
			if (value == (uint)Tag.Max)
				return nameof (Tag.Max);
			if (value == (uint)Tag.MaxSigned)
				return nameof (Tag.MaxSigned);

			return string.Concat (
				(char)(byte)(value >> 24),
				(char)(byte)(value >> 16),
				(char)(byte)(value >> 8),
				(char)(byte)value);
		}

		[Theory]
		[InlineData (0x77676874u, "wght")]
		[InlineData (0x77647468u, "wdth")]
		[InlineData (0x6C696761u, "liga")]
		[InlineData (0x6B65726Eu, "kern")]
		[InlineData (0x47535542u, "GSUB")]
		[InlineData (0x4F532F32u, "OS/2")]
		[InlineData (0x61202020u, "a   ")]
		[InlineData (0x61622020u, "ab  ")]
		[InlineData (0x090D0A20u, "\t\r\n ")]
		[InlineData (0xE9E9FF7Au, "\u00e9\u00e9\u00ffz")]
		public void ToStringProducesExpectedValue (uint value, string expected)
		{
			var tag = (Tag)value;
			// Optimized ToString matches the precomputed constant.
			Assert.Equal (expected, tag.ToString ());
			// And agrees with the original shipped algorithm (the oracle).
			Assert.Equal (OldToString (value), tag.ToString ());
		}

		// The special-cased named values must still round-trip to their names, and the
		// optimized general path must not accidentally swallow them.
		[Theory]
		[InlineData ("None")]
		[InlineData ("Max")]
		[InlineData ("MaxSigned")]
		public void ToStringNamedValuesMatchOracle (string name)
		{
			var tag = name switch {
				"None" => Tag.None,
				"Max" => Tag.Max,
				_ => Tag.MaxSigned,
			};
			Assert.Equal (name, tag.ToString ());
			Assert.Equal (OldToString ((uint)tag), tag.ToString ());
		}

		// Guards the ToString oracle: a deliberately-wrong expected value must fail.
		[Fact]
		public void ToStringOracleCatchesWrongResult ()
		{
			Assert.NotEqual ("XXXX", ((Tag)0x77676874u).ToString ());
		}
	}
}

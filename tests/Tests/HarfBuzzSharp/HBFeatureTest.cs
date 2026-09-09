using System;
using System.Runtime.InteropServices;

using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBFeatureTest : HBTest
	{
		[Fact]
		public void ShouldCreateFeatureFromString()
		{
			var feature = Feature.Parse("Kern");

			Assert.Equal(Tag.Parse("Kern"), feature.Tag);
		}

		[Fact]
		public void ToStringIsCorrect()
		{
			var feature = Feature.Parse("Kern");

			Assert.Equal("Kern", feature.ToString());
		}

		[Theory]
		[InlineData('k', 'e', 'r', 'n', 1u, 0u, uint.MaxValue)]
		[InlineData('l', 'i', 'g', 'a', 0u, 0u, uint.MaxValue)]
		[InlineData('a', 'a', 'l', 't', 2u, 0u, uint.MaxValue)]
		[InlineData('s', 'm', 'c', 'p', 1u, 3u, 5u)]
		[InlineData('o', 'n', 'u', 'm', 1u, 2u, 10u)]
		[InlineData('c', 'a', 'l', 't', 0u, 0u, 100u)]
		[InlineData('d', 'l', 'i', 'g', 5u, 1u, 2u)]
		[InlineData('s', 's', '0', '1', 1u, 0u, uint.MaxValue)]
		public void ToStringMatchesNativeMarshalledPath(char c1, char c2, char c3, char c4, uint value, uint start, uint end)
		{
			var feature = new Feature(new Tag(c1, c2, c3, c4), value, start, end);

			// The managed stackalloc path must produce byte-for-byte the same string as the
			// original AllocHGlobal/PtrToStringAnsi/FreeHGlobal implementation.
			Assert.Equal(OriginalToString(feature), feature.ToString());
		}

		[Fact]
		public void ToStringRoundTripsThroughParse()
		{
			var feature = new Feature(new Tag('k', 'e', 'r', 'n'), 1, 0, uint.MaxValue);

			var str = feature.ToString();

			Assert.True(Feature.TryParse(str, out var parsed));
			Assert.Equal(feature.Tag, parsed.Tag);
			Assert.Equal(feature.Value, parsed.Value);
			Assert.Equal(feature.Start, parsed.Start);
			Assert.Equal(feature.End, parsed.End);
		}

		[Fact]
		public void ShouldThrowFromUnknownString()
		{
			Assert.False(Feature.TryParse("", out var script));
			Assert.Equal(Tag.None, script.Tag);
			Assert.Throws<FormatException>(() => Feature.Parse(""));
		}

		// Verbatim copy of the ORIGINAL shipped ToString() body — the oracle the managed
		// fast path must match exactly.
		private static unsafe string OriginalToString(Feature feature)
		{
			const int MaxFeatureStringSize = 128;
			var f = &feature;
			var buffer = Marshal.AllocHGlobal(MaxFeatureStringSize);
			HarfBuzzApi.hb_feature_to_string(f, (void*)buffer, MaxFeatureStringSize);
			var str = Marshal.PtrToStringAnsi(buffer);
			Marshal.FreeHGlobal(buffer);
			return str;
		}
	}
}


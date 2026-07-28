using System;
using System.Collections.Generic;
using Xunit;

namespace SkiaSharp.Tests
{
	// Equivalence proof for the managed port of the single-value SKPMColor premultiply /
	// unpremultiply conversions (see SKPMColor.PreMultiply(SKColor) / UnPreMultiply(SKPMColor)).
	//
	// The native oracle is SkiaApi.sk_color_premultiply / sk_color_unpremultiply (the exact code
	// the binding called before the port). Both conversions are per-channel maps parameterised
	// only by alpha, so sweeping (alpha 0..255) x (channel 0..255) for each of the three colour
	// channel positions is a COMPLETE, bit-exact enumeration of every reachable behaviour — with
	// distinct sentinels in the other channels so a swapped/mis-shifted channel is also caught.
	// The exhaustive tests drive the PUBLIC API (SKPMColor.PreMultiply / UnPreMultiply) so they
	// guard the shipped managed implementation directly, not a copy of it.
	public class SKPMColorEquivalenceTest : SKTest
	{
		// Distinct sentinels (all different) placed in the non-swept channels so the test also
		// pins the packing order, not just the per-channel scale.
		private const uint R0 = 0x11;
		private const uint G0 = 0x77;
		private const uint B0 = 0xCC;

		// ---- Helpers used only by the deliberately-wrong "teeth" guards below ----

		private static uint UnPreMultiplyScale (uint alpha) =>
			// Skia stores round((255 << 24) / alpha), not the floor: the +(alpha >> 1) bias makes
			// the port bit-exact even for out-of-gamut inputs where a colour channel exceeds alpha.
			alpha == 0 ? 0u : ((255u << 24) + (alpha >> 1)) / alpha;

		private static uint ApplyUnPreMultiplyScale (uint scale, uint component) =>
			// 32-bit multiply wraps exactly like the native uint32_t path; the >> 24 of a 32-bit
			// value is always <= 255, so the packing never bleeds across channels.
			unchecked ((scale * component + (1u << 23)) >> 24);

		// ---- Exhaustive equivalence against the native oracle ----

		[Fact]
		public void ManagedPreMultiplyMatchesNativeForEveryAlphaAndChannel ()
		{
			var failures = new List<string> ();

			for (uint a = 0; a <= 255; a++) {
				for (uint v = 0; v <= 255; v++) {
					CheckPreMultiply (failures, (a << 24) | (v << 16) | (G0 << 8) | B0);   // sweep R
					CheckPreMultiply (failures, (a << 24) | (R0 << 16) | (v << 8) | B0);   // sweep G
					CheckPreMultiply (failures, (a << 24) | (R0 << 16) | (G0 << 8) | v);   // sweep B
				}
			}

			Assert.True (failures.Count == 0, FormatFailures ("premultiply", failures));
		}

		[Fact]
		public void ManagedUnPreMultiplyMatchesNativeForEveryAlphaAndChannel ()
		{
			var aShift = SKImageInfo.PlatformColorAlphaShift;
			var rShift = SKImageInfo.PlatformColorRedShift;
			var gShift = SKImageInfo.PlatformColorGreenShift;
			var bShift = SKImageInfo.PlatformColorBlueShift;

			var failures = new List<string> ();

			for (uint a = 0; a <= 255; a++) {
				for (uint v = 0; v <= 255; v++) {
					CheckUnPreMultiply (failures, (a << aShift) | (v << rShift) | (G0 << gShift) | (B0 << bShift));
					CheckUnPreMultiply (failures, (a << aShift) | (R0 << rShift) | (v << gShift) | (B0 << bShift));
					CheckUnPreMultiply (failures, (a << aShift) | (R0 << rShift) | (G0 << gShift) | (v << bShift));
				}
			}

			Assert.True (failures.Count == 0, FormatFailures ("unpremultiply", failures));
		}

		private static void CheckPreMultiply (List<string> failures, uint color)
		{
			var expected = SkiaApi.sk_color_premultiply (color);
			var actual = (uint)SKPMColor.PreMultiply ((SKColor)color);
			if (expected != actual && failures.Count < 16)
				failures.Add ($"color=0x{color:X8}: native=0x{expected:X8} managed=0x{actual:X8}");
		}

		private static void CheckUnPreMultiply (List<string> failures, uint pmcolor)
		{
			var expected = SkiaApi.sk_color_unpremultiply (pmcolor);
			var actual = (uint)(SKColor)SKPMColor.UnPreMultiply ((SKPMColor)pmcolor);
			if (expected != actual && failures.Count < 16)
				failures.Add ($"pm=0x{pmcolor:X8}: native=0x{expected:X8} managed=0x{actual:X8}");
		}

		private static string FormatFailures (string op, List<string> failures) =>
			failures.Count == 0
				? string.Empty
				: $"{op} diverged from native on {failures.Count}+ inputs:\n  " + string.Join ("\n  ", failures);

		// ---- Round trip: premultiply then unpremultiply recovers the original (opaque) colour ----

		[Fact]
		public void PreMultiplyRoundTripsForOpaqueColors ()
		{
			for (uint r = 0; r <= 255; r += 7) {
				for (uint g = 0; g <= 255; g += 11) {
					for (uint b = 0; b <= 255; b += 13) {
						var color = new SKColor ((byte)r, (byte)g, (byte)b, 255);
						var pm = SKPMColor.PreMultiply (color);
						Assert.Equal (color, SKPMColor.UnPreMultiply (pm));
					}
				}
			}
		}

		// ---- Guard: prove the exhaustive comparison actually has teeth ----
		//
		// A deliberately-wrong premultiply (drops the rounding bias) and a deliberately-wrong
		// unpremultiply (swaps two channels) must BOTH be detected by the same oracle comparison,
		// otherwise a green run above would be meaningless.

		[Fact]
		public void ExhaustiveComparisonCatchesAWrongPreMultiply ()
		{
			var caught = false;
			for (uint a = 0; a <= 255 && !caught; a++) {
				for (uint v = 0; v <= 255 && !caught; v++) {
					var color = (a << 24) | (v << 16) | (G0 << 8) | B0;
					if (SkiaApi.sk_color_premultiply (color) != WrongPreMultiply (color))
						caught = true;
				}
			}
			Assert.True (caught, "A knowingly-wrong premultiply was NOT detected — the oracle comparison lacks teeth.");
		}

		[Fact]
		public void ExhaustiveComparisonCatchesAWrongUnPreMultiply ()
		{
			var aShift = SKImageInfo.PlatformColorAlphaShift;
			var rShift = SKImageInfo.PlatformColorRedShift;
			var gShift = SKImageInfo.PlatformColorGreenShift;
			var bShift = SKImageInfo.PlatformColorBlueShift;

			var caught = false;
			for (uint a = 1; a <= 255 && !caught; a++) {
				for (uint v = 0; v <= 255 && !caught; v++) {
					var pm = (a << aShift) | (v << rShift) | (Math.Min (G0, a) << gShift) | (Math.Min (B0, a) << bShift);
					if (SkiaApi.sk_color_unpremultiply (pm) != WrongUnPreMultiply (pm))
						caught = true;
				}
			}
			Assert.True (caught, "A knowingly-wrong unpremultiply was NOT detected — the oracle comparison lacks teeth.");
		}

		private static uint WrongPreMultiply (uint color)
		{
			// The correct premultiply but with the +128 rounding bias dropped -> off-by-one on many inputs.
			uint a = (color >> 24) & 0xff;
			uint r = (color >> 16) & 0xff;
			uint g = (color >> 8) & 0xff;
			uint b = color & 0xff;
			if (a != 255) {
				r = (r * a) / 255;
				g = (g * a) / 255;
				b = (b * a) / 255;
			}
			return (a << SKImageInfo.PlatformColorAlphaShift) |
				(r << SKImageInfo.PlatformColorRedShift) |
				(g << SKImageInfo.PlatformColorGreenShift) |
				(b << SKImageInfo.PlatformColorBlueShift);
		}

		private static uint WrongUnPreMultiply (uint pmcolor)
		{
			// The correct unpremultiply but with red and blue swapped on output.
			uint a = (pmcolor >> SKImageInfo.PlatformColorAlphaShift) & 0xff;
			uint r = (pmcolor >> SKImageInfo.PlatformColorRedShift) & 0xff;
			uint g = (pmcolor >> SKImageInfo.PlatformColorGreenShift) & 0xff;
			uint b = (pmcolor >> SKImageInfo.PlatformColorBlueShift) & 0xff;
			uint scale = UnPreMultiplyScale (a);
			r = ApplyUnPreMultiplyScale (scale, r);
			g = ApplyUnPreMultiplyScale (scale, g);
			b = ApplyUnPreMultiplyScale (scale, b);
			return (a << 24) | (b << 16) | (g << 8) | r;
		}
	}
}

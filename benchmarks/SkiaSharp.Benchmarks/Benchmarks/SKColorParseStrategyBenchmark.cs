using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// Strategy shootout for the hex-nibble conversion at the heart of SKColor.TryParse.
//
// Every strategy shares the SAME outer structure (trim whitespace + leading '#', switch on the
// trimmed length, read each channel) and differs ONLY in how a hex char becomes a 0-15 nibble.
// The outer loop is a generic method specialized over a struct strategy, which RyuJIT devirtualizes
// and inlines, so we measure the conversion itself, not delegate/interface overhead.
//
// All strategies are case-insensitive (accept 'f', 'F', and mixed like 'fF'). GlobalSetup asserts
// every strategy agrees with the shipped SKColor.TryParse across a vector set before timing, so a
// fast-but-wrong strategy can't win.
//
// RESULTS (net10.0, Apple M3 Pro, BenchmarkDotNet 0.13.5), ns per parse; lower is better. Every
// candidate is validated against the shipped SKColor.TryParse in GlobalSetup before timing, so all
// rows are known-correct and case-insensitive.
//
// The "Mixed" column parses a diverse, varied stream of 24 real-world colors in a loop
// (OperationsPerInvoke) - this is the number that matters for real workloads. The two single-input
// columns are kept from earlier runs to show WHY: an all-same input like "#FFFFFFFF" flatters a
// jump-table switch (its indirect jump always hits one target -> perfectly predicted), while a varied
// stream exposes the misprediction cost.
//
// | Strategy                                | Mixed (real) | #FFFFFFFF (all-same) | #FFF (all-same) |
// |-----------------------------------------|-------------:|---------------------:|----------------:|
// | Old (byte/uint.TryParse baseline)       |        15.65 |                10.72 |           15.15 |
// | GiantSwitch (jump table)                |        14.00 |    4.63  (best here!) |    2.59 (best!) |
// | AccumLookup (single uint loop + table)  |         8.71 |                 7.61 |            4.86 |
// | FoldRange (fold then range)             |         8.58 |                 6.50 |            3.46 |
// | Branchless (c|0x20 fold, branch-free)   |         8.14 |                 7.99 |            4.14 |
// | IfRange (range checks) ** SHIPPED **    |         8.09 |                 7.73 |            4.15 |
// | LookupTable (256-entry) ** FASTEST **   |         7.54 |                 7.60 |            3.58 |
//
// KEY INSIGHT: GiantSwitch looks fastest on all-same inputs but is ~2x SLOWER on realistic varied
// input because its indirect jump mispredicts. The 256-entry LookupTable is the outright fastest
// (data-driven, no data-dependent branch, most consistent). SKColor.TryParseNibble ships the IfRange
// strategy instead: it is within ~7% of the table on real input, still ~2x faster than the old
// TryParse-based code, and needs no static data. The LookupTable remains the choice if this ever
// becomes a hot enough path to justify the 256-byte table.
[MemoryDiagnoser]
public class SKColorParseStrategyBenchmark
{
	// A realistic, varied stream of colors (mixed lengths, cases, and digits) parsed in a loop. This
	// is the number that matters for real workloads: consecutive parses see different digits, so any
	// strategy that relies on a well-predicted indirect branch (a jump-table switch) pays its true
	// cost here, unlike an all-same input such as "#FFFFFFFF" which the branch predictor nails.
	private static readonly string[] Mixed =
	{
		"#1B4F8C", "#A7A8A9A0", "#ff8800", "#3E7", "#abcdef", "#12345678", "#0a0", "#DEADBE",
		"#c0ffee", "#123", "#9B2D5F", "#FFAA0080", "#4682b4", "#7f7f7f", "#e91e63", "#0d1b2a",
		"#F0A", "#5566", "#00ff7f", "#8a2be2", "#Cd5C5c", "#2F4F4F", "#adff2f", "#ABCDEF01",
	};
	private const int MixedCount = 24;

	[GlobalSetup]
	public void Validate()
	{
		string[] vectors =
		{
			"#FFFFFFFF", "#ffffffff", "#fFfFfFfF", "#FFF", "#FFFF", "#FFFFFF",
			"#a4C5e6", "#ABC", "#ABCD", "#AAABACAD", "ABCDEF", "A7A8A9A0",
			"  #ABC  ", "#123456ug", "12sd", "#ABCDE", "Red", "", null, "#", "# 12345",
		};

		foreach (var v in vectors)
		{
			var shippedOk = SKColor.TryParse(v, out var expected);
			var e = (uint)expected;

			// Note: "Old" is the baseline and intentionally differs from shipped on whitespace-after-'#'
			// (e.g. "# 12345"), so it is not cross-checked here. Every candidate strategy must match shipped.
			CheckOne("Lookup", Parse<Lookup>(v, out var lc), lc, shippedOk, e, v);
			CheckOne("Branchless", Parse<Branchless>(v, out var bc), bc, shippedOk, e, v);
			CheckOne("GiantSwitch", Parse<GiantSwitch>(v, out var sc), sc, shippedOk, e, v);
			CheckOne("FoldRange", Parse<FoldRange>(v, out var fc), fc, shippedOk, e, v);
			CheckOne("IfRange", Parse<IfRange>(v, out var ic), ic, shippedOk, e, v);
			CheckOne("Accum", ParseAccum(v, out var ac), ac, shippedOk, e, v);
		}
	}

	private static void CheckOne(string name, bool ok, uint got, bool shippedOk, uint expected, string input)
	{
		if (ok != shippedOk)
			throw new InvalidOperationException($"{name} accept/reject mismatch on '{input ?? "<null>"}' (strategy={ok}, shipped={shippedOk})");
		if (ok && got != expected)
			throw new InvalidOperationException($"{name} value mismatch on '{input}': got {got:X8} want {expected:X8}");
	}

	// ---- Benchmarks (each parses the whole Mixed stream; OperationsPerInvoke normalizes to ns/parse) --

	[Benchmark(Baseline = true, OperationsPerInvoke = MixedCount)]
	public uint Shipped()
	{
		uint acc = 0;
		foreach (var s in Mixed)
		{
			SKColor.TryParse(s, out var c);
			acc ^= (uint)c;
		}
		return acc;
	}

	[Benchmark(OperationsPerInvoke = MixedCount)]
	public uint Old()
	{
		uint acc = 0;
		foreach (var s in Mixed)
		{
			OldTryParse(s, out var c);
			acc ^= c;
		}
		return acc;
	}

	[Benchmark(OperationsPerInvoke = MixedCount)]
	public uint LookupTable()
	{
		uint acc = 0;
		foreach (var s in Mixed)
		{
			Parse<Lookup>(s, out var c);
			acc ^= c;
		}
		return acc;
	}

	[Benchmark(OperationsPerInvoke = MixedCount)]
	public uint Branchless_()
	{
		uint acc = 0;
		foreach (var s in Mixed)
		{
			Parse<Branchless>(s, out var c);
			acc ^= c;
		}
		return acc;
	}

	[Benchmark(OperationsPerInvoke = MixedCount)]
	public uint GiantSwitch_()
	{
		uint acc = 0;
		foreach (var s in Mixed)
		{
			Parse<GiantSwitch>(s, out var c);
			acc ^= c;
		}
		return acc;
	}

	[Benchmark(OperationsPerInvoke = MixedCount)]
	public uint FoldRange_()
	{
		uint acc = 0;
		foreach (var s in Mixed)
		{
			Parse<FoldRange>(s, out var c);
			acc ^= c;
		}
		return acc;
	}

	[Benchmark(OperationsPerInvoke = MixedCount)]
	public uint IfRange_()
	{
		uint acc = 0;
		foreach (var s in Mixed)
		{
			Parse<IfRange>(s, out var c);
			acc ^= c;
		}
		return acc;
	}

	[Benchmark(OperationsPerInvoke = MixedCount)]
	public uint AccumLookup()
	{
		uint acc = 0;
		foreach (var s in Mixed)
		{
			ParseAccum(s, out var c);
			acc ^= c;
		}
		return acc;
	}

	// ---- Shared generic outer parser -------------------------------------------------------

	private interface INibble
	{
		bool TryNibble(char c, out int value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool Parse<T>(string hexString, out uint color) where T : struct, INibble
	{
		color = 0;
		if (string.IsNullOrEmpty(hexString))
			return false;

		var start = 0;
		var end = hexString.Length;
		while (start < end && char.IsWhiteSpace(hexString[start])) start++;
		while (end > start && char.IsWhiteSpace(hexString[end - 1])) end--;
		while (start < end && hexString[start] == '#') start++;

		var s = default(T);
		switch (end - start)
		{
			case 3:
			{
				if (!s.TryNibble(hexString[start], out var r) ||
					!s.TryNibble(hexString[start + 1], out var g) ||
					!s.TryNibble(hexString[start + 2], out var b))
					return false;
				color = 0xFF000000u | (uint)((r << 4 | r) << 16 | (g << 4 | g) << 8 | (b << 4 | b));
				return true;
			}
			case 4:
			{
				if (!s.TryNibble(hexString[start], out var a) ||
					!s.TryNibble(hexString[start + 1], out var r) ||
					!s.TryNibble(hexString[start + 2], out var g) ||
					!s.TryNibble(hexString[start + 3], out var b))
					return false;
				color = (uint)((a << 4 | a) << 24 | (r << 4 | r) << 16 | (g << 4 | g) << 8 | (b << 4 | b));
				return true;
			}
			case 6:
			{
				if (!s.TryNibble(hexString[start], out var r1) || !s.TryNibble(hexString[start + 1], out var r0) ||
					!s.TryNibble(hexString[start + 2], out var g1) || !s.TryNibble(hexString[start + 3], out var g0) ||
					!s.TryNibble(hexString[start + 4], out var b1) || !s.TryNibble(hexString[start + 5], out var b0))
					return false;
				color = 0xFF000000u | (uint)((r1 << 20) | (r0 << 16) | (g1 << 12) | (g0 << 8) | (b1 << 4) | b0);
				return true;
			}
			case 8:
			{
				if (!s.TryNibble(hexString[start], out var a1) || !s.TryNibble(hexString[start + 1], out var a0) ||
					!s.TryNibble(hexString[start + 2], out var r1) || !s.TryNibble(hexString[start + 3], out var r0) ||
					!s.TryNibble(hexString[start + 4], out var g1) || !s.TryNibble(hexString[start + 5], out var g0) ||
					!s.TryNibble(hexString[start + 6], out var b1) || !s.TryNibble(hexString[start + 7], out var b0))
					return false;
				color = (uint)((a1 << 28) | (a0 << 24) | (r1 << 20) | (r0 << 16) | (g1 << 12) | (g0 << 8) | (b1 << 4) | b0);
				return true;
			}
			default:
				return false;
		}
	}

	// ---- Nibble strategies -----------------------------------------------------------------

	private struct IfRange : INibble
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryNibble(char c, out int value)
		{
			if (c >= '0' && c <= '9') { value = c - '0'; return true; }
			if (c >= 'a' && c <= 'f') { value = c - 'a' + 10; return true; }
			if (c >= 'A' && c <= 'F') { value = c - 'A' + 10; return true; }
			value = 0;
			return false;
		}
	}

	// 256-entry table: index by char, 0xFF means invalid.
	private static ReadOnlySpan<byte> HexTable => new byte[]
	{
		//0     1     2     3     4     5     6     7     8     9     A     B     C     D     E     F
		0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 0x
		0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 1x
		0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 2x
		   0,    1,    2,    3,    4,    5,    6,    7,    8,    9, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 3x  '0'-'9'
		0xFF,   10,   11,   12,   13,   14,   15, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 4x  'A'-'F'
		0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 5x
		0xFF,   10,   11,   12,   13,   14,   15, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 6x  'a'-'f'
		0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 7x
		0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
	};

	private struct Lookup : INibble
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryNibble(char c, out int value)
		{
			if (c > 0xFF) { value = 0; return false; }
			int v = HexTable[c];
			value = v;
			return v != 0xFF;
		}
	}

	private struct Branchless : INibble
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryNibble(char c, out int value)
		{
			// Fold letters to lowercase (digits unaffected: they already have bit 0x20 set).
			uint x = (uint)c;
			uint digit = x - '0';           // 0..9 for '0'-'9'
			uint alpha = (x | 0x20) - 'a';  // 0..5 for 'a'-'f'/'A'-'F'
			bool isDigit = digit < 10u;
			bool isAlpha = alpha < 6u;
			value = (int)(isDigit ? digit : alpha + 10u);
			return isDigit | isAlpha;
		}
	}

	private struct GiantSwitch : INibble
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryNibble(char c, out int value)
		{
			switch (c)
			{
				case '0': value = 0; return true;
				case '1': value = 1; return true;
				case '2': value = 2; return true;
				case '3': value = 3; return true;
				case '4': value = 4; return true;
				case '5': value = 5; return true;
				case '6': value = 6; return true;
				case '7': value = 7; return true;
				case '8': value = 8; return true;
				case '9': value = 9; return true;
				case 'a': case 'A': value = 10; return true;
				case 'b': case 'B': value = 11; return true;
				case 'c': case 'C': value = 12; return true;
				case 'd': case 'D': value = 13; return true;
				case 'e': case 'E': value = 14; return true;
				case 'f': case 'F': value = 15; return true;
				default: value = 0; return false;
			}
		}
	}

	private struct FoldRange : INibble
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryNibble(char c, out int value)
		{
			if (c >= '0' && c <= '9') { value = c - '0'; return true; }
			var lower = (char)(c | 0x20); // fold 'A'-'F' to 'a'-'f'
			if (lower >= 'a' && lower <= 'f') { value = lower - 'a' + 10; return true; }
			value = 0;
			return false;
		}
	}

	// ---- uint-accumulation variant (single loop, lookup table) -----------------------------

	private static bool ParseAccum(string hexString, out uint color)
	{
		color = 0;
		if (string.IsNullOrEmpty(hexString))
			return false;

		var start = 0;
		var end = hexString.Length;
		while (start < end && char.IsWhiteSpace(hexString[start])) start++;
		while (end > start && char.IsWhiteSpace(hexString[end - 1])) end--;
		while (start < end && hexString[start] == '#') start++;

		var len = end - start;
		if (len != 3 && len != 4 && len != 6 && len != 8)
			return false;

		uint acc = 0;
		for (var i = start; i < end; i++)
		{
			char c = hexString[i];
			if (c > 0xFF)
				return false;
			int v = HexTable[c];
			if (v == 0xFF)
				return false;
			acc = (acc << 4) | (uint)v;
		}

		switch (len)
		{
			case 3:
				color = 0xFF000000u |
					((acc & 0xF00u) << 12) | ((acc & 0xF00u) << 8) |
					((acc & 0x0F0u) << 8) | ((acc & 0x0F0u) << 4) |
					((acc & 0x00Fu) << 4) | (acc & 0x00Fu);
				return true;
			case 4:
				color =
					((acc & 0xF000u) << 16) | ((acc & 0xF000u) << 12) |
					((acc & 0x0F00u) << 12) | ((acc & 0x0F00u) << 8) |
					((acc & 0x00F0u) << 8) | ((acc & 0x00F0u) << 4) |
					((acc & 0x000Fu) << 4) | (acc & 0x000Fu);
				return true;
			case 6:
				color = 0xFF000000u | acc;
				return true;
			default: // 8
				color = acc;
				return true;
		}
	}

	// ---- Old baseline (verbatim old span path) ---------------------------------------------

	private static bool OldTryParse(string hexString, out uint color)
	{
		color = 0;
		if (string.IsNullOrWhiteSpace(hexString))
			return false;

		var hexSpan = hexString.AsSpan().Trim().TrimStart('#');
		var len = hexSpan.Length;
		if (len == 3 || len == 4)
		{
			byte a;
			if (len == 4)
			{
				if (!byte.TryParse(hexSpan.Slice(0, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out a))
					return false;
				a = (byte)(a << 4 | a);
			}
			else a = 255;
			if (!byte.TryParse(hexSpan.Slice(len - 3, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) ||
				!byte.TryParse(hexSpan.Slice(len - 2, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) ||
				!byte.TryParse(hexSpan.Slice(len - 1, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
				return false;
			color = (uint)((a << 24) | ((r << 4 | r) << 16) | ((g << 4 | g) << 8) | (b << 4 | b));
			return true;
		}
		if (len == 6 || len == 8)
		{
			if (!uint.TryParse(hexSpan, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var number))
				return false;
			if (len == 6) number |= 0xFF000000u;
			color = number;
			return true;
		}
		return false;
	}
}

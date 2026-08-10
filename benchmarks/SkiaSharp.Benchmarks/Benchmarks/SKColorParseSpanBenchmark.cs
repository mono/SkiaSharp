using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// Does it matter whether SKColor.TryParse walks the input as a string (indexing hexString[i] with
// manually tracked start/end offsets) or as a ReadOnlySpan<char> (like the old
// "#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER" branch, which used
// hexString.AsSpan().Trim().TrimStart('#'))?
//
// Both variants below use the identical if-chain nibble conversion and produce identical results
// (GlobalSetup asserts each matches the shipped SKColor.TryParse before timing). The ONLY difference
// is trimming + character access: manual index loops over the string vs Trim()/TrimStart() slicing
// and span indexing. The Mixed stream is a diverse set of 24 real colors so results reflect a real
// workload rather than one repeated input.
//
// RESULTS (net10.0, Apple M3 Pro, BenchmarkDotNet 0.13.5), ns per parse over the Mixed stream,
// two runs:
//
// | Variant     | Run 1 | Run 2 | Notes                                            |
// |-------------|------:|------:|--------------------------------------------------|
// | StringIndex |  7.07 |  7.11 | manual index + string[i]                          |
// | SpanTrim    |  6.69 |  6.94 | ~2-3% faster, no allocations (ADOPTED)           |
//
// The span variant is consistently a hair faster (~0.2 ns / ~2-3%). The likely reason is bounds-check
// elimination: after `switch (s.Length) { case 8: ... }` the JIT knows s.Length == 8 and can prove
// s[0]..s[7] are all in range, so it drops the per-access bounds checks. With the string variant the
// index is hexString[start + k] where `start` is a runtime value, so the JIT keeps the checks.
//
// Based on this, SKColor.TryParse was moved to the span form and now also exposes public
// TryParse(ReadOnlySpan<char>) / Parse(ReadOnlySpan<char>) overloads (the string overloads delegate
// via AsSpan()). The old #if guard only existed because that path also called the byte/uint.TryParse
// span overloads (netstandard2.1+); this trimming-only span code compiles on every TFM.
[MemoryDiagnoser]
public class SKColorParseSpanBenchmark
{
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

			Check("StringIndex", ParseStringIndex(v, out var sc), sc, shippedOk, e, v);
			Check("SpanTrim", ParseSpanTrim(v, out var pc), pc, shippedOk, e, v);
		}
	}

	private static void Check(string name, bool ok, uint got, bool shippedOk, uint expected, string input)
	{
		if (ok != shippedOk)
			throw new InvalidOperationException($"{name} accept/reject mismatch on '{input ?? "<null>"}' (variant={ok}, shipped={shippedOk})");
		if (ok && got != expected)
			throw new InvalidOperationException($"{name} value mismatch on '{input}': got {got:X8} want {expected:X8}");
	}

	[Benchmark(Baseline = true, OperationsPerInvoke = MixedCount)]
	public uint StringIndex()
	{
		uint acc = 0;
		foreach (var s in Mixed)
		{
			ParseStringIndex(s, out var c);
			acc ^= c;
		}
		return acc;
	}

	[Benchmark(OperationsPerInvoke = MixedCount)]
	public uint SpanTrim()
	{
		uint acc = 0;
		foreach (var s in Mixed)
		{
			ParseSpanTrim(s, out var c);
			acc ^= c;
		}
		return acc;
	}

	// Variant A: index into the string with manually tracked offsets (the shipped approach).
	private static bool ParseStringIndex(string hexString, out uint color)
	{
		color = 0;
		if (string.IsNullOrEmpty(hexString))
			return false;

		var start = 0;
		var end = hexString.Length;
		while (start < end && char.IsWhiteSpace(hexString[start])) start++;
		while (end > start && char.IsWhiteSpace(hexString[end - 1])) end--;
		while (start < end && hexString[start] == '#') start++;

		switch (end - start)
		{
			case 3:
				if (!N(hexString[start], out var r3) || !N(hexString[start + 1], out var g3) || !N(hexString[start + 2], out var b3))
					return false;
				color = 0xFF000000u | (uint)((r3 << 4 | r3) << 16 | (g3 << 4 | g3) << 8 | (b3 << 4 | b3));
				return true;
			case 4:
				if (!N(hexString[start], out var a4) || !N(hexString[start + 1], out var r4) || !N(hexString[start + 2], out var g4) || !N(hexString[start + 3], out var b4))
					return false;
				color = (uint)((a4 << 4 | a4) << 24 | (r4 << 4 | r4) << 16 | (g4 << 4 | g4) << 8 | (b4 << 4 | b4));
				return true;
			case 6:
				if (!B(hexString[start], hexString[start + 1], out var r6) || !B(hexString[start + 2], hexString[start + 3], out var g6) || !B(hexString[start + 4], hexString[start + 5], out var b6))
					return false;
				color = 0xFF000000u | (uint)((r6 << 16) | (g6 << 8) | b6);
				return true;
			case 8:
				if (!B(hexString[start], hexString[start + 1], out var a8) || !B(hexString[start + 2], hexString[start + 3], out var r8) || !B(hexString[start + 4], hexString[start + 5], out var g8) || !B(hexString[start + 6], hexString[start + 7], out var b8))
					return false;
				color = (uint)((a8 << 24) | (r8 << 16) | (g8 << 8) | b8);
				return true;
			default:
				return false;
		}
	}

	// Variant B: trim with spans, then index the span (the old NETCOREAPP3_1/NETSTANDARD2_1 shape).
	private static bool ParseSpanTrim(string hexString, out uint color)
	{
		color = 0;
		if (string.IsNullOrEmpty(hexString))
			return false;

		var s = hexString.AsSpan().Trim().TrimStart('#');

		switch (s.Length)
		{
			case 3:
				if (!N(s[0], out var r3) || !N(s[1], out var g3) || !N(s[2], out var b3))
					return false;
				color = 0xFF000000u | (uint)((r3 << 4 | r3) << 16 | (g3 << 4 | g3) << 8 | (b3 << 4 | b3));
				return true;
			case 4:
				if (!N(s[0], out var a4) || !N(s[1], out var r4) || !N(s[2], out var g4) || !N(s[3], out var b4))
					return false;
				color = (uint)((a4 << 4 | a4) << 24 | (r4 << 4 | r4) << 16 | (g4 << 4 | g4) << 8 | (b4 << 4 | b4));
				return true;
			case 6:
				if (!B(s[0], s[1], out var r6) || !B(s[2], s[3], out var g6) || !B(s[4], s[5], out var b6))
					return false;
				color = 0xFF000000u | (uint)((r6 << 16) | (g6 << 8) | b6);
				return true;
			case 8:
				if (!B(s[0], s[1], out var a8) || !B(s[2], s[3], out var r8) || !B(s[4], s[5], out var g8) || !B(s[6], s[7], out var b8))
					return false;
				color = (uint)((a8 << 24) | (r8 << 16) | (g8 << 8) | b8);
				return true;
			default:
				return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool N(char c, out int value)
	{
		if (c >= '0' && c <= '9') { value = c - '0'; return true; }
		if (c >= 'a' && c <= 'f') { value = c - 'a' + 10; return true; }
		if (c >= 'A' && c <= 'F') { value = c - 'A' + 10; return true; }
		value = 0;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool B(char hi, char lo, out int value)
	{
		if (N(hi, out var h) && N(lo, out var l)) { value = h << 4 | l; return true; }
		value = 0;
		return false;
	}
}

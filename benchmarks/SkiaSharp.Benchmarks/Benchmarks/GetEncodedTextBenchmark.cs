using System;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// StringUtilities.GetEncodedText(text, encoding, addNull: true) is on the marshalling path for
// every string SkiaSharp hands to native as a NUL-terminated UTF-8/16/32 buffer — SKString
// construction, SKCanvas.DrawAnnotation / DrawUrlAnnotation, SKFontManager family lookups, and
// file-path stream constructors all go through it.
//
// The shipped ("Old") implementation appended the null terminator by string concatenation
// (`text += "\0"`), which allocates a brand-new managed string (heap object + full char copy)
// purely so the subsequent encode would emit the trailing zero bytes. The "New" implementation
// encodes the original text directly into a byte[] sized `byteCount + nullBytes`, leaving the
// trailing bytes zero — identical output (a '\0' encodes to exactly `GetCharacterByteSize`
// zero bytes in each encoding) with one fewer allocation and one fewer copy.
//
// Both variants below are the verbatim Old/New bodies; they produce byte-for-byte identical
// results (proven by StringUtilitiesTest in tests/Tests). Return the byte[] as a sink so the JIT
// cannot fold the work away.
//
// RESULTS (net10.0, AMD EPYC 7763, BenchmarkDotNet 0.15.8), UTF-8 with addNull, two runs agree:
//
//   | Text                | Old      | New      | Ratio | Old Alloc | New Alloc |
//   |-------------------- |---------:|---------:|------:|----------:|----------:|
//   | Arial               | 23.38 ns | 15.77 ns |  0.67 |      72 B |      32 B |
//   | Segoe UI Symbol     | 26.48 ns | 18.85 ns |  0.71 |      96 B |      40 B |
//   | https(...)/link[35] | 31.25 ns | 21.14 ns |  0.68 |     160 B |      64 B |
//
// ~30% faster and ~55-60% fewer bytes allocated, no allocation regression.
[MemoryDiagnoser]
public class GetEncodedTextBenchmark
{
	[Params("Arial", "Segoe UI Symbol", "https://example.com/annotation/link")]
	public string Text { get; set; }

	private const string NullTerminator = "\0";

	[Benchmark(Baseline = true)]
	public byte[] Old() => OldGetEncodedText(Text, SKTextEncoding.Utf8, true);

	[Benchmark]
	public byte[] New() => StringUtilities.GetEncodedText(Text, SKTextEncoding.Utf8, true);

	// The current-at-time-of-writing shipped body, kept verbatim as the baseline.
	private static byte[] OldGetEncodedText(string text, SKTextEncoding encoding, bool addNull)
	{
		if (!string.IsNullOrEmpty(text) && addNull)
			text += NullTerminator;

		return OldEncode(text.AsSpan(), encoding);
	}

	private static unsafe byte[] OldEncode(ReadOnlySpan<char> text, SKTextEncoding encoding)
	{
		if (text.Length == 0)
			return new byte[0];

		var enc = encoding switch {
			SKTextEncoding.Utf8 => Encoding.UTF8,
			SKTextEncoding.Utf16 => Encoding.Unicode,
			SKTextEncoding.Utf32 => Encoding.UTF32,
			_ => throw new ArgumentOutOfRangeException(nameof(encoding)),
		};

		fixed (char* t = text) {
			var byteCount = enc.GetByteCount(t, text.Length);
			if (byteCount == 0)
				return new byte[0];
			var bytes = new byte[byteCount];
			fixed (byte* b = bytes)
				enc.GetBytes(t, text.Length, b, byteCount);
			return bytes;
		}
	}
}

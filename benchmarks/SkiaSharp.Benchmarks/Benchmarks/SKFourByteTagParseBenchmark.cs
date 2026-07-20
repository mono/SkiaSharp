using System;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// New vs Old for SKFourByteTag.Parse(string).
//
// Old = the current shipped implementation (allocates a char[4] scratch buffer per call).
// New = the proposed managed fast path (no allocation; reads chars directly).
//
// The real caller shape is font/OpenType tag hydration: feature tags ("liga", "kern"),
// variation-axis tags ("wght", "wdth", "slnt"), and table tags parsed from strings, often a
// batch of many at once when configuring a font. This benchmark parses a representative batch
// of such tags (plus null/empty/short/over-long edges) so both the happy path and the padding
// branches are exercised.
[MemoryDiagnoser]
public class SKFourByteTagParseBenchmark
{
	private string[] tags;

	[GlobalSetup]
	public void GlobalSetup()
	{
		// A realistic mix of 4-char tags, plus edge shapes that hit the padding/truncation code.
		tags = new[]
		{
			"liga", "kern", "wght", "wdth", "slnt", "ital", "opsz", "GSUB",
			"GPOS", "cmap", "head", "name", "OS/2", "post", "glyf", "loca",
			"a",    "bc",   "def",  "toolong", "", null,
		};
	}

	[Benchmark(Baseline = true)]
	public uint Old()
	{
		uint sink = 0;
		foreach (var t in tags)
			sink ^= OldParse(t);
		return sink;
	}

	[Benchmark]
	public uint New()
	{
		uint sink = 0;
		foreach (var t in tags)
			sink ^= (uint)SKFourByteTag.Parse(t);
		return sink;
	}

	// Verbatim copy of the ORIGINAL shipped Parse(string) body, so the ratio is honest.
	private static uint OldParse(string tag)
	{
		if (string.IsNullOrEmpty(tag))
			return 0;

		var realTag = new char[4];
		var len = Math.Min(4, tag.Length);
		var i = 0;
		for (; i < len; i++)
			realTag[i] = tag[i];
		for (; i < 4; i++)
			realTag[i] = ' ';

		return (uint)(((byte)realTag[0] << 24) | ((byte)realTag[1] << 16) | ((byte)realTag[2] << 8) | (byte)realTag[3]);
	}
}

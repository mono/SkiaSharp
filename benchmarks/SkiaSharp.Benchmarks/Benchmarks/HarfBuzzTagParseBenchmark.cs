using System;
using BenchmarkDotNet.Attributes;
using HarfBuzzSharp;

namespace SkiaSharp.Benchmarks;

// New vs Old for HarfBuzzSharp.Tag.Parse(string).
//
// Old = the current shipped implementation (allocates a char[4] scratch buffer per call).
// New = the proposed managed fast path (no allocation; reads chars directly), mirroring the
//       already-optimized SkiaSharp.SKFourByteTag.Parse.
//
// The real caller shape is OpenType tag hydration: feature tags ("liga", "kern"), variation-axis
// tags ("wght", "wdth", "slnt") and table tags parsed from strings when configuring fonts and
// shaping features — often a batch of many at once. This benchmark parses a representative batch
// (plus null/empty/short/over-long edges) so both the happy path and the padding branches run.
[MemoryDiagnoser]
public class HarfBuzzTagParseBenchmark
{
	private string[] tags;

	[GlobalSetup]
	public void GlobalSetup()
	{
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
		for (var rep = 0; rep < 100; rep++)
			foreach (var t in tags)
				sink ^= OldParse(t);
		return sink;
	}

	[Benchmark]
	public uint New()
	{
		uint sink = 0;
		for (var rep = 0; rep < 100; rep++)
			foreach (var t in tags)
				sink ^= (uint)Tag.Parse(t);
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

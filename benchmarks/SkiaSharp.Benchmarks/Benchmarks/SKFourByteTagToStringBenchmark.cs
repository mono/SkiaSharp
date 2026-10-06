using System;
using BenchmarkDotNet.Attributes;
using SkiaSharp;

namespace SkiaSharp.Benchmarks;

// New vs Old for SKFourByteTag.ToString() (and, identically, HarfBuzzSharp.Tag.ToString()'s
// general path).
//
// Old = the current shipped implementation: string.Concat(char, char, char, char). There is no
//       string.Concat(char, char, char, char) overload, so the call binds to
//       Concat(object, object, object, object) and BOXES every char — four extra heap allocations
//       per call on top of the produced string.
// New = write the four chars into a stackalloc'd buffer and materialize the string once
//       (new string(char*, 0, 4)) — no boxing, identical result on every TFM.
//
// The real caller shape is OpenType tag formatting: variation-axis / feature / table tags rendered
// back to their 4-char text ("wght", "liga", "GSUB", ...) when inspecting typefaces, logging, or
// serializing font state — often a batch at once. This benchmark formats a representative batch so
// the ratio and the allocation delta are both visible.
[MemoryDiagnoser]
public class SKFourByteTagToStringBenchmark
{
	private uint[] tags;

	[GlobalSetup]
	public void GlobalSetup()
	{
		tags = new uint[]
		{
			0x77676874, // wght
			0x77647468, // wdth
			0x736C6E74, // slnt
			0x6C696761, // liga
			0x6B65726E, // kern
			0x47535542, // GSUB
			0x47504F53, // GPOS
			0x636D6170, // cmap
			0x68656164, // head
			0x6E616D65, // name
			0x4F532F32, // OS/2
			0x706F7374, // post
		};
	}

	[Benchmark(Baseline = true)]
	public int Old()
	{
		var sink = 0;
		for (var rep = 0; rep < 100; rep++)
			foreach (var t in tags)
				sink += OldToString(t).Length;
		return sink;
	}

	[Benchmark]
	public int New()
	{
		var sink = 0;
		for (var rep = 0; rep < 100; rep++)
			foreach (var t in tags)
				sink += ((SKFourByteTag)t).ToString().Length;
		return sink;
	}

	// Verbatim copy of the ORIGINAL shipped ToString() body, so the ratio is honest.
	private static string OldToString(uint value) =>
		string.Concat(
			(char)(byte)(value >> 24),
			(char)(byte)(value >> 16),
			(char)(byte)(value >> 8),
			(char)(byte)value);
}

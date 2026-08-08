using System;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// SKRuntimeEffectUniforms.Add (reached via the uniforms["name"] = value indexer used by
// SKRuntimeShaderBuilder) is the per-frame hot path for animated SkSL shaders: every uniform is
// re-pushed once per frame (iTime, iResolution, params, ...), so an effect with U uniforms rendered
// at 60fps calls Add U*60 times per second.
//
// The shipped ("New") Add resolves the uniform with a single Dictionary.TryGetValue. The previous
// ("Old") implementation did an Array.IndexOf(names, name) linear string scan (only to produce a
// friendly error) AND then a separate uniforms[name] dictionary lookup — two string-comparison
// passes to obtain one Variable. This benchmark re-implements a minimal uniforms map so both paths
// are measured in one process, isolating the lookup cost. Inputs cover realistic uniform counts and
// a name that lives near the end of the declaration array (worst case for the O(n) IndexOf scan).
[MemoryDiagnoser]
public class SKRuntimeEffectUniformsAddBenchmark
{
	// Number of declared uniforms (a small typical shader vs a large parameterised one).
	[Params(4, 16, 48)]
	public int UniformCount { get; set; }

	private string[] names;
	private System.Collections.Generic.Dictionary<string, int> map;
	private string lastName;

	[GlobalSetup]
	public void GlobalSetup()
	{
		names = new string[UniformCount];
		map = new System.Collections.Generic.Dictionary<string, int>(UniformCount);
		for (var i = 0; i < UniformCount; i++)
		{
			names[i] = $"uniform_{i}";
			map[names[i]] = i;
		}
		// Worst case for the linear IndexOf scan: the last declared uniform.
		lastName = names[UniformCount - 1];
	}

	// New: the shipped path — a single dictionary lookup resolves the uniform and detects a miss.
	[Benchmark]
	public int New()
	{
		if (!map.TryGetValue(lastName, out var index))
			throw new ArgumentOutOfRangeException(lastName);
		return index;
	}

	// Old: the previous path — Array.IndexOf linear scan for the miss-check, then a dictionary lookup.
	[Benchmark(Baseline = true)]
	public int Old()
	{
		var idx = Array.IndexOf(names, lastName);
		if (idx == -1)
			throw new ArgumentOutOfRangeException(lastName);
		var index = map[lastName];
		return index;
	}
}

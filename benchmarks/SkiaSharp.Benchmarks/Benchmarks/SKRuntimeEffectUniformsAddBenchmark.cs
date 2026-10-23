using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// Measures the managed lookup overhead of SKRuntimeEffectUniforms.Add — the method that runs every
// time a caller assigns a uniform (uniforms["name"] = value). Animated runtime-effect shaders set
// ALL their uniforms every frame (time, resolution, mouse, per-object params, ...), so this is a
// realistic per-frame hot path: N assignments per frame.
//
// The CURRENT shipped Add (binding/SkiaSharp/SKRuntimeEffect.cs) resolves the SAME name twice:
//   1. Array.IndexOf(names, name)  — an O(n) LINEAR scan whose only purpose is the "not found" check
//   2. uniforms[name]              — a second (hash) lookup that fetches the Variable
// The name->Variable dictionary already answers BOTH questions, so a single TryGetValue replaces the
// linear scan AND the redundant hashing.
//
// This benchmark mirrors Add's exact internals: the same names[] array, an equivalent
// name->offset map, and the same "validate then write into the packed buffer" tail. Old reproduces
// the double lookup; New uses one TryGetValue. Everything after the lookup is identical, so the
// ratio isolates the lookup layout on the real caller path.
[MemoryDiagnoser]
public class SKRuntimeEffectUniformsAddBenchmark
{
	// How many uniforms a shader declares (and the caller sets each frame).
	[Params(8, 32)]
	public int N { get; set; }

	private string[] names;
	private Dictionary<string, int> offsets;   // mirrors the name->Variable map (offset is what Add reads)
	private byte[] buffer;

	[GlobalSetup]
	public void GlobalSetup ()
	{
		names = new string[N];
		offsets = new Dictionary<string, int> (N);
		buffer = new byte[N * sizeof (float)];
		for (var i = 0; i < N; i++) {
			names[i] = $"uniform_{i}";
			offsets[names[i]] = i * sizeof (float);
		}
	}

	// Old: Array.IndexOf (linear) for existence + indexer (hash) to fetch — the current shipped shape.
	[Benchmark(Baseline = true)]
	public int Old ()
	{
		var sink = 0;
		for (var frame = 0; frame < N; frame++) {
			for (var i = 0; i < names.Length; i++) {
				var name = names[i];

				var index = Array.IndexOf (names, name);
				if (index == -1)
					throw new ArgumentOutOfRangeException (name);

				var offset = offsets[name];
				Write (offset, i + 0.5f);
				sink += offset;
			}
		}
		return sink;
	}

	// New: one TryGetValue answers existence AND fetches the value — no linear scan, no rehash.
	[Benchmark]
	public int New ()
	{
		var sink = 0;
		for (var frame = 0; frame < N; frame++) {
			for (var i = 0; i < names.Length; i++) {
				var name = names[i];

				if (!offsets.TryGetValue (name, out var offset))
					throw new ArgumentOutOfRangeException (name);

				Write (offset, i + 0.5f);
				sink += offset;
			}
		}
		return sink;
	}

	private void Write (int offset, float value) =>
		BitConverter.TryWriteBytes (buffer.AsSpan (offset, sizeof (float)), value);
}

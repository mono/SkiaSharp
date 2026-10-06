using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// Measures the cost of building the per-uniform lookup map inside the
// SKRuntimeEffectUniforms constructor (binding/SkiaSharp/SKRuntimeEffect.cs:263-280).
//
// The constructor knows the exact final count up front (names.Length) yet creates the
// Dictionary<string, Variable> with the default (zero) capacity, so it rehashes several
// times as the loop fills it with names.Length entries. Old = current (default sizing);
// New = pre-sized to the known count. Same job/TFM ⇒ honest Ratio + Allocated.
[MemoryDiagnoser]
public class SKRuntimeEffectUniformsDictionaryBenchmark
{
	// A value type the same shape/size as the internal SKRuntimeEffectUniforms.Variable
	// (5 ints + a string reference) so the entry array cost matches the real one.
	private readonly struct Variable
	{
		public Variable (int index, string name)
		{
			Index = index;
			Name = name;
			Offset = index * 4;
			Type = index & 0x0F;
			Count = 1;
			Flags = 0;
		}

		public int Index { get; }
		public string Name { get; }
		public int Offset { get; }
		public int Type { get; }
		public int Count { get; }
		public int Flags { get; }
	}

	// Uniform counts real shaders hit: a couple of controls up to a busy effect.
	[Params(4, 16, 48)]
	public int N { get; set; }

	private string[] names;

	[GlobalSetup]
	public void GlobalSetup ()
	{
		names = new string[N];
		for (var i = 0; i < N; i++)
			names[i] = "uniform_" + i;
	}

	[Benchmark (Baseline = true)]
	public int Old ()
	{
		var uniforms = new Dictionary<string, Variable> ();
		for (var i = 0; i < names.Length; i++) {
			var name = names[i];
			uniforms[name] = new Variable (i, name);
		}
		return uniforms.Count;
	}

	[Benchmark]
	public int New ()
	{
		var uniforms = new Dictionary<string, Variable> (names.Length);
		for (var i = 0; i < names.Length; i++) {
			var name = names[i];
			uniforms[name] = new Variable (i, name);
		}
		return uniforms.Count;
	}
}

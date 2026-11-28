using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

[MemoryDiagnoser]
public class SKRuntimeEffectJaggedUniformBenchmark
{
	[Params(4, 16, 64)]
	public int Rows { get; set; }

	private float[][] values;

	[GlobalSetup]
	public void GlobalSetup()
	{
		values = new float[Rows][];
		for (var row = 0; row < Rows; row++)
			values[row] = new[] { row + 0.25f, row + 0.5f, row + 0.75f, row + 1f };
	}

	[Benchmark(Baseline = true)]
	public int Old()
	{
		var floats = new List<float>();
		foreach (var array in values)
			floats.AddRange(array);

		SKRuntimeEffectUniform uniform = floats.ToArray();
		return uniform.Size;
	}

	[Benchmark]
	public int New()
	{
		var length = 0;
		foreach (var array in values)
		{
			if (array == null)
				throw new System.ArgumentNullException("collection");
			length += array.Length;
		}

		var floats = new float[length];
		var offset = 0;
		foreach (var array in values)
		{
			array.CopyTo(floats, offset);
			offset += array.Length;
		}

		SKRuntimeEffectUniform uniform = floats;
		return uniform.Size;
	}
}

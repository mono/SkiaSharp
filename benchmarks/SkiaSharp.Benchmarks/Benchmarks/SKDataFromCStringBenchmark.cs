using System;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

[MemoryDiagnoser]
public class SKDataFromCStringBenchmark
{
	public static object[] Values => [
		null,
		"",
		"https://example.com",
		"https://example.com/path/to/a/document?query=annotation#destination",
	];

	[ParamsSource(nameof(Values))]
	public string Value { get; set; }

	[Benchmark(Baseline = true)]
	public long Old()
	{
		var bytes = Encoding.ASCII.GetBytes((Value ?? string.Empty) + '\0');
		using var data = SKData.CreateCopy(bytes);
		return data.Size;
	}

	[Benchmark]
	public long ByteArray()
	{
		var span = Value.AsSpan();
		var bytes = new byte[Encoding.ASCII.GetByteCount(span) + 1];
		Encoding.ASCII.GetBytes(span, bytes);
		using var data = SKData.CreateCopy(bytes);
		return data.Size;
	}

	[Benchmark]
	public long New()
	{
		using var data = SKData.FromCString(Value);
		return data.Size;
	}
}

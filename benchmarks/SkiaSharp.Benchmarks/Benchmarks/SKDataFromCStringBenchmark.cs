using System.Text;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

[MemoryDiagnoser]
public class SKDataFromCStringBenchmark
{
	[Params("https://example.com", "https://example.com/path/to/a/document?query=annotation#destination")]
	public string Value { get; set; }

	[Benchmark(Baseline = true)]
	public byte[] Old()
	{
		return Encoding.ASCII.GetBytes(Value + '\0');
	}

	[Benchmark]
	public byte[] New()
	{
		var bytes = new byte[Encoding.ASCII.GetByteCount(Value) + 1];
		Encoding.ASCII.GetBytes(Value, 0, Value.Length, bytes, 0);
		return bytes;
	}
}

using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using HBBuffer = HarfBuzzSharp.Buffer;

namespace SkiaSharp.Benchmarks;

// Buffer.AddUtf32(string) is used by SKShaper's legacy UTF-32 text path. The previous
// implementation allocated Encoding.UTF32.GetBytes(string) for every shaped run. The shipped
// implementation encodes into a pooled buffer before making the same hb_buffer_add_utf32 call.
//
// RESULTS (net10.0, AMD EPYC 7763, BenchmarkDotNet 0.13.5), two runs:
//
//   | Text        | Old       | New       | Ratio   | Old Alloc | New Alloc |
//   |------------ |----------:|----------:|--------:|----------:|----------:|
//   | shaping     | 141-142 ns| 102 ns    | 0.72    |     280 B |     112 B |
//   | ASCII       | 396-399 ns| 277 ns    | 0.69-0.70|     424 B |     112 B |
//   | CJK         | 196 ns    | 140-141 ns| 0.71-0.72|     312 B |     112 B |
//   | mixed/emoji | 623-625 ns| 430-431 ns| 0.69    |     544 B |     112 B |
[Config(typeof(Config))]
[MemoryDiagnoser]
public class HarfBuzzAddUtf32Benchmark
{
	private class Config : ManualConfig
	{
		public Config() =>
			AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance));
	}

	[Params(
		"shaping",
		"The quick brown fox jumps over the lazy dog.",
		"日本語のテキストをシェイプする",
		"The quick brown fox — 素早い茶色の狐 — jumps over the lazy dog café 😀 1234567890.")]
	public string Text { get; set; }

	private HBBuffer buffer;

	[GlobalSetup]
	public void GlobalSetup() => buffer = new HBBuffer();

	[GlobalCleanup]
	public void GlobalCleanup() => buffer.Dispose();

	[Benchmark(Baseline = true)]
	public int Old()
	{
		buffer.ClearContents();
		buffer.AddUtf32(Encoding.UTF32.GetBytes(Text));
		return buffer.Length;
	}

	[Benchmark]
	public int New()
	{
		buffer.ClearContents();
		buffer.AddUtf32(Text);
		return buffer.Length;
	}
}

using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using HBBuffer = HarfBuzzSharp.Buffer;

namespace SkiaSharp.Benchmarks;

// HarfBuzzSharp.Buffer.AddUtf8(string) feeds text into a HarfBuzz buffer for shaping. It is on the
// canonical shaping hot path: SKShaper.Shape(string) -> buffer.AddUtf8(text) -> SKCanvas.DrawShapedText,
// so it runs once per shaped run / per frame for any RTL/complex-script text.
//
// The shipped ("New") implementation encodes the UTF-8 bytes into a pooled ArrayPool<byte> buffer and
// passes the pointer straight to hb_buffer_add_utf8 — zero managed allocation. The previous ("Old")
// implementation, reproduced verbatim below as the baseline, allocated a throwaway byte[] via
// Encoding.UTF8.GetBytes(string) on every call. Both are measured in one process/TFM so the Ratio and
// the Allocated column are honest; the win shows up first in Allocated (byte[] -> 0 B).
//
// One reused Buffer is cleared with ClearContents() (a native reset, no managed allocation) between
// adds so we measure AddUtf8 encoding+marshalling, not unbounded buffer growth. Inputs cover a short
// word, an ASCII sentence, multibyte CJK, and a longer mixed paragraph, since the byte count (and thus
// the size of the removed allocation) scales with the text.
[MemoryDiagnoser]
public class HarfBuzzAddUtf8Benchmark
{
	[Params(
		"shaping",
		"The quick brown fox jumps over the lazy dog.",
		"日本語のテキストをシェイプする",
		"The quick brown fox — 素早い茶色の狐 — jumps over the lazy dog café 1234567890.")]
	public string Text { get; set; }

	private HBBuffer buffer;

	[GlobalSetup]
	public void GlobalSetup() => buffer = new HBBuffer();

	[GlobalCleanup]
	public void GlobalCleanup() => buffer.Dispose();

	// New: the shipped AddUtf8(string) — encodes into a pooled buffer, zero managed allocation.
	[Benchmark]
	public int New()
	{
		buffer.ClearContents();
		buffer.AddUtf8(Text);
		return buffer.Length;
	}

	// Old: the previous implementation, verbatim —
	//   AddUtf8(string s) => AddUtf8(Encoding.UTF8.GetBytes(s), 0, -1);
	// allocates a throwaway byte[] on every call.
	[Benchmark(Baseline = true)]
	public int Old()
	{
		buffer.ClearContents();
		var bytes = Encoding.UTF8.GetBytes(Text);
		buffer.AddUtf8(new ReadOnlySpan<byte>(bytes), 0, -1);
		return buffer.Length;
	}
}

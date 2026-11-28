using System;
using System.IO;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using HarfBuzzSharp;
using Buffer = HarfBuzzSharp.Buffer;

namespace SkiaSharp.Benchmarks;

// SKShaper.Shape(string, ...) runs once per shaped text run and currently creates and disposes a
// HarfBuzz buffer on every call, despite SKShaper already owning a buffer for its lifetime. This
// benchmark compares that shipped lifecycle with resetting and reusing the owned buffer. Both paths
// perform the same text ingestion, property guessing, and native shaping.
[Config(typeof(Config))]
[MemoryDiagnoser]
public class SKShaperBufferReuseBenchmark
{
	private class Config : ManualConfig
	{
		public Config() =>
			AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance));
	}

	private const string ArabicText = "متن";
	private const string EnglishText = "The quick brown fox jumps over the lazy dog.";

	private Blob blob;
	private Face face;
	private Font font;
	private Buffer reusableBuffer;

	[GlobalSetup]
	public void GlobalSetup()
	{
		var fontBytes = LoadFont();
		blob = Blob.FromStream(new MemoryStream(fontBytes));
		face = new Face(blob, 0);
		font = new Font(face);
		font.SetScale(512, 512);
		font.SetFunctionsOpenType();
		reusableBuffer = new Buffer();
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		reusableBuffer.Dispose();
		font.Dispose();
		face.Dispose();
		blob.Dispose();
	}

	[Benchmark(Baseline = true)]
	public int Old()
	{
		return ShapeWithNewBuffer(ArabicText) + ShapeWithNewBuffer(EnglishText);
	}

	[Benchmark]
	public int New()
	{
		return ShapeWithReusableBuffer(ArabicText) + ShapeWithReusableBuffer(EnglishText);
	}

	private int ShapeWithNewBuffer(string text)
	{
		using var buffer = new Buffer();
		buffer.AddUtf8(text);
		buffer.GuessSegmentProperties();
		font.Shape(buffer);
		return buffer.Length;
	}

	private int ShapeWithReusableBuffer(string text)
	{
		reusableBuffer.Reset();
		reusableBuffer.AddUtf8(text);
		reusableBuffer.GuessSegmentProperties();
		font.Shape(reusableBuffer);
		return reusableBuffer.Length;
	}

	private static byte[] LoadFont()
	{
		var assembly = typeof(SKShaperBufferReuseBenchmark).Assembly;
		using var stream = assembly.GetManifestResourceStream("content-font.ttf")
			?? throw new InvalidOperationException("Embedded font 'content-font.ttf' was not found.");
		using var memory = new MemoryStream();
		stream.CopyTo(memory);
		return memory.ToArray();
	}
}

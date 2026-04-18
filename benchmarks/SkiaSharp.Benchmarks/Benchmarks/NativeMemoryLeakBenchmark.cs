#if OS_WINDOWS

using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace SkiaSharp.Benchmarks;

/// <summary>
/// Windows-only benchmark using ETW NativeMemoryProfiler.
/// Reports "Allocated native memory" and "Native memory leak" columns.
/// This definitively shows whether Skia native allocations are freed by GC.
/// </summary>
[MemoryDiagnoser]
[NativeMemoryProfiler]
[HtmlExporter]
[MarkdownExporterAttribute.GitHub]
[JsonExporterAttribute.Full]
[ShortRunJob]
public class NativeMemoryLeakBenchmark
{
	private string imagePath = null!;

	[GlobalSetup]
	public void Setup()
	{
		imagePath = Path.Combine(Path.GetTempPath(), "skiasharp-native-bench.png");
		using var bitmap = new SKBitmap(100, 100);
		using var image = SKImage.FromBitmap(bitmap);
		using var data = image.Encode(SKEncodedImageFormat.Png, 80);
		using var stream = File.OpenWrite(imagePath);
		data.SaveTo(stream);
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		if (File.Exists(imagePath))
			File.Delete(imagePath);
	}

	[Benchmark(Baseline = true)]
	public byte[] DecodeWithDisposal()
	{
		using var bitmap = SKBitmap.Decode(imagePath);
		using var scaled = new SKBitmap(60, 40, bitmap.ColorType, bitmap.AlphaType);
		bitmap.ScalePixels(scaled, new SKSamplingOptions(SKCubicResampler.Mitchell));
		using var image = SKImage.FromBitmap(scaled);
		using var data = image.Encode(SKEncodedImageFormat.Png, 80);
		var ms = new MemoryStream();
		data.SaveTo(ms);
		return ms.ToArray();
	}

	[Benchmark]
	public byte[] DecodeWithoutDisposal()
	{
		var bitmap = SKBitmap.Decode(imagePath);
		var scaled = new SKBitmap(60, 40, bitmap.ColorType, bitmap.AlphaType);
		bitmap.ScalePixels(scaled, new SKSamplingOptions(SKCubicResampler.Mitchell));
		var image = SKImage.FromBitmap(scaled);
		var data = image.Encode(SKEncodedImageFormat.Png, 80);
		var ms = new MemoryStream();
		data.SaveTo(ms);
		return ms.ToArray();
	}
}

#endif

using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;

namespace SkiaSharp.Benchmarks;

/// <summary>
/// Benchmarks that measure GC reclamation of undisposed Skia objects.
/// These mirror the patterns in SKBitmapThreadingTest and SKObjectTest
/// to determine whether native memory is properly freed by finalizers.
/// </summary>
[MemoryDiagnoser]
[HtmlExporter]
[MarkdownExporterAttribute.GitHub]
[JsonExporterAttribute.Full]
[ShortRunJob]
public class GCReclamationBenchmark
{
	private string imagePath = null!;

	[GlobalSetup]
	public void Setup()
	{
		// Create a small test image to decode
		imagePath = Path.Combine(Path.GetTempPath(), "skiasharp-bench.png");
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

	/// <summary>
	/// Baseline: decode + dispose immediately. No GC pressure.
	/// </summary>
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

	/// <summary>
	/// No disposal — relies entirely on GC/finalizers.
	/// This is the pattern from SKBitmapThreadingTest.ComputeThumbnail.
	/// Compare Gen0/Gen1/Gen2/Allocated with the disposed version.
	/// </summary>
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

	/// <summary>
	/// Threaded version without disposal — simulates the CI test scenario.
	/// Measures whether concurrent undisposed objects cause memory growth.
	/// </summary>
	[Benchmark]
	[Arguments(10)]
	[Arguments(100)]
	public void ThreadedDecodeWithoutDisposal(int threadCount)
	{
		var tasks = new Task[threadCount];
		for (int i = 0; i < threadCount; i++)
		{
			tasks[i] = Task.Run(() =>
			{
				var bitmap = SKBitmap.Decode(imagePath);
				var scaled = new SKBitmap(60, 40, bitmap.ColorType, bitmap.AlphaType);
				bitmap.ScalePixels(scaled, new SKSamplingOptions(SKCubicResampler.Mitchell));
				var image = SKImage.FromBitmap(scaled);
				var data = image.Encode(SKEncodedImageFormat.Png, 80);
				var ms = new MemoryStream();
				data.SaveTo(ms);
			});
		}
		Task.WaitAll(tasks);
	}
}

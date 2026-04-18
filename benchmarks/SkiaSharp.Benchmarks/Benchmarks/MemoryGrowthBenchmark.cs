using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace SkiaSharp.Benchmarks;

/// <summary>
/// Manual memory growth test that tracks native memory (working set) and
/// HandleDictionary size across batches of undisposed Skia object creation.
///
/// Unlike MemoryDiagnoser (which only sees managed allocations), this directly
/// measures whether native memory from Skia is reclaimed by GC finalizers.
///
/// Run:  dotnet run -c Release -- --filter "*MemoryGrowth*"
///   or: dotnet run -c Release -f net48 -p:Platform=x86 -- --filter "*MemoryGrowth*"
/// </summary>
[HtmlExporter]
[MarkdownExporterAttribute.GitHub]
[JsonExporterAttribute.Full]
[SimpleJob(RuntimeMoniker.HostProcess, iterationCount: 1, warmupCount: 0, invocationCount: 1)]
public class MemoryGrowthBenchmark
{
	private string imagePath = null!;
	private const int BatchSize = 1000;

	[Params(1000, 5000, 10000)]
	public int Iterations { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		imagePath = Path.Combine(Path.GetTempPath(), "skiasharp-memgrowth.png");
		using var bitmap = new SKBitmap(100, 100);
		using var canvas = new SKCanvas(bitmap);
		canvas.Clear(SKColors.CornflowerBlue);
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
	/// Baseline: proper disposal. Memory should be flat.
	/// </summary>
	[Benchmark(Baseline = true)]
	public void WithDisposal()
	{
		RunBatches(dispose: true);
	}

	/// <summary>
	/// No disposal, relying on GC — the pattern from the CI test.
	/// If native memory grows unbounded, this is a leak.
	/// If it stabilizes, GC/finalizers are working.
	/// </summary>
	[Benchmark]
	public void WithoutDisposal()
	{
		RunBatches(dispose: false);
	}

	/// <summary>
	/// No disposal + concurrent threads — the exact CI scenario.
	/// </summary>
	[Benchmark]
	public void ThreadedWithoutDisposal()
	{
		RunBatches(dispose: false, threadCount: 10);
	}

	private void RunBatches(bool dispose, int threadCount = 1)
	{
		ForceGC();
		var proc = Process.GetCurrentProcess();
		proc.Refresh();
		var wsBefore = proc.WorkingSet64;
		int handlesBefore;
		lock (HandleDictionary.instances)
			handlesBefore = HandleDictionary.instances.Count;

		Console.WriteLine($"  [Start] WorkingSet={wsBefore / 1024 / 1024}MB, Handles={handlesBefore}");

		var totalBatches = Math.Max(1, Iterations / BatchSize);
		for (int batch = 0; batch < totalBatches; batch++)
		{
			var batchIters = Math.Min(BatchSize, Iterations - batch * BatchSize);

			if (threadCount == 1)
			{
				for (int i = 0; i < batchIters; i++)
					DecodeOnce(dispose);
			}
			else
			{
				var workerCount = Math.Min(threadCount, batchIters);
				var tasks = new Task[workerCount];
				var perThread = batchIters / workerCount;
				var remainder = batchIters % workerCount;
				for (int t = 0; t < workerCount; t++)
				{
					var iterationsForThread = perThread + (t < remainder ? 1 : 0);
					tasks[t] = Task.Run(() =>
					{
						for (int i = 0; i < iterationsForThread; i++)
							DecodeOnce(dispose);
					});
				}
				Task.WaitAll(tasks);
			}

			ForceGC();

			proc.Refresh();
			int handlesNow;
			lock (HandleDictionary.instances)
				handlesNow = HandleDictionary.instances.Count;

			Console.WriteLine($"  [Batch {batch + 1}/{totalBatches}] WorkingSet={proc.WorkingSet64 / 1024 / 1024}MB, Handles={handlesNow} (delta={handlesNow - handlesBefore})");
		}

		ForceGC();

		proc.Refresh();
		var wsAfter = proc.WorkingSet64;
		int handlesAfter;
		lock (HandleDictionary.instances)
			handlesAfter = HandleDictionary.instances.Count;

		var deltaMB = (wsAfter - wsBefore) / 1024.0 / 1024.0;
		Console.WriteLine($"  [Final] WorkingSet={wsAfter / 1024 / 1024}MB (delta={deltaMB:+0.0;-0.0}MB), Handles={handlesAfter} (delta={handlesAfter - handlesBefore})");
	}

	private void DecodeOnce(bool dispose)
	{
		var bitmap = SKBitmap.Decode(imagePath);
		var scaled = new SKBitmap(60, 40, bitmap.ColorType, bitmap.AlphaType);
		bitmap.ScalePixels(scaled, new SKSamplingOptions(SKCubicResampler.Mitchell));
		var image = SKImage.FromBitmap(scaled);
		var data = image.Encode(SKEncodedImageFormat.Png, 80);
		var ms = new MemoryStream();
		data.SaveTo(ms);

		if (dispose)
		{
			data.Dispose();
			image.Dispose();
			scaled.Dispose();
			bitmap.Dispose();
		}
	}

	private static void ForceGC()
	{
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
		GC.WaitForPendingFinalizers();
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
		GC.WaitForPendingFinalizers();
	}
}

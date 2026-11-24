using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

[MemoryDiagnoser]
public class SKImagePixelCopyBenchmark
{
	[Params(32, 256)]
	public int Size { get; set; }

	private SKImageInfo info;
	private byte[] pixels;

	[GlobalSetup]
	public void GlobalSetup()
	{
		info = new SKImageInfo(Size, Size, SKColorType.Bgra8888, SKAlphaType.Premul);
		pixels = new byte[info.BytesSize];
		for (var i = 0; i < pixels.Length; i++)
			pixels[i] = (byte)i;
	}

	[Benchmark(Baseline = true)]
	public uint Old()
	{
		using var data = SKData.CreateCopy(pixels);
		using var image = SKImage.FromPixels(info, data, info.RowBytes);
		return image.UniqueId;
	}

	[Benchmark]
	public uint New()
	{
		using var image = SKImage.FromPixelCopy(info, pixels, info.RowBytes);
		return image.UniqueId;
	}
}

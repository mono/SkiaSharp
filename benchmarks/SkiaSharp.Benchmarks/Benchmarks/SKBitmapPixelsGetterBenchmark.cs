using System;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

[MemoryDiagnoser]
public class SKBitmapPixelsGetterBenchmark
{
	[Params(32, 256)]
	public int Size { get; set; }

	private SKBitmap bitmap;
	private SKColor[] destination;

	[GlobalSetup]
	public void GlobalSetup()
	{
		bitmap = new SKBitmap(Size, Size, SKColorType.Bgra8888, SKAlphaType.Premul);
		destination = new SKColor[Size * Size];
		for (var i = 0; i < destination.Length; i++)
			destination[i] = new SKColor((byte)i, (byte)(i >> 3), (byte)(i >> 7), (byte)(i | 0x80));
		if (!bitmap.WritePixels(destination))
			throw new InvalidOperationException("Unable to initialize the bitmap's pixels.");
	}

	[GlobalCleanup]
	public void GlobalCleanup() =>
		bitmap.Dispose();

	[Benchmark(Baseline = true)]
	public unsafe SKColor[] Old()
	{
		var info = bitmap.Info;
		var pixels = new SKColor[checked(info.Width * info.Height)];
		fixed (SKColor* p = pixels)
		{
			SkiaApi.sk_bitmap_get_pixel_colors(bitmap.Handle, (uint*)p);
			GC.KeepAlive(bitmap);
		}
		return pixels;
	}

	[Benchmark]
	public SKColor[] New() =>
		bitmap.Pixels;

	[Benchmark]
	public SKColor Span()
	{
		if (!bitmap.ReadPixels(destination))
			throw new InvalidOperationException("Unable to read the bitmap's pixels.");
		return destination[0];
	}
}

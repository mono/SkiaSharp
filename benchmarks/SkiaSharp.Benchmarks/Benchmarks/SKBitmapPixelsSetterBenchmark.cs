using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

[MemoryDiagnoser]
public class SKBitmapPixelsSetterBenchmark
{
	[Params(32, 256)]
	public int Size { get; set; }

	private SKImageInfo info;
	private SKImageInfo sourceInfo;
	private SKColor[] pixels;
	private SKBitmap destination;

	[GlobalSetup]
	public void GlobalSetup()
	{
		info = new SKImageInfo(Size, Size, SKColorType.Bgra8888, SKAlphaType.Premul);
		sourceInfo = new SKImageInfo(Size, Size, SKColorType.Bgra8888, SKAlphaType.Unpremul);
		pixels = new SKColor[Size * Size];
		for (var i = 0; i < pixels.Length; i++)
			pixels[i] = new SKColor((byte)i, (byte)(i >> 3), (byte)(i >> 7), (byte)(i | 0x80));
		destination = new SKBitmap(info);
	}

	[GlobalCleanup]
	public void GlobalCleanup() =>
		destination.Dispose();

	[Benchmark(Baseline = true)]
	public unsafe SKColor Old()
	{
		fixed (SKColor* p = pixels)
		{
			using var temp = new SKBitmap();
			temp.InstallPixels(sourceInfo, (nint)p);

			using var shader = temp.ToShader();
			using var canvas = new SKCanvas(destination);
			using var paint = new SKPaint
			{
				Shader = shader,
				BlendMode = SKBlendMode.Src,
			};
			canvas.DrawPaint(paint);
		}

		return destination.GetPixel(0, 0);
	}

	[Benchmark]
	public SKColor New()
	{
		destination.Pixels = pixels;
		return destination.GetPixel(0, 0);
	}
}

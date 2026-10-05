global using System;
using BenchmarkDotNet.Attributes;
using SkiaSharpSample.ImagePlaceholders;

namespace SkiaSharp.Benchmarks;

// Array input and explicit spans call the same codec method. The adapter rows
// include Skia normalization; caller-buffer rows exclude output allocation.
internal static class PlaceholderBenchmarkSource
{
	public const int BytesPerPixel = 4;
	public const int ComponentsX = 4;
	public const int ComponentsY = 3;

	public static SKBitmap Create(int side, SKAlphaType alphaType)
	{
		using var srgb = SKColorSpace.CreateSrgb();
		var bitmap = new SKBitmap(new SKImageInfo(side, side, SKColorType.Rgba8888, alphaType, srgb));
		for (var y = 0; y < side; y++)
		{
			for (var x = 0; x < side; x++)
			{
				bitmap.SetPixel(x, y, new SKColor((byte)(x * 255 / side),
					(byte)(y * 255 / side), (byte)((x + y) * 127 / side)));
			}
		}
		return bitmap;
	}
}

[MemoryDiagnoser]
public class BlurHashEncodeBenchmark
{
	[Params(32, 64, 100)]
	public int Side { get; set; }

	private SKBitmap bitmap = null!;
	private SKImage image = null!;
	private SKBitmap normalized = null!;
	private byte[] rgba = null!;

	[GlobalSetup]
	public void Setup()
	{
		bitmap = PlaceholderBenchmarkSource.Create(Side, SKAlphaType.Opaque);
		image = SKImage.FromBitmap(bitmap);
		normalized = PixelBuffers.Normalize(bitmap, PixelBuffers.MaximumThumbnailDimension, compositeWhite: true);
		(rgba, _, _) = PixelBuffers.FromBitmap(bitmap, PixelBuffers.MaximumThumbnailDimension, compositeWhite: true);
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		image.Dispose();
		normalized.Dispose();
		bitmap.Dispose();
	}

	[Benchmark(Baseline = true)]
	public string ArrayInput() => BlurHashCodec.Encode(rgba, Side, Side,
		Side * PlaceholderBenchmarkSource.BytesPerPixel, PlaceholderBenchmarkSource.ComponentsX, PlaceholderBenchmarkSource.ComponentsY);

	[Benchmark]
	public string SpanInput() => BlurHashCodec.Encode(rgba.AsSpan(), Side, Side,
		Side * PlaceholderBenchmarkSource.BytesPerPixel, PlaceholderBenchmarkSource.ComponentsX, PlaceholderBenchmarkSource.ComponentsY);

	[Benchmark]
	public string BitmapAdapter() => BlurHashCodec.Encode(bitmap,
		PlaceholderBenchmarkSource.ComponentsX, PlaceholderBenchmarkSource.ComponentsY);

	[Benchmark]
	public string ImageAdapter() => BlurHashCodec.Encode(image,
		PlaceholderBenchmarkSource.ComponentsX, PlaceholderBenchmarkSource.ComponentsY);

	[Benchmark]
	public string NormalizedPixmap() => BlurHashCodec.Encode(PixelBuffers.Pixels(normalized),
		normalized.Width, normalized.Height, normalized.RowBytes,
		PlaceholderBenchmarkSource.ComponentsX, PlaceholderBenchmarkSource.ComponentsY);
}

[MemoryDiagnoser]
public class BlurHashDecodeBenchmark
{
	[Params(32, 64)]
	public int Side { get; set; }

	private const string Hash = "LEHV6nWB2yk8pyo0adR*.7kCMdnj";
	private byte[] destination = null!;

	[GlobalSetup]
	public void Setup() => destination = new byte[Side * Side * PlaceholderBenchmarkSource.BytesPerPixel];

	[Benchmark(Baseline = true)]
	public byte[] AllocatedArray() => BlurHashCodec.Decode(Hash, Side, Side);

	[Benchmark]
	public byte[] CallerOwnedSpan()
	{
		BlurHashCodec.DecodeInto(Hash.AsSpan(), destination.AsSpan(), Side, Side,
			Side * PlaceholderBenchmarkSource.BytesPerPixel);
		return destination;
	}

	[Benchmark]
	public SKColor BitmapAdapter()
	{
		using var bitmap = BlurHashCodec.DecodeBitmap(Hash, Side, Side);
		return bitmap.GetPixel(Side / 2, Side / 2);
	}
}

[MemoryDiagnoser]
public class ThumbHashEncodeBenchmark
{
	[Params(32, 64, 100)]
	public int Side { get; set; }

	private SKBitmap bitmap = null!;
	private SKImage image = null!;
	private byte[] rgba = null!;

	[GlobalSetup]
	public void Setup()
	{
		bitmap = PlaceholderBenchmarkSource.Create(Side, SKAlphaType.Unpremul);
		image = SKImage.FromBitmap(bitmap);
		(rgba, _, _) = PixelBuffers.FromBitmap(bitmap, PixelBuffers.MaximumThumbnailDimension, compositeWhite: false);
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		image.Dispose();
		bitmap.Dispose();
	}

	[Benchmark(Baseline = true)]
	public byte[] ArrayInput() => ThumbHashCodec.Encode(rgba, Side, Side,
		Side * PlaceholderBenchmarkSource.BytesPerPixel);

	[Benchmark]
	public byte[] SpanInput() => ThumbHashCodec.Encode(rgba.AsSpan(), Side, Side,
		Side * PlaceholderBenchmarkSource.BytesPerPixel);

	[Benchmark]
	public byte[] BitmapAdapter() => ThumbHashCodec.Encode(bitmap);

	[Benchmark]
	public byte[] ImageAdapter() => ThumbHashCodec.Encode(image);
}

[MemoryDiagnoser]
public class ThumbHashDecodeBenchmark
{
	[Params(32, 64)]
	public int Side { get; set; }

	private static readonly byte[] Hash = Convert.FromHexString("934A062D069256C374055867DA8AB6679490510719");
	private byte[] destination = null!;
	private int stride;

	[GlobalSetup]
	public void Setup()
	{
		var size = ThumbHashCodec.GetDecodedSize(Hash, Side, Side);
		stride = size.Width * PlaceholderBenchmarkSource.BytesPerPixel;
		destination = new byte[stride * size.Height];
	}

	[Benchmark(Baseline = true)]
	public byte[] AllocatedArray() => ThumbHashCodec.Decode(Hash, Side, Side, out _, out _);

	[Benchmark]
	public byte[] CallerOwnedSpan()
	{
		ThumbHashCodec.DecodeInto(Hash, destination.AsSpan(), Side, Side, stride, out _, out _);
		return destination;
	}

	[Benchmark]
	public SKColor BitmapAdapter()
	{
		using var bitmap = ThumbHashCodec.DecodeBitmap(Hash, Side, Side);
		return bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2);
	}
}

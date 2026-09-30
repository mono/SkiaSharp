global using System;
using BenchmarkDotNet.Attributes;
using SkiaSharp.Extended;
using SkiaSharpSample.ImagePlaceholders;

namespace SkiaSharp.Benchmarks;

// The historical implementation is source-linked from a pinned MIT revision and
// compiled against the same in-tree SkiaSharp binding as the new implementation.
[MemoryDiagnoser]
public class BlurHashEncodeBenchmark
{
	[Params(32, 64, 100)]
	public int Side { get; set; }

	private SKBitmap bitmap = null!;
	private SKBitmap normalized = null!;

	[GlobalSetup]
	public void Setup()
	{
		bitmap = new SKBitmap(new SKImageInfo(Side, Side, SKColorType.Rgba8888, SKAlphaType.Unpremul));
		for (var y = 0; y < Side; y++)
		{
			for (var x = 0; x < Side; x++)
				bitmap.SetPixel(x, y, new SKColor((byte)(x * 255 / Side), (byte)(y * 255 / Side), (byte)((x + y) * 127 / Side)));
		}
		normalized = PixelBuffers.Normalize(bitmap, PixelBuffers.MaximumThumbnailDimension, compositeWhite: true);
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		normalized.Dispose();
		bitmap.Dispose();
	}

	[Benchmark(Baseline = true)]
	public string ExtendedSource() => SKBlurHash.Serialize(bitmap, 4, 3);

	[Benchmark]
	public string Gallery() => BlurHashCodec.Encode(bitmap);

	[Benchmark]
	public string GalleryNormalizedRgba() =>
		BlurHashCodec.Encode(PixelBuffers.Pixels(normalized), normalized.Width, normalized.Height, normalized.RowBytes);

	[Benchmark]
	public string GalleryWithManagedCopy()
	{
		var (pixels, width, height) = PixelBuffers.FromBitmap(bitmap, PixelBuffers.MaximumThumbnailDimension, compositeWhite: true);
		return BlurHashCodec.Encode(pixels, width, height, width * PixelBuffers.RgbaBytesPerPixel);
	}
}

[MemoryDiagnoser]
public class BlurHashKernelDecodeBenchmark
{
	[Params(32, 64, 100)]
	public int Side { get; set; }

	private const string Hash = "LEHV6nWB2yk8pyo0adR*.7kCMdnj";
	private byte[] destination = null!;

	[GlobalSetup]
	public void Setup() => destination = new byte[Side * Side * 4];

	[Benchmark(Baseline = true)]
	public byte RgbaKernel() => BlurHashCodec.Decode(Hash, Side, Side)[0];

	[Benchmark]
	public byte BitmapAdapter()
	{
		using var bitmap = BlurHashCodec.DecodeBitmap(Hash, Side, Side);
		return bitmap.GetPixel(0, 0).Red;
	}

	[Benchmark]
	public byte IntoCallerBuffer()
	{
		BlurHashCodec.DecodeInto(Hash.AsSpan(), destination, Side, Side, Side * 4, 1f);
		return destination[0];
	}
}

[MemoryDiagnoser]
public class BlurHashDecodeBenchmark
{
	[Params(32, 64, 100)]
	public int Side { get; set; }

	private const string Hash = "LEHV6nWB2yk8pyo0adR*.7kCMdnj";

	[Benchmark(Baseline = true)]
	public SKColor ExtendedSource()
	{
		using var result = SKBlurHash.DeserializeBitmap(Hash, Side, Side);
		return result.GetPixel(Side / 2, Side / 2);
	}

	[Benchmark]
	public SKColor Gallery()
	{
		using var result = BlurHashCodec.DecodeBitmap(Hash, Side, Side);
		return result.GetPixel(Side / 2, Side / 2);
	}

	[MemoryDiagnoser]
	public class ThumbHashEncodeBenchmark
	{
		[Params(32, 64, 100)]
		public int Side { get; set; }

		private SKBitmap bitmap = null!;

		[GlobalSetup]
		public void Setup()
		{
			bitmap = new SKBitmap(new SKImageInfo(Side, Side, SKColorType.Rgba8888, SKAlphaType.Unpremul));
			for (var y = 0; y < Side; y++)
			{
				for (var x = 0; x < Side; x++)
					bitmap.SetPixel(x, y, new SKColor((byte)(x * 255 / Side), (byte)(y * 255 / Side), (byte)((x + y) * 127 / Side)));
			}
		}

		[GlobalCleanup]
		public void Cleanup() => bitmap.Dispose();

		[Benchmark(Baseline = true)]
		public string Reference()
		{
			var (pixels, width, height) = PixelBuffers.FromBitmap(bitmap, 100, false);
			return Convert.ToBase64String(ThumbHashes.ThumbHash.FromImage(width, height, pixels).Hash.Span);
		}

		[Benchmark]
		public string Gallery() => Convert.ToBase64String(ThumbHashCodec.Encode(bitmap));
	}

	[MemoryDiagnoser]
	public class ThumbHashDecodeBenchmark
	{
		private static readonly byte[] Hash = Convert.FromHexString("934A062D069256C374055867DA8AB6679490510719");
		private readonly byte[] destination = new byte[32 * 32 * 4];

		[Benchmark(Baseline = true)]
		public byte Reference() => new ThumbHashes.ThumbHash(Hash).ToImage().rgba[0];

		[Benchmark]
		public byte Gallery() => ThumbHashCodec.Decode(Hash, 32, 32, out _, out _)[0];

		[Benchmark]
		public byte IntoCallerBuffer()
		{
			ThumbHashCodec.DecodeInto(Hash, destination, 32, 32, 32 * 4, out _, out _);
			return destination[0];
		}
	}

	[MemoryDiagnoser]
	public class ThumbHashKernelEncodeBenchmark
	{
		[Params(32, 64, 100)]
		public int Side { get; set; }

		private byte[] pixels = null!;

		[GlobalSetup]
		public void Setup()
		{
			pixels = new byte[Side * Side * 4];
			for (var y = 0; y < Side; y++)
			for (var x = 0; x < Side; x++)
			{
				var i = 4 * (y * Side + x);
				pixels[i] = (byte)(x * 255 / Side);
				pixels[i + 1] = (byte)(y * 255 / Side);
				pixels[i + 2] = (byte)((x + y) * 127 / Side);
				pixels[i + 3] = 255;
			}
		}

		[Benchmark(Baseline = true)]
		public int Reference() => ThumbHashes.ThumbHash.FromImage(Side, Side, pixels).Hash.Length;

		[Benchmark]
		public int Gallery() => ThumbHashCodec.Encode(pixels, Side, Side, Side * 4).Length;
	}
}

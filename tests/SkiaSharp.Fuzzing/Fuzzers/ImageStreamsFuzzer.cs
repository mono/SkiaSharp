using System.IO;

namespace SkiaSharp.Fuzzing.Fuzzers;

/// <summary>
/// Fuzzes <see cref="SKCodec" /> (full, multi-frame, subset, incremental, and scanline decoding),
/// <see cref="SKImage.FromEncodedData(Stream)" />, <see cref="SKManagedStream.CopyTo(SKWStream)" />, and
/// <see cref="SKBitmap.Encode(SKWStream, SKEncodedImageFormat, int)" /> over seekable, non-seekable, short-read,
/// and early-EOF managed streams (<see cref="SKManagedStream" />, <see cref="SKFrontBufferedManagedStream" />,
/// and <see cref="SKManagedWStream" />).
/// </summary>
public sealed class ImageStreamsFuzzer : IFuzzer
{
	private const long MaximumDecodedBytes = 4 * 1024 * 1024;
	private const int MaximumDimension = 1024;
	private const int MaximumInputBytes = 1 * 1024 * 1024;

	public static string Name => "image-streams";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		FuzzSeeds.EnsureInitialized(Name);
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		var payload = bytes.ToArray();

		FuzzAllModes(payload, maximumReadLength: int.MaxValue, earlyEndPosition: -1);
		FuzzAllModes(payload, maximumReadLength: 8, earlyEndPosition: -1);
		FuzzAllModes(payload, maximumReadLength: int.MaxValue, earlyEndPosition: payload.Length / 2);
	}

	private static void FuzzAllModes(byte[] payload, int maximumReadLength, int earlyEndPosition)
	{
		Fuzz(payload, seekable: true, maximumReadLength, earlyEndPosition);
		Fuzz(payload, seekable: false, maximumReadLength, earlyEndPosition);
	}

	private static void Fuzz(byte[] payload, bool seekable, int maximumReadLength, int earlyEndPosition)
	{
		CopyStream(payload, seekable, maximumReadLength, earlyEndPosition);
		DecodeCodec(payload, seekable, maximumReadLength, earlyEndPosition);
		DecodeIncremental(payload, seekable, maximumReadLength, earlyEndPosition);
		DecodeScanlines(payload, seekable, maximumReadLength, earlyEndPosition);
		DecodeImage(payload, seekable, maximumReadLength, earlyEndPosition);
	}

	private static void CopyStream(byte[] payload, bool seekable, int maximumReadLength, int earlyEndPosition)
	{
		using var stream = new FuzzStream(payload, seekable, maximumReadLength, earlyEndPosition);
		using var managed = new SKManagedStream(stream);
		using var output = new FuzzWStream(seekable);
		using var wstream = new SKManagedWStream(output);
		_ = managed.CopyTo(wstream);
		if (output.CanSeek)
			_ = wstream.BytesWritten;
	}

	private static void DecodeCodec(byte[] payload, bool seekable, int maximumReadLength, int earlyEndPosition)
	{
		using var stream = new FuzzStream(payload, seekable, maximumReadLength, earlyEndPosition);
		using var codec = SKCodec.Create(stream, out _);
		if (codec is null)
			return;

		var info = codec.Info;
		if (!IsBoundedImageInfo(info))
			return;

		_ = codec.EncodedFormat;
		_ = codec.EncodedOrigin;
		_ = codec.RepetitionCount;
		_ = codec.FrameInfo;
		_ = codec.GetScaledDimensions(0.5f);

		using var bitmap = new SKBitmap();
		if (!bitmap.TryAllocPixels(info))
			return;

		var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels());

		var subset = SKRectI.Create(Math.Max(1, info.Width / 2), Math.Max(1, info.Height / 2));
		if (codec.GetValidSubset(ref subset))
			_ = codec.GetPixels(bitmap.Info, bitmap.GetPixels(), new SKCodecOptions(subset));

		var frameCount = Math.Min(codec.FrameCount, 4);
		for (var frame = 1; frame < frameCount; frame++)
		{
			_ = codec.GetFrameInfo(frame, out _);
			_ = codec.GetPixels(bitmap.Info, bitmap.GetPixels(), new SKCodecOptions(frame));
		}

		if (result == SKCodecResult.Success && bitmap.Width <= 64 && bitmap.Height <= 64)
		{
			using var output = new FuzzWStream(seekable);
			using var wstream = new SKManagedWStream(output);
			_ = bitmap.Encode(wstream, SKEncodedImageFormat.Png, 80);
			wstream.Flush();
			if (output.CanSeek)
				_ = wstream.BytesWritten;
		}
	}

	private static void DecodeIncremental(byte[] payload, bool seekable, int maximumReadLength, int earlyEndPosition)
	{
		using var stream = new FuzzStream(payload, seekable, maximumReadLength, earlyEndPosition);
		using var codec = SKCodec.Create(stream);
		if (codec is null || !IsBoundedImageInfo(codec.Info))
			return;

		using var bitmap = new SKBitmap();
		if (!bitmap.TryAllocPixels(codec.Info))
			return;

		var startResult = codec.StartIncrementalDecode(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes);
		if (startResult == SKCodecResult.Success)
			_ = codec.IncrementalDecode(out _);
	}

	private static void DecodeScanlines(byte[] payload, bool seekable, int maximumReadLength, int earlyEndPosition)
	{
		using var stream = new FuzzStream(payload, seekable, maximumReadLength, earlyEndPosition);
		using var codec = SKCodec.Create(stream);
		if (codec is null || !IsBoundedImageInfo(codec.Info))
			return;

		using var bitmap = new SKBitmap();
		if (!bitmap.TryAllocPixels(codec.Info))
			return;

		if (codec.StartScanlineDecode(bitmap.Info) == SKCodecResult.Success)
		{
			_ = codec.ScanlineOrder;
			_ = codec.NextScanline;
			_ = codec.GetOutputScanline(0);
			if (bitmap.Height > 1)
				_ = codec.SkipScanlines(1);
			_ = codec.GetScanlines(bitmap.GetPixels(), Math.Min(bitmap.Height, 4), bitmap.RowBytes);
		}
	}

	private static void DecodeImage(byte[] payload, bool seekable, int maximumReadLength, int earlyEndPosition)
	{
		using var stream = new FuzzStream(payload, seekable, maximumReadLength, earlyEndPosition);
		using var image = SKImage.FromEncodedData(stream);
		if (image is null || image.Width <= 0 || image.Height <= 0 || image.Width > MaximumDimension || image.Height > MaximumDimension)
			return;

		using var surfaceBitmap = new SKBitmap(32, 32);
		using var canvas = new SKCanvas(surfaceBitmap);
		canvas.DrawImage(image, SKRect.Create(32, 32), SKSamplingOptions.Default);
	}

	private static bool IsBoundedImageInfo(SKImageInfo info) =>
		info.Width > 0 &&
		info.Height > 0 &&
		info.Width <= MaximumDimension &&
		info.Height <= MaximumDimension &&
		info.BytesSize64 > 0 &&
		info.BytesSize64 <= MaximumDecodedBytes;
}

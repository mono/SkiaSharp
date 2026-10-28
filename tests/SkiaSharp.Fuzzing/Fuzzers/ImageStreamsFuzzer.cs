using System.IO;

namespace SkiaSharp.Fuzzing.Fuzzers;

public sealed class ImageStreamsFuzzer : IFuzzer
{
	private const long MaximumDecodedBytes = 4 * 1024 * 1024;
	private const int MaximumDimension = 1024;
	private const int MaximumInputBytes = 1 * 1024 * 1024;

	public static string Name => "image-streams";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		var payload = bytes.ToArray();
		var selector = payload[^1];

		DecodeCodec(payload, seekable: true, selector);
		DecodeCodec(payload, seekable: false, (byte)(selector ^ 2));
		DecodeImage(payload, seekable: (selector & 1) == 0);
	}

	private static void DecodeCodec(byte[] payload, bool seekable, byte selector)
	{
		using var stream = new FuzzStream(payload, seekable);
		using var codec = SKCodec.Create(stream);
		if (codec is null)
			return;

		_ = codec.EncodedFormat;
		_ = codec.EncodedOrigin;
		_ = codec.RepetitionCount;

		var info = codec.Info;
		if (info.Width <= 0 ||
			info.Height <= 0 ||
			info.Width > MaximumDimension ||
			info.Height > MaximumDimension ||
			info.BytesSize64 <= 0 ||
			info.BytesSize64 > MaximumDecodedBytes)
			return;

		using var bitmap = new SKBitmap();
		if (!bitmap.TryAllocPixels(info))
			return;

		switch ((selector >> 1) & 3)
		{
			case 0:
			case 1:
				var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels());
				var frameCount = Math.Min(codec.FrameCount, 4);
				for (var frame = 1; frame < frameCount; frame++)
				{
					_ = codec.GetFrameInfo(frame, out _);
					_ = codec.GetPixels(bitmap.Info, bitmap.GetPixels(), new SKCodecOptions(frame));
				}

				if (result == SKCodecResult.Success && bitmap.Width <= 64 && bitmap.Height <= 64)
					EncodeToManagedWStream(bitmap, seekable);
				break;

			case 2:
				if (codec.StartIncrementalDecode(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes) == SKCodecResult.Success)
					_ = codec.IncrementalDecode(out _);
				break;

			case 3:
				if (codec.StartScanlineDecode(bitmap.Info) == SKCodecResult.Success)
				{
					_ = codec.NextScanline;
					_ = codec.ScanlineOrder;
					_ = codec.GetScanlines(bitmap.GetPixels(), Math.Min(bitmap.Height, 16), bitmap.RowBytes);
				}
				break;
		}
	}

	private static void DecodeImage(byte[] payload, bool seekable)
	{
		using var stream = new FuzzStream(payload, seekable);
		using var image = SKImage.FromEncodedData(stream);
		if (image is null || image.Width <= 0 || image.Height <= 0 || image.Width > MaximumDimension || image.Height > MaximumDimension)
			return;

		using var surfaceBitmap = new SKBitmap(32, 32);
		using var canvas = new SKCanvas(surfaceBitmap);
		canvas.DrawImage(image, SKRect.Create(32, 32), SKSamplingOptions.Default);
	}

	private static void EncodeToManagedWStream(SKBitmap bitmap, bool seekable)
	{
		using var output = new FuzzWStream(seekable);
		using var wstream = new SKManagedWStream(output);
		_ = bitmap.Encode(wstream, SKEncodedImageFormat.Png, 80);
		wstream.Flush();
		if (output.CanSeek)
			_ = wstream.BytesWritten;
	}
}

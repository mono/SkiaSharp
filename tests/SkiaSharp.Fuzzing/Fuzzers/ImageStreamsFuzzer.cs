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

		Fuzz(payload, seekable: true);
		Fuzz(payload, seekable: false);
	}

	private static void Fuzz(byte[] payload, bool seekable)
	{
		DecodeCodec(payload, seekable);
		DecodeImage(payload, seekable);
	}

	private static void DecodeCodec(byte[] payload, bool seekable)
	{
		using var stream = new FuzzStream(payload, seekable);
		using var codec = SKCodec.Create(stream);
		if (codec is null)
			return;

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

		var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels());

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
}

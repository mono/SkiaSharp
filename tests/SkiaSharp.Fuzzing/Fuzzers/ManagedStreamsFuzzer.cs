using System.IO;

namespace SkiaSharp.Fuzzing.Fuzzers;

public sealed class ManagedStreamsFuzzer : IFuzzer
{
	private const long MaximumDecodedBytes = 16 * 1024 * 1024;
	private const int MaximumDimension = 4096;
	private const int MaximumInputBytes = 1 * 1024 * 1024;

	public static string Name => "managed-streams";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		var payload = bytes.ToArray();

		TestNormalStreams(payload);
		TestShortReadStreams(payload);
		TestEarlyEndStreams(payload);
	}

	private static void TestNormalStreams(byte[] payload)
	{
		ExerciseStreams(payload, maximumReadLength: int.MaxValue, earlyEndPosition: -1);
	}

	private static void TestShortReadStreams(byte[] payload)
	{
		ExerciseStreams(payload, maximumReadLength: 8, earlyEndPosition: -1);
	}

	private static void TestEarlyEndStreams(byte[] payload)
	{
		ExerciseStreams(payload, maximumReadLength: int.MaxValue, earlyEndPosition: payload.Length / 2);
	}

	private static void ExerciseStreams(byte[] payload, int maximumReadLength, int earlyEndPosition)
	{
		ExerciseOperations(() => new FuzzStream(payload, seekable: true, maximumReadLength, earlyEndPosition));
		ExerciseOperations(() => new FuzzStream(payload, seekable: false, maximumReadLength, earlyEndPosition));
	}

	private static void ExerciseOperations(Func<Stream> streamFactory)
	{
		using (var stream = streamFactory())
			DecodeImage(stream);

		using (var stream = streamFactory())
			DecodeBitmap(stream);

		using (var stream = streamFactory())
			DeserializePicture(stream);

		using (var stream = streamFactory())
			CreateTypeface(stream);
	}

	private static void DecodeImage(Stream stream)
	{
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

		_ = codec.GetPixels(bitmap.Info, bitmap.GetPixels());
	}

	private static void DecodeBitmap(Stream stream)
	{
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

		using var bitmap = SKBitmap.Decode(codec);
		if (bitmap is null)
			return;

		_ = bitmap.Bytes.Length;
	}

	private static void DeserializePicture(Stream stream)
	{
		using var picture = SKPicture.Deserialize(stream);
		PictureDeserializeFuzzer.ExercisePicture(picture);
	}

	private static void CreateTypeface(Stream stream)
	{
		using var typeface = SKFontManager.Default.CreateTypeface(stream);
	}
}

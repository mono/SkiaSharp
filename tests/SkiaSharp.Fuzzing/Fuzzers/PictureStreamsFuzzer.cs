using System.IO;

namespace SkiaSharp.Fuzzing.Fuzzers;

public sealed class PictureStreamsFuzzer : IFuzzer
{
	private const int MaximumInputBytes = 1 * 1024 * 1024;

	public static string Name => "picture-streams";

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
		using var stream = new FuzzStream(payload, seekable);
		using var picture = SKPicture.Deserialize(stream);
		if (picture is null)
			return;

		using var bitmap = new SKBitmap(64, 64);
		using var canvas = new SKCanvas(bitmap);
		canvas.DrawPicture(picture);
		picture.Playback(canvas);

		using var output = new FuzzWStream(seekable);
		picture.Serialize(output);
	}
}

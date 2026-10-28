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
		var seekable = (payload[^1] & 1) == 0;

		using var stream = new FuzzStream(payload, seekable);
		using var picture = SKPicture.Deserialize(stream);
		if (picture is null)
			return;

		_ = picture.CullRect;
		_ = picture.ApproximateOperationCount;
		_ = picture.ApproximateBytesUsed;

		using var bitmap = new SKBitmap(64, 64);
		using var canvas = new SKCanvas(bitmap);
		canvas.Clear(SKColors.Transparent);
		if ((payload[^1] & 2) == 0)
			canvas.DrawPicture(picture);
		else
			picture.Playback(canvas);
		canvas.Flush();

		using var output = new FuzzWStream(seekable);
		picture.Serialize(output);
	}
}

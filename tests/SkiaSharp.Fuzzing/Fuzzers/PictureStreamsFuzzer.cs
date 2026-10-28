using System.IO;

namespace SkiaSharp.Fuzzing.Fuzzers;

/// <summary>
/// Fuzzes <see cref="SKPicture.Deserialize(Stream)" />, raster canvas playback (<see cref="SKCanvas.DrawPicture(SKPicture)" />
/// and <see cref="SKPicture.Playback(SKCanvas)" />), and <see cref="SKPicture.Serialize(Stream)" /> over seekable and
/// non-seekable managed streams (<see cref="SKManagedStream" /> and <see cref="SKManagedWStream" />).
/// </summary>
public sealed class PictureStreamsFuzzer : IFuzzer
{
	private const int MaximumInputBytes = 1 * 1024 * 1024;

	public static string Name => "picture-streams";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		var payload = bytes.ToArray();

		using var stream = new FuzzStream(payload, seekable: true);
		using var picture = SKPicture.Deserialize(stream);
		if (picture is null)
			return;

		using var bitmap = new SKBitmap(64, 64);
		using var canvas = new SKCanvas(bitmap);
		canvas.DrawPicture(picture);
		picture.Playback(canvas);

		Serialize(picture, seekable: true);
		Serialize(picture, seekable: false);
	}

	private static void Serialize(SKPicture picture, bool seekable)
	{
		using var output = new FuzzWStream(seekable);
		picture.Serialize(output);
	}
}

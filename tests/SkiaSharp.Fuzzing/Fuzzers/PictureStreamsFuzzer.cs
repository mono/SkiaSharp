using System.IO;

namespace SkiaSharp.Fuzzing.Fuzzers;

/// <summary>
/// Fuzzes <see cref="SKPicture.Deserialize(Stream)" />, raster canvas playback (<see cref="SKCanvas.DrawPicture(SKPicture)" />
/// and <see cref="SKPicture.Playback(SKCanvas)" />), and <see cref="SKPicture.Serialize(Stream)" /> over seekable,
/// non-seekable, short-read, and early-EOF managed streams (<see cref="SKManagedStream" /> and <see cref="SKManagedWStream" />).
/// </summary>
public sealed class PictureStreamsFuzzer : Fuzzer<PictureStreamsFuzzer>
{
	private const int MaximumInputBytes = 1 * 1024 * 1024;

	public override string Name => "picture-streams";

	protected override void OnFuzz(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		var payload = bytes.ToArray();

		Fuzz(payload, maximumReadLength: int.MaxValue, earlyEndPosition: -1);
		Fuzz(payload, maximumReadLength: 8, earlyEndPosition: -1);
		Fuzz(payload, maximumReadLength: int.MaxValue, earlyEndPosition: payload.Length / 2);
	}

	private static void Fuzz(byte[] payload, int maximumReadLength, int earlyEndPosition)
	{
		using var stream = new FuzzStream(payload, seekable: true, maximumReadLength, earlyEndPosition);
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
		using var wstream = new SKManagedWStream(output);
		picture.Serialize(wstream);
		wstream.Flush();
		if (output.CanSeek)
			_ = wstream.BytesWritten;
	}
}

using System.IO;

namespace SkiaSharp.Fuzzing.Fuzzers;

public sealed class PictureDeserializeFuzzer : IFuzzer
{
	public static string Name => "picture-deserialize";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty)
			return;

		TestDeserializeSpan(bytes);
		TestDeserializeData(bytes);
		TestDeserializeStream(bytes);
	}

	private static void TestDeserializeSpan(ReadOnlySpan<byte> bytes)
	{
		using var picture = SKPicture.Deserialize(bytes);
		ExercisePicture(picture);
	}

	private static void TestDeserializeData(ReadOnlySpan<byte> bytes)
	{
		using var data = SKData.CreateCopy(bytes);
		using var picture = SKPicture.Deserialize(data);
		ExercisePicture(picture);
	}

	private static void TestDeserializeStream(ReadOnlySpan<byte> bytes)
	{
		using var stream = new MemoryStream(bytes.ToArray(), writable: false);
		using var picture = SKPicture.Deserialize(stream);
		ExercisePicture(picture);
	}

	internal static void ExercisePicture(SKPicture? picture)
	{
		if (picture is null)
			return;

		using var bitmap = new SKBitmap(64, 64);
		using var canvas = new SKCanvas(bitmap);
		canvas.Clear(SKColors.Transparent);
		canvas.DrawPicture(picture);
		canvas.Flush();
	}
}

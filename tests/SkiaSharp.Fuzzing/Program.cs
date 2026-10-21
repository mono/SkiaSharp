namespace SkiaSharp.Fuzzing;

internal static class Program
{
	public static int Main(string[] args)
	{
		if (args.Length == 2 && args[0] == "--write-seeds")
		{
			WriteSeeds(args[1]);
			return 0;
		}

		if (args.Length == 2 && args[0] == "--smoke")
		{
			RunSmoke(args[1]);
			return 0;
		}

		Console.Error.WriteLine("Usage: SkiaSharp.Fuzzing --write-seeds <directory> | --smoke <directory>");
		return 1;
	}

	private static void WriteSeeds(string outputDirectory)
	{
		var managedStreams = Path.Combine(outputDirectory, "managed-streams");
		var pictureDeserialize = Path.Combine(outputDirectory, "picture-deserialize");
		Directory.CreateDirectory(managedStreams);
		Directory.CreateDirectory(pictureDeserialize);

		using var bitmap = new SKBitmap(new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul));
		bitmap.Erase(SKColors.CornflowerBlue);

		using var image = SKImage.FromBitmap(bitmap);
		using var encoded = image.Encode(SKEncodedImageFormat.Png, 100) ??
			throw new InvalidOperationException("Unable to create the PNG fuzz seed.");
		WriteManagedStreamSeed(Path.Combine(managedStreams, "image.png"), encoded.ToArray());

		var fontPath = Path.Combine(AppContext.BaseDirectory, "seed-sources", "ZeroGlyphs.ttf");
		if (!File.Exists(fontPath))
			throw new FileNotFoundException("Unable to find the font fuzz seed.", fontPath);
		WriteManagedStreamSeed(Path.Combine(managedStreams, "font.ttf"), File.ReadAllBytes(fontPath));

		using var recorder = new SKPictureRecorder();
		using var canvas = recorder.BeginRecording(SKRect.Create(0, 0, 16, 16));
		canvas.DrawImage(image, SKRect.Create(2, 2, 12, 12), SKSamplingOptions.Default);
		using var picture = recorder.EndRecording();
		using var serialized = picture.Serialize() ??
			throw new InvalidOperationException("Unable to create the SKP fuzz seed.");
		File.WriteAllBytes(Path.Combine(pictureDeserialize, "embedded-image.skp"), serialized.ToArray());
	}

	private static void RunSmoke(string seedDirectory)
	{
		foreach (var path in Directory.EnumerateFiles(Path.Combine(seedDirectory, "managed-streams")))
			FuzzTargets.FuzzManagedStreams(File.ReadAllBytes(path));

		foreach (var path in Directory.EnumerateFiles(Path.Combine(seedDirectory, "picture-deserialize")))
			FuzzTargets.FuzzPictureDeserialize(File.ReadAllBytes(path));

		FuzzTargets.FuzzManagedStreams(new byte[] { 0, 0 });
		FuzzTargets.FuzzManagedStreams(new byte[] { 0xc1, 1, 2, 3, 4 });
		FuzzTargets.FuzzPictureDeserialize(new byte[] { 0 });
		FuzzTargets.FuzzPictureDeserialize(new byte[] { 0xff, 0, 0xff });
	}

	private static void WriteManagedStreamSeed(string path, byte[] payload)
	{
		var seed = new byte[payload.Length + 1];
		payload.CopyTo(seed, 1);
		File.WriteAllBytes(path, seed);
	}
}

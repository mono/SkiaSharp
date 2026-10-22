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
		var harfBuzzCallbacks = Path.Combine(outputDirectory, "harfbuzz-managed-callbacks");
		var skottieProviders = Path.Combine(outputDirectory, "skottie-resource-providers");
		Directory.CreateDirectory(managedStreams);
		Directory.CreateDirectory(pictureDeserialize);
		Directory.CreateDirectory(harfBuzzCallbacks);
		Directory.CreateDirectory(skottieProviders);

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

		WriteModeSeed(Path.Combine(harfBuzzCallbacks, "shape-basic.bin"), 0x00, "SkiaSharp");
		WriteModeSeed(Path.Combine(harfBuzzCallbacks, "shape-truncated.bin"), 0x40, "الخط العربي");
		WriteModeSeed(Path.Combine(harfBuzzCallbacks, "shape-exception.bin"), 0x80, "callback-exception");

		WriteModeSeed(Path.Combine(skottieProviders, "skottie-basic.bin"), 0x00, "baboon.png");
		WriteModeSeed(Path.Combine(skottieProviders, "skottie-predecode.bin"), 0x03, "LottieLogo1.json");
		WriteModeSeed(Path.Combine(skottieProviders, "skottie-traversal.bin"), 0x00, "../outside.png");
	}

	private static void RunSmoke(string seedDirectory)
	{
		foreach (var path in Directory.EnumerateFiles(Path.Combine(seedDirectory, "managed-streams")))
			FuzzTargets.FuzzManagedStreams(File.ReadAllBytes(path));

		foreach (var path in Directory.EnumerateFiles(Path.Combine(seedDirectory, "picture-deserialize")))
			FuzzTargets.FuzzPictureDeserialize(File.ReadAllBytes(path));

		foreach (var path in Directory.EnumerateFiles(Path.Combine(seedDirectory, "harfbuzz-managed-callbacks")))
			FuzzTargets.FuzzHarfBuzzManagedTableCallbacks(File.ReadAllBytes(path));

		foreach (var path in Directory.EnumerateFiles(Path.Combine(seedDirectory, "skottie-resource-providers")))
			FuzzTargets.FuzzSkottieResourceProviders(File.ReadAllBytes(path));

		FuzzTargets.FuzzManagedStreams(new byte[] { 0, 0 });
		FuzzTargets.FuzzManagedStreams(new byte[] { 0xc1, 1, 2, 3, 4 });
		FuzzTargets.FuzzPictureDeserialize(new byte[] { 0 });
		FuzzTargets.FuzzPictureDeserialize(new byte[] { 0xff, 0, 0xff });
		FuzzTargets.FuzzHarfBuzzManagedTableCallbacks(new byte[] { 0, 0x48, 0x42 });
		FuzzTargets.FuzzHarfBuzzManagedTableCallbacks(new byte[] { 0x80, 0x6e, 0x61, 0x6d, 0x65 });
		FuzzTargets.FuzzSkottieResourceProviders(new byte[] { 0x00, 0x2e, 0x2e, 0x2f, 0x66, 0x2e, 0x70, 0x6e, 0x67 });
		FuzzTargets.FuzzSkottieResourceProviders(new byte[] { 0x03, 0x62, 0x61, 0x62, 0x6f, 0x6f, 0x6e, 0x2e, 0x70, 0x6e, 0x67 });
	}

	private static void WriteManagedStreamSeed(string path, byte[] payload)
	{
		var seed = new byte[payload.Length + 1];
		payload.CopyTo(seed, 1);
		File.WriteAllBytes(path, seed);
	}

	private static void WriteModeSeed(string path, byte mode, string text)
	{
		var payload = System.Text.Encoding.UTF8.GetBytes(text);
		var seed = new byte[payload.Length + 1];
		seed[0] = mode;
		payload.CopyTo(seed, 1);
		File.WriteAllBytes(path, seed);
	}
}

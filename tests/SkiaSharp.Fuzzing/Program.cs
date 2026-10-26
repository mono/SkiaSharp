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

		if (args.Length >= 3 && args[0] == "--replay")
			return Replay(args[1], args.Skip(2));

		Console.Error.WriteLine("Usage: SkiaSharp.Fuzzing --write-seeds <directory> | --smoke <directory> | --replay <target> <file|directory>...");
		return 1;
	}

	// Runs one fuzz target over saved inputs, such as the crash inputs the fuzzing service keeps, to reproduce a finding.
	// Each path is printed before it runs, so if the process dies (a native crash), the last line names the input.
	private static int Replay(string target, IEnumerable<string> paths)
	{
		Action<byte[]>? run = target switch
		{
			"managed-streams" => input => FuzzTargets.FuzzManagedStreams(input),
			"picture-deserialize" => input => FuzzTargets.FuzzPictureDeserialize(input),
			"harfbuzz-managed-callbacks" => input => FuzzTargets.FuzzHarfBuzzManagedTableCallbacks(input),
			"skottie-resource-providers" => input => FuzzTargets.FuzzSkottieResourceProviders(input),
			_ => null
		};

		if (run is null)
		{
			Console.Error.WriteLine($"Unknown target '{target}'. Use managed-streams, picture-deserialize, harfbuzz-managed-callbacks or skottie-resource-providers.");
			return 1;
		}

		var count = 0;
		foreach (var path in paths)
		{
			IEnumerable<string> files = Directory.Exists(path)
				? Directory.EnumerateFiles(path).OrderBy(f => f, StringComparer.Ordinal)
				: new[] { path };

			foreach (var file in files)
			{
				Console.WriteLine($"Replaying {file}");
				Console.Out.Flush();
				run(File.ReadAllBytes(file));
				count++;
			}
		}

		Console.WriteLine($"Replayed {count} inputs without a crash.");
		return 0;
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

		var seedSources = Path.Combine(AppContext.BaseDirectory, "seed-sources");
		var fontPath = Path.Combine(seedSources, "font.ttf");

		// A non-flat image with alpha, so the encoders and decoders exercise more than a solid fill.
		using var bitmap = new SKBitmap(new SKImageInfo(16, 16, SKColorType.Rgba8888, SKAlphaType.Unpremul));
		for (var y = 0; y < 16; y++)
		{
			for (var x = 0; x < 16; x++)
				bitmap.SetPixel(x, y, new SKColor((byte)(x * 16), (byte)(y * 16), (byte)((x ^ y) * 16), (byte)(255 - y * 8)));
		}

		using var image = SKImage.FromBitmap(bitmap);
		foreach (var (name, format) in new[]
		{
			("image.png", SKEncodedImageFormat.Png),
			("image.jpg", SKEncodedImageFormat.Jpeg),
			("image.webp", SKEncodedImageFormat.Webp)
		})
		{
			using var encoded = image.Encode(format, 90) ??
				throw new InvalidOperationException($"Unable to create the {name} fuzz seed.");
			WriteManagedStreamSeed(Path.Combine(managedStreams, name), encoded.ToArray());
		}

		WriteManagedStreamSeed(Path.Combine(managedStreams, "image.gif"), ReadSeedSource(seedSources, "animated-heart.gif"));
		WriteManagedStreamSeed(Path.Combine(managedStreams, "font.ttf"), ReadSeedSource(seedSources, "font.ttf"));

		// One picture that uses shaders, paths, layers, an image and text, so the deserializer has real structure to mutate.
		using var typeface = SKTypeface.FromFile(fontPath) ??
			throw new InvalidOperationException("Unable to load the font fuzz seed.");
		using var font = new SKFont(typeface, 12);
		using var recorder = new SKPictureRecorder();
		var canvas = recorder.BeginRecording(SKRect.Create(0, 0, 64, 64));
		using (var shader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(64, 64), new[] { SKColors.Red, SKColors.Blue }, SKShaderTileMode.Clamp))
		using (var paint = new SKPaint { Shader = shader })
			canvas.DrawRect(SKRect.Create(0, 0, 64, 64), paint);

		using (var builder = new SKPathBuilder())
		{
			builder.MoveTo(4, 4);
			builder.CubicTo(new SKPoint(60, 4), new SKPoint(60, 60), new SKPoint(4, 60));
			builder.Close();
			using var path = builder.Detach();
			using var paint = new SKPaint { IsAntialias = true, IsStroke = true, StrokeWidth = 3, Color = SKColors.White };
			canvas.DrawPath(path, paint);
		}

		using (var layerPaint = new SKPaint { Color = SKColors.White.WithAlpha(200) })
		{
			canvas.SaveLayer(layerPaint);
			canvas.DrawImage(image, SKRect.Create(8, 8, 24, 24), SKSamplingOptions.Default);
			using var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.Black };
			canvas.DrawText("HELLO", 8, 56, SKTextAlign.Left, font, textPaint);
			canvas.Restore();
		}

		using var picture = recorder.EndRecording();
		using var serialized = picture.Serialize() ??
			throw new InvalidOperationException("Unable to create the SKP fuzz seed.");
		File.WriteAllBytes(Path.Combine(pictureDeserialize, "scene.skp"), serialized.ToArray());

		WriteModeSeed(Path.Combine(harfBuzzCallbacks, "shape-basic.bin"), 0x00, "HELLO WORLD");
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

	private static byte[] ReadSeedSource(string seedSources, string name)
	{
		var path = Path.Combine(seedSources, name);
		if (!File.Exists(path))
			throw new FileNotFoundException("Unable to find the fuzz seed source.", path);

		return File.ReadAllBytes(path);
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

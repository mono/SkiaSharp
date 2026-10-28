using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using SkiaSharp.Fuzzing;

var fuzzers = typeof(Fuzzer).Assembly.GetTypes()
	.Where(t => t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(Fuzzer)))
	.Select(t => (Fuzzer)Activator.CreateInstance(t)!)
	.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
	.ToArray();

switch (args)
{
	case ["--prepare", var dropDirectory]:
		Prepare(fuzzers, Path.GetFullPath(dropDirectory), instrument: false);
		return 0;

	case ["--prepare", var dropDirectory, "--instrument"]:
		Prepare(fuzzers, Path.GetFullPath(dropDirectory), instrument: true);
		return 0;

	case [var fuzzerName, .. var rest] when rest.Length <= 1:
		var fuzzer = fuzzers.FirstOrDefault(f => f.Name.Equals(fuzzerName, StringComparison.OrdinalIgnoreCase));
		if (fuzzer is null)
		{
			Console.Error.WriteLine($"Unknown fuzzer '{fuzzerName}'.");
			PrintUsage(fuzzers);
			return 1;
		}

		Run(fuzzer, rest.FirstOrDefault());
		return 0;

	default:
		PrintUsage(fuzzers);
		return 1;
}

static void PrintUsage(Fuzzer[] fuzzers)
{
	var names = string.Join(Environment.NewLine, fuzzers.Select(f => $"  {f.Name}"));
	Console.Error.WriteLine(
		$"""
		Usage:
		  SkiaSharp.Fuzzing <fuzzer-name> [input-file-or-dir]
		  SkiaSharp.Fuzzing --prepare <drop-dir> [--instrument]

		Available fuzzers:
		{names}
		""");
}

static void Run(Fuzzer fuzzer, string? inputPath)
{
	if (!string.IsNullOrEmpty(inputPath))
	{
		var files = Directory.Exists(inputPath)
			? Directory.GetFiles(inputPath)
			: [inputPath];

		foreach (var file in files)
			fuzzer.Fuzz(File.ReadAllBytes(file));

		return;
	}

	SharpFuzz.Fuzzer.LibFuzzer.Run(fuzzer.Fuzz);
}

static void Prepare(Fuzzer[] fuzzers, string dropDirectory, bool instrument)
{
	string[] targetAssemblies =
	[
		"SkiaSharp.dll",
		"SkiaSharp.Resources.dll",
		"SkiaSharp.SceneGraph.dll",
		"SkiaSharp.Skottie.dll",
		"HarfBuzzSharp.dll",
	];

	var buildDirectory = AppContext.BaseDirectory;

	if (instrument)
	{
		CopyDirectory(buildDirectory, dropDirectory);
		foreach (var assembly in targetAssemblies)
			SharpFuzz.Fuzzer.Instrument(Path.Combine(dropDirectory, assembly), static _ => true, SharpFuzz.Options.Value);
		return;
	}

	var platforms = new[]
	{
		(
			Platform: "windows",
			NativeExtension: "dll",
			DependencyPath: ".\\*",
			NativeFiles: new[] { "libSkiaSharp.dll", "libSkiaSharp.pdb", "libHarfBuzzSharp.dll", "libHarfBuzzSharp.pdb" }
		),
		(
			Platform: "linux",
			NativeExtension: "so",
			DependencyPath: "./*",
			NativeFiles: new[] { "libSkiaSharp.so", "libHarfBuzzSharp.so" }
		),
	};

	var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

	foreach (var platform in platforms)
	{
		var platformDirectory = Path.Combine(dropDirectory, platform.Platform);
		Directory.CreateDirectory(platformDirectory);

		foreach (var assembly in targetAssemblies.Append("SkiaSharp.Fuzzing.dll"))
		{
			File.Copy(Path.Combine(buildDirectory, assembly), Path.Combine(platformDirectory, assembly), overwrite: true);
			var pdb = Path.ChangeExtension(assembly, ".pdb");
			File.Copy(Path.Combine(buildDirectory, pdb), Path.Combine(platformDirectory, pdb), overwrite: true);
		}

		foreach (var nativeFile in platform.NativeFiles)
			File.Copy(Path.Combine(buildDirectory, nativeFile), Path.Combine(platformDirectory, nativeFile), overwrite: true);

		CopyDirectory(Path.Combine(buildDirectory, "assets"), Path.Combine(platformDirectory, "assets"));
		CopyDirectory(Path.Combine(buildDirectory, "seeds"), Path.Combine(platformDirectory, "seeds"));

		var entries = new JsonArray();
		foreach (var fuzzer in fuzzers)
		{
			var binaries = new JsonArray(targetAssemblies.Select(a => (JsonNode)a).ToArray())
			{
				$"libSkiaSharp.{platform.NativeExtension}",
				$"libHarfBuzzSharp.{platform.NativeExtension}"
			};

			entries.Add(new JsonObject
			{
				["JobNotificationEmail"] = "dotnet-maui-eng@microsoft.com",
				["Skip"] = false,
				["Fuzzer"] = new JsonObject
				{
					["$type"] = "libfuzzerDotNet",
					["Dll"] = "SkiaSharp.Fuzzing.dll",
					["Class"] = $"{typeof(Fuzzer<>).FullName}[[{fuzzer.GetType().FullName}]]",
					["Method"] = nameof(Fuzzer<>.FuzzTarget),
					["FuzzingTargetBinaries"] = binaries
				},
				["FuzzerTimeoutInSeconds"] = 60,
				["MinAvailableMemoryMB"] = 100,
				["OneFuzzJobs"] = new JsonArray(
					new JsonObject
					{
						["ProjectName"] = "SkiaSharp",
						["TargetName"] = $"{fuzzer.Name}-{platform.Platform}-x64",
						["TargetOptions"] = new JsonArray("-timeout=60"),
						["FuzzingTargetOptions"] = new JsonArray("-timeout=60")
					}),
				["JobDependencies"] = new JsonArray(platform.DependencyPath),
				["PreservePathsJobDependencies"] = new JsonArray("./assets/**/*", "./seeds/**/*"),
				["SdlWorkItemId"] = 3082348
			});
		}

		var config = new JsonObject
		{
			["ConfigVersion"] = 3,
			["Entries"] = entries
		};

		File.WriteAllText(
			Path.Combine(platformDirectory, "OneFuzzConfig.json"),
			config.ToJsonString(jsonOptions));
	}
}

static void CopyDirectory(string sourceDirectory, string destinationDirectory)
{
	Directory.CreateDirectory(destinationDirectory);

	foreach (var file in Directory.GetFiles(sourceDirectory))
		File.Copy(file, Path.Combine(destinationDirectory, Path.GetFileName(file)), overwrite: true);

	foreach (var subDirectory in Directory.GetDirectories(sourceDirectory))
		CopyDirectory(subDirectory, Path.Combine(destinationDirectory, Path.GetFileName(subDirectory)));
}

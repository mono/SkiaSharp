using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SkiaSharp.Fuzzing;

/// <summary>
/// Stages bundled seed files from <c>{setup_dir}/seeds/&lt;target&gt;/</c> into OneFuzz's watched
/// <c>task_inputs_0</c> directory when the assembly loads on a worker VM, so seeds committed to
/// the repository flow into the cloud corpus automatically without manual container uploads.
/// </summary>
internal static class FuzzSeeds
{
	[ModuleInitializer]
	internal static void Initialize()
	{
		var inputsDirectory = Path.Combine(Environment.CurrentDirectory, "task_inputs_0");
		if (!Directory.Exists(inputsDirectory))
			return;

		var targetClass = Environment.GetEnvironmentVariable("LIBFUZZER_DOTNET_TARGET_CLASS")
			?? Environment.GetEnvironmentVariable("LIBFUZZER_DOTNET_TARGET")?.Split(':').ElementAtOrDefault(1);
		if (string.IsNullOrEmpty(targetClass))
			return;

		var type = typeof(IFuzzer).Assembly.GetType(targetClass);
		if (type is null || !type.IsAssignableTo(typeof(IFuzzer)))
			return;

		var fuzzer = FuzzerDescriptor.Create(type);
		var seedsDirectory = Path.Combine(
			Path.GetDirectoryName(typeof(IFuzzer).Assembly.Location)!,
			"seeds",
			fuzzer.Name);
		if (!Directory.Exists(seedsDirectory))
			return;

		foreach (var seedFile in Directory.GetFiles(seedsDirectory))
		{
			var destinationFile = Path.Combine(inputsDirectory, Path.GetFileName(seedFile));
			var tempFile = Path.Combine(Environment.CurrentDirectory, $".seed-{Guid.NewGuid():N}.tmp");
			File.Copy(seedFile, tempFile, overwrite: true);
			File.Move(tempFile, destinationFile, overwrite: true);
		}
	}
}

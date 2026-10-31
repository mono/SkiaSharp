using System.IO;
using System.Threading;

namespace SkiaSharp.Fuzzing;

/// <summary>
/// Stages bundled seed files from <c>{setup_dir}/seeds/&lt;target&gt;/</c> into OneFuzz's watched
/// <c>task_inputs_0</c> directory on worker startup so seeds committed to the repository flow into
/// the cloud corpus automatically.
/// </summary>
internal static class FuzzSeeds
{
	private static int initialized;

	public static void EnsureInitialized(string fuzzerName)
	{
		if (Interlocked.Exchange(ref initialized, 1) != 0)
			return;

		var inputsDirectory = Path.Combine(Environment.CurrentDirectory, "task_inputs_0");
		if (!Directory.Exists(inputsDirectory))
			return;

		var seedsDirectory = Path.Combine(
			Path.GetDirectoryName(typeof(IFuzzer).Assembly.Location)!,
			"seeds",
			fuzzerName);
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

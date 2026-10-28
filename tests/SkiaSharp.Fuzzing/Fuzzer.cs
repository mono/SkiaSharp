using System.IO;
using System.Threading;

namespace SkiaSharp.Fuzzing;

/// <summary>
/// Base class for a SkiaSharp fuzz target.
/// </summary>
public abstract class Fuzzer
{
	private int initialized;

	/// <summary>
	/// Gets the kebab-case identifier used for CLI selection, seed directories, and OneFuzz target names.
	/// </summary>
	public abstract string Name { get; }

	/// <summary>
	/// Stages bundled seeds on first run and executes the fuzz target for a single input buffer.
	/// </summary>
	public void Fuzz(ReadOnlySpan<byte> bytes)
	{
		EnsureSeedsInitialized();
		OnFuzz(bytes);
	}

	/// <summary>
	/// Executes the fuzzer-specific logic for a single input buffer.
	/// </summary>
	protected abstract void OnFuzz(ReadOnlySpan<byte> bytes);

	private void EnsureSeedsInitialized()
	{
		if (Interlocked.Exchange(ref initialized, 1) != 0)
			return;

		var inputsDirectory = Path.Combine(Environment.CurrentDirectory, "task_inputs_0");
		if (!Directory.Exists(inputsDirectory))
			return;

		var seedsDirectory = Path.Combine(
			Path.GetDirectoryName(typeof(Fuzzer).Assembly.Location)!,
			"seeds",
			Name);
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

/// <summary>
/// Generic base class that exposes the static <see cref="FuzzTarget" /> entry point required by OneFuzz's
/// <c>LibFuzzerDotnetLoader</c>.
/// </summary>
public abstract class Fuzzer<TFuzzer> : Fuzzer
	where TFuzzer : Fuzzer<TFuzzer>, new()
{
	private static TFuzzer? instance;

	public static void FuzzTarget(ReadOnlySpan<byte> bytes) =>
		(instance ??= new()).Fuzz(bytes);
}

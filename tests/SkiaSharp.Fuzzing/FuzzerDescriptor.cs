using System.IO;
using System.Threading;

namespace SkiaSharp.Fuzzing;

internal abstract class FuzzerDescriptor
{
	public abstract string Name { get; }

	public abstract string EntryPointClass { get; }

	public abstract void Fuzz(ReadOnlySpan<byte> bytes);

	public static FuzzerDescriptor Create(Type type) =>
		(FuzzerDescriptor)Activator.CreateInstance(typeof(FuzzerDescriptor<>).MakeGenericType(type))!;
}

internal sealed class FuzzerDescriptor<TFuzzer> : FuzzerDescriptor
	where TFuzzer : IFuzzer
{
	private static int initialized;

	public override string Name => TFuzzer.Name;

	public override string EntryPointClass => $"{typeof(FuzzerDescriptor<>).FullName}[[{typeof(TFuzzer).FullName}]]";

	public override void Fuzz(ReadOnlySpan<byte> bytes) =>
		TFuzzer.FuzzTarget(bytes);

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		EnsureSeedsInitialized();
		TFuzzer.FuzzTarget(bytes);
	}

	private static void EnsureSeedsInitialized()
	{
		if (Interlocked.Exchange(ref initialized, 1) != 0)
			return;

		var inputsDirectory = Path.Combine(Environment.CurrentDirectory, "task_inputs_0");
		if (!Directory.Exists(inputsDirectory))
			return;

		var seedsDirectory = Path.Combine(
			Path.GetDirectoryName(typeof(IFuzzer).Assembly.Location)!,
			"seeds",
			TFuzzer.Name);
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

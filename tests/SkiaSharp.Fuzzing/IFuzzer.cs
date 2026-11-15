namespace SkiaSharp.Fuzzing;

/// <summary>
/// Defines a coverage-guided fuzz target discovered and executed by <c>SkiaSharp.Fuzzing</c>.
/// </summary>
public interface IFuzzer
{
	/// <summary>
	/// Gets the kebab-case identifier used for CLI selection, seed directories, and OneFuzz target names.
	/// </summary>
	static abstract string Name { get; }

	/// <summary>
	/// Executes the fuzz target for a single input buffer.
	/// </summary>
	static abstract void FuzzTarget(ReadOnlySpan<byte> bytes);
}

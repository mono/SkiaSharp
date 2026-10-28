namespace SkiaSharp.Fuzzing;

/// <summary>
/// Contract for a coverage-guided fuzz target discovered by <c>SkiaSharp.Fuzzing</c>.
/// Each implementation has a matching seed corpus directory under <c>seeds/&lt;Name&gt;/</c>.
/// </summary>
public interface IFuzzer
{
	static abstract string Name { get; }
	static abstract void FuzzTarget(ReadOnlySpan<byte> bytes);
}

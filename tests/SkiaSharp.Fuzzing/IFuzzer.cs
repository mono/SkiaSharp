namespace SkiaSharp.Fuzzing;

public interface IFuzzer
{
	static abstract string Name { get; }
	static abstract void FuzzTarget(ReadOnlySpan<byte> bytes);
}

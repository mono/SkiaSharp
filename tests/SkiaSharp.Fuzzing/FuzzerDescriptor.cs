using System.Reflection;

namespace SkiaSharp.Fuzzing;

internal sealed record FuzzerDescriptor(string Name, Type Type, Action<ReadOnlySpan<byte>> Target)
{
	public static FuzzerDescriptor Create(Type type)
	{
		var name = (string)type.GetProperty(nameof(IFuzzer.Name), BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
		var method = type.GetMethod(nameof(IFuzzer.FuzzTarget), BindingFlags.Public | BindingFlags.Static, [typeof(ReadOnlySpan<byte>)])!;
		return new FuzzerDescriptor(name, type, method.CreateDelegate<Action<ReadOnlySpan<byte>>>());
	}
}

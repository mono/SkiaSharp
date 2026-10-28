using System.Runtime.InteropServices;
using System.Text;
using HarfBuzzSharp;

namespace SkiaSharp.Fuzzing.Fuzzers;

/// <summary>
/// Fuzzes pure-managed C# parsers and text encoding utilities: <see cref="SKColor.TryParse(string, out SKColor)" />
/// and <see cref="SKColor.TryParse(ReadOnlySpan{char}, out SKColor)" />, <see cref="SKFourByteTag.Parse(string)" />
/// and <see cref="SKFourByteTag.Parse(ReadOnlySpan{char})" />, <see cref="Tag.Parse(string)" /> and
/// <see cref="Tag.Parse(ReadOnlySpan{char})" />, and <see cref="StringUtilities" />.
/// </summary>
public sealed class ManagedParsersFuzzer : IFuzzer
{
	private const int MaximumInputBytes = 4 * 1024;

	public static string Name => "managed-parsers";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		var text = Encoding.UTF8.GetString(bytes);
		var chars = MemoryMarshal.Cast<byte, char>(bytes);

		FuzzColorParser(text, chars);
		FuzzTagParsers(text, chars);
		FuzzStringUtilities(text, bytes);
	}

	private static void FuzzColorParser(string text, ReadOnlySpan<char> chars)
	{
		if (SKColor.TryParse(text, out var color))
			_ = color.ToString();

		if (!chars.IsEmpty && SKColor.TryParse(chars, out var spanColor))
			_ = spanColor.ToString();
	}

	private static void FuzzTagParsers(string text, ReadOnlySpan<char> chars)
	{
		_ = SKFourByteTag.Parse(text).ToString();
		_ = Tag.Parse(text).ToString();

		if (!chars.IsEmpty)
		{
			_ = SKFourByteTag.Parse(chars).ToString();
			_ = Tag.Parse(chars).ToString();
		}
	}

	private static void FuzzStringUtilities(string text, ReadOnlySpan<byte> rawBytes)
	{
		foreach (var encoding in (ReadOnlySpan<SKTextEncoding>)[SKTextEncoding.Utf8, SKTextEncoding.Utf16, SKTextEncoding.Utf32])
		{
			var encoded = StringUtilities.GetEncodedText(text, encoding);
			_ = StringUtilities.GetString(encoded, encoding);
			_ = StringUtilities.GetString(rawBytes, encoding);
		}

		if (text.Length == 1)
		{
			_ = StringUtilities.GetUnicodeCharacterCode(text, SKTextEncoding.Utf8);
			_ = StringUtilities.GetUnicodeCharacterCode(text, SKTextEncoding.Utf16);
		}
		else if (text.Length == 2 && char.IsSurrogatePair(text, 0))
		{
			_ = StringUtilities.GetUnicodeCharacterCode(text, SKTextEncoding.Utf32);
		}
	}
}

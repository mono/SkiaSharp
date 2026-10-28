using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using HarfBuzzSharp;
using HBBlob = HarfBuzzSharp.Blob;
using HBBuffer = HarfBuzzSharp.Buffer;
using HBFace = HarfBuzzSharp.Face;
using HBFont = HarfBuzzSharp.Font;

namespace SkiaSharp.Fuzzing.Fuzzers;

/// <summary>
/// Fuzzes <see cref="HarfBuzzSharp.Buffer" /> text ingestion (span and pooled-string UTF-8, UTF-16, and UTF-32 overloads),
/// <see cref="HarfBuzzSharp.Face(GetTableDelegate, ReleaseDelegate)" /> table callback proxies, <see cref="HarfBuzzSharp.Font.Shape(HarfBuzzSharp.Buffer, HarfBuzzSharp.Feature[])" />,
/// pooled glyph name marshaling (<see cref="HarfBuzzSharp.Font.TryGetGlyphName(uint, out string)" /> and <see cref="HarfBuzzSharp.Font.GlyphToString(uint)" />),
/// and pooled <see cref="HarfBuzzSharp.Buffer.SerializeGlyphs(HarfBuzzSharp.Font, HarfBuzzSharp.SerializeFormat, HarfBuzzSharp.SerializeFlag)" /> /
/// <see cref="HarfBuzzSharp.Buffer.DeserializeGlyphs(string, HarfBuzzSharp.Font, HarfBuzzSharp.SerializeFormat)" />.
/// </summary>
public sealed class HarfBuzzShapingFuzzer : IFuzzer
{
	private const int MaximumInputBytes = 4 * 1024;

	private static readonly Lazy<byte[]> FontBytes = new(LoadFontBytes);

	public static string Name => "harfbuzz-shaping";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		using var blob = CreateOwnedBlob(FontBytes.Value);
		using var baseFace = new HBFace(blob, 0);
		_ = baseFace.Tables;

		using var face = new HBFace((_, tag) => baseFace.ReferenceTable(tag));
		using var font = new HBFont(face);

		ShapeUtf8(font, bytes);
		ShapeUtf16(font, bytes);
		ShapeUtf32(font, bytes);
		ShapeStrings(font, bytes);
		DeserializeGlyphs(font, bytes);
	}

	private static void ShapeUtf8(HBFont font, ReadOnlySpan<byte> bytes)
	{
		using var buffer = new HBBuffer();
		buffer.AddUtf8(bytes);
		ShapeAndSerialize(font, buffer);
	}

	private static void ShapeUtf16(HBFont font, ReadOnlySpan<byte> bytes)
	{
		var chars = MemoryMarshal.Cast<byte, char>(bytes);
		if (chars.IsEmpty)
			return;

		using var buffer = new HBBuffer();
		buffer.AddUtf16(chars);
		ShapeAndSerialize(font, buffer);
	}

	private static void ShapeUtf32(HBFont font, ReadOnlySpan<byte> bytes)
	{
		var utf32 = MemoryMarshal.Cast<byte, uint>(bytes);
		if (utf32.IsEmpty)
			return;

		using var buffer = new HBBuffer();
		buffer.AddUtf32(utf32);
		ShapeAndSerialize(font, buffer);
	}

	private static void ShapeStrings(HBFont font, ReadOnlySpan<byte> bytes)
	{
		var text = Encoding.UTF8.GetString(bytes);

		using (var utf8Buffer = new HBBuffer())
		{
			utf8Buffer.AddUtf8(text);
			ShapeAndSerialize(font, utf8Buffer);
		}

		using (var utf32Buffer = new HBBuffer())
		{
			utf32Buffer.AddUtf32(text);
			ShapeAndSerialize(font, utf32Buffer);
		}
	}

	private static void ShapeAndSerialize(HBFont font, HBBuffer buffer)
	{
		buffer.GuessSegmentProperties();
		font.Shape(buffer);

		var infos = buffer.GetGlyphInfoSpan();
		_ = buffer.GetGlyphPositionSpan().Length;

		if (!infos.IsEmpty)
		{
			_ = font.TryGetGlyphName(infos[0].Codepoint, out _);
			_ = font.GlyphToString(infos[0].Codepoint);
		}

		if (buffer.Length > 0 && buffer.ContentType == ContentType.Glyphs)
		{
			buffer.NormalizeGlyphs();
			_ = buffer.SerializeGlyphs(font, SerializeFormat.Text, SerializeFlag.Default);
			_ = buffer.SerializeGlyphs(font, SerializeFormat.Json, SerializeFlag.Default);
		}
	}

	private static void DeserializeGlyphs(HBFont font, ReadOnlySpan<byte> bytes)
	{
		var text = Encoding.Latin1.GetString(bytes);

		using (var textBuffer = new HBBuffer())
		{
			textBuffer.DeserializeGlyphs(text, font, SerializeFormat.Text);
			_ = textBuffer.Length;
		}

		using (var jsonBuffer = new HBBuffer())
		{
			jsonBuffer.DeserializeGlyphs(text, font, SerializeFormat.Json);
			_ = jsonBuffer.Length;
		}
	}

	private static HBBlob CreateOwnedBlob(byte[] bytes)
	{
		var data = Marshal.AllocHGlobal(bytes.Length);
		Marshal.Copy(bytes, 0, data, bytes.Length);
		return new HBBlob(data, bytes.Length, MemoryMode.ReadOnly, () => Marshal.FreeHGlobal(data));
	}

	private static byte[] LoadFontBytes()
	{
		var fontPath = Path.Combine(Path.GetDirectoryName(typeof(IFuzzer).Assembly.Location)!, "assets", "font.ttf");
		if (!File.Exists(fontPath))
			throw new FileNotFoundException("The HarfBuzz fuzzing font asset is missing.", fontPath);

		return File.ReadAllBytes(fontPath);
	}
}

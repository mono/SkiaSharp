using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using HarfBuzzSharp;
using HBBlob = HarfBuzzSharp.Blob;
using HBBuffer = HarfBuzzSharp.Buffer;
using HBFace = HarfBuzzSharp.Face;
using HBFont = HarfBuzzSharp.Font;

namespace SkiaSharp.Fuzzing.Fuzzers;

public sealed class HarfBuzzShapingFuzzer : IFuzzer
{
	private const int MaximumInputBytes = 64 * 1024;

	private static readonly Lazy<byte[]> FontBytes = new(LoadFontBytes);

	public static string Name => "harfbuzz-shaping";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		var slice = bytes[..Math.Min(bytes.Length, 512)];

		using var blob = CreateOwnedBlob(FontBytes.Value);
		using var face = new HBFace(blob, 0);
		using var font = new HBFont(face);

		ShapeUtf8(font, slice);
		ShapeUtf16(font, slice);
		ShapeUtf32(font, slice);
		DeserializeGlyphs(font, slice);
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

	private static void ShapeAndSerialize(HBFont font, HBBuffer buffer)
	{
		buffer.GuessSegmentProperties();
		font.Shape(buffer);

		_ = buffer.GetGlyphInfoSpan().Length;
		_ = buffer.GetGlyphPositionSpan().Length;

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

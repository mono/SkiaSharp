using System.Collections.Generic;
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
	private const int MaximumInputBytes = 1 * 1024 * 1024;

	private static readonly Lazy<(byte[] FontBytes, KeyValuePair<uint, byte[]>[] Tables)> FontData = new(LoadFontData);

	public static string Name => "harfbuzz-shaping";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		var payload = bytes.ToArray();
		var selector = payload[0];

		FuzzBufferAndSerialization(payload, selector);
		FuzzMutatedTableCallback(payload, selector);
		FuzzFontAndUnicodeCallbacks(payload, selector);
	}

	private static void FuzzBufferAndSerialization(byte[] payload, byte selector)
	{
		var (fontBytes, _) = FontData.Value;
		using var blob = CreateOwnedBlob(fontBytes);
		using var face = new HBFace(blob, 0);
		using var font = new HBFont(face);

		using var buffer = new HBBuffer();
		var slice = payload.AsSpan(0, Math.Min(payload.Length, 256));

		switch (selector % 3)
		{
			case 0:
				buffer.AddUtf8(slice);
				break;
			case 1:
				var chars = MemoryMarshal.Cast<byte, char>(slice);
				if (!chars.IsEmpty)
					buffer.AddUtf16(chars);
				else
					buffer.AddUtf8(slice);
				break;
			case 2:
				var utf32 = MemoryMarshal.Cast<byte, uint>(slice);
				if (!utf32.IsEmpty)
					buffer.AddUtf32(utf32);
				else
					buffer.AddUtf8(slice);
				break;
		}

		buffer.GuessSegmentProperties();

		if ((selector & 8) != 0)
			buffer.Reverse();
		if ((selector & 16) != 0)
			buffer.ReverseClusters();

		var featureText = Encoding.ASCII.GetString(payload, 0, Math.Min(payload.Length, 32));
		if (Feature.TryParse(featureText, out var feature))
			font.Shape(buffer, feature);
		else
			font.Shape(buffer);

		_ = buffer.GetGlyphInfoSpan().Length;
		_ = buffer.GetGlyphPositionSpan().Length;

		if (buffer.Length > 0 && buffer.ContentType == ContentType.Glyphs)
		{
			buffer.NormalizeGlyphs();
			_ = buffer.SerializeGlyphs(font, (selector & 4) == 0 ? SerializeFormat.Text : SerializeFormat.Json, SerializeFlag.Default);
		}

		var serializedInput = Encoding.Latin1.GetString(slice);
		using (var deserializeBuffer = new HBBuffer())
		{
			deserializeBuffer.DeserializeGlyphs(serializedInput, font, SerializeFormat.Text);
			_ = deserializeBuffer.Length;
		}
		using (var deserializeJsonBuffer = new HBBuffer())
		{
			deserializeJsonBuffer.DeserializeGlyphs(serializedInput, font, SerializeFormat.Json);
			_ = deserializeJsonBuffer.Length;
		}
	}

	private static void FuzzMutatedTableCallback(byte[] payload, byte selector)
	{
		var (_, tables) = FontData.Value;
		if (tables.Length == 0)
			return;

		var mutatedTableIndex = selector % tables.Length;
		var mutatedTag = tables[mutatedTableIndex].Key;

		using var face = new HBFace(
			(_, tag) =>
			{
				var tagValue = (uint)tag;
				for (var i = 0; i < tables.Length; i++)
				{
					if (tables[i].Key != tagValue)
						continue;

					var source = tables[i].Value;
					if (tagValue != mutatedTag)
						return CreateOwnedBlob(source);

					var length = Math.Min(Math.Max(payload.Length, 1), source.Length);
					var mutated = new byte[length];
					System.Buffer.BlockCopy(source, 0, mutated, 0, length);
					for (var b = 0; b < Math.Min(payload.Length, length); b++)
						mutated[b] ^= payload[b];

					return CreateOwnedBlob(mutated);
				}

				return null;
			},
			() => { });

		using var font = new HBFont(face);
		using var buffer = new HBBuffer();
		buffer.AddUtf8("Hello SkiaSharp! مرحبا "u8);
		buffer.AddUtf8(payload.AsSpan(0, Math.Min(payload.Length, 64)));
		buffer.GuessSegmentProperties();
		font.Shape(buffer);
		_ = buffer.Length;
	}

	private static void FuzzFontAndUnicodeCallbacks(byte[] payload, byte selector)
	{
		var (fontBytes, _) = FontData.Value;
		using var blob = CreateOwnedBlob(fontBytes);
		using var face = new HBFace(blob, 0);
		using var parentFont = new HBFont(face);
		using var subFont = new HBFont(parentFont);

		using var fontFunctions = new FontFunctions();
		fontFunctions.SetNominalGlyphDelegate((HBFont _, object? _, uint unicode, out uint glyph) =>
		{
			glyph = (unicode ^ selector) & 0x7fu;
			return glyph != 0;
		});
		fontFunctions.SetHorizontalGlyphAdvanceDelegate((HBFont _, object? _, uint glyph) =>
			(int)((glyph * 17u + selector) & 0x3ffu) - 128);
		fontFunctions.SetGlyphExtentsDelegate((HBFont _, object? _, uint glyph, out GlyphExtents extents) =>
		{
			extents = new GlyphExtents
			{
				XBearing = (int)(glyph & 0x1f),
				YBearing = (int)((glyph >> 1) & 0x1f),
				Width = (int)(10 + (glyph & 0x3f)),
				Height = -(int)(10 + (glyph & 0x3f)),
			};
			return true;
		});
		fontFunctions.MakeImmutable();
		subFont.SetFontFunctions(fontFunctions);

		using var unicodeFunctions = new UnicodeFunctions(UnicodeFunctions.Default);
		unicodeFunctions.SetCombiningClassDelegate((_, u) =>
			(UnicodeCombiningClass)((u + selector) & 0xff));
		unicodeFunctions.SetMirroringDelegate((_, u) => u ^ 1u);
		unicodeFunctions.MakeImmutable();

		using var buffer = new HBBuffer
		{
			UnicodeFunctions = unicodeFunctions,
		};
		buffer.AddUtf8(payload.AsSpan(0, Math.Min(payload.Length, 64)));
		buffer.GuessSegmentProperties();
		subFont.Shape(buffer);
		_ = buffer.Length;
	}

	private static HBBlob CreateOwnedBlob(byte[] bytes)
	{
		if (bytes.Length == 0)
			return HBBlob.Empty;

		var data = Marshal.AllocHGlobal(bytes.Length);
		Marshal.Copy(bytes, 0, data, bytes.Length);
		return new HBBlob(data, bytes.Length, MemoryMode.ReadOnly, () => Marshal.FreeHGlobal(data));
	}

	private static (byte[] FontBytes, KeyValuePair<uint, byte[]>[] Tables) LoadFontData()
	{
		var fontPath = Path.Combine(Path.GetDirectoryName(typeof(IFuzzer).Assembly.Location)!, "assets", "font.ttf");
		if (!File.Exists(fontPath))
			throw new FileNotFoundException("The HarfBuzz fuzzing font asset is missing.", fontPath);

		var fontBytes = File.ReadAllBytes(fontPath);
		var tables = new List<KeyValuePair<uint, byte[]>>();
		using var blob = HBBlob.FromFile(fontPath);
		using var face = new HBFace(blob, 0);
		foreach (var tag in face.Tables)
		{
			using var table = face.ReferenceTable(tag);
			var span = table.AsSpan();
			if (span.IsEmpty)
				continue;

			tables.Add(new KeyValuePair<uint, byte[]>((uint)tag, span.ToArray()));
		}

		return (fontBytes, tables.ToArray());
	}
}

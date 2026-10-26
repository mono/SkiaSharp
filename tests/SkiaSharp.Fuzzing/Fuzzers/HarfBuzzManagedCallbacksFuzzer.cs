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

public sealed class HarfBuzzManagedCallbacksFuzzer : IFuzzer
{
	private const int MaximumInputBytes = 1 * 1024 * 1024;

	private static readonly Tag[] HarfBuzzTags =
	[
		Tag.Parse("cmap"),
		Tag.Parse("head"),
		Tag.Parse("hhea"),
		Tag.Parse("hmtx"),
		Tag.Parse("name"),
		Tag.Parse("post"),
	];

	private static readonly Lazy<Dictionary<uint, byte[]>> HarfBuzzTableBytes = new(LoadHarfBuzzTableBytes);

	public static string Name => "harfbuzz-managed-callbacks";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		var payload = bytes.ToArray();
		var text = GetFuzzText(payload);

		TestNormalCallbacks(payload, text);
		TestTruncatedCallbacks(payload, text);
		TestMissingTagCallbacks(payload, text);
		TestReverseDisposeOrder(payload, text);
	}

	private static void TestNormalCallbacks(byte[] payload, string text)
	{
		ExerciseCallbacks(text, (_, tag) => CreateFuzzTable(tag, payload, truncate: false, dropEvenTags: false), reverseDispose: false);
	}

	private static void TestTruncatedCallbacks(byte[] payload, string text)
	{
		ExerciseCallbacks(text, (_, tag) => CreateFuzzTable(tag, payload, truncate: true, dropEvenTags: false), reverseDispose: false);
	}

	private static void TestMissingTagCallbacks(byte[] payload, string text)
	{
		ExerciseCallbacks(text, (_, tag) => CreateFuzzTable(tag, payload, truncate: false, dropEvenTags: true), reverseDispose: false);
	}

	private static void TestReverseDisposeOrder(byte[] payload, string text)
	{
		ExerciseCallbacks(text, (_, tag) => CreateFuzzTable(tag, payload, truncate: false, dropEvenTags: false), reverseDispose: true);
	}

	private static void ExerciseCallbacks(string text, GetTableDelegate getTable, bool reverseDispose)
	{
		HBFace? face = null;
		HBFont? font = null;

		try
		{
			face = new HBFace(getTable, () => { });
			font = new HBFont(face);

			using var buffer = new HBBuffer();
			buffer.AddUtf8(text);
			buffer.GuessSegmentProperties();

			font.Shape(buffer);
			_ = buffer.Length;

			foreach (var tag in HarfBuzzTags)
			{
				using var table = face.ReferenceTable(tag);
				_ = table.Length;
			}
		}
		finally
		{
			if (!reverseDispose)
			{
				font?.Dispose();
				face?.Dispose();
			}
			else
			{
				face?.Dispose();
				font?.Dispose();
			}
		}
	}

	private static HBBlob? CreateFuzzTable(Tag tag, byte[] payload, bool truncate, bool dropEvenTags)
	{
		if (dropEvenTags && (((uint)tag) & 1u) == 0)
			return null;

		if (!HarfBuzzTableBytes.Value.TryGetValue(tag, out var source))
			return null;

		var length = source.Length;
		if (truncate && source.Length > 1)
		{
			var cap = 1 + (payload[0] % Math.Min(source.Length, 255));
			length = Math.Max(1, Math.Min(source.Length, cap));
		}

		var bytes = new byte[length];
		System.Buffer.BlockCopy(source, 0, bytes, 0, length);
		return CreateOwnedBlob(bytes);
	}

	private static HBBlob CreateOwnedBlob(byte[] bytes)
	{
		if (bytes.Length == 0)
			return HBBlob.Empty;

		var data = Marshal.AllocHGlobal(bytes.Length);
		Marshal.Copy(bytes, 0, data, bytes.Length);
		return new HBBlob(data, bytes.Length, MemoryMode.ReadOnly, () => Marshal.FreeHGlobal(data));
	}

	private static Dictionary<uint, byte[]> LoadHarfBuzzTableBytes()
	{
		var tables = new Dictionary<uint, byte[]>();
		var fontPath = Path.Combine(Path.GetDirectoryName(typeof(IFuzzer).Assembly.Location)!, "assets", "font.ttf");
		if (!File.Exists(fontPath))
			throw new FileNotFoundException("The HarfBuzz fuzzing font asset is missing.", fontPath);

		using var blob = HBBlob.FromFile(fontPath);
		using var face = new HBFace(blob, 0);
		foreach (var tag in face.Tables)
		{
			using var table = face.ReferenceTable(tag);
			var span = table.AsSpan();
			if (span.Length == 0)
				continue;

			tables[(uint)tag] = span.ToArray();
		}

		return tables;
	}

	private static string GetFuzzText(byte[] payload)
	{
		var text = Encoding.UTF8.GetString(payload, 0, Math.Min(payload.Length, 128));
		return string.IsNullOrWhiteSpace(text) ? "SkiaSharp fuzz" : text;
	}
}

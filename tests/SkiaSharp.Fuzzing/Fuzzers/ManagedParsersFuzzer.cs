using System.Runtime.InteropServices;
using System.Text;
using HarfBuzzSharp;

namespace SkiaSharp.Fuzzing.Fuzzers;

/// <summary>
/// Fuzzes managed string and span parsers: <see cref="SKColor.TryParse(string, out SKColor)" />,
/// <see cref="SKPath.ParseSvgPathData(string)" /> (plus <see cref="SKPath.ToSvgPathData()" />, path iteration, and rendering),
/// <see cref="Tag.Parse(string)" />, and <see cref="Feature.TryParse(string, out Feature)" />.
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

		FuzzColorParser(text, bytes);
		FuzzSvgPathParser(text);
		FuzzHarfBuzzParsers(text);
	}

	private static void FuzzColorParser(string text, ReadOnlySpan<byte> rawBytes)
	{
		if (SKColor.TryParse(text, out var color))
			_ = color.ToString();

		var charSpan = MemoryMarshal.Cast<byte, char>(rawBytes);
		if (!charSpan.IsEmpty)
			_ = SKColor.TryParse(charSpan, out _);
	}

	private static void FuzzSvgPathParser(string text)
	{
		using var path = SKPath.ParseSvgPathData(text);
		if (path is null)
			return;

		_ = path.Bounds;
		_ = path.TightBounds;
		_ = path.ToSvgPathData();

		using var iterator = path.CreateRawIterator();
		var points = new SKPoint[4];
		while (iterator.Next(points) != SKPathVerb.Done)
		{
		}

		using var bitmap = new SKBitmap(64, 64);
		using var canvas = new SKCanvas(bitmap);
		using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.StrokeAndFill };
		canvas.DrawPath(path, paint);
	}

	private static void FuzzHarfBuzzParsers(string text)
	{
		_ = Tag.Parse(text).ToString();

		if (Feature.TryParse(text, out var feature))
			_ = feature.ToString();
	}
}

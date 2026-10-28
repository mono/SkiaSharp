namespace SkiaSharp.Fuzzing.Fuzzers;

public sealed class TypefaceStreamsFuzzer : IFuzzer
{
	private const int MaximumInputBytes = 1 * 1024 * 1024;

	public static string Name => "typeface-streams";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		var payload = bytes.ToArray();
		var seekable = (payload[^1] & 1) == 0;

		using var stream = new FuzzStream(payload, seekable);
		using var typeface = SKFontManager.Default.CreateTypeface(stream);
		if (typeface is null)
			return;

		_ = typeface.FamilyName;
		_ = typeface.FontWeight;
		_ = typeface.FontWidth;
		_ = typeface.FontSlant;
		_ = typeface.IsFixedPitch;
		_ = typeface.UnitsPerEm;
		_ = typeface.GlyphCount;

		var tags = typeface.GetTableTags();
		var tableLimit = Math.Min(tags.Length, 8);
		for (var i = 0; i < tableLimit; i++)
		{
			if (typeface.TryGetTableData(tags[i], out var tableData))
				_ = tableData.Length;
		}

		using (var fontStream = typeface.OpenStream(out _))
		{
			if (fontStream is not null)
				_ = fontStream.Length;
		}

		Span<byte> sampleBytes = stackalloc byte[80];
		"Hello SkiaSharp 123! "u8.CopyTo(sampleBytes);
		var tailLength = Math.Min(payload.Length, sampleBytes.Length - 21);
		payload.AsSpan(payload.Length - tailLength, tailLength).CopyTo(sampleBytes[21..]);
		sampleBytes = sampleBytes[..(21 + tailLength)];

		using var font = new SKFont(typeface, 16f);
		var glyphCount = font.CountGlyphs(sampleBytes, SKTextEncoding.Utf8);
		if (glyphCount > 0 && glyphCount <= 128)
		{
			Span<ushort> glyphs = stackalloc ushort[glyphCount];
			Span<float> widths = stackalloc float[glyphCount];
			Span<SKPoint> positions = stackalloc SKPoint[glyphCount];

			font.GetGlyphs(sampleBytes, SKTextEncoding.Utf8, glyphs);
			font.GetGlyphWidths(glyphs, widths, Span<SKRect>.Empty);
			font.GetGlyphPositions(glyphs, positions);
			_ = font.MeasureText(glyphs);
		}

		using var path = font.GetTextPath(sampleBytes, SKTextEncoding.Utf8, SKPoint.Empty);
		_ = path?.Bounds;

		using var blob = SKTextBlob.Create(sampleBytes, SKTextEncoding.Utf8, font);
		if (blob is not null)
		{
			_ = blob.Bounds;
			using var bitmap = new SKBitmap(64, 32);
			using var canvas = new SKCanvas(bitmap);
			using var paint = new SKPaint();
			canvas.DrawText(blob, 0, 20, paint);
		}
	}
}

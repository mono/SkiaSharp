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

		Fuzz(payload, seekable: true);
		Fuzz(payload, seekable: false);
	}

	private static void Fuzz(byte[] payload, bool seekable)
	{
		using var stream = new FuzzStream(payload, seekable);
		using var typeface = SKFontManager.Default.CreateTypeface(stream);
		if (typeface is null)
			return;

		_ = typeface.FamilyName;
		_ = typeface.UnitsPerEm;

		var tags = typeface.GetTableTags();
		for (var i = 0; i < Math.Min(tags.Length, 8); i++)
		{
			if (typeface.TryGetTableData(tags[i], out var tableData))
				_ = tableData.Length;
		}

		using (var fontStream = typeface.OpenStream(out _))
		{
			if (fontStream is not null)
				_ = fontStream.Length;
		}

		var glyphCount = Math.Min(typeface.GlyphCount, 16);
		if (glyphCount <= 0)
			return;

		Span<ushort> glyphs = stackalloc ushort[glyphCount];
		for (var i = 0; i < glyphCount; i++)
			glyphs[i] = (ushort)i;

		Span<float> widths = stackalloc float[glyphCount];
		Span<SKPoint> positions = stackalloc SKPoint[glyphCount];

		using var font = new SKFont(typeface, 16f);
		font.GetGlyphWidths(glyphs, widths, Span<SKRect>.Empty);
		font.GetGlyphPositions(glyphs, positions);

		using var glyphPath = font.GetGlyphPath(glyphs[0]);
		_ = glyphPath?.Bounds;

		using var builder = new SKTextBlobBuilder();
		builder.AddPositionedRun(glyphs, font, positions);
		using var blob = builder.Build();
		if (blob is not null)
		{
			using var bitmap = new SKBitmap(64, 32);
			using var canvas = new SKCanvas(bitmap);
			using var paint = new SKPaint();
			canvas.DrawText(blob, 0, 20, paint);
		}
	}
}

namespace SkiaSharp.Fuzzing.Fuzzers;

/// <summary>
/// Fuzzes <see cref="SKFontManager.CreateTypeface(Stream)" /> over seekable and non-seekable managed streams,
/// font table and variation axis marshaling (<see cref="SKTypeface" />), glyph metrics, kerning, and path callbacks
/// (<see cref="SKFont.GetGlyphPaths" /> and <see cref="SKFont.GetTextPathOnPath(ReadOnlySpan{ushort}, ReadOnlySpan{float}, ReadOnlySpan{SKPoint}, SKPath, SKTextAlign)" />),
/// and <see cref="SKTextBlobBuilder" /> / <see cref="SKTextBlob.GetIntercepts(float, float, SKPaint)" />.
/// </summary>
public sealed class TypefaceStreamsFuzzer : IFuzzer
{
	private const int MaximumInputBytes = 1 * 1024 * 1024;

	public static string Name => "typeface-streams";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		var payload = bytes.ToArray();

		FuzzAllModes(payload, maximumReadLength: int.MaxValue, earlyEndPosition: -1);
		FuzzAllModes(payload, maximumReadLength: 8, earlyEndPosition: -1);
		FuzzAllModes(payload, maximumReadLength: int.MaxValue, earlyEndPosition: payload.Length / 2);
	}

	private static void FuzzAllModes(byte[] payload, int maximumReadLength, int earlyEndPosition)
	{
		Fuzz(payload, seekable: true, maximumReadLength, earlyEndPosition);
		Fuzz(payload, seekable: false, maximumReadLength, earlyEndPosition);
	}

	private static void Fuzz(byte[] payload, bool seekable, int maximumReadLength, int earlyEndPosition)
	{
		using var stream = new FuzzStream(payload, seekable, maximumReadLength, earlyEndPosition);
		using var typeface = SKFontManager.Default.CreateTypeface(stream);
		if (typeface is null)
			return;

		_ = typeface.FamilyName;
		_ = typeface.UnitsPerEm;

		if (typeface.TryGetTableTags(out var tags))
		{
			for (var i = 0; i < Math.Min(tags.Length, 8); i++)
			{
				if (typeface.TryGetTableData(tags[i], out var tableData))
					_ = tableData.Length;
			}
		}

		Span<SKFontVariationAxis> axisBuffer = stackalloc SKFontVariationAxis[1];
		_ = typeface.GetVariationDesignParameters(axisBuffer);
		_ = typeface.VariationDesignParameters;

		Span<SKFontVariationPositionCoordinate> coordBuffer = stackalloc SKFontVariationPositionCoordinate[1];
		_ = typeface.GetVariationDesignPosition(coordBuffer);
		_ = typeface.VariationDesignPosition;

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

		Span<int> kerning = stackalloc int[glyphCount];
		_ = typeface.GetKerningPairAdjustments(glyphs, kerning);

		Span<float> widths = stackalloc float[glyphCount];
		Span<SKRect> bounds = stackalloc SKRect[glyphCount];
		Span<SKPoint> positions = stackalloc SKPoint[glyphCount];
		Span<float> offsets = stackalloc float[glyphCount];

		using var font = new SKFont(typeface, 16f);
		font.GetGlyphWidths(glyphs, widths, bounds);
		font.GetGlyphPositions(glyphs, positions);
		font.GetGlyphOffsets(glyphs, offsets);

		font.GetGlyphPaths(glyphs.Slice(0, Math.Min(glyphCount, 4)), static (path, matrix) =>
		{
			_ = path?.Bounds;
			_ = matrix.ScaleX;
		});

		using (var followBuilder = new SKPathBuilder())
		{
			followBuilder.MoveTo(0, 0);
			followBuilder.QuadTo(32, 16, 64, 0);
			using var followPath = followBuilder.Detach();
			using var morphedPath = font.GetTextPathOnPath(glyphs.Slice(0, Math.Min(glyphCount, 4)), widths.Slice(0, Math.Min(glyphCount, 4)), positions.Slice(0, Math.Min(glyphCount, 4)), followPath);
			_ = morphedPath.Bounds;
		}

		using var builder = new SKTextBlobBuilder();
		builder.AddPositionedRun(glyphs, font, positions);
		using var blob = builder.Build();
		if (blob is not null)
		{
			_ = blob.GetIntercepts(0, 20);
			using var bitmap = new SKBitmap(64, 32);
			using var canvas = new SKCanvas(bitmap);
			using var paint = new SKPaint();
			canvas.DrawText(blob, 0, 20, paint);
		}
	}
}

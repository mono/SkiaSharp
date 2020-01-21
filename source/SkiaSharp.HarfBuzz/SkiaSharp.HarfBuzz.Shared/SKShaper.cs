using System;

using HarfBuzzSharp;
using Buffer = HarfBuzzSharp.Buffer;

namespace SkiaSharp.HarfBuzz
{
	public class SKShaper : IDisposable
	{
		internal const int FONT_SIZE_SCALE = 512;

		private readonly Font hbFont;
		private readonly Buffer hbBuffer;

		public SKShaper(SKTypeface typeface)
		{
			Typeface = typeface ?? throw new ArgumentNullException(nameof(typeface));

			using var blob = Typeface.OpenStream(out var index).ToHarfBuzzBlob();
			using var face = new Face(blob, index);
			face.Index = index;
			face.UnitsPerEm = Typeface.UnitsPerEm;

			hbFont = new Font(face);
			hbFont.SetScale(FONT_SIZE_SCALE, FONT_SIZE_SCALE);
			hbFont.SetFunctionsOpenType();

			hbBuffer = new Buffer();
		}

		public SKTypeface Typeface { get; }

		public void Dispose()
		{
			hbFont?.Dispose();
			hbBuffer?.Dispose();
		}

		public Result Shape(Buffer buffer, SKFont font) =>
			Shape(buffer, 0, 0, font);

		public Result Shape(Buffer buffer, float xOffset, float yOffset, SKFont font)
		{
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (font == null)
				throw new ArgumentNullException(nameof(font));

			// do the shaping
			hbFont.Shape(buffer);

			// get the shaping results
			var len = buffer.Length;
			var info = buffer.GetGlyphInfoSpan();
			var pos = buffer.GetGlyphPositionSpan();

			// get the sizes
			float textSizeY = font.Size / FONT_SIZE_SCALE;
			float textSizeX = textSizeY * font.ScaleX;

			var points = new SKPoint[len];
			var clusters = new uint[len];
			var codepoints = new ushort[len];

			for (var i = 0; i < len; i++)
			{
				codepoints[i] = (ushort)info[i].Codepoint;

				clusters[i] = info[i].Cluster;

				points[i] = new SKPoint(
					xOffset + pos[i].XOffset * textSizeX,
					yOffset - pos[i].YOffset * textSizeY);

				// move the cursor
				xOffset += pos[i].XAdvance * textSizeX;
				yOffset += pos[i].YAdvance * textSizeY;
			}

			return new Result(codepoints, clusters, points);
		}

		public Result Shape(string text, SKFont font) =>
			Shape(text.AsSpan(), 0, 0, font);

		public Result Shape(string text, float xOffset, float yOffset, SKFont font) =>
			Shape(text.AsSpan(), xOffset, yOffset, font);

		public Result Shape(ReadOnlySpan<char> text, SKFont font) =>
			Shape(text, 0, 0, font);

		public Result Shape(ReadOnlySpan<char> text, float xOffset, float yOffset, SKFont font)
		{
			if (text.IsEmpty)
				return new Result();

			using var buffer = new Buffer();
			buffer.AddUtf16(text);
			buffer.GuessSegmentProperties();

			return Shape(buffer, xOffset, yOffset, font);
		}

		public Result Shape(ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font) =>
			Shape(text, encoding, 0, 0, font);

		public Result Shape(ReadOnlySpan<byte> text, SKTextEncoding encoding, float xOffset, float yOffset, SKFont font)
		{
			if (text.IsEmpty)
				return new Result();

			using var buffer = new Buffer();
			switch (encoding)
			{
				case SKTextEncoding.Utf8:
					buffer.AddUtf8(text);
					break;
				case SKTextEncoding.Utf16:
					buffer.AddUtf16(text);
					break;
				case SKTextEncoding.Utf32:
					buffer.AddUtf32(text);
					break;
				case SKTextEncoding.GlyphId:
				default:
					throw new NotSupportedException("SKTextEncoding of type GlyphId is not supported.");
			}
			buffer.GuessSegmentProperties();

			return Shape(buffer, xOffset, yOffset, font);
		}

		public class Result
		{
			public Result()
			{
				Codepoints = new ushort[0];
				Clusters = new uint[0];
				Points = new SKPoint[0];
			}

			public Result(ushort[] codepoints, uint[] clusters, SKPoint[] points)
			{
				Codepoints = codepoints;
				Clusters = clusters;
				Points = points;
			}

			public ushort[] Codepoints { get; }

			public uint[] Clusters { get; }

			public SKPoint[] Points { get; }
		}
	}
}

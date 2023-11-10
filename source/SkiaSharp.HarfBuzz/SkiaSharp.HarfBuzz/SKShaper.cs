using System;

using HarfBuzzSharp;
using Buffer = HarfBuzzSharp.Buffer;

namespace SkiaSharp.HarfBuzz
{
	public class SKShaper : IDisposable
	{
		internal const int FONT_SIZE_SCALE = 512;

		private Font hbFont;
		private Buffer hbBuffer;

		public SKShaper(SKTypeface typeface)
		{
			Typeface = typeface ?? throw new ArgumentNullException(nameof(typeface));

			int index;
			using (var blob = Typeface.OpenStream(out index).ToHarfBuzzBlob())
			using (var face = new Face(blob, index))
			{
				face.Index = index;
				face.UnitsPerEm = Typeface.UnitsPerEm;

				hbFont = new Font(face);
				hbFont.SetScale(FONT_SIZE_SCALE, FONT_SIZE_SCALE);

				hbFont.SetFunctionsOpenType();
			}

			hbBuffer = new Buffer();
		}

		public SKTypeface Typeface { get; private set; }

		public void Dispose()
		{
			hbFont?.Dispose();
			hbBuffer?.Dispose();
		}

		[Obsolete("Use Shape(Buffer buffer, SKFont font) instead.")]
		public Result Shape(Buffer buffer, SKPaint paint) =>
			Shape(buffer, 0, 0, paint.GetFont());

		[Obsolete("Use Shape(Buffer buffer, float xOffset, float yOffset, SKFont font) instead.")]
		public Result Shape(Buffer buffer, float xOffset, float yOffset, SKPaint paint) =>
			Shape(buffer, xOffset, yOffset, paint.GetFont());

		public Result Shape(Buffer buffer, SKFont font) =>
			Shape(buffer, 0, 0, font);

		public Result Shape(Buffer buffer, float xOffset, float yOffset, SKFont font)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			if (font == null)
			{
				throw new ArgumentNullException(nameof(font));
			}

			// do the shaping
			hbFont.Shape(buffer);

			// get the shaping results
			var len = buffer.Length;
			var info = buffer.GlyphInfos;
			var pos = buffer.GlyphPositions;

			// get the sizes
			float textSizeY = font.Size / FONT_SIZE_SCALE;
			float textSizeX = textSizeY * font.ScaleX;

			var points = new SKPoint[len];
			var clusters = new uint[len];
			var codepoints = new uint[len];
			var xOffsetStart = xOffset;

			for (var i = 0; i < len; i++)
			{
				codepoints[i] = info[i].Codepoint;

				clusters[i] = info[i].Cluster;

				points[i] = new SKPoint(
					xOffset + pos[i].XOffset * textSizeX,
					yOffset - pos[i].YOffset * textSizeY);

				// move the cursor
				xOffset += pos[i].XAdvance * textSizeX;
				yOffset += pos[i].YAdvance * textSizeY;
			}

			var width = xOffset - xOffsetStart;

			return new Result(codepoints, clusters, points, width);
		}

		[Obsolete("Use Shape(string text, SKFont font) instead.")]
		public Result Shape(string text, SKPaint paint) =>
			Shape(text, 0, 0, paint.GetFont());

		[Obsolete("Use Shape(string text, float xOffset, float yOffset, SKFont font) instead.")]
		public Result Shape(string text, float xOffset, float yOffset, SKPaint paint)
		{
			if (string.IsNullOrEmpty(text))
			{
				return new Result();
			}

			using var buffer = new Buffer();

			switch (paint.TextEncoding)
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
				default:
					throw new NotSupportedException("TextEncoding of type GlyphId is not supported.");
			}

			buffer.GuessSegmentProperties();

			return Shape(buffer, xOffset, yOffset, paint);
		}

		public Result Shape(string text, SKFont font) =>
			Shape(text, 0, 0, font);

		public Result Shape(string text, float xOffset, float yOffset, SKFont font)
		{
			if (string.IsNullOrEmpty(text))
			{
				return new Result();
			}

			using var buffer = new Buffer();
			buffer.AddUtf8(text);

			buffer.GuessSegmentProperties();

			return Shape(buffer, xOffset, yOffset, font);
		}

		public class Result
		{
			public Result()
			{
				Codepoints = Array.Empty<uint>();
				Clusters = Array.Empty<uint>();
				Points = Array.Empty<SKPoint>();
				Width = 0f;
			}

			public Result(uint[] codepoints, uint[] clusters, SKPoint[] points)
			{
				Codepoints = codepoints;
				Clusters = clusters;
				Points = points;
				Width = 0;
			}

			public Result(uint[] codepoints, uint[] clusters, SKPoint[] points, float width)
			{
				Codepoints = codepoints;
				Clusters = clusters;
				Points = points;
				Width = width;
			}

			public uint[] Codepoints { get; }

			public uint[] Clusters { get; }

			public SKPoint[] Points { get; }

			public float Width { get; }
		}
	}
}

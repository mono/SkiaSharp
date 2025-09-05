using System;

using HarfBuzzSharp;
using Buffer = HarfBuzzSharp.Buffer;

namespace SkiaSharp.HarfBuzz
{
	/// <summary>
	/// Encapsulates basic text shaping.
	/// </summary>
	public class SKShaper : IDisposable
	{
		internal const int FONT_SIZE_SCALE = 512;

		private Font hbFont;
		private Buffer hbBuffer;

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.HarfBuzz.SKShaper" /> instance using the specified typeface.
		/// </summary>
		/// <param name="typeface">The typeface to use for the text shaping.</param>
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

		/// <summary>
		/// Gets the typeface used when creating the shaper.
		/// </summary>
		public SKTypeface Typeface { get; private set; }

		/// <summary>
		/// Releases all resources used by this <see cref="T:SkiaSharp.HarfBuzz.SKShaper" />.
		/// </summary>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.HarfBuzz.SKShaper" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		public void Dispose()
		{
			hbFont?.Dispose();
			hbBuffer?.Dispose();
		}

		/// <param name="buffer"></param>
		/// <param name="paint"></param>
		[Obsolete("Use Shape(Buffer buffer, SKFont font) instead.")]
		public Result Shape(Buffer buffer, SKPaint paint) =>
			Shape(buffer, 0, 0, paint.GetFont());

		/// <param name="buffer"></param>
		/// <param name="xOffset"></param>
		/// <param name="yOffset"></param>
		/// <param name="paint"></param>
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

		/// <summary>
		/// Shapes the specified text using the properties from the paint.
		/// </summary>
		/// <param name="text">The text to shape.</param>
		/// <param name="paint">The paint to use.</param>
		/// <returns>Returns the results of the shaping operation.</returns>
		[Obsolete("Use Shape(string text, SKFont font) instead.")]
		public Result Shape(string text, SKPaint paint) =>
			Shape(text, 0, 0, paint.GetFont());

		/// <summary>
		/// Shapes the specified text using the properties from the paint.
		/// </summary>
		/// <param name="text">The text to shape.</param>
		/// <param name="xOffset">The x-offset to use when creating the shaping result.</param>
		/// <param name="yOffset">The y-offset to use when creating the shaping result.</param>
		/// <param name="paint">The paint to use.</param>
		/// <returns>Returns the results of the shaping operation.</returns>
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

		/// <summary>
		/// Represents the result of a shaping operation.
		/// </summary>
		public class Result
		{
			/// <summary>
			/// Creates a new <see cref="T:SkiaSharp.HarfBuzz.SKShaper.Result" /> instance using empty values.
			/// </summary>
			public Result()
			{
				Codepoints = new uint[0];
				Clusters = new uint[0];
				Points = new SKPoint[0];
				Width = 0f;
			}

			/// <summary>
			/// Creates a new <see cref="T:SkiaSharp.HarfBuzz.SKShaper.Result" /> instance using the specified values.
			/// </summary>
			/// <param name="codepoints">The glyph Unicode code points.</param>
			/// <param name="clusters">The glyph clusters.</param>
			/// <param name="points">The glyph positions.</param>
			public Result(uint[] codepoints, uint[] clusters, SKPoint[] points)
			{
				Codepoints = codepoints;
				Clusters = clusters;
				Points = points;
				Width = 0;
			}

			/// <param name="codepoints"></param>
			/// <param name="clusters"></param>
			/// <param name="points"></param>
			/// <param name="width"></param>
			public Result(uint[] codepoints, uint[] clusters, SKPoint[] points, float width)
			{
				Codepoints = codepoints;
				Clusters = clusters;
				Points = points;
				Width = width;
			}

			/// <summary>
			/// Gets the glyph Unicode code points.
			/// </summary>
			public uint[] Codepoints { get; }

			/// <summary>
			/// Gets the glyph clusters.
			/// </summary>
			public uint[] Clusters { get; }

			/// <summary>
			/// Gets the glyph positions.
			/// </summary>
			public SKPoint[] Points { get; }

			public float Width { get; }
		}
	}
}

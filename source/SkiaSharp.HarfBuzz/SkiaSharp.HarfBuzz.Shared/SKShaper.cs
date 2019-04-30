﻿using System;

using HarfBuzzSharp;
using Buffer = HarfBuzzSharp.Buffer;

namespace SkiaSharp.HarfBuzz
{
	public class SKShaper : IDisposable
	{
		internal const int FONT_SIZE_SCALE = 512;

		private Font font;
		private Buffer buffer;

		public SKShaper(SKTypeface typeface)
		{
			Typeface = typeface ?? throw new ArgumentNullException(nameof(typeface));

			int index;
			using (var blob = Typeface.OpenStream(out index).ToHarfBuzzBlob())
			using (var face = new Face(blob, index))
			{
				face.Index = index;
				face.UnitsPerEm = Typeface.UnitsPerEm;

				font = new Font(face);
				font.SetScale(FONT_SIZE_SCALE, FONT_SIZE_SCALE);

				font.SetFunctionsOpenType();
			}

			buffer = new Buffer();
		}

		public SKTypeface Typeface { get; private set; }

		public void Dispose()
		{
			font?.Dispose();
			buffer?.Dispose();
		}

		public Result Shape(Buffer buffer, SKPaint paint) =>
			Shape(buffer, 0, 0, paint);

		public Result Shape(Buffer buffer, float xOffset, float yOffset, SKPaint paint)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			if (paint == null)
			{
				throw new ArgumentNullException(nameof(paint));
			}

			// do the shaping
			font.Shape(buffer);

			// get the shaping results
			var len = buffer.Length;
			var info = buffer.GlyphInfos;
			var pos = buffer.GlyphPositions;

			// get the sizes
			float textSizeY = paint.TextSize / FONT_SIZE_SCALE;
			float textSizeX = textSizeY * paint.TextScaleX;

			var points = new SKPoint[len];
			var clusters = new uint[len];
			var codepoints = new uint[len];

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

			return new Result(codepoints, clusters, points);
		}

		public Result Shape(string text, SKPaint paint) =>
			Shape(text, 0, 0, paint);

		public Result Shape(string text, float xOffset, float yOffset, SKPaint paint)
		{
			if (string.IsNullOrEmpty(text))
			{
				return new Result();
			}

			using (var buffer = new Buffer())
			{
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
		}

		public class Result
		{
			public Result()
			{
				Codepoints = new uint[0];
				Clusters = new uint[0];
				Points = new SKPoint[0];
			}

			public Result(uint[] codepoints, uint[] clusters, SKPoint[] points)
			{
				Codepoints = codepoints;
				Clusters = clusters;
				Points = points;
			}

			public uint[] Codepoints { get; private set; }

			public uint[] Clusters { get; private set; }

			public SKPoint[] Points { get; private set; }
		}
	}
}

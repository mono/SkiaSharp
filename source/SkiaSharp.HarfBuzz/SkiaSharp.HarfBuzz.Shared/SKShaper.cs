using System;
using System.Linq;

using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz
{
	public class SKShaper : IDisposable
	{
		internal const int FONT_SIZE_SCALE = 512;

		private Font font;
		private HarfBuzzSharp.Buffer buffer;

		public SKShaper(SKTypeface typeface)
		{
			if (typeface == null)
				throw new ArgumentNullException(nameof(typeface)); ;

			Typeface = typeface;

			int index;
			using (var blob = Typeface.OpenStream(out index).ToHarfBuzzBlob())
			using (var face = new Face(blob, (uint)index))
			{
				face.Index = (uint)index;
				face.UnitsPerEm = (uint)Typeface.UnitsPerEm;

				font = new Font(face);
				font.SetScale(FONT_SIZE_SCALE, FONT_SIZE_SCALE);
				font.SetFunctionsOpenType();
			}

			buffer = new HarfBuzzSharp.Buffer();
		}

		public SKTypeface Typeface { get; private set; }

		public void Dispose()
		{
			font?.Dispose();
			buffer?.Dispose();
		}

		public Result Shape(string text, SKPaint paint)
		{
			return Shape(text, 0, 0, paint);
		}

		public Result Shape(string text, float xOffset, float yOffset, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			if (paint == null)
				throw new ArgumentNullException(nameof(paint));

			if (string.IsNullOrEmpty(text))
				return new Result();

			// add the text to the buffer
			buffer.ClearContents();
			buffer.AddUtf8(text);

			// try to understand the text
			buffer.GuessSegmentProperties();

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
			var codepointsTemp = new byte[len][];

			for (var i = 0; i < len; i++)
			{
				codepointsTemp[i] = BitConverter.GetBytes((ushort)info[i].Codepoint);

				clusters[i] = info[i].Cluster;

				points[i] = new SKPoint(
					xOffset + pos[i].XOffset * textSizeX,
					yOffset - pos[i].YOffset * textSizeY);

				// move the cursor
				xOffset += pos[i].XAdvance * textSizeX;
				yOffset += pos[i].YAdvance * textSizeY;
			}

			var codepoints = codepointsTemp.SelectMany(cp => cp).ToArray();

			return new Result(codepoints, clusters, points);
		}

		public class Result
		{
			public Result()
			{
				Codepoints = new byte[0];
				Clusters = new uint[0];
				Points = new SKPoint[0];
			}

			public Result(byte[] codepoints, uint[] clusters, SKPoint[] points)
			{
				Codepoints = codepoints;
				Clusters = clusters;
				Points = points;
			}

			public byte[] Codepoints { get; private set; }

			public uint[] Clusters { get; private set; }

			public SKPoint[] Points { get; private set; }
		}
	}
}

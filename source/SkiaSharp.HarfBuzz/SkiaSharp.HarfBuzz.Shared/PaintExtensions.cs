using System;

namespace SkiaSharp.HarfBuzz
{
	public static class PaintExtensions
	{
		public static SKPath GetShapedTextPath(this SKPaint paint, string text, SKPoint p) =>
			paint.GetShapedTextPath(text, p.X, p.Y);

		public static SKPath GetShapedTextPath(this SKPaint paint, string text, float x, float y)
		{
			if (string.IsNullOrEmpty(text))
				return new SKPath();

			using var shaper = new SKShaper(paint.GetFont().Typeface);
			return paint.GetShapedTextPath(shaper, text, x, y);
		}

		public static SKPath GetShapedTextPath(this SKPaint paint, SKShaper shaper, string text, float x, float y)
		{
			var returnPath = new SKPath();

			if (string.IsNullOrEmpty(text))
				return returnPath;

			if (shaper == null)
				throw new ArgumentNullException(nameof(shaper));
			if (paint == null)
				throw new ArgumentNullException(nameof(paint));

			using var font = paint.ToFont();
			font.Typeface = shaper.Typeface;

			// shape the text
			var result = shaper.Shape(text, x, y, paint);

			// adjust alignment
			var xOffset = 0.0f;
			if (paint.TextAlign != SKTextAlign.Left)
			{
				var width = result.Width;
				if (paint.TextAlign == SKTextAlign.Center)
					width *= 0.5f;
				xOffset -= width;
			}

			// get spans for more efficient accessing
			var glyphSpan = result.Codepoints.AsSpan();
			var pointSpan = result.Points.AsSpan();

			// generate a path for each glyph
			for (var i = 0; i < pointSpan.Length; i++)
			{
				// get the glyph path
				using var glyphPath = font.GetGlyphPath((ushort)glyphSpan[i]);

				if (glyphPath.IsEmpty)
					continue;

				// translate the glyph path
				var point = pointSpan[i];
				glyphPath.Transform(new SKMatrix(
					1, 0, point.X + xOffset,
					0, 1, point.Y,
					0, 0, 1
				));

				// append the glyph path
				returnPath.AddPath(glyphPath);
			}

			return returnPath;
		}
	}
}

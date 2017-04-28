using System;
using System.Linq;

using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz
{
	public static class CanvasExtensions
	{
		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, float x, float y, SKPaint paint)
		{
			if (canvas == null)
				throw new ArgumentNullException(nameof(canvas));
			if (shaper == null)
				throw new ArgumentNullException(nameof(shaper));
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			if (paint == null)
				throw new ArgumentNullException(nameof(paint));

			if (string.IsNullOrEmpty(text))
				return;

			// shape the text
			var result = shaper.Shape(text, x, y, paint);

			// draw the text

			using (var paintClone = paint.Clone())
			{
				paintClone.TextEncoding = SKTextEncoding.GlyphId;
				paintClone.Typeface = shaper.Typeface;

				canvas.DrawPositionedText(result.Codepoints, result.Points, paintClone);
			}
		}
	}
}

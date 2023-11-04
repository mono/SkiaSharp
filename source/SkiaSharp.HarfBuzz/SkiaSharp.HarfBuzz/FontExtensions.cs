using System;

using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz
{
	public static class FontExtensions
	{
		public static SKSizeI GetScale(this Font font)
		{
			if (font == null)
			{
				throw new ArgumentNullException(nameof(font));
			}

			font.GetScale(out var scaleX, out var scaleY);
			return new SKSizeI(scaleX, scaleY);
		}

		public static void SetScale(this Font font, SKSizeI scale)
		{
			if (font == null)
			{
				throw new ArgumentNullException(nameof(font));
			}

			font.SetScale(scale.Width, scale.Height);
		}

		public static SKPath GetShapedTextPath(this SKFont font, string text, SKPoint p, SKTextAlign textAlign) =>
			font.GetShapedTextPath(text, p.X, p.Y, textAlign);

		public static SKPath GetShapedTextPath(this SKFont font, string text, float x, float y, SKTextAlign textAlign)
		{
			if (string.IsNullOrEmpty(text))
				return new SKPath();

			using var shaper = new SKShaper(font.Typeface);
			return font.GetShapedTextPath(shaper, text, x, y, textAlign);
		}

		public static SKPath GetShapedTextPath(this SKFont font, SKShaper shaper, string text, float x, float y, SKTextAlign textAlign)
		{
			var returnPath = new SKPath();

			if (string.IsNullOrEmpty(text))
				return returnPath;

			if (shaper == null)
				throw new ArgumentNullException(nameof(shaper));

			font.Typeface = shaper.Typeface;

			// shape the text
			var result = shaper.Shape(text, x, y, font);

			// adjust alignment
			var xOffset = 0.0f;
			if (textAlign != SKTextAlign.Left)
			{
				var width = result.Width;
				if (textAlign == SKTextAlign.Center)
					width *= 0.5f;
				xOffset -= width;
			}

			// generate a path for each glyph
			for (var i = 0; i < result.Points.Length; i++)
			{
				// get the glyph path
				using var glyphPath = font.GetGlyphPath((ushort)result.Codepoints[i]);

				if (glyphPath.IsEmpty)
					continue;

				// translate the glyph path
				var point = result.Points[i];
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

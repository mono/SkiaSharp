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

		public static SKPath GetShapedTextPath(this SKFont font, string text, SKPoint p) =>
			font.GetShapedTextPath(text, p.X, p.Y);

		public static SKPath GetShapedTextPath(this SKFont font, string text, float x, float y) =>
			font.GetShapedTextPath(text, x, y, SKTextAlign.Left);

		public static SKPath GetShapedTextPath(this SKFont font, string text, float x, float y, SKTextAlign textAlign)
		{
			if (string.IsNullOrEmpty(text))
				return new SKPath();

			using var shaper = new SKShaper(font.Typeface);
			return font.GetShapedTextPath(shaper, text, x, y, textAlign);
		}

		public static SKPath GetShapedTextPath(this SKFont font, SKShaper shaper, string text, float x, float y, SKTextAlign textAlign)
		{
			if (string.IsNullOrEmpty(text))
				return new SKPath();

			if (shaper == null)
				throw new ArgumentNullException(nameof(shaper));
			if (font == null)
				throw new ArgumentNullException(nameof(font));

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

			// get spans for more efficient accessing
			var glyphSpan = result.Codepoints.AsSpan();
			var pointSpan = result.Points.AsSpan();

			using var pathBuilder = new SKPathBuilder();

			// generate a path for each glyph
			for (var i = 0; i < pointSpan.Length; i++)
			{
				// get the glyph path
				using var glyphPath = font.GetGlyphPath((ushort)glyphSpan[i]);

				if (glyphPath is null || glyphPath.IsEmpty)
					continue;

				// translate the glyph path
				var point = pointSpan[i];
				var matrix = new SKMatrix(
					1, 0, point.X + xOffset,
					0, 1, point.Y,
					0, 0, 1
				);
				glyphPath.Transform(in matrix);

				// append the glyph path
				pathBuilder.AddPath(glyphPath);
			}

			return pathBuilder.Detach();
		}
	}
}

using System;
using System.Collections.Generic;
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

		public static SKPath GetShapedTextPath(this SKFont font, string text) =>
			font.GetShapedTextPath(text, 0, 0);

		public static SKPath GetShapedTextPath(this SKFont font, string text, SKPoint p) =>
			font.GetShapedTextPath(text, p.X, p.Y);

		public static SKPath GetShapedTextPath(this SKFont font, string text, SKPoint p, SKTextAlign textAlign) =>
			font.GetShapedTextPath(text, p.X, p.Y, textAlign);

		public static SKPath GetShapedTextPath(this SKFont font, string text, float xOffset, float yOffset) =>
			font.GetShapedTextPath(text, xOffset, yOffset, SKTextAlign.Left);

		public static SKPath GetShapedTextPath(this SKFont font, string text, float xOffset, float yOffset, SKTextAlign textAlign)
		{
			if (string.IsNullOrEmpty(text))
				return new SKPath();

			if (font == null)
				throw new ArgumentNullException(nameof(font));

			using var shaper = new SKShaper(font.Typeface);
			return font.GetShapedTextPath(shaper, text, xOffset, yOffset, textAlign);
		}

		public static SKPath GetShapedTextPath(this SKFont font, SKShaper shaper, string text, float xOffset, float yOffset, SKTextAlign textAlign)
		{
			if (string.IsNullOrEmpty(text))
				return new SKPath();

			if (shaper == null)
				throw new ArgumentNullException(nameof(shaper));
			if (font == null)
				throw new ArgumentNullException(nameof(font));

			font.Typeface = shaper.Typeface;

			// shape the text
			var result = shaper.Shape(text, xOffset, yOffset, font);

			// adjust alignment
			var alignXOffset = 0.0f;
			if (textAlign != SKTextAlign.Left)
			{
				var width = result.Width;
				if (textAlign == SKTextAlign.Center)
					width *= 0.5f;
				alignXOffset -= width;
			}

			// get spans for more efficient accessing
			var glyphSpan = result.Codepoints.AsSpan();
			var pointSpan = result.Points.AsSpan();

			using var pathBuilder = new SKPathBuilder();

			// Many GetGlyphPath calls is faster and allocates less memory than a single GetGlyphPaths call
			var glyphCache = new Dictionary<ushort, SKPath>();
			try
			{
				// generate a path for each glyph
				for (var i = 0; i < pointSpan.Length; i++)
				{
					// get the glyph path
					var glyph = (ushort)glyphSpan[i];
#if NET6_0_OR_GREATER
					ref var glyphPath = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(glyphCache, glyph, out var exists);
					if (!exists)
					{
						glyphPath = font.GetGlyphPath(glyph);
					}
#else
					if (!glyphCache.TryGetValue(glyph, out var glyphPath))
					{
						glyphPath = font.GetGlyphPath(glyph);
						glyphCache.Add(glyph, glyphPath);
					}
#endif

					if (glyphPath is null || glyphPath.IsEmpty)
						continue;

					// translate the glyph path
					var point = pointSpan[i];
					var matrix = new SKMatrix(
						1, 0, point.X + alignXOffset,
						0, 1, point.Y,
						0, 0, 1
					);
					glyphPath.Transform(in matrix);

					// append the glyph path
					pathBuilder.AddPath(glyphPath);
				}
			}
			finally
			{
				foreach (var path in glyphCache.Values)
				{
					path.Dispose();
				}

				glyphCache.Clear();
			}

			return pathBuilder.Detach();
		}
	}
}

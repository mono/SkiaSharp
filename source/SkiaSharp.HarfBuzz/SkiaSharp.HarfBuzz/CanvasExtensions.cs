using System;

namespace SkiaSharp.HarfBuzz
{
	public static class CanvasExtensions
	{
		public static void DrawShapedText(this SKCanvas canvas, string text, SKPoint p, SKFont font, SKPaint paint) =>
			canvas.DrawShapedText(text, p.X, p.Y, font, paint);

		public static void DrawShapedText(this SKCanvas canvas, string text, float x, float y, SKFont font, SKPaint paint)
		{
			if (string.IsNullOrEmpty(text))
				return;

			using var shaper = new SKShaper(font.Typeface);
			canvas.DrawShapedText(shaper, text, x, y, font, paint);
		}

		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, SKPoint p, SKFont font, SKPaint paint) =>
			canvas.DrawShapedText(shaper, text, p.X, p.Y, SKTextAlign.Left, font, paint);

		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) =>
			canvas.DrawShapedText(shaper, text, p.X, p.Y, textAlign, font, paint);

		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, float x, float y, SKFont font, SKPaint paint) =>
			canvas.DrawShapedText(shaper, text, x, y, SKTextAlign.Left, font, paint);

		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint)
		{
			if (string.IsNullOrEmpty(text))
				return;

			if (canvas == null)
				throw new ArgumentNullException(nameof(canvas));
			if (shaper == null)
				throw new ArgumentNullException(nameof(shaper));
			if (paint == null)
				throw new ArgumentNullException(nameof(paint));

			font.Typeface = shaper.Typeface;

			// shape the text
			var result = shaper.Shape(text, x, y, font);

			// create the text blob
			using var builder = new SKTextBlobBuilder();
			var run = builder.AllocatePositionedRun(font, result.Codepoints.Length);

			// copy the glyphs
			var g = run.GetGlyphSpan();
			var p = run.GetPositionSpan();
			for (var i = 0; i < result.Codepoints.Length; i++)
			{
				g[i] = (ushort)result.Codepoints[i];
				p[i] = result.Points[i];
			}

			// build
			using var textBlob = builder.Build();

			// adjust alignment
			var xOffset = 0f;
			if (textAlign != SKTextAlign.Left) {
				var width = result.Width;
				if (textAlign == SKTextAlign.Center)
					width *= 0.5f;
				xOffset -= width;
			}

			// draw the text
			canvas.DrawText(textBlob, xOffset, 0, paint);
		}
	}
}

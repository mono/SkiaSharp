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

			if (font == null)
				throw new ArgumentNullException(nameof(font));

			using var shaper = new SKShaper(font.Typeface);
			canvas.DrawShapedText(shaper, text, x, y, font, paint);
		}

		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, SKPoint p, SKFont font, SKPaint paint) =>
			canvas.DrawShapedText(shaper, text, p.X, p.Y, font, paint);

		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, float x, float y, SKFont font, SKPaint paint)
		{
			if (string.IsNullOrEmpty(text))
				return;

			if (canvas == null)
				throw new ArgumentNullException(nameof(canvas));
			if (shaper == null)
				throw new ArgumentNullException(nameof(shaper));
			if (font == null)
				throw new ArgumentNullException(nameof(font));
			if (paint == null)
				throw new ArgumentNullException(nameof(paint));

			if (string.IsNullOrEmpty(text))
				return;

			// shape the text
			var result = shaper.Shape(text, x, y, font);

			// create the text blob
			using var textBlobBuilder = new SKTextBlobBuilder();
			textBlobBuilder.AddPositionedRun(result.Codepoints, font, result.Points);
			using var textBlob = textBlobBuilder.Build();

			// draw the text
			canvas.DrawText(textBlob, paint);
		}
	}
}

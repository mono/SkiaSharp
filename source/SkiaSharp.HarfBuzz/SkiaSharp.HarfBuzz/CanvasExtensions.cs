using System;

namespace SkiaSharp.HarfBuzz
{
	/// <summary>Various extension methods to integrate a SkiaSharp <see cref="T:SkiaSharp.SKCanvas" /> and HarfBuzz.</summary>
	/// <remarks />
	public static class CanvasExtensions
	{
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="p">The point at which to draw the text.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		/// <summary>Draws shaped text on the canvas at the specified point.</summary>
		/// <remarks />
		[Obsolete("Use DrawShapedText(string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.", error: true)]
		public static void DrawShapedText(this SKCanvas canvas, string text, SKPoint p, SKPaint paint) =>
			canvas.DrawShapedText(text, p.X, p.Y, paint.GetLegacyTextAlign(), paint.GetLegacyFont(), paint);

		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="p">The point at which to draw the text.</param>
		/// <param name="font">The font to use when shaping and drawing the text.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		/// <summary>Draws shaped text on the canvas at the specified point.</summary>
		/// <remarks />
		[Obsolete("Use the overload with SKTextAlign parameter instead.")]
		public static void DrawShapedText(this SKCanvas canvas, string text, SKPoint p, SKFont font, SKPaint paint) =>
			canvas.DrawShapedText(text, p.X, p.Y, paint.GetLegacyTextAlign(), font, paint);

		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="p">The point at which to draw the text.</param>
		/// <param name="textAlign">The text alignment to use when drawing the text.</param>
		/// <param name="font">The font to use when shaping and drawing the text.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		/// <summary>Draws shaped text on the canvas at the specified point with the specified alignment.</summary>
		/// <remarks />
		public static void DrawShapedText(this SKCanvas canvas, string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) =>
			canvas.DrawShapedText(text, p.X, p.Y, textAlign, font, paint);

		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="x">The x-coordinate of the origin of the text being drawn.</param>
		/// <param name="y">The y-coordinate of the origin of the text being drawn.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		/// <summary>Draws shaped text on the canvas at the specified coordinates.</summary>
		/// <remarks />
		[Obsolete("Use DrawShapedText(string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.", error: true)]
		public static void DrawShapedText(this SKCanvas canvas, string text, float x, float y, SKPaint paint) =>
			canvas.DrawShapedText(text, x, y, paint.GetLegacyTextAlign(), paint.GetLegacyFont(), paint);

		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="x">The x-coordinate of the origin of the text being drawn.</param>
		/// <param name="y">The y-coordinate of the origin of the text being drawn.</param>
		/// <param name="font">The font to use when shaping and drawing the text.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		/// <summary>Draws shaped text on the canvas at the specified coordinates.</summary>
		/// <remarks />
		[Obsolete("Use the overload with SKTextAlign parameter instead.")]
		public static void DrawShapedText(this SKCanvas canvas, string text, float x, float y, SKFont font, SKPaint paint) =>
			canvas.DrawShapedText(text, x, y, paint.GetLegacyTextAlign(), font, paint);

		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="x">The x-coordinate of the origin of the text being drawn.</param>
		/// <param name="y">The y-coordinate of the origin of the text being drawn.</param>
		/// <param name="textAlign">The text alignment to use when drawing the text.</param>
		/// <param name="font">The font to use when shaping and drawing the text.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		/// <summary>Draws shaped text on the canvas at the specified coordinates with the specified alignment.</summary>
		/// <remarks />
		public static void DrawShapedText(this SKCanvas canvas, string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint)
		{
			if (string.IsNullOrEmpty(text))
				return;

			using var shaper = new SKShaper(font.Typeface);
			canvas.DrawShapedText(shaper, text, x, y, textAlign, font, paint);
		}

		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="shaper">The text shaper to use when shaping the text.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="p">The point at which to draw the text.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		/// <summary>Draws shaped text on the canvas at the specified point using the provided shaper.</summary>
		/// <remarks />
		[Obsolete("Use DrawShapedText(SKShaper shaper, string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.", error: true)]
		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, SKPoint p, SKPaint paint) =>
			canvas.DrawShapedText(shaper, text, p.X, p.Y, paint.GetLegacyTextAlign(), paint.GetLegacyFont(), paint);

		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="shaper">The text shaper to use when shaping the text.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="p">The point at which to draw the text.</param>
		/// <param name="font">The font to use when shaping and drawing the text.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		/// <summary>Draws shaped text on the canvas at the specified point using the provided shaper.</summary>
		/// <remarks />
		[Obsolete("Use the overload with SKTextAlign parameter instead.")]
		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, SKPoint p, SKFont font, SKPaint paint) =>
			canvas.DrawShapedText(shaper, text, p.X, p.Y, paint.GetLegacyTextAlign(), font, paint);

		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="shaper">The text shaper to use when shaping the text.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="p">The point at which to draw the text.</param>
		/// <param name="textAlign">The text alignment to use when drawing the text.</param>
		/// <param name="font">The font to use when shaping and drawing the text.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		/// <summary>Draws shaped text on the canvas at the specified point with the specified alignment using the provided shaper.</summary>
		/// <remarks />
		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) =>
			canvas.DrawShapedText(shaper, text, p.X, p.Y, textAlign, font, paint);

		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="shaper">The text shaper to use when shaping the text.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="x">The x-coordinate of the origin of the text being drawn.</param>
		/// <param name="y">The y-coordinate of the origin of the text being drawn.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		/// <summary>Draws shaped text on the canvas at the specified coordinates.</summary>
		/// <remarks />
		[Obsolete("Use DrawShapedText(SKShaper shaper, string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.", error: true)]
		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, float x, float y, SKPaint paint) =>
			canvas.DrawShapedText(shaper, text, x, y, paint.GetLegacyTextAlign(), paint.GetLegacyFont(), paint);

		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="shaper">The text shaper to use when shaping the text.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="x">The x-coordinate of the origin of the text being drawn.</param>
		/// <param name="y">The y-coordinate of the origin of the text being drawn.</param>
		/// <param name="font">The font to use when shaping and drawing the text.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		/// <summary>Draws shaped text on the canvas at the specified coordinates using the provided shaper.</summary>
		/// <remarks />
		[Obsolete("Use the overload with SKTextAlign parameter instead.")]
		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, float x, float y, SKFont font, SKPaint paint) =>
			canvas.DrawShapedText(shaper, text, x, y, paint.GetLegacyTextAlign(), font, paint);

		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="shaper">The text shaper to use when shaping the text.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="x">The x-coordinate of the origin of the text being drawn.</param>
		/// <param name="y">The y-coordinate of the origin of the text being drawn.</param>
		/// <param name="textAlign">The text alignment to use when drawing the text.</param>
		/// <param name="font">The font to use when shaping and drawing the text.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		/// <summary>Draws shaped text on the canvas at the specified coordinates with the specified alignment using the provided shaper.</summary>
		/// <remarks />
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
			var run = builder.AllocateRawPositionedRun(font, result.Codepoints.Length, null);

			// copy the glyphs
			var g = run.Glyphs;
			var p = run.Positions;
			for (var i = 0; i < result.Codepoints.Length; i++)
			{
				g[i] = (ushort)result.Codepoints[i];
				p[i] = result.Points[i];
			}

			// build
			using var textBlob = builder.Build();

			// adjust alignment
			var xOffset = 0f;
			if (textAlign != SKTextAlign.Left)
			{
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

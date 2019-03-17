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

			var scale = font.Scale;
			return new SKSizeI(scale.X, scale.Y);
		}

		public static void SetScale(this Font font, SKSizeI scale)
		{
			if (font == null)
			{
				throw new ArgumentNullException(nameof(font));
			}

			font.Scale = new Point(scale.Width, scale.Height);
		}
	}
}

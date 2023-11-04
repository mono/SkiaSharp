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
	}
}

using System;

using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz
{
	/// <summary>
	/// Various extension methods to integrate SkiaSharp and a HarfBuzz <see cref="Font" />.
	/// </summary>
	public static class FontExtensions
	{
		/// <summary>
		/// Retrieves the font scale.
		/// </summary>
		/// <param name="font">The font to retrieve the scale.</param>
		/// <returns>Returns the font scale.</returns>
		public static SKSizeI GetScale(this Font font)
		{
			if (font == null)
			{
				throw new ArgumentNullException(nameof(font));
			}

			font.GetScale(out var scaleX, out var scaleY);
			return new SKSizeI(scaleX, scaleY);
		}

		/// <summary>
		/// Sets the font scale.
		/// </summary>
		/// <param name="font">The font to set the scale.</param>
		/// <param name="scale">The scale to set.</param>
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

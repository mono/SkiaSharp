#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// 16-bit, floating-point, ARGB unpremultiplied color value.
	/// </summary>
	/// <remarks>The color components are always in a known order.</remarks>
	public readonly unsafe partial struct SKColorF
	{
		private const float EPSILON = 0.001f;

		/// <summary>
		/// Gets an "empty" color, with zero for all the components.
		/// </summary>
		public static readonly SKColorF Empty;

		/// <summary>
		/// Creates a color from the specified red, green and blue components.
		/// </summary>
		/// <param name="red">The red component.</param>
		/// <param name="green">The green component.</param>
		/// <param name="blue">The blue component.</param>
		public SKColorF (float red, float green, float blue)
		{
			fR = red;
			fG = green;
			fB = blue;
			fA = 1f;
		}

		/// <summary>
		/// Creates a color from the specified red, green, blue and alpha components.
		/// </summary>
		/// <param name="red">The red component.</param>
		/// <param name="green">The green component.</param>
		/// <param name="blue">The blue component.</param>
		/// <param name="alpha">The alpha component.</param>
		public SKColorF (float red, float green, float blue, float alpha)
		{
			fR = red;
			fG = green;
			fB = blue;
			fA = alpha;
		}

		/// <summary>
		/// Returns a new color based on this current instance, but with the new red channel value.
		/// </summary>
		/// <param name="red">The new red component.</param>
		public readonly SKColorF WithRed (float red) =>
			new SKColorF (red, fG, fB, fA);

		/// <summary>
		/// Returns a new color based on this current instance, but with the new green channel value.
		/// </summary>
		/// <param name="green">The new green component.</param>
		public readonly SKColorF WithGreen (float green) =>
			new SKColorF (fR, green, fB, fA);

		/// <summary>
		/// Returns a new color based on this current instance, but with the new blue channel value.
		/// </summary>
		/// <param name="blue">The new blue component.</param>
		public readonly SKColorF WithBlue (float blue) =>
			new SKColorF (fR, fG, blue, fA);

		/// <summary>
		/// Returns a new color based on this current instance, but with the new alpha channel value.
		/// </summary>
		/// <param name="alpha">The new alpha component.</param>
		public readonly SKColorF WithAlpha (float alpha) =>
			new SKColorF (fR, fG, fB, alpha);

		/// <summary>
		/// Gets the hue value.
		/// </summary>
		public readonly float Hue {
			get {
				ToHsv (out var h, out _, out _);
				return h;
			}
		}

		/// <summary>
		/// Clamp the color components in the range [0..1].
		/// </summary>
		/// <returns>Returns the clamped color.</returns>
		public readonly SKColorF Clamp ()
		{
			return new SKColorF (Clamp (fR), Clamp (fG), Clamp (fB), Clamp (fA));

			static float Clamp (float v)
			{
				if (v > 1f)
					return 1f;
				if (v < 0f)
					return 0f;
				return v;
			}
		}

		/// <summary>
		/// Creates a color from the specified hue, saturation, lightness/luminosity and alpha values.
		/// </summary>
		/// <param name="h">The hue value.</param>
		/// <param name="s">The saturation value.</param>
		/// <param name="l">The lightness/luminosity value.</param>
		/// <param name="a">The alpha value.</param>
		/// <returns>The new <see cref="T:SkiaSharp.SKColorF" /> instance.</returns>
		public static SKColorF FromHsl (float h, float s, float l, float a = 1f)
		{
			// convert from percentages
			h = h / 360f;
			s = s / 100f;
			l = l / 100f;

			// RGB results from 0 to 1
			var r = l;
			var g = l;
			var b = l;

			// HSL from 0 to 1
			if (Math.Abs (s) > EPSILON) {
				float v2;
				if (l < 0.5f)
					v2 = l * (1f + s);
				else
					v2 = (l + s) - (s * l);

				var v1 = 2f * l - v2;

				r = HueToRgb (v1, v2, h + (1f / 3f));
				g = HueToRgb (v1, v2, h);
				b = HueToRgb (v1, v2, h - (1f / 3f));
			}

			return new SKColorF (r, g, b, a);
		}

		private static float HueToRgb (float v1, float v2, float vH)
		{
			if (vH < 0f)
				vH += 1f;
			if (vH > 1f)
				vH -= 1f;

			if ((6f * vH) < 1f)
				return (v1 + (v2 - v1) * 6f * vH);
			if ((2f * vH) < 1f)
				return (v2);
			if ((3f * vH) < 2f)
				return (v1 + (v2 - v1) * ((2f / 3f) - vH) * 6f);
			return (v1);
		}

		/// <summary>
		/// Creates a color from the specified hue, saturation, value/brightness and alpha values.
		/// </summary>
		/// <param name="h">The hue value.</param>
		/// <param name="s">The saturation value.</param>
		/// <param name="v">The value/brightness value.</param>
		/// <param name="a">The alpha value.</param>
		/// <returns>The new <see cref="T:SkiaSharp.SKColorF" /> instance.</returns>
		public static SKColorF FromHsv (float h, float s, float v, float a = 1f)
		{
			// convert from percentages
			h = h / 360f;
			s = s / 100f;
			v = v / 100f;

			// RGB results from 0 to 1
			var r = v;
			var g = v;
			var b = v;

			// HSL from 0 to 1
			if (Math.Abs (s) > EPSILON) {
				h = h * 6f;
				if (Math.Abs (h - 6f) < EPSILON)
					h = 0f; // H must be < 1

				var hInt = (int)h;
				var v1 = v * (1f - s);
				var v2 = v * (1f - s * (h - hInt));
				var v3 = v * (1f - s * (1f - (h - hInt)));

				if (hInt == 0) {
					r = v;
					g = v3;
					b = v1;
				} else if (hInt == 1) {
					r = v2;
					g = v;
					b = v1;
				} else if (hInt == 2) {
					r = v1;
					g = v;
					b = v3;
				} else if (hInt == 3) {
					r = v1;
					g = v2;
					b = v;
				} else if (hInt == 4) {
					r = v3;
					g = v1;
					b = v;
				} else {
					r = v;
					g = v1;
					b = v2;
				}
			}

			return new SKColorF (r, g, b, a);
		}

		/// <summary>
		/// Converts the current color into it's hue, saturation and lightness/luminosity values.
		/// </summary>
		/// <param name="h">The hue value.</param>
		/// <param name="s">The saturation value.</param>
		/// <param name="l">The lightness/luminosity value.</param>
		/// <remarks>The alpha value is separate from the HSL calculation and will always be the same as <see cref="P:SkiaSharp.SKColorF.Alpha" />.</remarks>
		public readonly void ToHsl (out float h, out float s, out float l)
		{
			// RGB from 0 to 1
			var r = fR;
			var g = fG;
			var b = fB;

			var min = Math.Min (Math.Min (r, g), b); // min value of RGB
			var max = Math.Max (Math.Max (r, g), b); // max value of RGB
			var delta = max - min; // delta RGB value

			// default to a gray, no chroma...
			h = 0f;
			s = 0f;
			l = (max + min) / 2f;

			// chromatic data...
			if (Math.Abs (delta) > EPSILON) {
				if (l < 0.5f)
					s = delta / (max + min);
				else
					s = delta / (2f - max - min);

				var deltaR = (((max - r) / 6f) + (delta / 2f)) / delta;
				var deltaG = (((max - g) / 6f) + (delta / 2f)) / delta;
				var deltaB = (((max - b) / 6f) + (delta / 2f)) / delta;

				if (Math.Abs (r - max) < EPSILON) // r == max
					h = deltaB - deltaG;
				else if (Math.Abs (g - max) < EPSILON) // g == max
					h = (1f / 3f) + deltaR - deltaB;
				else // b == max
					h = (2f / 3f) + deltaG - deltaR;

				if (h < 0f)
					h += 1f;
				if (h > 1f)
					h -= 1f;
			}

			// convert to percentages
			h = h * 360f;
			s = s * 100f;
			l = l * 100f;
		}

		/// <summary>
		/// Converts the current color into it's hue, saturation and value/brightness values.
		/// </summary>
		/// <param name="h">The hue value.</param>
		/// <param name="s">The saturation value.</param>
		/// <param name="v">The value/brightness value.</param>
		/// <remarks>The alpha value is separate from the HSV/HSB calculation and will always be the same as <see cref="P:SkiaSharp.SKColorF.Alpha" />.</remarks>
		public readonly void ToHsv (out float h, out float s, out float v)
		{
			// RGB from 0 to 1
			var r = fR;
			var g = fG;
			var b = fB;

			var min = Math.Min (Math.Min (r, g), b); // min value of RGB
			var max = Math.Max (Math.Max (r, g), b); // max value of RGB
			var delta = max - min; // delta RGB value 

			// default to a gray, no chroma...
			h = 0;
			s = 0;
			v = max;

			// chromatic data...
			if (Math.Abs (delta) > EPSILON) {
				s = delta / max;

				var deltaR = (((max - r) / 6f) + (delta / 2f)) / delta;
				var deltaG = (((max - g) / 6f) + (delta / 2f)) / delta;
				var deltaB = (((max - b) / 6f) + (delta / 2f)) / delta;

				if (Math.Abs (r - max) < EPSILON) // r == max
					h = deltaB - deltaG;
				else if (Math.Abs (g - max) < EPSILON) // g == max
					h = (1f / 3f) + deltaR - deltaB;
				else // b == max
					h = (2f / 3f) + deltaG - deltaR;

				if (h < 0f)
					h += 1f;
				if (h > 1f)
					h -= 1f;
			}

			// convert to percentages
			h = h * 360f;
			s = s * 100f;
			v = v * 100f;
		}

		/// <summary>
		/// Returns the color as a string in the format: #AARRGGBB.
		/// </summary>
		/// <remarks>As a result of converting a floating-point color to an integer color, some data loss will occur.</remarks>
		public readonly override string ToString () =>
			((SKColor)this).ToString ();

		/// <summary>
		/// Converts a <see cref="T:SkiaSharp.SKColor" /> to a <see cref="T:SkiaSharp.SKColorF" />.
		/// </summary>
		/// <param name="color">The <see cref="T:SkiaSharp.SKColor" />.</param>
		/// <returns>The new <see cref="T:SkiaSharp.SKColorF" /> instance.</returns>
		public static implicit operator SKColorF (SKColor color)
		{
			SKColorF colorF;
			SkiaApi.sk_color4f_from_color ((uint)color, &colorF);
			return colorF;
		}

		/// <summary>
		/// Converts a <see cref="T:SkiaSharp.SKColorF" /> to a <see cref="T:SkiaSharp.SKColor" />.
		/// </summary>
		/// <param name="color">The color to convert.</param>
		/// <returns>The <see cref="T:SkiaSharp.SKColor" />.</returns>
		/// <remarks>As a result of converting a floating-point color to an integer color, some data loss will occur.</remarks>
		public static explicit operator SKColor (SKColorF color) =>
			SkiaApi.sk_color4f_to_color (&color);
	}
}

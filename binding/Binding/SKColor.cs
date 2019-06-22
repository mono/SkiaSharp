using System;
using System.Globalization;

namespace SkiaSharp
{
	public struct SKPMColor
	{
		private uint color;

		public SKPMColor (uint value)
		{
			color = value;
		}

		public byte Alpha => (byte)((color >> SKImageInfo.PlatformColorAlphaShift) & 0xff);
		public byte Red => (byte)((color >> SKImageInfo.PlatformColorRedShift) & 0xff);
		public byte Green => (byte)((color >> SKImageInfo.PlatformColorGreenShift) & 0xff);
		public byte Blue => (byte)((color >> SKImageInfo.PlatformColorBlueShift) & 0xff);

		public static SKPMColor PreMultiply (SKColor color)
		{
			return SkiaApi.sk_color_premultiply (color);
		}

		public static SKColor UnPreMultiply (SKPMColor pmcolor)
		{
			return SkiaApi.sk_color_unpremultiply (pmcolor);
		}

		public static SKPMColor[] PreMultiply (SKColor[] colors)
		{
			var pmcolors = new SKPMColor [colors.Length];
			SkiaApi.sk_color_premultiply_array (colors, colors.Length, pmcolors);
			return pmcolors;
		}

		public static SKColor[] UnPreMultiply (SKPMColor[] pmcolors)
		{
			var colors = new SKColor [pmcolors.Length];
			SkiaApi.sk_color_unpremultiply_array (pmcolors, pmcolors.Length, colors);
			return colors;
		}

		public override bool Equals (object other)
		{
			if (!(other is SKPMColor))
				return false;

			var c = (SKPMColor)other;
			return c.color == this.color;
		}

		public override int GetHashCode ()
		{
			return (int)color;
		}

		public static bool operator == (SKPMColor left, SKPMColor right)
		{
			return left.color == right.color;
		}

		public static bool operator != (SKPMColor left, SKPMColor right)
		{
			return !(left == right);
		}

		public static explicit operator SKPMColor (SKColor color)
		{
			return SKPMColor.PreMultiply (color);
		}

		public static explicit operator SKColor (SKPMColor color)
		{
			return SKPMColor.UnPreMultiply (color);
		}

		public static implicit operator SKPMColor (uint color)
		{
			return new SKPMColor (color);
		}

		public static explicit operator uint (SKPMColor color)
		{
			return color.color;
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "#{0:x2}{1:x2}{2:x2}{3:x2}", Alpha, Red, Green, Blue);
		}
	}

	public struct SKColor
	{
		private const float EPSILON = 0.001f;

		public static readonly SKColor Empty;

		private uint color;

		public SKColor (uint value)
		{
			color = value;
		}

		public SKColor (byte red, byte green, byte blue, byte alpha)
		{
			color = (uint)((alpha << 24) | (red << 16) | (green << 8) | blue);
		}

		public SKColor (byte red, byte green, byte blue)
		{
			color = (0xff000000u | (uint)(red << 16) | (uint)(green << 8) | blue);
		}

		public SKColor WithRed (byte red)
		{
			return new SKColor (red, Green, Blue, Alpha);
		}

		public SKColor WithGreen (byte green)
		{
			return new SKColor (Red, green, Blue, Alpha);
		}

		public SKColor WithBlue (byte blue)
		{
			return new SKColor (Red, Green, blue, Alpha);
		}

		public SKColor WithAlpha (byte alpha)
		{
			return new SKColor (Red, Green, Blue, alpha);
		}

		public byte Alpha => (byte)((color >> 24) & 0xff);
		public byte Red => (byte)((color >> 16) & 0xff);
		public byte Green => (byte)((color >> 8) & 0xff);
		public byte Blue => (byte)((color) & 0xff);

		public float Hue {
			get {
				ToHsv (out var h, out var s, out var v);
				return h;
			}
		}
		
		public static SKColor FromHsl (float h, float s, float l, byte a = 255)
		{
			// convert from percentages
			h = h / 360f;
			s = s / 100f;
			l = l / 100f;

			// RGB results from 0 to 255
			var r = l * 255f;
			var g = l * 255f;
			var b = l * 255f;

			// HSL from 0 to 1
			if (Math.Abs (s) > EPSILON)
			{
				float v2;
				if (l < 0.5f)
					v2 = l * (1f + s);
				else
					v2 = (l + s) - (s * l);

				var v1 = 2f * l - v2;

				r = 255 * HueToRgb (v1, v2, h + (1f / 3f));
				g = 255 * HueToRgb (v1, v2, h);
				b = 255 * HueToRgb (v1, v2, h - (1f / 3f));
			}

			return new SKColor ((byte)r, (byte)g, (byte)b, a);
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

		public static SKColor FromHsv(float h, float s, float v, byte a = 255)
		{
			// convert from percentages
			h = h / 360f;
			s = s / 100f;
			v = v / 100f;

			// RGB results from 0 to 255
			var r = v;
			var g = v;
			var b = v;

			// HSL from 0 to 1
			if (Math.Abs (s) > EPSILON)
			{
				h = h * 6f;
				if (Math.Abs (h - 6f) < EPSILON)
					h = 0f; // H must be < 1

				var hInt = (int)h;
				var v1 = v * (1f - s);
				var v2 = v * (1f - s * (h - hInt));
				var v3 = v * (1f - s * (1f - (h - hInt)));

				if (hInt == 0)
				{
					r = v;
					g = v3;
					b = v1;
				}
				else if (hInt == 1)
				{
					r = v2;
					g = v;
					b = v1;
				}
				else if (hInt == 2)
				{
					r = v1;
					g = v;
					b = v3;
				}
				else if (hInt == 3)
				{
					r = v1;
					g = v2;
					b = v;
				}
				else if (hInt == 4)
				{
					r = v3;
					g = v1;
					b = v;
				}
				else
				{
					r = v;
					g = v1;
					b = v2;
				}
			}

			// RGB results from 0 to 255
			r = r * 255f;
			g = g * 255f;
			b = b * 255f;

			return new SKColor ((byte)r, (byte)g, (byte)b, a);
		}

		public void ToHsl (out float h, out float s, out float l)
		{
			// RGB from 0 to 255
			var r = (Red / 255f);
			var g = (Green / 255f);
			var b = (Blue / 255f);

			var min = Math.Min (Math.Min (r, g), b); // min value of RGB
			var max = Math.Max (Math.Max (r, g), b); // max value of RGB
			var delta = max - min; // delta RGB value

			// default to a gray, no chroma...
			h = 0f;
			s = 0f;
			l = (max + min) / 2f;

			// chromatic data...
			if (Math.Abs (delta) > EPSILON)
			{
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

		public void ToHsv (out float h, out float s, out float v)
		{
			// RGB from 0 to 255
			var r = (Red / 255f);
			var g = (Green / 255f);
			var b = (Blue / 255f);

			var min = Math.Min (Math.Min (r, g), b); // min value of RGB
			var max = Math.Max (Math.Max (r, g), b); // max value of RGB
			var delta = max - min; // delta RGB value 

			// default to a gray, no chroma...
			h = 0;
			s = 0;
			v = max;

			// chromatic data...
			if (Math.Abs (delta) > EPSILON)
			{
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

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "#{0:x2}{1:x2}{2:x2}{3:x2}",  Alpha, Red, Green, Blue);
		}

		public override bool Equals (object other)
		{
			if (!(other is SKColor))
				return false;

			var c = (SKColor) other;
			return c.color == this.color;
		}

		public override int GetHashCode ()
		{
			return (int) color;
		}

		public static implicit operator SKColor (uint color)
		{
			return new SKColor (color);
		}

		public static explicit operator uint (SKColor color)
		{
			return color.color;
		}

		public static bool operator == (SKColor left, SKColor right)
		{
			return left.color == right.color;
		}

		public static bool operator != (SKColor left, SKColor right)
		{
			return !(left == right);
		}

		public static SKColor Parse (string hexString)
		{
			if (!TryParse (hexString, out var color))
				throw new ArgumentException ("Invalid hexadecimal color string.", nameof (hexString));
			return color;
		}

		public static bool TryParse (string hexString, out SKColor color)
		{
			if (string.IsNullOrWhiteSpace (hexString)) {
				// error
				color = SKColor.Empty;
				return false;
			}

			// clean up string
			hexString = hexString.Trim ().ToUpperInvariant ();
			if (hexString [0] == '#')
				hexString = hexString.Substring (1);

			var len = hexString.Length;
			if (len == 3 || len == 4) {
				byte a;
				// parse [A]
				if (len == 4) {
					if (!byte.TryParse (string.Concat (hexString [len - 4], hexString [len - 4]), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out a)) {
						// error
						color = SKColor.Empty;
						return false;
					}
				} else {
					a = 255;
				}

				// parse RGB
				if (!byte.TryParse (string.Concat (hexString [len - 3], hexString [len - 3]), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) ||
					!byte.TryParse (string.Concat (hexString [len - 2], hexString [len - 2]), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) ||
					!byte.TryParse (string.Concat (hexString [len - 1], hexString [len - 1]), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b)) {
					// error
					color = SKColor.Empty;
					return false;
				}

				// success
				color = new SKColor(r, g, b, a);
				return true;
			}

			if (len == 6 || len == 8) {
				// parse [AA]RRGGBB
				if (!uint.TryParse (hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var number)) {
					// error
					color = SKColor.Empty;
					return false;
				}

				// success
				color = (SKColor)number;

				// alpha was not provided, so use 255
				if (len == 6) {
					color = color.WithAlpha (255);
				}
				return true;
			}

			// error
			color = SKColor.Empty;
			return false;
		}
	}
}

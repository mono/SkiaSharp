﻿using System;

namespace SkiaSharp
{
	public readonly unsafe partial struct SKColorF
	{
		private const float EPSILON = 0.001f;

		public static readonly SKColorF Empty;

		public SKColorF (float red, float green, float blue)
		{
			fR = red;
			fG = green;
			fB = blue;
			fA = 1f;
		}

		public SKColorF (float red, float green, float blue, float alpha)
		{
			fR = red;
			fG = green;
			fB = blue;
			fA = alpha;
		}

		public readonly SKColorF WithRed (float red) =>
			new SKColorF (red, fG, fB, fA);

		public readonly SKColorF WithGreen (float green) =>
			new SKColorF (fR, green, fB, fA);

		public readonly SKColorF WithBlue (float blue) =>
			new SKColorF (fR, fG, blue, fA);

		public readonly SKColorF WithAlpha (float alpha) =>
			new SKColorF (fR, fG, fB, alpha);

		public readonly float Hue {
			get {
				ToHsv (out var h, out _, out _);
				return h;
			}
		}

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

		public readonly override string ToString () =>
			((SKColor)this).ToString ();

		public static implicit operator SKColorF (SKColor color)
		{
			SKColorF colorF;
			SkiaApi.sk_color4f_from_color ((uint)color, &colorF);
			return colorF;
		}

		public static explicit operator SKColor (SKColorF color) =>
			SkiaApi.sk_color4f_to_color (&color);
	}
}

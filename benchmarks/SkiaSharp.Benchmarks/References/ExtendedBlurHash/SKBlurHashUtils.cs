using System;

namespace SkiaSharp.Extended
{
	internal static class SKBlurHashUtils
	{
		public static double SrgbToLinear(int srgb)
		{
			var v = srgb / 255.0;
			return v <= 0.04045
				? v / 12.92
				: Math.Pow((v + 0.055) / 1.055, 2.4);
		}

		public static int LinearToSrgb(double linear)
		{
			var v = Math.Max(0.0, Math.Min(1.0, linear));

			return v <= 0.0031308
				? (int)(v * 12.92 * 255.0 + 0.5)
				: (int)((1.055 * Math.Pow(v, 1.0 / 2.4) - 0.055) * 255.0 + 0.5);
		}

		public static double SignedPow(double value, double exp)
		{
			var pow = Math.Pow(Math.Abs(value), exp);
			return value < 0.0 ? -pow : pow;
		}

		public static int Clamp(int min, int max, int value) =>
			Math.Max(min, Math.Min(value, max));

		public struct Pixel
		{
			public double R;
			public double G;
			public double B;

			public Pixel(double r, double g, double b)
			{
				R = r;
				G = g;
				B = b;
			}

			public override string ToString() =>
				$"{{{R}, {G}, {B}}}";
		}
	}
}

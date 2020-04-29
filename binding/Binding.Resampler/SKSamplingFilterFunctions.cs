using System;

namespace SkiaSharp
{
	public static class SKSamplingFilterFunctions
	{
		public static readonly SKSamplingFilterFunction Lanczos = (t, a) =>
		{
			t = Math.Abs(t);
			return t < a ? Sinc(t) * Sinc(t / a) : 0.0;
		};

		private static double Sinc(double x)
		{
			x *= Math.PI;

			return x < 0.01 && (x > -0.01)
				? 1.0 + x * x * (-1.0 / 6.0 + x * x * 1.0 / 120.0)
				: Math.Sin(x) / x;
		}

	}
}

using System;
using ElmSharp;
using Tizen.System;

namespace SkiaSharp.Views.Tizen
{
	public static class ScalingInfo
	{
		private static readonly Lazy<string> profile = new Lazy<string>(() => Elementary.GetProfile());

		private static readonly Lazy<int> dpi = new Lazy<int>(() =>
		{
			// TV has fixed DPI value (72)
			if (Profile == "tv")
				return 72;

			SystemInfo.TryGetValue("http://tizen.org/feature/screen.dpi", out int dpi);
			return dpi;
		});

		// allows to convert pixels to Android-style device-independent pixels
		private static readonly Lazy<float> scalingFactor = new Lazy<float>(() => dpi.Value / 160.0f);

		public static string Profile => profile.Value;

		public static int Dpi => dpi.Value;

		public static float ScalingFactor => scalingFactor.Value;

		public static float FromPixel(float v) => v / ScalingFactor;

		public static float ToPixel(float v) => v * ScalingFactor;
	}
}

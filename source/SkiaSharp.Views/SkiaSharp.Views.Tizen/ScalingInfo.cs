using ElmSharp;
using System;

using TSystemInfo = Tizen.System.SystemInfo;

namespace SkiaSharp.Views.Tizen
{
    public static class ScalingInfo
    {
		private static Lazy<string> s_profile = new Lazy<string>(() =>
		{
			return Elementary.GetProfile();
		});

		/// <summary>
		/// DPI of the screen.
		/// </summary>
		private static Lazy<int> s_dpi = new Lazy<int>(() =>
		{
			if (Profile == "tv")
			{
				// TV has fixed DPI value (72)
				return 72;
			}

			int dpi = 0;
			TSystemInfo.TryGetValue("http://tizen.org/feature/screen.dpi", out dpi);
			return dpi;
		});

		/// <summary>
		/// Scaling factor, allows to convert pixels to Android-style device-independent pixels.
		/// </summary>
		private static Lazy<float> s_scalingFactor = new Lazy<float>(() => s_dpi.Value / 160.0f);

		public static string Profile => s_profile.Value;

		public static int DPI => s_dpi.Value;

		public static float ScalingFactor => s_scalingFactor.Value;

		public static float FromPixel(float v)
		{
			return v / ScalingFactor;
		}

		public static float ToPixel(float v)
		{
			return v * ScalingFactor;
		}
	}
}

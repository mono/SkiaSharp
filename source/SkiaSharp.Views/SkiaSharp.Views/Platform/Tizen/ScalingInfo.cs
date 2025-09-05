using System;
using ElmSharp;
using Tizen.System;

namespace SkiaSharp.Views.Tizen
{
	/// <summary>
	/// A utility class that can be used to determine screen densities.
	/// </summary>
	public static class ScalingInfo
	{
		private static readonly Lazy<string> profile = new Lazy<string>(() => Elementary.GetProfile());

		private static readonly Lazy<int> dpi = new Lazy<int>(() =>
		{
			// TV has fixed DPI value (72)
			if (Profile == "tv")
				return 72;

#pragma warning disable CS0618 // Type or member is obsolete
			SystemInfo.TryGetValue("http://tizen.org/feature/screen.dpi", out int dpi);
#pragma warning restore CS0618 // Type or member is obsolete
			return dpi;
		});

		// allows to convert pixels to Android-style device-independent pixels
		private static readonly Lazy<double> scalingFactor = new Lazy<double>(() => dpi.Value / 160.0);

		private static double? scalingFactorOverride;

		/// <summary>
		/// Gets the device profile.
		/// </summary>
		public static string Profile => profile.Value;

		/// <summary>
		/// Gets the DPI of the screen.
		/// </summary>
		public static int Dpi => dpi.Value;

		/// <summary>
		/// The scaling factor to convert between raw pixels and device-independent pixels.
		/// </summary>
		public static double ScalingFactor => scalingFactorOverride ?? scalingFactor.Value;

		/// <summary>
		/// Convert from raw pixels into device-independent pixels.
		/// </summary>
		/// <param name="v">The raw pixel dimension.</param>
		/// <returns>Returns the device-independent pixel dimension.</returns>
		public static double FromPixel(double v) => v / ScalingFactor;

		/// <summary>
		/// Convert from device-independent pixels into raw pixels.
		/// </summary>
		/// <param name="v">The device-independent pixel dimension.</param>
		/// <returns>Returns the raw pixel dimension.</returns>
		public static double ToPixel(double v) => v * ScalingFactor;

		/// <param name="scalingFactor"></param>
		public static void SetScalingFactor(double? scalingFactor)
		{
			scalingFactorOverride = scalingFactor;
		}
	}
}

using System;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Identifies the current host platform with a short, stable tag used to build
	/// platform-specific golden directories (e.g. <c>ganesh-gl.macos</c>). The same
	/// renderer produces different pixels on different platforms/drivers, so GPU
	/// goldens are recorded per platform; CPU raster is portable and shared.
	/// </summary>
	internal static class VisualPlatform
	{
		public static string Tag { get; } = DetermineTag();

		/// <summary>
		/// <see langword="true"/> on the desktop hosts (macOS / Windows / Linux),
		/// where CPU raster output is portable enough to share one
		/// <c>_shared</c> baseline. Device and browser hosts (Android / iOS /
		/// MacCatalyst / WASM) render on different architectures whose
		/// antialiasing rounds differently, so they record raster goldens per
		/// platform instead — matching what the prior-art harness found
		/// empirically (a shared desktop set plus separate
		/// <c>android-/ios-/wasm-</c> sets).
		/// </summary>
		public static bool IsDesktop { get; } = DetermineIsDesktop();

		private static bool DetermineIsDesktop()
		{
#if NET5_0_OR_GREATER
			return OperatingSystem.IsMacOS() || OperatingSystem.IsWindows() || OperatingSystem.IsLinux();
#else
			return TestConfig.Current.IsMac || TestConfig.Current.IsWindows || TestConfig.Current.IsLinux;
#endif
		}

		private static string DetermineTag()
		{
#if NET5_0_OR_GREATER
			if (OperatingSystem.IsBrowser())
				return "browser";
			if (OperatingSystem.IsAndroid())
				return "android";
			if (OperatingSystem.IsMacCatalyst())
				return "maccatalyst";
			if (OperatingSystem.IsIOS())
				return "ios";
			if (OperatingSystem.IsTvOS())
				return "tvos";
			if (OperatingSystem.IsMacOS())
				return "macos";
			if (OperatingSystem.IsWindows())
				return "windows";
			if (OperatingSystem.IsLinux())
				return "linux";
			return "unknown";
#else
			if (TestConfig.Current.IsMac)
				return "macos";
			if (TestConfig.Current.IsWindows)
				return "windows";
			if (TestConfig.Current.IsLinux)
				return "linux";
			return "unknown";
#endif
		}
	}
}

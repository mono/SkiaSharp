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

using System;
using System.Collections.Generic;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Identifies the current host platform with a short, stable tag used to build
	/// per-platform golden directories (e.g. <c>ganesh-gl.macos</c>). The same
	/// renderer produces different pixels on different platforms/drivers/
	/// architectures, so every cell records its golden per platform.
	/// </summary>
	internal static class VisualPlatform
	{
		/// <summary>
		/// Golden directory tags for the current host, most specific first. Usually a
		/// single entry, but Windows Nano Server rasterizes text with FreeType instead
		/// of DirectWrite, so it looks up its own <c>nanoserver</c> golden first and
		/// falls back to the shared <c>windows</c> golden for cells that render
		/// identically (shapes, gradients).
		/// </summary>
		public static IReadOnlyList<string> Tags { get; } = DetermineTags();

		public static string Tag => Tags[0];

		private static IReadOnlyList<string> DetermineTags()
		{
			var baseTag = DetermineTag();

			if (baseTag == "windows" && HasNoSystemFontManager())
				return new[] { "nanoserver", "windows" };

			return new[] { baseTag };
		}

		// Nano Server is the only Windows configuration with an empty system font
		// manager (SkFontMgr_New_Custom_Empty over FreeType); probing a basic Latin
		// character detects it.
		private static bool HasNoSystemFontManager() =>
			SkiaSharp.SKFontManager.Default.MatchCharacter('a') is null;

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

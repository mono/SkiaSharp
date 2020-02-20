using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if HARFBUZZ
namespace HarfBuzzSharp
#else
namespace SkiaSharp
#endif
{
	internal static class PlatformConfiguration
	{
		public static bool IsUnix { get; }
		public static bool IsWindows { get; }

		static PlatformConfiguration ()
		{
#if WINDOWS_UWP
			IsUnix = false;
			IsWindows = true;
#elif NET_STANDARD
			IsUnix = RuntimeInformation.IsOSPlatform (OSPlatform.OSX) || RuntimeInformation.IsOSPlatform (OSPlatform.Linux);
			IsWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
#else
			IsUnix = Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix;
			IsWindows = !IsUnix;
#endif
		}
	}
}

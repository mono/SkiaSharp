using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public abstract class SKTest
	{
		protected const string PathToFonts = "fonts";
		protected const string PathToImages = "images";

#if NET_STANDARD
		protected static bool IsUnix => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		protected static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
		protected static bool IsUnix => Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix;
		protected static bool IsWindows => !IsUnix;
#endif
	}
}

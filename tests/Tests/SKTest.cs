using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public abstract class SKTest
	{
#if NET_STANDARD
		protected static readonly string PathToAssembly = Path.GetDirectoryName(typeof(SKTest).GetTypeInfo().Assembly.Location);
		protected static readonly string PathToFonts = Path.Combine(PathToAssembly, "fonts");
		protected static readonly string PathToImages = Path.Combine(PathToAssembly, "images");

		protected static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		protected static bool IsMac => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		protected static bool IsUnix => IsLinux || IsMac;
		protected static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
		protected const string PathToFonts = "fonts";
		protected const string PathToImages = "images";

		protected static bool IsMac => Environment.OSVersion.Platform == PlatformID.MacOSX;
		protected static bool IsUnix => Environment.OSVersion.Platform == PlatformID.Unix || IsMac;
		protected static bool IsLinux => IsUnix && !IsMac;
		protected static bool IsWindows => !IsUnix;
#endif
	}
}

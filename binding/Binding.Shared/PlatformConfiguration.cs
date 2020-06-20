using System;
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

		public static bool IsMac { get; }

		public static bool IsLinux { get; }

		static PlatformConfiguration ()
		{
#if WINDOWS_UWP
			IsMac = false;
			IsLinux = false;
			IsUnix = false;
			IsWindows = true;
#elif NET_STANDARD || __NET_46__
			IsMac = RuntimeInformation.IsOSPlatform (OSPlatform.OSX);
			IsLinux = RuntimeInformation.IsOSPlatform (OSPlatform.Linux);
			IsUnix = IsMac || IsLinux;
			IsWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
#else
			IsUnix = Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix;
			IsWindows = !IsUnix;
			IsMac = IsUnix && MacPlatformDetector.IsMac.Value;
			IsLinux = IsUnix && !IsMac;
#endif
		}

#if __NET_45__
#pragma warning disable IDE1006 // Naming Styles
		private static class MacPlatformDetector
		{
			internal static readonly Lazy<bool> IsMac = new Lazy<bool> (IsRunningOnMac);

			[DllImport ("libc")]
			static extern int uname (IntPtr buf);

			static bool IsRunningOnMac ()
			{
				IntPtr buf = IntPtr.Zero;
				try {
					buf = Marshal.AllocHGlobal (8192);
					// This is a hacktastic way of getting sysname from uname ()
					if (uname (buf) == 0) {
						string os = Marshal.PtrToStringAnsi (buf);
						if (os == "Darwin")
							return true;
					}
				} catch {
				} finally {
					if (buf != IntPtr.Zero)
						Marshal.FreeHGlobal (buf);
				}
				return false;
			}
		}
#pragma warning restore IDE1006 // Naming Styles
#endif
	}
}

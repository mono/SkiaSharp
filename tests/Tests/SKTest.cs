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

		protected const string PathToFonts = "fonts";
		protected const string PathToImages = "images";

		protected static bool IsMac => MacPlatformDetector.IsMac.Value;
		protected static bool IsUnix => Environment.OSVersion.Platform == PlatformID.Unix || IsMac;
		protected static bool IsLinux => IsUnix && !IsMac;
		protected static bool IsWindows => !IsUnix;
#endif

		protected GlContext CreateGlContext()
		{
			if (IsLinux) {
				return new GlxContext();
			} else if (IsMac) {
				return null;
			} else if (IsWindows) {
				return null;
			} else {
				return null;
			}
		}
	}

    public abstract class GlContext : IDisposable
    {
        public abstract void MakeCurrent();
        public abstract void SwapBuffers();
        public abstract void Destroy();

		void IDisposable.Dispose() => Destroy();
    }
}

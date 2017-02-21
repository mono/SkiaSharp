using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace SkiaSharp.Tests
{
	public abstract class SKTest
	{
		protected static readonly string PathToAssembly = Path.GetDirectoryName(typeof(SKTest).GetTypeInfo().Assembly.Location);
		protected static readonly string PathToFonts = Path.Combine(PathToAssembly, "fonts");
		protected static readonly string PathToImages = Path.Combine(PathToAssembly, "images");

#if NET_STANDARD
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

		protected static bool IsMac => MacPlatformDetector.IsMac.Value;
		protected static bool IsUnix => Environment.OSVersion.Platform == PlatformID.Unix || IsMac;
		protected static bool IsLinux => IsUnix && !IsMac;
		protected static bool IsWindows => !IsUnix;
#endif

		public static class MacDynamicLibraries
		{
			private const string SystemLibrary = "/usr/lib/libSystem.dylib";
			[DllImport(SystemLibrary)]
			public static extern IntPtr dlopen(string path, int mode);
			[DllImport(SystemLibrary)]
			public static extern IntPtr dlsym(IntPtr handle, string symbol);
			[DllImport(SystemLibrary)]
			public static extern void dlclose(IntPtr handle);
		}

		public static class LinuxDynamicLibraries
		{
			private const string SystemLibrary = "libdl.so";
			[DllImport(SystemLibrary)]
			public static extern IntPtr dlopen(string path, int mode);
			[DllImport(SystemLibrary)]
			public static extern IntPtr dlsym(IntPtr handle, string symbol);
			[DllImport(SystemLibrary)]
			public static extern void dlclose(IntPtr handle);
		}

		public static class WindowsDynamicLibraries
		{
			private const string SystemLibrary = "Kernel32.dll";
			[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
			public static extern IntPtr LoadLibrary(string lpFileName);
			[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
			public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
			[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
			public static extern void FreeLibrary(IntPtr hModule);
		}

		protected GlContext CreateGlContext()
		{
			TestGlVersion();

			if (IsLinux) {
				return new GlxContext();
			} else if (IsMac) {
				return new CglContext();
			} else if (IsWindows) {
				return new WglContext();
			} else {
				return null;
			}
		}

		private void TestGlVersion()
		{
			var minimumVersion = new Version(1, 5);
			string versionString = null;

			if (IsLinux) {
			} else if (IsMac) {
			} else if (IsWindows) {
				versionString = Wgl.VersionString;
			} else {
			}

			// OpenGL version number is 'MAJOR.MINOR***'
			var versionNumber = versionString?.Trim()?.Split(' ')?.FirstOrDefault();

			Version version;
			if (versionNumber != null && Version.TryParse(versionNumber, out version)) {
				if (version < minimumVersion) {
					Assert.Ignore($"Available OpenGL version ({versionString}) is below minimum ({minimumVersion}).");
				}
			}
		}
	}
}

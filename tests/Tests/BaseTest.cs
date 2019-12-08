using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	public abstract class BaseTest
	{
		protected const string CategoryKey = "Category";

		protected const string GpuCategory = "GPU";
		protected const string MatchCharacterCategory = "MatchCharacter";

		protected static bool IsLinux;
		protected static bool IsMac;
		protected static bool IsUnix;
		protected static bool IsWindows;

		protected static bool IsRuntimeMono;

		protected static readonly string[] UnicodeFontFamilies;
		protected static readonly string DefaultFontFamily;
		protected static readonly string PathToAssembly;
		protected static readonly string PathToFonts;
		protected static readonly string PathToImages;

		static BaseTest()
		{
			// the the base paths
			PathToAssembly = Directory.GetCurrentDirectory();
			PathToFonts = Path.Combine(PathToAssembly, "fonts");
			PathToImages = Path.Combine(PathToAssembly, "images");

			// some platforms run the tests from a temporary location, so copy the native files
#if !NET_STANDARD
			var skiaRoot = Path.GetDirectoryName(typeof(SkiaSharp.SKImageInfo).Assembly.Location);
			var harfRoot = Path.GetDirectoryName(typeof(HarfBuzzSharp.Buffer).Assembly.Location);

			foreach (var file in Directory.GetFiles(PathToAssembly))
			{
				var fname = Path.GetFileNameWithoutExtension(file);

				var skiaDest = Path.Combine(skiaRoot, Path.GetFileName(file));
				if (fname == "libSkiaSharp" && !File.Exists(skiaDest))
				{
					File.Copy(file, skiaDest, true);
				}

				var harfDest = Path.Combine(harfRoot, Path.GetFileName(file));
				if (fname == "libHarfBuzzSharp" && !File.Exists(harfDest))
				{
					File.Copy(file, harfDest, true);
				}
			}
#endif

			// set the OS fields
#if NET_STANDARD
			IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
			IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
			IsUnix = IsLinux || IsMac;
			IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
			IsMac = MacPlatformDetector.IsMac.Value;
			IsUnix = Environment.OSVersion.Platform == PlatformID.Unix || IsMac;
			IsLinux = IsUnix && !IsMac;
			IsWindows = !IsUnix;
#endif

			IsRuntimeMono = Type.GetType("Mono.Runtime") != null;

			// set the test fields
			DefaultFontFamily = IsLinux ? "DejaVu Sans" : "Arial";
			UnicodeFontFamilies =
				IsLinux ? new[] { "Symbola" } :
				IsMac ? new[] { "Apple Color Emoji" } :
				new[] { "Segoe UI Emoji", "Segoe UI Symbol" };
		}

		public static void CollectGarbage()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		private static class MacPlatformDetector
		{
			internal static readonly Lazy<bool> IsMac = new Lazy<bool>(IsRunningOnMac);

			[DllImport("libc")]
			static extern int uname(IntPtr buf);

			static bool IsRunningOnMac()
			{
				IntPtr buf = IntPtr.Zero;
				try
				{
					buf = Marshal.AllocHGlobal(8192);
					// This is a hacktastic way of getting sysname from uname ()
					if (uname(buf) == 0)
					{
						string os = Marshal.PtrToStringAnsi(buf);
						if (os == "Darwin")
							return true;
					}
				}
				catch
				{
				}
				finally
				{
					if (buf != IntPtr.Zero)
						Marshal.FreeHGlobal(buf);
				}
				return false;
			}
		}

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
			[DllImport(SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
			public static extern IntPtr LoadLibrary(string lpFileName);
			[DllImport(SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
			public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
			[DllImport(SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
			public static extern void FreeLibrary(IntPtr hModule);
		}
	}
}

#nullable disable

using System;
using System.IO;
using System.Runtime.InteropServices;

#if HARFBUZZ
using HarfBuzzSharp.Internals;
#else
using SkiaSharp.Internals;
#endif

#if HARFBUZZ
namespace HarfBuzzSharp
#else
namespace SkiaSharp
#endif
{
#if USE_DELEGATES || USE_LIBRARY_LOADER
	internal static class LibraryLoader
	{
		static LibraryLoader ()
		{
			if (PlatformConfiguration.IsWindows)
				Extension = ".dll";
			else if (PlatformConfiguration.IsMac)
				Extension = ".dylib";
			else
				Extension = ".so";
		}

		public static string Extension { get; }

		public static IntPtr LoadLocalLibrary<T> (string libraryName)
		{
			var libraryPath = GetLibraryPath (libraryName);

			var handle = LoadLibrary (libraryPath);
			if (handle == IntPtr.Zero)
				throw new DllNotFoundException ($"Unable to load library '{libraryName}'.");

			return handle;

			static string GetLibraryPath (string libraryName)
			{
				var arch = PlatformConfiguration.Is64Bit
					? PlatformConfiguration.IsArm ? "arm64" : "x64"
					: PlatformConfiguration.IsArm ? "arm" : "x86";

				var libWithExt = libraryName;
				if (!libraryName.EndsWith (Extension, StringComparison.OrdinalIgnoreCase))
					libWithExt += Extension;

				// 1. try alongside managed assembly
				var path = typeof (T).Assembly.Location;
				if (!string.IsNullOrEmpty (path)) {
					path = Path.GetDirectoryName (path);
					if (CheckLibraryPath (path, arch, libWithExt, out var localLib))
						return localLib;
				}

				// 2. try current directory
				if (CheckLibraryPath (Directory.GetCurrentDirectory (), arch, libWithExt, out var lib))
					return lib;

				// 3. try app domain
				try {
					if (AppDomain.CurrentDomain is AppDomain domain) {
						// 3.1 RelativeSearchPath
						if (CheckLibraryPath (domain.RelativeSearchPath, arch, libWithExt, out lib))
							return lib;

						// 3.2 BaseDirectory
						if (CheckLibraryPath (domain.BaseDirectory, arch, libWithExt, out lib))
							return lib;
					}
				} catch {
					// no-op as there may not be any domain or path
				}

				// 4. use PATH or default loading mechanism
				return libWithExt;
			}

			static bool CheckLibraryPath(string root, string arch, string libWithExt, out string foundPath)
			{
				if (!string.IsNullOrEmpty (root)) {
					// a. in specific platform sub dir
					if (!string.IsNullOrEmpty (PlatformConfiguration.LinuxFlavor)) {
						var muslLib = Path.Combine (root, PlatformConfiguration.LinuxFlavor + "-" + arch, libWithExt);
						if (File.Exists (muslLib)) {
							foundPath = muslLib;
							return true;
						}
					}

					// b. in generic platform sub dir
					var searchLib = Path.Combine (root, arch, libWithExt);
					if (File.Exists (searchLib)) {
						foundPath = searchLib;
						return true;
					}

					// c. in root
					searchLib = Path.Combine (root, libWithExt);
					if (File.Exists (searchLib)) {
						foundPath = searchLib;
						return true;
					}
				}

				// d. nothing
				foundPath = null;
				return false;
			}
		}

		public static T GetSymbolDelegate<T> (IntPtr library, string name)
			where T : Delegate
		{
			var symbol = GetSymbol (library, name);
			if (symbol == IntPtr.Zero)
				throw new EntryPointNotFoundException ($"Unable to load symbol '{name}'.");

			return Marshal.GetDelegateForFunctionPointer<T> (symbol);
		}

		public static IntPtr LoadLibrary (string libraryName)
		{
			if (string.IsNullOrEmpty (libraryName))
				throw new ArgumentNullException (nameof (libraryName));

			IntPtr handle;
			if (PlatformConfiguration.IsWindows)
				handle = Win32.LoadLibrary (libraryName);
			else if (PlatformConfiguration.IsLinux)
				handle = Linux.dlopen (libraryName);
			else if (PlatformConfiguration.IsMac)
				handle = Mac.dlopen (libraryName);
			else
				throw new PlatformNotSupportedException ($"Current platform is unknown, unable to load library '{libraryName}'.");

			return handle;
		}

		public static IntPtr GetSymbol (IntPtr library, string symbolName)
		{
			if (string.IsNullOrEmpty (symbolName))
				throw new ArgumentNullException (nameof (symbolName));

			IntPtr handle;
			if (PlatformConfiguration.IsWindows)
				handle = Win32.GetProcAddress (library, symbolName);
			else if (PlatformConfiguration.IsLinux)
				handle = Linux.dlsym (library, symbolName);
			else if (PlatformConfiguration.IsMac)
				handle = Mac.dlsym (library, symbolName);
			else
				throw new PlatformNotSupportedException ($"Current platform is unknown, unable to load symbol '{symbolName}' from library {library}.");

			return handle;
		}

		public static void FreeLibrary (IntPtr library)
		{
			if (library == IntPtr.Zero)
				return;

			if (PlatformConfiguration.IsWindows)
				Win32.FreeLibrary (library);
			else if (PlatformConfiguration.IsLinux)
				Linux.dlclose (library);
			else if (PlatformConfiguration.IsMac)
				Mac.dlclose (library);
			else
				throw new PlatformNotSupportedException ($"Current platform is unknown, unable to close library '{library}'.");
		}

#pragma warning disable IDE1006 // Naming Styles
		private static class Mac
		{
			private const string SystemLibrary = "/usr/lib/libSystem.dylib";

			private const int RTLD_LAZY = 1;
			private const int RTLD_NOW = 2;

			public static IntPtr dlopen (string path, bool lazy = true) =>
				dlopen (path, lazy ? RTLD_LAZY : RTLD_NOW);

			[DllImport (SystemLibrary)]
			public static extern IntPtr dlopen (string path, int mode);

			[DllImport (SystemLibrary)]
			public static extern IntPtr dlsym (IntPtr handle, string symbol);

			[DllImport (SystemLibrary)]
			public static extern void dlclose (IntPtr handle);
		}

		private static class Linux
		{
			private const string SystemLibrary = "libdl.so";
			private const string SystemLibrary2 = "libdl.so.2"; // newer Linux distros use this

			private const int RTLD_LAZY = 1;
			private const int RTLD_NOW = 2;
			private const int RTLD_DEEPBIND = 8;

			private static bool UseSystemLibrary2 = true;

			public static IntPtr dlopen (string path, bool lazy = true)
			{
				try {
					return dlopen2 (path, (lazy ? RTLD_LAZY : RTLD_NOW) | RTLD_DEEPBIND);
				} catch (DllNotFoundException) {
					UseSystemLibrary2 = false;
					return dlopen1 (path, (lazy ? RTLD_LAZY : RTLD_NOW) | RTLD_DEEPBIND);
				}
			}

			public static IntPtr dlsym (IntPtr handle, string symbol)
			{
				return UseSystemLibrary2 ? dlsym2 (handle, symbol) : dlsym1 (handle, symbol);
			}

			public static void dlclose (IntPtr handle)
			{
				if (UseSystemLibrary2)
					dlclose2 (handle);
				else
					dlclose1 (handle);
			}

			[DllImport (SystemLibrary, EntryPoint="dlopen")]
			private static extern IntPtr dlopen1 (string path, int mode);

			[DllImport (SystemLibrary, EntryPoint="dlsym")]
			private static extern IntPtr dlsym1 (IntPtr handle, string symbol);

			[DllImport (SystemLibrary, EntryPoint="dlclose")]
			private static extern void dlclose1 (IntPtr handle);

			[DllImport (SystemLibrary2, EntryPoint="dlopen")]
			private static extern IntPtr dlopen2 (string path, int mode);

			[DllImport (SystemLibrary2, EntryPoint="dlsym")]
			private static extern IntPtr dlsym2 (IntPtr handle, string symbol);

			[DllImport (SystemLibrary2, EntryPoint="dlclose")]
			private static extern void dlclose2 (IntPtr handle);
		}

		private static class Win32
		{
			private const string SystemLibrary = "Kernel32.dll";

			[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
			public static extern IntPtr LoadLibrary (string lpFileName);

			[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
			public static extern IntPtr GetProcAddress (IntPtr hModule, string lpProcName);

			[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
			public static extern void FreeLibrary (IntPtr hModule);
		}
#pragma warning restore IDE1006 // Naming Styles
	}
#endif
}

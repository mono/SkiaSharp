using System;
using System.IO;
using System.Runtime.InteropServices;

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
					// 1.1 in platform sub dir
					var lib = Path.Combine (path, arch, libWithExt);
					if (File.Exists (lib))
						return lib;
					// 1.2 in root
					lib = Path.Combine (path, libWithExt);
					if (File.Exists (lib))
						return lib;
				}

				// 2. try current directory
				path = Directory.GetCurrentDirectory ();
				if (!string.IsNullOrEmpty (path)) {
					// 2.1 in platform sub dir
					var lib = Path.Combine (path, arch, libWithExt);
					if (File.Exists (lib))
						return lib;
					// 2.2 in root
					lib = Path.Combine (lib, libWithExt);
					if (File.Exists (lib))
						return lib;
				}

				// 3. try app domain
				try {
					path = AppDomain.CurrentDomain?.RelativeSearchPath;
					if (!string.IsNullOrEmpty (path)) {
						// 3.1 in platform sub dir
						var lib = Path.Combine (path, arch, libWithExt);
						if (File.Exists (lib))
							return lib;
						// 3.2 in root
						lib = Path.Combine (lib, libWithExt);
						if (File.Exists (lib))
							return lib;
					}
				} catch {
					// no-op as there may not be any domain or path
				}

				// 4. use PATH or default loading mechanism
				return libWithExt;
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

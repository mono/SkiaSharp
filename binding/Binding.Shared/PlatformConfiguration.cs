using System;
using System.Runtime.InteropServices;

#if WINDOWS_UWP
using Windows.ApplicationModel;
using Windows.System;
#endif

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

		public static bool IsArm { get; }

		public static bool Is64Bit { get; }

		static PlatformConfiguration ()
		{
#if WINDOWS_UWP
			IsMac = false;
			IsLinux = false;
			IsUnix = false;
			IsWindows = true;

			var arch = Package.Current.Id.Architecture;
			const ProcessorArchitecture arm64 = (ProcessorArchitecture)12;
			IsArm = arch == ProcessorArchitecture.Arm || arch == arm64;
#else
			IsMac = RuntimeInformation.IsOSPlatform (OSPlatform.OSX);
			IsLinux = RuntimeInformation.IsOSPlatform (OSPlatform.Linux);
			IsUnix = IsMac || IsLinux;
			IsWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);

			var arch = RuntimeInformation.ProcessArchitecture;
			IsArm = arch == Architecture.Arm || arch == Architecture.Arm64;
#endif

			Is64Bit = IntPtr.Size == 8;
		}

		public static bool? IsMuslOverride { get; set; }

#if WINDOWS_UWP
		public static bool IsMusl { get; }
#else
		private static readonly Lazy<bool> isMuslLazy = new Lazy<bool> (IsMuslImplementation);

		public static bool IsMusl => IsLinux && (IsMuslOverride ?? isMuslLazy.Value);

		private static bool IsMuslImplementation ()
		{
			try
			{
				gnu_get_libc_version ();
				return false;
			}
			catch (TypeLoadException)
			{
				return true;
			}
		}

		[DllImport ("c", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr gnu_get_libc_version ();
#endif
	}
}

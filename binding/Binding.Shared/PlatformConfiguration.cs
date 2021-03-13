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
		private static readonly Lazy<bool> isMuslLazy = new Lazy<bool>(IsMuslImplementation);

		public static bool IsUnix { get; }

		public static bool IsWindows { get; }

		public static bool IsMac { get; }

		public static bool IsLinux { get; }

		public static bool IsArm { get; }

		public static bool Is64Bit { get; }

		public static bool IsMusl => isMuslLazy.Value;

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

			isMuslLazy = new Lazy<bool>(IsMuslImplementation);
		}

		private static bool IsMuslImplementation()
		{
			try
			{
				var cpu = RuntimeInformation.ProcessArchitecture;
				switch (cpu)
				{
					case Architecture.X86:
						return AccessCheck("/lib/libc.musl-x86.so.1", 0) == 0;
					case Architecture.X64:
						return AccessCheck("/lib/libc.musl-x86_64.so.1", 0) == 0;
					case Architecture.Arm:
						return AccessCheck("/lib/libc.musl-armv7.so.1", 0) == 0;
					case Architecture.Arm64:
						return AccessCheck("/lib/libc.musl-aarch64.so.1", 0) == 0;
					default:
						return false;
				}
			}
			catch
			{
				return false;
			}
		}

		[DllImport("libc.so", EntryPoint = "access")]
		private static extern unsafe int AccessCheck([MarshalAs (UnmanagedType.LPStr)] string path, int mode);
	}
}

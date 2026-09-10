#nullable disable

using System;
using System.Runtime.InteropServices;

#if WINDOWS_UWP
using Windows.ApplicationModel;
using Windows.System;
#endif

#if HARFBUZZ
namespace HarfBuzzSharp.Internals
#else
namespace SkiaSharp.Internals
#endif
{
	/// <summary>Provides properties to detect the current platform and architecture for native library loading.</summary>
	/// <remarks />
	public static class PlatformConfiguration
	{
		private const string LibCLibrary = "libc";

		/// <summary>Gets a value indicating whether the current operating system is Unix-based.</summary>
		/// <value><see langword="true" /> if the operating system is Linux or macOS; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public static bool IsUnix => IsMac || IsLinux;

		/// <summary>Gets a value indicating whether the current operating system is Windows.</summary>
		/// <value><see langword="true" /> if the operating system is Windows; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public static bool IsWindows {
#if WINDOWS_UWP
			get => true;
#elif NET6_0_OR_GREATER
			get => OperatingSystem.IsWindows ();
#else
			get => RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
#endif
		}

		/// <summary>Gets a value indicating whether the current operating system is macOS.</summary>
		/// <value><see langword="true" /> if the operating system is macOS; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public static bool IsMac {
#if WINDOWS_UWP
			get => false;
#elif NET6_0_OR_GREATER
			get => OperatingSystem.IsMacOS ();
#else
			get => RuntimeInformation.IsOSPlatform (OSPlatform.OSX);
#endif
		}

		/// <summary>Gets a value indicating whether the current operating system is Linux.</summary>
		/// <value><see langword="true" /> if the operating system is Linux; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public static bool IsLinux {
#if WINDOWS_UWP
			get => false;
#elif NET6_0_OR_GREATER
			get => OperatingSystem.IsLinux ();
#else
			get => RuntimeInformation.IsOSPlatform (OSPlatform.Linux);
#endif
		}

		/// <summary>Gets a value indicating whether the current process is running on an ARM architecture.</summary>
		/// <value><see langword="true" /> if the architecture is ARM or ARM64; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public static bool IsArm {
#if WINDOWS_UWP
			get {
				var arch = Package.Current.Id.Architecture;
				const ProcessorArchitecture arm64 = (ProcessorArchitecture)12;
				return arch == ProcessorArchitecture.Arm || arch == arm64;
			}
#else
			get => RuntimeInformation.ProcessArchitecture is Architecture.Arm or Architecture.Arm64;
#endif
		}

		/// <summary>Gets a value indicating whether the current process is running as 64-bit.</summary>
		/// <value><see langword="true" /> if the process is 64-bit; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public static bool Is64Bit => IntPtr.Size == 8;

		private static string linuxFlavor;

		/// <summary>Gets or sets the Linux distribution flavor identifier used for native library resolution.</summary>
		/// <value>The Linux flavor identifier, such as "glibc" or "musl".</value>
		/// <remarks />
		public static string LinuxFlavor
		{
			get
			{
				if (!IsLinux)
					return null;

				if (!string.IsNullOrEmpty (linuxFlavor))
					return linuxFlavor;

				// we only check for musl/glibc right now
				if (!IsGlibc)
					return "musl";

				return null;
			}
			set => linuxFlavor = value;
		}

#if WINDOWS_UWP || __IOS__ || __TVOS__ || __MACOS__ || __MACCATALYST__ || __ANDROID__
		/// <summary>Gets a value indicating whether the current Linux system uses glibc.</summary>
		/// <value><see langword="true" /> if running on a glibc-based Linux system; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public static bool IsGlibc { get; }
#else
		private static readonly Lazy<bool> isGlibcLazy = new Lazy<bool> (IsGlibcImplementation);

		/// <summary>Gets a value indicating whether the current Linux system uses glibc.</summary>
		/// <value>
		///           <see langword="true" /> if running on a glibc-based Linux system; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public static bool IsGlibc => IsLinux && isGlibcLazy.Value;

		private static bool IsGlibcImplementation ()
		{
			try
			{
				gnu_get_libc_version ();
				return true;
			}
			catch (TypeLoadException)
			{
				return false;
			}
		}

		[DllImport (LibCLibrary, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr gnu_get_libc_version ();
#endif
	}
}

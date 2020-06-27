using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public static unsafe class SkiaSharpVersion
	{
		private static readonly Version Zero = new Version (0, 0);

		private static Version nativeMinimum;
		private static Version nativeVersion;

		public static Version NativeMinimum =>
			nativeMinimum ??= new Version (VersionConstants.Milestone, VersionConstants.Increment);

		public static Version Native {
			get {
				try {
					return nativeVersion ??= new Version (SkiaApi.sk_version_get_milestone (), SkiaApi.sk_version_get_increment ());
#if NETSTANDARD1_3 || UAP10_0_10240
				} catch (Exception ex) when (ex.GetType ().FullName == "System.EntryPointNotFoundException") {
#else
				} catch (EntryPointNotFoundException) {
#endif
					return nativeVersion ??= Zero;
				}
			}
		}

		internal static string NativeString =>
			Marshal.PtrToStringAnsi ((IntPtr)SkiaApi.sk_version_get_string ());

		public static bool CheckNativeLibraryCompatible (bool throwIfIncompatible = false) =>
			CheckNativeLibraryCompatible (NativeMinimum, Native, throwIfIncompatible);

		internal static bool CheckNativeLibraryCompatible (Version minimum, Version current, bool throwIfIncompatible = false)
		{
			minimum ??= Zero;
			current ??= Zero;

			// fail fast to success if SkiaSharp is compiled without a minimum
			if (minimum <= Zero)
				return true;

			var max = new Version (minimum.Major + 1, 0);

			// fail fast if a pre-2.80 version of libSkiaSharp is loaded
			if (current <= Zero) {
				if (throwIfIncompatible)
					throw new InvalidOperationException (
						$"The version of the native libSkiaSharp library is incompatible with this version of SkiaSharp. " +
						$"Supported versions of the native libSkiaSharp library are in the range [{minimum.ToString (2)}, {max.ToString (2)}).");
				return false;
			}

			var compat = current >= minimum && current < max;

			if (!compat && throwIfIncompatible)
				throw new InvalidOperationException (
					$"The version of the native libSkiaSharp library ({current.ToString (2)}) is incompatible with this version of SkiaSharp. " +
					$"Supported versions of the native libSkiaSharp library are in the range [{minimum.ToString (2)}, {max.ToString (2)}).");

			return compat;
		}
	}
}

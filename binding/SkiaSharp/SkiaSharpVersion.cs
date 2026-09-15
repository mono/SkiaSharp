#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	/// <summary>Provides version information for SkiaSharp and its native library.</summary>
	/// <remarks />
	public static unsafe class SkiaSharpVersion
	{
		private static readonly Version Zero = new Version (0, 0);

		private static Version nativeMinimum;
		private static Version nativeVersion;

		/// <summary>Gets the minimum required version of the native Skia library.</summary>
		/// <value>The minimum required version of the native library.</value>
		/// <remarks />
		public static Version NativeMinimum =>
			nativeMinimum ??= new Version (VersionConstants.Milestone, VersionConstants.Increment);

		/// <summary>Gets the version of the native Skia library.</summary>
		/// <value>The version of the native library.</value>
		/// <remarks />
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

		/// <summary>Checks whether the native library version is compatible with the managed assembly.</summary>
		/// <param name="throwIfIncompatible"><see langword="true" /> to throw an exception if the native library is incompatible; otherwise, <see langword="false" />.</param>
		/// <returns><see langword="true" /> if the native library is compatible; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool CheckNativeLibraryCompatible (bool throwIfIncompatible = false) =>
			CheckNativeLibraryCompatible (NativeMinimum, Native, throwIfIncompatible);

		internal static bool CheckNativeLibraryCompatible (Version minSupported, Version current, bool throwIfIncompatible = false)
		{
			minSupported ??= Zero;
			current ??= Zero;

			// fail fast to success if SkiaSharp is compiled without a minimum
			if (minSupported <= Zero)
				return true;

			// get the next MAJOR version which is always incompatible
			var maxSupported = new Version (minSupported.Major + 1, 0);

			// fail fast if a pre-2.80 version of libSkiaSharp is loaded
			if (current <= Zero) {
				if (throwIfIncompatible)
					throw new InvalidOperationException (
						$"The version of the native libSkiaSharp library is incompatible with this version of SkiaSharp. " +
						$"Supported versions of the native libSkiaSharp library are in the range [{minSupported.ToString (2)}, {maxSupported.ToString (2)}).");
				return false;
			}

			var isIncompatible = current < minSupported || current >= maxSupported;

			if (isIncompatible && throwIfIncompatible)
				throw new InvalidOperationException (
					$"The version of the native libSkiaSharp library ({current.ToString (2)}) is incompatible with this version of SkiaSharp. " +
					$"Supported versions of the native libSkiaSharp library are in the range [{minSupported.ToString (2)}, {maxSupported.ToString (2)}).");

			return !isIncompatible;
		}
	}
}

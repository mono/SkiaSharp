using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_colorspace.h

		// void sk_color4f_from_color(sk_color_t color, sk_color4f_t* color4f)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_color4f_from_color (UInt32 color, SKColorF* color4f);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_color4f_from_color (UInt32 color, SKColorF* color4f);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_color4f_from_color (UInt32 color, SKColorF* color4f);
		}
		private static Delegates.sk_color4f_from_color sk_color4f_from_color_delegate;
		internal static void sk_color4f_from_color (UInt32 color, SKColorF* color4f) =>
			(sk_color4f_from_color_delegate ??= GetSymbol<Delegates.sk_color4f_from_color> ("sk_color4f_from_color")).Invoke (color, color4f);
		#endif

		// sk_color_t sk_color4f_to_color(const sk_color4f_t* color4f)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt32 sk_color4f_to_color (SKColorF* color4f);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_color4f_to_color (SKColorF* color4f);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_color4f_to_color (SKColorF* color4f);
		}
		private static Delegates.sk_color4f_to_color sk_color4f_to_color_delegate;
		internal static UInt32 sk_color4f_to_color (SKColorF* color4f) =>
			(sk_color4f_to_color_delegate ??= GetSymbol<Delegates.sk_color4f_to_color> ("sk_color4f_to_color")).Invoke (color4f);
		#endif

		// bool sk_colorspace_equals(const sk_colorspace_t* src, const sk_colorspace_t* dst)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_colorspace_equals (sk_colorspace_t src, sk_colorspace_t dst);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_equals (sk_colorspace_t src, sk_colorspace_t dst);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_equals (sk_colorspace_t src, sk_colorspace_t dst);
		}
		private static Delegates.sk_colorspace_equals sk_colorspace_equals_delegate;
		internal static bool sk_colorspace_equals (sk_colorspace_t src, sk_colorspace_t dst) =>
			(sk_colorspace_equals_delegate ??= GetSymbol<Delegates.sk_colorspace_equals> ("sk_colorspace_equals")).Invoke (src, dst);
		#endif

		// bool sk_colorspace_gamma_close_to_srgb(const sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_colorspace_gamma_close_to_srgb (sk_colorspace_t colorspace);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_gamma_close_to_srgb (sk_colorspace_t colorspace);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_gamma_close_to_srgb (sk_colorspace_t colorspace);
		}
		private static Delegates.sk_colorspace_gamma_close_to_srgb sk_colorspace_gamma_close_to_srgb_delegate;
		internal static bool sk_colorspace_gamma_close_to_srgb (sk_colorspace_t colorspace) =>
			(sk_colorspace_gamma_close_to_srgb_delegate ??= GetSymbol<Delegates.sk_colorspace_gamma_close_to_srgb> ("sk_colorspace_gamma_close_to_srgb")).Invoke (colorspace);
		#endif

		// bool sk_colorspace_gamma_is_linear(const sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_colorspace_gamma_is_linear (sk_colorspace_t colorspace);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_gamma_is_linear (sk_colorspace_t colorspace);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_gamma_is_linear (sk_colorspace_t colorspace);
		}
		private static Delegates.sk_colorspace_gamma_is_linear sk_colorspace_gamma_is_linear_delegate;
		internal static bool sk_colorspace_gamma_is_linear (sk_colorspace_t colorspace) =>
			(sk_colorspace_gamma_is_linear_delegate ??= GetSymbol<Delegates.sk_colorspace_gamma_is_linear> ("sk_colorspace_gamma_is_linear")).Invoke (colorspace);
		#endif

		// void sk_colorspace_icc_profile_delete(sk_colorspace_icc_profile_t* profile)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_icc_profile_delete (sk_colorspace_icc_profile_t profile);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_icc_profile_delete (sk_colorspace_icc_profile_t profile);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_icc_profile_delete (sk_colorspace_icc_profile_t profile);
		}
		private static Delegates.sk_colorspace_icc_profile_delete sk_colorspace_icc_profile_delete_delegate;
		internal static void sk_colorspace_icc_profile_delete (sk_colorspace_icc_profile_t profile) =>
			(sk_colorspace_icc_profile_delete_delegate ??= GetSymbol<Delegates.sk_colorspace_icc_profile_delete> ("sk_colorspace_icc_profile_delete")).Invoke (profile);
		#endif

		// const uint8_t* sk_colorspace_icc_profile_get_buffer(const sk_colorspace_icc_profile_t* profile, uint32_t* size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Byte* sk_colorspace_icc_profile_get_buffer (sk_colorspace_icc_profile_t profile, UInt32* size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte* sk_colorspace_icc_profile_get_buffer (sk_colorspace_icc_profile_t profile, UInt32* size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte* sk_colorspace_icc_profile_get_buffer (sk_colorspace_icc_profile_t profile, UInt32* size);
		}
		private static Delegates.sk_colorspace_icc_profile_get_buffer sk_colorspace_icc_profile_get_buffer_delegate;
		internal static Byte* sk_colorspace_icc_profile_get_buffer (sk_colorspace_icc_profile_t profile, UInt32* size) =>
			(sk_colorspace_icc_profile_get_buffer_delegate ??= GetSymbol<Delegates.sk_colorspace_icc_profile_get_buffer> ("sk_colorspace_icc_profile_get_buffer")).Invoke (profile, size);
		#endif

		// bool sk_colorspace_icc_profile_get_to_xyzd50(const sk_colorspace_icc_profile_t* profile, sk_colorspace_xyz_t* toXYZD50)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_colorspace_icc_profile_get_to_xyzd50 (sk_colorspace_icc_profile_t profile, SKColorSpaceXyz* toXYZD50);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_icc_profile_get_to_xyzd50 (sk_colorspace_icc_profile_t profile, SKColorSpaceXyz* toXYZD50);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_icc_profile_get_to_xyzd50 (sk_colorspace_icc_profile_t profile, SKColorSpaceXyz* toXYZD50);
		}
		private static Delegates.sk_colorspace_icc_profile_get_to_xyzd50 sk_colorspace_icc_profile_get_to_xyzd50_delegate;
		internal static bool sk_colorspace_icc_profile_get_to_xyzd50 (sk_colorspace_icc_profile_t profile, SKColorSpaceXyz* toXYZD50) =>
			(sk_colorspace_icc_profile_get_to_xyzd50_delegate ??= GetSymbol<Delegates.sk_colorspace_icc_profile_get_to_xyzd50> ("sk_colorspace_icc_profile_get_to_xyzd50")).Invoke (profile, toXYZD50);
		#endif

		// sk_colorspace_icc_profile_t* sk_colorspace_icc_profile_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorspace_icc_profile_t sk_colorspace_icc_profile_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_icc_profile_t sk_colorspace_icc_profile_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_icc_profile_t sk_colorspace_icc_profile_new ();
		}
		private static Delegates.sk_colorspace_icc_profile_new sk_colorspace_icc_profile_new_delegate;
		internal static sk_colorspace_icc_profile_t sk_colorspace_icc_profile_new () =>
			(sk_colorspace_icc_profile_new_delegate ??= GetSymbol<Delegates.sk_colorspace_icc_profile_new> ("sk_colorspace_icc_profile_new")).Invoke ();
		#endif

		// bool sk_colorspace_icc_profile_parse(const void* buffer, size_t length, sk_colorspace_icc_profile_t* profile)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_colorspace_icc_profile_parse (void* buffer, /* size_t */ IntPtr length, sk_colorspace_icc_profile_t profile);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_icc_profile_parse (void* buffer, /* size_t */ IntPtr length, sk_colorspace_icc_profile_t profile);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_icc_profile_parse (void* buffer, /* size_t */ IntPtr length, sk_colorspace_icc_profile_t profile);
		}
		private static Delegates.sk_colorspace_icc_profile_parse sk_colorspace_icc_profile_parse_delegate;
		internal static bool sk_colorspace_icc_profile_parse (void* buffer, /* size_t */ IntPtr length, sk_colorspace_icc_profile_t profile) =>
			(sk_colorspace_icc_profile_parse_delegate ??= GetSymbol<Delegates.sk_colorspace_icc_profile_parse> ("sk_colorspace_icc_profile_parse")).Invoke (buffer, length, profile);
		#endif

		// bool sk_colorspace_is_numerical_transfer_fn(const sk_colorspace_t* colorspace, sk_colorspace_transfer_fn_t* transferFn)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_colorspace_is_numerical_transfer_fn (sk_colorspace_t colorspace, SKColorSpaceTransferFn* transferFn);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_is_numerical_transfer_fn (sk_colorspace_t colorspace, SKColorSpaceTransferFn* transferFn);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_is_numerical_transfer_fn (sk_colorspace_t colorspace, SKColorSpaceTransferFn* transferFn);
		}
		private static Delegates.sk_colorspace_is_numerical_transfer_fn sk_colorspace_is_numerical_transfer_fn_delegate;
		internal static bool sk_colorspace_is_numerical_transfer_fn (sk_colorspace_t colorspace, SKColorSpaceTransferFn* transferFn) =>
			(sk_colorspace_is_numerical_transfer_fn_delegate ??= GetSymbol<Delegates.sk_colorspace_is_numerical_transfer_fn> ("sk_colorspace_is_numerical_transfer_fn")).Invoke (colorspace, transferFn);
		#endif

		// bool sk_colorspace_is_srgb(const sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_colorspace_is_srgb (sk_colorspace_t colorspace);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_is_srgb (sk_colorspace_t colorspace);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_is_srgb (sk_colorspace_t colorspace);
		}
		private static Delegates.sk_colorspace_is_srgb sk_colorspace_is_srgb_delegate;
		internal static bool sk_colorspace_is_srgb (sk_colorspace_t colorspace) =>
			(sk_colorspace_is_srgb_delegate ??= GetSymbol<Delegates.sk_colorspace_is_srgb> ("sk_colorspace_is_srgb")).Invoke (colorspace);
		#endif

		// sk_colorspace_t* sk_colorspace_make_linear_gamma(const sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorspace_t sk_colorspace_make_linear_gamma (sk_colorspace_t colorspace);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_make_linear_gamma (sk_colorspace_t colorspace);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_colorspace_make_linear_gamma (sk_colorspace_t colorspace);
		}
		private static Delegates.sk_colorspace_make_linear_gamma sk_colorspace_make_linear_gamma_delegate;
		internal static sk_colorspace_t sk_colorspace_make_linear_gamma (sk_colorspace_t colorspace) =>
			(sk_colorspace_make_linear_gamma_delegate ??= GetSymbol<Delegates.sk_colorspace_make_linear_gamma> ("sk_colorspace_make_linear_gamma")).Invoke (colorspace);
		#endif

		// sk_colorspace_t* sk_colorspace_make_srgb_gamma(const sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorspace_t sk_colorspace_make_srgb_gamma (sk_colorspace_t colorspace);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_make_srgb_gamma (sk_colorspace_t colorspace);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_colorspace_make_srgb_gamma (sk_colorspace_t colorspace);
		}
		private static Delegates.sk_colorspace_make_srgb_gamma sk_colorspace_make_srgb_gamma_delegate;
		internal static sk_colorspace_t sk_colorspace_make_srgb_gamma (sk_colorspace_t colorspace) =>
			(sk_colorspace_make_srgb_gamma_delegate ??= GetSymbol<Delegates.sk_colorspace_make_srgb_gamma> ("sk_colorspace_make_srgb_gamma")).Invoke (colorspace);
		#endif

		// sk_colorspace_t* sk_colorspace_new_cicp(sk_colorspace_primaries_cicp_t colorPrimaries, sk_colorspace_transfer_fn_cicp_t transferCharacteristics)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorspace_t sk_colorspace_new_cicp (SKColorspacePrimariesCicp colorPrimaries, SKColorspaceTransferFnCicp transferCharacteristics);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_cicp (SKColorspacePrimariesCicp colorPrimaries, SKColorspaceTransferFnCicp transferCharacteristics);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_colorspace_new_cicp (SKColorspacePrimariesCicp colorPrimaries, SKColorspaceTransferFnCicp transferCharacteristics);
		}
		private static Delegates.sk_colorspace_new_cicp sk_colorspace_new_cicp_delegate;
		internal static sk_colorspace_t sk_colorspace_new_cicp (SKColorspacePrimariesCicp colorPrimaries, SKColorspaceTransferFnCicp transferCharacteristics) =>
			(sk_colorspace_new_cicp_delegate ??= GetSymbol<Delegates.sk_colorspace_new_cicp> ("sk_colorspace_new_cicp")).Invoke (colorPrimaries, transferCharacteristics);
		#endif

		// sk_colorspace_t* sk_colorspace_new_icc(const sk_colorspace_icc_profile_t* profile)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorspace_t sk_colorspace_new_icc (sk_colorspace_icc_profile_t profile);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_icc (sk_colorspace_icc_profile_t profile);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_colorspace_new_icc (sk_colorspace_icc_profile_t profile);
		}
		private static Delegates.sk_colorspace_new_icc sk_colorspace_new_icc_delegate;
		internal static sk_colorspace_t sk_colorspace_new_icc (sk_colorspace_icc_profile_t profile) =>
			(sk_colorspace_new_icc_delegate ??= GetSymbol<Delegates.sk_colorspace_new_icc> ("sk_colorspace_new_icc")).Invoke (profile);
		#endif

		// sk_colorspace_t* sk_colorspace_new_rgb(const sk_colorspace_transfer_fn_t* transferFn, const sk_colorspace_xyz_t* toXYZD50)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorspace_t sk_colorspace_new_rgb (SKColorSpaceTransferFn* transferFn, SKColorSpaceXyz* toXYZD50);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_rgb (SKColorSpaceTransferFn* transferFn, SKColorSpaceXyz* toXYZD50);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_colorspace_new_rgb (SKColorSpaceTransferFn* transferFn, SKColorSpaceXyz* toXYZD50);
		}
		private static Delegates.sk_colorspace_new_rgb sk_colorspace_new_rgb_delegate;
		internal static sk_colorspace_t sk_colorspace_new_rgb (SKColorSpaceTransferFn* transferFn, SKColorSpaceXyz* toXYZD50) =>
			(sk_colorspace_new_rgb_delegate ??= GetSymbol<Delegates.sk_colorspace_new_rgb> ("sk_colorspace_new_rgb")).Invoke (transferFn, toXYZD50);
		#endif

		// sk_colorspace_t* sk_colorspace_new_srgb()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorspace_t sk_colorspace_new_srgb ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_srgb ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_colorspace_new_srgb ();
		}
		private static Delegates.sk_colorspace_new_srgb sk_colorspace_new_srgb_delegate;
		internal static sk_colorspace_t sk_colorspace_new_srgb () =>
			(sk_colorspace_new_srgb_delegate ??= GetSymbol<Delegates.sk_colorspace_new_srgb> ("sk_colorspace_new_srgb")).Invoke ();
		#endif

		// sk_colorspace_t* sk_colorspace_new_srgb_linear()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorspace_t sk_colorspace_new_srgb_linear ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_srgb_linear ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_colorspace_new_srgb_linear ();
		}
		private static Delegates.sk_colorspace_new_srgb_linear sk_colorspace_new_srgb_linear_delegate;
		internal static sk_colorspace_t sk_colorspace_new_srgb_linear () =>
			(sk_colorspace_new_srgb_linear_delegate ??= GetSymbol<Delegates.sk_colorspace_new_srgb_linear> ("sk_colorspace_new_srgb_linear")).Invoke ();
		#endif

		// bool sk_colorspace_primaries_to_xyzd50(const sk_colorspace_primaries_t* primaries, sk_colorspace_xyz_t* toXYZD50)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_colorspace_primaries_to_xyzd50 (SKColorSpacePrimaries* primaries, SKColorSpaceXyz* toXYZD50);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_primaries_to_xyzd50 (SKColorSpacePrimaries* primaries, SKColorSpaceXyz* toXYZD50);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_primaries_to_xyzd50 (SKColorSpacePrimaries* primaries, SKColorSpaceXyz* toXYZD50);
		}
		private static Delegates.sk_colorspace_primaries_to_xyzd50 sk_colorspace_primaries_to_xyzd50_delegate;
		internal static bool sk_colorspace_primaries_to_xyzd50 (SKColorSpacePrimaries* primaries, SKColorSpaceXyz* toXYZD50) =>
			(sk_colorspace_primaries_to_xyzd50_delegate ??= GetSymbol<Delegates.sk_colorspace_primaries_to_xyzd50> ("sk_colorspace_primaries_to_xyzd50")).Invoke (primaries, toXYZD50);
		#endif

		// void sk_colorspace_ref(sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_ref (sk_colorspace_t colorspace);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_ref (sk_colorspace_t colorspace);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_ref (sk_colorspace_t colorspace);
		}
		private static Delegates.sk_colorspace_ref sk_colorspace_ref_delegate;
		internal static void sk_colorspace_ref (sk_colorspace_t colorspace) =>
			(sk_colorspace_ref_delegate ??= GetSymbol<Delegates.sk_colorspace_ref> ("sk_colorspace_ref")).Invoke (colorspace);
		#endif

		// void sk_colorspace_to_profile(const sk_colorspace_t* colorspace, sk_colorspace_icc_profile_t* profile)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_to_profile (sk_colorspace_t colorspace, sk_colorspace_icc_profile_t profile);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_to_profile (sk_colorspace_t colorspace, sk_colorspace_icc_profile_t profile);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_to_profile (sk_colorspace_t colorspace, sk_colorspace_icc_profile_t profile);
		}
		private static Delegates.sk_colorspace_to_profile sk_colorspace_to_profile_delegate;
		internal static void sk_colorspace_to_profile (sk_colorspace_t colorspace, sk_colorspace_icc_profile_t profile) =>
			(sk_colorspace_to_profile_delegate ??= GetSymbol<Delegates.sk_colorspace_to_profile> ("sk_colorspace_to_profile")).Invoke (colorspace, profile);
		#endif

		// bool sk_colorspace_to_xyzd50(const sk_colorspace_t* colorspace, sk_colorspace_xyz_t* toXYZD50)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_colorspace_to_xyzd50 (sk_colorspace_t colorspace, SKColorSpaceXyz* toXYZD50);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_to_xyzd50 (sk_colorspace_t colorspace, SKColorSpaceXyz* toXYZD50);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_to_xyzd50 (sk_colorspace_t colorspace, SKColorSpaceXyz* toXYZD50);
		}
		private static Delegates.sk_colorspace_to_xyzd50 sk_colorspace_to_xyzd50_delegate;
		internal static bool sk_colorspace_to_xyzd50 (sk_colorspace_t colorspace, SKColorSpaceXyz* toXYZD50) =>
			(sk_colorspace_to_xyzd50_delegate ??= GetSymbol<Delegates.sk_colorspace_to_xyzd50> ("sk_colorspace_to_xyzd50")).Invoke (colorspace, toXYZD50);
		#endif

		// float sk_colorspace_transfer_fn_eval(const sk_colorspace_transfer_fn_t* transferFn, float x)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_colorspace_transfer_fn_eval (SKColorSpaceTransferFn* transferFn, Single x);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_colorspace_transfer_fn_eval (SKColorSpaceTransferFn* transferFn, Single x);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_colorspace_transfer_fn_eval (SKColorSpaceTransferFn* transferFn, Single x);
		}
		private static Delegates.sk_colorspace_transfer_fn_eval sk_colorspace_transfer_fn_eval_delegate;
		internal static Single sk_colorspace_transfer_fn_eval (SKColorSpaceTransferFn* transferFn, Single x) =>
			(sk_colorspace_transfer_fn_eval_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_eval> ("sk_colorspace_transfer_fn_eval")).Invoke (transferFn, x);
		#endif

		// bool sk_colorspace_transfer_fn_invert(const sk_colorspace_transfer_fn_t* src, sk_colorspace_transfer_fn_t* dst)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_colorspace_transfer_fn_invert (SKColorSpaceTransferFn* src, SKColorSpaceTransferFn* dst);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_transfer_fn_invert (SKColorSpaceTransferFn* src, SKColorSpaceTransferFn* dst);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_transfer_fn_invert (SKColorSpaceTransferFn* src, SKColorSpaceTransferFn* dst);
		}
		private static Delegates.sk_colorspace_transfer_fn_invert sk_colorspace_transfer_fn_invert_delegate;
		internal static bool sk_colorspace_transfer_fn_invert (SKColorSpaceTransferFn* src, SKColorSpaceTransferFn* dst) =>
			(sk_colorspace_transfer_fn_invert_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_invert> ("sk_colorspace_transfer_fn_invert")).Invoke (src, dst);
		#endif

		// void sk_colorspace_transfer_fn_named_2dot2(sk_colorspace_transfer_fn_t* transferFn)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_transfer_fn_named_2dot2 (SKColorSpaceTransferFn* transferFn);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_transfer_fn_named_2dot2 (SKColorSpaceTransferFn* transferFn);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_transfer_fn_named_2dot2 (SKColorSpaceTransferFn* transferFn);
		}
		private static Delegates.sk_colorspace_transfer_fn_named_2dot2 sk_colorspace_transfer_fn_named_2dot2_delegate;
		internal static void sk_colorspace_transfer_fn_named_2dot2 (SKColorSpaceTransferFn* transferFn) =>
			(sk_colorspace_transfer_fn_named_2dot2_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_named_2dot2> ("sk_colorspace_transfer_fn_named_2dot2")).Invoke (transferFn);
		#endif

		// void sk_colorspace_transfer_fn_named_hlg(sk_colorspace_transfer_fn_t* transferFn)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_transfer_fn_named_hlg (SKColorSpaceTransferFn* transferFn);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_transfer_fn_named_hlg (SKColorSpaceTransferFn* transferFn);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_transfer_fn_named_hlg (SKColorSpaceTransferFn* transferFn);
		}
		private static Delegates.sk_colorspace_transfer_fn_named_hlg sk_colorspace_transfer_fn_named_hlg_delegate;
		internal static void sk_colorspace_transfer_fn_named_hlg (SKColorSpaceTransferFn* transferFn) =>
			(sk_colorspace_transfer_fn_named_hlg_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_named_hlg> ("sk_colorspace_transfer_fn_named_hlg")).Invoke (transferFn);
		#endif

		// void sk_colorspace_transfer_fn_named_linear(sk_colorspace_transfer_fn_t* transferFn)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_transfer_fn_named_linear (SKColorSpaceTransferFn* transferFn);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_transfer_fn_named_linear (SKColorSpaceTransferFn* transferFn);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_transfer_fn_named_linear (SKColorSpaceTransferFn* transferFn);
		}
		private static Delegates.sk_colorspace_transfer_fn_named_linear sk_colorspace_transfer_fn_named_linear_delegate;
		internal static void sk_colorspace_transfer_fn_named_linear (SKColorSpaceTransferFn* transferFn) =>
			(sk_colorspace_transfer_fn_named_linear_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_named_linear> ("sk_colorspace_transfer_fn_named_linear")).Invoke (transferFn);
		#endif

		// void sk_colorspace_transfer_fn_named_pq(sk_colorspace_transfer_fn_t* transferFn)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_transfer_fn_named_pq (SKColorSpaceTransferFn* transferFn);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_transfer_fn_named_pq (SKColorSpaceTransferFn* transferFn);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_transfer_fn_named_pq (SKColorSpaceTransferFn* transferFn);
		}
		private static Delegates.sk_colorspace_transfer_fn_named_pq sk_colorspace_transfer_fn_named_pq_delegate;
		internal static void sk_colorspace_transfer_fn_named_pq (SKColorSpaceTransferFn* transferFn) =>
			(sk_colorspace_transfer_fn_named_pq_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_named_pq> ("sk_colorspace_transfer_fn_named_pq")).Invoke (transferFn);
		#endif

		// void sk_colorspace_transfer_fn_named_rec2020(sk_colorspace_transfer_fn_t* transferFn)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_transfer_fn_named_rec2020 (SKColorSpaceTransferFn* transferFn);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_transfer_fn_named_rec2020 (SKColorSpaceTransferFn* transferFn);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_transfer_fn_named_rec2020 (SKColorSpaceTransferFn* transferFn);
		}
		private static Delegates.sk_colorspace_transfer_fn_named_rec2020 sk_colorspace_transfer_fn_named_rec2020_delegate;
		internal static void sk_colorspace_transfer_fn_named_rec2020 (SKColorSpaceTransferFn* transferFn) =>
			(sk_colorspace_transfer_fn_named_rec2020_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_named_rec2020> ("sk_colorspace_transfer_fn_named_rec2020")).Invoke (transferFn);
		#endif

		// void sk_colorspace_transfer_fn_named_srgb(sk_colorspace_transfer_fn_t* transferFn)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_transfer_fn_named_srgb (SKColorSpaceTransferFn* transferFn);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_transfer_fn_named_srgb (SKColorSpaceTransferFn* transferFn);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_transfer_fn_named_srgb (SKColorSpaceTransferFn* transferFn);
		}
		private static Delegates.sk_colorspace_transfer_fn_named_srgb sk_colorspace_transfer_fn_named_srgb_delegate;
		internal static void sk_colorspace_transfer_fn_named_srgb (SKColorSpaceTransferFn* transferFn) =>
			(sk_colorspace_transfer_fn_named_srgb_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_named_srgb> ("sk_colorspace_transfer_fn_named_srgb")).Invoke (transferFn);
		#endif

		// void sk_colorspace_unref(sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_unref (sk_colorspace_t colorspace);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_unref (sk_colorspace_t colorspace);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_unref (sk_colorspace_t colorspace);
		}
		private static Delegates.sk_colorspace_unref sk_colorspace_unref_delegate;
		internal static void sk_colorspace_unref (sk_colorspace_t colorspace) =>
			(sk_colorspace_unref_delegate ??= GetSymbol<Delegates.sk_colorspace_unref> ("sk_colorspace_unref")).Invoke (colorspace);
		#endif

		// void sk_colorspace_xyz_concat(const sk_colorspace_xyz_t* a, const sk_colorspace_xyz_t* b, sk_colorspace_xyz_t* result)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_xyz_concat (SKColorSpaceXyz* a, SKColorSpaceXyz* b, SKColorSpaceXyz* result);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_xyz_concat (SKColorSpaceXyz* a, SKColorSpaceXyz* b, SKColorSpaceXyz* result);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_xyz_concat (SKColorSpaceXyz* a, SKColorSpaceXyz* b, SKColorSpaceXyz* result);
		}
		private static Delegates.sk_colorspace_xyz_concat sk_colorspace_xyz_concat_delegate;
		internal static void sk_colorspace_xyz_concat (SKColorSpaceXyz* a, SKColorSpaceXyz* b, SKColorSpaceXyz* result) =>
			(sk_colorspace_xyz_concat_delegate ??= GetSymbol<Delegates.sk_colorspace_xyz_concat> ("sk_colorspace_xyz_concat")).Invoke (a, b, result);
		#endif

		// bool sk_colorspace_xyz_invert(const sk_colorspace_xyz_t* src, sk_colorspace_xyz_t* dst)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_colorspace_xyz_invert (SKColorSpaceXyz* src, SKColorSpaceXyz* dst);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_xyz_invert (SKColorSpaceXyz* src, SKColorSpaceXyz* dst);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_xyz_invert (SKColorSpaceXyz* src, SKColorSpaceXyz* dst);
		}
		private static Delegates.sk_colorspace_xyz_invert sk_colorspace_xyz_invert_delegate;
		internal static bool sk_colorspace_xyz_invert (SKColorSpaceXyz* src, SKColorSpaceXyz* dst) =>
			(sk_colorspace_xyz_invert_delegate ??= GetSymbol<Delegates.sk_colorspace_xyz_invert> ("sk_colorspace_xyz_invert")).Invoke (src, dst);
		#endif

		// void sk_colorspace_xyz_named_adobe_rgb(sk_colorspace_xyz_t* xyz)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_xyz_named_adobe_rgb (SKColorSpaceXyz* xyz);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_xyz_named_adobe_rgb (SKColorSpaceXyz* xyz);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_xyz_named_adobe_rgb (SKColorSpaceXyz* xyz);
		}
		private static Delegates.sk_colorspace_xyz_named_adobe_rgb sk_colorspace_xyz_named_adobe_rgb_delegate;
		internal static void sk_colorspace_xyz_named_adobe_rgb (SKColorSpaceXyz* xyz) =>
			(sk_colorspace_xyz_named_adobe_rgb_delegate ??= GetSymbol<Delegates.sk_colorspace_xyz_named_adobe_rgb> ("sk_colorspace_xyz_named_adobe_rgb")).Invoke (xyz);
		#endif

		// void sk_colorspace_xyz_named_display_p3(sk_colorspace_xyz_t* xyz)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_xyz_named_display_p3 (SKColorSpaceXyz* xyz);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_xyz_named_display_p3 (SKColorSpaceXyz* xyz);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_xyz_named_display_p3 (SKColorSpaceXyz* xyz);
		}
		private static Delegates.sk_colorspace_xyz_named_display_p3 sk_colorspace_xyz_named_display_p3_delegate;
		internal static void sk_colorspace_xyz_named_display_p3 (SKColorSpaceXyz* xyz) =>
			(sk_colorspace_xyz_named_display_p3_delegate ??= GetSymbol<Delegates.sk_colorspace_xyz_named_display_p3> ("sk_colorspace_xyz_named_display_p3")).Invoke (xyz);
		#endif

		// void sk_colorspace_xyz_named_rec2020(sk_colorspace_xyz_t* xyz)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_xyz_named_rec2020 (SKColorSpaceXyz* xyz);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_xyz_named_rec2020 (SKColorSpaceXyz* xyz);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_xyz_named_rec2020 (SKColorSpaceXyz* xyz);
		}
		private static Delegates.sk_colorspace_xyz_named_rec2020 sk_colorspace_xyz_named_rec2020_delegate;
		internal static void sk_colorspace_xyz_named_rec2020 (SKColorSpaceXyz* xyz) =>
			(sk_colorspace_xyz_named_rec2020_delegate ??= GetSymbol<Delegates.sk_colorspace_xyz_named_rec2020> ("sk_colorspace_xyz_named_rec2020")).Invoke (xyz);
		#endif

		// void sk_colorspace_xyz_named_srgb(sk_colorspace_xyz_t* xyz)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_xyz_named_srgb (SKColorSpaceXyz* xyz);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_xyz_named_srgb (SKColorSpaceXyz* xyz);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_xyz_named_srgb (SKColorSpaceXyz* xyz);
		}
		private static Delegates.sk_colorspace_xyz_named_srgb sk_colorspace_xyz_named_srgb_delegate;
		internal static void sk_colorspace_xyz_named_srgb (SKColorSpaceXyz* xyz) =>
			(sk_colorspace_xyz_named_srgb_delegate ??= GetSymbol<Delegates.sk_colorspace_xyz_named_srgb> ("sk_colorspace_xyz_named_srgb")).Invoke (xyz);
		#endif

		// void sk_colorspace_xyz_named_xyz(sk_colorspace_xyz_t* xyz)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorspace_xyz_named_xyz (SKColorSpaceXyz* xyz);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_xyz_named_xyz (SKColorSpaceXyz* xyz);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_xyz_named_xyz (SKColorSpaceXyz* xyz);
		}
		private static Delegates.sk_colorspace_xyz_named_xyz sk_colorspace_xyz_named_xyz_delegate;
		internal static void sk_colorspace_xyz_named_xyz (SKColorSpaceXyz* xyz) =>
			(sk_colorspace_xyz_named_xyz_delegate ??= GetSymbol<Delegates.sk_colorspace_xyz_named_xyz> ("sk_colorspace_xyz_named_xyz")).Invoke (xyz);
		#endif

		#endregion

	}
}

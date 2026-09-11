using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_maskfilter.h

		// sk_maskfilter_t* sk_maskfilter_new_blur(sk_blurstyle_t, float sigma)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_maskfilter_t sk_maskfilter_new_blur (SKBlurStyle param0, Single sigma);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_blur (SKBlurStyle param0, Single sigma);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_maskfilter_t sk_maskfilter_new_blur (SKBlurStyle param0, Single sigma);
		}
		private static Delegates.sk_maskfilter_new_blur sk_maskfilter_new_blur_delegate;
		internal static sk_maskfilter_t sk_maskfilter_new_blur (SKBlurStyle param0, Single sigma) =>
			(sk_maskfilter_new_blur_delegate ??= GetSymbol<Delegates.sk_maskfilter_new_blur> ("sk_maskfilter_new_blur")).Invoke (param0, sigma);
		#endif

		// sk_maskfilter_t* sk_maskfilter_new_blur_with_flags(sk_blurstyle_t, float sigma, bool respectCTM)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_maskfilter_t sk_maskfilter_new_blur_with_flags (SKBlurStyle param0, Single sigma, [MarshalAs (UnmanagedType.I1)] bool respectCTM);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_blur_with_flags (SKBlurStyle param0, Single sigma, [MarshalAs (UnmanagedType.I1)] bool respectCTM);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_maskfilter_t sk_maskfilter_new_blur_with_flags (SKBlurStyle param0, Single sigma, [MarshalAs (UnmanagedType.I1)] bool respectCTM);
		}
		private static Delegates.sk_maskfilter_new_blur_with_flags sk_maskfilter_new_blur_with_flags_delegate;
		internal static sk_maskfilter_t sk_maskfilter_new_blur_with_flags (SKBlurStyle param0, Single sigma, [MarshalAs (UnmanagedType.I1)] bool respectCTM) =>
			(sk_maskfilter_new_blur_with_flags_delegate ??= GetSymbol<Delegates.sk_maskfilter_new_blur_with_flags> ("sk_maskfilter_new_blur_with_flags")).Invoke (param0, sigma, respectCTM);
		#endif

		// sk_maskfilter_t* sk_maskfilter_new_clip(uint8_t min, uint8_t max)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_maskfilter_t sk_maskfilter_new_clip (Byte min, Byte max);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_clip (Byte min, Byte max);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_maskfilter_t sk_maskfilter_new_clip (Byte min, Byte max);
		}
		private static Delegates.sk_maskfilter_new_clip sk_maskfilter_new_clip_delegate;
		internal static sk_maskfilter_t sk_maskfilter_new_clip (Byte min, Byte max) =>
			(sk_maskfilter_new_clip_delegate ??= GetSymbol<Delegates.sk_maskfilter_new_clip> ("sk_maskfilter_new_clip")).Invoke (min, max);
		#endif

		// sk_maskfilter_t* sk_maskfilter_new_gamma(float gamma)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_maskfilter_t sk_maskfilter_new_gamma (Single gamma);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_gamma (Single gamma);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_maskfilter_t sk_maskfilter_new_gamma (Single gamma);
		}
		private static Delegates.sk_maskfilter_new_gamma sk_maskfilter_new_gamma_delegate;
		internal static sk_maskfilter_t sk_maskfilter_new_gamma (Single gamma) =>
			(sk_maskfilter_new_gamma_delegate ??= GetSymbol<Delegates.sk_maskfilter_new_gamma> ("sk_maskfilter_new_gamma")).Invoke (gamma);
		#endif

		// sk_maskfilter_t* sk_maskfilter_new_shader(sk_shader_t* cshader)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_maskfilter_t sk_maskfilter_new_shader (sk_shader_t cshader);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_shader (sk_shader_t cshader);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_maskfilter_t sk_maskfilter_new_shader (sk_shader_t cshader);
		}
		private static Delegates.sk_maskfilter_new_shader sk_maskfilter_new_shader_delegate;
		internal static sk_maskfilter_t sk_maskfilter_new_shader (sk_shader_t cshader) =>
			(sk_maskfilter_new_shader_delegate ??= GetSymbol<Delegates.sk_maskfilter_new_shader> ("sk_maskfilter_new_shader")).Invoke (cshader);
		#endif

		// sk_maskfilter_t* sk_maskfilter_new_table(const uint8_t[256] table = 256)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_maskfilter_t sk_maskfilter_new_table (Byte* table);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_table (Byte* table);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_maskfilter_t sk_maskfilter_new_table (Byte* table);
		}
		private static Delegates.sk_maskfilter_new_table sk_maskfilter_new_table_delegate;
		internal static sk_maskfilter_t sk_maskfilter_new_table (Byte* table) =>
			(sk_maskfilter_new_table_delegate ??= GetSymbol<Delegates.sk_maskfilter_new_table> ("sk_maskfilter_new_table")).Invoke (table);
		#endif

		// void sk_maskfilter_ref(sk_maskfilter_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_maskfilter_ref (sk_maskfilter_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_maskfilter_ref (sk_maskfilter_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_maskfilter_ref (sk_maskfilter_t param0);
		}
		private static Delegates.sk_maskfilter_ref sk_maskfilter_ref_delegate;
		internal static void sk_maskfilter_ref (sk_maskfilter_t param0) =>
			(sk_maskfilter_ref_delegate ??= GetSymbol<Delegates.sk_maskfilter_ref> ("sk_maskfilter_ref")).Invoke (param0);
		#endif

		// void sk_maskfilter_unref(sk_maskfilter_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_maskfilter_unref (sk_maskfilter_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_maskfilter_unref (sk_maskfilter_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_maskfilter_unref (sk_maskfilter_t param0);
		}
		private static Delegates.sk_maskfilter_unref sk_maskfilter_unref_delegate;
		internal static void sk_maskfilter_unref (sk_maskfilter_t param0) =>
			(sk_maskfilter_unref_delegate ??= GetSymbol<Delegates.sk_maskfilter_unref> ("sk_maskfilter_unref")).Invoke (param0);
		#endif

		#endregion

	}
}

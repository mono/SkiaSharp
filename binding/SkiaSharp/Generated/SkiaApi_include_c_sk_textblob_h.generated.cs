using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_textblob.h

		// void sk_textblob_builder_alloc_run(sk_textblob_builder_t* builder, const sk_font_t* font, int count, float x, float y, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_textblob_builder_alloc_run (IntPtr builder, IntPtr font, Int32 count, Single x, Single y, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run (IntPtr builder, IntPtr font, Int32 count, Single x, Single y, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run (IntPtr builder, IntPtr font, Int32 count, Single x, Single y, SKRect* bounds, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run sk_textblob_builder_alloc_run_delegate;
		internal static void sk_textblob_builder_alloc_run (IntPtr builder, IntPtr font, Int32 count, Single x, Single y, SKRect* bounds, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run> ("sk_textblob_builder_alloc_run")).Invoke (builder, font, count, x, y, bounds, runbuffer);
		#endif

		// void sk_textblob_builder_alloc_run_pos(sk_textblob_builder_t* builder, const sk_font_t* font, int count, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_textblob_builder_alloc_run_pos (IntPtr builder, IntPtr font, Int32 count, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_pos (IntPtr builder, IntPtr font, Int32 count, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run_pos (IntPtr builder, IntPtr font, Int32 count, SKRect* bounds, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run_pos sk_textblob_builder_alloc_run_pos_delegate;
		internal static void sk_textblob_builder_alloc_run_pos (IntPtr builder, IntPtr font, Int32 count, SKRect* bounds, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_pos_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run_pos> ("sk_textblob_builder_alloc_run_pos")).Invoke (builder, font, count, bounds, runbuffer);
		#endif

		// void sk_textblob_builder_alloc_run_pos_h(sk_textblob_builder_t* builder, const sk_font_t* font, int count, float y, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_textblob_builder_alloc_run_pos_h (IntPtr builder, IntPtr font, Int32 count, Single y, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_pos_h (IntPtr builder, IntPtr font, Int32 count, Single y, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run_pos_h (IntPtr builder, IntPtr font, Int32 count, Single y, SKRect* bounds, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run_pos_h sk_textblob_builder_alloc_run_pos_h_delegate;
		internal static void sk_textblob_builder_alloc_run_pos_h (IntPtr builder, IntPtr font, Int32 count, Single y, SKRect* bounds, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_pos_h_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run_pos_h> ("sk_textblob_builder_alloc_run_pos_h")).Invoke (builder, font, count, y, bounds, runbuffer);
		#endif

		// void sk_textblob_builder_alloc_run_rsxform(sk_textblob_builder_t* builder, const sk_font_t* font, int count, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_textblob_builder_alloc_run_rsxform (IntPtr builder, IntPtr font, Int32 count, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_rsxform (IntPtr builder, IntPtr font, Int32 count, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run_rsxform (IntPtr builder, IntPtr font, Int32 count, SKRect* bounds, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run_rsxform sk_textblob_builder_alloc_run_rsxform_delegate;
		internal static void sk_textblob_builder_alloc_run_rsxform (IntPtr builder, IntPtr font, Int32 count, SKRect* bounds, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_rsxform_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run_rsxform> ("sk_textblob_builder_alloc_run_rsxform")).Invoke (builder, font, count, bounds, runbuffer);
		#endif

		// void sk_textblob_builder_alloc_run_text(sk_textblob_builder_t* builder, const sk_font_t* font, int count, float x, float y, int textByteCount, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_textblob_builder_alloc_run_text (IntPtr builder, IntPtr font, Int32 count, Single x, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_text (IntPtr builder, IntPtr font, Int32 count, Single x, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run_text (IntPtr builder, IntPtr font, Int32 count, Single x, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run_text sk_textblob_builder_alloc_run_text_delegate;
		internal static void sk_textblob_builder_alloc_run_text (IntPtr builder, IntPtr font, Int32 count, Single x, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_text_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run_text> ("sk_textblob_builder_alloc_run_text")).Invoke (builder, font, count, x, y, textByteCount, bounds, runbuffer);
		#endif

		// void sk_textblob_builder_alloc_run_text_pos(sk_textblob_builder_t* builder, const sk_font_t* font, int count, int textByteCount, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_textblob_builder_alloc_run_text_pos (IntPtr builder, IntPtr font, Int32 count, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_text_pos (IntPtr builder, IntPtr font, Int32 count, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run_text_pos (IntPtr builder, IntPtr font, Int32 count, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run_text_pos sk_textblob_builder_alloc_run_text_pos_delegate;
		internal static void sk_textblob_builder_alloc_run_text_pos (IntPtr builder, IntPtr font, Int32 count, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_text_pos_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run_text_pos> ("sk_textblob_builder_alloc_run_text_pos")).Invoke (builder, font, count, textByteCount, bounds, runbuffer);
		#endif

		// void sk_textblob_builder_alloc_run_text_pos_h(sk_textblob_builder_t* builder, const sk_font_t* font, int count, float y, int textByteCount, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_textblob_builder_alloc_run_text_pos_h (IntPtr builder, IntPtr font, Int32 count, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_text_pos_h (IntPtr builder, IntPtr font, Int32 count, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run_text_pos_h (IntPtr builder, IntPtr font, Int32 count, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run_text_pos_h sk_textblob_builder_alloc_run_text_pos_h_delegate;
		internal static void sk_textblob_builder_alloc_run_text_pos_h (IntPtr builder, IntPtr font, Int32 count, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_text_pos_h_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run_text_pos_h> ("sk_textblob_builder_alloc_run_text_pos_h")).Invoke (builder, font, count, y, textByteCount, bounds, runbuffer);
		#endif

		// void sk_textblob_builder_alloc_run_text_rsxform(sk_textblob_builder_t* builder, const sk_font_t* font, int count, int textByteCount, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_textblob_builder_alloc_run_text_rsxform (IntPtr builder, IntPtr font, Int32 count, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_text_rsxform (IntPtr builder, IntPtr font, Int32 count, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run_text_rsxform (IntPtr builder, IntPtr font, Int32 count, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run_text_rsxform sk_textblob_builder_alloc_run_text_rsxform_delegate;
		internal static void sk_textblob_builder_alloc_run_text_rsxform (IntPtr builder, IntPtr font, Int32 count, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_text_rsxform_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run_text_rsxform> ("sk_textblob_builder_alloc_run_text_rsxform")).Invoke (builder, font, count, textByteCount, bounds, runbuffer);
		#endif

		// void sk_textblob_builder_delete(sk_textblob_builder_t* builder)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_textblob_builder_delete (IntPtr builder);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_delete (IntPtr builder);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_delete (IntPtr builder);
		}
		private static Delegates.sk_textblob_builder_delete sk_textblob_builder_delete_delegate;
		internal static void sk_textblob_builder_delete (IntPtr builder) =>
			(sk_textblob_builder_delete_delegate ??= GetSymbol<Delegates.sk_textblob_builder_delete> ("sk_textblob_builder_delete")).Invoke (builder);
		#endif

		// sk_textblob_t* sk_textblob_builder_make(sk_textblob_builder_t* builder)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_textblob_builder_make (IntPtr builder);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_textblob_builder_make (IntPtr builder);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_textblob_builder_make (IntPtr builder);
		}
		private static Delegates.sk_textblob_builder_make sk_textblob_builder_make_delegate;
		internal static IntPtr sk_textblob_builder_make (IntPtr builder) =>
			(sk_textblob_builder_make_delegate ??= GetSymbol<Delegates.sk_textblob_builder_make> ("sk_textblob_builder_make")).Invoke (builder);
		#endif

		// sk_textblob_builder_t* sk_textblob_builder_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_textblob_builder_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_textblob_builder_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_textblob_builder_new ();
		}
		private static Delegates.sk_textblob_builder_new sk_textblob_builder_new_delegate;
		internal static IntPtr sk_textblob_builder_new () =>
			(sk_textblob_builder_new_delegate ??= GetSymbol<Delegates.sk_textblob_builder_new> ("sk_textblob_builder_new")).Invoke ();
		#endif

		// void sk_textblob_get_bounds(const sk_textblob_t* blob, sk_rect_t* bounds)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_textblob_get_bounds (IntPtr blob, SKRect* bounds);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_get_bounds (IntPtr blob, SKRect* bounds);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_get_bounds (IntPtr blob, SKRect* bounds);
		}
		private static Delegates.sk_textblob_get_bounds sk_textblob_get_bounds_delegate;
		internal static void sk_textblob_get_bounds (IntPtr blob, SKRect* bounds) =>
			(sk_textblob_get_bounds_delegate ??= GetSymbol<Delegates.sk_textblob_get_bounds> ("sk_textblob_get_bounds")).Invoke (blob, bounds);
		#endif

		// int sk_textblob_get_intercepts(const sk_textblob_t* blob, const float[2] bounds = 2, float[-1] intervals, const sk_paint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_textblob_get_intercepts (IntPtr blob, Single* bounds, Single* intervals, IntPtr paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_textblob_get_intercepts (IntPtr blob, Single* bounds, Single* intervals, IntPtr paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_textblob_get_intercepts (IntPtr blob, Single* bounds, Single* intervals, IntPtr paint);
		}
		private static Delegates.sk_textblob_get_intercepts sk_textblob_get_intercepts_delegate;
		internal static Int32 sk_textblob_get_intercepts (IntPtr blob, Single* bounds, Single* intervals, IntPtr paint) =>
			(sk_textblob_get_intercepts_delegate ??= GetSymbol<Delegates.sk_textblob_get_intercepts> ("sk_textblob_get_intercepts")).Invoke (blob, bounds, intervals, paint);
		#endif

		// uint32_t sk_textblob_get_unique_id(const sk_textblob_t* blob)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt32 sk_textblob_get_unique_id (IntPtr blob);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_textblob_get_unique_id (IntPtr blob);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_textblob_get_unique_id (IntPtr blob);
		}
		private static Delegates.sk_textblob_get_unique_id sk_textblob_get_unique_id_delegate;
		internal static UInt32 sk_textblob_get_unique_id (IntPtr blob) =>
			(sk_textblob_get_unique_id_delegate ??= GetSymbol<Delegates.sk_textblob_get_unique_id> ("sk_textblob_get_unique_id")).Invoke (blob);
		#endif

		// void sk_textblob_ref(const sk_textblob_t* blob)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_textblob_ref (IntPtr blob);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_ref (IntPtr blob);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_ref (IntPtr blob);
		}
		private static Delegates.sk_textblob_ref sk_textblob_ref_delegate;
		internal static void sk_textblob_ref (IntPtr blob) =>
			(sk_textblob_ref_delegate ??= GetSymbol<Delegates.sk_textblob_ref> ("sk_textblob_ref")).Invoke (blob);
		#endif

		// void sk_textblob_unref(const sk_textblob_t* blob)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_textblob_unref (IntPtr blob);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_unref (IntPtr blob);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_unref (IntPtr blob);
		}
		private static Delegates.sk_textblob_unref sk_textblob_unref_delegate;
		internal static void sk_textblob_unref (IntPtr blob) =>
			(sk_textblob_unref_delegate ??= GetSymbol<Delegates.sk_textblob_unref> ("sk_textblob_unref")).Invoke (blob);
		#endif

		#endregion

	}
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_drawable.h

		// size_t sk_drawable_approximate_bytes_used(sk_drawable_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_drawable_approximate_bytes_used (IntPtr param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_drawable_approximate_bytes_used (IntPtr param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_drawable_approximate_bytes_used (IntPtr param0);
		}
		private static Delegates.sk_drawable_approximate_bytes_used sk_drawable_approximate_bytes_used_delegate;
		internal static /* size_t */ IntPtr sk_drawable_approximate_bytes_used (IntPtr param0) =>
			(sk_drawable_approximate_bytes_used_delegate ??= GetSymbol<Delegates.sk_drawable_approximate_bytes_used> ("sk_drawable_approximate_bytes_used")).Invoke (param0);
		#endif

		// void sk_drawable_draw(sk_drawable_t*, sk_canvas_t*, const sk_matrix_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_drawable_draw (IntPtr param0, IntPtr param1, SKMatrix* param2);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_drawable_draw (IntPtr param0, IntPtr param1, SKMatrix* param2);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_drawable_draw (IntPtr param0, IntPtr param1, SKMatrix* param2);
		}
		private static Delegates.sk_drawable_draw sk_drawable_draw_delegate;
		internal static void sk_drawable_draw (IntPtr param0, IntPtr param1, SKMatrix* param2) =>
			(sk_drawable_draw_delegate ??= GetSymbol<Delegates.sk_drawable_draw> ("sk_drawable_draw")).Invoke (param0, param1, param2);
		#endif

		// void sk_drawable_get_bounds(sk_drawable_t*, sk_rect_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_drawable_get_bounds (IntPtr param0, SKRect* param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_drawable_get_bounds (IntPtr param0, SKRect* param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_drawable_get_bounds (IntPtr param0, SKRect* param1);
		}
		private static Delegates.sk_drawable_get_bounds sk_drawable_get_bounds_delegate;
		internal static void sk_drawable_get_bounds (IntPtr param0, SKRect* param1) =>
			(sk_drawable_get_bounds_delegate ??= GetSymbol<Delegates.sk_drawable_get_bounds> ("sk_drawable_get_bounds")).Invoke (param0, param1);
		#endif

		// uint32_t sk_drawable_get_generation_id(sk_drawable_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt32 sk_drawable_get_generation_id (IntPtr param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_drawable_get_generation_id (IntPtr param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_drawable_get_generation_id (IntPtr param0);
		}
		private static Delegates.sk_drawable_get_generation_id sk_drawable_get_generation_id_delegate;
		internal static UInt32 sk_drawable_get_generation_id (IntPtr param0) =>
			(sk_drawable_get_generation_id_delegate ??= GetSymbol<Delegates.sk_drawable_get_generation_id> ("sk_drawable_get_generation_id")).Invoke (param0);
		#endif

		// sk_picture_t* sk_drawable_new_picture_snapshot(sk_drawable_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_drawable_new_picture_snapshot (IntPtr param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_drawable_new_picture_snapshot (IntPtr param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_drawable_new_picture_snapshot (IntPtr param0);
		}
		private static Delegates.sk_drawable_new_picture_snapshot sk_drawable_new_picture_snapshot_delegate;
		internal static IntPtr sk_drawable_new_picture_snapshot (IntPtr param0) =>
			(sk_drawable_new_picture_snapshot_delegate ??= GetSymbol<Delegates.sk_drawable_new_picture_snapshot> ("sk_drawable_new_picture_snapshot")).Invoke (param0);
		#endif

		// void sk_drawable_notify_drawing_changed(sk_drawable_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_drawable_notify_drawing_changed (IntPtr param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_drawable_notify_drawing_changed (IntPtr param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_drawable_notify_drawing_changed (IntPtr param0);
		}
		private static Delegates.sk_drawable_notify_drawing_changed sk_drawable_notify_drawing_changed_delegate;
		internal static void sk_drawable_notify_drawing_changed (IntPtr param0) =>
			(sk_drawable_notify_drawing_changed_delegate ??= GetSymbol<Delegates.sk_drawable_notify_drawing_changed> ("sk_drawable_notify_drawing_changed")).Invoke (param0);
		#endif

		// void sk_drawable_unref(sk_drawable_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_drawable_unref (IntPtr param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_drawable_unref (IntPtr param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_drawable_unref (IntPtr param0);
		}
		private static Delegates.sk_drawable_unref sk_drawable_unref_delegate;
		internal static void sk_drawable_unref (IntPtr param0) =>
			(sk_drawable_unref_delegate ??= GetSymbol<Delegates.sk_drawable_unref> ("sk_drawable_unref")).Invoke (param0);
		#endif

		#endregion

	}
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_rrect.h

		// bool sk_rrect_contains(const sk_rrect_t* rrect, const sk_rect_t* rect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_rrect_contains (IntPtr rrect, SKRect* rect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_rrect_contains (IntPtr rrect, SKRect* rect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_rrect_contains (IntPtr rrect, SKRect* rect);
		}
		private static Delegates.sk_rrect_contains sk_rrect_contains_delegate;
		internal static bool sk_rrect_contains (IntPtr rrect, SKRect* rect) =>
			(sk_rrect_contains_delegate ??= GetSymbol<Delegates.sk_rrect_contains> ("sk_rrect_contains")).Invoke (rrect, rect);
		#endif

		// void sk_rrect_delete(const sk_rrect_t* rrect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_rrect_delete (IntPtr rrect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_delete (IntPtr rrect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_delete (IntPtr rrect);
		}
		private static Delegates.sk_rrect_delete sk_rrect_delete_delegate;
		internal static void sk_rrect_delete (IntPtr rrect) =>
			(sk_rrect_delete_delegate ??= GetSymbol<Delegates.sk_rrect_delete> ("sk_rrect_delete")).Invoke (rrect);
		#endif

		// float sk_rrect_get_height(const sk_rrect_t* rrect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_rrect_get_height (IntPtr rrect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_rrect_get_height (IntPtr rrect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_rrect_get_height (IntPtr rrect);
		}
		private static Delegates.sk_rrect_get_height sk_rrect_get_height_delegate;
		internal static Single sk_rrect_get_height (IntPtr rrect) =>
			(sk_rrect_get_height_delegate ??= GetSymbol<Delegates.sk_rrect_get_height> ("sk_rrect_get_height")).Invoke (rrect);
		#endif

		// void sk_rrect_get_radii(const sk_rrect_t* rrect, sk_rrect_corner_t corner, sk_vector_t* radii)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_rrect_get_radii (IntPtr rrect, SKRoundRectCorner corner, SKPoint* radii);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_get_radii (IntPtr rrect, SKRoundRectCorner corner, SKPoint* radii);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_get_radii (IntPtr rrect, SKRoundRectCorner corner, SKPoint* radii);
		}
		private static Delegates.sk_rrect_get_radii sk_rrect_get_radii_delegate;
		internal static void sk_rrect_get_radii (IntPtr rrect, SKRoundRectCorner corner, SKPoint* radii) =>
			(sk_rrect_get_radii_delegate ??= GetSymbol<Delegates.sk_rrect_get_radii> ("sk_rrect_get_radii")).Invoke (rrect, corner, radii);
		#endif

		// void sk_rrect_get_rect(const sk_rrect_t* rrect, sk_rect_t* rect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_rrect_get_rect (IntPtr rrect, SKRect* rect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_get_rect (IntPtr rrect, SKRect* rect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_get_rect (IntPtr rrect, SKRect* rect);
		}
		private static Delegates.sk_rrect_get_rect sk_rrect_get_rect_delegate;
		internal static void sk_rrect_get_rect (IntPtr rrect, SKRect* rect) =>
			(sk_rrect_get_rect_delegate ??= GetSymbol<Delegates.sk_rrect_get_rect> ("sk_rrect_get_rect")).Invoke (rrect, rect);
		#endif

		// sk_rrect_type_t sk_rrect_get_type(const sk_rrect_t* rrect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKRoundRectType sk_rrect_get_type (IntPtr rrect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKRoundRectType sk_rrect_get_type (IntPtr rrect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKRoundRectType sk_rrect_get_type (IntPtr rrect);
		}
		private static Delegates.sk_rrect_get_type sk_rrect_get_type_delegate;
		internal static SKRoundRectType sk_rrect_get_type (IntPtr rrect) =>
			(sk_rrect_get_type_delegate ??= GetSymbol<Delegates.sk_rrect_get_type> ("sk_rrect_get_type")).Invoke (rrect);
		#endif

		// float sk_rrect_get_width(const sk_rrect_t* rrect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_rrect_get_width (IntPtr rrect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_rrect_get_width (IntPtr rrect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_rrect_get_width (IntPtr rrect);
		}
		private static Delegates.sk_rrect_get_width sk_rrect_get_width_delegate;
		internal static Single sk_rrect_get_width (IntPtr rrect) =>
			(sk_rrect_get_width_delegate ??= GetSymbol<Delegates.sk_rrect_get_width> ("sk_rrect_get_width")).Invoke (rrect);
		#endif

		// void sk_rrect_inset(sk_rrect_t* rrect, float dx, float dy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_rrect_inset (IntPtr rrect, Single dx, Single dy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_inset (IntPtr rrect, Single dx, Single dy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_inset (IntPtr rrect, Single dx, Single dy);
		}
		private static Delegates.sk_rrect_inset sk_rrect_inset_delegate;
		internal static void sk_rrect_inset (IntPtr rrect, Single dx, Single dy) =>
			(sk_rrect_inset_delegate ??= GetSymbol<Delegates.sk_rrect_inset> ("sk_rrect_inset")).Invoke (rrect, dx, dy);
		#endif

		// bool sk_rrect_is_valid(const sk_rrect_t* rrect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_rrect_is_valid (IntPtr rrect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_rrect_is_valid (IntPtr rrect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_rrect_is_valid (IntPtr rrect);
		}
		private static Delegates.sk_rrect_is_valid sk_rrect_is_valid_delegate;
		internal static bool sk_rrect_is_valid (IntPtr rrect) =>
			(sk_rrect_is_valid_delegate ??= GetSymbol<Delegates.sk_rrect_is_valid> ("sk_rrect_is_valid")).Invoke (rrect);
		#endif

		// sk_rrect_t* sk_rrect_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_rrect_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_rrect_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_rrect_new ();
		}
		private static Delegates.sk_rrect_new sk_rrect_new_delegate;
		internal static IntPtr sk_rrect_new () =>
			(sk_rrect_new_delegate ??= GetSymbol<Delegates.sk_rrect_new> ("sk_rrect_new")).Invoke ();
		#endif

		// sk_rrect_t* sk_rrect_new_copy(const sk_rrect_t* rrect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_rrect_new_copy (IntPtr rrect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_rrect_new_copy (IntPtr rrect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_rrect_new_copy (IntPtr rrect);
		}
		private static Delegates.sk_rrect_new_copy sk_rrect_new_copy_delegate;
		internal static IntPtr sk_rrect_new_copy (IntPtr rrect) =>
			(sk_rrect_new_copy_delegate ??= GetSymbol<Delegates.sk_rrect_new_copy> ("sk_rrect_new_copy")).Invoke (rrect);
		#endif

		// void sk_rrect_offset(sk_rrect_t* rrect, float dx, float dy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_rrect_offset (IntPtr rrect, Single dx, Single dy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_offset (IntPtr rrect, Single dx, Single dy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_offset (IntPtr rrect, Single dx, Single dy);
		}
		private static Delegates.sk_rrect_offset sk_rrect_offset_delegate;
		internal static void sk_rrect_offset (IntPtr rrect, Single dx, Single dy) =>
			(sk_rrect_offset_delegate ??= GetSymbol<Delegates.sk_rrect_offset> ("sk_rrect_offset")).Invoke (rrect, dx, dy);
		#endif

		// void sk_rrect_outset(sk_rrect_t* rrect, float dx, float dy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_rrect_outset (IntPtr rrect, Single dx, Single dy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_outset (IntPtr rrect, Single dx, Single dy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_outset (IntPtr rrect, Single dx, Single dy);
		}
		private static Delegates.sk_rrect_outset sk_rrect_outset_delegate;
		internal static void sk_rrect_outset (IntPtr rrect, Single dx, Single dy) =>
			(sk_rrect_outset_delegate ??= GetSymbol<Delegates.sk_rrect_outset> ("sk_rrect_outset")).Invoke (rrect, dx, dy);
		#endif

		// void sk_rrect_set_empty(sk_rrect_t* rrect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_rrect_set_empty (IntPtr rrect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_empty (IntPtr rrect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_set_empty (IntPtr rrect);
		}
		private static Delegates.sk_rrect_set_empty sk_rrect_set_empty_delegate;
		internal static void sk_rrect_set_empty (IntPtr rrect) =>
			(sk_rrect_set_empty_delegate ??= GetSymbol<Delegates.sk_rrect_set_empty> ("sk_rrect_set_empty")).Invoke (rrect);
		#endif

		// void sk_rrect_set_nine_patch(sk_rrect_t* rrect, const sk_rect_t* rect, float leftRad, float topRad, float rightRad, float bottomRad)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_rrect_set_nine_patch (IntPtr rrect, SKRect* rect, Single leftRad, Single topRad, Single rightRad, Single bottomRad);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_nine_patch (IntPtr rrect, SKRect* rect, Single leftRad, Single topRad, Single rightRad, Single bottomRad);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_set_nine_patch (IntPtr rrect, SKRect* rect, Single leftRad, Single topRad, Single rightRad, Single bottomRad);
		}
		private static Delegates.sk_rrect_set_nine_patch sk_rrect_set_nine_patch_delegate;
		internal static void sk_rrect_set_nine_patch (IntPtr rrect, SKRect* rect, Single leftRad, Single topRad, Single rightRad, Single bottomRad) =>
			(sk_rrect_set_nine_patch_delegate ??= GetSymbol<Delegates.sk_rrect_set_nine_patch> ("sk_rrect_set_nine_patch")).Invoke (rrect, rect, leftRad, topRad, rightRad, bottomRad);
		#endif

		// void sk_rrect_set_oval(sk_rrect_t* rrect, const sk_rect_t* rect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_rrect_set_oval (IntPtr rrect, SKRect* rect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_oval (IntPtr rrect, SKRect* rect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_set_oval (IntPtr rrect, SKRect* rect);
		}
		private static Delegates.sk_rrect_set_oval sk_rrect_set_oval_delegate;
		internal static void sk_rrect_set_oval (IntPtr rrect, SKRect* rect) =>
			(sk_rrect_set_oval_delegate ??= GetSymbol<Delegates.sk_rrect_set_oval> ("sk_rrect_set_oval")).Invoke (rrect, rect);
		#endif

		// void sk_rrect_set_rect(sk_rrect_t* rrect, const sk_rect_t* rect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_rrect_set_rect (IntPtr rrect, SKRect* rect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_rect (IntPtr rrect, SKRect* rect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_set_rect (IntPtr rrect, SKRect* rect);
		}
		private static Delegates.sk_rrect_set_rect sk_rrect_set_rect_delegate;
		internal static void sk_rrect_set_rect (IntPtr rrect, SKRect* rect) =>
			(sk_rrect_set_rect_delegate ??= GetSymbol<Delegates.sk_rrect_set_rect> ("sk_rrect_set_rect")).Invoke (rrect, rect);
		#endif

		// void sk_rrect_set_rect_radii(sk_rrect_t* rrect, const sk_rect_t* rect, const sk_vector_t* radii)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_rrect_set_rect_radii (IntPtr rrect, SKRect* rect, SKPoint* radii);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_rect_radii (IntPtr rrect, SKRect* rect, SKPoint* radii);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_set_rect_radii (IntPtr rrect, SKRect* rect, SKPoint* radii);
		}
		private static Delegates.sk_rrect_set_rect_radii sk_rrect_set_rect_radii_delegate;
		internal static void sk_rrect_set_rect_radii (IntPtr rrect, SKRect* rect, SKPoint* radii) =>
			(sk_rrect_set_rect_radii_delegate ??= GetSymbol<Delegates.sk_rrect_set_rect_radii> ("sk_rrect_set_rect_radii")).Invoke (rrect, rect, radii);
		#endif

		// void sk_rrect_set_rect_xy(sk_rrect_t* rrect, const sk_rect_t* rect, float xRad, float yRad)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_rrect_set_rect_xy (IntPtr rrect, SKRect* rect, Single xRad, Single yRad);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_rect_xy (IntPtr rrect, SKRect* rect, Single xRad, Single yRad);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_set_rect_xy (IntPtr rrect, SKRect* rect, Single xRad, Single yRad);
		}
		private static Delegates.sk_rrect_set_rect_xy sk_rrect_set_rect_xy_delegate;
		internal static void sk_rrect_set_rect_xy (IntPtr rrect, SKRect* rect, Single xRad, Single yRad) =>
			(sk_rrect_set_rect_xy_delegate ??= GetSymbol<Delegates.sk_rrect_set_rect_xy> ("sk_rrect_set_rect_xy")).Invoke (rrect, rect, xRad, yRad);
		#endif

		// bool sk_rrect_transform(sk_rrect_t* rrect, const sk_matrix_t* matrix, sk_rrect_t* dest)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_rrect_transform (IntPtr rrect, SKMatrix* matrix, IntPtr dest);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_rrect_transform (IntPtr rrect, SKMatrix* matrix, IntPtr dest);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_rrect_transform (IntPtr rrect, SKMatrix* matrix, IntPtr dest);
		}
		private static Delegates.sk_rrect_transform sk_rrect_transform_delegate;
		internal static bool sk_rrect_transform (IntPtr rrect, SKMatrix* matrix, IntPtr dest) =>
			(sk_rrect_transform_delegate ??= GetSymbol<Delegates.sk_rrect_transform> ("sk_rrect_transform")).Invoke (rrect, matrix, dest);
		#endif

		#endregion

	}
}

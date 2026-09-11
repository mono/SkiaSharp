using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces

using SkiaSharp.SceneGraph;

#endregion

namespace SkiaSharp
{
	internal unsafe partial class SceneGraphApi
	{
		#region sksg_invalidation_controller.h

		// void sksg_invalidation_controller_begin(sksg_invalidation_controller_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sksg_invalidation_controller_begin (sksg_invalidation_controller_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sksg_invalidation_controller_begin (sksg_invalidation_controller_t instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sksg_invalidation_controller_begin (sksg_invalidation_controller_t instance);
		}
		private static Delegates.sksg_invalidation_controller_begin sksg_invalidation_controller_begin_delegate;
		internal static void sksg_invalidation_controller_begin (sksg_invalidation_controller_t instance) =>
			(sksg_invalidation_controller_begin_delegate ??= GetSymbol<Delegates.sksg_invalidation_controller_begin> ("sksg_invalidation_controller_begin")).Invoke (instance);
		#endif

		// void sksg_invalidation_controller_delete(sksg_invalidation_controller_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sksg_invalidation_controller_delete (sksg_invalidation_controller_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sksg_invalidation_controller_delete (sksg_invalidation_controller_t instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sksg_invalidation_controller_delete (sksg_invalidation_controller_t instance);
		}
		private static Delegates.sksg_invalidation_controller_delete sksg_invalidation_controller_delete_delegate;
		internal static void sksg_invalidation_controller_delete (sksg_invalidation_controller_t instance) =>
			(sksg_invalidation_controller_delete_delegate ??= GetSymbol<Delegates.sksg_invalidation_controller_delete> ("sksg_invalidation_controller_delete")).Invoke (instance);
		#endif

		// void sksg_invalidation_controller_end(sksg_invalidation_controller_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sksg_invalidation_controller_end (sksg_invalidation_controller_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sksg_invalidation_controller_end (sksg_invalidation_controller_t instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sksg_invalidation_controller_end (sksg_invalidation_controller_t instance);
		}
		private static Delegates.sksg_invalidation_controller_end sksg_invalidation_controller_end_delegate;
		internal static void sksg_invalidation_controller_end (sksg_invalidation_controller_t instance) =>
			(sksg_invalidation_controller_end_delegate ??= GetSymbol<Delegates.sksg_invalidation_controller_end> ("sksg_invalidation_controller_end")).Invoke (instance);
		#endif

		// void sksg_invalidation_controller_get_bounds(sksg_invalidation_controller_t* instance, sk_rect_t* bounds)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sksg_invalidation_controller_get_bounds (sksg_invalidation_controller_t instance, SKRect* bounds);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sksg_invalidation_controller_get_bounds (sksg_invalidation_controller_t instance, SKRect* bounds);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sksg_invalidation_controller_get_bounds (sksg_invalidation_controller_t instance, SKRect* bounds);
		}
		private static Delegates.sksg_invalidation_controller_get_bounds sksg_invalidation_controller_get_bounds_delegate;
		internal static void sksg_invalidation_controller_get_bounds (sksg_invalidation_controller_t instance, SKRect* bounds) =>
			(sksg_invalidation_controller_get_bounds_delegate ??= GetSymbol<Delegates.sksg_invalidation_controller_get_bounds> ("sksg_invalidation_controller_get_bounds")).Invoke (instance, bounds);
		#endif

		// void sksg_invalidation_controller_inval(sksg_invalidation_controller_t* instance, sk_rect_t* rect, sk_matrix_t* matrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sksg_invalidation_controller_inval (sksg_invalidation_controller_t instance, SKRect* rect, SKMatrix* matrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sksg_invalidation_controller_inval (sksg_invalidation_controller_t instance, SKRect* rect, SKMatrix* matrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sksg_invalidation_controller_inval (sksg_invalidation_controller_t instance, SKRect* rect, SKMatrix* matrix);
		}
		private static Delegates.sksg_invalidation_controller_inval sksg_invalidation_controller_inval_delegate;
		internal static void sksg_invalidation_controller_inval (sksg_invalidation_controller_t instance, SKRect* rect, SKMatrix* matrix) =>
			(sksg_invalidation_controller_inval_delegate ??= GetSymbol<Delegates.sksg_invalidation_controller_inval> ("sksg_invalidation_controller_inval")).Invoke (instance, rect, matrix);
		#endif

		// sksg_invalidation_controller_t* sksg_invalidation_controller_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sksg_invalidation_controller_t sksg_invalidation_controller_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sksg_invalidation_controller_t sksg_invalidation_controller_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sksg_invalidation_controller_t sksg_invalidation_controller_new ();
		}
		private static Delegates.sksg_invalidation_controller_new sksg_invalidation_controller_new_delegate;
		internal static sksg_invalidation_controller_t sksg_invalidation_controller_new () =>
			(sksg_invalidation_controller_new_delegate ??= GetSymbol<Delegates.sksg_invalidation_controller_new> ("sksg_invalidation_controller_new")).Invoke ();
		#endif

		// void sksg_invalidation_controller_reset(sksg_invalidation_controller_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sksg_invalidation_controller_reset (sksg_invalidation_controller_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sksg_invalidation_controller_reset (sksg_invalidation_controller_t instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sksg_invalidation_controller_reset (sksg_invalidation_controller_t instance);
		}
		private static Delegates.sksg_invalidation_controller_reset sksg_invalidation_controller_reset_delegate;
		internal static void sksg_invalidation_controller_reset (sksg_invalidation_controller_t instance) =>
			(sksg_invalidation_controller_reset_delegate ??= GetSymbol<Delegates.sksg_invalidation_controller_reset> ("sksg_invalidation_controller_reset")).Invoke (instance);
		#endif

		#endregion

	}
}

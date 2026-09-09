using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_managedstream.h

		// void sk_managedstream_destroy(sk_stream_managedstream_t* s)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_managedstream_destroy (IntPtr s);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedstream_destroy (IntPtr s);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_managedstream_destroy (IntPtr s);
		}
		private static Delegates.sk_managedstream_destroy sk_managedstream_destroy_delegate;
		internal static void sk_managedstream_destroy (IntPtr s) =>
			(sk_managedstream_destroy_delegate ??= GetSymbol<Delegates.sk_managedstream_destroy> ("sk_managedstream_destroy")).Invoke (s);
		#endif

		// sk_stream_managedstream_t* sk_managedstream_new(void* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_managedstream_new (void* context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_managedstream_new (void* context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_managedstream_new (void* context);
		}
		private static Delegates.sk_managedstream_new sk_managedstream_new_delegate;
		internal static IntPtr sk_managedstream_new (void* context) =>
			(sk_managedstream_new_delegate ??= GetSymbol<Delegates.sk_managedstream_new> ("sk_managedstream_new")).Invoke (context);
		#endif

		// void sk_managedstream_set_procs(sk_managedstream_procs_t procs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_managedstream_set_procs (SKManagedStreamDelegates procs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedstream_set_procs (SKManagedStreamDelegates procs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_managedstream_set_procs (SKManagedStreamDelegates procs);
		}
		private static Delegates.sk_managedstream_set_procs sk_managedstream_set_procs_delegate;
		internal static void sk_managedstream_set_procs (SKManagedStreamDelegates procs) =>
			(sk_managedstream_set_procs_delegate ??= GetSymbol<Delegates.sk_managedstream_set_procs> ("sk_managedstream_set_procs")).Invoke (procs);
		#endif

		// void sk_managedwstream_destroy(sk_wstream_managedstream_t* s)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_managedwstream_destroy (IntPtr s);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedwstream_destroy (IntPtr s);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_managedwstream_destroy (IntPtr s);
		}
		private static Delegates.sk_managedwstream_destroy sk_managedwstream_destroy_delegate;
		internal static void sk_managedwstream_destroy (IntPtr s) =>
			(sk_managedwstream_destroy_delegate ??= GetSymbol<Delegates.sk_managedwstream_destroy> ("sk_managedwstream_destroy")).Invoke (s);
		#endif

		// sk_wstream_managedstream_t* sk_managedwstream_new(void* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_managedwstream_new (void* context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_managedwstream_new (void* context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_managedwstream_new (void* context);
		}
		private static Delegates.sk_managedwstream_new sk_managedwstream_new_delegate;
		internal static IntPtr sk_managedwstream_new (void* context) =>
			(sk_managedwstream_new_delegate ??= GetSymbol<Delegates.sk_managedwstream_new> ("sk_managedwstream_new")).Invoke (context);
		#endif

		// void sk_managedwstream_set_procs(sk_managedwstream_procs_t procs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_managedwstream_set_procs (SKManagedWStreamDelegates procs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedwstream_set_procs (SKManagedWStreamDelegates procs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_managedwstream_set_procs (SKManagedWStreamDelegates procs);
		}
		private static Delegates.sk_managedwstream_set_procs sk_managedwstream_set_procs_delegate;
		internal static void sk_managedwstream_set_procs (SKManagedWStreamDelegates procs) =>
			(sk_managedwstream_set_procs_delegate ??= GetSymbol<Delegates.sk_managedwstream_set_procs> ("sk_managedwstream_set_procs")).Invoke (procs);
		#endif

		#endregion

	}
}

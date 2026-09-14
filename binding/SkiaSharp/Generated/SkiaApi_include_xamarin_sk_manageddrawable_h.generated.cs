using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_manageddrawable.h

		// sk_manageddrawable_t* sk_manageddrawable_new(void* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_manageddrawable_t sk_manageddrawable_new (void* context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_manageddrawable_t sk_manageddrawable_new (void* context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_manageddrawable_t sk_manageddrawable_new (void* context);
		}
		private static Delegates.sk_manageddrawable_new sk_manageddrawable_new_delegate;
		internal static sk_manageddrawable_t sk_manageddrawable_new (void* context) =>
			(sk_manageddrawable_new_delegate ??= GetSymbol<Delegates.sk_manageddrawable_new> ("sk_manageddrawable_new")).Invoke (context);
		#endif

		// void sk_manageddrawable_set_procs(sk_manageddrawable_procs_t procs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_manageddrawable_set_procs (SKManagedDrawableDelegates procs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_manageddrawable_set_procs (SKManagedDrawableDelegates procs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_manageddrawable_set_procs (SKManagedDrawableDelegates procs);
		}
		private static Delegates.sk_manageddrawable_set_procs sk_manageddrawable_set_procs_delegate;
		internal static void sk_manageddrawable_set_procs (SKManagedDrawableDelegates procs) =>
			(sk_manageddrawable_set_procs_delegate ??= GetSymbol<Delegates.sk_manageddrawable_set_procs> ("sk_manageddrawable_set_procs")).Invoke (procs);
		#endif

		// void sk_manageddrawable_unref(sk_manageddrawable_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_manageddrawable_unref (sk_manageddrawable_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_manageddrawable_unref (sk_manageddrawable_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_manageddrawable_unref (sk_manageddrawable_t param0);
		}
		private static Delegates.sk_manageddrawable_unref sk_manageddrawable_unref_delegate;
		internal static void sk_manageddrawable_unref (sk_manageddrawable_t param0) =>
			(sk_manageddrawable_unref_delegate ??= GetSymbol<Delegates.sk_manageddrawable_unref> ("sk_manageddrawable_unref")).Invoke (param0);
		#endif

		#endregion

	}
}

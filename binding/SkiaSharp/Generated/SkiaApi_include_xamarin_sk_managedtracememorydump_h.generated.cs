using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_managedtracememorydump.h

		// void sk_managedtracememorydump_delete(sk_managedtracememorydump_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_managedtracememorydump_delete (IntPtr param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedtracememorydump_delete (IntPtr param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_managedtracememorydump_delete (IntPtr param0);
		}
		private static Delegates.sk_managedtracememorydump_delete sk_managedtracememorydump_delete_delegate;
		internal static void sk_managedtracememorydump_delete (IntPtr param0) =>
			(sk_managedtracememorydump_delete_delegate ??= GetSymbol<Delegates.sk_managedtracememorydump_delete> ("sk_managedtracememorydump_delete")).Invoke (param0);
		#endif

		// sk_managedtracememorydump_t* sk_managedtracememorydump_new(bool detailed, bool dumpWrapped, void* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_managedtracememorydump_new ([MarshalAs (UnmanagedType.I1)] bool detailed, [MarshalAs (UnmanagedType.I1)] bool dumpWrapped, void* context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_managedtracememorydump_new ([MarshalAs (UnmanagedType.I1)] bool detailed, [MarshalAs (UnmanagedType.I1)] bool dumpWrapped, void* context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_managedtracememorydump_new ([MarshalAs (UnmanagedType.I1)] bool detailed, [MarshalAs (UnmanagedType.I1)] bool dumpWrapped, void* context);
		}
		private static Delegates.sk_managedtracememorydump_new sk_managedtracememorydump_new_delegate;
		internal static IntPtr sk_managedtracememorydump_new ([MarshalAs (UnmanagedType.I1)] bool detailed, [MarshalAs (UnmanagedType.I1)] bool dumpWrapped, void* context) =>
			(sk_managedtracememorydump_new_delegate ??= GetSymbol<Delegates.sk_managedtracememorydump_new> ("sk_managedtracememorydump_new")).Invoke (detailed, dumpWrapped, context);
		#endif

		// void sk_managedtracememorydump_set_procs(sk_managedtracememorydump_procs_t procs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_managedtracememorydump_set_procs (SKManagedTraceMemoryDumpDelegates procs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedtracememorydump_set_procs (SKManagedTraceMemoryDumpDelegates procs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_managedtracememorydump_set_procs (SKManagedTraceMemoryDumpDelegates procs);
		}
		private static Delegates.sk_managedtracememorydump_set_procs sk_managedtracememorydump_set_procs_delegate;
		internal static void sk_managedtracememorydump_set_procs (SKManagedTraceMemoryDumpDelegates procs) =>
			(sk_managedtracememorydump_set_procs_delegate ??= GetSymbol<Delegates.sk_managedtracememorydump_set_procs> ("sk_managedtracememorydump_set_procs")).Invoke (procs);
		#endif

		#endregion

	}
}

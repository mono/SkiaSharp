using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_linker.h

		// void sk_linker_keep_alive()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_linker_keep_alive ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_linker_keep_alive ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_linker_keep_alive ();
		}
		private static Delegates.sk_linker_keep_alive sk_linker_keep_alive_delegate;
		internal static void sk_linker_keep_alive () =>
			(sk_linker_keep_alive_delegate ??= GetSymbol<Delegates.sk_linker_keep_alive> ("sk_linker_keep_alive")).Invoke ();
		#endif

		#endregion

	}
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_blender.h

		// sk_blender_t* sk_blender_new_arithmetic(float k1, float k2, float k3, float k4, bool enforcePremul)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_blender_new_arithmetic (Single k1, Single k2, Single k3, Single k4, [MarshalAs (UnmanagedType.I1)] bool enforcePremul);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_blender_new_arithmetic (Single k1, Single k2, Single k3, Single k4, [MarshalAs (UnmanagedType.I1)] bool enforcePremul);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_blender_new_arithmetic (Single k1, Single k2, Single k3, Single k4, [MarshalAs (UnmanagedType.I1)] bool enforcePremul);
		}
		private static Delegates.sk_blender_new_arithmetic sk_blender_new_arithmetic_delegate;
		internal static IntPtr sk_blender_new_arithmetic (Single k1, Single k2, Single k3, Single k4, [MarshalAs (UnmanagedType.I1)] bool enforcePremul) =>
			(sk_blender_new_arithmetic_delegate ??= GetSymbol<Delegates.sk_blender_new_arithmetic> ("sk_blender_new_arithmetic")).Invoke (k1, k2, k3, k4, enforcePremul);
		#endif

		// sk_blender_t* sk_blender_new_mode(sk_blendmode_t mode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_blender_new_mode (SKBlendMode mode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_blender_new_mode (SKBlendMode mode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_blender_new_mode (SKBlendMode mode);
		}
		private static Delegates.sk_blender_new_mode sk_blender_new_mode_delegate;
		internal static IntPtr sk_blender_new_mode (SKBlendMode mode) =>
			(sk_blender_new_mode_delegate ??= GetSymbol<Delegates.sk_blender_new_mode> ("sk_blender_new_mode")).Invoke (mode);
		#endif

		// void sk_blender_ref(sk_blender_t* blender)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_blender_ref (IntPtr blender);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_blender_ref (IntPtr blender);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_blender_ref (IntPtr blender);
		}
		private static Delegates.sk_blender_ref sk_blender_ref_delegate;
		internal static void sk_blender_ref (IntPtr blender) =>
			(sk_blender_ref_delegate ??= GetSymbol<Delegates.sk_blender_ref> ("sk_blender_ref")).Invoke (blender);
		#endif

		// void sk_blender_unref(sk_blender_t* blender)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_blender_unref (IntPtr blender);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_blender_unref (IntPtr blender);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_blender_unref (IntPtr blender);
		}
		private static Delegates.sk_blender_unref sk_blender_unref_delegate;
		internal static void sk_blender_unref (IntPtr blender) =>
			(sk_blender_unref_delegate ??= GetSymbol<Delegates.sk_blender_unref> ("sk_blender_unref")).Invoke (blender);
		#endif

		#endregion

	}
}

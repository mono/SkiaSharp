using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_graphite_dawn.h

		// sk_graphite_context_t* sk_graphite_context_make_dawn(const sk_graphite_dawn_backend_context_init_t* init, const sk_graphite_context_options_t* opts)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_graphite_context_make_dawn (SKGraphiteDawnBackendContextInit* init, SKGraphiteContextOptions* opts);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_graphite_context_make_dawn (SKGraphiteDawnBackendContextInit* init, SKGraphiteContextOptions* opts);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_graphite_context_make_dawn (SKGraphiteDawnBackendContextInit* init, SKGraphiteContextOptions* opts);
		}
		private static Delegates.sk_graphite_context_make_dawn sk_graphite_context_make_dawn_delegate;
		internal static IntPtr sk_graphite_context_make_dawn (SKGraphiteDawnBackendContextInit* init, SKGraphiteContextOptions* opts) =>
			(sk_graphite_context_make_dawn_delegate ??= GetSymbol<Delegates.sk_graphite_context_make_dawn> ("sk_graphite_context_make_dawn")).Invoke (init, opts);
		#endif

		// sk_graphite_backend_texture_t* sk_graphite_dawn_backend_texture_new(void* wgpuTexture)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_graphite_dawn_backend_texture_new (void* wgpuTexture);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_graphite_dawn_backend_texture_new (void* wgpuTexture);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_graphite_dawn_backend_texture_new (void* wgpuTexture);
		}
		private static Delegates.sk_graphite_dawn_backend_texture_new sk_graphite_dawn_backend_texture_new_delegate;
		internal static IntPtr sk_graphite_dawn_backend_texture_new (void* wgpuTexture) =>
			(sk_graphite_dawn_backend_texture_new_delegate ??= GetSymbol<Delegates.sk_graphite_dawn_backend_texture_new> ("sk_graphite_dawn_backend_texture_new")).Invoke (wgpuTexture);
		#endif

		#endregion

	}
}

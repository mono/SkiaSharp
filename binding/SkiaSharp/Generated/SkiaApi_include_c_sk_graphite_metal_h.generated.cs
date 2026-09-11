using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_graphite_metal.h

		// sk_graphite_context_t* sk_graphite_context_make_metal(const sk_graphite_mtl_backend_context_init_t* init, const sk_graphite_context_options_t* opts)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_graphite_context_t sk_graphite_context_make_metal (SKGraphiteMtlBackendContextInit* init, SKGraphiteContextOptions* opts);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_graphite_context_t sk_graphite_context_make_metal (SKGraphiteMtlBackendContextInit* init, SKGraphiteContextOptions* opts);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_graphite_context_t sk_graphite_context_make_metal (SKGraphiteMtlBackendContextInit* init, SKGraphiteContextOptions* opts);
		}
		private static Delegates.sk_graphite_context_make_metal sk_graphite_context_make_metal_delegate;
		internal static sk_graphite_context_t sk_graphite_context_make_metal (SKGraphiteMtlBackendContextInit* init, SKGraphiteContextOptions* opts) =>
			(sk_graphite_context_make_metal_delegate ??= GetSymbol<Delegates.sk_graphite_context_make_metal> ("sk_graphite_context_make_metal")).Invoke (init, opts);
		#endif

		// sk_graphite_backend_texture_t* sk_graphite_mtl_backend_texture_new(int32_t width, int32_t height, void* mtlTexture)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_graphite_backend_texture_t sk_graphite_mtl_backend_texture_new (Int32 width, Int32 height, void* mtlTexture);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_graphite_backend_texture_t sk_graphite_mtl_backend_texture_new (Int32 width, Int32 height, void* mtlTexture);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_graphite_backend_texture_t sk_graphite_mtl_backend_texture_new (Int32 width, Int32 height, void* mtlTexture);
		}
		private static Delegates.sk_graphite_mtl_backend_texture_new sk_graphite_mtl_backend_texture_new_delegate;
		internal static sk_graphite_backend_texture_t sk_graphite_mtl_backend_texture_new (Int32 width, Int32 height, void* mtlTexture) =>
			(sk_graphite_mtl_backend_texture_new_delegate ??= GetSymbol<Delegates.sk_graphite_mtl_backend_texture_new> ("sk_graphite_mtl_backend_texture_new")).Invoke (width, height, mtlTexture);
		#endif

		#endregion

	}
}

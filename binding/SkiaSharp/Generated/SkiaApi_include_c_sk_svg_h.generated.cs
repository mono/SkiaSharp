using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_svg.h

		// sk_canvas_t* sk_svgcanvas_create_with_stream(const sk_rect_t* bounds, sk_wstream_t* stream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_svgcanvas_create_with_stream (SKRect* bounds, IntPtr stream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_svgcanvas_create_with_stream (SKRect* bounds, IntPtr stream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_svgcanvas_create_with_stream (SKRect* bounds, IntPtr stream);
		}
		private static Delegates.sk_svgcanvas_create_with_stream sk_svgcanvas_create_with_stream_delegate;
		internal static IntPtr sk_svgcanvas_create_with_stream (SKRect* bounds, IntPtr stream) =>
			(sk_svgcanvas_create_with_stream_delegate ??= GetSymbol<Delegates.sk_svgcanvas_create_with_stream> ("sk_svgcanvas_create_with_stream")).Invoke (bounds, stream);
		#endif

		#endregion

	}
}

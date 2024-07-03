#nullable disable

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	// public delegates

	public delegate void SKBitmapReleaseDelegate (IntPtr address, object context);

	public delegate void SKDataReleaseDelegate (IntPtr address, object context);

	public delegate void SKImageRasterReleaseDelegate (IntPtr pixels, object context);

	public delegate void SKImageTextureReleaseDelegate (object context);

	public delegate void SKSurfaceReleaseDelegate (IntPtr address, object context);

	public delegate IntPtr GRGlGetProcedureAddressDelegate (string name);

	public delegate IntPtr GRVkGetProcedureAddressDelegate (string name, IntPtr instance, IntPtr device);

	public delegate void SKGlyphPathDelegate (SKPath path, SKMatrix matrix);

	internal unsafe static partial class DelegateProxies
	{
		// references to the proxy implementations
#if USE_LIBRARY_IMPORT
		public static readonly delegate* unmanaged[Cdecl] <void*, void*, void> SKBitmapReleaseDelegateProxy = &SKBitmapReleaseDelegateProxyImplementation;
		public static readonly delegate* unmanaged[Cdecl] <void*, void*, void> SKDataReleaseDelegateProxy = &SKDataReleaseDelegateProxyImplementation;
		public static readonly delegate* unmanaged[Cdecl] <void*, void*, void> SKImageRasterReleaseDelegateProxy = &SKImageRasterReleaseDelegateProxyImplementation;
		public static readonly delegate* unmanaged[Cdecl] <void*, void*, void> SKImageRasterReleaseDelegateProxyForCoTaskMem = &SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem;
		public static readonly delegate* unmanaged[Cdecl] <void*, void> SKImageTextureReleaseDelegateProxy = &SKImageTextureReleaseDelegateProxyImplementation;
		public static readonly delegate* unmanaged[Cdecl] <void*, void*, void> SKSurfaceReleaseDelegateProxy = &SKSurfaceReleaseDelegateProxyImplementation;
		public static readonly delegate* unmanaged[Cdecl] <void*, void*, nint> GRGlGetProcDelegateProxy = &GRGlGetProcDelegateProxyImplementation;
		public static readonly delegate* unmanaged[Cdecl] <void*, void*, nint, nint, nint> GRVkGetProcDelegateProxy = &GRVkGetProcDelegateProxyImplementation;
		public static readonly delegate* unmanaged[Cdecl] <nint, SKMatrix*, void*, void> SKGlyphPathDelegateProxy = &SKGlyphPathDelegateProxyImplementation;
#else
		public static readonly SKBitmapReleaseProxyDelegate SKBitmapReleaseDelegateProxy = SKBitmapReleaseDelegateProxyImplementation;
		public static readonly SKDataReleaseProxyDelegate SKDataReleaseDelegateProxy = SKDataReleaseDelegateProxyImplementation;
		public static readonly SKImageRasterReleaseProxyDelegate SKImageRasterReleaseDelegateProxy = SKImageRasterReleaseDelegateProxyImplementation;
		public static readonly SKImageRasterReleaseProxyDelegate SKImageRasterReleaseDelegateProxyForCoTaskMem = SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem;
		public static readonly SKImageTextureReleaseProxyDelegate SKImageTextureReleaseDelegateProxy = SKImageTextureReleaseDelegateProxyImplementation;
		public static readonly SKSurfaceRasterReleaseProxyDelegate SKSurfaceReleaseDelegateProxy = SKSurfaceReleaseDelegateProxyImplementation;
		public static readonly GRGlGetProcProxyDelegate GRGlGetProcDelegateProxy = GRGlGetProcDelegateProxyImplementation;
		public static readonly GRVkGetProcProxyDelegate GRVkGetProcDelegateProxy = GRVkGetProcDelegateProxyImplementation;
		public static readonly SKGlyphPathProxyDelegate SKGlyphPathDelegateProxy = SKGlyphPathDelegateProxyImplementation;
#endif

		// internal proxy implementations

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
		[MonoPInvokeCallback (typeof (SKBitmapReleaseProxyDelegate))]
#endif
		private static void SKBitmapReleaseDelegateProxyImplementation (void* address, void* context)
		{
			var del = Get<SKBitmapReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
		[MonoPInvokeCallback (typeof (SKDataReleaseProxyDelegate))]
#endif
		private static void SKDataReleaseDelegateProxyImplementation (void* address, void* context)
		{
			var del = Get<SKDataReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
		[MonoPInvokeCallback (typeof (SKImageRasterReleaseProxyDelegate))]
#endif
		private static void SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem (void* pixels, void* context)
		{
			Marshal.FreeCoTaskMem ((IntPtr)pixels);
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
		[MonoPInvokeCallback (typeof (SKImageRasterReleaseProxyDelegate))]
#endif
		private static void SKImageRasterReleaseDelegateProxyImplementation (void* pixels, void* context)
		{
			var del = Get<SKImageRasterReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)pixels, null);
			} finally {
				gch.Free ();
			}
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
		[MonoPInvokeCallback (typeof (SKImageTextureReleaseProxyDelegate))]
#endif
		private static void SKImageTextureReleaseDelegateProxyImplementation (void* context)
		{
			var del = Get<SKImageTextureReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke (null);
			} finally {
				gch.Free ();
			}
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
		[MonoPInvokeCallback (typeof (SKSurfaceRasterReleaseProxyDelegate))]
#endif
		private static void SKSurfaceReleaseDelegateProxyImplementation (void* address, void* context)
		{
			var del = Get<SKSurfaceReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
		[MonoPInvokeCallback (typeof (GRGlGetProcProxyDelegate))]
#endif
		private static IntPtr GRGlGetProcDelegateProxyImplementation (void* context, void* name)
		{
			var del = Get<GRGlGetProcedureAddressDelegate> ((IntPtr)context, out _);
			return del.Invoke (Marshal.PtrToStringAnsi ((IntPtr)name));
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
		[MonoPInvokeCallback (typeof (GRVkGetProcProxyDelegate))]
#endif
		private static IntPtr GRVkGetProcDelegateProxyImplementation (void* context, void* name, IntPtr instance, IntPtr device)
		{
			var del = Get<GRVkGetProcedureAddressDelegate> ((IntPtr)context, out _);

			return del.Invoke (Marshal.PtrToStringAnsi ((IntPtr)name), instance, device);
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
		[MonoPInvokeCallback (typeof (SKGlyphPathProxyDelegate))]
#endif
		private static void SKGlyphPathDelegateProxyImplementation (IntPtr pathOrNull, SKMatrix* matrix, void* context)
		{
			var del = Get<SKGlyphPathDelegate> ((IntPtr)context, out _);
			var path = SKPath.GetObject (pathOrNull, false);
			del.Invoke (path, *matrix);
		}
	}
}

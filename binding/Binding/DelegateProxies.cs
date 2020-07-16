using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	// public delegates

	public delegate void SKBitmapReleaseDelegate (IntPtr address, object context);

	public delegate void SKDataReleaseDelegate (IntPtr address, object context);

	public delegate void SKImageRasterReleaseDelegate (IntPtr pixels, object context);

	public delegate void SKImageTextureReleaseDelegate (object context);

	public delegate void SKSurfaceReleaseDelegate (IntPtr address, object context);

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("Use GRGlGetProcedureAddressDelegate instead.")]
	public delegate IntPtr GRGlGetProcDelegate (object context, string name);

	public delegate IntPtr GRGlGetProcedureAddressDelegate (string name);

	public delegate IntPtr GRVkGetProcedureAddressDelegate (string name, IntPtr instance, IntPtr device);

	public delegate void SKGlyphPathDelegate (SKPath path, SKMatrix matrix);

	internal unsafe static partial class DelegateProxies
	{
		// references to the proxy implementations
#if USE_INTPTR_DELEGATES
		public static readonly IntPtr SKBitmapReleaseDelegateProxy;
		public static readonly IntPtr SKDataReleaseDelegateProxy;
		public static readonly IntPtr SKImageRasterReleaseDelegateProxy;
		public static readonly IntPtr SKImageRasterReleaseDelegateProxyForCoTaskMem;
		public static readonly IntPtr SKImageTextureReleaseDelegateProxy;
		public static readonly IntPtr SKSurfaceReleaseDelegateProxy;
		public static readonly IntPtr GRGlGetProcDelegateProxy;
		public static readonly IntPtr GRVkGetProcDelegateProxy;
		public static readonly IntPtr SKGlyphPathDelegateProxy;
#else
		public static readonly SKBitmapReleaseProxyDelegate SKBitmapReleaseDelegateProxy;
		public static readonly SKDataReleaseProxyDelegate SKDataReleaseDelegateProxy;
		public static readonly SKImageRasterReleaseProxyDelegate SKImageRasterReleaseDelegateProxy;
		public static readonly SKImageRasterReleaseProxyDelegate SKImageRasterReleaseDelegateProxyForCoTaskMem;
		public static readonly SKImageTextureReleaseProxyDelegate SKImageTextureReleaseDelegateProxy;
		public static readonly SKSurfaceRasterReleaseProxyDelegate SKSurfaceReleaseDelegateProxy;
		public static readonly GRGlGetProcProxyDelegate GRGlGetProcDelegateProxy;
		public static readonly GRVkGetProcProxyDelegate GRVkGetProcDelegateProxy;
		public static readonly SKGlyphPathProxyDelegate SKGlyphPathDelegateProxy;
#endif

		static DelegateProxies ()
		{
#if __WASM__ && USE_INTPTR_DELEGATES
			var funcs = SkiaApi.BindWasmMembers (typeof (DelegateProxies), new[] {
				(nameof (DelegateProxies.SKBitmapReleaseDelegateProxyImplementation), "vii"),
				(nameof (DelegateProxies.SKDataReleaseDelegateProxyImplementation), "vii"),
				(nameof (DelegateProxies.SKImageRasterReleaseDelegateProxyImplementation), "vii"),
				(nameof (DelegateProxies.SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem), "vii"),
				(nameof (DelegateProxies.SKImageTextureReleaseDelegateProxyImplementation), "vi"),
				(nameof (DelegateProxies.SKSurfaceReleaseDelegateProxyImplementation), "vii"),
				(nameof (DelegateProxies.GRGlGetProcDelegateProxyImplementation), "iii"),
				(nameof (DelegateProxies.GRVkGetProcDelegateProxyImplementation), "iiiii"),
				(nameof (DelegateProxies.SKGlyphPathDelegateProxyImplementation), "viii"),
			});

			var SKBitmapReleaseDelegateProxyImplementation = funcs[0];
			var SKDataReleaseDelegateProxyImplementation = funcs[1];
			var SKImageRasterReleaseDelegateProxyImplementation = funcs[2];
			var SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem = funcs[3];
			var SKImageTextureReleaseDelegateProxyImplementation = funcs[4];
			var SKSurfaceReleaseDelegateProxyImplementation = funcs[5];
			var GRGlGetProcDelegateProxyImplementation = funcs[6];
			var GRVkGetProcDelegateProxyImplementation = funcs[7];
			var SKGlyphPathDelegateProxyImplementation = funcs[8];
#endif

			SKBitmapReleaseDelegateProxy = SKBitmapReleaseDelegateProxyImplementation;
			SKDataReleaseDelegateProxy = SKDataReleaseDelegateProxyImplementation;
			SKImageRasterReleaseDelegateProxy = SKImageRasterReleaseDelegateProxyImplementation;
			SKImageRasterReleaseDelegateProxyForCoTaskMem = SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem;
			SKImageTextureReleaseDelegateProxy = SKImageTextureReleaseDelegateProxyImplementation;
			SKSurfaceReleaseDelegateProxy = SKSurfaceReleaseDelegateProxyImplementation;
			GRGlGetProcDelegateProxy = GRGlGetProcDelegateProxyImplementation;
			GRVkGetProcDelegateProxy = GRVkGetProcDelegateProxyImplementation;
			SKGlyphPathDelegateProxy = SKGlyphPathDelegateProxyImplementation;
		}

		// internal proxy implementations

		[MonoPInvokeCallback (typeof (SKBitmapReleaseProxyDelegate))]
		private static void SKBitmapReleaseDelegateProxyImplementation (void* address, void* context)
		{
			var del = Get<SKBitmapReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (SKDataReleaseProxyDelegate))]
		private static void SKDataReleaseDelegateProxyImplementation (void* address, void* context)
		{
			var del = Get<SKDataReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (SKImageRasterReleaseProxyDelegate))]
		private static void SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem (void* pixels, void* context)
		{
			Marshal.FreeCoTaskMem ((IntPtr)pixels);
		}

		[MonoPInvokeCallback (typeof (SKImageRasterReleaseProxyDelegate))]
		private static void SKImageRasterReleaseDelegateProxyImplementation (void* pixels, void* context)
		{
			var del = Get<SKImageRasterReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)pixels, null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (SKImageTextureReleaseProxyDelegate))]
		private static void SKImageTextureReleaseDelegateProxyImplementation (void* context)
		{
			var del = Get<SKImageTextureReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke (null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (SKSurfaceRasterReleaseProxyDelegate))]
		private static void SKSurfaceReleaseDelegateProxyImplementation (void* address, void* context)
		{
			var del = Get<SKSurfaceReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (GRGlGetProcProxyDelegate))]
		private static IntPtr GRGlGetProcDelegateProxyImplementation (void* context, void* name)
		{
			var del = Get<GRGlGetProcedureAddressDelegate> ((IntPtr)context, out _);
			return del.Invoke (Marshal.PtrToStringAnsi ((IntPtr)name));
		}

		[MonoPInvokeCallback (typeof (GRVkGetProcProxyDelegate))]
		private static IntPtr GRVkGetProcDelegateProxyImplementation (void* context, void* name, IntPtr instance, IntPtr device)
		{
			var del = Get<GRVkGetProcedureAddressDelegate> ((IntPtr)context, out _);

			return del.Invoke (Marshal.PtrToStringAnsi ((IntPtr)name), instance, device);
		}

		[MonoPInvokeCallback (typeof (SKGlyphPathProxyDelegate))]
		private static void SKGlyphPathDelegateProxyImplementation (IntPtr pathOrNull, SKMatrix* matrix, void* context)
		{
			var del = Get<SKGlyphPathDelegate> ((IntPtr)context, out _);
			var path = SKPath.GetObject (pathOrNull, false);
			del.Invoke (path, *matrix);
		}
	}
}

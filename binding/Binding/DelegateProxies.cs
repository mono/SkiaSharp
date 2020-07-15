using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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
#if __WASM__
			const string js =
				"SkiaSharp_SkiaApi.bindMembers('[SkiaSharp] SkiaSharp.DelegateProxies', {" +
				"  '" + nameof (DelegateProxies.SKBitmapReleaseDelegateProxyImplementation) + "':                  'vii'," +
				"  '" + nameof (DelegateProxies.SKDataReleaseDelegateProxyImplementation) + "':                    'vii'," +
				"  '" + nameof (DelegateProxies.SKImageRasterReleaseDelegateProxyImplementation) + "':             'vii'," +
				"  '" + nameof (DelegateProxies.SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem) + "': 'vii'," +
				"  '" + nameof (DelegateProxies.SKImageTextureReleaseDelegateProxyImplementation) + "':            'vi'," +
				"  '" + nameof (DelegateProxies.SKSurfaceReleaseDelegateProxyImplementation) + "':                 'vii'," +
				"  '" + nameof (DelegateProxies.GRGlGetProcDelegateProxyImplementation) + "':                      'iii'," +
				"  '" + nameof (DelegateProxies.GRVkGetProcDelegateProxyImplementation) + "':                      'iiiii'," +
				"  '" + nameof (DelegateProxies.SKGlyphPathDelegateProxyImplementation) + "':                      'viii'," +
				"});";
			const int expected = 9;

			var ret = WebAssembly.Runtime.InvokeJS (js);
			var funcs = ret.Split (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select (f => (IntPtr)int.Parse (f, CultureInfo.InvariantCulture))
				.ToArray ();

			if (funcs.Length != expected)
				throw new InvalidOperationException ($"Mismatch when binding 'SkiaSharp.DelegateProxies' members. Returned {funcs.Length}, expected {expected}.");

			// we can do magic with variables
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
#if __WASM__
		private static void SKBitmapReleaseDelegateProxyImplementation (IntPtr address, IntPtr context)
#else
		private static void SKBitmapReleaseDelegateProxyImplementation (void* address, void* context)
#endif
		{
			var del = Get<SKBitmapReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (SKDataReleaseProxyDelegate))]
#if __WASM__
		private static void SKDataReleaseDelegateProxyImplementation (IntPtr address, IntPtr context)
#else
		private static void SKDataReleaseDelegateProxyImplementation (void* address, void* context)
#endif
		{
			var del = Get<SKDataReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (SKImageRasterReleaseProxyDelegate))]
#if __WASM__
		private static void SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem (IntPtr pixels, IntPtr context)
#else
		private static void SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem (void* pixels, void* context)
#endif
		{
			Marshal.FreeCoTaskMem ((IntPtr)pixels);
		}

		[MonoPInvokeCallback (typeof (SKImageRasterReleaseProxyDelegate))]
#if __WASM__
		private static void SKImageRasterReleaseDelegateProxyImplementation (IntPtr pixels, IntPtr context)
#else
		private static void SKImageRasterReleaseDelegateProxyImplementation (void* pixels, void* context)
#endif
		{
			var del = Get<SKImageRasterReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)pixels, null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (SKImageTextureReleaseProxyDelegate))]
#if __WASM__
		private static void SKImageTextureReleaseDelegateProxyImplementation (IntPtr context)
#else
		private static void SKImageTextureReleaseDelegateProxyImplementation (void* context)
#endif
		{
			var del = Get<SKImageTextureReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke (null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (SKSurfaceRasterReleaseProxyDelegate))]
#if __WASM__
		private static void SKSurfaceReleaseDelegateProxyImplementation (IntPtr address, IntPtr context)
#else
		private static void SKSurfaceReleaseDelegateProxyImplementation (void* address, void* context)
#endif
		{
			var del = Get<SKSurfaceReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (GRGlGetProcProxyDelegate))]
#if __WASM__
		private static IntPtr GRGlGetProcDelegateProxyImplementation (IntPtr context, IntPtr namePtr)
#else
		private static IntPtr GRGlGetProcDelegateProxyImplementation (void* context, string name)
#endif
		{
#if __WASM__
			var name = Marshal.PtrToStringAnsi (namePtr);
#endif
			var del = Get<GRGlGetProcedureAddressDelegate> ((IntPtr)context, out _);
			return del.Invoke (name);
		}

		[MonoPInvokeCallback (typeof (GRVkGetProcProxyDelegate))]
#if __WASM__
		private static IntPtr GRVkGetProcDelegateProxyImplementation (IntPtr context, IntPtr namePtr, IntPtr instance, IntPtr device)
#else
		private static IntPtr GRVkGetProcDelegateProxyImplementation (void* context, string name, IntPtr instance, IntPtr device)
#endif
		{
#if __WASM__
			var name = Marshal.PtrToStringAnsi (namePtr);
#endif
			var del = Get<GRVkGetProcedureAddressDelegate> ((IntPtr)context, out _);

			return del.Invoke (name, instance, device);
		}

		[MonoPInvokeCallback (typeof (SKGlyphPathProxyDelegate))]
#if __WASM__
		private static void SKGlyphPathDelegateProxyImplementation (IntPtr pathOrNull, IntPtr matrix, IntPtr context)
#else
		private static void SKGlyphPathDelegateProxyImplementation (IntPtr pathOrNull, SKMatrix* matrix, void* context)
#endif
		{
			var del = Get<SKGlyphPathDelegate> ((IntPtr)context, out _);
			var path = SKPath.GetObject (pathOrNull, false);
			del.Invoke (path, *(SKMatrix*)matrix);
		}
	}
}

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	// public delegates

	public delegate void SKBitmapReleaseDelegate (IntPtr address, object context);

	public delegate void SKDataReleaseDelegate (IntPtr address, object context);

	public delegate void SKImageRasterReleaseDelegate (IntPtr pixels, object context);

	public delegate void SKImageTextureReleaseDelegate (object context);

	public delegate void SKSurfaceReleaseDelegate (IntPtr address, object context);

	public delegate IntPtr GRGlGetProcDelegate (object context, string name);

	// internal proxy delegates

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void SKBitmapReleaseDelegateProxyDelegate (IntPtr address, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void SKDataReleaseDelegateProxyDelegate (IntPtr address, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void SKImageRasterReleaseDelegateProxyDelegate (IntPtr pixels, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void SKImageTextureReleaseDelegateProxyDelegate (IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void SKSurfaceReleaseDelegateProxyDelegate (IntPtr address, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate IntPtr GRGlGetProcDelegateProxyDelegate (IntPtr context, [MarshalAs (UnmanagedType.LPStr)] string name);

	internal static partial class DelegateProxies
	{
		// references to the proxy implementations
		public static SKBitmapReleaseDelegateProxyDelegate SKBitmapReleaseDelegateProxy { get; } = SKBitmapReleaseDelegateProxyImplementation;
		public static SKDataReleaseDelegateProxyDelegate SKDataReleaseDelegateProxy { get; } = SKDataReleaseDelegateProxyImplementation;
		public static SKImageRasterReleaseDelegateProxyDelegate SKImageRasterReleaseDelegateProxy { get; } = SKImageRasterReleaseDelegateProxyImplementation;
		public static SKImageRasterReleaseDelegateProxyDelegate SKImageRasterReleaseDelegateProxyForCoTaskMem { get; } = SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem;
		public static SKImageTextureReleaseDelegateProxyDelegate SKImageTextureReleaseDelegateProxy { get; } = SKImageTextureReleaseDelegateProxyImplementation;
		public static SKSurfaceReleaseDelegateProxyDelegate SKSurfaceReleaseDelegateProxy { get; } = SKSurfaceReleaseDelegateProxyImplementation;
		public static GRGlGetProcDelegateProxyDelegate GRGlGetProcDelegateProxy { get; } = GRGlGetProcDelegateProxyImplementation;

		// internal proxy implementations

		[MonoPInvokeCallback (typeof (SKBitmapReleaseDelegateProxyDelegate))]
		private static void SKBitmapReleaseDelegateProxyImplementation (IntPtr address, IntPtr context)
		{
			var del = Get<SKBitmapReleaseDelegate> (context, out var gch);
			try {
				del.Invoke (address, null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (SKDataReleaseDelegateProxyDelegate))]
		private static void SKDataReleaseDelegateProxyImplementation (IntPtr address, IntPtr context)
		{
			var del = Get<SKDataReleaseDelegate> (context, out var gch);
			try {
				del.Invoke (address, null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (SKImageRasterReleaseDelegateProxyDelegate))]
		private static void SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem (IntPtr pixels, IntPtr context)
		{
			Marshal.FreeCoTaskMem (pixels);
		}

		[MonoPInvokeCallback (typeof (SKImageRasterReleaseDelegateProxyDelegate))]
		private static void SKImageRasterReleaseDelegateProxyImplementation (IntPtr pixels, IntPtr context)
		{
			var del = Get<SKImageRasterReleaseDelegate> (context, out var gch);
			try {
				del.Invoke (pixels, null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (SKImageTextureReleaseDelegateProxyDelegate))]
		private static void SKImageTextureReleaseDelegateProxyImplementation (IntPtr context)
		{
			var del = Get<SKImageTextureReleaseDelegate> (context, out var gch);
			try {
				del.Invoke (null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (SKSurfaceReleaseDelegateProxyDelegate))]
		private static void SKSurfaceReleaseDelegateProxyImplementation (IntPtr address, IntPtr context)
		{
			var del = Get<SKSurfaceReleaseDelegate> (context, out var gch);
			try {
				del.Invoke (address, null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (GRGlGetProcDelegateProxyDelegate))]
		private static IntPtr GRGlGetProcDelegateProxyImplementation (IntPtr context, string name)
		{
			var del = Get<GRGlGetProcDelegate> (context, out _);
			return del.Invoke (null, name);
		}
	}
}

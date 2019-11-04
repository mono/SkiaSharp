﻿using System;
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

	internal unsafe static partial class DelegateProxies
	{
		// references to the proxy implementations
		public static readonly SKBitmapReleaseProxyDelegate SKBitmapReleaseDelegateProxy = SKBitmapReleaseDelegateProxyImplementation;
		public static readonly SKDataReleaseProxyDelegate SKDataReleaseDelegateProxy = SKDataReleaseDelegateProxyImplementation;
		public static readonly SKImageRasterReleaseProxyDelegate SKImageRasterReleaseDelegateProxy = SKImageRasterReleaseDelegateProxyImplementation;
		public static readonly SKImageRasterReleaseProxyDelegate SKImageRasterReleaseDelegateProxyForCoTaskMem = SKImageRasterReleaseDelegateProxyImplementationForCoTaskMem;
		public static readonly SKImageTextureReleaseProxyDelegate SKImageTextureReleaseDelegateProxy = SKImageTextureReleaseDelegateProxyImplementation;
		public static readonly SKSurfaceRasterReleaseProxyDelegate SKSurfaceReleaseDelegateProxy = SKSurfaceReleaseDelegateProxyImplementation;
		public static readonly GRGlGetProcProxyDelegate GRGlGetProcDelegateProxy = GRGlGetProcDelegateProxyImplementation;

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
		private static IntPtr GRGlGetProcDelegateProxyImplementation (void* context, string name)
		{
			var del = Get<GRGlGetProcDelegate> ((IntPtr)context, out _);
			return del.Invoke (null, name);
		}
	}
}

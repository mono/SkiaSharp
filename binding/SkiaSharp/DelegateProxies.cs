#nullable disable
// ReSharper disable InconsistentNaming
// ReSharper disable PartialMethodParameterNameMismatch

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

	internal static unsafe partial class DelegateProxies
	{
		// internal proxy implementations

		private static partial void SKBitmapReleaseProxyImplementation (void* address, void* context)
		{
			var del = Get<SKBitmapReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKDataReleaseProxyImplementation (void* address, void* context)
		{
			var del = Get<SKDataReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKImageRasterReleaseProxyImplementation (void* pixels, void* context)
		{
			var del = Get<SKImageRasterReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)pixels, null);
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKImageTextureReleaseProxyImplementation (void* context)
		{
			var del = Get<SKImageTextureReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke (null);
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKSurfaceRasterReleaseProxyImplementation (void* address, void* context)
		{
			var del = Get<SKSurfaceReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKImageRasterReleaseProxyImplementationForCoTaskMem (void* pixels, void* context)
		{
			Marshal.FreeCoTaskMem ((IntPtr)pixels);
		}

		private static partial IntPtr GRGlGetProcProxyImplementation (void* context, void* name)
		{
			var del = Get<GRGlGetProcedureAddressDelegate> ((IntPtr)context, out _);
			return del.Invoke (Marshal.PtrToStringAnsi ((IntPtr)name));
		}

		private static partial IntPtr GRVkGetProcProxyImplementation (void* context, void* name, IntPtr instance, IntPtr device)
		{
			var del = Get<GRVkGetProcedureAddressDelegate> ((IntPtr)context, out _);

			return del.Invoke (Marshal.PtrToStringAnsi ((IntPtr)name), instance, device);
		}

		private static partial void SKGlyphPathProxyImplementation (IntPtr pathOrNull, SKMatrix* matrix, void* context)
		{
			var del = Get<SKGlyphPathDelegate> ((IntPtr)context, out _);
			var path = SKPath.GetObject (pathOrNull, false);
			del.Invoke (path, *matrix);
		}
	}
}

#nullable disable
// ReSharper disable InconsistentNaming

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

	public delegate IntPtr SKGraphiteVkGetProcedureAddressDelegate (string name, IntPtr instance, IntPtr device);

	public delegate void SKGraphiteReleaseDelegate (object context);

	public delegate void SKGlyphPathDelegate (SKPath path, SKMatrix matrix);

	internal static unsafe partial class DelegateProxies
	{
		// internal proxy implementations

		private static partial void SKBitmapReleaseProxyImplementation (void* addr, void* context)
		{
			var del = Get<SKBitmapReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)addr, null);
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKDataReleaseProxyImplementation (void* ptr, void* context)
		{
			var del = Get<SKDataReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)ptr, null);
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKImageRasterReleaseProxyImplementation (void* addr, void* context)
		{
			var del = Get<SKImageRasterReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)addr, null);
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

		private static partial void SKSurfaceRasterReleaseProxyImplementation (void* addr, void* context)
		{
			var del = Get<SKSurfaceReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)addr, null);
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKImageRasterReleaseProxyImplementationForCoTaskMem (void* addr, void* context)
		{
			Marshal.FreeCoTaskMem ((IntPtr)addr);
		}

		private static partial IntPtr GRGlGetProcProxyImplementation (void* ctx, void* name)
		{
			var del = Get<GRGlGetProcedureAddressDelegate> ((IntPtr)ctx, out _);
			return del.Invoke (Marshal.PtrToStringAnsi ((IntPtr)name));
		}

		private static partial IntPtr GRVkGetProcProxyImplementation (void* ctx, void* name, IntPtr instance, IntPtr device)
		{
			var del = Get<GRVkGetProcedureAddressDelegate> ((IntPtr)ctx, out _);

			return del.Invoke (Marshal.PtrToStringAnsi ((IntPtr)name), instance, device);
		}

		private static partial IntPtr SKGraphiteVkGetProxyImplementation (void* userData, void* name, IntPtr instance, IntPtr device)
		{
			var del = Get<SKGraphiteVkGetProcedureAddressDelegate> ((IntPtr)userData, out _);

			return del.Invoke (Marshal.PtrToStringAnsi ((IntPtr)name), instance, device);
		}

		private static partial void SKGraphiteReleaseProxyImplementation (void* releaseContext)
		{
			var del = Get<SKGraphiteReleaseDelegate> ((IntPtr)releaseContext, out var gch);
			try {
				del.Invoke (null);
			} finally {
				gch.Free ();
			}
		}

		private static partial IntPtr SKGraphiteImageProviderProxyImplementation (void* userData, IntPtr recorder, IntPtr image, int mipmapped)
		{
			// userData is the GCHandle pinned by SKGraphiteImageProvider (kept alive
			// by the managed wrapper for the Context's lifetime — the GCHandle is
			// freed in SKGraphiteImageProvider.Dispose(), NOT here).
			//
			// The wrapped instance's FindOrCreate decides whether to upload (returns
			// a Graphite-backed SKImage) or to drop (returns null). Returning a null
			// IntPtr here triggers Skia's "Couldn't convert" / draw-dropped path —
			// same as if no provider were installed.
			var del = Get<SKGraphiteImageProvider.FindOrCreateProxy> ((IntPtr)userData, out _);
			try {
				return del.Invoke (recorder, image, mipmapped != 0);
			} catch {
				// Never throw across the FFI boundary. Drop the draw on any
				// managed exception inside FindOrCreate.
				return IntPtr.Zero;
			}
		}

		private static partial void SKGraphiteAsyncReadPixelsProxyImplementation (void* callbackContext, IntPtr result)
		{
			// The captured Action<IntPtr> is the closure built by SKGraphiteContext.ReadPixels.
			// `result` is non-owning and is only valid for the duration of this invocation —
			// the caller must call sk_graphite_async_read_result_get_* before returning.
			var del = Get<Action<IntPtr>> ((IntPtr)callbackContext, out var gch);
			try {
				del.Invoke (result);
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKGlyphPathProxyImplementation (IntPtr pathOrNull, SKMatrix* matrix, void* context)
		{
			var del = Get<SKGlyphPathDelegate> ((IntPtr)context, out _);
			var path = SKPath.GetObject (pathOrNull, false);
			del.Invoke (path, *matrix);
		}
	}
}

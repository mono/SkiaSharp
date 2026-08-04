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

	public delegate void SKGraphiteReleaseDelegate ();

	public delegate void SKGraphiteShaderErrorHandlerDelegate (string shader, string errors, bool shaderWasCached);

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
				del.Invoke ();
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKGraphiteShaderErrorHandlerProxyImplementation (void* userData, void* shader, void* errors, bool shaderWasCached)
		{
			// userData is a GCHandle pinned by SKGraphiteContext for the Context's lifetime and
			// freed in DisposeNative — this callback can fire many times, so we must NOT free here.
			// Never throw across FFI: any managed exception inside the user's handler is swallowed
			// so a bad diagnostic hook can't crash Skia's shader-compile path.
			//
			// The C ABI's `const char*` args are non-null in the current Skia path, but the type
			// is nullable in principle; normalize to empty string so user handlers can always
			// assume a non-null value without adding their own guards.
			try {
				var del = Get<SKGraphiteShaderErrorHandlerDelegate> ((IntPtr)userData, out _);
				del.Invoke (
					Marshal.PtrToStringAnsi ((IntPtr)shader) ?? string.Empty,
					Marshal.PtrToStringAnsi ((IntPtr)errors) ?? string.Empty,
					shaderWasCached);
			} catch {
			}
		}

		private static partial IntPtr SKGraphiteImageProviderProxyImplementation (void* userData, IntPtr recorder, IntPtr image, bool mipmapped)
		{
			// userData is a GCHandle pinned by SKGraphiteContext.CreateRecorder; the
			// recorder keeps it alive for its own lifetime and frees it in DisposeNative.
			// Returning IntPtr.Zero drops the draw, same as if no callback were installed.
			var del = Get<SKGraphiteFindOrCreateImageProxy> ((IntPtr)userData, out _);
			try {
				return del.Invoke (recorder, image, mipmapped);
			} catch {
				// Never throw across the FFI boundary. Drop the draw on any
				// managed exception inside FindOrCreate.
				return IntPtr.Zero;
			}
		}

		private static partial void SKGlyphPathProxyImplementation (IntPtr pathOrNull, SKMatrix* matrix, void* context)
		{
			var del = Get<SKGlyphPathDelegate> ((IntPtr)context, out _);
			var path = SKPath.GetObject (pathOrNull, false);
			del.Invoke (path, *matrix);
		}

		private static partial void SKImageAsyncReadPixelsProxyImplementation (void* context, IntPtr result)
		{
			// The captured Action<IntPtr> is the closure built by SKImage/SKSurface.RequestReadPixels.
			// `result` is non-owning and only valid for the duration of this invocation (it is IntPtr.Zero
			// on failure); the closure must read all data before returning. This fires at most once, so the
			// pinning handle is freed here.
			var del = Get<Action<IntPtr>> ((IntPtr)context, out var gch);
			try {
				del.Invoke (result);
			} finally {
				gch.Free ();
			}
		}
	}
}

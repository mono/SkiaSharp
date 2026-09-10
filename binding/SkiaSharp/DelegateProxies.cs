#nullable disable
// ReSharper disable InconsistentNaming

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	// public delegates

	/// <summary>The delegate that is used when releasing the memory for a bitmap.</summary>
	/// <param name="address">The memory address of the pixels being released.</param>
	/// <param name="context">The user data that was provided when installing the pixels.</param>
	/// <remarks />
	public delegate void SKBitmapReleaseDelegate (IntPtr address, object context);

	/// <summary>The delegate that is used when a <see cref="T:SkiaSharp.SKData" /> instance is about to be released.</summary>
	/// <param name="address">The pointer to the byte buffer.</param>
	/// <param name="context">The user state passed to <see cref="M:SkiaSharp.SKData.Create(System.IntPtr,System.Int32,SkiaSharp.SKDataReleaseDelegate,System.Object)" />.</param>
	/// <remarks />
	public delegate void SKDataReleaseDelegate (IntPtr address, object context);

	/// <summary>The delegate that is used when releasing the memory for a raster-based image.</summary>
	/// <param name="pixels">The memory address of the pixels being released.</param>
	/// <param name="context">The user data that was provided when creating the image.</param>
	/// <remarks />
	public delegate void SKImageRasterReleaseDelegate (IntPtr pixels, object context);

	/// <summary>The delegate that is used when releasing the memory for a texture-based image.</summary>
	/// <param name="context">The context of the image.</param>
	/// <remarks />
	public delegate void SKImageTextureReleaseDelegate (object context);

	/// <summary>The delegate that is used when releasing the memory for a surface.</summary>
	/// <param name="address">The memory address of the pixels being released.</param>
	/// <param name="context">The user data that was provided when creating the surface.</param>
	/// <remarks />
	public delegate void SKSurfaceReleaseDelegate (IntPtr address, object context);

	/// <summary>Represents a method that retrieves the address of an OpenGL function by name.</summary>
	/// <param name="name">The name of the OpenGL function to look up.</param>
	/// <returns>A pointer to the requested OpenGL function, or <see cref="F:System.IntPtr.Zero" /> if the function is not found.</returns>
	/// <remarks />
	public delegate IntPtr GRGlGetProcedureAddressDelegate (string name);

	/// <summary>A delegate for resolving Vulkan function addresses by name.</summary>
	/// <param name="name">The name of the Vulkan function to look up.</param>
	/// <param name="instance">The Vulkan instance handle, or <see cref="F:System.IntPtr.Zero" /> for global functions.</param>
	/// <param name="device">The Vulkan device handle, or <see cref="F:System.IntPtr.Zero" /> for instance-level functions.</param>
	/// <returns>The function pointer for the requested Vulkan function, or <see cref="F:System.IntPtr.Zero" /> if not found.</returns>
	/// <remarks />
	public delegate IntPtr GRVkGetProcedureAddressDelegate (string name, IntPtr instance, IntPtr device);

	/// <param name="name">The name of the Vulkan function to resolve.</param>
	/// <param name="instance">The Vulkan instance handle to resolve the function against, or <see langword="null" /> when resolving a global or device-level function.</param>
	/// <param name="device">The Vulkan device handle to resolve the function against, or <see langword="null" /> when resolving a global or instance-level function.</param>
	/// <summary>Represents the method that resolves the address of a Vulkan function by name.</summary>
	/// <returns>A pointer to the resolved Vulkan function, or <see cref="F:System.IntPtr.Zero" /> if the function could not be found.</returns>
	/// <remarks />
	public delegate IntPtr SKGraphiteVkGetProcedureAddressDelegate (string name, IntPtr instance, IntPtr device);

	/// <summary>Represents the method that is called when Skia is finished using a wrapped Graphite backend texture and the caller may release the underlying resource.</summary>
	/// <remarks />
	public delegate void SKGraphiteReleaseDelegate ();

	/// <summary>Represents a callback method that receives the path and transformation matrix for each glyph when enumerating glyph paths.</summary>
	/// <param name="path">The path of the glyph, or <see langword="null" /> if the glyph has no path.</param>
	/// <param name="matrix">The transformation matrix to position the glyph.</param>
	/// <remarks />
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

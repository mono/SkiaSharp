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

	/// <summary>Represents the method that resolves the address of a Vulkan function by name.</summary>
	/// <param name="name">The name of the Vulkan function to resolve.</param>
	/// <param name="instance">The Vulkan instance handle to resolve the function against, or <see langword="null" /> when resolving a global or device-level function.</param>
	/// <param name="device">The Vulkan device handle to resolve the function against, or <see langword="null" /> when resolving a global or instance-level function.</param>
	/// <returns>A pointer to the resolved Vulkan function, or <see cref="F:System.IntPtr.Zero" /> if the function could not be found.</returns>
	/// <remarks />
	public delegate IntPtr SKGraphiteVkGetProcedureAddressDelegate (string name, IntPtr instance, IntPtr device);

	/// <summary>Represents the method that is called when Skia is finished using a wrapped Graphite backend texture and the caller may release the underlying resource.</summary>
	/// <remarks />
	public delegate void SKGraphiteReleaseDelegate ();

	/// <summary>Represents the method that is called when Skia detects that the Vulkan device has been lost.</summary>
	/// <param name="info">A snapshot of what Skia reported about the loss. The instance owns its data, so it may be stored beyond the lifetime of the call.</param>
	/// <remarks>Invoked on whichever thread the driver reports the loss on, which is not necessarily the thread that created the context.</remarks>
	public delegate void GRVkDeviceLostDelegate (GRVkDeviceLostInfo info);

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

		// Fixed-size buffer for the driver-supplied description on VkDeviceFaultVendorInfoEXT.
		// Kept as a compile-time constant so the walk below stays independent of Vulkan.h
		// (which SkiaSharp does not include on the managed side). Matches VK_MAX_DESCRIPTION_SIZE.
		private const int VkDeviceFaultVendorDescriptionSize = 256;

		private static partial void GRVkDeviceLostProxyImplementation (void* userData, GRVkDeviceLostInfoNative* info)
		{
			var infoPtr = info;
			// userData is a GCHandle pinned by the caller (GRVkBackendContext or
			// SKGraphiteVkBackendContext); the pin lives for the Context's lifetime.
			// This proxy can fire from a driver-owned thread whenever Skia detects
			// VK_ERROR_DEVICE_LOST, so we do NOT free the handle here. Never throw
			// across the FFI boundary — Skia's device-lost path is running on a native
			// stack; an unhandled managed exception would tear down the process.
			//
			// infoPtr is a gr_vk_device_lost_info_t* whose storage is only valid during
			// this call (Skia's std::string / std::vector backing memory). We snapshot
			// everything into a managed GRVkDeviceLostInfo so the caller may keep the
			// object past the callback's return.
			try {
				if (infoPtr == null) return;

				var native = *infoPtr;
				var description = Marshal.PtrToStringAnsi ((IntPtr)native.fDescription) ?? string.Empty;

				var addressInfos = new GRVkDeviceFaultAddressInfo[native.fAddressInfoCount];
				for (int i = 0; i < native.fAddressInfoCount; i++) {
					var a = native.fAddressInfos + i;
					addressInfos[i] = new GRVkDeviceFaultAddressInfo (
						(GRVkDeviceFaultAddressType)a->fAddressType,
						a->fReportedAddress,
						a->fAddressPrecision);
				}

				// gr_vk_device_fault_vendor_info_t carries a fixed char[256] description
				// followed by two uint64_t fields — total 272 bytes. The generator emits
				// the description slot as a pointer (fixed-size arrays are unsupported),
				// so we walk the array with byte* arithmetic instead of using the
				// generated struct's layout.
				const int vendorInfoStride = VkDeviceFaultVendorDescriptionSize + 8 + 8;
				var vendorInfos = new GRVkDeviceFaultVendorInfo[native.fVendorInfoCount];
				var vendorBase = (byte*)native.fVendorInfos;
				for (int i = 0; i < native.fVendorInfoCount; i++) {
					var record = vendorBase + i * vendorInfoStride;
					var desc = Marshal.PtrToStringAnsi ((IntPtr)record) ?? string.Empty;
					var code = *(ulong*)(record + VkDeviceFaultVendorDescriptionSize);
					var data = *(ulong*)(record + VkDeviceFaultVendorDescriptionSize + 8);
					vendorInfos[i] = new GRVkDeviceFaultVendorInfo (desc, code, data);
				}

				var binarySize = (int)native.fVendorBinaryDataSize;
				var binary = binarySize > 0 ? new byte[binarySize] : Array.Empty<byte> ();
				if (binarySize > 0)
					Marshal.Copy ((IntPtr)native.fVendorBinaryData, binary, 0, binarySize);

				var managed = new GRVkDeviceLostInfo (description, addressInfos, vendorInfos, binary);

				var del = Get<GRVkDeviceLostDelegate> ((IntPtr)userData, out _);
				del.Invoke (managed);
			} catch {
			}
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

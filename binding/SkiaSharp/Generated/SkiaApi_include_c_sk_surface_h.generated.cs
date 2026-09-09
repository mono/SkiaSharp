using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_surface.h

		// void sk_surface_async_rescale_and_read_pixels(sk_surface_t* surface, const sk_imageinfo_t* dstInfo, const sk_irect_t* srcRect, sk_image_rescale_gamma_t rescaleGamma, sk_image_rescale_mode_t rescaleMode, sk_image_async_read_pixels_proc callback, void* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_surface_async_rescale_and_read_pixels (sk_surface_t surface, SKImageInfoNative* dstInfo, SKRectI* srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, void* callback, void* context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_surface_async_rescale_and_read_pixels (sk_surface_t surface, SKImageInfoNative* dstInfo, SKRectI* srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, SKImageAsyncReadPixelsProxyDelegate callback, void* context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_surface_async_rescale_and_read_pixels (sk_surface_t surface, SKImageInfoNative* dstInfo, SKRectI* srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, SKImageAsyncReadPixelsProxyDelegate callback, void* context);
		}
		private static Delegates.sk_surface_async_rescale_and_read_pixels sk_surface_async_rescale_and_read_pixels_delegate;
		internal static void sk_surface_async_rescale_and_read_pixels (sk_surface_t surface, SKImageInfoNative* dstInfo, SKRectI* srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, SKImageAsyncReadPixelsProxyDelegate callback, void* context) =>
			(sk_surface_async_rescale_and_read_pixels_delegate ??= GetSymbol<Delegates.sk_surface_async_rescale_and_read_pixels> ("sk_surface_async_rescale_and_read_pixels")).Invoke (surface, dstInfo, srcRect, rescaleGamma, rescaleMode, callback, context);
		#endif

		// void sk_surface_draw(sk_surface_t* surface, sk_canvas_t* canvas, float x, float y, const sk_paint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_surface_draw (sk_surface_t surface, sk_canvas_t canvas, Single x, Single y, sk_paint_t paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_surface_draw (sk_surface_t surface, sk_canvas_t canvas, Single x, Single y, sk_paint_t paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_surface_draw (sk_surface_t surface, sk_canvas_t canvas, Single x, Single y, sk_paint_t paint);
		}
		private static Delegates.sk_surface_draw sk_surface_draw_delegate;
		internal static void sk_surface_draw (sk_surface_t surface, sk_canvas_t canvas, Single x, Single y, sk_paint_t paint) =>
			(sk_surface_draw_delegate ??= GetSymbol<Delegates.sk_surface_draw> ("sk_surface_draw")).Invoke (surface, canvas, x, y, paint);
		#endif

		// void sk_surface_draw_with_sampling(sk_surface_t* surface, sk_canvas_t* canvas, float x, float y, const sk_sampling_options_t* sampling, const sk_paint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_surface_draw_with_sampling (sk_surface_t surface, sk_canvas_t canvas, Single x, Single y, SKSamplingOptions* sampling, sk_paint_t paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_surface_draw_with_sampling (sk_surface_t surface, sk_canvas_t canvas, Single x, Single y, SKSamplingOptions* sampling, sk_paint_t paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_surface_draw_with_sampling (sk_surface_t surface, sk_canvas_t canvas, Single x, Single y, SKSamplingOptions* sampling, sk_paint_t paint);
		}
		private static Delegates.sk_surface_draw_with_sampling sk_surface_draw_with_sampling_delegate;
		internal static void sk_surface_draw_with_sampling (sk_surface_t surface, sk_canvas_t canvas, Single x, Single y, SKSamplingOptions* sampling, sk_paint_t paint) =>
			(sk_surface_draw_with_sampling_delegate ??= GetSymbol<Delegates.sk_surface_draw_with_sampling> ("sk_surface_draw_with_sampling")).Invoke (surface, canvas, x, y, sampling, paint);
		#endif

		// sk_canvas_t* sk_surface_get_canvas(sk_surface_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_canvas_t sk_surface_get_canvas (sk_surface_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_surface_get_canvas (sk_surface_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_canvas_t sk_surface_get_canvas (sk_surface_t param0);
		}
		private static Delegates.sk_surface_get_canvas sk_surface_get_canvas_delegate;
		internal static sk_canvas_t sk_surface_get_canvas (sk_surface_t param0) =>
			(sk_surface_get_canvas_delegate ??= GetSymbol<Delegates.sk_surface_get_canvas> ("sk_surface_get_canvas")).Invoke (param0);
		#endif

		// const sk_surfaceprops_t* sk_surface_get_props(sk_surface_t* surface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_surfaceprops_t sk_surface_get_props (sk_surface_t surface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surfaceprops_t sk_surface_get_props (sk_surface_t surface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_surfaceprops_t sk_surface_get_props (sk_surface_t surface);
		}
		private static Delegates.sk_surface_get_props sk_surface_get_props_delegate;
		internal static sk_surfaceprops_t sk_surface_get_props (sk_surface_t surface) =>
			(sk_surface_get_props_delegate ??= GetSymbol<Delegates.sk_surface_get_props> ("sk_surface_get_props")).Invoke (surface);
		#endif

		// gr_recording_context_t* sk_surface_get_recording_context(sk_surface_t* surface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial gr_recording_context_t sk_surface_get_recording_context (sk_surface_t surface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_recording_context_t sk_surface_get_recording_context (sk_surface_t surface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_recording_context_t sk_surface_get_recording_context (sk_surface_t surface);
		}
		private static Delegates.sk_surface_get_recording_context sk_surface_get_recording_context_delegate;
		internal static gr_recording_context_t sk_surface_get_recording_context (sk_surface_t surface) =>
			(sk_surface_get_recording_context_delegate ??= GetSymbol<Delegates.sk_surface_get_recording_context> ("sk_surface_get_recording_context")).Invoke (surface);
		#endif

		// sk_surface_t* sk_surface_new_backend_render_target(gr_recording_context_t* context, const gr_backendrendertarget_t* target, gr_surfaceorigin_t origin, sk_colortype_t colorType, sk_colorspace_t* colorspace, const sk_surfaceprops_t* props)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_surface_t sk_surface_new_backend_render_target (gr_recording_context_t context, gr_backendrendertarget_t target, GRSurfaceOrigin origin, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_backend_render_target (gr_recording_context_t context, gr_backendrendertarget_t target, GRSurfaceOrigin origin, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_surface_t sk_surface_new_backend_render_target (gr_recording_context_t context, gr_backendrendertarget_t target, GRSurfaceOrigin origin, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);
		}
		private static Delegates.sk_surface_new_backend_render_target sk_surface_new_backend_render_target_delegate;
		internal static sk_surface_t sk_surface_new_backend_render_target (gr_recording_context_t context, gr_backendrendertarget_t target, GRSurfaceOrigin origin, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props) =>
			(sk_surface_new_backend_render_target_delegate ??= GetSymbol<Delegates.sk_surface_new_backend_render_target> ("sk_surface_new_backend_render_target")).Invoke (context, target, origin, colorType, colorspace, props);
		#endif

		// sk_surface_t* sk_surface_new_backend_texture(gr_recording_context_t* context, const gr_backendtexture_t* texture, gr_surfaceorigin_t origin, int samples, sk_colortype_t colorType, sk_colorspace_t* colorspace, const sk_surfaceprops_t* props)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_surface_t sk_surface_new_backend_texture (gr_recording_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, Int32 samples, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_backend_texture (gr_recording_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, Int32 samples, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_surface_t sk_surface_new_backend_texture (gr_recording_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, Int32 samples, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);
		}
		private static Delegates.sk_surface_new_backend_texture sk_surface_new_backend_texture_delegate;
		internal static sk_surface_t sk_surface_new_backend_texture (gr_recording_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, Int32 samples, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props) =>
			(sk_surface_new_backend_texture_delegate ??= GetSymbol<Delegates.sk_surface_new_backend_texture> ("sk_surface_new_backend_texture")).Invoke (context, texture, origin, samples, colorType, colorspace, props);
		#endif

		// sk_image_t* sk_surface_new_image_snapshot(sk_surface_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_image_t sk_surface_new_image_snapshot (sk_surface_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_surface_new_image_snapshot (sk_surface_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_surface_new_image_snapshot (sk_surface_t param0);
		}
		private static Delegates.sk_surface_new_image_snapshot sk_surface_new_image_snapshot_delegate;
		internal static sk_image_t sk_surface_new_image_snapshot (sk_surface_t param0) =>
			(sk_surface_new_image_snapshot_delegate ??= GetSymbol<Delegates.sk_surface_new_image_snapshot> ("sk_surface_new_image_snapshot")).Invoke (param0);
		#endif

		// sk_image_t* sk_surface_new_image_snapshot_with_crop(sk_surface_t* surface, const sk_irect_t* bounds)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_image_t sk_surface_new_image_snapshot_with_crop (sk_surface_t surface, SKRectI* bounds);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_surface_new_image_snapshot_with_crop (sk_surface_t surface, SKRectI* bounds);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_surface_new_image_snapshot_with_crop (sk_surface_t surface, SKRectI* bounds);
		}
		private static Delegates.sk_surface_new_image_snapshot_with_crop sk_surface_new_image_snapshot_with_crop_delegate;
		internal static sk_image_t sk_surface_new_image_snapshot_with_crop (sk_surface_t surface, SKRectI* bounds) =>
			(sk_surface_new_image_snapshot_with_crop_delegate ??= GetSymbol<Delegates.sk_surface_new_image_snapshot_with_crop> ("sk_surface_new_image_snapshot_with_crop")).Invoke (surface, bounds);
		#endif

		// sk_surface_t* sk_surface_new_metal_layer(gr_recording_context_t* context, const void* layer, gr_surfaceorigin_t origin, int sampleCount, sk_colortype_t colorType, sk_colorspace_t* colorspace, const sk_surfaceprops_t* props, const void** drawable)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_surface_t sk_surface_new_metal_layer (gr_recording_context_t context, void* layer, GRSurfaceOrigin origin, Int32 sampleCount, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props, void** drawable);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_metal_layer (gr_recording_context_t context, void* layer, GRSurfaceOrigin origin, Int32 sampleCount, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props, void** drawable);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_surface_t sk_surface_new_metal_layer (gr_recording_context_t context, void* layer, GRSurfaceOrigin origin, Int32 sampleCount, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props, void** drawable);
		}
		private static Delegates.sk_surface_new_metal_layer sk_surface_new_metal_layer_delegate;
		internal static sk_surface_t sk_surface_new_metal_layer (gr_recording_context_t context, void* layer, GRSurfaceOrigin origin, Int32 sampleCount, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props, void** drawable) =>
			(sk_surface_new_metal_layer_delegate ??= GetSymbol<Delegates.sk_surface_new_metal_layer> ("sk_surface_new_metal_layer")).Invoke (context, layer, origin, sampleCount, colorType, colorspace, props, drawable);
		#endif

		// sk_surface_t* sk_surface_new_metal_view(gr_recording_context_t* context, const void* mtkView, gr_surfaceorigin_t origin, int sampleCount, sk_colortype_t colorType, sk_colorspace_t* colorspace, const sk_surfaceprops_t* props)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_surface_t sk_surface_new_metal_view (gr_recording_context_t context, void* mtkView, GRSurfaceOrigin origin, Int32 sampleCount, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_metal_view (gr_recording_context_t context, void* mtkView, GRSurfaceOrigin origin, Int32 sampleCount, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_surface_t sk_surface_new_metal_view (gr_recording_context_t context, void* mtkView, GRSurfaceOrigin origin, Int32 sampleCount, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);
		}
		private static Delegates.sk_surface_new_metal_view sk_surface_new_metal_view_delegate;
		internal static sk_surface_t sk_surface_new_metal_view (gr_recording_context_t context, void* mtkView, GRSurfaceOrigin origin, Int32 sampleCount, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props) =>
			(sk_surface_new_metal_view_delegate ??= GetSymbol<Delegates.sk_surface_new_metal_view> ("sk_surface_new_metal_view")).Invoke (context, mtkView, origin, sampleCount, colorType, colorspace, props);
		#endif

		// sk_surface_t* sk_surface_new_null(int width, int height)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_surface_t sk_surface_new_null (Int32 width, Int32 height);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_null (Int32 width, Int32 height);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_surface_t sk_surface_new_null (Int32 width, Int32 height);
		}
		private static Delegates.sk_surface_new_null sk_surface_new_null_delegate;
		internal static sk_surface_t sk_surface_new_null (Int32 width, Int32 height) =>
			(sk_surface_new_null_delegate ??= GetSymbol<Delegates.sk_surface_new_null> ("sk_surface_new_null")).Invoke (width, height);
		#endif

		// sk_surface_t* sk_surface_new_raster(const sk_imageinfo_t*, size_t rowBytes, const sk_surfaceprops_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_surface_t sk_surface_new_raster (SKImageInfoNative* param0, /* size_t */ IntPtr rowBytes, sk_surfaceprops_t param2);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_raster (SKImageInfoNative* param0, /* size_t */ IntPtr rowBytes, sk_surfaceprops_t param2);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_surface_t sk_surface_new_raster (SKImageInfoNative* param0, /* size_t */ IntPtr rowBytes, sk_surfaceprops_t param2);
		}
		private static Delegates.sk_surface_new_raster sk_surface_new_raster_delegate;
		internal static sk_surface_t sk_surface_new_raster (SKImageInfoNative* param0, /* size_t */ IntPtr rowBytes, sk_surfaceprops_t param2) =>
			(sk_surface_new_raster_delegate ??= GetSymbol<Delegates.sk_surface_new_raster> ("sk_surface_new_raster")).Invoke (param0, rowBytes, param2);
		#endif

		// sk_surface_t* sk_surface_new_raster_direct(const sk_imageinfo_t*, void* pixels, size_t rowBytes, const sk_surface_raster_release_proc releaseProc, void* context, const sk_surfaceprops_t* props)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_surface_t sk_surface_new_raster_direct (SKImageInfoNative* param0, void* pixels, /* size_t */ IntPtr rowBytes, void* releaseProc, void* context, sk_surfaceprops_t props);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_raster_direct (SKImageInfoNative* param0, void* pixels, /* size_t */ IntPtr rowBytes, SKSurfaceRasterReleaseProxyDelegate releaseProc, void* context, sk_surfaceprops_t props);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_surface_t sk_surface_new_raster_direct (SKImageInfoNative* param0, void* pixels, /* size_t */ IntPtr rowBytes, SKSurfaceRasterReleaseProxyDelegate releaseProc, void* context, sk_surfaceprops_t props);
		}
		private static Delegates.sk_surface_new_raster_direct sk_surface_new_raster_direct_delegate;
		internal static sk_surface_t sk_surface_new_raster_direct (SKImageInfoNative* param0, void* pixels, /* size_t */ IntPtr rowBytes, SKSurfaceRasterReleaseProxyDelegate releaseProc, void* context, sk_surfaceprops_t props) =>
			(sk_surface_new_raster_direct_delegate ??= GetSymbol<Delegates.sk_surface_new_raster_direct> ("sk_surface_new_raster_direct")).Invoke (param0, pixels, rowBytes, releaseProc, context, props);
		#endif

		// sk_surface_t* sk_surface_new_render_target(gr_recording_context_t* context, bool budgeted, const sk_imageinfo_t* cinfo, int sampleCount, gr_surfaceorigin_t origin, const sk_surfaceprops_t* props, bool shouldCreateWithMips)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_surface_t sk_surface_new_render_target (gr_recording_context_t context, [MarshalAs (UnmanagedType.I1)] bool budgeted, SKImageInfoNative* cinfo, Int32 sampleCount, GRSurfaceOrigin origin, sk_surfaceprops_t props, [MarshalAs (UnmanagedType.I1)] bool shouldCreateWithMips);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_render_target (gr_recording_context_t context, [MarshalAs (UnmanagedType.I1)] bool budgeted, SKImageInfoNative* cinfo, Int32 sampleCount, GRSurfaceOrigin origin, sk_surfaceprops_t props, [MarshalAs (UnmanagedType.I1)] bool shouldCreateWithMips);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_surface_t sk_surface_new_render_target (gr_recording_context_t context, [MarshalAs (UnmanagedType.I1)] bool budgeted, SKImageInfoNative* cinfo, Int32 sampleCount, GRSurfaceOrigin origin, sk_surfaceprops_t props, [MarshalAs (UnmanagedType.I1)] bool shouldCreateWithMips);
		}
		private static Delegates.sk_surface_new_render_target sk_surface_new_render_target_delegate;
		internal static sk_surface_t sk_surface_new_render_target (gr_recording_context_t context, [MarshalAs (UnmanagedType.I1)] bool budgeted, SKImageInfoNative* cinfo, Int32 sampleCount, GRSurfaceOrigin origin, sk_surfaceprops_t props, [MarshalAs (UnmanagedType.I1)] bool shouldCreateWithMips) =>
			(sk_surface_new_render_target_delegate ??= GetSymbol<Delegates.sk_surface_new_render_target> ("sk_surface_new_render_target")).Invoke (context, budgeted, cinfo, sampleCount, origin, props, shouldCreateWithMips);
		#endif

		// bool sk_surface_peek_pixels(sk_surface_t* surface, sk_pixmap_t* pixmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_surface_peek_pixels (sk_surface_t surface, sk_pixmap_t pixmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_surface_peek_pixels (sk_surface_t surface, sk_pixmap_t pixmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_surface_peek_pixels (sk_surface_t surface, sk_pixmap_t pixmap);
		}
		private static Delegates.sk_surface_peek_pixels sk_surface_peek_pixels_delegate;
		internal static bool sk_surface_peek_pixels (sk_surface_t surface, sk_pixmap_t pixmap) =>
			(sk_surface_peek_pixels_delegate ??= GetSymbol<Delegates.sk_surface_peek_pixels> ("sk_surface_peek_pixels")).Invoke (surface, pixmap);
		#endif

		// bool sk_surface_read_pixels(sk_surface_t* surface, sk_imageinfo_t* dstInfo, void* dstPixels, size_t dstRowBytes, int srcX, int srcY)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_surface_read_pixels (sk_surface_t surface, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_surface_read_pixels (sk_surface_t surface, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_surface_read_pixels (sk_surface_t surface, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY);
		}
		private static Delegates.sk_surface_read_pixels sk_surface_read_pixels_delegate;
		internal static bool sk_surface_read_pixels (sk_surface_t surface, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY) =>
			(sk_surface_read_pixels_delegate ??= GetSymbol<Delegates.sk_surface_read_pixels> ("sk_surface_read_pixels")).Invoke (surface, dstInfo, dstPixels, dstRowBytes, srcX, srcY);
		#endif

		// void sk_surface_unref(sk_surface_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_surface_unref (sk_surface_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_surface_unref (sk_surface_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_surface_unref (sk_surface_t param0);
		}
		private static Delegates.sk_surface_unref sk_surface_unref_delegate;
		internal static void sk_surface_unref (sk_surface_t param0) =>
			(sk_surface_unref_delegate ??= GetSymbol<Delegates.sk_surface_unref> ("sk_surface_unref")).Invoke (param0);
		#endif

		// void sk_surfaceprops_delete(sk_surfaceprops_t* props)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_surfaceprops_delete (sk_surfaceprops_t props);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_surfaceprops_delete (sk_surfaceprops_t props);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_surfaceprops_delete (sk_surfaceprops_t props);
		}
		private static Delegates.sk_surfaceprops_delete sk_surfaceprops_delete_delegate;
		internal static void sk_surfaceprops_delete (sk_surfaceprops_t props) =>
			(sk_surfaceprops_delete_delegate ??= GetSymbol<Delegates.sk_surfaceprops_delete> ("sk_surfaceprops_delete")).Invoke (props);
		#endif

		// uint32_t sk_surfaceprops_get_flags(sk_surfaceprops_t* props)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt32 sk_surfaceprops_get_flags (sk_surfaceprops_t props);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_surfaceprops_get_flags (sk_surfaceprops_t props);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_surfaceprops_get_flags (sk_surfaceprops_t props);
		}
		private static Delegates.sk_surfaceprops_get_flags sk_surfaceprops_get_flags_delegate;
		internal static UInt32 sk_surfaceprops_get_flags (sk_surfaceprops_t props) =>
			(sk_surfaceprops_get_flags_delegate ??= GetSymbol<Delegates.sk_surfaceprops_get_flags> ("sk_surfaceprops_get_flags")).Invoke (props);
		#endif

		// sk_pixelgeometry_t sk_surfaceprops_get_pixel_geometry(sk_surfaceprops_t* props)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKPixelGeometry sk_surfaceprops_get_pixel_geometry (sk_surfaceprops_t props);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPixelGeometry sk_surfaceprops_get_pixel_geometry (sk_surfaceprops_t props);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKPixelGeometry sk_surfaceprops_get_pixel_geometry (sk_surfaceprops_t props);
		}
		private static Delegates.sk_surfaceprops_get_pixel_geometry sk_surfaceprops_get_pixel_geometry_delegate;
		internal static SKPixelGeometry sk_surfaceprops_get_pixel_geometry (sk_surfaceprops_t props) =>
			(sk_surfaceprops_get_pixel_geometry_delegate ??= GetSymbol<Delegates.sk_surfaceprops_get_pixel_geometry> ("sk_surfaceprops_get_pixel_geometry")).Invoke (props);
		#endif

		// sk_surfaceprops_t* sk_surfaceprops_new(uint32_t flags, sk_pixelgeometry_t geometry)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_surfaceprops_t sk_surfaceprops_new (UInt32 flags, SKPixelGeometry geometry);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surfaceprops_t sk_surfaceprops_new (UInt32 flags, SKPixelGeometry geometry);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_surfaceprops_t sk_surfaceprops_new (UInt32 flags, SKPixelGeometry geometry);
		}
		private static Delegates.sk_surfaceprops_new sk_surfaceprops_new_delegate;
		internal static sk_surfaceprops_t sk_surfaceprops_new (UInt32 flags, SKPixelGeometry geometry) =>
			(sk_surfaceprops_new_delegate ??= GetSymbol<Delegates.sk_surfaceprops_new> ("sk_surfaceprops_new")).Invoke (flags, geometry);
		#endif

		#endregion

	}
}

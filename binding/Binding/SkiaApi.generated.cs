using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	#region Class declarations

	using gr_backendrendertarget_t = IntPtr;
	using gr_backendtexture_t = IntPtr;
	using gr_context_t = IntPtr;
	using gr_glinterface_t = IntPtr;
	using sk_3dview_t = IntPtr;
	using sk_bitmap_t = IntPtr;
	using sk_canvas_t = IntPtr;
	using sk_codec_t = IntPtr;
	using sk_colorfilter_t = IntPtr;
	using sk_colorspace_t = IntPtr;
	using sk_colortable_t = IntPtr;
	using sk_data_t = IntPtr;
	using sk_document_t = IntPtr;
	using sk_drawable_t = IntPtr;
	using sk_fontmgr_t = IntPtr;
	using sk_fontstyle_t = IntPtr;
	using sk_fontstyleset_t = IntPtr;
	using sk_image_t = IntPtr;
	using sk_imagefilter_croprect_t = IntPtr;
	using sk_imagefilter_t = IntPtr;
	using sk_manageddrawable_t = IntPtr;
	using sk_maskfilter_t = IntPtr;
	using sk_matrix44_t = IntPtr;
	using sk_nodraw_canvas_t = IntPtr;
	using sk_nvrefcnt_t = IntPtr;
	using sk_nway_canvas_t = IntPtr;
	using sk_opbuilder_t = IntPtr;
	using sk_overdraw_canvas_t = IntPtr;
	using sk_paint_t = IntPtr;
	using sk_path_effect_t = IntPtr;
	using sk_path_iterator_t = IntPtr;
	using sk_path_rawiterator_t = IntPtr;
	using sk_path_t = IntPtr;
	using sk_pathmeasure_t = IntPtr;
	using sk_picture_recorder_t = IntPtr;
	using sk_picture_t = IntPtr;
	using sk_pixelref_factory_t = IntPtr;
	using sk_pixmap_t = IntPtr;
	using sk_refcnt_t = IntPtr;
	using sk_region_cliperator_t = IntPtr;
	using sk_region_iterator_t = IntPtr;
	using sk_region_spanerator_t = IntPtr;
	using sk_region_t = IntPtr;
	using sk_rrect_t = IntPtr;
	using sk_shader_t = IntPtr;
	using sk_stream_asset_t = IntPtr;
	using sk_stream_filestream_t = IntPtr;
	using sk_stream_managedstream_t = IntPtr;
	using sk_stream_memorystream_t = IntPtr;
	using sk_stream_streamrewindable_t = IntPtr;
	using sk_stream_t = IntPtr;
	using sk_string_t = IntPtr;
	using sk_surface_t = IntPtr;
	using sk_surfaceprops_t = IntPtr;
	using sk_svgcanvas_t = IntPtr;
	using sk_textblob_builder_t = IntPtr;
	using sk_textblob_t = IntPtr;
	using sk_typeface_t = IntPtr;
	using sk_vertices_t = IntPtr;
	using sk_wstream_dynamicmemorystream_t = IntPtr;
	using sk_wstream_filestream_t = IntPtr;
	using sk_wstream_managedstream_t = IntPtr;
	using sk_wstream_t = IntPtr;
	using sk_xmlstreamwriter_t = IntPtr;
	using sk_xmlwriter_t = IntPtr;

	#endregion

	internal unsafe partial class SkiaApi
	{
		#region gr_context.h

		// void gr_backendrendertarget_delete(gr_backendrendertarget_t* rendertarget)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_backendrendertarget_delete (gr_backendrendertarget_t rendertarget);

		// gr_backend_t gr_backendrendertarget_get_backend(const gr_backendrendertarget_t* rendertarget)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GRBackend gr_backendrendertarget_get_backend (gr_backendrendertarget_t rendertarget);

		// bool gr_backendrendertarget_get_gl_framebufferinfo(const gr_backendrendertarget_t* rendertarget, gr_gl_framebufferinfo_t* glInfo)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendrendertarget_get_gl_framebufferinfo (gr_backendrendertarget_t rendertarget, GRGlFramebufferInfo* glInfo);

		// int gr_backendrendertarget_get_height(const gr_backendrendertarget_t* rendertarget)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendrendertarget_get_height (gr_backendrendertarget_t rendertarget);

		// int gr_backendrendertarget_get_samples(const gr_backendrendertarget_t* rendertarget)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendrendertarget_get_samples (gr_backendrendertarget_t rendertarget);

		// int gr_backendrendertarget_get_stencils(const gr_backendrendertarget_t* rendertarget)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendrendertarget_get_stencils (gr_backendrendertarget_t rendertarget);

		// int gr_backendrendertarget_get_width(const gr_backendrendertarget_t* rendertarget)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendrendertarget_get_width (gr_backendrendertarget_t rendertarget);

		// bool gr_backendrendertarget_is_valid(const gr_backendrendertarget_t* rendertarget)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendrendertarget_is_valid (gr_backendrendertarget_t rendertarget);

		// gr_backendrendertarget_t* gr_backendrendertarget_new_gl(int width, int height, int samples, int stencils, const gr_gl_framebufferinfo_t* glInfo)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_backendrendertarget_t gr_backendrendertarget_new_gl (Int32 width, Int32 height, Int32 samples, Int32 stencils, GRGlFramebufferInfo* glInfo);

		// void gr_backendtexture_delete(gr_backendtexture_t* texture)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_backendtexture_delete (gr_backendtexture_t texture);

		// gr_backend_t gr_backendtexture_get_backend(const gr_backendtexture_t* texture)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GRBackend gr_backendtexture_get_backend (gr_backendtexture_t texture);

		// bool gr_backendtexture_get_gl_textureinfo(const gr_backendtexture_t* texture, gr_gl_textureinfo_t* glInfo)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendtexture_get_gl_textureinfo (gr_backendtexture_t texture, GRGlTextureInfo* glInfo);

		// int gr_backendtexture_get_height(const gr_backendtexture_t* texture)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendtexture_get_height (gr_backendtexture_t texture);

		// int gr_backendtexture_get_width(const gr_backendtexture_t* texture)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendtexture_get_width (gr_backendtexture_t texture);

		// bool gr_backendtexture_has_mipmaps(const gr_backendtexture_t* texture)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendtexture_has_mipmaps (gr_backendtexture_t texture);

		// bool gr_backendtexture_is_valid(const gr_backendtexture_t* texture)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendtexture_is_valid (gr_backendtexture_t texture);

		// gr_backendtexture_t* gr_backendtexture_new_gl(int width, int height, bool mipmapped, const gr_gl_textureinfo_t* glInfo)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_backendtexture_t gr_backendtexture_new_gl (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRGlTextureInfo* glInfo);

		// void gr_context_abandon_context(gr_context_t* context)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_context_abandon_context (gr_context_t context);

		// void gr_context_flush(gr_context_t* context)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_context_flush (gr_context_t context);

		// gr_backend_t gr_context_get_backend(gr_context_t* context)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GRBackend gr_context_get_backend (gr_context_t context);

		// int gr_context_get_max_surface_sample_count_for_color_type(gr_context_t* context, sk_colortype_t colorType)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_context_get_max_surface_sample_count_for_color_type (gr_context_t context, SKColorType colorType);

		// void gr_context_get_resource_cache_limits(gr_context_t* context, int* maxResources, size_t* maxResourceBytes)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_context_get_resource_cache_limits (gr_context_t context, Int32* maxResources, /* size_t */ IntPtr* maxResourceBytes);

		// void gr_context_get_resource_cache_usage(gr_context_t* context, int* maxResources, size_t* maxResourceBytes)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_context_get_resource_cache_usage (gr_context_t context, Int32* maxResources, /* size_t */ IntPtr* maxResourceBytes);

		// gr_context_t* gr_context_make_gl(const gr_glinterface_t* glInterface)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_context_t gr_context_make_gl (gr_glinterface_t glInterface);

		// void gr_context_release_resources_and_abandon_context(gr_context_t* context)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_context_release_resources_and_abandon_context (gr_context_t context);

		// void gr_context_reset_context(gr_context_t* context, uint32_t state)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_context_reset_context (gr_context_t context, UInt32 state);

		// void gr_context_set_resource_cache_limits(gr_context_t* context, int maxResources, size_t maxResourceBytes)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_context_set_resource_cache_limits (gr_context_t context, Int32 maxResources, /* size_t */ IntPtr maxResourceBytes);

		// void gr_context_unref(gr_context_t* context)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_context_unref (gr_context_t context);

		// const gr_glinterface_t* gr_glinterface_assemble_gl_interface(void* ctx, gr_gl_get_proc get)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_glinterface_t gr_glinterface_assemble_gl_interface (void* ctx, GRGlGetProcProxyDelegate get);

		// const gr_glinterface_t* gr_glinterface_assemble_gles_interface(void* ctx, gr_gl_get_proc get)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_glinterface_t gr_glinterface_assemble_gles_interface (void* ctx, GRGlGetProcProxyDelegate get);

		// const gr_glinterface_t* gr_glinterface_assemble_interface(void* ctx, gr_gl_get_proc get)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_glinterface_t gr_glinterface_assemble_interface (void* ctx, GRGlGetProcProxyDelegate get);

		// const gr_glinterface_t* gr_glinterface_create_native_interface()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_glinterface_t gr_glinterface_create_native_interface ();

		// bool gr_glinterface_has_extension(const gr_glinterface_t* glInterface, const char* extension)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_glinterface_has_extension (gr_glinterface_t glInterface, [MarshalAs (UnmanagedType.LPStr)] String extension);

		// void gr_glinterface_unref(const gr_glinterface_t* glInterface)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_glinterface_unref (gr_glinterface_t glInterface);

		// bool gr_glinterface_validate(const gr_glinterface_t* glInterface)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_glinterface_validate (gr_glinterface_t glInterface);

		#endregion

		#region sk_bitmap.h

		// void sk_bitmap_destructor(sk_bitmap_t* cbitmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_destructor (sk_bitmap_t cbitmap);

		// void sk_bitmap_erase(sk_bitmap_t* cbitmap, sk_color_t color)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_erase (sk_bitmap_t cbitmap, UInt32 color);

		// void sk_bitmap_erase_rect(sk_bitmap_t* cbitmap, sk_color_t color, sk_irect_t* rect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_erase_rect (sk_bitmap_t cbitmap, UInt32 color, SKRectI* rect);

		// bool sk_bitmap_extract_alpha(sk_bitmap_t* cbitmap, sk_bitmap_t* dst, const sk_paint_t* paint, sk_ipoint_t* offset)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_extract_alpha (sk_bitmap_t cbitmap, sk_bitmap_t dst, sk_paint_t paint, SKPointI* offset);

		// bool sk_bitmap_extract_subset(sk_bitmap_t* cbitmap, sk_bitmap_t* dst, sk_irect_t* subset)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_extract_subset (sk_bitmap_t cbitmap, sk_bitmap_t dst, SKRectI* subset);

		// void* sk_bitmap_get_addr(sk_bitmap_t* cbitmap, int x, int y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_bitmap_get_addr (sk_bitmap_t cbitmap, Int32 x, Int32 y);

		// uint16_t sk_bitmap_get_addr_16(sk_bitmap_t* cbitmap, int x, int y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt16 sk_bitmap_get_addr_16 (sk_bitmap_t cbitmap, Int32 x, Int32 y);

		// uint32_t sk_bitmap_get_addr_32(sk_bitmap_t* cbitmap, int x, int y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_bitmap_get_addr_32 (sk_bitmap_t cbitmap, Int32 x, Int32 y);

		// uint8_t sk_bitmap_get_addr_8(sk_bitmap_t* cbitmap, int x, int y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte sk_bitmap_get_addr_8 (sk_bitmap_t cbitmap, Int32 x, Int32 y);

		// size_t sk_bitmap_get_byte_count(sk_bitmap_t* cbitmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_bitmap_get_byte_count (sk_bitmap_t cbitmap);

		// void sk_bitmap_get_info(sk_bitmap_t* cbitmap, sk_imageinfo_t* info)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_get_info (sk_bitmap_t cbitmap, SKImageInfoNative* info);

		// sk_color_t sk_bitmap_get_pixel_color(sk_bitmap_t* cbitmap, int x, int y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_bitmap_get_pixel_color (sk_bitmap_t cbitmap, Int32 x, Int32 y);

		// void sk_bitmap_get_pixel_colors(sk_bitmap_t* cbitmap, sk_color_t* colors)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_get_pixel_colors (sk_bitmap_t cbitmap, UInt32* colors);

		// void* sk_bitmap_get_pixels(sk_bitmap_t* cbitmap, size_t* length)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_bitmap_get_pixels (sk_bitmap_t cbitmap, /* size_t */ IntPtr* length);

		// size_t sk_bitmap_get_row_bytes(sk_bitmap_t* cbitmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_bitmap_get_row_bytes (sk_bitmap_t cbitmap);

		// bool sk_bitmap_install_mask_pixels(sk_bitmap_t* cbitmap, const sk_mask_t* cmask)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_install_mask_pixels (sk_bitmap_t cbitmap, SKMask* cmask);

		// bool sk_bitmap_install_pixels(sk_bitmap_t* cbitmap, const sk_imageinfo_t* cinfo, void* pixels, size_t rowBytes, const sk_bitmap_release_proc releaseProc, void* context)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_install_pixels (sk_bitmap_t cbitmap, SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes, SKBitmapReleaseProxyDelegate releaseProc, void* context);

		// bool sk_bitmap_install_pixels_with_pixmap(sk_bitmap_t* cbitmap, const sk_pixmap_t* cpixmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_install_pixels_with_pixmap (sk_bitmap_t cbitmap, sk_pixmap_t cpixmap);

		// bool sk_bitmap_is_immutable(sk_bitmap_t* cbitmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_is_immutable (sk_bitmap_t cbitmap);

		// bool sk_bitmap_is_null(sk_bitmap_t* cbitmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_is_null (sk_bitmap_t cbitmap);

		// bool sk_bitmap_is_volatile(sk_bitmap_t* cbitmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_is_volatile (sk_bitmap_t cbitmap);

		// sk_bitmap_t* sk_bitmap_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_bitmap_t sk_bitmap_new ();

		// void sk_bitmap_notify_pixels_changed(sk_bitmap_t* cbitmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_notify_pixels_changed (sk_bitmap_t cbitmap);

		// bool sk_bitmap_peek_pixels(sk_bitmap_t* cbitmap, sk_pixmap_t* cpixmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_peek_pixels (sk_bitmap_t cbitmap, sk_pixmap_t cpixmap);

		// bool sk_bitmap_ready_to_draw(sk_bitmap_t* cbitmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_ready_to_draw (sk_bitmap_t cbitmap);

		// void sk_bitmap_reset(sk_bitmap_t* cbitmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_reset (sk_bitmap_t cbitmap);

		// void sk_bitmap_set_immutable(sk_bitmap_t* cbitmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_set_immutable (sk_bitmap_t cbitmap);

		// void sk_bitmap_set_pixel_color(sk_bitmap_t* cbitmap, int x, int y, sk_color_t color)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_set_pixel_color (sk_bitmap_t cbitmap, Int32 x, Int32 y, UInt32 color);

		// void sk_bitmap_set_pixel_colors(sk_bitmap_t* cbitmap, const sk_color_t* colors)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_set_pixel_colors (sk_bitmap_t cbitmap, UInt32* colors);

		// void sk_bitmap_set_pixels(sk_bitmap_t* cbitmap, void* pixels)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_set_pixels (sk_bitmap_t cbitmap, void* pixels);

		// void sk_bitmap_set_volatile(sk_bitmap_t* cbitmap, bool value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_set_volatile (sk_bitmap_t cbitmap, [MarshalAs (UnmanagedType.I1)] bool value);

		// void sk_bitmap_swap(sk_bitmap_t* cbitmap, sk_bitmap_t* cother)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_swap (sk_bitmap_t cbitmap, sk_bitmap_t cother);

		// bool sk_bitmap_try_alloc_pixels(sk_bitmap_t* cbitmap, const sk_imageinfo_t* requestedInfo, size_t rowBytes)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_try_alloc_pixels (sk_bitmap_t cbitmap, SKImageInfoNative* requestedInfo, /* size_t */ IntPtr rowBytes);

		// bool sk_bitmap_try_alloc_pixels_with_flags(sk_bitmap_t* cbitmap, const sk_imageinfo_t* requestedInfo, uint32_t flags)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_try_alloc_pixels_with_flags (sk_bitmap_t cbitmap, SKImageInfoNative* requestedInfo, UInt32 flags);

		#endregion

		#region sk_canvas.h

		// void sk_canvas_clear(sk_canvas_t*, sk_color_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clear (sk_canvas_t param0, UInt32 param1);

		// void sk_canvas_clip_path_with_operation(sk_canvas_t* t, const sk_path_t* crect, sk_clipop_t op, bool doAA)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clip_path_with_operation (sk_canvas_t t, sk_path_t crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);

		// void sk_canvas_clip_rect_with_operation(sk_canvas_t* t, const sk_rect_t* crect, sk_clipop_t op, bool doAA)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clip_rect_with_operation (sk_canvas_t t, SKRect* crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);

		// void sk_canvas_clip_region(sk_canvas_t* canvas, const sk_region_t* region, sk_clipop_t op)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clip_region (sk_canvas_t canvas, sk_region_t region, SKClipOperation op);

		// void sk_canvas_clip_rrect_with_operation(sk_canvas_t* t, const sk_rrect_t* crect, sk_clipop_t op, bool doAA)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clip_rrect_with_operation (sk_canvas_t t, sk_rrect_t crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);

		// void sk_canvas_concat(sk_canvas_t*, const sk_matrix_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_concat (sk_canvas_t param0, SKMatrix* param1);

		// void sk_canvas_destroy(sk_canvas_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_destroy (sk_canvas_t param0);

		// void sk_canvas_discard(sk_canvas_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_discard (sk_canvas_t param0);

		// void sk_canvas_draw_annotation(sk_canvas_t* t, const sk_rect_t* rect, const char* key, sk_data_t* value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_annotation (sk_canvas_t t, SKRect* rect, /* char */ void* key, sk_data_t value);

		// void sk_canvas_draw_arc(sk_canvas_t* ccanvas, const sk_rect_t* oval, float startAngle, float sweepAngle, bool useCenter, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_arc (sk_canvas_t ccanvas, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool useCenter, sk_paint_t paint);

		// void sk_canvas_draw_atlas(sk_canvas_t* ccanvas, const sk_image_t* atlas, const sk_rsxform_t* xform, const sk_rect_t* tex, const sk_color_t* colors, int count, sk_blendmode_t mode, const sk_rect_t* cullRect, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_atlas (sk_canvas_t ccanvas, sk_image_t atlas, SKRotationScaleMatrix* xform, SKRect* tex, UInt32* colors, Int32 count, SKBlendMode mode, SKRect* cullRect, sk_paint_t paint);

		// void sk_canvas_draw_bitmap(sk_canvas_t* ccanvas, const sk_bitmap_t* bitmap, float left, float top, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_bitmap (sk_canvas_t ccanvas, sk_bitmap_t bitmap, Single left, Single top, sk_paint_t paint);

		// void sk_canvas_draw_bitmap_lattice(sk_canvas_t* t, const sk_bitmap_t* bitmap, const sk_lattice_t* lattice, const sk_rect_t* dst, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_bitmap_lattice (sk_canvas_t t, sk_bitmap_t bitmap, SKLatticeInternal* lattice, SKRect* dst, sk_paint_t paint);

		// void sk_canvas_draw_bitmap_nine(sk_canvas_t* t, const sk_bitmap_t* bitmap, const sk_irect_t* center, const sk_rect_t* dst, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_bitmap_nine (sk_canvas_t t, sk_bitmap_t bitmap, SKRectI* center, SKRect* dst, sk_paint_t paint);

		// void sk_canvas_draw_bitmap_rect(sk_canvas_t* ccanvas, const sk_bitmap_t* bitmap, const sk_rect_t* src, const sk_rect_t* dst, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_bitmap_rect (sk_canvas_t ccanvas, sk_bitmap_t bitmap, SKRect* src, SKRect* dst, sk_paint_t paint);

		// void sk_canvas_draw_circle(sk_canvas_t*, float cx, float cy, float rad, const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_circle (sk_canvas_t param0, Single cx, Single cy, Single rad, sk_paint_t param4);

		// void sk_canvas_draw_color(sk_canvas_t* ccanvas, sk_color_t color, sk_blendmode_t mode)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_color (sk_canvas_t ccanvas, UInt32 color, SKBlendMode mode);

		// void sk_canvas_draw_drawable(sk_canvas_t*, sk_drawable_t*, const sk_matrix_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_drawable (sk_canvas_t param0, sk_drawable_t param1, SKMatrix* param2);

		// void sk_canvas_draw_drrect(sk_canvas_t* ccanvas, const sk_rrect_t* outer, const sk_rrect_t* inner, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_drrect (sk_canvas_t ccanvas, sk_rrect_t outer, sk_rrect_t inner, sk_paint_t paint);

		// void sk_canvas_draw_image(sk_canvas_t*, const sk_image_t*, float x, float y, const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_image (sk_canvas_t param0, sk_image_t param1, Single x, Single y, sk_paint_t param4);

		// void sk_canvas_draw_image_lattice(sk_canvas_t* t, const sk_image_t* image, const sk_lattice_t* lattice, const sk_rect_t* dst, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_image_lattice (sk_canvas_t t, sk_image_t image, SKLatticeInternal* lattice, SKRect* dst, sk_paint_t paint);

		// void sk_canvas_draw_image_nine(sk_canvas_t* t, const sk_image_t* image, const sk_irect_t* center, const sk_rect_t* dst, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_image_nine (sk_canvas_t t, sk_image_t image, SKRectI* center, SKRect* dst, sk_paint_t paint);

		// void sk_canvas_draw_image_rect(sk_canvas_t*, const sk_image_t*, const sk_rect_t* src, const sk_rect_t* dst, const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_image_rect (sk_canvas_t param0, sk_image_t param1, SKRect* src, SKRect* dst, sk_paint_t param4);

		// void sk_canvas_draw_line(sk_canvas_t* ccanvas, float x0, float y0, float x1, float y1, sk_paint_t* cpaint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_line (sk_canvas_t ccanvas, Single x0, Single y0, Single x1, Single y1, sk_paint_t cpaint);

		// void sk_canvas_draw_link_destination_annotation(sk_canvas_t* t, const sk_rect_t* rect, sk_data_t* value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_link_destination_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value);

		// void sk_canvas_draw_named_destination_annotation(sk_canvas_t* t, const sk_point_t* point, sk_data_t* value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_named_destination_annotation (sk_canvas_t t, SKPoint* point, sk_data_t value);

		// void sk_canvas_draw_oval(sk_canvas_t*, const sk_rect_t*, const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_oval (sk_canvas_t param0, SKRect* param1, sk_paint_t param2);

		// void sk_canvas_draw_paint(sk_canvas_t*, const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_paint (sk_canvas_t param0, sk_paint_t param1);

		// void sk_canvas_draw_patch(sk_canvas_t* ccanvas, const sk_point_t* cubics, const sk_color_t* colors, const sk_point_t* texCoords, sk_blendmode_t mode, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_patch (sk_canvas_t ccanvas, SKPoint* cubics, UInt32* colors, SKPoint* texCoords, SKBlendMode mode, sk_paint_t paint);

		// void sk_canvas_draw_path(sk_canvas_t*, const sk_path_t*, const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_path (sk_canvas_t param0, sk_path_t param1, sk_paint_t param2);

		// void sk_canvas_draw_picture(sk_canvas_t*, const sk_picture_t*, const sk_matrix_t*, const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_picture (sk_canvas_t param0, sk_picture_t param1, SKMatrix* param2, sk_paint_t param3);

		// void sk_canvas_draw_point(sk_canvas_t*, float, float, const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_point (sk_canvas_t param0, Single param1, Single param2, sk_paint_t param3);

		// void sk_canvas_draw_points(sk_canvas_t*, sk_point_mode_t, size_t, const sk_point_t[-1], const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_points (sk_canvas_t param0, SKPointMode param1, /* size_t */ IntPtr param2, SKPoint* param3, sk_paint_t param4);

		// void sk_canvas_draw_pos_text(sk_canvas_t*, const char* text, size_t byteLength, const sk_point_t[-1], const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_pos_text (sk_canvas_t param0, /* char */ void* text, /* size_t */ IntPtr byteLength, SKPoint* param3, sk_paint_t paint);

		// void sk_canvas_draw_rect(sk_canvas_t*, const sk_rect_t*, const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_rect (sk_canvas_t param0, SKRect* param1, sk_paint_t param2);

		// void sk_canvas_draw_region(sk_canvas_t*, const sk_region_t*, const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_region (sk_canvas_t param0, sk_region_t param1, sk_paint_t param2);

		// void sk_canvas_draw_round_rect(sk_canvas_t*, const sk_rect_t*, float rx, float ry, const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_round_rect (sk_canvas_t param0, SKRect* param1, Single rx, Single ry, sk_paint_t param4);

		// void sk_canvas_draw_rrect(sk_canvas_t*, const sk_rrect_t*, const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_rrect (sk_canvas_t param0, sk_rrect_t param1, sk_paint_t param2);

		// void sk_canvas_draw_text(sk_canvas_t*, const char* text, size_t byteLength, float x, float y, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_text (sk_canvas_t param0, /* char */ void* text, /* size_t */ IntPtr byteLength, Single x, Single y, sk_paint_t paint);

		// void sk_canvas_draw_text_blob(sk_canvas_t*, sk_textblob_t* text, float x, float y, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_text_blob (sk_canvas_t param0, sk_textblob_t text, Single x, Single y, sk_paint_t paint);

		// void sk_canvas_draw_text_on_path(sk_canvas_t*, const char* text, size_t byteLength, const sk_path_t* path, float hOffset, float vOffset, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_text_on_path (sk_canvas_t param0, /* char */ void* text, /* size_t */ IntPtr byteLength, sk_path_t path, Single hOffset, Single vOffset, sk_paint_t paint);

		// void sk_canvas_draw_url_annotation(sk_canvas_t* t, const sk_rect_t* rect, sk_data_t* value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_url_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value);

		// void sk_canvas_draw_vertices(sk_canvas_t* ccanvas, const sk_vertices_t* vertices, sk_blendmode_t mode, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_vertices (sk_canvas_t ccanvas, sk_vertices_t vertices, SKBlendMode mode, sk_paint_t paint);

		// void sk_canvas_flush(sk_canvas_t* ccanvas)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_flush (sk_canvas_t ccanvas);

		// bool sk_canvas_get_device_clip_bounds(sk_canvas_t* t, sk_irect_t* cbounds)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_get_device_clip_bounds (sk_canvas_t t, SKRectI* cbounds);

		// bool sk_canvas_get_local_clip_bounds(sk_canvas_t* t, sk_rect_t* cbounds)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_get_local_clip_bounds (sk_canvas_t t, SKRect* cbounds);

		// int sk_canvas_get_save_count(sk_canvas_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_canvas_get_save_count (sk_canvas_t param0);

		// void sk_canvas_get_total_matrix(sk_canvas_t* ccanvas, sk_matrix_t* matrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_get_total_matrix (sk_canvas_t ccanvas, SKMatrix* matrix);

		// bool sk_canvas_is_clip_empty(sk_canvas_t* ccanvas)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_is_clip_empty (sk_canvas_t ccanvas);

		// bool sk_canvas_is_clip_rect(sk_canvas_t* ccanvas)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_is_clip_rect (sk_canvas_t ccanvas);

		// sk_canvas_t* sk_canvas_new_from_bitmap(const sk_bitmap_t* bitmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_canvas_new_from_bitmap (sk_bitmap_t bitmap);

		// bool sk_canvas_quick_reject(sk_canvas_t*, const sk_rect_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_quick_reject (sk_canvas_t param0, SKRect* param1);

		// void sk_canvas_reset_matrix(sk_canvas_t* ccanvas)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_reset_matrix (sk_canvas_t ccanvas);

		// void sk_canvas_restore(sk_canvas_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_restore (sk_canvas_t param0);

		// void sk_canvas_restore_to_count(sk_canvas_t*, int saveCount)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_restore_to_count (sk_canvas_t param0, Int32 saveCount);

		// void sk_canvas_rotate_degrees(sk_canvas_t*, float degrees)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_rotate_degrees (sk_canvas_t param0, Single degrees);

		// void sk_canvas_rotate_radians(sk_canvas_t*, float radians)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_rotate_radians (sk_canvas_t param0, Single radians);

		// int sk_canvas_save(sk_canvas_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_canvas_save (sk_canvas_t param0);

		// int sk_canvas_save_layer(sk_canvas_t*, const sk_rect_t*, const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_canvas_save_layer (sk_canvas_t param0, SKRect* param1, sk_paint_t param2);

		// void sk_canvas_scale(sk_canvas_t*, float sx, float sy)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_scale (sk_canvas_t param0, Single sx, Single sy);

		// void sk_canvas_set_matrix(sk_canvas_t* ccanvas, const sk_matrix_t* matrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_set_matrix (sk_canvas_t ccanvas, SKMatrix* matrix);

		// void sk_canvas_skew(sk_canvas_t*, float sx, float sy)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_skew (sk_canvas_t param0, Single sx, Single sy);

		// void sk_canvas_translate(sk_canvas_t*, float dx, float dy)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_translate (sk_canvas_t param0, Single dx, Single dy);

		// void sk_nodraw_canvas_destroy(sk_nodraw_canvas_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nodraw_canvas_destroy (sk_nodraw_canvas_t param0);

		// sk_nodraw_canvas_t* sk_nodraw_canvas_new(int width, int height)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_nodraw_canvas_t sk_nodraw_canvas_new (Int32 width, Int32 height);

		// void sk_nway_canvas_add_canvas(sk_nway_canvas_t*, sk_canvas_t* canvas)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nway_canvas_add_canvas (sk_nway_canvas_t param0, sk_canvas_t canvas);

		// void sk_nway_canvas_destroy(sk_nway_canvas_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nway_canvas_destroy (sk_nway_canvas_t param0);

		// sk_nway_canvas_t* sk_nway_canvas_new(int width, int height)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_nway_canvas_t sk_nway_canvas_new (Int32 width, Int32 height);

		// void sk_nway_canvas_remove_all(sk_nway_canvas_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nway_canvas_remove_all (sk_nway_canvas_t param0);

		// void sk_nway_canvas_remove_canvas(sk_nway_canvas_t*, sk_canvas_t* canvas)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nway_canvas_remove_canvas (sk_nway_canvas_t param0, sk_canvas_t canvas);

		// void sk_overdraw_canvas_destroy(sk_overdraw_canvas_t* canvas)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_overdraw_canvas_destroy (sk_overdraw_canvas_t canvas);

		// sk_overdraw_canvas_t* sk_overdraw_canvas_new(sk_canvas_t* canvas)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_overdraw_canvas_t sk_overdraw_canvas_new (sk_canvas_t canvas);

		#endregion

		#region sk_codec.h

		// void sk_codec_destroy(sk_codec_t* codec)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_codec_destroy (sk_codec_t codec);

		// sk_encoded_image_format_t sk_codec_get_encoded_format(sk_codec_t* codec)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKEncodedImageFormat sk_codec_get_encoded_format (sk_codec_t codec);

		// int sk_codec_get_frame_count(sk_codec_t* codec)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_get_frame_count (sk_codec_t codec);

		// void sk_codec_get_frame_info(sk_codec_t* codec, sk_codec_frameinfo_t* frameInfo)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_codec_get_frame_info (sk_codec_t codec, SKCodecFrameInfo* frameInfo);

		// bool sk_codec_get_frame_info_for_index(sk_codec_t* codec, int index, sk_codec_frameinfo_t* frameInfo)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_codec_get_frame_info_for_index (sk_codec_t codec, Int32 index, SKCodecFrameInfo* frameInfo);

		// void sk_codec_get_info(sk_codec_t* codec, sk_imageinfo_t* info)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_codec_get_info (sk_codec_t codec, SKImageInfoNative* info);

		// sk_encodedorigin_t sk_codec_get_origin(sk_codec_t* codec)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKEncodedOrigin sk_codec_get_origin (sk_codec_t codec);

		// sk_codec_result_t sk_codec_get_pixels(sk_codec_t* codec, const sk_imageinfo_t* info, void* pixels, size_t rowBytes, const sk_codec_options_t* options)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecResult sk_codec_get_pixels (sk_codec_t codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options);

		// int sk_codec_get_repetition_count(sk_codec_t* codec)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_get_repetition_count (sk_codec_t codec);

		// void sk_codec_get_scaled_dimensions(sk_codec_t* codec, float desiredScale, sk_isize_t* dimensions)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_codec_get_scaled_dimensions (sk_codec_t codec, Single desiredScale, SKSizeI* dimensions);

		// sk_codec_scanline_order_t sk_codec_get_scanline_order(sk_codec_t* codec)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecScanlineOrder sk_codec_get_scanline_order (sk_codec_t codec);

		// int sk_codec_get_scanlines(sk_codec_t* codec, void* dst, int countLines, size_t rowBytes)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_get_scanlines (sk_codec_t codec, void* dst, Int32 countLines, /* size_t */ IntPtr rowBytes);

		// bool sk_codec_get_valid_subset(sk_codec_t* codec, sk_irect_t* desiredSubset)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_codec_get_valid_subset (sk_codec_t codec, SKRectI* desiredSubset);

		// sk_codec_result_t sk_codec_incremental_decode(sk_codec_t* codec, int* rowsDecoded)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecResult sk_codec_incremental_decode (sk_codec_t codec, Int32* rowsDecoded);

		// size_t sk_codec_min_buffered_bytes_needed()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_codec_min_buffered_bytes_needed ();

		// sk_codec_t* sk_codec_new_from_data(sk_data_t* data)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_codec_t sk_codec_new_from_data (sk_data_t data);

		// sk_codec_t* sk_codec_new_from_stream(sk_stream_t* stream, sk_codec_result_t* result)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_codec_t sk_codec_new_from_stream (sk_stream_t stream, SKCodecResult* result);

		// int sk_codec_next_scanline(sk_codec_t* codec)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_next_scanline (sk_codec_t codec);

		// int sk_codec_output_scanline(sk_codec_t* codec, int inputScanline)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_output_scanline (sk_codec_t codec, Int32 inputScanline);

		// bool sk_codec_skip_scanlines(sk_codec_t* codec, int countLines)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_codec_skip_scanlines (sk_codec_t codec, Int32 countLines);

		// sk_codec_result_t sk_codec_start_incremental_decode(sk_codec_t* codec, const sk_imageinfo_t* info, void* pixels, size_t rowBytes, const sk_codec_options_t* options)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecResult sk_codec_start_incremental_decode (sk_codec_t codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options);

		// sk_codec_result_t sk_codec_start_scanline_decode(sk_codec_t* codec, const sk_imageinfo_t* info, const sk_codec_options_t* options)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecResult sk_codec_start_scanline_decode (sk_codec_t codec, SKImageInfoNative* info, SKCodecOptionsInternal* options);

		#endregion

		#region sk_colorfilter.h

		// sk_colorfilter_t* sk_colorfilter_new_color_matrix(const float[20] array = 20)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_color_matrix (Single* array);

		// sk_colorfilter_t* sk_colorfilter_new_compose(sk_colorfilter_t* outer, sk_colorfilter_t* inner)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_compose (sk_colorfilter_t outer, sk_colorfilter_t inner);

		// sk_colorfilter_t* sk_colorfilter_new_high_contrast(const sk_highcontrastconfig_t* config)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_high_contrast (SKHighContrastConfig* config);

		// sk_colorfilter_t* sk_colorfilter_new_lighting(sk_color_t mul, sk_color_t add)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_lighting (UInt32 mul, UInt32 add);

		// sk_colorfilter_t* sk_colorfilter_new_luma_color()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_luma_color ();

		// sk_colorfilter_t* sk_colorfilter_new_mode(sk_color_t c, sk_blendmode_t mode)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_mode (UInt32 c, SKBlendMode mode);

		// sk_colorfilter_t* sk_colorfilter_new_table(const uint8_t[256] table = 256)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_table (Byte* table);

		// sk_colorfilter_t* sk_colorfilter_new_table_argb(const uint8_t[256] tableA = 256, const uint8_t[256] tableR = 256, const uint8_t[256] tableG = 256, const uint8_t[256] tableB = 256)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_table_argb (Byte* tableA, Byte* tableR, Byte* tableG, Byte* tableB);

		// void sk_colorfilter_unref(sk_colorfilter_t* filter)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorfilter_unref (sk_colorfilter_t filter);

		#endregion

		#region sk_colorspace.h

		// void sk_color4f_from_color(sk_color_t color, sk_color4f_t* color4f)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_color4f_from_color (UInt32 color, SKColorF* color4f);

		// void sk_color4f_pin(const sk_color4f_t* color4f, sk_color4f_t* pinned)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_color4f_pin (SKColorF* color4f, SKColorF* pinned);

		// sk_color_t sk_color4f_to_color(const sk_color4f_t* color4f)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_color4f_to_color (SKColorF* color4f);

		// const sk_matrix44_t* sk_colorspace_as_from_xyzd50(const sk_colorspace_t* cColorSpace)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_matrix44_t sk_colorspace_as_from_xyzd50 (sk_colorspace_t cColorSpace);

		// const sk_matrix44_t* sk_colorspace_as_to_xyzd50(const sk_colorspace_t* cColorSpace)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_matrix44_t sk_colorspace_as_to_xyzd50 (sk_colorspace_t cColorSpace);

		// bool sk_colorspace_equals(const sk_colorspace_t* src, const sk_colorspace_t* dst)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_equals (sk_colorspace_t src, sk_colorspace_t dst);

		// bool sk_colorspace_gamma_close_to_srgb(const sk_colorspace_t* cColorSpace)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_gamma_close_to_srgb (sk_colorspace_t cColorSpace);

		// sk_gamma_named_t sk_colorspace_gamma_get_gamma_named(const sk_colorspace_t* cColorSpace)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKNamedGamma sk_colorspace_gamma_get_gamma_named (sk_colorspace_t cColorSpace);

		// sk_colorspace_type_t sk_colorspace_gamma_get_type(const sk_colorspace_t* cColorSpace)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKColorSpaceType sk_colorspace_gamma_get_type (sk_colorspace_t cColorSpace);

		// bool sk_colorspace_gamma_is_linear(const sk_colorspace_t* cColorSpace)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_gamma_is_linear (sk_colorspace_t cColorSpace);

		// bool sk_colorspace_is_numerical_transfer_fn(const sk_colorspace_t* cColorSpace, sk_colorspace_transfer_fn_t* fn)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_is_numerical_transfer_fn (sk_colorspace_t cColorSpace, SKColorSpaceTransferFn* fn);

		// bool sk_colorspace_is_srgb(const sk_colorspace_t* cColorSpace)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_is_srgb (sk_colorspace_t cColorSpace);

		// sk_colorspace_t* sk_colorspace_new_icc(const void* input, size_t len)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_icc (void* input, /* size_t */ IntPtr len);

		// sk_colorspace_t* sk_colorspace_new_rgb_with_coeffs(const sk_colorspace_transfer_fn_t* coeffs, const sk_matrix44_t* toXYZD50)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_rgb_with_coeffs (SKColorSpaceTransferFn* coeffs, sk_matrix44_t toXYZD50);

		// sk_colorspace_t* sk_colorspace_new_rgb_with_coeffs_and_gamut(const sk_colorspace_transfer_fn_t* coeffs, sk_colorspace_gamut_t gamut)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_rgb_with_coeffs_and_gamut (SKColorSpaceTransferFn* coeffs, SKColorSpaceGamut gamut);

		// sk_colorspace_t* sk_colorspace_new_rgb_with_gamma(sk_colorspace_render_target_gamma_t gamma, const sk_matrix44_t* toXYZD50)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_rgb_with_gamma (SKColorSpaceRenderTargetGamma gamma, sk_matrix44_t toXYZD50);

		// sk_colorspace_t* sk_colorspace_new_rgb_with_gamma_and_gamut(sk_colorspace_render_target_gamma_t gamma, sk_colorspace_gamut_t gamut)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_rgb_with_gamma_and_gamut (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut);

		// sk_colorspace_t* sk_colorspace_new_rgb_with_gamma_named(sk_gamma_named_t gamma, const sk_matrix44_t* toXYZD50)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_rgb_with_gamma_named (SKNamedGamma gamma, sk_matrix44_t toXYZD50);

		// sk_colorspace_t* sk_colorspace_new_rgb_with_gamma_named_and_gamut(sk_gamma_named_t gamma, sk_colorspace_gamut_t gamut)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_rgb_with_gamma_named_and_gamut (SKNamedGamma gamma, SKColorSpaceGamut gamut);

		// sk_colorspace_t* sk_colorspace_new_srgb()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_srgb ();

		// sk_colorspace_t* sk_colorspace_new_srgb_linear()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_srgb_linear ();

		// bool sk_colorspace_to_xyzd50(const sk_colorspace_t* cColorSpace, sk_matrix44_t* toXYZD50)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_to_xyzd50 (sk_colorspace_t cColorSpace, sk_matrix44_t toXYZD50);

		// void sk_colorspace_transfer_fn_invert(const sk_colorspace_transfer_fn_t* transfer, sk_colorspace_transfer_fn_t* inverted)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_transfer_fn_invert (SKColorSpaceTransferFn* transfer, SKColorSpaceTransferFn* inverted);

		// float sk_colorspace_transfer_fn_transform(const sk_colorspace_transfer_fn_t* transfer, float x)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_colorspace_transfer_fn_transform (SKColorSpaceTransferFn* transfer, Single x);

		// void sk_colorspace_unref(sk_colorspace_t* cColorSpace)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_unref (sk_colorspace_t cColorSpace);

		// bool sk_colorspaceprimaries_to_xyzd50(const sk_colorspaceprimaries_t* primaries, sk_matrix44_t* toXYZD50)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspaceprimaries_to_xyzd50 (SKColorSpacePrimaries* primaries, sk_matrix44_t toXYZD50);

		#endregion

		#region sk_colortable.h

		// int sk_colortable_count(const sk_colortable_t* ctable)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_colortable_count (sk_colortable_t ctable);

		// sk_colortable_t* sk_colortable_new(const sk_pmcolor_t* colors, int count)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colortable_t sk_colortable_new (UInt32* colors, Int32 count);

		// void sk_colortable_read_colors(const sk_colortable_t* ctable, sk_pmcolor_t** colors)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colortable_read_colors (sk_colortable_t ctable, UInt32** colors);

		// void sk_colortable_unref(sk_colortable_t* ctable)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colortable_unref (sk_colortable_t ctable);

		#endregion

		#region sk_data.h

		// const uint8_t* sk_data_get_bytes(const sk_data_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte* sk_data_get_bytes (sk_data_t param0);

		// const void* sk_data_get_data(const sk_data_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_data_get_data (sk_data_t param0);

		// size_t sk_data_get_size(const sk_data_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_data_get_size (sk_data_t param0);

		// sk_data_t* sk_data_new_empty()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_empty ();

		// sk_data_t* sk_data_new_from_file(const char* path)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_from_file (/* char */ void* path);

		// sk_data_t* sk_data_new_from_stream(sk_stream_t* stream, size_t length)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_from_stream (sk_stream_t stream, /* size_t */ IntPtr length);

		// sk_data_t* sk_data_new_subset(const sk_data_t* src, size_t offset, size_t length)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_subset (sk_data_t src, /* size_t */ IntPtr offset, /* size_t */ IntPtr length);

		// sk_data_t* sk_data_new_uninitialized(size_t size)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_uninitialized (/* size_t */ IntPtr size);

		// sk_data_t* sk_data_new_with_copy(const void* src, size_t length)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_with_copy (void* src, /* size_t */ IntPtr length);

		// sk_data_t* sk_data_new_with_proc(const void* ptr, size_t length, sk_data_release_proc proc, void* ctx)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_with_proc (void* ptr, /* size_t */ IntPtr length, SKDataReleaseProxyDelegate proc, void* ctx);

		// void sk_data_ref(const sk_data_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_data_ref (sk_data_t param0);

		// void sk_data_unref(const sk_data_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_data_unref (sk_data_t param0);

		#endregion

		#region sk_document.h

		// void sk_document_abort(sk_document_t* document)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_document_abort (sk_document_t document);

		// sk_canvas_t* sk_document_begin_page(sk_document_t* document, float width, float height, const sk_rect_t* content)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_document_begin_page (sk_document_t document, Single width, Single height, SKRect* content);

		// void sk_document_close(sk_document_t* document)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_document_close (sk_document_t document);

		// sk_document_t* sk_document_create_pdf_from_stream(sk_wstream_t* stream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_document_t sk_document_create_pdf_from_stream (sk_wstream_t stream);

		// sk_document_t* sk_document_create_pdf_from_stream_with_metadata(sk_wstream_t* stream, const sk_document_pdf_metadata_t* metadata)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_document_t sk_document_create_pdf_from_stream_with_metadata (sk_wstream_t stream, SKDocumentPdfMetadataInternal* metadata);

		// sk_document_t* sk_document_create_xps_from_stream(sk_wstream_t* stream, float dpi)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_document_t sk_document_create_xps_from_stream (sk_wstream_t stream, Single dpi);

		// void sk_document_end_page(sk_document_t* document)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_document_end_page (sk_document_t document);

		// void sk_document_unref(sk_document_t* document)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_document_unref (sk_document_t document);

		#endregion

		#region sk_drawable.h

		// void sk_drawable_draw(sk_drawable_t*, sk_canvas_t*, const sk_matrix_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_drawable_draw (sk_drawable_t param0, sk_canvas_t param1, SKMatrix* param2);

		// void sk_drawable_get_bounds(sk_drawable_t*, sk_rect_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_drawable_get_bounds (sk_drawable_t param0, SKRect* param1);

		// uint32_t sk_drawable_get_generation_id(sk_drawable_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_drawable_get_generation_id (sk_drawable_t param0);

		// sk_picture_t* sk_drawable_new_picture_snapshot(sk_drawable_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_picture_t sk_drawable_new_picture_snapshot (sk_drawable_t param0);

		// void sk_drawable_notify_drawing_changed(sk_drawable_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_drawable_notify_drawing_changed (sk_drawable_t param0);

		// void sk_drawable_unref(sk_drawable_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_drawable_unref (sk_drawable_t param0);

		#endregion

		#region sk_general.h

		// sk_colortype_t sk_colortype_get_default_8888()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKColorType sk_colortype_get_default_8888 ();

		// int sk_nvrefcnt_get_ref_count(const sk_nvrefcnt_t* refcnt)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_nvrefcnt_get_ref_count (sk_nvrefcnt_t refcnt);

		// void sk_nvrefcnt_safe_ref(sk_nvrefcnt_t* refcnt)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nvrefcnt_safe_ref (sk_nvrefcnt_t refcnt);

		// void sk_nvrefcnt_safe_unref(sk_nvrefcnt_t* refcnt)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nvrefcnt_safe_unref (sk_nvrefcnt_t refcnt);

		// bool sk_nvrefcnt_unique(const sk_nvrefcnt_t* refcnt)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_nvrefcnt_unique (sk_nvrefcnt_t refcnt);

		// int sk_refcnt_get_ref_count(const sk_refcnt_t* refcnt)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_refcnt_get_ref_count (sk_refcnt_t refcnt);

		// void sk_refcnt_safe_ref(sk_refcnt_t* refcnt)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_refcnt_safe_ref (sk_refcnt_t refcnt);

		// void sk_refcnt_safe_unref(sk_refcnt_t* refcnt)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_refcnt_safe_unref (sk_refcnt_t refcnt);

		// bool sk_refcnt_unique(const sk_refcnt_t* refcnt)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_refcnt_unique (sk_refcnt_t refcnt);

		#endregion

		#region sk_image.h

		// sk_data_t* sk_image_encode(const sk_image_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_image_encode (sk_image_t param0);

		// sk_data_t* sk_image_encode_specific(const sk_image_t* cimage, sk_encoded_image_format_t encoder, int quality)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_image_encode_specific (sk_image_t cimage, SKEncodedImageFormat encoder, Int32 quality);

		// sk_alphatype_t sk_image_get_alpha_type(const sk_image_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKAlphaType sk_image_get_alpha_type (sk_image_t param0);

		// sk_colortype_t sk_image_get_color_type(const sk_image_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKColorType sk_image_get_color_type (sk_image_t param0);

		// sk_colorspace_t* sk_image_get_colorspace(const sk_image_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_image_get_colorspace (sk_image_t param0);

		// int sk_image_get_height(const sk_image_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_image_get_height (sk_image_t param0);

		// uint32_t sk_image_get_unique_id(const sk_image_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_image_get_unique_id (sk_image_t param0);

		// int sk_image_get_width(const sk_image_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_image_get_width (sk_image_t param0);

		// bool sk_image_is_alpha_only(const sk_image_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_is_alpha_only (sk_image_t param0);

		// bool sk_image_is_lazy_generated(const sk_image_t* image)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_is_lazy_generated (sk_image_t image);

		// bool sk_image_is_texture_backed(const sk_image_t* image)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_is_texture_backed (sk_image_t image);

		// bool sk_image_is_valid(const sk_image_t* image, gr_context_t* context)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_is_valid (sk_image_t image, gr_context_t context);

		// sk_image_t* sk_image_make_non_texture_image(const sk_image_t* cimage)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_make_non_texture_image (sk_image_t cimage);

		// sk_image_t* sk_image_make_raster_image(const sk_image_t* cimage)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_make_raster_image (sk_image_t cimage);

		// sk_shader_t* sk_image_make_shader(const sk_image_t*, sk_shader_tilemode_t tileX, sk_shader_tilemode_t tileY, const sk_matrix_t* localMatrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_image_make_shader (sk_image_t param0, SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix* localMatrix);

		// sk_image_t* sk_image_make_subset(const sk_image_t* cimage, const sk_irect_t* subset)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_make_subset (sk_image_t cimage, SKRectI* subset);

		// sk_image_t* sk_image_make_texture_image(const sk_image_t* cimage, gr_context_t* context, sk_colorspace_t* colorspace)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_make_texture_image (sk_image_t cimage, gr_context_t context, sk_colorspace_t colorspace);

		// sk_image_t* sk_image_make_with_filter(const sk_image_t* cimage, const sk_imagefilter_t* filter, const sk_irect_t* subset, const sk_irect_t* clipBounds, sk_irect_t* outSubset, sk_ipoint_t* outOffset)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_make_with_filter (sk_image_t cimage, sk_imagefilter_t filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);

		// sk_image_t* sk_image_new_from_adopted_texture(gr_context_t* context, const gr_backendtexture_t* texture, gr_surfaceorigin_t origin, sk_colortype_t colorType, sk_alphatype_t alpha, sk_colorspace_t* colorSpace)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_from_adopted_texture (gr_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, sk_colorspace_t colorSpace);

		// sk_image_t* sk_image_new_from_bitmap(const sk_bitmap_t* cbitmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_from_bitmap (sk_bitmap_t cbitmap);

		// sk_image_t* sk_image_new_from_encoded(sk_data_t* encoded, const sk_irect_t* subset)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_from_encoded (sk_data_t encoded, SKRectI* subset);

		// sk_image_t* sk_image_new_from_picture(sk_picture_t* picture, const sk_isize_t* dimensions, const sk_matrix_t* matrix, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_from_picture (sk_picture_t picture, SKSizeI* dimensions, SKMatrix* matrix, sk_paint_t paint);

		// sk_image_t* sk_image_new_from_texture(gr_context_t* context, const gr_backendtexture_t* texture, gr_surfaceorigin_t origin, sk_colortype_t colorType, sk_alphatype_t alpha, sk_colorspace_t* colorSpace, sk_image_texture_release_proc releaseProc, void* releaseContext)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_from_texture (gr_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, sk_colorspace_t colorSpace, SKImageTextureReleaseProxyDelegate releaseProc, void* releaseContext);

		// sk_image_t* sk_image_new_raster(const sk_pixmap_t* pixmap, sk_image_raster_release_proc releaseProc, void* context)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_raster (sk_pixmap_t pixmap, SKImageRasterReleaseProxyDelegate releaseProc, void* context);

		// sk_image_t* sk_image_new_raster_copy(const sk_imageinfo_t*, const void* pixels, size_t rowBytes)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_raster_copy (SKImageInfoNative* param0, void* pixels, /* size_t */ IntPtr rowBytes);

		// sk_image_t* sk_image_new_raster_copy_with_pixmap(const sk_pixmap_t* pixmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_raster_copy_with_pixmap (sk_pixmap_t pixmap);

		// sk_image_t* sk_image_new_raster_data(const sk_imageinfo_t* cinfo, sk_data_t* pixels, size_t rowBytes)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_raster_data (SKImageInfoNative* cinfo, sk_data_t pixels, /* size_t */ IntPtr rowBytes);

		// bool sk_image_peek_pixels(const sk_image_t* image, sk_pixmap_t* pixmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_peek_pixels (sk_image_t image, sk_pixmap_t pixmap);

		// bool sk_image_read_pixels(const sk_image_t* image, const sk_imageinfo_t* dstInfo, void* dstPixels, size_t dstRowBytes, int srcX, int srcY, sk_image_caching_hint_t cachingHint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_read_pixels (sk_image_t image, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint);

		// bool sk_image_read_pixels_into_pixmap(const sk_image_t* image, const sk_pixmap_t* dst, int srcX, int srcY, sk_image_caching_hint_t cachingHint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_read_pixels_into_pixmap (sk_image_t image, sk_pixmap_t dst, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint);

		// void sk_image_ref(const sk_image_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_image_ref (sk_image_t param0);

		// sk_data_t* sk_image_ref_encoded(const sk_image_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_image_ref_encoded (sk_image_t param0);

		// bool sk_image_scale_pixels(const sk_image_t* image, const sk_pixmap_t* dst, sk_filter_quality_t quality, sk_image_caching_hint_t cachingHint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_scale_pixels (sk_image_t image, sk_pixmap_t dst, SKFilterQuality quality, SKImageCachingHint cachingHint);

		// void sk_image_unref(const sk_image_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_image_unref (sk_image_t param0);

		#endregion

		#region sk_imagefilter.h

		// void sk_imagefilter_croprect_destructor(sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_imagefilter_croprect_destructor (sk_imagefilter_croprect_t cropRect);

		// uint32_t sk_imagefilter_croprect_get_flags(sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_imagefilter_croprect_get_flags (sk_imagefilter_croprect_t cropRect);

		// void sk_imagefilter_croprect_get_rect(sk_imagefilter_croprect_t* cropRect, sk_rect_t* rect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_imagefilter_croprect_get_rect (sk_imagefilter_croprect_t cropRect, SKRect* rect);

		// sk_imagefilter_croprect_t* sk_imagefilter_croprect_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_croprect_t sk_imagefilter_croprect_new ();

		// sk_imagefilter_croprect_t* sk_imagefilter_croprect_new_with_rect(const sk_rect_t* rect, uint32_t flags)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_croprect_t sk_imagefilter_croprect_new_with_rect (SKRect* rect, UInt32 flags);

		// sk_imagefilter_t* sk_imagefilter_new_alpha_threshold(const sk_region_t* region, float innerThreshold, float outerThreshold, sk_imagefilter_t* input)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_alpha_threshold (sk_region_t region, Single innerThreshold, Single outerThreshold, sk_imagefilter_t input);

		// sk_imagefilter_t* sk_imagefilter_new_arithmetic(float k1, float k2, float k3, float k4, bool enforcePMColor, sk_imagefilter_t* background, sk_imagefilter_t* foreground, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_arithmetic (Single k1, Single k2, Single k3, Single k4, [MarshalAs (UnmanagedType.I1)] bool enforcePMColor, sk_imagefilter_t background, sk_imagefilter_t foreground, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_blur(float sigmaX, float sigmaY, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_blur (Single sigmaX, Single sigmaY, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_color_filter(sk_colorfilter_t* cf, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_color_filter (sk_colorfilter_t cf, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_compose(sk_imagefilter_t* outer, sk_imagefilter_t* inner)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_compose (sk_imagefilter_t outer, sk_imagefilter_t inner);

		// sk_imagefilter_t* sk_imagefilter_new_dilate(int radiusX, int radiusY, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_dilate (Int32 radiusX, Int32 radiusY, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_displacement_map_effect(sk_displacement_map_effect_channel_selector_type_t xChannelSelector, sk_displacement_map_effect_channel_selector_type_t yChannelSelector, float scale, sk_imagefilter_t* displacement, sk_imagefilter_t* color, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_displacement_map_effect (SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, Single scale, sk_imagefilter_t displacement, sk_imagefilter_t color, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_distant_lit_diffuse(const sk_point3_t* direction, sk_color_t lightColor, float surfaceScale, float kd, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_distant_lit_diffuse (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_distant_lit_specular(const sk_point3_t* direction, sk_color_t lightColor, float surfaceScale, float ks, float shininess, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_distant_lit_specular (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_drop_shadow(float dx, float dy, float sigmaX, float sigmaY, sk_color_t color, sk_drop_shadow_image_filter_shadow_mode_t shadowMode, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_drop_shadow (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, SKDropShadowImageFilterShadowMode shadowMode, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_erode(int radiusX, int radiusY, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_erode (Int32 radiusX, Int32 radiusY, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_image_source(sk_image_t* image, const sk_rect_t* srcRect, const sk_rect_t* dstRect, sk_filter_quality_t filterQuality)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_image_source (sk_image_t image, SKRect* srcRect, SKRect* dstRect, SKFilterQuality filterQuality);

		// sk_imagefilter_t* sk_imagefilter_new_image_source_default(sk_image_t* image)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_image_source_default (sk_image_t image);

		// sk_imagefilter_t* sk_imagefilter_new_magnifier(const sk_rect_t* src, float inset, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_magnifier (SKRect* src, Single inset, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_matrix(const sk_matrix_t* matrix, sk_filter_quality_t quality, sk_imagefilter_t* input)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_matrix (SKMatrix* matrix, SKFilterQuality quality, sk_imagefilter_t input);

		// sk_imagefilter_t* sk_imagefilter_new_matrix_convolution(const sk_isize_t* kernelSize, const float[-1] kernel, float gain, float bias, const sk_ipoint_t* kernelOffset, sk_matrix_convolution_tilemode_t tileMode, bool convolveAlpha, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_matrix_convolution (SKSizeI* kernelSize, Single* kernel, Single gain, Single bias, SKPointI* kernelOffset, SKMatrixConvolutionTileMode tileMode, [MarshalAs (UnmanagedType.I1)] bool convolveAlpha, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_merge(sk_imagefilter_t*[-1] filters, int count, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_merge (sk_imagefilter_t* filters, Int32 count, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_offset(float dx, float dy, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_offset (Single dx, Single dy, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_paint(const sk_paint_t* paint, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_paint (sk_paint_t paint, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_picture(sk_picture_t* picture)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_picture (sk_picture_t picture);

		// sk_imagefilter_t* sk_imagefilter_new_picture_with_croprect(sk_picture_t* picture, const sk_rect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_picture_with_croprect (sk_picture_t picture, SKRect* cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_point_lit_diffuse(const sk_point3_t* location, sk_color_t lightColor, float surfaceScale, float kd, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_point_lit_diffuse (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_point_lit_specular(const sk_point3_t* location, sk_color_t lightColor, float surfaceScale, float ks, float shininess, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_point_lit_specular (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_spot_lit_diffuse(const sk_point3_t* location, const sk_point3_t* target, float specularExponent, float cutoffAngle, sk_color_t lightColor, float surfaceScale, float kd, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_spot_lit_diffuse (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_spot_lit_specular(const sk_point3_t* location, const sk_point3_t* target, float specularExponent, float cutoffAngle, sk_color_t lightColor, float surfaceScale, float ks, float shininess, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_spot_lit_specular (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);

		// sk_imagefilter_t* sk_imagefilter_new_tile(const sk_rect_t* src, const sk_rect_t* dst, sk_imagefilter_t* input)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_tile (SKRect* src, SKRect* dst, sk_imagefilter_t input);

		// sk_imagefilter_t* sk_imagefilter_new_xfermode(sk_blendmode_t mode, sk_imagefilter_t* background, sk_imagefilter_t* foreground, const sk_imagefilter_croprect_t* cropRect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_xfermode (SKBlendMode mode, sk_imagefilter_t background, sk_imagefilter_t foreground, sk_imagefilter_croprect_t cropRect);

		// void sk_imagefilter_unref(sk_imagefilter_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_imagefilter_unref (sk_imagefilter_t param0);

		#endregion

		#region sk_mask.h

		// uint8_t* sk_mask_alloc_image(size_t bytes)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte* sk_mask_alloc_image (/* size_t */ IntPtr bytes);

		// size_t sk_mask_compute_image_size(sk_mask_t* cmask)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_mask_compute_image_size (SKMask* cmask);

		// size_t sk_mask_compute_total_image_size(sk_mask_t* cmask)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_mask_compute_total_image_size (SKMask* cmask);

		// void sk_mask_free_image(void* image)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_mask_free_image (void* image);

		// void* sk_mask_get_addr(sk_mask_t* cmask, int x, int y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_mask_get_addr (SKMask* cmask, Int32 x, Int32 y);

		// uint8_t sk_mask_get_addr_1(sk_mask_t* cmask, int x, int y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte sk_mask_get_addr_1 (SKMask* cmask, Int32 x, Int32 y);

		// uint32_t sk_mask_get_addr_32(sk_mask_t* cmask, int x, int y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_mask_get_addr_32 (SKMask* cmask, Int32 x, Int32 y);

		// uint8_t sk_mask_get_addr_8(sk_mask_t* cmask, int x, int y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte sk_mask_get_addr_8 (SKMask* cmask, Int32 x, Int32 y);

		// uint16_t sk_mask_get_addr_lcd_16(sk_mask_t* cmask, int x, int y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt16 sk_mask_get_addr_lcd_16 (SKMask* cmask, Int32 x, Int32 y);

		// bool sk_mask_is_empty(sk_mask_t* cmask)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_mask_is_empty (SKMask* cmask);

		#endregion

		#region sk_maskfilter.h

		// sk_maskfilter_t* sk_maskfilter_new_blur(sk_blurstyle_t, float sigma)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_blur (SKBlurStyle param0, Single sigma);

		// sk_maskfilter_t* sk_maskfilter_new_blur_with_flags(sk_blurstyle_t, float sigma, const sk_rect_t* occluder, bool respectCTM)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_blur_with_flags (SKBlurStyle param0, Single sigma, SKRect* occluder, [MarshalAs (UnmanagedType.I1)] bool respectCTM);

		// sk_maskfilter_t* sk_maskfilter_new_clip(uint8_t min, uint8_t max)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_clip (Byte min, Byte max);

		// sk_maskfilter_t* sk_maskfilter_new_gamma(float gamma)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_gamma (Single gamma);

		// sk_maskfilter_t* sk_maskfilter_new_table(const uint8_t[256] table = 256)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_table (Byte* table);

		// void sk_maskfilter_ref(sk_maskfilter_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_maskfilter_ref (sk_maskfilter_t param0);

		// void sk_maskfilter_unref(sk_maskfilter_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_maskfilter_unref (sk_maskfilter_t param0);

		#endregion

		#region sk_matrix.h

		// void sk_3dview_apply_to_canvas(sk_3dview_t* cview, sk_canvas_t* ccanvas)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_apply_to_canvas (sk_3dview_t cview, sk_canvas_t ccanvas);

		// void sk_3dview_destroy(sk_3dview_t* cview)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_destroy (sk_3dview_t cview);

		// float sk_3dview_dot_with_normal(sk_3dview_t* cview, float dx, float dy, float dz)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_3dview_dot_with_normal (sk_3dview_t cview, Single dx, Single dy, Single dz);

		// void sk_3dview_get_matrix(sk_3dview_t* cview, sk_matrix_t* cmatrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_get_matrix (sk_3dview_t cview, SKMatrix* cmatrix);

		// sk_3dview_t* sk_3dview_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_3dview_t sk_3dview_new ();

		// void sk_3dview_restore(sk_3dview_t* cview)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_restore (sk_3dview_t cview);

		// void sk_3dview_rotate_x_degrees(sk_3dview_t* cview, float degrees)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_rotate_x_degrees (sk_3dview_t cview, Single degrees);

		// void sk_3dview_rotate_x_radians(sk_3dview_t* cview, float radians)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_rotate_x_radians (sk_3dview_t cview, Single radians);

		// void sk_3dview_rotate_y_degrees(sk_3dview_t* cview, float degrees)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_rotate_y_degrees (sk_3dview_t cview, Single degrees);

		// void sk_3dview_rotate_y_radians(sk_3dview_t* cview, float radians)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_rotate_y_radians (sk_3dview_t cview, Single radians);

		// void sk_3dview_rotate_z_degrees(sk_3dview_t* cview, float degrees)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_rotate_z_degrees (sk_3dview_t cview, Single degrees);

		// void sk_3dview_rotate_z_radians(sk_3dview_t* cview, float radians)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_rotate_z_radians (sk_3dview_t cview, Single radians);

		// void sk_3dview_save(sk_3dview_t* cview)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_save (sk_3dview_t cview);

		// void sk_3dview_translate(sk_3dview_t* cview, float x, float y, float z)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_translate (sk_3dview_t cview, Single x, Single y, Single z);

		// void sk_matrix_concat(sk_matrix_t* result, sk_matrix_t* first, sk_matrix_t* second)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_concat (SKMatrix* result, SKMatrix* first, SKMatrix* second);

		// void sk_matrix_map_points(sk_matrix_t* matrix, sk_point_t* dst, sk_point_t* src, int count)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_points (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count);

		// float sk_matrix_map_radius(sk_matrix_t* matrix, float radius)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_matrix_map_radius (SKMatrix* matrix, Single radius);

		// void sk_matrix_map_rect(sk_matrix_t* matrix, sk_rect_t* dest, sk_rect_t* source)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_rect (SKMatrix* matrix, SKRect* dest, SKRect* source);

		// void sk_matrix_map_vector(sk_matrix_t* matrix, float x, float y, sk_point_t* result)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_vector (SKMatrix* matrix, Single x, Single y, SKPoint* result);

		// void sk_matrix_map_vectors(sk_matrix_t* matrix, sk_point_t* dst, sk_point_t* src, int count)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_vectors (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count);

		// void sk_matrix_map_xy(sk_matrix_t* matrix, float x, float y, sk_point_t* result)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_xy (SKMatrix* matrix, Single x, Single y, SKPoint* result);

		// void sk_matrix_post_concat(sk_matrix_t* result, sk_matrix_t* matrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_post_concat (SKMatrix* result, SKMatrix* matrix);

		// void sk_matrix_pre_concat(sk_matrix_t* result, sk_matrix_t* matrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_pre_concat (SKMatrix* result, SKMatrix* matrix);

		// bool sk_matrix_try_invert(sk_matrix_t* matrix, sk_matrix_t* result)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_matrix_try_invert (SKMatrix* matrix, SKMatrix* result);

		// void sk_matrix44_as_col_major(sk_matrix44_t* matrix, float* dst)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_as_col_major (sk_matrix44_t matrix, Single* dst);

		// void sk_matrix44_as_row_major(sk_matrix44_t* matrix, float* dst)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_as_row_major (sk_matrix44_t matrix, Single* dst);

		// void sk_matrix44_destroy(sk_matrix44_t* matrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_destroy (sk_matrix44_t matrix);

		// double sk_matrix44_determinant(sk_matrix44_t* matrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Double sk_matrix44_determinant (sk_matrix44_t matrix);

		// bool sk_matrix44_equals(sk_matrix44_t* matrix, const sk_matrix44_t* other)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_matrix44_equals (sk_matrix44_t matrix, sk_matrix44_t other);

		// float sk_matrix44_get(sk_matrix44_t* matrix, int row, int col)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_matrix44_get (sk_matrix44_t matrix, Int32 row, Int32 col);

		// sk_matrix44_type_mask_t sk_matrix44_get_type(sk_matrix44_t* matrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKMatrix44TypeMask sk_matrix44_get_type (sk_matrix44_t matrix);

		// bool sk_matrix44_invert(sk_matrix44_t* matrix, sk_matrix44_t* inverse)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_matrix44_invert (sk_matrix44_t matrix, sk_matrix44_t inverse);

		// void sk_matrix44_map_scalars(sk_matrix44_t* matrix, const float* src, float* dst)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_map_scalars (sk_matrix44_t matrix, Single* src, Single* dst);

		// void sk_matrix44_map2(sk_matrix44_t* matrix, const float* src2, int count, float* dst4)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_map2 (sk_matrix44_t matrix, Single* src2, Int32 count, Single* dst4);

		// sk_matrix44_t* sk_matrix44_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_matrix44_t sk_matrix44_new ();

		// sk_matrix44_t* sk_matrix44_new_concat(const sk_matrix44_t* a, const sk_matrix44_t* b)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_matrix44_t sk_matrix44_new_concat (sk_matrix44_t a, sk_matrix44_t b);

		// sk_matrix44_t* sk_matrix44_new_copy(const sk_matrix44_t* src)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_matrix44_t sk_matrix44_new_copy (sk_matrix44_t src);

		// sk_matrix44_t* sk_matrix44_new_identity()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_matrix44_t sk_matrix44_new_identity ();

		// sk_matrix44_t* sk_matrix44_new_matrix(const sk_matrix_t* src)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_matrix44_t sk_matrix44_new_matrix (SKMatrix* src);

		// void sk_matrix44_post_concat(sk_matrix44_t* matrix, const sk_matrix44_t* m)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_post_concat (sk_matrix44_t matrix, sk_matrix44_t m);

		// void sk_matrix44_post_scale(sk_matrix44_t* matrix, float sx, float sy, float sz)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_post_scale (sk_matrix44_t matrix, Single sx, Single sy, Single sz);

		// void sk_matrix44_post_translate(sk_matrix44_t* matrix, float dx, float dy, float dz)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_post_translate (sk_matrix44_t matrix, Single dx, Single dy, Single dz);

		// void sk_matrix44_pre_concat(sk_matrix44_t* matrix, const sk_matrix44_t* m)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_pre_concat (sk_matrix44_t matrix, sk_matrix44_t m);

		// void sk_matrix44_pre_scale(sk_matrix44_t* matrix, float sx, float sy, float sz)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_pre_scale (sk_matrix44_t matrix, Single sx, Single sy, Single sz);

		// void sk_matrix44_pre_translate(sk_matrix44_t* matrix, float dx, float dy, float dz)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_pre_translate (sk_matrix44_t matrix, Single dx, Single dy, Single dz);

		// bool sk_matrix44_preserves_2d_axis_alignment(sk_matrix44_t* matrix, float epsilon)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_matrix44_preserves_2d_axis_alignment (sk_matrix44_t matrix, Single epsilon);

		// void sk_matrix44_set(sk_matrix44_t* matrix, int row, int col, float value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set (sk_matrix44_t matrix, Int32 row, Int32 col, Single value);

		// void sk_matrix44_set_3x3_row_major(sk_matrix44_t* matrix, float* dst)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_3x3_row_major (sk_matrix44_t matrix, Single* dst);

		// void sk_matrix44_set_col_major(sk_matrix44_t* matrix, float* dst)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_col_major (sk_matrix44_t matrix, Single* dst);

		// void sk_matrix44_set_concat(sk_matrix44_t* matrix, const sk_matrix44_t* a, const sk_matrix44_t* b)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_concat (sk_matrix44_t matrix, sk_matrix44_t a, sk_matrix44_t b);

		// void sk_matrix44_set_identity(sk_matrix44_t* matrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_identity (sk_matrix44_t matrix);

		// void sk_matrix44_set_rotate_about_degrees(sk_matrix44_t* matrix, float x, float y, float z, float degrees)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_rotate_about_degrees (sk_matrix44_t matrix, Single x, Single y, Single z, Single degrees);

		// void sk_matrix44_set_rotate_about_radians(sk_matrix44_t* matrix, float x, float y, float z, float radians)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_rotate_about_radians (sk_matrix44_t matrix, Single x, Single y, Single z, Single radians);

		// void sk_matrix44_set_rotate_about_radians_unit(sk_matrix44_t* matrix, float x, float y, float z, float radians)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_rotate_about_radians_unit (sk_matrix44_t matrix, Single x, Single y, Single z, Single radians);

		// void sk_matrix44_set_row_major(sk_matrix44_t* matrix, float* dst)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_row_major (sk_matrix44_t matrix, Single* dst);

		// void sk_matrix44_set_scale(sk_matrix44_t* matrix, float sx, float sy, float sz)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_scale (sk_matrix44_t matrix, Single sx, Single sy, Single sz);

		// void sk_matrix44_set_translate(sk_matrix44_t* matrix, float dx, float dy, float dz)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_translate (sk_matrix44_t matrix, Single dx, Single dy, Single dz);

		// void sk_matrix44_to_matrix(sk_matrix44_t* matrix, sk_matrix_t* dst)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_to_matrix (sk_matrix44_t matrix, SKMatrix* dst);

		// void sk_matrix44_transpose(sk_matrix44_t* matrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_transpose (sk_matrix44_t matrix);

		#endregion

		#region sk_paint.h

		// size_t sk_paint_break_text(const sk_paint_t* cpaint, const void* text, size_t length, float maxWidth, float* measuredWidth)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_paint_break_text (sk_paint_t cpaint, void* text, /* size_t */ IntPtr length, Single maxWidth, Single* measuredWidth);

		// sk_paint_t* sk_paint_clone(sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_paint_t sk_paint_clone (sk_paint_t param0);

		// bool sk_paint_contains_text(const sk_paint_t* cpaint, const void* text, size_t byteLength)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_contains_text (sk_paint_t cpaint, void* text, /* size_t */ IntPtr byteLength);

		// int sk_paint_count_text(const sk_paint_t* cpaint, const void* text, size_t byteLength)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_paint_count_text (sk_paint_t cpaint, void* text, /* size_t */ IntPtr byteLength);

		// void sk_paint_delete(sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_delete (sk_paint_t param0);

		// sk_blendmode_t sk_paint_get_blendmode(sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKBlendMode sk_paint_get_blendmode (sk_paint_t param0);

		// sk_color_t sk_paint_get_color(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_paint_get_color (sk_paint_t param0);

		// sk_colorfilter_t* sk_paint_get_colorfilter(sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_paint_get_colorfilter (sk_paint_t param0);

		// bool sk_paint_get_fill_path(const sk_paint_t*, const sk_path_t* src, sk_path_t* dst, const sk_rect_t* cullRect, float resScale)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_get_fill_path (sk_paint_t param0, sk_path_t src, sk_path_t dst, SKRect* cullRect, Single resScale);

		// sk_filter_quality_t sk_paint_get_filter_quality(sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKFilterQuality sk_paint_get_filter_quality (sk_paint_t param0);

		// float sk_paint_get_fontmetrics(sk_paint_t* cpaint, sk_fontmetrics_t* cfontmetrics, float scale)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_paint_get_fontmetrics (sk_paint_t cpaint, SKFontMetrics* cfontmetrics, Single scale);

		// sk_paint_hinting_t sk_paint_get_hinting(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPaintHinting sk_paint_get_hinting (sk_paint_t param0);

		// sk_imagefilter_t* sk_paint_get_imagefilter(sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_paint_get_imagefilter (sk_paint_t param0);

		// sk_maskfilter_t* sk_paint_get_maskfilter(sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_paint_get_maskfilter (sk_paint_t param0);

		// sk_path_effect_t* sk_paint_get_path_effect(sk_paint_t* cpaint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_paint_get_path_effect (sk_paint_t cpaint);

		// int sk_paint_get_pos_text_blob_intercepts(const sk_paint_t* cpaint, sk_textblob_t* blob, const float[2] bounds = 2, float* intervals)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_paint_get_pos_text_blob_intercepts (sk_paint_t cpaint, sk_textblob_t blob, Single* bounds, Single* intervals);

		// int sk_paint_get_pos_text_h_intercepts(const sk_paint_t* cpaint, const void* text, size_t byteLength, float* xpos, float y, const float[2] bounds = 2, float* intervals)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_paint_get_pos_text_h_intercepts (sk_paint_t cpaint, void* text, /* size_t */ IntPtr byteLength, Single* xpos, Single y, Single* bounds, Single* intervals);

		// int sk_paint_get_pos_text_intercepts(const sk_paint_t* cpaint, const void* text, size_t byteLength, sk_point_t* pos, const float[2] bounds = 2, float* intervals)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_paint_get_pos_text_intercepts (sk_paint_t cpaint, void* text, /* size_t */ IntPtr byteLength, SKPoint* pos, Single* bounds, Single* intervals);

		// sk_path_t* sk_paint_get_pos_text_path(sk_paint_t* cpaint, const void* text, size_t length, const sk_point_t[-1] pos)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_t sk_paint_get_pos_text_path (sk_paint_t cpaint, void* text, /* size_t */ IntPtr length, SKPoint* pos);

		// sk_shader_t* sk_paint_get_shader(sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_paint_get_shader (sk_paint_t param0);

		// sk_stroke_cap_t sk_paint_get_stroke_cap(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKStrokeCap sk_paint_get_stroke_cap (sk_paint_t param0);

		// sk_stroke_join_t sk_paint_get_stroke_join(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKStrokeJoin sk_paint_get_stroke_join (sk_paint_t param0);

		// float sk_paint_get_stroke_miter(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_paint_get_stroke_miter (sk_paint_t param0);

		// float sk_paint_get_stroke_width(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_paint_get_stroke_width (sk_paint_t param0);

		// sk_paint_style_t sk_paint_get_style(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPaintStyle sk_paint_get_style (sk_paint_t param0);

		// sk_text_align_t sk_paint_get_text_align(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKTextAlign sk_paint_get_text_align (sk_paint_t param0);

		// sk_text_encoding_t sk_paint_get_text_encoding(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKTextEncoding sk_paint_get_text_encoding (sk_paint_t param0);

		// int sk_paint_get_text_intercepts(const sk_paint_t* cpaint, const void* text, size_t byteLength, float x, float y, const float[2] bounds = 2, float* intervals)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_paint_get_text_intercepts (sk_paint_t cpaint, void* text, /* size_t */ IntPtr byteLength, Single x, Single y, Single* bounds, Single* intervals);

		// sk_path_t* sk_paint_get_text_path(sk_paint_t* cpaint, const void* text, size_t length, float x, float y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_t sk_paint_get_text_path (sk_paint_t cpaint, void* text, /* size_t */ IntPtr length, Single x, Single y);

		// float sk_paint_get_text_scale_x(const sk_paint_t* cpaint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_paint_get_text_scale_x (sk_paint_t cpaint);

		// float sk_paint_get_text_skew_x(const sk_paint_t* cpaint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_paint_get_text_skew_x (sk_paint_t cpaint);

		// int sk_paint_get_text_widths(const sk_paint_t* cpaint, const void* text, size_t byteLength, float* widths, sk_rect_t* bounds)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_paint_get_text_widths (sk_paint_t cpaint, void* text, /* size_t */ IntPtr byteLength, Single* widths, SKRect* bounds);

		// float sk_paint_get_textsize(sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_paint_get_textsize (sk_paint_t param0);

		// sk_typeface_t* sk_paint_get_typeface(sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_paint_get_typeface (sk_paint_t param0);

		// bool sk_paint_is_antialias(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_is_antialias (sk_paint_t param0);

		// bool sk_paint_is_autohinted(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_is_autohinted (sk_paint_t param0);

		// bool sk_paint_is_dev_kern_text(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_is_dev_kern_text (sk_paint_t param0);

		// bool sk_paint_is_dither(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_is_dither (sk_paint_t param0);

		// bool sk_paint_is_embedded_bitmap_text(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_is_embedded_bitmap_text (sk_paint_t param0);

		// bool sk_paint_is_fake_bold_text(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_is_fake_bold_text (sk_paint_t param0);

		// bool sk_paint_is_lcd_render_text(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_is_lcd_render_text (sk_paint_t param0);

		// bool sk_paint_is_linear_text(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_is_linear_text (sk_paint_t param0);

		// bool sk_paint_is_subpixel_text(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_is_subpixel_text (sk_paint_t param0);

		// bool sk_paint_is_verticaltext(const sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_is_verticaltext (sk_paint_t param0);

		// float sk_paint_measure_text(const sk_paint_t* cpaint, const void* text, size_t length, sk_rect_t* cbounds)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_paint_measure_text (sk_paint_t cpaint, void* text, /* size_t */ IntPtr length, SKRect* cbounds);

		// sk_paint_t* sk_paint_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_paint_t sk_paint_new ();

		// void sk_paint_reset(sk_paint_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_reset (sk_paint_t param0);

		// void sk_paint_set_antialias(sk_paint_t*, bool)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_antialias (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);

		// void sk_paint_set_autohinted(sk_paint_t*, bool)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_autohinted (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);

		// void sk_paint_set_blendmode(sk_paint_t*, sk_blendmode_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_blendmode (sk_paint_t param0, SKBlendMode param1);

		// void sk_paint_set_color(sk_paint_t*, sk_color_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_color (sk_paint_t param0, UInt32 param1);

		// void sk_paint_set_colorfilter(sk_paint_t*, sk_colorfilter_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_colorfilter (sk_paint_t param0, sk_colorfilter_t param1);

		// void sk_paint_set_dev_kern_text(sk_paint_t*, bool)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_dev_kern_text (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);

		// void sk_paint_set_dither(sk_paint_t*, bool)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_dither (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);

		// void sk_paint_set_embedded_bitmap_text(sk_paint_t*, bool)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_embedded_bitmap_text (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);

		// void sk_paint_set_fake_bold_text(sk_paint_t*, bool)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_fake_bold_text (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);

		// void sk_paint_set_filter_quality(sk_paint_t*, sk_filter_quality_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_filter_quality (sk_paint_t param0, SKFilterQuality param1);

		// void sk_paint_set_hinting(sk_paint_t*, sk_paint_hinting_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_hinting (sk_paint_t param0, SKPaintHinting param1);

		// void sk_paint_set_imagefilter(sk_paint_t*, sk_imagefilter_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_imagefilter (sk_paint_t param0, sk_imagefilter_t param1);

		// void sk_paint_set_lcd_render_text(sk_paint_t*, bool)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_lcd_render_text (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);

		// void sk_paint_set_linear_text(sk_paint_t*, bool)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_linear_text (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);

		// void sk_paint_set_maskfilter(sk_paint_t*, sk_maskfilter_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_maskfilter (sk_paint_t param0, sk_maskfilter_t param1);

		// void sk_paint_set_path_effect(sk_paint_t* cpaint, sk_path_effect_t* effect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_path_effect (sk_paint_t cpaint, sk_path_effect_t effect);

		// void sk_paint_set_shader(sk_paint_t*, sk_shader_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_shader (sk_paint_t param0, sk_shader_t param1);

		// void sk_paint_set_stroke_cap(sk_paint_t*, sk_stroke_cap_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_stroke_cap (sk_paint_t param0, SKStrokeCap param1);

		// void sk_paint_set_stroke_join(sk_paint_t*, sk_stroke_join_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_stroke_join (sk_paint_t param0, SKStrokeJoin param1);

		// void sk_paint_set_stroke_miter(sk_paint_t*, float miter)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_stroke_miter (sk_paint_t param0, Single miter);

		// void sk_paint_set_stroke_width(sk_paint_t*, float width)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_stroke_width (sk_paint_t param0, Single width);

		// void sk_paint_set_style(sk_paint_t*, sk_paint_style_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_style (sk_paint_t param0, SKPaintStyle param1);

		// void sk_paint_set_subpixel_text(sk_paint_t*, bool)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_subpixel_text (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);

		// void sk_paint_set_text_align(sk_paint_t*, sk_text_align_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_text_align (sk_paint_t param0, SKTextAlign param1);

		// void sk_paint_set_text_encoding(sk_paint_t*, sk_text_encoding_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_text_encoding (sk_paint_t param0, SKTextEncoding param1);

		// void sk_paint_set_text_scale_x(sk_paint_t* cpaint, float scale)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_text_scale_x (sk_paint_t cpaint, Single scale);

		// void sk_paint_set_text_skew_x(sk_paint_t* cpaint, float skew)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_text_skew_x (sk_paint_t cpaint, Single skew);

		// void sk_paint_set_textsize(sk_paint_t*, float)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_textsize (sk_paint_t param0, Single param1);

		// void sk_paint_set_typeface(sk_paint_t*, sk_typeface_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_typeface (sk_paint_t param0, sk_typeface_t param1);

		// void sk_paint_set_verticaltext(sk_paint_t*, bool)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_verticaltext (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);

		// int sk_paint_text_to_glyphs(const sk_paint_t* cpaint, const void* text, size_t byteLength, uint16_t* glyphs)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_paint_text_to_glyphs (sk_paint_t cpaint, void* text, /* size_t */ IntPtr byteLength, UInt16* glyphs);

		#endregion

		#region sk_path.h

		// void sk_opbuilder_add(sk_opbuilder_t* builder, const sk_path_t* path, sk_pathop_t op)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_opbuilder_add (sk_opbuilder_t builder, sk_path_t path, SKPathOp op);

		// void sk_opbuilder_destroy(sk_opbuilder_t* builder)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_opbuilder_destroy (sk_opbuilder_t builder);

		// sk_opbuilder_t* sk_opbuilder_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_opbuilder_t sk_opbuilder_new ();

		// bool sk_opbuilder_resolve(sk_opbuilder_t* builder, sk_path_t* result)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_opbuilder_resolve (sk_opbuilder_t builder, sk_path_t result);

		// void sk_path_add_arc(sk_path_t* cpath, const sk_rect_t* crect, float startAngle, float sweepAngle)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_arc (sk_path_t cpath, SKRect* crect, Single startAngle, Single sweepAngle);

		// void sk_path_add_circle(sk_path_t*, float x, float y, float radius, sk_path_direction_t dir)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_circle (sk_path_t param0, Single x, Single y, Single radius, SKPathDirection dir);

		// void sk_path_add_oval(sk_path_t*, const sk_rect_t*, sk_path_direction_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_oval (sk_path_t param0, SKRect* param1, SKPathDirection param2);

		// void sk_path_add_path(sk_path_t* cpath, sk_path_t* other, sk_path_add_mode_t add_mode)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_path (sk_path_t cpath, sk_path_t other, SKPathAddMode add_mode);

		// void sk_path_add_path_matrix(sk_path_t* cpath, sk_path_t* other, sk_matrix_t* matrix, sk_path_add_mode_t add_mode)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_path_matrix (sk_path_t cpath, sk_path_t other, SKMatrix* matrix, SKPathAddMode add_mode);

		// void sk_path_add_path_offset(sk_path_t* cpath, sk_path_t* other, float dx, float dy, sk_path_add_mode_t add_mode)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_path_offset (sk_path_t cpath, sk_path_t other, Single dx, Single dy, SKPathAddMode add_mode);

		// void sk_path_add_path_reverse(sk_path_t* cpath, sk_path_t* other)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_path_reverse (sk_path_t cpath, sk_path_t other);

		// void sk_path_add_poly(sk_path_t* cpath, const sk_point_t* points, int count, bool close)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_poly (sk_path_t cpath, SKPoint* points, Int32 count, [MarshalAs (UnmanagedType.I1)] bool close);

		// void sk_path_add_rect(sk_path_t*, const sk_rect_t*, sk_path_direction_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_rect (sk_path_t param0, SKRect* param1, SKPathDirection param2);

		// void sk_path_add_rect_start(sk_path_t* cpath, const sk_rect_t* crect, sk_path_direction_t cdir, uint32_t startIndex)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_rect_start (sk_path_t cpath, SKRect* crect, SKPathDirection cdir, UInt32 startIndex);

		// void sk_path_add_rounded_rect(sk_path_t*, const sk_rect_t*, float, float, sk_path_direction_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_rounded_rect (sk_path_t param0, SKRect* param1, Single param2, Single param3, SKPathDirection param4);

		// void sk_path_add_rrect(sk_path_t*, const sk_rrect_t*, sk_path_direction_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_rrect (sk_path_t param0, sk_rrect_t param1, SKPathDirection param2);

		// void sk_path_add_rrect_start(sk_path_t*, const sk_rrect_t*, sk_path_direction_t, uint32_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_rrect_start (sk_path_t param0, sk_rrect_t param1, SKPathDirection param2, UInt32 param3);

		// void sk_path_arc_to(sk_path_t*, float rx, float ry, float xAxisRotate, sk_path_arc_size_t largeArc, sk_path_direction_t sweep, float x, float y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_arc_to (sk_path_t param0, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y);

		// void sk_path_arc_to_with_oval(sk_path_t*, const sk_rect_t* oval, float startAngle, float sweepAngle, bool forceMoveTo)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_arc_to_with_oval (sk_path_t param0, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool forceMoveTo);

		// void sk_path_arc_to_with_points(sk_path_t*, float x1, float y1, float x2, float y2, float radius)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_arc_to_with_points (sk_path_t param0, Single x1, Single y1, Single x2, Single y2, Single radius);

		// sk_path_t* sk_path_clone(const sk_path_t* cpath)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_t sk_path_clone (sk_path_t cpath);

		// void sk_path_close(sk_path_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_close (sk_path_t param0);

		// void sk_path_compute_tight_bounds(const sk_path_t*, sk_rect_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_compute_tight_bounds (sk_path_t param0, SKRect* param1);

		// void sk_path_conic_to(sk_path_t*, float x0, float y0, float x1, float y1, float w)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_conic_to (sk_path_t param0, Single x0, Single y0, Single x1, Single y1, Single w);

		// bool sk_path_contains(const sk_path_t* cpath, float x, float y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_contains (sk_path_t cpath, Single x, Single y);

		// int sk_path_convert_conic_to_quads(const sk_point_t* p0, const sk_point_t* p1, const sk_point_t* p2, float w, sk_point_t* pts, int pow2)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_convert_conic_to_quads (SKPoint* p0, SKPoint* p1, SKPoint* p2, Single w, SKPoint* pts, Int32 pow2);

		// int sk_path_count_points(const sk_path_t* cpath)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_count_points (sk_path_t cpath);

		// int sk_path_count_verbs(const sk_path_t* cpath)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_count_verbs (sk_path_t cpath);

		// sk_path_iterator_t* sk_path_create_iter(sk_path_t* cpath, int forceClose)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_iterator_t sk_path_create_iter (sk_path_t cpath, Int32 forceClose);

		// sk_path_rawiterator_t* sk_path_create_rawiter(sk_path_t* cpath)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_rawiterator_t sk_path_create_rawiter (sk_path_t cpath);

		// void sk_path_cubic_to(sk_path_t*, float x0, float y0, float x1, float y1, float x2, float y2)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_cubic_to (sk_path_t param0, Single x0, Single y0, Single x1, Single y1, Single x2, Single y2);

		// void sk_path_delete(sk_path_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_delete (sk_path_t param0);

		// void sk_path_get_bounds(const sk_path_t*, sk_rect_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_get_bounds (sk_path_t param0, SKRect* param1);

		// sk_path_convexity_t sk_path_get_convexity(const sk_path_t* cpath)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPathConvexity sk_path_get_convexity (sk_path_t cpath);

		// sk_path_filltype_t sk_path_get_filltype(sk_path_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPathFillType sk_path_get_filltype (sk_path_t param0);

		// bool sk_path_get_last_point(const sk_path_t* cpath, sk_point_t* point)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_get_last_point (sk_path_t cpath, SKPoint* point);

		// void sk_path_get_point(const sk_path_t* cpath, int index, sk_point_t* point)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_get_point (sk_path_t cpath, Int32 index, SKPoint* point);

		// int sk_path_get_points(const sk_path_t* cpath, sk_point_t* points, int max)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_get_points (sk_path_t cpath, SKPoint* points, Int32 max);

		// uint32_t sk_path_get_segment_masks(sk_path_t* cpath)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_path_get_segment_masks (sk_path_t cpath);

		// bool sk_path_is_line(sk_path_t* cpath, sk_point_t[2] line = 2)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_is_line (sk_path_t cpath, SKPoint* line);

		// bool sk_path_is_oval(sk_path_t* cpath, sk_rect_t* bounds)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_is_oval (sk_path_t cpath, SKRect* bounds);

		// bool sk_path_is_rect(sk_path_t* cpath, sk_rect_t* rect, bool* isClosed, sk_path_direction_t* direction)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_is_rect (sk_path_t cpath, SKRect* rect, Byte* isClosed, SKPathDirection* direction);

		// bool sk_path_is_rrect(sk_path_t* cpath, sk_rrect_t* bounds)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_is_rrect (sk_path_t cpath, sk_rrect_t bounds);

		// float sk_path_iter_conic_weight(sk_path_iterator_t* iterator)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_path_iter_conic_weight (sk_path_iterator_t iterator);

		// void sk_path_iter_destroy(sk_path_iterator_t* iterator)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_iter_destroy (sk_path_iterator_t iterator);

		// int sk_path_iter_is_close_line(sk_path_iterator_t* iterator)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_iter_is_close_line (sk_path_iterator_t iterator);

		// int sk_path_iter_is_closed_contour(sk_path_iterator_t* iterator)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_iter_is_closed_contour (sk_path_iterator_t iterator);

		// sk_path_verb_t sk_path_iter_next(sk_path_iterator_t* iterator, sk_point_t[4] points = 4, int doConsumeDegenerates, int exact)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPathVerb sk_path_iter_next (sk_path_iterator_t iterator, SKPoint* points, Int32 doConsumeDegenerates, Int32 exact);

		// void sk_path_line_to(sk_path_t*, float x, float y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_line_to (sk_path_t param0, Single x, Single y);

		// void sk_path_move_to(sk_path_t*, float x, float y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_move_to (sk_path_t param0, Single x, Single y);

		// sk_path_t* sk_path_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_t sk_path_new ();

		// bool sk_path_parse_svg_string(sk_path_t* cpath, const char* str)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_parse_svg_string (sk_path_t cpath, [MarshalAs (UnmanagedType.LPStr)] String str);

		// void sk_path_quad_to(sk_path_t*, float x0, float y0, float x1, float y1)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_quad_to (sk_path_t param0, Single x0, Single y0, Single x1, Single y1);

		// void sk_path_rarc_to(sk_path_t*, float rx, float ry, float xAxisRotate, sk_path_arc_size_t largeArc, sk_path_direction_t sweep, float x, float y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rarc_to (sk_path_t param0, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y);

		// float sk_path_rawiter_conic_weight(sk_path_rawiterator_t* iterator)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_path_rawiter_conic_weight (sk_path_rawiterator_t iterator);

		// void sk_path_rawiter_destroy(sk_path_rawiterator_t* iterator)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rawiter_destroy (sk_path_rawiterator_t iterator);

		// sk_path_verb_t sk_path_rawiter_next(sk_path_rawiterator_t* iterator, sk_point_t[4] points = 4)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPathVerb sk_path_rawiter_next (sk_path_rawiterator_t iterator, SKPoint* points);

		// sk_path_verb_t sk_path_rawiter_peek(sk_path_rawiterator_t* iterator)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPathVerb sk_path_rawiter_peek (sk_path_rawiterator_t iterator);

		// void sk_path_rconic_to(sk_path_t*, float dx0, float dy0, float dx1, float dy1, float w)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rconic_to (sk_path_t param0, Single dx0, Single dy0, Single dx1, Single dy1, Single w);

		// void sk_path_rcubic_to(sk_path_t*, float dx0, float dy0, float dx1, float dy1, float dx2, float dy2)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rcubic_to (sk_path_t param0, Single dx0, Single dy0, Single dx1, Single dy1, Single dx2, Single dy2);

		// void sk_path_reset(sk_path_t* cpath)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_reset (sk_path_t cpath);

		// void sk_path_rewind(sk_path_t* cpath)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rewind (sk_path_t cpath);

		// void sk_path_rline_to(sk_path_t*, float dx, float yd)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rline_to (sk_path_t param0, Single dx, Single yd);

		// void sk_path_rmove_to(sk_path_t*, float dx, float dy)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rmove_to (sk_path_t param0, Single dx, Single dy);

		// void sk_path_rquad_to(sk_path_t*, float dx0, float dy0, float dx1, float dy1)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rquad_to (sk_path_t param0, Single dx0, Single dy0, Single dx1, Single dy1);

		// void sk_path_set_convexity(sk_path_t* cpath, sk_path_convexity_t convexity)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_set_convexity (sk_path_t cpath, SKPathConvexity convexity);

		// void sk_path_set_filltype(sk_path_t*, sk_path_filltype_t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_set_filltype (sk_path_t param0, SKPathFillType param1);

		// void sk_path_to_svg_string(const sk_path_t* cpath, sk_string_t* str)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_to_svg_string (sk_path_t cpath, sk_string_t str);

		// void sk_path_transform(sk_path_t* cpath, const sk_matrix_t* cmatrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_transform (sk_path_t cpath, SKMatrix* cmatrix);

		// void sk_path_transform_to_dest(sk_path_t* cpath, const sk_matrix_t* cmatrix, sk_path_t* destination)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_transform_to_dest (sk_path_t cpath, SKMatrix* cmatrix, sk_path_t destination);

		// void sk_pathmeasure_destroy(sk_pathmeasure_t* pathMeasure)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathmeasure_destroy (sk_pathmeasure_t pathMeasure);

		// float sk_pathmeasure_get_length(sk_pathmeasure_t* pathMeasure)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_pathmeasure_get_length (sk_pathmeasure_t pathMeasure);

		// bool sk_pathmeasure_get_matrix(sk_pathmeasure_t* pathMeasure, float distance, sk_matrix_t* matrix, sk_pathmeasure_matrixflags_t flags)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_get_matrix (sk_pathmeasure_t pathMeasure, Single distance, SKMatrix* matrix, SKPathMeasureMatrixFlags flags);

		// bool sk_pathmeasure_get_pos_tan(sk_pathmeasure_t* pathMeasure, float distance, sk_point_t* position, sk_vector_t* tangent)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_get_pos_tan (sk_pathmeasure_t pathMeasure, Single distance, SKPoint* position, SKPoint* tangent);

		// bool sk_pathmeasure_get_segment(sk_pathmeasure_t* pathMeasure, float start, float stop, sk_path_t* dst, bool startWithMoveTo)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_get_segment (sk_pathmeasure_t pathMeasure, Single start, Single stop, sk_path_t dst, [MarshalAs (UnmanagedType.I1)] bool startWithMoveTo);

		// bool sk_pathmeasure_is_closed(sk_pathmeasure_t* pathMeasure)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_is_closed (sk_pathmeasure_t pathMeasure);

		// sk_pathmeasure_t* sk_pathmeasure_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_pathmeasure_t sk_pathmeasure_new ();

		// sk_pathmeasure_t* sk_pathmeasure_new_with_path(const sk_path_t* path, bool forceClosed, float resScale)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_pathmeasure_t sk_pathmeasure_new_with_path (sk_path_t path, [MarshalAs (UnmanagedType.I1)] bool forceClosed, Single resScale);

		// bool sk_pathmeasure_next_contour(sk_pathmeasure_t* pathMeasure)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_next_contour (sk_pathmeasure_t pathMeasure);

		// void sk_pathmeasure_set_path(sk_pathmeasure_t* pathMeasure, const sk_path_t* path, bool forceClosed)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathmeasure_set_path (sk_pathmeasure_t pathMeasure, sk_path_t path, [MarshalAs (UnmanagedType.I1)] bool forceClosed);

		// bool sk_pathop_op(const sk_path_t* one, const sk_path_t* two, sk_pathop_t op, sk_path_t* result)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathop_op (sk_path_t one, sk_path_t two, SKPathOp op, sk_path_t result);

		// bool sk_pathop_simplify(const sk_path_t* path, sk_path_t* result)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathop_simplify (sk_path_t path, sk_path_t result);

		// bool sk_pathop_tight_bounds(const sk_path_t* path, sk_rect_t* result)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathop_tight_bounds (sk_path_t path, SKRect* result);

		#endregion

		#region sk_patheffect.h

		// sk_path_effect_t* sk_path_effect_create_1d_path(const sk_path_t* path, float advance, float phase, sk_path_effect_1d_style_t style)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_1d_path (sk_path_t path, Single advance, Single phase, SKPath1DPathEffectStyle style);

		// sk_path_effect_t* sk_path_effect_create_2d_line(float width, const sk_matrix_t* matrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_2d_line (Single width, SKMatrix* matrix);

		// sk_path_effect_t* sk_path_effect_create_2d_path(const sk_matrix_t* matrix, const sk_path_t* path)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_2d_path (SKMatrix* matrix, sk_path_t path);

		// sk_path_effect_t* sk_path_effect_create_compose(sk_path_effect_t* outer, sk_path_effect_t* inner)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_compose (sk_path_effect_t outer, sk_path_effect_t inner);

		// sk_path_effect_t* sk_path_effect_create_corner(float radius)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_corner (Single radius);

		// sk_path_effect_t* sk_path_effect_create_dash(const float[-1] intervals, int count, float phase)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_dash (Single* intervals, Int32 count, Single phase);

		// sk_path_effect_t* sk_path_effect_create_discrete(float segLength, float deviation, uint32_t seedAssist)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_discrete (Single segLength, Single deviation, UInt32 seedAssist);

		// sk_path_effect_t* sk_path_effect_create_sum(sk_path_effect_t* first, sk_path_effect_t* second)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_sum (sk_path_effect_t first, sk_path_effect_t second);

		// sk_path_effect_t* sk_path_effect_create_trim(float start, float stop, sk_path_effect_trim_mode_t mode)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_trim (Single start, Single stop, SKTrimPathEffectMode mode);

		// void sk_path_effect_unref(sk_path_effect_t* t)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_effect_unref (sk_path_effect_t t);

		#endregion

		#region sk_picture.h

		// void sk_picture_get_cull_rect(sk_picture_t*, sk_rect_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_get_cull_rect (sk_picture_t param0, SKRect* param1);

		// sk_canvas_t* sk_picture_get_recording_canvas(sk_picture_recorder_t* crec)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_picture_get_recording_canvas (sk_picture_recorder_t crec);

		// uint32_t sk_picture_get_unique_id(sk_picture_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_picture_get_unique_id (sk_picture_t param0);

		// sk_canvas_t* sk_picture_recorder_begin_recording(sk_picture_recorder_t*, const sk_rect_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_picture_recorder_begin_recording (sk_picture_recorder_t param0, SKRect* param1);

		// void sk_picture_recorder_delete(sk_picture_recorder_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_recorder_delete (sk_picture_recorder_t param0);

		// sk_picture_t* sk_picture_recorder_end_recording(sk_picture_recorder_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_picture_t sk_picture_recorder_end_recording (sk_picture_recorder_t param0);

		// sk_drawable_t* sk_picture_recorder_end_recording_as_drawable(sk_picture_recorder_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_drawable_t sk_picture_recorder_end_recording_as_drawable (sk_picture_recorder_t param0);

		// sk_picture_recorder_t* sk_picture_recorder_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_picture_recorder_t sk_picture_recorder_new ();

		// void sk_picture_ref(sk_picture_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_ref (sk_picture_t param0);

		// void sk_picture_unref(sk_picture_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_unref (sk_picture_t param0);

		#endregion

		#region sk_pixmap.h

		// void sk_color_get_bit_shift(int* a, int* r, int* g, int* b)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_color_get_bit_shift (Int32* a, Int32* r, Int32* g, Int32* b);

		// sk_pmcolor_t sk_color_premultiply(const sk_color_t color)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_color_premultiply (UInt32 color);

		// void sk_color_premultiply_array(const sk_color_t* colors, int size, sk_pmcolor_t* pmcolors)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_color_premultiply_array (UInt32* colors, Int32 size, UInt32* pmcolors);

		// sk_color_t sk_color_unpremultiply(const sk_pmcolor_t pmcolor)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_color_unpremultiply (UInt32 pmcolor);

		// void sk_color_unpremultiply_array(const sk_pmcolor_t* pmcolors, int size, sk_color_t* colors)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_color_unpremultiply_array (UInt32* pmcolors, Int32 size, UInt32* colors);

		// bool sk_jpegencoder_encode(sk_wstream_t* dst, const sk_pixmap_t* src, const sk_jpegencoder_options_t* options)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_jpegencoder_encode (sk_wstream_t dst, sk_pixmap_t src, SKJpegEncoderOptions* options);

		// void sk_pixmap_destructor(sk_pixmap_t* cpixmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pixmap_destructor (sk_pixmap_t cpixmap);

		// bool sk_pixmap_encode_image(sk_wstream_t* dst, const sk_pixmap_t* src, sk_encoded_image_format_t encoder, int quality)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_encode_image (sk_wstream_t dst, sk_pixmap_t src, SKEncodedImageFormat encoder, Int32 quality);

		// bool sk_pixmap_erase_color(const sk_pixmap_t* cpixmap, sk_color_t color, const sk_irect_t* subset)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_erase_color (sk_pixmap_t cpixmap, UInt32 color, SKRectI* subset);

		// bool sk_pixmap_erase_color4f(const sk_pixmap_t* cpixmap, const sk_color4f_t* color, const sk_irect_t* subset)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_erase_color4f (sk_pixmap_t cpixmap, SKColorF* color, SKRectI* subset);

		// bool sk_pixmap_extract_subset(const sk_pixmap_t* cpixmap, sk_pixmap_t* result, const sk_irect_t* subset)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_extract_subset (sk_pixmap_t cpixmap, sk_pixmap_t result, SKRectI* subset);

		// void sk_pixmap_get_info(const sk_pixmap_t* cpixmap, sk_imageinfo_t* cinfo)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pixmap_get_info (sk_pixmap_t cpixmap, SKImageInfoNative* cinfo);

		// sk_color_t sk_pixmap_get_pixel_color(const sk_pixmap_t* cpixmap, int x, int y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_pixmap_get_pixel_color (sk_pixmap_t cpixmap, Int32 x, Int32 y);

		// const void* sk_pixmap_get_pixels(const sk_pixmap_t* cpixmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_pixmap_get_pixels (sk_pixmap_t cpixmap);

		// const void* sk_pixmap_get_pixels_with_xy(const sk_pixmap_t* cpixmap, int x, int y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_pixmap_get_pixels_with_xy (sk_pixmap_t cpixmap, Int32 x, Int32 y);

		// size_t sk_pixmap_get_row_bytes(const sk_pixmap_t* cpixmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_pixmap_get_row_bytes (sk_pixmap_t cpixmap);

		// void* sk_pixmap_get_writable_addr(const sk_pixmap_t* cpixmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_pixmap_get_writable_addr (sk_pixmap_t cpixmap);

		// sk_pixmap_t* sk_pixmap_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_pixmap_t sk_pixmap_new ();

		// sk_pixmap_t* sk_pixmap_new_with_params(const sk_imageinfo_t* cinfo, const void* addr, size_t rowBytes)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_pixmap_t sk_pixmap_new_with_params (SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes);

		// bool sk_pixmap_read_pixels(const sk_pixmap_t* cpixmap, const sk_imageinfo_t* dstInfo, void* dstPixels, size_t dstRowBytes, int srcX, int srcY, sk_transfer_function_behavior_t behavior)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_read_pixels (sk_pixmap_t cpixmap, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY, SKTransferFunctionBehavior behavior);

		// void sk_pixmap_reset(sk_pixmap_t* cpixmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pixmap_reset (sk_pixmap_t cpixmap);

		// void sk_pixmap_reset_with_params(sk_pixmap_t* cpixmap, const sk_imageinfo_t* cinfo, const void* addr, size_t rowBytes)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pixmap_reset_with_params (sk_pixmap_t cpixmap, SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes);

		// bool sk_pixmap_scale_pixels(const sk_pixmap_t* cpixmap, const sk_pixmap_t* dst, sk_filter_quality_t quality)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_scale_pixels (sk_pixmap_t cpixmap, sk_pixmap_t dst, SKFilterQuality quality);

		// bool sk_pngencoder_encode(sk_wstream_t* dst, const sk_pixmap_t* src, const sk_pngencoder_options_t* options)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pngencoder_encode (sk_wstream_t dst, sk_pixmap_t src, SKPngEncoderOptions* options);

		// void sk_swizzle_swap_rb(uint32_t* dest, const uint32_t* src, int count)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_swizzle_swap_rb (UInt32* dest, UInt32* src, Int32 count);

		// bool sk_webpencoder_encode(sk_wstream_t* dst, const sk_pixmap_t* src, const sk_webpencoder_options_t* options)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_webpencoder_encode (sk_wstream_t dst, sk_pixmap_t src, SKWebpEncoderOptions* options);

		#endregion

		#region sk_region.h

		// void sk_region_cliperator_delete(sk_region_cliperator_t* iter)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_cliperator_delete (sk_region_cliperator_t iter);

		// bool sk_region_cliperator_done(sk_region_cliperator_t* iter)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_cliperator_done (sk_region_cliperator_t iter);

		// sk_region_cliperator_t* sk_region_cliperator_new(const sk_region_t* region, const sk_irect_t* clip)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_region_cliperator_t sk_region_cliperator_new (sk_region_t region, SKRectI* clip);

		// void sk_region_cliperator_next(sk_region_cliperator_t* iter)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_cliperator_next (sk_region_cliperator_t iter);

		// void sk_region_cliperator_rect(const sk_region_cliperator_t* iter, sk_irect_t* rect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_cliperator_rect (sk_region_cliperator_t iter, SKRectI* rect);

		// bool sk_region_contains(const sk_region_t* r, const sk_region_t* region)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_contains (sk_region_t r, sk_region_t region);

		// bool sk_region_contains_point(const sk_region_t* r, int x, int y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_contains_point (sk_region_t r, Int32 x, Int32 y);

		// bool sk_region_contains_rect(const sk_region_t* r, const sk_irect_t* rect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_contains_rect (sk_region_t r, SKRectI* rect);

		// void sk_region_delete(sk_region_t* r)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_delete (sk_region_t r);

		// bool sk_region_get_boundary_path(const sk_region_t* r, sk_path_t* path)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_get_boundary_path (sk_region_t r, sk_path_t path);

		// void sk_region_get_bounds(const sk_region_t* r, sk_irect_t* rect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_get_bounds (sk_region_t r, SKRectI* rect);

		// bool sk_region_intersects(const sk_region_t* r, const sk_region_t* src)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_intersects (sk_region_t r, sk_region_t src);

		// bool sk_region_intersects_rect(const sk_region_t* r, const sk_irect_t* rect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_intersects_rect (sk_region_t r, SKRectI* rect);

		// bool sk_region_is_complex(const sk_region_t* r)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_is_complex (sk_region_t r);

		// bool sk_region_is_empty(const sk_region_t* r)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_is_empty (sk_region_t r);

		// bool sk_region_is_rect(const sk_region_t* r)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_is_rect (sk_region_t r);

		// void sk_region_iterator_delete(sk_region_iterator_t* iter)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_iterator_delete (sk_region_iterator_t iter);

		// bool sk_region_iterator_done(const sk_region_iterator_t* iter)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_iterator_done (sk_region_iterator_t iter);

		// sk_region_iterator_t* sk_region_iterator_new(const sk_region_t* region)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_region_iterator_t sk_region_iterator_new (sk_region_t region);

		// void sk_region_iterator_next(sk_region_iterator_t* iter)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_iterator_next (sk_region_iterator_t iter);

		// void sk_region_iterator_rect(const sk_region_iterator_t* iter, sk_irect_t* rect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_iterator_rect (sk_region_iterator_t iter, SKRectI* rect);

		// bool sk_region_iterator_rewind(sk_region_iterator_t* iter)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_iterator_rewind (sk_region_iterator_t iter);

		// sk_region_t* sk_region_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_region_t sk_region_new ();

		// bool sk_region_op(sk_region_t* r, const sk_region_t* region, sk_region_op_t op)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_op (sk_region_t r, sk_region_t region, SKRegionOperation op);

		// bool sk_region_op_rect(sk_region_t* r, const sk_irect_t* rect, sk_region_op_t op)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_op_rect (sk_region_t r, SKRectI* rect, SKRegionOperation op);

		// bool sk_region_quick_contains(const sk_region_t* r, const sk_irect_t* rect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_quick_contains (sk_region_t r, SKRectI* rect);

		// bool sk_region_quick_reject(const sk_region_t* r, const sk_region_t* region)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_quick_reject (sk_region_t r, sk_region_t region);

		// bool sk_region_quick_reject_rect(const sk_region_t* r, const sk_irect_t* rect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_quick_reject_rect (sk_region_t r, SKRectI* rect);

		// bool sk_region_set_empty(sk_region_t* r)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_empty (sk_region_t r);

		// bool sk_region_set_path(sk_region_t* r, const sk_path_t* t, const sk_region_t* clip)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_path (sk_region_t r, sk_path_t t, sk_region_t clip);

		// bool sk_region_set_rect(sk_region_t* r, const sk_irect_t* rect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_rect (sk_region_t r, SKRectI* rect);

		// bool sk_region_set_rects(sk_region_t* r, const sk_irect_t* rects, int count)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_rects (sk_region_t r, SKRectI* rects, Int32 count);

		// bool sk_region_set_region(sk_region_t* r, const sk_region_t* region)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_region (sk_region_t r, sk_region_t region);

		// void sk_region_spanerator_delete(sk_region_spanerator_t* iter)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_spanerator_delete (sk_region_spanerator_t iter);

		// sk_region_spanerator_t* sk_region_spanerator_new(const sk_region_t* region, int y, int left, int right)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_region_spanerator_t sk_region_spanerator_new (sk_region_t region, Int32 y, Int32 left, Int32 right);

		// bool sk_region_spanerator_next(sk_region_spanerator_t* iter, int* left, int* right)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_spanerator_next (sk_region_spanerator_t iter, Int32* left, Int32* right);

		// void sk_region_translate(sk_region_t* r, int x, int y)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_translate (sk_region_t r, Int32 x, Int32 y);

		#endregion

		#region sk_rrect.h

		// bool sk_rrect_contains(const sk_rrect_t* rrect, const sk_rect_t* rect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_rrect_contains (sk_rrect_t rrect, SKRect* rect);

		// void sk_rrect_delete(const sk_rrect_t* rrect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_delete (sk_rrect_t rrect);

		// float sk_rrect_get_height(const sk_rrect_t* rrect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_rrect_get_height (sk_rrect_t rrect);

		// void sk_rrect_get_radii(const sk_rrect_t* rrect, sk_rrect_corner_t corner, sk_vector_t* radii)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_get_radii (sk_rrect_t rrect, SKRoundRectCorner corner, SKPoint* radii);

		// void sk_rrect_get_rect(const sk_rrect_t* rrect, sk_rect_t* rect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_get_rect (sk_rrect_t rrect, SKRect* rect);

		// sk_rrect_type_t sk_rrect_get_type(const sk_rrect_t* rrect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKRoundRectType sk_rrect_get_type (sk_rrect_t rrect);

		// float sk_rrect_get_width(const sk_rrect_t* rrect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_rrect_get_width (sk_rrect_t rrect);

		// void sk_rrect_inset(sk_rrect_t* rrect, float dx, float dy)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_inset (sk_rrect_t rrect, Single dx, Single dy);

		// bool sk_rrect_is_valid(const sk_rrect_t* rrect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_rrect_is_valid (sk_rrect_t rrect);

		// sk_rrect_t* sk_rrect_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_rrect_t sk_rrect_new ();

		// sk_rrect_t* sk_rrect_new_copy(const sk_rrect_t* rrect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_rrect_t sk_rrect_new_copy (sk_rrect_t rrect);

		// void sk_rrect_offset(sk_rrect_t* rrect, float dx, float dy)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_offset (sk_rrect_t rrect, Single dx, Single dy);

		// void sk_rrect_outset(sk_rrect_t* rrect, float dx, float dy)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_outset (sk_rrect_t rrect, Single dx, Single dy);

		// void sk_rrect_set_empty(sk_rrect_t* rrect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_empty (sk_rrect_t rrect);

		// void sk_rrect_set_nine_patch(sk_rrect_t* rrect, const sk_rect_t* rect, float leftRad, float topRad, float rightRad, float bottomRad)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_nine_patch (sk_rrect_t rrect, SKRect* rect, Single leftRad, Single topRad, Single rightRad, Single bottomRad);

		// void sk_rrect_set_oval(sk_rrect_t* rrect, const sk_rect_t* rect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_oval (sk_rrect_t rrect, SKRect* rect);

		// void sk_rrect_set_rect(sk_rrect_t* rrect, const sk_rect_t* rect)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_rect (sk_rrect_t rrect, SKRect* rect);

		// void sk_rrect_set_rect_radii(sk_rrect_t* rrect, const sk_rect_t* rect, const sk_vector_t* radii)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_rect_radii (sk_rrect_t rrect, SKRect* rect, SKPoint* radii);

		// void sk_rrect_set_rect_xy(sk_rrect_t* rrect, const sk_rect_t* rect, float xRad, float yRad)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_rect_xy (sk_rrect_t rrect, SKRect* rect, Single xRad, Single yRad);

		// bool sk_rrect_transform(sk_rrect_t* rrect, const sk_matrix_t* matrix, sk_rrect_t* dest)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_rrect_transform (sk_rrect_t rrect, SKMatrix* matrix, sk_rrect_t dest);

		#endregion

		#region sk_shader.h

		// sk_shader_t* sk_shader_new_bitmap(const sk_bitmap_t* src, sk_shader_tilemode_t tmx, sk_shader_tilemode_t tmy, const sk_matrix_t* localMatrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_bitmap (sk_bitmap_t src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix* localMatrix);

		// sk_shader_t* sk_shader_new_color(sk_color_t color)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_color (UInt32 color);

		// sk_shader_t* sk_shader_new_color4f(const sk_color4f_t* color, const sk_colorspace_t* colorspace)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_color4f (SKColorF* color, sk_colorspace_t colorspace);

		// sk_shader_t* sk_shader_new_compose(const sk_shader_t* shaderA, const sk_shader_t* shaderB, sk_blendmode_t mode)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_compose (sk_shader_t shaderA, sk_shader_t shaderB, SKBlendMode mode);

		// sk_shader_t* sk_shader_new_empty()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_empty ();

		// sk_shader_t* sk_shader_new_linear_gradient(const sk_point_t[2] points = 2, const sk_color_t[-1] colors, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_linear_gradient (SKPoint* points, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

		// sk_shader_t* sk_shader_new_linear_gradient_color4f(const sk_point_t[2] points = 2, const sk_color4f_t* colors, const sk_colorspace_t* colorspace, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_linear_gradient_color4f (SKPoint* points, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

		// sk_shader_t* sk_shader_new_perlin_noise_fractal_noise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, const sk_isize_t* tileSize)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_perlin_noise_fractal_noise (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize);

		// sk_shader_t* sk_shader_new_perlin_noise_improved_noise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float z)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_perlin_noise_improved_noise (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single z);

		// sk_shader_t* sk_shader_new_perlin_noise_turbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, const sk_isize_t* tileSize)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_perlin_noise_turbulence (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize);

		// sk_shader_t* sk_shader_new_picture(sk_picture_t* src, sk_shader_tilemode_t tmx, sk_shader_tilemode_t tmy, const sk_matrix_t* localMatrix, const sk_rect_t* tile)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_picture (sk_picture_t src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix* localMatrix, SKRect* tile);

		// sk_shader_t* sk_shader_new_radial_gradient(const sk_point_t* center, float radius, const sk_color_t[-1] colors, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_radial_gradient (SKPoint* center, Single radius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

		// sk_shader_t* sk_shader_new_radial_gradient_color4f(const sk_point_t* center, float radius, const sk_color4f_t* colors, const sk_colorspace_t* colorspace, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_radial_gradient_color4f (SKPoint* center, Single radius, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

		// sk_shader_t* sk_shader_new_sweep_gradient(const sk_point_t* center, const sk_color_t[-1] colors, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, float startAngle, float endAngle, const sk_matrix_t* localMatrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_sweep_gradient (SKPoint* center, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix);

		// sk_shader_t* sk_shader_new_sweep_gradient_color4f(const sk_point_t* center, const sk_color4f_t* colors, const sk_colorspace_t* colorspace, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, float startAngle, float endAngle, const sk_matrix_t* localMatrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_sweep_gradient_color4f (SKPoint* center, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix);

		// sk_shader_t* sk_shader_new_two_point_conical_gradient(const sk_point_t* start, float startRadius, const sk_point_t* end, float endRadius, const sk_color_t[-1] colors, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_two_point_conical_gradient (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

		// sk_shader_t* sk_shader_new_two_point_conical_gradient_color4f(const sk_point_t* start, float startRadius, const sk_point_t* end, float endRadius, const sk_color4f_t* colors, const sk_colorspace_t* colorspace, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_two_point_conical_gradient_color4f (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

		// void sk_shader_ref(sk_shader_t* shader)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_shader_ref (sk_shader_t shader);

		// void sk_shader_unref(sk_shader_t* shader)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_shader_unref (sk_shader_t shader);

		// sk_shader_t* sk_shader_with_color_filter(const sk_shader_t* shader, const sk_colorfilter_t* filter)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_with_color_filter (sk_shader_t shader, sk_colorfilter_t filter);

		// sk_shader_t* sk_shader_with_local_matrix(const sk_shader_t* shader, const sk_matrix_t* localMatrix)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_with_local_matrix (sk_shader_t shader, SKMatrix* localMatrix);

		#endregion

		#region sk_stream.h

		// void sk_dynamicmemorywstream_copy_to(sk_wstream_dynamicmemorystream_t* cstream, void* data)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_dynamicmemorywstream_copy_to (sk_wstream_dynamicmemorystream_t cstream, void* data);

		// void sk_dynamicmemorywstream_destroy(sk_wstream_dynamicmemorystream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_dynamicmemorywstream_destroy (sk_wstream_dynamicmemorystream_t cstream);

		// sk_data_t* sk_dynamicmemorywstream_detach_as_data(sk_wstream_dynamicmemorystream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_dynamicmemorywstream_detach_as_data (sk_wstream_dynamicmemorystream_t cstream);

		// sk_stream_asset_t* sk_dynamicmemorywstream_detach_as_stream(sk_wstream_dynamicmemorystream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_asset_t sk_dynamicmemorywstream_detach_as_stream (sk_wstream_dynamicmemorystream_t cstream);

		// sk_wstream_dynamicmemorystream_t* sk_dynamicmemorywstream_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_wstream_dynamicmemorystream_t sk_dynamicmemorywstream_new ();

		// bool sk_dynamicmemorywstream_write_to_stream(sk_wstream_dynamicmemorystream_t* cstream, sk_wstream_t* dst)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_dynamicmemorywstream_write_to_stream (sk_wstream_dynamicmemorystream_t cstream, sk_wstream_t dst);

		// void sk_filestream_destroy(sk_stream_filestream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_filestream_destroy (sk_stream_filestream_t cstream);

		// bool sk_filestream_is_valid(sk_stream_filestream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_filestream_is_valid (sk_stream_filestream_t cstream);

		// sk_stream_filestream_t* sk_filestream_new(const char* path)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_filestream_t sk_filestream_new (/* char */ void* path);

		// void sk_filewstream_destroy(sk_wstream_filestream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_filewstream_destroy (sk_wstream_filestream_t cstream);

		// bool sk_filewstream_is_valid(sk_wstream_filestream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_filewstream_is_valid (sk_wstream_filestream_t cstream);

		// sk_wstream_filestream_t* sk_filewstream_new(const char* path)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_wstream_filestream_t sk_filewstream_new (/* char */ void* path);

		// void sk_memorystream_destroy(sk_stream_memorystream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_memorystream_destroy (sk_stream_memorystream_t cstream);

		// sk_stream_memorystream_t* sk_memorystream_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_memorystream_t sk_memorystream_new ();

		// sk_stream_memorystream_t* sk_memorystream_new_with_data(const void* data, size_t length, bool copyData)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_memorystream_t sk_memorystream_new_with_data (void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData);

		// sk_stream_memorystream_t* sk_memorystream_new_with_length(size_t length)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_memorystream_t sk_memorystream_new_with_length (/* size_t */ IntPtr length);

		// sk_stream_memorystream_t* sk_memorystream_new_with_skdata(sk_data_t* data)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_memorystream_t sk_memorystream_new_with_skdata (sk_data_t data);

		// void sk_memorystream_set_memory(sk_stream_memorystream_t* cmemorystream, const void* data, size_t length, bool copyData)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_memorystream_set_memory (sk_stream_memorystream_t cmemorystream, void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData);

		// void sk_stream_asset_destroy(sk_stream_asset_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_stream_asset_destroy (sk_stream_asset_t cstream);

		// void sk_stream_destroy(sk_stream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_stream_destroy (sk_stream_t cstream);

		// sk_stream_t* sk_stream_duplicate(sk_stream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_t sk_stream_duplicate (sk_stream_t cstream);

		// sk_stream_t* sk_stream_fork(sk_stream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_t sk_stream_fork (sk_stream_t cstream);

		// size_t sk_stream_get_length(sk_stream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_get_length (sk_stream_t cstream);

		// const void* sk_stream_get_memory_base(sk_stream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_stream_get_memory_base (sk_stream_t cstream);

		// size_t sk_stream_get_position(sk_stream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_get_position (sk_stream_t cstream);

		// bool sk_stream_has_length(sk_stream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_has_length (sk_stream_t cstream);

		// bool sk_stream_has_position(sk_stream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_has_position (sk_stream_t cstream);

		// bool sk_stream_is_at_end(sk_stream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_is_at_end (sk_stream_t cstream);

		// bool sk_stream_move(sk_stream_t* cstream, int offset)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_move (sk_stream_t cstream, Int32 offset);

		// size_t sk_stream_peek(sk_stream_t* cstream, void* buffer, size_t size)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_peek (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size);

		// size_t sk_stream_read(sk_stream_t* cstream, void* buffer, size_t size)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_read (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size);

		// bool sk_stream_read_bool(sk_stream_t* cstream, bool* buffer)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_bool (sk_stream_t cstream, Byte* buffer);

		// bool sk_stream_read_s16(sk_stream_t* cstream, int16_t* buffer)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_s16 (sk_stream_t cstream, Int16* buffer);

		// bool sk_stream_read_s32(sk_stream_t* cstream, int32_t* buffer)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_s32 (sk_stream_t cstream, Int32* buffer);

		// bool sk_stream_read_s8(sk_stream_t* cstream, int8_t* buffer)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_s8 (sk_stream_t cstream, SByte* buffer);

		// bool sk_stream_read_u16(sk_stream_t* cstream, uint16_t* buffer)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_u16 (sk_stream_t cstream, UInt16* buffer);

		// bool sk_stream_read_u32(sk_stream_t* cstream, uint32_t* buffer)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_u32 (sk_stream_t cstream, UInt32* buffer);

		// bool sk_stream_read_u8(sk_stream_t* cstream, uint8_t* buffer)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_u8 (sk_stream_t cstream, Byte* buffer);

		// bool sk_stream_rewind(sk_stream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_rewind (sk_stream_t cstream);

		// bool sk_stream_seek(sk_stream_t* cstream, size_t position)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_seek (sk_stream_t cstream, /* size_t */ IntPtr position);

		// size_t sk_stream_skip(sk_stream_t* cstream, size_t size)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_skip (sk_stream_t cstream, /* size_t */ IntPtr size);

		// size_t sk_wstream_bytes_written(sk_wstream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_wstream_bytes_written (sk_wstream_t cstream);

		// void sk_wstream_flush(sk_wstream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_wstream_flush (sk_wstream_t cstream);

		// int sk_wstream_get_size_of_packed_uint(size_t value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_wstream_get_size_of_packed_uint (/* size_t */ IntPtr value);

		// bool sk_wstream_newline(sk_wstream_t* cstream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_newline (sk_wstream_t cstream);

		// bool sk_wstream_write(sk_wstream_t* cstream, const void* buffer, size_t size)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write (sk_wstream_t cstream, void* buffer, /* size_t */ IntPtr size);

		// bool sk_wstream_write_16(sk_wstream_t* cstream, uint16_t value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_16 (sk_wstream_t cstream, UInt16 value);

		// bool sk_wstream_write_32(sk_wstream_t* cstream, uint32_t value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_32 (sk_wstream_t cstream, UInt32 value);

		// bool sk_wstream_write_8(sk_wstream_t* cstream, uint8_t value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_8 (sk_wstream_t cstream, Byte value);

		// bool sk_wstream_write_bigdec_as_text(sk_wstream_t* cstream, int64_t value, int minDigits)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_bigdec_as_text (sk_wstream_t cstream, Int64 value, Int32 minDigits);

		// bool sk_wstream_write_bool(sk_wstream_t* cstream, bool value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_bool (sk_wstream_t cstream, [MarshalAs (UnmanagedType.I1)] bool value);

		// bool sk_wstream_write_dec_as_text(sk_wstream_t* cstream, int32_t value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_dec_as_text (sk_wstream_t cstream, Int32 value);

		// bool sk_wstream_write_hex_as_text(sk_wstream_t* cstream, uint32_t value, int minDigits)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_hex_as_text (sk_wstream_t cstream, UInt32 value, Int32 minDigits);

		// bool sk_wstream_write_packed_uint(sk_wstream_t* cstream, size_t value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_packed_uint (sk_wstream_t cstream, /* size_t */ IntPtr value);

		// bool sk_wstream_write_scalar(sk_wstream_t* cstream, float value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_scalar (sk_wstream_t cstream, Single value);

		// bool sk_wstream_write_scalar_as_text(sk_wstream_t* cstream, float value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_scalar_as_text (sk_wstream_t cstream, Single value);

		// bool sk_wstream_write_stream(sk_wstream_t* cstream, sk_stream_t* input, size_t length)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_stream (sk_wstream_t cstream, sk_stream_t input, /* size_t */ IntPtr length);

		// bool sk_wstream_write_text(sk_wstream_t* cstream, const char* value)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_text (sk_wstream_t cstream, [MarshalAs (UnmanagedType.LPStr)] String value);

		#endregion

		#region sk_string.h

		// void sk_string_destructor(const sk_string_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_string_destructor (sk_string_t param0);

		// const char* sk_string_get_c_str(const sk_string_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* sk_string_get_c_str (sk_string_t param0);

		// size_t sk_string_get_size(const sk_string_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_string_get_size (sk_string_t param0);

		// sk_string_t* sk_string_new_empty()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_string_t sk_string_new_empty ();

		// sk_string_t* sk_string_new_with_copy(const char* src, size_t length)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_string_t sk_string_new_with_copy (/* char */ void* src, /* size_t */ IntPtr length);

		#endregion

		#region sk_surface.h

		// void sk_surface_draw(sk_surface_t* surface, sk_canvas_t* canvas, float x, float y, const sk_paint_t* paint)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_surface_draw (sk_surface_t surface, sk_canvas_t canvas, Single x, Single y, sk_paint_t paint);

		// sk_canvas_t* sk_surface_get_canvas(sk_surface_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_surface_get_canvas (sk_surface_t param0);

		// const sk_surfaceprops_t* sk_surface_get_props(sk_surface_t* surface)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surfaceprops_t sk_surface_get_props (sk_surface_t surface);

		// sk_surface_t* sk_surface_new_backend_render_target(gr_context_t* context, const gr_backendrendertarget_t* target, gr_surfaceorigin_t origin, sk_colortype_t colorType, sk_colorspace_t* colorspace, const sk_surfaceprops_t* props)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_backend_render_target (gr_context_t context, gr_backendrendertarget_t target, GRSurfaceOrigin origin, SKColorType colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);

		// sk_surface_t* sk_surface_new_backend_texture(gr_context_t* context, const gr_backendtexture_t* texture, gr_surfaceorigin_t origin, int samples, sk_colortype_t colorType, sk_colorspace_t* colorspace, const sk_surfaceprops_t* props)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_backend_texture (gr_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, Int32 samples, SKColorType colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);

		// sk_surface_t* sk_surface_new_backend_texture_as_render_target(gr_context_t* context, const gr_backendtexture_t* texture, gr_surfaceorigin_t origin, int samples, sk_colortype_t colorType, sk_colorspace_t* colorspace, const sk_surfaceprops_t* props)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_backend_texture_as_render_target (gr_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, Int32 samples, SKColorType colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);

		// sk_image_t* sk_surface_new_image_snapshot(sk_surface_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_surface_new_image_snapshot (sk_surface_t param0);

		// sk_surface_t* sk_surface_new_null(int width, int height)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_null (Int32 width, Int32 height);

		// sk_surface_t* sk_surface_new_raster(const sk_imageinfo_t*, size_t rowBytes, const sk_surfaceprops_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_raster (SKImageInfoNative* param0, /* size_t */ IntPtr rowBytes, sk_surfaceprops_t param2);

		// sk_surface_t* sk_surface_new_raster_direct(const sk_imageinfo_t*, void* pixels, size_t rowBytes, const sk_surface_raster_release_proc releaseProc, void* context, const sk_surfaceprops_t* props)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_raster_direct (SKImageInfoNative* param0, void* pixels, /* size_t */ IntPtr rowBytes, SKSurfaceRasterReleaseProxyDelegate releaseProc, void* context, sk_surfaceprops_t props);

		// sk_surface_t* sk_surface_new_render_target(gr_context_t* context, bool budgeted, const sk_imageinfo_t* cinfo, int sampleCount, gr_surfaceorigin_t origin, const sk_surfaceprops_t* props, bool shouldCreateWithMips)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_render_target (gr_context_t context, [MarshalAs (UnmanagedType.I1)] bool budgeted, SKImageInfoNative* cinfo, Int32 sampleCount, GRSurfaceOrigin origin, sk_surfaceprops_t props, [MarshalAs (UnmanagedType.I1)] bool shouldCreateWithMips);

		// bool sk_surface_peek_pixels(sk_surface_t* surface, sk_pixmap_t* pixmap)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_surface_peek_pixels (sk_surface_t surface, sk_pixmap_t pixmap);

		// bool sk_surface_read_pixels(sk_surface_t* surface, sk_imageinfo_t* dstInfo, void* dstPixels, size_t dstRowBytes, int srcX, int srcY)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_surface_read_pixels (sk_surface_t surface, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY);

		// void sk_surface_unref(sk_surface_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_surface_unref (sk_surface_t param0);

		// void sk_surfaceprops_delete(sk_surfaceprops_t* props)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_surfaceprops_delete (sk_surfaceprops_t props);

		// uint32_t sk_surfaceprops_get_flags(sk_surfaceprops_t* props)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_surfaceprops_get_flags (sk_surfaceprops_t props);

		// sk_pixelgeometry_t sk_surfaceprops_get_pixel_geometry(sk_surfaceprops_t* props)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPixelGeometry sk_surfaceprops_get_pixel_geometry (sk_surfaceprops_t props);

		// sk_surfaceprops_t* sk_surfaceprops_new(uint32_t flags, sk_pixelgeometry_t geometry)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surfaceprops_t sk_surfaceprops_new (UInt32 flags, SKPixelGeometry geometry);

		#endregion

		#region sk_svg.h

		// sk_canvas_t* sk_svgcanvas_create_with_stream(const sk_rect_t* bounds, sk_wstream_t* stream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_svgcanvas_create_with_stream (SKRect* bounds, sk_wstream_t stream);

		// sk_canvas_t* sk_svgcanvas_create_with_writer(const sk_rect_t* bounds, sk_xmlwriter_t* writer)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_svgcanvas_create_with_writer (SKRect* bounds, sk_xmlwriter_t writer);

		#endregion

		#region sk_textblob.h

		// void sk_textblob_builder_alloc_run_text(sk_textblob_builder_t* builder, const sk_paint_t* font, int count, float x, float y, int textByteCount, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_text (sk_textblob_builder_t builder, sk_paint_t font, Int32 count, Single x, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);

		// void sk_textblob_builder_alloc_run_text_pos(sk_textblob_builder_t* builder, const sk_paint_t* font, int count, int textByteCount, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_text_pos (sk_textblob_builder_t builder, sk_paint_t font, Int32 count, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);

		// void sk_textblob_builder_alloc_run_text_pos_h(sk_textblob_builder_t* builder, const sk_paint_t* font, int count, float y, int textByteCount, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_text_pos_h (sk_textblob_builder_t builder, sk_paint_t font, Int32 count, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);

		// void sk_textblob_builder_delete(sk_textblob_builder_t* builder)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_delete (sk_textblob_builder_t builder);

		// sk_textblob_t* sk_textblob_builder_make(sk_textblob_builder_t* builder)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_textblob_t sk_textblob_builder_make (sk_textblob_builder_t builder);

		// sk_textblob_builder_t* sk_textblob_builder_new()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_textblob_builder_t sk_textblob_builder_new ();

		// void sk_textblob_get_bounds(const sk_textblob_t* blob, sk_rect_t* bounds)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_get_bounds (sk_textblob_t blob, SKRect* bounds);

		// uint32_t sk_textblob_get_unique_id(const sk_textblob_t* blob)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_textblob_get_unique_id (sk_textblob_t blob);

		// void sk_textblob_ref(const sk_textblob_t* blob)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_ref (sk_textblob_t blob);

		// void sk_textblob_unref(const sk_textblob_t* blob)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_unref (sk_textblob_t blob);

		#endregion

		#region sk_typeface.h

		// int sk_fontmgr_count_families(sk_fontmgr_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_fontmgr_count_families (sk_fontmgr_t param0);

		// sk_fontmgr_t* sk_fontmgr_create_default()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_fontmgr_t sk_fontmgr_create_default ();

		// sk_typeface_t* sk_fontmgr_create_from_data(sk_fontmgr_t*, sk_data_t* data, int index)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontmgr_create_from_data (sk_fontmgr_t param0, sk_data_t data, Int32 index);

		// sk_typeface_t* sk_fontmgr_create_from_file(sk_fontmgr_t*, const char* path, int index)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontmgr_create_from_file (sk_fontmgr_t param0, /* char */ void* path, Int32 index);

		// sk_typeface_t* sk_fontmgr_create_from_stream(sk_fontmgr_t*, sk_stream_asset_t* stream, int index)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontmgr_create_from_stream (sk_fontmgr_t param0, sk_stream_asset_t stream, Int32 index);

		// sk_fontstyleset_t* sk_fontmgr_create_styleset(sk_fontmgr_t*, int index)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_fontstyleset_t sk_fontmgr_create_styleset (sk_fontmgr_t param0, Int32 index);

		// void sk_fontmgr_get_family_name(sk_fontmgr_t*, int index, sk_string_t* familyName)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontmgr_get_family_name (sk_fontmgr_t param0, Int32 index, sk_string_t familyName);

		// sk_typeface_t* sk_fontmgr_match_face_style(sk_fontmgr_t*, const sk_typeface_t* face, sk_fontstyle_t* style)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontmgr_match_face_style (sk_fontmgr_t param0, sk_typeface_t face, sk_fontstyle_t style);

		// sk_fontstyleset_t* sk_fontmgr_match_family(sk_fontmgr_t*, const char* familyName)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_fontstyleset_t sk_fontmgr_match_family (sk_fontmgr_t param0, [MarshalAs (UnmanagedType.LPStr)] String familyName);

		// sk_typeface_t* sk_fontmgr_match_family_style(sk_fontmgr_t*, const char* familyName, sk_fontstyle_t* style)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontmgr_match_family_style (sk_fontmgr_t param0, [MarshalAs (UnmanagedType.LPStr)] String familyName, sk_fontstyle_t style);

		// sk_typeface_t* sk_fontmgr_match_family_style_character(sk_fontmgr_t*, const char* familyName, sk_fontstyle_t* style, const char** bcp47, int bcp47Count, int32_t character)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontmgr_match_family_style_character (sk_fontmgr_t param0, [MarshalAs (UnmanagedType.LPStr)] String familyName, sk_fontstyle_t style, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] bcp47, Int32 bcp47Count, Int32 character);

		// sk_fontmgr_t* sk_fontmgr_ref_default()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_fontmgr_t sk_fontmgr_ref_default ();

		// void sk_fontmgr_unref(sk_fontmgr_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontmgr_unref (sk_fontmgr_t param0);

		// void sk_fontstyle_delete(sk_fontstyle_t* fs)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontstyle_delete (sk_fontstyle_t fs);

		// sk_font_style_slant_t sk_fontstyle_get_slant(const sk_fontstyle_t* fs)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKFontStyleSlant sk_fontstyle_get_slant (sk_fontstyle_t fs);

		// int sk_fontstyle_get_weight(const sk_fontstyle_t* fs)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_fontstyle_get_weight (sk_fontstyle_t fs);

		// int sk_fontstyle_get_width(const sk_fontstyle_t* fs)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_fontstyle_get_width (sk_fontstyle_t fs);

		// sk_fontstyle_t* sk_fontstyle_new(int weight, int width, sk_font_style_slant_t slant)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_fontstyle_t sk_fontstyle_new (Int32 weight, Int32 width, SKFontStyleSlant slant);

		// sk_fontstyleset_t* sk_fontstyleset_create_empty()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_fontstyleset_t sk_fontstyleset_create_empty ();

		// sk_typeface_t* sk_fontstyleset_create_typeface(sk_fontstyleset_t* fss, int index)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontstyleset_create_typeface (sk_fontstyleset_t fss, Int32 index);

		// int sk_fontstyleset_get_count(sk_fontstyleset_t* fss)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_fontstyleset_get_count (sk_fontstyleset_t fss);

		// void sk_fontstyleset_get_style(sk_fontstyleset_t* fss, int index, sk_fontstyle_t* fs, sk_string_t* style)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontstyleset_get_style (sk_fontstyleset_t fss, Int32 index, sk_fontstyle_t fs, sk_string_t style);

		// sk_typeface_t* sk_fontstyleset_match_style(sk_fontstyleset_t* fss, sk_fontstyle_t* style)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontstyleset_match_style (sk_fontstyleset_t fss, sk_fontstyle_t style);

		// void sk_fontstyleset_unref(sk_fontstyleset_t* fss)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontstyleset_unref (sk_fontstyleset_t fss);

		// int sk_typeface_chars_to_glyphs(sk_typeface_t* typeface, const char* chars, sk_encoding_t encoding, uint16_t[-1] glyphs, int glyphCount)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_chars_to_glyphs (sk_typeface_t typeface, /* char */ void* chars, SKEncoding encoding, UInt16* glyphs, Int32 glyphCount);

		// int sk_typeface_count_glyphs(sk_typeface_t* typeface)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_count_glyphs (sk_typeface_t typeface);

		// int sk_typeface_count_tables(sk_typeface_t* typeface)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_count_tables (sk_typeface_t typeface);

		// sk_typeface_t* sk_typeface_create_default()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_typeface_create_default ();

		// sk_typeface_t* sk_typeface_create_from_file(const char* path, int index)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_typeface_create_from_file (/* char */ void* path, Int32 index);

		// sk_typeface_t* sk_typeface_create_from_name_with_font_style(const char* familyName, sk_fontstyle_t* style)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_typeface_create_from_name_with_font_style ([MarshalAs (UnmanagedType.LPStr)] String familyName, sk_fontstyle_t style);

		// sk_typeface_t* sk_typeface_create_from_stream(sk_stream_asset_t* stream, int index)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_typeface_create_from_stream (sk_stream_asset_t stream, Int32 index);

		// sk_string_t* sk_typeface_get_family_name(sk_typeface_t* typeface)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_string_t sk_typeface_get_family_name (sk_typeface_t typeface);

		// sk_font_style_slant_t sk_typeface_get_font_slant(sk_typeface_t* typeface)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKFontStyleSlant sk_typeface_get_font_slant (sk_typeface_t typeface);

		// int sk_typeface_get_font_weight(sk_typeface_t* typeface)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_get_font_weight (sk_typeface_t typeface);

		// int sk_typeface_get_font_width(sk_typeface_t* typeface)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_get_font_width (sk_typeface_t typeface);

		// sk_fontstyle_t* sk_typeface_get_fontstyle(sk_typeface_t* typeface)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_fontstyle_t sk_typeface_get_fontstyle (sk_typeface_t typeface);

		// bool sk_typeface_get_kerning_pair_adjustments(sk_typeface_t* typeface, const uint16_t* glyphs, int count, int32_t* adjustments)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_typeface_get_kerning_pair_adjustments (sk_typeface_t typeface, UInt16* glyphs, Int32 count, Int32* adjustments);

		// size_t sk_typeface_get_table_data(sk_typeface_t* typeface, sk_font_table_tag_t tag, size_t offset, size_t length, void* data)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_typeface_get_table_data (sk_typeface_t typeface, UInt32 tag, /* size_t */ IntPtr offset, /* size_t */ IntPtr length, void* data);

		// size_t sk_typeface_get_table_size(sk_typeface_t* typeface, sk_font_table_tag_t tag)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_typeface_get_table_size (sk_typeface_t typeface, UInt32 tag);

		// int sk_typeface_get_table_tags(sk_typeface_t* typeface, sk_font_table_tag_t[-1] tags)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_get_table_tags (sk_typeface_t typeface, UInt32* tags);

		// int sk_typeface_get_units_per_em(sk_typeface_t* typeface)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_get_units_per_em (sk_typeface_t typeface);

		// bool sk_typeface_is_fixed_pitch(sk_typeface_t* typeface)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_typeface_is_fixed_pitch (sk_typeface_t typeface);

		// sk_stream_asset_t* sk_typeface_open_stream(sk_typeface_t* typeface, int* ttcIndex)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_asset_t sk_typeface_open_stream (sk_typeface_t typeface, Int32* ttcIndex);

		// sk_typeface_t* sk_typeface_ref_default()
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_typeface_ref_default ();

		// void sk_typeface_unref(sk_typeface_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_typeface_unref (sk_typeface_t param0);

		#endregion

		#region sk_vertices.h

		// sk_vertices_t* sk_vertices_make_copy(sk_vertices_vertex_mode_t vmode, int vertexCount, const sk_point_t* positions, const sk_point_t* texs, const sk_color_t* colors, int indexCount, const uint16_t* indices)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_vertices_t sk_vertices_make_copy (SKVertexMode vmode, Int32 vertexCount, SKPoint* positions, SKPoint* texs, UInt32* colors, Int32 indexCount, UInt16* indices);

		// void sk_vertices_ref(sk_vertices_t* cvertices)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_vertices_ref (sk_vertices_t cvertices);

		// void sk_vertices_unref(sk_vertices_t* cvertices)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_vertices_unref (sk_vertices_t cvertices);

		#endregion

		#region sk_xml.h

		// void sk_xmlstreamwriter_delete(sk_xmlstreamwriter_t* writer)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_xmlstreamwriter_delete (sk_xmlstreamwriter_t writer);

		// sk_xmlstreamwriter_t* sk_xmlstreamwriter_new(sk_wstream_t* stream)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_xmlstreamwriter_t sk_xmlstreamwriter_new (sk_wstream_t stream);

		#endregion

		#region sk_manageddrawable.h

		// sk_manageddrawable_t* sk_manageddrawable_new(void* context)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_manageddrawable_t sk_manageddrawable_new (void* context);

		// void sk_manageddrawable_set_procs(sk_manageddrawable_procs_t procs)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_manageddrawable_set_procs (SKManagedDrawableDelegates procs);

		// void sk_manageddrawable_unref(sk_manageddrawable_t*)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_manageddrawable_unref (sk_manageddrawable_t param0);

		#endregion

		#region sk_managedstream.h

		// void sk_managedstream_destroy(sk_stream_managedstream_t* s)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedstream_destroy (sk_stream_managedstream_t s);

		// sk_stream_managedstream_t* sk_managedstream_new(void* context)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_managedstream_t sk_managedstream_new (void* context);

		// void sk_managedstream_set_procs(sk_managedstream_procs_t procs)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedstream_set_procs (SKManagedStreamDelegates procs);

		// void sk_managedwstream_destroy(sk_wstream_managedstream_t* s)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedwstream_destroy (sk_wstream_managedstream_t s);

		// sk_wstream_managedstream_t* sk_managedwstream_new(void* context)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_wstream_managedstream_t sk_managedwstream_new (void* context);

		// void sk_managedwstream_set_procs(sk_managedwstream_procs_t procs)
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedwstream_set_procs (SKManagedWStreamDelegates procs);

		#endregion

	}

	#region Delegates

	// typedef void (*)()* gr_gl_func_ptr
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void GRGlFuncPtr();

	// typedef gr_gl_func_ptr (*)(void* ctx, const char* name)* gr_gl_get_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate IntPtr GRGlGetProcProxyDelegate(void* ctx, [MarshalAs (UnmanagedType.LPStr)] String name);

	// typedef void (*)(void* addr, void* context)* sk_bitmap_release_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKBitmapReleaseProxyDelegate(void* addr, void* context);

	// typedef void (*)(const void* ptr, void* context)* sk_data_release_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKDataReleaseProxyDelegate(void* ptr, void* context);

	// typedef void (*)(const void* addr, void* context)* sk_image_raster_release_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKImageRasterReleaseProxyDelegate(void* addr, void* context);

	// typedef void (*)(void* context)* sk_image_texture_release_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKImageTextureReleaseProxyDelegate(void* context);

	// typedef void (*)(sk_manageddrawable_t* d, void* context)* sk_manageddrawable_destroy_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKManagedDrawableDestroyProxyDelegate(sk_manageddrawable_t d, void* context);

	// typedef void (*)(sk_manageddrawable_t* d, void* context, sk_canvas_t* ccanvas)* sk_manageddrawable_draw_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKManagedDrawableDrawProxyDelegate(sk_manageddrawable_t d, void* context, sk_canvas_t ccanvas);

	// typedef void (*)(sk_manageddrawable_t* d, void* context, sk_rect_t* rect)* sk_manageddrawable_getBounds_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKManagedDrawableGetBoundsProxyDelegate(sk_manageddrawable_t d, void* context, SKRect* rect);

	// typedef sk_picture_t* (*)(sk_manageddrawable_t* d, void* context)* sk_manageddrawable_newPictureSnapshot_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate sk_picture_t SKManagedDrawableNewPictureSnapshotProxyDelegate(sk_manageddrawable_t d, void* context);

	// typedef void (*)(sk_stream_managedstream_t* s, void* context)* sk_managedstream_destroy_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKManagedStreamDestroyProxyDelegate(sk_stream_managedstream_t s, void* context);

	// typedef sk_stream_managedstream_t* (*)(const sk_stream_managedstream_t* s, void* context)* sk_managedstream_duplicate_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate sk_stream_managedstream_t SKManagedStreamDuplicateProxyDelegate(sk_stream_managedstream_t s, void* context);

	// typedef sk_stream_managedstream_t* (*)(const sk_stream_managedstream_t* s, void* context)* sk_managedstream_fork_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate sk_stream_managedstream_t SKManagedStreamForkProxyDelegate(sk_stream_managedstream_t s, void* context);

	// typedef size_t (*)(const sk_stream_managedstream_t* s, void* context)* sk_managedstream_getLength_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate /* size_t */ IntPtr SKManagedStreamGetLengthProxyDelegate(sk_stream_managedstream_t s, void* context);

	// typedef size_t (*)(const sk_stream_managedstream_t* s, void* context)* sk_managedstream_getPosition_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate /* size_t */ IntPtr SKManagedStreamGetPositionProxyDelegate(sk_stream_managedstream_t s, void* context);

	// typedef bool (*)(const sk_stream_managedstream_t* s, void* context)* sk_managedstream_hasLength_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool SKManagedStreamHasLengthProxyDelegate(sk_stream_managedstream_t s, void* context);

	// typedef bool (*)(const sk_stream_managedstream_t* s, void* context)* sk_managedstream_hasPosition_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool SKManagedStreamHasPositionProxyDelegate(sk_stream_managedstream_t s, void* context);

	// typedef bool (*)(const sk_stream_managedstream_t* s, void* context)* sk_managedstream_isAtEnd_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool SKManagedStreamIsAtEndProxyDelegate(sk_stream_managedstream_t s, void* context);

	// typedef bool (*)(sk_stream_managedstream_t* s, void* context, int offset)* sk_managedstream_move_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool SKManagedStreamMoveProxyDelegate(sk_stream_managedstream_t s, void* context, Int32 offset);

	// typedef size_t (*)(const sk_stream_managedstream_t* s, void* context, void* buffer, size_t size)* sk_managedstream_peek_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate /* size_t */ IntPtr SKManagedStreamPeekProxyDelegate(sk_stream_managedstream_t s, void* context, void* buffer, /* size_t */ IntPtr size);

	// typedef size_t (*)(sk_stream_managedstream_t* s, void* context, void* buffer, size_t size)* sk_managedstream_read_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate /* size_t */ IntPtr SKManagedStreamReadProxyDelegate(sk_stream_managedstream_t s, void* context, void* buffer, /* size_t */ IntPtr size);

	// typedef bool (*)(sk_stream_managedstream_t* s, void* context)* sk_managedstream_rewind_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool SKManagedStreamRewindProxyDelegate(sk_stream_managedstream_t s, void* context);

	// typedef bool (*)(sk_stream_managedstream_t* s, void* context, size_t position)* sk_managedstream_seek_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool SKManagedStreamSeekProxyDelegate(sk_stream_managedstream_t s, void* context, /* size_t */ IntPtr position);

	// typedef size_t (*)(const sk_wstream_managedstream_t* s, void* context)* sk_managedwstream_bytesWritten_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate /* size_t */ IntPtr SKManagedWStreamBytesWrittenProxyDelegate(sk_wstream_managedstream_t s, void* context);

	// typedef void (*)(sk_wstream_managedstream_t* s, void* context)* sk_managedwstream_destroy_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKManagedWStreamDestroyProxyDelegate(sk_wstream_managedstream_t s, void* context);

	// typedef void (*)(sk_wstream_managedstream_t* s, void* context)* sk_managedwstream_flush_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKManagedWStreamFlushProxyDelegate(sk_wstream_managedstream_t s, void* context);

	// typedef bool (*)(sk_wstream_managedstream_t* s, void* context, const void* buffer, size_t size)* sk_managedwstream_write_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool SKManagedWStreamWriteProxyDelegate(sk_wstream_managedstream_t s, void* context, void* buffer, /* size_t */ IntPtr size);

	// typedef void (*)(void* addr, void* context)* sk_surface_raster_release_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKSurfaceRasterReleaseProxyDelegate(void* addr, void* context);

	#endregion

	#region Structs

	// gr_gl_framebufferinfo_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GRGlFramebufferInfo : IEquatable<GRGlFramebufferInfo> {
		// public unsigned int fFBOID
		private UInt32 fFBOID;
		public UInt32 FramebufferObjectId {
			readonly get => fFBOID;
			set => fFBOID = value;
		}

		// public unsigned int fFormat
		private UInt32 fFormat;
		public UInt32 Format {
			readonly get => fFormat;
			set => fFormat = value;
		}

		public readonly bool Equals (GRGlFramebufferInfo obj) =>
			fFBOID == obj.fFBOID && fFormat == obj.fFormat;

		public readonly override bool Equals (object obj) =>
			obj is GRGlFramebufferInfo f && Equals (f);

		public static bool operator == (GRGlFramebufferInfo left, GRGlFramebufferInfo right) =>
			left.Equals (right);

		public static bool operator != (GRGlFramebufferInfo left, GRGlFramebufferInfo right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fFBOID);
			hash.Add (fFormat);
			return hash.ToHashCode ();
		}

	}

	// gr_gl_textureinfo_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GRGlTextureInfo : IEquatable<GRGlTextureInfo> {
		// public unsigned int fTarget
		private UInt32 fTarget;
		public UInt32 Target {
			readonly get => fTarget;
			set => fTarget = value;
		}

		// public unsigned int fID
		private UInt32 fID;
		public UInt32 Id {
			readonly get => fID;
			set => fID = value;
		}

		// public unsigned int fFormat
		private UInt32 fFormat;
		public UInt32 Format {
			readonly get => fFormat;
			set => fFormat = value;
		}

		public readonly bool Equals (GRGlTextureInfo obj) =>
			fTarget == obj.fTarget && fID == obj.fID && fFormat == obj.fFormat;

		public readonly override bool Equals (object obj) =>
			obj is GRGlTextureInfo f && Equals (f);

		public static bool operator == (GRGlTextureInfo left, GRGlTextureInfo right) =>
			left.Equals (right);

		public static bool operator != (GRGlTextureInfo left, GRGlTextureInfo right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fTarget);
			hash.Add (fID);
			hash.Add (fFormat);
			return hash.ToHashCode ();
		}

	}

	// sk_codec_frameinfo_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKCodecFrameInfo : IEquatable<SKCodecFrameInfo> {
		// public int fRequiredFrame
		private Int32 fRequiredFrame;
		public Int32 RequiredFrame {
			readonly get => fRequiredFrame;
			set => fRequiredFrame = value;
		}

		// public int fDuration
		private Int32 fDuration;
		public Int32 Duration {
			readonly get => fDuration;
			set => fDuration = value;
		}

		// public bool fFullyReceived
		private Byte fFullyReceived;
		public bool FullyRecieved {
			readonly get => fFullyReceived > 0;
			set => fFullyReceived = value ? (byte)1 : (byte)0;
		}

		// public sk_alphatype_t fAlphaType
		private SKAlphaType fAlphaType;
		public SKAlphaType AlphaType {
			readonly get => fAlphaType;
			set => fAlphaType = value;
		}

		// public sk_codecanimation_disposalmethod_t fDisposalMethod
		private SKCodecAnimationDisposalMethod fDisposalMethod;
		public SKCodecAnimationDisposalMethod DisposalMethod {
			readonly get => fDisposalMethod;
			set => fDisposalMethod = value;
		}

		public readonly bool Equals (SKCodecFrameInfo obj) =>
			fRequiredFrame == obj.fRequiredFrame && fDuration == obj.fDuration && fFullyReceived == obj.fFullyReceived && fAlphaType == obj.fAlphaType && fDisposalMethod == obj.fDisposalMethod;

		public readonly override bool Equals (object obj) =>
			obj is SKCodecFrameInfo f && Equals (f);

		public static bool operator == (SKCodecFrameInfo left, SKCodecFrameInfo right) =>
			left.Equals (right);

		public static bool operator != (SKCodecFrameInfo left, SKCodecFrameInfo right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fRequiredFrame);
			hash.Add (fDuration);
			hash.Add (fFullyReceived);
			hash.Add (fAlphaType);
			hash.Add (fDisposalMethod);
			return hash.ToHashCode ();
		}

	}

	// sk_codec_options_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKCodecOptionsInternal : IEquatable<SKCodecOptionsInternal> {
		// public sk_codec_zero_initialized_t fZeroInitialized
		public SKZeroInitialized fZeroInitialized;

		// public sk_irect_t* fSubset
		public SKRectI* fSubset;

		// public int fFrameIndex
		public Int32 fFrameIndex;

		// public int fPriorFrame
		public Int32 fPriorFrame;

		// public sk_transfer_function_behavior_t fPremulBehavior
		public SKTransferFunctionBehavior fPremulBehavior;

		public readonly bool Equals (SKCodecOptionsInternal obj) =>
			fZeroInitialized == obj.fZeroInitialized && fSubset == obj.fSubset && fFrameIndex == obj.fFrameIndex && fPriorFrame == obj.fPriorFrame && fPremulBehavior == obj.fPremulBehavior;

		public readonly override bool Equals (object obj) =>
			obj is SKCodecOptionsInternal f && Equals (f);

		public static bool operator == (SKCodecOptionsInternal left, SKCodecOptionsInternal right) =>
			left.Equals (right);

		public static bool operator != (SKCodecOptionsInternal left, SKCodecOptionsInternal right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fZeroInitialized);
			hash.Add (fSubset);
			hash.Add (fFrameIndex);
			hash.Add (fPriorFrame);
			hash.Add (fPremulBehavior);
			return hash.ToHashCode ();
		}

	}

	// sk_color4f_t
	[StructLayout (LayoutKind.Sequential)]
	public readonly unsafe partial struct SKColorF : IEquatable<SKColorF> {
		// public float fR
		private readonly Single fR;
		public readonly Single Red => fR;

		// public float fG
		private readonly Single fG;
		public readonly Single Green => fG;

		// public float fB
		private readonly Single fB;
		public readonly Single Blue => fB;

		// public float fA
		private readonly Single fA;
		public readonly Single Alpha => fA;

		public readonly bool Equals (SKColorF obj) =>
			fR == obj.fR && fG == obj.fG && fB == obj.fB && fA == obj.fA;

		public readonly override bool Equals (object obj) =>
			obj is SKColorF f && Equals (f);

		public static bool operator == (SKColorF left, SKColorF right) =>
			left.Equals (right);

		public static bool operator != (SKColorF left, SKColorF right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fR);
			hash.Add (fG);
			hash.Add (fB);
			hash.Add (fA);
			return hash.ToHashCode ();
		}

	}

	// sk_colorspace_transfer_fn_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKColorSpaceTransferFn : IEquatable<SKColorSpaceTransferFn> {
		// public float fG
		private Single fG;
		public Single G {
			readonly get => fG;
			set => fG = value;
		}

		// public float fA
		private Single fA;
		public Single A {
			readonly get => fA;
			set => fA = value;
		}

		// public float fB
		private Single fB;
		public Single B {
			readonly get => fB;
			set => fB = value;
		}

		// public float fC
		private Single fC;
		public Single C {
			readonly get => fC;
			set => fC = value;
		}

		// public float fD
		private Single fD;
		public Single D {
			readonly get => fD;
			set => fD = value;
		}

		// public float fE
		private Single fE;
		public Single E {
			readonly get => fE;
			set => fE = value;
		}

		// public float fF
		private Single fF;
		public Single F {
			readonly get => fF;
			set => fF = value;
		}

		public readonly bool Equals (SKColorSpaceTransferFn obj) =>
			fG == obj.fG && fA == obj.fA && fB == obj.fB && fC == obj.fC && fD == obj.fD && fE == obj.fE && fF == obj.fF;

		public readonly override bool Equals (object obj) =>
			obj is SKColorSpaceTransferFn f && Equals (f);

		public static bool operator == (SKColorSpaceTransferFn left, SKColorSpaceTransferFn right) =>
			left.Equals (right);

		public static bool operator != (SKColorSpaceTransferFn left, SKColorSpaceTransferFn right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fG);
			hash.Add (fA);
			hash.Add (fB);
			hash.Add (fC);
			hash.Add (fD);
			hash.Add (fE);
			hash.Add (fF);
			return hash.ToHashCode ();
		}

	}

	// sk_colorspaceprimaries_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKColorSpacePrimaries : IEquatable<SKColorSpacePrimaries> {
		// public float fRX
		private Single fRX;
		public Single RX {
			readonly get => fRX;
			set => fRX = value;
		}

		// public float fRY
		private Single fRY;
		public Single RY {
			readonly get => fRY;
			set => fRY = value;
		}

		// public float fGX
		private Single fGX;
		public Single GX {
			readonly get => fGX;
			set => fGX = value;
		}

		// public float fGY
		private Single fGY;
		public Single GY {
			readonly get => fGY;
			set => fGY = value;
		}

		// public float fBX
		private Single fBX;
		public Single BX {
			readonly get => fBX;
			set => fBX = value;
		}

		// public float fBY
		private Single fBY;
		public Single BY {
			readonly get => fBY;
			set => fBY = value;
		}

		// public float fWX
		private Single fWX;
		public Single WX {
			readonly get => fWX;
			set => fWX = value;
		}

		// public float fWY
		private Single fWY;
		public Single WY {
			readonly get => fWY;
			set => fWY = value;
		}

		public readonly bool Equals (SKColorSpacePrimaries obj) =>
			fRX == obj.fRX && fRY == obj.fRY && fGX == obj.fGX && fGY == obj.fGY && fBX == obj.fBX && fBY == obj.fBY && fWX == obj.fWX && fWY == obj.fWY;

		public readonly override bool Equals (object obj) =>
			obj is SKColorSpacePrimaries f && Equals (f);

		public static bool operator == (SKColorSpacePrimaries left, SKColorSpacePrimaries right) =>
			left.Equals (right);

		public static bool operator != (SKColorSpacePrimaries left, SKColorSpacePrimaries right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fRX);
			hash.Add (fRY);
			hash.Add (fGX);
			hash.Add (fGY);
			hash.Add (fBX);
			hash.Add (fBY);
			hash.Add (fWX);
			hash.Add (fWY);
			return hash.ToHashCode ();
		}

	}

	// sk_document_pdf_metadata_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKDocumentPdfMetadataInternal : IEquatable<SKDocumentPdfMetadataInternal> {
		// public sk_string_t* fTitle
		public sk_string_t fTitle;

		// public sk_string_t* fAuthor
		public sk_string_t fAuthor;

		// public sk_string_t* fSubject
		public sk_string_t fSubject;

		// public sk_string_t* fKeywords
		public sk_string_t fKeywords;

		// public sk_string_t* fCreator
		public sk_string_t fCreator;

		// public sk_string_t* fProducer
		public sk_string_t fProducer;

		// public sk_time_datetime_t* fCreation
		public SKTimeDateTimeInternal* fCreation;

		// public sk_time_datetime_t* fModified
		public SKTimeDateTimeInternal* fModified;

		// public float fRasterDPI
		public Single fRasterDPI;

		// public bool fPDFA
		public Byte fPDFA;

		// public int fEncodingQuality
		public Int32 fEncodingQuality;

		public readonly bool Equals (SKDocumentPdfMetadataInternal obj) =>
			fTitle == obj.fTitle && fAuthor == obj.fAuthor && fSubject == obj.fSubject && fKeywords == obj.fKeywords && fCreator == obj.fCreator && fProducer == obj.fProducer && fCreation == obj.fCreation && fModified == obj.fModified && fRasterDPI == obj.fRasterDPI && fPDFA == obj.fPDFA && fEncodingQuality == obj.fEncodingQuality;

		public readonly override bool Equals (object obj) =>
			obj is SKDocumentPdfMetadataInternal f && Equals (f);

		public static bool operator == (SKDocumentPdfMetadataInternal left, SKDocumentPdfMetadataInternal right) =>
			left.Equals (right);

		public static bool operator != (SKDocumentPdfMetadataInternal left, SKDocumentPdfMetadataInternal right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fTitle);
			hash.Add (fAuthor);
			hash.Add (fSubject);
			hash.Add (fKeywords);
			hash.Add (fCreator);
			hash.Add (fProducer);
			hash.Add (fCreation);
			hash.Add (fModified);
			hash.Add (fRasterDPI);
			hash.Add (fPDFA);
			hash.Add (fEncodingQuality);
			return hash.ToHashCode ();
		}

	}

	// sk_fontmetrics_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKFontMetrics : IEquatable<SKFontMetrics> {
		// public uint32_t fFlags
		private UInt32 fFlags;

		// public float fTop
		private Single fTop;

		// public float fAscent
		private Single fAscent;

		// public float fDescent
		private Single fDescent;

		// public float fBottom
		private Single fBottom;

		// public float fLeading
		private Single fLeading;

		// public float fAvgCharWidth
		private Single fAvgCharWidth;

		// public float fMaxCharWidth
		private Single fMaxCharWidth;

		// public float fXMin
		private Single fXMin;

		// public float fXMax
		private Single fXMax;

		// public float fXHeight
		private Single fXHeight;

		// public float fCapHeight
		private Single fCapHeight;

		// public float fUnderlineThickness
		private Single fUnderlineThickness;

		// public float fUnderlinePosition
		private Single fUnderlinePosition;

		// public float fStrikeoutThickness
		private Single fStrikeoutThickness;

		// public float fStrikeoutPosition
		private Single fStrikeoutPosition;

		public readonly bool Equals (SKFontMetrics obj) =>
			fFlags == obj.fFlags && fTop == obj.fTop && fAscent == obj.fAscent && fDescent == obj.fDescent && fBottom == obj.fBottom && fLeading == obj.fLeading && fAvgCharWidth == obj.fAvgCharWidth && fMaxCharWidth == obj.fMaxCharWidth && fXMin == obj.fXMin && fXMax == obj.fXMax && fXHeight == obj.fXHeight && fCapHeight == obj.fCapHeight && fUnderlineThickness == obj.fUnderlineThickness && fUnderlinePosition == obj.fUnderlinePosition && fStrikeoutThickness == obj.fStrikeoutThickness && fStrikeoutPosition == obj.fStrikeoutPosition;

		public readonly override bool Equals (object obj) =>
			obj is SKFontMetrics f && Equals (f);

		public static bool operator == (SKFontMetrics left, SKFontMetrics right) =>
			left.Equals (right);

		public static bool operator != (SKFontMetrics left, SKFontMetrics right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fFlags);
			hash.Add (fTop);
			hash.Add (fAscent);
			hash.Add (fDescent);
			hash.Add (fBottom);
			hash.Add (fLeading);
			hash.Add (fAvgCharWidth);
			hash.Add (fMaxCharWidth);
			hash.Add (fXMin);
			hash.Add (fXMax);
			hash.Add (fXHeight);
			hash.Add (fCapHeight);
			hash.Add (fUnderlineThickness);
			hash.Add (fUnderlinePosition);
			hash.Add (fStrikeoutThickness);
			hash.Add (fStrikeoutPosition);
			return hash.ToHashCode ();
		}

	}

	// sk_highcontrastconfig_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKHighContrastConfig : IEquatable<SKHighContrastConfig> {
		// public bool fGrayscale
		private Byte fGrayscale;
		public bool Grayscale {
			readonly get => fGrayscale > 0;
			set => fGrayscale = value ? (byte)1 : (byte)0;
		}

		// public sk_highcontrastconfig_invertstyle_t fInvertStyle
		private SKHighContrastConfigInvertStyle fInvertStyle;
		public SKHighContrastConfigInvertStyle InvertStyle {
			readonly get => fInvertStyle;
			set => fInvertStyle = value;
		}

		// public float fContrast
		private Single fContrast;
		public Single Contrast {
			readonly get => fContrast;
			set => fContrast = value;
		}

		public readonly bool Equals (SKHighContrastConfig obj) =>
			fGrayscale == obj.fGrayscale && fInvertStyle == obj.fInvertStyle && fContrast == obj.fContrast;

		public readonly override bool Equals (object obj) =>
			obj is SKHighContrastConfig f && Equals (f);

		public static bool operator == (SKHighContrastConfig left, SKHighContrastConfig right) =>
			left.Equals (right);

		public static bool operator != (SKHighContrastConfig left, SKHighContrastConfig right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fGrayscale);
			hash.Add (fInvertStyle);
			hash.Add (fContrast);
			return hash.ToHashCode ();
		}

	}

	// sk_imageinfo_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKImageInfoNative : IEquatable<SKImageInfoNative> {
		// public sk_colorspace_t* colorspace
		public sk_colorspace_t colorspace;

		// public int32_t width
		public Int32 width;

		// public int32_t height
		public Int32 height;

		// public sk_colortype_t colorType
		public SKColorType colorType;

		// public sk_alphatype_t alphaType
		public SKAlphaType alphaType;

		public readonly bool Equals (SKImageInfoNative obj) =>
			colorspace == obj.colorspace && width == obj.width && height == obj.height && colorType == obj.colorType && alphaType == obj.alphaType;

		public readonly override bool Equals (object obj) =>
			obj is SKImageInfoNative f && Equals (f);

		public static bool operator == (SKImageInfoNative left, SKImageInfoNative right) =>
			left.Equals (right);

		public static bool operator != (SKImageInfoNative left, SKImageInfoNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (colorspace);
			hash.Add (width);
			hash.Add (height);
			hash.Add (colorType);
			hash.Add (alphaType);
			return hash.ToHashCode ();
		}

	}

	// sk_ipoint_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKPointI : IEquatable<SKPointI> {
		// public int32_t x
		private Int32 x;
		public Int32 X {
			readonly get => x;
			set => x = value;
		}

		// public int32_t y
		private Int32 y;
		public Int32 Y {
			readonly get => y;
			set => y = value;
		}

		public readonly bool Equals (SKPointI obj) =>
			x == obj.x && y == obj.y;

		public readonly override bool Equals (object obj) =>
			obj is SKPointI f && Equals (f);

		public static bool operator == (SKPointI left, SKPointI right) =>
			left.Equals (right);

		public static bool operator != (SKPointI left, SKPointI right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (x);
			hash.Add (y);
			return hash.ToHashCode ();
		}

	}

	// sk_irect_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKRectI : IEquatable<SKRectI> {
		// public int32_t left
		private Int32 left;
		public Int32 Left {
			readonly get => left;
			set => left = value;
		}

		// public int32_t top
		private Int32 top;
		public Int32 Top {
			readonly get => top;
			set => top = value;
		}

		// public int32_t right
		private Int32 right;
		public Int32 Right {
			readonly get => right;
			set => right = value;
		}

		// public int32_t bottom
		private Int32 bottom;
		public Int32 Bottom {
			readonly get => bottom;
			set => bottom = value;
		}

		public readonly bool Equals (SKRectI obj) =>
			left == obj.left && top == obj.top && right == obj.right && bottom == obj.bottom;

		public readonly override bool Equals (object obj) =>
			obj is SKRectI f && Equals (f);

		public static bool operator == (SKRectI left, SKRectI right) =>
			left.Equals (right);

		public static bool operator != (SKRectI left, SKRectI right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (left);
			hash.Add (top);
			hash.Add (right);
			hash.Add (bottom);
			return hash.ToHashCode ();
		}

	}

	// sk_isize_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKSizeI : IEquatable<SKSizeI> {
		// public int32_t w
		private Int32 w;
		public Int32 Width {
			readonly get => w;
			set => w = value;
		}

		// public int32_t h
		private Int32 h;
		public Int32 Height {
			readonly get => h;
			set => h = value;
		}

		public readonly bool Equals (SKSizeI obj) =>
			w == obj.w && h == obj.h;

		public readonly override bool Equals (object obj) =>
			obj is SKSizeI f && Equals (f);

		public static bool operator == (SKSizeI left, SKSizeI right) =>
			left.Equals (right);

		public static bool operator != (SKSizeI left, SKSizeI right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (w);
			hash.Add (h);
			return hash.ToHashCode ();
		}

	}

	// sk_jpegencoder_options_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKJpegEncoderOptions : IEquatable<SKJpegEncoderOptions> {
		// public int fQuality
		private Int32 fQuality;
		public Int32 Quality {
			readonly get => fQuality;
			set => fQuality = value;
		}

		// public sk_jpegencoder_downsample_t fDownsample
		private SKJpegEncoderDownsample fDownsample;
		public SKJpegEncoderDownsample Downsample {
			readonly get => fDownsample;
			set => fDownsample = value;
		}

		// public sk_jpegencoder_alphaoption_t fAlphaOption
		private SKJpegEncoderAlphaOption fAlphaOption;
		public SKJpegEncoderAlphaOption AlphaOption {
			readonly get => fAlphaOption;
			set => fAlphaOption = value;
		}

		// public sk_transfer_function_behavior_t fBlendBehavior
		private SKTransferFunctionBehavior fBlendBehavior;
		public SKTransferFunctionBehavior BlendBehavior {
			readonly get => fBlendBehavior;
			set => fBlendBehavior = value;
		}

		public readonly bool Equals (SKJpegEncoderOptions obj) =>
			fQuality == obj.fQuality && fDownsample == obj.fDownsample && fAlphaOption == obj.fAlphaOption && fBlendBehavior == obj.fBlendBehavior;

		public readonly override bool Equals (object obj) =>
			obj is SKJpegEncoderOptions f && Equals (f);

		public static bool operator == (SKJpegEncoderOptions left, SKJpegEncoderOptions right) =>
			left.Equals (right);

		public static bool operator != (SKJpegEncoderOptions left, SKJpegEncoderOptions right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fQuality);
			hash.Add (fDownsample);
			hash.Add (fAlphaOption);
			hash.Add (fBlendBehavior);
			return hash.ToHashCode ();
		}

	}

	// sk_lattice_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKLatticeInternal : IEquatable<SKLatticeInternal> {
		// public const int* fXDivs
		public Int32* fXDivs;

		// public const int* fYDivs
		public Int32* fYDivs;

		// public const sk_lattice_recttype_t* fRectTypes
		public SKLatticeRectType* fRectTypes;

		// public int fXCount
		public Int32 fXCount;

		// public int fYCount
		public Int32 fYCount;

		// public const sk_irect_t* fBounds
		public SKRectI* fBounds;

		// public const sk_color_t* fColors
		public UInt32* fColors;

		public readonly bool Equals (SKLatticeInternal obj) =>
			fXDivs == obj.fXDivs && fYDivs == obj.fYDivs && fRectTypes == obj.fRectTypes && fXCount == obj.fXCount && fYCount == obj.fYCount && fBounds == obj.fBounds && fColors == obj.fColors;

		public readonly override bool Equals (object obj) =>
			obj is SKLatticeInternal f && Equals (f);

		public static bool operator == (SKLatticeInternal left, SKLatticeInternal right) =>
			left.Equals (right);

		public static bool operator != (SKLatticeInternal left, SKLatticeInternal right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fXDivs);
			hash.Add (fYDivs);
			hash.Add (fRectTypes);
			hash.Add (fXCount);
			hash.Add (fYCount);
			hash.Add (fBounds);
			hash.Add (fColors);
			return hash.ToHashCode ();
		}

	}

	// sk_manageddrawable_procs_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKManagedDrawableDelegates : IEquatable<SKManagedDrawableDelegates> {
		// public sk_manageddrawable_draw_proc fDraw
		public SKManagedDrawableDrawProxyDelegate fDraw;

		// public sk_manageddrawable_getBounds_proc fGetBounds
		public SKManagedDrawableGetBoundsProxyDelegate fGetBounds;

		// public sk_manageddrawable_newPictureSnapshot_proc fNewPictureSnapshot
		public SKManagedDrawableNewPictureSnapshotProxyDelegate fNewPictureSnapshot;

		// public sk_manageddrawable_destroy_proc fDestroy
		public SKManagedDrawableDestroyProxyDelegate fDestroy;

		public readonly bool Equals (SKManagedDrawableDelegates obj) =>
			fDraw == obj.fDraw && fGetBounds == obj.fGetBounds && fNewPictureSnapshot == obj.fNewPictureSnapshot && fDestroy == obj.fDestroy;

		public readonly override bool Equals (object obj) =>
			obj is SKManagedDrawableDelegates f && Equals (f);

		public static bool operator == (SKManagedDrawableDelegates left, SKManagedDrawableDelegates right) =>
			left.Equals (right);

		public static bool operator != (SKManagedDrawableDelegates left, SKManagedDrawableDelegates right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fDraw);
			hash.Add (fGetBounds);
			hash.Add (fNewPictureSnapshot);
			hash.Add (fDestroy);
			return hash.ToHashCode ();
		}

	}

	// sk_managedstream_procs_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKManagedStreamDelegates : IEquatable<SKManagedStreamDelegates> {
		// public sk_managedstream_read_proc fRead
		public SKManagedStreamReadProxyDelegate fRead;

		// public sk_managedstream_peek_proc fPeek
		public SKManagedStreamPeekProxyDelegate fPeek;

		// public sk_managedstream_isAtEnd_proc fIsAtEnd
		public SKManagedStreamIsAtEndProxyDelegate fIsAtEnd;

		// public sk_managedstream_hasPosition_proc fHasPosition
		public SKManagedStreamHasPositionProxyDelegate fHasPosition;

		// public sk_managedstream_hasLength_proc fHasLength
		public SKManagedStreamHasLengthProxyDelegate fHasLength;

		// public sk_managedstream_rewind_proc fRewind
		public SKManagedStreamRewindProxyDelegate fRewind;

		// public sk_managedstream_getPosition_proc fGetPosition
		public SKManagedStreamGetPositionProxyDelegate fGetPosition;

		// public sk_managedstream_seek_proc fSeek
		public SKManagedStreamSeekProxyDelegate fSeek;

		// public sk_managedstream_move_proc fMove
		public SKManagedStreamMoveProxyDelegate fMove;

		// public sk_managedstream_getLength_proc fGetLength
		public SKManagedStreamGetLengthProxyDelegate fGetLength;

		// public sk_managedstream_duplicate_proc fDuplicate
		public SKManagedStreamDuplicateProxyDelegate fDuplicate;

		// public sk_managedstream_fork_proc fFork
		public SKManagedStreamForkProxyDelegate fFork;

		// public sk_managedstream_destroy_proc fDestroy
		public SKManagedStreamDestroyProxyDelegate fDestroy;

		public readonly bool Equals (SKManagedStreamDelegates obj) =>
			fRead == obj.fRead && fPeek == obj.fPeek && fIsAtEnd == obj.fIsAtEnd && fHasPosition == obj.fHasPosition && fHasLength == obj.fHasLength && fRewind == obj.fRewind && fGetPosition == obj.fGetPosition && fSeek == obj.fSeek && fMove == obj.fMove && fGetLength == obj.fGetLength && fDuplicate == obj.fDuplicate && fFork == obj.fFork && fDestroy == obj.fDestroy;

		public readonly override bool Equals (object obj) =>
			obj is SKManagedStreamDelegates f && Equals (f);

		public static bool operator == (SKManagedStreamDelegates left, SKManagedStreamDelegates right) =>
			left.Equals (right);

		public static bool operator != (SKManagedStreamDelegates left, SKManagedStreamDelegates right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fRead);
			hash.Add (fPeek);
			hash.Add (fIsAtEnd);
			hash.Add (fHasPosition);
			hash.Add (fHasLength);
			hash.Add (fRewind);
			hash.Add (fGetPosition);
			hash.Add (fSeek);
			hash.Add (fMove);
			hash.Add (fGetLength);
			hash.Add (fDuplicate);
			hash.Add (fFork);
			hash.Add (fDestroy);
			return hash.ToHashCode ();
		}

	}

	// sk_managedwstream_procs_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKManagedWStreamDelegates : IEquatable<SKManagedWStreamDelegates> {
		// public sk_managedwstream_write_proc fWrite
		public SKManagedWStreamWriteProxyDelegate fWrite;

		// public sk_managedwstream_flush_proc fFlush
		public SKManagedWStreamFlushProxyDelegate fFlush;

		// public sk_managedwstream_bytesWritten_proc fBytesWritten
		public SKManagedWStreamBytesWrittenProxyDelegate fBytesWritten;

		// public sk_managedwstream_destroy_proc fDestroy
		public SKManagedWStreamDestroyProxyDelegate fDestroy;

		public readonly bool Equals (SKManagedWStreamDelegates obj) =>
			fWrite == obj.fWrite && fFlush == obj.fFlush && fBytesWritten == obj.fBytesWritten && fDestroy == obj.fDestroy;

		public readonly override bool Equals (object obj) =>
			obj is SKManagedWStreamDelegates f && Equals (f);

		public static bool operator == (SKManagedWStreamDelegates left, SKManagedWStreamDelegates right) =>
			left.Equals (right);

		public static bool operator != (SKManagedWStreamDelegates left, SKManagedWStreamDelegates right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fWrite);
			hash.Add (fFlush);
			hash.Add (fBytesWritten);
			hash.Add (fDestroy);
			return hash.ToHashCode ();
		}

	}

	// sk_mask_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKMask : IEquatable<SKMask> {
		// public uint8_t* fImage
		private Byte* fImage;

		// public sk_irect_t fBounds
		private SKRectI fBounds;

		// public uint32_t fRowBytes
		private UInt32 fRowBytes;

		// public sk_mask_format_t fFormat
		private SKMaskFormat fFormat;

		public readonly bool Equals (SKMask obj) =>
			fImage == obj.fImage && fBounds == obj.fBounds && fRowBytes == obj.fRowBytes && fFormat == obj.fFormat;

		public readonly override bool Equals (object obj) =>
			obj is SKMask f && Equals (f);

		public static bool operator == (SKMask left, SKMask right) =>
			left.Equals (right);

		public static bool operator != (SKMask left, SKMask right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fImage);
			hash.Add (fBounds);
			hash.Add (fRowBytes);
			hash.Add (fFormat);
			return hash.ToHashCode ();
		}

	}

	// sk_matrix_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKMatrix : IEquatable<SKMatrix> {
		// public float scaleX
		private Single scaleX;
		public Single ScaleX {
			readonly get => scaleX;
			set => scaleX = value;
		}

		// public float skewX
		private Single skewX;
		public Single SkewX {
			readonly get => skewX;
			set => skewX = value;
		}

		// public float transX
		private Single transX;
		public Single TransX {
			readonly get => transX;
			set => transX = value;
		}

		// public float skewY
		private Single skewY;
		public Single SkewY {
			readonly get => skewY;
			set => skewY = value;
		}

		// public float scaleY
		private Single scaleY;
		public Single ScaleY {
			readonly get => scaleY;
			set => scaleY = value;
		}

		// public float transY
		private Single transY;
		public Single TransY {
			readonly get => transY;
			set => transY = value;
		}

		// public float persp0
		private Single persp0;
		public Single Persp0 {
			readonly get => persp0;
			set => persp0 = value;
		}

		// public float persp1
		private Single persp1;
		public Single Persp1 {
			readonly get => persp1;
			set => persp1 = value;
		}

		// public float persp2
		private Single persp2;
		public Single Persp2 {
			readonly get => persp2;
			set => persp2 = value;
		}

		public readonly bool Equals (SKMatrix obj) =>
			scaleX == obj.scaleX && skewX == obj.skewX && transX == obj.transX && skewY == obj.skewY && scaleY == obj.scaleY && transY == obj.transY && persp0 == obj.persp0 && persp1 == obj.persp1 && persp2 == obj.persp2;

		public readonly override bool Equals (object obj) =>
			obj is SKMatrix f && Equals (f);

		public static bool operator == (SKMatrix left, SKMatrix right) =>
			left.Equals (right);

		public static bool operator != (SKMatrix left, SKMatrix right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (scaleX);
			hash.Add (skewX);
			hash.Add (transX);
			hash.Add (skewY);
			hash.Add (scaleY);
			hash.Add (transY);
			hash.Add (persp0);
			hash.Add (persp1);
			hash.Add (persp2);
			return hash.ToHashCode ();
		}

	}

	// sk_pngencoder_options_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKPngEncoderOptions : IEquatable<SKPngEncoderOptions> {
		// public sk_pngencoder_filterflags_t fFilterFlags
		private SKPngEncoderFilterFlags fFilterFlags;

		// public int fZLibLevel
		private Int32 fZLibLevel;

		// public sk_transfer_function_behavior_t fUnpremulBehavior
		private SKTransferFunctionBehavior fUnpremulBehavior;

		// public void* fComments
		private void* fComments;

		public readonly bool Equals (SKPngEncoderOptions obj) =>
			fFilterFlags == obj.fFilterFlags && fZLibLevel == obj.fZLibLevel && fUnpremulBehavior == obj.fUnpremulBehavior && fComments == obj.fComments;

		public readonly override bool Equals (object obj) =>
			obj is SKPngEncoderOptions f && Equals (f);

		public static bool operator == (SKPngEncoderOptions left, SKPngEncoderOptions right) =>
			left.Equals (right);

		public static bool operator != (SKPngEncoderOptions left, SKPngEncoderOptions right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fFilterFlags);
			hash.Add (fZLibLevel);
			hash.Add (fUnpremulBehavior);
			hash.Add (fComments);
			return hash.ToHashCode ();
		}

	}

	// sk_point_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKPoint : IEquatable<SKPoint> {
		// public float x
		private Single x;
		public Single X {
			readonly get => x;
			set => x = value;
		}

		// public float y
		private Single y;
		public Single Y {
			readonly get => y;
			set => y = value;
		}

		public readonly bool Equals (SKPoint obj) =>
			x == obj.x && y == obj.y;

		public readonly override bool Equals (object obj) =>
			obj is SKPoint f && Equals (f);

		public static bool operator == (SKPoint left, SKPoint right) =>
			left.Equals (right);

		public static bool operator != (SKPoint left, SKPoint right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (x);
			hash.Add (y);
			return hash.ToHashCode ();
		}

	}

	// sk_point3_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKPoint3 : IEquatable<SKPoint3> {
		// public float x
		private Single x;
		public Single X {
			readonly get => x;
			set => x = value;
		}

		// public float y
		private Single y;
		public Single Y {
			readonly get => y;
			set => y = value;
		}

		// public float z
		private Single z;
		public Single Z {
			readonly get => z;
			set => z = value;
		}

		public readonly bool Equals (SKPoint3 obj) =>
			x == obj.x && y == obj.y && z == obj.z;

		public readonly override bool Equals (object obj) =>
			obj is SKPoint3 f && Equals (f);

		public static bool operator == (SKPoint3 left, SKPoint3 right) =>
			left.Equals (right);

		public static bool operator != (SKPoint3 left, SKPoint3 right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (x);
			hash.Add (y);
			hash.Add (z);
			return hash.ToHashCode ();
		}

	}

	// sk_rect_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKRect : IEquatable<SKRect> {
		// public float left
		private Single left;
		public Single Left {
			readonly get => left;
			set => left = value;
		}

		// public float top
		private Single top;
		public Single Top {
			readonly get => top;
			set => top = value;
		}

		// public float right
		private Single right;
		public Single Right {
			readonly get => right;
			set => right = value;
		}

		// public float bottom
		private Single bottom;
		public Single Bottom {
			readonly get => bottom;
			set => bottom = value;
		}

		public readonly bool Equals (SKRect obj) =>
			left == obj.left && top == obj.top && right == obj.right && bottom == obj.bottom;

		public readonly override bool Equals (object obj) =>
			obj is SKRect f && Equals (f);

		public static bool operator == (SKRect left, SKRect right) =>
			left.Equals (right);

		public static bool operator != (SKRect left, SKRect right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (left);
			hash.Add (top);
			hash.Add (right);
			hash.Add (bottom);
			return hash.ToHashCode ();
		}

	}

	// sk_rsxform_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKRotationScaleMatrix : IEquatable<SKRotationScaleMatrix> {
		// public float fSCos
		private Single fSCos;
		public Single SCos {
			readonly get => fSCos;
			set => fSCos = value;
		}

		// public float fSSin
		private Single fSSin;
		public Single SSin {
			readonly get => fSSin;
			set => fSSin = value;
		}

		// public float fTX
		private Single fTX;
		public Single TX {
			readonly get => fTX;
			set => fTX = value;
		}

		// public float fTY
		private Single fTY;
		public Single TY {
			readonly get => fTY;
			set => fTY = value;
		}

		public readonly bool Equals (SKRotationScaleMatrix obj) =>
			fSCos == obj.fSCos && fSSin == obj.fSSin && fTX == obj.fTX && fTY == obj.fTY;

		public readonly override bool Equals (object obj) =>
			obj is SKRotationScaleMatrix f && Equals (f);

		public static bool operator == (SKRotationScaleMatrix left, SKRotationScaleMatrix right) =>
			left.Equals (right);

		public static bool operator != (SKRotationScaleMatrix left, SKRotationScaleMatrix right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fSCos);
			hash.Add (fSSin);
			hash.Add (fTX);
			hash.Add (fTY);
			return hash.ToHashCode ();
		}

	}

	// sk_size_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKSize : IEquatable<SKSize> {
		// public float w
		private Single w;
		public Single Width {
			readonly get => w;
			set => w = value;
		}

		// public float h
		private Single h;
		public Single Height {
			readonly get => h;
			set => h = value;
		}

		public readonly bool Equals (SKSize obj) =>
			w == obj.w && h == obj.h;

		public readonly override bool Equals (object obj) =>
			obj is SKSize f && Equals (f);

		public static bool operator == (SKSize left, SKSize right) =>
			left.Equals (right);

		public static bool operator != (SKSize left, SKSize right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (w);
			hash.Add (h);
			return hash.ToHashCode ();
		}

	}

	// sk_textblob_builder_runbuffer_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKRunBufferInternal : IEquatable<SKRunBufferInternal> {
		// public void* glyphs
		public void* glyphs;

		// public void* pos
		public void* pos;

		// public void* utf8text
		public void* utf8text;

		// public void* clusters
		public void* clusters;

		public readonly bool Equals (SKRunBufferInternal obj) =>
			glyphs == obj.glyphs && pos == obj.pos && utf8text == obj.utf8text && clusters == obj.clusters;

		public readonly override bool Equals (object obj) =>
			obj is SKRunBufferInternal f && Equals (f);

		public static bool operator == (SKRunBufferInternal left, SKRunBufferInternal right) =>
			left.Equals (right);

		public static bool operator != (SKRunBufferInternal left, SKRunBufferInternal right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (glyphs);
			hash.Add (pos);
			hash.Add (utf8text);
			hash.Add (clusters);
			return hash.ToHashCode ();
		}

	}

	// sk_time_datetime_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKTimeDateTimeInternal : IEquatable<SKTimeDateTimeInternal> {
		// public int16_t fTimeZoneMinutes
		public Int16 fTimeZoneMinutes;

		// public uint16_t fYear
		public UInt16 fYear;

		// public uint8_t fMonth
		public Byte fMonth;

		// public uint8_t fDayOfWeek
		public Byte fDayOfWeek;

		// public uint8_t fDay
		public Byte fDay;

		// public uint8_t fHour
		public Byte fHour;

		// public uint8_t fMinute
		public Byte fMinute;

		// public uint8_t fSecond
		public Byte fSecond;

		public readonly bool Equals (SKTimeDateTimeInternal obj) =>
			fTimeZoneMinutes == obj.fTimeZoneMinutes && fYear == obj.fYear && fMonth == obj.fMonth && fDayOfWeek == obj.fDayOfWeek && fDay == obj.fDay && fHour == obj.fHour && fMinute == obj.fMinute && fSecond == obj.fSecond;

		public readonly override bool Equals (object obj) =>
			obj is SKTimeDateTimeInternal f && Equals (f);

		public static bool operator == (SKTimeDateTimeInternal left, SKTimeDateTimeInternal right) =>
			left.Equals (right);

		public static bool operator != (SKTimeDateTimeInternal left, SKTimeDateTimeInternal right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fTimeZoneMinutes);
			hash.Add (fYear);
			hash.Add (fMonth);
			hash.Add (fDayOfWeek);
			hash.Add (fDay);
			hash.Add (fHour);
			hash.Add (fMinute);
			hash.Add (fSecond);
			return hash.ToHashCode ();
		}

	}

	// sk_webpencoder_options_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKWebpEncoderOptions : IEquatable<SKWebpEncoderOptions> {
		// public sk_webpencoder_compression_t fCompression
		private SKWebpEncoderCompression fCompression;
		public SKWebpEncoderCompression Compression {
			readonly get => fCompression;
			set => fCompression = value;
		}

		// public float fQuality
		private Single fQuality;
		public Single Quality {
			readonly get => fQuality;
			set => fQuality = value;
		}

		// public sk_transfer_function_behavior_t fUnpremulBehavior
		private SKTransferFunctionBehavior fUnpremulBehavior;
		public SKTransferFunctionBehavior UnpremulBehavior {
			readonly get => fUnpremulBehavior;
			set => fUnpremulBehavior = value;
		}

		public readonly bool Equals (SKWebpEncoderOptions obj) =>
			fCompression == obj.fCompression && fQuality == obj.fQuality && fUnpremulBehavior == obj.fUnpremulBehavior;

		public readonly override bool Equals (object obj) =>
			obj is SKWebpEncoderOptions f && Equals (f);

		public static bool operator == (SKWebpEncoderOptions left, SKWebpEncoderOptions right) =>
			left.Equals (right);

		public static bool operator != (SKWebpEncoderOptions left, SKWebpEncoderOptions right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fCompression);
			hash.Add (fQuality);
			hash.Add (fUnpremulBehavior);
			return hash.ToHashCode ();
		}

	}

	#endregion

	#region Enums

	// gr_backend_t
	public enum GRBackend {
		// METAL_GR_BACKEND = 0
		Metal = 0,
		// OPENGL_GR_BACKEND = 1
		OpenGL = 1,
		// VULKAN_GR_BACKEND = 2
		Vulkan = 2,
	}

	// gr_pixelconfig_t
	public enum GRPixelConfig {
		// UNKNOWN_GR_PIXEL_CONFIG = 0
		Unknown = 0,
		// ALPHA_8_GR_PIXEL_CONFIG = 1
		Alpha8 = 1,
		// GRAY_8_GR_PIXEL_CONFIG = 2
		Gray8 = 2,
		// RGB_565_GR_PIXEL_CONFIG = 3
		Rgb565 = 3,
		// RGBA_4444_GR_PIXEL_CONFIG = 4
		Rgba4444 = 4,
		// RGBA_8888_GR_PIXEL_CONFIG = 5
		Rgba8888 = 5,
		// RGB_888_GR_PIXEL_CONFIG = 6
		Rgb888 = 6,
		// BGRA_8888_GR_PIXEL_CONFIG = 7
		Bgra8888 = 7,
		// SRGBA_8888_GR_PIXEL_CONFIG = 8
		Srgba8888 = 8,
		// SBGRA_8888_GR_PIXEL_CONFIG = 9
		Sbgra8888 = 9,
		// RGBA_1010102_GR_PIXEL_CONFIG = 10
		Rgba1010102 = 10,
		// RGBA_FLOAT_GR_PIXEL_CONFIG = 11
		RgbaFloat = 11,
		// RG_FLOAT_GR_PIXEL_CONFIG = 12
		RgFloat = 12,
		// ALPHA_HALF_GR_PIXEL_CONFIG = 13
		AlphaHalf = 13,
		// RGBA_HALF_GR_PIXEL_CONFIG = 14
		RgbaHalf = 14,
	}

	// gr_surfaceorigin_t
	public enum GRSurfaceOrigin {
		// TOP_LEFT_GR_SURFACE_ORIGIN = 0
		TopLeft = 0,
		// BOTTOM_LEFT_GR_SURFACE_ORIGIN = 1
		BottomLeft = 1,
	}

	// sk_alphatype_t
	public enum SKAlphaType {
		// UNKNOWN_SK_ALPHATYPE = 0
		Unknown = 0,
		// OPAQUE_SK_ALPHATYPE = 1
		Opaque = 1,
		// PREMUL_SK_ALPHATYPE = 2
		Premul = 2,
		// UNPREMUL_SK_ALPHATYPE = 3
		Unpremul = 3,
	}

	// sk_bitmap_allocflags_t
	[Flags]
	public enum SKBitmapAllocFlags {
		// NONE_SK_BITMAP_ALLOC_FLAGS = 0
		None = 0,
		// ZERO_PIXELS_SK_BITMAP_ALLOC_FLAGS = 1 << 0
		ZeroPixels = 1,
	}

	// sk_blendmode_t
	public enum SKBlendMode {
		// CLEAR_SK_BLENDMODE = 0
		Clear = 0,
		// SRC_SK_BLENDMODE = 1
		Src = 1,
		// DST_SK_BLENDMODE = 2
		Dst = 2,
		// SRCOVER_SK_BLENDMODE = 3
		SrcOver = 3,
		// DSTOVER_SK_BLENDMODE = 4
		DstOver = 4,
		// SRCIN_SK_BLENDMODE = 5
		SrcIn = 5,
		// DSTIN_SK_BLENDMODE = 6
		DstIn = 6,
		// SRCOUT_SK_BLENDMODE = 7
		SrcOut = 7,
		// DSTOUT_SK_BLENDMODE = 8
		DstOut = 8,
		// SRCATOP_SK_BLENDMODE = 9
		SrcATop = 9,
		// DSTATOP_SK_BLENDMODE = 10
		DstATop = 10,
		// XOR_SK_BLENDMODE = 11
		Xor = 11,
		// PLUS_SK_BLENDMODE = 12
		Plus = 12,
		// MODULATE_SK_BLENDMODE = 13
		Modulate = 13,
		// SCREEN_SK_BLENDMODE = 14
		Screen = 14,
		// OVERLAY_SK_BLENDMODE = 15
		Overlay = 15,
		// DARKEN_SK_BLENDMODE = 16
		Darken = 16,
		// LIGHTEN_SK_BLENDMODE = 17
		Lighten = 17,
		// COLORDODGE_SK_BLENDMODE = 18
		ColorDodge = 18,
		// COLORBURN_SK_BLENDMODE = 19
		ColorBurn = 19,
		// HARDLIGHT_SK_BLENDMODE = 20
		HardLight = 20,
		// SOFTLIGHT_SK_BLENDMODE = 21
		SoftLight = 21,
		// DIFFERENCE_SK_BLENDMODE = 22
		Difference = 22,
		// EXCLUSION_SK_BLENDMODE = 23
		Exclusion = 23,
		// MULTIPLY_SK_BLENDMODE = 24
		Multiply = 24,
		// HUE_SK_BLENDMODE = 25
		Hue = 25,
		// SATURATION_SK_BLENDMODE = 26
		Saturation = 26,
		// COLOR_SK_BLENDMODE = 27
		Color = 27,
		// LUMINOSITY_SK_BLENDMODE = 28
		Luminosity = 28,
	}

	// sk_blurstyle_t
	public enum SKBlurStyle {
		// NORMAL_SK_BLUR_STYLE = 0
		Normal = 0,
		// SOLID_SK_BLUR_STYLE = 1
		Solid = 1,
		// OUTER_SK_BLUR_STYLE = 2
		Outer = 2,
		// INNER_SK_BLUR_STYLE = 3
		Inner = 3,
	}

	// sk_clipop_t
	public enum SKClipOperation {
		// DIFFERENCE_SK_CLIPOP = 0
		Difference = 0,
		// INTERSECT_SK_CLIPOP = 1
		Intersect = 1,
	}

	// sk_codec_result_t
	public enum SKCodecResult {
		// SUCCESS_SK_CODEC_RESULT = 0
		Success = 0,
		// INCOMPLETE_INPUT_SK_CODEC_RESULT = 1
		IncompleteInput = 1,
		// ERROR_IN_INPUT_SK_CODEC_RESULT = 2
		ErrorInInput = 2,
		// INVALID_CONVERSION_SK_CODEC_RESULT = 3
		InvalidConversion = 3,
		// INVALID_SCALE_SK_CODEC_RESULT = 4
		InvalidScale = 4,
		// INVALID_PARAMETERS_SK_CODEC_RESULT = 5
		InvalidParameters = 5,
		// INVALID_INPUT_SK_CODEC_RESULT = 6
		InvalidInput = 6,
		// COULD_NOT_REWIND_SK_CODEC_RESULT = 7
		CouldNotRewind = 7,
		// INTERNAL_ERROR_SK_CODEC_RESULT = 8
		InternalError = 8,
		// UNIMPLEMENTED_SK_CODEC_RESULT = 9
		Unimplemented = 9,
	}

	// sk_codec_scanline_order_t
	public enum SKCodecScanlineOrder {
		// TOP_DOWN_SK_CODEC_SCANLINE_ORDER = 0
		TopDown = 0,
		// BOTTOM_UP_SK_CODEC_SCANLINE_ORDER = 1
		BottomUp = 1,
	}

	// sk_codec_zero_initialized_t
	public enum SKZeroInitialized {
		// YES_SK_CODEC_ZERO_INITIALIZED = 0
		Yes = 0,
		// NO_SK_CODEC_ZERO_INITIALIZED = 1
		No = 1,
	}

	// sk_codecanimation_disposalmethod_t
	public enum SKCodecAnimationDisposalMethod {
		// KEEP_SK_CODEC_ANIMATION_DISPOSAL_METHOD = 1
		Keep = 1,
		// RESTORE_BG_COLOR_SK_CODEC_ANIMATION_DISPOSAL_METHOD = 2
		RestoreBackgroundColor = 2,
		// RESTORE_PREVIOUS_SK_CODEC_ANIMATION_DISPOSAL_METHOD = 3
		RestorePrevious = 3,
	}

	// sk_colorspace_gamut_t
	public enum SKColorSpaceGamut {
		// SRGB_SK_COLORSPACE_GAMUT = 0
		Srgb = 0,
		// ADOBE_RGB_SK_COLORSPACE_GAMUT = 1
		AdobeRgb = 1,
		// DCIP3_D65_SK_COLORSPACE_GAMUT = 2
		Dcip3D65 = 2,
		// REC2020_SK_COLORSPACE_GAMUT = 3
		Rec2020 = 3,
	}

	// sk_colorspace_render_target_gamma_t
	public enum SKColorSpaceRenderTargetGamma {
		// LINEAR_SK_COLORSPACE_RENDER_TARGET_GAMMA = 0
		Linear = 0,
		// SRGB_SK_COLORSPACE_RENDER_TARGET_GAMMA = 1
		Srgb = 1,
	}

	// sk_colorspace_type_t
	public enum SKColorSpaceType {
		// RGB_SK_COLORSPACE_TYPE = 0
		Rgb = 0,
		// CMYK_SK_COLORSPACE_TYPE = 1
		Cmyk = 1,
		// GRAY_SK_COLORSPACE_TYPE = 2
		Gray = 2,
	}

	// sk_colortype_t
	public enum SKColorType {
		// UNKNOWN_SK_COLORTYPE = 0
		Unknown = 0,
		// ALPHA_8_SK_COLORTYPE = 1
		Alpha8 = 1,
		// RGB_565_SK_COLORTYPE = 2
		Rgb565 = 2,
		// ARGB_4444_SK_COLORTYPE = 3
		Argb4444 = 3,
		// RGBA_8888_SK_COLORTYPE = 4
		Rgba8888 = 4,
		// RGB_888X_SK_COLORTYPE = 5
		Rgb888x = 5,
		// BGRA_8888_SK_COLORTYPE = 6
		Bgra8888 = 6,
		// RGBA_1010102_SK_COLORTYPE = 7
		Rgba1010102 = 7,
		// RGB_101010X_SK_COLORTYPE = 8
		Rgb101010x = 8,
		// GRAY_8_SK_COLORTYPE = 9
		Gray8 = 9,
		// RGBA_F16_SK_COLORTYPE = 10
		RgbaF16 = 10,
	}

	// sk_crop_rect_flags_t
	[Flags]
	public enum SKCropRectFlags {
		// HAS_NONE_SK_CROP_RECT_FLAG = 0x00
		HasNone = 0,
		// HAS_LEFT_SK_CROP_RECT_FLAG = 0x01
		HasLeft = 1,
		// HAS_TOP_SK_CROP_RECT_FLAG = 0x02
		HasTop = 2,
		// HAS_WIDTH_SK_CROP_RECT_FLAG = 0x04
		HasWidth = 4,
		// HAS_HEIGHT_SK_CROP_RECT_FLAG = 0x08
		HasHeight = 8,
		// HAS_ALL_SK_CROP_RECT_FLAG = 0x0F
		HasAll = 15,
	}

	// sk_displacement_map_effect_channel_selector_type_t
	public enum SKDisplacementMapEffectChannelSelectorType {
		// UNKNOWN_SK_DISPLACEMENT_MAP_EFFECT_CHANNEL_SELECTOR_TYPE = 0
		Unknown = 0,
		// R_SK_DISPLACEMENT_MAP_EFFECT_CHANNEL_SELECTOR_TYPE = 1
		R = 1,
		// G_SK_DISPLACEMENT_MAP_EFFECT_CHANNEL_SELECTOR_TYPE = 2
		G = 2,
		// B_SK_DISPLACEMENT_MAP_EFFECT_CHANNEL_SELECTOR_TYPE = 3
		B = 3,
		// A_SK_DISPLACEMENT_MAP_EFFECT_CHANNEL_SELECTOR_TYPE = 4
		A = 4,
	}

	// sk_drop_shadow_image_filter_shadow_mode_t
	public enum SKDropShadowImageFilterShadowMode {
		// DRAW_SHADOW_AND_FOREGROUND_SK_DROP_SHADOW_IMAGE_FILTER_SHADOW_MODE = 0
		DrawShadowAndForeground = 0,
		// DRAW_SHADOW_ONLY_SK_DROP_SHADOW_IMAGE_FILTER_SHADOW_MODE = 1
		DrawShadowOnly = 1,
	}

	// sk_encoded_image_format_t
	public enum SKEncodedImageFormat {
		// BMP_SK_ENCODED_FORMAT = 0
		Bmp = 0,
		// GIF_SK_ENCODED_FORMAT = 1
		Gif = 1,
		// ICO_SK_ENCODED_FORMAT = 2
		Ico = 2,
		// JPEG_SK_ENCODED_FORMAT = 3
		Jpeg = 3,
		// PNG_SK_ENCODED_FORMAT = 4
		Png = 4,
		// WBMP_SK_ENCODED_FORMAT = 5
		Wbmp = 5,
		// WEBP_SK_ENCODED_FORMAT = 6
		Webp = 6,
		// PKM_SK_ENCODED_FORMAT = 7
		Pkm = 7,
		// KTX_SK_ENCODED_FORMAT = 8
		Ktx = 8,
		// ASTC_SK_ENCODED_FORMAT = 9
		Astc = 9,
		// DNG_SK_ENCODED_FORMAT = 10
		Dng = 10,
		// HEIF_SK_ENCODED_FORMAT = 11
		Heif = 11,
	}

	// sk_encodedorigin_t
	public enum SKEncodedOrigin {
		// TOP_LEFT_SK_ENCODED_ORIGIN = 1
		TopLeft = 1,
		// TOP_RIGHT_SK_ENCODED_ORIGIN = 2
		TopRight = 2,
		// BOTTOM_RIGHT_SK_ENCODED_ORIGIN = 3
		BottomRight = 3,
		// BOTTOM_LEFT_SK_ENCODED_ORIGIN = 4
		BottomLeft = 4,
		// LEFT_TOP_SK_ENCODED_ORIGIN = 5
		LeftTop = 5,
		// RIGHT_TOP_SK_ENCODED_ORIGIN = 6
		RightTop = 6,
		// RIGHT_BOTTOM_SK_ENCODED_ORIGIN = 7
		RightBottom = 7,
		// LEFT_BOTTOM_SK_ENCODED_ORIGIN = 8
		LeftBottom = 8,
		// DEFAULT_SK_ENCODED_ORIGIN = TOP_LEFT_SK_ENCODED_ORIGIN
		Default = 1,
	}

	// sk_encoding_t
	public enum SKEncoding {
		// UTF8_SK_ENCODING = 0
		Utf8 = 0,
		// UTF16_SK_ENCODING = 1
		Utf16 = 1,
		// UTF32_SK_ENCODING = 2
		Utf32 = 2,
	}

	// sk_filter_quality_t
	public enum SKFilterQuality {
		// NONE_SK_FILTER_QUALITY = 0
		None = 0,
		// LOW_SK_FILTER_QUALITY = 1
		Low = 1,
		// MEDIUM_SK_FILTER_QUALITY = 2
		Medium = 2,
		// HIGH_SK_FILTER_QUALITY = 3
		High = 3,
	}

	// sk_font_style_slant_t
	public enum SKFontStyleSlant {
		// UPRIGHT_SK_FONT_STYLE_SLANT = 0
		Upright = 0,
		// ITALIC_SK_FONT_STYLE_SLANT = 1
		Italic = 1,
		// OBLIQUE_SK_FONT_STYLE_SLANT = 2
		Oblique = 2,
	}

	// sk_gamma_named_t
	public enum SKNamedGamma {
		// LINEAR_SK_GAMMA_NAMED = 0
		Linear = 0,
		// SRGB_SK_GAMMA_NAMED = 1
		Srgb = 1,
		// TWO_DOT_TWO_CURVE_SK_GAMMA_NAMED = 2
		TwoDotTwoCurve = 2,
		// NON_STANDARD_SK_GAMMA_NAMED = 3
		NonStandard = 3,
	}

	// sk_highcontrastconfig_invertstyle_t
	public enum SKHighContrastConfigInvertStyle {
		// NO_INVERT_SK_HIGH_CONTRAST_CONFIG_INVERT_STYLE = 0
		NoInvert = 0,
		// INVERT_BRIGHTNESS_SK_HIGH_CONTRAST_CONFIG_INVERT_STYLE = 1
		InvertBrightness = 1,
		// INVERT_LIGHTNESS_SK_HIGH_CONTRAST_CONFIG_INVERT_STYLE = 2
		InvertLightness = 2,
	}

	// sk_image_caching_hint_t
	public enum SKImageCachingHint {
		// ALLOW_SK_IMAGE_CACHING_HINT = 0
		Allow = 0,
		// DISALLOW_SK_IMAGE_CACHING_HINT = 1
		Disallow = 1,
	}

	// sk_jpegencoder_alphaoption_t
	public enum SKJpegEncoderAlphaOption {
		// IGNORE_SK_JPEGENCODER_ALPHA_OPTION = 0
		Ignore = 0,
		// BLEND_ON_BLACK_SK_JPEGENCODER_ALPHA_OPTION = 1
		BlendOnBlack = 1,
	}

	// sk_jpegencoder_downsample_t
	public enum SKJpegEncoderDownsample {
		// DOWNSAMPLE_420_SK_JPEGENCODER_DOWNSAMPLE = 0
		Downsample420 = 0,
		// DOWNSAMPLE_422_SK_JPEGENCODER_DOWNSAMPLE = 1
		Downsample422 = 1,
		// DOWNSAMPLE_444_SK_JPEGENCODER_DOWNSAMPLE = 2
		Downsample444 = 2,
	}

	// sk_lattice_recttype_t
	public enum SKLatticeRectType {
		// DEFAULT_SK_LATTICE_RECT_TYPE = 0
		Default = 0,
		// TRANSPARENT_SK_LATTICE_RECT_TYPE = 1
		Transparent = 1,
		// FIXED_COLOR_SK_LATTICE_RECT_TYPE = 2
		FixedColor = 2,
	}

	// sk_mask_format_t
	public enum SKMaskFormat {
		// BW_SK_MASK_FORMAT = 0
		BW = 0,
		// A8_SK_MASK_FORMAT = 1
		A8 = 1,
		// THREE_D_SK_MASK_FORMAT = 2
		ThreeD = 2,
		// ARGB32_SK_MASK_FORMAT = 3
		Argb32 = 3,
		// LCD16_SK_MASK_FORMAT = 4
		Lcd16 = 4,
	}

	// sk_matrix_convolution_tilemode_t
	public enum SKMatrixConvolutionTileMode {
		// CLAMP_SK_MATRIX_CONVOLUTION_TILEMODE = 0
		Clamp = 0,
		// REPEAT_SK_MATRIX_CONVOLUTION_TILEMODE = 1
		Repeat = 1,
		// CLAMP_TO_BLACK_SK_MATRIX_CONVOLUTION_TILEMODE = 2
		ClampToBlack = 2,
	}

	// sk_matrix44_type_mask_t
	[Flags]
	public enum SKMatrix44TypeMask {
		// IDENTITY_SK_MATRIX44_TYPE_MASK = 0
		Identity = 0,
		// TRANSLATE_SK_MATRIX44_TYPE_MASK = 0x01
		Translate = 1,
		// SCALE_SK_MATRIX44_TYPE_MASK = 0x02
		Scale = 2,
		// AFFINE_SK_MATRIX44_TYPE_MASK = 0x04
		Affine = 4,
		// PERSPECTIVE_SK_MATRIX44_TYPE_MASK = 0x08
		Perspective = 8,
	}

	// sk_paint_hinting_t
	public enum SKPaintHinting {
		// NO_HINTING_SK_PAINT_HINTING = 0
		NoHinting = 0,
		// SLIGHT_HINTING_SK_PAINT_HINTING = 1
		Slight = 1,
		// NORMAL_HINTING_SK_PAINT_HINTING = 2
		Normal = 2,
		// FULL_HINTING_SK_PAINT_HINTING = 3
		Full = 3,
	}

	// sk_paint_style_t
	public enum SKPaintStyle {
		// FILL_SK_PAINT_STYLE = 0
		Fill = 0,
		// STROKE_SK_PAINT_STYLE = 1
		Stroke = 1,
		// STROKE_AND_FILL_SK_PAINT_STYLE = 2
		StrokeAndFill = 2,
	}

	// sk_path_add_mode_t
	public enum SKPathAddMode {
		// APPEND_SK_PATH_ADD_MODE = 0
		Append = 0,
		// EXTEND_SK_PATH_ADD_MODE = 1
		Extend = 1,
	}

	// sk_path_arc_size_t
	public enum SKPathArcSize {
		// SMALL_SK_PATH_ARC_SIZE = 0
		Small = 0,
		// LARGE_SK_PATH_ARC_SIZE = 1
		Large = 1,
	}

	// sk_path_convexity_t
	public enum SKPathConvexity {
		// UNKNOWN_SK_PATH_CONVEXITY = 0
		Unknown = 0,
		// CONVEX_SK_PATH_CONVEXITY = 1
		Convex = 1,
		// CONCAVE_SK_PATH_CONVEXITY = 2
		Concave = 2,
	}

	// sk_path_direction_t
	public enum SKPathDirection {
		// CW_SK_PATH_DIRECTION = 0
		Clockwise = 0,
		// CCW_SK_PATH_DIRECTION = 1
		CounterClockwise = 1,
	}

	// sk_path_effect_1d_style_t
	public enum SKPath1DPathEffectStyle {
		// TRANSLATE_SK_PATH_EFFECT_1D_STYLE = 0
		Translate = 0,
		// ROTATE_SK_PATH_EFFECT_1D_STYLE = 1
		Rotate = 1,
		// MORPH_SK_PATH_EFFECT_1D_STYLE = 2
		Morph = 2,
	}

	// sk_path_effect_trim_mode_t
	public enum SKTrimPathEffectMode {
		// NORMAL_SK_PATH_EFFECT_TRIM_MODE = 0
		Normal = 0,
		// INVERTED_SK_PATH_EFFECT_TRIM_MODE = 1
		Inverted = 1,
	}

	// sk_path_filltype_t
	public enum SKPathFillType {
		// WINDING_SK_PATH_FILLTYPE = 0
		Winding = 0,
		// EVENODD_SK_PATH_FILLTYPE = 1
		EvenOdd = 1,
		// INVERSE_WINDING_SK_PATH_FILLTYPE = 2
		InverseWinding = 2,
		// INVERSE_EVENODD_SK_PATH_FILLTYPE = 3
		InverseEvenOdd = 3,
	}

	// sk_path_segment_mask_t
	[Flags]
	public enum SKPathSegmentMask {
		// LINE_SK_PATH_SEGMENT_MASK = 1 << 0
		Line = 1,
		// QUAD_SK_PATH_SEGMENT_MASK = 1 << 1
		Quad = 2,
		// CONIC_SK_PATH_SEGMENT_MASK = 1 << 2
		Conic = 4,
		// CUBIC_SK_PATH_SEGMENT_MASK = 1 << 3
		Cubic = 8,
	}

	// sk_path_verb_t
	public enum SKPathVerb {
		// MOVE_SK_PATH_VERB = 0
		Move = 0,
		// LINE_SK_PATH_VERB = 1
		Line = 1,
		// QUAD_SK_PATH_VERB = 2
		Quad = 2,
		// CONIC_SK_PATH_VERB = 3
		Conic = 3,
		// CUBIC_SK_PATH_VERB = 4
		Cubic = 4,
		// CLOSE_SK_PATH_VERB = 5
		Close = 5,
		// DONE_SK_PATH_VERB = 6
		Done = 6,
	}

	// sk_pathmeasure_matrixflags_t
	[Flags]
	public enum SKPathMeasureMatrixFlags {
		// GET_POSITION_SK_PATHMEASURE_MATRIXFLAGS = 0x01
		GetPosition = 1,
		// GET_TANGENT_SK_PATHMEASURE_MATRIXFLAGS = 0x02
		GetTangent = 2,
		// GET_POS_AND_TAN_SK_PATHMEASURE_MATRIXFLAGS = GET_POSITION_SK_PATHMEASURE_MATRIXFLAGS | GET_TANGENT_SK_PATHMEASURE_MATRIXFLAGS
		GetPositionAndTangent = 3,
	}

	// sk_pathop_t
	public enum SKPathOp {
		// DIFFERENCE_SK_PATHOP = 0
		Difference = 0,
		// INTERSECT_SK_PATHOP = 1
		Intersect = 1,
		// UNION_SK_PATHOP = 2
		Union = 2,
		// XOR_SK_PATHOP = 3
		Xor = 3,
		// REVERSE_DIFFERENCE_SK_PATHOP = 4
		ReverseDifference = 4,
	}

	// sk_pixelgeometry_t
	public enum SKPixelGeometry {
		// UNKNOWN_SK_PIXELGEOMETRY = 0
		Unknown = 0,
		// RGB_H_SK_PIXELGEOMETRY = 1
		RgbHorizontal = 1,
		// BGR_H_SK_PIXELGEOMETRY = 2
		BgrHorizontal = 2,
		// RGB_V_SK_PIXELGEOMETRY = 3
		RgbVertical = 3,
		// BGR_V_SK_PIXELGEOMETRY = 4
		BgrVertical = 4,
	}

	// sk_pngencoder_filterflags_t
	[Flags]
	public enum SKPngEncoderFilterFlags {
		// ZERO_SK_PNGENCODER_FILTER_FLAGS = 0x00
		NoFilters = 0,
		// NONE_SK_PNGENCODER_FILTER_FLAGS = 0x08
		None = 8,
		// SUB_SK_PNGENCODER_FILTER_FLAGS = 0x10
		Sub = 16,
		// UP_SK_PNGENCODER_FILTER_FLAGS = 0x20
		Up = 32,
		// AVG_SK_PNGENCODER_FILTER_FLAGS = 0x40
		Avg = 64,
		// PAETH_SK_PNGENCODER_FILTER_FLAGS = 0x80
		Paeth = 128,
		// ALL_SK_PNGENCODER_FILTER_FLAGS = NONE_SK_PNGENCODER_FILTER_FLAGS | SUB_SK_PNGENCODER_FILTER_FLAGS | UP_SK_PNGENCODER_FILTER_FLAGS | AVG_SK_PNGENCODER_FILTER_FLAGS | PAETH_SK_PNGENCODER_FILTER_FLAGS
		AllFilters = 248,
	}

	// sk_point_mode_t
	public enum SKPointMode {
		// POINTS_SK_POINT_MODE = 0
		Points = 0,
		// LINES_SK_POINT_MODE = 1
		Lines = 1,
		// POLYGON_SK_POINT_MODE = 2
		Polygon = 2,
	}

	// sk_region_op_t
	public enum SKRegionOperation {
		// DIFFERENCE_SK_REGION_OP = 0
		Difference = 0,
		// INTERSECT_SK_REGION_OP = 1
		Intersect = 1,
		// UNION_SK_REGION_OP = 2
		Union = 2,
		// XOR_SK_REGION_OP = 3
		XOR = 3,
		// REVERSE_DIFFERENCE_SK_REGION_OP = 4
		ReverseDifference = 4,
		// REPLACE_SK_REGION_OP = 5
		Replace = 5,
	}

	// sk_rrect_corner_t
	public enum SKRoundRectCorner {
		// UPPER_LEFT_SK_RRECT_CORNER = 0
		UpperLeft = 0,
		// UPPER_RIGHT_SK_RRECT_CORNER = 1
		UpperRight = 1,
		// LOWER_RIGHT_SK_RRECT_CORNER = 2
		LowerRight = 2,
		// LOWER_LEFT_SK_RRECT_CORNER = 3
		LowerLeft = 3,
	}

	// sk_rrect_type_t
	public enum SKRoundRectType {
		// EMPTY_SK_RRECT_TYPE = 0
		Empty = 0,
		// RECT_SK_RRECT_TYPE = 1
		Rect = 1,
		// OVAL_SK_RRECT_TYPE = 2
		Oval = 2,
		// SIMPLE_SK_RRECT_TYPE = 3
		Simple = 3,
		// NINE_PATCH_SK_RRECT_TYPE = 4
		NinePatch = 4,
		// COMPLEX_SK_RRECT_TYPE = 5
		Complex = 5,
	}

	// sk_shader_tilemode_t
	public enum SKShaderTileMode {
		// CLAMP_SK_SHADER_TILEMODE = 0
		Clamp = 0,
		// REPEAT_SK_SHADER_TILEMODE = 1
		Repeat = 1,
		// MIRROR_SK_SHADER_TILEMODE = 2
		Mirror = 2,
	}

	// sk_stroke_cap_t
	public enum SKStrokeCap {
		// BUTT_SK_STROKE_CAP = 0
		Butt = 0,
		// ROUND_SK_STROKE_CAP = 1
		Round = 1,
		// SQUARE_SK_STROKE_CAP = 2
		Square = 2,
	}

	// sk_stroke_join_t
	public enum SKStrokeJoin {
		// MITER_SK_STROKE_JOIN = 0
		Miter = 0,
		// ROUND_SK_STROKE_JOIN = 1
		Round = 1,
		// BEVEL_SK_STROKE_JOIN = 2
		Bevel = 2,
	}

	// sk_surfaceprops_flags_t
	[Flags]
	public enum SKSurfacePropsFlags {
		// NONE_SK_SURFACE_PROPS_FLAGS = 0
		None = 0,
		// USE_DEVICE_INDEPENDENT_FONTS_SK_SURFACE_PROPS_FLAGS = 1 << 0
		UseDeviceIndependentFonts = 1,
	}

	// sk_text_align_t
	public enum SKTextAlign {
		// LEFT_SK_TEXT_ALIGN = 0
		Left = 0,
		// CENTER_SK_TEXT_ALIGN = 1
		Center = 1,
		// RIGHT_SK_TEXT_ALIGN = 2
		Right = 2,
	}

	// sk_text_encoding_t
	public enum SKTextEncoding {
		// UTF8_SK_TEXT_ENCODING = 0
		Utf8 = 0,
		// UTF16_SK_TEXT_ENCODING = 1
		Utf16 = 1,
		// UTF32_SK_TEXT_ENCODING = 2
		Utf32 = 2,
		// GLYPH_ID_SK_TEXT_ENCODING = 3
		GlyphId = 3,
	}

	// sk_transfer_function_behavior_t
	public enum SKTransferFunctionBehavior {
		// RESPECT_SK_TRANSFER_FUNCTION_BEHAVIOR = 0
		Respect = 0,
		// IGNORE_SK_TRANSFER_FUNCTION_BEHAVIOR = 1
		Ignore = 1,
	}

	// sk_vertices_vertex_mode_t
	public enum SKVertexMode {
		// TRIANGLES_SK_VERTICES_VERTEX_MODE = 0
		Triangles = 0,
		// TRIANGLE_STRIP_SK_VERTICES_VERTEX_MODE = 1
		TriangleStrip = 1,
		// TRIANGLE_FAN_SK_VERTICES_VERTEX_MODE = 2
		TriangleFan = 2,
	}

	// sk_webpencoder_compression_t
	public enum SKWebpEncoderCompression {
		// LOSSY_SK_WEBPENCODER_COMPTRESSION = 0
		Lossy = 0,
		// LOSSLESS_SK_WEBPENCODER_COMPTRESSION = 1
		Lossless = 1,
	}

	#endregion
}

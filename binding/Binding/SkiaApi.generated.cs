using System;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#region Class declarations

using gr_backendrendertarget_t = System.IntPtr;
using gr_backendtexture_t = System.IntPtr;
using gr_direct_context_t = System.IntPtr;
using gr_glinterface_t = System.IntPtr;
using gr_recording_context_t = System.IntPtr;
using gr_vk_extensions_t = System.IntPtr;
using gr_vk_memory_allocator_t = System.IntPtr;
using gr_vkinterface_t = System.IntPtr;
using sk_3dview_t = System.IntPtr;
using sk_bitmap_t = System.IntPtr;
using sk_canvas_t = System.IntPtr;
using sk_codec_t = System.IntPtr;
using sk_colorfilter_t = System.IntPtr;
using sk_colorspace_icc_profile_t = System.IntPtr;
using sk_colorspace_t = System.IntPtr;
using sk_colortable_t = System.IntPtr;
using sk_compatpaint_t = System.IntPtr;
using sk_data_t = System.IntPtr;
using sk_document_t = System.IntPtr;
using sk_drawable_t = System.IntPtr;
using sk_font_t = System.IntPtr;
using sk_fontmgr_t = System.IntPtr;
using sk_fontstyle_t = System.IntPtr;
using sk_fontstyleset_t = System.IntPtr;
using sk_image_t = System.IntPtr;
using sk_imagefilter_croprect_t = System.IntPtr;
using sk_imagefilter_t = System.IntPtr;
using sk_manageddrawable_t = System.IntPtr;
using sk_managedtracememorydump_t = System.IntPtr;
using sk_maskfilter_t = System.IntPtr;
using sk_matrix44_t = System.IntPtr;
using sk_nodraw_canvas_t = System.IntPtr;
using sk_nvrefcnt_t = System.IntPtr;
using sk_nway_canvas_t = System.IntPtr;
using sk_opbuilder_t = System.IntPtr;
using sk_overdraw_canvas_t = System.IntPtr;
using sk_paint_t = System.IntPtr;
using sk_path_effect_t = System.IntPtr;
using sk_path_iterator_t = System.IntPtr;
using sk_path_rawiterator_t = System.IntPtr;
using sk_path_t = System.IntPtr;
using sk_pathmeasure_t = System.IntPtr;
using sk_picture_recorder_t = System.IntPtr;
using sk_picture_t = System.IntPtr;
using sk_pixelref_factory_t = System.IntPtr;
using sk_pixmap_t = System.IntPtr;
using sk_refcnt_t = System.IntPtr;
using sk_region_cliperator_t = System.IntPtr;
using sk_region_iterator_t = System.IntPtr;
using sk_region_spanerator_t = System.IntPtr;
using sk_region_t = System.IntPtr;
using sk_rrect_t = System.IntPtr;
using sk_runtimeeffect_t = System.IntPtr;
using sk_runtimeeffect_uniform_t = System.IntPtr;
using sk_shader_t = System.IntPtr;
using sk_stream_asset_t = System.IntPtr;
using sk_stream_filestream_t = System.IntPtr;
using sk_stream_managedstream_t = System.IntPtr;
using sk_stream_memorystream_t = System.IntPtr;
using sk_stream_streamrewindable_t = System.IntPtr;
using sk_stream_t = System.IntPtr;
using sk_string_t = System.IntPtr;
using sk_surface_t = System.IntPtr;
using sk_surfaceprops_t = System.IntPtr;
using sk_svgcanvas_t = System.IntPtr;
using sk_textblob_builder_t = System.IntPtr;
using sk_textblob_t = System.IntPtr;
using sk_tracememorydump_t = System.IntPtr;
using sk_typeface_t = System.IntPtr;
using sk_vertices_t = System.IntPtr;
using sk_wstream_dynamicmemorystream_t = System.IntPtr;
using sk_wstream_filestream_t = System.IntPtr;
using sk_wstream_managedstream_t = System.IntPtr;
using sk_wstream_t = System.IntPtr;
using sk_xmlstreamwriter_t = System.IntPtr;
using sk_xmlwriter_t = System.IntPtr;
using skottie_animation_builder_t = System.IntPtr;
using skottie_animation_t = System.IntPtr;
using skottie_logger_t = System.IntPtr;
using skottie_marker_observer_t = System.IntPtr;
using skottie_property_observer_t = System.IntPtr;
using skottie_resource_provider_t = System.IntPtr;
using sksg_invalidation_controller_t = System.IntPtr;
using vk_device_t = System.IntPtr;
using vk_instance_t = System.IntPtr;
using vk_physical_device_features_2_t = System.IntPtr;
using vk_physical_device_features_t = System.IntPtr;
using vk_physical_device_t = System.IntPtr;
using vk_queue_t = System.IntPtr;

#endregion

#region Functions

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region gr_context.h

		// void gr_backendrendertarget_delete(gr_backendrendertarget_t* rendertarget)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_backendrendertarget_delete (gr_backendrendertarget_t rendertarget);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_backendrendertarget_delete (gr_backendrendertarget_t rendertarget);
		}
		private static Delegates.gr_backendrendertarget_delete gr_backendrendertarget_delete_delegate;
		internal static void gr_backendrendertarget_delete (gr_backendrendertarget_t rendertarget) =>
			(gr_backendrendertarget_delete_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_delete> ("gr_backendrendertarget_delete")).Invoke (rendertarget);
		#endif

		// gr_backend_t gr_backendrendertarget_get_backend(const gr_backendrendertarget_t* rendertarget)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GRBackendNative gr_backendrendertarget_get_backend (gr_backendrendertarget_t rendertarget);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate GRBackendNative gr_backendrendertarget_get_backend (gr_backendrendertarget_t rendertarget);
		}
		private static Delegates.gr_backendrendertarget_get_backend gr_backendrendertarget_get_backend_delegate;
		internal static GRBackendNative gr_backendrendertarget_get_backend (gr_backendrendertarget_t rendertarget) =>
			(gr_backendrendertarget_get_backend_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_get_backend> ("gr_backendrendertarget_get_backend")).Invoke (rendertarget);
		#endif

		// bool gr_backendrendertarget_get_gl_framebufferinfo(const gr_backendrendertarget_t* rendertarget, gr_gl_framebufferinfo_t* glInfo)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendrendertarget_get_gl_framebufferinfo (gr_backendrendertarget_t rendertarget, GRGlFramebufferInfo* glInfo);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_backendrendertarget_get_gl_framebufferinfo (gr_backendrendertarget_t rendertarget, GRGlFramebufferInfo* glInfo);
		}
		private static Delegates.gr_backendrendertarget_get_gl_framebufferinfo gr_backendrendertarget_get_gl_framebufferinfo_delegate;
		internal static bool gr_backendrendertarget_get_gl_framebufferinfo (gr_backendrendertarget_t rendertarget, GRGlFramebufferInfo* glInfo) =>
			(gr_backendrendertarget_get_gl_framebufferinfo_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_get_gl_framebufferinfo> ("gr_backendrendertarget_get_gl_framebufferinfo")).Invoke (rendertarget, glInfo);
		#endif

		// int gr_backendrendertarget_get_height(const gr_backendrendertarget_t* rendertarget)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendrendertarget_get_height (gr_backendrendertarget_t rendertarget);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_backendrendertarget_get_height (gr_backendrendertarget_t rendertarget);
		}
		private static Delegates.gr_backendrendertarget_get_height gr_backendrendertarget_get_height_delegate;
		internal static Int32 gr_backendrendertarget_get_height (gr_backendrendertarget_t rendertarget) =>
			(gr_backendrendertarget_get_height_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_get_height> ("gr_backendrendertarget_get_height")).Invoke (rendertarget);
		#endif

		// int gr_backendrendertarget_get_samples(const gr_backendrendertarget_t* rendertarget)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendrendertarget_get_samples (gr_backendrendertarget_t rendertarget);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_backendrendertarget_get_samples (gr_backendrendertarget_t rendertarget);
		}
		private static Delegates.gr_backendrendertarget_get_samples gr_backendrendertarget_get_samples_delegate;
		internal static Int32 gr_backendrendertarget_get_samples (gr_backendrendertarget_t rendertarget) =>
			(gr_backendrendertarget_get_samples_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_get_samples> ("gr_backendrendertarget_get_samples")).Invoke (rendertarget);
		#endif

		// int gr_backendrendertarget_get_stencils(const gr_backendrendertarget_t* rendertarget)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendrendertarget_get_stencils (gr_backendrendertarget_t rendertarget);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_backendrendertarget_get_stencils (gr_backendrendertarget_t rendertarget);
		}
		private static Delegates.gr_backendrendertarget_get_stencils gr_backendrendertarget_get_stencils_delegate;
		internal static Int32 gr_backendrendertarget_get_stencils (gr_backendrendertarget_t rendertarget) =>
			(gr_backendrendertarget_get_stencils_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_get_stencils> ("gr_backendrendertarget_get_stencils")).Invoke (rendertarget);
		#endif

		// int gr_backendrendertarget_get_width(const gr_backendrendertarget_t* rendertarget)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendrendertarget_get_width (gr_backendrendertarget_t rendertarget);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_backendrendertarget_get_width (gr_backendrendertarget_t rendertarget);
		}
		private static Delegates.gr_backendrendertarget_get_width gr_backendrendertarget_get_width_delegate;
		internal static Int32 gr_backendrendertarget_get_width (gr_backendrendertarget_t rendertarget) =>
			(gr_backendrendertarget_get_width_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_get_width> ("gr_backendrendertarget_get_width")).Invoke (rendertarget);
		#endif

		// bool gr_backendrendertarget_is_valid(const gr_backendrendertarget_t* rendertarget)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendrendertarget_is_valid (gr_backendrendertarget_t rendertarget);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_backendrendertarget_is_valid (gr_backendrendertarget_t rendertarget);
		}
		private static Delegates.gr_backendrendertarget_is_valid gr_backendrendertarget_is_valid_delegate;
		internal static bool gr_backendrendertarget_is_valid (gr_backendrendertarget_t rendertarget) =>
			(gr_backendrendertarget_is_valid_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_is_valid> ("gr_backendrendertarget_is_valid")).Invoke (rendertarget);
		#endif

		// gr_backendrendertarget_t* gr_backendrendertarget_new_gl(int width, int height, int samples, int stencils, const gr_gl_framebufferinfo_t* glInfo)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_backendrendertarget_t gr_backendrendertarget_new_gl (Int32 width, Int32 height, Int32 samples, Int32 stencils, GRGlFramebufferInfo* glInfo);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_backendrendertarget_t gr_backendrendertarget_new_gl (Int32 width, Int32 height, Int32 samples, Int32 stencils, GRGlFramebufferInfo* glInfo);
		}
		private static Delegates.gr_backendrendertarget_new_gl gr_backendrendertarget_new_gl_delegate;
		internal static gr_backendrendertarget_t gr_backendrendertarget_new_gl (Int32 width, Int32 height, Int32 samples, Int32 stencils, GRGlFramebufferInfo* glInfo) =>
			(gr_backendrendertarget_new_gl_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_new_gl> ("gr_backendrendertarget_new_gl")).Invoke (width, height, samples, stencils, glInfo);
		#endif

		// gr_backendrendertarget_t* gr_backendrendertarget_new_metal(int width, int height, int samples, const gr_mtl_textureinfo_t* mtlInfo)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_backendrendertarget_t gr_backendrendertarget_new_metal (Int32 width, Int32 height, Int32 samples, GRMtlTextureInfoNative* mtlInfo);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_backendrendertarget_t gr_backendrendertarget_new_metal (Int32 width, Int32 height, Int32 samples, GRMtlTextureInfoNative* mtlInfo);
		}
		private static Delegates.gr_backendrendertarget_new_metal gr_backendrendertarget_new_metal_delegate;
		internal static gr_backendrendertarget_t gr_backendrendertarget_new_metal (Int32 width, Int32 height, Int32 samples, GRMtlTextureInfoNative* mtlInfo) =>
			(gr_backendrendertarget_new_metal_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_new_metal> ("gr_backendrendertarget_new_metal")).Invoke (width, height, samples, mtlInfo);
		#endif

		// gr_backendrendertarget_t* gr_backendrendertarget_new_vulkan(int width, int height, int samples, const gr_vk_imageinfo_t* vkImageInfo)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_backendrendertarget_t gr_backendrendertarget_new_vulkan (Int32 width, Int32 height, Int32 samples, GRVkImageInfo* vkImageInfo);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_backendrendertarget_t gr_backendrendertarget_new_vulkan (Int32 width, Int32 height, Int32 samples, GRVkImageInfo* vkImageInfo);
		}
		private static Delegates.gr_backendrendertarget_new_vulkan gr_backendrendertarget_new_vulkan_delegate;
		internal static gr_backendrendertarget_t gr_backendrendertarget_new_vulkan (Int32 width, Int32 height, Int32 samples, GRVkImageInfo* vkImageInfo) =>
			(gr_backendrendertarget_new_vulkan_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_new_vulkan> ("gr_backendrendertarget_new_vulkan")).Invoke (width, height, samples, vkImageInfo);
		#endif

		// void gr_backendtexture_delete(gr_backendtexture_t* texture)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_backendtexture_delete (gr_backendtexture_t texture);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_backendtexture_delete (gr_backendtexture_t texture);
		}
		private static Delegates.gr_backendtexture_delete gr_backendtexture_delete_delegate;
		internal static void gr_backendtexture_delete (gr_backendtexture_t texture) =>
			(gr_backendtexture_delete_delegate ??= GetSymbol<Delegates.gr_backendtexture_delete> ("gr_backendtexture_delete")).Invoke (texture);
		#endif

		// gr_backend_t gr_backendtexture_get_backend(const gr_backendtexture_t* texture)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GRBackendNative gr_backendtexture_get_backend (gr_backendtexture_t texture);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate GRBackendNative gr_backendtexture_get_backend (gr_backendtexture_t texture);
		}
		private static Delegates.gr_backendtexture_get_backend gr_backendtexture_get_backend_delegate;
		internal static GRBackendNative gr_backendtexture_get_backend (gr_backendtexture_t texture) =>
			(gr_backendtexture_get_backend_delegate ??= GetSymbol<Delegates.gr_backendtexture_get_backend> ("gr_backendtexture_get_backend")).Invoke (texture);
		#endif

		// bool gr_backendtexture_get_gl_textureinfo(const gr_backendtexture_t* texture, gr_gl_textureinfo_t* glInfo)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendtexture_get_gl_textureinfo (gr_backendtexture_t texture, GRGlTextureInfo* glInfo);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_backendtexture_get_gl_textureinfo (gr_backendtexture_t texture, GRGlTextureInfo* glInfo);
		}
		private static Delegates.gr_backendtexture_get_gl_textureinfo gr_backendtexture_get_gl_textureinfo_delegate;
		internal static bool gr_backendtexture_get_gl_textureinfo (gr_backendtexture_t texture, GRGlTextureInfo* glInfo) =>
			(gr_backendtexture_get_gl_textureinfo_delegate ??= GetSymbol<Delegates.gr_backendtexture_get_gl_textureinfo> ("gr_backendtexture_get_gl_textureinfo")).Invoke (texture, glInfo);
		#endif

		// int gr_backendtexture_get_height(const gr_backendtexture_t* texture)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendtexture_get_height (gr_backendtexture_t texture);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_backendtexture_get_height (gr_backendtexture_t texture);
		}
		private static Delegates.gr_backendtexture_get_height gr_backendtexture_get_height_delegate;
		internal static Int32 gr_backendtexture_get_height (gr_backendtexture_t texture) =>
			(gr_backendtexture_get_height_delegate ??= GetSymbol<Delegates.gr_backendtexture_get_height> ("gr_backendtexture_get_height")).Invoke (texture);
		#endif

		// int gr_backendtexture_get_width(const gr_backendtexture_t* texture)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendtexture_get_width (gr_backendtexture_t texture);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_backendtexture_get_width (gr_backendtexture_t texture);
		}
		private static Delegates.gr_backendtexture_get_width gr_backendtexture_get_width_delegate;
		internal static Int32 gr_backendtexture_get_width (gr_backendtexture_t texture) =>
			(gr_backendtexture_get_width_delegate ??= GetSymbol<Delegates.gr_backendtexture_get_width> ("gr_backendtexture_get_width")).Invoke (texture);
		#endif

		// bool gr_backendtexture_has_mipmaps(const gr_backendtexture_t* texture)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendtexture_has_mipmaps (gr_backendtexture_t texture);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_backendtexture_has_mipmaps (gr_backendtexture_t texture);
		}
		private static Delegates.gr_backendtexture_has_mipmaps gr_backendtexture_has_mipmaps_delegate;
		internal static bool gr_backendtexture_has_mipmaps (gr_backendtexture_t texture) =>
			(gr_backendtexture_has_mipmaps_delegate ??= GetSymbol<Delegates.gr_backendtexture_has_mipmaps> ("gr_backendtexture_has_mipmaps")).Invoke (texture);
		#endif

		// bool gr_backendtexture_is_valid(const gr_backendtexture_t* texture)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendtexture_is_valid (gr_backendtexture_t texture);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_backendtexture_is_valid (gr_backendtexture_t texture);
		}
		private static Delegates.gr_backendtexture_is_valid gr_backendtexture_is_valid_delegate;
		internal static bool gr_backendtexture_is_valid (gr_backendtexture_t texture) =>
			(gr_backendtexture_is_valid_delegate ??= GetSymbol<Delegates.gr_backendtexture_is_valid> ("gr_backendtexture_is_valid")).Invoke (texture);
		#endif

		// gr_backendtexture_t* gr_backendtexture_new_gl(int width, int height, bool mipmapped, const gr_gl_textureinfo_t* glInfo)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_backendtexture_t gr_backendtexture_new_gl (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRGlTextureInfo* glInfo);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_backendtexture_t gr_backendtexture_new_gl (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRGlTextureInfo* glInfo);
		}
		private static Delegates.gr_backendtexture_new_gl gr_backendtexture_new_gl_delegate;
		internal static gr_backendtexture_t gr_backendtexture_new_gl (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRGlTextureInfo* glInfo) =>
			(gr_backendtexture_new_gl_delegate ??= GetSymbol<Delegates.gr_backendtexture_new_gl> ("gr_backendtexture_new_gl")).Invoke (width, height, mipmapped, glInfo);
		#endif

		// gr_backendtexture_t* gr_backendtexture_new_metal(int width, int height, bool mipmapped, const gr_mtl_textureinfo_t* mtlInfo)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_backendtexture_t gr_backendtexture_new_metal (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRMtlTextureInfoNative* mtlInfo);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_backendtexture_t gr_backendtexture_new_metal (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRMtlTextureInfoNative* mtlInfo);
		}
		private static Delegates.gr_backendtexture_new_metal gr_backendtexture_new_metal_delegate;
		internal static gr_backendtexture_t gr_backendtexture_new_metal (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRMtlTextureInfoNative* mtlInfo) =>
			(gr_backendtexture_new_metal_delegate ??= GetSymbol<Delegates.gr_backendtexture_new_metal> ("gr_backendtexture_new_metal")).Invoke (width, height, mipmapped, mtlInfo);
		#endif

		// gr_backendtexture_t* gr_backendtexture_new_vulkan(int width, int height, const gr_vk_imageinfo_t* vkInfo)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_backendtexture_t gr_backendtexture_new_vulkan (Int32 width, Int32 height, GRVkImageInfo* vkInfo);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_backendtexture_t gr_backendtexture_new_vulkan (Int32 width, Int32 height, GRVkImageInfo* vkInfo);
		}
		private static Delegates.gr_backendtexture_new_vulkan gr_backendtexture_new_vulkan_delegate;
		internal static gr_backendtexture_t gr_backendtexture_new_vulkan (Int32 width, Int32 height, GRVkImageInfo* vkInfo) =>
			(gr_backendtexture_new_vulkan_delegate ??= GetSymbol<Delegates.gr_backendtexture_new_vulkan> ("gr_backendtexture_new_vulkan")).Invoke (width, height, vkInfo);
		#endif

		// void gr_direct_context_abandon_context(gr_direct_context_t* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_abandon_context (gr_direct_context_t context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_abandon_context (gr_direct_context_t context);
		}
		private static Delegates.gr_direct_context_abandon_context gr_direct_context_abandon_context_delegate;
		internal static void gr_direct_context_abandon_context (gr_direct_context_t context) =>
			(gr_direct_context_abandon_context_delegate ??= GetSymbol<Delegates.gr_direct_context_abandon_context> ("gr_direct_context_abandon_context")).Invoke (context);
		#endif

		// void gr_direct_context_dump_memory_statistics(const gr_direct_context_t* context, sk_tracememorydump_t* dump)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_dump_memory_statistics (gr_direct_context_t context, sk_tracememorydump_t dump);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_dump_memory_statistics (gr_direct_context_t context, sk_tracememorydump_t dump);
		}
		private static Delegates.gr_direct_context_dump_memory_statistics gr_direct_context_dump_memory_statistics_delegate;
		internal static void gr_direct_context_dump_memory_statistics (gr_direct_context_t context, sk_tracememorydump_t dump) =>
			(gr_direct_context_dump_memory_statistics_delegate ??= GetSymbol<Delegates.gr_direct_context_dump_memory_statistics> ("gr_direct_context_dump_memory_statistics")).Invoke (context, dump);
		#endif

		// void gr_direct_context_flush(gr_direct_context_t* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_flush (gr_direct_context_t context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_flush (gr_direct_context_t context);
		}
		private static Delegates.gr_direct_context_flush gr_direct_context_flush_delegate;
		internal static void gr_direct_context_flush (gr_direct_context_t context) =>
			(gr_direct_context_flush_delegate ??= GetSymbol<Delegates.gr_direct_context_flush> ("gr_direct_context_flush")).Invoke (context);
		#endif

		// void gr_direct_context_flush_and_submit(gr_direct_context_t* context, bool syncCpu)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_flush_and_submit (gr_direct_context_t context, [MarshalAs (UnmanagedType.I1)] bool syncCpu);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_flush_and_submit (gr_direct_context_t context, [MarshalAs (UnmanagedType.I1)] bool syncCpu);
		}
		private static Delegates.gr_direct_context_flush_and_submit gr_direct_context_flush_and_submit_delegate;
		internal static void gr_direct_context_flush_and_submit (gr_direct_context_t context, [MarshalAs (UnmanagedType.I1)] bool syncCpu) =>
			(gr_direct_context_flush_and_submit_delegate ??= GetSymbol<Delegates.gr_direct_context_flush_and_submit> ("gr_direct_context_flush_and_submit")).Invoke (context, syncCpu);
		#endif

		// void gr_direct_context_free_gpu_resources(gr_direct_context_t* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_free_gpu_resources (gr_direct_context_t context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_free_gpu_resources (gr_direct_context_t context);
		}
		private static Delegates.gr_direct_context_free_gpu_resources gr_direct_context_free_gpu_resources_delegate;
		internal static void gr_direct_context_free_gpu_resources (gr_direct_context_t context) =>
			(gr_direct_context_free_gpu_resources_delegate ??= GetSymbol<Delegates.gr_direct_context_free_gpu_resources> ("gr_direct_context_free_gpu_resources")).Invoke (context);
		#endif

		// size_t gr_direct_context_get_resource_cache_limit(gr_direct_context_t* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr gr_direct_context_get_resource_cache_limit (gr_direct_context_t context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr gr_direct_context_get_resource_cache_limit (gr_direct_context_t context);
		}
		private static Delegates.gr_direct_context_get_resource_cache_limit gr_direct_context_get_resource_cache_limit_delegate;
		internal static /* size_t */ IntPtr gr_direct_context_get_resource_cache_limit (gr_direct_context_t context) =>
			(gr_direct_context_get_resource_cache_limit_delegate ??= GetSymbol<Delegates.gr_direct_context_get_resource_cache_limit> ("gr_direct_context_get_resource_cache_limit")).Invoke (context);
		#endif

		// void gr_direct_context_get_resource_cache_usage(gr_direct_context_t* context, int* maxResources, size_t* maxResourceBytes)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_get_resource_cache_usage (gr_direct_context_t context, Int32* maxResources, /* size_t */ IntPtr* maxResourceBytes);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_get_resource_cache_usage (gr_direct_context_t context, Int32* maxResources, /* size_t */ IntPtr* maxResourceBytes);
		}
		private static Delegates.gr_direct_context_get_resource_cache_usage gr_direct_context_get_resource_cache_usage_delegate;
		internal static void gr_direct_context_get_resource_cache_usage (gr_direct_context_t context, Int32* maxResources, /* size_t */ IntPtr* maxResourceBytes) =>
			(gr_direct_context_get_resource_cache_usage_delegate ??= GetSymbol<Delegates.gr_direct_context_get_resource_cache_usage> ("gr_direct_context_get_resource_cache_usage")).Invoke (context, maxResources, maxResourceBytes);
		#endif

		// bool gr_direct_context_is_abandoned(gr_direct_context_t* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_direct_context_is_abandoned (gr_direct_context_t context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_direct_context_is_abandoned (gr_direct_context_t context);
		}
		private static Delegates.gr_direct_context_is_abandoned gr_direct_context_is_abandoned_delegate;
		internal static bool gr_direct_context_is_abandoned (gr_direct_context_t context) =>
			(gr_direct_context_is_abandoned_delegate ??= GetSymbol<Delegates.gr_direct_context_is_abandoned> ("gr_direct_context_is_abandoned")).Invoke (context);
		#endif

		// gr_direct_context_t* gr_direct_context_make_gl(const gr_glinterface_t* glInterface)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_direct_context_t gr_direct_context_make_gl (gr_glinterface_t glInterface);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_direct_context_t gr_direct_context_make_gl (gr_glinterface_t glInterface);
		}
		private static Delegates.gr_direct_context_make_gl gr_direct_context_make_gl_delegate;
		internal static gr_direct_context_t gr_direct_context_make_gl (gr_glinterface_t glInterface) =>
			(gr_direct_context_make_gl_delegate ??= GetSymbol<Delegates.gr_direct_context_make_gl> ("gr_direct_context_make_gl")).Invoke (glInterface);
		#endif

		// gr_direct_context_t* gr_direct_context_make_gl_with_options(const gr_glinterface_t* glInterface, const gr_context_options_t* options)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_direct_context_t gr_direct_context_make_gl_with_options (gr_glinterface_t glInterface, GRContextOptionsNative* options);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_direct_context_t gr_direct_context_make_gl_with_options (gr_glinterface_t glInterface, GRContextOptionsNative* options);
		}
		private static Delegates.gr_direct_context_make_gl_with_options gr_direct_context_make_gl_with_options_delegate;
		internal static gr_direct_context_t gr_direct_context_make_gl_with_options (gr_glinterface_t glInterface, GRContextOptionsNative* options) =>
			(gr_direct_context_make_gl_with_options_delegate ??= GetSymbol<Delegates.gr_direct_context_make_gl_with_options> ("gr_direct_context_make_gl_with_options")).Invoke (glInterface, options);
		#endif

		// gr_direct_context_t* gr_direct_context_make_metal(void* device, void* queue)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_direct_context_t gr_direct_context_make_metal (void* device, void* queue);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_direct_context_t gr_direct_context_make_metal (void* device, void* queue);
		}
		private static Delegates.gr_direct_context_make_metal gr_direct_context_make_metal_delegate;
		internal static gr_direct_context_t gr_direct_context_make_metal (void* device, void* queue) =>
			(gr_direct_context_make_metal_delegate ??= GetSymbol<Delegates.gr_direct_context_make_metal> ("gr_direct_context_make_metal")).Invoke (device, queue);
		#endif

		// gr_direct_context_t* gr_direct_context_make_metal_with_options(void* device, void* queue, const gr_context_options_t* options)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_direct_context_t gr_direct_context_make_metal_with_options (void* device, void* queue, GRContextOptionsNative* options);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_direct_context_t gr_direct_context_make_metal_with_options (void* device, void* queue, GRContextOptionsNative* options);
		}
		private static Delegates.gr_direct_context_make_metal_with_options gr_direct_context_make_metal_with_options_delegate;
		internal static gr_direct_context_t gr_direct_context_make_metal_with_options (void* device, void* queue, GRContextOptionsNative* options) =>
			(gr_direct_context_make_metal_with_options_delegate ??= GetSymbol<Delegates.gr_direct_context_make_metal_with_options> ("gr_direct_context_make_metal_with_options")).Invoke (device, queue, options);
		#endif

		// gr_direct_context_t* gr_direct_context_make_vulkan(const gr_vk_backendcontext_t vkBackendContext)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_direct_context_t gr_direct_context_make_vulkan (GRVkBackendContextNative vkBackendContext);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_direct_context_t gr_direct_context_make_vulkan (GRVkBackendContextNative vkBackendContext);
		}
		private static Delegates.gr_direct_context_make_vulkan gr_direct_context_make_vulkan_delegate;
		internal static gr_direct_context_t gr_direct_context_make_vulkan (GRVkBackendContextNative vkBackendContext) =>
			(gr_direct_context_make_vulkan_delegate ??= GetSymbol<Delegates.gr_direct_context_make_vulkan> ("gr_direct_context_make_vulkan")).Invoke (vkBackendContext);
		#endif

		// gr_direct_context_t* gr_direct_context_make_vulkan_with_options(const gr_vk_backendcontext_t vkBackendContext, const gr_context_options_t* options)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_direct_context_t gr_direct_context_make_vulkan_with_options (GRVkBackendContextNative vkBackendContext, GRContextOptionsNative* options);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_direct_context_t gr_direct_context_make_vulkan_with_options (GRVkBackendContextNative vkBackendContext, GRContextOptionsNative* options);
		}
		private static Delegates.gr_direct_context_make_vulkan_with_options gr_direct_context_make_vulkan_with_options_delegate;
		internal static gr_direct_context_t gr_direct_context_make_vulkan_with_options (GRVkBackendContextNative vkBackendContext, GRContextOptionsNative* options) =>
			(gr_direct_context_make_vulkan_with_options_delegate ??= GetSymbol<Delegates.gr_direct_context_make_vulkan_with_options> ("gr_direct_context_make_vulkan_with_options")).Invoke (vkBackendContext, options);
		#endif

		// void gr_direct_context_perform_deferred_cleanup(gr_direct_context_t* context, long long ms)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_perform_deferred_cleanup (gr_direct_context_t context, Int64 ms);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_perform_deferred_cleanup (gr_direct_context_t context, Int64 ms);
		}
		private static Delegates.gr_direct_context_perform_deferred_cleanup gr_direct_context_perform_deferred_cleanup_delegate;
		internal static void gr_direct_context_perform_deferred_cleanup (gr_direct_context_t context, Int64 ms) =>
			(gr_direct_context_perform_deferred_cleanup_delegate ??= GetSymbol<Delegates.gr_direct_context_perform_deferred_cleanup> ("gr_direct_context_perform_deferred_cleanup")).Invoke (context, ms);
		#endif

		// void gr_direct_context_purge_unlocked_resources(gr_direct_context_t* context, bool scratchResourcesOnly)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_purge_unlocked_resources (gr_direct_context_t context, [MarshalAs (UnmanagedType.I1)] bool scratchResourcesOnly);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_purge_unlocked_resources (gr_direct_context_t context, [MarshalAs (UnmanagedType.I1)] bool scratchResourcesOnly);
		}
		private static Delegates.gr_direct_context_purge_unlocked_resources gr_direct_context_purge_unlocked_resources_delegate;
		internal static void gr_direct_context_purge_unlocked_resources (gr_direct_context_t context, [MarshalAs (UnmanagedType.I1)] bool scratchResourcesOnly) =>
			(gr_direct_context_purge_unlocked_resources_delegate ??= GetSymbol<Delegates.gr_direct_context_purge_unlocked_resources> ("gr_direct_context_purge_unlocked_resources")).Invoke (context, scratchResourcesOnly);
		#endif

		// void gr_direct_context_purge_unlocked_resources_bytes(gr_direct_context_t* context, size_t bytesToPurge, bool preferScratchResources)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_purge_unlocked_resources_bytes (gr_direct_context_t context, /* size_t */ IntPtr bytesToPurge, [MarshalAs (UnmanagedType.I1)] bool preferScratchResources);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_purge_unlocked_resources_bytes (gr_direct_context_t context, /* size_t */ IntPtr bytesToPurge, [MarshalAs (UnmanagedType.I1)] bool preferScratchResources);
		}
		private static Delegates.gr_direct_context_purge_unlocked_resources_bytes gr_direct_context_purge_unlocked_resources_bytes_delegate;
		internal static void gr_direct_context_purge_unlocked_resources_bytes (gr_direct_context_t context, /* size_t */ IntPtr bytesToPurge, [MarshalAs (UnmanagedType.I1)] bool preferScratchResources) =>
			(gr_direct_context_purge_unlocked_resources_bytes_delegate ??= GetSymbol<Delegates.gr_direct_context_purge_unlocked_resources_bytes> ("gr_direct_context_purge_unlocked_resources_bytes")).Invoke (context, bytesToPurge, preferScratchResources);
		#endif

		// void gr_direct_context_release_resources_and_abandon_context(gr_direct_context_t* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_release_resources_and_abandon_context (gr_direct_context_t context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_release_resources_and_abandon_context (gr_direct_context_t context);
		}
		private static Delegates.gr_direct_context_release_resources_and_abandon_context gr_direct_context_release_resources_and_abandon_context_delegate;
		internal static void gr_direct_context_release_resources_and_abandon_context (gr_direct_context_t context) =>
			(gr_direct_context_release_resources_and_abandon_context_delegate ??= GetSymbol<Delegates.gr_direct_context_release_resources_and_abandon_context> ("gr_direct_context_release_resources_and_abandon_context")).Invoke (context);
		#endif

		// void gr_direct_context_reset_context(gr_direct_context_t* context, uint32_t state)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_reset_context (gr_direct_context_t context, UInt32 state);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_reset_context (gr_direct_context_t context, UInt32 state);
		}
		private static Delegates.gr_direct_context_reset_context gr_direct_context_reset_context_delegate;
		internal static void gr_direct_context_reset_context (gr_direct_context_t context, UInt32 state) =>
			(gr_direct_context_reset_context_delegate ??= GetSymbol<Delegates.gr_direct_context_reset_context> ("gr_direct_context_reset_context")).Invoke (context, state);
		#endif

		// void gr_direct_context_set_resource_cache_limit(gr_direct_context_t* context, size_t maxResourceBytes)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_set_resource_cache_limit (gr_direct_context_t context, /* size_t */ IntPtr maxResourceBytes);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_set_resource_cache_limit (gr_direct_context_t context, /* size_t */ IntPtr maxResourceBytes);
		}
		private static Delegates.gr_direct_context_set_resource_cache_limit gr_direct_context_set_resource_cache_limit_delegate;
		internal static void gr_direct_context_set_resource_cache_limit (gr_direct_context_t context, /* size_t */ IntPtr maxResourceBytes) =>
			(gr_direct_context_set_resource_cache_limit_delegate ??= GetSymbol<Delegates.gr_direct_context_set_resource_cache_limit> ("gr_direct_context_set_resource_cache_limit")).Invoke (context, maxResourceBytes);
		#endif

		// bool gr_direct_context_submit(gr_direct_context_t* context, bool syncCpu)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_direct_context_submit (gr_direct_context_t context, [MarshalAs (UnmanagedType.I1)] bool syncCpu);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_direct_context_submit (gr_direct_context_t context, [MarshalAs (UnmanagedType.I1)] bool syncCpu);
		}
		private static Delegates.gr_direct_context_submit gr_direct_context_submit_delegate;
		internal static bool gr_direct_context_submit (gr_direct_context_t context, [MarshalAs (UnmanagedType.I1)] bool syncCpu) =>
			(gr_direct_context_submit_delegate ??= GetSymbol<Delegates.gr_direct_context_submit> ("gr_direct_context_submit")).Invoke (context, syncCpu);
		#endif

		// const gr_glinterface_t* gr_glinterface_assemble_gl_interface(void* ctx, gr_gl_get_proc get)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_glinterface_t gr_glinterface_assemble_gl_interface (void* ctx, GRGlGetProcProxyDelegate get);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_glinterface_t gr_glinterface_assemble_gl_interface (void* ctx, GRGlGetProcProxyDelegate get);
		}
		private static Delegates.gr_glinterface_assemble_gl_interface gr_glinterface_assemble_gl_interface_delegate;
		internal static gr_glinterface_t gr_glinterface_assemble_gl_interface (void* ctx, GRGlGetProcProxyDelegate get) =>
			(gr_glinterface_assemble_gl_interface_delegate ??= GetSymbol<Delegates.gr_glinterface_assemble_gl_interface> ("gr_glinterface_assemble_gl_interface")).Invoke (ctx, get);
		#endif

		// const gr_glinterface_t* gr_glinterface_assemble_gles_interface(void* ctx, gr_gl_get_proc get)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_glinterface_t gr_glinterface_assemble_gles_interface (void* ctx, GRGlGetProcProxyDelegate get);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_glinterface_t gr_glinterface_assemble_gles_interface (void* ctx, GRGlGetProcProxyDelegate get);
		}
		private static Delegates.gr_glinterface_assemble_gles_interface gr_glinterface_assemble_gles_interface_delegate;
		internal static gr_glinterface_t gr_glinterface_assemble_gles_interface (void* ctx, GRGlGetProcProxyDelegate get) =>
			(gr_glinterface_assemble_gles_interface_delegate ??= GetSymbol<Delegates.gr_glinterface_assemble_gles_interface> ("gr_glinterface_assemble_gles_interface")).Invoke (ctx, get);
		#endif

		// const gr_glinterface_t* gr_glinterface_assemble_interface(void* ctx, gr_gl_get_proc get)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_glinterface_t gr_glinterface_assemble_interface (void* ctx, GRGlGetProcProxyDelegate get);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_glinterface_t gr_glinterface_assemble_interface (void* ctx, GRGlGetProcProxyDelegate get);
		}
		private static Delegates.gr_glinterface_assemble_interface gr_glinterface_assemble_interface_delegate;
		internal static gr_glinterface_t gr_glinterface_assemble_interface (void* ctx, GRGlGetProcProxyDelegate get) =>
			(gr_glinterface_assemble_interface_delegate ??= GetSymbol<Delegates.gr_glinterface_assemble_interface> ("gr_glinterface_assemble_interface")).Invoke (ctx, get);
		#endif

		// const gr_glinterface_t* gr_glinterface_assemble_webgl_interface(void* ctx, gr_gl_get_proc get)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_glinterface_t gr_glinterface_assemble_webgl_interface (void* ctx, GRGlGetProcProxyDelegate get);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_glinterface_t gr_glinterface_assemble_webgl_interface (void* ctx, GRGlGetProcProxyDelegate get);
		}
		private static Delegates.gr_glinterface_assemble_webgl_interface gr_glinterface_assemble_webgl_interface_delegate;
		internal static gr_glinterface_t gr_glinterface_assemble_webgl_interface (void* ctx, GRGlGetProcProxyDelegate get) =>
			(gr_glinterface_assemble_webgl_interface_delegate ??= GetSymbol<Delegates.gr_glinterface_assemble_webgl_interface> ("gr_glinterface_assemble_webgl_interface")).Invoke (ctx, get);
		#endif

		// const gr_glinterface_t* gr_glinterface_create_native_interface()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_glinterface_t gr_glinterface_create_native_interface ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_glinterface_t gr_glinterface_create_native_interface ();
		}
		private static Delegates.gr_glinterface_create_native_interface gr_glinterface_create_native_interface_delegate;
		internal static gr_glinterface_t gr_glinterface_create_native_interface () =>
			(gr_glinterface_create_native_interface_delegate ??= GetSymbol<Delegates.gr_glinterface_create_native_interface> ("gr_glinterface_create_native_interface")).Invoke ();
		#endif

		// bool gr_glinterface_has_extension(const gr_glinterface_t* glInterface, const char* extension)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_glinterface_has_extension (gr_glinterface_t glInterface, [MarshalAs (UnmanagedType.LPStr)] String extension);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_glinterface_has_extension (gr_glinterface_t glInterface, [MarshalAs (UnmanagedType.LPStr)] String extension);
		}
		private static Delegates.gr_glinterface_has_extension gr_glinterface_has_extension_delegate;
		internal static bool gr_glinterface_has_extension (gr_glinterface_t glInterface, [MarshalAs (UnmanagedType.LPStr)] String extension) =>
			(gr_glinterface_has_extension_delegate ??= GetSymbol<Delegates.gr_glinterface_has_extension> ("gr_glinterface_has_extension")).Invoke (glInterface, extension);
		#endif

		// void gr_glinterface_unref(const gr_glinterface_t* glInterface)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_glinterface_unref (gr_glinterface_t glInterface);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_glinterface_unref (gr_glinterface_t glInterface);
		}
		private static Delegates.gr_glinterface_unref gr_glinterface_unref_delegate;
		internal static void gr_glinterface_unref (gr_glinterface_t glInterface) =>
			(gr_glinterface_unref_delegate ??= GetSymbol<Delegates.gr_glinterface_unref> ("gr_glinterface_unref")).Invoke (glInterface);
		#endif

		// bool gr_glinterface_validate(const gr_glinterface_t* glInterface)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_glinterface_validate (gr_glinterface_t glInterface);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_glinterface_validate (gr_glinterface_t glInterface);
		}
		private static Delegates.gr_glinterface_validate gr_glinterface_validate_delegate;
		internal static bool gr_glinterface_validate (gr_glinterface_t glInterface) =>
			(gr_glinterface_validate_delegate ??= GetSymbol<Delegates.gr_glinterface_validate> ("gr_glinterface_validate")).Invoke (glInterface);
		#endif

		// gr_backend_t gr_recording_context_get_backend(gr_recording_context_t* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GRBackendNative gr_recording_context_get_backend (gr_recording_context_t context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate GRBackendNative gr_recording_context_get_backend (gr_recording_context_t context);
		}
		private static Delegates.gr_recording_context_get_backend gr_recording_context_get_backend_delegate;
		internal static GRBackendNative gr_recording_context_get_backend (gr_recording_context_t context) =>
			(gr_recording_context_get_backend_delegate ??= GetSymbol<Delegates.gr_recording_context_get_backend> ("gr_recording_context_get_backend")).Invoke (context);
		#endif

		// int gr_recording_context_get_max_surface_sample_count_for_color_type(gr_recording_context_t* context, sk_colortype_t colorType)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_recording_context_get_max_surface_sample_count_for_color_type (gr_recording_context_t context, SKColorTypeNative colorType);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_recording_context_get_max_surface_sample_count_for_color_type (gr_recording_context_t context, SKColorTypeNative colorType);
		}
		private static Delegates.gr_recording_context_get_max_surface_sample_count_for_color_type gr_recording_context_get_max_surface_sample_count_for_color_type_delegate;
		internal static Int32 gr_recording_context_get_max_surface_sample_count_for_color_type (gr_recording_context_t context, SKColorTypeNative colorType) =>
			(gr_recording_context_get_max_surface_sample_count_for_color_type_delegate ??= GetSymbol<Delegates.gr_recording_context_get_max_surface_sample_count_for_color_type> ("gr_recording_context_get_max_surface_sample_count_for_color_type")).Invoke (context, colorType);
		#endif

		// void gr_recording_context_unref(gr_recording_context_t* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_recording_context_unref (gr_recording_context_t context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_recording_context_unref (gr_recording_context_t context);
		}
		private static Delegates.gr_recording_context_unref gr_recording_context_unref_delegate;
		internal static void gr_recording_context_unref (gr_recording_context_t context) =>
			(gr_recording_context_unref_delegate ??= GetSymbol<Delegates.gr_recording_context_unref> ("gr_recording_context_unref")).Invoke (context);
		#endif

		// void gr_vk_extensions_delete(gr_vk_extensions_t* extensions)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_vk_extensions_delete (gr_vk_extensions_t extensions);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_vk_extensions_delete (gr_vk_extensions_t extensions);
		}
		private static Delegates.gr_vk_extensions_delete gr_vk_extensions_delete_delegate;
		internal static void gr_vk_extensions_delete (gr_vk_extensions_t extensions) =>
			(gr_vk_extensions_delete_delegate ??= GetSymbol<Delegates.gr_vk_extensions_delete> ("gr_vk_extensions_delete")).Invoke (extensions);
		#endif

		// bool gr_vk_extensions_has_extension(gr_vk_extensions_t* extensions, const char* ext, uint32_t minVersion)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_vk_extensions_has_extension (gr_vk_extensions_t extensions, [MarshalAs (UnmanagedType.LPStr)] String ext, UInt32 minVersion);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_vk_extensions_has_extension (gr_vk_extensions_t extensions, [MarshalAs (UnmanagedType.LPStr)] String ext, UInt32 minVersion);
		}
		private static Delegates.gr_vk_extensions_has_extension gr_vk_extensions_has_extension_delegate;
		internal static bool gr_vk_extensions_has_extension (gr_vk_extensions_t extensions, [MarshalAs (UnmanagedType.LPStr)] String ext, UInt32 minVersion) =>
			(gr_vk_extensions_has_extension_delegate ??= GetSymbol<Delegates.gr_vk_extensions_has_extension> ("gr_vk_extensions_has_extension")).Invoke (extensions, ext, minVersion);
		#endif

		// void gr_vk_extensions_init(gr_vk_extensions_t* extensions, gr_vk_get_proc getProc, void* userData, vk_instance_t* instance, vk_physical_device_t* physDev, uint32_t instanceExtensionCount, const char** instanceExtensions, uint32_t deviceExtensionCount, const char** deviceExtensions)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_vk_extensions_init (gr_vk_extensions_t extensions, GRVkGetProcProxyDelegate getProc, void* userData, vk_instance_t instance, vk_physical_device_t physDev, UInt32 instanceExtensionCount, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] instanceExtensions, UInt32 deviceExtensionCount, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] deviceExtensions);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_vk_extensions_init (gr_vk_extensions_t extensions, GRVkGetProcProxyDelegate getProc, void* userData, vk_instance_t instance, vk_physical_device_t physDev, UInt32 instanceExtensionCount, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] instanceExtensions, UInt32 deviceExtensionCount, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] deviceExtensions);
		}
		private static Delegates.gr_vk_extensions_init gr_vk_extensions_init_delegate;
		internal static void gr_vk_extensions_init (gr_vk_extensions_t extensions, GRVkGetProcProxyDelegate getProc, void* userData, vk_instance_t instance, vk_physical_device_t physDev, UInt32 instanceExtensionCount, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] instanceExtensions, UInt32 deviceExtensionCount, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] deviceExtensions) =>
			(gr_vk_extensions_init_delegate ??= GetSymbol<Delegates.gr_vk_extensions_init> ("gr_vk_extensions_init")).Invoke (extensions, getProc, userData, instance, physDev, instanceExtensionCount, instanceExtensions, deviceExtensionCount, deviceExtensions);
		#endif

		// gr_vk_extensions_t* gr_vk_extensions_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_vk_extensions_t gr_vk_extensions_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_vk_extensions_t gr_vk_extensions_new ();
		}
		private static Delegates.gr_vk_extensions_new gr_vk_extensions_new_delegate;
		internal static gr_vk_extensions_t gr_vk_extensions_new () =>
			(gr_vk_extensions_new_delegate ??= GetSymbol<Delegates.gr_vk_extensions_new> ("gr_vk_extensions_new")).Invoke ();
		#endif

		#endregion

		#region sk_bitmap.h

		// void sk_bitmap_destructor(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_destructor (sk_bitmap_t cbitmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_destructor (sk_bitmap_t cbitmap);
		}
		private static Delegates.sk_bitmap_destructor sk_bitmap_destructor_delegate;
		internal static void sk_bitmap_destructor (sk_bitmap_t cbitmap) =>
			(sk_bitmap_destructor_delegate ??= GetSymbol<Delegates.sk_bitmap_destructor> ("sk_bitmap_destructor")).Invoke (cbitmap);
		#endif

		// void sk_bitmap_erase(sk_bitmap_t* cbitmap, sk_color_t color)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_erase (sk_bitmap_t cbitmap, UInt32 color);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_erase (sk_bitmap_t cbitmap, UInt32 color);
		}
		private static Delegates.sk_bitmap_erase sk_bitmap_erase_delegate;
		internal static void sk_bitmap_erase (sk_bitmap_t cbitmap, UInt32 color) =>
			(sk_bitmap_erase_delegate ??= GetSymbol<Delegates.sk_bitmap_erase> ("sk_bitmap_erase")).Invoke (cbitmap, color);
		#endif

		// void sk_bitmap_erase_rect(sk_bitmap_t* cbitmap, sk_color_t color, sk_irect_t* rect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_erase_rect (sk_bitmap_t cbitmap, UInt32 color, SKRectI* rect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_erase_rect (sk_bitmap_t cbitmap, UInt32 color, SKRectI* rect);
		}
		private static Delegates.sk_bitmap_erase_rect sk_bitmap_erase_rect_delegate;
		internal static void sk_bitmap_erase_rect (sk_bitmap_t cbitmap, UInt32 color, SKRectI* rect) =>
			(sk_bitmap_erase_rect_delegate ??= GetSymbol<Delegates.sk_bitmap_erase_rect> ("sk_bitmap_erase_rect")).Invoke (cbitmap, color, rect);
		#endif

		// bool sk_bitmap_extract_alpha(sk_bitmap_t* cbitmap, sk_bitmap_t* dst, const sk_paint_t* paint, sk_ipoint_t* offset)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_extract_alpha (sk_bitmap_t cbitmap, sk_bitmap_t dst, sk_paint_t paint, SKPointI* offset);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_extract_alpha (sk_bitmap_t cbitmap, sk_bitmap_t dst, sk_paint_t paint, SKPointI* offset);
		}
		private static Delegates.sk_bitmap_extract_alpha sk_bitmap_extract_alpha_delegate;
		internal static bool sk_bitmap_extract_alpha (sk_bitmap_t cbitmap, sk_bitmap_t dst, sk_paint_t paint, SKPointI* offset) =>
			(sk_bitmap_extract_alpha_delegate ??= GetSymbol<Delegates.sk_bitmap_extract_alpha> ("sk_bitmap_extract_alpha")).Invoke (cbitmap, dst, paint, offset);
		#endif

		// bool sk_bitmap_extract_subset(sk_bitmap_t* cbitmap, sk_bitmap_t* dst, sk_irect_t* subset)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_extract_subset (sk_bitmap_t cbitmap, sk_bitmap_t dst, SKRectI* subset);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_extract_subset (sk_bitmap_t cbitmap, sk_bitmap_t dst, SKRectI* subset);
		}
		private static Delegates.sk_bitmap_extract_subset sk_bitmap_extract_subset_delegate;
		internal static bool sk_bitmap_extract_subset (sk_bitmap_t cbitmap, sk_bitmap_t dst, SKRectI* subset) =>
			(sk_bitmap_extract_subset_delegate ??= GetSymbol<Delegates.sk_bitmap_extract_subset> ("sk_bitmap_extract_subset")).Invoke (cbitmap, dst, subset);
		#endif

		// void* sk_bitmap_get_addr(sk_bitmap_t* cbitmap, int x, int y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_bitmap_get_addr (sk_bitmap_t cbitmap, Int32 x, Int32 y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_bitmap_get_addr (sk_bitmap_t cbitmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_bitmap_get_addr sk_bitmap_get_addr_delegate;
		internal static void* sk_bitmap_get_addr (sk_bitmap_t cbitmap, Int32 x, Int32 y) =>
			(sk_bitmap_get_addr_delegate ??= GetSymbol<Delegates.sk_bitmap_get_addr> ("sk_bitmap_get_addr")).Invoke (cbitmap, x, y);
		#endif

		// uint16_t* sk_bitmap_get_addr_16(sk_bitmap_t* cbitmap, int x, int y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt16* sk_bitmap_get_addr_16 (sk_bitmap_t cbitmap, Int32 x, Int32 y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt16* sk_bitmap_get_addr_16 (sk_bitmap_t cbitmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_bitmap_get_addr_16 sk_bitmap_get_addr_16_delegate;
		internal static UInt16* sk_bitmap_get_addr_16 (sk_bitmap_t cbitmap, Int32 x, Int32 y) =>
			(sk_bitmap_get_addr_16_delegate ??= GetSymbol<Delegates.sk_bitmap_get_addr_16> ("sk_bitmap_get_addr_16")).Invoke (cbitmap, x, y);
		#endif

		// uint32_t* sk_bitmap_get_addr_32(sk_bitmap_t* cbitmap, int x, int y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32* sk_bitmap_get_addr_32 (sk_bitmap_t cbitmap, Int32 x, Int32 y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32* sk_bitmap_get_addr_32 (sk_bitmap_t cbitmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_bitmap_get_addr_32 sk_bitmap_get_addr_32_delegate;
		internal static UInt32* sk_bitmap_get_addr_32 (sk_bitmap_t cbitmap, Int32 x, Int32 y) =>
			(sk_bitmap_get_addr_32_delegate ??= GetSymbol<Delegates.sk_bitmap_get_addr_32> ("sk_bitmap_get_addr_32")).Invoke (cbitmap, x, y);
		#endif

		// uint8_t* sk_bitmap_get_addr_8(sk_bitmap_t* cbitmap, int x, int y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte* sk_bitmap_get_addr_8 (sk_bitmap_t cbitmap, Int32 x, Int32 y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte* sk_bitmap_get_addr_8 (sk_bitmap_t cbitmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_bitmap_get_addr_8 sk_bitmap_get_addr_8_delegate;
		internal static Byte* sk_bitmap_get_addr_8 (sk_bitmap_t cbitmap, Int32 x, Int32 y) =>
			(sk_bitmap_get_addr_8_delegate ??= GetSymbol<Delegates.sk_bitmap_get_addr_8> ("sk_bitmap_get_addr_8")).Invoke (cbitmap, x, y);
		#endif

		// size_t sk_bitmap_get_byte_count(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_bitmap_get_byte_count (sk_bitmap_t cbitmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_bitmap_get_byte_count (sk_bitmap_t cbitmap);
		}
		private static Delegates.sk_bitmap_get_byte_count sk_bitmap_get_byte_count_delegate;
		internal static /* size_t */ IntPtr sk_bitmap_get_byte_count (sk_bitmap_t cbitmap) =>
			(sk_bitmap_get_byte_count_delegate ??= GetSymbol<Delegates.sk_bitmap_get_byte_count> ("sk_bitmap_get_byte_count")).Invoke (cbitmap);
		#endif

		// void sk_bitmap_get_info(sk_bitmap_t* cbitmap, sk_imageinfo_t* info)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_get_info (sk_bitmap_t cbitmap, SKImageInfoNative* info);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_get_info (sk_bitmap_t cbitmap, SKImageInfoNative* info);
		}
		private static Delegates.sk_bitmap_get_info sk_bitmap_get_info_delegate;
		internal static void sk_bitmap_get_info (sk_bitmap_t cbitmap, SKImageInfoNative* info) =>
			(sk_bitmap_get_info_delegate ??= GetSymbol<Delegates.sk_bitmap_get_info> ("sk_bitmap_get_info")).Invoke (cbitmap, info);
		#endif

		// sk_color_t sk_bitmap_get_pixel_color(sk_bitmap_t* cbitmap, int x, int y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_bitmap_get_pixel_color (sk_bitmap_t cbitmap, Int32 x, Int32 y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_bitmap_get_pixel_color (sk_bitmap_t cbitmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_bitmap_get_pixel_color sk_bitmap_get_pixel_color_delegate;
		internal static UInt32 sk_bitmap_get_pixel_color (sk_bitmap_t cbitmap, Int32 x, Int32 y) =>
			(sk_bitmap_get_pixel_color_delegate ??= GetSymbol<Delegates.sk_bitmap_get_pixel_color> ("sk_bitmap_get_pixel_color")).Invoke (cbitmap, x, y);
		#endif

		// void sk_bitmap_get_pixel_colors(sk_bitmap_t* cbitmap, sk_color_t* colors)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_get_pixel_colors (sk_bitmap_t cbitmap, UInt32* colors);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_get_pixel_colors (sk_bitmap_t cbitmap, UInt32* colors);
		}
		private static Delegates.sk_bitmap_get_pixel_colors sk_bitmap_get_pixel_colors_delegate;
		internal static void sk_bitmap_get_pixel_colors (sk_bitmap_t cbitmap, UInt32* colors) =>
			(sk_bitmap_get_pixel_colors_delegate ??= GetSymbol<Delegates.sk_bitmap_get_pixel_colors> ("sk_bitmap_get_pixel_colors")).Invoke (cbitmap, colors);
		#endif

		// void* sk_bitmap_get_pixels(sk_bitmap_t* cbitmap, size_t* length)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_bitmap_get_pixels (sk_bitmap_t cbitmap, /* size_t */ IntPtr* length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_bitmap_get_pixels (sk_bitmap_t cbitmap, /* size_t */ IntPtr* length);
		}
		private static Delegates.sk_bitmap_get_pixels sk_bitmap_get_pixels_delegate;
		internal static void* sk_bitmap_get_pixels (sk_bitmap_t cbitmap, /* size_t */ IntPtr* length) =>
			(sk_bitmap_get_pixels_delegate ??= GetSymbol<Delegates.sk_bitmap_get_pixels> ("sk_bitmap_get_pixels")).Invoke (cbitmap, length);
		#endif

		// size_t sk_bitmap_get_row_bytes(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_bitmap_get_row_bytes (sk_bitmap_t cbitmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_bitmap_get_row_bytes (sk_bitmap_t cbitmap);
		}
		private static Delegates.sk_bitmap_get_row_bytes sk_bitmap_get_row_bytes_delegate;
		internal static /* size_t */ IntPtr sk_bitmap_get_row_bytes (sk_bitmap_t cbitmap) =>
			(sk_bitmap_get_row_bytes_delegate ??= GetSymbol<Delegates.sk_bitmap_get_row_bytes> ("sk_bitmap_get_row_bytes")).Invoke (cbitmap);
		#endif

		// bool sk_bitmap_install_mask_pixels(sk_bitmap_t* cbitmap, const sk_mask_t* cmask)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_install_mask_pixels (sk_bitmap_t cbitmap, SKMask* cmask);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_install_mask_pixels (sk_bitmap_t cbitmap, SKMask* cmask);
		}
		private static Delegates.sk_bitmap_install_mask_pixels sk_bitmap_install_mask_pixels_delegate;
		internal static bool sk_bitmap_install_mask_pixels (sk_bitmap_t cbitmap, SKMask* cmask) =>
			(sk_bitmap_install_mask_pixels_delegate ??= GetSymbol<Delegates.sk_bitmap_install_mask_pixels> ("sk_bitmap_install_mask_pixels")).Invoke (cbitmap, cmask);
		#endif

		// bool sk_bitmap_install_pixels(sk_bitmap_t* cbitmap, const sk_imageinfo_t* cinfo, void* pixels, size_t rowBytes, const sk_bitmap_release_proc releaseProc, void* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_install_pixels (sk_bitmap_t cbitmap, SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes, SKBitmapReleaseProxyDelegate releaseProc, void* context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_install_pixels (sk_bitmap_t cbitmap, SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes, SKBitmapReleaseProxyDelegate releaseProc, void* context);
		}
		private static Delegates.sk_bitmap_install_pixels sk_bitmap_install_pixels_delegate;
		internal static bool sk_bitmap_install_pixels (sk_bitmap_t cbitmap, SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes, SKBitmapReleaseProxyDelegate releaseProc, void* context) =>
			(sk_bitmap_install_pixels_delegate ??= GetSymbol<Delegates.sk_bitmap_install_pixels> ("sk_bitmap_install_pixels")).Invoke (cbitmap, cinfo, pixels, rowBytes, releaseProc, context);
		#endif

		// bool sk_bitmap_install_pixels_with_pixmap(sk_bitmap_t* cbitmap, const sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_install_pixels_with_pixmap (sk_bitmap_t cbitmap, sk_pixmap_t cpixmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_install_pixels_with_pixmap (sk_bitmap_t cbitmap, sk_pixmap_t cpixmap);
		}
		private static Delegates.sk_bitmap_install_pixels_with_pixmap sk_bitmap_install_pixels_with_pixmap_delegate;
		internal static bool sk_bitmap_install_pixels_with_pixmap (sk_bitmap_t cbitmap, sk_pixmap_t cpixmap) =>
			(sk_bitmap_install_pixels_with_pixmap_delegate ??= GetSymbol<Delegates.sk_bitmap_install_pixels_with_pixmap> ("sk_bitmap_install_pixels_with_pixmap")).Invoke (cbitmap, cpixmap);
		#endif

		// bool sk_bitmap_is_immutable(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_is_immutable (sk_bitmap_t cbitmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_is_immutable (sk_bitmap_t cbitmap);
		}
		private static Delegates.sk_bitmap_is_immutable sk_bitmap_is_immutable_delegate;
		internal static bool sk_bitmap_is_immutable (sk_bitmap_t cbitmap) =>
			(sk_bitmap_is_immutable_delegate ??= GetSymbol<Delegates.sk_bitmap_is_immutable> ("sk_bitmap_is_immutable")).Invoke (cbitmap);
		#endif

		// bool sk_bitmap_is_null(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_is_null (sk_bitmap_t cbitmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_is_null (sk_bitmap_t cbitmap);
		}
		private static Delegates.sk_bitmap_is_null sk_bitmap_is_null_delegate;
		internal static bool sk_bitmap_is_null (sk_bitmap_t cbitmap) =>
			(sk_bitmap_is_null_delegate ??= GetSymbol<Delegates.sk_bitmap_is_null> ("sk_bitmap_is_null")).Invoke (cbitmap);
		#endif

		// sk_shader_t* sk_bitmap_make_shader(sk_bitmap_t* cbitmap, sk_shader_tilemode_t tmx, sk_shader_tilemode_t tmy, const sk_matrix_t* cmatrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_bitmap_make_shader (sk_bitmap_t cbitmap, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix* cmatrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_bitmap_make_shader (sk_bitmap_t cbitmap, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix* cmatrix);
		}
		private static Delegates.sk_bitmap_make_shader sk_bitmap_make_shader_delegate;
		internal static sk_shader_t sk_bitmap_make_shader (sk_bitmap_t cbitmap, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix* cmatrix) =>
			(sk_bitmap_make_shader_delegate ??= GetSymbol<Delegates.sk_bitmap_make_shader> ("sk_bitmap_make_shader")).Invoke (cbitmap, tmx, tmy, cmatrix);
		#endif

		// sk_bitmap_t* sk_bitmap_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_bitmap_t sk_bitmap_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_bitmap_t sk_bitmap_new ();
		}
		private static Delegates.sk_bitmap_new sk_bitmap_new_delegate;
		internal static sk_bitmap_t sk_bitmap_new () =>
			(sk_bitmap_new_delegate ??= GetSymbol<Delegates.sk_bitmap_new> ("sk_bitmap_new")).Invoke ();
		#endif

		// void sk_bitmap_notify_pixels_changed(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_notify_pixels_changed (sk_bitmap_t cbitmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_notify_pixels_changed (sk_bitmap_t cbitmap);
		}
		private static Delegates.sk_bitmap_notify_pixels_changed sk_bitmap_notify_pixels_changed_delegate;
		internal static void sk_bitmap_notify_pixels_changed (sk_bitmap_t cbitmap) =>
			(sk_bitmap_notify_pixels_changed_delegate ??= GetSymbol<Delegates.sk_bitmap_notify_pixels_changed> ("sk_bitmap_notify_pixels_changed")).Invoke (cbitmap);
		#endif

		// bool sk_bitmap_peek_pixels(sk_bitmap_t* cbitmap, sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_peek_pixels (sk_bitmap_t cbitmap, sk_pixmap_t cpixmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_peek_pixels (sk_bitmap_t cbitmap, sk_pixmap_t cpixmap);
		}
		private static Delegates.sk_bitmap_peek_pixels sk_bitmap_peek_pixels_delegate;
		internal static bool sk_bitmap_peek_pixels (sk_bitmap_t cbitmap, sk_pixmap_t cpixmap) =>
			(sk_bitmap_peek_pixels_delegate ??= GetSymbol<Delegates.sk_bitmap_peek_pixels> ("sk_bitmap_peek_pixels")).Invoke (cbitmap, cpixmap);
		#endif

		// bool sk_bitmap_ready_to_draw(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_ready_to_draw (sk_bitmap_t cbitmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_ready_to_draw (sk_bitmap_t cbitmap);
		}
		private static Delegates.sk_bitmap_ready_to_draw sk_bitmap_ready_to_draw_delegate;
		internal static bool sk_bitmap_ready_to_draw (sk_bitmap_t cbitmap) =>
			(sk_bitmap_ready_to_draw_delegate ??= GetSymbol<Delegates.sk_bitmap_ready_to_draw> ("sk_bitmap_ready_to_draw")).Invoke (cbitmap);
		#endif

		// void sk_bitmap_reset(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_reset (sk_bitmap_t cbitmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_reset (sk_bitmap_t cbitmap);
		}
		private static Delegates.sk_bitmap_reset sk_bitmap_reset_delegate;
		internal static void sk_bitmap_reset (sk_bitmap_t cbitmap) =>
			(sk_bitmap_reset_delegate ??= GetSymbol<Delegates.sk_bitmap_reset> ("sk_bitmap_reset")).Invoke (cbitmap);
		#endif

		// void sk_bitmap_set_immutable(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_set_immutable (sk_bitmap_t cbitmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_set_immutable (sk_bitmap_t cbitmap);
		}
		private static Delegates.sk_bitmap_set_immutable sk_bitmap_set_immutable_delegate;
		internal static void sk_bitmap_set_immutable (sk_bitmap_t cbitmap) =>
			(sk_bitmap_set_immutable_delegate ??= GetSymbol<Delegates.sk_bitmap_set_immutable> ("sk_bitmap_set_immutable")).Invoke (cbitmap);
		#endif

		// void sk_bitmap_set_pixels(sk_bitmap_t* cbitmap, void* pixels)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_set_pixels (sk_bitmap_t cbitmap, void* pixels);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_set_pixels (sk_bitmap_t cbitmap, void* pixels);
		}
		private static Delegates.sk_bitmap_set_pixels sk_bitmap_set_pixels_delegate;
		internal static void sk_bitmap_set_pixels (sk_bitmap_t cbitmap, void* pixels) =>
			(sk_bitmap_set_pixels_delegate ??= GetSymbol<Delegates.sk_bitmap_set_pixels> ("sk_bitmap_set_pixels")).Invoke (cbitmap, pixels);
		#endif

		// void sk_bitmap_swap(sk_bitmap_t* cbitmap, sk_bitmap_t* cother)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_swap (sk_bitmap_t cbitmap, sk_bitmap_t cother);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_swap (sk_bitmap_t cbitmap, sk_bitmap_t cother);
		}
		private static Delegates.sk_bitmap_swap sk_bitmap_swap_delegate;
		internal static void sk_bitmap_swap (sk_bitmap_t cbitmap, sk_bitmap_t cother) =>
			(sk_bitmap_swap_delegate ??= GetSymbol<Delegates.sk_bitmap_swap> ("sk_bitmap_swap")).Invoke (cbitmap, cother);
		#endif

		// bool sk_bitmap_try_alloc_pixels(sk_bitmap_t* cbitmap, const sk_imageinfo_t* requestedInfo, size_t rowBytes)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_try_alloc_pixels (sk_bitmap_t cbitmap, SKImageInfoNative* requestedInfo, /* size_t */ IntPtr rowBytes);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_try_alloc_pixels (sk_bitmap_t cbitmap, SKImageInfoNative* requestedInfo, /* size_t */ IntPtr rowBytes);
		}
		private static Delegates.sk_bitmap_try_alloc_pixels sk_bitmap_try_alloc_pixels_delegate;
		internal static bool sk_bitmap_try_alloc_pixels (sk_bitmap_t cbitmap, SKImageInfoNative* requestedInfo, /* size_t */ IntPtr rowBytes) =>
			(sk_bitmap_try_alloc_pixels_delegate ??= GetSymbol<Delegates.sk_bitmap_try_alloc_pixels> ("sk_bitmap_try_alloc_pixels")).Invoke (cbitmap, requestedInfo, rowBytes);
		#endif

		// bool sk_bitmap_try_alloc_pixels_with_flags(sk_bitmap_t* cbitmap, const sk_imageinfo_t* requestedInfo, uint32_t flags)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_try_alloc_pixels_with_flags (sk_bitmap_t cbitmap, SKImageInfoNative* requestedInfo, UInt32 flags);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_try_alloc_pixels_with_flags (sk_bitmap_t cbitmap, SKImageInfoNative* requestedInfo, UInt32 flags);
		}
		private static Delegates.sk_bitmap_try_alloc_pixels_with_flags sk_bitmap_try_alloc_pixels_with_flags_delegate;
		internal static bool sk_bitmap_try_alloc_pixels_with_flags (sk_bitmap_t cbitmap, SKImageInfoNative* requestedInfo, UInt32 flags) =>
			(sk_bitmap_try_alloc_pixels_with_flags_delegate ??= GetSymbol<Delegates.sk_bitmap_try_alloc_pixels_with_flags> ("sk_bitmap_try_alloc_pixels_with_flags")).Invoke (cbitmap, requestedInfo, flags);
		#endif

		#endregion

		#region sk_canvas.h

		// void sk_canvas_clear(sk_canvas_t*, sk_color_t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clear (sk_canvas_t param0, UInt32 param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_clear (sk_canvas_t param0, UInt32 param1);
		}
		private static Delegates.sk_canvas_clear sk_canvas_clear_delegate;
		internal static void sk_canvas_clear (sk_canvas_t param0, UInt32 param1) =>
			(sk_canvas_clear_delegate ??= GetSymbol<Delegates.sk_canvas_clear> ("sk_canvas_clear")).Invoke (param0, param1);
		#endif

		// void sk_canvas_clear_color4f(sk_canvas_t*, sk_color4f_t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clear_color4f (sk_canvas_t param0, SKColorF param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_clear_color4f (sk_canvas_t param0, SKColorF param1);
		}
		private static Delegates.sk_canvas_clear_color4f sk_canvas_clear_color4f_delegate;
		internal static void sk_canvas_clear_color4f (sk_canvas_t param0, SKColorF param1) =>
			(sk_canvas_clear_color4f_delegate ??= GetSymbol<Delegates.sk_canvas_clear_color4f> ("sk_canvas_clear_color4f")).Invoke (param0, param1);
		#endif

		// void sk_canvas_clip_path_with_operation(sk_canvas_t* t, const sk_path_t* crect, sk_clipop_t op, bool doAA)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clip_path_with_operation (sk_canvas_t t, sk_path_t crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_clip_path_with_operation (sk_canvas_t t, sk_path_t crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		}
		private static Delegates.sk_canvas_clip_path_with_operation sk_canvas_clip_path_with_operation_delegate;
		internal static void sk_canvas_clip_path_with_operation (sk_canvas_t t, sk_path_t crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA) =>
			(sk_canvas_clip_path_with_operation_delegate ??= GetSymbol<Delegates.sk_canvas_clip_path_with_operation> ("sk_canvas_clip_path_with_operation")).Invoke (t, crect, op, doAA);
		#endif

		// void sk_canvas_clip_rect_with_operation(sk_canvas_t* t, const sk_rect_t* crect, sk_clipop_t op, bool doAA)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clip_rect_with_operation (sk_canvas_t t, SKRect* crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_clip_rect_with_operation (sk_canvas_t t, SKRect* crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		}
		private static Delegates.sk_canvas_clip_rect_with_operation sk_canvas_clip_rect_with_operation_delegate;
		internal static void sk_canvas_clip_rect_with_operation (sk_canvas_t t, SKRect* crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA) =>
			(sk_canvas_clip_rect_with_operation_delegate ??= GetSymbol<Delegates.sk_canvas_clip_rect_with_operation> ("sk_canvas_clip_rect_with_operation")).Invoke (t, crect, op, doAA);
		#endif

		// void sk_canvas_clip_region(sk_canvas_t* canvas, const sk_region_t* region, sk_clipop_t op)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clip_region (sk_canvas_t canvas, sk_region_t region, SKClipOperation op);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_clip_region (sk_canvas_t canvas, sk_region_t region, SKClipOperation op);
		}
		private static Delegates.sk_canvas_clip_region sk_canvas_clip_region_delegate;
		internal static void sk_canvas_clip_region (sk_canvas_t canvas, sk_region_t region, SKClipOperation op) =>
			(sk_canvas_clip_region_delegate ??= GetSymbol<Delegates.sk_canvas_clip_region> ("sk_canvas_clip_region")).Invoke (canvas, region, op);
		#endif

		// void sk_canvas_clip_rrect_with_operation(sk_canvas_t* t, const sk_rrect_t* crect, sk_clipop_t op, bool doAA)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clip_rrect_with_operation (sk_canvas_t t, sk_rrect_t crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_clip_rrect_with_operation (sk_canvas_t t, sk_rrect_t crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		}
		private static Delegates.sk_canvas_clip_rrect_with_operation sk_canvas_clip_rrect_with_operation_delegate;
		internal static void sk_canvas_clip_rrect_with_operation (sk_canvas_t t, sk_rrect_t crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA) =>
			(sk_canvas_clip_rrect_with_operation_delegate ??= GetSymbol<Delegates.sk_canvas_clip_rrect_with_operation> ("sk_canvas_clip_rrect_with_operation")).Invoke (t, crect, op, doAA);
		#endif

		// void sk_canvas_concat(sk_canvas_t*, const sk_matrix_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_concat (sk_canvas_t param0, SKMatrix* param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_concat (sk_canvas_t param0, SKMatrix* param1);
		}
		private static Delegates.sk_canvas_concat sk_canvas_concat_delegate;
		internal static void sk_canvas_concat (sk_canvas_t param0, SKMatrix* param1) =>
			(sk_canvas_concat_delegate ??= GetSymbol<Delegates.sk_canvas_concat> ("sk_canvas_concat")).Invoke (param0, param1);
		#endif

		// void sk_canvas_destroy(sk_canvas_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_destroy (sk_canvas_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_destroy (sk_canvas_t param0);
		}
		private static Delegates.sk_canvas_destroy sk_canvas_destroy_delegate;
		internal static void sk_canvas_destroy (sk_canvas_t param0) =>
			(sk_canvas_destroy_delegate ??= GetSymbol<Delegates.sk_canvas_destroy> ("sk_canvas_destroy")).Invoke (param0);
		#endif

		// void sk_canvas_discard(sk_canvas_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_discard (sk_canvas_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_discard (sk_canvas_t param0);
		}
		private static Delegates.sk_canvas_discard sk_canvas_discard_delegate;
		internal static void sk_canvas_discard (sk_canvas_t param0) =>
			(sk_canvas_discard_delegate ??= GetSymbol<Delegates.sk_canvas_discard> ("sk_canvas_discard")).Invoke (param0);
		#endif

		// void sk_canvas_draw_annotation(sk_canvas_t* t, const sk_rect_t* rect, const char* key, sk_data_t* value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_annotation (sk_canvas_t t, SKRect* rect, /* char */ void* key, sk_data_t value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_annotation (sk_canvas_t t, SKRect* rect, /* char */ void* key, sk_data_t value);
		}
		private static Delegates.sk_canvas_draw_annotation sk_canvas_draw_annotation_delegate;
		internal static void sk_canvas_draw_annotation (sk_canvas_t t, SKRect* rect, /* char */ void* key, sk_data_t value) =>
			(sk_canvas_draw_annotation_delegate ??= GetSymbol<Delegates.sk_canvas_draw_annotation> ("sk_canvas_draw_annotation")).Invoke (t, rect, key, value);
		#endif

		// void sk_canvas_draw_arc(sk_canvas_t* ccanvas, const sk_rect_t* oval, float startAngle, float sweepAngle, bool useCenter, const sk_paint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_arc (sk_canvas_t ccanvas, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool useCenter, sk_paint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_arc (sk_canvas_t ccanvas, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool useCenter, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_arc sk_canvas_draw_arc_delegate;
		internal static void sk_canvas_draw_arc (sk_canvas_t ccanvas, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool useCenter, sk_paint_t paint) =>
			(sk_canvas_draw_arc_delegate ??= GetSymbol<Delegates.sk_canvas_draw_arc> ("sk_canvas_draw_arc")).Invoke (ccanvas, oval, startAngle, sweepAngle, useCenter, paint);
		#endif

		// void sk_canvas_draw_atlas(sk_canvas_t* ccanvas, const sk_image_t* atlas, const sk_rsxform_t* xform, const sk_rect_t* tex, const sk_color_t* colors, int count, sk_blendmode_t mode, const sk_rect_t* cullRect, const sk_paint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_atlas (sk_canvas_t ccanvas, sk_image_t atlas, SKRotationScaleMatrix* xform, SKRect* tex, UInt32* colors, Int32 count, SKBlendMode mode, SKRect* cullRect, sk_paint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_atlas (sk_canvas_t ccanvas, sk_image_t atlas, SKRotationScaleMatrix* xform, SKRect* tex, UInt32* colors, Int32 count, SKBlendMode mode, SKRect* cullRect, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_atlas sk_canvas_draw_atlas_delegate;
		internal static void sk_canvas_draw_atlas (sk_canvas_t ccanvas, sk_image_t atlas, SKRotationScaleMatrix* xform, SKRect* tex, UInt32* colors, Int32 count, SKBlendMode mode, SKRect* cullRect, sk_paint_t paint) =>
			(sk_canvas_draw_atlas_delegate ??= GetSymbol<Delegates.sk_canvas_draw_atlas> ("sk_canvas_draw_atlas")).Invoke (ccanvas, atlas, xform, tex, colors, count, mode, cullRect, paint);
		#endif

		// void sk_canvas_draw_circle(sk_canvas_t*, float cx, float cy, float rad, const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_circle (sk_canvas_t param0, Single cx, Single cy, Single rad, sk_paint_t param4);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_circle (sk_canvas_t param0, Single cx, Single cy, Single rad, sk_paint_t param4);
		}
		private static Delegates.sk_canvas_draw_circle sk_canvas_draw_circle_delegate;
		internal static void sk_canvas_draw_circle (sk_canvas_t param0, Single cx, Single cy, Single rad, sk_paint_t param4) =>
			(sk_canvas_draw_circle_delegate ??= GetSymbol<Delegates.sk_canvas_draw_circle> ("sk_canvas_draw_circle")).Invoke (param0, cx, cy, rad, param4);
		#endif

		// void sk_canvas_draw_color(sk_canvas_t* ccanvas, sk_color_t color, sk_blendmode_t mode)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_color (sk_canvas_t ccanvas, UInt32 color, SKBlendMode mode);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_color (sk_canvas_t ccanvas, UInt32 color, SKBlendMode mode);
		}
		private static Delegates.sk_canvas_draw_color sk_canvas_draw_color_delegate;
		internal static void sk_canvas_draw_color (sk_canvas_t ccanvas, UInt32 color, SKBlendMode mode) =>
			(sk_canvas_draw_color_delegate ??= GetSymbol<Delegates.sk_canvas_draw_color> ("sk_canvas_draw_color")).Invoke (ccanvas, color, mode);
		#endif

		// void sk_canvas_draw_color4f(sk_canvas_t* ccanvas, sk_color4f_t color, sk_blendmode_t mode)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_color4f (sk_canvas_t ccanvas, SKColorF color, SKBlendMode mode);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_color4f (sk_canvas_t ccanvas, SKColorF color, SKBlendMode mode);
		}
		private static Delegates.sk_canvas_draw_color4f sk_canvas_draw_color4f_delegate;
		internal static void sk_canvas_draw_color4f (sk_canvas_t ccanvas, SKColorF color, SKBlendMode mode) =>
			(sk_canvas_draw_color4f_delegate ??= GetSymbol<Delegates.sk_canvas_draw_color4f> ("sk_canvas_draw_color4f")).Invoke (ccanvas, color, mode);
		#endif

		// void sk_canvas_draw_drawable(sk_canvas_t*, sk_drawable_t*, const sk_matrix_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_drawable (sk_canvas_t param0, sk_drawable_t param1, SKMatrix* param2);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_drawable (sk_canvas_t param0, sk_drawable_t param1, SKMatrix* param2);
		}
		private static Delegates.sk_canvas_draw_drawable sk_canvas_draw_drawable_delegate;
		internal static void sk_canvas_draw_drawable (sk_canvas_t param0, sk_drawable_t param1, SKMatrix* param2) =>
			(sk_canvas_draw_drawable_delegate ??= GetSymbol<Delegates.sk_canvas_draw_drawable> ("sk_canvas_draw_drawable")).Invoke (param0, param1, param2);
		#endif

		// void sk_canvas_draw_drrect(sk_canvas_t* ccanvas, const sk_rrect_t* outer, const sk_rrect_t* inner, const sk_paint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_drrect (sk_canvas_t ccanvas, sk_rrect_t outer, sk_rrect_t inner, sk_paint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_drrect (sk_canvas_t ccanvas, sk_rrect_t outer, sk_rrect_t inner, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_drrect sk_canvas_draw_drrect_delegate;
		internal static void sk_canvas_draw_drrect (sk_canvas_t ccanvas, sk_rrect_t outer, sk_rrect_t inner, sk_paint_t paint) =>
			(sk_canvas_draw_drrect_delegate ??= GetSymbol<Delegates.sk_canvas_draw_drrect> ("sk_canvas_draw_drrect")).Invoke (ccanvas, outer, inner, paint);
		#endif

		// void sk_canvas_draw_image(sk_canvas_t*, const sk_image_t*, float x, float y, const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_image (sk_canvas_t param0, sk_image_t param1, Single x, Single y, sk_paint_t param4);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_image (sk_canvas_t param0, sk_image_t param1, Single x, Single y, sk_paint_t param4);
		}
		private static Delegates.sk_canvas_draw_image sk_canvas_draw_image_delegate;
		internal static void sk_canvas_draw_image (sk_canvas_t param0, sk_image_t param1, Single x, Single y, sk_paint_t param4) =>
			(sk_canvas_draw_image_delegate ??= GetSymbol<Delegates.sk_canvas_draw_image> ("sk_canvas_draw_image")).Invoke (param0, param1, x, y, param4);
		#endif

		// void sk_canvas_draw_image_lattice(sk_canvas_t* t, const sk_image_t* image, const sk_lattice_t* lattice, const sk_rect_t* dst, const sk_paint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_image_lattice (sk_canvas_t t, sk_image_t image, SKLatticeInternal* lattice, SKRect* dst, sk_paint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_image_lattice (sk_canvas_t t, sk_image_t image, SKLatticeInternal* lattice, SKRect* dst, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_image_lattice sk_canvas_draw_image_lattice_delegate;
		internal static void sk_canvas_draw_image_lattice (sk_canvas_t t, sk_image_t image, SKLatticeInternal* lattice, SKRect* dst, sk_paint_t paint) =>
			(sk_canvas_draw_image_lattice_delegate ??= GetSymbol<Delegates.sk_canvas_draw_image_lattice> ("sk_canvas_draw_image_lattice")).Invoke (t, image, lattice, dst, paint);
		#endif

		// void sk_canvas_draw_image_nine(sk_canvas_t* t, const sk_image_t* image, const sk_irect_t* center, const sk_rect_t* dst, const sk_paint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_image_nine (sk_canvas_t t, sk_image_t image, SKRectI* center, SKRect* dst, sk_paint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_image_nine (sk_canvas_t t, sk_image_t image, SKRectI* center, SKRect* dst, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_image_nine sk_canvas_draw_image_nine_delegate;
		internal static void sk_canvas_draw_image_nine (sk_canvas_t t, sk_image_t image, SKRectI* center, SKRect* dst, sk_paint_t paint) =>
			(sk_canvas_draw_image_nine_delegate ??= GetSymbol<Delegates.sk_canvas_draw_image_nine> ("sk_canvas_draw_image_nine")).Invoke (t, image, center, dst, paint);
		#endif

		// void sk_canvas_draw_image_rect(sk_canvas_t*, const sk_image_t*, const sk_rect_t* src, const sk_rect_t* dst, const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_image_rect (sk_canvas_t param0, sk_image_t param1, SKRect* src, SKRect* dst, sk_paint_t param4);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_image_rect (sk_canvas_t param0, sk_image_t param1, SKRect* src, SKRect* dst, sk_paint_t param4);
		}
		private static Delegates.sk_canvas_draw_image_rect sk_canvas_draw_image_rect_delegate;
		internal static void sk_canvas_draw_image_rect (sk_canvas_t param0, sk_image_t param1, SKRect* src, SKRect* dst, sk_paint_t param4) =>
			(sk_canvas_draw_image_rect_delegate ??= GetSymbol<Delegates.sk_canvas_draw_image_rect> ("sk_canvas_draw_image_rect")).Invoke (param0, param1, src, dst, param4);
		#endif

		// void sk_canvas_draw_line(sk_canvas_t* ccanvas, float x0, float y0, float x1, float y1, sk_paint_t* cpaint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_line (sk_canvas_t ccanvas, Single x0, Single y0, Single x1, Single y1, sk_paint_t cpaint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_line (sk_canvas_t ccanvas, Single x0, Single y0, Single x1, Single y1, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_line sk_canvas_draw_line_delegate;
		internal static void sk_canvas_draw_line (sk_canvas_t ccanvas, Single x0, Single y0, Single x1, Single y1, sk_paint_t cpaint) =>
			(sk_canvas_draw_line_delegate ??= GetSymbol<Delegates.sk_canvas_draw_line> ("sk_canvas_draw_line")).Invoke (ccanvas, x0, y0, x1, y1, cpaint);
		#endif

		// void sk_canvas_draw_link_destination_annotation(sk_canvas_t* t, const sk_rect_t* rect, sk_data_t* value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_link_destination_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_link_destination_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value);
		}
		private static Delegates.sk_canvas_draw_link_destination_annotation sk_canvas_draw_link_destination_annotation_delegate;
		internal static void sk_canvas_draw_link_destination_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value) =>
			(sk_canvas_draw_link_destination_annotation_delegate ??= GetSymbol<Delegates.sk_canvas_draw_link_destination_annotation> ("sk_canvas_draw_link_destination_annotation")).Invoke (t, rect, value);
		#endif

		// void sk_canvas_draw_named_destination_annotation(sk_canvas_t* t, const sk_point_t* point, sk_data_t* value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_named_destination_annotation (sk_canvas_t t, SKPoint* point, sk_data_t value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_named_destination_annotation (sk_canvas_t t, SKPoint* point, sk_data_t value);
		}
		private static Delegates.sk_canvas_draw_named_destination_annotation sk_canvas_draw_named_destination_annotation_delegate;
		internal static void sk_canvas_draw_named_destination_annotation (sk_canvas_t t, SKPoint* point, sk_data_t value) =>
			(sk_canvas_draw_named_destination_annotation_delegate ??= GetSymbol<Delegates.sk_canvas_draw_named_destination_annotation> ("sk_canvas_draw_named_destination_annotation")).Invoke (t, point, value);
		#endif

		// void sk_canvas_draw_oval(sk_canvas_t*, const sk_rect_t*, const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_oval (sk_canvas_t param0, SKRect* param1, sk_paint_t param2);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_oval (sk_canvas_t param0, SKRect* param1, sk_paint_t param2);
		}
		private static Delegates.sk_canvas_draw_oval sk_canvas_draw_oval_delegate;
		internal static void sk_canvas_draw_oval (sk_canvas_t param0, SKRect* param1, sk_paint_t param2) =>
			(sk_canvas_draw_oval_delegate ??= GetSymbol<Delegates.sk_canvas_draw_oval> ("sk_canvas_draw_oval")).Invoke (param0, param1, param2);
		#endif

		// void sk_canvas_draw_paint(sk_canvas_t*, const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_paint (sk_canvas_t param0, sk_paint_t param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_paint (sk_canvas_t param0, sk_paint_t param1);
		}
		private static Delegates.sk_canvas_draw_paint sk_canvas_draw_paint_delegate;
		internal static void sk_canvas_draw_paint (sk_canvas_t param0, sk_paint_t param1) =>
			(sk_canvas_draw_paint_delegate ??= GetSymbol<Delegates.sk_canvas_draw_paint> ("sk_canvas_draw_paint")).Invoke (param0, param1);
		#endif

		// void sk_canvas_draw_patch(sk_canvas_t* ccanvas, const sk_point_t* cubics, const sk_color_t* colors, const sk_point_t* texCoords, sk_blendmode_t mode, const sk_paint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_patch (sk_canvas_t ccanvas, SKPoint* cubics, UInt32* colors, SKPoint* texCoords, SKBlendMode mode, sk_paint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_patch (sk_canvas_t ccanvas, SKPoint* cubics, UInt32* colors, SKPoint* texCoords, SKBlendMode mode, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_patch sk_canvas_draw_patch_delegate;
		internal static void sk_canvas_draw_patch (sk_canvas_t ccanvas, SKPoint* cubics, UInt32* colors, SKPoint* texCoords, SKBlendMode mode, sk_paint_t paint) =>
			(sk_canvas_draw_patch_delegate ??= GetSymbol<Delegates.sk_canvas_draw_patch> ("sk_canvas_draw_patch")).Invoke (ccanvas, cubics, colors, texCoords, mode, paint);
		#endif

		// void sk_canvas_draw_path(sk_canvas_t*, const sk_path_t*, const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_path (sk_canvas_t param0, sk_path_t param1, sk_paint_t param2);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_path (sk_canvas_t param0, sk_path_t param1, sk_paint_t param2);
		}
		private static Delegates.sk_canvas_draw_path sk_canvas_draw_path_delegate;
		internal static void sk_canvas_draw_path (sk_canvas_t param0, sk_path_t param1, sk_paint_t param2) =>
			(sk_canvas_draw_path_delegate ??= GetSymbol<Delegates.sk_canvas_draw_path> ("sk_canvas_draw_path")).Invoke (param0, param1, param2);
		#endif

		// void sk_canvas_draw_picture(sk_canvas_t*, const sk_picture_t*, const sk_matrix_t*, const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_picture (sk_canvas_t param0, sk_picture_t param1, SKMatrix* param2, sk_paint_t param3);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_picture (sk_canvas_t param0, sk_picture_t param1, SKMatrix* param2, sk_paint_t param3);
		}
		private static Delegates.sk_canvas_draw_picture sk_canvas_draw_picture_delegate;
		internal static void sk_canvas_draw_picture (sk_canvas_t param0, sk_picture_t param1, SKMatrix* param2, sk_paint_t param3) =>
			(sk_canvas_draw_picture_delegate ??= GetSymbol<Delegates.sk_canvas_draw_picture> ("sk_canvas_draw_picture")).Invoke (param0, param1, param2, param3);
		#endif

		// void sk_canvas_draw_point(sk_canvas_t*, float, float, const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_point (sk_canvas_t param0, Single param1, Single param2, sk_paint_t param3);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_point (sk_canvas_t param0, Single param1, Single param2, sk_paint_t param3);
		}
		private static Delegates.sk_canvas_draw_point sk_canvas_draw_point_delegate;
		internal static void sk_canvas_draw_point (sk_canvas_t param0, Single param1, Single param2, sk_paint_t param3) =>
			(sk_canvas_draw_point_delegate ??= GetSymbol<Delegates.sk_canvas_draw_point> ("sk_canvas_draw_point")).Invoke (param0, param1, param2, param3);
		#endif

		// void sk_canvas_draw_points(sk_canvas_t*, sk_point_mode_t, size_t, const sk_point_t[-1], const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_points (sk_canvas_t param0, SKPointMode param1, /* size_t */ IntPtr param2, SKPoint* param3, sk_paint_t param4);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_points (sk_canvas_t param0, SKPointMode param1, /* size_t */ IntPtr param2, SKPoint* param3, sk_paint_t param4);
		}
		private static Delegates.sk_canvas_draw_points sk_canvas_draw_points_delegate;
		internal static void sk_canvas_draw_points (sk_canvas_t param0, SKPointMode param1, /* size_t */ IntPtr param2, SKPoint* param3, sk_paint_t param4) =>
			(sk_canvas_draw_points_delegate ??= GetSymbol<Delegates.sk_canvas_draw_points> ("sk_canvas_draw_points")).Invoke (param0, param1, param2, param3, param4);
		#endif

		// void sk_canvas_draw_rect(sk_canvas_t*, const sk_rect_t*, const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_rect (sk_canvas_t param0, SKRect* param1, sk_paint_t param2);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_rect (sk_canvas_t param0, SKRect* param1, sk_paint_t param2);
		}
		private static Delegates.sk_canvas_draw_rect sk_canvas_draw_rect_delegate;
		internal static void sk_canvas_draw_rect (sk_canvas_t param0, SKRect* param1, sk_paint_t param2) =>
			(sk_canvas_draw_rect_delegate ??= GetSymbol<Delegates.sk_canvas_draw_rect> ("sk_canvas_draw_rect")).Invoke (param0, param1, param2);
		#endif

		// void sk_canvas_draw_region(sk_canvas_t*, const sk_region_t*, const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_region (sk_canvas_t param0, sk_region_t param1, sk_paint_t param2);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_region (sk_canvas_t param0, sk_region_t param1, sk_paint_t param2);
		}
		private static Delegates.sk_canvas_draw_region sk_canvas_draw_region_delegate;
		internal static void sk_canvas_draw_region (sk_canvas_t param0, sk_region_t param1, sk_paint_t param2) =>
			(sk_canvas_draw_region_delegate ??= GetSymbol<Delegates.sk_canvas_draw_region> ("sk_canvas_draw_region")).Invoke (param0, param1, param2);
		#endif

		// void sk_canvas_draw_round_rect(sk_canvas_t*, const sk_rect_t*, float rx, float ry, const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_round_rect (sk_canvas_t param0, SKRect* param1, Single rx, Single ry, sk_paint_t param4);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_round_rect (sk_canvas_t param0, SKRect* param1, Single rx, Single ry, sk_paint_t param4);
		}
		private static Delegates.sk_canvas_draw_round_rect sk_canvas_draw_round_rect_delegate;
		internal static void sk_canvas_draw_round_rect (sk_canvas_t param0, SKRect* param1, Single rx, Single ry, sk_paint_t param4) =>
			(sk_canvas_draw_round_rect_delegate ??= GetSymbol<Delegates.sk_canvas_draw_round_rect> ("sk_canvas_draw_round_rect")).Invoke (param0, param1, rx, ry, param4);
		#endif

		// void sk_canvas_draw_rrect(sk_canvas_t*, const sk_rrect_t*, const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_rrect (sk_canvas_t param0, sk_rrect_t param1, sk_paint_t param2);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_rrect (sk_canvas_t param0, sk_rrect_t param1, sk_paint_t param2);
		}
		private static Delegates.sk_canvas_draw_rrect sk_canvas_draw_rrect_delegate;
		internal static void sk_canvas_draw_rrect (sk_canvas_t param0, sk_rrect_t param1, sk_paint_t param2) =>
			(sk_canvas_draw_rrect_delegate ??= GetSymbol<Delegates.sk_canvas_draw_rrect> ("sk_canvas_draw_rrect")).Invoke (param0, param1, param2);
		#endif

		// void sk_canvas_draw_simple_text(sk_canvas_t* ccanvas, const void* text, size_t byte_length, sk_text_encoding_t encoding, float x, float y, const sk_font_t* cfont, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_simple_text (sk_canvas_t ccanvas, void* text, /* size_t */ IntPtr byte_length, SKTextEncoding encoding, Single x, Single y, sk_font_t cfont, sk_paint_t cpaint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_simple_text (sk_canvas_t ccanvas, void* text, /* size_t */ IntPtr byte_length, SKTextEncoding encoding, Single x, Single y, sk_font_t cfont, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_simple_text sk_canvas_draw_simple_text_delegate;
		internal static void sk_canvas_draw_simple_text (sk_canvas_t ccanvas, void* text, /* size_t */ IntPtr byte_length, SKTextEncoding encoding, Single x, Single y, sk_font_t cfont, sk_paint_t cpaint) =>
			(sk_canvas_draw_simple_text_delegate ??= GetSymbol<Delegates.sk_canvas_draw_simple_text> ("sk_canvas_draw_simple_text")).Invoke (ccanvas, text, byte_length, encoding, x, y, cfont, cpaint);
		#endif

		// void sk_canvas_draw_text_blob(sk_canvas_t*, sk_textblob_t* text, float x, float y, const sk_paint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_text_blob (sk_canvas_t param0, sk_textblob_t text, Single x, Single y, sk_paint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_text_blob (sk_canvas_t param0, sk_textblob_t text, Single x, Single y, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_text_blob sk_canvas_draw_text_blob_delegate;
		internal static void sk_canvas_draw_text_blob (sk_canvas_t param0, sk_textblob_t text, Single x, Single y, sk_paint_t paint) =>
			(sk_canvas_draw_text_blob_delegate ??= GetSymbol<Delegates.sk_canvas_draw_text_blob> ("sk_canvas_draw_text_blob")).Invoke (param0, text, x, y, paint);
		#endif

		// void sk_canvas_draw_url_annotation(sk_canvas_t* t, const sk_rect_t* rect, sk_data_t* value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_url_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_url_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value);
		}
		private static Delegates.sk_canvas_draw_url_annotation sk_canvas_draw_url_annotation_delegate;
		internal static void sk_canvas_draw_url_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value) =>
			(sk_canvas_draw_url_annotation_delegate ??= GetSymbol<Delegates.sk_canvas_draw_url_annotation> ("sk_canvas_draw_url_annotation")).Invoke (t, rect, value);
		#endif

		// void sk_canvas_draw_vertices(sk_canvas_t* ccanvas, const sk_vertices_t* vertices, sk_blendmode_t mode, const sk_paint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_vertices (sk_canvas_t ccanvas, sk_vertices_t vertices, SKBlendMode mode, sk_paint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_vertices (sk_canvas_t ccanvas, sk_vertices_t vertices, SKBlendMode mode, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_vertices sk_canvas_draw_vertices_delegate;
		internal static void sk_canvas_draw_vertices (sk_canvas_t ccanvas, sk_vertices_t vertices, SKBlendMode mode, sk_paint_t paint) =>
			(sk_canvas_draw_vertices_delegate ??= GetSymbol<Delegates.sk_canvas_draw_vertices> ("sk_canvas_draw_vertices")).Invoke (ccanvas, vertices, mode, paint);
		#endif

		// void sk_canvas_flush(sk_canvas_t* ccanvas)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_flush (sk_canvas_t ccanvas);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_flush (sk_canvas_t ccanvas);
		}
		private static Delegates.sk_canvas_flush sk_canvas_flush_delegate;
		internal static void sk_canvas_flush (sk_canvas_t ccanvas) =>
			(sk_canvas_flush_delegate ??= GetSymbol<Delegates.sk_canvas_flush> ("sk_canvas_flush")).Invoke (ccanvas);
		#endif

		// bool sk_canvas_get_device_clip_bounds(sk_canvas_t* t, sk_irect_t* cbounds)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_get_device_clip_bounds (sk_canvas_t t, SKRectI* cbounds);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_canvas_get_device_clip_bounds (sk_canvas_t t, SKRectI* cbounds);
		}
		private static Delegates.sk_canvas_get_device_clip_bounds sk_canvas_get_device_clip_bounds_delegate;
		internal static bool sk_canvas_get_device_clip_bounds (sk_canvas_t t, SKRectI* cbounds) =>
			(sk_canvas_get_device_clip_bounds_delegate ??= GetSymbol<Delegates.sk_canvas_get_device_clip_bounds> ("sk_canvas_get_device_clip_bounds")).Invoke (t, cbounds);
		#endif

		// bool sk_canvas_get_local_clip_bounds(sk_canvas_t* t, sk_rect_t* cbounds)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_get_local_clip_bounds (sk_canvas_t t, SKRect* cbounds);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_canvas_get_local_clip_bounds (sk_canvas_t t, SKRect* cbounds);
		}
		private static Delegates.sk_canvas_get_local_clip_bounds sk_canvas_get_local_clip_bounds_delegate;
		internal static bool sk_canvas_get_local_clip_bounds (sk_canvas_t t, SKRect* cbounds) =>
			(sk_canvas_get_local_clip_bounds_delegate ??= GetSymbol<Delegates.sk_canvas_get_local_clip_bounds> ("sk_canvas_get_local_clip_bounds")).Invoke (t, cbounds);
		#endif

		// int sk_canvas_get_save_count(sk_canvas_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_canvas_get_save_count (sk_canvas_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_canvas_get_save_count (sk_canvas_t param0);
		}
		private static Delegates.sk_canvas_get_save_count sk_canvas_get_save_count_delegate;
		internal static Int32 sk_canvas_get_save_count (sk_canvas_t param0) =>
			(sk_canvas_get_save_count_delegate ??= GetSymbol<Delegates.sk_canvas_get_save_count> ("sk_canvas_get_save_count")).Invoke (param0);
		#endif

		// void sk_canvas_get_total_matrix(sk_canvas_t* ccanvas, sk_matrix_t* matrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_get_total_matrix (sk_canvas_t ccanvas, SKMatrix* matrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_get_total_matrix (sk_canvas_t ccanvas, SKMatrix* matrix);
		}
		private static Delegates.sk_canvas_get_total_matrix sk_canvas_get_total_matrix_delegate;
		internal static void sk_canvas_get_total_matrix (sk_canvas_t ccanvas, SKMatrix* matrix) =>
			(sk_canvas_get_total_matrix_delegate ??= GetSymbol<Delegates.sk_canvas_get_total_matrix> ("sk_canvas_get_total_matrix")).Invoke (ccanvas, matrix);
		#endif

		// bool sk_canvas_is_clip_empty(sk_canvas_t* ccanvas)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_is_clip_empty (sk_canvas_t ccanvas);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_canvas_is_clip_empty (sk_canvas_t ccanvas);
		}
		private static Delegates.sk_canvas_is_clip_empty sk_canvas_is_clip_empty_delegate;
		internal static bool sk_canvas_is_clip_empty (sk_canvas_t ccanvas) =>
			(sk_canvas_is_clip_empty_delegate ??= GetSymbol<Delegates.sk_canvas_is_clip_empty> ("sk_canvas_is_clip_empty")).Invoke (ccanvas);
		#endif

		// bool sk_canvas_is_clip_rect(sk_canvas_t* ccanvas)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_is_clip_rect (sk_canvas_t ccanvas);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_canvas_is_clip_rect (sk_canvas_t ccanvas);
		}
		private static Delegates.sk_canvas_is_clip_rect sk_canvas_is_clip_rect_delegate;
		internal static bool sk_canvas_is_clip_rect (sk_canvas_t ccanvas) =>
			(sk_canvas_is_clip_rect_delegate ??= GetSymbol<Delegates.sk_canvas_is_clip_rect> ("sk_canvas_is_clip_rect")).Invoke (ccanvas);
		#endif

		// sk_canvas_t* sk_canvas_new_from_bitmap(const sk_bitmap_t* bitmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_canvas_new_from_bitmap (sk_bitmap_t bitmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_canvas_t sk_canvas_new_from_bitmap (sk_bitmap_t bitmap);
		}
		private static Delegates.sk_canvas_new_from_bitmap sk_canvas_new_from_bitmap_delegate;
		internal static sk_canvas_t sk_canvas_new_from_bitmap (sk_bitmap_t bitmap) =>
			(sk_canvas_new_from_bitmap_delegate ??= GetSymbol<Delegates.sk_canvas_new_from_bitmap> ("sk_canvas_new_from_bitmap")).Invoke (bitmap);
		#endif

		// bool sk_canvas_quick_reject(sk_canvas_t*, const sk_rect_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_quick_reject (sk_canvas_t param0, SKRect* param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_canvas_quick_reject (sk_canvas_t param0, SKRect* param1);
		}
		private static Delegates.sk_canvas_quick_reject sk_canvas_quick_reject_delegate;
		internal static bool sk_canvas_quick_reject (sk_canvas_t param0, SKRect* param1) =>
			(sk_canvas_quick_reject_delegate ??= GetSymbol<Delegates.sk_canvas_quick_reject> ("sk_canvas_quick_reject")).Invoke (param0, param1);
		#endif

		// void sk_canvas_reset_matrix(sk_canvas_t* ccanvas)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_reset_matrix (sk_canvas_t ccanvas);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_reset_matrix (sk_canvas_t ccanvas);
		}
		private static Delegates.sk_canvas_reset_matrix sk_canvas_reset_matrix_delegate;
		internal static void sk_canvas_reset_matrix (sk_canvas_t ccanvas) =>
			(sk_canvas_reset_matrix_delegate ??= GetSymbol<Delegates.sk_canvas_reset_matrix> ("sk_canvas_reset_matrix")).Invoke (ccanvas);
		#endif

		// void sk_canvas_restore(sk_canvas_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_restore (sk_canvas_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_restore (sk_canvas_t param0);
		}
		private static Delegates.sk_canvas_restore sk_canvas_restore_delegate;
		internal static void sk_canvas_restore (sk_canvas_t param0) =>
			(sk_canvas_restore_delegate ??= GetSymbol<Delegates.sk_canvas_restore> ("sk_canvas_restore")).Invoke (param0);
		#endif

		// void sk_canvas_restore_to_count(sk_canvas_t*, int saveCount)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_restore_to_count (sk_canvas_t param0, Int32 saveCount);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_restore_to_count (sk_canvas_t param0, Int32 saveCount);
		}
		private static Delegates.sk_canvas_restore_to_count sk_canvas_restore_to_count_delegate;
		internal static void sk_canvas_restore_to_count (sk_canvas_t param0, Int32 saveCount) =>
			(sk_canvas_restore_to_count_delegate ??= GetSymbol<Delegates.sk_canvas_restore_to_count> ("sk_canvas_restore_to_count")).Invoke (param0, saveCount);
		#endif

		// void sk_canvas_rotate_degrees(sk_canvas_t*, float degrees)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_rotate_degrees (sk_canvas_t param0, Single degrees);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_rotate_degrees (sk_canvas_t param0, Single degrees);
		}
		private static Delegates.sk_canvas_rotate_degrees sk_canvas_rotate_degrees_delegate;
		internal static void sk_canvas_rotate_degrees (sk_canvas_t param0, Single degrees) =>
			(sk_canvas_rotate_degrees_delegate ??= GetSymbol<Delegates.sk_canvas_rotate_degrees> ("sk_canvas_rotate_degrees")).Invoke (param0, degrees);
		#endif

		// void sk_canvas_rotate_radians(sk_canvas_t*, float radians)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_rotate_radians (sk_canvas_t param0, Single radians);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_rotate_radians (sk_canvas_t param0, Single radians);
		}
		private static Delegates.sk_canvas_rotate_radians sk_canvas_rotate_radians_delegate;
		internal static void sk_canvas_rotate_radians (sk_canvas_t param0, Single radians) =>
			(sk_canvas_rotate_radians_delegate ??= GetSymbol<Delegates.sk_canvas_rotate_radians> ("sk_canvas_rotate_radians")).Invoke (param0, radians);
		#endif

		// int sk_canvas_save(sk_canvas_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_canvas_save (sk_canvas_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_canvas_save (sk_canvas_t param0);
		}
		private static Delegates.sk_canvas_save sk_canvas_save_delegate;
		internal static Int32 sk_canvas_save (sk_canvas_t param0) =>
			(sk_canvas_save_delegate ??= GetSymbol<Delegates.sk_canvas_save> ("sk_canvas_save")).Invoke (param0);
		#endif

		// int sk_canvas_save_layer(sk_canvas_t*, const sk_rect_t*, const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_canvas_save_layer (sk_canvas_t param0, SKRect* param1, sk_paint_t param2);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_canvas_save_layer (sk_canvas_t param0, SKRect* param1, sk_paint_t param2);
		}
		private static Delegates.sk_canvas_save_layer sk_canvas_save_layer_delegate;
		internal static Int32 sk_canvas_save_layer (sk_canvas_t param0, SKRect* param1, sk_paint_t param2) =>
			(sk_canvas_save_layer_delegate ??= GetSymbol<Delegates.sk_canvas_save_layer> ("sk_canvas_save_layer")).Invoke (param0, param1, param2);
		#endif

		// void sk_canvas_scale(sk_canvas_t*, float sx, float sy)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_scale (sk_canvas_t param0, Single sx, Single sy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_scale (sk_canvas_t param0, Single sx, Single sy);
		}
		private static Delegates.sk_canvas_scale sk_canvas_scale_delegate;
		internal static void sk_canvas_scale (sk_canvas_t param0, Single sx, Single sy) =>
			(sk_canvas_scale_delegate ??= GetSymbol<Delegates.sk_canvas_scale> ("sk_canvas_scale")).Invoke (param0, sx, sy);
		#endif

		// void sk_canvas_set_matrix(sk_canvas_t* ccanvas, const sk_matrix_t* matrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_set_matrix (sk_canvas_t ccanvas, SKMatrix* matrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_set_matrix (sk_canvas_t ccanvas, SKMatrix* matrix);
		}
		private static Delegates.sk_canvas_set_matrix sk_canvas_set_matrix_delegate;
		internal static void sk_canvas_set_matrix (sk_canvas_t ccanvas, SKMatrix* matrix) =>
			(sk_canvas_set_matrix_delegate ??= GetSymbol<Delegates.sk_canvas_set_matrix> ("sk_canvas_set_matrix")).Invoke (ccanvas, matrix);
		#endif

		// void sk_canvas_skew(sk_canvas_t*, float sx, float sy)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_skew (sk_canvas_t param0, Single sx, Single sy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_skew (sk_canvas_t param0, Single sx, Single sy);
		}
		private static Delegates.sk_canvas_skew sk_canvas_skew_delegate;
		internal static void sk_canvas_skew (sk_canvas_t param0, Single sx, Single sy) =>
			(sk_canvas_skew_delegate ??= GetSymbol<Delegates.sk_canvas_skew> ("sk_canvas_skew")).Invoke (param0, sx, sy);
		#endif

		// void sk_canvas_translate(sk_canvas_t*, float dx, float dy)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_translate (sk_canvas_t param0, Single dx, Single dy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_translate (sk_canvas_t param0, Single dx, Single dy);
		}
		private static Delegates.sk_canvas_translate sk_canvas_translate_delegate;
		internal static void sk_canvas_translate (sk_canvas_t param0, Single dx, Single dy) =>
			(sk_canvas_translate_delegate ??= GetSymbol<Delegates.sk_canvas_translate> ("sk_canvas_translate")).Invoke (param0, dx, dy);
		#endif

		// void sk_nodraw_canvas_destroy(sk_nodraw_canvas_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nodraw_canvas_destroy (sk_nodraw_canvas_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_nodraw_canvas_destroy (sk_nodraw_canvas_t param0);
		}
		private static Delegates.sk_nodraw_canvas_destroy sk_nodraw_canvas_destroy_delegate;
		internal static void sk_nodraw_canvas_destroy (sk_nodraw_canvas_t param0) =>
			(sk_nodraw_canvas_destroy_delegate ??= GetSymbol<Delegates.sk_nodraw_canvas_destroy> ("sk_nodraw_canvas_destroy")).Invoke (param0);
		#endif

		// sk_nodraw_canvas_t* sk_nodraw_canvas_new(int width, int height)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_nodraw_canvas_t sk_nodraw_canvas_new (Int32 width, Int32 height);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_nodraw_canvas_t sk_nodraw_canvas_new (Int32 width, Int32 height);
		}
		private static Delegates.sk_nodraw_canvas_new sk_nodraw_canvas_new_delegate;
		internal static sk_nodraw_canvas_t sk_nodraw_canvas_new (Int32 width, Int32 height) =>
			(sk_nodraw_canvas_new_delegate ??= GetSymbol<Delegates.sk_nodraw_canvas_new> ("sk_nodraw_canvas_new")).Invoke (width, height);
		#endif

		// void sk_nway_canvas_add_canvas(sk_nway_canvas_t*, sk_canvas_t* canvas)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nway_canvas_add_canvas (sk_nway_canvas_t param0, sk_canvas_t canvas);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_nway_canvas_add_canvas (sk_nway_canvas_t param0, sk_canvas_t canvas);
		}
		private static Delegates.sk_nway_canvas_add_canvas sk_nway_canvas_add_canvas_delegate;
		internal static void sk_nway_canvas_add_canvas (sk_nway_canvas_t param0, sk_canvas_t canvas) =>
			(sk_nway_canvas_add_canvas_delegate ??= GetSymbol<Delegates.sk_nway_canvas_add_canvas> ("sk_nway_canvas_add_canvas")).Invoke (param0, canvas);
		#endif

		// void sk_nway_canvas_destroy(sk_nway_canvas_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nway_canvas_destroy (sk_nway_canvas_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_nway_canvas_destroy (sk_nway_canvas_t param0);
		}
		private static Delegates.sk_nway_canvas_destroy sk_nway_canvas_destroy_delegate;
		internal static void sk_nway_canvas_destroy (sk_nway_canvas_t param0) =>
			(sk_nway_canvas_destroy_delegate ??= GetSymbol<Delegates.sk_nway_canvas_destroy> ("sk_nway_canvas_destroy")).Invoke (param0);
		#endif

		// sk_nway_canvas_t* sk_nway_canvas_new(int width, int height)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_nway_canvas_t sk_nway_canvas_new (Int32 width, Int32 height);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_nway_canvas_t sk_nway_canvas_new (Int32 width, Int32 height);
		}
		private static Delegates.sk_nway_canvas_new sk_nway_canvas_new_delegate;
		internal static sk_nway_canvas_t sk_nway_canvas_new (Int32 width, Int32 height) =>
			(sk_nway_canvas_new_delegate ??= GetSymbol<Delegates.sk_nway_canvas_new> ("sk_nway_canvas_new")).Invoke (width, height);
		#endif

		// void sk_nway_canvas_remove_all(sk_nway_canvas_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nway_canvas_remove_all (sk_nway_canvas_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_nway_canvas_remove_all (sk_nway_canvas_t param0);
		}
		private static Delegates.sk_nway_canvas_remove_all sk_nway_canvas_remove_all_delegate;
		internal static void sk_nway_canvas_remove_all (sk_nway_canvas_t param0) =>
			(sk_nway_canvas_remove_all_delegate ??= GetSymbol<Delegates.sk_nway_canvas_remove_all> ("sk_nway_canvas_remove_all")).Invoke (param0);
		#endif

		// void sk_nway_canvas_remove_canvas(sk_nway_canvas_t*, sk_canvas_t* canvas)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nway_canvas_remove_canvas (sk_nway_canvas_t param0, sk_canvas_t canvas);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_nway_canvas_remove_canvas (sk_nway_canvas_t param0, sk_canvas_t canvas);
		}
		private static Delegates.sk_nway_canvas_remove_canvas sk_nway_canvas_remove_canvas_delegate;
		internal static void sk_nway_canvas_remove_canvas (sk_nway_canvas_t param0, sk_canvas_t canvas) =>
			(sk_nway_canvas_remove_canvas_delegate ??= GetSymbol<Delegates.sk_nway_canvas_remove_canvas> ("sk_nway_canvas_remove_canvas")).Invoke (param0, canvas);
		#endif

		// void sk_overdraw_canvas_destroy(sk_overdraw_canvas_t* canvas)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_overdraw_canvas_destroy (sk_overdraw_canvas_t canvas);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_overdraw_canvas_destroy (sk_overdraw_canvas_t canvas);
		}
		private static Delegates.sk_overdraw_canvas_destroy sk_overdraw_canvas_destroy_delegate;
		internal static void sk_overdraw_canvas_destroy (sk_overdraw_canvas_t canvas) =>
			(sk_overdraw_canvas_destroy_delegate ??= GetSymbol<Delegates.sk_overdraw_canvas_destroy> ("sk_overdraw_canvas_destroy")).Invoke (canvas);
		#endif

		// sk_overdraw_canvas_t* sk_overdraw_canvas_new(sk_canvas_t* canvas)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_overdraw_canvas_t sk_overdraw_canvas_new (sk_canvas_t canvas);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_overdraw_canvas_t sk_overdraw_canvas_new (sk_canvas_t canvas);
		}
		private static Delegates.sk_overdraw_canvas_new sk_overdraw_canvas_new_delegate;
		internal static sk_overdraw_canvas_t sk_overdraw_canvas_new (sk_canvas_t canvas) =>
			(sk_overdraw_canvas_new_delegate ??= GetSymbol<Delegates.sk_overdraw_canvas_new> ("sk_overdraw_canvas_new")).Invoke (canvas);
		#endif

		#endregion

		#region sk_codec.h

		// void sk_codec_destroy(sk_codec_t* codec)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_codec_destroy (sk_codec_t codec);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_codec_destroy (sk_codec_t codec);
		}
		private static Delegates.sk_codec_destroy sk_codec_destroy_delegate;
		internal static void sk_codec_destroy (sk_codec_t codec) =>
			(sk_codec_destroy_delegate ??= GetSymbol<Delegates.sk_codec_destroy> ("sk_codec_destroy")).Invoke (codec);
		#endif

		// sk_encoded_image_format_t sk_codec_get_encoded_format(sk_codec_t* codec)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKEncodedImageFormat sk_codec_get_encoded_format (sk_codec_t codec);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKEncodedImageFormat sk_codec_get_encoded_format (sk_codec_t codec);
		}
		private static Delegates.sk_codec_get_encoded_format sk_codec_get_encoded_format_delegate;
		internal static SKEncodedImageFormat sk_codec_get_encoded_format (sk_codec_t codec) =>
			(sk_codec_get_encoded_format_delegate ??= GetSymbol<Delegates.sk_codec_get_encoded_format> ("sk_codec_get_encoded_format")).Invoke (codec);
		#endif

		// int sk_codec_get_frame_count(sk_codec_t* codec)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_get_frame_count (sk_codec_t codec);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_codec_get_frame_count (sk_codec_t codec);
		}
		private static Delegates.sk_codec_get_frame_count sk_codec_get_frame_count_delegate;
		internal static Int32 sk_codec_get_frame_count (sk_codec_t codec) =>
			(sk_codec_get_frame_count_delegate ??= GetSymbol<Delegates.sk_codec_get_frame_count> ("sk_codec_get_frame_count")).Invoke (codec);
		#endif

		// void sk_codec_get_frame_info(sk_codec_t* codec, sk_codec_frameinfo_t* frameInfo)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_codec_get_frame_info (sk_codec_t codec, SKCodecFrameInfo* frameInfo);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_codec_get_frame_info (sk_codec_t codec, SKCodecFrameInfo* frameInfo);
		}
		private static Delegates.sk_codec_get_frame_info sk_codec_get_frame_info_delegate;
		internal static void sk_codec_get_frame_info (sk_codec_t codec, SKCodecFrameInfo* frameInfo) =>
			(sk_codec_get_frame_info_delegate ??= GetSymbol<Delegates.sk_codec_get_frame_info> ("sk_codec_get_frame_info")).Invoke (codec, frameInfo);
		#endif

		// bool sk_codec_get_frame_info_for_index(sk_codec_t* codec, int index, sk_codec_frameinfo_t* frameInfo)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_codec_get_frame_info_for_index (sk_codec_t codec, Int32 index, SKCodecFrameInfo* frameInfo);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_codec_get_frame_info_for_index (sk_codec_t codec, Int32 index, SKCodecFrameInfo* frameInfo);
		}
		private static Delegates.sk_codec_get_frame_info_for_index sk_codec_get_frame_info_for_index_delegate;
		internal static bool sk_codec_get_frame_info_for_index (sk_codec_t codec, Int32 index, SKCodecFrameInfo* frameInfo) =>
			(sk_codec_get_frame_info_for_index_delegate ??= GetSymbol<Delegates.sk_codec_get_frame_info_for_index> ("sk_codec_get_frame_info_for_index")).Invoke (codec, index, frameInfo);
		#endif

		// void sk_codec_get_info(sk_codec_t* codec, sk_imageinfo_t* info)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_codec_get_info (sk_codec_t codec, SKImageInfoNative* info);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_codec_get_info (sk_codec_t codec, SKImageInfoNative* info);
		}
		private static Delegates.sk_codec_get_info sk_codec_get_info_delegate;
		internal static void sk_codec_get_info (sk_codec_t codec, SKImageInfoNative* info) =>
			(sk_codec_get_info_delegate ??= GetSymbol<Delegates.sk_codec_get_info> ("sk_codec_get_info")).Invoke (codec, info);
		#endif

		// sk_encodedorigin_t sk_codec_get_origin(sk_codec_t* codec)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKEncodedOrigin sk_codec_get_origin (sk_codec_t codec);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKEncodedOrigin sk_codec_get_origin (sk_codec_t codec);
		}
		private static Delegates.sk_codec_get_origin sk_codec_get_origin_delegate;
		internal static SKEncodedOrigin sk_codec_get_origin (sk_codec_t codec) =>
			(sk_codec_get_origin_delegate ??= GetSymbol<Delegates.sk_codec_get_origin> ("sk_codec_get_origin")).Invoke (codec);
		#endif

		// sk_codec_result_t sk_codec_get_pixels(sk_codec_t* codec, const sk_imageinfo_t* info, void* pixels, size_t rowBytes, const sk_codec_options_t* options)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecResult sk_codec_get_pixels (sk_codec_t codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKCodecResult sk_codec_get_pixels (sk_codec_t codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options);
		}
		private static Delegates.sk_codec_get_pixels sk_codec_get_pixels_delegate;
		internal static SKCodecResult sk_codec_get_pixels (sk_codec_t codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options) =>
			(sk_codec_get_pixels_delegate ??= GetSymbol<Delegates.sk_codec_get_pixels> ("sk_codec_get_pixels")).Invoke (codec, info, pixels, rowBytes, options);
		#endif

		// int sk_codec_get_repetition_count(sk_codec_t* codec)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_get_repetition_count (sk_codec_t codec);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_codec_get_repetition_count (sk_codec_t codec);
		}
		private static Delegates.sk_codec_get_repetition_count sk_codec_get_repetition_count_delegate;
		internal static Int32 sk_codec_get_repetition_count (sk_codec_t codec) =>
			(sk_codec_get_repetition_count_delegate ??= GetSymbol<Delegates.sk_codec_get_repetition_count> ("sk_codec_get_repetition_count")).Invoke (codec);
		#endif

		// void sk_codec_get_scaled_dimensions(sk_codec_t* codec, float desiredScale, sk_isize_t* dimensions)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_codec_get_scaled_dimensions (sk_codec_t codec, Single desiredScale, SKSizeI* dimensions);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_codec_get_scaled_dimensions (sk_codec_t codec, Single desiredScale, SKSizeI* dimensions);
		}
		private static Delegates.sk_codec_get_scaled_dimensions sk_codec_get_scaled_dimensions_delegate;
		internal static void sk_codec_get_scaled_dimensions (sk_codec_t codec, Single desiredScale, SKSizeI* dimensions) =>
			(sk_codec_get_scaled_dimensions_delegate ??= GetSymbol<Delegates.sk_codec_get_scaled_dimensions> ("sk_codec_get_scaled_dimensions")).Invoke (codec, desiredScale, dimensions);
		#endif

		// sk_codec_scanline_order_t sk_codec_get_scanline_order(sk_codec_t* codec)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecScanlineOrder sk_codec_get_scanline_order (sk_codec_t codec);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKCodecScanlineOrder sk_codec_get_scanline_order (sk_codec_t codec);
		}
		private static Delegates.sk_codec_get_scanline_order sk_codec_get_scanline_order_delegate;
		internal static SKCodecScanlineOrder sk_codec_get_scanline_order (sk_codec_t codec) =>
			(sk_codec_get_scanline_order_delegate ??= GetSymbol<Delegates.sk_codec_get_scanline_order> ("sk_codec_get_scanline_order")).Invoke (codec);
		#endif

		// int sk_codec_get_scanlines(sk_codec_t* codec, void* dst, int countLines, size_t rowBytes)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_get_scanlines (sk_codec_t codec, void* dst, Int32 countLines, /* size_t */ IntPtr rowBytes);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_codec_get_scanlines (sk_codec_t codec, void* dst, Int32 countLines, /* size_t */ IntPtr rowBytes);
		}
		private static Delegates.sk_codec_get_scanlines sk_codec_get_scanlines_delegate;
		internal static Int32 sk_codec_get_scanlines (sk_codec_t codec, void* dst, Int32 countLines, /* size_t */ IntPtr rowBytes) =>
			(sk_codec_get_scanlines_delegate ??= GetSymbol<Delegates.sk_codec_get_scanlines> ("sk_codec_get_scanlines")).Invoke (codec, dst, countLines, rowBytes);
		#endif

		// bool sk_codec_get_valid_subset(sk_codec_t* codec, sk_irect_t* desiredSubset)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_codec_get_valid_subset (sk_codec_t codec, SKRectI* desiredSubset);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_codec_get_valid_subset (sk_codec_t codec, SKRectI* desiredSubset);
		}
		private static Delegates.sk_codec_get_valid_subset sk_codec_get_valid_subset_delegate;
		internal static bool sk_codec_get_valid_subset (sk_codec_t codec, SKRectI* desiredSubset) =>
			(sk_codec_get_valid_subset_delegate ??= GetSymbol<Delegates.sk_codec_get_valid_subset> ("sk_codec_get_valid_subset")).Invoke (codec, desiredSubset);
		#endif

		// sk_codec_result_t sk_codec_incremental_decode(sk_codec_t* codec, int* rowsDecoded)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecResult sk_codec_incremental_decode (sk_codec_t codec, Int32* rowsDecoded);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKCodecResult sk_codec_incremental_decode (sk_codec_t codec, Int32* rowsDecoded);
		}
		private static Delegates.sk_codec_incremental_decode sk_codec_incremental_decode_delegate;
		internal static SKCodecResult sk_codec_incremental_decode (sk_codec_t codec, Int32* rowsDecoded) =>
			(sk_codec_incremental_decode_delegate ??= GetSymbol<Delegates.sk_codec_incremental_decode> ("sk_codec_incremental_decode")).Invoke (codec, rowsDecoded);
		#endif

		// size_t sk_codec_min_buffered_bytes_needed()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_codec_min_buffered_bytes_needed ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_codec_min_buffered_bytes_needed ();
		}
		private static Delegates.sk_codec_min_buffered_bytes_needed sk_codec_min_buffered_bytes_needed_delegate;
		internal static /* size_t */ IntPtr sk_codec_min_buffered_bytes_needed () =>
			(sk_codec_min_buffered_bytes_needed_delegate ??= GetSymbol<Delegates.sk_codec_min_buffered_bytes_needed> ("sk_codec_min_buffered_bytes_needed")).Invoke ();
		#endif

		// sk_codec_t* sk_codec_new_from_data(sk_data_t* data)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_codec_t sk_codec_new_from_data (sk_data_t data);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_codec_t sk_codec_new_from_data (sk_data_t data);
		}
		private static Delegates.sk_codec_new_from_data sk_codec_new_from_data_delegate;
		internal static sk_codec_t sk_codec_new_from_data (sk_data_t data) =>
			(sk_codec_new_from_data_delegate ??= GetSymbol<Delegates.sk_codec_new_from_data> ("sk_codec_new_from_data")).Invoke (data);
		#endif

		// sk_codec_t* sk_codec_new_from_stream(sk_stream_t* stream, sk_codec_result_t* result)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_codec_t sk_codec_new_from_stream (sk_stream_t stream, SKCodecResult* result);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_codec_t sk_codec_new_from_stream (sk_stream_t stream, SKCodecResult* result);
		}
		private static Delegates.sk_codec_new_from_stream sk_codec_new_from_stream_delegate;
		internal static sk_codec_t sk_codec_new_from_stream (sk_stream_t stream, SKCodecResult* result) =>
			(sk_codec_new_from_stream_delegate ??= GetSymbol<Delegates.sk_codec_new_from_stream> ("sk_codec_new_from_stream")).Invoke (stream, result);
		#endif

		// int sk_codec_next_scanline(sk_codec_t* codec)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_next_scanline (sk_codec_t codec);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_codec_next_scanline (sk_codec_t codec);
		}
		private static Delegates.sk_codec_next_scanline sk_codec_next_scanline_delegate;
		internal static Int32 sk_codec_next_scanline (sk_codec_t codec) =>
			(sk_codec_next_scanline_delegate ??= GetSymbol<Delegates.sk_codec_next_scanline> ("sk_codec_next_scanline")).Invoke (codec);
		#endif

		// int sk_codec_output_scanline(sk_codec_t* codec, int inputScanline)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_output_scanline (sk_codec_t codec, Int32 inputScanline);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_codec_output_scanline (sk_codec_t codec, Int32 inputScanline);
		}
		private static Delegates.sk_codec_output_scanline sk_codec_output_scanline_delegate;
		internal static Int32 sk_codec_output_scanline (sk_codec_t codec, Int32 inputScanline) =>
			(sk_codec_output_scanline_delegate ??= GetSymbol<Delegates.sk_codec_output_scanline> ("sk_codec_output_scanline")).Invoke (codec, inputScanline);
		#endif

		// bool sk_codec_skip_scanlines(sk_codec_t* codec, int countLines)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_codec_skip_scanlines (sk_codec_t codec, Int32 countLines);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_codec_skip_scanlines (sk_codec_t codec, Int32 countLines);
		}
		private static Delegates.sk_codec_skip_scanlines sk_codec_skip_scanlines_delegate;
		internal static bool sk_codec_skip_scanlines (sk_codec_t codec, Int32 countLines) =>
			(sk_codec_skip_scanlines_delegate ??= GetSymbol<Delegates.sk_codec_skip_scanlines> ("sk_codec_skip_scanlines")).Invoke (codec, countLines);
		#endif

		// sk_codec_result_t sk_codec_start_incremental_decode(sk_codec_t* codec, const sk_imageinfo_t* info, void* pixels, size_t rowBytes, const sk_codec_options_t* options)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecResult sk_codec_start_incremental_decode (sk_codec_t codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKCodecResult sk_codec_start_incremental_decode (sk_codec_t codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options);
		}
		private static Delegates.sk_codec_start_incremental_decode sk_codec_start_incremental_decode_delegate;
		internal static SKCodecResult sk_codec_start_incremental_decode (sk_codec_t codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options) =>
			(sk_codec_start_incremental_decode_delegate ??= GetSymbol<Delegates.sk_codec_start_incremental_decode> ("sk_codec_start_incremental_decode")).Invoke (codec, info, pixels, rowBytes, options);
		#endif

		// sk_codec_result_t sk_codec_start_scanline_decode(sk_codec_t* codec, const sk_imageinfo_t* info, const sk_codec_options_t* options)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecResult sk_codec_start_scanline_decode (sk_codec_t codec, SKImageInfoNative* info, SKCodecOptionsInternal* options);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKCodecResult sk_codec_start_scanline_decode (sk_codec_t codec, SKImageInfoNative* info, SKCodecOptionsInternal* options);
		}
		private static Delegates.sk_codec_start_scanline_decode sk_codec_start_scanline_decode_delegate;
		internal static SKCodecResult sk_codec_start_scanline_decode (sk_codec_t codec, SKImageInfoNative* info, SKCodecOptionsInternal* options) =>
			(sk_codec_start_scanline_decode_delegate ??= GetSymbol<Delegates.sk_codec_start_scanline_decode> ("sk_codec_start_scanline_decode")).Invoke (codec, info, options);
		#endif

		#endregion

		#region sk_colorfilter.h

		// sk_colorfilter_t* sk_colorfilter_new_color_matrix(const float[20] array = 20)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_color_matrix (Single* array);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_color_matrix (Single* array);
		}
		private static Delegates.sk_colorfilter_new_color_matrix sk_colorfilter_new_color_matrix_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_color_matrix (Single* array) =>
			(sk_colorfilter_new_color_matrix_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_color_matrix> ("sk_colorfilter_new_color_matrix")).Invoke (array);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_compose(sk_colorfilter_t* outer, sk_colorfilter_t* inner)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_compose (sk_colorfilter_t outer, sk_colorfilter_t inner);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_compose (sk_colorfilter_t outer, sk_colorfilter_t inner);
		}
		private static Delegates.sk_colorfilter_new_compose sk_colorfilter_new_compose_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_compose (sk_colorfilter_t outer, sk_colorfilter_t inner) =>
			(sk_colorfilter_new_compose_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_compose> ("sk_colorfilter_new_compose")).Invoke (outer, inner);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_high_contrast(const sk_highcontrastconfig_t* config)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_high_contrast (SKHighContrastConfig* config);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_high_contrast (SKHighContrastConfig* config);
		}
		private static Delegates.sk_colorfilter_new_high_contrast sk_colorfilter_new_high_contrast_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_high_contrast (SKHighContrastConfig* config) =>
			(sk_colorfilter_new_high_contrast_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_high_contrast> ("sk_colorfilter_new_high_contrast")).Invoke (config);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_lighting(sk_color_t mul, sk_color_t add)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_lighting (UInt32 mul, UInt32 add);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_lighting (UInt32 mul, UInt32 add);
		}
		private static Delegates.sk_colorfilter_new_lighting sk_colorfilter_new_lighting_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_lighting (UInt32 mul, UInt32 add) =>
			(sk_colorfilter_new_lighting_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_lighting> ("sk_colorfilter_new_lighting")).Invoke (mul, add);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_luma_color()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_luma_color ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_luma_color ();
		}
		private static Delegates.sk_colorfilter_new_luma_color sk_colorfilter_new_luma_color_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_luma_color () =>
			(sk_colorfilter_new_luma_color_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_luma_color> ("sk_colorfilter_new_luma_color")).Invoke ();
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_mode(sk_color_t c, sk_blendmode_t mode)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_mode (UInt32 c, SKBlendMode mode);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_mode (UInt32 c, SKBlendMode mode);
		}
		private static Delegates.sk_colorfilter_new_mode sk_colorfilter_new_mode_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_mode (UInt32 c, SKBlendMode mode) =>
			(sk_colorfilter_new_mode_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_mode> ("sk_colorfilter_new_mode")).Invoke (c, mode);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_table(const uint8_t[256] table = 256)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_table (Byte* table);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_table (Byte* table);
		}
		private static Delegates.sk_colorfilter_new_table sk_colorfilter_new_table_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_table (Byte* table) =>
			(sk_colorfilter_new_table_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_table> ("sk_colorfilter_new_table")).Invoke (table);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_table_argb(const uint8_t[256] tableA = 256, const uint8_t[256] tableR = 256, const uint8_t[256] tableG = 256, const uint8_t[256] tableB = 256)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_table_argb (Byte* tableA, Byte* tableR, Byte* tableG, Byte* tableB);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_table_argb (Byte* tableA, Byte* tableR, Byte* tableG, Byte* tableB);
		}
		private static Delegates.sk_colorfilter_new_table_argb sk_colorfilter_new_table_argb_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_table_argb (Byte* tableA, Byte* tableR, Byte* tableG, Byte* tableB) =>
			(sk_colorfilter_new_table_argb_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_table_argb> ("sk_colorfilter_new_table_argb")).Invoke (tableA, tableR, tableG, tableB);
		#endif

		// void sk_colorfilter_unref(sk_colorfilter_t* filter)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorfilter_unref (sk_colorfilter_t filter);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorfilter_unref (sk_colorfilter_t filter);
		}
		private static Delegates.sk_colorfilter_unref sk_colorfilter_unref_delegate;
		internal static void sk_colorfilter_unref (sk_colorfilter_t filter) =>
			(sk_colorfilter_unref_delegate ??= GetSymbol<Delegates.sk_colorfilter_unref> ("sk_colorfilter_unref")).Invoke (filter);
		#endif

		#endregion

		#region sk_colorspace.h

		// void sk_color4f_from_color(sk_color_t color, sk_color4f_t* color4f)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_color4f_from_color (UInt32 color, SKColorF* color4f);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_color4f_from_color (UInt32 color, SKColorF* color4f);
		}
		private static Delegates.sk_color4f_from_color sk_color4f_from_color_delegate;
		internal static void sk_color4f_from_color (UInt32 color, SKColorF* color4f) =>
			(sk_color4f_from_color_delegate ??= GetSymbol<Delegates.sk_color4f_from_color> ("sk_color4f_from_color")).Invoke (color, color4f);
		#endif

		// sk_color_t sk_color4f_to_color(const sk_color4f_t* color4f)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_color4f_to_color (SKColorF* color4f);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_color4f_to_color (SKColorF* color4f);
		}
		private static Delegates.sk_color4f_to_color sk_color4f_to_color_delegate;
		internal static UInt32 sk_color4f_to_color (SKColorF* color4f) =>
			(sk_color4f_to_color_delegate ??= GetSymbol<Delegates.sk_color4f_to_color> ("sk_color4f_to_color")).Invoke (color4f);
		#endif

		// bool sk_colorspace_equals(const sk_colorspace_t* src, const sk_colorspace_t* dst)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_equals (sk_colorspace_t src, sk_colorspace_t dst);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_equals (sk_colorspace_t src, sk_colorspace_t dst);
		}
		private static Delegates.sk_colorspace_equals sk_colorspace_equals_delegate;
		internal static bool sk_colorspace_equals (sk_colorspace_t src, sk_colorspace_t dst) =>
			(sk_colorspace_equals_delegate ??= GetSymbol<Delegates.sk_colorspace_equals> ("sk_colorspace_equals")).Invoke (src, dst);
		#endif

		// bool sk_colorspace_gamma_close_to_srgb(const sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_gamma_close_to_srgb (sk_colorspace_t colorspace);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_gamma_close_to_srgb (sk_colorspace_t colorspace);
		}
		private static Delegates.sk_colorspace_gamma_close_to_srgb sk_colorspace_gamma_close_to_srgb_delegate;
		internal static bool sk_colorspace_gamma_close_to_srgb (sk_colorspace_t colorspace) =>
			(sk_colorspace_gamma_close_to_srgb_delegate ??= GetSymbol<Delegates.sk_colorspace_gamma_close_to_srgb> ("sk_colorspace_gamma_close_to_srgb")).Invoke (colorspace);
		#endif

		// bool sk_colorspace_gamma_is_linear(const sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_gamma_is_linear (sk_colorspace_t colorspace);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_gamma_is_linear (sk_colorspace_t colorspace);
		}
		private static Delegates.sk_colorspace_gamma_is_linear sk_colorspace_gamma_is_linear_delegate;
		internal static bool sk_colorspace_gamma_is_linear (sk_colorspace_t colorspace) =>
			(sk_colorspace_gamma_is_linear_delegate ??= GetSymbol<Delegates.sk_colorspace_gamma_is_linear> ("sk_colorspace_gamma_is_linear")).Invoke (colorspace);
		#endif

		// void sk_colorspace_icc_profile_delete(sk_colorspace_icc_profile_t* profile)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_icc_profile_delete (sk_colorspace_icc_profile_t profile);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_icc_profile_delete (sk_colorspace_icc_profile_t profile);
		}
		private static Delegates.sk_colorspace_icc_profile_delete sk_colorspace_icc_profile_delete_delegate;
		internal static void sk_colorspace_icc_profile_delete (sk_colorspace_icc_profile_t profile) =>
			(sk_colorspace_icc_profile_delete_delegate ??= GetSymbol<Delegates.sk_colorspace_icc_profile_delete> ("sk_colorspace_icc_profile_delete")).Invoke (profile);
		#endif

		// const uint8_t* sk_colorspace_icc_profile_get_buffer(const sk_colorspace_icc_profile_t* profile, uint32_t* size)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte* sk_colorspace_icc_profile_get_buffer (sk_colorspace_icc_profile_t profile, UInt32* size);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte* sk_colorspace_icc_profile_get_buffer (sk_colorspace_icc_profile_t profile, UInt32* size);
		}
		private static Delegates.sk_colorspace_icc_profile_get_buffer sk_colorspace_icc_profile_get_buffer_delegate;
		internal static Byte* sk_colorspace_icc_profile_get_buffer (sk_colorspace_icc_profile_t profile, UInt32* size) =>
			(sk_colorspace_icc_profile_get_buffer_delegate ??= GetSymbol<Delegates.sk_colorspace_icc_profile_get_buffer> ("sk_colorspace_icc_profile_get_buffer")).Invoke (profile, size);
		#endif

		// bool sk_colorspace_icc_profile_get_to_xyzd50(const sk_colorspace_icc_profile_t* profile, sk_colorspace_xyz_t* toXYZD50)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_icc_profile_get_to_xyzd50 (sk_colorspace_icc_profile_t profile, SKColorSpaceXyz* toXYZD50);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_icc_profile_get_to_xyzd50 (sk_colorspace_icc_profile_t profile, SKColorSpaceXyz* toXYZD50);
		}
		private static Delegates.sk_colorspace_icc_profile_get_to_xyzd50 sk_colorspace_icc_profile_get_to_xyzd50_delegate;
		internal static bool sk_colorspace_icc_profile_get_to_xyzd50 (sk_colorspace_icc_profile_t profile, SKColorSpaceXyz* toXYZD50) =>
			(sk_colorspace_icc_profile_get_to_xyzd50_delegate ??= GetSymbol<Delegates.sk_colorspace_icc_profile_get_to_xyzd50> ("sk_colorspace_icc_profile_get_to_xyzd50")).Invoke (profile, toXYZD50);
		#endif

		// sk_colorspace_icc_profile_t* sk_colorspace_icc_profile_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_icc_profile_t sk_colorspace_icc_profile_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_icc_profile_t sk_colorspace_icc_profile_new ();
		}
		private static Delegates.sk_colorspace_icc_profile_new sk_colorspace_icc_profile_new_delegate;
		internal static sk_colorspace_icc_profile_t sk_colorspace_icc_profile_new () =>
			(sk_colorspace_icc_profile_new_delegate ??= GetSymbol<Delegates.sk_colorspace_icc_profile_new> ("sk_colorspace_icc_profile_new")).Invoke ();
		#endif

		// bool sk_colorspace_icc_profile_parse(const void* buffer, size_t length, sk_colorspace_icc_profile_t* profile)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_icc_profile_parse (void* buffer, /* size_t */ IntPtr length, sk_colorspace_icc_profile_t profile);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_icc_profile_parse (void* buffer, /* size_t */ IntPtr length, sk_colorspace_icc_profile_t profile);
		}
		private static Delegates.sk_colorspace_icc_profile_parse sk_colorspace_icc_profile_parse_delegate;
		internal static bool sk_colorspace_icc_profile_parse (void* buffer, /* size_t */ IntPtr length, sk_colorspace_icc_profile_t profile) =>
			(sk_colorspace_icc_profile_parse_delegate ??= GetSymbol<Delegates.sk_colorspace_icc_profile_parse> ("sk_colorspace_icc_profile_parse")).Invoke (buffer, length, profile);
		#endif

		// bool sk_colorspace_is_numerical_transfer_fn(const sk_colorspace_t* colorspace, sk_colorspace_transfer_fn_t* transferFn)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_is_numerical_transfer_fn (sk_colorspace_t colorspace, SKColorSpaceTransferFn* transferFn);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_is_numerical_transfer_fn (sk_colorspace_t colorspace, SKColorSpaceTransferFn* transferFn);
		}
		private static Delegates.sk_colorspace_is_numerical_transfer_fn sk_colorspace_is_numerical_transfer_fn_delegate;
		internal static bool sk_colorspace_is_numerical_transfer_fn (sk_colorspace_t colorspace, SKColorSpaceTransferFn* transferFn) =>
			(sk_colorspace_is_numerical_transfer_fn_delegate ??= GetSymbol<Delegates.sk_colorspace_is_numerical_transfer_fn> ("sk_colorspace_is_numerical_transfer_fn")).Invoke (colorspace, transferFn);
		#endif

		// bool sk_colorspace_is_srgb(const sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_is_srgb (sk_colorspace_t colorspace);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_is_srgb (sk_colorspace_t colorspace);
		}
		private static Delegates.sk_colorspace_is_srgb sk_colorspace_is_srgb_delegate;
		internal static bool sk_colorspace_is_srgb (sk_colorspace_t colorspace) =>
			(sk_colorspace_is_srgb_delegate ??= GetSymbol<Delegates.sk_colorspace_is_srgb> ("sk_colorspace_is_srgb")).Invoke (colorspace);
		#endif

		// sk_colorspace_t* sk_colorspace_make_linear_gamma(const sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_make_linear_gamma (sk_colorspace_t colorspace);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_colorspace_make_linear_gamma (sk_colorspace_t colorspace);
		}
		private static Delegates.sk_colorspace_make_linear_gamma sk_colorspace_make_linear_gamma_delegate;
		internal static sk_colorspace_t sk_colorspace_make_linear_gamma (sk_colorspace_t colorspace) =>
			(sk_colorspace_make_linear_gamma_delegate ??= GetSymbol<Delegates.sk_colorspace_make_linear_gamma> ("sk_colorspace_make_linear_gamma")).Invoke (colorspace);
		#endif

		// sk_colorspace_t* sk_colorspace_make_srgb_gamma(const sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_make_srgb_gamma (sk_colorspace_t colorspace);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_colorspace_make_srgb_gamma (sk_colorspace_t colorspace);
		}
		private static Delegates.sk_colorspace_make_srgb_gamma sk_colorspace_make_srgb_gamma_delegate;
		internal static sk_colorspace_t sk_colorspace_make_srgb_gamma (sk_colorspace_t colorspace) =>
			(sk_colorspace_make_srgb_gamma_delegate ??= GetSymbol<Delegates.sk_colorspace_make_srgb_gamma> ("sk_colorspace_make_srgb_gamma")).Invoke (colorspace);
		#endif

		// sk_colorspace_t* sk_colorspace_new_icc(const sk_colorspace_icc_profile_t* profile)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_icc (sk_colorspace_icc_profile_t profile);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_colorspace_new_icc (sk_colorspace_icc_profile_t profile);
		}
		private static Delegates.sk_colorspace_new_icc sk_colorspace_new_icc_delegate;
		internal static sk_colorspace_t sk_colorspace_new_icc (sk_colorspace_icc_profile_t profile) =>
			(sk_colorspace_new_icc_delegate ??= GetSymbol<Delegates.sk_colorspace_new_icc> ("sk_colorspace_new_icc")).Invoke (profile);
		#endif

		// sk_colorspace_t* sk_colorspace_new_rgb(const sk_colorspace_transfer_fn_t* transferFn, const sk_colorspace_xyz_t* toXYZD50)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_rgb (SKColorSpaceTransferFn* transferFn, SKColorSpaceXyz* toXYZD50);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_colorspace_new_rgb (SKColorSpaceTransferFn* transferFn, SKColorSpaceXyz* toXYZD50);
		}
		private static Delegates.sk_colorspace_new_rgb sk_colorspace_new_rgb_delegate;
		internal static sk_colorspace_t sk_colorspace_new_rgb (SKColorSpaceTransferFn* transferFn, SKColorSpaceXyz* toXYZD50) =>
			(sk_colorspace_new_rgb_delegate ??= GetSymbol<Delegates.sk_colorspace_new_rgb> ("sk_colorspace_new_rgb")).Invoke (transferFn, toXYZD50);
		#endif

		// sk_colorspace_t* sk_colorspace_new_srgb()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_srgb ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_colorspace_new_srgb ();
		}
		private static Delegates.sk_colorspace_new_srgb sk_colorspace_new_srgb_delegate;
		internal static sk_colorspace_t sk_colorspace_new_srgb () =>
			(sk_colorspace_new_srgb_delegate ??= GetSymbol<Delegates.sk_colorspace_new_srgb> ("sk_colorspace_new_srgb")).Invoke ();
		#endif

		// sk_colorspace_t* sk_colorspace_new_srgb_linear()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_colorspace_new_srgb_linear ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_colorspace_new_srgb_linear ();
		}
		private static Delegates.sk_colorspace_new_srgb_linear sk_colorspace_new_srgb_linear_delegate;
		internal static sk_colorspace_t sk_colorspace_new_srgb_linear () =>
			(sk_colorspace_new_srgb_linear_delegate ??= GetSymbol<Delegates.sk_colorspace_new_srgb_linear> ("sk_colorspace_new_srgb_linear")).Invoke ();
		#endif

		// bool sk_colorspace_primaries_to_xyzd50(const sk_colorspace_primaries_t* primaries, sk_colorspace_xyz_t* toXYZD50)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_primaries_to_xyzd50 (SKColorSpacePrimaries* primaries, SKColorSpaceXyz* toXYZD50);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_primaries_to_xyzd50 (SKColorSpacePrimaries* primaries, SKColorSpaceXyz* toXYZD50);
		}
		private static Delegates.sk_colorspace_primaries_to_xyzd50 sk_colorspace_primaries_to_xyzd50_delegate;
		internal static bool sk_colorspace_primaries_to_xyzd50 (SKColorSpacePrimaries* primaries, SKColorSpaceXyz* toXYZD50) =>
			(sk_colorspace_primaries_to_xyzd50_delegate ??= GetSymbol<Delegates.sk_colorspace_primaries_to_xyzd50> ("sk_colorspace_primaries_to_xyzd50")).Invoke (primaries, toXYZD50);
		#endif

		// void sk_colorspace_ref(sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_ref (sk_colorspace_t colorspace);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_ref (sk_colorspace_t colorspace);
		}
		private static Delegates.sk_colorspace_ref sk_colorspace_ref_delegate;
		internal static void sk_colorspace_ref (sk_colorspace_t colorspace) =>
			(sk_colorspace_ref_delegate ??= GetSymbol<Delegates.sk_colorspace_ref> ("sk_colorspace_ref")).Invoke (colorspace);
		#endif

		// void sk_colorspace_to_profile(const sk_colorspace_t* colorspace, sk_colorspace_icc_profile_t* profile)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_to_profile (sk_colorspace_t colorspace, sk_colorspace_icc_profile_t profile);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_to_profile (sk_colorspace_t colorspace, sk_colorspace_icc_profile_t profile);
		}
		private static Delegates.sk_colorspace_to_profile sk_colorspace_to_profile_delegate;
		internal static void sk_colorspace_to_profile (sk_colorspace_t colorspace, sk_colorspace_icc_profile_t profile) =>
			(sk_colorspace_to_profile_delegate ??= GetSymbol<Delegates.sk_colorspace_to_profile> ("sk_colorspace_to_profile")).Invoke (colorspace, profile);
		#endif

		// bool sk_colorspace_to_xyzd50(const sk_colorspace_t* colorspace, sk_colorspace_xyz_t* toXYZD50)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_to_xyzd50 (sk_colorspace_t colorspace, SKColorSpaceXyz* toXYZD50);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_to_xyzd50 (sk_colorspace_t colorspace, SKColorSpaceXyz* toXYZD50);
		}
		private static Delegates.sk_colorspace_to_xyzd50 sk_colorspace_to_xyzd50_delegate;
		internal static bool sk_colorspace_to_xyzd50 (sk_colorspace_t colorspace, SKColorSpaceXyz* toXYZD50) =>
			(sk_colorspace_to_xyzd50_delegate ??= GetSymbol<Delegates.sk_colorspace_to_xyzd50> ("sk_colorspace_to_xyzd50")).Invoke (colorspace, toXYZD50);
		#endif

		// float sk_colorspace_transfer_fn_eval(const sk_colorspace_transfer_fn_t* transferFn, float x)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_colorspace_transfer_fn_eval (SKColorSpaceTransferFn* transferFn, Single x);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_colorspace_transfer_fn_eval (SKColorSpaceTransferFn* transferFn, Single x);
		}
		private static Delegates.sk_colorspace_transfer_fn_eval sk_colorspace_transfer_fn_eval_delegate;
		internal static Single sk_colorspace_transfer_fn_eval (SKColorSpaceTransferFn* transferFn, Single x) =>
			(sk_colorspace_transfer_fn_eval_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_eval> ("sk_colorspace_transfer_fn_eval")).Invoke (transferFn, x);
		#endif

		// bool sk_colorspace_transfer_fn_invert(const sk_colorspace_transfer_fn_t* src, sk_colorspace_transfer_fn_t* dst)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_transfer_fn_invert (SKColorSpaceTransferFn* src, SKColorSpaceTransferFn* dst);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_transfer_fn_invert (SKColorSpaceTransferFn* src, SKColorSpaceTransferFn* dst);
		}
		private static Delegates.sk_colorspace_transfer_fn_invert sk_colorspace_transfer_fn_invert_delegate;
		internal static bool sk_colorspace_transfer_fn_invert (SKColorSpaceTransferFn* src, SKColorSpaceTransferFn* dst) =>
			(sk_colorspace_transfer_fn_invert_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_invert> ("sk_colorspace_transfer_fn_invert")).Invoke (src, dst);
		#endif

		// void sk_colorspace_transfer_fn_named_2dot2(sk_colorspace_transfer_fn_t* transferFn)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_transfer_fn_named_2dot2 (SKColorSpaceTransferFn* transferFn);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_transfer_fn_named_2dot2 (SKColorSpaceTransferFn* transferFn);
		}
		private static Delegates.sk_colorspace_transfer_fn_named_2dot2 sk_colorspace_transfer_fn_named_2dot2_delegate;
		internal static void sk_colorspace_transfer_fn_named_2dot2 (SKColorSpaceTransferFn* transferFn) =>
			(sk_colorspace_transfer_fn_named_2dot2_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_named_2dot2> ("sk_colorspace_transfer_fn_named_2dot2")).Invoke (transferFn);
		#endif

		// void sk_colorspace_transfer_fn_named_hlg(sk_colorspace_transfer_fn_t* transferFn)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_transfer_fn_named_hlg (SKColorSpaceTransferFn* transferFn);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_transfer_fn_named_hlg (SKColorSpaceTransferFn* transferFn);
		}
		private static Delegates.sk_colorspace_transfer_fn_named_hlg sk_colorspace_transfer_fn_named_hlg_delegate;
		internal static void sk_colorspace_transfer_fn_named_hlg (SKColorSpaceTransferFn* transferFn) =>
			(sk_colorspace_transfer_fn_named_hlg_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_named_hlg> ("sk_colorspace_transfer_fn_named_hlg")).Invoke (transferFn);
		#endif

		// void sk_colorspace_transfer_fn_named_linear(sk_colorspace_transfer_fn_t* transferFn)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_transfer_fn_named_linear (SKColorSpaceTransferFn* transferFn);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_transfer_fn_named_linear (SKColorSpaceTransferFn* transferFn);
		}
		private static Delegates.sk_colorspace_transfer_fn_named_linear sk_colorspace_transfer_fn_named_linear_delegate;
		internal static void sk_colorspace_transfer_fn_named_linear (SKColorSpaceTransferFn* transferFn) =>
			(sk_colorspace_transfer_fn_named_linear_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_named_linear> ("sk_colorspace_transfer_fn_named_linear")).Invoke (transferFn);
		#endif

		// void sk_colorspace_transfer_fn_named_pq(sk_colorspace_transfer_fn_t* transferFn)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_transfer_fn_named_pq (SKColorSpaceTransferFn* transferFn);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_transfer_fn_named_pq (SKColorSpaceTransferFn* transferFn);
		}
		private static Delegates.sk_colorspace_transfer_fn_named_pq sk_colorspace_transfer_fn_named_pq_delegate;
		internal static void sk_colorspace_transfer_fn_named_pq (SKColorSpaceTransferFn* transferFn) =>
			(sk_colorspace_transfer_fn_named_pq_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_named_pq> ("sk_colorspace_transfer_fn_named_pq")).Invoke (transferFn);
		#endif

		// void sk_colorspace_transfer_fn_named_rec2020(sk_colorspace_transfer_fn_t* transferFn)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_transfer_fn_named_rec2020 (SKColorSpaceTransferFn* transferFn);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_transfer_fn_named_rec2020 (SKColorSpaceTransferFn* transferFn);
		}
		private static Delegates.sk_colorspace_transfer_fn_named_rec2020 sk_colorspace_transfer_fn_named_rec2020_delegate;
		internal static void sk_colorspace_transfer_fn_named_rec2020 (SKColorSpaceTransferFn* transferFn) =>
			(sk_colorspace_transfer_fn_named_rec2020_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_named_rec2020> ("sk_colorspace_transfer_fn_named_rec2020")).Invoke (transferFn);
		#endif

		// void sk_colorspace_transfer_fn_named_srgb(sk_colorspace_transfer_fn_t* transferFn)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_transfer_fn_named_srgb (SKColorSpaceTransferFn* transferFn);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_transfer_fn_named_srgb (SKColorSpaceTransferFn* transferFn);
		}
		private static Delegates.sk_colorspace_transfer_fn_named_srgb sk_colorspace_transfer_fn_named_srgb_delegate;
		internal static void sk_colorspace_transfer_fn_named_srgb (SKColorSpaceTransferFn* transferFn) =>
			(sk_colorspace_transfer_fn_named_srgb_delegate ??= GetSymbol<Delegates.sk_colorspace_transfer_fn_named_srgb> ("sk_colorspace_transfer_fn_named_srgb")).Invoke (transferFn);
		#endif

		// void sk_colorspace_unref(sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_unref (sk_colorspace_t colorspace);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_unref (sk_colorspace_t colorspace);
		}
		private static Delegates.sk_colorspace_unref sk_colorspace_unref_delegate;
		internal static void sk_colorspace_unref (sk_colorspace_t colorspace) =>
			(sk_colorspace_unref_delegate ??= GetSymbol<Delegates.sk_colorspace_unref> ("sk_colorspace_unref")).Invoke (colorspace);
		#endif

		// void sk_colorspace_xyz_concat(const sk_colorspace_xyz_t* a, const sk_colorspace_xyz_t* b, sk_colorspace_xyz_t* result)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_xyz_concat (SKColorSpaceXyz* a, SKColorSpaceXyz* b, SKColorSpaceXyz* result);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_xyz_concat (SKColorSpaceXyz* a, SKColorSpaceXyz* b, SKColorSpaceXyz* result);
		}
		private static Delegates.sk_colorspace_xyz_concat sk_colorspace_xyz_concat_delegate;
		internal static void sk_colorspace_xyz_concat (SKColorSpaceXyz* a, SKColorSpaceXyz* b, SKColorSpaceXyz* result) =>
			(sk_colorspace_xyz_concat_delegate ??= GetSymbol<Delegates.sk_colorspace_xyz_concat> ("sk_colorspace_xyz_concat")).Invoke (a, b, result);
		#endif

		// bool sk_colorspace_xyz_invert(const sk_colorspace_xyz_t* src, sk_colorspace_xyz_t* dst)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_colorspace_xyz_invert (SKColorSpaceXyz* src, SKColorSpaceXyz* dst);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_colorspace_xyz_invert (SKColorSpaceXyz* src, SKColorSpaceXyz* dst);
		}
		private static Delegates.sk_colorspace_xyz_invert sk_colorspace_xyz_invert_delegate;
		internal static bool sk_colorspace_xyz_invert (SKColorSpaceXyz* src, SKColorSpaceXyz* dst) =>
			(sk_colorspace_xyz_invert_delegate ??= GetSymbol<Delegates.sk_colorspace_xyz_invert> ("sk_colorspace_xyz_invert")).Invoke (src, dst);
		#endif

		// void sk_colorspace_xyz_named_adobe_rgb(sk_colorspace_xyz_t* xyz)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_xyz_named_adobe_rgb (SKColorSpaceXyz* xyz);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_xyz_named_adobe_rgb (SKColorSpaceXyz* xyz);
		}
		private static Delegates.sk_colorspace_xyz_named_adobe_rgb sk_colorspace_xyz_named_adobe_rgb_delegate;
		internal static void sk_colorspace_xyz_named_adobe_rgb (SKColorSpaceXyz* xyz) =>
			(sk_colorspace_xyz_named_adobe_rgb_delegate ??= GetSymbol<Delegates.sk_colorspace_xyz_named_adobe_rgb> ("sk_colorspace_xyz_named_adobe_rgb")).Invoke (xyz);
		#endif

		// void sk_colorspace_xyz_named_display_p3(sk_colorspace_xyz_t* xyz)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_xyz_named_display_p3 (SKColorSpaceXyz* xyz);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_xyz_named_display_p3 (SKColorSpaceXyz* xyz);
		}
		private static Delegates.sk_colorspace_xyz_named_display_p3 sk_colorspace_xyz_named_display_p3_delegate;
		internal static void sk_colorspace_xyz_named_display_p3 (SKColorSpaceXyz* xyz) =>
			(sk_colorspace_xyz_named_display_p3_delegate ??= GetSymbol<Delegates.sk_colorspace_xyz_named_display_p3> ("sk_colorspace_xyz_named_display_p3")).Invoke (xyz);
		#endif

		// void sk_colorspace_xyz_named_rec2020(sk_colorspace_xyz_t* xyz)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_xyz_named_rec2020 (SKColorSpaceXyz* xyz);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_xyz_named_rec2020 (SKColorSpaceXyz* xyz);
		}
		private static Delegates.sk_colorspace_xyz_named_rec2020 sk_colorspace_xyz_named_rec2020_delegate;
		internal static void sk_colorspace_xyz_named_rec2020 (SKColorSpaceXyz* xyz) =>
			(sk_colorspace_xyz_named_rec2020_delegate ??= GetSymbol<Delegates.sk_colorspace_xyz_named_rec2020> ("sk_colorspace_xyz_named_rec2020")).Invoke (xyz);
		#endif

		// void sk_colorspace_xyz_named_srgb(sk_colorspace_xyz_t* xyz)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_xyz_named_srgb (SKColorSpaceXyz* xyz);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_xyz_named_srgb (SKColorSpaceXyz* xyz);
		}
		private static Delegates.sk_colorspace_xyz_named_srgb sk_colorspace_xyz_named_srgb_delegate;
		internal static void sk_colorspace_xyz_named_srgb (SKColorSpaceXyz* xyz) =>
			(sk_colorspace_xyz_named_srgb_delegate ??= GetSymbol<Delegates.sk_colorspace_xyz_named_srgb> ("sk_colorspace_xyz_named_srgb")).Invoke (xyz);
		#endif

		// void sk_colorspace_xyz_named_xyz(sk_colorspace_xyz_t* xyz)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorspace_xyz_named_xyz (SKColorSpaceXyz* xyz);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorspace_xyz_named_xyz (SKColorSpaceXyz* xyz);
		}
		private static Delegates.sk_colorspace_xyz_named_xyz sk_colorspace_xyz_named_xyz_delegate;
		internal static void sk_colorspace_xyz_named_xyz (SKColorSpaceXyz* xyz) =>
			(sk_colorspace_xyz_named_xyz_delegate ??= GetSymbol<Delegates.sk_colorspace_xyz_named_xyz> ("sk_colorspace_xyz_named_xyz")).Invoke (xyz);
		#endif

		#endregion

		#region sk_colortable.h

		// int sk_colortable_count(const sk_colortable_t* ctable)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_colortable_count (sk_colortable_t ctable);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_colortable_count (sk_colortable_t ctable);
		}
		private static Delegates.sk_colortable_count sk_colortable_count_delegate;
		internal static Int32 sk_colortable_count (sk_colortable_t ctable) =>
			(sk_colortable_count_delegate ??= GetSymbol<Delegates.sk_colortable_count> ("sk_colortable_count")).Invoke (ctable);
		#endif

		// sk_colortable_t* sk_colortable_new(const sk_pmcolor_t* colors, int count)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colortable_t sk_colortable_new (UInt32* colors, Int32 count);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colortable_t sk_colortable_new (UInt32* colors, Int32 count);
		}
		private static Delegates.sk_colortable_new sk_colortable_new_delegate;
		internal static sk_colortable_t sk_colortable_new (UInt32* colors, Int32 count) =>
			(sk_colortable_new_delegate ??= GetSymbol<Delegates.sk_colortable_new> ("sk_colortable_new")).Invoke (colors, count);
		#endif

		// void sk_colortable_read_colors(const sk_colortable_t* ctable, sk_pmcolor_t** colors)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colortable_read_colors (sk_colortable_t ctable, UInt32** colors);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colortable_read_colors (sk_colortable_t ctable, UInt32** colors);
		}
		private static Delegates.sk_colortable_read_colors sk_colortable_read_colors_delegate;
		internal static void sk_colortable_read_colors (sk_colortable_t ctable, UInt32** colors) =>
			(sk_colortable_read_colors_delegate ??= GetSymbol<Delegates.sk_colortable_read_colors> ("sk_colortable_read_colors")).Invoke (ctable, colors);
		#endif

		// void sk_colortable_unref(sk_colortable_t* ctable)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colortable_unref (sk_colortable_t ctable);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colortable_unref (sk_colortable_t ctable);
		}
		private static Delegates.sk_colortable_unref sk_colortable_unref_delegate;
		internal static void sk_colortable_unref (sk_colortable_t ctable) =>
			(sk_colortable_unref_delegate ??= GetSymbol<Delegates.sk_colortable_unref> ("sk_colortable_unref")).Invoke (ctable);
		#endif

		#endregion

		#region sk_data.h

		// const uint8_t* sk_data_get_bytes(const sk_data_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte* sk_data_get_bytes (sk_data_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte* sk_data_get_bytes (sk_data_t param0);
		}
		private static Delegates.sk_data_get_bytes sk_data_get_bytes_delegate;
		internal static Byte* sk_data_get_bytes (sk_data_t param0) =>
			(sk_data_get_bytes_delegate ??= GetSymbol<Delegates.sk_data_get_bytes> ("sk_data_get_bytes")).Invoke (param0);
		#endif

		// const void* sk_data_get_data(const sk_data_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_data_get_data (sk_data_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_data_get_data (sk_data_t param0);
		}
		private static Delegates.sk_data_get_data sk_data_get_data_delegate;
		internal static void* sk_data_get_data (sk_data_t param0) =>
			(sk_data_get_data_delegate ??= GetSymbol<Delegates.sk_data_get_data> ("sk_data_get_data")).Invoke (param0);
		#endif

		// size_t sk_data_get_size(const sk_data_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_data_get_size (sk_data_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_data_get_size (sk_data_t param0);
		}
		private static Delegates.sk_data_get_size sk_data_get_size_delegate;
		internal static /* size_t */ IntPtr sk_data_get_size (sk_data_t param0) =>
			(sk_data_get_size_delegate ??= GetSymbol<Delegates.sk_data_get_size> ("sk_data_get_size")).Invoke (param0);
		#endif

		// sk_data_t* sk_data_new_empty()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_empty ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_data_new_empty ();
		}
		private static Delegates.sk_data_new_empty sk_data_new_empty_delegate;
		internal static sk_data_t sk_data_new_empty () =>
			(sk_data_new_empty_delegate ??= GetSymbol<Delegates.sk_data_new_empty> ("sk_data_new_empty")).Invoke ();
		#endif

		// sk_data_t* sk_data_new_from_file(const char* path)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_from_file (/* char */ void* path);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_data_new_from_file (/* char */ void* path);
		}
		private static Delegates.sk_data_new_from_file sk_data_new_from_file_delegate;
		internal static sk_data_t sk_data_new_from_file (/* char */ void* path) =>
			(sk_data_new_from_file_delegate ??= GetSymbol<Delegates.sk_data_new_from_file> ("sk_data_new_from_file")).Invoke (path);
		#endif

		// sk_data_t* sk_data_new_from_stream(sk_stream_t* stream, size_t length)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_from_stream (sk_stream_t stream, /* size_t */ IntPtr length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_data_new_from_stream (sk_stream_t stream, /* size_t */ IntPtr length);
		}
		private static Delegates.sk_data_new_from_stream sk_data_new_from_stream_delegate;
		internal static sk_data_t sk_data_new_from_stream (sk_stream_t stream, /* size_t */ IntPtr length) =>
			(sk_data_new_from_stream_delegate ??= GetSymbol<Delegates.sk_data_new_from_stream> ("sk_data_new_from_stream")).Invoke (stream, length);
		#endif

		// sk_data_t* sk_data_new_subset(const sk_data_t* src, size_t offset, size_t length)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_subset (sk_data_t src, /* size_t */ IntPtr offset, /* size_t */ IntPtr length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_data_new_subset (sk_data_t src, /* size_t */ IntPtr offset, /* size_t */ IntPtr length);
		}
		private static Delegates.sk_data_new_subset sk_data_new_subset_delegate;
		internal static sk_data_t sk_data_new_subset (sk_data_t src, /* size_t */ IntPtr offset, /* size_t */ IntPtr length) =>
			(sk_data_new_subset_delegate ??= GetSymbol<Delegates.sk_data_new_subset> ("sk_data_new_subset")).Invoke (src, offset, length);
		#endif

		// sk_data_t* sk_data_new_uninitialized(size_t size)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_uninitialized (/* size_t */ IntPtr size);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_data_new_uninitialized (/* size_t */ IntPtr size);
		}
		private static Delegates.sk_data_new_uninitialized sk_data_new_uninitialized_delegate;
		internal static sk_data_t sk_data_new_uninitialized (/* size_t */ IntPtr size) =>
			(sk_data_new_uninitialized_delegate ??= GetSymbol<Delegates.sk_data_new_uninitialized> ("sk_data_new_uninitialized")).Invoke (size);
		#endif

		// sk_data_t* sk_data_new_with_copy(const void* src, size_t length)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_with_copy (void* src, /* size_t */ IntPtr length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_data_new_with_copy (void* src, /* size_t */ IntPtr length);
		}
		private static Delegates.sk_data_new_with_copy sk_data_new_with_copy_delegate;
		internal static sk_data_t sk_data_new_with_copy (void* src, /* size_t */ IntPtr length) =>
			(sk_data_new_with_copy_delegate ??= GetSymbol<Delegates.sk_data_new_with_copy> ("sk_data_new_with_copy")).Invoke (src, length);
		#endif

		// sk_data_t* sk_data_new_with_proc(const void* ptr, size_t length, sk_data_release_proc proc, void* ctx)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_with_proc (void* ptr, /* size_t */ IntPtr length, SKDataReleaseProxyDelegate proc, void* ctx);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_data_new_with_proc (void* ptr, /* size_t */ IntPtr length, SKDataReleaseProxyDelegate proc, void* ctx);
		}
		private static Delegates.sk_data_new_with_proc sk_data_new_with_proc_delegate;
		internal static sk_data_t sk_data_new_with_proc (void* ptr, /* size_t */ IntPtr length, SKDataReleaseProxyDelegate proc, void* ctx) =>
			(sk_data_new_with_proc_delegate ??= GetSymbol<Delegates.sk_data_new_with_proc> ("sk_data_new_with_proc")).Invoke (ptr, length, proc, ctx);
		#endif

		// void sk_data_ref(const sk_data_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_data_ref (sk_data_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_data_ref (sk_data_t param0);
		}
		private static Delegates.sk_data_ref sk_data_ref_delegate;
		internal static void sk_data_ref (sk_data_t param0) =>
			(sk_data_ref_delegate ??= GetSymbol<Delegates.sk_data_ref> ("sk_data_ref")).Invoke (param0);
		#endif

		// void sk_data_unref(const sk_data_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_data_unref (sk_data_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_data_unref (sk_data_t param0);
		}
		private static Delegates.sk_data_unref sk_data_unref_delegate;
		internal static void sk_data_unref (sk_data_t param0) =>
			(sk_data_unref_delegate ??= GetSymbol<Delegates.sk_data_unref> ("sk_data_unref")).Invoke (param0);
		#endif

		#endregion

		#region sk_document.h

		// void sk_document_abort(sk_document_t* document)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_document_abort (sk_document_t document);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_document_abort (sk_document_t document);
		}
		private static Delegates.sk_document_abort sk_document_abort_delegate;
		internal static void sk_document_abort (sk_document_t document) =>
			(sk_document_abort_delegate ??= GetSymbol<Delegates.sk_document_abort> ("sk_document_abort")).Invoke (document);
		#endif

		// sk_canvas_t* sk_document_begin_page(sk_document_t* document, float width, float height, const sk_rect_t* content)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_document_begin_page (sk_document_t document, Single width, Single height, SKRect* content);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_canvas_t sk_document_begin_page (sk_document_t document, Single width, Single height, SKRect* content);
		}
		private static Delegates.sk_document_begin_page sk_document_begin_page_delegate;
		internal static sk_canvas_t sk_document_begin_page (sk_document_t document, Single width, Single height, SKRect* content) =>
			(sk_document_begin_page_delegate ??= GetSymbol<Delegates.sk_document_begin_page> ("sk_document_begin_page")).Invoke (document, width, height, content);
		#endif

		// void sk_document_close(sk_document_t* document)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_document_close (sk_document_t document);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_document_close (sk_document_t document);
		}
		private static Delegates.sk_document_close sk_document_close_delegate;
		internal static void sk_document_close (sk_document_t document) =>
			(sk_document_close_delegate ??= GetSymbol<Delegates.sk_document_close> ("sk_document_close")).Invoke (document);
		#endif

		// sk_document_t* sk_document_create_pdf_from_stream(sk_wstream_t* stream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_document_t sk_document_create_pdf_from_stream (sk_wstream_t stream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_document_t sk_document_create_pdf_from_stream (sk_wstream_t stream);
		}
		private static Delegates.sk_document_create_pdf_from_stream sk_document_create_pdf_from_stream_delegate;
		internal static sk_document_t sk_document_create_pdf_from_stream (sk_wstream_t stream) =>
			(sk_document_create_pdf_from_stream_delegate ??= GetSymbol<Delegates.sk_document_create_pdf_from_stream> ("sk_document_create_pdf_from_stream")).Invoke (stream);
		#endif

		// sk_document_t* sk_document_create_pdf_from_stream_with_metadata(sk_wstream_t* stream, const sk_document_pdf_metadata_t* metadata)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_document_t sk_document_create_pdf_from_stream_with_metadata (sk_wstream_t stream, SKDocumentPdfMetadataInternal* metadata);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_document_t sk_document_create_pdf_from_stream_with_metadata (sk_wstream_t stream, SKDocumentPdfMetadataInternal* metadata);
		}
		private static Delegates.sk_document_create_pdf_from_stream_with_metadata sk_document_create_pdf_from_stream_with_metadata_delegate;
		internal static sk_document_t sk_document_create_pdf_from_stream_with_metadata (sk_wstream_t stream, SKDocumentPdfMetadataInternal* metadata) =>
			(sk_document_create_pdf_from_stream_with_metadata_delegate ??= GetSymbol<Delegates.sk_document_create_pdf_from_stream_with_metadata> ("sk_document_create_pdf_from_stream_with_metadata")).Invoke (stream, metadata);
		#endif

		// sk_document_t* sk_document_create_xps_from_stream(sk_wstream_t* stream, float dpi)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_document_t sk_document_create_xps_from_stream (sk_wstream_t stream, Single dpi);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_document_t sk_document_create_xps_from_stream (sk_wstream_t stream, Single dpi);
		}
		private static Delegates.sk_document_create_xps_from_stream sk_document_create_xps_from_stream_delegate;
		internal static sk_document_t sk_document_create_xps_from_stream (sk_wstream_t stream, Single dpi) =>
			(sk_document_create_xps_from_stream_delegate ??= GetSymbol<Delegates.sk_document_create_xps_from_stream> ("sk_document_create_xps_from_stream")).Invoke (stream, dpi);
		#endif

		// void sk_document_end_page(sk_document_t* document)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_document_end_page (sk_document_t document);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_document_end_page (sk_document_t document);
		}
		private static Delegates.sk_document_end_page sk_document_end_page_delegate;
		internal static void sk_document_end_page (sk_document_t document) =>
			(sk_document_end_page_delegate ??= GetSymbol<Delegates.sk_document_end_page> ("sk_document_end_page")).Invoke (document);
		#endif

		// void sk_document_unref(sk_document_t* document)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_document_unref (sk_document_t document);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_document_unref (sk_document_t document);
		}
		private static Delegates.sk_document_unref sk_document_unref_delegate;
		internal static void sk_document_unref (sk_document_t document) =>
			(sk_document_unref_delegate ??= GetSymbol<Delegates.sk_document_unref> ("sk_document_unref")).Invoke (document);
		#endif

		#endregion

		#region sk_drawable.h

		// void sk_drawable_draw(sk_drawable_t*, sk_canvas_t*, const sk_matrix_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_drawable_draw (sk_drawable_t param0, sk_canvas_t param1, SKMatrix* param2);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_drawable_draw (sk_drawable_t param0, sk_canvas_t param1, SKMatrix* param2);
		}
		private static Delegates.sk_drawable_draw sk_drawable_draw_delegate;
		internal static void sk_drawable_draw (sk_drawable_t param0, sk_canvas_t param1, SKMatrix* param2) =>
			(sk_drawable_draw_delegate ??= GetSymbol<Delegates.sk_drawable_draw> ("sk_drawable_draw")).Invoke (param0, param1, param2);
		#endif

		// void sk_drawable_get_bounds(sk_drawable_t*, sk_rect_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_drawable_get_bounds (sk_drawable_t param0, SKRect* param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_drawable_get_bounds (sk_drawable_t param0, SKRect* param1);
		}
		private static Delegates.sk_drawable_get_bounds sk_drawable_get_bounds_delegate;
		internal static void sk_drawable_get_bounds (sk_drawable_t param0, SKRect* param1) =>
			(sk_drawable_get_bounds_delegate ??= GetSymbol<Delegates.sk_drawable_get_bounds> ("sk_drawable_get_bounds")).Invoke (param0, param1);
		#endif

		// uint32_t sk_drawable_get_generation_id(sk_drawable_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_drawable_get_generation_id (sk_drawable_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_drawable_get_generation_id (sk_drawable_t param0);
		}
		private static Delegates.sk_drawable_get_generation_id sk_drawable_get_generation_id_delegate;
		internal static UInt32 sk_drawable_get_generation_id (sk_drawable_t param0) =>
			(sk_drawable_get_generation_id_delegate ??= GetSymbol<Delegates.sk_drawable_get_generation_id> ("sk_drawable_get_generation_id")).Invoke (param0);
		#endif

		// sk_picture_t* sk_drawable_new_picture_snapshot(sk_drawable_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_picture_t sk_drawable_new_picture_snapshot (sk_drawable_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_picture_t sk_drawable_new_picture_snapshot (sk_drawable_t param0);
		}
		private static Delegates.sk_drawable_new_picture_snapshot sk_drawable_new_picture_snapshot_delegate;
		internal static sk_picture_t sk_drawable_new_picture_snapshot (sk_drawable_t param0) =>
			(sk_drawable_new_picture_snapshot_delegate ??= GetSymbol<Delegates.sk_drawable_new_picture_snapshot> ("sk_drawable_new_picture_snapshot")).Invoke (param0);
		#endif

		// void sk_drawable_notify_drawing_changed(sk_drawable_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_drawable_notify_drawing_changed (sk_drawable_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_drawable_notify_drawing_changed (sk_drawable_t param0);
		}
		private static Delegates.sk_drawable_notify_drawing_changed sk_drawable_notify_drawing_changed_delegate;
		internal static void sk_drawable_notify_drawing_changed (sk_drawable_t param0) =>
			(sk_drawable_notify_drawing_changed_delegate ??= GetSymbol<Delegates.sk_drawable_notify_drawing_changed> ("sk_drawable_notify_drawing_changed")).Invoke (param0);
		#endif

		// void sk_drawable_unref(sk_drawable_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_drawable_unref (sk_drawable_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_drawable_unref (sk_drawable_t param0);
		}
		private static Delegates.sk_drawable_unref sk_drawable_unref_delegate;
		internal static void sk_drawable_unref (sk_drawable_t param0) =>
			(sk_drawable_unref_delegate ??= GetSymbol<Delegates.sk_drawable_unref> ("sk_drawable_unref")).Invoke (param0);
		#endif

		#endregion

		#region sk_font.h

		// size_t sk_font_break_text(const sk_font_t* font, const void* text, size_t byteLength, sk_text_encoding_t encoding, float maxWidth, float* measuredWidth, const sk_paint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_font_break_text (sk_font_t font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, Single maxWidth, Single* measuredWidth, sk_paint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_font_break_text (sk_font_t font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, Single maxWidth, Single* measuredWidth, sk_paint_t paint);
		}
		private static Delegates.sk_font_break_text sk_font_break_text_delegate;
		internal static /* size_t */ IntPtr sk_font_break_text (sk_font_t font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, Single maxWidth, Single* measuredWidth, sk_paint_t paint) =>
			(sk_font_break_text_delegate ??= GetSymbol<Delegates.sk_font_break_text> ("sk_font_break_text")).Invoke (font, text, byteLength, encoding, maxWidth, measuredWidth, paint);
		#endif

		// void sk_font_delete(sk_font_t* font)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_delete (sk_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_delete (sk_font_t font);
		}
		private static Delegates.sk_font_delete sk_font_delete_delegate;
		internal static void sk_font_delete (sk_font_t font) =>
			(sk_font_delete_delegate ??= GetSymbol<Delegates.sk_font_delete> ("sk_font_delete")).Invoke (font);
		#endif

		// sk_font_edging_t sk_font_get_edging(const sk_font_t* font)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKFontEdging sk_font_get_edging (sk_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKFontEdging sk_font_get_edging (sk_font_t font);
		}
		private static Delegates.sk_font_get_edging sk_font_get_edging_delegate;
		internal static SKFontEdging sk_font_get_edging (sk_font_t font) =>
			(sk_font_get_edging_delegate ??= GetSymbol<Delegates.sk_font_get_edging> ("sk_font_get_edging")).Invoke (font);
		#endif

		// sk_font_hinting_t sk_font_get_hinting(const sk_font_t* font)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKFontHinting sk_font_get_hinting (sk_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKFontHinting sk_font_get_hinting (sk_font_t font);
		}
		private static Delegates.sk_font_get_hinting sk_font_get_hinting_delegate;
		internal static SKFontHinting sk_font_get_hinting (sk_font_t font) =>
			(sk_font_get_hinting_delegate ??= GetSymbol<Delegates.sk_font_get_hinting> ("sk_font_get_hinting")).Invoke (font);
		#endif

		// float sk_font_get_metrics(const sk_font_t* font, sk_fontmetrics_t* metrics)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_font_get_metrics (sk_font_t font, SKFontMetrics* metrics);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_font_get_metrics (sk_font_t font, SKFontMetrics* metrics);
		}
		private static Delegates.sk_font_get_metrics sk_font_get_metrics_delegate;
		internal static Single sk_font_get_metrics (sk_font_t font, SKFontMetrics* metrics) =>
			(sk_font_get_metrics_delegate ??= GetSymbol<Delegates.sk_font_get_metrics> ("sk_font_get_metrics")).Invoke (font, metrics);
		#endif

		// bool sk_font_get_path(const sk_font_t* font, uint16_t glyph, sk_path_t* path)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_font_get_path (sk_font_t font, UInt16 glyph, sk_path_t path);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_font_get_path (sk_font_t font, UInt16 glyph, sk_path_t path);
		}
		private static Delegates.sk_font_get_path sk_font_get_path_delegate;
		internal static bool sk_font_get_path (sk_font_t font, UInt16 glyph, sk_path_t path) =>
			(sk_font_get_path_delegate ??= GetSymbol<Delegates.sk_font_get_path> ("sk_font_get_path")).Invoke (font, glyph, path);
		#endif

		// void sk_font_get_paths(const sk_font_t* font, uint16_t[-1] glyphs, int count, const sk_glyph_path_proc glyphPathProc, void* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_get_paths (sk_font_t font, UInt16* glyphs, Int32 count, SKGlyphPathProxyDelegate glyphPathProc, void* context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_get_paths (sk_font_t font, UInt16* glyphs, Int32 count, SKGlyphPathProxyDelegate glyphPathProc, void* context);
		}
		private static Delegates.sk_font_get_paths sk_font_get_paths_delegate;
		internal static void sk_font_get_paths (sk_font_t font, UInt16* glyphs, Int32 count, SKGlyphPathProxyDelegate glyphPathProc, void* context) =>
			(sk_font_get_paths_delegate ??= GetSymbol<Delegates.sk_font_get_paths> ("sk_font_get_paths")).Invoke (font, glyphs, count, glyphPathProc, context);
		#endif

		// void sk_font_get_pos(const sk_font_t* font, const uint16_t[-1] glyphs, int count, sk_point_t[-1] pos, sk_point_t* origin)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_get_pos (sk_font_t font, UInt16* glyphs, Int32 count, SKPoint* pos, SKPoint* origin);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_get_pos (sk_font_t font, UInt16* glyphs, Int32 count, SKPoint* pos, SKPoint* origin);
		}
		private static Delegates.sk_font_get_pos sk_font_get_pos_delegate;
		internal static void sk_font_get_pos (sk_font_t font, UInt16* glyphs, Int32 count, SKPoint* pos, SKPoint* origin) =>
			(sk_font_get_pos_delegate ??= GetSymbol<Delegates.sk_font_get_pos> ("sk_font_get_pos")).Invoke (font, glyphs, count, pos, origin);
		#endif

		// float sk_font_get_scale_x(const sk_font_t* font)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_font_get_scale_x (sk_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_font_get_scale_x (sk_font_t font);
		}
		private static Delegates.sk_font_get_scale_x sk_font_get_scale_x_delegate;
		internal static Single sk_font_get_scale_x (sk_font_t font) =>
			(sk_font_get_scale_x_delegate ??= GetSymbol<Delegates.sk_font_get_scale_x> ("sk_font_get_scale_x")).Invoke (font);
		#endif

		// float sk_font_get_size(const sk_font_t* font)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_font_get_size (sk_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_font_get_size (sk_font_t font);
		}
		private static Delegates.sk_font_get_size sk_font_get_size_delegate;
		internal static Single sk_font_get_size (sk_font_t font) =>
			(sk_font_get_size_delegate ??= GetSymbol<Delegates.sk_font_get_size> ("sk_font_get_size")).Invoke (font);
		#endif

		// float sk_font_get_skew_x(const sk_font_t* font)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_font_get_skew_x (sk_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_font_get_skew_x (sk_font_t font);
		}
		private static Delegates.sk_font_get_skew_x sk_font_get_skew_x_delegate;
		internal static Single sk_font_get_skew_x (sk_font_t font) =>
			(sk_font_get_skew_x_delegate ??= GetSymbol<Delegates.sk_font_get_skew_x> ("sk_font_get_skew_x")).Invoke (font);
		#endif

		// sk_typeface_t* sk_font_get_typeface(const sk_font_t* font)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_font_get_typeface (sk_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_font_get_typeface (sk_font_t font);
		}
		private static Delegates.sk_font_get_typeface sk_font_get_typeface_delegate;
		internal static sk_typeface_t sk_font_get_typeface (sk_font_t font) =>
			(sk_font_get_typeface_delegate ??= GetSymbol<Delegates.sk_font_get_typeface> ("sk_font_get_typeface")).Invoke (font);
		#endif

		// void sk_font_get_widths_bounds(const sk_font_t* font, const uint16_t[-1] glyphs, int count, float[-1] widths, sk_rect_t[-1] bounds, const sk_paint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_get_widths_bounds (sk_font_t font, UInt16* glyphs, Int32 count, Single* widths, SKRect* bounds, sk_paint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_get_widths_bounds (sk_font_t font, UInt16* glyphs, Int32 count, Single* widths, SKRect* bounds, sk_paint_t paint);
		}
		private static Delegates.sk_font_get_widths_bounds sk_font_get_widths_bounds_delegate;
		internal static void sk_font_get_widths_bounds (sk_font_t font, UInt16* glyphs, Int32 count, Single* widths, SKRect* bounds, sk_paint_t paint) =>
			(sk_font_get_widths_bounds_delegate ??= GetSymbol<Delegates.sk_font_get_widths_bounds> ("sk_font_get_widths_bounds")).Invoke (font, glyphs, count, widths, bounds, paint);
		#endif

		// void sk_font_get_xpos(const sk_font_t* font, const uint16_t[-1] glyphs, int count, float[-1] xpos, float origin)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_get_xpos (sk_font_t font, UInt16* glyphs, Int32 count, Single* xpos, Single origin);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_get_xpos (sk_font_t font, UInt16* glyphs, Int32 count, Single* xpos, Single origin);
		}
		private static Delegates.sk_font_get_xpos sk_font_get_xpos_delegate;
		internal static void sk_font_get_xpos (sk_font_t font, UInt16* glyphs, Int32 count, Single* xpos, Single origin) =>
			(sk_font_get_xpos_delegate ??= GetSymbol<Delegates.sk_font_get_xpos> ("sk_font_get_xpos")).Invoke (font, glyphs, count, xpos, origin);
		#endif

		// bool sk_font_is_baseline_snap(const sk_font_t* font)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_font_is_baseline_snap (sk_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_font_is_baseline_snap (sk_font_t font);
		}
		private static Delegates.sk_font_is_baseline_snap sk_font_is_baseline_snap_delegate;
		internal static bool sk_font_is_baseline_snap (sk_font_t font) =>
			(sk_font_is_baseline_snap_delegate ??= GetSymbol<Delegates.sk_font_is_baseline_snap> ("sk_font_is_baseline_snap")).Invoke (font);
		#endif

		// bool sk_font_is_embedded_bitmaps(const sk_font_t* font)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_font_is_embedded_bitmaps (sk_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_font_is_embedded_bitmaps (sk_font_t font);
		}
		private static Delegates.sk_font_is_embedded_bitmaps sk_font_is_embedded_bitmaps_delegate;
		internal static bool sk_font_is_embedded_bitmaps (sk_font_t font) =>
			(sk_font_is_embedded_bitmaps_delegate ??= GetSymbol<Delegates.sk_font_is_embedded_bitmaps> ("sk_font_is_embedded_bitmaps")).Invoke (font);
		#endif

		// bool sk_font_is_embolden(const sk_font_t* font)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_font_is_embolden (sk_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_font_is_embolden (sk_font_t font);
		}
		private static Delegates.sk_font_is_embolden sk_font_is_embolden_delegate;
		internal static bool sk_font_is_embolden (sk_font_t font) =>
			(sk_font_is_embolden_delegate ??= GetSymbol<Delegates.sk_font_is_embolden> ("sk_font_is_embolden")).Invoke (font);
		#endif

		// bool sk_font_is_force_auto_hinting(const sk_font_t* font)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_font_is_force_auto_hinting (sk_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_font_is_force_auto_hinting (sk_font_t font);
		}
		private static Delegates.sk_font_is_force_auto_hinting sk_font_is_force_auto_hinting_delegate;
		internal static bool sk_font_is_force_auto_hinting (sk_font_t font) =>
			(sk_font_is_force_auto_hinting_delegate ??= GetSymbol<Delegates.sk_font_is_force_auto_hinting> ("sk_font_is_force_auto_hinting")).Invoke (font);
		#endif

		// bool sk_font_is_linear_metrics(const sk_font_t* font)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_font_is_linear_metrics (sk_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_font_is_linear_metrics (sk_font_t font);
		}
		private static Delegates.sk_font_is_linear_metrics sk_font_is_linear_metrics_delegate;
		internal static bool sk_font_is_linear_metrics (sk_font_t font) =>
			(sk_font_is_linear_metrics_delegate ??= GetSymbol<Delegates.sk_font_is_linear_metrics> ("sk_font_is_linear_metrics")).Invoke (font);
		#endif

		// bool sk_font_is_subpixel(const sk_font_t* font)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_font_is_subpixel (sk_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_font_is_subpixel (sk_font_t font);
		}
		private static Delegates.sk_font_is_subpixel sk_font_is_subpixel_delegate;
		internal static bool sk_font_is_subpixel (sk_font_t font) =>
			(sk_font_is_subpixel_delegate ??= GetSymbol<Delegates.sk_font_is_subpixel> ("sk_font_is_subpixel")).Invoke (font);
		#endif

		// float sk_font_measure_text(const sk_font_t* font, const void* text, size_t byteLength, sk_text_encoding_t encoding, sk_rect_t* bounds, const sk_paint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_font_measure_text (sk_font_t font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, sk_paint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_font_measure_text (sk_font_t font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, sk_paint_t paint);
		}
		private static Delegates.sk_font_measure_text sk_font_measure_text_delegate;
		internal static Single sk_font_measure_text (sk_font_t font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, sk_paint_t paint) =>
			(sk_font_measure_text_delegate ??= GetSymbol<Delegates.sk_font_measure_text> ("sk_font_measure_text")).Invoke (font, text, byteLength, encoding, bounds, paint);
		#endif

		// void sk_font_measure_text_no_return(const sk_font_t* font, const void* text, size_t byteLength, sk_text_encoding_t encoding, sk_rect_t* bounds, const sk_paint_t* paint, float* measuredWidth)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_measure_text_no_return (sk_font_t font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, sk_paint_t paint, Single* measuredWidth);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_measure_text_no_return (sk_font_t font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, sk_paint_t paint, Single* measuredWidth);
		}
		private static Delegates.sk_font_measure_text_no_return sk_font_measure_text_no_return_delegate;
		internal static void sk_font_measure_text_no_return (sk_font_t font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, sk_paint_t paint, Single* measuredWidth) =>
			(sk_font_measure_text_no_return_delegate ??= GetSymbol<Delegates.sk_font_measure_text_no_return> ("sk_font_measure_text_no_return")).Invoke (font, text, byteLength, encoding, bounds, paint, measuredWidth);
		#endif

		// sk_font_t* sk_font_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_font_t sk_font_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_font_t sk_font_new ();
		}
		private static Delegates.sk_font_new sk_font_new_delegate;
		internal static sk_font_t sk_font_new () =>
			(sk_font_new_delegate ??= GetSymbol<Delegates.sk_font_new> ("sk_font_new")).Invoke ();
		#endif

		// sk_font_t* sk_font_new_with_values(sk_typeface_t* typeface, float size, float scaleX, float skewX)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_font_t sk_font_new_with_values (sk_typeface_t typeface, Single size, Single scaleX, Single skewX);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_font_t sk_font_new_with_values (sk_typeface_t typeface, Single size, Single scaleX, Single skewX);
		}
		private static Delegates.sk_font_new_with_values sk_font_new_with_values_delegate;
		internal static sk_font_t sk_font_new_with_values (sk_typeface_t typeface, Single size, Single scaleX, Single skewX) =>
			(sk_font_new_with_values_delegate ??= GetSymbol<Delegates.sk_font_new_with_values> ("sk_font_new_with_values")).Invoke (typeface, size, scaleX, skewX);
		#endif

		// void sk_font_set_baseline_snap(sk_font_t* font, bool value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_baseline_snap (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_baseline_snap (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value);
		}
		private static Delegates.sk_font_set_baseline_snap sk_font_set_baseline_snap_delegate;
		internal static void sk_font_set_baseline_snap (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value) =>
			(sk_font_set_baseline_snap_delegate ??= GetSymbol<Delegates.sk_font_set_baseline_snap> ("sk_font_set_baseline_snap")).Invoke (font, value);
		#endif

		// void sk_font_set_edging(sk_font_t* font, sk_font_edging_t value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_edging (sk_font_t font, SKFontEdging value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_edging (sk_font_t font, SKFontEdging value);
		}
		private static Delegates.sk_font_set_edging sk_font_set_edging_delegate;
		internal static void sk_font_set_edging (sk_font_t font, SKFontEdging value) =>
			(sk_font_set_edging_delegate ??= GetSymbol<Delegates.sk_font_set_edging> ("sk_font_set_edging")).Invoke (font, value);
		#endif

		// void sk_font_set_embedded_bitmaps(sk_font_t* font, bool value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_embedded_bitmaps (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_embedded_bitmaps (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value);
		}
		private static Delegates.sk_font_set_embedded_bitmaps sk_font_set_embedded_bitmaps_delegate;
		internal static void sk_font_set_embedded_bitmaps (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value) =>
			(sk_font_set_embedded_bitmaps_delegate ??= GetSymbol<Delegates.sk_font_set_embedded_bitmaps> ("sk_font_set_embedded_bitmaps")).Invoke (font, value);
		#endif

		// void sk_font_set_embolden(sk_font_t* font, bool value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_embolden (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_embolden (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value);
		}
		private static Delegates.sk_font_set_embolden sk_font_set_embolden_delegate;
		internal static void sk_font_set_embolden (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value) =>
			(sk_font_set_embolden_delegate ??= GetSymbol<Delegates.sk_font_set_embolden> ("sk_font_set_embolden")).Invoke (font, value);
		#endif

		// void sk_font_set_force_auto_hinting(sk_font_t* font, bool value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_force_auto_hinting (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_force_auto_hinting (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value);
		}
		private static Delegates.sk_font_set_force_auto_hinting sk_font_set_force_auto_hinting_delegate;
		internal static void sk_font_set_force_auto_hinting (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value) =>
			(sk_font_set_force_auto_hinting_delegate ??= GetSymbol<Delegates.sk_font_set_force_auto_hinting> ("sk_font_set_force_auto_hinting")).Invoke (font, value);
		#endif

		// void sk_font_set_hinting(sk_font_t* font, sk_font_hinting_t value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_hinting (sk_font_t font, SKFontHinting value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_hinting (sk_font_t font, SKFontHinting value);
		}
		private static Delegates.sk_font_set_hinting sk_font_set_hinting_delegate;
		internal static void sk_font_set_hinting (sk_font_t font, SKFontHinting value) =>
			(sk_font_set_hinting_delegate ??= GetSymbol<Delegates.sk_font_set_hinting> ("sk_font_set_hinting")).Invoke (font, value);
		#endif

		// void sk_font_set_linear_metrics(sk_font_t* font, bool value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_linear_metrics (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_linear_metrics (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value);
		}
		private static Delegates.sk_font_set_linear_metrics sk_font_set_linear_metrics_delegate;
		internal static void sk_font_set_linear_metrics (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value) =>
			(sk_font_set_linear_metrics_delegate ??= GetSymbol<Delegates.sk_font_set_linear_metrics> ("sk_font_set_linear_metrics")).Invoke (font, value);
		#endif

		// void sk_font_set_scale_x(sk_font_t* font, float value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_scale_x (sk_font_t font, Single value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_scale_x (sk_font_t font, Single value);
		}
		private static Delegates.sk_font_set_scale_x sk_font_set_scale_x_delegate;
		internal static void sk_font_set_scale_x (sk_font_t font, Single value) =>
			(sk_font_set_scale_x_delegate ??= GetSymbol<Delegates.sk_font_set_scale_x> ("sk_font_set_scale_x")).Invoke (font, value);
		#endif

		// void sk_font_set_size(sk_font_t* font, float value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_size (sk_font_t font, Single value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_size (sk_font_t font, Single value);
		}
		private static Delegates.sk_font_set_size sk_font_set_size_delegate;
		internal static void sk_font_set_size (sk_font_t font, Single value) =>
			(sk_font_set_size_delegate ??= GetSymbol<Delegates.sk_font_set_size> ("sk_font_set_size")).Invoke (font, value);
		#endif

		// void sk_font_set_skew_x(sk_font_t* font, float value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_skew_x (sk_font_t font, Single value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_skew_x (sk_font_t font, Single value);
		}
		private static Delegates.sk_font_set_skew_x sk_font_set_skew_x_delegate;
		internal static void sk_font_set_skew_x (sk_font_t font, Single value) =>
			(sk_font_set_skew_x_delegate ??= GetSymbol<Delegates.sk_font_set_skew_x> ("sk_font_set_skew_x")).Invoke (font, value);
		#endif

		// void sk_font_set_subpixel(sk_font_t* font, bool value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_subpixel (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_subpixel (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value);
		}
		private static Delegates.sk_font_set_subpixel sk_font_set_subpixel_delegate;
		internal static void sk_font_set_subpixel (sk_font_t font, [MarshalAs (UnmanagedType.I1)] bool value) =>
			(sk_font_set_subpixel_delegate ??= GetSymbol<Delegates.sk_font_set_subpixel> ("sk_font_set_subpixel")).Invoke (font, value);
		#endif

		// void sk_font_set_typeface(sk_font_t* font, sk_typeface_t* value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_typeface (sk_font_t font, sk_typeface_t value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_typeface (sk_font_t font, sk_typeface_t value);
		}
		private static Delegates.sk_font_set_typeface sk_font_set_typeface_delegate;
		internal static void sk_font_set_typeface (sk_font_t font, sk_typeface_t value) =>
			(sk_font_set_typeface_delegate ??= GetSymbol<Delegates.sk_font_set_typeface> ("sk_font_set_typeface")).Invoke (font, value);
		#endif

		// int sk_font_text_to_glyphs(const sk_font_t* font, const void* text, size_t byteLength, sk_text_encoding_t encoding, uint16_t[-1] glyphs, int maxGlyphCount)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_font_text_to_glyphs (sk_font_t font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, UInt16* glyphs, Int32 maxGlyphCount);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_font_text_to_glyphs (sk_font_t font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, UInt16* glyphs, Int32 maxGlyphCount);
		}
		private static Delegates.sk_font_text_to_glyphs sk_font_text_to_glyphs_delegate;
		internal static Int32 sk_font_text_to_glyphs (sk_font_t font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, UInt16* glyphs, Int32 maxGlyphCount) =>
			(sk_font_text_to_glyphs_delegate ??= GetSymbol<Delegates.sk_font_text_to_glyphs> ("sk_font_text_to_glyphs")).Invoke (font, text, byteLength, encoding, glyphs, maxGlyphCount);
		#endif

		// uint16_t sk_font_unichar_to_glyph(const sk_font_t* font, int32_t uni)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt16 sk_font_unichar_to_glyph (sk_font_t font, Int32 uni);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt16 sk_font_unichar_to_glyph (sk_font_t font, Int32 uni);
		}
		private static Delegates.sk_font_unichar_to_glyph sk_font_unichar_to_glyph_delegate;
		internal static UInt16 sk_font_unichar_to_glyph (sk_font_t font, Int32 uni) =>
			(sk_font_unichar_to_glyph_delegate ??= GetSymbol<Delegates.sk_font_unichar_to_glyph> ("sk_font_unichar_to_glyph")).Invoke (font, uni);
		#endif

		// void sk_font_unichars_to_glyphs(const sk_font_t* font, const int32_t[-1] uni, int count, uint16_t[-1] glyphs)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_unichars_to_glyphs (sk_font_t font, Int32* uni, Int32 count, UInt16* glyphs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_unichars_to_glyphs (sk_font_t font, Int32* uni, Int32 count, UInt16* glyphs);
		}
		private static Delegates.sk_font_unichars_to_glyphs sk_font_unichars_to_glyphs_delegate;
		internal static void sk_font_unichars_to_glyphs (sk_font_t font, Int32* uni, Int32 count, UInt16* glyphs) =>
			(sk_font_unichars_to_glyphs_delegate ??= GetSymbol<Delegates.sk_font_unichars_to_glyphs> ("sk_font_unichars_to_glyphs")).Invoke (font, uni, count, glyphs);
		#endif

		// void sk_text_utils_get_path(const void* text, size_t length, sk_text_encoding_t encoding, float x, float y, const sk_font_t* font, sk_path_t* path)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_text_utils_get_path (void* text, /* size_t */ IntPtr length, SKTextEncoding encoding, Single x, Single y, sk_font_t font, sk_path_t path);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_text_utils_get_path (void* text, /* size_t */ IntPtr length, SKTextEncoding encoding, Single x, Single y, sk_font_t font, sk_path_t path);
		}
		private static Delegates.sk_text_utils_get_path sk_text_utils_get_path_delegate;
		internal static void sk_text_utils_get_path (void* text, /* size_t */ IntPtr length, SKTextEncoding encoding, Single x, Single y, sk_font_t font, sk_path_t path) =>
			(sk_text_utils_get_path_delegate ??= GetSymbol<Delegates.sk_text_utils_get_path> ("sk_text_utils_get_path")).Invoke (text, length, encoding, x, y, font, path);
		#endif

		// void sk_text_utils_get_pos_path(const void* text, size_t length, sk_text_encoding_t encoding, const sk_point_t[-1] pos, const sk_font_t* font, sk_path_t* path)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_text_utils_get_pos_path (void* text, /* size_t */ IntPtr length, SKTextEncoding encoding, SKPoint* pos, sk_font_t font, sk_path_t path);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_text_utils_get_pos_path (void* text, /* size_t */ IntPtr length, SKTextEncoding encoding, SKPoint* pos, sk_font_t font, sk_path_t path);
		}
		private static Delegates.sk_text_utils_get_pos_path sk_text_utils_get_pos_path_delegate;
		internal static void sk_text_utils_get_pos_path (void* text, /* size_t */ IntPtr length, SKTextEncoding encoding, SKPoint* pos, sk_font_t font, sk_path_t path) =>
			(sk_text_utils_get_pos_path_delegate ??= GetSymbol<Delegates.sk_text_utils_get_pos_path> ("sk_text_utils_get_pos_path")).Invoke (text, length, encoding, pos, font, path);
		#endif

		#endregion

		#region sk_general.h

		// sk_colortype_t sk_colortype_get_default_8888()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKColorTypeNative sk_colortype_get_default_8888 ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKColorTypeNative sk_colortype_get_default_8888 ();
		}
		private static Delegates.sk_colortype_get_default_8888 sk_colortype_get_default_8888_delegate;
		internal static SKColorTypeNative sk_colortype_get_default_8888 () =>
			(sk_colortype_get_default_8888_delegate ??= GetSymbol<Delegates.sk_colortype_get_default_8888> ("sk_colortype_get_default_8888")).Invoke ();
		#endif

		// int sk_nvrefcnt_get_ref_count(const sk_nvrefcnt_t* refcnt)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_nvrefcnt_get_ref_count (sk_nvrefcnt_t refcnt);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_nvrefcnt_get_ref_count (sk_nvrefcnt_t refcnt);
		}
		private static Delegates.sk_nvrefcnt_get_ref_count sk_nvrefcnt_get_ref_count_delegate;
		internal static Int32 sk_nvrefcnt_get_ref_count (sk_nvrefcnt_t refcnt) =>
			(sk_nvrefcnt_get_ref_count_delegate ??= GetSymbol<Delegates.sk_nvrefcnt_get_ref_count> ("sk_nvrefcnt_get_ref_count")).Invoke (refcnt);
		#endif

		// void sk_nvrefcnt_safe_ref(sk_nvrefcnt_t* refcnt)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nvrefcnt_safe_ref (sk_nvrefcnt_t refcnt);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_nvrefcnt_safe_ref (sk_nvrefcnt_t refcnt);
		}
		private static Delegates.sk_nvrefcnt_safe_ref sk_nvrefcnt_safe_ref_delegate;
		internal static void sk_nvrefcnt_safe_ref (sk_nvrefcnt_t refcnt) =>
			(sk_nvrefcnt_safe_ref_delegate ??= GetSymbol<Delegates.sk_nvrefcnt_safe_ref> ("sk_nvrefcnt_safe_ref")).Invoke (refcnt);
		#endif

		// void sk_nvrefcnt_safe_unref(sk_nvrefcnt_t* refcnt)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nvrefcnt_safe_unref (sk_nvrefcnt_t refcnt);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_nvrefcnt_safe_unref (sk_nvrefcnt_t refcnt);
		}
		private static Delegates.sk_nvrefcnt_safe_unref sk_nvrefcnt_safe_unref_delegate;
		internal static void sk_nvrefcnt_safe_unref (sk_nvrefcnt_t refcnt) =>
			(sk_nvrefcnt_safe_unref_delegate ??= GetSymbol<Delegates.sk_nvrefcnt_safe_unref> ("sk_nvrefcnt_safe_unref")).Invoke (refcnt);
		#endif

		// bool sk_nvrefcnt_unique(const sk_nvrefcnt_t* refcnt)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_nvrefcnt_unique (sk_nvrefcnt_t refcnt);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_nvrefcnt_unique (sk_nvrefcnt_t refcnt);
		}
		private static Delegates.sk_nvrefcnt_unique sk_nvrefcnt_unique_delegate;
		internal static bool sk_nvrefcnt_unique (sk_nvrefcnt_t refcnt) =>
			(sk_nvrefcnt_unique_delegate ??= GetSymbol<Delegates.sk_nvrefcnt_unique> ("sk_nvrefcnt_unique")).Invoke (refcnt);
		#endif

		// int sk_refcnt_get_ref_count(const sk_refcnt_t* refcnt)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_refcnt_get_ref_count (sk_refcnt_t refcnt);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_refcnt_get_ref_count (sk_refcnt_t refcnt);
		}
		private static Delegates.sk_refcnt_get_ref_count sk_refcnt_get_ref_count_delegate;
		internal static Int32 sk_refcnt_get_ref_count (sk_refcnt_t refcnt) =>
			(sk_refcnt_get_ref_count_delegate ??= GetSymbol<Delegates.sk_refcnt_get_ref_count> ("sk_refcnt_get_ref_count")).Invoke (refcnt);
		#endif

		// void sk_refcnt_safe_ref(sk_refcnt_t* refcnt)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_refcnt_safe_ref (sk_refcnt_t refcnt);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_refcnt_safe_ref (sk_refcnt_t refcnt);
		}
		private static Delegates.sk_refcnt_safe_ref sk_refcnt_safe_ref_delegate;
		internal static void sk_refcnt_safe_ref (sk_refcnt_t refcnt) =>
			(sk_refcnt_safe_ref_delegate ??= GetSymbol<Delegates.sk_refcnt_safe_ref> ("sk_refcnt_safe_ref")).Invoke (refcnt);
		#endif

		// void sk_refcnt_safe_unref(sk_refcnt_t* refcnt)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_refcnt_safe_unref (sk_refcnt_t refcnt);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_refcnt_safe_unref (sk_refcnt_t refcnt);
		}
		private static Delegates.sk_refcnt_safe_unref sk_refcnt_safe_unref_delegate;
		internal static void sk_refcnt_safe_unref (sk_refcnt_t refcnt) =>
			(sk_refcnt_safe_unref_delegate ??= GetSymbol<Delegates.sk_refcnt_safe_unref> ("sk_refcnt_safe_unref")).Invoke (refcnt);
		#endif

		// bool sk_refcnt_unique(const sk_refcnt_t* refcnt)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_refcnt_unique (sk_refcnt_t refcnt);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_refcnt_unique (sk_refcnt_t refcnt);
		}
		private static Delegates.sk_refcnt_unique sk_refcnt_unique_delegate;
		internal static bool sk_refcnt_unique (sk_refcnt_t refcnt) =>
			(sk_refcnt_unique_delegate ??= GetSymbol<Delegates.sk_refcnt_unique> ("sk_refcnt_unique")).Invoke (refcnt);
		#endif

		// int sk_version_get_increment()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_version_get_increment ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_version_get_increment ();
		}
		private static Delegates.sk_version_get_increment sk_version_get_increment_delegate;
		internal static Int32 sk_version_get_increment () =>
			(sk_version_get_increment_delegate ??= GetSymbol<Delegates.sk_version_get_increment> ("sk_version_get_increment")).Invoke ();
		#endif

		// int sk_version_get_milestone()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_version_get_milestone ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_version_get_milestone ();
		}
		private static Delegates.sk_version_get_milestone sk_version_get_milestone_delegate;
		internal static Int32 sk_version_get_milestone () =>
			(sk_version_get_milestone_delegate ??= GetSymbol<Delegates.sk_version_get_milestone> ("sk_version_get_milestone")).Invoke ();
		#endif

		// const char* sk_version_get_string()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* sk_version_get_string ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void* sk_version_get_string ();
		}
		private static Delegates.sk_version_get_string sk_version_get_string_delegate;
		internal static /* char */ void* sk_version_get_string () =>
			(sk_version_get_string_delegate ??= GetSymbol<Delegates.sk_version_get_string> ("sk_version_get_string")).Invoke ();
		#endif

		#endregion

		#region sk_graphics.h

		// void sk_graphics_dump_memory_statistics(sk_tracememorydump_t* dump)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphics_dump_memory_statistics (sk_tracememorydump_t dump);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphics_dump_memory_statistics (sk_tracememorydump_t dump);
		}
		private static Delegates.sk_graphics_dump_memory_statistics sk_graphics_dump_memory_statistics_delegate;
		internal static void sk_graphics_dump_memory_statistics (sk_tracememorydump_t dump) =>
			(sk_graphics_dump_memory_statistics_delegate ??= GetSymbol<Delegates.sk_graphics_dump_memory_statistics> ("sk_graphics_dump_memory_statistics")).Invoke (dump);
		#endif

		// int sk_graphics_get_font_cache_count_limit()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_graphics_get_font_cache_count_limit ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_graphics_get_font_cache_count_limit ();
		}
		private static Delegates.sk_graphics_get_font_cache_count_limit sk_graphics_get_font_cache_count_limit_delegate;
		internal static Int32 sk_graphics_get_font_cache_count_limit () =>
			(sk_graphics_get_font_cache_count_limit_delegate ??= GetSymbol<Delegates.sk_graphics_get_font_cache_count_limit> ("sk_graphics_get_font_cache_count_limit")).Invoke ();
		#endif

		// int sk_graphics_get_font_cache_count_used()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_graphics_get_font_cache_count_used ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_graphics_get_font_cache_count_used ();
		}
		private static Delegates.sk_graphics_get_font_cache_count_used sk_graphics_get_font_cache_count_used_delegate;
		internal static Int32 sk_graphics_get_font_cache_count_used () =>
			(sk_graphics_get_font_cache_count_used_delegate ??= GetSymbol<Delegates.sk_graphics_get_font_cache_count_used> ("sk_graphics_get_font_cache_count_used")).Invoke ();
		#endif

		// size_t sk_graphics_get_font_cache_limit()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_get_font_cache_limit ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_get_font_cache_limit ();
		}
		private static Delegates.sk_graphics_get_font_cache_limit sk_graphics_get_font_cache_limit_delegate;
		internal static /* size_t */ IntPtr sk_graphics_get_font_cache_limit () =>
			(sk_graphics_get_font_cache_limit_delegate ??= GetSymbol<Delegates.sk_graphics_get_font_cache_limit> ("sk_graphics_get_font_cache_limit")).Invoke ();
		#endif

		// int sk_graphics_get_font_cache_point_size_limit()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_graphics_get_font_cache_point_size_limit ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_graphics_get_font_cache_point_size_limit ();
		}
		private static Delegates.sk_graphics_get_font_cache_point_size_limit sk_graphics_get_font_cache_point_size_limit_delegate;
		internal static Int32 sk_graphics_get_font_cache_point_size_limit () =>
			(sk_graphics_get_font_cache_point_size_limit_delegate ??= GetSymbol<Delegates.sk_graphics_get_font_cache_point_size_limit> ("sk_graphics_get_font_cache_point_size_limit")).Invoke ();
		#endif

		// size_t sk_graphics_get_font_cache_used()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_get_font_cache_used ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_get_font_cache_used ();
		}
		private static Delegates.sk_graphics_get_font_cache_used sk_graphics_get_font_cache_used_delegate;
		internal static /* size_t */ IntPtr sk_graphics_get_font_cache_used () =>
			(sk_graphics_get_font_cache_used_delegate ??= GetSymbol<Delegates.sk_graphics_get_font_cache_used> ("sk_graphics_get_font_cache_used")).Invoke ();
		#endif

		// size_t sk_graphics_get_resource_cache_single_allocation_byte_limit()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_get_resource_cache_single_allocation_byte_limit ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_get_resource_cache_single_allocation_byte_limit ();
		}
		private static Delegates.sk_graphics_get_resource_cache_single_allocation_byte_limit sk_graphics_get_resource_cache_single_allocation_byte_limit_delegate;
		internal static /* size_t */ IntPtr sk_graphics_get_resource_cache_single_allocation_byte_limit () =>
			(sk_graphics_get_resource_cache_single_allocation_byte_limit_delegate ??= GetSymbol<Delegates.sk_graphics_get_resource_cache_single_allocation_byte_limit> ("sk_graphics_get_resource_cache_single_allocation_byte_limit")).Invoke ();
		#endif

		// size_t sk_graphics_get_resource_cache_total_byte_limit()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_get_resource_cache_total_byte_limit ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_get_resource_cache_total_byte_limit ();
		}
		private static Delegates.sk_graphics_get_resource_cache_total_byte_limit sk_graphics_get_resource_cache_total_byte_limit_delegate;
		internal static /* size_t */ IntPtr sk_graphics_get_resource_cache_total_byte_limit () =>
			(sk_graphics_get_resource_cache_total_byte_limit_delegate ??= GetSymbol<Delegates.sk_graphics_get_resource_cache_total_byte_limit> ("sk_graphics_get_resource_cache_total_byte_limit")).Invoke ();
		#endif

		// size_t sk_graphics_get_resource_cache_total_bytes_used()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_get_resource_cache_total_bytes_used ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_get_resource_cache_total_bytes_used ();
		}
		private static Delegates.sk_graphics_get_resource_cache_total_bytes_used sk_graphics_get_resource_cache_total_bytes_used_delegate;
		internal static /* size_t */ IntPtr sk_graphics_get_resource_cache_total_bytes_used () =>
			(sk_graphics_get_resource_cache_total_bytes_used_delegate ??= GetSymbol<Delegates.sk_graphics_get_resource_cache_total_bytes_used> ("sk_graphics_get_resource_cache_total_bytes_used")).Invoke ();
		#endif

		// void sk_graphics_init()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphics_init ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphics_init ();
		}
		private static Delegates.sk_graphics_init sk_graphics_init_delegate;
		internal static void sk_graphics_init () =>
			(sk_graphics_init_delegate ??= GetSymbol<Delegates.sk_graphics_init> ("sk_graphics_init")).Invoke ();
		#endif

		// void sk_graphics_purge_all_caches()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphics_purge_all_caches ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphics_purge_all_caches ();
		}
		private static Delegates.sk_graphics_purge_all_caches sk_graphics_purge_all_caches_delegate;
		internal static void sk_graphics_purge_all_caches () =>
			(sk_graphics_purge_all_caches_delegate ??= GetSymbol<Delegates.sk_graphics_purge_all_caches> ("sk_graphics_purge_all_caches")).Invoke ();
		#endif

		// void sk_graphics_purge_font_cache()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphics_purge_font_cache ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphics_purge_font_cache ();
		}
		private static Delegates.sk_graphics_purge_font_cache sk_graphics_purge_font_cache_delegate;
		internal static void sk_graphics_purge_font_cache () =>
			(sk_graphics_purge_font_cache_delegate ??= GetSymbol<Delegates.sk_graphics_purge_font_cache> ("sk_graphics_purge_font_cache")).Invoke ();
		#endif

		// void sk_graphics_purge_resource_cache()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphics_purge_resource_cache ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphics_purge_resource_cache ();
		}
		private static Delegates.sk_graphics_purge_resource_cache sk_graphics_purge_resource_cache_delegate;
		internal static void sk_graphics_purge_resource_cache () =>
			(sk_graphics_purge_resource_cache_delegate ??= GetSymbol<Delegates.sk_graphics_purge_resource_cache> ("sk_graphics_purge_resource_cache")).Invoke ();
		#endif

		// int sk_graphics_set_font_cache_count_limit(int count)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_graphics_set_font_cache_count_limit (Int32 count);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_graphics_set_font_cache_count_limit (Int32 count);
		}
		private static Delegates.sk_graphics_set_font_cache_count_limit sk_graphics_set_font_cache_count_limit_delegate;
		internal static Int32 sk_graphics_set_font_cache_count_limit (Int32 count) =>
			(sk_graphics_set_font_cache_count_limit_delegate ??= GetSymbol<Delegates.sk_graphics_set_font_cache_count_limit> ("sk_graphics_set_font_cache_count_limit")).Invoke (count);
		#endif

		// size_t sk_graphics_set_font_cache_limit(size_t bytes)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_set_font_cache_limit (/* size_t */ IntPtr bytes);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_set_font_cache_limit (/* size_t */ IntPtr bytes);
		}
		private static Delegates.sk_graphics_set_font_cache_limit sk_graphics_set_font_cache_limit_delegate;
		internal static /* size_t */ IntPtr sk_graphics_set_font_cache_limit (/* size_t */ IntPtr bytes) =>
			(sk_graphics_set_font_cache_limit_delegate ??= GetSymbol<Delegates.sk_graphics_set_font_cache_limit> ("sk_graphics_set_font_cache_limit")).Invoke (bytes);
		#endif

		// int sk_graphics_set_font_cache_point_size_limit(int maxPointSize)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_graphics_set_font_cache_point_size_limit (Int32 maxPointSize);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_graphics_set_font_cache_point_size_limit (Int32 maxPointSize);
		}
		private static Delegates.sk_graphics_set_font_cache_point_size_limit sk_graphics_set_font_cache_point_size_limit_delegate;
		internal static Int32 sk_graphics_set_font_cache_point_size_limit (Int32 maxPointSize) =>
			(sk_graphics_set_font_cache_point_size_limit_delegate ??= GetSymbol<Delegates.sk_graphics_set_font_cache_point_size_limit> ("sk_graphics_set_font_cache_point_size_limit")).Invoke (maxPointSize);
		#endif

		// size_t sk_graphics_set_resource_cache_single_allocation_byte_limit(size_t newLimit)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_set_resource_cache_single_allocation_byte_limit (/* size_t */ IntPtr newLimit);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_set_resource_cache_single_allocation_byte_limit (/* size_t */ IntPtr newLimit);
		}
		private static Delegates.sk_graphics_set_resource_cache_single_allocation_byte_limit sk_graphics_set_resource_cache_single_allocation_byte_limit_delegate;
		internal static /* size_t */ IntPtr sk_graphics_set_resource_cache_single_allocation_byte_limit (/* size_t */ IntPtr newLimit) =>
			(sk_graphics_set_resource_cache_single_allocation_byte_limit_delegate ??= GetSymbol<Delegates.sk_graphics_set_resource_cache_single_allocation_byte_limit> ("sk_graphics_set_resource_cache_single_allocation_byte_limit")).Invoke (newLimit);
		#endif

		// size_t sk_graphics_set_resource_cache_total_byte_limit(size_t newLimit)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_set_resource_cache_total_byte_limit (/* size_t */ IntPtr newLimit);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_set_resource_cache_total_byte_limit (/* size_t */ IntPtr newLimit);
		}
		private static Delegates.sk_graphics_set_resource_cache_total_byte_limit sk_graphics_set_resource_cache_total_byte_limit_delegate;
		internal static /* size_t */ IntPtr sk_graphics_set_resource_cache_total_byte_limit (/* size_t */ IntPtr newLimit) =>
			(sk_graphics_set_resource_cache_total_byte_limit_delegate ??= GetSymbol<Delegates.sk_graphics_set_resource_cache_total_byte_limit> ("sk_graphics_set_resource_cache_total_byte_limit")).Invoke (newLimit);
		#endif

		#endregion

		#region sk_image.h

		// sk_data_t* sk_image_encode(const sk_image_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_image_encode (sk_image_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_image_encode (sk_image_t param0);
		}
		private static Delegates.sk_image_encode sk_image_encode_delegate;
		internal static sk_data_t sk_image_encode (sk_image_t param0) =>
			(sk_image_encode_delegate ??= GetSymbol<Delegates.sk_image_encode> ("sk_image_encode")).Invoke (param0);
		#endif

		// sk_data_t* sk_image_encode_specific(const sk_image_t* cimage, sk_encoded_image_format_t encoder, int quality)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_image_encode_specific (sk_image_t cimage, SKEncodedImageFormat encoder, Int32 quality);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_image_encode_specific (sk_image_t cimage, SKEncodedImageFormat encoder, Int32 quality);
		}
		private static Delegates.sk_image_encode_specific sk_image_encode_specific_delegate;
		internal static sk_data_t sk_image_encode_specific (sk_image_t cimage, SKEncodedImageFormat encoder, Int32 quality) =>
			(sk_image_encode_specific_delegate ??= GetSymbol<Delegates.sk_image_encode_specific> ("sk_image_encode_specific")).Invoke (cimage, encoder, quality);
		#endif

		// sk_alphatype_t sk_image_get_alpha_type(const sk_image_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKAlphaType sk_image_get_alpha_type (sk_image_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKAlphaType sk_image_get_alpha_type (sk_image_t param0);
		}
		private static Delegates.sk_image_get_alpha_type sk_image_get_alpha_type_delegate;
		internal static SKAlphaType sk_image_get_alpha_type (sk_image_t param0) =>
			(sk_image_get_alpha_type_delegate ??= GetSymbol<Delegates.sk_image_get_alpha_type> ("sk_image_get_alpha_type")).Invoke (param0);
		#endif

		// sk_colortype_t sk_image_get_color_type(const sk_image_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKColorTypeNative sk_image_get_color_type (sk_image_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKColorTypeNative sk_image_get_color_type (sk_image_t param0);
		}
		private static Delegates.sk_image_get_color_type sk_image_get_color_type_delegate;
		internal static SKColorTypeNative sk_image_get_color_type (sk_image_t param0) =>
			(sk_image_get_color_type_delegate ??= GetSymbol<Delegates.sk_image_get_color_type> ("sk_image_get_color_type")).Invoke (param0);
		#endif

		// sk_colorspace_t* sk_image_get_colorspace(const sk_image_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_image_get_colorspace (sk_image_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_image_get_colorspace (sk_image_t param0);
		}
		private static Delegates.sk_image_get_colorspace sk_image_get_colorspace_delegate;
		internal static sk_colorspace_t sk_image_get_colorspace (sk_image_t param0) =>
			(sk_image_get_colorspace_delegate ??= GetSymbol<Delegates.sk_image_get_colorspace> ("sk_image_get_colorspace")).Invoke (param0);
		#endif

		// int sk_image_get_height(const sk_image_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_image_get_height (sk_image_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_image_get_height (sk_image_t param0);
		}
		private static Delegates.sk_image_get_height sk_image_get_height_delegate;
		internal static Int32 sk_image_get_height (sk_image_t param0) =>
			(sk_image_get_height_delegate ??= GetSymbol<Delegates.sk_image_get_height> ("sk_image_get_height")).Invoke (param0);
		#endif

		// uint32_t sk_image_get_unique_id(const sk_image_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_image_get_unique_id (sk_image_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_image_get_unique_id (sk_image_t param0);
		}
		private static Delegates.sk_image_get_unique_id sk_image_get_unique_id_delegate;
		internal static UInt32 sk_image_get_unique_id (sk_image_t param0) =>
			(sk_image_get_unique_id_delegate ??= GetSymbol<Delegates.sk_image_get_unique_id> ("sk_image_get_unique_id")).Invoke (param0);
		#endif

		// int sk_image_get_width(const sk_image_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_image_get_width (sk_image_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_image_get_width (sk_image_t param0);
		}
		private static Delegates.sk_image_get_width sk_image_get_width_delegate;
		internal static Int32 sk_image_get_width (sk_image_t param0) =>
			(sk_image_get_width_delegate ??= GetSymbol<Delegates.sk_image_get_width> ("sk_image_get_width")).Invoke (param0);
		#endif

		// bool sk_image_is_alpha_only(const sk_image_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_is_alpha_only (sk_image_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_is_alpha_only (sk_image_t param0);
		}
		private static Delegates.sk_image_is_alpha_only sk_image_is_alpha_only_delegate;
		internal static bool sk_image_is_alpha_only (sk_image_t param0) =>
			(sk_image_is_alpha_only_delegate ??= GetSymbol<Delegates.sk_image_is_alpha_only> ("sk_image_is_alpha_only")).Invoke (param0);
		#endif

		// bool sk_image_is_lazy_generated(const sk_image_t* image)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_is_lazy_generated (sk_image_t image);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_is_lazy_generated (sk_image_t image);
		}
		private static Delegates.sk_image_is_lazy_generated sk_image_is_lazy_generated_delegate;
		internal static bool sk_image_is_lazy_generated (sk_image_t image) =>
			(sk_image_is_lazy_generated_delegate ??= GetSymbol<Delegates.sk_image_is_lazy_generated> ("sk_image_is_lazy_generated")).Invoke (image);
		#endif

		// bool sk_image_is_texture_backed(const sk_image_t* image)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_is_texture_backed (sk_image_t image);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_is_texture_backed (sk_image_t image);
		}
		private static Delegates.sk_image_is_texture_backed sk_image_is_texture_backed_delegate;
		internal static bool sk_image_is_texture_backed (sk_image_t image) =>
			(sk_image_is_texture_backed_delegate ??= GetSymbol<Delegates.sk_image_is_texture_backed> ("sk_image_is_texture_backed")).Invoke (image);
		#endif

		// bool sk_image_is_valid(const sk_image_t* image, gr_recording_context_t* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_is_valid (sk_image_t image, gr_recording_context_t context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_is_valid (sk_image_t image, gr_recording_context_t context);
		}
		private static Delegates.sk_image_is_valid sk_image_is_valid_delegate;
		internal static bool sk_image_is_valid (sk_image_t image, gr_recording_context_t context) =>
			(sk_image_is_valid_delegate ??= GetSymbol<Delegates.sk_image_is_valid> ("sk_image_is_valid")).Invoke (image, context);
		#endif

		// sk_image_t* sk_image_make_non_texture_image(const sk_image_t* cimage)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_make_non_texture_image (sk_image_t cimage);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_make_non_texture_image (sk_image_t cimage);
		}
		private static Delegates.sk_image_make_non_texture_image sk_image_make_non_texture_image_delegate;
		internal static sk_image_t sk_image_make_non_texture_image (sk_image_t cimage) =>
			(sk_image_make_non_texture_image_delegate ??= GetSymbol<Delegates.sk_image_make_non_texture_image> ("sk_image_make_non_texture_image")).Invoke (cimage);
		#endif

		// sk_image_t* sk_image_make_raster_image(const sk_image_t* cimage)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_make_raster_image (sk_image_t cimage);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_make_raster_image (sk_image_t cimage);
		}
		private static Delegates.sk_image_make_raster_image sk_image_make_raster_image_delegate;
		internal static sk_image_t sk_image_make_raster_image (sk_image_t cimage) =>
			(sk_image_make_raster_image_delegate ??= GetSymbol<Delegates.sk_image_make_raster_image> ("sk_image_make_raster_image")).Invoke (cimage);
		#endif

		// sk_shader_t* sk_image_make_shader(const sk_image_t*, sk_shader_tilemode_t tileX, sk_shader_tilemode_t tileY, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_image_make_shader (sk_image_t param0, SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix* localMatrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_image_make_shader (sk_image_t param0, SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix* localMatrix);
		}
		private static Delegates.sk_image_make_shader sk_image_make_shader_delegate;
		internal static sk_shader_t sk_image_make_shader (sk_image_t param0, SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix* localMatrix) =>
			(sk_image_make_shader_delegate ??= GetSymbol<Delegates.sk_image_make_shader> ("sk_image_make_shader")).Invoke (param0, tileX, tileY, localMatrix);
		#endif

		// sk_image_t* sk_image_make_subset(const sk_image_t* cimage, const sk_irect_t* subset)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_make_subset (sk_image_t cimage, SKRectI* subset);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_make_subset (sk_image_t cimage, SKRectI* subset);
		}
		private static Delegates.sk_image_make_subset sk_image_make_subset_delegate;
		internal static sk_image_t sk_image_make_subset (sk_image_t cimage, SKRectI* subset) =>
			(sk_image_make_subset_delegate ??= GetSymbol<Delegates.sk_image_make_subset> ("sk_image_make_subset")).Invoke (cimage, subset);
		#endif

		// sk_image_t* sk_image_make_texture_image(const sk_image_t* cimage, gr_direct_context_t* context, bool mipmapped)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_make_texture_image (sk_image_t cimage, gr_direct_context_t context, [MarshalAs (UnmanagedType.I1)] bool mipmapped);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_make_texture_image (sk_image_t cimage, gr_direct_context_t context, [MarshalAs (UnmanagedType.I1)] bool mipmapped);
		}
		private static Delegates.sk_image_make_texture_image sk_image_make_texture_image_delegate;
		internal static sk_image_t sk_image_make_texture_image (sk_image_t cimage, gr_direct_context_t context, [MarshalAs (UnmanagedType.I1)] bool mipmapped) =>
			(sk_image_make_texture_image_delegate ??= GetSymbol<Delegates.sk_image_make_texture_image> ("sk_image_make_texture_image")).Invoke (cimage, context, mipmapped);
		#endif

		// sk_image_t* sk_image_make_with_filter(const sk_image_t* cimage, gr_recording_context_t* context, const sk_imagefilter_t* filter, const sk_irect_t* subset, const sk_irect_t* clipBounds, sk_irect_t* outSubset, sk_ipoint_t* outOffset)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_make_with_filter (sk_image_t cimage, gr_recording_context_t context, sk_imagefilter_t filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_make_with_filter (sk_image_t cimage, gr_recording_context_t context, sk_imagefilter_t filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);
		}
		private static Delegates.sk_image_make_with_filter sk_image_make_with_filter_delegate;
		internal static sk_image_t sk_image_make_with_filter (sk_image_t cimage, gr_recording_context_t context, sk_imagefilter_t filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset) =>
			(sk_image_make_with_filter_delegate ??= GetSymbol<Delegates.sk_image_make_with_filter> ("sk_image_make_with_filter")).Invoke (cimage, context, filter, subset, clipBounds, outSubset, outOffset);
		#endif

		// sk_image_t* sk_image_make_with_filter_legacy(const sk_image_t* cimage, const sk_imagefilter_t* filter, const sk_irect_t* subset, const sk_irect_t* clipBounds, sk_irect_t* outSubset, sk_ipoint_t* outOffset)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_make_with_filter_legacy (sk_image_t cimage, sk_imagefilter_t filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_make_with_filter_legacy (sk_image_t cimage, sk_imagefilter_t filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);
		}
		private static Delegates.sk_image_make_with_filter_legacy sk_image_make_with_filter_legacy_delegate;
		internal static sk_image_t sk_image_make_with_filter_legacy (sk_image_t cimage, sk_imagefilter_t filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset) =>
			(sk_image_make_with_filter_legacy_delegate ??= GetSymbol<Delegates.sk_image_make_with_filter_legacy> ("sk_image_make_with_filter_legacy")).Invoke (cimage, filter, subset, clipBounds, outSubset, outOffset);
		#endif

		// sk_image_t* sk_image_new_from_adopted_texture(gr_recording_context_t* context, const gr_backendtexture_t* texture, gr_surfaceorigin_t origin, sk_colortype_t colorType, sk_alphatype_t alpha, sk_colorspace_t* colorSpace)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_from_adopted_texture (gr_recording_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, sk_colorspace_t colorSpace);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_new_from_adopted_texture (gr_recording_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, sk_colorspace_t colorSpace);
		}
		private static Delegates.sk_image_new_from_adopted_texture sk_image_new_from_adopted_texture_delegate;
		internal static sk_image_t sk_image_new_from_adopted_texture (gr_recording_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, sk_colorspace_t colorSpace) =>
			(sk_image_new_from_adopted_texture_delegate ??= GetSymbol<Delegates.sk_image_new_from_adopted_texture> ("sk_image_new_from_adopted_texture")).Invoke (context, texture, origin, colorType, alpha, colorSpace);
		#endif

		// sk_image_t* sk_image_new_from_bitmap(const sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_from_bitmap (sk_bitmap_t cbitmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_new_from_bitmap (sk_bitmap_t cbitmap);
		}
		private static Delegates.sk_image_new_from_bitmap sk_image_new_from_bitmap_delegate;
		internal static sk_image_t sk_image_new_from_bitmap (sk_bitmap_t cbitmap) =>
			(sk_image_new_from_bitmap_delegate ??= GetSymbol<Delegates.sk_image_new_from_bitmap> ("sk_image_new_from_bitmap")).Invoke (cbitmap);
		#endif

		// sk_image_t* sk_image_new_from_encoded(sk_data_t* encoded)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_from_encoded (sk_data_t encoded);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_new_from_encoded (sk_data_t encoded);
		}
		private static Delegates.sk_image_new_from_encoded sk_image_new_from_encoded_delegate;
		internal static sk_image_t sk_image_new_from_encoded (sk_data_t encoded) =>
			(sk_image_new_from_encoded_delegate ??= GetSymbol<Delegates.sk_image_new_from_encoded> ("sk_image_new_from_encoded")).Invoke (encoded);
		#endif

		// sk_image_t* sk_image_new_from_picture(sk_picture_t* picture, const sk_isize_t* dimensions, const sk_matrix_t* matrix, const sk_paint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_from_picture (sk_picture_t picture, SKSizeI* dimensions, SKMatrix* matrix, sk_paint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_new_from_picture (sk_picture_t picture, SKSizeI* dimensions, SKMatrix* matrix, sk_paint_t paint);
		}
		private static Delegates.sk_image_new_from_picture sk_image_new_from_picture_delegate;
		internal static sk_image_t sk_image_new_from_picture (sk_picture_t picture, SKSizeI* dimensions, SKMatrix* matrix, sk_paint_t paint) =>
			(sk_image_new_from_picture_delegate ??= GetSymbol<Delegates.sk_image_new_from_picture> ("sk_image_new_from_picture")).Invoke (picture, dimensions, matrix, paint);
		#endif

		// sk_image_t* sk_image_new_from_texture(gr_recording_context_t* context, const gr_backendtexture_t* texture, gr_surfaceorigin_t origin, sk_colortype_t colorType, sk_alphatype_t alpha, sk_colorspace_t* colorSpace, sk_image_texture_release_proc releaseProc, void* releaseContext)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_from_texture (gr_recording_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, sk_colorspace_t colorSpace, SKImageTextureReleaseProxyDelegate releaseProc, void* releaseContext);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_new_from_texture (gr_recording_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, sk_colorspace_t colorSpace, SKImageTextureReleaseProxyDelegate releaseProc, void* releaseContext);
		}
		private static Delegates.sk_image_new_from_texture sk_image_new_from_texture_delegate;
		internal static sk_image_t sk_image_new_from_texture (gr_recording_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, sk_colorspace_t colorSpace, SKImageTextureReleaseProxyDelegate releaseProc, void* releaseContext) =>
			(sk_image_new_from_texture_delegate ??= GetSymbol<Delegates.sk_image_new_from_texture> ("sk_image_new_from_texture")).Invoke (context, texture, origin, colorType, alpha, colorSpace, releaseProc, releaseContext);
		#endif

		// sk_image_t* sk_image_new_raster(const sk_pixmap_t* pixmap, sk_image_raster_release_proc releaseProc, void* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_raster (sk_pixmap_t pixmap, SKImageRasterReleaseProxyDelegate releaseProc, void* context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_new_raster (sk_pixmap_t pixmap, SKImageRasterReleaseProxyDelegate releaseProc, void* context);
		}
		private static Delegates.sk_image_new_raster sk_image_new_raster_delegate;
		internal static sk_image_t sk_image_new_raster (sk_pixmap_t pixmap, SKImageRasterReleaseProxyDelegate releaseProc, void* context) =>
			(sk_image_new_raster_delegate ??= GetSymbol<Delegates.sk_image_new_raster> ("sk_image_new_raster")).Invoke (pixmap, releaseProc, context);
		#endif

		// sk_image_t* sk_image_new_raster_copy(const sk_imageinfo_t*, const void* pixels, size_t rowBytes)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_raster_copy (SKImageInfoNative* param0, void* pixels, /* size_t */ IntPtr rowBytes);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_new_raster_copy (SKImageInfoNative* param0, void* pixels, /* size_t */ IntPtr rowBytes);
		}
		private static Delegates.sk_image_new_raster_copy sk_image_new_raster_copy_delegate;
		internal static sk_image_t sk_image_new_raster_copy (SKImageInfoNative* param0, void* pixels, /* size_t */ IntPtr rowBytes) =>
			(sk_image_new_raster_copy_delegate ??= GetSymbol<Delegates.sk_image_new_raster_copy> ("sk_image_new_raster_copy")).Invoke (param0, pixels, rowBytes);
		#endif

		// sk_image_t* sk_image_new_raster_copy_with_pixmap(const sk_pixmap_t* pixmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_raster_copy_with_pixmap (sk_pixmap_t pixmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_new_raster_copy_with_pixmap (sk_pixmap_t pixmap);
		}
		private static Delegates.sk_image_new_raster_copy_with_pixmap sk_image_new_raster_copy_with_pixmap_delegate;
		internal static sk_image_t sk_image_new_raster_copy_with_pixmap (sk_pixmap_t pixmap) =>
			(sk_image_new_raster_copy_with_pixmap_delegate ??= GetSymbol<Delegates.sk_image_new_raster_copy_with_pixmap> ("sk_image_new_raster_copy_with_pixmap")).Invoke (pixmap);
		#endif

		// sk_image_t* sk_image_new_raster_data(const sk_imageinfo_t* cinfo, sk_data_t* pixels, size_t rowBytes)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_image_new_raster_data (SKImageInfoNative* cinfo, sk_data_t pixels, /* size_t */ IntPtr rowBytes);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_image_new_raster_data (SKImageInfoNative* cinfo, sk_data_t pixels, /* size_t */ IntPtr rowBytes);
		}
		private static Delegates.sk_image_new_raster_data sk_image_new_raster_data_delegate;
		internal static sk_image_t sk_image_new_raster_data (SKImageInfoNative* cinfo, sk_data_t pixels, /* size_t */ IntPtr rowBytes) =>
			(sk_image_new_raster_data_delegate ??= GetSymbol<Delegates.sk_image_new_raster_data> ("sk_image_new_raster_data")).Invoke (cinfo, pixels, rowBytes);
		#endif

		// bool sk_image_peek_pixels(const sk_image_t* image, sk_pixmap_t* pixmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_peek_pixels (sk_image_t image, sk_pixmap_t pixmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_peek_pixels (sk_image_t image, sk_pixmap_t pixmap);
		}
		private static Delegates.sk_image_peek_pixels sk_image_peek_pixels_delegate;
		internal static bool sk_image_peek_pixels (sk_image_t image, sk_pixmap_t pixmap) =>
			(sk_image_peek_pixels_delegate ??= GetSymbol<Delegates.sk_image_peek_pixels> ("sk_image_peek_pixels")).Invoke (image, pixmap);
		#endif

		// bool sk_image_read_pixels(const sk_image_t* image, const sk_imageinfo_t* dstInfo, void* dstPixels, size_t dstRowBytes, int srcX, int srcY, sk_image_caching_hint_t cachingHint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_read_pixels (sk_image_t image, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_read_pixels (sk_image_t image, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint);
		}
		private static Delegates.sk_image_read_pixels sk_image_read_pixels_delegate;
		internal static bool sk_image_read_pixels (sk_image_t image, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint) =>
			(sk_image_read_pixels_delegate ??= GetSymbol<Delegates.sk_image_read_pixels> ("sk_image_read_pixels")).Invoke (image, dstInfo, dstPixels, dstRowBytes, srcX, srcY, cachingHint);
		#endif

		// bool sk_image_read_pixels_into_pixmap(const sk_image_t* image, const sk_pixmap_t* dst, int srcX, int srcY, sk_image_caching_hint_t cachingHint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_read_pixels_into_pixmap (sk_image_t image, sk_pixmap_t dst, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_read_pixels_into_pixmap (sk_image_t image, sk_pixmap_t dst, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint);
		}
		private static Delegates.sk_image_read_pixels_into_pixmap sk_image_read_pixels_into_pixmap_delegate;
		internal static bool sk_image_read_pixels_into_pixmap (sk_image_t image, sk_pixmap_t dst, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint) =>
			(sk_image_read_pixels_into_pixmap_delegate ??= GetSymbol<Delegates.sk_image_read_pixels_into_pixmap> ("sk_image_read_pixels_into_pixmap")).Invoke (image, dst, srcX, srcY, cachingHint);
		#endif

		// void sk_image_ref(const sk_image_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_image_ref (sk_image_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_image_ref (sk_image_t param0);
		}
		private static Delegates.sk_image_ref sk_image_ref_delegate;
		internal static void sk_image_ref (sk_image_t param0) =>
			(sk_image_ref_delegate ??= GetSymbol<Delegates.sk_image_ref> ("sk_image_ref")).Invoke (param0);
		#endif

		// sk_data_t* sk_image_ref_encoded(const sk_image_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_image_ref_encoded (sk_image_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_image_ref_encoded (sk_image_t param0);
		}
		private static Delegates.sk_image_ref_encoded sk_image_ref_encoded_delegate;
		internal static sk_data_t sk_image_ref_encoded (sk_image_t param0) =>
			(sk_image_ref_encoded_delegate ??= GetSymbol<Delegates.sk_image_ref_encoded> ("sk_image_ref_encoded")).Invoke (param0);
		#endif

		// bool sk_image_scale_pixels(const sk_image_t* image, const sk_pixmap_t* dst, sk_filter_quality_t quality, sk_image_caching_hint_t cachingHint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_scale_pixels (sk_image_t image, sk_pixmap_t dst, SKFilterQuality quality, SKImageCachingHint cachingHint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_scale_pixels (sk_image_t image, sk_pixmap_t dst, SKFilterQuality quality, SKImageCachingHint cachingHint);
		}
		private static Delegates.sk_image_scale_pixels sk_image_scale_pixels_delegate;
		internal static bool sk_image_scale_pixels (sk_image_t image, sk_pixmap_t dst, SKFilterQuality quality, SKImageCachingHint cachingHint) =>
			(sk_image_scale_pixels_delegate ??= GetSymbol<Delegates.sk_image_scale_pixels> ("sk_image_scale_pixels")).Invoke (image, dst, quality, cachingHint);
		#endif

		// void sk_image_unref(const sk_image_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_image_unref (sk_image_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_image_unref (sk_image_t param0);
		}
		private static Delegates.sk_image_unref sk_image_unref_delegate;
		internal static void sk_image_unref (sk_image_t param0) =>
			(sk_image_unref_delegate ??= GetSymbol<Delegates.sk_image_unref> ("sk_image_unref")).Invoke (param0);
		#endif

		#endregion

		#region sk_imagefilter.h

		// void sk_imagefilter_croprect_destructor(sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_imagefilter_croprect_destructor (sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_imagefilter_croprect_destructor (sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_croprect_destructor sk_imagefilter_croprect_destructor_delegate;
		internal static void sk_imagefilter_croprect_destructor (sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_croprect_destructor_delegate ??= GetSymbol<Delegates.sk_imagefilter_croprect_destructor> ("sk_imagefilter_croprect_destructor")).Invoke (cropRect);
		#endif

		// uint32_t sk_imagefilter_croprect_get_flags(sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_imagefilter_croprect_get_flags (sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_imagefilter_croprect_get_flags (sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_croprect_get_flags sk_imagefilter_croprect_get_flags_delegate;
		internal static UInt32 sk_imagefilter_croprect_get_flags (sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_croprect_get_flags_delegate ??= GetSymbol<Delegates.sk_imagefilter_croprect_get_flags> ("sk_imagefilter_croprect_get_flags")).Invoke (cropRect);
		#endif

		// void sk_imagefilter_croprect_get_rect(sk_imagefilter_croprect_t* cropRect, sk_rect_t* rect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_imagefilter_croprect_get_rect (sk_imagefilter_croprect_t cropRect, SKRect* rect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_imagefilter_croprect_get_rect (sk_imagefilter_croprect_t cropRect, SKRect* rect);
		}
		private static Delegates.sk_imagefilter_croprect_get_rect sk_imagefilter_croprect_get_rect_delegate;
		internal static void sk_imagefilter_croprect_get_rect (sk_imagefilter_croprect_t cropRect, SKRect* rect) =>
			(sk_imagefilter_croprect_get_rect_delegate ??= GetSymbol<Delegates.sk_imagefilter_croprect_get_rect> ("sk_imagefilter_croprect_get_rect")).Invoke (cropRect, rect);
		#endif

		// sk_imagefilter_croprect_t* sk_imagefilter_croprect_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_croprect_t sk_imagefilter_croprect_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_croprect_t sk_imagefilter_croprect_new ();
		}
		private static Delegates.sk_imagefilter_croprect_new sk_imagefilter_croprect_new_delegate;
		internal static sk_imagefilter_croprect_t sk_imagefilter_croprect_new () =>
			(sk_imagefilter_croprect_new_delegate ??= GetSymbol<Delegates.sk_imagefilter_croprect_new> ("sk_imagefilter_croprect_new")).Invoke ();
		#endif

		// sk_imagefilter_croprect_t* sk_imagefilter_croprect_new_with_rect(const sk_rect_t* rect, uint32_t flags)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_croprect_t sk_imagefilter_croprect_new_with_rect (SKRect* rect, UInt32 flags);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_croprect_t sk_imagefilter_croprect_new_with_rect (SKRect* rect, UInt32 flags);
		}
		private static Delegates.sk_imagefilter_croprect_new_with_rect sk_imagefilter_croprect_new_with_rect_delegate;
		internal static sk_imagefilter_croprect_t sk_imagefilter_croprect_new_with_rect (SKRect* rect, UInt32 flags) =>
			(sk_imagefilter_croprect_new_with_rect_delegate ??= GetSymbol<Delegates.sk_imagefilter_croprect_new_with_rect> ("sk_imagefilter_croprect_new_with_rect")).Invoke (rect, flags);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_alpha_threshold(const sk_region_t* region, float innerThreshold, float outerThreshold, sk_imagefilter_t* input)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_alpha_threshold (sk_region_t region, Single innerThreshold, Single outerThreshold, sk_imagefilter_t input);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_alpha_threshold (sk_region_t region, Single innerThreshold, Single outerThreshold, sk_imagefilter_t input);
		}
		private static Delegates.sk_imagefilter_new_alpha_threshold sk_imagefilter_new_alpha_threshold_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_alpha_threshold (sk_region_t region, Single innerThreshold, Single outerThreshold, sk_imagefilter_t input) =>
			(sk_imagefilter_new_alpha_threshold_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_alpha_threshold> ("sk_imagefilter_new_alpha_threshold")).Invoke (region, innerThreshold, outerThreshold, input);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_arithmetic(float k1, float k2, float k3, float k4, bool enforcePMColor, sk_imagefilter_t* background, sk_imagefilter_t* foreground, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_arithmetic (Single k1, Single k2, Single k3, Single k4, [MarshalAs (UnmanagedType.I1)] bool enforcePMColor, sk_imagefilter_t background, sk_imagefilter_t foreground, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_arithmetic (Single k1, Single k2, Single k3, Single k4, [MarshalAs (UnmanagedType.I1)] bool enforcePMColor, sk_imagefilter_t background, sk_imagefilter_t foreground, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_arithmetic sk_imagefilter_new_arithmetic_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_arithmetic (Single k1, Single k2, Single k3, Single k4, [MarshalAs (UnmanagedType.I1)] bool enforcePMColor, sk_imagefilter_t background, sk_imagefilter_t foreground, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_arithmetic_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_arithmetic> ("sk_imagefilter_new_arithmetic")).Invoke (k1, k2, k3, k4, enforcePMColor, background, foreground, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_blur(float sigmaX, float sigmaY, sk_shader_tilemode_t tileMode, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_blur (Single sigmaX, Single sigmaY, SKShaderTileMode tileMode, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_blur (Single sigmaX, Single sigmaY, SKShaderTileMode tileMode, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_blur sk_imagefilter_new_blur_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_blur (Single sigmaX, Single sigmaY, SKShaderTileMode tileMode, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_blur_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_blur> ("sk_imagefilter_new_blur")).Invoke (sigmaX, sigmaY, tileMode, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_color_filter(sk_colorfilter_t* cf, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_color_filter (sk_colorfilter_t cf, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_color_filter (sk_colorfilter_t cf, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_color_filter sk_imagefilter_new_color_filter_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_color_filter (sk_colorfilter_t cf, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_color_filter_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_color_filter> ("sk_imagefilter_new_color_filter")).Invoke (cf, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_compose(sk_imagefilter_t* outer, sk_imagefilter_t* inner)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_compose (sk_imagefilter_t outer, sk_imagefilter_t inner);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_compose (sk_imagefilter_t outer, sk_imagefilter_t inner);
		}
		private static Delegates.sk_imagefilter_new_compose sk_imagefilter_new_compose_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_compose (sk_imagefilter_t outer, sk_imagefilter_t inner) =>
			(sk_imagefilter_new_compose_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_compose> ("sk_imagefilter_new_compose")).Invoke (outer, inner);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_dilate(float radiusX, float radiusY, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_dilate (Single radiusX, Single radiusY, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_dilate (Single radiusX, Single radiusY, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_dilate sk_imagefilter_new_dilate_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_dilate (Single radiusX, Single radiusY, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_dilate_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_dilate> ("sk_imagefilter_new_dilate")).Invoke (radiusX, radiusY, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_displacement_map_effect(sk_color_channel_t xChannelSelector, sk_color_channel_t yChannelSelector, float scale, sk_imagefilter_t* displacement, sk_imagefilter_t* color, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_displacement_map_effect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, Single scale, sk_imagefilter_t displacement, sk_imagefilter_t color, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_displacement_map_effect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, Single scale, sk_imagefilter_t displacement, sk_imagefilter_t color, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_displacement_map_effect sk_imagefilter_new_displacement_map_effect_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_displacement_map_effect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, Single scale, sk_imagefilter_t displacement, sk_imagefilter_t color, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_displacement_map_effect_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_displacement_map_effect> ("sk_imagefilter_new_displacement_map_effect")).Invoke (xChannelSelector, yChannelSelector, scale, displacement, color, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_distant_lit_diffuse(const sk_point3_t* direction, sk_color_t lightColor, float surfaceScale, float kd, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_distant_lit_diffuse (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_distant_lit_diffuse (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_distant_lit_diffuse sk_imagefilter_new_distant_lit_diffuse_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_distant_lit_diffuse (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_distant_lit_diffuse_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_distant_lit_diffuse> ("sk_imagefilter_new_distant_lit_diffuse")).Invoke (direction, lightColor, surfaceScale, kd, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_distant_lit_specular(const sk_point3_t* direction, sk_color_t lightColor, float surfaceScale, float ks, float shininess, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_distant_lit_specular (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_distant_lit_specular (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_distant_lit_specular sk_imagefilter_new_distant_lit_specular_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_distant_lit_specular (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_distant_lit_specular_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_distant_lit_specular> ("sk_imagefilter_new_distant_lit_specular")).Invoke (direction, lightColor, surfaceScale, ks, shininess, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_drop_shadow(float dx, float dy, float sigmaX, float sigmaY, sk_color_t color, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_drop_shadow (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_drop_shadow (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_drop_shadow sk_imagefilter_new_drop_shadow_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_drop_shadow (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_drop_shadow_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_drop_shadow> ("sk_imagefilter_new_drop_shadow")).Invoke (dx, dy, sigmaX, sigmaY, color, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_drop_shadow_only(float dx, float dy, float sigmaX, float sigmaY, sk_color_t color, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_drop_shadow_only (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_drop_shadow_only (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_drop_shadow_only sk_imagefilter_new_drop_shadow_only_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_drop_shadow_only (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_drop_shadow_only_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_drop_shadow_only> ("sk_imagefilter_new_drop_shadow_only")).Invoke (dx, dy, sigmaX, sigmaY, color, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_erode(float radiusX, float radiusY, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_erode (Single radiusX, Single radiusY, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_erode (Single radiusX, Single radiusY, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_erode sk_imagefilter_new_erode_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_erode (Single radiusX, Single radiusY, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_erode_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_erode> ("sk_imagefilter_new_erode")).Invoke (radiusX, radiusY, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_image_source(sk_image_t* image, const sk_rect_t* srcRect, const sk_rect_t* dstRect, sk_filter_quality_t filterQuality)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_image_source (sk_image_t image, SKRect* srcRect, SKRect* dstRect, SKFilterQuality filterQuality);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_image_source (sk_image_t image, SKRect* srcRect, SKRect* dstRect, SKFilterQuality filterQuality);
		}
		private static Delegates.sk_imagefilter_new_image_source sk_imagefilter_new_image_source_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_image_source (sk_image_t image, SKRect* srcRect, SKRect* dstRect, SKFilterQuality filterQuality) =>
			(sk_imagefilter_new_image_source_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_image_source> ("sk_imagefilter_new_image_source")).Invoke (image, srcRect, dstRect, filterQuality);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_image_source_default(sk_image_t* image)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_image_source_default (sk_image_t image);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_image_source_default (sk_image_t image);
		}
		private static Delegates.sk_imagefilter_new_image_source_default sk_imagefilter_new_image_source_default_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_image_source_default (sk_image_t image) =>
			(sk_imagefilter_new_image_source_default_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_image_source_default> ("sk_imagefilter_new_image_source_default")).Invoke (image);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_magnifier(const sk_rect_t* src, float inset, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_magnifier (SKRect* src, Single inset, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_magnifier (SKRect* src, Single inset, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_magnifier sk_imagefilter_new_magnifier_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_magnifier (SKRect* src, Single inset, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_magnifier_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_magnifier> ("sk_imagefilter_new_magnifier")).Invoke (src, inset, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_matrix(const sk_matrix_t* matrix, sk_filter_quality_t quality, sk_imagefilter_t* input)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_matrix (SKMatrix* matrix, SKFilterQuality quality, sk_imagefilter_t input);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_matrix (SKMatrix* matrix, SKFilterQuality quality, sk_imagefilter_t input);
		}
		private static Delegates.sk_imagefilter_new_matrix sk_imagefilter_new_matrix_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_matrix (SKMatrix* matrix, SKFilterQuality quality, sk_imagefilter_t input) =>
			(sk_imagefilter_new_matrix_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_matrix> ("sk_imagefilter_new_matrix")).Invoke (matrix, quality, input);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_matrix_convolution(const sk_isize_t* kernelSize, const float[-1] kernel, float gain, float bias, const sk_ipoint_t* kernelOffset, sk_shader_tilemode_t tileMode, bool convolveAlpha, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_matrix_convolution (SKSizeI* kernelSize, Single* kernel, Single gain, Single bias, SKPointI* kernelOffset, SKShaderTileMode tileMode, [MarshalAs (UnmanagedType.I1)] bool convolveAlpha, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_matrix_convolution (SKSizeI* kernelSize, Single* kernel, Single gain, Single bias, SKPointI* kernelOffset, SKShaderTileMode tileMode, [MarshalAs (UnmanagedType.I1)] bool convolveAlpha, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_matrix_convolution sk_imagefilter_new_matrix_convolution_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_matrix_convolution (SKSizeI* kernelSize, Single* kernel, Single gain, Single bias, SKPointI* kernelOffset, SKShaderTileMode tileMode, [MarshalAs (UnmanagedType.I1)] bool convolveAlpha, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_matrix_convolution_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_matrix_convolution> ("sk_imagefilter_new_matrix_convolution")).Invoke (kernelSize, kernel, gain, bias, kernelOffset, tileMode, convolveAlpha, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_merge(sk_imagefilter_t*[-1] filters, int count, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_merge (sk_imagefilter_t* filters, Int32 count, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_merge (sk_imagefilter_t* filters, Int32 count, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_merge sk_imagefilter_new_merge_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_merge (sk_imagefilter_t* filters, Int32 count, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_merge_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_merge> ("sk_imagefilter_new_merge")).Invoke (filters, count, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_offset(float dx, float dy, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_offset (Single dx, Single dy, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_offset (Single dx, Single dy, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_offset sk_imagefilter_new_offset_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_offset (Single dx, Single dy, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_offset_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_offset> ("sk_imagefilter_new_offset")).Invoke (dx, dy, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_paint(const sk_paint_t* paint, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_paint (sk_paint_t paint, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_paint (sk_paint_t paint, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_paint sk_imagefilter_new_paint_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_paint (sk_paint_t paint, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_paint_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_paint> ("sk_imagefilter_new_paint")).Invoke (paint, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_picture(sk_picture_t* picture)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_picture (sk_picture_t picture);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_picture (sk_picture_t picture);
		}
		private static Delegates.sk_imagefilter_new_picture sk_imagefilter_new_picture_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_picture (sk_picture_t picture) =>
			(sk_imagefilter_new_picture_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_picture> ("sk_imagefilter_new_picture")).Invoke (picture);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_picture_with_croprect(sk_picture_t* picture, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_picture_with_croprect (sk_picture_t picture, SKRect* cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_picture_with_croprect (sk_picture_t picture, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_picture_with_croprect sk_imagefilter_new_picture_with_croprect_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_picture_with_croprect (sk_picture_t picture, SKRect* cropRect) =>
			(sk_imagefilter_new_picture_with_croprect_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_picture_with_croprect> ("sk_imagefilter_new_picture_with_croprect")).Invoke (picture, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_point_lit_diffuse(const sk_point3_t* location, sk_color_t lightColor, float surfaceScale, float kd, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_point_lit_diffuse (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_point_lit_diffuse (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_point_lit_diffuse sk_imagefilter_new_point_lit_diffuse_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_point_lit_diffuse (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_point_lit_diffuse_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_point_lit_diffuse> ("sk_imagefilter_new_point_lit_diffuse")).Invoke (location, lightColor, surfaceScale, kd, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_point_lit_specular(const sk_point3_t* location, sk_color_t lightColor, float surfaceScale, float ks, float shininess, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_point_lit_specular (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_point_lit_specular (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_point_lit_specular sk_imagefilter_new_point_lit_specular_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_point_lit_specular (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_point_lit_specular_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_point_lit_specular> ("sk_imagefilter_new_point_lit_specular")).Invoke (location, lightColor, surfaceScale, ks, shininess, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_spot_lit_diffuse(const sk_point3_t* location, const sk_point3_t* target, float specularExponent, float cutoffAngle, sk_color_t lightColor, float surfaceScale, float kd, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_spot_lit_diffuse (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_spot_lit_diffuse (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_spot_lit_diffuse sk_imagefilter_new_spot_lit_diffuse_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_spot_lit_diffuse (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_spot_lit_diffuse_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_spot_lit_diffuse> ("sk_imagefilter_new_spot_lit_diffuse")).Invoke (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_spot_lit_specular(const sk_point3_t* location, const sk_point3_t* target, float specularExponent, float cutoffAngle, sk_color_t lightColor, float surfaceScale, float ks, float shininess, sk_imagefilter_t* input, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_spot_lit_specular (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_spot_lit_specular (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_spot_lit_specular sk_imagefilter_new_spot_lit_specular_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_spot_lit_specular (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_spot_lit_specular_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_spot_lit_specular> ("sk_imagefilter_new_spot_lit_specular")).Invoke (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_tile(const sk_rect_t* src, const sk_rect_t* dst, sk_imagefilter_t* input)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_tile (SKRect* src, SKRect* dst, sk_imagefilter_t input);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_tile (SKRect* src, SKRect* dst, sk_imagefilter_t input);
		}
		private static Delegates.sk_imagefilter_new_tile sk_imagefilter_new_tile_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_tile (SKRect* src, SKRect* dst, sk_imagefilter_t input) =>
			(sk_imagefilter_new_tile_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_tile> ("sk_imagefilter_new_tile")).Invoke (src, dst, input);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_xfermode(sk_blendmode_t mode, sk_imagefilter_t* background, sk_imagefilter_t* foreground, const sk_imagefilter_croprect_t* cropRect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_xfermode (SKBlendMode mode, sk_imagefilter_t background, sk_imagefilter_t foreground, sk_imagefilter_croprect_t cropRect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_xfermode (SKBlendMode mode, sk_imagefilter_t background, sk_imagefilter_t foreground, sk_imagefilter_croprect_t cropRect);
		}
		private static Delegates.sk_imagefilter_new_xfermode sk_imagefilter_new_xfermode_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_xfermode (SKBlendMode mode, sk_imagefilter_t background, sk_imagefilter_t foreground, sk_imagefilter_croprect_t cropRect) =>
			(sk_imagefilter_new_xfermode_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_xfermode> ("sk_imagefilter_new_xfermode")).Invoke (mode, background, foreground, cropRect);
		#endif

		// void sk_imagefilter_unref(sk_imagefilter_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_imagefilter_unref (sk_imagefilter_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_imagefilter_unref (sk_imagefilter_t param0);
		}
		private static Delegates.sk_imagefilter_unref sk_imagefilter_unref_delegate;
		internal static void sk_imagefilter_unref (sk_imagefilter_t param0) =>
			(sk_imagefilter_unref_delegate ??= GetSymbol<Delegates.sk_imagefilter_unref> ("sk_imagefilter_unref")).Invoke (param0);
		#endif

		#endregion

		#region sk_mask.h

		// uint8_t* sk_mask_alloc_image(size_t bytes)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte* sk_mask_alloc_image (/* size_t */ IntPtr bytes);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte* sk_mask_alloc_image (/* size_t */ IntPtr bytes);
		}
		private static Delegates.sk_mask_alloc_image sk_mask_alloc_image_delegate;
		internal static Byte* sk_mask_alloc_image (/* size_t */ IntPtr bytes) =>
			(sk_mask_alloc_image_delegate ??= GetSymbol<Delegates.sk_mask_alloc_image> ("sk_mask_alloc_image")).Invoke (bytes);
		#endif

		// size_t sk_mask_compute_image_size(sk_mask_t* cmask)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_mask_compute_image_size (SKMask* cmask);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_mask_compute_image_size (SKMask* cmask);
		}
		private static Delegates.sk_mask_compute_image_size sk_mask_compute_image_size_delegate;
		internal static /* size_t */ IntPtr sk_mask_compute_image_size (SKMask* cmask) =>
			(sk_mask_compute_image_size_delegate ??= GetSymbol<Delegates.sk_mask_compute_image_size> ("sk_mask_compute_image_size")).Invoke (cmask);
		#endif

		// size_t sk_mask_compute_total_image_size(sk_mask_t* cmask)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_mask_compute_total_image_size (SKMask* cmask);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_mask_compute_total_image_size (SKMask* cmask);
		}
		private static Delegates.sk_mask_compute_total_image_size sk_mask_compute_total_image_size_delegate;
		internal static /* size_t */ IntPtr sk_mask_compute_total_image_size (SKMask* cmask) =>
			(sk_mask_compute_total_image_size_delegate ??= GetSymbol<Delegates.sk_mask_compute_total_image_size> ("sk_mask_compute_total_image_size")).Invoke (cmask);
		#endif

		// void sk_mask_free_image(void* image)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_mask_free_image (void* image);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_mask_free_image (void* image);
		}
		private static Delegates.sk_mask_free_image sk_mask_free_image_delegate;
		internal static void sk_mask_free_image (void* image) =>
			(sk_mask_free_image_delegate ??= GetSymbol<Delegates.sk_mask_free_image> ("sk_mask_free_image")).Invoke (image);
		#endif

		// void* sk_mask_get_addr(sk_mask_t* cmask, int x, int y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_mask_get_addr (SKMask* cmask, Int32 x, Int32 y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_mask_get_addr (SKMask* cmask, Int32 x, Int32 y);
		}
		private static Delegates.sk_mask_get_addr sk_mask_get_addr_delegate;
		internal static void* sk_mask_get_addr (SKMask* cmask, Int32 x, Int32 y) =>
			(sk_mask_get_addr_delegate ??= GetSymbol<Delegates.sk_mask_get_addr> ("sk_mask_get_addr")).Invoke (cmask, x, y);
		#endif

		// uint8_t* sk_mask_get_addr_1(sk_mask_t* cmask, int x, int y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte* sk_mask_get_addr_1 (SKMask* cmask, Int32 x, Int32 y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte* sk_mask_get_addr_1 (SKMask* cmask, Int32 x, Int32 y);
		}
		private static Delegates.sk_mask_get_addr_1 sk_mask_get_addr_1_delegate;
		internal static Byte* sk_mask_get_addr_1 (SKMask* cmask, Int32 x, Int32 y) =>
			(sk_mask_get_addr_1_delegate ??= GetSymbol<Delegates.sk_mask_get_addr_1> ("sk_mask_get_addr_1")).Invoke (cmask, x, y);
		#endif

		// uint32_t* sk_mask_get_addr_32(sk_mask_t* cmask, int x, int y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32* sk_mask_get_addr_32 (SKMask* cmask, Int32 x, Int32 y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32* sk_mask_get_addr_32 (SKMask* cmask, Int32 x, Int32 y);
		}
		private static Delegates.sk_mask_get_addr_32 sk_mask_get_addr_32_delegate;
		internal static UInt32* sk_mask_get_addr_32 (SKMask* cmask, Int32 x, Int32 y) =>
			(sk_mask_get_addr_32_delegate ??= GetSymbol<Delegates.sk_mask_get_addr_32> ("sk_mask_get_addr_32")).Invoke (cmask, x, y);
		#endif

		// uint8_t* sk_mask_get_addr_8(sk_mask_t* cmask, int x, int y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte* sk_mask_get_addr_8 (SKMask* cmask, Int32 x, Int32 y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte* sk_mask_get_addr_8 (SKMask* cmask, Int32 x, Int32 y);
		}
		private static Delegates.sk_mask_get_addr_8 sk_mask_get_addr_8_delegate;
		internal static Byte* sk_mask_get_addr_8 (SKMask* cmask, Int32 x, Int32 y) =>
			(sk_mask_get_addr_8_delegate ??= GetSymbol<Delegates.sk_mask_get_addr_8> ("sk_mask_get_addr_8")).Invoke (cmask, x, y);
		#endif

		// uint16_t* sk_mask_get_addr_lcd_16(sk_mask_t* cmask, int x, int y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt16* sk_mask_get_addr_lcd_16 (SKMask* cmask, Int32 x, Int32 y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt16* sk_mask_get_addr_lcd_16 (SKMask* cmask, Int32 x, Int32 y);
		}
		private static Delegates.sk_mask_get_addr_lcd_16 sk_mask_get_addr_lcd_16_delegate;
		internal static UInt16* sk_mask_get_addr_lcd_16 (SKMask* cmask, Int32 x, Int32 y) =>
			(sk_mask_get_addr_lcd_16_delegate ??= GetSymbol<Delegates.sk_mask_get_addr_lcd_16> ("sk_mask_get_addr_lcd_16")).Invoke (cmask, x, y);
		#endif

		// bool sk_mask_is_empty(sk_mask_t* cmask)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_mask_is_empty (SKMask* cmask);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_mask_is_empty (SKMask* cmask);
		}
		private static Delegates.sk_mask_is_empty sk_mask_is_empty_delegate;
		internal static bool sk_mask_is_empty (SKMask* cmask) =>
			(sk_mask_is_empty_delegate ??= GetSymbol<Delegates.sk_mask_is_empty> ("sk_mask_is_empty")).Invoke (cmask);
		#endif

		#endregion

		#region sk_maskfilter.h

		// sk_maskfilter_t* sk_maskfilter_new_blur(sk_blurstyle_t, float sigma)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_blur (SKBlurStyle param0, Single sigma);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_maskfilter_t sk_maskfilter_new_blur (SKBlurStyle param0, Single sigma);
		}
		private static Delegates.sk_maskfilter_new_blur sk_maskfilter_new_blur_delegate;
		internal static sk_maskfilter_t sk_maskfilter_new_blur (SKBlurStyle param0, Single sigma) =>
			(sk_maskfilter_new_blur_delegate ??= GetSymbol<Delegates.sk_maskfilter_new_blur> ("sk_maskfilter_new_blur")).Invoke (param0, sigma);
		#endif

		// sk_maskfilter_t* sk_maskfilter_new_blur_with_flags(sk_blurstyle_t, float sigma, bool respectCTM)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_blur_with_flags (SKBlurStyle param0, Single sigma, [MarshalAs (UnmanagedType.I1)] bool respectCTM);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_maskfilter_t sk_maskfilter_new_blur_with_flags (SKBlurStyle param0, Single sigma, [MarshalAs (UnmanagedType.I1)] bool respectCTM);
		}
		private static Delegates.sk_maskfilter_new_blur_with_flags sk_maskfilter_new_blur_with_flags_delegate;
		internal static sk_maskfilter_t sk_maskfilter_new_blur_with_flags (SKBlurStyle param0, Single sigma, [MarshalAs (UnmanagedType.I1)] bool respectCTM) =>
			(sk_maskfilter_new_blur_with_flags_delegate ??= GetSymbol<Delegates.sk_maskfilter_new_blur_with_flags> ("sk_maskfilter_new_blur_with_flags")).Invoke (param0, sigma, respectCTM);
		#endif

		// sk_maskfilter_t* sk_maskfilter_new_clip(uint8_t min, uint8_t max)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_clip (Byte min, Byte max);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_maskfilter_t sk_maskfilter_new_clip (Byte min, Byte max);
		}
		private static Delegates.sk_maskfilter_new_clip sk_maskfilter_new_clip_delegate;
		internal static sk_maskfilter_t sk_maskfilter_new_clip (Byte min, Byte max) =>
			(sk_maskfilter_new_clip_delegate ??= GetSymbol<Delegates.sk_maskfilter_new_clip> ("sk_maskfilter_new_clip")).Invoke (min, max);
		#endif

		// sk_maskfilter_t* sk_maskfilter_new_gamma(float gamma)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_gamma (Single gamma);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_maskfilter_t sk_maskfilter_new_gamma (Single gamma);
		}
		private static Delegates.sk_maskfilter_new_gamma sk_maskfilter_new_gamma_delegate;
		internal static sk_maskfilter_t sk_maskfilter_new_gamma (Single gamma) =>
			(sk_maskfilter_new_gamma_delegate ??= GetSymbol<Delegates.sk_maskfilter_new_gamma> ("sk_maskfilter_new_gamma")).Invoke (gamma);
		#endif

		// sk_maskfilter_t* sk_maskfilter_new_shader(sk_shader_t* cshader)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_shader (sk_shader_t cshader);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_maskfilter_t sk_maskfilter_new_shader (sk_shader_t cshader);
		}
		private static Delegates.sk_maskfilter_new_shader sk_maskfilter_new_shader_delegate;
		internal static sk_maskfilter_t sk_maskfilter_new_shader (sk_shader_t cshader) =>
			(sk_maskfilter_new_shader_delegate ??= GetSymbol<Delegates.sk_maskfilter_new_shader> ("sk_maskfilter_new_shader")).Invoke (cshader);
		#endif

		// sk_maskfilter_t* sk_maskfilter_new_table(const uint8_t[256] table = 256)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_maskfilter_new_table (Byte* table);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_maskfilter_t sk_maskfilter_new_table (Byte* table);
		}
		private static Delegates.sk_maskfilter_new_table sk_maskfilter_new_table_delegate;
		internal static sk_maskfilter_t sk_maskfilter_new_table (Byte* table) =>
			(sk_maskfilter_new_table_delegate ??= GetSymbol<Delegates.sk_maskfilter_new_table> ("sk_maskfilter_new_table")).Invoke (table);
		#endif

		// void sk_maskfilter_ref(sk_maskfilter_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_maskfilter_ref (sk_maskfilter_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_maskfilter_ref (sk_maskfilter_t param0);
		}
		private static Delegates.sk_maskfilter_ref sk_maskfilter_ref_delegate;
		internal static void sk_maskfilter_ref (sk_maskfilter_t param0) =>
			(sk_maskfilter_ref_delegate ??= GetSymbol<Delegates.sk_maskfilter_ref> ("sk_maskfilter_ref")).Invoke (param0);
		#endif

		// void sk_maskfilter_unref(sk_maskfilter_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_maskfilter_unref (sk_maskfilter_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_maskfilter_unref (sk_maskfilter_t param0);
		}
		private static Delegates.sk_maskfilter_unref sk_maskfilter_unref_delegate;
		internal static void sk_maskfilter_unref (sk_maskfilter_t param0) =>
			(sk_maskfilter_unref_delegate ??= GetSymbol<Delegates.sk_maskfilter_unref> ("sk_maskfilter_unref")).Invoke (param0);
		#endif

		#endregion

		#region sk_matrix.h

		// void sk_3dview_apply_to_canvas(sk_3dview_t* cview, sk_canvas_t* ccanvas)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_apply_to_canvas (sk_3dview_t cview, sk_canvas_t ccanvas);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_3dview_apply_to_canvas (sk_3dview_t cview, sk_canvas_t ccanvas);
		}
		private static Delegates.sk_3dview_apply_to_canvas sk_3dview_apply_to_canvas_delegate;
		internal static void sk_3dview_apply_to_canvas (sk_3dview_t cview, sk_canvas_t ccanvas) =>
			(sk_3dview_apply_to_canvas_delegate ??= GetSymbol<Delegates.sk_3dview_apply_to_canvas> ("sk_3dview_apply_to_canvas")).Invoke (cview, ccanvas);
		#endif

		// void sk_3dview_destroy(sk_3dview_t* cview)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_destroy (sk_3dview_t cview);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_3dview_destroy (sk_3dview_t cview);
		}
		private static Delegates.sk_3dview_destroy sk_3dview_destroy_delegate;
		internal static void sk_3dview_destroy (sk_3dview_t cview) =>
			(sk_3dview_destroy_delegate ??= GetSymbol<Delegates.sk_3dview_destroy> ("sk_3dview_destroy")).Invoke (cview);
		#endif

		// float sk_3dview_dot_with_normal(sk_3dview_t* cview, float dx, float dy, float dz)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_3dview_dot_with_normal (sk_3dview_t cview, Single dx, Single dy, Single dz);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_3dview_dot_with_normal (sk_3dview_t cview, Single dx, Single dy, Single dz);
		}
		private static Delegates.sk_3dview_dot_with_normal sk_3dview_dot_with_normal_delegate;
		internal static Single sk_3dview_dot_with_normal (sk_3dview_t cview, Single dx, Single dy, Single dz) =>
			(sk_3dview_dot_with_normal_delegate ??= GetSymbol<Delegates.sk_3dview_dot_with_normal> ("sk_3dview_dot_with_normal")).Invoke (cview, dx, dy, dz);
		#endif

		// void sk_3dview_get_matrix(sk_3dview_t* cview, sk_matrix_t* cmatrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_get_matrix (sk_3dview_t cview, SKMatrix* cmatrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_3dview_get_matrix (sk_3dview_t cview, SKMatrix* cmatrix);
		}
		private static Delegates.sk_3dview_get_matrix sk_3dview_get_matrix_delegate;
		internal static void sk_3dview_get_matrix (sk_3dview_t cview, SKMatrix* cmatrix) =>
			(sk_3dview_get_matrix_delegate ??= GetSymbol<Delegates.sk_3dview_get_matrix> ("sk_3dview_get_matrix")).Invoke (cview, cmatrix);
		#endif

		// sk_3dview_t* sk_3dview_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_3dview_t sk_3dview_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_3dview_t sk_3dview_new ();
		}
		private static Delegates.sk_3dview_new sk_3dview_new_delegate;
		internal static sk_3dview_t sk_3dview_new () =>
			(sk_3dview_new_delegate ??= GetSymbol<Delegates.sk_3dview_new> ("sk_3dview_new")).Invoke ();
		#endif

		// void sk_3dview_restore(sk_3dview_t* cview)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_restore (sk_3dview_t cview);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_3dview_restore (sk_3dview_t cview);
		}
		private static Delegates.sk_3dview_restore sk_3dview_restore_delegate;
		internal static void sk_3dview_restore (sk_3dview_t cview) =>
			(sk_3dview_restore_delegate ??= GetSymbol<Delegates.sk_3dview_restore> ("sk_3dview_restore")).Invoke (cview);
		#endif

		// void sk_3dview_rotate_x_degrees(sk_3dview_t* cview, float degrees)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_rotate_x_degrees (sk_3dview_t cview, Single degrees);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_3dview_rotate_x_degrees (sk_3dview_t cview, Single degrees);
		}
		private static Delegates.sk_3dview_rotate_x_degrees sk_3dview_rotate_x_degrees_delegate;
		internal static void sk_3dview_rotate_x_degrees (sk_3dview_t cview, Single degrees) =>
			(sk_3dview_rotate_x_degrees_delegate ??= GetSymbol<Delegates.sk_3dview_rotate_x_degrees> ("sk_3dview_rotate_x_degrees")).Invoke (cview, degrees);
		#endif

		// void sk_3dview_rotate_x_radians(sk_3dview_t* cview, float radians)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_rotate_x_radians (sk_3dview_t cview, Single radians);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_3dview_rotate_x_radians (sk_3dview_t cview, Single radians);
		}
		private static Delegates.sk_3dview_rotate_x_radians sk_3dview_rotate_x_radians_delegate;
		internal static void sk_3dview_rotate_x_radians (sk_3dview_t cview, Single radians) =>
			(sk_3dview_rotate_x_radians_delegate ??= GetSymbol<Delegates.sk_3dview_rotate_x_radians> ("sk_3dview_rotate_x_radians")).Invoke (cview, radians);
		#endif

		// void sk_3dview_rotate_y_degrees(sk_3dview_t* cview, float degrees)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_rotate_y_degrees (sk_3dview_t cview, Single degrees);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_3dview_rotate_y_degrees (sk_3dview_t cview, Single degrees);
		}
		private static Delegates.sk_3dview_rotate_y_degrees sk_3dview_rotate_y_degrees_delegate;
		internal static void sk_3dview_rotate_y_degrees (sk_3dview_t cview, Single degrees) =>
			(sk_3dview_rotate_y_degrees_delegate ??= GetSymbol<Delegates.sk_3dview_rotate_y_degrees> ("sk_3dview_rotate_y_degrees")).Invoke (cview, degrees);
		#endif

		// void sk_3dview_rotate_y_radians(sk_3dview_t* cview, float radians)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_rotate_y_radians (sk_3dview_t cview, Single radians);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_3dview_rotate_y_radians (sk_3dview_t cview, Single radians);
		}
		private static Delegates.sk_3dview_rotate_y_radians sk_3dview_rotate_y_radians_delegate;
		internal static void sk_3dview_rotate_y_radians (sk_3dview_t cview, Single radians) =>
			(sk_3dview_rotate_y_radians_delegate ??= GetSymbol<Delegates.sk_3dview_rotate_y_radians> ("sk_3dview_rotate_y_radians")).Invoke (cview, radians);
		#endif

		// void sk_3dview_rotate_z_degrees(sk_3dview_t* cview, float degrees)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_rotate_z_degrees (sk_3dview_t cview, Single degrees);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_3dview_rotate_z_degrees (sk_3dview_t cview, Single degrees);
		}
		private static Delegates.sk_3dview_rotate_z_degrees sk_3dview_rotate_z_degrees_delegate;
		internal static void sk_3dview_rotate_z_degrees (sk_3dview_t cview, Single degrees) =>
			(sk_3dview_rotate_z_degrees_delegate ??= GetSymbol<Delegates.sk_3dview_rotate_z_degrees> ("sk_3dview_rotate_z_degrees")).Invoke (cview, degrees);
		#endif

		// void sk_3dview_rotate_z_radians(sk_3dview_t* cview, float radians)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_rotate_z_radians (sk_3dview_t cview, Single radians);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_3dview_rotate_z_radians (sk_3dview_t cview, Single radians);
		}
		private static Delegates.sk_3dview_rotate_z_radians sk_3dview_rotate_z_radians_delegate;
		internal static void sk_3dview_rotate_z_radians (sk_3dview_t cview, Single radians) =>
			(sk_3dview_rotate_z_radians_delegate ??= GetSymbol<Delegates.sk_3dview_rotate_z_radians> ("sk_3dview_rotate_z_radians")).Invoke (cview, radians);
		#endif

		// void sk_3dview_save(sk_3dview_t* cview)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_save (sk_3dview_t cview);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_3dview_save (sk_3dview_t cview);
		}
		private static Delegates.sk_3dview_save sk_3dview_save_delegate;
		internal static void sk_3dview_save (sk_3dview_t cview) =>
			(sk_3dview_save_delegate ??= GetSymbol<Delegates.sk_3dview_save> ("sk_3dview_save")).Invoke (cview);
		#endif

		// void sk_3dview_translate(sk_3dview_t* cview, float x, float y, float z)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_3dview_translate (sk_3dview_t cview, Single x, Single y, Single z);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_3dview_translate (sk_3dview_t cview, Single x, Single y, Single z);
		}
		private static Delegates.sk_3dview_translate sk_3dview_translate_delegate;
		internal static void sk_3dview_translate (sk_3dview_t cview, Single x, Single y, Single z) =>
			(sk_3dview_translate_delegate ??= GetSymbol<Delegates.sk_3dview_translate> ("sk_3dview_translate")).Invoke (cview, x, y, z);
		#endif

		// void sk_matrix_concat(sk_matrix_t* result, sk_matrix_t* first, sk_matrix_t* second)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_concat (SKMatrix* result, SKMatrix* first, SKMatrix* second);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_concat (SKMatrix* result, SKMatrix* first, SKMatrix* second);
		}
		private static Delegates.sk_matrix_concat sk_matrix_concat_delegate;
		internal static void sk_matrix_concat (SKMatrix* result, SKMatrix* first, SKMatrix* second) =>
			(sk_matrix_concat_delegate ??= GetSymbol<Delegates.sk_matrix_concat> ("sk_matrix_concat")).Invoke (result, first, second);
		#endif

		// void sk_matrix_map_points(sk_matrix_t* matrix, sk_point_t* dst, sk_point_t* src, int count)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_points (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_map_points (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count);
		}
		private static Delegates.sk_matrix_map_points sk_matrix_map_points_delegate;
		internal static void sk_matrix_map_points (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count) =>
			(sk_matrix_map_points_delegate ??= GetSymbol<Delegates.sk_matrix_map_points> ("sk_matrix_map_points")).Invoke (matrix, dst, src, count);
		#endif

		// float sk_matrix_map_radius(sk_matrix_t* matrix, float radius)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_matrix_map_radius (SKMatrix* matrix, Single radius);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_matrix_map_radius (SKMatrix* matrix, Single radius);
		}
		private static Delegates.sk_matrix_map_radius sk_matrix_map_radius_delegate;
		internal static Single sk_matrix_map_radius (SKMatrix* matrix, Single radius) =>
			(sk_matrix_map_radius_delegate ??= GetSymbol<Delegates.sk_matrix_map_radius> ("sk_matrix_map_radius")).Invoke (matrix, radius);
		#endif

		// void sk_matrix_map_rect(sk_matrix_t* matrix, sk_rect_t* dest, sk_rect_t* source)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_rect (SKMatrix* matrix, SKRect* dest, SKRect* source);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_map_rect (SKMatrix* matrix, SKRect* dest, SKRect* source);
		}
		private static Delegates.sk_matrix_map_rect sk_matrix_map_rect_delegate;
		internal static void sk_matrix_map_rect (SKMatrix* matrix, SKRect* dest, SKRect* source) =>
			(sk_matrix_map_rect_delegate ??= GetSymbol<Delegates.sk_matrix_map_rect> ("sk_matrix_map_rect")).Invoke (matrix, dest, source);
		#endif

		// void sk_matrix_map_vector(sk_matrix_t* matrix, float x, float y, sk_point_t* result)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_vector (SKMatrix* matrix, Single x, Single y, SKPoint* result);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_map_vector (SKMatrix* matrix, Single x, Single y, SKPoint* result);
		}
		private static Delegates.sk_matrix_map_vector sk_matrix_map_vector_delegate;
		internal static void sk_matrix_map_vector (SKMatrix* matrix, Single x, Single y, SKPoint* result) =>
			(sk_matrix_map_vector_delegate ??= GetSymbol<Delegates.sk_matrix_map_vector> ("sk_matrix_map_vector")).Invoke (matrix, x, y, result);
		#endif

		// void sk_matrix_map_vectors(sk_matrix_t* matrix, sk_point_t* dst, sk_point_t* src, int count)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_vectors (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_map_vectors (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count);
		}
		private static Delegates.sk_matrix_map_vectors sk_matrix_map_vectors_delegate;
		internal static void sk_matrix_map_vectors (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count) =>
			(sk_matrix_map_vectors_delegate ??= GetSymbol<Delegates.sk_matrix_map_vectors> ("sk_matrix_map_vectors")).Invoke (matrix, dst, src, count);
		#endif

		// void sk_matrix_map_xy(sk_matrix_t* matrix, float x, float y, sk_point_t* result)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_xy (SKMatrix* matrix, Single x, Single y, SKPoint* result);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_map_xy (SKMatrix* matrix, Single x, Single y, SKPoint* result);
		}
		private static Delegates.sk_matrix_map_xy sk_matrix_map_xy_delegate;
		internal static void sk_matrix_map_xy (SKMatrix* matrix, Single x, Single y, SKPoint* result) =>
			(sk_matrix_map_xy_delegate ??= GetSymbol<Delegates.sk_matrix_map_xy> ("sk_matrix_map_xy")).Invoke (matrix, x, y, result);
		#endif

		// void sk_matrix_post_concat(sk_matrix_t* result, sk_matrix_t* matrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_post_concat (SKMatrix* result, SKMatrix* matrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_post_concat (SKMatrix* result, SKMatrix* matrix);
		}
		private static Delegates.sk_matrix_post_concat sk_matrix_post_concat_delegate;
		internal static void sk_matrix_post_concat (SKMatrix* result, SKMatrix* matrix) =>
			(sk_matrix_post_concat_delegate ??= GetSymbol<Delegates.sk_matrix_post_concat> ("sk_matrix_post_concat")).Invoke (result, matrix);
		#endif

		// void sk_matrix_pre_concat(sk_matrix_t* result, sk_matrix_t* matrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_pre_concat (SKMatrix* result, SKMatrix* matrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_pre_concat (SKMatrix* result, SKMatrix* matrix);
		}
		private static Delegates.sk_matrix_pre_concat sk_matrix_pre_concat_delegate;
		internal static void sk_matrix_pre_concat (SKMatrix* result, SKMatrix* matrix) =>
			(sk_matrix_pre_concat_delegate ??= GetSymbol<Delegates.sk_matrix_pre_concat> ("sk_matrix_pre_concat")).Invoke (result, matrix);
		#endif

		// bool sk_matrix_try_invert(sk_matrix_t* matrix, sk_matrix_t* result)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_matrix_try_invert (SKMatrix* matrix, SKMatrix* result);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_matrix_try_invert (SKMatrix* matrix, SKMatrix* result);
		}
		private static Delegates.sk_matrix_try_invert sk_matrix_try_invert_delegate;
		internal static bool sk_matrix_try_invert (SKMatrix* matrix, SKMatrix* result) =>
			(sk_matrix_try_invert_delegate ??= GetSymbol<Delegates.sk_matrix_try_invert> ("sk_matrix_try_invert")).Invoke (matrix, result);
		#endif

		// void sk_matrix44_as_col_major(sk_matrix44_t* matrix, float* dst)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_as_col_major (sk_matrix44_t matrix, Single* dst);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_as_col_major (sk_matrix44_t matrix, Single* dst);
		}
		private static Delegates.sk_matrix44_as_col_major sk_matrix44_as_col_major_delegate;
		internal static void sk_matrix44_as_col_major (sk_matrix44_t matrix, Single* dst) =>
			(sk_matrix44_as_col_major_delegate ??= GetSymbol<Delegates.sk_matrix44_as_col_major> ("sk_matrix44_as_col_major")).Invoke (matrix, dst);
		#endif

		// void sk_matrix44_as_row_major(sk_matrix44_t* matrix, float* dst)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_as_row_major (sk_matrix44_t matrix, Single* dst);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_as_row_major (sk_matrix44_t matrix, Single* dst);
		}
		private static Delegates.sk_matrix44_as_row_major sk_matrix44_as_row_major_delegate;
		internal static void sk_matrix44_as_row_major (sk_matrix44_t matrix, Single* dst) =>
			(sk_matrix44_as_row_major_delegate ??= GetSymbol<Delegates.sk_matrix44_as_row_major> ("sk_matrix44_as_row_major")).Invoke (matrix, dst);
		#endif

		// void sk_matrix44_destroy(sk_matrix44_t* matrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_destroy (sk_matrix44_t matrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_destroy (sk_matrix44_t matrix);
		}
		private static Delegates.sk_matrix44_destroy sk_matrix44_destroy_delegate;
		internal static void sk_matrix44_destroy (sk_matrix44_t matrix) =>
			(sk_matrix44_destroy_delegate ??= GetSymbol<Delegates.sk_matrix44_destroy> ("sk_matrix44_destroy")).Invoke (matrix);
		#endif

		// double sk_matrix44_determinant(sk_matrix44_t* matrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Double sk_matrix44_determinant (sk_matrix44_t matrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Double sk_matrix44_determinant (sk_matrix44_t matrix);
		}
		private static Delegates.sk_matrix44_determinant sk_matrix44_determinant_delegate;
		internal static Double sk_matrix44_determinant (sk_matrix44_t matrix) =>
			(sk_matrix44_determinant_delegate ??= GetSymbol<Delegates.sk_matrix44_determinant> ("sk_matrix44_determinant")).Invoke (matrix);
		#endif

		// bool sk_matrix44_equals(sk_matrix44_t* matrix, const sk_matrix44_t* other)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_matrix44_equals (sk_matrix44_t matrix, sk_matrix44_t other);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_matrix44_equals (sk_matrix44_t matrix, sk_matrix44_t other);
		}
		private static Delegates.sk_matrix44_equals sk_matrix44_equals_delegate;
		internal static bool sk_matrix44_equals (sk_matrix44_t matrix, sk_matrix44_t other) =>
			(sk_matrix44_equals_delegate ??= GetSymbol<Delegates.sk_matrix44_equals> ("sk_matrix44_equals")).Invoke (matrix, other);
		#endif

		// float sk_matrix44_get(sk_matrix44_t* matrix, int row, int col)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_matrix44_get (sk_matrix44_t matrix, Int32 row, Int32 col);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_matrix44_get (sk_matrix44_t matrix, Int32 row, Int32 col);
		}
		private static Delegates.sk_matrix44_get sk_matrix44_get_delegate;
		internal static Single sk_matrix44_get (sk_matrix44_t matrix, Int32 row, Int32 col) =>
			(sk_matrix44_get_delegate ??= GetSymbol<Delegates.sk_matrix44_get> ("sk_matrix44_get")).Invoke (matrix, row, col);
		#endif

		// sk_matrix44_type_mask_t sk_matrix44_get_type(sk_matrix44_t* matrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKMatrix44TypeMask sk_matrix44_get_type (sk_matrix44_t matrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKMatrix44TypeMask sk_matrix44_get_type (sk_matrix44_t matrix);
		}
		private static Delegates.sk_matrix44_get_type sk_matrix44_get_type_delegate;
		internal static SKMatrix44TypeMask sk_matrix44_get_type (sk_matrix44_t matrix) =>
			(sk_matrix44_get_type_delegate ??= GetSymbol<Delegates.sk_matrix44_get_type> ("sk_matrix44_get_type")).Invoke (matrix);
		#endif

		// bool sk_matrix44_invert(sk_matrix44_t* matrix, sk_matrix44_t* inverse)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_matrix44_invert (sk_matrix44_t matrix, sk_matrix44_t inverse);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_matrix44_invert (sk_matrix44_t matrix, sk_matrix44_t inverse);
		}
		private static Delegates.sk_matrix44_invert sk_matrix44_invert_delegate;
		internal static bool sk_matrix44_invert (sk_matrix44_t matrix, sk_matrix44_t inverse) =>
			(sk_matrix44_invert_delegate ??= GetSymbol<Delegates.sk_matrix44_invert> ("sk_matrix44_invert")).Invoke (matrix, inverse);
		#endif

		// void sk_matrix44_map_scalars(sk_matrix44_t* matrix, const float* src, float* dst)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_map_scalars (sk_matrix44_t matrix, Single* src, Single* dst);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_map_scalars (sk_matrix44_t matrix, Single* src, Single* dst);
		}
		private static Delegates.sk_matrix44_map_scalars sk_matrix44_map_scalars_delegate;
		internal static void sk_matrix44_map_scalars (sk_matrix44_t matrix, Single* src, Single* dst) =>
			(sk_matrix44_map_scalars_delegate ??= GetSymbol<Delegates.sk_matrix44_map_scalars> ("sk_matrix44_map_scalars")).Invoke (matrix, src, dst);
		#endif

		// void sk_matrix44_map2(sk_matrix44_t* matrix, const float* src2, int count, float* dst4)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_map2 (sk_matrix44_t matrix, Single* src2, Int32 count, Single* dst4);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_map2 (sk_matrix44_t matrix, Single* src2, Int32 count, Single* dst4);
		}
		private static Delegates.sk_matrix44_map2 sk_matrix44_map2_delegate;
		internal static void sk_matrix44_map2 (sk_matrix44_t matrix, Single* src2, Int32 count, Single* dst4) =>
			(sk_matrix44_map2_delegate ??= GetSymbol<Delegates.sk_matrix44_map2> ("sk_matrix44_map2")).Invoke (matrix, src2, count, dst4);
		#endif

		// sk_matrix44_t* sk_matrix44_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_matrix44_t sk_matrix44_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_matrix44_t sk_matrix44_new ();
		}
		private static Delegates.sk_matrix44_new sk_matrix44_new_delegate;
		internal static sk_matrix44_t sk_matrix44_new () =>
			(sk_matrix44_new_delegate ??= GetSymbol<Delegates.sk_matrix44_new> ("sk_matrix44_new")).Invoke ();
		#endif

		// sk_matrix44_t* sk_matrix44_new_concat(const sk_matrix44_t* a, const sk_matrix44_t* b)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_matrix44_t sk_matrix44_new_concat (sk_matrix44_t a, sk_matrix44_t b);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_matrix44_t sk_matrix44_new_concat (sk_matrix44_t a, sk_matrix44_t b);
		}
		private static Delegates.sk_matrix44_new_concat sk_matrix44_new_concat_delegate;
		internal static sk_matrix44_t sk_matrix44_new_concat (sk_matrix44_t a, sk_matrix44_t b) =>
			(sk_matrix44_new_concat_delegate ??= GetSymbol<Delegates.sk_matrix44_new_concat> ("sk_matrix44_new_concat")).Invoke (a, b);
		#endif

		// sk_matrix44_t* sk_matrix44_new_copy(const sk_matrix44_t* src)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_matrix44_t sk_matrix44_new_copy (sk_matrix44_t src);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_matrix44_t sk_matrix44_new_copy (sk_matrix44_t src);
		}
		private static Delegates.sk_matrix44_new_copy sk_matrix44_new_copy_delegate;
		internal static sk_matrix44_t sk_matrix44_new_copy (sk_matrix44_t src) =>
			(sk_matrix44_new_copy_delegate ??= GetSymbol<Delegates.sk_matrix44_new_copy> ("sk_matrix44_new_copy")).Invoke (src);
		#endif

		// sk_matrix44_t* sk_matrix44_new_identity()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_matrix44_t sk_matrix44_new_identity ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_matrix44_t sk_matrix44_new_identity ();
		}
		private static Delegates.sk_matrix44_new_identity sk_matrix44_new_identity_delegate;
		internal static sk_matrix44_t sk_matrix44_new_identity () =>
			(sk_matrix44_new_identity_delegate ??= GetSymbol<Delegates.sk_matrix44_new_identity> ("sk_matrix44_new_identity")).Invoke ();
		#endif

		// sk_matrix44_t* sk_matrix44_new_matrix(const sk_matrix_t* src)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_matrix44_t sk_matrix44_new_matrix (SKMatrix* src);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_matrix44_t sk_matrix44_new_matrix (SKMatrix* src);
		}
		private static Delegates.sk_matrix44_new_matrix sk_matrix44_new_matrix_delegate;
		internal static sk_matrix44_t sk_matrix44_new_matrix (SKMatrix* src) =>
			(sk_matrix44_new_matrix_delegate ??= GetSymbol<Delegates.sk_matrix44_new_matrix> ("sk_matrix44_new_matrix")).Invoke (src);
		#endif

		// void sk_matrix44_post_concat(sk_matrix44_t* matrix, const sk_matrix44_t* m)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_post_concat (sk_matrix44_t matrix, sk_matrix44_t m);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_post_concat (sk_matrix44_t matrix, sk_matrix44_t m);
		}
		private static Delegates.sk_matrix44_post_concat sk_matrix44_post_concat_delegate;
		internal static void sk_matrix44_post_concat (sk_matrix44_t matrix, sk_matrix44_t m) =>
			(sk_matrix44_post_concat_delegate ??= GetSymbol<Delegates.sk_matrix44_post_concat> ("sk_matrix44_post_concat")).Invoke (matrix, m);
		#endif

		// void sk_matrix44_post_scale(sk_matrix44_t* matrix, float sx, float sy, float sz)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_post_scale (sk_matrix44_t matrix, Single sx, Single sy, Single sz);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_post_scale (sk_matrix44_t matrix, Single sx, Single sy, Single sz);
		}
		private static Delegates.sk_matrix44_post_scale sk_matrix44_post_scale_delegate;
		internal static void sk_matrix44_post_scale (sk_matrix44_t matrix, Single sx, Single sy, Single sz) =>
			(sk_matrix44_post_scale_delegate ??= GetSymbol<Delegates.sk_matrix44_post_scale> ("sk_matrix44_post_scale")).Invoke (matrix, sx, sy, sz);
		#endif

		// void sk_matrix44_post_translate(sk_matrix44_t* matrix, float dx, float dy, float dz)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_post_translate (sk_matrix44_t matrix, Single dx, Single dy, Single dz);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_post_translate (sk_matrix44_t matrix, Single dx, Single dy, Single dz);
		}
		private static Delegates.sk_matrix44_post_translate sk_matrix44_post_translate_delegate;
		internal static void sk_matrix44_post_translate (sk_matrix44_t matrix, Single dx, Single dy, Single dz) =>
			(sk_matrix44_post_translate_delegate ??= GetSymbol<Delegates.sk_matrix44_post_translate> ("sk_matrix44_post_translate")).Invoke (matrix, dx, dy, dz);
		#endif

		// void sk_matrix44_pre_concat(sk_matrix44_t* matrix, const sk_matrix44_t* m)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_pre_concat (sk_matrix44_t matrix, sk_matrix44_t m);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_pre_concat (sk_matrix44_t matrix, sk_matrix44_t m);
		}
		private static Delegates.sk_matrix44_pre_concat sk_matrix44_pre_concat_delegate;
		internal static void sk_matrix44_pre_concat (sk_matrix44_t matrix, sk_matrix44_t m) =>
			(sk_matrix44_pre_concat_delegate ??= GetSymbol<Delegates.sk_matrix44_pre_concat> ("sk_matrix44_pre_concat")).Invoke (matrix, m);
		#endif

		// void sk_matrix44_pre_scale(sk_matrix44_t* matrix, float sx, float sy, float sz)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_pre_scale (sk_matrix44_t matrix, Single sx, Single sy, Single sz);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_pre_scale (sk_matrix44_t matrix, Single sx, Single sy, Single sz);
		}
		private static Delegates.sk_matrix44_pre_scale sk_matrix44_pre_scale_delegate;
		internal static void sk_matrix44_pre_scale (sk_matrix44_t matrix, Single sx, Single sy, Single sz) =>
			(sk_matrix44_pre_scale_delegate ??= GetSymbol<Delegates.sk_matrix44_pre_scale> ("sk_matrix44_pre_scale")).Invoke (matrix, sx, sy, sz);
		#endif

		// void sk_matrix44_pre_translate(sk_matrix44_t* matrix, float dx, float dy, float dz)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_pre_translate (sk_matrix44_t matrix, Single dx, Single dy, Single dz);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_pre_translate (sk_matrix44_t matrix, Single dx, Single dy, Single dz);
		}
		private static Delegates.sk_matrix44_pre_translate sk_matrix44_pre_translate_delegate;
		internal static void sk_matrix44_pre_translate (sk_matrix44_t matrix, Single dx, Single dy, Single dz) =>
			(sk_matrix44_pre_translate_delegate ??= GetSymbol<Delegates.sk_matrix44_pre_translate> ("sk_matrix44_pre_translate")).Invoke (matrix, dx, dy, dz);
		#endif

		// bool sk_matrix44_preserves_2d_axis_alignment(sk_matrix44_t* matrix, float epsilon)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_matrix44_preserves_2d_axis_alignment (sk_matrix44_t matrix, Single epsilon);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_matrix44_preserves_2d_axis_alignment (sk_matrix44_t matrix, Single epsilon);
		}
		private static Delegates.sk_matrix44_preserves_2d_axis_alignment sk_matrix44_preserves_2d_axis_alignment_delegate;
		internal static bool sk_matrix44_preserves_2d_axis_alignment (sk_matrix44_t matrix, Single epsilon) =>
			(sk_matrix44_preserves_2d_axis_alignment_delegate ??= GetSymbol<Delegates.sk_matrix44_preserves_2d_axis_alignment> ("sk_matrix44_preserves_2d_axis_alignment")).Invoke (matrix, epsilon);
		#endif

		// void sk_matrix44_set(sk_matrix44_t* matrix, int row, int col, float value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set (sk_matrix44_t matrix, Int32 row, Int32 col, Single value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_set (sk_matrix44_t matrix, Int32 row, Int32 col, Single value);
		}
		private static Delegates.sk_matrix44_set sk_matrix44_set_delegate;
		internal static void sk_matrix44_set (sk_matrix44_t matrix, Int32 row, Int32 col, Single value) =>
			(sk_matrix44_set_delegate ??= GetSymbol<Delegates.sk_matrix44_set> ("sk_matrix44_set")).Invoke (matrix, row, col, value);
		#endif

		// void sk_matrix44_set_3x3_row_major(sk_matrix44_t* matrix, float* dst)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_3x3_row_major (sk_matrix44_t matrix, Single* dst);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_set_3x3_row_major (sk_matrix44_t matrix, Single* dst);
		}
		private static Delegates.sk_matrix44_set_3x3_row_major sk_matrix44_set_3x3_row_major_delegate;
		internal static void sk_matrix44_set_3x3_row_major (sk_matrix44_t matrix, Single* dst) =>
			(sk_matrix44_set_3x3_row_major_delegate ??= GetSymbol<Delegates.sk_matrix44_set_3x3_row_major> ("sk_matrix44_set_3x3_row_major")).Invoke (matrix, dst);
		#endif

		// void sk_matrix44_set_col_major(sk_matrix44_t* matrix, float* dst)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_col_major (sk_matrix44_t matrix, Single* dst);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_set_col_major (sk_matrix44_t matrix, Single* dst);
		}
		private static Delegates.sk_matrix44_set_col_major sk_matrix44_set_col_major_delegate;
		internal static void sk_matrix44_set_col_major (sk_matrix44_t matrix, Single* dst) =>
			(sk_matrix44_set_col_major_delegate ??= GetSymbol<Delegates.sk_matrix44_set_col_major> ("sk_matrix44_set_col_major")).Invoke (matrix, dst);
		#endif

		// void sk_matrix44_set_concat(sk_matrix44_t* matrix, const sk_matrix44_t* a, const sk_matrix44_t* b)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_concat (sk_matrix44_t matrix, sk_matrix44_t a, sk_matrix44_t b);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_set_concat (sk_matrix44_t matrix, sk_matrix44_t a, sk_matrix44_t b);
		}
		private static Delegates.sk_matrix44_set_concat sk_matrix44_set_concat_delegate;
		internal static void sk_matrix44_set_concat (sk_matrix44_t matrix, sk_matrix44_t a, sk_matrix44_t b) =>
			(sk_matrix44_set_concat_delegate ??= GetSymbol<Delegates.sk_matrix44_set_concat> ("sk_matrix44_set_concat")).Invoke (matrix, a, b);
		#endif

		// void sk_matrix44_set_identity(sk_matrix44_t* matrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_identity (sk_matrix44_t matrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_set_identity (sk_matrix44_t matrix);
		}
		private static Delegates.sk_matrix44_set_identity sk_matrix44_set_identity_delegate;
		internal static void sk_matrix44_set_identity (sk_matrix44_t matrix) =>
			(sk_matrix44_set_identity_delegate ??= GetSymbol<Delegates.sk_matrix44_set_identity> ("sk_matrix44_set_identity")).Invoke (matrix);
		#endif

		// void sk_matrix44_set_rotate_about_degrees(sk_matrix44_t* matrix, float x, float y, float z, float degrees)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_rotate_about_degrees (sk_matrix44_t matrix, Single x, Single y, Single z, Single degrees);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_set_rotate_about_degrees (sk_matrix44_t matrix, Single x, Single y, Single z, Single degrees);
		}
		private static Delegates.sk_matrix44_set_rotate_about_degrees sk_matrix44_set_rotate_about_degrees_delegate;
		internal static void sk_matrix44_set_rotate_about_degrees (sk_matrix44_t matrix, Single x, Single y, Single z, Single degrees) =>
			(sk_matrix44_set_rotate_about_degrees_delegate ??= GetSymbol<Delegates.sk_matrix44_set_rotate_about_degrees> ("sk_matrix44_set_rotate_about_degrees")).Invoke (matrix, x, y, z, degrees);
		#endif

		// void sk_matrix44_set_rotate_about_radians(sk_matrix44_t* matrix, float x, float y, float z, float radians)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_rotate_about_radians (sk_matrix44_t matrix, Single x, Single y, Single z, Single radians);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_set_rotate_about_radians (sk_matrix44_t matrix, Single x, Single y, Single z, Single radians);
		}
		private static Delegates.sk_matrix44_set_rotate_about_radians sk_matrix44_set_rotate_about_radians_delegate;
		internal static void sk_matrix44_set_rotate_about_radians (sk_matrix44_t matrix, Single x, Single y, Single z, Single radians) =>
			(sk_matrix44_set_rotate_about_radians_delegate ??= GetSymbol<Delegates.sk_matrix44_set_rotate_about_radians> ("sk_matrix44_set_rotate_about_radians")).Invoke (matrix, x, y, z, radians);
		#endif

		// void sk_matrix44_set_rotate_about_radians_unit(sk_matrix44_t* matrix, float x, float y, float z, float radians)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_rotate_about_radians_unit (sk_matrix44_t matrix, Single x, Single y, Single z, Single radians);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_set_rotate_about_radians_unit (sk_matrix44_t matrix, Single x, Single y, Single z, Single radians);
		}
		private static Delegates.sk_matrix44_set_rotate_about_radians_unit sk_matrix44_set_rotate_about_radians_unit_delegate;
		internal static void sk_matrix44_set_rotate_about_radians_unit (sk_matrix44_t matrix, Single x, Single y, Single z, Single radians) =>
			(sk_matrix44_set_rotate_about_radians_unit_delegate ??= GetSymbol<Delegates.sk_matrix44_set_rotate_about_radians_unit> ("sk_matrix44_set_rotate_about_radians_unit")).Invoke (matrix, x, y, z, radians);
		#endif

		// void sk_matrix44_set_row_major(sk_matrix44_t* matrix, float* dst)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_row_major (sk_matrix44_t matrix, Single* dst);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_set_row_major (sk_matrix44_t matrix, Single* dst);
		}
		private static Delegates.sk_matrix44_set_row_major sk_matrix44_set_row_major_delegate;
		internal static void sk_matrix44_set_row_major (sk_matrix44_t matrix, Single* dst) =>
			(sk_matrix44_set_row_major_delegate ??= GetSymbol<Delegates.sk_matrix44_set_row_major> ("sk_matrix44_set_row_major")).Invoke (matrix, dst);
		#endif

		// void sk_matrix44_set_scale(sk_matrix44_t* matrix, float sx, float sy, float sz)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_scale (sk_matrix44_t matrix, Single sx, Single sy, Single sz);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_set_scale (sk_matrix44_t matrix, Single sx, Single sy, Single sz);
		}
		private static Delegates.sk_matrix44_set_scale sk_matrix44_set_scale_delegate;
		internal static void sk_matrix44_set_scale (sk_matrix44_t matrix, Single sx, Single sy, Single sz) =>
			(sk_matrix44_set_scale_delegate ??= GetSymbol<Delegates.sk_matrix44_set_scale> ("sk_matrix44_set_scale")).Invoke (matrix, sx, sy, sz);
		#endif

		// void sk_matrix44_set_translate(sk_matrix44_t* matrix, float dx, float dy, float dz)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_set_translate (sk_matrix44_t matrix, Single dx, Single dy, Single dz);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_set_translate (sk_matrix44_t matrix, Single dx, Single dy, Single dz);
		}
		private static Delegates.sk_matrix44_set_translate sk_matrix44_set_translate_delegate;
		internal static void sk_matrix44_set_translate (sk_matrix44_t matrix, Single dx, Single dy, Single dz) =>
			(sk_matrix44_set_translate_delegate ??= GetSymbol<Delegates.sk_matrix44_set_translate> ("sk_matrix44_set_translate")).Invoke (matrix, dx, dy, dz);
		#endif

		// void sk_matrix44_to_matrix(sk_matrix44_t* matrix, sk_matrix_t* dst)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_to_matrix (sk_matrix44_t matrix, SKMatrix* dst);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_to_matrix (sk_matrix44_t matrix, SKMatrix* dst);
		}
		private static Delegates.sk_matrix44_to_matrix sk_matrix44_to_matrix_delegate;
		internal static void sk_matrix44_to_matrix (sk_matrix44_t matrix, SKMatrix* dst) =>
			(sk_matrix44_to_matrix_delegate ??= GetSymbol<Delegates.sk_matrix44_to_matrix> ("sk_matrix44_to_matrix")).Invoke (matrix, dst);
		#endif

		// void sk_matrix44_transpose(sk_matrix44_t* matrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix44_transpose (sk_matrix44_t matrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix44_transpose (sk_matrix44_t matrix);
		}
		private static Delegates.sk_matrix44_transpose sk_matrix44_transpose_delegate;
		internal static void sk_matrix44_transpose (sk_matrix44_t matrix) =>
			(sk_matrix44_transpose_delegate ??= GetSymbol<Delegates.sk_matrix44_transpose> ("sk_matrix44_transpose")).Invoke (matrix);
		#endif

		#endregion

		#region sk_paint.h

		// sk_paint_t* sk_paint_clone(sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_paint_t sk_paint_clone (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_paint_t sk_paint_clone (sk_paint_t param0);
		}
		private static Delegates.sk_paint_clone sk_paint_clone_delegate;
		internal static sk_paint_t sk_paint_clone (sk_paint_t param0) =>
			(sk_paint_clone_delegate ??= GetSymbol<Delegates.sk_paint_clone> ("sk_paint_clone")).Invoke (param0);
		#endif

		// void sk_paint_delete(sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_delete (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_delete (sk_paint_t param0);
		}
		private static Delegates.sk_paint_delete sk_paint_delete_delegate;
		internal static void sk_paint_delete (sk_paint_t param0) =>
			(sk_paint_delete_delegate ??= GetSymbol<Delegates.sk_paint_delete> ("sk_paint_delete")).Invoke (param0);
		#endif

		// sk_blendmode_t sk_paint_get_blendmode(sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKBlendMode sk_paint_get_blendmode (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKBlendMode sk_paint_get_blendmode (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_blendmode sk_paint_get_blendmode_delegate;
		internal static SKBlendMode sk_paint_get_blendmode (sk_paint_t param0) =>
			(sk_paint_get_blendmode_delegate ??= GetSymbol<Delegates.sk_paint_get_blendmode> ("sk_paint_get_blendmode")).Invoke (param0);
		#endif

		// sk_color_t sk_paint_get_color(const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_paint_get_color (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_paint_get_color (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_color sk_paint_get_color_delegate;
		internal static UInt32 sk_paint_get_color (sk_paint_t param0) =>
			(sk_paint_get_color_delegate ??= GetSymbol<Delegates.sk_paint_get_color> ("sk_paint_get_color")).Invoke (param0);
		#endif

		// void sk_paint_get_color4f(const sk_paint_t* paint, sk_color4f_t* color)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_get_color4f (sk_paint_t paint, SKColorF* color);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_get_color4f (sk_paint_t paint, SKColorF* color);
		}
		private static Delegates.sk_paint_get_color4f sk_paint_get_color4f_delegate;
		internal static void sk_paint_get_color4f (sk_paint_t paint, SKColorF* color) =>
			(sk_paint_get_color4f_delegate ??= GetSymbol<Delegates.sk_paint_get_color4f> ("sk_paint_get_color4f")).Invoke (paint, color);
		#endif

		// sk_colorfilter_t* sk_paint_get_colorfilter(sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_paint_get_colorfilter (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_paint_get_colorfilter (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_colorfilter sk_paint_get_colorfilter_delegate;
		internal static sk_colorfilter_t sk_paint_get_colorfilter (sk_paint_t param0) =>
			(sk_paint_get_colorfilter_delegate ??= GetSymbol<Delegates.sk_paint_get_colorfilter> ("sk_paint_get_colorfilter")).Invoke (param0);
		#endif

		// bool sk_paint_get_fill_path(const sk_paint_t*, const sk_path_t* src, sk_path_t* dst, const sk_rect_t* cullRect, float resScale)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_get_fill_path (sk_paint_t param0, sk_path_t src, sk_path_t dst, SKRect* cullRect, Single resScale);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_paint_get_fill_path (sk_paint_t param0, sk_path_t src, sk_path_t dst, SKRect* cullRect, Single resScale);
		}
		private static Delegates.sk_paint_get_fill_path sk_paint_get_fill_path_delegate;
		internal static bool sk_paint_get_fill_path (sk_paint_t param0, sk_path_t src, sk_path_t dst, SKRect* cullRect, Single resScale) =>
			(sk_paint_get_fill_path_delegate ??= GetSymbol<Delegates.sk_paint_get_fill_path> ("sk_paint_get_fill_path")).Invoke (param0, src, dst, cullRect, resScale);
		#endif

		// sk_filter_quality_t sk_paint_get_filter_quality(sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKFilterQuality sk_paint_get_filter_quality (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKFilterQuality sk_paint_get_filter_quality (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_filter_quality sk_paint_get_filter_quality_delegate;
		internal static SKFilterQuality sk_paint_get_filter_quality (sk_paint_t param0) =>
			(sk_paint_get_filter_quality_delegate ??= GetSymbol<Delegates.sk_paint_get_filter_quality> ("sk_paint_get_filter_quality")).Invoke (param0);
		#endif

		// sk_imagefilter_t* sk_paint_get_imagefilter(sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_paint_get_imagefilter (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_paint_get_imagefilter (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_imagefilter sk_paint_get_imagefilter_delegate;
		internal static sk_imagefilter_t sk_paint_get_imagefilter (sk_paint_t param0) =>
			(sk_paint_get_imagefilter_delegate ??= GetSymbol<Delegates.sk_paint_get_imagefilter> ("sk_paint_get_imagefilter")).Invoke (param0);
		#endif

		// sk_maskfilter_t* sk_paint_get_maskfilter(sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_paint_get_maskfilter (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_maskfilter_t sk_paint_get_maskfilter (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_maskfilter sk_paint_get_maskfilter_delegate;
		internal static sk_maskfilter_t sk_paint_get_maskfilter (sk_paint_t param0) =>
			(sk_paint_get_maskfilter_delegate ??= GetSymbol<Delegates.sk_paint_get_maskfilter> ("sk_paint_get_maskfilter")).Invoke (param0);
		#endif

		// sk_path_effect_t* sk_paint_get_path_effect(sk_paint_t* cpaint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_paint_get_path_effect (sk_paint_t cpaint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_paint_get_path_effect (sk_paint_t cpaint);
		}
		private static Delegates.sk_paint_get_path_effect sk_paint_get_path_effect_delegate;
		internal static sk_path_effect_t sk_paint_get_path_effect (sk_paint_t cpaint) =>
			(sk_paint_get_path_effect_delegate ??= GetSymbol<Delegates.sk_paint_get_path_effect> ("sk_paint_get_path_effect")).Invoke (cpaint);
		#endif

		// sk_shader_t* sk_paint_get_shader(sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_paint_get_shader (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_paint_get_shader (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_shader sk_paint_get_shader_delegate;
		internal static sk_shader_t sk_paint_get_shader (sk_paint_t param0) =>
			(sk_paint_get_shader_delegate ??= GetSymbol<Delegates.sk_paint_get_shader> ("sk_paint_get_shader")).Invoke (param0);
		#endif

		// sk_stroke_cap_t sk_paint_get_stroke_cap(const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKStrokeCap sk_paint_get_stroke_cap (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKStrokeCap sk_paint_get_stroke_cap (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_stroke_cap sk_paint_get_stroke_cap_delegate;
		internal static SKStrokeCap sk_paint_get_stroke_cap (sk_paint_t param0) =>
			(sk_paint_get_stroke_cap_delegate ??= GetSymbol<Delegates.sk_paint_get_stroke_cap> ("sk_paint_get_stroke_cap")).Invoke (param0);
		#endif

		// sk_stroke_join_t sk_paint_get_stroke_join(const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKStrokeJoin sk_paint_get_stroke_join (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKStrokeJoin sk_paint_get_stroke_join (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_stroke_join sk_paint_get_stroke_join_delegate;
		internal static SKStrokeJoin sk_paint_get_stroke_join (sk_paint_t param0) =>
			(sk_paint_get_stroke_join_delegate ??= GetSymbol<Delegates.sk_paint_get_stroke_join> ("sk_paint_get_stroke_join")).Invoke (param0);
		#endif

		// float sk_paint_get_stroke_miter(const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_paint_get_stroke_miter (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_paint_get_stroke_miter (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_stroke_miter sk_paint_get_stroke_miter_delegate;
		internal static Single sk_paint_get_stroke_miter (sk_paint_t param0) =>
			(sk_paint_get_stroke_miter_delegate ??= GetSymbol<Delegates.sk_paint_get_stroke_miter> ("sk_paint_get_stroke_miter")).Invoke (param0);
		#endif

		// float sk_paint_get_stroke_width(const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_paint_get_stroke_width (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_paint_get_stroke_width (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_stroke_width sk_paint_get_stroke_width_delegate;
		internal static Single sk_paint_get_stroke_width (sk_paint_t param0) =>
			(sk_paint_get_stroke_width_delegate ??= GetSymbol<Delegates.sk_paint_get_stroke_width> ("sk_paint_get_stroke_width")).Invoke (param0);
		#endif

		// sk_paint_style_t sk_paint_get_style(const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPaintStyle sk_paint_get_style (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKPaintStyle sk_paint_get_style (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_style sk_paint_get_style_delegate;
		internal static SKPaintStyle sk_paint_get_style (sk_paint_t param0) =>
			(sk_paint_get_style_delegate ??= GetSymbol<Delegates.sk_paint_get_style> ("sk_paint_get_style")).Invoke (param0);
		#endif

		// bool sk_paint_is_antialias(const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_is_antialias (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_paint_is_antialias (sk_paint_t param0);
		}
		private static Delegates.sk_paint_is_antialias sk_paint_is_antialias_delegate;
		internal static bool sk_paint_is_antialias (sk_paint_t param0) =>
			(sk_paint_is_antialias_delegate ??= GetSymbol<Delegates.sk_paint_is_antialias> ("sk_paint_is_antialias")).Invoke (param0);
		#endif

		// bool sk_paint_is_dither(const sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_is_dither (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_paint_is_dither (sk_paint_t param0);
		}
		private static Delegates.sk_paint_is_dither sk_paint_is_dither_delegate;
		internal static bool sk_paint_is_dither (sk_paint_t param0) =>
			(sk_paint_is_dither_delegate ??= GetSymbol<Delegates.sk_paint_is_dither> ("sk_paint_is_dither")).Invoke (param0);
		#endif

		// sk_paint_t* sk_paint_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_paint_t sk_paint_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_paint_t sk_paint_new ();
		}
		private static Delegates.sk_paint_new sk_paint_new_delegate;
		internal static sk_paint_t sk_paint_new () =>
			(sk_paint_new_delegate ??= GetSymbol<Delegates.sk_paint_new> ("sk_paint_new")).Invoke ();
		#endif

		// void sk_paint_reset(sk_paint_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_reset (sk_paint_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_reset (sk_paint_t param0);
		}
		private static Delegates.sk_paint_reset sk_paint_reset_delegate;
		internal static void sk_paint_reset (sk_paint_t param0) =>
			(sk_paint_reset_delegate ??= GetSymbol<Delegates.sk_paint_reset> ("sk_paint_reset")).Invoke (param0);
		#endif

		// void sk_paint_set_antialias(sk_paint_t*, bool)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_antialias (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_antialias (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);
		}
		private static Delegates.sk_paint_set_antialias sk_paint_set_antialias_delegate;
		internal static void sk_paint_set_antialias (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1) =>
			(sk_paint_set_antialias_delegate ??= GetSymbol<Delegates.sk_paint_set_antialias> ("sk_paint_set_antialias")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_blendmode(sk_paint_t*, sk_blendmode_t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_blendmode (sk_paint_t param0, SKBlendMode param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_blendmode (sk_paint_t param0, SKBlendMode param1);
		}
		private static Delegates.sk_paint_set_blendmode sk_paint_set_blendmode_delegate;
		internal static void sk_paint_set_blendmode (sk_paint_t param0, SKBlendMode param1) =>
			(sk_paint_set_blendmode_delegate ??= GetSymbol<Delegates.sk_paint_set_blendmode> ("sk_paint_set_blendmode")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_color(sk_paint_t*, sk_color_t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_color (sk_paint_t param0, UInt32 param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_color (sk_paint_t param0, UInt32 param1);
		}
		private static Delegates.sk_paint_set_color sk_paint_set_color_delegate;
		internal static void sk_paint_set_color (sk_paint_t param0, UInt32 param1) =>
			(sk_paint_set_color_delegate ??= GetSymbol<Delegates.sk_paint_set_color> ("sk_paint_set_color")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_color4f(sk_paint_t* paint, sk_color4f_t* color, sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_color4f (sk_paint_t paint, SKColorF* color, sk_colorspace_t colorspace);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_color4f (sk_paint_t paint, SKColorF* color, sk_colorspace_t colorspace);
		}
		private static Delegates.sk_paint_set_color4f sk_paint_set_color4f_delegate;
		internal static void sk_paint_set_color4f (sk_paint_t paint, SKColorF* color, sk_colorspace_t colorspace) =>
			(sk_paint_set_color4f_delegate ??= GetSymbol<Delegates.sk_paint_set_color4f> ("sk_paint_set_color4f")).Invoke (paint, color, colorspace);
		#endif

		// void sk_paint_set_colorfilter(sk_paint_t*, sk_colorfilter_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_colorfilter (sk_paint_t param0, sk_colorfilter_t param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_colorfilter (sk_paint_t param0, sk_colorfilter_t param1);
		}
		private static Delegates.sk_paint_set_colorfilter sk_paint_set_colorfilter_delegate;
		internal static void sk_paint_set_colorfilter (sk_paint_t param0, sk_colorfilter_t param1) =>
			(sk_paint_set_colorfilter_delegate ??= GetSymbol<Delegates.sk_paint_set_colorfilter> ("sk_paint_set_colorfilter")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_dither(sk_paint_t*, bool)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_dither (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_dither (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);
		}
		private static Delegates.sk_paint_set_dither sk_paint_set_dither_delegate;
		internal static void sk_paint_set_dither (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1) =>
			(sk_paint_set_dither_delegate ??= GetSymbol<Delegates.sk_paint_set_dither> ("sk_paint_set_dither")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_filter_quality(sk_paint_t*, sk_filter_quality_t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_filter_quality (sk_paint_t param0, SKFilterQuality param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_filter_quality (sk_paint_t param0, SKFilterQuality param1);
		}
		private static Delegates.sk_paint_set_filter_quality sk_paint_set_filter_quality_delegate;
		internal static void sk_paint_set_filter_quality (sk_paint_t param0, SKFilterQuality param1) =>
			(sk_paint_set_filter_quality_delegate ??= GetSymbol<Delegates.sk_paint_set_filter_quality> ("sk_paint_set_filter_quality")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_imagefilter(sk_paint_t*, sk_imagefilter_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_imagefilter (sk_paint_t param0, sk_imagefilter_t param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_imagefilter (sk_paint_t param0, sk_imagefilter_t param1);
		}
		private static Delegates.sk_paint_set_imagefilter sk_paint_set_imagefilter_delegate;
		internal static void sk_paint_set_imagefilter (sk_paint_t param0, sk_imagefilter_t param1) =>
			(sk_paint_set_imagefilter_delegate ??= GetSymbol<Delegates.sk_paint_set_imagefilter> ("sk_paint_set_imagefilter")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_maskfilter(sk_paint_t*, sk_maskfilter_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_maskfilter (sk_paint_t param0, sk_maskfilter_t param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_maskfilter (sk_paint_t param0, sk_maskfilter_t param1);
		}
		private static Delegates.sk_paint_set_maskfilter sk_paint_set_maskfilter_delegate;
		internal static void sk_paint_set_maskfilter (sk_paint_t param0, sk_maskfilter_t param1) =>
			(sk_paint_set_maskfilter_delegate ??= GetSymbol<Delegates.sk_paint_set_maskfilter> ("sk_paint_set_maskfilter")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_path_effect(sk_paint_t* cpaint, sk_path_effect_t* effect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_path_effect (sk_paint_t cpaint, sk_path_effect_t effect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_path_effect (sk_paint_t cpaint, sk_path_effect_t effect);
		}
		private static Delegates.sk_paint_set_path_effect sk_paint_set_path_effect_delegate;
		internal static void sk_paint_set_path_effect (sk_paint_t cpaint, sk_path_effect_t effect) =>
			(sk_paint_set_path_effect_delegate ??= GetSymbol<Delegates.sk_paint_set_path_effect> ("sk_paint_set_path_effect")).Invoke (cpaint, effect);
		#endif

		// void sk_paint_set_shader(sk_paint_t*, sk_shader_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_shader (sk_paint_t param0, sk_shader_t param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_shader (sk_paint_t param0, sk_shader_t param1);
		}
		private static Delegates.sk_paint_set_shader sk_paint_set_shader_delegate;
		internal static void sk_paint_set_shader (sk_paint_t param0, sk_shader_t param1) =>
			(sk_paint_set_shader_delegate ??= GetSymbol<Delegates.sk_paint_set_shader> ("sk_paint_set_shader")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_stroke_cap(sk_paint_t*, sk_stroke_cap_t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_stroke_cap (sk_paint_t param0, SKStrokeCap param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_stroke_cap (sk_paint_t param0, SKStrokeCap param1);
		}
		private static Delegates.sk_paint_set_stroke_cap sk_paint_set_stroke_cap_delegate;
		internal static void sk_paint_set_stroke_cap (sk_paint_t param0, SKStrokeCap param1) =>
			(sk_paint_set_stroke_cap_delegate ??= GetSymbol<Delegates.sk_paint_set_stroke_cap> ("sk_paint_set_stroke_cap")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_stroke_join(sk_paint_t*, sk_stroke_join_t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_stroke_join (sk_paint_t param0, SKStrokeJoin param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_stroke_join (sk_paint_t param0, SKStrokeJoin param1);
		}
		private static Delegates.sk_paint_set_stroke_join sk_paint_set_stroke_join_delegate;
		internal static void sk_paint_set_stroke_join (sk_paint_t param0, SKStrokeJoin param1) =>
			(sk_paint_set_stroke_join_delegate ??= GetSymbol<Delegates.sk_paint_set_stroke_join> ("sk_paint_set_stroke_join")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_stroke_miter(sk_paint_t*, float miter)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_stroke_miter (sk_paint_t param0, Single miter);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_stroke_miter (sk_paint_t param0, Single miter);
		}
		private static Delegates.sk_paint_set_stroke_miter sk_paint_set_stroke_miter_delegate;
		internal static void sk_paint_set_stroke_miter (sk_paint_t param0, Single miter) =>
			(sk_paint_set_stroke_miter_delegate ??= GetSymbol<Delegates.sk_paint_set_stroke_miter> ("sk_paint_set_stroke_miter")).Invoke (param0, miter);
		#endif

		// void sk_paint_set_stroke_width(sk_paint_t*, float width)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_stroke_width (sk_paint_t param0, Single width);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_stroke_width (sk_paint_t param0, Single width);
		}
		private static Delegates.sk_paint_set_stroke_width sk_paint_set_stroke_width_delegate;
		internal static void sk_paint_set_stroke_width (sk_paint_t param0, Single width) =>
			(sk_paint_set_stroke_width_delegate ??= GetSymbol<Delegates.sk_paint_set_stroke_width> ("sk_paint_set_stroke_width")).Invoke (param0, width);
		#endif

		// void sk_paint_set_style(sk_paint_t*, sk_paint_style_t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_style (sk_paint_t param0, SKPaintStyle param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_style (sk_paint_t param0, SKPaintStyle param1);
		}
		private static Delegates.sk_paint_set_style sk_paint_set_style_delegate;
		internal static void sk_paint_set_style (sk_paint_t param0, SKPaintStyle param1) =>
			(sk_paint_set_style_delegate ??= GetSymbol<Delegates.sk_paint_set_style> ("sk_paint_set_style")).Invoke (param0, param1);
		#endif

		#endregion

		#region sk_path.h

		// void sk_opbuilder_add(sk_opbuilder_t* builder, const sk_path_t* path, sk_pathop_t op)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_opbuilder_add (sk_opbuilder_t builder, sk_path_t path, SKPathOp op);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_opbuilder_add (sk_opbuilder_t builder, sk_path_t path, SKPathOp op);
		}
		private static Delegates.sk_opbuilder_add sk_opbuilder_add_delegate;
		internal static void sk_opbuilder_add (sk_opbuilder_t builder, sk_path_t path, SKPathOp op) =>
			(sk_opbuilder_add_delegate ??= GetSymbol<Delegates.sk_opbuilder_add> ("sk_opbuilder_add")).Invoke (builder, path, op);
		#endif

		// void sk_opbuilder_destroy(sk_opbuilder_t* builder)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_opbuilder_destroy (sk_opbuilder_t builder);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_opbuilder_destroy (sk_opbuilder_t builder);
		}
		private static Delegates.sk_opbuilder_destroy sk_opbuilder_destroy_delegate;
		internal static void sk_opbuilder_destroy (sk_opbuilder_t builder) =>
			(sk_opbuilder_destroy_delegate ??= GetSymbol<Delegates.sk_opbuilder_destroy> ("sk_opbuilder_destroy")).Invoke (builder);
		#endif

		// sk_opbuilder_t* sk_opbuilder_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_opbuilder_t sk_opbuilder_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_opbuilder_t sk_opbuilder_new ();
		}
		private static Delegates.sk_opbuilder_new sk_opbuilder_new_delegate;
		internal static sk_opbuilder_t sk_opbuilder_new () =>
			(sk_opbuilder_new_delegate ??= GetSymbol<Delegates.sk_opbuilder_new> ("sk_opbuilder_new")).Invoke ();
		#endif

		// bool sk_opbuilder_resolve(sk_opbuilder_t* builder, sk_path_t* result)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_opbuilder_resolve (sk_opbuilder_t builder, sk_path_t result);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_opbuilder_resolve (sk_opbuilder_t builder, sk_path_t result);
		}
		private static Delegates.sk_opbuilder_resolve sk_opbuilder_resolve_delegate;
		internal static bool sk_opbuilder_resolve (sk_opbuilder_t builder, sk_path_t result) =>
			(sk_opbuilder_resolve_delegate ??= GetSymbol<Delegates.sk_opbuilder_resolve> ("sk_opbuilder_resolve")).Invoke (builder, result);
		#endif

		// void sk_path_add_arc(sk_path_t* cpath, const sk_rect_t* crect, float startAngle, float sweepAngle)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_arc (sk_path_t cpath, SKRect* crect, Single startAngle, Single sweepAngle);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_add_arc (sk_path_t cpath, SKRect* crect, Single startAngle, Single sweepAngle);
		}
		private static Delegates.sk_path_add_arc sk_path_add_arc_delegate;
		internal static void sk_path_add_arc (sk_path_t cpath, SKRect* crect, Single startAngle, Single sweepAngle) =>
			(sk_path_add_arc_delegate ??= GetSymbol<Delegates.sk_path_add_arc> ("sk_path_add_arc")).Invoke (cpath, crect, startAngle, sweepAngle);
		#endif

		// void sk_path_add_circle(sk_path_t*, float x, float y, float radius, sk_path_direction_t dir)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_circle (sk_path_t param0, Single x, Single y, Single radius, SKPathDirection dir);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_add_circle (sk_path_t param0, Single x, Single y, Single radius, SKPathDirection dir);
		}
		private static Delegates.sk_path_add_circle sk_path_add_circle_delegate;
		internal static void sk_path_add_circle (sk_path_t param0, Single x, Single y, Single radius, SKPathDirection dir) =>
			(sk_path_add_circle_delegate ??= GetSymbol<Delegates.sk_path_add_circle> ("sk_path_add_circle")).Invoke (param0, x, y, radius, dir);
		#endif

		// void sk_path_add_oval(sk_path_t*, const sk_rect_t*, sk_path_direction_t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_oval (sk_path_t param0, SKRect* param1, SKPathDirection param2);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_add_oval (sk_path_t param0, SKRect* param1, SKPathDirection param2);
		}
		private static Delegates.sk_path_add_oval sk_path_add_oval_delegate;
		internal static void sk_path_add_oval (sk_path_t param0, SKRect* param1, SKPathDirection param2) =>
			(sk_path_add_oval_delegate ??= GetSymbol<Delegates.sk_path_add_oval> ("sk_path_add_oval")).Invoke (param0, param1, param2);
		#endif

		// void sk_path_add_path(sk_path_t* cpath, sk_path_t* other, sk_path_add_mode_t add_mode)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_path (sk_path_t cpath, sk_path_t other, SKPathAddMode add_mode);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_add_path (sk_path_t cpath, sk_path_t other, SKPathAddMode add_mode);
		}
		private static Delegates.sk_path_add_path sk_path_add_path_delegate;
		internal static void sk_path_add_path (sk_path_t cpath, sk_path_t other, SKPathAddMode add_mode) =>
			(sk_path_add_path_delegate ??= GetSymbol<Delegates.sk_path_add_path> ("sk_path_add_path")).Invoke (cpath, other, add_mode);
		#endif

		// void sk_path_add_path_matrix(sk_path_t* cpath, sk_path_t* other, sk_matrix_t* matrix, sk_path_add_mode_t add_mode)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_path_matrix (sk_path_t cpath, sk_path_t other, SKMatrix* matrix, SKPathAddMode add_mode);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_add_path_matrix (sk_path_t cpath, sk_path_t other, SKMatrix* matrix, SKPathAddMode add_mode);
		}
		private static Delegates.sk_path_add_path_matrix sk_path_add_path_matrix_delegate;
		internal static void sk_path_add_path_matrix (sk_path_t cpath, sk_path_t other, SKMatrix* matrix, SKPathAddMode add_mode) =>
			(sk_path_add_path_matrix_delegate ??= GetSymbol<Delegates.sk_path_add_path_matrix> ("sk_path_add_path_matrix")).Invoke (cpath, other, matrix, add_mode);
		#endif

		// void sk_path_add_path_offset(sk_path_t* cpath, sk_path_t* other, float dx, float dy, sk_path_add_mode_t add_mode)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_path_offset (sk_path_t cpath, sk_path_t other, Single dx, Single dy, SKPathAddMode add_mode);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_add_path_offset (sk_path_t cpath, sk_path_t other, Single dx, Single dy, SKPathAddMode add_mode);
		}
		private static Delegates.sk_path_add_path_offset sk_path_add_path_offset_delegate;
		internal static void sk_path_add_path_offset (sk_path_t cpath, sk_path_t other, Single dx, Single dy, SKPathAddMode add_mode) =>
			(sk_path_add_path_offset_delegate ??= GetSymbol<Delegates.sk_path_add_path_offset> ("sk_path_add_path_offset")).Invoke (cpath, other, dx, dy, add_mode);
		#endif

		// void sk_path_add_path_reverse(sk_path_t* cpath, sk_path_t* other)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_path_reverse (sk_path_t cpath, sk_path_t other);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_add_path_reverse (sk_path_t cpath, sk_path_t other);
		}
		private static Delegates.sk_path_add_path_reverse sk_path_add_path_reverse_delegate;
		internal static void sk_path_add_path_reverse (sk_path_t cpath, sk_path_t other) =>
			(sk_path_add_path_reverse_delegate ??= GetSymbol<Delegates.sk_path_add_path_reverse> ("sk_path_add_path_reverse")).Invoke (cpath, other);
		#endif

		// void sk_path_add_poly(sk_path_t* cpath, const sk_point_t* points, int count, bool close)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_poly (sk_path_t cpath, SKPoint* points, Int32 count, [MarshalAs (UnmanagedType.I1)] bool close);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_add_poly (sk_path_t cpath, SKPoint* points, Int32 count, [MarshalAs (UnmanagedType.I1)] bool close);
		}
		private static Delegates.sk_path_add_poly sk_path_add_poly_delegate;
		internal static void sk_path_add_poly (sk_path_t cpath, SKPoint* points, Int32 count, [MarshalAs (UnmanagedType.I1)] bool close) =>
			(sk_path_add_poly_delegate ??= GetSymbol<Delegates.sk_path_add_poly> ("sk_path_add_poly")).Invoke (cpath, points, count, close);
		#endif

		// void sk_path_add_rect(sk_path_t*, const sk_rect_t*, sk_path_direction_t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_rect (sk_path_t param0, SKRect* param1, SKPathDirection param2);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_add_rect (sk_path_t param0, SKRect* param1, SKPathDirection param2);
		}
		private static Delegates.sk_path_add_rect sk_path_add_rect_delegate;
		internal static void sk_path_add_rect (sk_path_t param0, SKRect* param1, SKPathDirection param2) =>
			(sk_path_add_rect_delegate ??= GetSymbol<Delegates.sk_path_add_rect> ("sk_path_add_rect")).Invoke (param0, param1, param2);
		#endif

		// void sk_path_add_rect_start(sk_path_t* cpath, const sk_rect_t* crect, sk_path_direction_t cdir, uint32_t startIndex)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_rect_start (sk_path_t cpath, SKRect* crect, SKPathDirection cdir, UInt32 startIndex);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_add_rect_start (sk_path_t cpath, SKRect* crect, SKPathDirection cdir, UInt32 startIndex);
		}
		private static Delegates.sk_path_add_rect_start sk_path_add_rect_start_delegate;
		internal static void sk_path_add_rect_start (sk_path_t cpath, SKRect* crect, SKPathDirection cdir, UInt32 startIndex) =>
			(sk_path_add_rect_start_delegate ??= GetSymbol<Delegates.sk_path_add_rect_start> ("sk_path_add_rect_start")).Invoke (cpath, crect, cdir, startIndex);
		#endif

		// void sk_path_add_rounded_rect(sk_path_t*, const sk_rect_t*, float, float, sk_path_direction_t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_rounded_rect (sk_path_t param0, SKRect* param1, Single param2, Single param3, SKPathDirection param4);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_add_rounded_rect (sk_path_t param0, SKRect* param1, Single param2, Single param3, SKPathDirection param4);
		}
		private static Delegates.sk_path_add_rounded_rect sk_path_add_rounded_rect_delegate;
		internal static void sk_path_add_rounded_rect (sk_path_t param0, SKRect* param1, Single param2, Single param3, SKPathDirection param4) =>
			(sk_path_add_rounded_rect_delegate ??= GetSymbol<Delegates.sk_path_add_rounded_rect> ("sk_path_add_rounded_rect")).Invoke (param0, param1, param2, param3, param4);
		#endif

		// void sk_path_add_rrect(sk_path_t*, const sk_rrect_t*, sk_path_direction_t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_rrect (sk_path_t param0, sk_rrect_t param1, SKPathDirection param2);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_add_rrect (sk_path_t param0, sk_rrect_t param1, SKPathDirection param2);
		}
		private static Delegates.sk_path_add_rrect sk_path_add_rrect_delegate;
		internal static void sk_path_add_rrect (sk_path_t param0, sk_rrect_t param1, SKPathDirection param2) =>
			(sk_path_add_rrect_delegate ??= GetSymbol<Delegates.sk_path_add_rrect> ("sk_path_add_rrect")).Invoke (param0, param1, param2);
		#endif

		// void sk_path_add_rrect_start(sk_path_t*, const sk_rrect_t*, sk_path_direction_t, uint32_t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_add_rrect_start (sk_path_t param0, sk_rrect_t param1, SKPathDirection param2, UInt32 param3);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_add_rrect_start (sk_path_t param0, sk_rrect_t param1, SKPathDirection param2, UInt32 param3);
		}
		private static Delegates.sk_path_add_rrect_start sk_path_add_rrect_start_delegate;
		internal static void sk_path_add_rrect_start (sk_path_t param0, sk_rrect_t param1, SKPathDirection param2, UInt32 param3) =>
			(sk_path_add_rrect_start_delegate ??= GetSymbol<Delegates.sk_path_add_rrect_start> ("sk_path_add_rrect_start")).Invoke (param0, param1, param2, param3);
		#endif

		// void sk_path_arc_to(sk_path_t*, float rx, float ry, float xAxisRotate, sk_path_arc_size_t largeArc, sk_path_direction_t sweep, float x, float y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_arc_to (sk_path_t param0, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_arc_to (sk_path_t param0, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y);
		}
		private static Delegates.sk_path_arc_to sk_path_arc_to_delegate;
		internal static void sk_path_arc_to (sk_path_t param0, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y) =>
			(sk_path_arc_to_delegate ??= GetSymbol<Delegates.sk_path_arc_to> ("sk_path_arc_to")).Invoke (param0, rx, ry, xAxisRotate, largeArc, sweep, x, y);
		#endif

		// void sk_path_arc_to_with_oval(sk_path_t*, const sk_rect_t* oval, float startAngle, float sweepAngle, bool forceMoveTo)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_arc_to_with_oval (sk_path_t param0, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool forceMoveTo);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_arc_to_with_oval (sk_path_t param0, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool forceMoveTo);
		}
		private static Delegates.sk_path_arc_to_with_oval sk_path_arc_to_with_oval_delegate;
		internal static void sk_path_arc_to_with_oval (sk_path_t param0, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool forceMoveTo) =>
			(sk_path_arc_to_with_oval_delegate ??= GetSymbol<Delegates.sk_path_arc_to_with_oval> ("sk_path_arc_to_with_oval")).Invoke (param0, oval, startAngle, sweepAngle, forceMoveTo);
		#endif

		// void sk_path_arc_to_with_points(sk_path_t*, float x1, float y1, float x2, float y2, float radius)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_arc_to_with_points (sk_path_t param0, Single x1, Single y1, Single x2, Single y2, Single radius);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_arc_to_with_points (sk_path_t param0, Single x1, Single y1, Single x2, Single y2, Single radius);
		}
		private static Delegates.sk_path_arc_to_with_points sk_path_arc_to_with_points_delegate;
		internal static void sk_path_arc_to_with_points (sk_path_t param0, Single x1, Single y1, Single x2, Single y2, Single radius) =>
			(sk_path_arc_to_with_points_delegate ??= GetSymbol<Delegates.sk_path_arc_to_with_points> ("sk_path_arc_to_with_points")).Invoke (param0, x1, y1, x2, y2, radius);
		#endif

		// sk_path_t* sk_path_clone(const sk_path_t* cpath)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_t sk_path_clone (sk_path_t cpath);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_t sk_path_clone (sk_path_t cpath);
		}
		private static Delegates.sk_path_clone sk_path_clone_delegate;
		internal static sk_path_t sk_path_clone (sk_path_t cpath) =>
			(sk_path_clone_delegate ??= GetSymbol<Delegates.sk_path_clone> ("sk_path_clone")).Invoke (cpath);
		#endif

		// void sk_path_close(sk_path_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_close (sk_path_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_close (sk_path_t param0);
		}
		private static Delegates.sk_path_close sk_path_close_delegate;
		internal static void sk_path_close (sk_path_t param0) =>
			(sk_path_close_delegate ??= GetSymbol<Delegates.sk_path_close> ("sk_path_close")).Invoke (param0);
		#endif

		// void sk_path_compute_tight_bounds(const sk_path_t*, sk_rect_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_compute_tight_bounds (sk_path_t param0, SKRect* param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_compute_tight_bounds (sk_path_t param0, SKRect* param1);
		}
		private static Delegates.sk_path_compute_tight_bounds sk_path_compute_tight_bounds_delegate;
		internal static void sk_path_compute_tight_bounds (sk_path_t param0, SKRect* param1) =>
			(sk_path_compute_tight_bounds_delegate ??= GetSymbol<Delegates.sk_path_compute_tight_bounds> ("sk_path_compute_tight_bounds")).Invoke (param0, param1);
		#endif

		// void sk_path_conic_to(sk_path_t*, float x0, float y0, float x1, float y1, float w)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_conic_to (sk_path_t param0, Single x0, Single y0, Single x1, Single y1, Single w);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_conic_to (sk_path_t param0, Single x0, Single y0, Single x1, Single y1, Single w);
		}
		private static Delegates.sk_path_conic_to sk_path_conic_to_delegate;
		internal static void sk_path_conic_to (sk_path_t param0, Single x0, Single y0, Single x1, Single y1, Single w) =>
			(sk_path_conic_to_delegate ??= GetSymbol<Delegates.sk_path_conic_to> ("sk_path_conic_to")).Invoke (param0, x0, y0, x1, y1, w);
		#endif

		// bool sk_path_contains(const sk_path_t* cpath, float x, float y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_contains (sk_path_t cpath, Single x, Single y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_contains (sk_path_t cpath, Single x, Single y);
		}
		private static Delegates.sk_path_contains sk_path_contains_delegate;
		internal static bool sk_path_contains (sk_path_t cpath, Single x, Single y) =>
			(sk_path_contains_delegate ??= GetSymbol<Delegates.sk_path_contains> ("sk_path_contains")).Invoke (cpath, x, y);
		#endif

		// int sk_path_convert_conic_to_quads(const sk_point_t* p0, const sk_point_t* p1, const sk_point_t* p2, float w, sk_point_t* pts, int pow2)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_convert_conic_to_quads (SKPoint* p0, SKPoint* p1, SKPoint* p2, Single w, SKPoint* pts, Int32 pow2);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_path_convert_conic_to_quads (SKPoint* p0, SKPoint* p1, SKPoint* p2, Single w, SKPoint* pts, Int32 pow2);
		}
		private static Delegates.sk_path_convert_conic_to_quads sk_path_convert_conic_to_quads_delegate;
		internal static Int32 sk_path_convert_conic_to_quads (SKPoint* p0, SKPoint* p1, SKPoint* p2, Single w, SKPoint* pts, Int32 pow2) =>
			(sk_path_convert_conic_to_quads_delegate ??= GetSymbol<Delegates.sk_path_convert_conic_to_quads> ("sk_path_convert_conic_to_quads")).Invoke (p0, p1, p2, w, pts, pow2);
		#endif

		// int sk_path_count_points(const sk_path_t* cpath)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_count_points (sk_path_t cpath);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_path_count_points (sk_path_t cpath);
		}
		private static Delegates.sk_path_count_points sk_path_count_points_delegate;
		internal static Int32 sk_path_count_points (sk_path_t cpath) =>
			(sk_path_count_points_delegate ??= GetSymbol<Delegates.sk_path_count_points> ("sk_path_count_points")).Invoke (cpath);
		#endif

		// int sk_path_count_verbs(const sk_path_t* cpath)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_count_verbs (sk_path_t cpath);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_path_count_verbs (sk_path_t cpath);
		}
		private static Delegates.sk_path_count_verbs sk_path_count_verbs_delegate;
		internal static Int32 sk_path_count_verbs (sk_path_t cpath) =>
			(sk_path_count_verbs_delegate ??= GetSymbol<Delegates.sk_path_count_verbs> ("sk_path_count_verbs")).Invoke (cpath);
		#endif

		// sk_path_iterator_t* sk_path_create_iter(sk_path_t* cpath, int forceClose)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_iterator_t sk_path_create_iter (sk_path_t cpath, Int32 forceClose);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_iterator_t sk_path_create_iter (sk_path_t cpath, Int32 forceClose);
		}
		private static Delegates.sk_path_create_iter sk_path_create_iter_delegate;
		internal static sk_path_iterator_t sk_path_create_iter (sk_path_t cpath, Int32 forceClose) =>
			(sk_path_create_iter_delegate ??= GetSymbol<Delegates.sk_path_create_iter> ("sk_path_create_iter")).Invoke (cpath, forceClose);
		#endif

		// sk_path_rawiterator_t* sk_path_create_rawiter(sk_path_t* cpath)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_rawiterator_t sk_path_create_rawiter (sk_path_t cpath);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_rawiterator_t sk_path_create_rawiter (sk_path_t cpath);
		}
		private static Delegates.sk_path_create_rawiter sk_path_create_rawiter_delegate;
		internal static sk_path_rawiterator_t sk_path_create_rawiter (sk_path_t cpath) =>
			(sk_path_create_rawiter_delegate ??= GetSymbol<Delegates.sk_path_create_rawiter> ("sk_path_create_rawiter")).Invoke (cpath);
		#endif

		// void sk_path_cubic_to(sk_path_t*, float x0, float y0, float x1, float y1, float x2, float y2)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_cubic_to (sk_path_t param0, Single x0, Single y0, Single x1, Single y1, Single x2, Single y2);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_cubic_to (sk_path_t param0, Single x0, Single y0, Single x1, Single y1, Single x2, Single y2);
		}
		private static Delegates.sk_path_cubic_to sk_path_cubic_to_delegate;
		internal static void sk_path_cubic_to (sk_path_t param0, Single x0, Single y0, Single x1, Single y1, Single x2, Single y2) =>
			(sk_path_cubic_to_delegate ??= GetSymbol<Delegates.sk_path_cubic_to> ("sk_path_cubic_to")).Invoke (param0, x0, y0, x1, y1, x2, y2);
		#endif

		// void sk_path_delete(sk_path_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_delete (sk_path_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_delete (sk_path_t param0);
		}
		private static Delegates.sk_path_delete sk_path_delete_delegate;
		internal static void sk_path_delete (sk_path_t param0) =>
			(sk_path_delete_delegate ??= GetSymbol<Delegates.sk_path_delete> ("sk_path_delete")).Invoke (param0);
		#endif

		// void sk_path_get_bounds(const sk_path_t*, sk_rect_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_get_bounds (sk_path_t param0, SKRect* param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_get_bounds (sk_path_t param0, SKRect* param1);
		}
		private static Delegates.sk_path_get_bounds sk_path_get_bounds_delegate;
		internal static void sk_path_get_bounds (sk_path_t param0, SKRect* param1) =>
			(sk_path_get_bounds_delegate ??= GetSymbol<Delegates.sk_path_get_bounds> ("sk_path_get_bounds")).Invoke (param0, param1);
		#endif

		// sk_path_filltype_t sk_path_get_filltype(sk_path_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPathFillType sk_path_get_filltype (sk_path_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKPathFillType sk_path_get_filltype (sk_path_t param0);
		}
		private static Delegates.sk_path_get_filltype sk_path_get_filltype_delegate;
		internal static SKPathFillType sk_path_get_filltype (sk_path_t param0) =>
			(sk_path_get_filltype_delegate ??= GetSymbol<Delegates.sk_path_get_filltype> ("sk_path_get_filltype")).Invoke (param0);
		#endif

		// bool sk_path_get_last_point(const sk_path_t* cpath, sk_point_t* point)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_get_last_point (sk_path_t cpath, SKPoint* point);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_get_last_point (sk_path_t cpath, SKPoint* point);
		}
		private static Delegates.sk_path_get_last_point sk_path_get_last_point_delegate;
		internal static bool sk_path_get_last_point (sk_path_t cpath, SKPoint* point) =>
			(sk_path_get_last_point_delegate ??= GetSymbol<Delegates.sk_path_get_last_point> ("sk_path_get_last_point")).Invoke (cpath, point);
		#endif

		// void sk_path_get_point(const sk_path_t* cpath, int index, sk_point_t* point)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_get_point (sk_path_t cpath, Int32 index, SKPoint* point);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_get_point (sk_path_t cpath, Int32 index, SKPoint* point);
		}
		private static Delegates.sk_path_get_point sk_path_get_point_delegate;
		internal static void sk_path_get_point (sk_path_t cpath, Int32 index, SKPoint* point) =>
			(sk_path_get_point_delegate ??= GetSymbol<Delegates.sk_path_get_point> ("sk_path_get_point")).Invoke (cpath, index, point);
		#endif

		// int sk_path_get_points(const sk_path_t* cpath, sk_point_t* points, int max)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_get_points (sk_path_t cpath, SKPoint* points, Int32 max);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_path_get_points (sk_path_t cpath, SKPoint* points, Int32 max);
		}
		private static Delegates.sk_path_get_points sk_path_get_points_delegate;
		internal static Int32 sk_path_get_points (sk_path_t cpath, SKPoint* points, Int32 max) =>
			(sk_path_get_points_delegate ??= GetSymbol<Delegates.sk_path_get_points> ("sk_path_get_points")).Invoke (cpath, points, max);
		#endif

		// uint32_t sk_path_get_segment_masks(sk_path_t* cpath)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_path_get_segment_masks (sk_path_t cpath);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_path_get_segment_masks (sk_path_t cpath);
		}
		private static Delegates.sk_path_get_segment_masks sk_path_get_segment_masks_delegate;
		internal static UInt32 sk_path_get_segment_masks (sk_path_t cpath) =>
			(sk_path_get_segment_masks_delegate ??= GetSymbol<Delegates.sk_path_get_segment_masks> ("sk_path_get_segment_masks")).Invoke (cpath);
		#endif

		// bool sk_path_is_convex(const sk_path_t* cpath)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_is_convex (sk_path_t cpath);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_is_convex (sk_path_t cpath);
		}
		private static Delegates.sk_path_is_convex sk_path_is_convex_delegate;
		internal static bool sk_path_is_convex (sk_path_t cpath) =>
			(sk_path_is_convex_delegate ??= GetSymbol<Delegates.sk_path_is_convex> ("sk_path_is_convex")).Invoke (cpath);
		#endif

		// bool sk_path_is_line(sk_path_t* cpath, sk_point_t[2] line = 2)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_is_line (sk_path_t cpath, SKPoint* line);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_is_line (sk_path_t cpath, SKPoint* line);
		}
		private static Delegates.sk_path_is_line sk_path_is_line_delegate;
		internal static bool sk_path_is_line (sk_path_t cpath, SKPoint* line) =>
			(sk_path_is_line_delegate ??= GetSymbol<Delegates.sk_path_is_line> ("sk_path_is_line")).Invoke (cpath, line);
		#endif

		// bool sk_path_is_oval(sk_path_t* cpath, sk_rect_t* bounds)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_is_oval (sk_path_t cpath, SKRect* bounds);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_is_oval (sk_path_t cpath, SKRect* bounds);
		}
		private static Delegates.sk_path_is_oval sk_path_is_oval_delegate;
		internal static bool sk_path_is_oval (sk_path_t cpath, SKRect* bounds) =>
			(sk_path_is_oval_delegate ??= GetSymbol<Delegates.sk_path_is_oval> ("sk_path_is_oval")).Invoke (cpath, bounds);
		#endif

		// bool sk_path_is_rect(sk_path_t* cpath, sk_rect_t* rect, bool* isClosed, sk_path_direction_t* direction)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_is_rect (sk_path_t cpath, SKRect* rect, Byte* isClosed, SKPathDirection* direction);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_is_rect (sk_path_t cpath, SKRect* rect, Byte* isClosed, SKPathDirection* direction);
		}
		private static Delegates.sk_path_is_rect sk_path_is_rect_delegate;
		internal static bool sk_path_is_rect (sk_path_t cpath, SKRect* rect, Byte* isClosed, SKPathDirection* direction) =>
			(sk_path_is_rect_delegate ??= GetSymbol<Delegates.sk_path_is_rect> ("sk_path_is_rect")).Invoke (cpath, rect, isClosed, direction);
		#endif

		// bool sk_path_is_rrect(sk_path_t* cpath, sk_rrect_t* bounds)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_is_rrect (sk_path_t cpath, sk_rrect_t bounds);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_is_rrect (sk_path_t cpath, sk_rrect_t bounds);
		}
		private static Delegates.sk_path_is_rrect sk_path_is_rrect_delegate;
		internal static bool sk_path_is_rrect (sk_path_t cpath, sk_rrect_t bounds) =>
			(sk_path_is_rrect_delegate ??= GetSymbol<Delegates.sk_path_is_rrect> ("sk_path_is_rrect")).Invoke (cpath, bounds);
		#endif

		// float sk_path_iter_conic_weight(sk_path_iterator_t* iterator)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_path_iter_conic_weight (sk_path_iterator_t iterator);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_path_iter_conic_weight (sk_path_iterator_t iterator);
		}
		private static Delegates.sk_path_iter_conic_weight sk_path_iter_conic_weight_delegate;
		internal static Single sk_path_iter_conic_weight (sk_path_iterator_t iterator) =>
			(sk_path_iter_conic_weight_delegate ??= GetSymbol<Delegates.sk_path_iter_conic_weight> ("sk_path_iter_conic_weight")).Invoke (iterator);
		#endif

		// void sk_path_iter_destroy(sk_path_iterator_t* iterator)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_iter_destroy (sk_path_iterator_t iterator);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_iter_destroy (sk_path_iterator_t iterator);
		}
		private static Delegates.sk_path_iter_destroy sk_path_iter_destroy_delegate;
		internal static void sk_path_iter_destroy (sk_path_iterator_t iterator) =>
			(sk_path_iter_destroy_delegate ??= GetSymbol<Delegates.sk_path_iter_destroy> ("sk_path_iter_destroy")).Invoke (iterator);
		#endif

		// int sk_path_iter_is_close_line(sk_path_iterator_t* iterator)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_iter_is_close_line (sk_path_iterator_t iterator);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_path_iter_is_close_line (sk_path_iterator_t iterator);
		}
		private static Delegates.sk_path_iter_is_close_line sk_path_iter_is_close_line_delegate;
		internal static Int32 sk_path_iter_is_close_line (sk_path_iterator_t iterator) =>
			(sk_path_iter_is_close_line_delegate ??= GetSymbol<Delegates.sk_path_iter_is_close_line> ("sk_path_iter_is_close_line")).Invoke (iterator);
		#endif

		// int sk_path_iter_is_closed_contour(sk_path_iterator_t* iterator)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_iter_is_closed_contour (sk_path_iterator_t iterator);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_path_iter_is_closed_contour (sk_path_iterator_t iterator);
		}
		private static Delegates.sk_path_iter_is_closed_contour sk_path_iter_is_closed_contour_delegate;
		internal static Int32 sk_path_iter_is_closed_contour (sk_path_iterator_t iterator) =>
			(sk_path_iter_is_closed_contour_delegate ??= GetSymbol<Delegates.sk_path_iter_is_closed_contour> ("sk_path_iter_is_closed_contour")).Invoke (iterator);
		#endif

		// sk_path_verb_t sk_path_iter_next(sk_path_iterator_t* iterator, sk_point_t[4] points = 4)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPathVerb sk_path_iter_next (sk_path_iterator_t iterator, SKPoint* points);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKPathVerb sk_path_iter_next (sk_path_iterator_t iterator, SKPoint* points);
		}
		private static Delegates.sk_path_iter_next sk_path_iter_next_delegate;
		internal static SKPathVerb sk_path_iter_next (sk_path_iterator_t iterator, SKPoint* points) =>
			(sk_path_iter_next_delegate ??= GetSymbol<Delegates.sk_path_iter_next> ("sk_path_iter_next")).Invoke (iterator, points);
		#endif

		// void sk_path_line_to(sk_path_t*, float x, float y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_line_to (sk_path_t param0, Single x, Single y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_line_to (sk_path_t param0, Single x, Single y);
		}
		private static Delegates.sk_path_line_to sk_path_line_to_delegate;
		internal static void sk_path_line_to (sk_path_t param0, Single x, Single y) =>
			(sk_path_line_to_delegate ??= GetSymbol<Delegates.sk_path_line_to> ("sk_path_line_to")).Invoke (param0, x, y);
		#endif

		// void sk_path_move_to(sk_path_t*, float x, float y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_move_to (sk_path_t param0, Single x, Single y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_move_to (sk_path_t param0, Single x, Single y);
		}
		private static Delegates.sk_path_move_to sk_path_move_to_delegate;
		internal static void sk_path_move_to (sk_path_t param0, Single x, Single y) =>
			(sk_path_move_to_delegate ??= GetSymbol<Delegates.sk_path_move_to> ("sk_path_move_to")).Invoke (param0, x, y);
		#endif

		// sk_path_t* sk_path_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_t sk_path_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_t sk_path_new ();
		}
		private static Delegates.sk_path_new sk_path_new_delegate;
		internal static sk_path_t sk_path_new () =>
			(sk_path_new_delegate ??= GetSymbol<Delegates.sk_path_new> ("sk_path_new")).Invoke ();
		#endif

		// bool sk_path_parse_svg_string(sk_path_t* cpath, const char* str)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_parse_svg_string (sk_path_t cpath, [MarshalAs (UnmanagedType.LPStr)] String str);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_parse_svg_string (sk_path_t cpath, [MarshalAs (UnmanagedType.LPStr)] String str);
		}
		private static Delegates.sk_path_parse_svg_string sk_path_parse_svg_string_delegate;
		internal static bool sk_path_parse_svg_string (sk_path_t cpath, [MarshalAs (UnmanagedType.LPStr)] String str) =>
			(sk_path_parse_svg_string_delegate ??= GetSymbol<Delegates.sk_path_parse_svg_string> ("sk_path_parse_svg_string")).Invoke (cpath, str);
		#endif

		// void sk_path_quad_to(sk_path_t*, float x0, float y0, float x1, float y1)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_quad_to (sk_path_t param0, Single x0, Single y0, Single x1, Single y1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_quad_to (sk_path_t param0, Single x0, Single y0, Single x1, Single y1);
		}
		private static Delegates.sk_path_quad_to sk_path_quad_to_delegate;
		internal static void sk_path_quad_to (sk_path_t param0, Single x0, Single y0, Single x1, Single y1) =>
			(sk_path_quad_to_delegate ??= GetSymbol<Delegates.sk_path_quad_to> ("sk_path_quad_to")).Invoke (param0, x0, y0, x1, y1);
		#endif

		// void sk_path_rarc_to(sk_path_t*, float rx, float ry, float xAxisRotate, sk_path_arc_size_t largeArc, sk_path_direction_t sweep, float x, float y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rarc_to (sk_path_t param0, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_rarc_to (sk_path_t param0, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y);
		}
		private static Delegates.sk_path_rarc_to sk_path_rarc_to_delegate;
		internal static void sk_path_rarc_to (sk_path_t param0, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y) =>
			(sk_path_rarc_to_delegate ??= GetSymbol<Delegates.sk_path_rarc_to> ("sk_path_rarc_to")).Invoke (param0, rx, ry, xAxisRotate, largeArc, sweep, x, y);
		#endif

		// float sk_path_rawiter_conic_weight(sk_path_rawiterator_t* iterator)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_path_rawiter_conic_weight (sk_path_rawiterator_t iterator);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_path_rawiter_conic_weight (sk_path_rawiterator_t iterator);
		}
		private static Delegates.sk_path_rawiter_conic_weight sk_path_rawiter_conic_weight_delegate;
		internal static Single sk_path_rawiter_conic_weight (sk_path_rawiterator_t iterator) =>
			(sk_path_rawiter_conic_weight_delegate ??= GetSymbol<Delegates.sk_path_rawiter_conic_weight> ("sk_path_rawiter_conic_weight")).Invoke (iterator);
		#endif

		// void sk_path_rawiter_destroy(sk_path_rawiterator_t* iterator)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rawiter_destroy (sk_path_rawiterator_t iterator);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_rawiter_destroy (sk_path_rawiterator_t iterator);
		}
		private static Delegates.sk_path_rawiter_destroy sk_path_rawiter_destroy_delegate;
		internal static void sk_path_rawiter_destroy (sk_path_rawiterator_t iterator) =>
			(sk_path_rawiter_destroy_delegate ??= GetSymbol<Delegates.sk_path_rawiter_destroy> ("sk_path_rawiter_destroy")).Invoke (iterator);
		#endif

		// sk_path_verb_t sk_path_rawiter_next(sk_path_rawiterator_t* iterator, sk_point_t[4] points = 4)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPathVerb sk_path_rawiter_next (sk_path_rawiterator_t iterator, SKPoint* points);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKPathVerb sk_path_rawiter_next (sk_path_rawiterator_t iterator, SKPoint* points);
		}
		private static Delegates.sk_path_rawiter_next sk_path_rawiter_next_delegate;
		internal static SKPathVerb sk_path_rawiter_next (sk_path_rawiterator_t iterator, SKPoint* points) =>
			(sk_path_rawiter_next_delegate ??= GetSymbol<Delegates.sk_path_rawiter_next> ("sk_path_rawiter_next")).Invoke (iterator, points);
		#endif

		// sk_path_verb_t sk_path_rawiter_peek(sk_path_rawiterator_t* iterator)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPathVerb sk_path_rawiter_peek (sk_path_rawiterator_t iterator);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKPathVerb sk_path_rawiter_peek (sk_path_rawiterator_t iterator);
		}
		private static Delegates.sk_path_rawiter_peek sk_path_rawiter_peek_delegate;
		internal static SKPathVerb sk_path_rawiter_peek (sk_path_rawiterator_t iterator) =>
			(sk_path_rawiter_peek_delegate ??= GetSymbol<Delegates.sk_path_rawiter_peek> ("sk_path_rawiter_peek")).Invoke (iterator);
		#endif

		// void sk_path_rconic_to(sk_path_t*, float dx0, float dy0, float dx1, float dy1, float w)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rconic_to (sk_path_t param0, Single dx0, Single dy0, Single dx1, Single dy1, Single w);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_rconic_to (sk_path_t param0, Single dx0, Single dy0, Single dx1, Single dy1, Single w);
		}
		private static Delegates.sk_path_rconic_to sk_path_rconic_to_delegate;
		internal static void sk_path_rconic_to (sk_path_t param0, Single dx0, Single dy0, Single dx1, Single dy1, Single w) =>
			(sk_path_rconic_to_delegate ??= GetSymbol<Delegates.sk_path_rconic_to> ("sk_path_rconic_to")).Invoke (param0, dx0, dy0, dx1, dy1, w);
		#endif

		// void sk_path_rcubic_to(sk_path_t*, float dx0, float dy0, float dx1, float dy1, float dx2, float dy2)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rcubic_to (sk_path_t param0, Single dx0, Single dy0, Single dx1, Single dy1, Single dx2, Single dy2);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_rcubic_to (sk_path_t param0, Single dx0, Single dy0, Single dx1, Single dy1, Single dx2, Single dy2);
		}
		private static Delegates.sk_path_rcubic_to sk_path_rcubic_to_delegate;
		internal static void sk_path_rcubic_to (sk_path_t param0, Single dx0, Single dy0, Single dx1, Single dy1, Single dx2, Single dy2) =>
			(sk_path_rcubic_to_delegate ??= GetSymbol<Delegates.sk_path_rcubic_to> ("sk_path_rcubic_to")).Invoke (param0, dx0, dy0, dx1, dy1, dx2, dy2);
		#endif

		// void sk_path_reset(sk_path_t* cpath)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_reset (sk_path_t cpath);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_reset (sk_path_t cpath);
		}
		private static Delegates.sk_path_reset sk_path_reset_delegate;
		internal static void sk_path_reset (sk_path_t cpath) =>
			(sk_path_reset_delegate ??= GetSymbol<Delegates.sk_path_reset> ("sk_path_reset")).Invoke (cpath);
		#endif

		// void sk_path_rewind(sk_path_t* cpath)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rewind (sk_path_t cpath);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_rewind (sk_path_t cpath);
		}
		private static Delegates.sk_path_rewind sk_path_rewind_delegate;
		internal static void sk_path_rewind (sk_path_t cpath) =>
			(sk_path_rewind_delegate ??= GetSymbol<Delegates.sk_path_rewind> ("sk_path_rewind")).Invoke (cpath);
		#endif

		// void sk_path_rline_to(sk_path_t*, float dx, float yd)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rline_to (sk_path_t param0, Single dx, Single yd);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_rline_to (sk_path_t param0, Single dx, Single yd);
		}
		private static Delegates.sk_path_rline_to sk_path_rline_to_delegate;
		internal static void sk_path_rline_to (sk_path_t param0, Single dx, Single yd) =>
			(sk_path_rline_to_delegate ??= GetSymbol<Delegates.sk_path_rline_to> ("sk_path_rline_to")).Invoke (param0, dx, yd);
		#endif

		// void sk_path_rmove_to(sk_path_t*, float dx, float dy)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rmove_to (sk_path_t param0, Single dx, Single dy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_rmove_to (sk_path_t param0, Single dx, Single dy);
		}
		private static Delegates.sk_path_rmove_to sk_path_rmove_to_delegate;
		internal static void sk_path_rmove_to (sk_path_t param0, Single dx, Single dy) =>
			(sk_path_rmove_to_delegate ??= GetSymbol<Delegates.sk_path_rmove_to> ("sk_path_rmove_to")).Invoke (param0, dx, dy);
		#endif

		// void sk_path_rquad_to(sk_path_t*, float dx0, float dy0, float dx1, float dy1)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rquad_to (sk_path_t param0, Single dx0, Single dy0, Single dx1, Single dy1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_rquad_to (sk_path_t param0, Single dx0, Single dy0, Single dx1, Single dy1);
		}
		private static Delegates.sk_path_rquad_to sk_path_rquad_to_delegate;
		internal static void sk_path_rquad_to (sk_path_t param0, Single dx0, Single dy0, Single dx1, Single dy1) =>
			(sk_path_rquad_to_delegate ??= GetSymbol<Delegates.sk_path_rquad_to> ("sk_path_rquad_to")).Invoke (param0, dx0, dy0, dx1, dy1);
		#endif

		// void sk_path_set_filltype(sk_path_t*, sk_path_filltype_t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_set_filltype (sk_path_t param0, SKPathFillType param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_set_filltype (sk_path_t param0, SKPathFillType param1);
		}
		private static Delegates.sk_path_set_filltype sk_path_set_filltype_delegate;
		internal static void sk_path_set_filltype (sk_path_t param0, SKPathFillType param1) =>
			(sk_path_set_filltype_delegate ??= GetSymbol<Delegates.sk_path_set_filltype> ("sk_path_set_filltype")).Invoke (param0, param1);
		#endif

		// void sk_path_to_svg_string(const sk_path_t* cpath, sk_string_t* str)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_to_svg_string (sk_path_t cpath, sk_string_t str);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_to_svg_string (sk_path_t cpath, sk_string_t str);
		}
		private static Delegates.sk_path_to_svg_string sk_path_to_svg_string_delegate;
		internal static void sk_path_to_svg_string (sk_path_t cpath, sk_string_t str) =>
			(sk_path_to_svg_string_delegate ??= GetSymbol<Delegates.sk_path_to_svg_string> ("sk_path_to_svg_string")).Invoke (cpath, str);
		#endif

		// void sk_path_transform(sk_path_t* cpath, const sk_matrix_t* cmatrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_transform (sk_path_t cpath, SKMatrix* cmatrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_transform (sk_path_t cpath, SKMatrix* cmatrix);
		}
		private static Delegates.sk_path_transform sk_path_transform_delegate;
		internal static void sk_path_transform (sk_path_t cpath, SKMatrix* cmatrix) =>
			(sk_path_transform_delegate ??= GetSymbol<Delegates.sk_path_transform> ("sk_path_transform")).Invoke (cpath, cmatrix);
		#endif

		// void sk_path_transform_to_dest(const sk_path_t* cpath, const sk_matrix_t* cmatrix, sk_path_t* destination)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_transform_to_dest (sk_path_t cpath, SKMatrix* cmatrix, sk_path_t destination);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_transform_to_dest (sk_path_t cpath, SKMatrix* cmatrix, sk_path_t destination);
		}
		private static Delegates.sk_path_transform_to_dest sk_path_transform_to_dest_delegate;
		internal static void sk_path_transform_to_dest (sk_path_t cpath, SKMatrix* cmatrix, sk_path_t destination) =>
			(sk_path_transform_to_dest_delegate ??= GetSymbol<Delegates.sk_path_transform_to_dest> ("sk_path_transform_to_dest")).Invoke (cpath, cmatrix, destination);
		#endif

		// void sk_pathmeasure_destroy(sk_pathmeasure_t* pathMeasure)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathmeasure_destroy (sk_pathmeasure_t pathMeasure);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathmeasure_destroy (sk_pathmeasure_t pathMeasure);
		}
		private static Delegates.sk_pathmeasure_destroy sk_pathmeasure_destroy_delegate;
		internal static void sk_pathmeasure_destroy (sk_pathmeasure_t pathMeasure) =>
			(sk_pathmeasure_destroy_delegate ??= GetSymbol<Delegates.sk_pathmeasure_destroy> ("sk_pathmeasure_destroy")).Invoke (pathMeasure);
		#endif

		// float sk_pathmeasure_get_length(sk_pathmeasure_t* pathMeasure)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_pathmeasure_get_length (sk_pathmeasure_t pathMeasure);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_pathmeasure_get_length (sk_pathmeasure_t pathMeasure);
		}
		private static Delegates.sk_pathmeasure_get_length sk_pathmeasure_get_length_delegate;
		internal static Single sk_pathmeasure_get_length (sk_pathmeasure_t pathMeasure) =>
			(sk_pathmeasure_get_length_delegate ??= GetSymbol<Delegates.sk_pathmeasure_get_length> ("sk_pathmeasure_get_length")).Invoke (pathMeasure);
		#endif

		// bool sk_pathmeasure_get_matrix(sk_pathmeasure_t* pathMeasure, float distance, sk_matrix_t* matrix, sk_pathmeasure_matrixflags_t flags)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_get_matrix (sk_pathmeasure_t pathMeasure, Single distance, SKMatrix* matrix, SKPathMeasureMatrixFlags flags);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathmeasure_get_matrix (sk_pathmeasure_t pathMeasure, Single distance, SKMatrix* matrix, SKPathMeasureMatrixFlags flags);
		}
		private static Delegates.sk_pathmeasure_get_matrix sk_pathmeasure_get_matrix_delegate;
		internal static bool sk_pathmeasure_get_matrix (sk_pathmeasure_t pathMeasure, Single distance, SKMatrix* matrix, SKPathMeasureMatrixFlags flags) =>
			(sk_pathmeasure_get_matrix_delegate ??= GetSymbol<Delegates.sk_pathmeasure_get_matrix> ("sk_pathmeasure_get_matrix")).Invoke (pathMeasure, distance, matrix, flags);
		#endif

		// bool sk_pathmeasure_get_pos_tan(sk_pathmeasure_t* pathMeasure, float distance, sk_point_t* position, sk_vector_t* tangent)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_get_pos_tan (sk_pathmeasure_t pathMeasure, Single distance, SKPoint* position, SKPoint* tangent);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathmeasure_get_pos_tan (sk_pathmeasure_t pathMeasure, Single distance, SKPoint* position, SKPoint* tangent);
		}
		private static Delegates.sk_pathmeasure_get_pos_tan sk_pathmeasure_get_pos_tan_delegate;
		internal static bool sk_pathmeasure_get_pos_tan (sk_pathmeasure_t pathMeasure, Single distance, SKPoint* position, SKPoint* tangent) =>
			(sk_pathmeasure_get_pos_tan_delegate ??= GetSymbol<Delegates.sk_pathmeasure_get_pos_tan> ("sk_pathmeasure_get_pos_tan")).Invoke (pathMeasure, distance, position, tangent);
		#endif

		// bool sk_pathmeasure_get_segment(sk_pathmeasure_t* pathMeasure, float start, float stop, sk_path_t* dst, bool startWithMoveTo)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_get_segment (sk_pathmeasure_t pathMeasure, Single start, Single stop, sk_path_t dst, [MarshalAs (UnmanagedType.I1)] bool startWithMoveTo);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathmeasure_get_segment (sk_pathmeasure_t pathMeasure, Single start, Single stop, sk_path_t dst, [MarshalAs (UnmanagedType.I1)] bool startWithMoveTo);
		}
		private static Delegates.sk_pathmeasure_get_segment sk_pathmeasure_get_segment_delegate;
		internal static bool sk_pathmeasure_get_segment (sk_pathmeasure_t pathMeasure, Single start, Single stop, sk_path_t dst, [MarshalAs (UnmanagedType.I1)] bool startWithMoveTo) =>
			(sk_pathmeasure_get_segment_delegate ??= GetSymbol<Delegates.sk_pathmeasure_get_segment> ("sk_pathmeasure_get_segment")).Invoke (pathMeasure, start, stop, dst, startWithMoveTo);
		#endif

		// bool sk_pathmeasure_is_closed(sk_pathmeasure_t* pathMeasure)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_is_closed (sk_pathmeasure_t pathMeasure);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathmeasure_is_closed (sk_pathmeasure_t pathMeasure);
		}
		private static Delegates.sk_pathmeasure_is_closed sk_pathmeasure_is_closed_delegate;
		internal static bool sk_pathmeasure_is_closed (sk_pathmeasure_t pathMeasure) =>
			(sk_pathmeasure_is_closed_delegate ??= GetSymbol<Delegates.sk_pathmeasure_is_closed> ("sk_pathmeasure_is_closed")).Invoke (pathMeasure);
		#endif

		// sk_pathmeasure_t* sk_pathmeasure_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_pathmeasure_t sk_pathmeasure_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_pathmeasure_t sk_pathmeasure_new ();
		}
		private static Delegates.sk_pathmeasure_new sk_pathmeasure_new_delegate;
		internal static sk_pathmeasure_t sk_pathmeasure_new () =>
			(sk_pathmeasure_new_delegate ??= GetSymbol<Delegates.sk_pathmeasure_new> ("sk_pathmeasure_new")).Invoke ();
		#endif

		// sk_pathmeasure_t* sk_pathmeasure_new_with_path(const sk_path_t* path, bool forceClosed, float resScale)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_pathmeasure_t sk_pathmeasure_new_with_path (sk_path_t path, [MarshalAs (UnmanagedType.I1)] bool forceClosed, Single resScale);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_pathmeasure_t sk_pathmeasure_new_with_path (sk_path_t path, [MarshalAs (UnmanagedType.I1)] bool forceClosed, Single resScale);
		}
		private static Delegates.sk_pathmeasure_new_with_path sk_pathmeasure_new_with_path_delegate;
		internal static sk_pathmeasure_t sk_pathmeasure_new_with_path (sk_path_t path, [MarshalAs (UnmanagedType.I1)] bool forceClosed, Single resScale) =>
			(sk_pathmeasure_new_with_path_delegate ??= GetSymbol<Delegates.sk_pathmeasure_new_with_path> ("sk_pathmeasure_new_with_path")).Invoke (path, forceClosed, resScale);
		#endif

		// bool sk_pathmeasure_next_contour(sk_pathmeasure_t* pathMeasure)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_next_contour (sk_pathmeasure_t pathMeasure);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathmeasure_next_contour (sk_pathmeasure_t pathMeasure);
		}
		private static Delegates.sk_pathmeasure_next_contour sk_pathmeasure_next_contour_delegate;
		internal static bool sk_pathmeasure_next_contour (sk_pathmeasure_t pathMeasure) =>
			(sk_pathmeasure_next_contour_delegate ??= GetSymbol<Delegates.sk_pathmeasure_next_contour> ("sk_pathmeasure_next_contour")).Invoke (pathMeasure);
		#endif

		// void sk_pathmeasure_set_path(sk_pathmeasure_t* pathMeasure, const sk_path_t* path, bool forceClosed)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathmeasure_set_path (sk_pathmeasure_t pathMeasure, sk_path_t path, [MarshalAs (UnmanagedType.I1)] bool forceClosed);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathmeasure_set_path (sk_pathmeasure_t pathMeasure, sk_path_t path, [MarshalAs (UnmanagedType.I1)] bool forceClosed);
		}
		private static Delegates.sk_pathmeasure_set_path sk_pathmeasure_set_path_delegate;
		internal static void sk_pathmeasure_set_path (sk_pathmeasure_t pathMeasure, sk_path_t path, [MarshalAs (UnmanagedType.I1)] bool forceClosed) =>
			(sk_pathmeasure_set_path_delegate ??= GetSymbol<Delegates.sk_pathmeasure_set_path> ("sk_pathmeasure_set_path")).Invoke (pathMeasure, path, forceClosed);
		#endif

		// bool sk_pathop_as_winding(const sk_path_t* path, sk_path_t* result)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathop_as_winding (sk_path_t path, sk_path_t result);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathop_as_winding (sk_path_t path, sk_path_t result);
		}
		private static Delegates.sk_pathop_as_winding sk_pathop_as_winding_delegate;
		internal static bool sk_pathop_as_winding (sk_path_t path, sk_path_t result) =>
			(sk_pathop_as_winding_delegate ??= GetSymbol<Delegates.sk_pathop_as_winding> ("sk_pathop_as_winding")).Invoke (path, result);
		#endif

		// bool sk_pathop_op(const sk_path_t* one, const sk_path_t* two, sk_pathop_t op, sk_path_t* result)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathop_op (sk_path_t one, sk_path_t two, SKPathOp op, sk_path_t result);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathop_op (sk_path_t one, sk_path_t two, SKPathOp op, sk_path_t result);
		}
		private static Delegates.sk_pathop_op sk_pathop_op_delegate;
		internal static bool sk_pathop_op (sk_path_t one, sk_path_t two, SKPathOp op, sk_path_t result) =>
			(sk_pathop_op_delegate ??= GetSymbol<Delegates.sk_pathop_op> ("sk_pathop_op")).Invoke (one, two, op, result);
		#endif

		// bool sk_pathop_simplify(const sk_path_t* path, sk_path_t* result)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathop_simplify (sk_path_t path, sk_path_t result);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathop_simplify (sk_path_t path, sk_path_t result);
		}
		private static Delegates.sk_pathop_simplify sk_pathop_simplify_delegate;
		internal static bool sk_pathop_simplify (sk_path_t path, sk_path_t result) =>
			(sk_pathop_simplify_delegate ??= GetSymbol<Delegates.sk_pathop_simplify> ("sk_pathop_simplify")).Invoke (path, result);
		#endif

		// bool sk_pathop_tight_bounds(const sk_path_t* path, sk_rect_t* result)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathop_tight_bounds (sk_path_t path, SKRect* result);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathop_tight_bounds (sk_path_t path, SKRect* result);
		}
		private static Delegates.sk_pathop_tight_bounds sk_pathop_tight_bounds_delegate;
		internal static bool sk_pathop_tight_bounds (sk_path_t path, SKRect* result) =>
			(sk_pathop_tight_bounds_delegate ??= GetSymbol<Delegates.sk_pathop_tight_bounds> ("sk_pathop_tight_bounds")).Invoke (path, result);
		#endif

		#endregion

		#region sk_patheffect.h

		// sk_path_effect_t* sk_path_effect_create_1d_path(const sk_path_t* path, float advance, float phase, sk_path_effect_1d_style_t style)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_1d_path (sk_path_t path, Single advance, Single phase, SKPath1DPathEffectStyle style);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_1d_path (sk_path_t path, Single advance, Single phase, SKPath1DPathEffectStyle style);
		}
		private static Delegates.sk_path_effect_create_1d_path sk_path_effect_create_1d_path_delegate;
		internal static sk_path_effect_t sk_path_effect_create_1d_path (sk_path_t path, Single advance, Single phase, SKPath1DPathEffectStyle style) =>
			(sk_path_effect_create_1d_path_delegate ??= GetSymbol<Delegates.sk_path_effect_create_1d_path> ("sk_path_effect_create_1d_path")).Invoke (path, advance, phase, style);
		#endif

		// sk_path_effect_t* sk_path_effect_create_2d_line(float width, const sk_matrix_t* matrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_2d_line (Single width, SKMatrix* matrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_2d_line (Single width, SKMatrix* matrix);
		}
		private static Delegates.sk_path_effect_create_2d_line sk_path_effect_create_2d_line_delegate;
		internal static sk_path_effect_t sk_path_effect_create_2d_line (Single width, SKMatrix* matrix) =>
			(sk_path_effect_create_2d_line_delegate ??= GetSymbol<Delegates.sk_path_effect_create_2d_line> ("sk_path_effect_create_2d_line")).Invoke (width, matrix);
		#endif

		// sk_path_effect_t* sk_path_effect_create_2d_path(const sk_matrix_t* matrix, const sk_path_t* path)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_2d_path (SKMatrix* matrix, sk_path_t path);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_2d_path (SKMatrix* matrix, sk_path_t path);
		}
		private static Delegates.sk_path_effect_create_2d_path sk_path_effect_create_2d_path_delegate;
		internal static sk_path_effect_t sk_path_effect_create_2d_path (SKMatrix* matrix, sk_path_t path) =>
			(sk_path_effect_create_2d_path_delegate ??= GetSymbol<Delegates.sk_path_effect_create_2d_path> ("sk_path_effect_create_2d_path")).Invoke (matrix, path);
		#endif

		// sk_path_effect_t* sk_path_effect_create_compose(sk_path_effect_t* outer, sk_path_effect_t* inner)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_compose (sk_path_effect_t outer, sk_path_effect_t inner);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_compose (sk_path_effect_t outer, sk_path_effect_t inner);
		}
		private static Delegates.sk_path_effect_create_compose sk_path_effect_create_compose_delegate;
		internal static sk_path_effect_t sk_path_effect_create_compose (sk_path_effect_t outer, sk_path_effect_t inner) =>
			(sk_path_effect_create_compose_delegate ??= GetSymbol<Delegates.sk_path_effect_create_compose> ("sk_path_effect_create_compose")).Invoke (outer, inner);
		#endif

		// sk_path_effect_t* sk_path_effect_create_corner(float radius)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_corner (Single radius);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_corner (Single radius);
		}
		private static Delegates.sk_path_effect_create_corner sk_path_effect_create_corner_delegate;
		internal static sk_path_effect_t sk_path_effect_create_corner (Single radius) =>
			(sk_path_effect_create_corner_delegate ??= GetSymbol<Delegates.sk_path_effect_create_corner> ("sk_path_effect_create_corner")).Invoke (radius);
		#endif

		// sk_path_effect_t* sk_path_effect_create_dash(const float[-1] intervals, int count, float phase)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_dash (Single* intervals, Int32 count, Single phase);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_dash (Single* intervals, Int32 count, Single phase);
		}
		private static Delegates.sk_path_effect_create_dash sk_path_effect_create_dash_delegate;
		internal static sk_path_effect_t sk_path_effect_create_dash (Single* intervals, Int32 count, Single phase) =>
			(sk_path_effect_create_dash_delegate ??= GetSymbol<Delegates.sk_path_effect_create_dash> ("sk_path_effect_create_dash")).Invoke (intervals, count, phase);
		#endif

		// sk_path_effect_t* sk_path_effect_create_discrete(float segLength, float deviation, uint32_t seedAssist)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_discrete (Single segLength, Single deviation, UInt32 seedAssist);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_discrete (Single segLength, Single deviation, UInt32 seedAssist);
		}
		private static Delegates.sk_path_effect_create_discrete sk_path_effect_create_discrete_delegate;
		internal static sk_path_effect_t sk_path_effect_create_discrete (Single segLength, Single deviation, UInt32 seedAssist) =>
			(sk_path_effect_create_discrete_delegate ??= GetSymbol<Delegates.sk_path_effect_create_discrete> ("sk_path_effect_create_discrete")).Invoke (segLength, deviation, seedAssist);
		#endif

		// sk_path_effect_t* sk_path_effect_create_sum(sk_path_effect_t* first, sk_path_effect_t* second)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_sum (sk_path_effect_t first, sk_path_effect_t second);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_sum (sk_path_effect_t first, sk_path_effect_t second);
		}
		private static Delegates.sk_path_effect_create_sum sk_path_effect_create_sum_delegate;
		internal static sk_path_effect_t sk_path_effect_create_sum (sk_path_effect_t first, sk_path_effect_t second) =>
			(sk_path_effect_create_sum_delegate ??= GetSymbol<Delegates.sk_path_effect_create_sum> ("sk_path_effect_create_sum")).Invoke (first, second);
		#endif

		// sk_path_effect_t* sk_path_effect_create_trim(float start, float stop, sk_path_effect_trim_mode_t mode)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_trim (Single start, Single stop, SKTrimPathEffectMode mode);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_trim (Single start, Single stop, SKTrimPathEffectMode mode);
		}
		private static Delegates.sk_path_effect_create_trim sk_path_effect_create_trim_delegate;
		internal static sk_path_effect_t sk_path_effect_create_trim (Single start, Single stop, SKTrimPathEffectMode mode) =>
			(sk_path_effect_create_trim_delegate ??= GetSymbol<Delegates.sk_path_effect_create_trim> ("sk_path_effect_create_trim")).Invoke (start, stop, mode);
		#endif

		// void sk_path_effect_unref(sk_path_effect_t* t)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_effect_unref (sk_path_effect_t t);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_effect_unref (sk_path_effect_t t);
		}
		private static Delegates.sk_path_effect_unref sk_path_effect_unref_delegate;
		internal static void sk_path_effect_unref (sk_path_effect_t t) =>
			(sk_path_effect_unref_delegate ??= GetSymbol<Delegates.sk_path_effect_unref> ("sk_path_effect_unref")).Invoke (t);
		#endif

		#endregion

		#region sk_picture.h

		// sk_picture_t* sk_picture_deserialize_from_data(sk_data_t* data)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_picture_t sk_picture_deserialize_from_data (sk_data_t data);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_picture_t sk_picture_deserialize_from_data (sk_data_t data);
		}
		private static Delegates.sk_picture_deserialize_from_data sk_picture_deserialize_from_data_delegate;
		internal static sk_picture_t sk_picture_deserialize_from_data (sk_data_t data) =>
			(sk_picture_deserialize_from_data_delegate ??= GetSymbol<Delegates.sk_picture_deserialize_from_data> ("sk_picture_deserialize_from_data")).Invoke (data);
		#endif

		// sk_picture_t* sk_picture_deserialize_from_memory(void* buffer, size_t length)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_picture_t sk_picture_deserialize_from_memory (void* buffer, /* size_t */ IntPtr length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_picture_t sk_picture_deserialize_from_memory (void* buffer, /* size_t */ IntPtr length);
		}
		private static Delegates.sk_picture_deserialize_from_memory sk_picture_deserialize_from_memory_delegate;
		internal static sk_picture_t sk_picture_deserialize_from_memory (void* buffer, /* size_t */ IntPtr length) =>
			(sk_picture_deserialize_from_memory_delegate ??= GetSymbol<Delegates.sk_picture_deserialize_from_memory> ("sk_picture_deserialize_from_memory")).Invoke (buffer, length);
		#endif

		// sk_picture_t* sk_picture_deserialize_from_stream(sk_stream_t* stream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_picture_t sk_picture_deserialize_from_stream (sk_stream_t stream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_picture_t sk_picture_deserialize_from_stream (sk_stream_t stream);
		}
		private static Delegates.sk_picture_deserialize_from_stream sk_picture_deserialize_from_stream_delegate;
		internal static sk_picture_t sk_picture_deserialize_from_stream (sk_stream_t stream) =>
			(sk_picture_deserialize_from_stream_delegate ??= GetSymbol<Delegates.sk_picture_deserialize_from_stream> ("sk_picture_deserialize_from_stream")).Invoke (stream);
		#endif

		// void sk_picture_get_cull_rect(sk_picture_t*, sk_rect_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_get_cull_rect (sk_picture_t param0, SKRect* param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_picture_get_cull_rect (sk_picture_t param0, SKRect* param1);
		}
		private static Delegates.sk_picture_get_cull_rect sk_picture_get_cull_rect_delegate;
		internal static void sk_picture_get_cull_rect (sk_picture_t param0, SKRect* param1) =>
			(sk_picture_get_cull_rect_delegate ??= GetSymbol<Delegates.sk_picture_get_cull_rect> ("sk_picture_get_cull_rect")).Invoke (param0, param1);
		#endif

		// sk_canvas_t* sk_picture_get_recording_canvas(sk_picture_recorder_t* crec)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_picture_get_recording_canvas (sk_picture_recorder_t crec);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_canvas_t sk_picture_get_recording_canvas (sk_picture_recorder_t crec);
		}
		private static Delegates.sk_picture_get_recording_canvas sk_picture_get_recording_canvas_delegate;
		internal static sk_canvas_t sk_picture_get_recording_canvas (sk_picture_recorder_t crec) =>
			(sk_picture_get_recording_canvas_delegate ??= GetSymbol<Delegates.sk_picture_get_recording_canvas> ("sk_picture_get_recording_canvas")).Invoke (crec);
		#endif

		// uint32_t sk_picture_get_unique_id(sk_picture_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_picture_get_unique_id (sk_picture_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_picture_get_unique_id (sk_picture_t param0);
		}
		private static Delegates.sk_picture_get_unique_id sk_picture_get_unique_id_delegate;
		internal static UInt32 sk_picture_get_unique_id (sk_picture_t param0) =>
			(sk_picture_get_unique_id_delegate ??= GetSymbol<Delegates.sk_picture_get_unique_id> ("sk_picture_get_unique_id")).Invoke (param0);
		#endif

		// sk_shader_t* sk_picture_make_shader(sk_picture_t* src, sk_shader_tilemode_t tmx, sk_shader_tilemode_t tmy, const sk_matrix_t* localMatrix, const sk_rect_t* tile)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_picture_make_shader (sk_picture_t src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix* localMatrix, SKRect* tile);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_picture_make_shader (sk_picture_t src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix* localMatrix, SKRect* tile);
		}
		private static Delegates.sk_picture_make_shader sk_picture_make_shader_delegate;
		internal static sk_shader_t sk_picture_make_shader (sk_picture_t src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix* localMatrix, SKRect* tile) =>
			(sk_picture_make_shader_delegate ??= GetSymbol<Delegates.sk_picture_make_shader> ("sk_picture_make_shader")).Invoke (src, tmx, tmy, localMatrix, tile);
		#endif

		// sk_canvas_t* sk_picture_recorder_begin_recording(sk_picture_recorder_t*, const sk_rect_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_picture_recorder_begin_recording (sk_picture_recorder_t param0, SKRect* param1);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_canvas_t sk_picture_recorder_begin_recording (sk_picture_recorder_t param0, SKRect* param1);
		}
		private static Delegates.sk_picture_recorder_begin_recording sk_picture_recorder_begin_recording_delegate;
		internal static sk_canvas_t sk_picture_recorder_begin_recording (sk_picture_recorder_t param0, SKRect* param1) =>
			(sk_picture_recorder_begin_recording_delegate ??= GetSymbol<Delegates.sk_picture_recorder_begin_recording> ("sk_picture_recorder_begin_recording")).Invoke (param0, param1);
		#endif

		// void sk_picture_recorder_delete(sk_picture_recorder_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_recorder_delete (sk_picture_recorder_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_picture_recorder_delete (sk_picture_recorder_t param0);
		}
		private static Delegates.sk_picture_recorder_delete sk_picture_recorder_delete_delegate;
		internal static void sk_picture_recorder_delete (sk_picture_recorder_t param0) =>
			(sk_picture_recorder_delete_delegate ??= GetSymbol<Delegates.sk_picture_recorder_delete> ("sk_picture_recorder_delete")).Invoke (param0);
		#endif

		// sk_picture_t* sk_picture_recorder_end_recording(sk_picture_recorder_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_picture_t sk_picture_recorder_end_recording (sk_picture_recorder_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_picture_t sk_picture_recorder_end_recording (sk_picture_recorder_t param0);
		}
		private static Delegates.sk_picture_recorder_end_recording sk_picture_recorder_end_recording_delegate;
		internal static sk_picture_t sk_picture_recorder_end_recording (sk_picture_recorder_t param0) =>
			(sk_picture_recorder_end_recording_delegate ??= GetSymbol<Delegates.sk_picture_recorder_end_recording> ("sk_picture_recorder_end_recording")).Invoke (param0);
		#endif

		// sk_drawable_t* sk_picture_recorder_end_recording_as_drawable(sk_picture_recorder_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_drawable_t sk_picture_recorder_end_recording_as_drawable (sk_picture_recorder_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_drawable_t sk_picture_recorder_end_recording_as_drawable (sk_picture_recorder_t param0);
		}
		private static Delegates.sk_picture_recorder_end_recording_as_drawable sk_picture_recorder_end_recording_as_drawable_delegate;
		internal static sk_drawable_t sk_picture_recorder_end_recording_as_drawable (sk_picture_recorder_t param0) =>
			(sk_picture_recorder_end_recording_as_drawable_delegate ??= GetSymbol<Delegates.sk_picture_recorder_end_recording_as_drawable> ("sk_picture_recorder_end_recording_as_drawable")).Invoke (param0);
		#endif

		// sk_picture_recorder_t* sk_picture_recorder_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_picture_recorder_t sk_picture_recorder_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_picture_recorder_t sk_picture_recorder_new ();
		}
		private static Delegates.sk_picture_recorder_new sk_picture_recorder_new_delegate;
		internal static sk_picture_recorder_t sk_picture_recorder_new () =>
			(sk_picture_recorder_new_delegate ??= GetSymbol<Delegates.sk_picture_recorder_new> ("sk_picture_recorder_new")).Invoke ();
		#endif

		// void sk_picture_ref(sk_picture_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_ref (sk_picture_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_picture_ref (sk_picture_t param0);
		}
		private static Delegates.sk_picture_ref sk_picture_ref_delegate;
		internal static void sk_picture_ref (sk_picture_t param0) =>
			(sk_picture_ref_delegate ??= GetSymbol<Delegates.sk_picture_ref> ("sk_picture_ref")).Invoke (param0);
		#endif

		// sk_data_t* sk_picture_serialize_to_data(const sk_picture_t* picture)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_picture_serialize_to_data (sk_picture_t picture);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_picture_serialize_to_data (sk_picture_t picture);
		}
		private static Delegates.sk_picture_serialize_to_data sk_picture_serialize_to_data_delegate;
		internal static sk_data_t sk_picture_serialize_to_data (sk_picture_t picture) =>
			(sk_picture_serialize_to_data_delegate ??= GetSymbol<Delegates.sk_picture_serialize_to_data> ("sk_picture_serialize_to_data")).Invoke (picture);
		#endif

		// void sk_picture_serialize_to_stream(const sk_picture_t* picture, sk_wstream_t* stream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_serialize_to_stream (sk_picture_t picture, sk_wstream_t stream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_picture_serialize_to_stream (sk_picture_t picture, sk_wstream_t stream);
		}
		private static Delegates.sk_picture_serialize_to_stream sk_picture_serialize_to_stream_delegate;
		internal static void sk_picture_serialize_to_stream (sk_picture_t picture, sk_wstream_t stream) =>
			(sk_picture_serialize_to_stream_delegate ??= GetSymbol<Delegates.sk_picture_serialize_to_stream> ("sk_picture_serialize_to_stream")).Invoke (picture, stream);
		#endif

		// void sk_picture_unref(sk_picture_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_unref (sk_picture_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_picture_unref (sk_picture_t param0);
		}
		private static Delegates.sk_picture_unref sk_picture_unref_delegate;
		internal static void sk_picture_unref (sk_picture_t param0) =>
			(sk_picture_unref_delegate ??= GetSymbol<Delegates.sk_picture_unref> ("sk_picture_unref")).Invoke (param0);
		#endif

		#endregion

		#region sk_pixmap.h

		// void sk_color_get_bit_shift(int* a, int* r, int* g, int* b)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_color_get_bit_shift (Int32* a, Int32* r, Int32* g, Int32* b);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_color_get_bit_shift (Int32* a, Int32* r, Int32* g, Int32* b);
		}
		private static Delegates.sk_color_get_bit_shift sk_color_get_bit_shift_delegate;
		internal static void sk_color_get_bit_shift (Int32* a, Int32* r, Int32* g, Int32* b) =>
			(sk_color_get_bit_shift_delegate ??= GetSymbol<Delegates.sk_color_get_bit_shift> ("sk_color_get_bit_shift")).Invoke (a, r, g, b);
		#endif

		// sk_pmcolor_t sk_color_premultiply(const sk_color_t color)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_color_premultiply (UInt32 color);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_color_premultiply (UInt32 color);
		}
		private static Delegates.sk_color_premultiply sk_color_premultiply_delegate;
		internal static UInt32 sk_color_premultiply (UInt32 color) =>
			(sk_color_premultiply_delegate ??= GetSymbol<Delegates.sk_color_premultiply> ("sk_color_premultiply")).Invoke (color);
		#endif

		// void sk_color_premultiply_array(const sk_color_t* colors, int size, sk_pmcolor_t* pmcolors)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_color_premultiply_array (UInt32* colors, Int32 size, UInt32* pmcolors);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_color_premultiply_array (UInt32* colors, Int32 size, UInt32* pmcolors);
		}
		private static Delegates.sk_color_premultiply_array sk_color_premultiply_array_delegate;
		internal static void sk_color_premultiply_array (UInt32* colors, Int32 size, UInt32* pmcolors) =>
			(sk_color_premultiply_array_delegate ??= GetSymbol<Delegates.sk_color_premultiply_array> ("sk_color_premultiply_array")).Invoke (colors, size, pmcolors);
		#endif

		// sk_color_t sk_color_unpremultiply(const sk_pmcolor_t pmcolor)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_color_unpremultiply (UInt32 pmcolor);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_color_unpremultiply (UInt32 pmcolor);
		}
		private static Delegates.sk_color_unpremultiply sk_color_unpremultiply_delegate;
		internal static UInt32 sk_color_unpremultiply (UInt32 pmcolor) =>
			(sk_color_unpremultiply_delegate ??= GetSymbol<Delegates.sk_color_unpremultiply> ("sk_color_unpremultiply")).Invoke (pmcolor);
		#endif

		// void sk_color_unpremultiply_array(const sk_pmcolor_t* pmcolors, int size, sk_color_t* colors)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_color_unpremultiply_array (UInt32* pmcolors, Int32 size, UInt32* colors);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_color_unpremultiply_array (UInt32* pmcolors, Int32 size, UInt32* colors);
		}
		private static Delegates.sk_color_unpremultiply_array sk_color_unpremultiply_array_delegate;
		internal static void sk_color_unpremultiply_array (UInt32* pmcolors, Int32 size, UInt32* colors) =>
			(sk_color_unpremultiply_array_delegate ??= GetSymbol<Delegates.sk_color_unpremultiply_array> ("sk_color_unpremultiply_array")).Invoke (pmcolors, size, colors);
		#endif

		// bool sk_jpegencoder_encode(sk_wstream_t* dst, const sk_pixmap_t* src, const sk_jpegencoder_options_t* options)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_jpegencoder_encode (sk_wstream_t dst, sk_pixmap_t src, SKJpegEncoderOptions* options);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_jpegencoder_encode (sk_wstream_t dst, sk_pixmap_t src, SKJpegEncoderOptions* options);
		}
		private static Delegates.sk_jpegencoder_encode sk_jpegencoder_encode_delegate;
		internal static bool sk_jpegencoder_encode (sk_wstream_t dst, sk_pixmap_t src, SKJpegEncoderOptions* options) =>
			(sk_jpegencoder_encode_delegate ??= GetSymbol<Delegates.sk_jpegencoder_encode> ("sk_jpegencoder_encode")).Invoke (dst, src, options);
		#endif

		// void sk_pixmap_destructor(sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pixmap_destructor (sk_pixmap_t cpixmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pixmap_destructor (sk_pixmap_t cpixmap);
		}
		private static Delegates.sk_pixmap_destructor sk_pixmap_destructor_delegate;
		internal static void sk_pixmap_destructor (sk_pixmap_t cpixmap) =>
			(sk_pixmap_destructor_delegate ??= GetSymbol<Delegates.sk_pixmap_destructor> ("sk_pixmap_destructor")).Invoke (cpixmap);
		#endif

		// bool sk_pixmap_encode_image(sk_wstream_t* dst, const sk_pixmap_t* src, sk_encoded_image_format_t encoder, int quality)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_encode_image (sk_wstream_t dst, sk_pixmap_t src, SKEncodedImageFormat encoder, Int32 quality);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pixmap_encode_image (sk_wstream_t dst, sk_pixmap_t src, SKEncodedImageFormat encoder, Int32 quality);
		}
		private static Delegates.sk_pixmap_encode_image sk_pixmap_encode_image_delegate;
		internal static bool sk_pixmap_encode_image (sk_wstream_t dst, sk_pixmap_t src, SKEncodedImageFormat encoder, Int32 quality) =>
			(sk_pixmap_encode_image_delegate ??= GetSymbol<Delegates.sk_pixmap_encode_image> ("sk_pixmap_encode_image")).Invoke (dst, src, encoder, quality);
		#endif

		// bool sk_pixmap_erase_color(const sk_pixmap_t* cpixmap, sk_color_t color, const sk_irect_t* subset)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_erase_color (sk_pixmap_t cpixmap, UInt32 color, SKRectI* subset);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pixmap_erase_color (sk_pixmap_t cpixmap, UInt32 color, SKRectI* subset);
		}
		private static Delegates.sk_pixmap_erase_color sk_pixmap_erase_color_delegate;
		internal static bool sk_pixmap_erase_color (sk_pixmap_t cpixmap, UInt32 color, SKRectI* subset) =>
			(sk_pixmap_erase_color_delegate ??= GetSymbol<Delegates.sk_pixmap_erase_color> ("sk_pixmap_erase_color")).Invoke (cpixmap, color, subset);
		#endif

		// bool sk_pixmap_erase_color4f(const sk_pixmap_t* cpixmap, const sk_color4f_t* color, sk_colorspace_t* colorspace, const sk_irect_t* subset)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_erase_color4f (sk_pixmap_t cpixmap, SKColorF* color, sk_colorspace_t colorspace, SKRectI* subset);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pixmap_erase_color4f (sk_pixmap_t cpixmap, SKColorF* color, sk_colorspace_t colorspace, SKRectI* subset);
		}
		private static Delegates.sk_pixmap_erase_color4f sk_pixmap_erase_color4f_delegate;
		internal static bool sk_pixmap_erase_color4f (sk_pixmap_t cpixmap, SKColorF* color, sk_colorspace_t colorspace, SKRectI* subset) =>
			(sk_pixmap_erase_color4f_delegate ??= GetSymbol<Delegates.sk_pixmap_erase_color4f> ("sk_pixmap_erase_color4f")).Invoke (cpixmap, color, colorspace, subset);
		#endif

		// bool sk_pixmap_extract_subset(const sk_pixmap_t* cpixmap, sk_pixmap_t* result, const sk_irect_t* subset)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_extract_subset (sk_pixmap_t cpixmap, sk_pixmap_t result, SKRectI* subset);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pixmap_extract_subset (sk_pixmap_t cpixmap, sk_pixmap_t result, SKRectI* subset);
		}
		private static Delegates.sk_pixmap_extract_subset sk_pixmap_extract_subset_delegate;
		internal static bool sk_pixmap_extract_subset (sk_pixmap_t cpixmap, sk_pixmap_t result, SKRectI* subset) =>
			(sk_pixmap_extract_subset_delegate ??= GetSymbol<Delegates.sk_pixmap_extract_subset> ("sk_pixmap_extract_subset")).Invoke (cpixmap, result, subset);
		#endif

		// void sk_pixmap_get_info(const sk_pixmap_t* cpixmap, sk_imageinfo_t* cinfo)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pixmap_get_info (sk_pixmap_t cpixmap, SKImageInfoNative* cinfo);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pixmap_get_info (sk_pixmap_t cpixmap, SKImageInfoNative* cinfo);
		}
		private static Delegates.sk_pixmap_get_info sk_pixmap_get_info_delegate;
		internal static void sk_pixmap_get_info (sk_pixmap_t cpixmap, SKImageInfoNative* cinfo) =>
			(sk_pixmap_get_info_delegate ??= GetSymbol<Delegates.sk_pixmap_get_info> ("sk_pixmap_get_info")).Invoke (cpixmap, cinfo);
		#endif

		// sk_color_t sk_pixmap_get_pixel_color(const sk_pixmap_t* cpixmap, int x, int y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_pixmap_get_pixel_color (sk_pixmap_t cpixmap, Int32 x, Int32 y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_pixmap_get_pixel_color (sk_pixmap_t cpixmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_pixmap_get_pixel_color sk_pixmap_get_pixel_color_delegate;
		internal static UInt32 sk_pixmap_get_pixel_color (sk_pixmap_t cpixmap, Int32 x, Int32 y) =>
			(sk_pixmap_get_pixel_color_delegate ??= GetSymbol<Delegates.sk_pixmap_get_pixel_color> ("sk_pixmap_get_pixel_color")).Invoke (cpixmap, x, y);
		#endif

		// const void* sk_pixmap_get_pixels(const sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_pixmap_get_pixels (sk_pixmap_t cpixmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_pixmap_get_pixels (sk_pixmap_t cpixmap);
		}
		private static Delegates.sk_pixmap_get_pixels sk_pixmap_get_pixels_delegate;
		internal static void* sk_pixmap_get_pixels (sk_pixmap_t cpixmap) =>
			(sk_pixmap_get_pixels_delegate ??= GetSymbol<Delegates.sk_pixmap_get_pixels> ("sk_pixmap_get_pixels")).Invoke (cpixmap);
		#endif

		// const void* sk_pixmap_get_pixels_with_xy(const sk_pixmap_t* cpixmap, int x, int y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_pixmap_get_pixels_with_xy (sk_pixmap_t cpixmap, Int32 x, Int32 y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_pixmap_get_pixels_with_xy (sk_pixmap_t cpixmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_pixmap_get_pixels_with_xy sk_pixmap_get_pixels_with_xy_delegate;
		internal static void* sk_pixmap_get_pixels_with_xy (sk_pixmap_t cpixmap, Int32 x, Int32 y) =>
			(sk_pixmap_get_pixels_with_xy_delegate ??= GetSymbol<Delegates.sk_pixmap_get_pixels_with_xy> ("sk_pixmap_get_pixels_with_xy")).Invoke (cpixmap, x, y);
		#endif

		// size_t sk_pixmap_get_row_bytes(const sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_pixmap_get_row_bytes (sk_pixmap_t cpixmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_pixmap_get_row_bytes (sk_pixmap_t cpixmap);
		}
		private static Delegates.sk_pixmap_get_row_bytes sk_pixmap_get_row_bytes_delegate;
		internal static /* size_t */ IntPtr sk_pixmap_get_row_bytes (sk_pixmap_t cpixmap) =>
			(sk_pixmap_get_row_bytes_delegate ??= GetSymbol<Delegates.sk_pixmap_get_row_bytes> ("sk_pixmap_get_row_bytes")).Invoke (cpixmap);
		#endif

		// void* sk_pixmap_get_writable_addr(const sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_pixmap_get_writable_addr (sk_pixmap_t cpixmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_pixmap_get_writable_addr (sk_pixmap_t cpixmap);
		}
		private static Delegates.sk_pixmap_get_writable_addr sk_pixmap_get_writable_addr_delegate;
		internal static void* sk_pixmap_get_writable_addr (sk_pixmap_t cpixmap) =>
			(sk_pixmap_get_writable_addr_delegate ??= GetSymbol<Delegates.sk_pixmap_get_writable_addr> ("sk_pixmap_get_writable_addr")).Invoke (cpixmap);
		#endif

		// sk_pixmap_t* sk_pixmap_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_pixmap_t sk_pixmap_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_pixmap_t sk_pixmap_new ();
		}
		private static Delegates.sk_pixmap_new sk_pixmap_new_delegate;
		internal static sk_pixmap_t sk_pixmap_new () =>
			(sk_pixmap_new_delegate ??= GetSymbol<Delegates.sk_pixmap_new> ("sk_pixmap_new")).Invoke ();
		#endif

		// sk_pixmap_t* sk_pixmap_new_with_params(const sk_imageinfo_t* cinfo, const void* addr, size_t rowBytes)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_pixmap_t sk_pixmap_new_with_params (SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_pixmap_t sk_pixmap_new_with_params (SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes);
		}
		private static Delegates.sk_pixmap_new_with_params sk_pixmap_new_with_params_delegate;
		internal static sk_pixmap_t sk_pixmap_new_with_params (SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes) =>
			(sk_pixmap_new_with_params_delegate ??= GetSymbol<Delegates.sk_pixmap_new_with_params> ("sk_pixmap_new_with_params")).Invoke (cinfo, addr, rowBytes);
		#endif

		// bool sk_pixmap_read_pixels(const sk_pixmap_t* cpixmap, const sk_imageinfo_t* dstInfo, void* dstPixels, size_t dstRowBytes, int srcX, int srcY)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_read_pixels (sk_pixmap_t cpixmap, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pixmap_read_pixels (sk_pixmap_t cpixmap, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY);
		}
		private static Delegates.sk_pixmap_read_pixels sk_pixmap_read_pixels_delegate;
		internal static bool sk_pixmap_read_pixels (sk_pixmap_t cpixmap, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY) =>
			(sk_pixmap_read_pixels_delegate ??= GetSymbol<Delegates.sk_pixmap_read_pixels> ("sk_pixmap_read_pixels")).Invoke (cpixmap, dstInfo, dstPixels, dstRowBytes, srcX, srcY);
		#endif

		// void sk_pixmap_reset(sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pixmap_reset (sk_pixmap_t cpixmap);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pixmap_reset (sk_pixmap_t cpixmap);
		}
		private static Delegates.sk_pixmap_reset sk_pixmap_reset_delegate;
		internal static void sk_pixmap_reset (sk_pixmap_t cpixmap) =>
			(sk_pixmap_reset_delegate ??= GetSymbol<Delegates.sk_pixmap_reset> ("sk_pixmap_reset")).Invoke (cpixmap);
		#endif

		// void sk_pixmap_reset_with_params(sk_pixmap_t* cpixmap, const sk_imageinfo_t* cinfo, const void* addr, size_t rowBytes)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pixmap_reset_with_params (sk_pixmap_t cpixmap, SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pixmap_reset_with_params (sk_pixmap_t cpixmap, SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes);
		}
		private static Delegates.sk_pixmap_reset_with_params sk_pixmap_reset_with_params_delegate;
		internal static void sk_pixmap_reset_with_params (sk_pixmap_t cpixmap, SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes) =>
			(sk_pixmap_reset_with_params_delegate ??= GetSymbol<Delegates.sk_pixmap_reset_with_params> ("sk_pixmap_reset_with_params")).Invoke (cpixmap, cinfo, addr, rowBytes);
		#endif

		// bool sk_pixmap_scale_pixels(const sk_pixmap_t* cpixmap, const sk_pixmap_t* dst, sk_filter_quality_t quality)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_scale_pixels (sk_pixmap_t cpixmap, sk_pixmap_t dst, SKFilterQuality quality);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pixmap_scale_pixels (sk_pixmap_t cpixmap, sk_pixmap_t dst, SKFilterQuality quality);
		}
		private static Delegates.sk_pixmap_scale_pixels sk_pixmap_scale_pixels_delegate;
		internal static bool sk_pixmap_scale_pixels (sk_pixmap_t cpixmap, sk_pixmap_t dst, SKFilterQuality quality) =>
			(sk_pixmap_scale_pixels_delegate ??= GetSymbol<Delegates.sk_pixmap_scale_pixels> ("sk_pixmap_scale_pixels")).Invoke (cpixmap, dst, quality);
		#endif

		// bool sk_pngencoder_encode(sk_wstream_t* dst, const sk_pixmap_t* src, const sk_pngencoder_options_t* options)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pngencoder_encode (sk_wstream_t dst, sk_pixmap_t src, SKPngEncoderOptions* options);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pngencoder_encode (sk_wstream_t dst, sk_pixmap_t src, SKPngEncoderOptions* options);
		}
		private static Delegates.sk_pngencoder_encode sk_pngencoder_encode_delegate;
		internal static bool sk_pngencoder_encode (sk_wstream_t dst, sk_pixmap_t src, SKPngEncoderOptions* options) =>
			(sk_pngencoder_encode_delegate ??= GetSymbol<Delegates.sk_pngencoder_encode> ("sk_pngencoder_encode")).Invoke (dst, src, options);
		#endif

		// void sk_swizzle_swap_rb(uint32_t* dest, const uint32_t* src, int count)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_swizzle_swap_rb (UInt32* dest, UInt32* src, Int32 count);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_swizzle_swap_rb (UInt32* dest, UInt32* src, Int32 count);
		}
		private static Delegates.sk_swizzle_swap_rb sk_swizzle_swap_rb_delegate;
		internal static void sk_swizzle_swap_rb (UInt32* dest, UInt32* src, Int32 count) =>
			(sk_swizzle_swap_rb_delegate ??= GetSymbol<Delegates.sk_swizzle_swap_rb> ("sk_swizzle_swap_rb")).Invoke (dest, src, count);
		#endif

		// bool sk_webpencoder_encode(sk_wstream_t* dst, const sk_pixmap_t* src, const sk_webpencoder_options_t* options)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_webpencoder_encode (sk_wstream_t dst, sk_pixmap_t src, SKWebpEncoderOptions* options);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_webpencoder_encode (sk_wstream_t dst, sk_pixmap_t src, SKWebpEncoderOptions* options);
		}
		private static Delegates.sk_webpencoder_encode sk_webpencoder_encode_delegate;
		internal static bool sk_webpencoder_encode (sk_wstream_t dst, sk_pixmap_t src, SKWebpEncoderOptions* options) =>
			(sk_webpencoder_encode_delegate ??= GetSymbol<Delegates.sk_webpencoder_encode> ("sk_webpencoder_encode")).Invoke (dst, src, options);
		#endif

		#endregion

		#region sk_region.h

		// void sk_region_cliperator_delete(sk_region_cliperator_t* iter)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_cliperator_delete (sk_region_cliperator_t iter);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_cliperator_delete (sk_region_cliperator_t iter);
		}
		private static Delegates.sk_region_cliperator_delete sk_region_cliperator_delete_delegate;
		internal static void sk_region_cliperator_delete (sk_region_cliperator_t iter) =>
			(sk_region_cliperator_delete_delegate ??= GetSymbol<Delegates.sk_region_cliperator_delete> ("sk_region_cliperator_delete")).Invoke (iter);
		#endif

		// bool sk_region_cliperator_done(sk_region_cliperator_t* iter)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_cliperator_done (sk_region_cliperator_t iter);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_cliperator_done (sk_region_cliperator_t iter);
		}
		private static Delegates.sk_region_cliperator_done sk_region_cliperator_done_delegate;
		internal static bool sk_region_cliperator_done (sk_region_cliperator_t iter) =>
			(sk_region_cliperator_done_delegate ??= GetSymbol<Delegates.sk_region_cliperator_done> ("sk_region_cliperator_done")).Invoke (iter);
		#endif

		// sk_region_cliperator_t* sk_region_cliperator_new(const sk_region_t* region, const sk_irect_t* clip)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_region_cliperator_t sk_region_cliperator_new (sk_region_t region, SKRectI* clip);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_region_cliperator_t sk_region_cliperator_new (sk_region_t region, SKRectI* clip);
		}
		private static Delegates.sk_region_cliperator_new sk_region_cliperator_new_delegate;
		internal static sk_region_cliperator_t sk_region_cliperator_new (sk_region_t region, SKRectI* clip) =>
			(sk_region_cliperator_new_delegate ??= GetSymbol<Delegates.sk_region_cliperator_new> ("sk_region_cliperator_new")).Invoke (region, clip);
		#endif

		// void sk_region_cliperator_next(sk_region_cliperator_t* iter)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_cliperator_next (sk_region_cliperator_t iter);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_cliperator_next (sk_region_cliperator_t iter);
		}
		private static Delegates.sk_region_cliperator_next sk_region_cliperator_next_delegate;
		internal static void sk_region_cliperator_next (sk_region_cliperator_t iter) =>
			(sk_region_cliperator_next_delegate ??= GetSymbol<Delegates.sk_region_cliperator_next> ("sk_region_cliperator_next")).Invoke (iter);
		#endif

		// void sk_region_cliperator_rect(const sk_region_cliperator_t* iter, sk_irect_t* rect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_cliperator_rect (sk_region_cliperator_t iter, SKRectI* rect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_cliperator_rect (sk_region_cliperator_t iter, SKRectI* rect);
		}
		private static Delegates.sk_region_cliperator_rect sk_region_cliperator_rect_delegate;
		internal static void sk_region_cliperator_rect (sk_region_cliperator_t iter, SKRectI* rect) =>
			(sk_region_cliperator_rect_delegate ??= GetSymbol<Delegates.sk_region_cliperator_rect> ("sk_region_cliperator_rect")).Invoke (iter, rect);
		#endif

		// bool sk_region_contains(const sk_region_t* r, const sk_region_t* region)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_contains (sk_region_t r, sk_region_t region);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_contains (sk_region_t r, sk_region_t region);
		}
		private static Delegates.sk_region_contains sk_region_contains_delegate;
		internal static bool sk_region_contains (sk_region_t r, sk_region_t region) =>
			(sk_region_contains_delegate ??= GetSymbol<Delegates.sk_region_contains> ("sk_region_contains")).Invoke (r, region);
		#endif

		// bool sk_region_contains_point(const sk_region_t* r, int x, int y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_contains_point (sk_region_t r, Int32 x, Int32 y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_contains_point (sk_region_t r, Int32 x, Int32 y);
		}
		private static Delegates.sk_region_contains_point sk_region_contains_point_delegate;
		internal static bool sk_region_contains_point (sk_region_t r, Int32 x, Int32 y) =>
			(sk_region_contains_point_delegate ??= GetSymbol<Delegates.sk_region_contains_point> ("sk_region_contains_point")).Invoke (r, x, y);
		#endif

		// bool sk_region_contains_rect(const sk_region_t* r, const sk_irect_t* rect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_contains_rect (sk_region_t r, SKRectI* rect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_contains_rect (sk_region_t r, SKRectI* rect);
		}
		private static Delegates.sk_region_contains_rect sk_region_contains_rect_delegate;
		internal static bool sk_region_contains_rect (sk_region_t r, SKRectI* rect) =>
			(sk_region_contains_rect_delegate ??= GetSymbol<Delegates.sk_region_contains_rect> ("sk_region_contains_rect")).Invoke (r, rect);
		#endif

		// void sk_region_delete(sk_region_t* r)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_delete (sk_region_t r);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_delete (sk_region_t r);
		}
		private static Delegates.sk_region_delete sk_region_delete_delegate;
		internal static void sk_region_delete (sk_region_t r) =>
			(sk_region_delete_delegate ??= GetSymbol<Delegates.sk_region_delete> ("sk_region_delete")).Invoke (r);
		#endif

		// bool sk_region_get_boundary_path(const sk_region_t* r, sk_path_t* path)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_get_boundary_path (sk_region_t r, sk_path_t path);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_get_boundary_path (sk_region_t r, sk_path_t path);
		}
		private static Delegates.sk_region_get_boundary_path sk_region_get_boundary_path_delegate;
		internal static bool sk_region_get_boundary_path (sk_region_t r, sk_path_t path) =>
			(sk_region_get_boundary_path_delegate ??= GetSymbol<Delegates.sk_region_get_boundary_path> ("sk_region_get_boundary_path")).Invoke (r, path);
		#endif

		// void sk_region_get_bounds(const sk_region_t* r, sk_irect_t* rect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_get_bounds (sk_region_t r, SKRectI* rect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_get_bounds (sk_region_t r, SKRectI* rect);
		}
		private static Delegates.sk_region_get_bounds sk_region_get_bounds_delegate;
		internal static void sk_region_get_bounds (sk_region_t r, SKRectI* rect) =>
			(sk_region_get_bounds_delegate ??= GetSymbol<Delegates.sk_region_get_bounds> ("sk_region_get_bounds")).Invoke (r, rect);
		#endif

		// bool sk_region_intersects(const sk_region_t* r, const sk_region_t* src)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_intersects (sk_region_t r, sk_region_t src);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_intersects (sk_region_t r, sk_region_t src);
		}
		private static Delegates.sk_region_intersects sk_region_intersects_delegate;
		internal static bool sk_region_intersects (sk_region_t r, sk_region_t src) =>
			(sk_region_intersects_delegate ??= GetSymbol<Delegates.sk_region_intersects> ("sk_region_intersects")).Invoke (r, src);
		#endif

		// bool sk_region_intersects_rect(const sk_region_t* r, const sk_irect_t* rect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_intersects_rect (sk_region_t r, SKRectI* rect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_intersects_rect (sk_region_t r, SKRectI* rect);
		}
		private static Delegates.sk_region_intersects_rect sk_region_intersects_rect_delegate;
		internal static bool sk_region_intersects_rect (sk_region_t r, SKRectI* rect) =>
			(sk_region_intersects_rect_delegate ??= GetSymbol<Delegates.sk_region_intersects_rect> ("sk_region_intersects_rect")).Invoke (r, rect);
		#endif

		// bool sk_region_is_complex(const sk_region_t* r)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_is_complex (sk_region_t r);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_is_complex (sk_region_t r);
		}
		private static Delegates.sk_region_is_complex sk_region_is_complex_delegate;
		internal static bool sk_region_is_complex (sk_region_t r) =>
			(sk_region_is_complex_delegate ??= GetSymbol<Delegates.sk_region_is_complex> ("sk_region_is_complex")).Invoke (r);
		#endif

		// bool sk_region_is_empty(const sk_region_t* r)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_is_empty (sk_region_t r);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_is_empty (sk_region_t r);
		}
		private static Delegates.sk_region_is_empty sk_region_is_empty_delegate;
		internal static bool sk_region_is_empty (sk_region_t r) =>
			(sk_region_is_empty_delegate ??= GetSymbol<Delegates.sk_region_is_empty> ("sk_region_is_empty")).Invoke (r);
		#endif

		// bool sk_region_is_rect(const sk_region_t* r)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_is_rect (sk_region_t r);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_is_rect (sk_region_t r);
		}
		private static Delegates.sk_region_is_rect sk_region_is_rect_delegate;
		internal static bool sk_region_is_rect (sk_region_t r) =>
			(sk_region_is_rect_delegate ??= GetSymbol<Delegates.sk_region_is_rect> ("sk_region_is_rect")).Invoke (r);
		#endif

		// void sk_region_iterator_delete(sk_region_iterator_t* iter)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_iterator_delete (sk_region_iterator_t iter);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_iterator_delete (sk_region_iterator_t iter);
		}
		private static Delegates.sk_region_iterator_delete sk_region_iterator_delete_delegate;
		internal static void sk_region_iterator_delete (sk_region_iterator_t iter) =>
			(sk_region_iterator_delete_delegate ??= GetSymbol<Delegates.sk_region_iterator_delete> ("sk_region_iterator_delete")).Invoke (iter);
		#endif

		// bool sk_region_iterator_done(const sk_region_iterator_t* iter)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_iterator_done (sk_region_iterator_t iter);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_iterator_done (sk_region_iterator_t iter);
		}
		private static Delegates.sk_region_iterator_done sk_region_iterator_done_delegate;
		internal static bool sk_region_iterator_done (sk_region_iterator_t iter) =>
			(sk_region_iterator_done_delegate ??= GetSymbol<Delegates.sk_region_iterator_done> ("sk_region_iterator_done")).Invoke (iter);
		#endif

		// sk_region_iterator_t* sk_region_iterator_new(const sk_region_t* region)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_region_iterator_t sk_region_iterator_new (sk_region_t region);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_region_iterator_t sk_region_iterator_new (sk_region_t region);
		}
		private static Delegates.sk_region_iterator_new sk_region_iterator_new_delegate;
		internal static sk_region_iterator_t sk_region_iterator_new (sk_region_t region) =>
			(sk_region_iterator_new_delegate ??= GetSymbol<Delegates.sk_region_iterator_new> ("sk_region_iterator_new")).Invoke (region);
		#endif

		// void sk_region_iterator_next(sk_region_iterator_t* iter)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_iterator_next (sk_region_iterator_t iter);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_iterator_next (sk_region_iterator_t iter);
		}
		private static Delegates.sk_region_iterator_next sk_region_iterator_next_delegate;
		internal static void sk_region_iterator_next (sk_region_iterator_t iter) =>
			(sk_region_iterator_next_delegate ??= GetSymbol<Delegates.sk_region_iterator_next> ("sk_region_iterator_next")).Invoke (iter);
		#endif

		// void sk_region_iterator_rect(const sk_region_iterator_t* iter, sk_irect_t* rect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_iterator_rect (sk_region_iterator_t iter, SKRectI* rect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_iterator_rect (sk_region_iterator_t iter, SKRectI* rect);
		}
		private static Delegates.sk_region_iterator_rect sk_region_iterator_rect_delegate;
		internal static void sk_region_iterator_rect (sk_region_iterator_t iter, SKRectI* rect) =>
			(sk_region_iterator_rect_delegate ??= GetSymbol<Delegates.sk_region_iterator_rect> ("sk_region_iterator_rect")).Invoke (iter, rect);
		#endif

		// bool sk_region_iterator_rewind(sk_region_iterator_t* iter)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_iterator_rewind (sk_region_iterator_t iter);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_iterator_rewind (sk_region_iterator_t iter);
		}
		private static Delegates.sk_region_iterator_rewind sk_region_iterator_rewind_delegate;
		internal static bool sk_region_iterator_rewind (sk_region_iterator_t iter) =>
			(sk_region_iterator_rewind_delegate ??= GetSymbol<Delegates.sk_region_iterator_rewind> ("sk_region_iterator_rewind")).Invoke (iter);
		#endif

		// sk_region_t* sk_region_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_region_t sk_region_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_region_t sk_region_new ();
		}
		private static Delegates.sk_region_new sk_region_new_delegate;
		internal static sk_region_t sk_region_new () =>
			(sk_region_new_delegate ??= GetSymbol<Delegates.sk_region_new> ("sk_region_new")).Invoke ();
		#endif

		// bool sk_region_op(sk_region_t* r, const sk_region_t* region, sk_region_op_t op)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_op (sk_region_t r, sk_region_t region, SKRegionOperation op);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_op (sk_region_t r, sk_region_t region, SKRegionOperation op);
		}
		private static Delegates.sk_region_op sk_region_op_delegate;
		internal static bool sk_region_op (sk_region_t r, sk_region_t region, SKRegionOperation op) =>
			(sk_region_op_delegate ??= GetSymbol<Delegates.sk_region_op> ("sk_region_op")).Invoke (r, region, op);
		#endif

		// bool sk_region_op_rect(sk_region_t* r, const sk_irect_t* rect, sk_region_op_t op)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_op_rect (sk_region_t r, SKRectI* rect, SKRegionOperation op);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_op_rect (sk_region_t r, SKRectI* rect, SKRegionOperation op);
		}
		private static Delegates.sk_region_op_rect sk_region_op_rect_delegate;
		internal static bool sk_region_op_rect (sk_region_t r, SKRectI* rect, SKRegionOperation op) =>
			(sk_region_op_rect_delegate ??= GetSymbol<Delegates.sk_region_op_rect> ("sk_region_op_rect")).Invoke (r, rect, op);
		#endif

		// bool sk_region_quick_contains(const sk_region_t* r, const sk_irect_t* rect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_quick_contains (sk_region_t r, SKRectI* rect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_quick_contains (sk_region_t r, SKRectI* rect);
		}
		private static Delegates.sk_region_quick_contains sk_region_quick_contains_delegate;
		internal static bool sk_region_quick_contains (sk_region_t r, SKRectI* rect) =>
			(sk_region_quick_contains_delegate ??= GetSymbol<Delegates.sk_region_quick_contains> ("sk_region_quick_contains")).Invoke (r, rect);
		#endif

		// bool sk_region_quick_reject(const sk_region_t* r, const sk_region_t* region)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_quick_reject (sk_region_t r, sk_region_t region);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_quick_reject (sk_region_t r, sk_region_t region);
		}
		private static Delegates.sk_region_quick_reject sk_region_quick_reject_delegate;
		internal static bool sk_region_quick_reject (sk_region_t r, sk_region_t region) =>
			(sk_region_quick_reject_delegate ??= GetSymbol<Delegates.sk_region_quick_reject> ("sk_region_quick_reject")).Invoke (r, region);
		#endif

		// bool sk_region_quick_reject_rect(const sk_region_t* r, const sk_irect_t* rect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_quick_reject_rect (sk_region_t r, SKRectI* rect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_quick_reject_rect (sk_region_t r, SKRectI* rect);
		}
		private static Delegates.sk_region_quick_reject_rect sk_region_quick_reject_rect_delegate;
		internal static bool sk_region_quick_reject_rect (sk_region_t r, SKRectI* rect) =>
			(sk_region_quick_reject_rect_delegate ??= GetSymbol<Delegates.sk_region_quick_reject_rect> ("sk_region_quick_reject_rect")).Invoke (r, rect);
		#endif

		// bool sk_region_set_empty(sk_region_t* r)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_empty (sk_region_t r);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_set_empty (sk_region_t r);
		}
		private static Delegates.sk_region_set_empty sk_region_set_empty_delegate;
		internal static bool sk_region_set_empty (sk_region_t r) =>
			(sk_region_set_empty_delegate ??= GetSymbol<Delegates.sk_region_set_empty> ("sk_region_set_empty")).Invoke (r);
		#endif

		// bool sk_region_set_path(sk_region_t* r, const sk_path_t* t, const sk_region_t* clip)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_path (sk_region_t r, sk_path_t t, sk_region_t clip);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_set_path (sk_region_t r, sk_path_t t, sk_region_t clip);
		}
		private static Delegates.sk_region_set_path sk_region_set_path_delegate;
		internal static bool sk_region_set_path (sk_region_t r, sk_path_t t, sk_region_t clip) =>
			(sk_region_set_path_delegate ??= GetSymbol<Delegates.sk_region_set_path> ("sk_region_set_path")).Invoke (r, t, clip);
		#endif

		// bool sk_region_set_rect(sk_region_t* r, const sk_irect_t* rect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_rect (sk_region_t r, SKRectI* rect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_set_rect (sk_region_t r, SKRectI* rect);
		}
		private static Delegates.sk_region_set_rect sk_region_set_rect_delegate;
		internal static bool sk_region_set_rect (sk_region_t r, SKRectI* rect) =>
			(sk_region_set_rect_delegate ??= GetSymbol<Delegates.sk_region_set_rect> ("sk_region_set_rect")).Invoke (r, rect);
		#endif

		// bool sk_region_set_rects(sk_region_t* r, const sk_irect_t* rects, int count)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_rects (sk_region_t r, SKRectI* rects, Int32 count);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_set_rects (sk_region_t r, SKRectI* rects, Int32 count);
		}
		private static Delegates.sk_region_set_rects sk_region_set_rects_delegate;
		internal static bool sk_region_set_rects (sk_region_t r, SKRectI* rects, Int32 count) =>
			(sk_region_set_rects_delegate ??= GetSymbol<Delegates.sk_region_set_rects> ("sk_region_set_rects")).Invoke (r, rects, count);
		#endif

		// bool sk_region_set_region(sk_region_t* r, const sk_region_t* region)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_region (sk_region_t r, sk_region_t region);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_set_region (sk_region_t r, sk_region_t region);
		}
		private static Delegates.sk_region_set_region sk_region_set_region_delegate;
		internal static bool sk_region_set_region (sk_region_t r, sk_region_t region) =>
			(sk_region_set_region_delegate ??= GetSymbol<Delegates.sk_region_set_region> ("sk_region_set_region")).Invoke (r, region);
		#endif

		// void sk_region_spanerator_delete(sk_region_spanerator_t* iter)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_spanerator_delete (sk_region_spanerator_t iter);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_spanerator_delete (sk_region_spanerator_t iter);
		}
		private static Delegates.sk_region_spanerator_delete sk_region_spanerator_delete_delegate;
		internal static void sk_region_spanerator_delete (sk_region_spanerator_t iter) =>
			(sk_region_spanerator_delete_delegate ??= GetSymbol<Delegates.sk_region_spanerator_delete> ("sk_region_spanerator_delete")).Invoke (iter);
		#endif

		// sk_region_spanerator_t* sk_region_spanerator_new(const sk_region_t* region, int y, int left, int right)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_region_spanerator_t sk_region_spanerator_new (sk_region_t region, Int32 y, Int32 left, Int32 right);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_region_spanerator_t sk_region_spanerator_new (sk_region_t region, Int32 y, Int32 left, Int32 right);
		}
		private static Delegates.sk_region_spanerator_new sk_region_spanerator_new_delegate;
		internal static sk_region_spanerator_t sk_region_spanerator_new (sk_region_t region, Int32 y, Int32 left, Int32 right) =>
			(sk_region_spanerator_new_delegate ??= GetSymbol<Delegates.sk_region_spanerator_new> ("sk_region_spanerator_new")).Invoke (region, y, left, right);
		#endif

		// bool sk_region_spanerator_next(sk_region_spanerator_t* iter, int* left, int* right)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_spanerator_next (sk_region_spanerator_t iter, Int32* left, Int32* right);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_spanerator_next (sk_region_spanerator_t iter, Int32* left, Int32* right);
		}
		private static Delegates.sk_region_spanerator_next sk_region_spanerator_next_delegate;
		internal static bool sk_region_spanerator_next (sk_region_spanerator_t iter, Int32* left, Int32* right) =>
			(sk_region_spanerator_next_delegate ??= GetSymbol<Delegates.sk_region_spanerator_next> ("sk_region_spanerator_next")).Invoke (iter, left, right);
		#endif

		// void sk_region_translate(sk_region_t* r, int x, int y)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_translate (sk_region_t r, Int32 x, Int32 y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_translate (sk_region_t r, Int32 x, Int32 y);
		}
		private static Delegates.sk_region_translate sk_region_translate_delegate;
		internal static void sk_region_translate (sk_region_t r, Int32 x, Int32 y) =>
			(sk_region_translate_delegate ??= GetSymbol<Delegates.sk_region_translate> ("sk_region_translate")).Invoke (r, x, y);
		#endif

		#endregion

		#region sk_rrect.h

		// bool sk_rrect_contains(const sk_rrect_t* rrect, const sk_rect_t* rect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_rrect_contains (sk_rrect_t rrect, SKRect* rect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_rrect_contains (sk_rrect_t rrect, SKRect* rect);
		}
		private static Delegates.sk_rrect_contains sk_rrect_contains_delegate;
		internal static bool sk_rrect_contains (sk_rrect_t rrect, SKRect* rect) =>
			(sk_rrect_contains_delegate ??= GetSymbol<Delegates.sk_rrect_contains> ("sk_rrect_contains")).Invoke (rrect, rect);
		#endif

		// void sk_rrect_delete(const sk_rrect_t* rrect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_delete (sk_rrect_t rrect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_delete (sk_rrect_t rrect);
		}
		private static Delegates.sk_rrect_delete sk_rrect_delete_delegate;
		internal static void sk_rrect_delete (sk_rrect_t rrect) =>
			(sk_rrect_delete_delegate ??= GetSymbol<Delegates.sk_rrect_delete> ("sk_rrect_delete")).Invoke (rrect);
		#endif

		// float sk_rrect_get_height(const sk_rrect_t* rrect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_rrect_get_height (sk_rrect_t rrect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_rrect_get_height (sk_rrect_t rrect);
		}
		private static Delegates.sk_rrect_get_height sk_rrect_get_height_delegate;
		internal static Single sk_rrect_get_height (sk_rrect_t rrect) =>
			(sk_rrect_get_height_delegate ??= GetSymbol<Delegates.sk_rrect_get_height> ("sk_rrect_get_height")).Invoke (rrect);
		#endif

		// void sk_rrect_get_radii(const sk_rrect_t* rrect, sk_rrect_corner_t corner, sk_vector_t* radii)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_get_radii (sk_rrect_t rrect, SKRoundRectCorner corner, SKPoint* radii);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_get_radii (sk_rrect_t rrect, SKRoundRectCorner corner, SKPoint* radii);
		}
		private static Delegates.sk_rrect_get_radii sk_rrect_get_radii_delegate;
		internal static void sk_rrect_get_radii (sk_rrect_t rrect, SKRoundRectCorner corner, SKPoint* radii) =>
			(sk_rrect_get_radii_delegate ??= GetSymbol<Delegates.sk_rrect_get_radii> ("sk_rrect_get_radii")).Invoke (rrect, corner, radii);
		#endif

		// void sk_rrect_get_rect(const sk_rrect_t* rrect, sk_rect_t* rect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_get_rect (sk_rrect_t rrect, SKRect* rect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_get_rect (sk_rrect_t rrect, SKRect* rect);
		}
		private static Delegates.sk_rrect_get_rect sk_rrect_get_rect_delegate;
		internal static void sk_rrect_get_rect (sk_rrect_t rrect, SKRect* rect) =>
			(sk_rrect_get_rect_delegate ??= GetSymbol<Delegates.sk_rrect_get_rect> ("sk_rrect_get_rect")).Invoke (rrect, rect);
		#endif

		// sk_rrect_type_t sk_rrect_get_type(const sk_rrect_t* rrect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKRoundRectType sk_rrect_get_type (sk_rrect_t rrect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKRoundRectType sk_rrect_get_type (sk_rrect_t rrect);
		}
		private static Delegates.sk_rrect_get_type sk_rrect_get_type_delegate;
		internal static SKRoundRectType sk_rrect_get_type (sk_rrect_t rrect) =>
			(sk_rrect_get_type_delegate ??= GetSymbol<Delegates.sk_rrect_get_type> ("sk_rrect_get_type")).Invoke (rrect);
		#endif

		// float sk_rrect_get_width(const sk_rrect_t* rrect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_rrect_get_width (sk_rrect_t rrect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_rrect_get_width (sk_rrect_t rrect);
		}
		private static Delegates.sk_rrect_get_width sk_rrect_get_width_delegate;
		internal static Single sk_rrect_get_width (sk_rrect_t rrect) =>
			(sk_rrect_get_width_delegate ??= GetSymbol<Delegates.sk_rrect_get_width> ("sk_rrect_get_width")).Invoke (rrect);
		#endif

		// void sk_rrect_inset(sk_rrect_t* rrect, float dx, float dy)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_inset (sk_rrect_t rrect, Single dx, Single dy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_inset (sk_rrect_t rrect, Single dx, Single dy);
		}
		private static Delegates.sk_rrect_inset sk_rrect_inset_delegate;
		internal static void sk_rrect_inset (sk_rrect_t rrect, Single dx, Single dy) =>
			(sk_rrect_inset_delegate ??= GetSymbol<Delegates.sk_rrect_inset> ("sk_rrect_inset")).Invoke (rrect, dx, dy);
		#endif

		// bool sk_rrect_is_valid(const sk_rrect_t* rrect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_rrect_is_valid (sk_rrect_t rrect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_rrect_is_valid (sk_rrect_t rrect);
		}
		private static Delegates.sk_rrect_is_valid sk_rrect_is_valid_delegate;
		internal static bool sk_rrect_is_valid (sk_rrect_t rrect) =>
			(sk_rrect_is_valid_delegate ??= GetSymbol<Delegates.sk_rrect_is_valid> ("sk_rrect_is_valid")).Invoke (rrect);
		#endif

		// sk_rrect_t* sk_rrect_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_rrect_t sk_rrect_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_rrect_t sk_rrect_new ();
		}
		private static Delegates.sk_rrect_new sk_rrect_new_delegate;
		internal static sk_rrect_t sk_rrect_new () =>
			(sk_rrect_new_delegate ??= GetSymbol<Delegates.sk_rrect_new> ("sk_rrect_new")).Invoke ();
		#endif

		// sk_rrect_t* sk_rrect_new_copy(const sk_rrect_t* rrect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_rrect_t sk_rrect_new_copy (sk_rrect_t rrect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_rrect_t sk_rrect_new_copy (sk_rrect_t rrect);
		}
		private static Delegates.sk_rrect_new_copy sk_rrect_new_copy_delegate;
		internal static sk_rrect_t sk_rrect_new_copy (sk_rrect_t rrect) =>
			(sk_rrect_new_copy_delegate ??= GetSymbol<Delegates.sk_rrect_new_copy> ("sk_rrect_new_copy")).Invoke (rrect);
		#endif

		// void sk_rrect_offset(sk_rrect_t* rrect, float dx, float dy)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_offset (sk_rrect_t rrect, Single dx, Single dy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_offset (sk_rrect_t rrect, Single dx, Single dy);
		}
		private static Delegates.sk_rrect_offset sk_rrect_offset_delegate;
		internal static void sk_rrect_offset (sk_rrect_t rrect, Single dx, Single dy) =>
			(sk_rrect_offset_delegate ??= GetSymbol<Delegates.sk_rrect_offset> ("sk_rrect_offset")).Invoke (rrect, dx, dy);
		#endif

		// void sk_rrect_outset(sk_rrect_t* rrect, float dx, float dy)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_outset (sk_rrect_t rrect, Single dx, Single dy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_outset (sk_rrect_t rrect, Single dx, Single dy);
		}
		private static Delegates.sk_rrect_outset sk_rrect_outset_delegate;
		internal static void sk_rrect_outset (sk_rrect_t rrect, Single dx, Single dy) =>
			(sk_rrect_outset_delegate ??= GetSymbol<Delegates.sk_rrect_outset> ("sk_rrect_outset")).Invoke (rrect, dx, dy);
		#endif

		// void sk_rrect_set_empty(sk_rrect_t* rrect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_empty (sk_rrect_t rrect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_set_empty (sk_rrect_t rrect);
		}
		private static Delegates.sk_rrect_set_empty sk_rrect_set_empty_delegate;
		internal static void sk_rrect_set_empty (sk_rrect_t rrect) =>
			(sk_rrect_set_empty_delegate ??= GetSymbol<Delegates.sk_rrect_set_empty> ("sk_rrect_set_empty")).Invoke (rrect);
		#endif

		// void sk_rrect_set_nine_patch(sk_rrect_t* rrect, const sk_rect_t* rect, float leftRad, float topRad, float rightRad, float bottomRad)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_nine_patch (sk_rrect_t rrect, SKRect* rect, Single leftRad, Single topRad, Single rightRad, Single bottomRad);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_set_nine_patch (sk_rrect_t rrect, SKRect* rect, Single leftRad, Single topRad, Single rightRad, Single bottomRad);
		}
		private static Delegates.sk_rrect_set_nine_patch sk_rrect_set_nine_patch_delegate;
		internal static void sk_rrect_set_nine_patch (sk_rrect_t rrect, SKRect* rect, Single leftRad, Single topRad, Single rightRad, Single bottomRad) =>
			(sk_rrect_set_nine_patch_delegate ??= GetSymbol<Delegates.sk_rrect_set_nine_patch> ("sk_rrect_set_nine_patch")).Invoke (rrect, rect, leftRad, topRad, rightRad, bottomRad);
		#endif

		// void sk_rrect_set_oval(sk_rrect_t* rrect, const sk_rect_t* rect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_oval (sk_rrect_t rrect, SKRect* rect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_set_oval (sk_rrect_t rrect, SKRect* rect);
		}
		private static Delegates.sk_rrect_set_oval sk_rrect_set_oval_delegate;
		internal static void sk_rrect_set_oval (sk_rrect_t rrect, SKRect* rect) =>
			(sk_rrect_set_oval_delegate ??= GetSymbol<Delegates.sk_rrect_set_oval> ("sk_rrect_set_oval")).Invoke (rrect, rect);
		#endif

		// void sk_rrect_set_rect(sk_rrect_t* rrect, const sk_rect_t* rect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_rect (sk_rrect_t rrect, SKRect* rect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_set_rect (sk_rrect_t rrect, SKRect* rect);
		}
		private static Delegates.sk_rrect_set_rect sk_rrect_set_rect_delegate;
		internal static void sk_rrect_set_rect (sk_rrect_t rrect, SKRect* rect) =>
			(sk_rrect_set_rect_delegate ??= GetSymbol<Delegates.sk_rrect_set_rect> ("sk_rrect_set_rect")).Invoke (rrect, rect);
		#endif

		// void sk_rrect_set_rect_radii(sk_rrect_t* rrect, const sk_rect_t* rect, const sk_vector_t* radii)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_rect_radii (sk_rrect_t rrect, SKRect* rect, SKPoint* radii);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_set_rect_radii (sk_rrect_t rrect, SKRect* rect, SKPoint* radii);
		}
		private static Delegates.sk_rrect_set_rect_radii sk_rrect_set_rect_radii_delegate;
		internal static void sk_rrect_set_rect_radii (sk_rrect_t rrect, SKRect* rect, SKPoint* radii) =>
			(sk_rrect_set_rect_radii_delegate ??= GetSymbol<Delegates.sk_rrect_set_rect_radii> ("sk_rrect_set_rect_radii")).Invoke (rrect, rect, radii);
		#endif

		// void sk_rrect_set_rect_xy(sk_rrect_t* rrect, const sk_rect_t* rect, float xRad, float yRad)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rrect_set_rect_xy (sk_rrect_t rrect, SKRect* rect, Single xRad, Single yRad);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rrect_set_rect_xy (sk_rrect_t rrect, SKRect* rect, Single xRad, Single yRad);
		}
		private static Delegates.sk_rrect_set_rect_xy sk_rrect_set_rect_xy_delegate;
		internal static void sk_rrect_set_rect_xy (sk_rrect_t rrect, SKRect* rect, Single xRad, Single yRad) =>
			(sk_rrect_set_rect_xy_delegate ??= GetSymbol<Delegates.sk_rrect_set_rect_xy> ("sk_rrect_set_rect_xy")).Invoke (rrect, rect, xRad, yRad);
		#endif

		// bool sk_rrect_transform(sk_rrect_t* rrect, const sk_matrix_t* matrix, sk_rrect_t* dest)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_rrect_transform (sk_rrect_t rrect, SKMatrix* matrix, sk_rrect_t dest);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_rrect_transform (sk_rrect_t rrect, SKMatrix* matrix, sk_rrect_t dest);
		}
		private static Delegates.sk_rrect_transform sk_rrect_transform_delegate;
		internal static bool sk_rrect_transform (sk_rrect_t rrect, SKMatrix* matrix, sk_rrect_t dest) =>
			(sk_rrect_transform_delegate ??= GetSymbol<Delegates.sk_rrect_transform> ("sk_rrect_transform")).Invoke (rrect, matrix, dest);
		#endif

		#endregion

		#region sk_runtimeeffect.h

		// void sk_runtimeeffect_get_child_name(const sk_runtimeeffect_t* effect, int index, sk_string_t* name)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_runtimeeffect_get_child_name (sk_runtimeeffect_t effect, Int32 index, sk_string_t name);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_runtimeeffect_get_child_name (sk_runtimeeffect_t effect, Int32 index, sk_string_t name);
		}
		private static Delegates.sk_runtimeeffect_get_child_name sk_runtimeeffect_get_child_name_delegate;
		internal static void sk_runtimeeffect_get_child_name (sk_runtimeeffect_t effect, Int32 index, sk_string_t name) =>
			(sk_runtimeeffect_get_child_name_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_child_name> ("sk_runtimeeffect_get_child_name")).Invoke (effect, index, name);
		#endif

		// size_t sk_runtimeeffect_get_children_count(const sk_runtimeeffect_t* effect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_runtimeeffect_get_children_count (sk_runtimeeffect_t effect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_runtimeeffect_get_children_count (sk_runtimeeffect_t effect);
		}
		private static Delegates.sk_runtimeeffect_get_children_count sk_runtimeeffect_get_children_count_delegate;
		internal static /* size_t */ IntPtr sk_runtimeeffect_get_children_count (sk_runtimeeffect_t effect) =>
			(sk_runtimeeffect_get_children_count_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_children_count> ("sk_runtimeeffect_get_children_count")).Invoke (effect);
		#endif

		// const sk_runtimeeffect_uniform_t* sk_runtimeeffect_get_uniform_from_index(const sk_runtimeeffect_t* effect, int index)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_runtimeeffect_uniform_t sk_runtimeeffect_get_uniform_from_index (sk_runtimeeffect_t effect, Int32 index);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_runtimeeffect_uniform_t sk_runtimeeffect_get_uniform_from_index (sk_runtimeeffect_t effect, Int32 index);
		}
		private static Delegates.sk_runtimeeffect_get_uniform_from_index sk_runtimeeffect_get_uniform_from_index_delegate;
		internal static sk_runtimeeffect_uniform_t sk_runtimeeffect_get_uniform_from_index (sk_runtimeeffect_t effect, Int32 index) =>
			(sk_runtimeeffect_get_uniform_from_index_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_uniform_from_index> ("sk_runtimeeffect_get_uniform_from_index")).Invoke (effect, index);
		#endif

		// const sk_runtimeeffect_uniform_t* sk_runtimeeffect_get_uniform_from_name(const sk_runtimeeffect_t* effect, const char* name, size_t len)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_runtimeeffect_uniform_t sk_runtimeeffect_get_uniform_from_name (sk_runtimeeffect_t effect, /* char */ void* name, /* size_t */ IntPtr len);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_runtimeeffect_uniform_t sk_runtimeeffect_get_uniform_from_name (sk_runtimeeffect_t effect, /* char */ void* name, /* size_t */ IntPtr len);
		}
		private static Delegates.sk_runtimeeffect_get_uniform_from_name sk_runtimeeffect_get_uniform_from_name_delegate;
		internal static sk_runtimeeffect_uniform_t sk_runtimeeffect_get_uniform_from_name (sk_runtimeeffect_t effect, /* char */ void* name, /* size_t */ IntPtr len) =>
			(sk_runtimeeffect_get_uniform_from_name_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_uniform_from_name> ("sk_runtimeeffect_get_uniform_from_name")).Invoke (effect, name, len);
		#endif

		// void sk_runtimeeffect_get_uniform_name(const sk_runtimeeffect_t* effect, int index, sk_string_t* name)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_runtimeeffect_get_uniform_name (sk_runtimeeffect_t effect, Int32 index, sk_string_t name);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_runtimeeffect_get_uniform_name (sk_runtimeeffect_t effect, Int32 index, sk_string_t name);
		}
		private static Delegates.sk_runtimeeffect_get_uniform_name sk_runtimeeffect_get_uniform_name_delegate;
		internal static void sk_runtimeeffect_get_uniform_name (sk_runtimeeffect_t effect, Int32 index, sk_string_t name) =>
			(sk_runtimeeffect_get_uniform_name_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_uniform_name> ("sk_runtimeeffect_get_uniform_name")).Invoke (effect, index, name);
		#endif

		// size_t sk_runtimeeffect_get_uniform_size(const sk_runtimeeffect_t* effect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_runtimeeffect_get_uniform_size (sk_runtimeeffect_t effect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_runtimeeffect_get_uniform_size (sk_runtimeeffect_t effect);
		}
		private static Delegates.sk_runtimeeffect_get_uniform_size sk_runtimeeffect_get_uniform_size_delegate;
		internal static /* size_t */ IntPtr sk_runtimeeffect_get_uniform_size (sk_runtimeeffect_t effect) =>
			(sk_runtimeeffect_get_uniform_size_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_uniform_size> ("sk_runtimeeffect_get_uniform_size")).Invoke (effect);
		#endif

		// size_t sk_runtimeeffect_get_uniforms_count(const sk_runtimeeffect_t* effect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_runtimeeffect_get_uniforms_count (sk_runtimeeffect_t effect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_runtimeeffect_get_uniforms_count (sk_runtimeeffect_t effect);
		}
		private static Delegates.sk_runtimeeffect_get_uniforms_count sk_runtimeeffect_get_uniforms_count_delegate;
		internal static /* size_t */ IntPtr sk_runtimeeffect_get_uniforms_count (sk_runtimeeffect_t effect) =>
			(sk_runtimeeffect_get_uniforms_count_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_uniforms_count> ("sk_runtimeeffect_get_uniforms_count")).Invoke (effect);
		#endif

		// sk_runtimeeffect_t* sk_runtimeeffect_make(sk_string_t* sksl, sk_string_t* error)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_runtimeeffect_t sk_runtimeeffect_make (sk_string_t sksl, sk_string_t error);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_runtimeeffect_t sk_runtimeeffect_make (sk_string_t sksl, sk_string_t error);
		}
		private static Delegates.sk_runtimeeffect_make sk_runtimeeffect_make_delegate;
		internal static sk_runtimeeffect_t sk_runtimeeffect_make (sk_string_t sksl, sk_string_t error) =>
			(sk_runtimeeffect_make_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_make> ("sk_runtimeeffect_make")).Invoke (sksl, error);
		#endif

		// sk_colorfilter_t* sk_runtimeeffect_make_color_filter(sk_runtimeeffect_t* effect, sk_data_t* uniforms, sk_colorfilter_t** children, size_t childCount)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_runtimeeffect_make_color_filter (sk_runtimeeffect_t effect, sk_data_t uniforms, sk_colorfilter_t* children, /* size_t */ IntPtr childCount);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_runtimeeffect_make_color_filter (sk_runtimeeffect_t effect, sk_data_t uniforms, sk_colorfilter_t* children, /* size_t */ IntPtr childCount);
		}
		private static Delegates.sk_runtimeeffect_make_color_filter sk_runtimeeffect_make_color_filter_delegate;
		internal static sk_colorfilter_t sk_runtimeeffect_make_color_filter (sk_runtimeeffect_t effect, sk_data_t uniforms, sk_colorfilter_t* children, /* size_t */ IntPtr childCount) =>
			(sk_runtimeeffect_make_color_filter_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_make_color_filter> ("sk_runtimeeffect_make_color_filter")).Invoke (effect, uniforms, children, childCount);
		#endif

		// sk_shader_t* sk_runtimeeffect_make_shader(sk_runtimeeffect_t* effect, sk_data_t* uniforms, sk_shader_t** children, size_t childCount, const sk_matrix_t* localMatrix, bool isOpaque)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_runtimeeffect_make_shader (sk_runtimeeffect_t effect, sk_data_t uniforms, sk_shader_t* children, /* size_t */ IntPtr childCount, SKMatrix* localMatrix, [MarshalAs (UnmanagedType.I1)] bool isOpaque);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_runtimeeffect_make_shader (sk_runtimeeffect_t effect, sk_data_t uniforms, sk_shader_t* children, /* size_t */ IntPtr childCount, SKMatrix* localMatrix, [MarshalAs (UnmanagedType.I1)] bool isOpaque);
		}
		private static Delegates.sk_runtimeeffect_make_shader sk_runtimeeffect_make_shader_delegate;
		internal static sk_shader_t sk_runtimeeffect_make_shader (sk_runtimeeffect_t effect, sk_data_t uniforms, sk_shader_t* children, /* size_t */ IntPtr childCount, SKMatrix* localMatrix, [MarshalAs (UnmanagedType.I1)] bool isOpaque) =>
			(sk_runtimeeffect_make_shader_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_make_shader> ("sk_runtimeeffect_make_shader")).Invoke (effect, uniforms, children, childCount, localMatrix, isOpaque);
		#endif

		// size_t sk_runtimeeffect_uniform_get_offset(const sk_runtimeeffect_uniform_t* variable)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_runtimeeffect_uniform_get_offset (sk_runtimeeffect_uniform_t variable);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_runtimeeffect_uniform_get_offset (sk_runtimeeffect_uniform_t variable);
		}
		private static Delegates.sk_runtimeeffect_uniform_get_offset sk_runtimeeffect_uniform_get_offset_delegate;
		internal static /* size_t */ IntPtr sk_runtimeeffect_uniform_get_offset (sk_runtimeeffect_uniform_t variable) =>
			(sk_runtimeeffect_uniform_get_offset_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_uniform_get_offset> ("sk_runtimeeffect_uniform_get_offset")).Invoke (variable);
		#endif

		// size_t sk_runtimeeffect_uniform_get_size_in_bytes(const sk_runtimeeffect_uniform_t* variable)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_runtimeeffect_uniform_get_size_in_bytes (sk_runtimeeffect_uniform_t variable);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_runtimeeffect_uniform_get_size_in_bytes (sk_runtimeeffect_uniform_t variable);
		}
		private static Delegates.sk_runtimeeffect_uniform_get_size_in_bytes sk_runtimeeffect_uniform_get_size_in_bytes_delegate;
		internal static /* size_t */ IntPtr sk_runtimeeffect_uniform_get_size_in_bytes (sk_runtimeeffect_uniform_t variable) =>
			(sk_runtimeeffect_uniform_get_size_in_bytes_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_uniform_get_size_in_bytes> ("sk_runtimeeffect_uniform_get_size_in_bytes")).Invoke (variable);
		#endif

		// void sk_runtimeeffect_unref(sk_runtimeeffect_t* effect)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_runtimeeffect_unref (sk_runtimeeffect_t effect);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_runtimeeffect_unref (sk_runtimeeffect_t effect);
		}
		private static Delegates.sk_runtimeeffect_unref sk_runtimeeffect_unref_delegate;
		internal static void sk_runtimeeffect_unref (sk_runtimeeffect_t effect) =>
			(sk_runtimeeffect_unref_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_unref> ("sk_runtimeeffect_unref")).Invoke (effect);
		#endif

		#endregion

		#region sk_shader.h

		// sk_shader_t* sk_shader_new_blend(sk_blendmode_t mode, const sk_shader_t* dst, const sk_shader_t* src)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_blend (SKBlendMode mode, sk_shader_t dst, sk_shader_t src);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_blend (SKBlendMode mode, sk_shader_t dst, sk_shader_t src);
		}
		private static Delegates.sk_shader_new_blend sk_shader_new_blend_delegate;
		internal static sk_shader_t sk_shader_new_blend (SKBlendMode mode, sk_shader_t dst, sk_shader_t src) =>
			(sk_shader_new_blend_delegate ??= GetSymbol<Delegates.sk_shader_new_blend> ("sk_shader_new_blend")).Invoke (mode, dst, src);
		#endif

		// sk_shader_t* sk_shader_new_color(sk_color_t color)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_color (UInt32 color);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_color (UInt32 color);
		}
		private static Delegates.sk_shader_new_color sk_shader_new_color_delegate;
		internal static sk_shader_t sk_shader_new_color (UInt32 color) =>
			(sk_shader_new_color_delegate ??= GetSymbol<Delegates.sk_shader_new_color> ("sk_shader_new_color")).Invoke (color);
		#endif

		// sk_shader_t* sk_shader_new_color4f(const sk_color4f_t* color, const sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_color4f (SKColorF* color, sk_colorspace_t colorspace);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_color4f (SKColorF* color, sk_colorspace_t colorspace);
		}
		private static Delegates.sk_shader_new_color4f sk_shader_new_color4f_delegate;
		internal static sk_shader_t sk_shader_new_color4f (SKColorF* color, sk_colorspace_t colorspace) =>
			(sk_shader_new_color4f_delegate ??= GetSymbol<Delegates.sk_shader_new_color4f> ("sk_shader_new_color4f")).Invoke (color, colorspace);
		#endif

		// sk_shader_t* sk_shader_new_empty()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_empty ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_empty ();
		}
		private static Delegates.sk_shader_new_empty sk_shader_new_empty_delegate;
		internal static sk_shader_t sk_shader_new_empty () =>
			(sk_shader_new_empty_delegate ??= GetSymbol<Delegates.sk_shader_new_empty> ("sk_shader_new_empty")).Invoke ();
		#endif

		// sk_shader_t* sk_shader_new_lerp(float t, const sk_shader_t* dst, const sk_shader_t* src)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_lerp (Single t, sk_shader_t dst, sk_shader_t src);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_lerp (Single t, sk_shader_t dst, sk_shader_t src);
		}
		private static Delegates.sk_shader_new_lerp sk_shader_new_lerp_delegate;
		internal static sk_shader_t sk_shader_new_lerp (Single t, sk_shader_t dst, sk_shader_t src) =>
			(sk_shader_new_lerp_delegate ??= GetSymbol<Delegates.sk_shader_new_lerp> ("sk_shader_new_lerp")).Invoke (t, dst, src);
		#endif

		// sk_shader_t* sk_shader_new_linear_gradient(const sk_point_t[2] points = 2, const sk_color_t[-1] colors, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_linear_gradient (SKPoint* points, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_linear_gradient (SKPoint* points, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_linear_gradient sk_shader_new_linear_gradient_delegate;
		internal static sk_shader_t sk_shader_new_linear_gradient (SKPoint* points, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix) =>
			(sk_shader_new_linear_gradient_delegate ??= GetSymbol<Delegates.sk_shader_new_linear_gradient> ("sk_shader_new_linear_gradient")).Invoke (points, colors, colorPos, colorCount, tileMode, localMatrix);
		#endif

		// sk_shader_t* sk_shader_new_linear_gradient_color4f(const sk_point_t[2] points = 2, const sk_color4f_t* colors, const sk_colorspace_t* colorspace, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_linear_gradient_color4f (SKPoint* points, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_linear_gradient_color4f (SKPoint* points, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_linear_gradient_color4f sk_shader_new_linear_gradient_color4f_delegate;
		internal static sk_shader_t sk_shader_new_linear_gradient_color4f (SKPoint* points, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix) =>
			(sk_shader_new_linear_gradient_color4f_delegate ??= GetSymbol<Delegates.sk_shader_new_linear_gradient_color4f> ("sk_shader_new_linear_gradient_color4f")).Invoke (points, colors, colorspace, colorPos, colorCount, tileMode, localMatrix);
		#endif

		// sk_shader_t* sk_shader_new_perlin_noise_fractal_noise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, const sk_isize_t* tileSize)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_perlin_noise_fractal_noise (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_perlin_noise_fractal_noise (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize);
		}
		private static Delegates.sk_shader_new_perlin_noise_fractal_noise sk_shader_new_perlin_noise_fractal_noise_delegate;
		internal static sk_shader_t sk_shader_new_perlin_noise_fractal_noise (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize) =>
			(sk_shader_new_perlin_noise_fractal_noise_delegate ??= GetSymbol<Delegates.sk_shader_new_perlin_noise_fractal_noise> ("sk_shader_new_perlin_noise_fractal_noise")).Invoke (baseFrequencyX, baseFrequencyY, numOctaves, seed, tileSize);
		#endif

		// sk_shader_t* sk_shader_new_perlin_noise_improved_noise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float z)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_perlin_noise_improved_noise (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single z);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_perlin_noise_improved_noise (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single z);
		}
		private static Delegates.sk_shader_new_perlin_noise_improved_noise sk_shader_new_perlin_noise_improved_noise_delegate;
		internal static sk_shader_t sk_shader_new_perlin_noise_improved_noise (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single z) =>
			(sk_shader_new_perlin_noise_improved_noise_delegate ??= GetSymbol<Delegates.sk_shader_new_perlin_noise_improved_noise> ("sk_shader_new_perlin_noise_improved_noise")).Invoke (baseFrequencyX, baseFrequencyY, numOctaves, z);
		#endif

		// sk_shader_t* sk_shader_new_perlin_noise_turbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, const sk_isize_t* tileSize)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_perlin_noise_turbulence (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_perlin_noise_turbulence (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize);
		}
		private static Delegates.sk_shader_new_perlin_noise_turbulence sk_shader_new_perlin_noise_turbulence_delegate;
		internal static sk_shader_t sk_shader_new_perlin_noise_turbulence (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize) =>
			(sk_shader_new_perlin_noise_turbulence_delegate ??= GetSymbol<Delegates.sk_shader_new_perlin_noise_turbulence> ("sk_shader_new_perlin_noise_turbulence")).Invoke (baseFrequencyX, baseFrequencyY, numOctaves, seed, tileSize);
		#endif

		// sk_shader_t* sk_shader_new_radial_gradient(const sk_point_t* center, float radius, const sk_color_t[-1] colors, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_radial_gradient (SKPoint* center, Single radius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_radial_gradient (SKPoint* center, Single radius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_radial_gradient sk_shader_new_radial_gradient_delegate;
		internal static sk_shader_t sk_shader_new_radial_gradient (SKPoint* center, Single radius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix) =>
			(sk_shader_new_radial_gradient_delegate ??= GetSymbol<Delegates.sk_shader_new_radial_gradient> ("sk_shader_new_radial_gradient")).Invoke (center, radius, colors, colorPos, colorCount, tileMode, localMatrix);
		#endif

		// sk_shader_t* sk_shader_new_radial_gradient_color4f(const sk_point_t* center, float radius, const sk_color4f_t* colors, const sk_colorspace_t* colorspace, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_radial_gradient_color4f (SKPoint* center, Single radius, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_radial_gradient_color4f (SKPoint* center, Single radius, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_radial_gradient_color4f sk_shader_new_radial_gradient_color4f_delegate;
		internal static sk_shader_t sk_shader_new_radial_gradient_color4f (SKPoint* center, Single radius, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix) =>
			(sk_shader_new_radial_gradient_color4f_delegate ??= GetSymbol<Delegates.sk_shader_new_radial_gradient_color4f> ("sk_shader_new_radial_gradient_color4f")).Invoke (center, radius, colors, colorspace, colorPos, colorCount, tileMode, localMatrix);
		#endif

		// sk_shader_t* sk_shader_new_sweep_gradient(const sk_point_t* center, const sk_color_t[-1] colors, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, float startAngle, float endAngle, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_sweep_gradient (SKPoint* center, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_sweep_gradient (SKPoint* center, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_sweep_gradient sk_shader_new_sweep_gradient_delegate;
		internal static sk_shader_t sk_shader_new_sweep_gradient (SKPoint* center, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix) =>
			(sk_shader_new_sweep_gradient_delegate ??= GetSymbol<Delegates.sk_shader_new_sweep_gradient> ("sk_shader_new_sweep_gradient")).Invoke (center, colors, colorPos, colorCount, tileMode, startAngle, endAngle, localMatrix);
		#endif

		// sk_shader_t* sk_shader_new_sweep_gradient_color4f(const sk_point_t* center, const sk_color4f_t* colors, const sk_colorspace_t* colorspace, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, float startAngle, float endAngle, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_sweep_gradient_color4f (SKPoint* center, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_sweep_gradient_color4f (SKPoint* center, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_sweep_gradient_color4f sk_shader_new_sweep_gradient_color4f_delegate;
		internal static sk_shader_t sk_shader_new_sweep_gradient_color4f (SKPoint* center, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix) =>
			(sk_shader_new_sweep_gradient_color4f_delegate ??= GetSymbol<Delegates.sk_shader_new_sweep_gradient_color4f> ("sk_shader_new_sweep_gradient_color4f")).Invoke (center, colors, colorspace, colorPos, colorCount, tileMode, startAngle, endAngle, localMatrix);
		#endif

		// sk_shader_t* sk_shader_new_two_point_conical_gradient(const sk_point_t* start, float startRadius, const sk_point_t* end, float endRadius, const sk_color_t[-1] colors, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_two_point_conical_gradient (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_two_point_conical_gradient (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_two_point_conical_gradient sk_shader_new_two_point_conical_gradient_delegate;
		internal static sk_shader_t sk_shader_new_two_point_conical_gradient (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix) =>
			(sk_shader_new_two_point_conical_gradient_delegate ??= GetSymbol<Delegates.sk_shader_new_two_point_conical_gradient> ("sk_shader_new_two_point_conical_gradient")).Invoke (start, startRadius, end, endRadius, colors, colorPos, colorCount, tileMode, localMatrix);
		#endif

		// sk_shader_t* sk_shader_new_two_point_conical_gradient_color4f(const sk_point_t* start, float startRadius, const sk_point_t* end, float endRadius, const sk_color4f_t* colors, const sk_colorspace_t* colorspace, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_new_two_point_conical_gradient_color4f (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_new_two_point_conical_gradient_color4f (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_two_point_conical_gradient_color4f sk_shader_new_two_point_conical_gradient_color4f_delegate;
		internal static sk_shader_t sk_shader_new_two_point_conical_gradient_color4f (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, SKColorF* colors, sk_colorspace_t colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix) =>
			(sk_shader_new_two_point_conical_gradient_color4f_delegate ??= GetSymbol<Delegates.sk_shader_new_two_point_conical_gradient_color4f> ("sk_shader_new_two_point_conical_gradient_color4f")).Invoke (start, startRadius, end, endRadius, colors, colorspace, colorPos, colorCount, tileMode, localMatrix);
		#endif

		// void sk_shader_ref(sk_shader_t* shader)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_shader_ref (sk_shader_t shader);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_shader_ref (sk_shader_t shader);
		}
		private static Delegates.sk_shader_ref sk_shader_ref_delegate;
		internal static void sk_shader_ref (sk_shader_t shader) =>
			(sk_shader_ref_delegate ??= GetSymbol<Delegates.sk_shader_ref> ("sk_shader_ref")).Invoke (shader);
		#endif

		// void sk_shader_unref(sk_shader_t* shader)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_shader_unref (sk_shader_t shader);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_shader_unref (sk_shader_t shader);
		}
		private static Delegates.sk_shader_unref sk_shader_unref_delegate;
		internal static void sk_shader_unref (sk_shader_t shader) =>
			(sk_shader_unref_delegate ??= GetSymbol<Delegates.sk_shader_unref> ("sk_shader_unref")).Invoke (shader);
		#endif

		// sk_shader_t* sk_shader_with_color_filter(const sk_shader_t* shader, const sk_colorfilter_t* filter)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_with_color_filter (sk_shader_t shader, sk_colorfilter_t filter);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_with_color_filter (sk_shader_t shader, sk_colorfilter_t filter);
		}
		private static Delegates.sk_shader_with_color_filter sk_shader_with_color_filter_delegate;
		internal static sk_shader_t sk_shader_with_color_filter (sk_shader_t shader, sk_colorfilter_t filter) =>
			(sk_shader_with_color_filter_delegate ??= GetSymbol<Delegates.sk_shader_with_color_filter> ("sk_shader_with_color_filter")).Invoke (shader, filter);
		#endif

		// sk_shader_t* sk_shader_with_local_matrix(const sk_shader_t* shader, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_shader_with_local_matrix (sk_shader_t shader, SKMatrix* localMatrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_shader_with_local_matrix (sk_shader_t shader, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_with_local_matrix sk_shader_with_local_matrix_delegate;
		internal static sk_shader_t sk_shader_with_local_matrix (sk_shader_t shader, SKMatrix* localMatrix) =>
			(sk_shader_with_local_matrix_delegate ??= GetSymbol<Delegates.sk_shader_with_local_matrix> ("sk_shader_with_local_matrix")).Invoke (shader, localMatrix);
		#endif

		#endregion

		#region sk_stream.h

		// void sk_dynamicmemorywstream_copy_to(sk_wstream_dynamicmemorystream_t* cstream, void* data)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_dynamicmemorywstream_copy_to (sk_wstream_dynamicmemorystream_t cstream, void* data);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_dynamicmemorywstream_copy_to (sk_wstream_dynamicmemorystream_t cstream, void* data);
		}
		private static Delegates.sk_dynamicmemorywstream_copy_to sk_dynamicmemorywstream_copy_to_delegate;
		internal static void sk_dynamicmemorywstream_copy_to (sk_wstream_dynamicmemorystream_t cstream, void* data) =>
			(sk_dynamicmemorywstream_copy_to_delegate ??= GetSymbol<Delegates.sk_dynamicmemorywstream_copy_to> ("sk_dynamicmemorywstream_copy_to")).Invoke (cstream, data);
		#endif

		// void sk_dynamicmemorywstream_destroy(sk_wstream_dynamicmemorystream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_dynamicmemorywstream_destroy (sk_wstream_dynamicmemorystream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_dynamicmemorywstream_destroy (sk_wstream_dynamicmemorystream_t cstream);
		}
		private static Delegates.sk_dynamicmemorywstream_destroy sk_dynamicmemorywstream_destroy_delegate;
		internal static void sk_dynamicmemorywstream_destroy (sk_wstream_dynamicmemorystream_t cstream) =>
			(sk_dynamicmemorywstream_destroy_delegate ??= GetSymbol<Delegates.sk_dynamicmemorywstream_destroy> ("sk_dynamicmemorywstream_destroy")).Invoke (cstream);
		#endif

		// sk_data_t* sk_dynamicmemorywstream_detach_as_data(sk_wstream_dynamicmemorystream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_dynamicmemorywstream_detach_as_data (sk_wstream_dynamicmemorystream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_dynamicmemorywstream_detach_as_data (sk_wstream_dynamicmemorystream_t cstream);
		}
		private static Delegates.sk_dynamicmemorywstream_detach_as_data sk_dynamicmemorywstream_detach_as_data_delegate;
		internal static sk_data_t sk_dynamicmemorywstream_detach_as_data (sk_wstream_dynamicmemorystream_t cstream) =>
			(sk_dynamicmemorywstream_detach_as_data_delegate ??= GetSymbol<Delegates.sk_dynamicmemorywstream_detach_as_data> ("sk_dynamicmemorywstream_detach_as_data")).Invoke (cstream);
		#endif

		// sk_stream_asset_t* sk_dynamicmemorywstream_detach_as_stream(sk_wstream_dynamicmemorystream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_asset_t sk_dynamicmemorywstream_detach_as_stream (sk_wstream_dynamicmemorystream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_asset_t sk_dynamicmemorywstream_detach_as_stream (sk_wstream_dynamicmemorystream_t cstream);
		}
		private static Delegates.sk_dynamicmemorywstream_detach_as_stream sk_dynamicmemorywstream_detach_as_stream_delegate;
		internal static sk_stream_asset_t sk_dynamicmemorywstream_detach_as_stream (sk_wstream_dynamicmemorystream_t cstream) =>
			(sk_dynamicmemorywstream_detach_as_stream_delegate ??= GetSymbol<Delegates.sk_dynamicmemorywstream_detach_as_stream> ("sk_dynamicmemorywstream_detach_as_stream")).Invoke (cstream);
		#endif

		// sk_wstream_dynamicmemorystream_t* sk_dynamicmemorywstream_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_wstream_dynamicmemorystream_t sk_dynamicmemorywstream_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_wstream_dynamicmemorystream_t sk_dynamicmemorywstream_new ();
		}
		private static Delegates.sk_dynamicmemorywstream_new sk_dynamicmemorywstream_new_delegate;
		internal static sk_wstream_dynamicmemorystream_t sk_dynamicmemorywstream_new () =>
			(sk_dynamicmemorywstream_new_delegate ??= GetSymbol<Delegates.sk_dynamicmemorywstream_new> ("sk_dynamicmemorywstream_new")).Invoke ();
		#endif

		// bool sk_dynamicmemorywstream_write_to_stream(sk_wstream_dynamicmemorystream_t* cstream, sk_wstream_t* dst)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_dynamicmemorywstream_write_to_stream (sk_wstream_dynamicmemorystream_t cstream, sk_wstream_t dst);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_dynamicmemorywstream_write_to_stream (sk_wstream_dynamicmemorystream_t cstream, sk_wstream_t dst);
		}
		private static Delegates.sk_dynamicmemorywstream_write_to_stream sk_dynamicmemorywstream_write_to_stream_delegate;
		internal static bool sk_dynamicmemorywstream_write_to_stream (sk_wstream_dynamicmemorystream_t cstream, sk_wstream_t dst) =>
			(sk_dynamicmemorywstream_write_to_stream_delegate ??= GetSymbol<Delegates.sk_dynamicmemorywstream_write_to_stream> ("sk_dynamicmemorywstream_write_to_stream")).Invoke (cstream, dst);
		#endif

		// void sk_filestream_destroy(sk_stream_filestream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_filestream_destroy (sk_stream_filestream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_filestream_destroy (sk_stream_filestream_t cstream);
		}
		private static Delegates.sk_filestream_destroy sk_filestream_destroy_delegate;
		internal static void sk_filestream_destroy (sk_stream_filestream_t cstream) =>
			(sk_filestream_destroy_delegate ??= GetSymbol<Delegates.sk_filestream_destroy> ("sk_filestream_destroy")).Invoke (cstream);
		#endif

		// bool sk_filestream_is_valid(sk_stream_filestream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_filestream_is_valid (sk_stream_filestream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_filestream_is_valid (sk_stream_filestream_t cstream);
		}
		private static Delegates.sk_filestream_is_valid sk_filestream_is_valid_delegate;
		internal static bool sk_filestream_is_valid (sk_stream_filestream_t cstream) =>
			(sk_filestream_is_valid_delegate ??= GetSymbol<Delegates.sk_filestream_is_valid> ("sk_filestream_is_valid")).Invoke (cstream);
		#endif

		// sk_stream_filestream_t* sk_filestream_new(const char* path)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_filestream_t sk_filestream_new (/* char */ void* path);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_filestream_t sk_filestream_new (/* char */ void* path);
		}
		private static Delegates.sk_filestream_new sk_filestream_new_delegate;
		internal static sk_stream_filestream_t sk_filestream_new (/* char */ void* path) =>
			(sk_filestream_new_delegate ??= GetSymbol<Delegates.sk_filestream_new> ("sk_filestream_new")).Invoke (path);
		#endif

		// void sk_filewstream_destroy(sk_wstream_filestream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_filewstream_destroy (sk_wstream_filestream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_filewstream_destroy (sk_wstream_filestream_t cstream);
		}
		private static Delegates.sk_filewstream_destroy sk_filewstream_destroy_delegate;
		internal static void sk_filewstream_destroy (sk_wstream_filestream_t cstream) =>
			(sk_filewstream_destroy_delegate ??= GetSymbol<Delegates.sk_filewstream_destroy> ("sk_filewstream_destroy")).Invoke (cstream);
		#endif

		// bool sk_filewstream_is_valid(sk_wstream_filestream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_filewstream_is_valid (sk_wstream_filestream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_filewstream_is_valid (sk_wstream_filestream_t cstream);
		}
		private static Delegates.sk_filewstream_is_valid sk_filewstream_is_valid_delegate;
		internal static bool sk_filewstream_is_valid (sk_wstream_filestream_t cstream) =>
			(sk_filewstream_is_valid_delegate ??= GetSymbol<Delegates.sk_filewstream_is_valid> ("sk_filewstream_is_valid")).Invoke (cstream);
		#endif

		// sk_wstream_filestream_t* sk_filewstream_new(const char* path)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_wstream_filestream_t sk_filewstream_new (/* char */ void* path);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_wstream_filestream_t sk_filewstream_new (/* char */ void* path);
		}
		private static Delegates.sk_filewstream_new sk_filewstream_new_delegate;
		internal static sk_wstream_filestream_t sk_filewstream_new (/* char */ void* path) =>
			(sk_filewstream_new_delegate ??= GetSymbol<Delegates.sk_filewstream_new> ("sk_filewstream_new")).Invoke (path);
		#endif

		// void sk_memorystream_destroy(sk_stream_memorystream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_memorystream_destroy (sk_stream_memorystream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_memorystream_destroy (sk_stream_memorystream_t cstream);
		}
		private static Delegates.sk_memorystream_destroy sk_memorystream_destroy_delegate;
		internal static void sk_memorystream_destroy (sk_stream_memorystream_t cstream) =>
			(sk_memorystream_destroy_delegate ??= GetSymbol<Delegates.sk_memorystream_destroy> ("sk_memorystream_destroy")).Invoke (cstream);
		#endif

		// sk_stream_memorystream_t* sk_memorystream_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_memorystream_t sk_memorystream_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_memorystream_t sk_memorystream_new ();
		}
		private static Delegates.sk_memorystream_new sk_memorystream_new_delegate;
		internal static sk_stream_memorystream_t sk_memorystream_new () =>
			(sk_memorystream_new_delegate ??= GetSymbol<Delegates.sk_memorystream_new> ("sk_memorystream_new")).Invoke ();
		#endif

		// sk_stream_memorystream_t* sk_memorystream_new_with_data(const void* data, size_t length, bool copyData)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_memorystream_t sk_memorystream_new_with_data (void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_memorystream_t sk_memorystream_new_with_data (void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData);
		}
		private static Delegates.sk_memorystream_new_with_data sk_memorystream_new_with_data_delegate;
		internal static sk_stream_memorystream_t sk_memorystream_new_with_data (void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData) =>
			(sk_memorystream_new_with_data_delegate ??= GetSymbol<Delegates.sk_memorystream_new_with_data> ("sk_memorystream_new_with_data")).Invoke (data, length, copyData);
		#endif

		// sk_stream_memorystream_t* sk_memorystream_new_with_length(size_t length)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_memorystream_t sk_memorystream_new_with_length (/* size_t */ IntPtr length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_memorystream_t sk_memorystream_new_with_length (/* size_t */ IntPtr length);
		}
		private static Delegates.sk_memorystream_new_with_length sk_memorystream_new_with_length_delegate;
		internal static sk_stream_memorystream_t sk_memorystream_new_with_length (/* size_t */ IntPtr length) =>
			(sk_memorystream_new_with_length_delegate ??= GetSymbol<Delegates.sk_memorystream_new_with_length> ("sk_memorystream_new_with_length")).Invoke (length);
		#endif

		// sk_stream_memorystream_t* sk_memorystream_new_with_skdata(sk_data_t* data)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_memorystream_t sk_memorystream_new_with_skdata (sk_data_t data);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_memorystream_t sk_memorystream_new_with_skdata (sk_data_t data);
		}
		private static Delegates.sk_memorystream_new_with_skdata sk_memorystream_new_with_skdata_delegate;
		internal static sk_stream_memorystream_t sk_memorystream_new_with_skdata (sk_data_t data) =>
			(sk_memorystream_new_with_skdata_delegate ??= GetSymbol<Delegates.sk_memorystream_new_with_skdata> ("sk_memorystream_new_with_skdata")).Invoke (data);
		#endif

		// void sk_memorystream_set_memory(sk_stream_memorystream_t* cmemorystream, const void* data, size_t length, bool copyData)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_memorystream_set_memory (sk_stream_memorystream_t cmemorystream, void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_memorystream_set_memory (sk_stream_memorystream_t cmemorystream, void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData);
		}
		private static Delegates.sk_memorystream_set_memory sk_memorystream_set_memory_delegate;
		internal static void sk_memorystream_set_memory (sk_stream_memorystream_t cmemorystream, void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData) =>
			(sk_memorystream_set_memory_delegate ??= GetSymbol<Delegates.sk_memorystream_set_memory> ("sk_memorystream_set_memory")).Invoke (cmemorystream, data, length, copyData);
		#endif

		// void sk_stream_asset_destroy(sk_stream_asset_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_stream_asset_destroy (sk_stream_asset_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_stream_asset_destroy (sk_stream_asset_t cstream);
		}
		private static Delegates.sk_stream_asset_destroy sk_stream_asset_destroy_delegate;
		internal static void sk_stream_asset_destroy (sk_stream_asset_t cstream) =>
			(sk_stream_asset_destroy_delegate ??= GetSymbol<Delegates.sk_stream_asset_destroy> ("sk_stream_asset_destroy")).Invoke (cstream);
		#endif

		// void sk_stream_destroy(sk_stream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_stream_destroy (sk_stream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_stream_destroy (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_destroy sk_stream_destroy_delegate;
		internal static void sk_stream_destroy (sk_stream_t cstream) =>
			(sk_stream_destroy_delegate ??= GetSymbol<Delegates.sk_stream_destroy> ("sk_stream_destroy")).Invoke (cstream);
		#endif

		// sk_stream_t* sk_stream_duplicate(sk_stream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_t sk_stream_duplicate (sk_stream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_t sk_stream_duplicate (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_duplicate sk_stream_duplicate_delegate;
		internal static sk_stream_t sk_stream_duplicate (sk_stream_t cstream) =>
			(sk_stream_duplicate_delegate ??= GetSymbol<Delegates.sk_stream_duplicate> ("sk_stream_duplicate")).Invoke (cstream);
		#endif

		// sk_stream_t* sk_stream_fork(sk_stream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_t sk_stream_fork (sk_stream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_t sk_stream_fork (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_fork sk_stream_fork_delegate;
		internal static sk_stream_t sk_stream_fork (sk_stream_t cstream) =>
			(sk_stream_fork_delegate ??= GetSymbol<Delegates.sk_stream_fork> ("sk_stream_fork")).Invoke (cstream);
		#endif

		// size_t sk_stream_get_length(sk_stream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_get_length (sk_stream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_stream_get_length (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_get_length sk_stream_get_length_delegate;
		internal static /* size_t */ IntPtr sk_stream_get_length (sk_stream_t cstream) =>
			(sk_stream_get_length_delegate ??= GetSymbol<Delegates.sk_stream_get_length> ("sk_stream_get_length")).Invoke (cstream);
		#endif

		// const void* sk_stream_get_memory_base(sk_stream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_stream_get_memory_base (sk_stream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_stream_get_memory_base (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_get_memory_base sk_stream_get_memory_base_delegate;
		internal static void* sk_stream_get_memory_base (sk_stream_t cstream) =>
			(sk_stream_get_memory_base_delegate ??= GetSymbol<Delegates.sk_stream_get_memory_base> ("sk_stream_get_memory_base")).Invoke (cstream);
		#endif

		// size_t sk_stream_get_position(sk_stream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_get_position (sk_stream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_stream_get_position (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_get_position sk_stream_get_position_delegate;
		internal static /* size_t */ IntPtr sk_stream_get_position (sk_stream_t cstream) =>
			(sk_stream_get_position_delegate ??= GetSymbol<Delegates.sk_stream_get_position> ("sk_stream_get_position")).Invoke (cstream);
		#endif

		// bool sk_stream_has_length(sk_stream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_has_length (sk_stream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_has_length (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_has_length sk_stream_has_length_delegate;
		internal static bool sk_stream_has_length (sk_stream_t cstream) =>
			(sk_stream_has_length_delegate ??= GetSymbol<Delegates.sk_stream_has_length> ("sk_stream_has_length")).Invoke (cstream);
		#endif

		// bool sk_stream_has_position(sk_stream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_has_position (sk_stream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_has_position (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_has_position sk_stream_has_position_delegate;
		internal static bool sk_stream_has_position (sk_stream_t cstream) =>
			(sk_stream_has_position_delegate ??= GetSymbol<Delegates.sk_stream_has_position> ("sk_stream_has_position")).Invoke (cstream);
		#endif

		// bool sk_stream_is_at_end(sk_stream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_is_at_end (sk_stream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_is_at_end (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_is_at_end sk_stream_is_at_end_delegate;
		internal static bool sk_stream_is_at_end (sk_stream_t cstream) =>
			(sk_stream_is_at_end_delegate ??= GetSymbol<Delegates.sk_stream_is_at_end> ("sk_stream_is_at_end")).Invoke (cstream);
		#endif

		// bool sk_stream_move(sk_stream_t* cstream, int offset)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_move (sk_stream_t cstream, Int32 offset);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_move (sk_stream_t cstream, Int32 offset);
		}
		private static Delegates.sk_stream_move sk_stream_move_delegate;
		internal static bool sk_stream_move (sk_stream_t cstream, Int32 offset) =>
			(sk_stream_move_delegate ??= GetSymbol<Delegates.sk_stream_move> ("sk_stream_move")).Invoke (cstream, offset);
		#endif

		// size_t sk_stream_peek(sk_stream_t* cstream, void* buffer, size_t size)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_peek (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_stream_peek (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_stream_peek sk_stream_peek_delegate;
		internal static /* size_t */ IntPtr sk_stream_peek (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size) =>
			(sk_stream_peek_delegate ??= GetSymbol<Delegates.sk_stream_peek> ("sk_stream_peek")).Invoke (cstream, buffer, size);
		#endif

		// size_t sk_stream_read(sk_stream_t* cstream, void* buffer, size_t size)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_read (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_stream_read (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_stream_read sk_stream_read_delegate;
		internal static /* size_t */ IntPtr sk_stream_read (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size) =>
			(sk_stream_read_delegate ??= GetSymbol<Delegates.sk_stream_read> ("sk_stream_read")).Invoke (cstream, buffer, size);
		#endif

		// bool sk_stream_read_bool(sk_stream_t* cstream, bool* buffer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_bool (sk_stream_t cstream, Byte* buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_read_bool (sk_stream_t cstream, Byte* buffer);
		}
		private static Delegates.sk_stream_read_bool sk_stream_read_bool_delegate;
		internal static bool sk_stream_read_bool (sk_stream_t cstream, Byte* buffer) =>
			(sk_stream_read_bool_delegate ??= GetSymbol<Delegates.sk_stream_read_bool> ("sk_stream_read_bool")).Invoke (cstream, buffer);
		#endif

		// bool sk_stream_read_s16(sk_stream_t* cstream, int16_t* buffer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_s16 (sk_stream_t cstream, Int16* buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_read_s16 (sk_stream_t cstream, Int16* buffer);
		}
		private static Delegates.sk_stream_read_s16 sk_stream_read_s16_delegate;
		internal static bool sk_stream_read_s16 (sk_stream_t cstream, Int16* buffer) =>
			(sk_stream_read_s16_delegate ??= GetSymbol<Delegates.sk_stream_read_s16> ("sk_stream_read_s16")).Invoke (cstream, buffer);
		#endif

		// bool sk_stream_read_s32(sk_stream_t* cstream, int32_t* buffer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_s32 (sk_stream_t cstream, Int32* buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_read_s32 (sk_stream_t cstream, Int32* buffer);
		}
		private static Delegates.sk_stream_read_s32 sk_stream_read_s32_delegate;
		internal static bool sk_stream_read_s32 (sk_stream_t cstream, Int32* buffer) =>
			(sk_stream_read_s32_delegate ??= GetSymbol<Delegates.sk_stream_read_s32> ("sk_stream_read_s32")).Invoke (cstream, buffer);
		#endif

		// bool sk_stream_read_s8(sk_stream_t* cstream, int8_t* buffer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_s8 (sk_stream_t cstream, SByte* buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_read_s8 (sk_stream_t cstream, SByte* buffer);
		}
		private static Delegates.sk_stream_read_s8 sk_stream_read_s8_delegate;
		internal static bool sk_stream_read_s8 (sk_stream_t cstream, SByte* buffer) =>
			(sk_stream_read_s8_delegate ??= GetSymbol<Delegates.sk_stream_read_s8> ("sk_stream_read_s8")).Invoke (cstream, buffer);
		#endif

		// bool sk_stream_read_u16(sk_stream_t* cstream, uint16_t* buffer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_u16 (sk_stream_t cstream, UInt16* buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_read_u16 (sk_stream_t cstream, UInt16* buffer);
		}
		private static Delegates.sk_stream_read_u16 sk_stream_read_u16_delegate;
		internal static bool sk_stream_read_u16 (sk_stream_t cstream, UInt16* buffer) =>
			(sk_stream_read_u16_delegate ??= GetSymbol<Delegates.sk_stream_read_u16> ("sk_stream_read_u16")).Invoke (cstream, buffer);
		#endif

		// bool sk_stream_read_u32(sk_stream_t* cstream, uint32_t* buffer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_u32 (sk_stream_t cstream, UInt32* buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_read_u32 (sk_stream_t cstream, UInt32* buffer);
		}
		private static Delegates.sk_stream_read_u32 sk_stream_read_u32_delegate;
		internal static bool sk_stream_read_u32 (sk_stream_t cstream, UInt32* buffer) =>
			(sk_stream_read_u32_delegate ??= GetSymbol<Delegates.sk_stream_read_u32> ("sk_stream_read_u32")).Invoke (cstream, buffer);
		#endif

		// bool sk_stream_read_u8(sk_stream_t* cstream, uint8_t* buffer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_u8 (sk_stream_t cstream, Byte* buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_read_u8 (sk_stream_t cstream, Byte* buffer);
		}
		private static Delegates.sk_stream_read_u8 sk_stream_read_u8_delegate;
		internal static bool sk_stream_read_u8 (sk_stream_t cstream, Byte* buffer) =>
			(sk_stream_read_u8_delegate ??= GetSymbol<Delegates.sk_stream_read_u8> ("sk_stream_read_u8")).Invoke (cstream, buffer);
		#endif

		// bool sk_stream_rewind(sk_stream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_rewind (sk_stream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_rewind (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_rewind sk_stream_rewind_delegate;
		internal static bool sk_stream_rewind (sk_stream_t cstream) =>
			(sk_stream_rewind_delegate ??= GetSymbol<Delegates.sk_stream_rewind> ("sk_stream_rewind")).Invoke (cstream);
		#endif

		// bool sk_stream_seek(sk_stream_t* cstream, size_t position)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_seek (sk_stream_t cstream, /* size_t */ IntPtr position);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_seek (sk_stream_t cstream, /* size_t */ IntPtr position);
		}
		private static Delegates.sk_stream_seek sk_stream_seek_delegate;
		internal static bool sk_stream_seek (sk_stream_t cstream, /* size_t */ IntPtr position) =>
			(sk_stream_seek_delegate ??= GetSymbol<Delegates.sk_stream_seek> ("sk_stream_seek")).Invoke (cstream, position);
		#endif

		// size_t sk_stream_skip(sk_stream_t* cstream, size_t size)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_skip (sk_stream_t cstream, /* size_t */ IntPtr size);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_stream_skip (sk_stream_t cstream, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_stream_skip sk_stream_skip_delegate;
		internal static /* size_t */ IntPtr sk_stream_skip (sk_stream_t cstream, /* size_t */ IntPtr size) =>
			(sk_stream_skip_delegate ??= GetSymbol<Delegates.sk_stream_skip> ("sk_stream_skip")).Invoke (cstream, size);
		#endif

		// size_t sk_wstream_bytes_written(sk_wstream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_wstream_bytes_written (sk_wstream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_wstream_bytes_written (sk_wstream_t cstream);
		}
		private static Delegates.sk_wstream_bytes_written sk_wstream_bytes_written_delegate;
		internal static /* size_t */ IntPtr sk_wstream_bytes_written (sk_wstream_t cstream) =>
			(sk_wstream_bytes_written_delegate ??= GetSymbol<Delegates.sk_wstream_bytes_written> ("sk_wstream_bytes_written")).Invoke (cstream);
		#endif

		// void sk_wstream_flush(sk_wstream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_wstream_flush (sk_wstream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_wstream_flush (sk_wstream_t cstream);
		}
		private static Delegates.sk_wstream_flush sk_wstream_flush_delegate;
		internal static void sk_wstream_flush (sk_wstream_t cstream) =>
			(sk_wstream_flush_delegate ??= GetSymbol<Delegates.sk_wstream_flush> ("sk_wstream_flush")).Invoke (cstream);
		#endif

		// int sk_wstream_get_size_of_packed_uint(size_t value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_wstream_get_size_of_packed_uint (/* size_t */ IntPtr value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_wstream_get_size_of_packed_uint (/* size_t */ IntPtr value);
		}
		private static Delegates.sk_wstream_get_size_of_packed_uint sk_wstream_get_size_of_packed_uint_delegate;
		internal static Int32 sk_wstream_get_size_of_packed_uint (/* size_t */ IntPtr value) =>
			(sk_wstream_get_size_of_packed_uint_delegate ??= GetSymbol<Delegates.sk_wstream_get_size_of_packed_uint> ("sk_wstream_get_size_of_packed_uint")).Invoke (value);
		#endif

		// bool sk_wstream_newline(sk_wstream_t* cstream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_newline (sk_wstream_t cstream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_newline (sk_wstream_t cstream);
		}
		private static Delegates.sk_wstream_newline sk_wstream_newline_delegate;
		internal static bool sk_wstream_newline (sk_wstream_t cstream) =>
			(sk_wstream_newline_delegate ??= GetSymbol<Delegates.sk_wstream_newline> ("sk_wstream_newline")).Invoke (cstream);
		#endif

		// bool sk_wstream_write(sk_wstream_t* cstream, const void* buffer, size_t size)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write (sk_wstream_t cstream, void* buffer, /* size_t */ IntPtr size);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write (sk_wstream_t cstream, void* buffer, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_wstream_write sk_wstream_write_delegate;
		internal static bool sk_wstream_write (sk_wstream_t cstream, void* buffer, /* size_t */ IntPtr size) =>
			(sk_wstream_write_delegate ??= GetSymbol<Delegates.sk_wstream_write> ("sk_wstream_write")).Invoke (cstream, buffer, size);
		#endif

		// bool sk_wstream_write_16(sk_wstream_t* cstream, uint16_t value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_16 (sk_wstream_t cstream, UInt16 value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_16 (sk_wstream_t cstream, UInt16 value);
		}
		private static Delegates.sk_wstream_write_16 sk_wstream_write_16_delegate;
		internal static bool sk_wstream_write_16 (sk_wstream_t cstream, UInt16 value) =>
			(sk_wstream_write_16_delegate ??= GetSymbol<Delegates.sk_wstream_write_16> ("sk_wstream_write_16")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_32(sk_wstream_t* cstream, uint32_t value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_32 (sk_wstream_t cstream, UInt32 value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_32 (sk_wstream_t cstream, UInt32 value);
		}
		private static Delegates.sk_wstream_write_32 sk_wstream_write_32_delegate;
		internal static bool sk_wstream_write_32 (sk_wstream_t cstream, UInt32 value) =>
			(sk_wstream_write_32_delegate ??= GetSymbol<Delegates.sk_wstream_write_32> ("sk_wstream_write_32")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_8(sk_wstream_t* cstream, uint8_t value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_8 (sk_wstream_t cstream, Byte value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_8 (sk_wstream_t cstream, Byte value);
		}
		private static Delegates.sk_wstream_write_8 sk_wstream_write_8_delegate;
		internal static bool sk_wstream_write_8 (sk_wstream_t cstream, Byte value) =>
			(sk_wstream_write_8_delegate ??= GetSymbol<Delegates.sk_wstream_write_8> ("sk_wstream_write_8")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_bigdec_as_text(sk_wstream_t* cstream, int64_t value, int minDigits)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_bigdec_as_text (sk_wstream_t cstream, Int64 value, Int32 minDigits);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_bigdec_as_text (sk_wstream_t cstream, Int64 value, Int32 minDigits);
		}
		private static Delegates.sk_wstream_write_bigdec_as_text sk_wstream_write_bigdec_as_text_delegate;
		internal static bool sk_wstream_write_bigdec_as_text (sk_wstream_t cstream, Int64 value, Int32 minDigits) =>
			(sk_wstream_write_bigdec_as_text_delegate ??= GetSymbol<Delegates.sk_wstream_write_bigdec_as_text> ("sk_wstream_write_bigdec_as_text")).Invoke (cstream, value, minDigits);
		#endif

		// bool sk_wstream_write_bool(sk_wstream_t* cstream, bool value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_bool (sk_wstream_t cstream, [MarshalAs (UnmanagedType.I1)] bool value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_bool (sk_wstream_t cstream, [MarshalAs (UnmanagedType.I1)] bool value);
		}
		private static Delegates.sk_wstream_write_bool sk_wstream_write_bool_delegate;
		internal static bool sk_wstream_write_bool (sk_wstream_t cstream, [MarshalAs (UnmanagedType.I1)] bool value) =>
			(sk_wstream_write_bool_delegate ??= GetSymbol<Delegates.sk_wstream_write_bool> ("sk_wstream_write_bool")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_dec_as_text(sk_wstream_t* cstream, int32_t value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_dec_as_text (sk_wstream_t cstream, Int32 value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_dec_as_text (sk_wstream_t cstream, Int32 value);
		}
		private static Delegates.sk_wstream_write_dec_as_text sk_wstream_write_dec_as_text_delegate;
		internal static bool sk_wstream_write_dec_as_text (sk_wstream_t cstream, Int32 value) =>
			(sk_wstream_write_dec_as_text_delegate ??= GetSymbol<Delegates.sk_wstream_write_dec_as_text> ("sk_wstream_write_dec_as_text")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_hex_as_text(sk_wstream_t* cstream, uint32_t value, int minDigits)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_hex_as_text (sk_wstream_t cstream, UInt32 value, Int32 minDigits);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_hex_as_text (sk_wstream_t cstream, UInt32 value, Int32 minDigits);
		}
		private static Delegates.sk_wstream_write_hex_as_text sk_wstream_write_hex_as_text_delegate;
		internal static bool sk_wstream_write_hex_as_text (sk_wstream_t cstream, UInt32 value, Int32 minDigits) =>
			(sk_wstream_write_hex_as_text_delegate ??= GetSymbol<Delegates.sk_wstream_write_hex_as_text> ("sk_wstream_write_hex_as_text")).Invoke (cstream, value, minDigits);
		#endif

		// bool sk_wstream_write_packed_uint(sk_wstream_t* cstream, size_t value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_packed_uint (sk_wstream_t cstream, /* size_t */ IntPtr value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_packed_uint (sk_wstream_t cstream, /* size_t */ IntPtr value);
		}
		private static Delegates.sk_wstream_write_packed_uint sk_wstream_write_packed_uint_delegate;
		internal static bool sk_wstream_write_packed_uint (sk_wstream_t cstream, /* size_t */ IntPtr value) =>
			(sk_wstream_write_packed_uint_delegate ??= GetSymbol<Delegates.sk_wstream_write_packed_uint> ("sk_wstream_write_packed_uint")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_scalar(sk_wstream_t* cstream, float value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_scalar (sk_wstream_t cstream, Single value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_scalar (sk_wstream_t cstream, Single value);
		}
		private static Delegates.sk_wstream_write_scalar sk_wstream_write_scalar_delegate;
		internal static bool sk_wstream_write_scalar (sk_wstream_t cstream, Single value) =>
			(sk_wstream_write_scalar_delegate ??= GetSymbol<Delegates.sk_wstream_write_scalar> ("sk_wstream_write_scalar")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_scalar_as_text(sk_wstream_t* cstream, float value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_scalar_as_text (sk_wstream_t cstream, Single value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_scalar_as_text (sk_wstream_t cstream, Single value);
		}
		private static Delegates.sk_wstream_write_scalar_as_text sk_wstream_write_scalar_as_text_delegate;
		internal static bool sk_wstream_write_scalar_as_text (sk_wstream_t cstream, Single value) =>
			(sk_wstream_write_scalar_as_text_delegate ??= GetSymbol<Delegates.sk_wstream_write_scalar_as_text> ("sk_wstream_write_scalar_as_text")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_stream(sk_wstream_t* cstream, sk_stream_t* input, size_t length)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_stream (sk_wstream_t cstream, sk_stream_t input, /* size_t */ IntPtr length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_stream (sk_wstream_t cstream, sk_stream_t input, /* size_t */ IntPtr length);
		}
		private static Delegates.sk_wstream_write_stream sk_wstream_write_stream_delegate;
		internal static bool sk_wstream_write_stream (sk_wstream_t cstream, sk_stream_t input, /* size_t */ IntPtr length) =>
			(sk_wstream_write_stream_delegate ??= GetSymbol<Delegates.sk_wstream_write_stream> ("sk_wstream_write_stream")).Invoke (cstream, input, length);
		#endif

		// bool sk_wstream_write_text(sk_wstream_t* cstream, const char* value)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_text (sk_wstream_t cstream, [MarshalAs (UnmanagedType.LPStr)] String value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_text (sk_wstream_t cstream, [MarshalAs (UnmanagedType.LPStr)] String value);
		}
		private static Delegates.sk_wstream_write_text sk_wstream_write_text_delegate;
		internal static bool sk_wstream_write_text (sk_wstream_t cstream, [MarshalAs (UnmanagedType.LPStr)] String value) =>
			(sk_wstream_write_text_delegate ??= GetSymbol<Delegates.sk_wstream_write_text> ("sk_wstream_write_text")).Invoke (cstream, value);
		#endif

		#endregion

		#region sk_string.h

		// void sk_string_destructor(const sk_string_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_string_destructor (sk_string_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_string_destructor (sk_string_t param0);
		}
		private static Delegates.sk_string_destructor sk_string_destructor_delegate;
		internal static void sk_string_destructor (sk_string_t param0) =>
			(sk_string_destructor_delegate ??= GetSymbol<Delegates.sk_string_destructor> ("sk_string_destructor")).Invoke (param0);
		#endif

		// const char* sk_string_get_c_str(const sk_string_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* sk_string_get_c_str (sk_string_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void* sk_string_get_c_str (sk_string_t param0);
		}
		private static Delegates.sk_string_get_c_str sk_string_get_c_str_delegate;
		internal static /* char */ void* sk_string_get_c_str (sk_string_t param0) =>
			(sk_string_get_c_str_delegate ??= GetSymbol<Delegates.sk_string_get_c_str> ("sk_string_get_c_str")).Invoke (param0);
		#endif

		// size_t sk_string_get_size(const sk_string_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_string_get_size (sk_string_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_string_get_size (sk_string_t param0);
		}
		private static Delegates.sk_string_get_size sk_string_get_size_delegate;
		internal static /* size_t */ IntPtr sk_string_get_size (sk_string_t param0) =>
			(sk_string_get_size_delegate ??= GetSymbol<Delegates.sk_string_get_size> ("sk_string_get_size")).Invoke (param0);
		#endif

		// sk_string_t* sk_string_new_empty()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_string_t sk_string_new_empty ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_string_t sk_string_new_empty ();
		}
		private static Delegates.sk_string_new_empty sk_string_new_empty_delegate;
		internal static sk_string_t sk_string_new_empty () =>
			(sk_string_new_empty_delegate ??= GetSymbol<Delegates.sk_string_new_empty> ("sk_string_new_empty")).Invoke ();
		#endif

		// sk_string_t* sk_string_new_with_copy(const char* src, size_t length)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_string_t sk_string_new_with_copy (/* char */ void* src, /* size_t */ IntPtr length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_string_t sk_string_new_with_copy (/* char */ void* src, /* size_t */ IntPtr length);
		}
		private static Delegates.sk_string_new_with_copy sk_string_new_with_copy_delegate;
		internal static sk_string_t sk_string_new_with_copy (/* char */ void* src, /* size_t */ IntPtr length) =>
			(sk_string_new_with_copy_delegate ??= GetSymbol<Delegates.sk_string_new_with_copy> ("sk_string_new_with_copy")).Invoke (src, length);
		#endif

		#endregion

		#region sk_surface.h

		// void sk_surface_draw(sk_surface_t* surface, sk_canvas_t* canvas, float x, float y, const sk_paint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_surface_draw (sk_surface_t surface, sk_canvas_t canvas, Single x, Single y, sk_paint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_surface_draw (sk_surface_t surface, sk_canvas_t canvas, Single x, Single y, sk_paint_t paint);
		}
		private static Delegates.sk_surface_draw sk_surface_draw_delegate;
		internal static void sk_surface_draw (sk_surface_t surface, sk_canvas_t canvas, Single x, Single y, sk_paint_t paint) =>
			(sk_surface_draw_delegate ??= GetSymbol<Delegates.sk_surface_draw> ("sk_surface_draw")).Invoke (surface, canvas, x, y, paint);
		#endif

		// void sk_surface_flush(sk_surface_t* surface)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_surface_flush (sk_surface_t surface);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_surface_flush (sk_surface_t surface);
		}
		private static Delegates.sk_surface_flush sk_surface_flush_delegate;
		internal static void sk_surface_flush (sk_surface_t surface) =>
			(sk_surface_flush_delegate ??= GetSymbol<Delegates.sk_surface_flush> ("sk_surface_flush")).Invoke (surface);
		#endif

		// void sk_surface_flush_and_submit(sk_surface_t* surface, bool syncCpu)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_surface_flush_and_submit (sk_surface_t surface, [MarshalAs (UnmanagedType.I1)] bool syncCpu);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_surface_flush_and_submit (sk_surface_t surface, [MarshalAs (UnmanagedType.I1)] bool syncCpu);
		}
		private static Delegates.sk_surface_flush_and_submit sk_surface_flush_and_submit_delegate;
		internal static void sk_surface_flush_and_submit (sk_surface_t surface, [MarshalAs (UnmanagedType.I1)] bool syncCpu) =>
			(sk_surface_flush_and_submit_delegate ??= GetSymbol<Delegates.sk_surface_flush_and_submit> ("sk_surface_flush_and_submit")).Invoke (surface, syncCpu);
		#endif

		// sk_canvas_t* sk_surface_get_canvas(sk_surface_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_surface_get_canvas (sk_surface_t param0);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surfaceprops_t sk_surface_get_props (sk_surface_t surface);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_recording_context_t sk_surface_get_recording_context (sk_surface_t surface);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_backend_render_target (gr_recording_context_t context, gr_backendrendertarget_t target, GRSurfaceOrigin origin, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_backend_texture (gr_recording_context_t context, gr_backendtexture_t texture, GRSurfaceOrigin origin, Int32 samples, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_surface_new_image_snapshot (sk_surface_t param0);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_surface_new_image_snapshot_with_crop (sk_surface_t surface, SKRectI* bounds);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_metal_layer (gr_recording_context_t context, void* layer, GRSurfaceOrigin origin, Int32 sampleCount, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props, void** drawable);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_metal_view (gr_recording_context_t context, void* mtkView, GRSurfaceOrigin origin, Int32 sampleCount, SKColorTypeNative colorType, sk_colorspace_t colorspace, sk_surfaceprops_t props);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_null (Int32 width, Int32 height);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_raster (SKImageInfoNative* param0, /* size_t */ IntPtr rowBytes, sk_surfaceprops_t param2);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_raster_direct (SKImageInfoNative* param0, void* pixels, /* size_t */ IntPtr rowBytes, SKSurfaceRasterReleaseProxyDelegate releaseProc, void* context, sk_surfaceprops_t props);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_surface_new_render_target (gr_recording_context_t context, [MarshalAs (UnmanagedType.I1)] bool budgeted, SKImageInfoNative* cinfo, Int32 sampleCount, GRSurfaceOrigin origin, sk_surfaceprops_t props, [MarshalAs (UnmanagedType.I1)] bool shouldCreateWithMips);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_surface_peek_pixels (sk_surface_t surface, sk_pixmap_t pixmap);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_surface_read_pixels (sk_surface_t surface, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_surface_unref (sk_surface_t param0);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_surfaceprops_delete (sk_surfaceprops_t props);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_surfaceprops_get_flags (sk_surfaceprops_t props);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPixelGeometry sk_surfaceprops_get_pixel_geometry (sk_surfaceprops_t props);
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
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surfaceprops_t sk_surfaceprops_new (UInt32 flags, SKPixelGeometry geometry);
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

		#region sk_svg.h

		// sk_canvas_t* sk_svgcanvas_create_with_stream(const sk_rect_t* bounds, sk_wstream_t* stream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_svgcanvas_create_with_stream (SKRect* bounds, sk_wstream_t stream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_canvas_t sk_svgcanvas_create_with_stream (SKRect* bounds, sk_wstream_t stream);
		}
		private static Delegates.sk_svgcanvas_create_with_stream sk_svgcanvas_create_with_stream_delegate;
		internal static sk_canvas_t sk_svgcanvas_create_with_stream (SKRect* bounds, sk_wstream_t stream) =>
			(sk_svgcanvas_create_with_stream_delegate ??= GetSymbol<Delegates.sk_svgcanvas_create_with_stream> ("sk_svgcanvas_create_with_stream")).Invoke (bounds, stream);
		#endif

		// sk_canvas_t* sk_svgcanvas_create_with_writer(const sk_rect_t* bounds, sk_xmlwriter_t* writer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_svgcanvas_create_with_writer (SKRect* bounds, sk_xmlwriter_t writer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_canvas_t sk_svgcanvas_create_with_writer (SKRect* bounds, sk_xmlwriter_t writer);
		}
		private static Delegates.sk_svgcanvas_create_with_writer sk_svgcanvas_create_with_writer_delegate;
		internal static sk_canvas_t sk_svgcanvas_create_with_writer (SKRect* bounds, sk_xmlwriter_t writer) =>
			(sk_svgcanvas_create_with_writer_delegate ??= GetSymbol<Delegates.sk_svgcanvas_create_with_writer> ("sk_svgcanvas_create_with_writer")).Invoke (bounds, writer);
		#endif

		#endregion

		#region sk_textblob.h

		// void sk_textblob_builder_alloc_run(sk_textblob_builder_t* builder, const sk_font_t* font, int count, float x, float y, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Single x, Single y, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Single x, Single y, SKRect* bounds, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run sk_textblob_builder_alloc_run_delegate;
		internal static void sk_textblob_builder_alloc_run (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Single x, Single y, SKRect* bounds, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run> ("sk_textblob_builder_alloc_run")).Invoke (builder, font, count, x, y, bounds, runbuffer);
		#endif

		// void sk_textblob_builder_alloc_run_pos(sk_textblob_builder_t* builder, const sk_font_t* font, int count, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_pos (sk_textblob_builder_t builder, sk_font_t font, Int32 count, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run_pos (sk_textblob_builder_t builder, sk_font_t font, Int32 count, SKRect* bounds, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run_pos sk_textblob_builder_alloc_run_pos_delegate;
		internal static void sk_textblob_builder_alloc_run_pos (sk_textblob_builder_t builder, sk_font_t font, Int32 count, SKRect* bounds, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_pos_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run_pos> ("sk_textblob_builder_alloc_run_pos")).Invoke (builder, font, count, bounds, runbuffer);
		#endif

		// void sk_textblob_builder_alloc_run_pos_h(sk_textblob_builder_t* builder, const sk_font_t* font, int count, float y, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_pos_h (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Single y, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run_pos_h (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Single y, SKRect* bounds, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run_pos_h sk_textblob_builder_alloc_run_pos_h_delegate;
		internal static void sk_textblob_builder_alloc_run_pos_h (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Single y, SKRect* bounds, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_pos_h_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run_pos_h> ("sk_textblob_builder_alloc_run_pos_h")).Invoke (builder, font, count, y, bounds, runbuffer);
		#endif

		// void sk_textblob_builder_alloc_run_rsxform(sk_textblob_builder_t* builder, const sk_font_t* font, int count, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_rsxform (sk_textblob_builder_t builder, sk_font_t font, Int32 count, SKRunBufferInternal* runbuffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run_rsxform (sk_textblob_builder_t builder, sk_font_t font, Int32 count, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run_rsxform sk_textblob_builder_alloc_run_rsxform_delegate;
		internal static void sk_textblob_builder_alloc_run_rsxform (sk_textblob_builder_t builder, sk_font_t font, Int32 count, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_rsxform_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run_rsxform> ("sk_textblob_builder_alloc_run_rsxform")).Invoke (builder, font, count, runbuffer);
		#endif

		// void sk_textblob_builder_alloc_run_text(sk_textblob_builder_t* builder, const sk_font_t* font, int count, float x, float y, int textByteCount, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_text (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Single x, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run_text (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Single x, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run_text sk_textblob_builder_alloc_run_text_delegate;
		internal static void sk_textblob_builder_alloc_run_text (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Single x, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_text_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run_text> ("sk_textblob_builder_alloc_run_text")).Invoke (builder, font, count, x, y, textByteCount, bounds, runbuffer);
		#endif

		// void sk_textblob_builder_alloc_run_text_pos(sk_textblob_builder_t* builder, const sk_font_t* font, int count, int textByteCount, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_text_pos (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run_text_pos (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run_text_pos sk_textblob_builder_alloc_run_text_pos_delegate;
		internal static void sk_textblob_builder_alloc_run_text_pos (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_text_pos_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run_text_pos> ("sk_textblob_builder_alloc_run_text_pos")).Invoke (builder, font, count, textByteCount, bounds, runbuffer);
		#endif

		// void sk_textblob_builder_alloc_run_text_pos_h(sk_textblob_builder_t* builder, const sk_font_t* font, int count, float y, int textByteCount, const sk_rect_t* bounds, sk_textblob_builder_runbuffer_t* runbuffer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_alloc_run_text_pos_h (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_alloc_run_text_pos_h (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);
		}
		private static Delegates.sk_textblob_builder_alloc_run_text_pos_h sk_textblob_builder_alloc_run_text_pos_h_delegate;
		internal static void sk_textblob_builder_alloc_run_text_pos_h (sk_textblob_builder_t builder, sk_font_t font, Int32 count, Single y, Int32 textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer) =>
			(sk_textblob_builder_alloc_run_text_pos_h_delegate ??= GetSymbol<Delegates.sk_textblob_builder_alloc_run_text_pos_h> ("sk_textblob_builder_alloc_run_text_pos_h")).Invoke (builder, font, count, y, textByteCount, bounds, runbuffer);
		#endif

		// void sk_textblob_builder_delete(sk_textblob_builder_t* builder)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_builder_delete (sk_textblob_builder_t builder);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_builder_delete (sk_textblob_builder_t builder);
		}
		private static Delegates.sk_textblob_builder_delete sk_textblob_builder_delete_delegate;
		internal static void sk_textblob_builder_delete (sk_textblob_builder_t builder) =>
			(sk_textblob_builder_delete_delegate ??= GetSymbol<Delegates.sk_textblob_builder_delete> ("sk_textblob_builder_delete")).Invoke (builder);
		#endif

		// sk_textblob_t* sk_textblob_builder_make(sk_textblob_builder_t* builder)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_textblob_t sk_textblob_builder_make (sk_textblob_builder_t builder);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_textblob_t sk_textblob_builder_make (sk_textblob_builder_t builder);
		}
		private static Delegates.sk_textblob_builder_make sk_textblob_builder_make_delegate;
		internal static sk_textblob_t sk_textblob_builder_make (sk_textblob_builder_t builder) =>
			(sk_textblob_builder_make_delegate ??= GetSymbol<Delegates.sk_textblob_builder_make> ("sk_textblob_builder_make")).Invoke (builder);
		#endif

		// sk_textblob_builder_t* sk_textblob_builder_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_textblob_builder_t sk_textblob_builder_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_textblob_builder_t sk_textblob_builder_new ();
		}
		private static Delegates.sk_textblob_builder_new sk_textblob_builder_new_delegate;
		internal static sk_textblob_builder_t sk_textblob_builder_new () =>
			(sk_textblob_builder_new_delegate ??= GetSymbol<Delegates.sk_textblob_builder_new> ("sk_textblob_builder_new")).Invoke ();
		#endif

		// void sk_textblob_get_bounds(const sk_textblob_t* blob, sk_rect_t* bounds)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_get_bounds (sk_textblob_t blob, SKRect* bounds);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_get_bounds (sk_textblob_t blob, SKRect* bounds);
		}
		private static Delegates.sk_textblob_get_bounds sk_textblob_get_bounds_delegate;
		internal static void sk_textblob_get_bounds (sk_textblob_t blob, SKRect* bounds) =>
			(sk_textblob_get_bounds_delegate ??= GetSymbol<Delegates.sk_textblob_get_bounds> ("sk_textblob_get_bounds")).Invoke (blob, bounds);
		#endif

		// int sk_textblob_get_intercepts(const sk_textblob_t* blob, const float[2] bounds = 2, float[-1] intervals, const sk_paint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_textblob_get_intercepts (sk_textblob_t blob, Single* bounds, Single* intervals, sk_paint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_textblob_get_intercepts (sk_textblob_t blob, Single* bounds, Single* intervals, sk_paint_t paint);
		}
		private static Delegates.sk_textblob_get_intercepts sk_textblob_get_intercepts_delegate;
		internal static Int32 sk_textblob_get_intercepts (sk_textblob_t blob, Single* bounds, Single* intervals, sk_paint_t paint) =>
			(sk_textblob_get_intercepts_delegate ??= GetSymbol<Delegates.sk_textblob_get_intercepts> ("sk_textblob_get_intercepts")).Invoke (blob, bounds, intervals, paint);
		#endif

		// uint32_t sk_textblob_get_unique_id(const sk_textblob_t* blob)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_textblob_get_unique_id (sk_textblob_t blob);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_textblob_get_unique_id (sk_textblob_t blob);
		}
		private static Delegates.sk_textblob_get_unique_id sk_textblob_get_unique_id_delegate;
		internal static UInt32 sk_textblob_get_unique_id (sk_textblob_t blob) =>
			(sk_textblob_get_unique_id_delegate ??= GetSymbol<Delegates.sk_textblob_get_unique_id> ("sk_textblob_get_unique_id")).Invoke (blob);
		#endif

		// void sk_textblob_ref(const sk_textblob_t* blob)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_ref (sk_textblob_t blob);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_ref (sk_textblob_t blob);
		}
		private static Delegates.sk_textblob_ref sk_textblob_ref_delegate;
		internal static void sk_textblob_ref (sk_textblob_t blob) =>
			(sk_textblob_ref_delegate ??= GetSymbol<Delegates.sk_textblob_ref> ("sk_textblob_ref")).Invoke (blob);
		#endif

		// void sk_textblob_unref(const sk_textblob_t* blob)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_textblob_unref (sk_textblob_t blob);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_textblob_unref (sk_textblob_t blob);
		}
		private static Delegates.sk_textblob_unref sk_textblob_unref_delegate;
		internal static void sk_textblob_unref (sk_textblob_t blob) =>
			(sk_textblob_unref_delegate ??= GetSymbol<Delegates.sk_textblob_unref> ("sk_textblob_unref")).Invoke (blob);
		#endif

		#endregion

		#region sk_typeface.h

		// int sk_fontmgr_count_families(sk_fontmgr_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_fontmgr_count_families (sk_fontmgr_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_fontmgr_count_families (sk_fontmgr_t param0);
		}
		private static Delegates.sk_fontmgr_count_families sk_fontmgr_count_families_delegate;
		internal static Int32 sk_fontmgr_count_families (sk_fontmgr_t param0) =>
			(sk_fontmgr_count_families_delegate ??= GetSymbol<Delegates.sk_fontmgr_count_families> ("sk_fontmgr_count_families")).Invoke (param0);
		#endif

		// sk_fontmgr_t* sk_fontmgr_create_default()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_fontmgr_t sk_fontmgr_create_default ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_fontmgr_t sk_fontmgr_create_default ();
		}
		private static Delegates.sk_fontmgr_create_default sk_fontmgr_create_default_delegate;
		internal static sk_fontmgr_t sk_fontmgr_create_default () =>
			(sk_fontmgr_create_default_delegate ??= GetSymbol<Delegates.sk_fontmgr_create_default> ("sk_fontmgr_create_default")).Invoke ();
		#endif

		// sk_typeface_t* sk_fontmgr_create_from_data(sk_fontmgr_t*, sk_data_t* data, int index)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontmgr_create_from_data (sk_fontmgr_t param0, sk_data_t data, Int32 index);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_fontmgr_create_from_data (sk_fontmgr_t param0, sk_data_t data, Int32 index);
		}
		private static Delegates.sk_fontmgr_create_from_data sk_fontmgr_create_from_data_delegate;
		internal static sk_typeface_t sk_fontmgr_create_from_data (sk_fontmgr_t param0, sk_data_t data, Int32 index) =>
			(sk_fontmgr_create_from_data_delegate ??= GetSymbol<Delegates.sk_fontmgr_create_from_data> ("sk_fontmgr_create_from_data")).Invoke (param0, data, index);
		#endif

		// sk_typeface_t* sk_fontmgr_create_from_file(sk_fontmgr_t*, const char* path, int index)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontmgr_create_from_file (sk_fontmgr_t param0, /* char */ void* path, Int32 index);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_fontmgr_create_from_file (sk_fontmgr_t param0, /* char */ void* path, Int32 index);
		}
		private static Delegates.sk_fontmgr_create_from_file sk_fontmgr_create_from_file_delegate;
		internal static sk_typeface_t sk_fontmgr_create_from_file (sk_fontmgr_t param0, /* char */ void* path, Int32 index) =>
			(sk_fontmgr_create_from_file_delegate ??= GetSymbol<Delegates.sk_fontmgr_create_from_file> ("sk_fontmgr_create_from_file")).Invoke (param0, path, index);
		#endif

		// sk_typeface_t* sk_fontmgr_create_from_stream(sk_fontmgr_t*, sk_stream_asset_t* stream, int index)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontmgr_create_from_stream (sk_fontmgr_t param0, sk_stream_asset_t stream, Int32 index);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_fontmgr_create_from_stream (sk_fontmgr_t param0, sk_stream_asset_t stream, Int32 index);
		}
		private static Delegates.sk_fontmgr_create_from_stream sk_fontmgr_create_from_stream_delegate;
		internal static sk_typeface_t sk_fontmgr_create_from_stream (sk_fontmgr_t param0, sk_stream_asset_t stream, Int32 index) =>
			(sk_fontmgr_create_from_stream_delegate ??= GetSymbol<Delegates.sk_fontmgr_create_from_stream> ("sk_fontmgr_create_from_stream")).Invoke (param0, stream, index);
		#endif

		// sk_fontstyleset_t* sk_fontmgr_create_styleset(sk_fontmgr_t*, int index)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_fontstyleset_t sk_fontmgr_create_styleset (sk_fontmgr_t param0, Int32 index);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_fontstyleset_t sk_fontmgr_create_styleset (sk_fontmgr_t param0, Int32 index);
		}
		private static Delegates.sk_fontmgr_create_styleset sk_fontmgr_create_styleset_delegate;
		internal static sk_fontstyleset_t sk_fontmgr_create_styleset (sk_fontmgr_t param0, Int32 index) =>
			(sk_fontmgr_create_styleset_delegate ??= GetSymbol<Delegates.sk_fontmgr_create_styleset> ("sk_fontmgr_create_styleset")).Invoke (param0, index);
		#endif

		// void sk_fontmgr_get_family_name(sk_fontmgr_t*, int index, sk_string_t* familyName)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontmgr_get_family_name (sk_fontmgr_t param0, Int32 index, sk_string_t familyName);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_fontmgr_get_family_name (sk_fontmgr_t param0, Int32 index, sk_string_t familyName);
		}
		private static Delegates.sk_fontmgr_get_family_name sk_fontmgr_get_family_name_delegate;
		internal static void sk_fontmgr_get_family_name (sk_fontmgr_t param0, Int32 index, sk_string_t familyName) =>
			(sk_fontmgr_get_family_name_delegate ??= GetSymbol<Delegates.sk_fontmgr_get_family_name> ("sk_fontmgr_get_family_name")).Invoke (param0, index, familyName);
		#endif

		// sk_typeface_t* sk_fontmgr_match_face_style(sk_fontmgr_t*, const sk_typeface_t* face, sk_fontstyle_t* style)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontmgr_match_face_style (sk_fontmgr_t param0, sk_typeface_t face, sk_fontstyle_t style);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_fontmgr_match_face_style (sk_fontmgr_t param0, sk_typeface_t face, sk_fontstyle_t style);
		}
		private static Delegates.sk_fontmgr_match_face_style sk_fontmgr_match_face_style_delegate;
		internal static sk_typeface_t sk_fontmgr_match_face_style (sk_fontmgr_t param0, sk_typeface_t face, sk_fontstyle_t style) =>
			(sk_fontmgr_match_face_style_delegate ??= GetSymbol<Delegates.sk_fontmgr_match_face_style> ("sk_fontmgr_match_face_style")).Invoke (param0, face, style);
		#endif

		// sk_fontstyleset_t* sk_fontmgr_match_family(sk_fontmgr_t*, const char* familyName)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_fontstyleset_t sk_fontmgr_match_family (sk_fontmgr_t param0, [MarshalAs (UnmanagedType.LPStr)] String familyName);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_fontstyleset_t sk_fontmgr_match_family (sk_fontmgr_t param0, [MarshalAs (UnmanagedType.LPStr)] String familyName);
		}
		private static Delegates.sk_fontmgr_match_family sk_fontmgr_match_family_delegate;
		internal static sk_fontstyleset_t sk_fontmgr_match_family (sk_fontmgr_t param0, [MarshalAs (UnmanagedType.LPStr)] String familyName) =>
			(sk_fontmgr_match_family_delegate ??= GetSymbol<Delegates.sk_fontmgr_match_family> ("sk_fontmgr_match_family")).Invoke (param0, familyName);
		#endif

		// sk_typeface_t* sk_fontmgr_match_family_style(sk_fontmgr_t*, const char* familyName, sk_fontstyle_t* style)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontmgr_match_family_style (sk_fontmgr_t param0, [MarshalAs (UnmanagedType.LPStr)] String familyName, sk_fontstyle_t style);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_fontmgr_match_family_style (sk_fontmgr_t param0, [MarshalAs (UnmanagedType.LPStr)] String familyName, sk_fontstyle_t style);
		}
		private static Delegates.sk_fontmgr_match_family_style sk_fontmgr_match_family_style_delegate;
		internal static sk_typeface_t sk_fontmgr_match_family_style (sk_fontmgr_t param0, [MarshalAs (UnmanagedType.LPStr)] String familyName, sk_fontstyle_t style) =>
			(sk_fontmgr_match_family_style_delegate ??= GetSymbol<Delegates.sk_fontmgr_match_family_style> ("sk_fontmgr_match_family_style")).Invoke (param0, familyName, style);
		#endif

		// sk_typeface_t* sk_fontmgr_match_family_style_character(sk_fontmgr_t*, const char* familyName, sk_fontstyle_t* style, const char** bcp47, int bcp47Count, int32_t character)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontmgr_match_family_style_character (sk_fontmgr_t param0, [MarshalAs (UnmanagedType.LPStr)] String familyName, sk_fontstyle_t style, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] bcp47, Int32 bcp47Count, Int32 character);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_fontmgr_match_family_style_character (sk_fontmgr_t param0, [MarshalAs (UnmanagedType.LPStr)] String familyName, sk_fontstyle_t style, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] bcp47, Int32 bcp47Count, Int32 character);
		}
		private static Delegates.sk_fontmgr_match_family_style_character sk_fontmgr_match_family_style_character_delegate;
		internal static sk_typeface_t sk_fontmgr_match_family_style_character (sk_fontmgr_t param0, [MarshalAs (UnmanagedType.LPStr)] String familyName, sk_fontstyle_t style, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] bcp47, Int32 bcp47Count, Int32 character) =>
			(sk_fontmgr_match_family_style_character_delegate ??= GetSymbol<Delegates.sk_fontmgr_match_family_style_character> ("sk_fontmgr_match_family_style_character")).Invoke (param0, familyName, style, bcp47, bcp47Count, character);
		#endif

		// sk_fontmgr_t* sk_fontmgr_ref_default()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_fontmgr_t sk_fontmgr_ref_default ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_fontmgr_t sk_fontmgr_ref_default ();
		}
		private static Delegates.sk_fontmgr_ref_default sk_fontmgr_ref_default_delegate;
		internal static sk_fontmgr_t sk_fontmgr_ref_default () =>
			(sk_fontmgr_ref_default_delegate ??= GetSymbol<Delegates.sk_fontmgr_ref_default> ("sk_fontmgr_ref_default")).Invoke ();
		#endif

		// void sk_fontmgr_unref(sk_fontmgr_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontmgr_unref (sk_fontmgr_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_fontmgr_unref (sk_fontmgr_t param0);
		}
		private static Delegates.sk_fontmgr_unref sk_fontmgr_unref_delegate;
		internal static void sk_fontmgr_unref (sk_fontmgr_t param0) =>
			(sk_fontmgr_unref_delegate ??= GetSymbol<Delegates.sk_fontmgr_unref> ("sk_fontmgr_unref")).Invoke (param0);
		#endif

		// void sk_fontstyle_delete(sk_fontstyle_t* fs)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontstyle_delete (sk_fontstyle_t fs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_fontstyle_delete (sk_fontstyle_t fs);
		}
		private static Delegates.sk_fontstyle_delete sk_fontstyle_delete_delegate;
		internal static void sk_fontstyle_delete (sk_fontstyle_t fs) =>
			(sk_fontstyle_delete_delegate ??= GetSymbol<Delegates.sk_fontstyle_delete> ("sk_fontstyle_delete")).Invoke (fs);
		#endif

		// sk_font_style_slant_t sk_fontstyle_get_slant(const sk_fontstyle_t* fs)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKFontStyleSlant sk_fontstyle_get_slant (sk_fontstyle_t fs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKFontStyleSlant sk_fontstyle_get_slant (sk_fontstyle_t fs);
		}
		private static Delegates.sk_fontstyle_get_slant sk_fontstyle_get_slant_delegate;
		internal static SKFontStyleSlant sk_fontstyle_get_slant (sk_fontstyle_t fs) =>
			(sk_fontstyle_get_slant_delegate ??= GetSymbol<Delegates.sk_fontstyle_get_slant> ("sk_fontstyle_get_slant")).Invoke (fs);
		#endif

		// int sk_fontstyle_get_weight(const sk_fontstyle_t* fs)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_fontstyle_get_weight (sk_fontstyle_t fs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_fontstyle_get_weight (sk_fontstyle_t fs);
		}
		private static Delegates.sk_fontstyle_get_weight sk_fontstyle_get_weight_delegate;
		internal static Int32 sk_fontstyle_get_weight (sk_fontstyle_t fs) =>
			(sk_fontstyle_get_weight_delegate ??= GetSymbol<Delegates.sk_fontstyle_get_weight> ("sk_fontstyle_get_weight")).Invoke (fs);
		#endif

		// int sk_fontstyle_get_width(const sk_fontstyle_t* fs)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_fontstyle_get_width (sk_fontstyle_t fs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_fontstyle_get_width (sk_fontstyle_t fs);
		}
		private static Delegates.sk_fontstyle_get_width sk_fontstyle_get_width_delegate;
		internal static Int32 sk_fontstyle_get_width (sk_fontstyle_t fs) =>
			(sk_fontstyle_get_width_delegate ??= GetSymbol<Delegates.sk_fontstyle_get_width> ("sk_fontstyle_get_width")).Invoke (fs);
		#endif

		// sk_fontstyle_t* sk_fontstyle_new(int weight, int width, sk_font_style_slant_t slant)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_fontstyle_t sk_fontstyle_new (Int32 weight, Int32 width, SKFontStyleSlant slant);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_fontstyle_t sk_fontstyle_new (Int32 weight, Int32 width, SKFontStyleSlant slant);
		}
		private static Delegates.sk_fontstyle_new sk_fontstyle_new_delegate;
		internal static sk_fontstyle_t sk_fontstyle_new (Int32 weight, Int32 width, SKFontStyleSlant slant) =>
			(sk_fontstyle_new_delegate ??= GetSymbol<Delegates.sk_fontstyle_new> ("sk_fontstyle_new")).Invoke (weight, width, slant);
		#endif

		// sk_fontstyleset_t* sk_fontstyleset_create_empty()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_fontstyleset_t sk_fontstyleset_create_empty ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_fontstyleset_t sk_fontstyleset_create_empty ();
		}
		private static Delegates.sk_fontstyleset_create_empty sk_fontstyleset_create_empty_delegate;
		internal static sk_fontstyleset_t sk_fontstyleset_create_empty () =>
			(sk_fontstyleset_create_empty_delegate ??= GetSymbol<Delegates.sk_fontstyleset_create_empty> ("sk_fontstyleset_create_empty")).Invoke ();
		#endif

		// sk_typeface_t* sk_fontstyleset_create_typeface(sk_fontstyleset_t* fss, int index)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontstyleset_create_typeface (sk_fontstyleset_t fss, Int32 index);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_fontstyleset_create_typeface (sk_fontstyleset_t fss, Int32 index);
		}
		private static Delegates.sk_fontstyleset_create_typeface sk_fontstyleset_create_typeface_delegate;
		internal static sk_typeface_t sk_fontstyleset_create_typeface (sk_fontstyleset_t fss, Int32 index) =>
			(sk_fontstyleset_create_typeface_delegate ??= GetSymbol<Delegates.sk_fontstyleset_create_typeface> ("sk_fontstyleset_create_typeface")).Invoke (fss, index);
		#endif

		// int sk_fontstyleset_get_count(sk_fontstyleset_t* fss)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_fontstyleset_get_count (sk_fontstyleset_t fss);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_fontstyleset_get_count (sk_fontstyleset_t fss);
		}
		private static Delegates.sk_fontstyleset_get_count sk_fontstyleset_get_count_delegate;
		internal static Int32 sk_fontstyleset_get_count (sk_fontstyleset_t fss) =>
			(sk_fontstyleset_get_count_delegate ??= GetSymbol<Delegates.sk_fontstyleset_get_count> ("sk_fontstyleset_get_count")).Invoke (fss);
		#endif

		// void sk_fontstyleset_get_style(sk_fontstyleset_t* fss, int index, sk_fontstyle_t* fs, sk_string_t* style)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontstyleset_get_style (sk_fontstyleset_t fss, Int32 index, sk_fontstyle_t fs, sk_string_t style);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_fontstyleset_get_style (sk_fontstyleset_t fss, Int32 index, sk_fontstyle_t fs, sk_string_t style);
		}
		private static Delegates.sk_fontstyleset_get_style sk_fontstyleset_get_style_delegate;
		internal static void sk_fontstyleset_get_style (sk_fontstyleset_t fss, Int32 index, sk_fontstyle_t fs, sk_string_t style) =>
			(sk_fontstyleset_get_style_delegate ??= GetSymbol<Delegates.sk_fontstyleset_get_style> ("sk_fontstyleset_get_style")).Invoke (fss, index, fs, style);
		#endif

		// sk_typeface_t* sk_fontstyleset_match_style(sk_fontstyleset_t* fss, sk_fontstyle_t* style)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_fontstyleset_match_style (sk_fontstyleset_t fss, sk_fontstyle_t style);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_fontstyleset_match_style (sk_fontstyleset_t fss, sk_fontstyle_t style);
		}
		private static Delegates.sk_fontstyleset_match_style sk_fontstyleset_match_style_delegate;
		internal static sk_typeface_t sk_fontstyleset_match_style (sk_fontstyleset_t fss, sk_fontstyle_t style) =>
			(sk_fontstyleset_match_style_delegate ??= GetSymbol<Delegates.sk_fontstyleset_match_style> ("sk_fontstyleset_match_style")).Invoke (fss, style);
		#endif

		// void sk_fontstyleset_unref(sk_fontstyleset_t* fss)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontstyleset_unref (sk_fontstyleset_t fss);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_fontstyleset_unref (sk_fontstyleset_t fss);
		}
		private static Delegates.sk_fontstyleset_unref sk_fontstyleset_unref_delegate;
		internal static void sk_fontstyleset_unref (sk_fontstyleset_t fss) =>
			(sk_fontstyleset_unref_delegate ??= GetSymbol<Delegates.sk_fontstyleset_unref> ("sk_fontstyleset_unref")).Invoke (fss);
		#endif

		// sk_data_t* sk_typeface_copy_table_data(const sk_typeface_t* typeface, sk_font_table_tag_t tag)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_typeface_copy_table_data (sk_typeface_t typeface, UInt32 tag);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_typeface_copy_table_data (sk_typeface_t typeface, UInt32 tag);
		}
		private static Delegates.sk_typeface_copy_table_data sk_typeface_copy_table_data_delegate;
		internal static sk_data_t sk_typeface_copy_table_data (sk_typeface_t typeface, UInt32 tag) =>
			(sk_typeface_copy_table_data_delegate ??= GetSymbol<Delegates.sk_typeface_copy_table_data> ("sk_typeface_copy_table_data")).Invoke (typeface, tag);
		#endif

		// int sk_typeface_count_glyphs(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_count_glyphs (sk_typeface_t typeface);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_typeface_count_glyphs (sk_typeface_t typeface);
		}
		private static Delegates.sk_typeface_count_glyphs sk_typeface_count_glyphs_delegate;
		internal static Int32 sk_typeface_count_glyphs (sk_typeface_t typeface) =>
			(sk_typeface_count_glyphs_delegate ??= GetSymbol<Delegates.sk_typeface_count_glyphs> ("sk_typeface_count_glyphs")).Invoke (typeface);
		#endif

		// int sk_typeface_count_tables(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_count_tables (sk_typeface_t typeface);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_typeface_count_tables (sk_typeface_t typeface);
		}
		private static Delegates.sk_typeface_count_tables sk_typeface_count_tables_delegate;
		internal static Int32 sk_typeface_count_tables (sk_typeface_t typeface) =>
			(sk_typeface_count_tables_delegate ??= GetSymbol<Delegates.sk_typeface_count_tables> ("sk_typeface_count_tables")).Invoke (typeface);
		#endif

		// sk_typeface_t* sk_typeface_create_default()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_typeface_create_default ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_typeface_create_default ();
		}
		private static Delegates.sk_typeface_create_default sk_typeface_create_default_delegate;
		internal static sk_typeface_t sk_typeface_create_default () =>
			(sk_typeface_create_default_delegate ??= GetSymbol<Delegates.sk_typeface_create_default> ("sk_typeface_create_default")).Invoke ();
		#endif

		// sk_typeface_t* sk_typeface_create_from_data(sk_data_t* data, int index)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_typeface_create_from_data (sk_data_t data, Int32 index);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_typeface_create_from_data (sk_data_t data, Int32 index);
		}
		private static Delegates.sk_typeface_create_from_data sk_typeface_create_from_data_delegate;
		internal static sk_typeface_t sk_typeface_create_from_data (sk_data_t data, Int32 index) =>
			(sk_typeface_create_from_data_delegate ??= GetSymbol<Delegates.sk_typeface_create_from_data> ("sk_typeface_create_from_data")).Invoke (data, index);
		#endif

		// sk_typeface_t* sk_typeface_create_from_file(const char* path, int index)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_typeface_create_from_file (/* char */ void* path, Int32 index);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_typeface_create_from_file (/* char */ void* path, Int32 index);
		}
		private static Delegates.sk_typeface_create_from_file sk_typeface_create_from_file_delegate;
		internal static sk_typeface_t sk_typeface_create_from_file (/* char */ void* path, Int32 index) =>
			(sk_typeface_create_from_file_delegate ??= GetSymbol<Delegates.sk_typeface_create_from_file> ("sk_typeface_create_from_file")).Invoke (path, index);
		#endif

		// sk_typeface_t* sk_typeface_create_from_name(const char* familyName, const sk_fontstyle_t* style)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_typeface_create_from_name ([MarshalAs (UnmanagedType.LPStr)] String familyName, sk_fontstyle_t style);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_typeface_create_from_name ([MarshalAs (UnmanagedType.LPStr)] String familyName, sk_fontstyle_t style);
		}
		private static Delegates.sk_typeface_create_from_name sk_typeface_create_from_name_delegate;
		internal static sk_typeface_t sk_typeface_create_from_name ([MarshalAs (UnmanagedType.LPStr)] String familyName, sk_fontstyle_t style) =>
			(sk_typeface_create_from_name_delegate ??= GetSymbol<Delegates.sk_typeface_create_from_name> ("sk_typeface_create_from_name")).Invoke (familyName, style);
		#endif

		// sk_typeface_t* sk_typeface_create_from_stream(sk_stream_asset_t* stream, int index)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_typeface_create_from_stream (sk_stream_asset_t stream, Int32 index);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_typeface_create_from_stream (sk_stream_asset_t stream, Int32 index);
		}
		private static Delegates.sk_typeface_create_from_stream sk_typeface_create_from_stream_delegate;
		internal static sk_typeface_t sk_typeface_create_from_stream (sk_stream_asset_t stream, Int32 index) =>
			(sk_typeface_create_from_stream_delegate ??= GetSymbol<Delegates.sk_typeface_create_from_stream> ("sk_typeface_create_from_stream")).Invoke (stream, index);
		#endif

		// sk_string_t* sk_typeface_get_family_name(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_string_t sk_typeface_get_family_name (sk_typeface_t typeface);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_string_t sk_typeface_get_family_name (sk_typeface_t typeface);
		}
		private static Delegates.sk_typeface_get_family_name sk_typeface_get_family_name_delegate;
		internal static sk_string_t sk_typeface_get_family_name (sk_typeface_t typeface) =>
			(sk_typeface_get_family_name_delegate ??= GetSymbol<Delegates.sk_typeface_get_family_name> ("sk_typeface_get_family_name")).Invoke (typeface);
		#endif

		// sk_font_style_slant_t sk_typeface_get_font_slant(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKFontStyleSlant sk_typeface_get_font_slant (sk_typeface_t typeface);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKFontStyleSlant sk_typeface_get_font_slant (sk_typeface_t typeface);
		}
		private static Delegates.sk_typeface_get_font_slant sk_typeface_get_font_slant_delegate;
		internal static SKFontStyleSlant sk_typeface_get_font_slant (sk_typeface_t typeface) =>
			(sk_typeface_get_font_slant_delegate ??= GetSymbol<Delegates.sk_typeface_get_font_slant> ("sk_typeface_get_font_slant")).Invoke (typeface);
		#endif

		// int sk_typeface_get_font_weight(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_get_font_weight (sk_typeface_t typeface);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_typeface_get_font_weight (sk_typeface_t typeface);
		}
		private static Delegates.sk_typeface_get_font_weight sk_typeface_get_font_weight_delegate;
		internal static Int32 sk_typeface_get_font_weight (sk_typeface_t typeface) =>
			(sk_typeface_get_font_weight_delegate ??= GetSymbol<Delegates.sk_typeface_get_font_weight> ("sk_typeface_get_font_weight")).Invoke (typeface);
		#endif

		// int sk_typeface_get_font_width(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_get_font_width (sk_typeface_t typeface);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_typeface_get_font_width (sk_typeface_t typeface);
		}
		private static Delegates.sk_typeface_get_font_width sk_typeface_get_font_width_delegate;
		internal static Int32 sk_typeface_get_font_width (sk_typeface_t typeface) =>
			(sk_typeface_get_font_width_delegate ??= GetSymbol<Delegates.sk_typeface_get_font_width> ("sk_typeface_get_font_width")).Invoke (typeface);
		#endif

		// sk_fontstyle_t* sk_typeface_get_fontstyle(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_fontstyle_t sk_typeface_get_fontstyle (sk_typeface_t typeface);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_fontstyle_t sk_typeface_get_fontstyle (sk_typeface_t typeface);
		}
		private static Delegates.sk_typeface_get_fontstyle sk_typeface_get_fontstyle_delegate;
		internal static sk_fontstyle_t sk_typeface_get_fontstyle (sk_typeface_t typeface) =>
			(sk_typeface_get_fontstyle_delegate ??= GetSymbol<Delegates.sk_typeface_get_fontstyle> ("sk_typeface_get_fontstyle")).Invoke (typeface);
		#endif

		// bool sk_typeface_get_kerning_pair_adjustments(const sk_typeface_t* typeface, const uint16_t[-1] glyphs, int count, int32_t[-1] adjustments)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_typeface_get_kerning_pair_adjustments (sk_typeface_t typeface, UInt16* glyphs, Int32 count, Int32* adjustments);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_typeface_get_kerning_pair_adjustments (sk_typeface_t typeface, UInt16* glyphs, Int32 count, Int32* adjustments);
		}
		private static Delegates.sk_typeface_get_kerning_pair_adjustments sk_typeface_get_kerning_pair_adjustments_delegate;
		internal static bool sk_typeface_get_kerning_pair_adjustments (sk_typeface_t typeface, UInt16* glyphs, Int32 count, Int32* adjustments) =>
			(sk_typeface_get_kerning_pair_adjustments_delegate ??= GetSymbol<Delegates.sk_typeface_get_kerning_pair_adjustments> ("sk_typeface_get_kerning_pair_adjustments")).Invoke (typeface, glyphs, count, adjustments);
		#endif

		// size_t sk_typeface_get_table_data(const sk_typeface_t* typeface, sk_font_table_tag_t tag, size_t offset, size_t length, void* data)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_typeface_get_table_data (sk_typeface_t typeface, UInt32 tag, /* size_t */ IntPtr offset, /* size_t */ IntPtr length, void* data);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_typeface_get_table_data (sk_typeface_t typeface, UInt32 tag, /* size_t */ IntPtr offset, /* size_t */ IntPtr length, void* data);
		}
		private static Delegates.sk_typeface_get_table_data sk_typeface_get_table_data_delegate;
		internal static /* size_t */ IntPtr sk_typeface_get_table_data (sk_typeface_t typeface, UInt32 tag, /* size_t */ IntPtr offset, /* size_t */ IntPtr length, void* data) =>
			(sk_typeface_get_table_data_delegate ??= GetSymbol<Delegates.sk_typeface_get_table_data> ("sk_typeface_get_table_data")).Invoke (typeface, tag, offset, length, data);
		#endif

		// size_t sk_typeface_get_table_size(const sk_typeface_t* typeface, sk_font_table_tag_t tag)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_typeface_get_table_size (sk_typeface_t typeface, UInt32 tag);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_typeface_get_table_size (sk_typeface_t typeface, UInt32 tag);
		}
		private static Delegates.sk_typeface_get_table_size sk_typeface_get_table_size_delegate;
		internal static /* size_t */ IntPtr sk_typeface_get_table_size (sk_typeface_t typeface, UInt32 tag) =>
			(sk_typeface_get_table_size_delegate ??= GetSymbol<Delegates.sk_typeface_get_table_size> ("sk_typeface_get_table_size")).Invoke (typeface, tag);
		#endif

		// int sk_typeface_get_table_tags(const sk_typeface_t* typeface, sk_font_table_tag_t[-1] tags)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_get_table_tags (sk_typeface_t typeface, UInt32* tags);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_typeface_get_table_tags (sk_typeface_t typeface, UInt32* tags);
		}
		private static Delegates.sk_typeface_get_table_tags sk_typeface_get_table_tags_delegate;
		internal static Int32 sk_typeface_get_table_tags (sk_typeface_t typeface, UInt32* tags) =>
			(sk_typeface_get_table_tags_delegate ??= GetSymbol<Delegates.sk_typeface_get_table_tags> ("sk_typeface_get_table_tags")).Invoke (typeface, tags);
		#endif

		// int sk_typeface_get_units_per_em(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_get_units_per_em (sk_typeface_t typeface);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_typeface_get_units_per_em (sk_typeface_t typeface);
		}
		private static Delegates.sk_typeface_get_units_per_em sk_typeface_get_units_per_em_delegate;
		internal static Int32 sk_typeface_get_units_per_em (sk_typeface_t typeface) =>
			(sk_typeface_get_units_per_em_delegate ??= GetSymbol<Delegates.sk_typeface_get_units_per_em> ("sk_typeface_get_units_per_em")).Invoke (typeface);
		#endif

		// bool sk_typeface_is_fixed_pitch(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_typeface_is_fixed_pitch (sk_typeface_t typeface);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_typeface_is_fixed_pitch (sk_typeface_t typeface);
		}
		private static Delegates.sk_typeface_is_fixed_pitch sk_typeface_is_fixed_pitch_delegate;
		internal static bool sk_typeface_is_fixed_pitch (sk_typeface_t typeface) =>
			(sk_typeface_is_fixed_pitch_delegate ??= GetSymbol<Delegates.sk_typeface_is_fixed_pitch> ("sk_typeface_is_fixed_pitch")).Invoke (typeface);
		#endif

		// sk_stream_asset_t* sk_typeface_open_stream(const sk_typeface_t* typeface, int* ttcIndex)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_asset_t sk_typeface_open_stream (sk_typeface_t typeface, Int32* ttcIndex);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_asset_t sk_typeface_open_stream (sk_typeface_t typeface, Int32* ttcIndex);
		}
		private static Delegates.sk_typeface_open_stream sk_typeface_open_stream_delegate;
		internal static sk_stream_asset_t sk_typeface_open_stream (sk_typeface_t typeface, Int32* ttcIndex) =>
			(sk_typeface_open_stream_delegate ??= GetSymbol<Delegates.sk_typeface_open_stream> ("sk_typeface_open_stream")).Invoke (typeface, ttcIndex);
		#endif

		// sk_typeface_t* sk_typeface_ref_default()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t sk_typeface_ref_default ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t sk_typeface_ref_default ();
		}
		private static Delegates.sk_typeface_ref_default sk_typeface_ref_default_delegate;
		internal static sk_typeface_t sk_typeface_ref_default () =>
			(sk_typeface_ref_default_delegate ??= GetSymbol<Delegates.sk_typeface_ref_default> ("sk_typeface_ref_default")).Invoke ();
		#endif

		// uint16_t sk_typeface_unichar_to_glyph(const sk_typeface_t* typeface, const int32_t unichar)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt16 sk_typeface_unichar_to_glyph (sk_typeface_t typeface, Int32 unichar);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt16 sk_typeface_unichar_to_glyph (sk_typeface_t typeface, Int32 unichar);
		}
		private static Delegates.sk_typeface_unichar_to_glyph sk_typeface_unichar_to_glyph_delegate;
		internal static UInt16 sk_typeface_unichar_to_glyph (sk_typeface_t typeface, Int32 unichar) =>
			(sk_typeface_unichar_to_glyph_delegate ??= GetSymbol<Delegates.sk_typeface_unichar_to_glyph> ("sk_typeface_unichar_to_glyph")).Invoke (typeface, unichar);
		#endif

		// void sk_typeface_unichars_to_glyphs(const sk_typeface_t* typeface, const int32_t[-1] unichars, int count, uint16_t[-1] glyphs)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_typeface_unichars_to_glyphs (sk_typeface_t typeface, Int32* unichars, Int32 count, UInt16* glyphs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_typeface_unichars_to_glyphs (sk_typeface_t typeface, Int32* unichars, Int32 count, UInt16* glyphs);
		}
		private static Delegates.sk_typeface_unichars_to_glyphs sk_typeface_unichars_to_glyphs_delegate;
		internal static void sk_typeface_unichars_to_glyphs (sk_typeface_t typeface, Int32* unichars, Int32 count, UInt16* glyphs) =>
			(sk_typeface_unichars_to_glyphs_delegate ??= GetSymbol<Delegates.sk_typeface_unichars_to_glyphs> ("sk_typeface_unichars_to_glyphs")).Invoke (typeface, unichars, count, glyphs);
		#endif

		// void sk_typeface_unref(sk_typeface_t* typeface)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_typeface_unref (sk_typeface_t typeface);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_typeface_unref (sk_typeface_t typeface);
		}
		private static Delegates.sk_typeface_unref sk_typeface_unref_delegate;
		internal static void sk_typeface_unref (sk_typeface_t typeface) =>
			(sk_typeface_unref_delegate ??= GetSymbol<Delegates.sk_typeface_unref> ("sk_typeface_unref")).Invoke (typeface);
		#endif

		#endregion

		#region sk_vertices.h

		// sk_vertices_t* sk_vertices_make_copy(sk_vertices_vertex_mode_t vmode, int vertexCount, const sk_point_t* positions, const sk_point_t* texs, const sk_color_t* colors, int indexCount, const uint16_t* indices)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_vertices_t sk_vertices_make_copy (SKVertexMode vmode, Int32 vertexCount, SKPoint* positions, SKPoint* texs, UInt32* colors, Int32 indexCount, UInt16* indices);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_vertices_t sk_vertices_make_copy (SKVertexMode vmode, Int32 vertexCount, SKPoint* positions, SKPoint* texs, UInt32* colors, Int32 indexCount, UInt16* indices);
		}
		private static Delegates.sk_vertices_make_copy sk_vertices_make_copy_delegate;
		internal static sk_vertices_t sk_vertices_make_copy (SKVertexMode vmode, Int32 vertexCount, SKPoint* positions, SKPoint* texs, UInt32* colors, Int32 indexCount, UInt16* indices) =>
			(sk_vertices_make_copy_delegate ??= GetSymbol<Delegates.sk_vertices_make_copy> ("sk_vertices_make_copy")).Invoke (vmode, vertexCount, positions, texs, colors, indexCount, indices);
		#endif

		// void sk_vertices_ref(sk_vertices_t* cvertices)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_vertices_ref (sk_vertices_t cvertices);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_vertices_ref (sk_vertices_t cvertices);
		}
		private static Delegates.sk_vertices_ref sk_vertices_ref_delegate;
		internal static void sk_vertices_ref (sk_vertices_t cvertices) =>
			(sk_vertices_ref_delegate ??= GetSymbol<Delegates.sk_vertices_ref> ("sk_vertices_ref")).Invoke (cvertices);
		#endif

		// void sk_vertices_unref(sk_vertices_t* cvertices)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_vertices_unref (sk_vertices_t cvertices);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_vertices_unref (sk_vertices_t cvertices);
		}
		private static Delegates.sk_vertices_unref sk_vertices_unref_delegate;
		internal static void sk_vertices_unref (sk_vertices_t cvertices) =>
			(sk_vertices_unref_delegate ??= GetSymbol<Delegates.sk_vertices_unref> ("sk_vertices_unref")).Invoke (cvertices);
		#endif

		#endregion

		#region sk_xml.h

		// void sk_xmlstreamwriter_delete(sk_xmlstreamwriter_t* writer)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_xmlstreamwriter_delete (sk_xmlstreamwriter_t writer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_xmlstreamwriter_delete (sk_xmlstreamwriter_t writer);
		}
		private static Delegates.sk_xmlstreamwriter_delete sk_xmlstreamwriter_delete_delegate;
		internal static void sk_xmlstreamwriter_delete (sk_xmlstreamwriter_t writer) =>
			(sk_xmlstreamwriter_delete_delegate ??= GetSymbol<Delegates.sk_xmlstreamwriter_delete> ("sk_xmlstreamwriter_delete")).Invoke (writer);
		#endif

		// sk_xmlstreamwriter_t* sk_xmlstreamwriter_new(sk_wstream_t* stream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_xmlstreamwriter_t sk_xmlstreamwriter_new (sk_wstream_t stream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_xmlstreamwriter_t sk_xmlstreamwriter_new (sk_wstream_t stream);
		}
		private static Delegates.sk_xmlstreamwriter_new sk_xmlstreamwriter_new_delegate;
		internal static sk_xmlstreamwriter_t sk_xmlstreamwriter_new (sk_wstream_t stream) =>
			(sk_xmlstreamwriter_new_delegate ??= GetSymbol<Delegates.sk_xmlstreamwriter_new> ("sk_xmlstreamwriter_new")).Invoke (stream);
		#endif

		#endregion

		#region sk_compatpaint.h

		// sk_compatpaint_t* sk_compatpaint_clone(const sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_compatpaint_t sk_compatpaint_clone (sk_compatpaint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_compatpaint_t sk_compatpaint_clone (sk_compatpaint_t paint);
		}
		private static Delegates.sk_compatpaint_clone sk_compatpaint_clone_delegate;
		internal static sk_compatpaint_t sk_compatpaint_clone (sk_compatpaint_t paint) =>
			(sk_compatpaint_clone_delegate ??= GetSymbol<Delegates.sk_compatpaint_clone> ("sk_compatpaint_clone")).Invoke (paint);
		#endif

		// void sk_compatpaint_delete(sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_compatpaint_delete (sk_compatpaint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_compatpaint_delete (sk_compatpaint_t paint);
		}
		private static Delegates.sk_compatpaint_delete sk_compatpaint_delete_delegate;
		internal static void sk_compatpaint_delete (sk_compatpaint_t paint) =>
			(sk_compatpaint_delete_delegate ??= GetSymbol<Delegates.sk_compatpaint_delete> ("sk_compatpaint_delete")).Invoke (paint);
		#endif

		// sk_font_t* sk_compatpaint_get_font(sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_font_t sk_compatpaint_get_font (sk_compatpaint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_font_t sk_compatpaint_get_font (sk_compatpaint_t paint);
		}
		private static Delegates.sk_compatpaint_get_font sk_compatpaint_get_font_delegate;
		internal static sk_font_t sk_compatpaint_get_font (sk_compatpaint_t paint) =>
			(sk_compatpaint_get_font_delegate ??= GetSymbol<Delegates.sk_compatpaint_get_font> ("sk_compatpaint_get_font")).Invoke (paint);
		#endif

		// sk_text_align_t sk_compatpaint_get_text_align(const sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKTextAlign sk_compatpaint_get_text_align (sk_compatpaint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKTextAlign sk_compatpaint_get_text_align (sk_compatpaint_t paint);
		}
		private static Delegates.sk_compatpaint_get_text_align sk_compatpaint_get_text_align_delegate;
		internal static SKTextAlign sk_compatpaint_get_text_align (sk_compatpaint_t paint) =>
			(sk_compatpaint_get_text_align_delegate ??= GetSymbol<Delegates.sk_compatpaint_get_text_align> ("sk_compatpaint_get_text_align")).Invoke (paint);
		#endif

		// sk_text_encoding_t sk_compatpaint_get_text_encoding(const sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKTextEncoding sk_compatpaint_get_text_encoding (sk_compatpaint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKTextEncoding sk_compatpaint_get_text_encoding (sk_compatpaint_t paint);
		}
		private static Delegates.sk_compatpaint_get_text_encoding sk_compatpaint_get_text_encoding_delegate;
		internal static SKTextEncoding sk_compatpaint_get_text_encoding (sk_compatpaint_t paint) =>
			(sk_compatpaint_get_text_encoding_delegate ??= GetSymbol<Delegates.sk_compatpaint_get_text_encoding> ("sk_compatpaint_get_text_encoding")).Invoke (paint);
		#endif

		// sk_font_t* sk_compatpaint_make_font(sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_font_t sk_compatpaint_make_font (sk_compatpaint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_font_t sk_compatpaint_make_font (sk_compatpaint_t paint);
		}
		private static Delegates.sk_compatpaint_make_font sk_compatpaint_make_font_delegate;
		internal static sk_font_t sk_compatpaint_make_font (sk_compatpaint_t paint) =>
			(sk_compatpaint_make_font_delegate ??= GetSymbol<Delegates.sk_compatpaint_make_font> ("sk_compatpaint_make_font")).Invoke (paint);
		#endif

		// sk_compatpaint_t* sk_compatpaint_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_compatpaint_t sk_compatpaint_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_compatpaint_t sk_compatpaint_new ();
		}
		private static Delegates.sk_compatpaint_new sk_compatpaint_new_delegate;
		internal static sk_compatpaint_t sk_compatpaint_new () =>
			(sk_compatpaint_new_delegate ??= GetSymbol<Delegates.sk_compatpaint_new> ("sk_compatpaint_new")).Invoke ();
		#endif

		// sk_compatpaint_t* sk_compatpaint_new_with_font(const sk_font_t* font)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_compatpaint_t sk_compatpaint_new_with_font (sk_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_compatpaint_t sk_compatpaint_new_with_font (sk_font_t font);
		}
		private static Delegates.sk_compatpaint_new_with_font sk_compatpaint_new_with_font_delegate;
		internal static sk_compatpaint_t sk_compatpaint_new_with_font (sk_font_t font) =>
			(sk_compatpaint_new_with_font_delegate ??= GetSymbol<Delegates.sk_compatpaint_new_with_font> ("sk_compatpaint_new_with_font")).Invoke (font);
		#endif

		// void sk_compatpaint_reset(sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_compatpaint_reset (sk_compatpaint_t paint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_compatpaint_reset (sk_compatpaint_t paint);
		}
		private static Delegates.sk_compatpaint_reset sk_compatpaint_reset_delegate;
		internal static void sk_compatpaint_reset (sk_compatpaint_t paint) =>
			(sk_compatpaint_reset_delegate ??= GetSymbol<Delegates.sk_compatpaint_reset> ("sk_compatpaint_reset")).Invoke (paint);
		#endif

		// void sk_compatpaint_set_text_align(sk_compatpaint_t* paint, sk_text_align_t align)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_compatpaint_set_text_align (sk_compatpaint_t paint, SKTextAlign align);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_compatpaint_set_text_align (sk_compatpaint_t paint, SKTextAlign align);
		}
		private static Delegates.sk_compatpaint_set_text_align sk_compatpaint_set_text_align_delegate;
		internal static void sk_compatpaint_set_text_align (sk_compatpaint_t paint, SKTextAlign align) =>
			(sk_compatpaint_set_text_align_delegate ??= GetSymbol<Delegates.sk_compatpaint_set_text_align> ("sk_compatpaint_set_text_align")).Invoke (paint, align);
		#endif

		// void sk_compatpaint_set_text_encoding(sk_compatpaint_t* paint, sk_text_encoding_t encoding)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_compatpaint_set_text_encoding (sk_compatpaint_t paint, SKTextEncoding encoding);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_compatpaint_set_text_encoding (sk_compatpaint_t paint, SKTextEncoding encoding);
		}
		private static Delegates.sk_compatpaint_set_text_encoding sk_compatpaint_set_text_encoding_delegate;
		internal static void sk_compatpaint_set_text_encoding (sk_compatpaint_t paint, SKTextEncoding encoding) =>
			(sk_compatpaint_set_text_encoding_delegate ??= GetSymbol<Delegates.sk_compatpaint_set_text_encoding> ("sk_compatpaint_set_text_encoding")).Invoke (paint, encoding);
		#endif

		#endregion

		#region sk_manageddrawable.h

		// sk_manageddrawable_t* sk_manageddrawable_new(void* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_manageddrawable_t sk_manageddrawable_new (void* context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_manageddrawable_t sk_manageddrawable_new (void* context);
		}
		private static Delegates.sk_manageddrawable_new sk_manageddrawable_new_delegate;
		internal static sk_manageddrawable_t sk_manageddrawable_new (void* context) =>
			(sk_manageddrawable_new_delegate ??= GetSymbol<Delegates.sk_manageddrawable_new> ("sk_manageddrawable_new")).Invoke (context);
		#endif

		// void sk_manageddrawable_set_procs(sk_manageddrawable_procs_t procs)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_manageddrawable_set_procs (SKManagedDrawableDelegates procs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_manageddrawable_set_procs (SKManagedDrawableDelegates procs);
		}
		private static Delegates.sk_manageddrawable_set_procs sk_manageddrawable_set_procs_delegate;
		internal static void sk_manageddrawable_set_procs (SKManagedDrawableDelegates procs) =>
			(sk_manageddrawable_set_procs_delegate ??= GetSymbol<Delegates.sk_manageddrawable_set_procs> ("sk_manageddrawable_set_procs")).Invoke (procs);
		#endif

		// void sk_manageddrawable_unref(sk_manageddrawable_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_manageddrawable_unref (sk_manageddrawable_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_manageddrawable_unref (sk_manageddrawable_t param0);
		}
		private static Delegates.sk_manageddrawable_unref sk_manageddrawable_unref_delegate;
		internal static void sk_manageddrawable_unref (sk_manageddrawable_t param0) =>
			(sk_manageddrawable_unref_delegate ??= GetSymbol<Delegates.sk_manageddrawable_unref> ("sk_manageddrawable_unref")).Invoke (param0);
		#endif

		#endregion

		#region sk_managedstream.h

		// void sk_managedstream_destroy(sk_stream_managedstream_t* s)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedstream_destroy (sk_stream_managedstream_t s);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_managedstream_destroy (sk_stream_managedstream_t s);
		}
		private static Delegates.sk_managedstream_destroy sk_managedstream_destroy_delegate;
		internal static void sk_managedstream_destroy (sk_stream_managedstream_t s) =>
			(sk_managedstream_destroy_delegate ??= GetSymbol<Delegates.sk_managedstream_destroy> ("sk_managedstream_destroy")).Invoke (s);
		#endif

		// sk_stream_managedstream_t* sk_managedstream_new(void* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_managedstream_t sk_managedstream_new (void* context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_managedstream_t sk_managedstream_new (void* context);
		}
		private static Delegates.sk_managedstream_new sk_managedstream_new_delegate;
		internal static sk_stream_managedstream_t sk_managedstream_new (void* context) =>
			(sk_managedstream_new_delegate ??= GetSymbol<Delegates.sk_managedstream_new> ("sk_managedstream_new")).Invoke (context);
		#endif

		// void sk_managedstream_set_procs(sk_managedstream_procs_t procs)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedstream_set_procs (SKManagedStreamDelegates procs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_managedstream_set_procs (SKManagedStreamDelegates procs);
		}
		private static Delegates.sk_managedstream_set_procs sk_managedstream_set_procs_delegate;
		internal static void sk_managedstream_set_procs (SKManagedStreamDelegates procs) =>
			(sk_managedstream_set_procs_delegate ??= GetSymbol<Delegates.sk_managedstream_set_procs> ("sk_managedstream_set_procs")).Invoke (procs);
		#endif

		// void sk_managedwstream_destroy(sk_wstream_managedstream_t* s)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedwstream_destroy (sk_wstream_managedstream_t s);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_managedwstream_destroy (sk_wstream_managedstream_t s);
		}
		private static Delegates.sk_managedwstream_destroy sk_managedwstream_destroy_delegate;
		internal static void sk_managedwstream_destroy (sk_wstream_managedstream_t s) =>
			(sk_managedwstream_destroy_delegate ??= GetSymbol<Delegates.sk_managedwstream_destroy> ("sk_managedwstream_destroy")).Invoke (s);
		#endif

		// sk_wstream_managedstream_t* sk_managedwstream_new(void* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_wstream_managedstream_t sk_managedwstream_new (void* context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_wstream_managedstream_t sk_managedwstream_new (void* context);
		}
		private static Delegates.sk_managedwstream_new sk_managedwstream_new_delegate;
		internal static sk_wstream_managedstream_t sk_managedwstream_new (void* context) =>
			(sk_managedwstream_new_delegate ??= GetSymbol<Delegates.sk_managedwstream_new> ("sk_managedwstream_new")).Invoke (context);
		#endif

		// void sk_managedwstream_set_procs(sk_managedwstream_procs_t procs)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedwstream_set_procs (SKManagedWStreamDelegates procs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_managedwstream_set_procs (SKManagedWStreamDelegates procs);
		}
		private static Delegates.sk_managedwstream_set_procs sk_managedwstream_set_procs_delegate;
		internal static void sk_managedwstream_set_procs (SKManagedWStreamDelegates procs) =>
			(sk_managedwstream_set_procs_delegate ??= GetSymbol<Delegates.sk_managedwstream_set_procs> ("sk_managedwstream_set_procs")).Invoke (procs);
		#endif

		#endregion

		#region sk_managedtracememorydump.h

		// void sk_managedtracememorydump_delete(sk_managedtracememorydump_t*)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedtracememorydump_delete (sk_managedtracememorydump_t param0);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_managedtracememorydump_delete (sk_managedtracememorydump_t param0);
		}
		private static Delegates.sk_managedtracememorydump_delete sk_managedtracememorydump_delete_delegate;
		internal static void sk_managedtracememorydump_delete (sk_managedtracememorydump_t param0) =>
			(sk_managedtracememorydump_delete_delegate ??= GetSymbol<Delegates.sk_managedtracememorydump_delete> ("sk_managedtracememorydump_delete")).Invoke (param0);
		#endif

		// sk_managedtracememorydump_t* sk_managedtracememorydump_new(bool detailed, bool dumpWrapped, void* context)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_managedtracememorydump_t sk_managedtracememorydump_new ([MarshalAs (UnmanagedType.I1)] bool detailed, [MarshalAs (UnmanagedType.I1)] bool dumpWrapped, void* context);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_managedtracememorydump_t sk_managedtracememorydump_new ([MarshalAs (UnmanagedType.I1)] bool detailed, [MarshalAs (UnmanagedType.I1)] bool dumpWrapped, void* context);
		}
		private static Delegates.sk_managedtracememorydump_new sk_managedtracememorydump_new_delegate;
		internal static sk_managedtracememorydump_t sk_managedtracememorydump_new ([MarshalAs (UnmanagedType.I1)] bool detailed, [MarshalAs (UnmanagedType.I1)] bool dumpWrapped, void* context) =>
			(sk_managedtracememorydump_new_delegate ??= GetSymbol<Delegates.sk_managedtracememorydump_new> ("sk_managedtracememorydump_new")).Invoke (detailed, dumpWrapped, context);
		#endif

		// void sk_managedtracememorydump_set_procs(sk_managedtracememorydump_procs_t procs)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_managedtracememorydump_set_procs (SKManagedTraceMemoryDumpDelegates procs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_managedtracememorydump_set_procs (SKManagedTraceMemoryDumpDelegates procs);
		}
		private static Delegates.sk_managedtracememorydump_set_procs sk_managedtracememorydump_set_procs_delegate;
		internal static void sk_managedtracememorydump_set_procs (SKManagedTraceMemoryDumpDelegates procs) =>
			(sk_managedtracememorydump_set_procs_delegate ??= GetSymbol<Delegates.sk_managedtracememorydump_set_procs> ("sk_managedtracememorydump_set_procs")).Invoke (procs);
		#endif

		#endregion

	}
}

#endregion Functions

#region Delegates

namespace SkiaSharp {
	// typedef void (*)()* gr_gl_func_ptr
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void GRGlFuncPtr();

	// typedef gr_gl_func_ptr (*)(void* ctx, const char* name)* gr_gl_get_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate IntPtr GRGlGetProcProxyDelegate(void* ctx, /* char */ void* name);

	// typedef void (*)()* gr_vk_func_ptr
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void GRVkFuncPtr();

	// typedef gr_vk_func_ptr (*)(void* ctx, const char* name, vk_instance_t* instance, vk_device_t* device)* gr_vk_get_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate IntPtr GRVkGetProcProxyDelegate(void* ctx, /* char */ void* name, vk_instance_t instance, vk_device_t device);

	// typedef void (*)(void* addr, void* context)* sk_bitmap_release_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKBitmapReleaseProxyDelegate(void* addr, void* context);

	// typedef void (*)(const void* ptr, void* context)* sk_data_release_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKDataReleaseProxyDelegate(void* ptr, void* context);

	// typedef void (*)(const sk_path_t* pathOrNull, const sk_matrix_t* matrix, void* context)* sk_glyph_path_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKGlyphPathProxyDelegate(sk_path_t pathOrNull, SKMatrix* matrix, void* context);

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

	// typedef void (*)(sk_managedtracememorydump_t* d, void* context, const char* dumpName, const char* valueName, const char* units, uint64_t value)* sk_managedtraceMemoryDump_dumpNumericValue_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKManagedTraceMemoryDumpDumpNumericValueProxyDelegate(sk_managedtracememorydump_t d, void* context, /* char */ void* dumpName, /* char */ void* valueName, /* char */ void* units, UInt64 value);

	// typedef void (*)(sk_managedtracememorydump_t* d, void* context, const char* dumpName, const char* valueName, const char* value)* sk_managedtraceMemoryDump_dumpStringValue_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKManagedTraceMemoryDumpDumpStringValueProxyDelegate(sk_managedtracememorydump_t d, void* context, /* char */ void* dumpName, /* char */ void* valueName, /* char */ void* value);

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

}

#endregion

#region Structs

namespace SkiaSharp {

	// gr_context_options_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct GRContextOptionsNative : IEquatable<GRContextOptionsNative> {
		// public bool fAvoidStencilBuffers
		public Byte fAvoidStencilBuffers;

		// public int fRuntimeProgramCacheSize
		public Int32 fRuntimeProgramCacheSize;

		// public size_t fGlyphCacheTextureMaximumBytes
		public /* size_t */ IntPtr fGlyphCacheTextureMaximumBytes;

		// public bool fAllowPathMaskCaching
		public Byte fAllowPathMaskCaching;

		// public bool fDoManualMipmapping
		public Byte fDoManualMipmapping;

		// public int fBufferMapThreshold
		public Int32 fBufferMapThreshold;

		public readonly bool Equals (GRContextOptionsNative obj) =>
			fAvoidStencilBuffers == obj.fAvoidStencilBuffers && fRuntimeProgramCacheSize == obj.fRuntimeProgramCacheSize && fGlyphCacheTextureMaximumBytes == obj.fGlyphCacheTextureMaximumBytes && fAllowPathMaskCaching == obj.fAllowPathMaskCaching && fDoManualMipmapping == obj.fDoManualMipmapping && fBufferMapThreshold == obj.fBufferMapThreshold;

		public readonly override bool Equals (object obj) =>
			obj is GRContextOptionsNative f && Equals (f);

		public static bool operator == (GRContextOptionsNative left, GRContextOptionsNative right) =>
			left.Equals (right);

		public static bool operator != (GRContextOptionsNative left, GRContextOptionsNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fAvoidStencilBuffers);
			hash.Add (fRuntimeProgramCacheSize);
			hash.Add (fGlyphCacheTextureMaximumBytes);
			hash.Add (fAllowPathMaskCaching);
			hash.Add (fDoManualMipmapping);
			hash.Add (fBufferMapThreshold);
			return hash.ToHashCode ();
		}

	}

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

	// gr_mtl_textureinfo_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct GRMtlTextureInfoNative : IEquatable<GRMtlTextureInfoNative> {
		// public const void* fTexture
		public void* fTexture;

		public readonly bool Equals (GRMtlTextureInfoNative obj) =>
			fTexture == obj.fTexture;

		public readonly override bool Equals (object obj) =>
			obj is GRMtlTextureInfoNative f && Equals (f);

		public static bool operator == (GRMtlTextureInfoNative left, GRMtlTextureInfoNative right) =>
			left.Equals (right);

		public static bool operator != (GRMtlTextureInfoNative left, GRMtlTextureInfoNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fTexture);
			return hash.ToHashCode ();
		}

	}

	// gr_vk_alloc_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GRVkAlloc : IEquatable<GRVkAlloc> {
		// public uint64_t fMemory
		private UInt64 fMemory;
		public UInt64 Memory {
			readonly get => fMemory;
			set => fMemory = value;
		}

		// public uint64_t fOffset
		private UInt64 fOffset;
		public UInt64 Offset {
			readonly get => fOffset;
			set => fOffset = value;
		}

		// public uint64_t fSize
		private UInt64 fSize;
		public UInt64 Size {
			readonly get => fSize;
			set => fSize = value;
		}

		// public uint32_t fFlags
		private UInt32 fFlags;
		public UInt32 Flags {
			readonly get => fFlags;
			set => fFlags = value;
		}

		// public gr_vk_backendmemory_t fBackendMemory
		private IntPtr fBackendMemory;
		public IntPtr BackendMemory {
			readonly get => fBackendMemory;
			set => fBackendMemory = value;
		}

		// public bool _private_fUsesSystemHeap
		private Byte fUsesSystemHeap;

		public readonly bool Equals (GRVkAlloc obj) =>
			fMemory == obj.fMemory && fOffset == obj.fOffset && fSize == obj.fSize && fFlags == obj.fFlags && fBackendMemory == obj.fBackendMemory && fUsesSystemHeap == obj.fUsesSystemHeap;

		public readonly override bool Equals (object obj) =>
			obj is GRVkAlloc f && Equals (f);

		public static bool operator == (GRVkAlloc left, GRVkAlloc right) =>
			left.Equals (right);

		public static bool operator != (GRVkAlloc left, GRVkAlloc right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fMemory);
			hash.Add (fOffset);
			hash.Add (fSize);
			hash.Add (fFlags);
			hash.Add (fBackendMemory);
			hash.Add (fUsesSystemHeap);
			return hash.ToHashCode ();
		}

	}

	// gr_vk_backendcontext_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct GRVkBackendContextNative : IEquatable<GRVkBackendContextNative> {
		// public vk_instance_t* fInstance
		public vk_instance_t fInstance;

		// public vk_physical_device_t* fPhysicalDevice
		public vk_physical_device_t fPhysicalDevice;

		// public vk_device_t* fDevice
		public vk_device_t fDevice;

		// public vk_queue_t* fQueue
		public vk_queue_t fQueue;

		// public uint32_t fGraphicsQueueIndex
		public UInt32 fGraphicsQueueIndex;

		// public uint32_t fMinAPIVersion
		public UInt32 fMinAPIVersion;

		// public uint32_t fInstanceVersion
		public UInt32 fInstanceVersion;

		// public uint32_t fMaxAPIVersion
		public UInt32 fMaxAPIVersion;

		// public uint32_t fExtensions
		public UInt32 fExtensions;

		// public const gr_vk_extensions_t* fVkExtensions
		public gr_vk_extensions_t fVkExtensions;

		// public uint32_t fFeatures
		public UInt32 fFeatures;

		// public const vk_physical_device_features_t* fDeviceFeatures
		public vk_physical_device_features_t fDeviceFeatures;

		// public const vk_physical_device_features_2_t* fDeviceFeatures2
		public vk_physical_device_features_2_t fDeviceFeatures2;

		// public gr_vk_memory_allocator_t* fMemoryAllocator
		public gr_vk_memory_allocator_t fMemoryAllocator;

		// public gr_vk_get_proc fGetProc
		public GRVkGetProcProxyDelegate fGetProc;

		// public void* fGetProcUserData
		public void* fGetProcUserData;

		// public bool fOwnsInstanceAndDevice
		public Byte fOwnsInstanceAndDevice;

		// public bool fProtectedContext
		public Byte fProtectedContext;

		public readonly bool Equals (GRVkBackendContextNative obj) =>
			fInstance == obj.fInstance && fPhysicalDevice == obj.fPhysicalDevice && fDevice == obj.fDevice && fQueue == obj.fQueue && fGraphicsQueueIndex == obj.fGraphicsQueueIndex && fMinAPIVersion == obj.fMinAPIVersion && fInstanceVersion == obj.fInstanceVersion && fMaxAPIVersion == obj.fMaxAPIVersion && fExtensions == obj.fExtensions && fVkExtensions == obj.fVkExtensions && fFeatures == obj.fFeatures && fDeviceFeatures == obj.fDeviceFeatures && fDeviceFeatures2 == obj.fDeviceFeatures2 && fMemoryAllocator == obj.fMemoryAllocator && fGetProc == obj.fGetProc && fGetProcUserData == obj.fGetProcUserData && fOwnsInstanceAndDevice == obj.fOwnsInstanceAndDevice && fProtectedContext == obj.fProtectedContext;

		public readonly override bool Equals (object obj) =>
			obj is GRVkBackendContextNative f && Equals (f);

		public static bool operator == (GRVkBackendContextNative left, GRVkBackendContextNative right) =>
			left.Equals (right);

		public static bool operator != (GRVkBackendContextNative left, GRVkBackendContextNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fInstance);
			hash.Add (fPhysicalDevice);
			hash.Add (fDevice);
			hash.Add (fQueue);
			hash.Add (fGraphicsQueueIndex);
			hash.Add (fMinAPIVersion);
			hash.Add (fInstanceVersion);
			hash.Add (fMaxAPIVersion);
			hash.Add (fExtensions);
			hash.Add (fVkExtensions);
			hash.Add (fFeatures);
			hash.Add (fDeviceFeatures);
			hash.Add (fDeviceFeatures2);
			hash.Add (fMemoryAllocator);
			hash.Add (fGetProc);
			hash.Add (fGetProcUserData);
			hash.Add (fOwnsInstanceAndDevice);
			hash.Add (fProtectedContext);
			return hash.ToHashCode ();
		}

	}

	// gr_vk_imageinfo_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GRVkImageInfo : IEquatable<GRVkImageInfo> {
		// public uint64_t fImage
		private UInt64 fImage;
		public UInt64 Image {
			readonly get => fImage;
			set => fImage = value;
		}

		// public gr_vk_alloc_t fAlloc
		private GRVkAlloc fAlloc;
		public GRVkAlloc Alloc {
			readonly get => fAlloc;
			set => fAlloc = value;
		}

		// public uint32_t fImageTiling
		private UInt32 fImageTiling;
		public UInt32 ImageTiling {
			readonly get => fImageTiling;
			set => fImageTiling = value;
		}

		// public uint32_t fImageLayout
		private UInt32 fImageLayout;
		public UInt32 ImageLayout {
			readonly get => fImageLayout;
			set => fImageLayout = value;
		}

		// public uint32_t fFormat
		private UInt32 fFormat;
		public UInt32 Format {
			readonly get => fFormat;
			set => fFormat = value;
		}

		// public uint32_t fImageUsageFlags
		private UInt32 fImageUsageFlags;
		public UInt32 ImageUsageFlags {
			readonly get => fImageUsageFlags;
			set => fImageUsageFlags = value;
		}

		// public uint32_t fSampleCount
		private UInt32 fSampleCount;
		public UInt32 SampleCount {
			readonly get => fSampleCount;
			set => fSampleCount = value;
		}

		// public uint32_t fLevelCount
		private UInt32 fLevelCount;
		public UInt32 LevelCount {
			readonly get => fLevelCount;
			set => fLevelCount = value;
		}

		// public uint32_t fCurrentQueueFamily
		private UInt32 fCurrentQueueFamily;
		public UInt32 CurrentQueueFamily {
			readonly get => fCurrentQueueFamily;
			set => fCurrentQueueFamily = value;
		}

		// public bool fProtected
		private Byte fProtected;
		public bool Protected {
			readonly get => fProtected > 0;
			set => fProtected = value ? (byte)1 : (byte)0;
		}

		// public gr_vk_ycbcrconversioninfo_t fYcbcrConversionInfo
		private GrVkYcbcrConversionInfo fYcbcrConversionInfo;
		public GrVkYcbcrConversionInfo YcbcrConversionInfo {
			readonly get => fYcbcrConversionInfo;
			set => fYcbcrConversionInfo = value;
		}

		// public uint32_t fSharingMode
		private UInt32 fSharingMode;
		public UInt32 SharingMode {
			readonly get => fSharingMode;
			set => fSharingMode = value;
		}

		public readonly bool Equals (GRVkImageInfo obj) =>
			fImage == obj.fImage && fAlloc == obj.fAlloc && fImageTiling == obj.fImageTiling && fImageLayout == obj.fImageLayout && fFormat == obj.fFormat && fImageUsageFlags == obj.fImageUsageFlags && fSampleCount == obj.fSampleCount && fLevelCount == obj.fLevelCount && fCurrentQueueFamily == obj.fCurrentQueueFamily && fProtected == obj.fProtected && fYcbcrConversionInfo == obj.fYcbcrConversionInfo && fSharingMode == obj.fSharingMode;

		public readonly override bool Equals (object obj) =>
			obj is GRVkImageInfo f && Equals (f);

		public static bool operator == (GRVkImageInfo left, GRVkImageInfo right) =>
			left.Equals (right);

		public static bool operator != (GRVkImageInfo left, GRVkImageInfo right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fImage);
			hash.Add (fAlloc);
			hash.Add (fImageTiling);
			hash.Add (fImageLayout);
			hash.Add (fFormat);
			hash.Add (fImageUsageFlags);
			hash.Add (fSampleCount);
			hash.Add (fLevelCount);
			hash.Add (fCurrentQueueFamily);
			hash.Add (fProtected);
			hash.Add (fYcbcrConversionInfo);
			hash.Add (fSharingMode);
			return hash.ToHashCode ();
		}

	}

	// gr_vk_ycbcrconversioninfo_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GrVkYcbcrConversionInfo : IEquatable<GrVkYcbcrConversionInfo> {
		// public uint32_t fFormat
		private UInt32 fFormat;
		public UInt32 Format {
			readonly get => fFormat;
			set => fFormat = value;
		}

		// public uint64_t fExternalFormat
		private UInt64 fExternalFormat;
		public UInt64 ExternalFormat {
			readonly get => fExternalFormat;
			set => fExternalFormat = value;
		}

		// public uint32_t fYcbcrModel
		private UInt32 fYcbcrModel;
		public UInt32 YcbcrModel {
			readonly get => fYcbcrModel;
			set => fYcbcrModel = value;
		}

		// public uint32_t fYcbcrRange
		private UInt32 fYcbcrRange;
		public UInt32 YcbcrRange {
			readonly get => fYcbcrRange;
			set => fYcbcrRange = value;
		}

		// public uint32_t fXChromaOffset
		private UInt32 fXChromaOffset;
		public UInt32 XChromaOffset {
			readonly get => fXChromaOffset;
			set => fXChromaOffset = value;
		}

		// public uint32_t fYChromaOffset
		private UInt32 fYChromaOffset;
		public UInt32 YChromaOffset {
			readonly get => fYChromaOffset;
			set => fYChromaOffset = value;
		}

		// public uint32_t fChromaFilter
		private UInt32 fChromaFilter;
		public UInt32 ChromaFilter {
			readonly get => fChromaFilter;
			set => fChromaFilter = value;
		}

		// public uint32_t fForceExplicitReconstruction
		private UInt32 fForceExplicitReconstruction;
		public UInt32 ForceExplicitReconstruction {
			readonly get => fForceExplicitReconstruction;
			set => fForceExplicitReconstruction = value;
		}

		// public uint32_t fFormatFeatures
		private UInt32 fFormatFeatures;
		public UInt32 FormatFeatures {
			readonly get => fFormatFeatures;
			set => fFormatFeatures = value;
		}

		public readonly bool Equals (GrVkYcbcrConversionInfo obj) =>
			fFormat == obj.fFormat && fExternalFormat == obj.fExternalFormat && fYcbcrModel == obj.fYcbcrModel && fYcbcrRange == obj.fYcbcrRange && fXChromaOffset == obj.fXChromaOffset && fYChromaOffset == obj.fYChromaOffset && fChromaFilter == obj.fChromaFilter && fForceExplicitReconstruction == obj.fForceExplicitReconstruction && fFormatFeatures == obj.fFormatFeatures;

		public readonly override bool Equals (object obj) =>
			obj is GrVkYcbcrConversionInfo f && Equals (f);

		public static bool operator == (GrVkYcbcrConversionInfo left, GrVkYcbcrConversionInfo right) =>
			left.Equals (right);

		public static bool operator != (GrVkYcbcrConversionInfo left, GrVkYcbcrConversionInfo right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fFormat);
			hash.Add (fExternalFormat);
			hash.Add (fYcbcrModel);
			hash.Add (fYcbcrRange);
			hash.Add (fXChromaOffset);
			hash.Add (fYChromaOffset);
			hash.Add (fChromaFilter);
			hash.Add (fForceExplicitReconstruction);
			hash.Add (fFormatFeatures);
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

		public readonly bool Equals (SKCodecOptionsInternal obj) =>
			fZeroInitialized == obj.fZeroInitialized && fSubset == obj.fSubset && fFrameIndex == obj.fFrameIndex && fPriorFrame == obj.fPriorFrame;

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

	// sk_colorspace_primaries_t
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

	// sk_colorspace_xyz_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKColorSpaceXyz : IEquatable<SKColorSpaceXyz> {
		// public float fM00
		private Single fM00;

		// public float fM01
		private Single fM01;

		// public float fM02
		private Single fM02;

		// public float fM10
		private Single fM10;

		// public float fM11
		private Single fM11;

		// public float fM12
		private Single fM12;

		// public float fM20
		private Single fM20;

		// public float fM21
		private Single fM21;

		// public float fM22
		private Single fM22;

		public readonly bool Equals (SKColorSpaceXyz obj) =>
			fM00 == obj.fM00 && fM01 == obj.fM01 && fM02 == obj.fM02 && fM10 == obj.fM10 && fM11 == obj.fM11 && fM12 == obj.fM12 && fM20 == obj.fM20 && fM21 == obj.fM21 && fM22 == obj.fM22;

		public readonly override bool Equals (object obj) =>
			obj is SKColorSpaceXyz f && Equals (f);

		public static bool operator == (SKColorSpaceXyz left, SKColorSpaceXyz right) =>
			left.Equals (right);

		public static bool operator != (SKColorSpaceXyz left, SKColorSpaceXyz right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fM00);
			hash.Add (fM01);
			hash.Add (fM02);
			hash.Add (fM10);
			hash.Add (fM11);
			hash.Add (fM12);
			hash.Add (fM20);
			hash.Add (fM21);
			hash.Add (fM22);
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
		public SKColorTypeNative colorType;

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

		public readonly bool Equals (SKJpegEncoderOptions obj) =>
			fQuality == obj.fQuality && fDownsample == obj.fDownsample && fAlphaOption == obj.fAlphaOption;

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

	// sk_managedtracememorydump_procs_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKManagedTraceMemoryDumpDelegates : IEquatable<SKManagedTraceMemoryDumpDelegates> {
		// public sk_managedtraceMemoryDump_dumpNumericValue_proc fDumpNumericValue
		public SKManagedTraceMemoryDumpDumpNumericValueProxyDelegate fDumpNumericValue;

		// public sk_managedtraceMemoryDump_dumpStringValue_proc fDumpStringValue
		public SKManagedTraceMemoryDumpDumpStringValueProxyDelegate fDumpStringValue;

		public readonly bool Equals (SKManagedTraceMemoryDumpDelegates obj) =>
			fDumpNumericValue == obj.fDumpNumericValue && fDumpStringValue == obj.fDumpStringValue;

		public readonly override bool Equals (object obj) =>
			obj is SKManagedTraceMemoryDumpDelegates f && Equals (f);

		public static bool operator == (SKManagedTraceMemoryDumpDelegates left, SKManagedTraceMemoryDumpDelegates right) =>
			left.Equals (right);

		public static bool operator != (SKManagedTraceMemoryDumpDelegates left, SKManagedTraceMemoryDumpDelegates right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fDumpNumericValue);
			hash.Add (fDumpStringValue);
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

		// public void* fComments
		private void* fComments;

		public readonly bool Equals (SKPngEncoderOptions obj) =>
			fFilterFlags == obj.fFilterFlags && fZLibLevel == obj.fZLibLevel && fComments == obj.fComments;

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

		public readonly bool Equals (SKWebpEncoderOptions obj) =>
			fCompression == obj.fCompression && fQuality == obj.fQuality;

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
			return hash.ToHashCode ();
		}

	}
}

#endregion

#region Enums

namespace SkiaSharp {

	// gr_backend_t
	internal enum GRBackendNative {
		// OPENGL_GR_BACKEND = 0
		OpenGL = 0,
		// VULKAN_GR_BACKEND = 1
		Vulkan = 1,
		// METAL_GR_BACKEND = 2
		Metal = 2,
		// DIRECT3D_GR_BACKEND = 3
		Direct3D = 3,
		// DAWN_GR_BACKEND = 4
		Dawn = 4,
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

	// sk_color_channel_t
	public enum SKColorChannel {
		// R_SK_COLOR_CHANNEL = 0
		R = 0,
		// G_SK_COLOR_CHANNEL = 1
		G = 1,
		// B_SK_COLOR_CHANNEL = 2
		B = 2,
		// A_SK_COLOR_CHANNEL = 3
		A = 3,
	}

	// sk_colortype_t
	internal enum SKColorTypeNative {
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
		// BGRA_1010102_SK_COLORTYPE = 8
		Bgra1010102 = 8,
		// RGB_101010X_SK_COLORTYPE = 9
		Rgb101010x = 9,
		// BGR_101010X_SK_COLORTYPE = 10
		Bgr101010x = 10,
		// GRAY_8_SK_COLORTYPE = 11
		Gray8 = 11,
		// RGBA_F16_NORM_SK_COLORTYPE = 12
		RgbaF16Norm = 12,
		// RGBA_F16_SK_COLORTYPE = 13
		RgbaF16 = 13,
		// RGBA_F32_SK_COLORTYPE = 14
		RgbaF32 = 14,
		// R8G8_UNORM_SK_COLORTYPE = 15
		R8g8Unorm = 15,
		// A16_FLOAT_SK_COLORTYPE = 16
		A16Float = 16,
		// R16G16_FLOAT_SK_COLORTYPE = 17
		R16g16Float = 17,
		// A16_UNORM_SK_COLORTYPE = 18
		A16Unorm = 18,
		// R16G16_UNORM_SK_COLORTYPE = 19
		R16g16Unorm = 19,
		// R16G16B16A16_UNORM_SK_COLORTYPE = 20
		R16g16b16a16Unorm = 20,
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

	// sk_font_edging_t
	public enum SKFontEdging {
		// ALIAS_SK_FONT_EDGING = 0
		Alias = 0,
		// ANTIALIAS_SK_FONT_EDGING = 1
		Antialias = 1,
		// SUBPIXEL_ANTIALIAS_SK_FONT_EDGING = 2
		SubpixelAntialias = 2,
	}

	// sk_font_hinting_t
	public enum SKFontHinting {
		// NONE_SK_FONT_HINTING = 0
		None = 0,
		// SLIGHT_SK_FONT_HINTING = 1
		Slight = 1,
		// NORMAL_SK_FONT_HINTING = 2
		Normal = 2,
		// FULL_SK_FONT_HINTING = 3
		Full = 3,
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
		// SDF_SK_MASK_FORMAT = 5
		Sdf = 5,
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
		// DECAL_SK_SHADER_TILEMODE = 3
		Decal = 3,
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
}

#endregion

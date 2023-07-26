using System;
using System.Runtime.InteropServices;

#region Namespaces

using SkiaSharp.SceneGraph;

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
	internal unsafe partial class SceneGraphApi
	{
		#region sksg_invalidation_controller.h

		// void sksg_invalidation_controller_begin(sksg_invalidation_controller_t* instance)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sksg_invalidation_controller_begin (sksg_invalidation_controller_t instance);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sksg_invalidation_controller_begin (sksg_invalidation_controller_t instance);
		}
		private static Delegates.sksg_invalidation_controller_begin sksg_invalidation_controller_begin_delegate;
		internal static void sksg_invalidation_controller_begin (sksg_invalidation_controller_t instance) =>
			(sksg_invalidation_controller_begin_delegate ??= GetSymbol<Delegates.sksg_invalidation_controller_begin> ("sksg_invalidation_controller_begin")).Invoke (instance);
		#endif

		// void sksg_invalidation_controller_delete(sksg_invalidation_controller_t* instance)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sksg_invalidation_controller_delete (sksg_invalidation_controller_t instance);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sksg_invalidation_controller_delete (sksg_invalidation_controller_t instance);
		}
		private static Delegates.sksg_invalidation_controller_delete sksg_invalidation_controller_delete_delegate;
		internal static void sksg_invalidation_controller_delete (sksg_invalidation_controller_t instance) =>
			(sksg_invalidation_controller_delete_delegate ??= GetSymbol<Delegates.sksg_invalidation_controller_delete> ("sksg_invalidation_controller_delete")).Invoke (instance);
		#endif

		// void sksg_invalidation_controller_end(sksg_invalidation_controller_t* instance)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sksg_invalidation_controller_end (sksg_invalidation_controller_t instance);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sksg_invalidation_controller_end (sksg_invalidation_controller_t instance);
		}
		private static Delegates.sksg_invalidation_controller_end sksg_invalidation_controller_end_delegate;
		internal static void sksg_invalidation_controller_end (sksg_invalidation_controller_t instance) =>
			(sksg_invalidation_controller_end_delegate ??= GetSymbol<Delegates.sksg_invalidation_controller_end> ("sksg_invalidation_controller_end")).Invoke (instance);
		#endif

		// void sksg_invalidation_controller_get_bounds(sksg_invalidation_controller_t* instance, sk_rect_t* bounds)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sksg_invalidation_controller_get_bounds (sksg_invalidation_controller_t instance, SKRect* bounds);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sksg_invalidation_controller_get_bounds (sksg_invalidation_controller_t instance, SKRect* bounds);
		}
		private static Delegates.sksg_invalidation_controller_get_bounds sksg_invalidation_controller_get_bounds_delegate;
		internal static void sksg_invalidation_controller_get_bounds (sksg_invalidation_controller_t instance, SKRect* bounds) =>
			(sksg_invalidation_controller_get_bounds_delegate ??= GetSymbol<Delegates.sksg_invalidation_controller_get_bounds> ("sksg_invalidation_controller_get_bounds")).Invoke (instance, bounds);
		#endif

		// void sksg_invalidation_controller_inval(sksg_invalidation_controller_t* instance, sk_rect_t* rect, sk_matrix_t* matrix)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sksg_invalidation_controller_inval (sksg_invalidation_controller_t instance, SKRect* rect, SKMatrix* matrix);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sksg_invalidation_controller_inval (sksg_invalidation_controller_t instance, SKRect* rect, SKMatrix* matrix);
		}
		private static Delegates.sksg_invalidation_controller_inval sksg_invalidation_controller_inval_delegate;
		internal static void sksg_invalidation_controller_inval (sksg_invalidation_controller_t instance, SKRect* rect, SKMatrix* matrix) =>
			(sksg_invalidation_controller_inval_delegate ??= GetSymbol<Delegates.sksg_invalidation_controller_inval> ("sksg_invalidation_controller_inval")).Invoke (instance, rect, matrix);
		#endif

		// sksg_invalidation_controller_t* sksg_invalidation_controller_new()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sksg_invalidation_controller_t sksg_invalidation_controller_new ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sksg_invalidation_controller_t sksg_invalidation_controller_new ();
		}
		private static Delegates.sksg_invalidation_controller_new sksg_invalidation_controller_new_delegate;
		internal static sksg_invalidation_controller_t sksg_invalidation_controller_new () =>
			(sksg_invalidation_controller_new_delegate ??= GetSymbol<Delegates.sksg_invalidation_controller_new> ("sksg_invalidation_controller_new")).Invoke ();
		#endif

		// void sksg_invalidation_controller_reset(sksg_invalidation_controller_t* instance)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sksg_invalidation_controller_reset (sksg_invalidation_controller_t instance);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sksg_invalidation_controller_reset (sksg_invalidation_controller_t instance);
		}
		private static Delegates.sksg_invalidation_controller_reset sksg_invalidation_controller_reset_delegate;
		internal static void sksg_invalidation_controller_reset (sksg_invalidation_controller_t instance) =>
			(sksg_invalidation_controller_reset_delegate ??= GetSymbol<Delegates.sksg_invalidation_controller_reset> ("sksg_invalidation_controller_reset")).Invoke (instance);
		#endif

		#endregion

	}
}

#endregion Functions

#region Delegates

#endregion

#region Structs

#endregion

#region Enums

#endregion

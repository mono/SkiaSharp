using System;
using System.Runtime.InteropServices;

#region Namespaces

using SkiaSharp.Skottie;

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
	internal unsafe partial class SkottieApi
	{
		#region skottie_animation.h

		// void skottie_animation_delete(skottie_animation_t* instance)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_delete (skottie_animation_t instance);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_delete (skottie_animation_t instance);
		}
		private static Delegates.skottie_animation_delete skottie_animation_delete_delegate;
		internal static void skottie_animation_delete (skottie_animation_t instance) =>
			(skottie_animation_delete_delegate ??= GetSymbol<Delegates.skottie_animation_delete> ("skottie_animation_delete")).Invoke (instance);
		#endif

		// double skottie_animation_get_duration(skottie_animation_t* instance)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Double skottie_animation_get_duration (skottie_animation_t instance);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Double skottie_animation_get_duration (skottie_animation_t instance);
		}
		private static Delegates.skottie_animation_get_duration skottie_animation_get_duration_delegate;
		internal static Double skottie_animation_get_duration (skottie_animation_t instance) =>
			(skottie_animation_get_duration_delegate ??= GetSymbol<Delegates.skottie_animation_get_duration> ("skottie_animation_get_duration")).Invoke (instance);
		#endif

		// double skottie_animation_get_fps(skottie_animation_t* instance)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Double skottie_animation_get_fps (skottie_animation_t instance);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Double skottie_animation_get_fps (skottie_animation_t instance);
		}
		private static Delegates.skottie_animation_get_fps skottie_animation_get_fps_delegate;
		internal static Double skottie_animation_get_fps (skottie_animation_t instance) =>
			(skottie_animation_get_fps_delegate ??= GetSymbol<Delegates.skottie_animation_get_fps> ("skottie_animation_get_fps")).Invoke (instance);
		#endif

		// double skottie_animation_get_in_point(skottie_animation_t* instance)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Double skottie_animation_get_in_point (skottie_animation_t instance);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Double skottie_animation_get_in_point (skottie_animation_t instance);
		}
		private static Delegates.skottie_animation_get_in_point skottie_animation_get_in_point_delegate;
		internal static Double skottie_animation_get_in_point (skottie_animation_t instance) =>
			(skottie_animation_get_in_point_delegate ??= GetSymbol<Delegates.skottie_animation_get_in_point> ("skottie_animation_get_in_point")).Invoke (instance);
		#endif

		// double skottie_animation_get_out_point(skottie_animation_t* instance)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Double skottie_animation_get_out_point (skottie_animation_t instance);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Double skottie_animation_get_out_point (skottie_animation_t instance);
		}
		private static Delegates.skottie_animation_get_out_point skottie_animation_get_out_point_delegate;
		internal static Double skottie_animation_get_out_point (skottie_animation_t instance) =>
			(skottie_animation_get_out_point_delegate ??= GetSymbol<Delegates.skottie_animation_get_out_point> ("skottie_animation_get_out_point")).Invoke (instance);
		#endif

		// void skottie_animation_get_size(skottie_animation_t* instance, sk_size_t* size)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_get_size (skottie_animation_t instance, SKSize* size);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_get_size (skottie_animation_t instance, SKSize* size);
		}
		private static Delegates.skottie_animation_get_size skottie_animation_get_size_delegate;
		internal static void skottie_animation_get_size (skottie_animation_t instance, SKSize* size) =>
			(skottie_animation_get_size_delegate ??= GetSymbol<Delegates.skottie_animation_get_size> ("skottie_animation_get_size")).Invoke (instance, size);
		#endif

		// void skottie_animation_get_version(skottie_animation_t* instance, sk_string_t* version)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_get_version (skottie_animation_t instance, sk_string_t version);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_get_version (skottie_animation_t instance, sk_string_t version);
		}
		private static Delegates.skottie_animation_get_version skottie_animation_get_version_delegate;
		internal static void skottie_animation_get_version (skottie_animation_t instance, sk_string_t version) =>
			(skottie_animation_get_version_delegate ??= GetSymbol<Delegates.skottie_animation_get_version> ("skottie_animation_get_version")).Invoke (instance, version);
		#endif

		// void skottie_animation_keepalive()
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_keepalive ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_keepalive ();
		}
		private static Delegates.skottie_animation_keepalive skottie_animation_keepalive_delegate;
		internal static void skottie_animation_keepalive () =>
			(skottie_animation_keepalive_delegate ??= GetSymbol<Delegates.skottie_animation_keepalive> ("skottie_animation_keepalive")).Invoke ();
		#endif

		// skottie_animation_t* skottie_animation_make_from_file(const char* path)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_t skottie_animation_make_from_file ([MarshalAs (UnmanagedType.LPStr)] String path);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate skottie_animation_t skottie_animation_make_from_file ([MarshalAs (UnmanagedType.LPStr)] String path);
		}
		private static Delegates.skottie_animation_make_from_file skottie_animation_make_from_file_delegate;
		internal static skottie_animation_t skottie_animation_make_from_file ([MarshalAs (UnmanagedType.LPStr)] String path) =>
			(skottie_animation_make_from_file_delegate ??= GetSymbol<Delegates.skottie_animation_make_from_file> ("skottie_animation_make_from_file")).Invoke (path);
		#endif

		// skottie_animation_t* skottie_animation_make_from_stream(sk_stream_t* stream)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_t skottie_animation_make_from_stream (sk_stream_t stream);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate skottie_animation_t skottie_animation_make_from_stream (sk_stream_t stream);
		}
		private static Delegates.skottie_animation_make_from_stream skottie_animation_make_from_stream_delegate;
		internal static skottie_animation_t skottie_animation_make_from_stream (sk_stream_t stream) =>
			(skottie_animation_make_from_stream_delegate ??= GetSymbol<Delegates.skottie_animation_make_from_stream> ("skottie_animation_make_from_stream")).Invoke (stream);
		#endif

		// skottie_animation_t* skottie_animation_make_from_string(const char* data, size_t length)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_t skottie_animation_make_from_string ([MarshalAs (UnmanagedType.LPStr)] String data, int length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate skottie_animation_t skottie_animation_make_from_string ([MarshalAs (UnmanagedType.LPStr)] String data, int length);
		}
		private static Delegates.skottie_animation_make_from_string skottie_animation_make_from_string_delegate;
		internal static skottie_animation_t skottie_animation_make_from_string ([MarshalAs (UnmanagedType.LPStr)] String data, int length) =>
			(skottie_animation_make_from_string_delegate ??= GetSymbol<Delegates.skottie_animation_make_from_string> ("skottie_animation_make_from_string")).Invoke (data, length);
		#endif

		// void skottie_animation_ref(skottie_animation_t* instance)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_ref (skottie_animation_t instance);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_ref (skottie_animation_t instance);
		}
		private static Delegates.skottie_animation_ref skottie_animation_ref_delegate;
		internal static void skottie_animation_ref (skottie_animation_t instance) =>
			(skottie_animation_ref_delegate ??= GetSymbol<Delegates.skottie_animation_ref> ("skottie_animation_ref")).Invoke (instance);
		#endif

		// void skottie_animation_render(skottie_animation_t* instance, sk_canvas_t* canvas, sk_rect_t* dst)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_render (skottie_animation_t instance, sk_canvas_t canvas, SKRect* dst);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_render (skottie_animation_t instance, sk_canvas_t canvas, SKRect* dst);
		}
		private static Delegates.skottie_animation_render skottie_animation_render_delegate;
		internal static void skottie_animation_render (skottie_animation_t instance, sk_canvas_t canvas, SKRect* dst) =>
			(skottie_animation_render_delegate ??= GetSymbol<Delegates.skottie_animation_render> ("skottie_animation_render")).Invoke (instance, canvas, dst);
		#endif

		// void skottie_animation_render_with_flags(skottie_animation_t* instance, sk_canvas_t* canvas, sk_rect_t* dst, skottie_animation_renderflags_t flags)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_render_with_flags (skottie_animation_t instance, sk_canvas_t canvas, SKRect* dst, AnimationRenderFlags flags);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_render_with_flags (skottie_animation_t instance, sk_canvas_t canvas, SKRect* dst, AnimationRenderFlags flags);
		}
		private static Delegates.skottie_animation_render_with_flags skottie_animation_render_with_flags_delegate;
		internal static void skottie_animation_render_with_flags (skottie_animation_t instance, sk_canvas_t canvas, SKRect* dst, AnimationRenderFlags flags) =>
			(skottie_animation_render_with_flags_delegate ??= GetSymbol<Delegates.skottie_animation_render_with_flags> ("skottie_animation_render_with_flags")).Invoke (instance, canvas, dst, flags);
		#endif

		// void skottie_animation_seek(skottie_animation_t* instance, float t, sksg_invalidation_controller_t* ic)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_seek (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_seek (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic);
		}
		private static Delegates.skottie_animation_seek skottie_animation_seek_delegate;
		internal static void skottie_animation_seek (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic) =>
			(skottie_animation_seek_delegate ??= GetSymbol<Delegates.skottie_animation_seek> ("skottie_animation_seek")).Invoke (instance, t, ic);
		#endif

		// void skottie_animation_seek_frame(skottie_animation_t* instance, float t, sksg_invalidation_controller_t* ic)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_seek_frame (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_seek_frame (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic);
		}
		private static Delegates.skottie_animation_seek_frame skottie_animation_seek_frame_delegate;
		internal static void skottie_animation_seek_frame (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic) =>
			(skottie_animation_seek_frame_delegate ??= GetSymbol<Delegates.skottie_animation_seek_frame> ("skottie_animation_seek_frame")).Invoke (instance, t, ic);
		#endif

		// void skottie_animation_seek_frame_time(skottie_animation_t* instance, float t, sksg_invalidation_controller_t* ic)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_seek_frame_time (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_seek_frame_time (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic);
		}
		private static Delegates.skottie_animation_seek_frame_time skottie_animation_seek_frame_time_delegate;
		internal static void skottie_animation_seek_frame_time (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic) =>
			(skottie_animation_seek_frame_time_delegate ??= GetSymbol<Delegates.skottie_animation_seek_frame_time> ("skottie_animation_seek_frame_time")).Invoke (instance, t, ic);
		#endif

		// void skottie_animation_unref(skottie_animation_t* instance)
		#if !USE_DELEGATES
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_unref (skottie_animation_t instance);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_unref (skottie_animation_t instance);
		}
		private static Delegates.skottie_animation_unref skottie_animation_unref_delegate;
		internal static void skottie_animation_unref (skottie_animation_t instance) =>
			(skottie_animation_unref_delegate ??= GetSymbol<Delegates.skottie_animation_unref> ("skottie_animation_unref")).Invoke (instance);
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

namespace SkiaSharp.Skottie {

	// skottie_animation_renderflags_t
	[Flags]
	public enum AnimationRenderFlags {
		// SKIP_TOP_LEVEL_ISOLATION = 0x01
		SkipTopLevelIsolation = 1,
		// DISABLE_TOP_LEVEL_CLIPPING = 0x02
		DisableTopLevelClipping = 2,
	}
}

#endregion

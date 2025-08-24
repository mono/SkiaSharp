using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

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
using sk_bbh_factory_t = System.IntPtr;
using sk_bitmap_t = System.IntPtr;
using sk_blender_t = System.IntPtr;
using sk_canvas_t = System.IntPtr;
using sk_codec_t = System.IntPtr;
using sk_colorfilter_t = System.IntPtr;
using sk_colorspace_icc_profile_t = System.IntPtr;
using sk_colorspace_t = System.IntPtr;
using sk_compatpaint_t = System.IntPtr;
using sk_data_t = System.IntPtr;
using sk_document_t = System.IntPtr;
using sk_drawable_t = System.IntPtr;
using sk_flattenable_t = System.IntPtr;
using sk_font_t = System.IntPtr;
using sk_fontmgr_t = System.IntPtr;
using sk_fontstyle_t = System.IntPtr;
using sk_fontstyleset_t = System.IntPtr;
using sk_image_t = System.IntPtr;
using sk_imagefilter_t = System.IntPtr;
using sk_manageddrawable_t = System.IntPtr;
using sk_managedtracememorydump_t = System.IntPtr;
using sk_maskfilter_t = System.IntPtr;
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
using sk_rtree_factory_t = System.IntPtr;
using sk_runtimeeffect_t = System.IntPtr;
using sk_runtimeshaderbuilder_t = System.IntPtr;
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
using skottie_animation_builder_t = System.IntPtr;
using skottie_animation_t = System.IntPtr;
using skottie_logger_t = System.IntPtr;
using skottie_marker_observer_t = System.IntPtr;
using skottie_property_observer_t = System.IntPtr;
using skottie_resource_provider_t = System.IntPtr;
using skresources_external_track_asset_t = System.IntPtr;
using skresources_image_asset_t = System.IntPtr;
using skresources_multi_frame_image_asset_t = System.IntPtr;
using skresources_resource_provider_t = System.IntPtr;
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

		// void skottie_animation_builder_delete(skottie_animation_builder_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_builder_delete (skottie_animation_builder_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_builder_delete (skottie_animation_builder_t instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_builder_delete (skottie_animation_builder_t instance);
		}
		private static Delegates.skottie_animation_builder_delete skottie_animation_builder_delete_delegate;
		internal static void skottie_animation_builder_delete (skottie_animation_builder_t instance) =>
			(skottie_animation_builder_delete_delegate ??= GetSymbol<Delegates.skottie_animation_builder_delete> ("skottie_animation_builder_delete")).Invoke (instance);
		#endif

		// void skottie_animation_builder_get_stats(skottie_animation_builder_t* instance, skottie_animation_builder_stats_t* stats)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_builder_get_stats (skottie_animation_builder_t instance, AnimationBuilderStats* stats);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_builder_get_stats (skottie_animation_builder_t instance, AnimationBuilderStats* stats);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_builder_get_stats (skottie_animation_builder_t instance, AnimationBuilderStats* stats);
		}
		private static Delegates.skottie_animation_builder_get_stats skottie_animation_builder_get_stats_delegate;
		internal static void skottie_animation_builder_get_stats (skottie_animation_builder_t instance, AnimationBuilderStats* stats) =>
			(skottie_animation_builder_get_stats_delegate ??= GetSymbol<Delegates.skottie_animation_builder_get_stats> ("skottie_animation_builder_get_stats")).Invoke (instance, stats);
		#endif

		// skottie_animation_t* skottie_animation_builder_make_from_data(skottie_animation_builder_t* instance, const char* data, size_t length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial skottie_animation_t skottie_animation_builder_make_from_data (skottie_animation_builder_t instance, /* char */ void* data, /* size_t */ IntPtr length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_t skottie_animation_builder_make_from_data (skottie_animation_builder_t instance, /* char */ void* data, /* size_t */ IntPtr length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate skottie_animation_t skottie_animation_builder_make_from_data (skottie_animation_builder_t instance, /* char */ void* data, /* size_t */ IntPtr length);
		}
		private static Delegates.skottie_animation_builder_make_from_data skottie_animation_builder_make_from_data_delegate;
		internal static skottie_animation_t skottie_animation_builder_make_from_data (skottie_animation_builder_t instance, /* char */ void* data, /* size_t */ IntPtr length) =>
			(skottie_animation_builder_make_from_data_delegate ??= GetSymbol<Delegates.skottie_animation_builder_make_from_data> ("skottie_animation_builder_make_from_data")).Invoke (instance, data, length);
		#endif

		// skottie_animation_t* skottie_animation_builder_make_from_file(skottie_animation_builder_t* instance, const char* path)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial skottie_animation_t skottie_animation_builder_make_from_file (skottie_animation_builder_t instance, /* char */ void* path);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_t skottie_animation_builder_make_from_file (skottie_animation_builder_t instance, /* char */ void* path);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate skottie_animation_t skottie_animation_builder_make_from_file (skottie_animation_builder_t instance, /* char */ void* path);
		}
		private static Delegates.skottie_animation_builder_make_from_file skottie_animation_builder_make_from_file_delegate;
		internal static skottie_animation_t skottie_animation_builder_make_from_file (skottie_animation_builder_t instance, /* char */ void* path) =>
			(skottie_animation_builder_make_from_file_delegate ??= GetSymbol<Delegates.skottie_animation_builder_make_from_file> ("skottie_animation_builder_make_from_file")).Invoke (instance, path);
		#endif

		// skottie_animation_t* skottie_animation_builder_make_from_stream(skottie_animation_builder_t* instance, sk_stream_t* stream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial skottie_animation_t skottie_animation_builder_make_from_stream (skottie_animation_builder_t instance, sk_stream_t stream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_t skottie_animation_builder_make_from_stream (skottie_animation_builder_t instance, sk_stream_t stream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate skottie_animation_t skottie_animation_builder_make_from_stream (skottie_animation_builder_t instance, sk_stream_t stream);
		}
		private static Delegates.skottie_animation_builder_make_from_stream skottie_animation_builder_make_from_stream_delegate;
		internal static skottie_animation_t skottie_animation_builder_make_from_stream (skottie_animation_builder_t instance, sk_stream_t stream) =>
			(skottie_animation_builder_make_from_stream_delegate ??= GetSymbol<Delegates.skottie_animation_builder_make_from_stream> ("skottie_animation_builder_make_from_stream")).Invoke (instance, stream);
		#endif

		// skottie_animation_t* skottie_animation_builder_make_from_string(skottie_animation_builder_t* instance, const char* data, size_t length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial skottie_animation_t skottie_animation_builder_make_from_string (skottie_animation_builder_t instance, /* char */ void* data, /* size_t */ IntPtr length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_t skottie_animation_builder_make_from_string (skottie_animation_builder_t instance, /* char */ void* data, /* size_t */ IntPtr length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate skottie_animation_t skottie_animation_builder_make_from_string (skottie_animation_builder_t instance, /* char */ void* data, /* size_t */ IntPtr length);
		}
		private static Delegates.skottie_animation_builder_make_from_string skottie_animation_builder_make_from_string_delegate;
		internal static skottie_animation_t skottie_animation_builder_make_from_string (skottie_animation_builder_t instance, /* char */ void* data, /* size_t */ IntPtr length) =>
			(skottie_animation_builder_make_from_string_delegate ??= GetSymbol<Delegates.skottie_animation_builder_make_from_string> ("skottie_animation_builder_make_from_string")).Invoke (instance, data, length);
		#endif

		// skottie_animation_builder_t* skottie_animation_builder_new(skottie_animation_builder_flags_t flags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial skottie_animation_builder_t skottie_animation_builder_new (AnimationBuilderFlags flags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_builder_t skottie_animation_builder_new (AnimationBuilderFlags flags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate skottie_animation_builder_t skottie_animation_builder_new (AnimationBuilderFlags flags);
		}
		private static Delegates.skottie_animation_builder_new skottie_animation_builder_new_delegate;
		internal static skottie_animation_builder_t skottie_animation_builder_new (AnimationBuilderFlags flags) =>
			(skottie_animation_builder_new_delegate ??= GetSymbol<Delegates.skottie_animation_builder_new> ("skottie_animation_builder_new")).Invoke (flags);
		#endif

		// void skottie_animation_builder_set_font_manager(skottie_animation_builder_t* instance, sk_fontmgr_t* fontManager)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_builder_set_font_manager (skottie_animation_builder_t instance, sk_fontmgr_t fontManager);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_builder_set_font_manager (skottie_animation_builder_t instance, sk_fontmgr_t fontManager);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_builder_set_font_manager (skottie_animation_builder_t instance, sk_fontmgr_t fontManager);
		}
		private static Delegates.skottie_animation_builder_set_font_manager skottie_animation_builder_set_font_manager_delegate;
		internal static void skottie_animation_builder_set_font_manager (skottie_animation_builder_t instance, sk_fontmgr_t fontManager) =>
			(skottie_animation_builder_set_font_manager_delegate ??= GetSymbol<Delegates.skottie_animation_builder_set_font_manager> ("skottie_animation_builder_set_font_manager")).Invoke (instance, fontManager);
		#endif

		// void skottie_animation_builder_set_resource_provider(skottie_animation_builder_t* instance, skottie_resource_provider_t* resourceProvider)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_builder_set_resource_provider (skottie_animation_builder_t instance, skottie_resource_provider_t resourceProvider);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_builder_set_resource_provider (skottie_animation_builder_t instance, skottie_resource_provider_t resourceProvider);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_builder_set_resource_provider (skottie_animation_builder_t instance, skottie_resource_provider_t resourceProvider);
		}
		private static Delegates.skottie_animation_builder_set_resource_provider skottie_animation_builder_set_resource_provider_delegate;
		internal static void skottie_animation_builder_set_resource_provider (skottie_animation_builder_t instance, skottie_resource_provider_t resourceProvider) =>
			(skottie_animation_builder_set_resource_provider_delegate ??= GetSymbol<Delegates.skottie_animation_builder_set_resource_provider> ("skottie_animation_builder_set_resource_provider")).Invoke (instance, resourceProvider);
		#endif

		// void skottie_animation_delete(skottie_animation_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_delete (skottie_animation_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_delete (skottie_animation_t instance);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Double skottie_animation_get_duration (skottie_animation_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Double skottie_animation_get_duration (skottie_animation_t instance);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Double skottie_animation_get_fps (skottie_animation_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Double skottie_animation_get_fps (skottie_animation_t instance);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Double skottie_animation_get_in_point (skottie_animation_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Double skottie_animation_get_in_point (skottie_animation_t instance);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Double skottie_animation_get_out_point (skottie_animation_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Double skottie_animation_get_out_point (skottie_animation_t instance);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_get_size (skottie_animation_t instance, SKSize* size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_get_size (skottie_animation_t instance, SKSize* size);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_get_version (skottie_animation_t instance, sk_string_t version);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_get_version (skottie_animation_t instance, sk_string_t version);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_get_version (skottie_animation_t instance, sk_string_t version);
		}
		private static Delegates.skottie_animation_get_version skottie_animation_get_version_delegate;
		internal static void skottie_animation_get_version (skottie_animation_t instance, sk_string_t version) =>
			(skottie_animation_get_version_delegate ??= GetSymbol<Delegates.skottie_animation_get_version> ("skottie_animation_get_version")).Invoke (instance, version);
		#endif

		// skottie_animation_t* skottie_animation_make_from_data(const char* data, size_t length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial skottie_animation_t skottie_animation_make_from_data (/* char */ void* data, /* size_t */ IntPtr length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_t skottie_animation_make_from_data (/* char */ void* data, /* size_t */ IntPtr length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate skottie_animation_t skottie_animation_make_from_data (/* char */ void* data, /* size_t */ IntPtr length);
		}
		private static Delegates.skottie_animation_make_from_data skottie_animation_make_from_data_delegate;
		internal static skottie_animation_t skottie_animation_make_from_data (/* char */ void* data, /* size_t */ IntPtr length) =>
			(skottie_animation_make_from_data_delegate ??= GetSymbol<Delegates.skottie_animation_make_from_data> ("skottie_animation_make_from_data")).Invoke (data, length);
		#endif

		// skottie_animation_t* skottie_animation_make_from_file(const char* path)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial skottie_animation_t skottie_animation_make_from_file ([MarshalAs (UnmanagedType.LPStr)] String path);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_t skottie_animation_make_from_file ([MarshalAs (UnmanagedType.LPStr)] String path);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial skottie_animation_t skottie_animation_make_from_stream (sk_stream_t stream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_t skottie_animation_make_from_stream (sk_stream_t stream);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial skottie_animation_t skottie_animation_make_from_string ([MarshalAs (UnmanagedType.LPStr)] String data, int length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_t skottie_animation_make_from_string ([MarshalAs (UnmanagedType.LPStr)] String data, int length);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_ref (skottie_animation_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_ref (skottie_animation_t instance);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_render (skottie_animation_t instance, sk_canvas_t canvas, SKRect* dst);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_render (skottie_animation_t instance, sk_canvas_t canvas, SKRect* dst);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_render_with_flags (skottie_animation_t instance, sk_canvas_t canvas, SKRect* dst, AnimationRenderFlags flags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_render_with_flags (skottie_animation_t instance, sk_canvas_t canvas, SKRect* dst, AnimationRenderFlags flags);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_seek (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_seek (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_seek_frame (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_seek_frame (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_seek_frame_time (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_seek_frame_time (skottie_animation_t instance, Single t, sksg_invalidation_controller_t ic);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_unref (skottie_animation_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_unref (skottie_animation_t instance);
		#endif
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
#if !USE_LIBRARY_IMPORT

#endif // !USE_LIBRARY_IMPORT
#endregion

#region Structs

namespace SkiaSharp.Skottie {

	// skottie_animation_builder_stats_t
	[StructLayout (LayoutKind.Sequential)]
	public readonly unsafe partial struct AnimationBuilderStats : IEquatable<AnimationBuilderStats> {
		// public float fTotalLoadTimeMS
		private readonly Single fTotalLoadTimeMS;

		// public float fJsonParseTimeMS
		private readonly Single fJsonParseTimeMS;

		// public float fSceneParseTimeMS
		private readonly Single fSceneParseTimeMS;

		// public size_t fJsonSize
		private readonly /* size_t */ IntPtr fJsonSize;

		// public size_t fAnimatorCount
		private readonly /* size_t */ IntPtr fAnimatorCount;

		public readonly bool Equals (AnimationBuilderStats obj) =>
#pragma warning disable CS8909
			fTotalLoadTimeMS == obj.fTotalLoadTimeMS && fJsonParseTimeMS == obj.fJsonParseTimeMS && fSceneParseTimeMS == obj.fSceneParseTimeMS && fJsonSize == obj.fJsonSize && fAnimatorCount == obj.fAnimatorCount;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is AnimationBuilderStats f && Equals (f);

		public static bool operator == (AnimationBuilderStats left, AnimationBuilderStats right) =>
			left.Equals (right);

		public static bool operator != (AnimationBuilderStats left, AnimationBuilderStats right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fTotalLoadTimeMS);
			hash.Add (fJsonParseTimeMS);
			hash.Add (fSceneParseTimeMS);
			hash.Add (fJsonSize);
			hash.Add (fAnimatorCount);
			return hash.ToHashCode ();
		}

	}
}

#endregion

#region Enums

namespace SkiaSharp.Skottie {

	// skottie_animation_builder_flags_t
	public enum AnimationBuilderFlags {
		// NONE_SKOTTIE_ANIMATION_BUILDER_FLAGS = 0
		None = 0,
		// DEFER_IMAGE_LOADING_SKOTTIE_ANIMATION_BUILDER_FLAGS = 0x01
		DeferImageLoading = 1,
		// PREFER_EMBEDDED_FONTS_SKOTTIE_ANIMATION_BUILDER_FLAGS = 0x02
		PreferEmbeddedFonts = 2,
	}

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

#region DelegateProxies

#endregion

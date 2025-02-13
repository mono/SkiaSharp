using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#region Namespaces

using SkiaSharp.Resources;

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
	internal unsafe partial class ResourcesApi
	{
		#region skresources_resource_provider.h

		// skresources_resource_provider_t* skresources_caching_resource_provider_proxy_make(skresources_resource_provider_t* rp)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial skresources_resource_provider_t skresources_caching_resource_provider_proxy_make (skresources_resource_provider_t rp);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skresources_resource_provider_t skresources_caching_resource_provider_proxy_make (skresources_resource_provider_t rp);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate skresources_resource_provider_t skresources_caching_resource_provider_proxy_make (skresources_resource_provider_t rp);
		}
		private static Delegates.skresources_caching_resource_provider_proxy_make skresources_caching_resource_provider_proxy_make_delegate;
		internal static skresources_resource_provider_t skresources_caching_resource_provider_proxy_make (skresources_resource_provider_t rp) =>
			(skresources_caching_resource_provider_proxy_make_delegate ??= GetSymbol<Delegates.skresources_caching_resource_provider_proxy_make> ("skresources_caching_resource_provider_proxy_make")).Invoke (rp);
		#endif

		// skresources_resource_provider_t* skresources_data_uri_resource_provider_proxy_make(skresources_resource_provider_t* rp, bool predecode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial skresources_resource_provider_t skresources_data_uri_resource_provider_proxy_make (skresources_resource_provider_t rp, [MarshalAs (UnmanagedType.I1)] bool predecode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skresources_resource_provider_t skresources_data_uri_resource_provider_proxy_make (skresources_resource_provider_t rp, [MarshalAs (UnmanagedType.I1)] bool predecode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate skresources_resource_provider_t skresources_data_uri_resource_provider_proxy_make (skresources_resource_provider_t rp, [MarshalAs (UnmanagedType.I1)] bool predecode);
		}
		private static Delegates.skresources_data_uri_resource_provider_proxy_make skresources_data_uri_resource_provider_proxy_make_delegate;
		internal static skresources_resource_provider_t skresources_data_uri_resource_provider_proxy_make (skresources_resource_provider_t rp, [MarshalAs (UnmanagedType.I1)] bool predecode) =>
			(skresources_data_uri_resource_provider_proxy_make_delegate ??= GetSymbol<Delegates.skresources_data_uri_resource_provider_proxy_make> ("skresources_data_uri_resource_provider_proxy_make")).Invoke (rp, predecode);
		#endif

		// skresources_resource_provider_t* skresources_file_resource_provider_make(sk_string_t* base_dir, bool predecode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial skresources_resource_provider_t skresources_file_resource_provider_make (sk_string_t base_dir, [MarshalAs (UnmanagedType.I1)] bool predecode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skresources_resource_provider_t skresources_file_resource_provider_make (sk_string_t base_dir, [MarshalAs (UnmanagedType.I1)] bool predecode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate skresources_resource_provider_t skresources_file_resource_provider_make (sk_string_t base_dir, [MarshalAs (UnmanagedType.I1)] bool predecode);
		}
		private static Delegates.skresources_file_resource_provider_make skresources_file_resource_provider_make_delegate;
		internal static skresources_resource_provider_t skresources_file_resource_provider_make (sk_string_t base_dir, [MarshalAs (UnmanagedType.I1)] bool predecode) =>
			(skresources_file_resource_provider_make_delegate ??= GetSymbol<Delegates.skresources_file_resource_provider_make> ("skresources_file_resource_provider_make")).Invoke (base_dir, predecode);
		#endif

		// void skresources_resource_provider_delete(skresources_resource_provider_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skresources_resource_provider_delete (skresources_resource_provider_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skresources_resource_provider_delete (skresources_resource_provider_t instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skresources_resource_provider_delete (skresources_resource_provider_t instance);
		}
		private static Delegates.skresources_resource_provider_delete skresources_resource_provider_delete_delegate;
		internal static void skresources_resource_provider_delete (skresources_resource_provider_t instance) =>
			(skresources_resource_provider_delete_delegate ??= GetSymbol<Delegates.skresources_resource_provider_delete> ("skresources_resource_provider_delete")).Invoke (instance);
		#endif

		// sk_data_t* skresources_resource_provider_load(skresources_resource_provider_t* instance, const char* path, const char* name)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_data_t skresources_resource_provider_load (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String path, [MarshalAs (UnmanagedType.LPStr)] String name);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t skresources_resource_provider_load (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String path, [MarshalAs (UnmanagedType.LPStr)] String name);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t skresources_resource_provider_load (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String path, [MarshalAs (UnmanagedType.LPStr)] String name);
		}
		private static Delegates.skresources_resource_provider_load skresources_resource_provider_load_delegate;
		internal static sk_data_t skresources_resource_provider_load (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String path, [MarshalAs (UnmanagedType.LPStr)] String name) =>
			(skresources_resource_provider_load_delegate ??= GetSymbol<Delegates.skresources_resource_provider_load> ("skresources_resource_provider_load")).Invoke (instance, path, name);
		#endif

		// skresources_external_track_asset_t* skresources_resource_provider_load_audio_asset(skresources_resource_provider_t* instance, const char* path, const char* name, const char* id)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial skresources_external_track_asset_t skresources_resource_provider_load_audio_asset (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String path, [MarshalAs (UnmanagedType.LPStr)] String name, [MarshalAs (UnmanagedType.LPStr)] String id);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skresources_external_track_asset_t skresources_resource_provider_load_audio_asset (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String path, [MarshalAs (UnmanagedType.LPStr)] String name, [MarshalAs (UnmanagedType.LPStr)] String id);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate skresources_external_track_asset_t skresources_resource_provider_load_audio_asset (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String path, [MarshalAs (UnmanagedType.LPStr)] String name, [MarshalAs (UnmanagedType.LPStr)] String id);
		}
		private static Delegates.skresources_resource_provider_load_audio_asset skresources_resource_provider_load_audio_asset_delegate;
		internal static skresources_external_track_asset_t skresources_resource_provider_load_audio_asset (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String path, [MarshalAs (UnmanagedType.LPStr)] String name, [MarshalAs (UnmanagedType.LPStr)] String id) =>
			(skresources_resource_provider_load_audio_asset_delegate ??= GetSymbol<Delegates.skresources_resource_provider_load_audio_asset> ("skresources_resource_provider_load_audio_asset")).Invoke (instance, path, name, id);
		#endif

		// skresources_image_asset_t* skresources_resource_provider_load_image_asset(skresources_resource_provider_t* instance, const char* path, const char* name, const char* id)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial skresources_image_asset_t skresources_resource_provider_load_image_asset (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String path, [MarshalAs (UnmanagedType.LPStr)] String name, [MarshalAs (UnmanagedType.LPStr)] String id);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skresources_image_asset_t skresources_resource_provider_load_image_asset (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String path, [MarshalAs (UnmanagedType.LPStr)] String name, [MarshalAs (UnmanagedType.LPStr)] String id);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate skresources_image_asset_t skresources_resource_provider_load_image_asset (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String path, [MarshalAs (UnmanagedType.LPStr)] String name, [MarshalAs (UnmanagedType.LPStr)] String id);
		}
		private static Delegates.skresources_resource_provider_load_image_asset skresources_resource_provider_load_image_asset_delegate;
		internal static skresources_image_asset_t skresources_resource_provider_load_image_asset (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String path, [MarshalAs (UnmanagedType.LPStr)] String name, [MarshalAs (UnmanagedType.LPStr)] String id) =>
			(skresources_resource_provider_load_image_asset_delegate ??= GetSymbol<Delegates.skresources_resource_provider_load_image_asset> ("skresources_resource_provider_load_image_asset")).Invoke (instance, path, name, id);
		#endif

		// sk_typeface_t* skresources_resource_provider_load_typeface(skresources_resource_provider_t* instance, const char* name, const char* url)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_typeface_t skresources_resource_provider_load_typeface (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String name, [MarshalAs (UnmanagedType.LPStr)] String url);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_typeface_t skresources_resource_provider_load_typeface (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String name, [MarshalAs (UnmanagedType.LPStr)] String url);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_typeface_t skresources_resource_provider_load_typeface (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String name, [MarshalAs (UnmanagedType.LPStr)] String url);
		}
		private static Delegates.skresources_resource_provider_load_typeface skresources_resource_provider_load_typeface_delegate;
		internal static sk_typeface_t skresources_resource_provider_load_typeface (skresources_resource_provider_t instance, [MarshalAs (UnmanagedType.LPStr)] String name, [MarshalAs (UnmanagedType.LPStr)] String url) =>
			(skresources_resource_provider_load_typeface_delegate ??= GetSymbol<Delegates.skresources_resource_provider_load_typeface> ("skresources_resource_provider_load_typeface")).Invoke (instance, name, url);
		#endif

		// void skresources_resource_provider_ref(skresources_resource_provider_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skresources_resource_provider_ref (skresources_resource_provider_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skresources_resource_provider_ref (skresources_resource_provider_t instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skresources_resource_provider_ref (skresources_resource_provider_t instance);
		}
		private static Delegates.skresources_resource_provider_ref skresources_resource_provider_ref_delegate;
		internal static void skresources_resource_provider_ref (skresources_resource_provider_t instance) =>
			(skresources_resource_provider_ref_delegate ??= GetSymbol<Delegates.skresources_resource_provider_ref> ("skresources_resource_provider_ref")).Invoke (instance);
		#endif

		// void skresources_resource_provider_unref(skresources_resource_provider_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skresources_resource_provider_unref (skresources_resource_provider_t instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skresources_resource_provider_unref (skresources_resource_provider_t instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skresources_resource_provider_unref (skresources_resource_provider_t instance);
		}
		private static Delegates.skresources_resource_provider_unref skresources_resource_provider_unref_delegate;
		internal static void skresources_resource_provider_unref (skresources_resource_provider_t instance) =>
			(skresources_resource_provider_unref_delegate ??= GetSymbol<Delegates.skresources_resource_provider_unref> ("skresources_resource_provider_unref")).Invoke (instance);
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

#endregion

#region Enums

#endregion

#region DelegateProxies

#endregion

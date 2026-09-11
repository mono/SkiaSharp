using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_graphite.h

		// bool sk_graphite_backend_is_available(sk_graphite_backend_t backend)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_graphite_backend_is_available (SKGraphiteBackend backend);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_graphite_backend_is_available (SKGraphiteBackend backend);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_graphite_backend_is_available (SKGraphiteBackend backend);
		}
		private static Delegates.sk_graphite_backend_is_available sk_graphite_backend_is_available_delegate;
		internal static bool sk_graphite_backend_is_available (SKGraphiteBackend backend) =>
			(sk_graphite_backend_is_available_delegate ??= GetSymbol<Delegates.sk_graphite_backend_is_available> ("sk_graphite_backend_is_available")).Invoke (backend);
		#endif

		// void sk_graphite_backend_texture_delete(sk_graphite_backend_texture_t* tex)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_backend_texture_delete (sk_graphite_backend_texture_t tex);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_backend_texture_delete (sk_graphite_backend_texture_t tex);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_backend_texture_delete (sk_graphite_backend_texture_t tex);
		}
		private static Delegates.sk_graphite_backend_texture_delete sk_graphite_backend_texture_delete_delegate;
		internal static void sk_graphite_backend_texture_delete (sk_graphite_backend_texture_t tex) =>
			(sk_graphite_backend_texture_delete_delegate ??= GetSymbol<Delegates.sk_graphite_backend_texture_delete> ("sk_graphite_backend_texture_delete")).Invoke (tex);
		#endif

		// sk_graphite_backend_t sk_graphite_backend_texture_get_backend(const sk_graphite_backend_texture_t* tex)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKGraphiteBackend sk_graphite_backend_texture_get_backend (sk_graphite_backend_texture_t tex);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKGraphiteBackend sk_graphite_backend_texture_get_backend (sk_graphite_backend_texture_t tex);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKGraphiteBackend sk_graphite_backend_texture_get_backend (sk_graphite_backend_texture_t tex);
		}
		private static Delegates.sk_graphite_backend_texture_get_backend sk_graphite_backend_texture_get_backend_delegate;
		internal static SKGraphiteBackend sk_graphite_backend_texture_get_backend (sk_graphite_backend_texture_t tex) =>
			(sk_graphite_backend_texture_get_backend_delegate ??= GetSymbol<Delegates.sk_graphite_backend_texture_get_backend> ("sk_graphite_backend_texture_get_backend")).Invoke (tex);
		#endif

		// void sk_graphite_backend_texture_get_dimensions(const sk_graphite_backend_texture_t* tex, int32_t* outWidth, int32_t* outHeight)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_backend_texture_get_dimensions (sk_graphite_backend_texture_t tex, Int32* outWidth, Int32* outHeight);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_backend_texture_get_dimensions (sk_graphite_backend_texture_t tex, Int32* outWidth, Int32* outHeight);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_backend_texture_get_dimensions (sk_graphite_backend_texture_t tex, Int32* outWidth, Int32* outHeight);
		}
		private static Delegates.sk_graphite_backend_texture_get_dimensions sk_graphite_backend_texture_get_dimensions_delegate;
		internal static void sk_graphite_backend_texture_get_dimensions (sk_graphite_backend_texture_t tex, Int32* outWidth, Int32* outHeight) =>
			(sk_graphite_backend_texture_get_dimensions_delegate ??= GetSymbol<Delegates.sk_graphite_backend_texture_get_dimensions> ("sk_graphite_backend_texture_get_dimensions")).Invoke (tex, outWidth, outHeight);
		#endif

		// bool sk_graphite_backend_texture_is_valid(const sk_graphite_backend_texture_t* tex)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_graphite_backend_texture_is_valid (sk_graphite_backend_texture_t tex);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_graphite_backend_texture_is_valid (sk_graphite_backend_texture_t tex);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_graphite_backend_texture_is_valid (sk_graphite_backend_texture_t tex);
		}
		private static Delegates.sk_graphite_backend_texture_is_valid sk_graphite_backend_texture_is_valid_delegate;
		internal static bool sk_graphite_backend_texture_is_valid (sk_graphite_backend_texture_t tex) =>
			(sk_graphite_backend_texture_is_valid_delegate ??= GetSymbol<Delegates.sk_graphite_backend_texture_is_valid> ("sk_graphite_backend_texture_is_valid")).Invoke (tex);
		#endif

		// void sk_graphite_context_async_rescale_and_read_pixels_surface(sk_graphite_context_t* context, const sk_surface_t* surface, const sk_imageinfo_t* dstInfo, const sk_irect_t* srcRect, sk_image_rescale_gamma_t rescaleGamma, sk_image_rescale_mode_t rescaleMode, sk_image_async_read_pixels_proc callback, void* callbackContext)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_context_async_rescale_and_read_pixels_surface (sk_graphite_context_t context, sk_surface_t surface, SKImageInfoNative* dstInfo, SKRectI* srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, void* callback, void* callbackContext);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_context_async_rescale_and_read_pixels_surface (sk_graphite_context_t context, sk_surface_t surface, SKImageInfoNative* dstInfo, SKRectI* srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, SKImageAsyncReadPixelsProxyDelegate callback, void* callbackContext);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_context_async_rescale_and_read_pixels_surface (sk_graphite_context_t context, sk_surface_t surface, SKImageInfoNative* dstInfo, SKRectI* srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, SKImageAsyncReadPixelsProxyDelegate callback, void* callbackContext);
		}
		private static Delegates.sk_graphite_context_async_rescale_and_read_pixels_surface sk_graphite_context_async_rescale_and_read_pixels_surface_delegate;
		internal static void sk_graphite_context_async_rescale_and_read_pixels_surface (sk_graphite_context_t context, sk_surface_t surface, SKImageInfoNative* dstInfo, SKRectI* srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, SKImageAsyncReadPixelsProxyDelegate callback, void* callbackContext) =>
			(sk_graphite_context_async_rescale_and_read_pixels_surface_delegate ??= GetSymbol<Delegates.sk_graphite_context_async_rescale_and_read_pixels_surface> ("sk_graphite_context_async_rescale_and_read_pixels_surface")).Invoke (context, surface, dstInfo, srcRect, rescaleGamma, rescaleMode, callback, callbackContext);
		#endif

		// void sk_graphite_context_check_async_work_completion(sk_graphite_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_context_check_async_work_completion (sk_graphite_context_t context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_context_check_async_work_completion (sk_graphite_context_t context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_context_check_async_work_completion (sk_graphite_context_t context);
		}
		private static Delegates.sk_graphite_context_check_async_work_completion sk_graphite_context_check_async_work_completion_delegate;
		internal static void sk_graphite_context_check_async_work_completion (sk_graphite_context_t context) =>
			(sk_graphite_context_check_async_work_completion_delegate ??= GetSymbol<Delegates.sk_graphite_context_check_async_work_completion> ("sk_graphite_context_check_async_work_completion")).Invoke (context);
		#endif

		// void sk_graphite_context_delete(sk_graphite_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_context_delete (sk_graphite_context_t context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_context_delete (sk_graphite_context_t context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_context_delete (sk_graphite_context_t context);
		}
		private static Delegates.sk_graphite_context_delete sk_graphite_context_delete_delegate;
		internal static void sk_graphite_context_delete (sk_graphite_context_t context) =>
			(sk_graphite_context_delete_delegate ??= GetSymbol<Delegates.sk_graphite_context_delete> ("sk_graphite_context_delete")).Invoke (context);
		#endif

		// void sk_graphite_context_delete_backend_texture(sk_graphite_context_t* context, const sk_graphite_backend_texture_t* tex)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_context_delete_backend_texture (sk_graphite_context_t context, sk_graphite_backend_texture_t tex);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_context_delete_backend_texture (sk_graphite_context_t context, sk_graphite_backend_texture_t tex);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_context_delete_backend_texture (sk_graphite_context_t context, sk_graphite_backend_texture_t tex);
		}
		private static Delegates.sk_graphite_context_delete_backend_texture sk_graphite_context_delete_backend_texture_delegate;
		internal static void sk_graphite_context_delete_backend_texture (sk_graphite_context_t context, sk_graphite_backend_texture_t tex) =>
			(sk_graphite_context_delete_backend_texture_delegate ??= GetSymbol<Delegates.sk_graphite_context_delete_backend_texture> ("sk_graphite_context_delete_backend_texture")).Invoke (context, tex);
		#endif

		// void sk_graphite_context_free_gpu_resources(sk_graphite_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_context_free_gpu_resources (sk_graphite_context_t context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_context_free_gpu_resources (sk_graphite_context_t context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_context_free_gpu_resources (sk_graphite_context_t context);
		}
		private static Delegates.sk_graphite_context_free_gpu_resources sk_graphite_context_free_gpu_resources_delegate;
		internal static void sk_graphite_context_free_gpu_resources (sk_graphite_context_t context) =>
			(sk_graphite_context_free_gpu_resources_delegate ??= GetSymbol<Delegates.sk_graphite_context_free_gpu_resources> ("sk_graphite_context_free_gpu_resources")).Invoke (context);
		#endif

		// sk_graphite_backend_t sk_graphite_context_get_backend(const sk_graphite_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKGraphiteBackend sk_graphite_context_get_backend (sk_graphite_context_t context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKGraphiteBackend sk_graphite_context_get_backend (sk_graphite_context_t context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKGraphiteBackend sk_graphite_context_get_backend (sk_graphite_context_t context);
		}
		private static Delegates.sk_graphite_context_get_backend sk_graphite_context_get_backend_delegate;
		internal static SKGraphiteBackend sk_graphite_context_get_backend (sk_graphite_context_t context) =>
			(sk_graphite_context_get_backend_delegate ??= GetSymbol<Delegates.sk_graphite_context_get_backend> ("sk_graphite_context_get_backend")).Invoke (context);
		#endif

		// int64_t sk_graphite_context_get_current_budgeted_bytes(const sk_graphite_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int64 sk_graphite_context_get_current_budgeted_bytes (sk_graphite_context_t context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int64 sk_graphite_context_get_current_budgeted_bytes (sk_graphite_context_t context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int64 sk_graphite_context_get_current_budgeted_bytes (sk_graphite_context_t context);
		}
		private static Delegates.sk_graphite_context_get_current_budgeted_bytes sk_graphite_context_get_current_budgeted_bytes_delegate;
		internal static Int64 sk_graphite_context_get_current_budgeted_bytes (sk_graphite_context_t context) =>
			(sk_graphite_context_get_current_budgeted_bytes_delegate ??= GetSymbol<Delegates.sk_graphite_context_get_current_budgeted_bytes> ("sk_graphite_context_get_current_budgeted_bytes")).Invoke (context);
		#endif

		// int64_t sk_graphite_context_get_max_budgeted_bytes(const sk_graphite_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int64 sk_graphite_context_get_max_budgeted_bytes (sk_graphite_context_t context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int64 sk_graphite_context_get_max_budgeted_bytes (sk_graphite_context_t context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int64 sk_graphite_context_get_max_budgeted_bytes (sk_graphite_context_t context);
		}
		private static Delegates.sk_graphite_context_get_max_budgeted_bytes sk_graphite_context_get_max_budgeted_bytes_delegate;
		internal static Int64 sk_graphite_context_get_max_budgeted_bytes (sk_graphite_context_t context) =>
			(sk_graphite_context_get_max_budgeted_bytes_delegate ??= GetSymbol<Delegates.sk_graphite_context_get_max_budgeted_bytes> ("sk_graphite_context_get_max_budgeted_bytes")).Invoke (context);
		#endif

		// int32_t sk_graphite_context_get_max_texture_size(const sk_graphite_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_graphite_context_get_max_texture_size (sk_graphite_context_t context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_graphite_context_get_max_texture_size (sk_graphite_context_t context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_graphite_context_get_max_texture_size (sk_graphite_context_t context);
		}
		private static Delegates.sk_graphite_context_get_max_texture_size sk_graphite_context_get_max_texture_size_delegate;
		internal static Int32 sk_graphite_context_get_max_texture_size (sk_graphite_context_t context) =>
			(sk_graphite_context_get_max_texture_size_delegate ??= GetSymbol<Delegates.sk_graphite_context_get_max_texture_size> ("sk_graphite_context_get_max_texture_size")).Invoke (context);
		#endif

		// sk_graphite_insert_status_t sk_graphite_context_insert_recording(sk_graphite_context_t* context, const sk_graphite_insert_recording_info_t* info)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKGraphiteInsertStatus sk_graphite_context_insert_recording (sk_graphite_context_t context, SKGraphiteInsertRecordingInfo* info);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKGraphiteInsertStatus sk_graphite_context_insert_recording (sk_graphite_context_t context, SKGraphiteInsertRecordingInfo* info);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKGraphiteInsertStatus sk_graphite_context_insert_recording (sk_graphite_context_t context, SKGraphiteInsertRecordingInfo* info);
		}
		private static Delegates.sk_graphite_context_insert_recording sk_graphite_context_insert_recording_delegate;
		internal static SKGraphiteInsertStatus sk_graphite_context_insert_recording (sk_graphite_context_t context, SKGraphiteInsertRecordingInfo* info) =>
			(sk_graphite_context_insert_recording_delegate ??= GetSymbol<Delegates.sk_graphite_context_insert_recording> ("sk_graphite_context_insert_recording")).Invoke (context, info);
		#endif

		// bool sk_graphite_context_is_device_lost(const sk_graphite_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_graphite_context_is_device_lost (sk_graphite_context_t context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_graphite_context_is_device_lost (sk_graphite_context_t context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_graphite_context_is_device_lost (sk_graphite_context_t context);
		}
		private static Delegates.sk_graphite_context_is_device_lost sk_graphite_context_is_device_lost_delegate;
		internal static bool sk_graphite_context_is_device_lost (sk_graphite_context_t context) =>
			(sk_graphite_context_is_device_lost_delegate ??= GetSymbol<Delegates.sk_graphite_context_is_device_lost> ("sk_graphite_context_is_device_lost")).Invoke (context);
		#endif

		// sk_graphite_recorder_t* sk_graphite_context_make_recorder(sk_graphite_context_t* context, int64_t recorderBudgetBytes, sk_graphite_image_provider_t* imageProvider)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_graphite_recorder_t sk_graphite_context_make_recorder (sk_graphite_context_t context, Int64 recorderBudgetBytes, sk_graphite_image_provider_t imageProvider);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_graphite_recorder_t sk_graphite_context_make_recorder (sk_graphite_context_t context, Int64 recorderBudgetBytes, sk_graphite_image_provider_t imageProvider);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_graphite_recorder_t sk_graphite_context_make_recorder (sk_graphite_context_t context, Int64 recorderBudgetBytes, sk_graphite_image_provider_t imageProvider);
		}
		private static Delegates.sk_graphite_context_make_recorder sk_graphite_context_make_recorder_delegate;
		internal static sk_graphite_recorder_t sk_graphite_context_make_recorder (sk_graphite_context_t context, Int64 recorderBudgetBytes, sk_graphite_image_provider_t imageProvider) =>
			(sk_graphite_context_make_recorder_delegate ??= GetSymbol<Delegates.sk_graphite_context_make_recorder> ("sk_graphite_context_make_recorder")).Invoke (context, recorderBudgetBytes, imageProvider);
		#endif

		// void sk_graphite_context_options_init_defaults(sk_graphite_context_options_t* out)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_context_options_init_defaults (SKGraphiteContextOptions* @out);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_context_options_init_defaults (SKGraphiteContextOptions* @out);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_context_options_init_defaults (SKGraphiteContextOptions* @out);
		}
		private static Delegates.sk_graphite_context_options_init_defaults sk_graphite_context_options_init_defaults_delegate;
		internal static void sk_graphite_context_options_init_defaults (SKGraphiteContextOptions* @out) =>
			(sk_graphite_context_options_init_defaults_delegate ??= GetSymbol<Delegates.sk_graphite_context_options_init_defaults> ("sk_graphite_context_options_init_defaults")).Invoke (@out);
		#endif

		// void sk_graphite_context_perform_deferred_cleanup(sk_graphite_context_t* context, int64_t milliseconds)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_context_perform_deferred_cleanup (sk_graphite_context_t context, Int64 milliseconds);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_context_perform_deferred_cleanup (sk_graphite_context_t context, Int64 milliseconds);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_context_perform_deferred_cleanup (sk_graphite_context_t context, Int64 milliseconds);
		}
		private static Delegates.sk_graphite_context_perform_deferred_cleanup sk_graphite_context_perform_deferred_cleanup_delegate;
		internal static void sk_graphite_context_perform_deferred_cleanup (sk_graphite_context_t context, Int64 milliseconds) =>
			(sk_graphite_context_perform_deferred_cleanup_delegate ??= GetSymbol<Delegates.sk_graphite_context_perform_deferred_cleanup> ("sk_graphite_context_perform_deferred_cleanup")).Invoke (context, milliseconds);
		#endif

		// void sk_graphite_context_set_max_budgeted_bytes(sk_graphite_context_t* context, int64_t bytes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_context_set_max_budgeted_bytes (sk_graphite_context_t context, Int64 bytes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_context_set_max_budgeted_bytes (sk_graphite_context_t context, Int64 bytes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_context_set_max_budgeted_bytes (sk_graphite_context_t context, Int64 bytes);
		}
		private static Delegates.sk_graphite_context_set_max_budgeted_bytes sk_graphite_context_set_max_budgeted_bytes_delegate;
		internal static void sk_graphite_context_set_max_budgeted_bytes (sk_graphite_context_t context, Int64 bytes) =>
			(sk_graphite_context_set_max_budgeted_bytes_delegate ??= GetSymbol<Delegates.sk_graphite_context_set_max_budgeted_bytes> ("sk_graphite_context_set_max_budgeted_bytes")).Invoke (context, bytes);
		#endif

		// bool sk_graphite_context_submit(sk_graphite_context_t* context, const sk_graphite_submit_info_t* info)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_graphite_context_submit (sk_graphite_context_t context, SKGraphiteSubmitInfo* info);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_graphite_context_submit (sk_graphite_context_t context, SKGraphiteSubmitInfo* info);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_graphite_context_submit (sk_graphite_context_t context, SKGraphiteSubmitInfo* info);
		}
		private static Delegates.sk_graphite_context_submit sk_graphite_context_submit_delegate;
		internal static bool sk_graphite_context_submit (sk_graphite_context_t context, SKGraphiteSubmitInfo* info) =>
			(sk_graphite_context_submit_delegate ??= GetSymbol<Delegates.sk_graphite_context_submit> ("sk_graphite_context_submit")).Invoke (context, info);
		#endif

		// bool sk_graphite_context_supports_protected_content(const sk_graphite_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_graphite_context_supports_protected_content (sk_graphite_context_t context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_graphite_context_supports_protected_content (sk_graphite_context_t context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_graphite_context_supports_protected_content (sk_graphite_context_t context);
		}
		private static Delegates.sk_graphite_context_supports_protected_content sk_graphite_context_supports_protected_content_delegate;
		internal static bool sk_graphite_context_supports_protected_content (sk_graphite_context_t context) =>
			(sk_graphite_context_supports_protected_content_delegate ??= GetSymbol<Delegates.sk_graphite_context_supports_protected_content> ("sk_graphite_context_supports_protected_content")).Invoke (context);
		#endif

		// sk_image_t* sk_graphite_image_make_texture(sk_graphite_recorder_t* recorder, const sk_image_t* image, bool mipmapped)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_image_t sk_graphite_image_make_texture (sk_graphite_recorder_t recorder, sk_image_t image, [MarshalAs (UnmanagedType.I1)] bool mipmapped);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_graphite_image_make_texture (sk_graphite_recorder_t recorder, sk_image_t image, [MarshalAs (UnmanagedType.I1)] bool mipmapped);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_graphite_image_make_texture (sk_graphite_recorder_t recorder, sk_image_t image, [MarshalAs (UnmanagedType.I1)] bool mipmapped);
		}
		private static Delegates.sk_graphite_image_make_texture sk_graphite_image_make_texture_delegate;
		internal static sk_image_t sk_graphite_image_make_texture (sk_graphite_recorder_t recorder, sk_image_t image, [MarshalAs (UnmanagedType.I1)] bool mipmapped) =>
			(sk_graphite_image_make_texture_delegate ??= GetSymbol<Delegates.sk_graphite_image_make_texture> ("sk_graphite_image_make_texture")).Invoke (recorder, image, mipmapped);
		#endif

		// void sk_graphite_image_provider_delete(sk_graphite_image_provider_t* provider)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_image_provider_delete (sk_graphite_image_provider_t provider);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_image_provider_delete (sk_graphite_image_provider_t provider);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_image_provider_delete (sk_graphite_image_provider_t provider);
		}
		private static Delegates.sk_graphite_image_provider_delete sk_graphite_image_provider_delete_delegate;
		internal static void sk_graphite_image_provider_delete (sk_graphite_image_provider_t provider) =>
			(sk_graphite_image_provider_delete_delegate ??= GetSymbol<Delegates.sk_graphite_image_provider_delete> ("sk_graphite_image_provider_delete")).Invoke (provider);
		#endif

		// sk_graphite_image_provider_t* sk_graphite_image_provider_new(sk_graphite_image_provider_proc proc, void* userData)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_graphite_image_provider_t sk_graphite_image_provider_new (void* proc, void* userData);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_graphite_image_provider_t sk_graphite_image_provider_new (SKGraphiteImageProviderProxyDelegate proc, void* userData);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_graphite_image_provider_t sk_graphite_image_provider_new (SKGraphiteImageProviderProxyDelegate proc, void* userData);
		}
		private static Delegates.sk_graphite_image_provider_new sk_graphite_image_provider_new_delegate;
		internal static sk_graphite_image_provider_t sk_graphite_image_provider_new (SKGraphiteImageProviderProxyDelegate proc, void* userData) =>
			(sk_graphite_image_provider_new_delegate ??= GetSymbol<Delegates.sk_graphite_image_provider_new> ("sk_graphite_image_provider_new")).Invoke (proc, userData);
		#endif

		// sk_image_t* sk_graphite_image_wrap_texture(sk_graphite_recorder_t* recorder, const sk_graphite_backend_texture_t* backendTexture, sk_colortype_t colorType, sk_alphatype_t alphaType, sk_colorspace_t* colorSpace, sk_graphite_release_proc releaseProc, void* releaseContext)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_image_t sk_graphite_image_wrap_texture (sk_graphite_recorder_t recorder, sk_graphite_backend_texture_t backendTexture, SKColorTypeNative colorType, SKAlphaType alphaType, sk_colorspace_t colorSpace, void* releaseProc, void* releaseContext);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_image_t sk_graphite_image_wrap_texture (sk_graphite_recorder_t recorder, sk_graphite_backend_texture_t backendTexture, SKColorTypeNative colorType, SKAlphaType alphaType, sk_colorspace_t colorSpace, SKGraphiteReleaseProxyDelegate releaseProc, void* releaseContext);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_image_t sk_graphite_image_wrap_texture (sk_graphite_recorder_t recorder, sk_graphite_backend_texture_t backendTexture, SKColorTypeNative colorType, SKAlphaType alphaType, sk_colorspace_t colorSpace, SKGraphiteReleaseProxyDelegate releaseProc, void* releaseContext);
		}
		private static Delegates.sk_graphite_image_wrap_texture sk_graphite_image_wrap_texture_delegate;
		internal static sk_image_t sk_graphite_image_wrap_texture (sk_graphite_recorder_t recorder, sk_graphite_backend_texture_t backendTexture, SKColorTypeNative colorType, SKAlphaType alphaType, sk_colorspace_t colorSpace, SKGraphiteReleaseProxyDelegate releaseProc, void* releaseContext) =>
			(sk_graphite_image_wrap_texture_delegate ??= GetSymbol<Delegates.sk_graphite_image_wrap_texture> ("sk_graphite_image_wrap_texture")).Invoke (recorder, backendTexture, colorType, alphaType, colorSpace, releaseProc, releaseContext);
		#endif

		// sk_graphite_backend_texture_t* sk_graphite_recorder_create_backend_texture(sk_graphite_recorder_t* recorder, int32_t width, int32_t height, const sk_graphite_texture_info_t* info)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_graphite_backend_texture_t sk_graphite_recorder_create_backend_texture (sk_graphite_recorder_t recorder, Int32 width, Int32 height, sk_graphite_texture_info_t info);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_graphite_backend_texture_t sk_graphite_recorder_create_backend_texture (sk_graphite_recorder_t recorder, Int32 width, Int32 height, sk_graphite_texture_info_t info);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_graphite_backend_texture_t sk_graphite_recorder_create_backend_texture (sk_graphite_recorder_t recorder, Int32 width, Int32 height, sk_graphite_texture_info_t info);
		}
		private static Delegates.sk_graphite_recorder_create_backend_texture sk_graphite_recorder_create_backend_texture_delegate;
		internal static sk_graphite_backend_texture_t sk_graphite_recorder_create_backend_texture (sk_graphite_recorder_t recorder, Int32 width, Int32 height, sk_graphite_texture_info_t info) =>
			(sk_graphite_recorder_create_backend_texture_delegate ??= GetSymbol<Delegates.sk_graphite_recorder_create_backend_texture> ("sk_graphite_recorder_create_backend_texture")).Invoke (recorder, width, height, info);
		#endif

		// void sk_graphite_recorder_delete(sk_graphite_recorder_t* recorder)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_recorder_delete (sk_graphite_recorder_t recorder);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_recorder_delete (sk_graphite_recorder_t recorder);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_recorder_delete (sk_graphite_recorder_t recorder);
		}
		private static Delegates.sk_graphite_recorder_delete sk_graphite_recorder_delete_delegate;
		internal static void sk_graphite_recorder_delete (sk_graphite_recorder_t recorder) =>
			(sk_graphite_recorder_delete_delegate ??= GetSymbol<Delegates.sk_graphite_recorder_delete> ("sk_graphite_recorder_delete")).Invoke (recorder);
		#endif

		// void sk_graphite_recorder_delete_backend_texture(sk_graphite_recorder_t* recorder, const sk_graphite_backend_texture_t* tex)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_recorder_delete_backend_texture (sk_graphite_recorder_t recorder, sk_graphite_backend_texture_t tex);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_recorder_delete_backend_texture (sk_graphite_recorder_t recorder, sk_graphite_backend_texture_t tex);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_recorder_delete_backend_texture (sk_graphite_recorder_t recorder, sk_graphite_backend_texture_t tex);
		}
		private static Delegates.sk_graphite_recorder_delete_backend_texture sk_graphite_recorder_delete_backend_texture_delegate;
		internal static void sk_graphite_recorder_delete_backend_texture (sk_graphite_recorder_t recorder, sk_graphite_backend_texture_t tex) =>
			(sk_graphite_recorder_delete_backend_texture_delegate ??= GetSymbol<Delegates.sk_graphite_recorder_delete_backend_texture> ("sk_graphite_recorder_delete_backend_texture")).Invoke (recorder, tex);
		#endif

		// sk_graphite_backend_t sk_graphite_recorder_get_backend(const sk_graphite_recorder_t* recorder)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKGraphiteBackend sk_graphite_recorder_get_backend (sk_graphite_recorder_t recorder);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKGraphiteBackend sk_graphite_recorder_get_backend (sk_graphite_recorder_t recorder);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKGraphiteBackend sk_graphite_recorder_get_backend (sk_graphite_recorder_t recorder);
		}
		private static Delegates.sk_graphite_recorder_get_backend sk_graphite_recorder_get_backend_delegate;
		internal static SKGraphiteBackend sk_graphite_recorder_get_backend (sk_graphite_recorder_t recorder) =>
			(sk_graphite_recorder_get_backend_delegate ??= GetSymbol<Delegates.sk_graphite_recorder_get_backend> ("sk_graphite_recorder_get_backend")).Invoke (recorder);
		#endif

		// int32_t sk_graphite_recorder_get_max_texture_size(const sk_graphite_recorder_t* recorder)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_graphite_recorder_get_max_texture_size (sk_graphite_recorder_t recorder);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_graphite_recorder_get_max_texture_size (sk_graphite_recorder_t recorder);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_graphite_recorder_get_max_texture_size (sk_graphite_recorder_t recorder);
		}
		private static Delegates.sk_graphite_recorder_get_max_texture_size sk_graphite_recorder_get_max_texture_size_delegate;
		internal static Int32 sk_graphite_recorder_get_max_texture_size (sk_graphite_recorder_t recorder) =>
			(sk_graphite_recorder_get_max_texture_size_delegate ??= GetSymbol<Delegates.sk_graphite_recorder_get_max_texture_size> ("sk_graphite_recorder_get_max_texture_size")).Invoke (recorder);
		#endif

		// sk_graphite_recording_t* sk_graphite_recorder_snap(sk_graphite_recorder_t* recorder)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_graphite_recording_t sk_graphite_recorder_snap (sk_graphite_recorder_t recorder);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_graphite_recording_t sk_graphite_recorder_snap (sk_graphite_recorder_t recorder);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_graphite_recording_t sk_graphite_recorder_snap (sk_graphite_recorder_t recorder);
		}
		private static Delegates.sk_graphite_recorder_snap sk_graphite_recorder_snap_delegate;
		internal static sk_graphite_recording_t sk_graphite_recorder_snap (sk_graphite_recorder_t recorder) =>
			(sk_graphite_recorder_snap_delegate ??= GetSymbol<Delegates.sk_graphite_recorder_snap> ("sk_graphite_recorder_snap")).Invoke (recorder);
		#endif

		// void sk_graphite_recording_delete(sk_graphite_recording_t* recording)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_recording_delete (sk_graphite_recording_t recording);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_recording_delete (sk_graphite_recording_t recording);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_recording_delete (sk_graphite_recording_t recording);
		}
		private static Delegates.sk_graphite_recording_delete sk_graphite_recording_delete_delegate;
		internal static void sk_graphite_recording_delete (sk_graphite_recording_t recording) =>
			(sk_graphite_recording_delete_delegate ??= GetSymbol<Delegates.sk_graphite_recording_delete> ("sk_graphite_recording_delete")).Invoke (recording);
		#endif

		// sk_surface_t* sk_graphite_surface_make_render_target(sk_graphite_recorder_t* recorder, const sk_imageinfo_t* info, bool mipmapped, const sk_surfaceprops_t* props)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_surface_t sk_graphite_surface_make_render_target (sk_graphite_recorder_t recorder, SKImageInfoNative* info, [MarshalAs (UnmanagedType.I1)] bool mipmapped, sk_surfaceprops_t props);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_graphite_surface_make_render_target (sk_graphite_recorder_t recorder, SKImageInfoNative* info, [MarshalAs (UnmanagedType.I1)] bool mipmapped, sk_surfaceprops_t props);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_surface_t sk_graphite_surface_make_render_target (sk_graphite_recorder_t recorder, SKImageInfoNative* info, [MarshalAs (UnmanagedType.I1)] bool mipmapped, sk_surfaceprops_t props);
		}
		private static Delegates.sk_graphite_surface_make_render_target sk_graphite_surface_make_render_target_delegate;
		internal static sk_surface_t sk_graphite_surface_make_render_target (sk_graphite_recorder_t recorder, SKImageInfoNative* info, [MarshalAs (UnmanagedType.I1)] bool mipmapped, sk_surfaceprops_t props) =>
			(sk_graphite_surface_make_render_target_delegate ??= GetSymbol<Delegates.sk_graphite_surface_make_render_target> ("sk_graphite_surface_make_render_target")).Invoke (recorder, info, mipmapped, props);
		#endif

		// sk_surface_t* sk_graphite_surface_wrap_backend_texture(sk_graphite_recorder_t* recorder, const sk_graphite_backend_texture_t* backendTexture, sk_colortype_t colorType, sk_colorspace_t* colorSpace, const sk_surfaceprops_t* props, sk_graphite_release_proc releaseProc, void* releaseContext)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_surface_t sk_graphite_surface_wrap_backend_texture (sk_graphite_recorder_t recorder, sk_graphite_backend_texture_t backendTexture, SKColorTypeNative colorType, sk_colorspace_t colorSpace, sk_surfaceprops_t props, void* releaseProc, void* releaseContext);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_graphite_surface_wrap_backend_texture (sk_graphite_recorder_t recorder, sk_graphite_backend_texture_t backendTexture, SKColorTypeNative colorType, sk_colorspace_t colorSpace, sk_surfaceprops_t props, SKGraphiteReleaseProxyDelegate releaseProc, void* releaseContext);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_surface_t sk_graphite_surface_wrap_backend_texture (sk_graphite_recorder_t recorder, sk_graphite_backend_texture_t backendTexture, SKColorTypeNative colorType, sk_colorspace_t colorSpace, sk_surfaceprops_t props, SKGraphiteReleaseProxyDelegate releaseProc, void* releaseContext);
		}
		private static Delegates.sk_graphite_surface_wrap_backend_texture sk_graphite_surface_wrap_backend_texture_delegate;
		internal static sk_surface_t sk_graphite_surface_wrap_backend_texture (sk_graphite_recorder_t recorder, sk_graphite_backend_texture_t backendTexture, SKColorTypeNative colorType, sk_colorspace_t colorSpace, sk_surfaceprops_t props, SKGraphiteReleaseProxyDelegate releaseProc, void* releaseContext) =>
			(sk_graphite_surface_wrap_backend_texture_delegate ??= GetSymbol<Delegates.sk_graphite_surface_wrap_backend_texture> ("sk_graphite_surface_wrap_backend_texture")).Invoke (recorder, backendTexture, colorType, colorSpace, props, releaseProc, releaseContext);
		#endif

		// void sk_graphite_texture_info_delete(sk_graphite_texture_info_t* info)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphite_texture_info_delete (sk_graphite_texture_info_t info);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphite_texture_info_delete (sk_graphite_texture_info_t info);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphite_texture_info_delete (sk_graphite_texture_info_t info);
		}
		private static Delegates.sk_graphite_texture_info_delete sk_graphite_texture_info_delete_delegate;
		internal static void sk_graphite_texture_info_delete (sk_graphite_texture_info_t info) =>
			(sk_graphite_texture_info_delete_delegate ??= GetSymbol<Delegates.sk_graphite_texture_info_delete> ("sk_graphite_texture_info_delete")).Invoke (info);
		#endif

		// sk_graphite_backend_t sk_graphite_texture_info_get_backend(const sk_graphite_texture_info_t* info)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKGraphiteBackend sk_graphite_texture_info_get_backend (sk_graphite_texture_info_t info);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKGraphiteBackend sk_graphite_texture_info_get_backend (sk_graphite_texture_info_t info);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKGraphiteBackend sk_graphite_texture_info_get_backend (sk_graphite_texture_info_t info);
		}
		private static Delegates.sk_graphite_texture_info_get_backend sk_graphite_texture_info_get_backend_delegate;
		internal static SKGraphiteBackend sk_graphite_texture_info_get_backend (sk_graphite_texture_info_t info) =>
			(sk_graphite_texture_info_get_backend_delegate ??= GetSymbol<Delegates.sk_graphite_texture_info_get_backend> ("sk_graphite_texture_info_get_backend")).Invoke (info);
		#endif

		// bool sk_graphite_texture_info_get_mipmapped(const sk_graphite_texture_info_t* info)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_graphite_texture_info_get_mipmapped (sk_graphite_texture_info_t info);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_graphite_texture_info_get_mipmapped (sk_graphite_texture_info_t info);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_graphite_texture_info_get_mipmapped (sk_graphite_texture_info_t info);
		}
		private static Delegates.sk_graphite_texture_info_get_mipmapped sk_graphite_texture_info_get_mipmapped_delegate;
		internal static bool sk_graphite_texture_info_get_mipmapped (sk_graphite_texture_info_t info) =>
			(sk_graphite_texture_info_get_mipmapped_delegate ??= GetSymbol<Delegates.sk_graphite_texture_info_get_mipmapped> ("sk_graphite_texture_info_get_mipmapped")).Invoke (info);
		#endif

		// int32_t sk_graphite_texture_info_get_sample_count(const sk_graphite_texture_info_t* info)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_graphite_texture_info_get_sample_count (sk_graphite_texture_info_t info);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_graphite_texture_info_get_sample_count (sk_graphite_texture_info_t info);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_graphite_texture_info_get_sample_count (sk_graphite_texture_info_t info);
		}
		private static Delegates.sk_graphite_texture_info_get_sample_count sk_graphite_texture_info_get_sample_count_delegate;
		internal static Int32 sk_graphite_texture_info_get_sample_count (sk_graphite_texture_info_t info) =>
			(sk_graphite_texture_info_get_sample_count_delegate ??= GetSymbol<Delegates.sk_graphite_texture_info_get_sample_count> ("sk_graphite_texture_info_get_sample_count")).Invoke (info);
		#endif

		// bool sk_graphite_texture_info_is_valid(const sk_graphite_texture_info_t* info)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_graphite_texture_info_is_valid (sk_graphite_texture_info_t info);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_graphite_texture_info_is_valid (sk_graphite_texture_info_t info);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_graphite_texture_info_is_valid (sk_graphite_texture_info_t info);
		}
		private static Delegates.sk_graphite_texture_info_is_valid sk_graphite_texture_info_is_valid_delegate;
		internal static bool sk_graphite_texture_info_is_valid (sk_graphite_texture_info_t info) =>
			(sk_graphite_texture_info_is_valid_delegate ??= GetSymbol<Delegates.sk_graphite_texture_info_is_valid> ("sk_graphite_texture_info_is_valid")).Invoke (info);
		#endif

		#endregion

	}
}

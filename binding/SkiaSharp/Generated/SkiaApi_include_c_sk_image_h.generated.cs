using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_image.h

		// int32_t sk_image_async_read_result_get_count(const sk_image_async_read_result_t* result)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_image_async_read_result_get_count (IntPtr result);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_image_async_read_result_get_count (IntPtr result);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_image_async_read_result_get_count (IntPtr result);
		}
		private static Delegates.sk_image_async_read_result_get_count sk_image_async_read_result_get_count_delegate;
		internal static Int32 sk_image_async_read_result_get_count (IntPtr result) =>
			(sk_image_async_read_result_get_count_delegate ??= GetSymbol<Delegates.sk_image_async_read_result_get_count> ("sk_image_async_read_result_get_count")).Invoke (result);
		#endif

		// const void* sk_image_async_read_result_get_data(const sk_image_async_read_result_t* result, int32_t planeIndex)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void* sk_image_async_read_result_get_data (IntPtr result, Int32 planeIndex);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_image_async_read_result_get_data (IntPtr result, Int32 planeIndex);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_image_async_read_result_get_data (IntPtr result, Int32 planeIndex);
		}
		private static Delegates.sk_image_async_read_result_get_data sk_image_async_read_result_get_data_delegate;
		internal static void* sk_image_async_read_result_get_data (IntPtr result, Int32 planeIndex) =>
			(sk_image_async_read_result_get_data_delegate ??= GetSymbol<Delegates.sk_image_async_read_result_get_data> ("sk_image_async_read_result_get_data")).Invoke (result, planeIndex);
		#endif

		// size_t sk_image_async_read_result_get_row_bytes(const sk_image_async_read_result_t* result, int32_t planeIndex)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_image_async_read_result_get_row_bytes (IntPtr result, Int32 planeIndex);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_image_async_read_result_get_row_bytes (IntPtr result, Int32 planeIndex);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_image_async_read_result_get_row_bytes (IntPtr result, Int32 planeIndex);
		}
		private static Delegates.sk_image_async_read_result_get_row_bytes sk_image_async_read_result_get_row_bytes_delegate;
		internal static /* size_t */ IntPtr sk_image_async_read_result_get_row_bytes (IntPtr result, Int32 planeIndex) =>
			(sk_image_async_read_result_get_row_bytes_delegate ??= GetSymbol<Delegates.sk_image_async_read_result_get_row_bytes> ("sk_image_async_read_result_get_row_bytes")).Invoke (result, planeIndex);
		#endif

		// void sk_image_async_rescale_and_read_pixels(const sk_image_t* image, const sk_imageinfo_t* dstInfo, const sk_irect_t* srcRect, sk_image_rescale_gamma_t rescaleGamma, sk_image_rescale_mode_t rescaleMode, sk_image_async_read_pixels_proc callback, void* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_image_async_rescale_and_read_pixels (IntPtr image, SKImageInfoNative* dstInfo, SKRectI* srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, void* callback, void* context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_image_async_rescale_and_read_pixels (IntPtr image, SKImageInfoNative* dstInfo, SKRectI* srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, SKImageAsyncReadPixelsProxyDelegate callback, void* context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_image_async_rescale_and_read_pixels (IntPtr image, SKImageInfoNative* dstInfo, SKRectI* srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, SKImageAsyncReadPixelsProxyDelegate callback, void* context);
		}
		private static Delegates.sk_image_async_rescale_and_read_pixels sk_image_async_rescale_and_read_pixels_delegate;
		internal static void sk_image_async_rescale_and_read_pixels (IntPtr image, SKImageInfoNative* dstInfo, SKRectI* srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, SKImageAsyncReadPixelsProxyDelegate callback, void* context) =>
			(sk_image_async_rescale_and_read_pixels_delegate ??= GetSymbol<Delegates.sk_image_async_rescale_and_read_pixels> ("sk_image_async_rescale_and_read_pixels")).Invoke (image, dstInfo, srcRect, rescaleGamma, rescaleMode, callback, context);
		#endif

		// sk_alphatype_t sk_image_get_alpha_type(const sk_image_t* image)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKAlphaType sk_image_get_alpha_type (IntPtr image);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKAlphaType sk_image_get_alpha_type (IntPtr image);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKAlphaType sk_image_get_alpha_type (IntPtr image);
		}
		private static Delegates.sk_image_get_alpha_type sk_image_get_alpha_type_delegate;
		internal static SKAlphaType sk_image_get_alpha_type (IntPtr image) =>
			(sk_image_get_alpha_type_delegate ??= GetSymbol<Delegates.sk_image_get_alpha_type> ("sk_image_get_alpha_type")).Invoke (image);
		#endif

		// sk_colortype_t sk_image_get_color_type(const sk_image_t* image)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKColorTypeNative sk_image_get_color_type (IntPtr image);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKColorTypeNative sk_image_get_color_type (IntPtr image);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKColorTypeNative sk_image_get_color_type (IntPtr image);
		}
		private static Delegates.sk_image_get_color_type sk_image_get_color_type_delegate;
		internal static SKColorTypeNative sk_image_get_color_type (IntPtr image) =>
			(sk_image_get_color_type_delegate ??= GetSymbol<Delegates.sk_image_get_color_type> ("sk_image_get_color_type")).Invoke (image);
		#endif

		// sk_colorspace_t* sk_image_get_colorspace(const sk_image_t* image)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_get_colorspace (IntPtr image);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_get_colorspace (IntPtr image);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_get_colorspace (IntPtr image);
		}
		private static Delegates.sk_image_get_colorspace sk_image_get_colorspace_delegate;
		internal static IntPtr sk_image_get_colorspace (IntPtr image) =>
			(sk_image_get_colorspace_delegate ??= GetSymbol<Delegates.sk_image_get_colorspace> ("sk_image_get_colorspace")).Invoke (image);
		#endif

		// int sk_image_get_height(const sk_image_t* cimage)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_image_get_height (IntPtr cimage);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_image_get_height (IntPtr cimage);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_image_get_height (IntPtr cimage);
		}
		private static Delegates.sk_image_get_height sk_image_get_height_delegate;
		internal static Int32 sk_image_get_height (IntPtr cimage) =>
			(sk_image_get_height_delegate ??= GetSymbol<Delegates.sk_image_get_height> ("sk_image_get_height")).Invoke (cimage);
		#endif

		// uint32_t sk_image_get_unique_id(const sk_image_t* cimage)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt32 sk_image_get_unique_id (IntPtr cimage);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_image_get_unique_id (IntPtr cimage);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_image_get_unique_id (IntPtr cimage);
		}
		private static Delegates.sk_image_get_unique_id sk_image_get_unique_id_delegate;
		internal static UInt32 sk_image_get_unique_id (IntPtr cimage) =>
			(sk_image_get_unique_id_delegate ??= GetSymbol<Delegates.sk_image_get_unique_id> ("sk_image_get_unique_id")).Invoke (cimage);
		#endif

		// int sk_image_get_width(const sk_image_t* cimage)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_image_get_width (IntPtr cimage);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_image_get_width (IntPtr cimage);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_image_get_width (IntPtr cimage);
		}
		private static Delegates.sk_image_get_width sk_image_get_width_delegate;
		internal static Int32 sk_image_get_width (IntPtr cimage) =>
			(sk_image_get_width_delegate ??= GetSymbol<Delegates.sk_image_get_width> ("sk_image_get_width")).Invoke (cimage);
		#endif

		// bool sk_image_is_alpha_only(const sk_image_t* image)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_image_is_alpha_only (IntPtr image);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_is_alpha_only (IntPtr image);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_is_alpha_only (IntPtr image);
		}
		private static Delegates.sk_image_is_alpha_only sk_image_is_alpha_only_delegate;
		internal static bool sk_image_is_alpha_only (IntPtr image) =>
			(sk_image_is_alpha_only_delegate ??= GetSymbol<Delegates.sk_image_is_alpha_only> ("sk_image_is_alpha_only")).Invoke (image);
		#endif

		// bool sk_image_is_lazy_generated(const sk_image_t* image)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_image_is_lazy_generated (IntPtr image);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_is_lazy_generated (IntPtr image);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_is_lazy_generated (IntPtr image);
		}
		private static Delegates.sk_image_is_lazy_generated sk_image_is_lazy_generated_delegate;
		internal static bool sk_image_is_lazy_generated (IntPtr image) =>
			(sk_image_is_lazy_generated_delegate ??= GetSymbol<Delegates.sk_image_is_lazy_generated> ("sk_image_is_lazy_generated")).Invoke (image);
		#endif

		// bool sk_image_is_texture_backed(const sk_image_t* image)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_image_is_texture_backed (IntPtr image);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_is_texture_backed (IntPtr image);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_is_texture_backed (IntPtr image);
		}
		private static Delegates.sk_image_is_texture_backed sk_image_is_texture_backed_delegate;
		internal static bool sk_image_is_texture_backed (IntPtr image) =>
			(sk_image_is_texture_backed_delegate ??= GetSymbol<Delegates.sk_image_is_texture_backed> ("sk_image_is_texture_backed")).Invoke (image);
		#endif

		// bool sk_image_is_valid(const sk_image_t* image, gr_recording_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_image_is_valid (IntPtr image, IntPtr context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_is_valid (IntPtr image, IntPtr context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_is_valid (IntPtr image, IntPtr context);
		}
		private static Delegates.sk_image_is_valid sk_image_is_valid_delegate;
		internal static bool sk_image_is_valid (IntPtr image, IntPtr context) =>
			(sk_image_is_valid_delegate ??= GetSymbol<Delegates.sk_image_is_valid> ("sk_image_is_valid")).Invoke (image, context);
		#endif

		// sk_image_t* sk_image_make_non_texture_image(const sk_image_t* cimage)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_make_non_texture_image (IntPtr cimage);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_make_non_texture_image (IntPtr cimage);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_make_non_texture_image (IntPtr cimage);
		}
		private static Delegates.sk_image_make_non_texture_image sk_image_make_non_texture_image_delegate;
		internal static IntPtr sk_image_make_non_texture_image (IntPtr cimage) =>
			(sk_image_make_non_texture_image_delegate ??= GetSymbol<Delegates.sk_image_make_non_texture_image> ("sk_image_make_non_texture_image")).Invoke (cimage);
		#endif

		// sk_image_t* sk_image_make_raster_image(const sk_image_t* cimage)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_make_raster_image (IntPtr cimage);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_make_raster_image (IntPtr cimage);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_make_raster_image (IntPtr cimage);
		}
		private static Delegates.sk_image_make_raster_image sk_image_make_raster_image_delegate;
		internal static IntPtr sk_image_make_raster_image (IntPtr cimage) =>
			(sk_image_make_raster_image_delegate ??= GetSymbol<Delegates.sk_image_make_raster_image> ("sk_image_make_raster_image")).Invoke (cimage);
		#endif

		// sk_shader_t* sk_image_make_raw_shader(const sk_image_t* image, sk_shader_tilemode_t tileX, sk_shader_tilemode_t tileY, const sk_sampling_options_t* sampling, const sk_matrix_t* cmatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_make_raw_shader (IntPtr image, SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions* sampling, SKMatrix* cmatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_make_raw_shader (IntPtr image, SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions* sampling, SKMatrix* cmatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_make_raw_shader (IntPtr image, SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions* sampling, SKMatrix* cmatrix);
		}
		private static Delegates.sk_image_make_raw_shader sk_image_make_raw_shader_delegate;
		internal static IntPtr sk_image_make_raw_shader (IntPtr image, SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions* sampling, SKMatrix* cmatrix) =>
			(sk_image_make_raw_shader_delegate ??= GetSymbol<Delegates.sk_image_make_raw_shader> ("sk_image_make_raw_shader")).Invoke (image, tileX, tileY, sampling, cmatrix);
		#endif

		// sk_shader_t* sk_image_make_shader(const sk_image_t* image, sk_shader_tilemode_t tileX, sk_shader_tilemode_t tileY, const sk_sampling_options_t* sampling, const sk_matrix_t* cmatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_make_shader (IntPtr image, SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions* sampling, SKMatrix* cmatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_make_shader (IntPtr image, SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions* sampling, SKMatrix* cmatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_make_shader (IntPtr image, SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions* sampling, SKMatrix* cmatrix);
		}
		private static Delegates.sk_image_make_shader sk_image_make_shader_delegate;
		internal static IntPtr sk_image_make_shader (IntPtr image, SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions* sampling, SKMatrix* cmatrix) =>
			(sk_image_make_shader_delegate ??= GetSymbol<Delegates.sk_image_make_shader> ("sk_image_make_shader")).Invoke (image, tileX, tileY, sampling, cmatrix);
		#endif

		// sk_image_t* sk_image_make_subset(const sk_image_t* cimage, gr_direct_context_t* context, const sk_irect_t* subset)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_make_subset (IntPtr cimage, IntPtr context, SKRectI* subset);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_make_subset (IntPtr cimage, IntPtr context, SKRectI* subset);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_make_subset (IntPtr cimage, IntPtr context, SKRectI* subset);
		}
		private static Delegates.sk_image_make_subset sk_image_make_subset_delegate;
		internal static IntPtr sk_image_make_subset (IntPtr cimage, IntPtr context, SKRectI* subset) =>
			(sk_image_make_subset_delegate ??= GetSymbol<Delegates.sk_image_make_subset> ("sk_image_make_subset")).Invoke (cimage, context, subset);
		#endif

		// sk_image_t* sk_image_make_subset_raster(const sk_image_t* cimage, const sk_irect_t* subset)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_make_subset_raster (IntPtr cimage, SKRectI* subset);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_make_subset_raster (IntPtr cimage, SKRectI* subset);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_make_subset_raster (IntPtr cimage, SKRectI* subset);
		}
		private static Delegates.sk_image_make_subset_raster sk_image_make_subset_raster_delegate;
		internal static IntPtr sk_image_make_subset_raster (IntPtr cimage, SKRectI* subset) =>
			(sk_image_make_subset_raster_delegate ??= GetSymbol<Delegates.sk_image_make_subset_raster> ("sk_image_make_subset_raster")).Invoke (cimage, subset);
		#endif

		// sk_image_t* sk_image_make_texture_image(const sk_image_t* cimage, gr_direct_context_t* context, bool mipmapped, bool budgeted)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_make_texture_image (IntPtr cimage, IntPtr context, [MarshalAs (UnmanagedType.I1)] bool mipmapped, [MarshalAs (UnmanagedType.I1)] bool budgeted);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_make_texture_image (IntPtr cimage, IntPtr context, [MarshalAs (UnmanagedType.I1)] bool mipmapped, [MarshalAs (UnmanagedType.I1)] bool budgeted);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_make_texture_image (IntPtr cimage, IntPtr context, [MarshalAs (UnmanagedType.I1)] bool mipmapped, [MarshalAs (UnmanagedType.I1)] bool budgeted);
		}
		private static Delegates.sk_image_make_texture_image sk_image_make_texture_image_delegate;
		internal static IntPtr sk_image_make_texture_image (IntPtr cimage, IntPtr context, [MarshalAs (UnmanagedType.I1)] bool mipmapped, [MarshalAs (UnmanagedType.I1)] bool budgeted) =>
			(sk_image_make_texture_image_delegate ??= GetSymbol<Delegates.sk_image_make_texture_image> ("sk_image_make_texture_image")).Invoke (cimage, context, mipmapped, budgeted);
		#endif

		// sk_image_t* sk_image_make_with_filter(const sk_image_t* cimage, gr_recording_context_t* context, const sk_imagefilter_t* filter, const sk_irect_t* subset, const sk_irect_t* clipBounds, sk_irect_t* outSubset, sk_ipoint_t* outOffset)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_make_with_filter (IntPtr cimage, IntPtr context, IntPtr filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_make_with_filter (IntPtr cimage, IntPtr context, IntPtr filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_make_with_filter (IntPtr cimage, IntPtr context, IntPtr filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);
		}
		private static Delegates.sk_image_make_with_filter sk_image_make_with_filter_delegate;
		internal static IntPtr sk_image_make_with_filter (IntPtr cimage, IntPtr context, IntPtr filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset) =>
			(sk_image_make_with_filter_delegate ??= GetSymbol<Delegates.sk_image_make_with_filter> ("sk_image_make_with_filter")).Invoke (cimage, context, filter, subset, clipBounds, outSubset, outOffset);
		#endif

		// sk_image_t* sk_image_make_with_filter_raster(const sk_image_t* cimage, const sk_imagefilter_t* filter, const sk_irect_t* subset, const sk_irect_t* clipBounds, sk_irect_t* outSubset, sk_ipoint_t* outOffset)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_make_with_filter_raster (IntPtr cimage, IntPtr filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_make_with_filter_raster (IntPtr cimage, IntPtr filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_make_with_filter_raster (IntPtr cimage, IntPtr filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);
		}
		private static Delegates.sk_image_make_with_filter_raster sk_image_make_with_filter_raster_delegate;
		internal static IntPtr sk_image_make_with_filter_raster (IntPtr cimage, IntPtr filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset) =>
			(sk_image_make_with_filter_raster_delegate ??= GetSymbol<Delegates.sk_image_make_with_filter_raster> ("sk_image_make_with_filter_raster")).Invoke (cimage, filter, subset, clipBounds, outSubset, outOffset);
		#endif

		// sk_image_t* sk_image_new_from_adopted_texture(gr_recording_context_t* context, const gr_backendtexture_t* texture, gr_surfaceorigin_t origin, sk_colortype_t colorType, sk_alphatype_t alpha, const sk_colorspace_t* colorSpace)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_new_from_adopted_texture (IntPtr context, IntPtr texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, IntPtr colorSpace);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_new_from_adopted_texture (IntPtr context, IntPtr texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, IntPtr colorSpace);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_new_from_adopted_texture (IntPtr context, IntPtr texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, IntPtr colorSpace);
		}
		private static Delegates.sk_image_new_from_adopted_texture sk_image_new_from_adopted_texture_delegate;
		internal static IntPtr sk_image_new_from_adopted_texture (IntPtr context, IntPtr texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, IntPtr colorSpace) =>
			(sk_image_new_from_adopted_texture_delegate ??= GetSymbol<Delegates.sk_image_new_from_adopted_texture> ("sk_image_new_from_adopted_texture")).Invoke (context, texture, origin, colorType, alpha, colorSpace);
		#endif

		// sk_image_t* sk_image_new_from_bitmap(const sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_new_from_bitmap (IntPtr cbitmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_new_from_bitmap (IntPtr cbitmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_new_from_bitmap (IntPtr cbitmap);
		}
		private static Delegates.sk_image_new_from_bitmap sk_image_new_from_bitmap_delegate;
		internal static IntPtr sk_image_new_from_bitmap (IntPtr cbitmap) =>
			(sk_image_new_from_bitmap_delegate ??= GetSymbol<Delegates.sk_image_new_from_bitmap> ("sk_image_new_from_bitmap")).Invoke (cbitmap);
		#endif

		// sk_image_t* sk_image_new_from_encoded(const sk_data_t* cdata)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_new_from_encoded (IntPtr cdata);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_new_from_encoded (IntPtr cdata);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_new_from_encoded (IntPtr cdata);
		}
		private static Delegates.sk_image_new_from_encoded sk_image_new_from_encoded_delegate;
		internal static IntPtr sk_image_new_from_encoded (IntPtr cdata) =>
			(sk_image_new_from_encoded_delegate ??= GetSymbol<Delegates.sk_image_new_from_encoded> ("sk_image_new_from_encoded")).Invoke (cdata);
		#endif

		// sk_image_t* sk_image_new_from_picture(sk_picture_t* picture, const sk_isize_t* dimensions, const sk_matrix_t* cmatrix, const sk_paint_t* paint, bool useFloatingPointBitDepth, const sk_colorspace_t* colorSpace, const sk_surfaceprops_t* props)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_new_from_picture (IntPtr picture, SKSizeI* dimensions, SKMatrix* cmatrix, IntPtr paint, [MarshalAs (UnmanagedType.I1)] bool useFloatingPointBitDepth, IntPtr colorSpace, IntPtr props);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_new_from_picture (IntPtr picture, SKSizeI* dimensions, SKMatrix* cmatrix, IntPtr paint, [MarshalAs (UnmanagedType.I1)] bool useFloatingPointBitDepth, IntPtr colorSpace, IntPtr props);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_new_from_picture (IntPtr picture, SKSizeI* dimensions, SKMatrix* cmatrix, IntPtr paint, [MarshalAs (UnmanagedType.I1)] bool useFloatingPointBitDepth, IntPtr colorSpace, IntPtr props);
		}
		private static Delegates.sk_image_new_from_picture sk_image_new_from_picture_delegate;
		internal static IntPtr sk_image_new_from_picture (IntPtr picture, SKSizeI* dimensions, SKMatrix* cmatrix, IntPtr paint, [MarshalAs (UnmanagedType.I1)] bool useFloatingPointBitDepth, IntPtr colorSpace, IntPtr props) =>
			(sk_image_new_from_picture_delegate ??= GetSymbol<Delegates.sk_image_new_from_picture> ("sk_image_new_from_picture")).Invoke (picture, dimensions, cmatrix, paint, useFloatingPointBitDepth, colorSpace, props);
		#endif

		// sk_image_t* sk_image_new_from_texture(gr_recording_context_t* context, const gr_backendtexture_t* texture, gr_surfaceorigin_t origin, sk_colortype_t colorType, sk_alphatype_t alpha, const sk_colorspace_t* colorSpace, const sk_image_texture_release_proc releaseProc, void* releaseContext)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_new_from_texture (IntPtr context, IntPtr texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, IntPtr colorSpace, void* releaseProc, void* releaseContext);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_new_from_texture (IntPtr context, IntPtr texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, IntPtr colorSpace, SKImageTextureReleaseProxyDelegate releaseProc, void* releaseContext);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_new_from_texture (IntPtr context, IntPtr texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, IntPtr colorSpace, SKImageTextureReleaseProxyDelegate releaseProc, void* releaseContext);
		}
		private static Delegates.sk_image_new_from_texture sk_image_new_from_texture_delegate;
		internal static IntPtr sk_image_new_from_texture (IntPtr context, IntPtr texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, IntPtr colorSpace, SKImageTextureReleaseProxyDelegate releaseProc, void* releaseContext) =>
			(sk_image_new_from_texture_delegate ??= GetSymbol<Delegates.sk_image_new_from_texture> ("sk_image_new_from_texture")).Invoke (context, texture, origin, colorType, alpha, colorSpace, releaseProc, releaseContext);
		#endif

		// sk_image_t* sk_image_new_raster(const sk_pixmap_t* pixmap, sk_image_raster_release_proc releaseProc, void* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_new_raster (IntPtr pixmap, void* releaseProc, void* context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_new_raster (IntPtr pixmap, SKImageRasterReleaseProxyDelegate releaseProc, void* context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_new_raster (IntPtr pixmap, SKImageRasterReleaseProxyDelegate releaseProc, void* context);
		}
		private static Delegates.sk_image_new_raster sk_image_new_raster_delegate;
		internal static IntPtr sk_image_new_raster (IntPtr pixmap, SKImageRasterReleaseProxyDelegate releaseProc, void* context) =>
			(sk_image_new_raster_delegate ??= GetSymbol<Delegates.sk_image_new_raster> ("sk_image_new_raster")).Invoke (pixmap, releaseProc, context);
		#endif

		// sk_image_t* sk_image_new_raster_copy(const sk_imageinfo_t* cinfo, const void* pixels, size_t rowBytes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_new_raster_copy (SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_new_raster_copy (SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_new_raster_copy (SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes);
		}
		private static Delegates.sk_image_new_raster_copy sk_image_new_raster_copy_delegate;
		internal static IntPtr sk_image_new_raster_copy (SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes) =>
			(sk_image_new_raster_copy_delegate ??= GetSymbol<Delegates.sk_image_new_raster_copy> ("sk_image_new_raster_copy")).Invoke (cinfo, pixels, rowBytes);
		#endif

		// sk_image_t* sk_image_new_raster_copy_with_pixmap(const sk_pixmap_t* pixmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_new_raster_copy_with_pixmap (IntPtr pixmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_new_raster_copy_with_pixmap (IntPtr pixmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_new_raster_copy_with_pixmap (IntPtr pixmap);
		}
		private static Delegates.sk_image_new_raster_copy_with_pixmap sk_image_new_raster_copy_with_pixmap_delegate;
		internal static IntPtr sk_image_new_raster_copy_with_pixmap (IntPtr pixmap) =>
			(sk_image_new_raster_copy_with_pixmap_delegate ??= GetSymbol<Delegates.sk_image_new_raster_copy_with_pixmap> ("sk_image_new_raster_copy_with_pixmap")).Invoke (pixmap);
		#endif

		// sk_image_t* sk_image_new_raster_data(const sk_imageinfo_t* cinfo, sk_data_t* pixels, size_t rowBytes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_new_raster_data (SKImageInfoNative* cinfo, IntPtr pixels, /* size_t */ IntPtr rowBytes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_new_raster_data (SKImageInfoNative* cinfo, IntPtr pixels, /* size_t */ IntPtr rowBytes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_new_raster_data (SKImageInfoNative* cinfo, IntPtr pixels, /* size_t */ IntPtr rowBytes);
		}
		private static Delegates.sk_image_new_raster_data sk_image_new_raster_data_delegate;
		internal static IntPtr sk_image_new_raster_data (SKImageInfoNative* cinfo, IntPtr pixels, /* size_t */ IntPtr rowBytes) =>
			(sk_image_new_raster_data_delegate ??= GetSymbol<Delegates.sk_image_new_raster_data> ("sk_image_new_raster_data")).Invoke (cinfo, pixels, rowBytes);
		#endif

		// bool sk_image_peek_pixels(const sk_image_t* image, sk_pixmap_t* pixmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_image_peek_pixels (IntPtr image, IntPtr pixmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_peek_pixels (IntPtr image, IntPtr pixmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_peek_pixels (IntPtr image, IntPtr pixmap);
		}
		private static Delegates.sk_image_peek_pixels sk_image_peek_pixels_delegate;
		internal static bool sk_image_peek_pixels (IntPtr image, IntPtr pixmap) =>
			(sk_image_peek_pixels_delegate ??= GetSymbol<Delegates.sk_image_peek_pixels> ("sk_image_peek_pixels")).Invoke (image, pixmap);
		#endif

		// bool sk_image_read_pixels(const sk_image_t* image, const sk_imageinfo_t* dstInfo, void* dstPixels, size_t dstRowBytes, int srcX, int srcY, sk_image_caching_hint_t cachingHint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_image_read_pixels (IntPtr image, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_read_pixels (IntPtr image, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_read_pixels (IntPtr image, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint);
		}
		private static Delegates.sk_image_read_pixels sk_image_read_pixels_delegate;
		internal static bool sk_image_read_pixels (IntPtr image, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint) =>
			(sk_image_read_pixels_delegate ??= GetSymbol<Delegates.sk_image_read_pixels> ("sk_image_read_pixels")).Invoke (image, dstInfo, dstPixels, dstRowBytes, srcX, srcY, cachingHint);
		#endif

		// bool sk_image_read_pixels_into_pixmap(const sk_image_t* image, const sk_pixmap_t* dst, int srcX, int srcY, sk_image_caching_hint_t cachingHint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_image_read_pixels_into_pixmap (IntPtr image, IntPtr dst, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_read_pixels_into_pixmap (IntPtr image, IntPtr dst, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_read_pixels_into_pixmap (IntPtr image, IntPtr dst, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint);
		}
		private static Delegates.sk_image_read_pixels_into_pixmap sk_image_read_pixels_into_pixmap_delegate;
		internal static bool sk_image_read_pixels_into_pixmap (IntPtr image, IntPtr dst, Int32 srcX, Int32 srcY, SKImageCachingHint cachingHint) =>
			(sk_image_read_pixels_into_pixmap_delegate ??= GetSymbol<Delegates.sk_image_read_pixels_into_pixmap> ("sk_image_read_pixels_into_pixmap")).Invoke (image, dst, srcX, srcY, cachingHint);
		#endif

		// void sk_image_ref(const sk_image_t* cimage)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_image_ref (IntPtr cimage);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_image_ref (IntPtr cimage);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_image_ref (IntPtr cimage);
		}
		private static Delegates.sk_image_ref sk_image_ref_delegate;
		internal static void sk_image_ref (IntPtr cimage) =>
			(sk_image_ref_delegate ??= GetSymbol<Delegates.sk_image_ref> ("sk_image_ref")).Invoke (cimage);
		#endif

		// sk_data_t* sk_image_ref_encoded(const sk_image_t* cimage)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_image_ref_encoded (IntPtr cimage);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_image_ref_encoded (IntPtr cimage);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_image_ref_encoded (IntPtr cimage);
		}
		private static Delegates.sk_image_ref_encoded sk_image_ref_encoded_delegate;
		internal static IntPtr sk_image_ref_encoded (IntPtr cimage) =>
			(sk_image_ref_encoded_delegate ??= GetSymbol<Delegates.sk_image_ref_encoded> ("sk_image_ref_encoded")).Invoke (cimage);
		#endif

		// bool sk_image_scale_pixels(const sk_image_t* image, const sk_pixmap_t* dst, const sk_sampling_options_t* sampling, sk_image_caching_hint_t cachingHint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_image_scale_pixels (IntPtr image, IntPtr dst, SKSamplingOptions* sampling, SKImageCachingHint cachingHint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_image_scale_pixels (IntPtr image, IntPtr dst, SKSamplingOptions* sampling, SKImageCachingHint cachingHint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_image_scale_pixels (IntPtr image, IntPtr dst, SKSamplingOptions* sampling, SKImageCachingHint cachingHint);
		}
		private static Delegates.sk_image_scale_pixels sk_image_scale_pixels_delegate;
		internal static bool sk_image_scale_pixels (IntPtr image, IntPtr dst, SKSamplingOptions* sampling, SKImageCachingHint cachingHint) =>
			(sk_image_scale_pixels_delegate ??= GetSymbol<Delegates.sk_image_scale_pixels> ("sk_image_scale_pixels")).Invoke (image, dst, sampling, cachingHint);
		#endif

		// void sk_image_unref(const sk_image_t* cimage)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_image_unref (IntPtr cimage);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_image_unref (IntPtr cimage);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_image_unref (IntPtr cimage);
		}
		private static Delegates.sk_image_unref sk_image_unref_delegate;
		internal static void sk_image_unref (IntPtr cimage) =>
			(sk_image_unref_delegate ??= GetSymbol<Delegates.sk_image_unref> ("sk_image_unref")).Invoke (cimage);
		#endif

		#endregion

	}
}

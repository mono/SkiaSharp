using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_codec.h

		// void sk_codec_destroy(sk_codec_t* codec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_codec_destroy (IntPtr codec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_codec_destroy (IntPtr codec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_codec_destroy (IntPtr codec);
		}
		private static Delegates.sk_codec_destroy sk_codec_destroy_delegate;
		internal static void sk_codec_destroy (IntPtr codec) =>
			(sk_codec_destroy_delegate ??= GetSymbol<Delegates.sk_codec_destroy> ("sk_codec_destroy")).Invoke (codec);
		#endif

		// sk_encoded_image_format_t sk_codec_get_encoded_format(sk_codec_t* codec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKEncodedImageFormat sk_codec_get_encoded_format (IntPtr codec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKEncodedImageFormat sk_codec_get_encoded_format (IntPtr codec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKEncodedImageFormat sk_codec_get_encoded_format (IntPtr codec);
		}
		private static Delegates.sk_codec_get_encoded_format sk_codec_get_encoded_format_delegate;
		internal static SKEncodedImageFormat sk_codec_get_encoded_format (IntPtr codec) =>
			(sk_codec_get_encoded_format_delegate ??= GetSymbol<Delegates.sk_codec_get_encoded_format> ("sk_codec_get_encoded_format")).Invoke (codec);
		#endif

		// int sk_codec_get_frame_count(sk_codec_t* codec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_codec_get_frame_count (IntPtr codec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_get_frame_count (IntPtr codec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_codec_get_frame_count (IntPtr codec);
		}
		private static Delegates.sk_codec_get_frame_count sk_codec_get_frame_count_delegate;
		internal static Int32 sk_codec_get_frame_count (IntPtr codec) =>
			(sk_codec_get_frame_count_delegate ??= GetSymbol<Delegates.sk_codec_get_frame_count> ("sk_codec_get_frame_count")).Invoke (codec);
		#endif

		// void sk_codec_get_frame_info(sk_codec_t* codec, sk_codec_frameinfo_t* frameInfo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_codec_get_frame_info (IntPtr codec, SKCodecFrameInfo* frameInfo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_codec_get_frame_info (IntPtr codec, SKCodecFrameInfo* frameInfo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_codec_get_frame_info (IntPtr codec, SKCodecFrameInfo* frameInfo);
		}
		private static Delegates.sk_codec_get_frame_info sk_codec_get_frame_info_delegate;
		internal static void sk_codec_get_frame_info (IntPtr codec, SKCodecFrameInfo* frameInfo) =>
			(sk_codec_get_frame_info_delegate ??= GetSymbol<Delegates.sk_codec_get_frame_info> ("sk_codec_get_frame_info")).Invoke (codec, frameInfo);
		#endif

		// bool sk_codec_get_frame_info_for_index(sk_codec_t* codec, int index, sk_codec_frameinfo_t* frameInfo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_codec_get_frame_info_for_index (IntPtr codec, Int32 index, SKCodecFrameInfo* frameInfo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_codec_get_frame_info_for_index (IntPtr codec, Int32 index, SKCodecFrameInfo* frameInfo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_codec_get_frame_info_for_index (IntPtr codec, Int32 index, SKCodecFrameInfo* frameInfo);
		}
		private static Delegates.sk_codec_get_frame_info_for_index sk_codec_get_frame_info_for_index_delegate;
		internal static bool sk_codec_get_frame_info_for_index (IntPtr codec, Int32 index, SKCodecFrameInfo* frameInfo) =>
			(sk_codec_get_frame_info_for_index_delegate ??= GetSymbol<Delegates.sk_codec_get_frame_info_for_index> ("sk_codec_get_frame_info_for_index")).Invoke (codec, index, frameInfo);
		#endif

		// void sk_codec_get_info(sk_codec_t* codec, sk_imageinfo_t* info)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_codec_get_info (IntPtr codec, SKImageInfoNative* info);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_codec_get_info (IntPtr codec, SKImageInfoNative* info);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_codec_get_info (IntPtr codec, SKImageInfoNative* info);
		}
		private static Delegates.sk_codec_get_info sk_codec_get_info_delegate;
		internal static void sk_codec_get_info (IntPtr codec, SKImageInfoNative* info) =>
			(sk_codec_get_info_delegate ??= GetSymbol<Delegates.sk_codec_get_info> ("sk_codec_get_info")).Invoke (codec, info);
		#endif

		// sk_encodedorigin_t sk_codec_get_origin(sk_codec_t* codec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKEncodedOrigin sk_codec_get_origin (IntPtr codec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKEncodedOrigin sk_codec_get_origin (IntPtr codec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKEncodedOrigin sk_codec_get_origin (IntPtr codec);
		}
		private static Delegates.sk_codec_get_origin sk_codec_get_origin_delegate;
		internal static SKEncodedOrigin sk_codec_get_origin (IntPtr codec) =>
			(sk_codec_get_origin_delegate ??= GetSymbol<Delegates.sk_codec_get_origin> ("sk_codec_get_origin")).Invoke (codec);
		#endif

		// sk_codec_result_t sk_codec_get_pixels(sk_codec_t* codec, const sk_imageinfo_t* info, void* pixels, size_t rowBytes, const sk_codec_options_t* options)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKCodecResult sk_codec_get_pixels (IntPtr codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecResult sk_codec_get_pixels (IntPtr codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKCodecResult sk_codec_get_pixels (IntPtr codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options);
		}
		private static Delegates.sk_codec_get_pixels sk_codec_get_pixels_delegate;
		internal static SKCodecResult sk_codec_get_pixels (IntPtr codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options) =>
			(sk_codec_get_pixels_delegate ??= GetSymbol<Delegates.sk_codec_get_pixels> ("sk_codec_get_pixels")).Invoke (codec, info, pixels, rowBytes, options);
		#endif

		// int sk_codec_get_repetition_count(sk_codec_t* codec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_codec_get_repetition_count (IntPtr codec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_get_repetition_count (IntPtr codec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_codec_get_repetition_count (IntPtr codec);
		}
		private static Delegates.sk_codec_get_repetition_count sk_codec_get_repetition_count_delegate;
		internal static Int32 sk_codec_get_repetition_count (IntPtr codec) =>
			(sk_codec_get_repetition_count_delegate ??= GetSymbol<Delegates.sk_codec_get_repetition_count> ("sk_codec_get_repetition_count")).Invoke (codec);
		#endif

		// void sk_codec_get_scaled_dimensions(sk_codec_t* codec, float desiredScale, sk_isize_t* dimensions)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_codec_get_scaled_dimensions (IntPtr codec, Single desiredScale, SKSizeI* dimensions);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_codec_get_scaled_dimensions (IntPtr codec, Single desiredScale, SKSizeI* dimensions);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_codec_get_scaled_dimensions (IntPtr codec, Single desiredScale, SKSizeI* dimensions);
		}
		private static Delegates.sk_codec_get_scaled_dimensions sk_codec_get_scaled_dimensions_delegate;
		internal static void sk_codec_get_scaled_dimensions (IntPtr codec, Single desiredScale, SKSizeI* dimensions) =>
			(sk_codec_get_scaled_dimensions_delegate ??= GetSymbol<Delegates.sk_codec_get_scaled_dimensions> ("sk_codec_get_scaled_dimensions")).Invoke (codec, desiredScale, dimensions);
		#endif

		// sk_codec_scanline_order_t sk_codec_get_scanline_order(sk_codec_t* codec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKCodecScanlineOrder sk_codec_get_scanline_order (IntPtr codec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecScanlineOrder sk_codec_get_scanline_order (IntPtr codec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKCodecScanlineOrder sk_codec_get_scanline_order (IntPtr codec);
		}
		private static Delegates.sk_codec_get_scanline_order sk_codec_get_scanline_order_delegate;
		internal static SKCodecScanlineOrder sk_codec_get_scanline_order (IntPtr codec) =>
			(sk_codec_get_scanline_order_delegate ??= GetSymbol<Delegates.sk_codec_get_scanline_order> ("sk_codec_get_scanline_order")).Invoke (codec);
		#endif

		// int sk_codec_get_scanlines(sk_codec_t* codec, void* dst, int countLines, size_t rowBytes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_codec_get_scanlines (IntPtr codec, void* dst, Int32 countLines, /* size_t */ IntPtr rowBytes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_get_scanlines (IntPtr codec, void* dst, Int32 countLines, /* size_t */ IntPtr rowBytes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_codec_get_scanlines (IntPtr codec, void* dst, Int32 countLines, /* size_t */ IntPtr rowBytes);
		}
		private static Delegates.sk_codec_get_scanlines sk_codec_get_scanlines_delegate;
		internal static Int32 sk_codec_get_scanlines (IntPtr codec, void* dst, Int32 countLines, /* size_t */ IntPtr rowBytes) =>
			(sk_codec_get_scanlines_delegate ??= GetSymbol<Delegates.sk_codec_get_scanlines> ("sk_codec_get_scanlines")).Invoke (codec, dst, countLines, rowBytes);
		#endif

		// bool sk_codec_get_valid_subset(sk_codec_t* codec, sk_irect_t* desiredSubset)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_codec_get_valid_subset (IntPtr codec, SKRectI* desiredSubset);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_codec_get_valid_subset (IntPtr codec, SKRectI* desiredSubset);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_codec_get_valid_subset (IntPtr codec, SKRectI* desiredSubset);
		}
		private static Delegates.sk_codec_get_valid_subset sk_codec_get_valid_subset_delegate;
		internal static bool sk_codec_get_valid_subset (IntPtr codec, SKRectI* desiredSubset) =>
			(sk_codec_get_valid_subset_delegate ??= GetSymbol<Delegates.sk_codec_get_valid_subset> ("sk_codec_get_valid_subset")).Invoke (codec, desiredSubset);
		#endif

		// sk_codec_result_t sk_codec_incremental_decode(sk_codec_t* codec, int* rowsDecoded)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKCodecResult sk_codec_incremental_decode (IntPtr codec, Int32* rowsDecoded);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecResult sk_codec_incremental_decode (IntPtr codec, Int32* rowsDecoded);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKCodecResult sk_codec_incremental_decode (IntPtr codec, Int32* rowsDecoded);
		}
		private static Delegates.sk_codec_incremental_decode sk_codec_incremental_decode_delegate;
		internal static SKCodecResult sk_codec_incremental_decode (IntPtr codec, Int32* rowsDecoded) =>
			(sk_codec_incremental_decode_delegate ??= GetSymbol<Delegates.sk_codec_incremental_decode> ("sk_codec_incremental_decode")).Invoke (codec, rowsDecoded);
		#endif

		// size_t sk_codec_min_buffered_bytes_needed()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_codec_min_buffered_bytes_needed ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_codec_min_buffered_bytes_needed ();
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_codec_new_from_data (IntPtr data);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_codec_new_from_data (IntPtr data);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_codec_new_from_data (IntPtr data);
		}
		private static Delegates.sk_codec_new_from_data sk_codec_new_from_data_delegate;
		internal static IntPtr sk_codec_new_from_data (IntPtr data) =>
			(sk_codec_new_from_data_delegate ??= GetSymbol<Delegates.sk_codec_new_from_data> ("sk_codec_new_from_data")).Invoke (data);
		#endif

		// sk_codec_t* sk_codec_new_from_stream(sk_stream_t* stream, sk_codec_result_t* result)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_codec_new_from_stream (IntPtr stream, SKCodecResult* result);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_codec_new_from_stream (IntPtr stream, SKCodecResult* result);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_codec_new_from_stream (IntPtr stream, SKCodecResult* result);
		}
		private static Delegates.sk_codec_new_from_stream sk_codec_new_from_stream_delegate;
		internal static IntPtr sk_codec_new_from_stream (IntPtr stream, SKCodecResult* result) =>
			(sk_codec_new_from_stream_delegate ??= GetSymbol<Delegates.sk_codec_new_from_stream> ("sk_codec_new_from_stream")).Invoke (stream, result);
		#endif

		// int sk_codec_next_scanline(sk_codec_t* codec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_codec_next_scanline (IntPtr codec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_next_scanline (IntPtr codec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_codec_next_scanline (IntPtr codec);
		}
		private static Delegates.sk_codec_next_scanline sk_codec_next_scanline_delegate;
		internal static Int32 sk_codec_next_scanline (IntPtr codec) =>
			(sk_codec_next_scanline_delegate ??= GetSymbol<Delegates.sk_codec_next_scanline> ("sk_codec_next_scanline")).Invoke (codec);
		#endif

		// int sk_codec_output_scanline(sk_codec_t* codec, int inputScanline)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_codec_output_scanline (IntPtr codec, Int32 inputScanline);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_codec_output_scanline (IntPtr codec, Int32 inputScanline);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_codec_output_scanline (IntPtr codec, Int32 inputScanline);
		}
		private static Delegates.sk_codec_output_scanline sk_codec_output_scanline_delegate;
		internal static Int32 sk_codec_output_scanline (IntPtr codec, Int32 inputScanline) =>
			(sk_codec_output_scanline_delegate ??= GetSymbol<Delegates.sk_codec_output_scanline> ("sk_codec_output_scanline")).Invoke (codec, inputScanline);
		#endif

		// bool sk_codec_skip_scanlines(sk_codec_t* codec, int countLines)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_codec_skip_scanlines (IntPtr codec, Int32 countLines);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_codec_skip_scanlines (IntPtr codec, Int32 countLines);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_codec_skip_scanlines (IntPtr codec, Int32 countLines);
		}
		private static Delegates.sk_codec_skip_scanlines sk_codec_skip_scanlines_delegate;
		internal static bool sk_codec_skip_scanlines (IntPtr codec, Int32 countLines) =>
			(sk_codec_skip_scanlines_delegate ??= GetSymbol<Delegates.sk_codec_skip_scanlines> ("sk_codec_skip_scanlines")).Invoke (codec, countLines);
		#endif

		// sk_codec_result_t sk_codec_start_incremental_decode(sk_codec_t* codec, const sk_imageinfo_t* info, void* pixels, size_t rowBytes, const sk_codec_options_t* options)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKCodecResult sk_codec_start_incremental_decode (IntPtr codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecResult sk_codec_start_incremental_decode (IntPtr codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKCodecResult sk_codec_start_incremental_decode (IntPtr codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options);
		}
		private static Delegates.sk_codec_start_incremental_decode sk_codec_start_incremental_decode_delegate;
		internal static SKCodecResult sk_codec_start_incremental_decode (IntPtr codec, SKImageInfoNative* info, void* pixels, /* size_t */ IntPtr rowBytes, SKCodecOptionsInternal* options) =>
			(sk_codec_start_incremental_decode_delegate ??= GetSymbol<Delegates.sk_codec_start_incremental_decode> ("sk_codec_start_incremental_decode")).Invoke (codec, info, pixels, rowBytes, options);
		#endif

		// sk_codec_result_t sk_codec_start_scanline_decode(sk_codec_t* codec, const sk_imageinfo_t* info, const sk_codec_options_t* options)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKCodecResult sk_codec_start_scanline_decode (IntPtr codec, SKImageInfoNative* info, SKCodecOptionsInternal* options);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKCodecResult sk_codec_start_scanline_decode (IntPtr codec, SKImageInfoNative* info, SKCodecOptionsInternal* options);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKCodecResult sk_codec_start_scanline_decode (IntPtr codec, SKImageInfoNative* info, SKCodecOptionsInternal* options);
		}
		private static Delegates.sk_codec_start_scanline_decode sk_codec_start_scanline_decode_delegate;
		internal static SKCodecResult sk_codec_start_scanline_decode (IntPtr codec, SKImageInfoNative* info, SKCodecOptionsInternal* options) =>
			(sk_codec_start_scanline_decode_delegate ??= GetSymbol<Delegates.sk_codec_start_scanline_decode> ("sk_codec_start_scanline_decode")).Invoke (codec, info, options);
		#endif

		#endregion

	}
}

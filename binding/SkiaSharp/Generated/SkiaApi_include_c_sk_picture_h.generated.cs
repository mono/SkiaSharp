using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_picture.h

		// size_t sk_picture_approximate_bytes_used(const sk_picture_t* picture)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_picture_approximate_bytes_used (sk_picture_t picture);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_picture_approximate_bytes_used (sk_picture_t picture);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_picture_approximate_bytes_used (sk_picture_t picture);
		}
		private static Delegates.sk_picture_approximate_bytes_used sk_picture_approximate_bytes_used_delegate;
		internal static /* size_t */ IntPtr sk_picture_approximate_bytes_used (sk_picture_t picture) =>
			(sk_picture_approximate_bytes_used_delegate ??= GetSymbol<Delegates.sk_picture_approximate_bytes_used> ("sk_picture_approximate_bytes_used")).Invoke (picture);
		#endif

		// int sk_picture_approximate_op_count(const sk_picture_t* picture, bool nested)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_picture_approximate_op_count (sk_picture_t picture, [MarshalAs (UnmanagedType.I1)] bool nested);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_picture_approximate_op_count (sk_picture_t picture, [MarshalAs (UnmanagedType.I1)] bool nested);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_picture_approximate_op_count (sk_picture_t picture, [MarshalAs (UnmanagedType.I1)] bool nested);
		}
		private static Delegates.sk_picture_approximate_op_count sk_picture_approximate_op_count_delegate;
		internal static Int32 sk_picture_approximate_op_count (sk_picture_t picture, [MarshalAs (UnmanagedType.I1)] bool nested) =>
			(sk_picture_approximate_op_count_delegate ??= GetSymbol<Delegates.sk_picture_approximate_op_count> ("sk_picture_approximate_op_count")).Invoke (picture, nested);
		#endif

		// sk_picture_t* sk_picture_deserialize_from_data(sk_data_t* data)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_picture_t sk_picture_deserialize_from_data (sk_data_t data);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_picture_t sk_picture_deserialize_from_data (sk_data_t data);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_picture_t sk_picture_deserialize_from_memory (void* buffer, /* size_t */ IntPtr length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_picture_t sk_picture_deserialize_from_memory (void* buffer, /* size_t */ IntPtr length);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_picture_t sk_picture_deserialize_from_stream (sk_stream_t stream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_picture_t sk_picture_deserialize_from_stream (sk_stream_t stream);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_picture_get_cull_rect (sk_picture_t param0, SKRect* param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_get_cull_rect (sk_picture_t param0, SKRect* param1);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_canvas_t sk_picture_get_recording_canvas (sk_picture_recorder_t crec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_picture_get_recording_canvas (sk_picture_recorder_t crec);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt32 sk_picture_get_unique_id (sk_picture_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_picture_get_unique_id (sk_picture_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_picture_get_unique_id (sk_picture_t param0);
		}
		private static Delegates.sk_picture_get_unique_id sk_picture_get_unique_id_delegate;
		internal static UInt32 sk_picture_get_unique_id (sk_picture_t param0) =>
			(sk_picture_get_unique_id_delegate ??= GetSymbol<Delegates.sk_picture_get_unique_id> ("sk_picture_get_unique_id")).Invoke (param0);
		#endif

		// sk_shader_t* sk_picture_make_shader(sk_picture_t* src, sk_shader_tilemode_t tmx, sk_shader_tilemode_t tmy, sk_filter_mode_t mode, const sk_matrix_t* localMatrix, const sk_rect_t* tile)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_shader_t sk_picture_make_shader (sk_picture_t src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode mode, SKMatrix* localMatrix, SKRect* tile);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_picture_make_shader (sk_picture_t src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode mode, SKMatrix* localMatrix, SKRect* tile);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_picture_make_shader (sk_picture_t src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode mode, SKMatrix* localMatrix, SKRect* tile);
		}
		private static Delegates.sk_picture_make_shader sk_picture_make_shader_delegate;
		internal static sk_shader_t sk_picture_make_shader (sk_picture_t src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode mode, SKMatrix* localMatrix, SKRect* tile) =>
			(sk_picture_make_shader_delegate ??= GetSymbol<Delegates.sk_picture_make_shader> ("sk_picture_make_shader")).Invoke (src, tmx, tmy, mode, localMatrix, tile);
		#endif

		// void sk_picture_playback(const sk_picture_t* picture, sk_canvas_t* canvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_picture_playback (sk_picture_t picture, sk_canvas_t canvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_playback (sk_picture_t picture, sk_canvas_t canvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_picture_playback (sk_picture_t picture, sk_canvas_t canvas);
		}
		private static Delegates.sk_picture_playback sk_picture_playback_delegate;
		internal static void sk_picture_playback (sk_picture_t picture, sk_canvas_t canvas) =>
			(sk_picture_playback_delegate ??= GetSymbol<Delegates.sk_picture_playback> ("sk_picture_playback")).Invoke (picture, canvas);
		#endif

		// sk_canvas_t* sk_picture_recorder_begin_recording(sk_picture_recorder_t*, const sk_rect_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_canvas_t sk_picture_recorder_begin_recording (sk_picture_recorder_t param0, SKRect* param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_picture_recorder_begin_recording (sk_picture_recorder_t param0, SKRect* param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_canvas_t sk_picture_recorder_begin_recording (sk_picture_recorder_t param0, SKRect* param1);
		}
		private static Delegates.sk_picture_recorder_begin_recording sk_picture_recorder_begin_recording_delegate;
		internal static sk_canvas_t sk_picture_recorder_begin_recording (sk_picture_recorder_t param0, SKRect* param1) =>
			(sk_picture_recorder_begin_recording_delegate ??= GetSymbol<Delegates.sk_picture_recorder_begin_recording> ("sk_picture_recorder_begin_recording")).Invoke (param0, param1);
		#endif

		// sk_canvas_t* sk_picture_recorder_begin_recording_with_bbh_factory(sk_picture_recorder_t*, const sk_rect_t*, sk_bbh_factory_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_canvas_t sk_picture_recorder_begin_recording_with_bbh_factory (sk_picture_recorder_t param0, SKRect* param1, sk_bbh_factory_t param2);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_picture_recorder_begin_recording_with_bbh_factory (sk_picture_recorder_t param0, SKRect* param1, sk_bbh_factory_t param2);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_canvas_t sk_picture_recorder_begin_recording_with_bbh_factory (sk_picture_recorder_t param0, SKRect* param1, sk_bbh_factory_t param2);
		}
		private static Delegates.sk_picture_recorder_begin_recording_with_bbh_factory sk_picture_recorder_begin_recording_with_bbh_factory_delegate;
		internal static sk_canvas_t sk_picture_recorder_begin_recording_with_bbh_factory (sk_picture_recorder_t param0, SKRect* param1, sk_bbh_factory_t param2) =>
			(sk_picture_recorder_begin_recording_with_bbh_factory_delegate ??= GetSymbol<Delegates.sk_picture_recorder_begin_recording_with_bbh_factory> ("sk_picture_recorder_begin_recording_with_bbh_factory")).Invoke (param0, param1, param2);
		#endif

		// void sk_picture_recorder_delete(sk_picture_recorder_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_picture_recorder_delete (sk_picture_recorder_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_recorder_delete (sk_picture_recorder_t param0);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_picture_t sk_picture_recorder_end_recording (sk_picture_recorder_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_picture_t sk_picture_recorder_end_recording (sk_picture_recorder_t param0);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_drawable_t sk_picture_recorder_end_recording_as_drawable (sk_picture_recorder_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_drawable_t sk_picture_recorder_end_recording_as_drawable (sk_picture_recorder_t param0);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_picture_recorder_t sk_picture_recorder_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_picture_recorder_t sk_picture_recorder_new ();
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_picture_ref (sk_picture_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_ref (sk_picture_t param0);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_data_t sk_picture_serialize_to_data (sk_picture_t picture);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_picture_serialize_to_data (sk_picture_t picture);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_picture_serialize_to_stream (sk_picture_t picture, sk_wstream_t stream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_serialize_to_stream (sk_picture_t picture, sk_wstream_t stream);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_picture_unref (sk_picture_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_picture_unref (sk_picture_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_picture_unref (sk_picture_t param0);
		}
		private static Delegates.sk_picture_unref sk_picture_unref_delegate;
		internal static void sk_picture_unref (sk_picture_t param0) =>
			(sk_picture_unref_delegate ??= GetSymbol<Delegates.sk_picture_unref> ("sk_picture_unref")).Invoke (param0);
		#endif

		// void sk_rtree_factory_delete(sk_rtree_factory_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_rtree_factory_delete (sk_rtree_factory_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_rtree_factory_delete (sk_rtree_factory_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_rtree_factory_delete (sk_rtree_factory_t param0);
		}
		private static Delegates.sk_rtree_factory_delete sk_rtree_factory_delete_delegate;
		internal static void sk_rtree_factory_delete (sk_rtree_factory_t param0) =>
			(sk_rtree_factory_delete_delegate ??= GetSymbol<Delegates.sk_rtree_factory_delete> ("sk_rtree_factory_delete")).Invoke (param0);
		#endif

		// sk_rtree_factory_t* sk_rtree_factory_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_rtree_factory_t sk_rtree_factory_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_rtree_factory_t sk_rtree_factory_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_rtree_factory_t sk_rtree_factory_new ();
		}
		private static Delegates.sk_rtree_factory_new sk_rtree_factory_new_delegate;
		internal static sk_rtree_factory_t sk_rtree_factory_new () =>
			(sk_rtree_factory_new_delegate ??= GetSymbol<Delegates.sk_rtree_factory_new> ("sk_rtree_factory_new")).Invoke ();
		#endif

		#endregion

	}
}

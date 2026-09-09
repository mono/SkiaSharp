using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces

using SkiaSharp.Skottie;

#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkottieApi
	{
		#region skottie_animation.h

		// void skottie_animation_builder_delete(skottie_animation_builder_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_builder_delete (IntPtr instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_builder_delete (IntPtr instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_builder_delete (IntPtr instance);
		}
		private static Delegates.skottie_animation_builder_delete skottie_animation_builder_delete_delegate;
		internal static void skottie_animation_builder_delete (IntPtr instance) =>
			(skottie_animation_builder_delete_delegate ??= GetSymbol<Delegates.skottie_animation_builder_delete> ("skottie_animation_builder_delete")).Invoke (instance);
		#endif

		// void skottie_animation_builder_get_stats(skottie_animation_builder_t* instance, skottie_animation_builder_stats_t* stats)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_builder_get_stats (IntPtr instance, AnimationBuilderStats* stats);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_builder_get_stats (IntPtr instance, AnimationBuilderStats* stats);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_builder_get_stats (IntPtr instance, AnimationBuilderStats* stats);
		}
		private static Delegates.skottie_animation_builder_get_stats skottie_animation_builder_get_stats_delegate;
		internal static void skottie_animation_builder_get_stats (IntPtr instance, AnimationBuilderStats* stats) =>
			(skottie_animation_builder_get_stats_delegate ??= GetSymbol<Delegates.skottie_animation_builder_get_stats> ("skottie_animation_builder_get_stats")).Invoke (instance, stats);
		#endif

		// skottie_animation_t* skottie_animation_builder_make_from_data(skottie_animation_builder_t* instance, const char* data, size_t length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr skottie_animation_builder_make_from_data (IntPtr instance, /* char */ void* data, /* size_t */ IntPtr length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr skottie_animation_builder_make_from_data (IntPtr instance, /* char */ void* data, /* size_t */ IntPtr length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr skottie_animation_builder_make_from_data (IntPtr instance, /* char */ void* data, /* size_t */ IntPtr length);
		}
		private static Delegates.skottie_animation_builder_make_from_data skottie_animation_builder_make_from_data_delegate;
		internal static IntPtr skottie_animation_builder_make_from_data (IntPtr instance, /* char */ void* data, /* size_t */ IntPtr length) =>
			(skottie_animation_builder_make_from_data_delegate ??= GetSymbol<Delegates.skottie_animation_builder_make_from_data> ("skottie_animation_builder_make_from_data")).Invoke (instance, data, length);
		#endif

		// skottie_animation_t* skottie_animation_builder_make_from_file(skottie_animation_builder_t* instance, const char* path)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr skottie_animation_builder_make_from_file (IntPtr instance, /* char */ void* path);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr skottie_animation_builder_make_from_file (IntPtr instance, /* char */ void* path);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr skottie_animation_builder_make_from_file (IntPtr instance, /* char */ void* path);
		}
		private static Delegates.skottie_animation_builder_make_from_file skottie_animation_builder_make_from_file_delegate;
		internal static IntPtr skottie_animation_builder_make_from_file (IntPtr instance, /* char */ void* path) =>
			(skottie_animation_builder_make_from_file_delegate ??= GetSymbol<Delegates.skottie_animation_builder_make_from_file> ("skottie_animation_builder_make_from_file")).Invoke (instance, path);
		#endif

		// skottie_animation_t* skottie_animation_builder_make_from_stream(skottie_animation_builder_t* instance, sk_stream_t* stream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr skottie_animation_builder_make_from_stream (IntPtr instance, IntPtr stream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr skottie_animation_builder_make_from_stream (IntPtr instance, IntPtr stream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr skottie_animation_builder_make_from_stream (IntPtr instance, IntPtr stream);
		}
		private static Delegates.skottie_animation_builder_make_from_stream skottie_animation_builder_make_from_stream_delegate;
		internal static IntPtr skottie_animation_builder_make_from_stream (IntPtr instance, IntPtr stream) =>
			(skottie_animation_builder_make_from_stream_delegate ??= GetSymbol<Delegates.skottie_animation_builder_make_from_stream> ("skottie_animation_builder_make_from_stream")).Invoke (instance, stream);
		#endif

		// skottie_animation_t* skottie_animation_builder_make_from_string(skottie_animation_builder_t* instance, const char* data, size_t length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr skottie_animation_builder_make_from_string (IntPtr instance, /* char */ void* data, /* size_t */ IntPtr length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr skottie_animation_builder_make_from_string (IntPtr instance, /* char */ void* data, /* size_t */ IntPtr length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr skottie_animation_builder_make_from_string (IntPtr instance, /* char */ void* data, /* size_t */ IntPtr length);
		}
		private static Delegates.skottie_animation_builder_make_from_string skottie_animation_builder_make_from_string_delegate;
		internal static IntPtr skottie_animation_builder_make_from_string (IntPtr instance, /* char */ void* data, /* size_t */ IntPtr length) =>
			(skottie_animation_builder_make_from_string_delegate ??= GetSymbol<Delegates.skottie_animation_builder_make_from_string> ("skottie_animation_builder_make_from_string")).Invoke (instance, data, length);
		#endif

		// skottie_animation_builder_t* skottie_animation_builder_new(skottie_animation_builder_flags_t flags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr skottie_animation_builder_new (AnimationBuilderFlags flags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr skottie_animation_builder_new (AnimationBuilderFlags flags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr skottie_animation_builder_new (AnimationBuilderFlags flags);
		}
		private static Delegates.skottie_animation_builder_new skottie_animation_builder_new_delegate;
		internal static IntPtr skottie_animation_builder_new (AnimationBuilderFlags flags) =>
			(skottie_animation_builder_new_delegate ??= GetSymbol<Delegates.skottie_animation_builder_new> ("skottie_animation_builder_new")).Invoke (flags);
		#endif

		// void skottie_animation_builder_set_font_manager(skottie_animation_builder_t* instance, sk_fontmgr_t* fontManager)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_builder_set_font_manager (IntPtr instance, IntPtr fontManager);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_builder_set_font_manager (IntPtr instance, IntPtr fontManager);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_builder_set_font_manager (IntPtr instance, IntPtr fontManager);
		}
		private static Delegates.skottie_animation_builder_set_font_manager skottie_animation_builder_set_font_manager_delegate;
		internal static void skottie_animation_builder_set_font_manager (IntPtr instance, IntPtr fontManager) =>
			(skottie_animation_builder_set_font_manager_delegate ??= GetSymbol<Delegates.skottie_animation_builder_set_font_manager> ("skottie_animation_builder_set_font_manager")).Invoke (instance, fontManager);
		#endif

		// void skottie_animation_builder_set_resource_provider(skottie_animation_builder_t* instance, skottie_resource_provider_t* resourceProvider)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_builder_set_resource_provider (IntPtr instance, IntPtr resourceProvider);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_builder_set_resource_provider (IntPtr instance, IntPtr resourceProvider);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_builder_set_resource_provider (IntPtr instance, IntPtr resourceProvider);
		}
		private static Delegates.skottie_animation_builder_set_resource_provider skottie_animation_builder_set_resource_provider_delegate;
		internal static void skottie_animation_builder_set_resource_provider (IntPtr instance, IntPtr resourceProvider) =>
			(skottie_animation_builder_set_resource_provider_delegate ??= GetSymbol<Delegates.skottie_animation_builder_set_resource_provider> ("skottie_animation_builder_set_resource_provider")).Invoke (instance, resourceProvider);
		#endif

		// void skottie_animation_delete(skottie_animation_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_delete (IntPtr instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_delete (IntPtr instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_delete (IntPtr instance);
		}
		private static Delegates.skottie_animation_delete skottie_animation_delete_delegate;
		internal static void skottie_animation_delete (IntPtr instance) =>
			(skottie_animation_delete_delegate ??= GetSymbol<Delegates.skottie_animation_delete> ("skottie_animation_delete")).Invoke (instance);
		#endif

		// double skottie_animation_get_duration(skottie_animation_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Double skottie_animation_get_duration (IntPtr instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Double skottie_animation_get_duration (IntPtr instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Double skottie_animation_get_duration (IntPtr instance);
		}
		private static Delegates.skottie_animation_get_duration skottie_animation_get_duration_delegate;
		internal static Double skottie_animation_get_duration (IntPtr instance) =>
			(skottie_animation_get_duration_delegate ??= GetSymbol<Delegates.skottie_animation_get_duration> ("skottie_animation_get_duration")).Invoke (instance);
		#endif

		// double skottie_animation_get_fps(skottie_animation_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Double skottie_animation_get_fps (IntPtr instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Double skottie_animation_get_fps (IntPtr instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Double skottie_animation_get_fps (IntPtr instance);
		}
		private static Delegates.skottie_animation_get_fps skottie_animation_get_fps_delegate;
		internal static Double skottie_animation_get_fps (IntPtr instance) =>
			(skottie_animation_get_fps_delegate ??= GetSymbol<Delegates.skottie_animation_get_fps> ("skottie_animation_get_fps")).Invoke (instance);
		#endif

		// double skottie_animation_get_in_point(skottie_animation_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Double skottie_animation_get_in_point (IntPtr instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Double skottie_animation_get_in_point (IntPtr instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Double skottie_animation_get_in_point (IntPtr instance);
		}
		private static Delegates.skottie_animation_get_in_point skottie_animation_get_in_point_delegate;
		internal static Double skottie_animation_get_in_point (IntPtr instance) =>
			(skottie_animation_get_in_point_delegate ??= GetSymbol<Delegates.skottie_animation_get_in_point> ("skottie_animation_get_in_point")).Invoke (instance);
		#endif

		// double skottie_animation_get_out_point(skottie_animation_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Double skottie_animation_get_out_point (IntPtr instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Double skottie_animation_get_out_point (IntPtr instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Double skottie_animation_get_out_point (IntPtr instance);
		}
		private static Delegates.skottie_animation_get_out_point skottie_animation_get_out_point_delegate;
		internal static Double skottie_animation_get_out_point (IntPtr instance) =>
			(skottie_animation_get_out_point_delegate ??= GetSymbol<Delegates.skottie_animation_get_out_point> ("skottie_animation_get_out_point")).Invoke (instance);
		#endif

		// void skottie_animation_get_size(skottie_animation_t* instance, sk_size_t* size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_get_size (IntPtr instance, SKSize* size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_get_size (IntPtr instance, SKSize* size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_get_size (IntPtr instance, SKSize* size);
		}
		private static Delegates.skottie_animation_get_size skottie_animation_get_size_delegate;
		internal static void skottie_animation_get_size (IntPtr instance, SKSize* size) =>
			(skottie_animation_get_size_delegate ??= GetSymbol<Delegates.skottie_animation_get_size> ("skottie_animation_get_size")).Invoke (instance, size);
		#endif

		// void skottie_animation_get_version(skottie_animation_t* instance, sk_string_t* version)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_get_version (IntPtr instance, IntPtr version);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_get_version (IntPtr instance, IntPtr version);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_get_version (IntPtr instance, IntPtr version);
		}
		private static Delegates.skottie_animation_get_version skottie_animation_get_version_delegate;
		internal static void skottie_animation_get_version (IntPtr instance, IntPtr version) =>
			(skottie_animation_get_version_delegate ??= GetSymbol<Delegates.skottie_animation_get_version> ("skottie_animation_get_version")).Invoke (instance, version);
		#endif

		// skottie_animation_t* skottie_animation_make_from_data(const char* data, size_t length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr skottie_animation_make_from_data (/* char */ void* data, /* size_t */ IntPtr length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr skottie_animation_make_from_data (/* char */ void* data, /* size_t */ IntPtr length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr skottie_animation_make_from_data (/* char */ void* data, /* size_t */ IntPtr length);
		}
		private static Delegates.skottie_animation_make_from_data skottie_animation_make_from_data_delegate;
		internal static IntPtr skottie_animation_make_from_data (/* char */ void* data, /* size_t */ IntPtr length) =>
			(skottie_animation_make_from_data_delegate ??= GetSymbol<Delegates.skottie_animation_make_from_data> ("skottie_animation_make_from_data")).Invoke (data, length);
		#endif

		// skottie_animation_t* skottie_animation_make_from_file(const char* path)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr skottie_animation_make_from_file ([MarshalAs (UnmanagedType.LPStr)] String path);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr skottie_animation_make_from_file ([MarshalAs (UnmanagedType.LPStr)] String path);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr skottie_animation_make_from_file ([MarshalAs (UnmanagedType.LPStr)] String path);
		}
		private static Delegates.skottie_animation_make_from_file skottie_animation_make_from_file_delegate;
		internal static IntPtr skottie_animation_make_from_file ([MarshalAs (UnmanagedType.LPStr)] String path) =>
			(skottie_animation_make_from_file_delegate ??= GetSymbol<Delegates.skottie_animation_make_from_file> ("skottie_animation_make_from_file")).Invoke (path);
		#endif

		// skottie_animation_t* skottie_animation_make_from_stream(sk_stream_t* stream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr skottie_animation_make_from_stream (IntPtr stream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr skottie_animation_make_from_stream (IntPtr stream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr skottie_animation_make_from_stream (IntPtr stream);
		}
		private static Delegates.skottie_animation_make_from_stream skottie_animation_make_from_stream_delegate;
		internal static IntPtr skottie_animation_make_from_stream (IntPtr stream) =>
			(skottie_animation_make_from_stream_delegate ??= GetSymbol<Delegates.skottie_animation_make_from_stream> ("skottie_animation_make_from_stream")).Invoke (stream);
		#endif

		// skottie_animation_t* skottie_animation_make_from_string(const char* data, size_t length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr skottie_animation_make_from_string ([MarshalAs (UnmanagedType.LPStr)] String data, int length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr skottie_animation_make_from_string ([MarshalAs (UnmanagedType.LPStr)] String data, int length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr skottie_animation_make_from_string ([MarshalAs (UnmanagedType.LPStr)] String data, int length);
		}
		private static Delegates.skottie_animation_make_from_string skottie_animation_make_from_string_delegate;
		internal static IntPtr skottie_animation_make_from_string ([MarshalAs (UnmanagedType.LPStr)] String data, int length) =>
			(skottie_animation_make_from_string_delegate ??= GetSymbol<Delegates.skottie_animation_make_from_string> ("skottie_animation_make_from_string")).Invoke (data, length);
		#endif

		// void skottie_animation_ref(skottie_animation_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_ref (IntPtr instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_ref (IntPtr instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_ref (IntPtr instance);
		}
		private static Delegates.skottie_animation_ref skottie_animation_ref_delegate;
		internal static void skottie_animation_ref (IntPtr instance) =>
			(skottie_animation_ref_delegate ??= GetSymbol<Delegates.skottie_animation_ref> ("skottie_animation_ref")).Invoke (instance);
		#endif

		// void skottie_animation_render(skottie_animation_t* instance, sk_canvas_t* canvas, sk_rect_t* dst)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_render (IntPtr instance, IntPtr canvas, SKRect* dst);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_render (IntPtr instance, IntPtr canvas, SKRect* dst);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_render (IntPtr instance, IntPtr canvas, SKRect* dst);
		}
		private static Delegates.skottie_animation_render skottie_animation_render_delegate;
		internal static void skottie_animation_render (IntPtr instance, IntPtr canvas, SKRect* dst) =>
			(skottie_animation_render_delegate ??= GetSymbol<Delegates.skottie_animation_render> ("skottie_animation_render")).Invoke (instance, canvas, dst);
		#endif

		// void skottie_animation_render_with_flags(skottie_animation_t* instance, sk_canvas_t* canvas, sk_rect_t* dst, skottie_animation_renderflags_t flags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_render_with_flags (IntPtr instance, IntPtr canvas, SKRect* dst, AnimationRenderFlags flags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_render_with_flags (IntPtr instance, IntPtr canvas, SKRect* dst, AnimationRenderFlags flags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_render_with_flags (IntPtr instance, IntPtr canvas, SKRect* dst, AnimationRenderFlags flags);
		}
		private static Delegates.skottie_animation_render_with_flags skottie_animation_render_with_flags_delegate;
		internal static void skottie_animation_render_with_flags (IntPtr instance, IntPtr canvas, SKRect* dst, AnimationRenderFlags flags) =>
			(skottie_animation_render_with_flags_delegate ??= GetSymbol<Delegates.skottie_animation_render_with_flags> ("skottie_animation_render_with_flags")).Invoke (instance, canvas, dst, flags);
		#endif

		// void skottie_animation_seek(skottie_animation_t* instance, float t, sksg_invalidation_controller_t* ic)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_seek (IntPtr instance, Single t, IntPtr ic);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_seek (IntPtr instance, Single t, IntPtr ic);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_seek (IntPtr instance, Single t, IntPtr ic);
		}
		private static Delegates.skottie_animation_seek skottie_animation_seek_delegate;
		internal static void skottie_animation_seek (IntPtr instance, Single t, IntPtr ic) =>
			(skottie_animation_seek_delegate ??= GetSymbol<Delegates.skottie_animation_seek> ("skottie_animation_seek")).Invoke (instance, t, ic);
		#endif

		// void skottie_animation_seek_frame(skottie_animation_t* instance, float t, sksg_invalidation_controller_t* ic)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_seek_frame (IntPtr instance, Single t, IntPtr ic);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_seek_frame (IntPtr instance, Single t, IntPtr ic);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_seek_frame (IntPtr instance, Single t, IntPtr ic);
		}
		private static Delegates.skottie_animation_seek_frame skottie_animation_seek_frame_delegate;
		internal static void skottie_animation_seek_frame (IntPtr instance, Single t, IntPtr ic) =>
			(skottie_animation_seek_frame_delegate ??= GetSymbol<Delegates.skottie_animation_seek_frame> ("skottie_animation_seek_frame")).Invoke (instance, t, ic);
		#endif

		// void skottie_animation_seek_frame_time(skottie_animation_t* instance, float t, sksg_invalidation_controller_t* ic)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_seek_frame_time (IntPtr instance, Single t, IntPtr ic);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_seek_frame_time (IntPtr instance, Single t, IntPtr ic);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_seek_frame_time (IntPtr instance, Single t, IntPtr ic);
		}
		private static Delegates.skottie_animation_seek_frame_time skottie_animation_seek_frame_time_delegate;
		internal static void skottie_animation_seek_frame_time (IntPtr instance, Single t, IntPtr ic) =>
			(skottie_animation_seek_frame_time_delegate ??= GetSymbol<Delegates.skottie_animation_seek_frame_time> ("skottie_animation_seek_frame_time")).Invoke (instance, t, ic);
		#endif

		// void skottie_animation_unref(skottie_animation_t* instance)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void skottie_animation_unref (IntPtr instance);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void skottie_animation_unref (IntPtr instance);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void skottie_animation_unref (IntPtr instance);
		}
		private static Delegates.skottie_animation_unref skottie_animation_unref_delegate;
		internal static void skottie_animation_unref (IntPtr instance) =>
			(skottie_animation_unref_delegate ??= GetSymbol<Delegates.skottie_animation_unref> ("skottie_animation_unref")).Invoke (instance);
		#endif

		#endregion

	}
}

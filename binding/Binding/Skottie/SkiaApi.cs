using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	#region Class declarations

	using sk_canvas_t = IntPtr;
	using sk_stream_filestream_t = IntPtr;
	using sk_string_t = IntPtr;

	#endregion

	using skottie_animation_t = IntPtr;
	using sksg_invalidation_controller_t = IntPtr;

	internal partial class SkiaApi
	{
		//
		// Animation
		//

		// SK_C_API skottie_animation_t* skottie_animation_make_from_string(const char* data, size_t length);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_t skottie_animation_make_from_string (string data, int length);

		// SK_C_API skottie_animation_t* skottie_animation_make_from_stream(sk_stream_filestream_t* stream);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_t skottie_animation_make_from_stream (sk_stream_filestream_t fileStream);

		// SK_C_API skottie_animation_t* skottie_animation_make_from_file(const char* path);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern skottie_animation_t skottie_animation_make_from_file (string path);

		// SK_C_API void skottie_animation_render(skottie_animation_t *instance, sk_canvas_t *canvas, sk_rect_t *dst);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern void skottie_animation_render (skottie_animation_t instance, sk_canvas_t canvas, SKRect* dst);

		// SK_C_API void skottie_animation_render_with_flags(skottie_animation_t *instance, sk_canvas_t *canvas, sk_rect_t *dst, skottie_animation_renderflags_t flags);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern void skottie_animation_render_with_flags (skottie_animation_t instance, sk_canvas_t canvas, SKRect* dst, Skottie.Animation.RenderFlags flags);

		// SK_C_API void skottie_animation_seek(skottie_animation_t *instance, SkScalar t, sksg_invalidation_controller_t *ic);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern void skottie_animation_seek (skottie_animation_t instance, float t, sksg_invalidation_controller_t ic);

		// SK_C_API void skottie_animation_seek_frame(skottie_animation_t *instance, double t, sksg_invalidation_controller_t *ic);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern void skottie_animation_seek_frame (skottie_animation_t instance, float t, sksg_invalidation_controller_t ic);

		// SK_C_API void skottie_animation_seek_frame_time(skottie_animation_t *instance, double t, sksg_invalidation_controller_t *ic);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern void skottie_animation_seek_frame_time (skottie_animation_t instance, float t, sksg_invalidation_controller_t ic);

		// SK_C_API double skottie_animation_get_duration(skottie_animation_t *instance);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern double skottie_animation_get_duration (skottie_animation_t instance);

		// SK_C_API double skottie_animation_get_fps(skottie_animation_t *instance);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern double skottie_animation_get_fps (skottie_animation_t instance);

		// SK_C_API double skottie_animation_get_in_point(skottie_animation_t *instance);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern double skottie_animation_get_in_point (skottie_animation_t instance);

		// SK_C_API double skottie_animation_get_out_point(skottie_animation_t *instance);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern double skottie_animation_get_out_point (skottie_animation_t instance);

		// SK_C_API sk_string_t* skottie_animation_get_version(skottie_animation_t *instance);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern void skottie_animation_get_version (skottie_animation_t instance, sk_string_t version);

		// SK_C_API sk_size_t skottie_animation_get_size(skottie_animation_t *instance);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern void skottie_animation_get_size (skottie_animation_t instance, SKSize* size);

		//
		// InvalidationController
		//

		// SK_C_API sksg_invalidation_controller_t* sksg_invalidation_controller_new ();
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern sksg_invalidation_controller_t sksg_invalidation_controller_new ();

		// SK_C_API void sksg_invalidation_controller_inval (sksg_invalidation_controller_t* instance, sk_rect_t* rect, sk_matrix_t* matrix);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern void sksg_invalidation_controller_inval (sksg_invalidation_controller_t instance, SKRect* rect, SKMatrix* matrix);

		// SK_C_API void sksg_invalidation_controller_bounds (sksg_invalidation_controller_t* instance, sk_rect_t* bounds);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern void sksg_invalidation_controller_get_bounds (sksg_invalidation_controller_t instance, SKRect* bounds);

		// SK_C_API void sksg_invalidation_controller_begin (sksg_invalidation_controller_t* instance);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern void sksg_invalidation_controller_begin (sksg_invalidation_controller_t instance);

		// SK_C_API void sksg_invalidation_controller_end (sksg_invalidation_controller_t* instance);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern void sksg_invalidation_controller_end (sksg_invalidation_controller_t instance);

		// SK_C_API void sksg_invalidation_controller_reset (sksg_invalidation_controller_t* instance);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal unsafe static extern void sksg_invalidation_controller_reset (sksg_invalidation_controller_t instance);

	}
}

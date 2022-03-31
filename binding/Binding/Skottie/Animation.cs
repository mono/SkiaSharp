using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp.Skottie
{
	public unsafe class Animation : SKObject
	{
		public enum RenderFlags
		{
			SkipTopLevelIsolation = 0x01,
			DisableTopLevelClipping = 0x02,
		}

		internal Animation (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		// SK_C_API skottie_animation_t* skottie_animation_make_from_string(const char* data, size_t length);
		public static Animation Make (string data)
			=> GetObject (SkiaApi.skottie_animation_make_from_string (data, data.Length));

		// SK_C_API skottie_animation_t* skottie_animation_make_from_stream(sk_stream_filestream_t* stream);
		public static Animation Make (SKStream stream)
			=> GetObject (SkiaApi.skottie_animation_make_from_stream (stream.Handle));

		// SK_C_API skottie_animation_t* skottie_animation_make_from_file(const char* path);
		public static Animation MakeFromFile(string path)
			=> GetObject (SkiaApi.skottie_animation_make_from_file (path));

		//SK_C_API void skottie_animation_render(skottie_animation_t *instance, sk_canvas_t *canvas, sk_rect_t *dst);
		public unsafe void Render(SKCanvas canvas, SKRect dst)
			=> SkiaApi.skottie_animation_render (Handle, canvas.Handle, &dst);

		//SK_C_API void skottie_animation_render_with_flags(skottie_animation_t *instance, sk_canvas_t *canvas, sk_rect_t *dst, skottie_animation_renderflags_t flags);
		public void Render (SKCanvas canvas, SKRect dst, RenderFlags flags)
			=> SkiaApi.skottie_animation_render_with_flags (Handle, canvas.Handle, &dst, flags);

		//SK_C_API void skottie_animation_seek(skottie_animation_t *instance, SkScalar t, sksg_invalidation_controller_t *ic);
		public void Seek (float t, InvalidationController ic)			=> SkiaApi.skottie_animation_seek (Handle, t, ic?.Handle ?? IntPtr.Zero);

		//SK_C_API void skottie_animation_seek_frame(skottie_animation_t *instance, double t, sksg_invalidation_controller_t *ic);
		public void SeekFrame(float t, InvalidationController ic)
			=> SkiaApi.skottie_animation_seek_frame (Handle, t, ic.Handle);

		//SK_C_API void skottie_animation_seek_frame_time(skottie_animation_t *instance, double t, sksg_invalidation_controller_t *ic);
		public void SeekFrameTime(float t, InvalidationController ic)
			=> SkiaApi.skottie_animation_seek_frame_time (Handle, t, ic?.Handle ?? IntPtr.Zero);

		//SK_C_API double skottie_animation_get_duration(skottie_animation_t *instance);
		public double Duration
			=> SkiaApi.skottie_animation_get_duration (Handle);

		//SK_C_API double skottie_animation_get_fps(skottie_animation_t *instance);
		public double Fps
			=> SkiaApi.skottie_animation_get_fps (Handle);

		//SK_C_API double skottie_animation_get_in_point(skottie_animation_t *instance);
		public double InPoint
			=> SkiaApi.skottie_animation_get_in_point (Handle);

		//SK_C_API double skottie_animation_get_out_point(skottie_animation_t *instance);
		public double OutPoint
			=> SkiaApi.skottie_animation_get_out_point (Handle);

		//SK_C_API void skottie_animation_get_version(skottie_animation_t *instance, sk_string_t *);
		public string Version {
			get {
				using var str = new SKString ();

				SkiaApi.skottie_animation_get_version (Handle, str.Handle);

				return str.ToString();
			}
		}

		//SK_C_API skottie_animation_get_size(skottie_animation_t *instance, sk_size_t* size);
		public unsafe SKSize Size {
			get {
				SKSize size;
				SkiaApi.skottie_animation_get_size (Handle, &size);
				return size;
			}
		}

		internal static Animation GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new Animation (handle, true);
	}
}

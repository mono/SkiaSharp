using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp.SceneGraph;

namespace SkiaSharp.Skottie
{
	public unsafe class Animation : SKObject, ISKNonVirtualReferenceCounted, ISKSkipObjectRegistration
	{
		internal Animation (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		void ISKNonVirtualReferenceCounted.ReferenceNative ()
			=> SkiaApi.skottie_animation_ref (Handle);

		void ISKNonVirtualReferenceCounted.UnreferenceNative ()
			=> SkiaApi.skottie_animation_unref (Handle);

		protected override void DisposeNative ()
			=> SkiaApi.skottie_animation_delete (Handle);

		public static bool TryParse (string data, out Animation animation)
		{
			animation = GetObject (SkiaApi.skottie_animation_make_from_string (data, data.Length));
			return animation != null;
		}

		public static bool TryCreate (Stream stream, out Animation animation)
		{
			using (var managed = new SKManagedStream (stream)) {
				return TryCreate (managed, out animation);
			}
		}

		public static bool TryCreate (SKStream stream, out Animation animation)
		{
			animation = GetObject (SkiaApi.skottie_animation_make_from_stream (stream.Handle));
			return animation != null;
		}

		public static bool TryCreate (string path, out Animation animation)
		{
			animation = GetObject (SkiaApi.skottie_animation_make_from_file (path));
			return animation != null;
		}

		public unsafe void Render(SKCanvas canvas, SKRect dst)
			=> SkiaApi.skottie_animation_render (Handle, canvas.Handle, &dst);

		public void Render (SKCanvas canvas, SKRect dst, SkottieAnimationRenderflags flags)
			=> SkiaApi.skottie_animation_render_with_flags (Handle, canvas.Handle, &dst, flags);

		public void Seek (double t, InvalidationController ic = null)
			=> SkiaApi.skottie_animation_seek (Handle, (float)t, ic?.Handle ?? IntPtr.Zero);

		public void SeekFrame(double t, InvalidationController ic = null)
			=> SkiaApi.skottie_animation_seek_frame (Handle, (float)t, ic?.Handle ?? IntPtr.Zero);

		public void SeekFrameTime(double t, InvalidationController ic = null)
			=> SkiaApi.skottie_animation_seek_frame_time (Handle, (float)t, ic?.Handle ?? IntPtr.Zero);

		public double Duration
			=> SkiaApi.skottie_animation_get_duration (Handle);

		public double Fps
			=> SkiaApi.skottie_animation_get_fps (Handle);

		public double InPoint
			=> SkiaApi.skottie_animation_get_in_point (Handle);

		public double OutPoint
			=> SkiaApi.skottie_animation_get_out_point (Handle);

		public string Version {
			get {
				using var str = new SKString ();

				SkiaApi.skottie_animation_get_version (Handle, str.Handle);

				return str.ToString();
			}
		}

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

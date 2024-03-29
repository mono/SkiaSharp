using System;
using System.IO;
using SkiaSharp.Resources;
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
			=> SkottieApi.skottie_animation_ref (Handle);

		void ISKNonVirtualReferenceCounted.UnreferenceNative ()
			=> SkottieApi.skottie_animation_unref (Handle);

		protected override void DisposeNative ()
			=> SkottieApi.skottie_animation_delete (Handle);

		// AnimationBuilder

		public static AnimationBuilder CreateBuilder (AnimationBuilderFlags flags = AnimationBuilderFlags.None) =>
			new AnimationBuilder (flags);

		// Parse

		public static Animation? Parse (string json) =>
			TryParse (json, out var animation)
				? animation
				: null;

		public static bool TryParse (string json, [System.Diagnostics.CodeAnalysis.NotNullWhen (true)] out Animation? animation)
		{
			_ = json ?? throw new ArgumentNullException (nameof (json));

			animation = GetObject (SkottieApi.skottie_animation_make_from_string (json, json.Length));
			return animation != null;
		}

		// Create

		public static Animation? Create (Stream stream) =>
			TryCreate (stream, out var animation)
				? animation
				: null;

		public static bool TryCreate (Stream stream, [System.Diagnostics.CodeAnalysis.NotNullWhen (true)] out Animation? animation)
		{
			_ = stream ?? throw new ArgumentNullException (nameof (stream));

			using var data = SKData.Create (stream);
			return TryCreate (data, out animation);
		}

		public static Animation? Create (SKStream stream) =>
			TryCreate (stream, out var animation)
				? animation
				: null;

		public static bool TryCreate (SKStream stream, [System.Diagnostics.CodeAnalysis.NotNullWhen (true)] out Animation? animation)
		{
			_ = stream ?? throw new ArgumentNullException (nameof (stream));

			using var data = SKData.Create (stream);
			return TryCreate (data, out animation);
		}

		public static Animation? Create (SKData data) =>
			TryCreate (data, out var animation)
				? animation
				: null;

		public static bool TryCreate (SKData data, [System.Diagnostics.CodeAnalysis.NotNullWhen (true)] out Animation? animation)
		{
			_ = data ?? throw new ArgumentNullException (nameof (data));

			var preamble = Utils.GetPreambleSize (data);
			var span = data.AsSpan ().Slice (preamble);

			fixed (byte* ptr = span) {
				animation = GetObject (SkottieApi.skottie_animation_make_from_data (ptr, (IntPtr)span.Length));
				GC.KeepAlive(data);
				return animation != null;
			}
		}

		public static Animation? Create (string path) =>
			TryCreate (path, out var animation)
				? animation
				: null;

		public static bool TryCreate (string path, [System.Diagnostics.CodeAnalysis.NotNullWhen (true)] out Animation? animation)
		{
			_ = path ?? throw new ArgumentNullException (nameof (path));

			using var data = SKData.Create (path);
			return TryCreate (data, out animation);
		}

		// Render

		public unsafe void Render (SKCanvas canvas, SKRect dst)
			=> SkottieApi.skottie_animation_render (Handle, canvas.Handle, &dst);

		public void Render (SKCanvas canvas, SKRect dst, AnimationRenderFlags flags)
			=> SkottieApi.skottie_animation_render_with_flags (Handle, canvas.Handle, &dst, flags);

		// Seek*

		public void Seek (double percent, InvalidationController? ic = null)
			=> SkottieApi.skottie_animation_seek (Handle, (float)percent, ic?.Handle ?? IntPtr.Zero);

		public void SeekFrame (double frame, InvalidationController? ic = null)
			=> SkottieApi.skottie_animation_seek_frame (Handle, (float)frame, ic?.Handle ?? IntPtr.Zero);

		public void SeekFrameTime (double seconds, InvalidationController? ic = null)
			=> SkottieApi.skottie_animation_seek_frame_time (Handle, (float)seconds, ic?.Handle ?? IntPtr.Zero);

		public void SeekFrameTime (TimeSpan time, InvalidationController? ic = null)
			=> SeekFrameTime (time.TotalSeconds, ic);

		// Properties

		public TimeSpan Duration
			=> TimeSpan.FromSeconds (SkottieApi.skottie_animation_get_duration (Handle));

		public double Fps
			=> SkottieApi.skottie_animation_get_fps (Handle);

		public double InPoint
			=> SkottieApi.skottie_animation_get_in_point (Handle);

		public double OutPoint
			=> SkottieApi.skottie_animation_get_out_point (Handle);

		public string Version {
			get {
				using var str = new SKString ();
				SkottieApi.skottie_animation_get_version (Handle, str.Handle);
				return str.ToString ();
			}
		}

		public unsafe SKSize Size {
			get {
				SKSize size;
				SkottieApi.skottie_animation_get_size (Handle, &size);
				return size;
			}
		}

		internal static Animation? GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new Animation (handle, true);
	}
}

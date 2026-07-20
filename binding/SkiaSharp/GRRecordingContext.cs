#nullable disable

using System;

namespace SkiaSharp
{
	public unsafe class GRRecordingContext : SKObject, ISKReferenceCounted
	{
		internal GRRecordingContext (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		public virtual GRBackend Backend {
			get {
				var result = SkiaApi.gr_recording_context_get_backend (Handle).FromNative ();
				GC.KeepAlive (this);
				return result;
			}
		}

		public virtual bool IsAbandoned {
			get {
				var result = SkiaApi.gr_recording_context_is_abandoned (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		public int MaxTextureSize {
			get {
				var result = SkiaApi.gr_recording_context_max_texture_size (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		public int MaxRenderTargetSize {
			get {
				var result = SkiaApi.gr_recording_context_max_render_target_size (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		public int GetMaxSurfaceSampleCount (SKColorType colorType)
		{
			var result = SkiaApi.gr_recording_context_get_max_surface_sample_count_for_color_type (Handle, colorType.ToNative ());
			GC.KeepAlive (this);
			return result;
		}

		internal static GRRecordingContext GetObject (IntPtr handle, bool owns = true, bool unrefExisting = true)
		{
			var directContext = SkiaApi.gr_recording_context_get_direct_context (handle);
			if (directContext != IntPtr.Zero) {
				return GRContext.GetObject (directContext, owns: false, unrefExisting: false);
			}

			return GetOrAddObject (handle, owns, unrefExisting, (h, o) => new GRRecordingContext (h, o));
		}
	}
}

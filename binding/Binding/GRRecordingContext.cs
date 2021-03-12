using System;

namespace SkiaSharp
{
	public unsafe class GRRecordingContext : SKObject, ISKReferenceCounted
	{
		internal GRRecordingContext (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		public GRBackend Backend => SkiaApi.gr_recording_context_get_backend (Handle).FromNative ();

		public int GetMaxSurfaceSampleCount (SKColorType colorType) =>
			SkiaApi.gr_recording_context_get_max_surface_sample_count_for_color_type (Handle, colorType.ToNative ());

		internal static GRRecordingContext GetObject (IntPtr handle, bool owns = true) =>
			GetOrAddObject (handle, owns, (h, o) => new GRRecordingContext (h, o));
	}
}

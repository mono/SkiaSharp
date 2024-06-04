#nullable disable

using System;

namespace SkiaSharp
{
	public unsafe class SKPictureRecorder : SKObject, ISKSkipObjectRegistration
	{
		internal SKPictureRecorder (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKPictureRecorder ()
			: this (SkiaApi.sk_picture_recorder_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPictureRecorder instance.");
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_picture_recorder_delete (Handle);

		public SKCanvas BeginRecording (SKRect cullRect)
		{
			return OwnedBy (SKCanvas.GetObject (SkiaApi.sk_picture_recorder_begin_recording (Handle, &cullRect), false), this);
		}

		public SKCanvas BeginRecording (SKRect cullRect, bool useRTree)
		{
			// no R-Tree is being used, so use the default path
			if (!useRTree) {
				return BeginRecording (cullRect);
			}

			// an R-Tree was requested, so create the R-Tree BBH factory
			var rtreeHandle = IntPtr.Zero;
			try {
				rtreeHandle = SkiaApi.sk_rtree_factory_new ();
				return OwnedBy (SKCanvas.GetObject (SkiaApi.sk_picture_recorder_begin_recording_with_bbh_factory (Handle, &cullRect, rtreeHandle), false), this);
			} finally {
				if (rtreeHandle != IntPtr.Zero) {
					SkiaApi.sk_rtree_factory_delete (rtreeHandle);
				}
			}
		}

		public SKPicture EndRecording ()
		{
			return SKPicture.GetObject (SkiaApi.sk_picture_recorder_end_recording (Handle));
		}

		public SKDrawable EndRecordingAsDrawable ()
		{
			return SKDrawable.GetObject (SkiaApi.sk_picture_recorder_end_recording_as_drawable (Handle));
		}

		public SKCanvas RecordingCanvas =>
			OwnedBy (SKCanvas.GetObject (SkiaApi.sk_picture_get_recording_canvas (Handle), false), this);
	}
}

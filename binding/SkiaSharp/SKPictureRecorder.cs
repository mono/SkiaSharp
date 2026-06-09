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
			var result = OwnedBy (SKCanvas.GetObject (SkiaApi.sk_picture_recorder_begin_recording (Handle, &cullRect), false), this);
			return result;
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
				var result = OwnedBy (SKCanvas.GetObject (SkiaApi.sk_picture_recorder_begin_recording_with_bbh_factory (Handle, &cullRect, rtreeHandle), false), this);
				return result;
			} finally {
				if (rtreeHandle != IntPtr.Zero) {
					SkiaApi.sk_rtree_factory_delete (rtreeHandle);
				}
			}
		}

		public SKPicture EndRecording ()
		{
			var result = SKPicture.GetObject (SkiaApi.sk_picture_recorder_end_recording (Handle));
			return result;
		}

		public SKDrawable EndRecordingAsDrawable ()
		{
			var result = SKDrawable.GetObject (SkiaApi.sk_picture_recorder_end_recording_as_drawable (Handle));
			return result;
		}

		public SKCanvas RecordingCanvas {
			get {
				var result = OwnedBy (SKCanvas.GetObject (SkiaApi.sk_picture_get_recording_canvas (Handle), false), this);
				return result;
			}
		}
	}
}

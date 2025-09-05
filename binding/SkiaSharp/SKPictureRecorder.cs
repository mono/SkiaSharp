#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// The picture recorder is used to record drawing operations made to a <see cref="SKCanvas" /> and stored in a <see cref="SKPicture" />.
	/// </summary>
	public unsafe class SKPictureRecorder : SKObject, ISKSkipObjectRegistration
	{
		internal SKPictureRecorder (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="SKPictureRecorder" />.
		/// </summary>
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

		/// <summary>
		/// Start the recording process and return the recording canvas.
		/// </summary>
		/// <param name="cullRect">The culling rectangle for the new picture.</param>
		/// <returns>Returns the current recording canvas. The same can be retrieved using <see cref="SKPictureRecorder.RecordingCanvas" />.</returns>
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

		/// <summary>
		/// Signal that the caller is done recording.
		/// </summary>
		/// <returns>Returns the <see cref="SKPicture" /> containing the recorded content.</returns>
		/// <remarks>This invalidates the canvas returned by
		/// <see cref="SkiaSharp.SKPictureRecorder.BeginRecording%2A" /> and
		/// <see cref="SkiaSharp.SKPictureRecorder.RecordingCanvas" />.</remarks>
		public SKPicture EndRecording ()
		{
			return SKPicture.GetObject (SkiaApi.sk_picture_recorder_end_recording (Handle));
		}

		/// <summary>
		/// Signal that the caller is done recording.
		/// </summary>
		/// <returns>Returns the <see cref="SKDrawable" /> containing the recorded content.</returns>
		/// <remarks>This invalidates the canvas returned by
		/// <see cref="SkiaSharp.SKPictureRecorder.BeginRecording%2A" /> and
		/// <see cref="SkiaSharp.SKPictureRecorder.RecordingCanvas" />.
		/// Unlike <see cref="SkiaSharp.SKPictureRecorder.EndRecording%2A" />, which returns an
		/// immutable picture, the returned drawable may contain live references to other
		/// drawables (if they were added to the recording canvas) and therefore this
		/// drawable will reflect the current state of those nested drawables anytime it
		/// is drawn or a new picture is snapped from it (by calling
		/// <see cref="SkiaSharp.SKDrawable.Snapshot%2A" />).</remarks>
		public SKDrawable EndRecordingAsDrawable ()
		{
			return SKDrawable.GetObject (SkiaApi.sk_picture_recorder_end_recording_as_drawable (Handle));
		}

		/// <summary>
		/// Gets the current recording canvas.
		/// </summary>
		public SKCanvas RecordingCanvas =>
			OwnedBy (SKCanvas.GetObject (SkiaApi.sk_picture_get_recording_canvas (Handle), false), this);
	}
}

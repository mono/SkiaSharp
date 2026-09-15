#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>The picture recorder is used to record drawing operations made to a <see cref="T:SkiaSharp.SKCanvas" /> and stored in a <see cref="T:SkiaSharp.SKPicture" />.</summary>
	/// <remarks />
	public unsafe class SKPictureRecorder : SKObject, ISKSkipObjectRegistration
	{
		internal SKPictureRecorder (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Creates a new instance of the <see cref="T:SkiaSharp.SKPictureRecorder" />.</summary>
		/// <remarks />
		public SKPictureRecorder ()
			: this (SkiaApi.sk_picture_recorder_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPictureRecorder instance.");
			}
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKPictureRecorder" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKPictureRecorder" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_picture_recorder_delete (Handle);

		/// <summary>Start the recording process and return the recording canvas.</summary>
		/// <param name="cullRect">The culling rectangle for the new picture.</param>
		/// <returns>Returns the current recording canvas. The same can be retrieved using <see cref="P:SkiaSharp.SKPictureRecorder.RecordingCanvas" />.</returns>
		/// <remarks />
		public SKCanvas BeginRecording (SKRect cullRect)
		{
			var result = OwnedBy (SKCanvas.GetObject (SkiaApi.sk_picture_recorder_begin_recording (Handle, &cullRect), false), this);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Start the recording process with optional R-Tree bounding box hierarchy and return the recording canvas.</summary>
		/// <param name="cullRect">The culling rectangle for the new picture.</param>
		/// <param name="useRTree">Whether to use an R-Tree for spatial indexing to optimize playback.</param>
		/// <returns>Returns the current recording canvas. The same can be retrieved using <see cref="P:SkiaSharp.SKPictureRecorder.RecordingCanvas" />.</returns>
		/// <remarks>Using an R-Tree can improve playback performance when drawing pictures that contain many primitives, as it allows skipping draw operations that don't intersect with the current clip.</remarks>
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
				GC.KeepAlive (this);
				return result;
			} finally {
				if (rtreeHandle != IntPtr.Zero) {
					SkiaApi.sk_rtree_factory_delete (rtreeHandle);
				}
			}
		}

		/// <summary>Signal that the caller is done recording.</summary>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKPicture" /> containing the recorded content.</returns>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// This invalidates the canvas returned by
		/// <xref:SkiaSharp.SKPictureRecorder.BeginRecording%2A> and
		/// <xref:SkiaSharp.SKPictureRecorder.RecordingCanvas>.
		/// ]]></format></remarks>
		public SKPicture EndRecording ()
		{
			var result = SKPicture.GetObject (SkiaApi.sk_picture_recorder_end_recording (Handle));
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Signal that the caller is done recording.</summary>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKDrawable" /> containing the recorded content.</returns>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// This invalidates the canvas returned by
		/// <xref:SkiaSharp.SKPictureRecorder.BeginRecording%2A> and
		/// <xref:SkiaSharp.SKPictureRecorder.RecordingCanvas>.
		///
		/// Unlike <xref:SkiaSharp.SKPictureRecorder.EndRecording%2A>, which returns an
		/// immutable picture, the returned drawable may contain live references to other
		/// drawables (if they were added to the recording canvas) and therefore this
		/// drawable will reflect the current state of those nested drawables anytime it
		/// is drawn or a new picture is snapped from it (by calling
		/// <xref:SkiaSharp.SKDrawable.Snapshot%2A>).
		/// ]]></format></remarks>
		public SKDrawable EndRecordingAsDrawable ()
		{
			var result = SKDrawable.GetObject (SkiaApi.sk_picture_recorder_end_recording_as_drawable (Handle));
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Gets the current recording canvas.</summary>
		/// <value>The recording canvas.</value>
		/// <remarks />
		public SKCanvas RecordingCanvas {
			get {
				var result = OwnedBy (SKCanvas.GetObject (SkiaApi.sk_picture_get_recording_canvas (Handle), false), this);
				GC.KeepAlive (this);
				return result;
			}
		}
	}
}

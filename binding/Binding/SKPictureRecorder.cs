//
// Bindings for Picture Recorder
//
// Author:
//   Miguel de Icaza
//
// Copyright 2016 Xamarin Inc
//

using System;

namespace SkiaSharp
{
	public class SKPictureRecorder : SKObject
	{		
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_picture_recorder_delete (Handle);
			}

			base.Dispose (disposing);
		}
		
		[Preserve]
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

		public SKCanvas BeginRecording (SKRect rect)
		{
			return GetObject<SKCanvas> (SkiaApi.sk_picture_recorder_begin_recording (Handle, ref rect), false);
		}

		public SKPicture EndRecording ()
		{
			return GetObject<SKPicture> (SkiaApi.sk_picture_recorder_end_recording (Handle));
		}

		public SKCanvas RecordingCanvas => GetObject<SKCanvas> (SkiaApi.sk_picture_get_recording_canvas (Handle), false);
	}
}


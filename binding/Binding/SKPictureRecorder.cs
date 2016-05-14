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
			if (Handle != IntPtr.Zero) {
				SkiaApi.sk_picture_recorder_delete (Handle);
			}

			base.Dispose (disposing);
		}
		
		[Preserve]
		public SKPictureRecorder (IntPtr handle)
			: base (handle)
		{
		}

		public SKPictureRecorder ()
			: this (SkiaApi.sk_picture_recorder_new ())
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPictureRecorder instance.");
			}
		}

		public SKCanvas BeginRecording (SKRect rect)
		{
			return GetObject<SKCanvas> (SkiaApi.sk_picture_recorder_begin_recording (Handle, ref rect));
		}

		public SKPicture EndRecording ()
		{
			return GetObject<SKPicture> (SkiaApi.sk_picture_recorder_end_recording (Handle));
		}
	}
}


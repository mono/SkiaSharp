//
// Bindings for Picture Recorder
//
// Author:
//   Miguel de Icaza
//
// Copyright 2015 Xamarin Inc
//

using System;

namespace SkiaSharp
{
	public class SKPictureRecorder : IDisposable
	{
		internal IntPtr handle;

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				SkiaApi.sk_picture_recorder_delete (handle);
				handle = IntPtr.Zero;
			}
		}

		~SKPictureRecorder()
		{
			Dispose (false);
		}

		public SKPictureRecorder ()
		{ 
			handle = SkiaApi.sk_picture_recorder_new ();
		}

		public SKCanvas BeginRecording (SKRect rect)
		{
			return new SKCanvas (SkiaApi.sk_picture_recorder_begin_recording (handle, ref rect));
		}

		public SKPicture EndRecording ()
		{
			return new SKPicture (SkiaApi.sk_picture_recorder_end_recording (handle));
		}
	}
}


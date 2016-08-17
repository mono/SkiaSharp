//
// Bindings for GRContext
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//

using System;

namespace SkiaSharp
{
	public class GRGlInterface : SKObject
	{
		[Preserve]
		internal GRGlInterface (IntPtr h, bool owns)
			: base (h, owns)
		{
		}
		
		public static GRGlInterface CreateDefaultInterface ()
		{
			return GetObject<GRGlInterface> (SkiaApi.gr_gl_default_interface ());
		}

		public static GRGlInterface CreateNativeInterface ()
		{
			return GetObject<GRGlInterface> (SkiaApi.gr_gl_create_native_interface ());
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.gr_context_unref (Handle);
			}

			base.Dispose (disposing);
		}
	}
}


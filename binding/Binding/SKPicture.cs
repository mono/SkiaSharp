//
// Bindings for SKPicture
//
// Author:
//   Miguel de Icaza
//
// Copyright 2015 Xamarin Inc
//

using System;

namespace SkiaSharp
{
	public class SKPicture : IDisposable
	{
		internal IntPtr handle;

		internal SKPicture (IntPtr h)
		{
			handle = h;
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				SkiaApi.sk_picture_unref (handle);
				handle = IntPtr.Zero;
			}
		}

		~SKPicture()
		{
			Dispose (false);
		}

		public uint UniqueId => SkiaApi.sk_picture_get_unique_id (handle);
		public SKRect Bounds => SkiaApi.sk_picture_get_bounds (handle);
	}
}


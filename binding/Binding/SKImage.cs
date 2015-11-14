//
// Bindings for SKImage
//
// Author:
//   Miguel de Icaza
//
// Copyright 2015 Xamarin Inc
//
using System;

namespace SkiaSharp
{
	public class SKImage : IDisposable
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
				SkiaApi.sk_image_unref (handle);
				handle = IntPtr.Zero;
			}
		}

		~SKImage()
		{
			Dispose (false);
		}

		internal SKImage (IntPtr x)
		{
			handle = x;
		}
		
		public static SKImage FromPixels (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			var handle = SkiaApi.sk_image_new_raster_copy (ref info, pixels, (IntPtr) rowBytes);
			if (handle == IntPtr.Zero)
				return null;
			return new SKImage (handle);
		}

		public static SKImage FromData (SKData data, SKRectI subset)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			
			var handle = SkiaApi.sk_image_new_from_encoded (data.handle, ref subset);
			if (handle == IntPtr.Zero)
				return null;
			return new SKImage (handle);
		}

		public SKData Encode ()
		{
			return new SKData (SkiaApi.sk_image_encode (handle));
		}

		public int Width => SkiaApi.sk_image_get_width (handle);
		public int Height => SkiaApi.sk_image_get_height (handle); 
		public uint UniqueId => SkiaApi.sk_image_get_unique_id (handle);
	}
}


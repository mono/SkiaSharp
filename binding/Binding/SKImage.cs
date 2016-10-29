//
// Bindings for SKImage
//
// Author:
//   Miguel de Icaza
//
// Copyright 2016 Xamarin Inc
//
using System;

namespace SkiaSharp
{
	public enum SKImageEncodeFormat {
		Unknown,
		Bmp,
		Gif,
		Ico,
		Jpeg,
		Png,
		Wbmp,
		Webp,
		Ktx,
	}
	
	public class SKImage : SKObject
	{
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_image_unref (Handle);
			}

			base.Dispose (disposing);
		}

		[Preserve]
		internal SKImage (IntPtr x, bool owns)
			: base (x, owns)
		{
		}
		
		public static SKImage FromPixels (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));
			var handle = SkiaApi.sk_image_new_raster_copy (ref info, pixels, (IntPtr) rowBytes);
			return GetObject<SKImage> (handle);
		}

		public static SKImage FromData (SKData data, SKRectI subset)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			var handle = SkiaApi.sk_image_new_from_encoded (data.Handle, ref subset);
			return GetObject<SKImage> (handle);
		}

		public static SKImage FromData (SKData data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			var handle = SkiaApi.sk_image_new_from_encoded (data.Handle, IntPtr.Zero);
			return GetObject<SKImage> (handle);
		}

		public static SKImage FromBitmap (SKBitmap bitmap)
		{
			if (bitmap == null)
				throw new ArgumentNullException (nameof (bitmap));
			var handle = SkiaApi.sk_image_new_from_bitmap (bitmap.Handle);
			return GetObject<SKImage> (handle);
		}

		public SKData Encode ()
		{
			return GetObject<SKData> (SkiaApi.sk_image_encode (Handle));
		}

		public SKData Encode (SKImageEncodeFormat format, int quality)
		{
			return GetObject<SKData> (SkiaApi.sk_image_encode_specific (Handle, format, quality));
		}

		public int Width => SkiaApi.sk_image_get_width (Handle);
		public int Height => SkiaApi.sk_image_get_height (Handle); 
		public uint UniqueId => SkiaApi.sk_image_get_unique_id (Handle);
	}
}


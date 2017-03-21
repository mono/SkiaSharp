//
// Bindings for SKPixmap
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2017 Xamarin Inc
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public class SKPixmap : SKObject
	{
		private const string UnableToCreateInstanceMessage = "Unable to create a new SKPixmap instance.";

		[Preserve]
		internal SKPixmap (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKPixmap ()
			: this (SkiaApi.sk_pixmap_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException (UnableToCreateInstanceMessage);
			}
		}

		public SKPixmap (SKImageInfo info, IntPtr addr)
			: this (info, addr, info.RowBytes)
		{
		}

		public SKPixmap (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable = null)
			: this (SkiaApi.sk_pixmap_new_with_params (ref info, addr, (IntPtr)rowBytes, ctable == null ? IntPtr.Zero : ctable.Handle), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException (UnableToCreateInstanceMessage);
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_pixmap_destructor (Handle);
			}

			base.Dispose (disposing);
		}

		public void Reset ()
		{
			SkiaApi.sk_pixmap_reset (Handle);
		}

		public void Reset (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable = null)
		{
			SkiaApi.sk_pixmap_reset_with_params (Handle, ref info, addr, (IntPtr)rowBytes, ctable == null ? IntPtr.Zero : ctable.Handle);
		}

		public SKImageInfo Info {
			get {
				SKImageInfo info;
				SkiaApi.sk_pixmap_get_info (Handle, out info);
				return info;
			}
		}

		public int Width {
			get { return Info.Width; }
		}

		public int Height {
			get { return Info.Height; }
		}

		public SKColorType ColorType {
			get { return Info.ColorType; }
		}

		public SKAlphaType AlphaType {
			get { return Info.AlphaType; }
		}

		public int BytesPerPixel {
			get { return Info.BytesPerPixel; }
		}

		public int RowBytes {
			get { return (int)SkiaApi.sk_pixmap_get_row_bytes (Handle); }
		}

		public IntPtr GetPixels ()
		{
			return SkiaApi.sk_pixmap_get_pixels (Handle);
		}

		public SKColorTable ColorTable {
			get { return GetObject<SKColorTable> (SkiaApi.sk_pixmap_get_colortable (Handle), false); }
		}

		public static bool Resize (SKPixmap dst, SKPixmap src, SKBitmapResizeMethod method)
		{
			return SkiaApi.sk_bitmapscaler_resize (dst.Handle, src.Handle, method);
		}

		public SKData Encode (SKEncodedImageFormat encoder, int quality)
		{
			using (var stream = new SKDynamicMemoryWStream ()) {
				var result = Encode (stream, this, encoder, quality);
				return result ? stream.DetachAsData () : null;
			}
		}

		public bool Encode (SKWStream dst, SKEncodedImageFormat encoder, int quality)
		{
			return Encode (dst, this, encoder, quality);
		}

		public static bool Encode (SKWStream dst, SKPixmap src, SKEncodedImageFormat encoder, int quality)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return SkiaApi.sk_pixmap_encode_image (dst.Handle, src.Handle, encoder, quality);
		}
	}
}

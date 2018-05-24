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
			: this (IntPtr.Zero, true)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			Handle = SkiaApi.sk_pixmap_new_with_params (ref cinfo, addr, (IntPtr)rowBytes, ctable == null ? IntPtr.Zero : ctable.Handle);
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
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			SkiaApi.sk_pixmap_reset_with_params (Handle, ref cinfo, addr, (IntPtr)rowBytes, ctable == null ? IntPtr.Zero : ctable.Handle);
		}

		public SKImageInfo Info {
			get {
				SKImageInfoNative cinfo;
				SkiaApi.sk_pixmap_get_info (Handle, out cinfo);
				return SKImageInfoNative.ToManaged (ref cinfo);
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

		public SKColorSpace ColorSpace {
			get { return Info.ColorSpace; }
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

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			return SkiaApi.sk_pixmap_read_pixels (Handle, ref cinfo, dstPixels, (IntPtr)dstRowBytes, srcX, srcY);
		}

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes)
		{
			return ReadPixels (dstInfo, dstPixels, dstRowBytes, 0, 0);
		}

		public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY)
		{
			return ReadPixels (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, srcX, srcY);
		}

		public bool ReadPixels (SKPixmap pixmap)
		{
			return ReadPixels (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, 0, 0);
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

		public static bool Encode (SKWStream dst, SKBitmap src, SKEncodedImageFormat format, int quality)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			using (var pixmap = new SKPixmap ()) {
				return src.PeekPixels (pixmap) && Encode (dst, pixmap, format, quality);
			}
		}

		public static bool Encode (SKWStream dst, SKPixmap src, SKEncodedImageFormat encoder, int quality)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return SkiaApi.sk_pixmap_encode_image (dst.Handle, src.Handle, encoder, quality);
		}

		public SKData Encode (SKWebpEncoderOptions options)
		{
			using (var stream = new SKDynamicMemoryWStream ()) {
				var result = Encode (stream, this, options);
				return result ? stream.DetachAsData () : null;
			}
		}

		public bool Encode (SKWStream dst, SKWebpEncoderOptions options)
		{
			return Encode (dst, this, options);
		}

		public static bool Encode (SKWStream dst, SKPixmap src, SKWebpEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return SkiaApi.sk_webpencoder_encode (dst.Handle, src.Handle, options);
		}

		public SKData Encode (SKJpegEncoderOptions options)
		{
			using (var stream = new SKDynamicMemoryWStream ()) {
				var result = Encode (stream, this, options);
				return result ? stream.DetachAsData () : null;
			}
		}

		public bool Encode (SKWStream dst, SKJpegEncoderOptions options)
		{
			return Encode (dst, this, options);
		}

		public static bool Encode (SKWStream dst, SKPixmap src, SKJpegEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return SkiaApi.sk_jpegencoder_encode (dst.Handle, src.Handle, options);
		}

		public SKData Encode (SKPngEncoderOptions options)
		{
			using (var stream = new SKDynamicMemoryWStream ()) {
				var result = Encode (stream, this, options);
				return result ? stream.DetachAsData () : null;
			}
		}

		public bool Encode (SKWStream dst, SKPngEncoderOptions options)
		{
			return Encode (dst, this, options);
		}

		public static bool Encode (SKWStream dst, SKPixmap src, SKPngEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return SkiaApi.sk_pngencoder_encode (dst.Handle, src.Handle, options);
		}

		public SKPixmap WithColorType (SKColorType newColorType)
		{
			return new SKPixmap (Info.WithColorType (newColorType), GetPixels (), RowBytes, ColorTable);
		}

		public SKPixmap WithColorSpace (SKColorSpace newColorSpace)
		{
			return new SKPixmap (Info.WithColorSpace (newColorSpace), GetPixels (), RowBytes, ColorTable);
		}

		public SKPixmap WithAlphaType (SKAlphaType newAlphaType)
		{
			return new SKPixmap (Info.WithAlphaType (newAlphaType), GetPixels (), RowBytes, ColorTable);
		}
	}
}

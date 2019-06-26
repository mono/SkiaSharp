using System;

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

		[Obsolete ("The Index8 color type and color table is no longer supported. Use SKPixmap(SKImageInfo, IntPtr, int) instead.")]
		public SKPixmap (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable)
			: this (info, addr, info.RowBytes)
		{
		}

		public SKPixmap (SKImageInfo info, IntPtr addr, int rowBytes)
			: this (IntPtr.Zero, true)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			Handle = SkiaApi.sk_pixmap_new_with_params (ref cinfo, addr, (IntPtr)rowBytes);
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

		[Obsolete ("The Index8 color type and color table is no longer supported. Use Reset(SKImageInfo, IntPtr, int) instead.")]
		public void Reset (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable)
		{
			Reset (info, addr, rowBytes);
		}

		public void Reset (SKImageInfo info, IntPtr addr, int rowBytes)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			SkiaApi.sk_pixmap_reset_with_params (Handle, ref cinfo, addr, (IntPtr)rowBytes);
		}

		public SKImageInfo Info {
			get {
				SkiaApi.sk_pixmap_get_info (Handle, out var cinfo);
				return SKImageInfoNative.ToManaged (ref cinfo);
			}
		}

		public int Width => Info.Width;

		public int Height => Info.Height;

		public SKSizeI Size => new SKSizeI (Width, Height);

		public SKRectI Rect => SKRectI.Create (Width, Height);

		public SKColorType ColorType => Info.ColorType;

		public SKAlphaType AlphaType => Info.AlphaType;

		public SKColorSpace ColorSpace => Info.ColorSpace;

		public int BytesPerPixel => Info.BytesPerPixel;

		public int RowBytes => (int)SkiaApi.sk_pixmap_get_row_bytes (Handle);

		public int BytesSize => Info.BytesSize;

		public IntPtr GetPixels () =>
			SkiaApi.sk_pixmap_get_pixels (Handle);

		public IntPtr GetPixels (int x, int y) =>
			SkiaApi.sk_pixmap_get_pixels_with_xy (Handle, x, y);

		public ReadOnlySpan<byte> GetPixelSpan ()
		{
			unsafe {
				return new ReadOnlySpan<byte> ((void*)GetPixels (), BytesSize);
			}
		}

		public SKColor GetPixelColor (int x, int y)
		{
			return SkiaApi.sk_pixmap_get_pixel_color (Handle, x, y);
		}

		[Obsolete ("The Index8 color type and color table is no longer supported.")]
		public SKColorTable ColorTable => null;

		[Obsolete ("Use ScalePixels(SKPixmap, SKFilterQuality) instead.")]
		public static bool Resize (SKPixmap dst, SKPixmap src, SKBitmapResizeMethod method)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ScalePixels (dst, method.ToFilterQuality ());
		}

		public bool ScalePixels (SKPixmap destination, SKFilterQuality quality)
		{
			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			return SkiaApi.sk_pixmap_scale_pixels (Handle, destination.Handle, quality);
		}

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKTransferFunctionBehavior behavior)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			return SkiaApi.sk_pixmap_read_pixels (Handle, ref cinfo, dstPixels, (IntPtr)dstRowBytes, srcX, srcY, behavior);
		}

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
		{
			return ReadPixels (dstInfo, dstPixels, dstRowBytes, 0, 0, SKTransferFunctionBehavior.Respect);
		}

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes)
		{
			return ReadPixels (dstInfo, dstPixels, dstRowBytes, 0, 0, SKTransferFunctionBehavior.Respect);
		}

		public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY)
		{
			return ReadPixels (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, srcX, srcY, SKTransferFunctionBehavior.Respect);
		}

		public bool ReadPixels (SKPixmap pixmap)
		{
			return ReadPixels (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, 0, 0, SKTransferFunctionBehavior.Respect);
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

		public SKPixmap ExtractSubset (SKRectI subset)
		{
			var result = new SKPixmap ();
			if (!ExtractSubset (result, subset)) {
				result.Dispose ();
				result = null;
			}
			return result;
		}

		public bool ExtractSubset (SKPixmap result, SKRectI subset)
		{
			if (result == null)
				throw new ArgumentNullException (nameof (result));

			return SkiaApi.sk_pixmap_extract_subset (Handle, result.Handle, ref subset);
		}

		public bool Erase (SKColor color)
		{
			return Erase (color, Rect);
		}

		public bool Erase (SKColor color, SKRectI subset)
		{
			return SkiaApi.sk_pixmap_erase_color (Handle, color, ref subset);
		}

		public SKPixmap WithColorType (SKColorType newColorType)
		{
			return new SKPixmap (Info.WithColorType (newColorType), GetPixels (), RowBytes);
		}

		public SKPixmap WithColorSpace (SKColorSpace newColorSpace)
		{
			return new SKPixmap (Info.WithColorSpace (newColorSpace), GetPixels (), RowBytes);
		}

		public SKPixmap WithAlphaType (SKAlphaType newAlphaType)
		{
			return new SKPixmap (Info.WithAlphaType (newAlphaType), GetPixels (), RowBytes);
		}
	}
}

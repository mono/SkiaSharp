using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	public unsafe class SKPixmap : SKObject
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
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException (UnableToCreateInstanceMessage);
		}

		public SKPixmap (SKImageInfo info, IntPtr addr)
			: this (info, addr, info.RowBytes)
		{
		}

		public SKPixmap (SKImageInfo info, IntPtr addr, int rowBytes)
			: this (IntPtr.Zero, true)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			Handle = SkiaApi.sk_pixmap_new_with_params (&cinfo, (void*)addr, (IntPtr)rowBytes);

			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException (UnableToCreateInstanceMessage);
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_pixmap_destructor (Handle);

		public void Reset () =>
			SkiaApi.sk_pixmap_reset (Handle);

		public void Reset (SKImageInfo info, IntPtr addr, int rowBytes)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			SkiaApi.sk_pixmap_reset_with_params (Handle, &cinfo, (void*)addr, (IntPtr)rowBytes);
		}

		public SKImageInfo Info {
			get {
				SKImageInfoNative cinfo;
				SkiaApi.sk_pixmap_get_info (Handle, &cinfo);
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
			(IntPtr)SkiaApi.sk_pixmap_get_pixels (Handle);

		public IntPtr GetPixels (int x, int y) =>
			(IntPtr)SkiaApi.sk_pixmap_get_pixels_with_xy (Handle, x, y);

		public Span<byte> GetPixelSpan () =>
			new Span<byte> ((void*)GetPixels (), BytesSize);

		public SKColor GetPixelColor (int x, int y) =>
			SkiaApi.sk_pixmap_get_pixel_color (Handle, x, y);

		// ScalePixels

		public bool ScalePixels (SKPixmap destination, SKFilterQuality quality)
		{
			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			return SkiaApi.sk_pixmap_scale_pixels (Handle, destination.Handle, quality);
		}

		// ReadPixels

		public bool ReadPixels (SKPixmap pixmap) =>
			ReadPixels (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, 0, 0);

		public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY) =>
			ReadPixels (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, srcX, srcY);

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels) =>
			ReadPixels (dstInfo, dstPixels, dstInfo.RowBytes, 0, 0);

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes) =>
			ReadPixels (dstInfo, dstPixels, dstRowBytes, 0, 0);

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			return SkiaApi.sk_pixmap_read_pixels (Handle, &cinfo, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY);
		}

		// Encode

		public SKData Encode (SKEncodedImageFormat format, int quality)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, this, format, quality);
			return result ? stream.DetachAsData () : null;
		}

		public bool Encode (Stream dst, SKEncodedImageFormat format, int quality) =>
			Encode (dst, this, format, quality);

		public bool Encode (SKWStream dst, SKEncodedImageFormat format, int quality) =>
			Encode (dst, this, format, quality);

		public static bool Encode (Stream dst, SKPixmap src, SKEncodedImageFormat format, int quality)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, src, format, quality);
		}

		public static bool Encode (SKWStream dst, SKPixmap src, SKEncodedImageFormat format, int quality)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return SkiaApi.sk_pixmap_encode_image (dst.Handle, src.Handle, format, quality);
		}

		// Encode (webp)

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use ScalePixels(SKPixmap, SKFilterQuality) instead.")]
		public static bool Resize (SKPixmap dst, SKPixmap src, SKBitmapResizeMethod method)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ScalePixels (dst, method.ToFilterQuality ());
		}

		public SKData Encode (SKWebpEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, this, options);
			return result ? stream.DetachAsData () : null;
		}

		public bool Encode (Stream dst, SKWebpEncoderOptions options) =>
			Encode (dst, this, options);

		public bool Encode (SKWStream dst, SKWebpEncoderOptions options) =>
			Encode (dst, this, options);

		public static bool Encode (Stream dst, SKPixmap src, SKWebpEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, src, options);
		}

		public static bool Encode (SKWStream dst, SKPixmap src, SKWebpEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return SkiaApi.sk_webpencoder_encode (dst.Handle, src.Handle, options);
		}

		// Encode (jpeg)

		public SKData Encode (SKJpegEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, this, options);
			return result ? stream.DetachAsData () : null;
		}

		public bool Encode (Stream dst, SKJpegEncoderOptions options) =>
			Encode (dst, this, options);

		public bool Encode (SKWStream dst, SKJpegEncoderOptions options) =>
			Encode (dst, this, options);

		public static bool Encode (Stream dst, SKPixmap src, SKJpegEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, src, options);
		}

		public static bool Encode (SKWStream dst, SKPixmap src, SKJpegEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return SkiaApi.sk_jpegencoder_encode (dst.Handle, src.Handle, options);
		}

		// Encode (png)

		public SKData Encode (SKPngEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, this, options);
			return result ? stream.DetachAsData () : null;
		}

		public bool Encode (Stream dst, SKPngEncoderOptions options) =>
			Encode (dst, this, options);

		public bool Encode (SKWStream dst, SKPngEncoderOptions options) =>
			Encode (dst, this, options);

		public static bool Encode (Stream dst, SKPixmap src, SKPngEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, src, options);
		}

		public static bool Encode (SKWStream dst, SKPixmap src, SKPngEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return SkiaApi.sk_pngencoder_encode (dst.Handle, src.Handle, options);
		}

		// ExtractSubset

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

			return SkiaApi.sk_pixmap_extract_subset (Handle, result.Handle, &subset);
		}

		// Erase

		public bool Erase (SKColor color) =>
			Erase (color, Rect);

		public bool Erase (SKColor color, SKRectI subset) =>
			SkiaApi.sk_pixmap_erase_color (Handle, (uint)color, &subset);

		public bool Erase (SKColorF color) =>
			Erase (color, Rect);

		public bool Erase (SKColorF color, SKRectI subset) =>
			SkiaApi.sk_pixmap_erase_color4f (Handle, &color, &subset);

		// With*

		public SKPixmap WithColorType (SKColorType newColorType) =>
			new SKPixmap (Info.WithColorType (newColorType), GetPixels (), RowBytes);

		public SKPixmap WithColorSpace (SKColorSpace newColorSpace) =>
			new SKPixmap (Info.WithColorSpace (newColorSpace), GetPixels (), RowBytes);

		public SKPixmap WithAlphaType (SKAlphaType newAlphaType) =>
			new SKPixmap (Info.WithAlphaType (newAlphaType), GetPixels (), RowBytes);
	}
}

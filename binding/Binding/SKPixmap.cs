using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	public unsafe class SKPixmap : SKObject
	{
		private const string UnableToCreateInstanceMessage = "Unable to create a new SKPixmap instance.";

		// this is not meant to be anything but a GC reference to keep the actual pixel data alive
		internal SKObject pixelSource;

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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use SKPixmap(SKImageInfo, IntPtr, int) instead.")]
		public SKPixmap (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable)
			: this (info, addr, info.RowBytes)
		{
		}

		public SKPixmap (SKImageInfo info, IntPtr addr, int rowBytes)
			: this (IntPtr.Zero, true)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			Handle = SkiaApi.sk_pixmap_new_with_params (&cinfo, (void*)addr, (IntPtr)rowBytes);
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException (UnableToCreateInstanceMessage);
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_pixmap_destructor (Handle);

		protected override void DisposeManaged ()
		{
			base.DisposeManaged ();

			pixelSource = null;
		}

		// Reset

		public void Reset ()
		{
			SkiaApi.sk_pixmap_reset (Handle);
			pixelSource = null;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use Reset(SKImageInfo, IntPtr, int) instead.")]
		public void Reset (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable)
		{
			Reset (info, addr, rowBytes);
		}

		public void Reset (SKImageInfo info, IntPtr addr, int rowBytes)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			SkiaApi.sk_pixmap_reset_with_params (Handle, &cinfo, (void*)addr, (IntPtr)rowBytes);
			pixelSource = null;
		}

		// properties

		public SKImageInfo Info {
			get {
				SKImageInfoNative cinfo;
				SkiaApi.sk_pixmap_get_info (Handle, &cinfo);
				return SKImageInfoNative.ToManaged (ref cinfo);
			}
		}

		public int Width => Info.Width;

		public int Height => Info.Height;

		public SKSizeI Size {
			get {
				var info = Info;
				return new SKSizeI (info.Width, info.Height);
			}
		}

		public SKRectI Rect => SKRectI.Create (Size);

		public SKColorType ColorType => Info.ColorType;

		public SKAlphaType AlphaType => Info.AlphaType;

		public SKColorSpace ColorSpace => Info.ColorSpace;

		public int BytesPerPixel => Info.BytesPerPixel;

		public int RowBytes => (int)SkiaApi.sk_pixmap_get_row_bytes (Handle);

		public int BytesSize => Info.BytesSize;

		// pixels

		public IntPtr GetPixels () =>
			(IntPtr)SkiaApi.sk_pixmap_get_pixels (Handle);

		public IntPtr GetPixels (int x, int y) =>
			(IntPtr)SkiaApi.sk_pixmap_get_pixels_with_xy (Handle, x, y);

		public ReadOnlySpan<byte> GetPixelSpan () =>
			new ReadOnlySpan<byte> (SkiaApi.sk_pixmap_get_pixels (Handle), BytesSize);

		public unsafe Span<T> GetPixelSpan<T> ()
			where T : unmanaged
		{
			var info = Info;
			if (info.IsEmpty)
				return null;

			var bpp = info.BytesPerPixel;
			if (bpp <= 0)
				return null;

			// byte is always valid
			if (typeof (T) == typeof (byte))
				return new Span<T> (SkiaApi.sk_pixmap_get_writable_addr (Handle), info.BytesSize);

			// other types need to make sure they fit
			var size = sizeof (T);
			if (bpp != size)
				throw new ArgumentException ($"Size of T ({size}) is not the same as the size of each pixel ({bpp}).", nameof (T));

			return new Span<T> (SkiaApi.sk_pixmap_get_writable_addr (Handle), info.Width * info.Height);
		}

		public SKColor GetPixelColor (int x, int y)
		{
			return SkiaApi.sk_pixmap_get_pixel_color (Handle, x, y);
		}

		// ColorTable

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported.")]
		public SKColorTable ColorTable => null;

		// Resize

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

		// ScalePixels

		public bool ScalePixels (SKPixmap destination, SKFilterQuality quality)
		{
			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			return SkiaApi.sk_pixmap_scale_pixels (Handle, destination.Handle, quality);
		}

		// ReadPixels

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKTransferFunctionBehavior behavior)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			return SkiaApi.sk_pixmap_read_pixels (Handle, &cinfo, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY, behavior);
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

		// Encode

		public SKData Encode (SKEncodedImageFormat encoder, int quality)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, encoder, quality);
			return result ? stream.DetachAsData () : null;
		}

		public bool Encode (Stream dst, SKEncodedImageFormat encoder, int quality)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, encoder, quality);
		}

		public bool Encode (SKWStream dst, SKEncodedImageFormat encoder, int quality)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_pixmap_encode_image (dst.Handle, Handle, encoder, quality);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use Encode(SKWStream, SKEncodedImageFormat, int) instead.")]
		public static bool Encode (SKWStream dst, SKBitmap src, SKEncodedImageFormat format, int quality)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.Encode (dst, format, quality);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use Encode(SKWStream, SKEncodedImageFormat, int) instead.")]
		public static bool Encode (SKWStream dst, SKPixmap src, SKEncodedImageFormat encoder, int quality)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.Encode (dst, encoder, quality);
		}

		// Encode (webp)

		public SKData Encode (SKWebpEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, options);
			return result ? stream.DetachAsData () : null;
		}

		public bool Encode (Stream dst, SKWebpEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, options);
		}

		public bool Encode (SKWStream dst, SKWebpEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_webpencoder_encode (dst.Handle, Handle, &options);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use Encode(SKWStream, SKWebpEncoderOptions) instead.")]
		public static bool Encode (SKWStream dst, SKPixmap src, SKWebpEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.Encode (dst, options);
		}

		// Encode (jpeg)

		public SKData Encode (SKJpegEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, options);
			return result ? stream.DetachAsData () : null;
		}

		public bool Encode (Stream dst, SKJpegEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, options);
		}

		public bool Encode (SKWStream dst, SKJpegEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_jpegencoder_encode (dst.Handle, Handle, &options);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use Encode(SKWStream, SKJpegEncoderOptions) instead.")]
		public static bool Encode (SKWStream dst, SKPixmap src, SKJpegEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.Encode (dst, options);
		}

		// Encode (png)

		public SKData Encode (SKPngEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, options);
			return result ? stream.DetachAsData () : null;
		}

		public bool Encode (Stream dst, SKPngEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, options);
		}

		public bool Encode (SKWStream dst, SKPngEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_pngencoder_encode (dst.Handle, Handle, &options);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use Encode(SKWStream, SKPngEncoderOptions) instead.")]
		public static bool Encode (SKWStream dst, SKPixmap src, SKPngEncoderOptions options)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.Encode (dst, options);
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

		public bool Erase (SKColor color)
		{
			return Erase (color, Rect);
		}

		public bool Erase (SKColor color, SKRectI subset)
		{
			return SkiaApi.sk_pixmap_erase_color (Handle, (uint)color, &subset);
		}

		public bool Erase (SKColorF color) =>
			Erase (color, Rect);

		public bool Erase (SKColorF color, SKRectI subset) =>
			SkiaApi.sk_pixmap_erase_color4f (Handle, &color, &subset);

		// With*

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

using System;
using System.IO;

namespace SkiaSharp
{
	public unsafe class SKPixmap : SKObject
	{
		private const string UnableToCreateInstanceMessage = "Unable to create a new SKPixmap instance.";

		// this is not meant to be anything but a GC reference to keep the actual pixel data alive
		internal SKObject? pixelSource;

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
			GC.KeepAlive (this);
			pixelSource = null;
		}

		public void Reset (SKImageInfo info, IntPtr addr, int rowBytes)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			SkiaApi.sk_pixmap_reset_with_params (Handle, &cinfo, (void*)addr, (IntPtr)rowBytes);
			GC.KeepAlive (this);
			pixelSource = null;
		}

		// properties

		public SKImageInfo Info {
			get {
				SKImageInfoNative cinfo;
				SkiaApi.sk_pixmap_get_info (Handle, &cinfo);
				GC.KeepAlive (this);
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

		public SKColorSpace? ColorSpace {
			get {
				var result = SKColorSpace.GetObject (SkiaApi.sk_pixmap_get_colorspace (Handle));
				GC.KeepAlive (this);
				return result;
			}
		}

		public int BytesPerPixel => Info.BytesPerPixel;

		public int BitShiftPerPixel => Info.BitShiftPerPixel;

		public int RowBytes {
			get {
				var result = (int)SkiaApi.sk_pixmap_get_row_bytes (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		public int BytesSize => Info.BytesSize;

		public long BytesSize64 => Info.BytesSize64;

		// pixels

		public IntPtr GetPixels ()
		{
			var result = (IntPtr)SkiaApi.sk_pixmap_get_writable_addr (Handle);
			GC.KeepAlive (this);
			return result;
		}

		public IntPtr GetPixels (int x, int y)
		{
			var result = (IntPtr)SkiaApi.sk_pixmap_get_writeable_addr_with_xy (Handle, x, y);
			GC.KeepAlive (this);
			return result;
		}

		public Span<byte> GetPixelSpan () =>
			GetPixelSpan<byte> (0, 0);

		public Span<byte> GetPixelSpan (int x, int y) =>
			GetPixelSpan<byte> (x, y);

		public unsafe Span<T> GetPixelSpan<T> ()
			where T : unmanaged
		{
			return GetPixelSpan<T> (0, 0);
		}

		public unsafe Span<T> GetPixelSpan<T> (int x, int y)
			where T : unmanaged
		{
			var info = Info;
			if (info.IsEmpty)
				return null;

			var bpp = info.BytesPerPixel;
			if (bpp <= 0)
				return null;

			// use the actual stride of the pixmap as it may differ from
			// (Width * BytesPerPixel) when the pixmap is a subset of a larger one
			var rowBytes = RowBytes;

			// the span covers from the first pixel up to and including the last
			// valid pixel of the last row, accounting for any row padding
			var spanLengthBytes = checked((info.Height - 1) * rowBytes + info.Width * bpp);
			var spanOffsetBytes = (x != 0 || y != 0)
				? checked(y * rowBytes + x * bpp)
				: 0;

			int spanLength;
			int spanOffset;
			if (typeof (T) == typeof (byte))
			{
				// byte is always valid

				spanLength = spanLengthBytes;
				spanOffset = spanOffsetBytes;
			}
			else
			{
				// other types need to make sure they fit

				var size = sizeof (T);
				if (bpp != size)
					throw new ArgumentException ($"Size of T ({size}) is not the same as the size of each pixel ({bpp}).", nameof (T));

				spanLength = spanLengthBytes / size;
				spanOffset = spanOffsetBytes / size;
			}

			var addr = SkiaApi.sk_pixmap_get_writable_addr (Handle);
			GC.KeepAlive (this);
			var span = new Span<T> (addr, spanLength);

			if (spanOffset != 0)
				span = span.Slice (spanOffset);

			return span;
		}

		public SKColor GetPixelColor (int x, int y)
		{
			var result = SkiaApi.sk_pixmap_get_pixel_color (Handle, x, y);
			GC.KeepAlive (this);
			return result;
		}

		public SKColorF GetPixelColorF (int x, int y)
		{
			SKColorF color;
			SkiaApi.sk_pixmap_get_pixel_color4f (Handle, x, y, &color);
			GC.KeepAlive (this);
			return color;
		}

		public float GetPixelAlpha (int x, int y)
		{
			var result = SkiaApi.sk_pixmap_get_pixel_alphaf (Handle, x, y);
			GC.KeepAlive (this);
			return result;
		}

		// ScalePixels

		[Obsolete ("Use ScalePixels(SKPixmap destination, SKSamplingOptions sampling) instead.", error: true)]
		public bool ScalePixels (SKPixmap destination, SKFilterQuality quality) =>
			ScalePixels (destination, quality.ToSamplingOptions ());

		public bool ScalePixels (SKPixmap destination) =>
			ScalePixels (destination, SKSamplingOptions.Default);

		public bool ScalePixels (SKPixmap destination, SKSamplingOptions sampling)
		{
			_ = destination ?? throw new ArgumentNullException (nameof (destination));
			var result = SkiaApi.sk_pixmap_scale_pixels (Handle, destination.Handle, &sampling);
			GC.KeepAlive (this);
			GC.KeepAlive (destination);
			return result;
		}

		// ReadPixels

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			var result = SkiaApi.sk_pixmap_read_pixels (Handle, &cinfo, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY);
			GC.KeepAlive (this);
			return result;
		}

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes) =>
			ReadPixels (dstInfo, dstPixels, dstRowBytes, 0, 0);

		public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY) =>
			ReadPixels (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, srcX, srcY);

		public bool ReadPixels (SKPixmap pixmap) =>
			ReadPixels (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, 0, 0);

		// Encode

		public SKData? Encode (SKEncodedImageFormat encoder, int quality)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, encoder, quality);
			return result ? stream.DetachAsData () : null;
		}

		public bool Encode (Stream dst, SKEncodedImageFormat encoder, int quality)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, encoder, quality);
		}

		public bool Encode (SKWStream dst, SKEncodedImageFormat encoder, int quality) =>
			encoder switch {
				SKEncodedImageFormat.Jpeg =>
					Encode (dst, new SKJpegEncoderOptions (quality)),
				SKEncodedImageFormat.Png =>
					Encode (dst, SKPngEncoderOptions.Default),
				SKEncodedImageFormat.Webp when quality == 100 =>
					Encode (dst, new SKWebpEncoderOptions (SKWebpEncoderCompression.Lossless, 75)),
				SKEncodedImageFormat.Webp =>
					Encode (dst, new SKWebpEncoderOptions (SKWebpEncoderCompression.Lossy, quality)),
				_ => false,
			};

		// Encode (webp)

		public SKData? Encode (SKWebpEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, options);
			return result ? stream.DetachAsData () : null;
		}

		public bool Encode (Stream dst, SKWebpEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, options);
		}

		public bool Encode (SKWStream dst, SKWebpEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			var result = SkiaApi.sk_webpencoder_encode (dst.Handle, Handle, &options);
			GC.KeepAlive (this);
			GC.KeepAlive (dst);
			return result;
		}

		// Encode (jpeg)

		public SKData? Encode (SKJpegEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, options);
			return result ? stream.DetachAsData () : null;
		}

		public bool Encode (Stream dst, SKJpegEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, options);
		}

		public bool Encode (SKWStream dst, SKJpegEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			var result = SkiaApi.sk_jpegencoder_encode (dst.Handle, Handle, &options);
			GC.KeepAlive (this);
			GC.KeepAlive (dst);
			return result;
		}

		// Encode (png)

		public SKData? Encode (SKPngEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, options);
			return result ? stream.DetachAsData () : null;
		}

		public bool Encode (Stream dst, SKPngEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, options);
		}

		public bool Encode (SKWStream dst, SKPngEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			var result = SkiaApi.sk_pngencoder_encode (dst.Handle, Handle, &options);
			GC.KeepAlive (this);
			GC.KeepAlive (dst);
			return result;
		}

		// ExtractSubset

		public SKPixmap? ExtractSubset (SKRectI subset)
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
			_ = result ?? throw new ArgumentNullException (nameof (result));
			var extracted = SkiaApi.sk_pixmap_extract_subset (Handle, result.Handle, &subset);
			GC.KeepAlive (this);
			GC.KeepAlive (result);
			return extracted;
		}

		// Erase

		public bool Erase (SKColor color) =>
			Erase (color, Rect);

		public bool Erase (SKColor color, SKRectI subset)
		{
			var result = SkiaApi.sk_pixmap_erase_color (Handle, (uint)color, &subset);
			GC.KeepAlive (this);
			return result;
		}

		public bool Erase (SKColorF color) =>
			Erase (color, Rect);

		public bool Erase (SKColorF color, SKRectI subset)
		{
			var result = SkiaApi.sk_pixmap_erase_color4f (Handle, &color, &subset);
			GC.KeepAlive (this);
			return result;
		}

		// ComputeIsOpaque

		public bool ComputeIsOpaque ()
		{
			var result = SkiaApi.sk_pixmap_compute_is_opaque (Handle);
			GC.KeepAlive (this);
			return result;
		}

		// With*

		public SKPixmap WithColorType (SKColorType newColorType) =>
			new SKPixmap (Info.WithColorType (newColorType), GetPixels (), RowBytes);

		public SKPixmap WithColorSpace (SKColorSpace newColorSpace) =>
			new SKPixmap (Info.WithColorSpace (newColorSpace), GetPixels (), RowBytes);

		public SKPixmap WithAlphaType (SKAlphaType newAlphaType) =>
			new SKPixmap (Info.WithAlphaType (newAlphaType), GetPixels (), RowBytes);
	}
}

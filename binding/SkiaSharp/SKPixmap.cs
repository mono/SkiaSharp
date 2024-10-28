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
			pixelSource = null;
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

		public SKColorSpace? ColorSpace =>
			SKColorSpace.GetObject (SkiaApi.sk_pixmap_get_colorspace (Handle));

		public int BytesPerPixel => Info.BytesPerPixel;

		public int BitShiftPerPixel => Info.BitShiftPerPixel;

		public int RowBytes => (int)SkiaApi.sk_pixmap_get_row_bytes (Handle);

		public int BytesSize => Info.BytesSize;

		public long BytesSize64 => Info.BytesSize64;

		// pixels

		public IntPtr GetPixels () =>
			(IntPtr)SkiaApi.sk_pixmap_get_writable_addr (Handle);

		public IntPtr GetPixels (int x, int y) =>
			(IntPtr)SkiaApi.sk_pixmap_get_writeable_addr_with_xy (Handle, x, y);

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

			var spanLength = 0;
			var spanOffset = 0;
			if (typeof (T) == typeof (byte))
			{
				// byte is always valid

				spanLength = info.BytesSize;

				if (x != 0 || y != 0)
					spanOffset = info.GetPixelBytesOffset (x, y);
			}
			else
			{
				// other types need to make sure they fit

				var size = sizeof (T);
				if (bpp != size)
					throw new ArgumentException ($"Size of T ({size}) is not the same as the size of each pixel ({bpp}).", nameof (T));

				spanLength = info.Width * info.Height;

				if (x != 0 || y != 0)
					spanOffset = y * info.Height + x;
			}

			var addr = SkiaApi.sk_pixmap_get_writable_addr (Handle);
			var span = new Span<T> (addr, spanLength);

			if (spanOffset != 0)
				span = span.Slice (spanOffset);

			return span;
		}

		public SKColor GetPixelColor (int x, int y) =>
			SkiaApi.sk_pixmap_get_pixel_color (Handle, x, y);

		public SKColorF GetPixelColorF (int x, int y)
		{
			SKColorF color;
			SkiaApi.sk_pixmap_get_pixel_color4f (Handle, x, y, &color);
			return color;
		}

		public float GetPixelAlpha (int x, int y) =>
			SkiaApi.sk_pixmap_get_pixel_alphaf (Handle, x, y);

		// ScalePixels

		[Obsolete ("Use ScalePixels(SKPixmap destination, SKSamplingOptions sampling) instead.")]
		public bool ScalePixels (SKPixmap destination, SKFilterQuality quality) =>
			ScalePixels (destination, quality.ToSamplingOptions ());

		public bool ScalePixels (SKPixmap destination) =>
			ScalePixels (destination, SKSamplingOptions.Default);

		public bool ScalePixels (SKPixmap destination, SKSamplingOptions sampling)
		{
			_ = destination ?? throw new ArgumentNullException (nameof (destination));
			return SkiaApi.sk_pixmap_scale_pixels (Handle, destination.Handle, &sampling);
		}

		// ReadPixels

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			return SkiaApi.sk_pixmap_read_pixels (Handle, &cinfo, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY);
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
			return SkiaApi.sk_webpencoder_encode (dst.Handle, Handle, &options);
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
			return SkiaApi.sk_jpegencoder_encode (dst.Handle, Handle, &options);
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
			return SkiaApi.sk_pngencoder_encode (dst.Handle, Handle, &options);
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

		// ComputeIsOpaque

		public bool ComputeIsOpaque () =>
			SkiaApi.sk_pixmap_compute_is_opaque (Handle);

		// With*

		public SKPixmap WithColorType (SKColorType newColorType) =>
			new SKPixmap (Info.WithColorType (newColorType), GetPixels (), RowBytes);

		public SKPixmap WithColorSpace (SKColorSpace newColorSpace) =>
			new SKPixmap (Info.WithColorSpace (newColorSpace), GetPixels (), RowBytes);

		public SKPixmap WithAlphaType (SKAlphaType newAlphaType) =>
			new SKPixmap (Info.WithAlphaType (newAlphaType), GetPixels (), RowBytes);
	}
}

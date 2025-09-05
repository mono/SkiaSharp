using System;
using System.IO;

namespace SkiaSharp
{
	/// <summary>
	/// Pairs <see cref="SKImageInfo" /> with actual pixels and rowbytes.
	/// </summary>
	/// <remarks>This class does not try to manage the lifetime of the pixel memory (nor the color table if provided).</remarks>
	public unsafe class SKPixmap : SKObject
	{
		private const string UnableToCreateInstanceMessage = "Unable to create a new SKPixmap instance.";

		// this is not meant to be anything but a GC reference to keep the actual pixel data alive
		internal SKObject? pixelSource;

		internal SKPixmap (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates an empty instance of <see cref="SKPixmap" />.
		/// </summary>
		public SKPixmap ()
			: this (SkiaApi.sk_pixmap_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException (UnableToCreateInstanceMessage);
			}
		}

		/// <summary>
		/// Creates an instance of <see cref="SKPixmap" />.
		/// </summary>
		/// <param name="info">The image information of the pixels.</param>
		/// <param name="addr">The memory address of the pixels.</param>
		public SKPixmap (SKImageInfo info, IntPtr addr)
			: this (info, addr, info.RowBytes)
		{
		}

		/// <summary>
		/// Creates an instance of <see cref="SKPixmap" />.
		/// </summary>
		/// <param name="info">The image information of the pixels.</param>
		/// <param name="addr">The memory address of the pixels.</param>
		/// <param name="rowBytes">The number of bytes per row.</param>
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

		/// <summary>
		/// Reset the pixmap to an empty pixmap.
		/// </summary>
		public void Reset ()
		{
			SkiaApi.sk_pixmap_reset (Handle);
			pixelSource = null;
		}

		/// <summary>
		/// Resets the pixmap to the specified pixels.
		/// </summary>
		/// <param name="info">The image information of the pixels.</param>
		/// <param name="addr">The memory address of the pixels.</param>
		/// <param name="rowBytes">The number of bytes per row.</param>
		public void Reset (SKImageInfo info, IntPtr addr, int rowBytes)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			SkiaApi.sk_pixmap_reset_with_params (Handle, &cinfo, (void*)addr, (IntPtr)rowBytes);
			pixelSource = null;
		}

		// properties

		/// <summary>
		/// Gets the image info.
		/// </summary>
		public SKImageInfo Info {
			get {
				SKImageInfoNative cinfo;
				SkiaApi.sk_pixmap_get_info (Handle, &cinfo);
				return SKImageInfoNative.ToManaged (ref cinfo);
			}
		}

		/// <summary>
		/// Gets the image width.
		/// </summary>
		public int Width => Info.Width;

		/// <summary>
		/// Gets the image height.
		/// </summary>
		public int Height => Info.Height;

		/// <summary>
		/// Gets the current size of the pixmap.
		/// </summary>
		public SKSizeI Size {
			get {
				var info = Info;
				return new SKSizeI (info.Width, info.Height);
			}
		}

		/// <summary>
		/// Gets a rectangle with the current width and height.
		/// </summary>
		public SKRectI Rect => SKRectI.Create (Size);

		/// <summary>
		/// Gets the color type.
		/// </summary>
		public SKColorType ColorType => Info.ColorType;

		/// <summary>
		/// Gets the alpha type.
		/// </summary>
		public SKAlphaType AlphaType => Info.AlphaType;

		/// <summary>
		/// Gets the color space.
		/// </summary>
		public SKColorSpace? ColorSpace =>
			SKColorSpace.GetObject (SkiaApi.sk_pixmap_get_colorspace (Handle));

		/// <summary>
		/// Gets the number of bytes per pixel.
		/// </summary>
		public int BytesPerPixel => Info.BytesPerPixel;

		public int BitShiftPerPixel => Info.BitShiftPerPixel;

		/// <summary>
		/// Gets the number of bytes per row.
		/// </summary>
		public int RowBytes => (int)SkiaApi.sk_pixmap_get_row_bytes (Handle);

		/// <summary>
		/// Gets the total number of bytes needed to store the pixel data.
		/// </summary>
		public int BytesSize => Info.BytesSize;

		public long BytesSize64 => Info.BytesSize64;

		// pixels

		/// <summary>
		/// Returns the memory address of the pixels.
		/// </summary>
		public IntPtr GetPixels () =>
			(IntPtr)SkiaApi.sk_pixmap_get_writable_addr (Handle);

		/// <summary>
		/// Returns the memory address of the pixels at (x, y).
		/// </summary>
		/// <param name="x">The column index, zero or greater, and less than the pixmap width.</param>
		/// <param name="y">The row index, zero or greater, and less than the pixmap height.</param>
		public IntPtr GetPixels (int x, int y) =>
			(IntPtr)SkiaApi.sk_pixmap_get_writeable_addr_with_xy (Handle, x, y);

		/// <summary>
		/// Returns a span that wraps the pixel data.
		/// </summary>
		/// <returns>Returns the span.</returns>
		/// <remarks>This span is only valid as long as the pixmap is valid</remarks>
		public Span<byte> GetPixelSpan () =>
			GetPixelSpan<byte> (0, 0);

		public Span<byte> GetPixelSpan (int x, int y) =>
			GetPixelSpan<byte> (x, y);

		/// <typeparam name="T"></typeparam>
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

		/// <summary>
		/// Returns the color of the pixel at the specified coordinates.
		/// </summary>
		/// <param name="x">The column index, zero or greater, and less than the pixmap width.</param>
		/// <param name="y">The row index, zero or greater, and less than the pixmap height.</param>
		/// <returns>Returns the color of the pixel.</returns>
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

		/// <summary>
		/// Copies this pixmap to the destination, scaling the pixels to fit the destination size and converting the pixels to match the color type and alpha type.
		/// </summary>
		/// <param name="destination">The pixmap to receive the scaled and converted pixels.</param>
		/// <param name="quality">The level of quality to use when scaling the pixels.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if there was an error.</returns>
		/// <remarks>Pixels are copied only if pixel conversion is possible.
		/// If the color type is <see cref="SkiaSharp.SKColorType.Gray8" />, or
		/// <see cref="SkiaSharp.SKColorType.Alpha8" />, the destination color type must match.
		/// If the color type is <see cref="SkiaSharp.SKColorType.Gray8" />, destination
		/// colorspace must also match.
		/// If the alpha type is <see cref="SkiaSharp.SKAlphaType.Opaque" />, the destination
		/// alpha type must match.
		/// If the colorspace is <see langword="null" />, the destination colorspace must also be <see langword="null" />.
		/// Filter Quality:
		/// - <see cref="SkiaSharp.SKFilterQuality.None" /> is fastest, typically implemented
		/// with nearest neighbor filter.
		/// - <see cref="SkiaSharp.SKFilterQuality.Low" /> is typically implemented with bilerp
		/// filter.
		/// - <see cref="SkiaSharp.SKFilterQuality.Medium" /> is typically implemented with
		/// bilerp filter, and mipmap when size is reduced.
		/// - <see cref="SkiaSharp.SKFilterQuality.High" /> is slowest, typically implemented
		/// with the bicubic filter.</remarks>
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

		/// <summary>
		/// Copies the pixels from the image into the specified buffer.
		/// </summary>
		/// <param name="dstInfo">The image information describing the destination pixel buffer.</param>
		/// <param name="dstPixels">The pixel buffer to read the pixel data into.</param>
		/// <param name="dstRowBytes">The number of bytes in each row of in the destination buffer.</param>
		/// <param name="srcX">The source x-coordinate to start reading from.</param>
		/// <param name="srcY">The source y-coordinate to start reading from.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			return SkiaApi.sk_pixmap_read_pixels (Handle, &cinfo, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY);
		}

		/// <summary>
		/// Copies the pixels from the image into the specified buffer.
		/// </summary>
		/// <param name="dstInfo">The image information describing the destination pixel buffer.</param>
		/// <param name="dstPixels">The pixel buffer to read the pixel data into.</param>
		/// <param name="dstRowBytes">The number of bytes in each row of in the destination buffer.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes) =>
			ReadPixels (dstInfo, dstPixels, dstRowBytes, 0, 0);

		/// <summary>
		/// Copies the pixels from the image into the specified pixmap.
		/// </summary>
		/// <param name="pixmap">The pixmap to read the pixel data into.</param>
		/// <param name="srcX">The source x-coordinate to start reading from.</param>
		/// <param name="srcY">The source y-coordinate to start reading from.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY) =>
			ReadPixels (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, srcX, srcY);

		/// <summary>
		/// Copies the pixels from the image into the specified pixmap.
		/// </summary>
		/// <param name="pixmap">The pixmap to read the pixel data into.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		public bool ReadPixels (SKPixmap pixmap) =>
			ReadPixels (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, 0, 0);

		// Encode

		/// <summary>
		/// Encodes the pixmap using the specified format.
		/// </summary>
		/// <param name="encoder">The file format used to encode the pixmap.</param>
		/// <param name="quality">The quality level to use for the pixmap.</param>
		/// <returns>Returns the <see cref="SKData" /> wrapping the encoded pixmap.</returns>
		public SKData? Encode (SKEncodedImageFormat encoder, int quality)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, encoder, quality);
			return result ? stream.DetachAsData () : null;
		}

		/// <param name="dst"></param>
		/// <param name="encoder"></param>
		/// <param name="quality"></param>
		public bool Encode (Stream dst, SKEncodedImageFormat encoder, int quality)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, encoder, quality);
		}

		/// <summary>
		/// Encodes the pixmap using the specified format.
		/// </summary>
		/// <param name="dst">The stream to write the encoded pixmap to.</param>
		/// <param name="encoder">The file format used to encode the pixmap.</param>
		/// <param name="quality">The quality level to use for the pixmap.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if there was an error.</returns>
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

		/// <summary>
		/// Encodes the pixmap as a WEBP.
		/// </summary>
		/// <param name="options">The options to use when creating the encoder.</param>
		/// <returns>Returns the <see cref="SKData" /> wrapping the encoded pixmap.</returns>
		public SKData? Encode (SKWebpEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, options);
			return result ? stream.DetachAsData () : null;
		}

		/// <param name="dst"></param>
		/// <param name="options"></param>
		public bool Encode (Stream dst, SKWebpEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, options);
		}

		/// <summary>
		/// Encodes the pixmap as a WEBP.
		/// </summary>
		/// <param name="dst">The stream to write the encoded pixmap to.</param>
		/// <param name="options">The options to use when creating the encoder.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if there was an error.</returns>
		public bool Encode (SKWStream dst, SKWebpEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			return SkiaApi.sk_webpencoder_encode (dst.Handle, Handle, &options);
		}

		// Encode (jpeg)

		/// <summary>
		/// Encodes the pixmap as a JPEG.
		/// </summary>
		/// <param name="options">The options to use when creating the encoder.</param>
		/// <returns>Returns the <see cref="SKData" /> wrapping the encoded pixmap.</returns>
		public SKData? Encode (SKJpegEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, options);
			return result ? stream.DetachAsData () : null;
		}

		/// <param name="dst"></param>
		/// <param name="options"></param>
		public bool Encode (Stream dst, SKJpegEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, options);
		}

		/// <summary>
		/// Encodes the pixmap as a JPEG.
		/// </summary>
		/// <param name="dst">The stream to write the encoded pixmap to.</param>
		/// <param name="options">The options to use when creating the encoder.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if there was an error.</returns>
		public bool Encode (SKWStream dst, SKJpegEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			return SkiaApi.sk_jpegencoder_encode (dst.Handle, Handle, &options);
		}

		// Encode (png)

		/// <summary>
		/// Encodes the pixmap as a PNG.
		/// </summary>
		/// <param name="options">The options to use when creating the encoder.</param>
		/// <returns>Returns the <see cref="SKData" /> wrapping the encoded pixmap.</returns>
		public SKData? Encode (SKPngEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, options);
			return result ? stream.DetachAsData () : null;
		}

		/// <param name="dst"></param>
		/// <param name="options"></param>
		public bool Encode (Stream dst, SKPngEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, options);
		}

		/// <summary>
		/// Encodes the pixmap as a PNG.
		/// </summary>
		/// <param name="dst">The stream to write the encoded pixmap to.</param>
		/// <param name="options">The options to use when creating the encoder.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if there was an error.</returns>
		public bool Encode (SKWStream dst, SKPngEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			return SkiaApi.sk_pngencoder_encode (dst.Handle, Handle, &options);
		}

		// ExtractSubset

		/// <summary>
		/// Creates a new <see cref="SKPixmap" /> which is a subset of this pixmap.
		/// </summary>
		/// <param name="subset">The bounds of the pixmap subset to retrieve.</param>
		/// <returns>Returns a subset of the pixmap.</returns>
		public SKPixmap? ExtractSubset (SKRectI subset)
		{
			var result = new SKPixmap ();
			if (!ExtractSubset (result, subset)) {
				result.Dispose ();
				result = null;
			}
			return result;
		}

		/// <summary>
		/// Creates a new <see cref="SKPixmap" /> which is a subset of this pixmap.
		/// </summary>
		/// <param name="result">The pixmap to store the subset pixels.</param>
		/// <param name="subset">The bounds of the pixmap subset to retrieve.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		public bool ExtractSubset (SKPixmap result, SKRectI subset)
		{
			_ = result ?? throw new ArgumentNullException (nameof (result));
			return SkiaApi.sk_pixmap_extract_subset (Handle, result.Handle, &subset);
		}

		// Erase

		/// <summary>
		/// Fill the entire pixmap with the specified color.
		/// </summary>
		/// <param name="color">The color to fill.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were changed, otherwise <see langword="false" />.</returns>
		/// <remarks>If the pixmap's color type does not support alpha (e.g. 565) then the alpha of the color is ignored (treated as opaque). If the color type only supports alpha (e.g. A1 or A8) then the color's R, G, B components are ignored.</remarks>
		public bool Erase (SKColor color) =>
			Erase (color, Rect);

		/// <summary>
		/// Fill the entire pixmap with the specified color.
		/// </summary>
		/// <param name="color">The color to fill.</param>
		/// <param name="subset">The subset of the pixmap to fill.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were changed, otherwise <see langword="false" />.</returns>
		/// <remarks>If the pixmap's color type does not support alpha (e.g. 565) then the alpha of the color is ignored (treated as opaque). If the color type only supports alpha (e.g. A1 or A8) then the color's R, G, B components are ignored.</remarks>
		public bool Erase (SKColor color, SKRectI subset) =>
			SkiaApi.sk_pixmap_erase_color (Handle, (uint)color, &subset);

		/// <param name="color"></param>
		public bool Erase (SKColorF color) =>
			Erase (color, Rect);

		/// <param name="color"></param>
		/// <param name="subset"></param>
		public bool Erase (SKColorF color, SKRectI subset) =>
			SkiaApi.sk_pixmap_erase_color4f (Handle, &color, &subset);

		// ComputeIsOpaque

		public bool ComputeIsOpaque () =>
			SkiaApi.sk_pixmap_compute_is_opaque (Handle);

		// With*

		/// <summary>
		/// Creates a new <see cref="SKPixmap" /> with the same properties as this <see cref="SKPixmap" />, but with the specified color type.
		/// </summary>
		/// <param name="newColorType">The color type.</param>
		/// <returns>Returns the new <see cref="SKPixmap" />.</returns>
		public SKPixmap WithColorType (SKColorType newColorType) =>
			new SKPixmap (Info.WithColorType (newColorType), GetPixels (), RowBytes);

		/// <summary>
		/// Creates a new <see cref="SKPixmap" /> with the same properties as this <see cref="SKPixmap" />, but with the specified color space.
		/// </summary>
		/// <param name="newColorSpace">The color space.</param>
		/// <returns>Returns the new <see cref="SKPixmap" />.</returns>
		public SKPixmap WithColorSpace (SKColorSpace newColorSpace) =>
			new SKPixmap (Info.WithColorSpace (newColorSpace), GetPixels (), RowBytes);

		/// <summary>
		/// Creates a new <see cref="SKPixmap" /> with the same properties as this <see cref="SKPixmap" />, but with the specified transparency type.
		/// </summary>
		/// <param name="newAlphaType">The alpha/transparency type.</param>
		/// <returns>Returns the new <see cref="SKPixmap" />.</returns>
		public SKPixmap WithAlphaType (SKAlphaType newAlphaType) =>
			new SKPixmap (Info.WithAlphaType (newAlphaType), GetPixels (), RowBytes);
	}
}

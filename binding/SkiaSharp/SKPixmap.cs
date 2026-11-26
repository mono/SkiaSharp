using System;
using System.IO;

namespace SkiaSharp
{
	/// <summary>Pairs <see cref="T:SkiaSharp.SKImageInfo" /> with actual pixels and rowbytes.</summary>
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

		/// <summary>Creates an empty instance of <see cref="T:SkiaSharp.SKPixmap" />.</summary>
		/// <remarks />
		public SKPixmap ()
			: this (SkiaApi.sk_pixmap_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException (UnableToCreateInstanceMessage);
			}
		}

		/// <summary>Creates an instance of <see cref="T:SkiaSharp.SKPixmap" />.</summary>
		/// <param name="info">The image information of the pixels.</param>
		/// <param name="addr">The memory address of the pixels.</param>
		/// <remarks />
		public SKPixmap (SKImageInfo info, IntPtr addr)
			: this (info, addr, info.RowBytes)
		{
		}

		/// <summary>Creates an instance of <see cref="T:SkiaSharp.SKPixmap" />.</summary>
		/// <param name="info">The image information of the pixels.</param>
		/// <param name="addr">The memory address of the pixels.</param>
		/// <param name="rowBytes">The number of bytes per row.</param>
		/// <remarks />
		public SKPixmap (SKImageInfo info, IntPtr addr, int rowBytes)
			: this (IntPtr.Zero, true)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			Handle = SkiaApi.sk_pixmap_new_with_params (&cinfo, (void*)addr, (IntPtr)rowBytes);
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException (UnableToCreateInstanceMessage);
			}
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKPixmap" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKPixmap" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_pixmap_destructor (Handle);

		/// <summary>Releases managed resources.</summary>
		/// <remarks />
		protected override void DisposeManaged ()
		{
			base.DisposeManaged ();

			pixelSource = null;
		}

		// Reset

		/// <summary>Reset the pixmap to an empty pixmap.</summary>
		/// <remarks />
		public void Reset ()
		{
			SkiaApi.sk_pixmap_reset (Handle);
			GC.KeepAlive (this);
			pixelSource = null;
		}

		/// <summary>Resets the pixmap to the specified pixels.</summary>
		/// <param name="info">The image information of the pixels.</param>
		/// <param name="addr">The memory address of the pixels.</param>
		/// <param name="rowBytes">The number of bytes per row.</param>
		/// <remarks />
		public void Reset (SKImageInfo info, IntPtr addr, int rowBytes)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			SkiaApi.sk_pixmap_reset_with_params (Handle, &cinfo, (void*)addr, (IntPtr)rowBytes);
			GC.KeepAlive (this);
			pixelSource = null;
		}

		// properties

		/// <summary>Gets the image info.</summary>
		/// <value>The image info.</value>
		/// <remarks />
		public SKImageInfo Info {
			get {
				SKImageInfoNative cinfo;
				SkiaApi.sk_pixmap_get_info (Handle, &cinfo);
				GC.KeepAlive (this);
				return SKImageInfoNative.ToManaged (ref cinfo);
			}
		}

		/// <summary>Gets the image width.</summary>
		/// <value>The image width.</value>
		/// <remarks />
		public int Width => Info.Width;

		/// <summary>Gets the image height.</summary>
		/// <value>The image height.</value>
		/// <remarks />
		public int Height => Info.Height;

		/// <summary>Gets the current size of the pixmap.</summary>
		/// <value>The current size of the pixmap.</value>
		/// <remarks />
		public SKSizeI Size {
			get {
				var info = Info;
				return new SKSizeI (info.Width, info.Height);
			}
		}

		/// <summary>Gets a rectangle with the current width and height.</summary>
		/// <value>A rectangle with the current width and height.</value>
		/// <remarks />
		public SKRectI Rect => SKRectI.Create (Size);

		/// <summary>Gets the color type.</summary>
		/// <value>One of the enumeration values that specifies the color type.</value>
		/// <remarks />
		public SKColorType ColorType => Info.ColorType;

		/// <summary>Gets the alpha type.</summary>
		/// <value>One of the enumeration values that specifies the alpha type.</value>
		/// <remarks />
		public SKAlphaType AlphaType => Info.AlphaType;

		/// <summary>Gets the color space.</summary>
		/// <value>The color space, or <see langword="null" /> if the pixmap has no color space.</value>
		/// <remarks />
		public SKColorSpace? ColorSpace {
			get {
				var result = SKColorSpace.GetObject (SkiaApi.sk_pixmap_get_colorspace (Handle));
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets the number of bytes per pixel.</summary>
		/// <value>The number of bytes per pixel.</value>
		/// <remarks />
		public int BytesPerPixel => Info.BytesPerPixel;

		/// <summary>Gets the bit shift value per pixel.</summary>
		/// <value>The bit shift value.</value>
		/// <remarks />
		public int BitShiftPerPixel => Info.BitShiftPerPixel;

		/// <summary>Gets the number of bytes per row.</summary>
		/// <value>The number of bytes per row.</value>
		/// <remarks />
		public int RowBytes {
			get {
				var result = (int)SkiaApi.sk_pixmap_get_row_bytes (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets the total number of bytes needed to store the pixel data.</summary>
		/// <value>The total number of bytes needed to store the pixel data.</value>
		/// <remarks />
		public int BytesSize => Info.BytesSize;

		/// <summary>Gets the total byte size as a 64-bit integer.</summary>
		/// <value>The total byte size.</value>
		/// <remarks />
		public long BytesSize64 => Info.BytesSize64;

		// pixels

		/// <summary>Returns the memory address of the pixels.</summary>
		/// <returns>The memory address of the pixel data.</returns>
		/// <remarks />
		public IntPtr GetPixels ()
		{
			var result = (IntPtr)SkiaApi.sk_pixmap_get_writable_addr (Handle);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Returns the memory address of the pixels at (x, y).</summary>
		/// <param name="x">The column index, zero or greater, and less than the pixmap width.</param>
		/// <param name="y">The row index, zero or greater, and less than the pixmap height.</param>
		/// <returns>The memory address of the pixel at the specified location.</returns>
		/// <remarks />
		public IntPtr GetPixels (int x, int y)
		{
			var result = (IntPtr)SkiaApi.sk_pixmap_get_writeable_addr_with_xy (Handle, x, y);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Returns a span that wraps the pixel data.</summary>
		/// <returns>Returns the span.</returns>
		/// <remarks>This span is only valid as long as the pixmap is valid</remarks>
		public Span<byte> GetPixelSpan () =>
			GetPixelSpan<byte> (0, 0);

		/// <summary>Gets a span starting at the specified pixel coordinates.</summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <returns>A span of pixel data.</returns>
		/// <remarks />
		public Span<byte> GetPixelSpan (int x, int y) =>
			GetPixelSpan<byte> (x, y);

		/// <summary>Gets a typed span of all pixel data.</summary>
		/// <typeparam name="T">The pixel type.</typeparam>
		/// <returns>A span of pixel data.</returns>
		/// <remarks />
		public unsafe Span<T> GetPixelSpan<T> ()
			where T : unmanaged
		{
			return GetPixelSpan<T> (0, 0);
		}

		/// <summary>Gets a typed span starting at the specified coordinates.</summary>
		/// <typeparam name="T">The pixel type.</typeparam>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <returns>A span of pixel data.</returns>
		/// <remarks />
		public unsafe Span<T> GetPixelSpan<T> (int x, int y)
			where T : unmanaged
		{
			var info = Info;
			if (info.IsEmpty)
				return null;

			var bpp = info.BytesPerPixel;
			if (bpp <= 0)
				return null;

			if (x < 0 || x >= info.Width)
				throw new ArgumentOutOfRangeException (nameof (x));
			if (y < 0 || y >= info.Height)
				throw new ArgumentOutOfRangeException (nameof (y));

			// use the actual stride of the pixmap as it may differ from
			// (Width * BytesPerPixel) when the pixmap is a subset of a larger one
			var rowBytes = RowBytes;

			int spanLength;
			int spanOffset;
			if (typeof (T) == typeof (byte))
			{
				// byte is always valid, so work in bytes

				// the span covers from the first pixel up to and including the
				// last valid pixel of the last row, accounting for row padding
				spanLength = checked((info.Height - 1) * rowBytes + info.Width * bpp);
				spanOffset = (x != 0 || y != 0)
					? info.GetPixelBytesOffset (x, y, rowBytes)
					: 0;
			}
			else
			{
				// other types need to make sure they fit

				var size = sizeof (T);
				if (bpp != size)
					throw new ArgumentException ($"Size of T ({size}) is not the same as the size of each pixel ({bpp}).", nameof (T));

				// a typed span cannot represent a row stride that is not a whole
				// number of T elements, but only when the span actually spans
				// multiple rows; a single-row pixmap never crosses the stride so
				// any stride is fine (the byte overload supports any stride too)
				if (info.Height > 1 && rowBytes % size != 0)
					throw new ArgumentException ($"The row stride ({rowBytes}) is not a multiple of the size of each pixel ({size}).");

				// work in T elements: since each pixel is exactly one T, the only
				// unit conversion is the stride from bytes to elements
				var rowLength = rowBytes / size;

				spanLength = checked((info.Height - 1) * rowLength + info.Width);
				spanOffset = (x != 0 || y != 0)
					? checked(y * rowLength + x)
					: 0;
			}

			var addr = SkiaApi.sk_pixmap_get_writable_addr (Handle);
			GC.KeepAlive (this);
			var span = new Span<T> (addr, spanLength);

			if (spanOffset != 0)
				span = span.Slice (spanOffset);

			return span;
		}

		/// <summary>Returns the color of the pixel at the specified coordinates.</summary>
		/// <param name="x">The column index, zero or greater, and less than the pixmap width.</param>
		/// <param name="y">The row index, zero or greater, and less than the pixmap height.</param>
		/// <returns>Returns the color of the pixel.</returns>
		/// <remarks />
		public SKColor GetPixelColor (int x, int y)
		{
			var result = SkiaApi.sk_pixmap_get_pixel_color (Handle, x, y);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Gets the color of the pixel at the specified coordinates as an SKColorF.</summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <returns>The color of the pixel.</returns>
		/// <remarks />
		public SKColorF GetPixelColorF (int x, int y)
		{
			SKColorF color;
			SkiaApi.sk_pixmap_get_pixel_color4f (Handle, x, y, &color);
			GC.KeepAlive (this);
			return color;
		}

		/// <summary>Gets the alpha value of the pixel at the specified coordinates.</summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <returns>The alpha value (0.0 to 1.0).</returns>
		/// <remarks />
		public float GetPixelAlpha (int x, int y)
		{
			var result = SkiaApi.sk_pixmap_get_pixel_alphaf (Handle, x, y);
			GC.KeepAlive (this);
			return result;
		}

		// ScalePixels

		/// <summary>Scales pixels to a destination pixmap using the legacy filtering quality.</summary>
		/// <param name="destination">The destination bitmap or pixmap.</param>
		/// <param name="quality">The legacy filtering quality.</param>
		/// <returns><see langword="true" /> if scaling succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		[Obsolete ("Use ScalePixels(SKPixmap destination, SKSamplingOptions sampling) instead.", error: true)]
		public bool ScalePixels (SKPixmap destination, SKFilterQuality quality) =>
			ScalePixels (destination, quality.ToSamplingOptions ());

		/// <summary>Scales pixels to the destination using default sampling.</summary>
		/// <param name="destination">The destination pixmap.</param>
		/// <returns>Returns <see langword="true" /> on success.</returns>
		/// <remarks />
		public bool ScalePixels (SKPixmap destination) =>
			ScalePixels (destination, SKSamplingOptions.Default);

		/// <summary>Scales pixels to the destination using the specified sampling options.</summary>
		/// <param name="destination">The destination pixmap.</param>
		/// <param name="sampling">The sampling options for scaling.</param>
		/// <returns>Returns <see langword="true" /> on success.</returns>
		/// <remarks />
		public bool ScalePixels (SKPixmap destination, SKSamplingOptions sampling)
		{
			_ = destination ?? throw new ArgumentNullException (nameof (destination));
			var result = SkiaApi.sk_pixmap_scale_pixels (Handle, destination.Handle, &sampling);
			GC.KeepAlive (this);
			GC.KeepAlive (destination);
			return result;
		}

		// ReadPixels

		/// <summary>Copies the pixels from the image into the specified buffer.</summary>
		/// <param name="dstInfo">The image information describing the destination pixel buffer.</param>
		/// <param name="dstPixels">The pixel buffer to read the pixel data into.</param>
		/// <param name="dstRowBytes">The number of bytes in each row of in the destination buffer.</param>
		/// <param name="srcX">The source x-coordinate to start reading from.</param>
		/// <param name="srcY">The source y-coordinate to start reading from.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		/// <remarks />
		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			var result = SkiaApi.sk_pixmap_read_pixels (Handle, &cinfo, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Copies the pixels from the image into the specified buffer.</summary>
		/// <param name="dstInfo">The image information describing the destination pixel buffer.</param>
		/// <param name="dstPixels">The pixel buffer to read the pixel data into.</param>
		/// <param name="dstRowBytes">The number of bytes in each row of in the destination buffer.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		/// <remarks />
		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes) =>
			ReadPixels (dstInfo, dstPixels, dstRowBytes, 0, 0);

		/// <summary>Copies the pixels from the image into the specified pixmap.</summary>
		/// <param name="pixmap">The pixmap to read the pixel data into.</param>
		/// <param name="srcX">The source x-coordinate to start reading from.</param>
		/// <param name="srcY">The source y-coordinate to start reading from.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		/// <remarks />
		public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY) =>
			ReadPixels (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, srcX, srcY);

		/// <summary>Copies the pixels from the image into the specified pixmap.</summary>
		/// <param name="pixmap">The pixmap to read the pixel data into.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		/// <remarks />
		public bool ReadPixels (SKPixmap pixmap) =>
			ReadPixels (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, 0, 0);

		// Encode

		/// <summary>Encodes the pixmap using the specified format.</summary>
		/// <param name="encoder">The file format used to encode the pixmap.</param>
		/// <param name="quality">The quality level to use for the pixmap.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKData" /> wrapping the encoded pixmap.</returns>
		/// <remarks />
		public SKData? Encode (SKEncodedImageFormat encoder, int quality)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, encoder, quality);
			return result ? stream.DetachAsData () : null;
		}

		/// <summary>Encodes the pixmap to the stream using the specified format and quality.</summary>
		/// <param name="dst">The destination stream.</param>
		/// <param name="encoder">The encoded image format.</param>
		/// <param name="quality">The encoding quality (0-100).</param>
		/// <returns>Returns <see langword="true" /> on success.</returns>
		/// <remarks />
		public bool Encode (Stream dst, SKEncodedImageFormat encoder, int quality)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, encoder, quality);
		}

		/// <summary>Encodes the pixmap using the specified format.</summary>
		/// <param name="dst">The stream to write the encoded pixmap to.</param>
		/// <param name="encoder">The file format used to encode the pixmap.</param>
		/// <param name="quality">The quality level to use for the pixmap.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if there was an error.</returns>
		/// <remarks />
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

		/// <summary>Encodes the pixmap as a WEBP.</summary>
		/// <param name="options">The options to use when creating the encoder.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKData" /> wrapping the encoded pixmap.</returns>
		/// <remarks />
		public SKData? Encode (SKWebpEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, options);
			return result ? stream.DetachAsData () : null;
		}

		/// <summary>Encodes the pixmap to the stream using the specified options.</summary>
		/// <param name="dst">The destination stream.</param>
		/// <param name="options">The encoder options.</param>
		/// <returns>Returns <see langword="true" /> on success.</returns>
		/// <remarks />
		public bool Encode (Stream dst, SKWebpEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, options);
		}

		/// <summary>Encodes the pixmap as a WEBP.</summary>
		/// <param name="dst">The stream to write the encoded pixmap to.</param>
		/// <param name="options">The options to use when creating the encoder.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if there was an error.</returns>
		/// <remarks />
		public bool Encode (SKWStream dst, SKWebpEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			var result = SkiaApi.sk_webpencoder_encode (dst.Handle, Handle, &options);
			GC.KeepAlive (this);
			GC.KeepAlive (dst);
			return result;
		}

		// Encode (jpeg)

		/// <summary>Encodes the pixmap as a JPEG.</summary>
		/// <param name="options">The options to use when creating the encoder.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKData" /> wrapping the encoded pixmap.</returns>
		/// <remarks />
		public SKData? Encode (SKJpegEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, options);
			return result ? stream.DetachAsData () : null;
		}

		/// <summary>Encodes the pixmap to the stream using the specified options.</summary>
		/// <param name="dst">The destination stream.</param>
		/// <param name="options">The encoder options.</param>
		/// <returns>Returns <see langword="true" /> on success.</returns>
		/// <remarks />
		public bool Encode (Stream dst, SKJpegEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, options);
		}

		/// <summary>Encodes the pixmap as a JPEG.</summary>
		/// <param name="dst">The stream to write the encoded pixmap to.</param>
		/// <param name="options">The options to use when creating the encoder.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if there was an error.</returns>
		/// <remarks />
		public bool Encode (SKWStream dst, SKJpegEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			var result = SkiaApi.sk_jpegencoder_encode (dst.Handle, Handle, &options);
			GC.KeepAlive (this);
			GC.KeepAlive (dst);
			return result;
		}

		// Encode (png)

		/// <summary>Encodes the pixmap as a PNG.</summary>
		/// <param name="options">The options to use when creating the encoder.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKData" /> wrapping the encoded pixmap.</returns>
		/// <remarks />
		public SKData? Encode (SKPngEncoderOptions options)
		{
			using var stream = new SKDynamicMemoryWStream ();
			var result = Encode (stream, options);
			return result ? stream.DetachAsData () : null;
		}

		/// <summary>Encodes the pixmap to the stream using the specified options.</summary>
		/// <param name="dst">The destination stream.</param>
		/// <param name="options">The encoder options.</param>
		/// <returns>Returns <see langword="true" /> on success.</returns>
		/// <remarks />
		public bool Encode (Stream dst, SKPngEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, options);
		}

		/// <summary>Encodes the pixmap as a PNG.</summary>
		/// <param name="dst">The stream to write the encoded pixmap to.</param>
		/// <param name="options">The options to use when creating the encoder.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if there was an error.</returns>
		/// <remarks />
		public bool Encode (SKWStream dst, SKPngEncoderOptions options)
		{
			_ = dst ?? throw new ArgumentNullException (nameof (dst));
			var result = SkiaApi.sk_pngencoder_encode (dst.Handle, Handle, &options);
			GC.KeepAlive (this);
			GC.KeepAlive (dst);
			return result;
		}

		// ExtractSubset

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKPixmap" /> which is a subset of this pixmap.</summary>
		/// <param name="subset">The bounds of the pixmap subset to retrieve.</param>
		/// <returns>Returns a subset of the pixmap.</returns>
		/// <remarks />
		public SKPixmap? ExtractSubset (SKRectI subset)
		{
			var result = new SKPixmap ();
			if (!ExtractSubset (result, subset)) {
				result.Dispose ();
				result = null;
			}
			return result;
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKPixmap" /> which is a subset of this pixmap.</summary>
		/// <param name="result">The pixmap to store the subset pixels.</param>
		/// <param name="subset">The bounds of the pixmap subset to retrieve.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		/// <remarks />
		public bool ExtractSubset (SKPixmap result, SKRectI subset)
		{
			_ = result ?? throw new ArgumentNullException (nameof (result));
			var extracted = SkiaApi.sk_pixmap_extract_subset (Handle, result.Handle, &subset);
			GC.KeepAlive (this);
			GC.KeepAlive (result);
			if (extracted)
				result.pixelSource = pixelSource ?? this;
			return extracted;
		}

		// Erase

		/// <summary>Fill the entire pixmap with the specified color.</summary>
		/// <param name="color">The color to fill.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were changed, otherwise <see langword="false" />.</returns>
		/// <remarks>If the pixmap's color type does not support alpha (e.g. 565) then the alpha of the color is ignored (treated as opaque). If the color type only supports alpha (e.g. A1 or A8) then the color's R, G, B components are ignored.</remarks>
		public bool Erase (SKColor color) =>
			Erase (color, Rect);

		/// <summary>Fill the entire pixmap with the specified color.</summary>
		/// <param name="color">The color to fill.</param>
		/// <param name="subset">The subset of the pixmap to fill.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were changed, otherwise <see langword="false" />.</returns>
		/// <remarks>If the pixmap's color type does not support alpha (e.g. 565) then the alpha of the color is ignored (treated as opaque). If the color type only supports alpha (e.g. A1 or A8) then the color's R, G, B components are ignored.</remarks>
		public bool Erase (SKColor color, SKRectI subset)
		{
			var result = SkiaApi.sk_pixmap_erase_color (Handle, (uint)color, &subset);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Fills the entire pixmap with the specified color.</summary>
		/// <param name="color">The color to fill.</param>
		/// <returns>Returns <see langword="true" /> if pixels were changed.</returns>
		/// <remarks />
		public bool Erase (SKColorF color) =>
			Erase (color, Rect);

		/// <summary>Fills a subset of the pixmap with the specified color.</summary>
		/// <param name="color">The color to fill.</param>
		/// <param name="subset">The subset area to fill/process.</param>
		/// <returns>Returns <see langword="true" /> if pixels were changed.</returns>
		/// <remarks />
		public bool Erase (SKColorF color, SKRectI subset)
		{
			var result = SkiaApi.sk_pixmap_erase_color4f (Handle, &color, &subset);
			GC.KeepAlive (this);
			return result;
		}

		// ComputeIsOpaque

		/// <summary>Returns whether all pixels are opaque.</summary>
		/// <returns>Returns <see langword="true" /> if all pixels are opaque.</returns>
		/// <remarks />
		public bool ComputeIsOpaque ()
		{
			var result = SkiaApi.sk_pixmap_compute_is_opaque (Handle);
			GC.KeepAlive (this);
			return result;
		}

		// With*

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKPixmap" /> with the same properties as this <see cref="T:SkiaSharp.SKPixmap" />, but with the specified color type.</summary>
		/// <param name="newColorType">The color type.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKPixmap" />.</returns>
		/// <remarks />
		public SKPixmap WithColorType (SKColorType newColorType) =>
			WithPixelSource (new SKPixmap (Info.WithColorType (newColorType), GetPixels (), RowBytes));

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKPixmap" /> with the same properties as this <see cref="T:SkiaSharp.SKPixmap" />, but with the specified color space.</summary>
		/// <param name="newColorSpace">The color space.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKPixmap" />.</returns>
		/// <remarks />
		public SKPixmap WithColorSpace (SKColorSpace newColorSpace) =>
			WithPixelSource (new SKPixmap (Info.WithColorSpace (newColorSpace), GetPixels (), RowBytes));

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKPixmap" /> with the same properties as this <see cref="T:SkiaSharp.SKPixmap" />, but with the specified transparency type.</summary>
		/// <param name="newAlphaType">The alpha/transparency type.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKPixmap" />.</returns>
		/// <remarks />
		public SKPixmap WithAlphaType (SKAlphaType newAlphaType) =>
			WithPixelSource (new SKPixmap (Info.WithAlphaType (newAlphaType), GetPixels (), RowBytes));

		private SKPixmap WithPixelSource (SKPixmap pixmap)
		{
			// the returned pixmap wraps this pixmap's pixel memory, so it must root
			// the ultimate pixel owner for its whole lifetime
			pixmap.pixelSource = pixelSource ?? this;
			return pixmap;
		}
	}
}

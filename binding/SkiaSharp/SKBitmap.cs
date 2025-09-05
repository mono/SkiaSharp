#nullable disable

using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	// TODO: keep in mind SKBitmap may be going away (according to Google)
	// TODO: `ComputeIsOpaque` may be useful
	// TODO: `GenerationID` may be useful
	// TODO: `GetAddr` and `GetPixel` are confusing

	/// <summary>
	/// The <see cref="T:SkiaSharp.SKBitmap" /> specifies a raster bitmap.
	/// </summary>
	/// <remarks><para>A bitmap has an integer width and height, and a format (color type), and a pointer to the actual pixels. Bitmaps can be drawn into a <see cref="T:SkiaSharp.SKCanvas" />, but they are also used to specify the target of a <see cref="T:SkiaSharp.SKCanvas" />' drawing operations.</para><para>A <see cref="T:SkiaSharp.SKBitmap" /> exposes <see cref="M:SkiaSharp.SKBitmap.GetPixels" />, which lets a caller write its pixels. To retrieve a pointer to the raw image data of the bitmap, call the <see cref="M:SkiaSharp.SKBitmap.LockPixels" /> method, and then call the <see cref="M:SkiaSharp.SKBitmap.GetPixels" /> method to get a pointer to the image data.  Once you no longer need to use the raw data pointer, call the <see cref="M:SkiaSharp.SKBitmap.UnlockPixels" /> method. The raw data is laid out in the format configured at the time that the bitmap was created.</para><para>(Note: As of SkiaSharp 1.60.0, calls to <see cref="M:SkiaSharp.SKBitmap.LockPixels" /> and <see cref="M:SkiaSharp.SKBitmap.UnlockPixels" /> are no longer required, and they no longer exist as part of the API.)</para></remarks>
	public unsafe class SKBitmap : SKObject, ISKSkipObjectRegistration
	{
		private const string UnsupportedColorTypeMessage = "Setting the ColorTable is only supported for bitmaps with ColorTypes of Index8.";
		private const string UnableToAllocatePixelsMessage = "Unable to allocate pixels for the bitmap.";

		internal SKBitmap (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Default constructor that creates a bitmap with zero width and height, and no pixels. Its color type is set to <see cref="F:SkiaSharp.SKColorType.Unknown" />.
		/// </summary>
		/// <remarks>This constructor does not allocate a backing store for the bitmap.</remarks>
		public SKBitmap ()
			: this (SkiaApi.sk_bitmap_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKBitmap instance.");
			}
		}

		/// <summary>
		/// Creates a bitmap with the given width, height and opacity with color type set to <see cref="F:SkiaSharp.SKImageInfo.PlatformColorType" />
		/// </summary>
		/// <param name="width">The desired width in pixels.</param>
		/// <param name="height">The desired height in pixels.</param>
		/// <param name="isOpaque">If true, sets the <see cref="T:SkiaSharp.SKAlphaType" /> to <see cref="F:SkiaSharp.SKAlphaType.Opaque" />, otherwise it sets it to <see cref="F:SkiaSharp.SKAlphaType.Premul" />.</param>
		/// <remarks>This constructor might throw an exception if it is not possible to create a bitmap with the specified configuration (for example, the image info requires a color table, and there is no color table).</remarks>
		public SKBitmap (int width, int height, bool isOpaque = false)
			: this (width, height, SKImageInfo.PlatformColorType, isOpaque ? SKAlphaType.Opaque : SKAlphaType.Premul)
		{
		}

		/// <summary>
		/// Creates a bitmap with the given width, height, color type and alpha type.
		/// </summary>
		/// <param name="width">The desired width in pixels.</param>
		/// <param name="height">The desired height in pixels.</param>
		/// <param name="colorType">The desired <see cref="T:SkiaSharp.SKColorType" />.</param>
		/// <param name="alphaType">The desired <see cref="T:SkiaSharp.SKAlphaType" />.</param>
		/// <remarks>This constructor might throw an exception if it is not possible to create a bitmap with the specified configuration (for example, the image info requires a color table, and there is no color table).</remarks>
		public SKBitmap (int width, int height, SKColorType colorType, SKAlphaType alphaType)
			: this (new SKImageInfo (width, height, colorType, alphaType))
		{
		}

		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colorType"></param>
		/// <param name="alphaType"></param>
		/// <param name="colorspace"></param>
		public SKBitmap (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorspace)
			: this (new SKImageInfo (width, height, colorType, alphaType, colorspace))
		{
		}

		/// <summary>
		/// Constructor that configures the bitmap based on an <see cref="T:SkiaSharp.SKImageInfo" /> specification.
		/// </summary>
		/// <param name="info">The description of the desired image format.</param>
		/// <remarks>This constructor might throw an exception if it is not possible to create a bitmap with the specified configuration (for example, the image info requires a color table, and there is no color table).</remarks>
		public SKBitmap (SKImageInfo info)
			: this (info, info.RowBytes)
		{
		}

		/// <summary>
		/// Constructor that configures the bitmap based on an <see cref="T:SkiaSharp.SKImageInfo" /> specification, and the specified number of bytes per row (the stride size)
		/// </summary>
		/// <param name="info">The description of the desired image format.</param>
		/// <param name="rowBytes">The number of bytes per row.</param>
		/// <remarks>This constructor might throw an exception if it is not possible to create a bitmap with the specified configuration (for example, the image info requires a color table, and there is no color table).</remarks>
		public SKBitmap (SKImageInfo info, int rowBytes)
			: this ()
		{
			if (!TryAllocPixels (info, rowBytes)) {
				throw new Exception (UnableToAllocatePixelsMessage);
			}
		}

		/// <summary>
		/// Constructor that configures the bitmap based on an <see cref="T:SkiaSharp.SKImageInfo" /> specification.
		/// </summary>
		/// <param name="info">The description of the desired image format.</param>
		/// <param name="flags">The additional flags.</param>
		/// <remarks>This constructor might throw an exception if it is not possible to create a bitmap with the specified configuration (for example, the image info requires a color table, and there is no color table).</remarks>
		public SKBitmap (SKImageInfo info, SKBitmapAllocFlags flags)
			: this ()
		{
			if (!TryAllocPixels (info, flags)) {
				throw new Exception (UnableToAllocatePixelsMessage);
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_bitmap_destructor (Handle);

		// TryAllocPixels

		/// <summary>
		/// Allocates the memory for the bitmap using the specified image information.
		/// </summary>
		/// <param name="info">The image information describing the pixels.</param>
		/// <returns>Returns true if the allocation was successful, otherwise false.</returns>
		public bool TryAllocPixels (SKImageInfo info)
		{
			return TryAllocPixels (info, info.RowBytes);
		}

		/// <summary>
		/// Allocates the memory for the bitmap using the specified image information.
		/// </summary>
		/// <param name="info">The image information describing the pixels.</param>
		/// <param name="rowBytes">The stride of the pixels being allocated.</param>
		/// <returns>Returns true if the allocation was successful, otherwise false.</returns>
		public bool TryAllocPixels (SKImageInfo info, int rowBytes)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return SkiaApi.sk_bitmap_try_alloc_pixels (Handle, &cinfo, (IntPtr)rowBytes);
		}

		/// <summary>
		/// Allocates the memory for the bitmap using the specified image information.
		/// </summary>
		/// <param name="info">The image information describing the pixels.</param>
		/// <param name="flags">The additional flags.</param>
		/// <returns>Returns true if the allocation was successful, otherwise false.</returns>
		public bool TryAllocPixels (SKImageInfo info, SKBitmapAllocFlags flags)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return SkiaApi.sk_bitmap_try_alloc_pixels_with_flags (Handle, &cinfo, (uint)flags);
		}

		// Reset

		/// <summary>
		/// Reset the bitmap to its initial state.
		/// </summary>
		/// <remarks>The result is a bitmap with zero width and height, and no pixels. Its color type is set to <see cref="F:SkiaSharp.SKColorType.Unknown" />. If we are a (shared) owner of the pixels, that ownership is decremented.</remarks>
		public void Reset ()
		{
			SkiaApi.sk_bitmap_reset (Handle);
		}

		// SetImmutable

		/// <summary>
		/// Marks the bitmap as immutable.
		/// </summary>
		/// <remarks>Marks this bitmap as immutable, meaning that the contents of its pixels will not change for the lifetime of the bitmap and of the underlying pixelref. This state can be set, but it cannot be cleared once it is set. This state propagates to all other bitmaps that share the same pixelref.</remarks>
		public void SetImmutable ()
		{
			SkiaApi.sk_bitmap_set_immutable (Handle);
		}

		// Erase

		/// <summary>
		/// Fill the entire bitmap with the specified color.
		/// </summary>
		/// <param name="color">The color to fill.</param>
		/// <remarks>If the bitmap's color type does not support alpha (e.g. 565) then the alpha of the color is ignored (treated as opaque). If the color type only supports alpha (e.g. A1 or A8) then the color's R, G, B components are ignored.</remarks>
		public void Erase (SKColor color)
		{
			SkiaApi.sk_bitmap_erase (Handle, (uint)color);
		}

		/// <summary>
		/// Fill the specified area of this bitmap with the specified color.
		/// </summary>
		/// <param name="color">The color to fill.</param>
		/// <param name="rect">The area to fill.</param>
		/// <remarks>If the bitmap's color type does not support alpha (e.g. 565) then the alpha of the color is ignored (treated as opaque). If the color type only supports alpha (e.g. A1 or A8) then the color's R, G, B components are ignored.</remarks>
		public void Erase (SKColor color, SKRectI rect)
		{
			SkiaApi.sk_bitmap_erase_rect (Handle, (uint)color, &rect);
		}

		// GetAddress

		/// <param name="x"></param>
		/// <param name="y"></param>
		public IntPtr GetAddress (int x, int y) =>
			(IntPtr)SkiaApi.sk_bitmap_get_addr (Handle, x, y);

		// Pixels (color)

		/// <summary>
		/// Returns the color for the pixel at the specified location.
		/// </summary>
		/// <param name="x">The x-cordinate.</param>
		/// <param name="y">The y-cordinate.</param>
		/// <returns>Alpha only color types return black with the appropriate alpha set. The value is undefined for <see cref="F:SkiaSharp.SKColorType.Unknown" />, if the coordinates are out of bounds, if the bitmap does not have any pixels, or has not be locked with <see cref="M:SkiaSharp.SKBitmap.LockPixels" />.</returns>
		/// <remarks>In most cases this will require unpremultiplying the color.</remarks>
		public SKColor GetPixel (int x, int y)
		{
			return SkiaApi.sk_bitmap_get_pixel_color (Handle, x, y);
		}

		/// <summary>
		/// Sets the color of the pixel at a specified location.
		/// </summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="color">The color to set.</param>
		/// <remarks>This method will set the color of the pixel on the bitmap to the specified <paramref name="color" /> performing any necessary color conversions to the format of the bitmap.</remarks>
		public void SetPixel (int x, int y, SKColor color)
		{
			var info = Info;
			if (x < 0 || x >= info.Width)
				throw new ArgumentOutOfRangeException (nameof (x));
			if (y < 0 || y >= info.Height)
				throw new ArgumentOutOfRangeException (nameof (y));

			using var canvas = new SKCanvas (this);
			canvas.DrawPoint (x, y, color);
		}

		// Copy

		/// <summary>
		/// Returns true if this bitmap's pixels can be converted into the requested color type, such that <see cref="M:SkiaSharp.SKBitmap.Copy" /> or <see cref="M:SkiaSharp.SKBitmap.CopyTo(SkiaSharp.SKBitmap)" /> could succeed.
		/// </summary>
		/// <param name="colorType">The color type to check with.</param>
		/// <returns>Returns true if this bitmap's pixels can be converted into the requested color type.</returns>
		public bool CanCopyTo (SKColorType colorType)
		{
			// TODO: optimize as this does more work that we really want

			if (colorType == SKColorType.Unknown)
				return false;

			using var bmp = new SKBitmap ();

			var info = Info
				.WithColorType (colorType)
				.WithSize (1, 1);
			return bmp.TryAllocPixels (info);
		}

		/// <summary>
		/// Copies the contents of the bitmap and returns the copy.
		/// </summary>
		/// <returns>The copy of the bitmap, or <paramref name="null" /> on error.</returns>
		public SKBitmap Copy ()
		{
			return Copy (ColorType);
		}

		/// <summary>
		/// Copies the contents of the bitmap with the specified color type and returns the copy.
		/// </summary>
		/// <param name="colorType">The color type to use for the copy of the bitmap.</param>
		/// <returns>The copy of the bitmap, or <paramref name="null" /> on error.</returns>
		public SKBitmap Copy (SKColorType colorType)
		{
			var destination = new SKBitmap ();
			if (!CopyTo (destination, colorType)) {
				destination.Dispose ();
				destination = null;
			}
			return destination;
		}

		/// <summary>
		/// Copies the contents of the bitmap into the specified bitmap.
		/// </summary>
		/// <param name="destination">The bitmap to received the copied contents.</param>
		/// <returns>Returns true if the copy was made.</returns>
		public bool CopyTo (SKBitmap destination)
		{
			if (destination == null) {
				throw new ArgumentNullException (nameof (destination));
			}
			return CopyTo (destination, ColorType);
		}

		/// <summary>
		/// Copies the contents of the bitmap into the specified bitmap.
		/// </summary>
		/// <param name="destination">The bitmap to received the copied contents.</param>
		/// <param name="colorType">The color type to use for the copy of the bitmap.</param>
		/// <returns>Returns true if the copy was made.</returns>
		public bool CopyTo (SKBitmap destination, SKColorType colorType)
		{
			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			if (colorType == SKColorType.Unknown)
				return false;

			using var srcPixmap = PeekPixels ();
			if (srcPixmap == null)
				return false;

			using var temp = new SKBitmap ();

			var dstInfo = srcPixmap.Info.WithColorType (colorType);
			if (!temp.TryAllocPixels (dstInfo))
				return false;

			using var canvas = new SKCanvas (temp);

			using var paint = new SKPaint {
				Shader = ToShader (),
				BlendMode = SKBlendMode.Src
			};

			canvas.DrawPaint (paint);

			destination.Swap (temp);
			return true;
		}

		// ExtractSubset

		/// <summary>
		/// Retrieve a subset of this bitmap.
		/// </summary>
		/// <param name="destination">The bitmap that will be set to a subset of this bitmap.</param>
		/// <param name="subset">The rectangle of pixels in this bitmap that the destination will reference.</param>
		/// <returns>Returns true if the subset was retrieved, false otherwise.</returns>
		/// <remarks>If possible, the retrieved bitmap will share the pixel memory, and just point into a subset of it. However, if the color type does not support this, a local copy will be made and associated with the destination bitmap.</remarks>
		public bool ExtractSubset (SKBitmap destination, SKRectI subset)
		{
			if (destination == null) {
				throw new ArgumentNullException (nameof (destination));
			}
			return SkiaApi.sk_bitmap_extract_subset (Handle, destination.Handle, &subset);
		}

		// ExtractAlpha

		/// <summary>
		/// Retrieve the alpha layer of this bitmap.
		/// </summary>
		/// <param name="destination">The bitmap to be filled with alpha layer.</param>
		/// <returns>Returns true if the alpha layer was retrieved, false otherwise.</returns>
		public bool ExtractAlpha (SKBitmap destination)
		{
			return ExtractAlpha (destination, null, out var offset);
		}

		/// <summary>
		/// Retrieve the alpha layer of this bitmap.
		/// </summary>
		/// <param name="destination">The bitmap to be filled with alpha layer.</param>
		/// <param name="offset">The top-left coordinate to position the retrieved bitmap so that it visually lines up with the original.</param>
		/// <returns>Returns true if the alpha layer was retrieved, false otherwise.</returns>
		public bool ExtractAlpha (SKBitmap destination, out SKPointI offset)
		{
			return ExtractAlpha (destination, null, out offset);
		}

		/// <summary>
		/// Retrieve the alpha layer of this bitmap after applying the specified paint.
		/// </summary>
		/// <param name="destination">The bitmap to be filled with alpha layer.</param>
		/// <param name="paint">The paint to draw with.</param>
		/// <returns>Returns true if the alpha layer was retrieved, false otherwise.</returns>
		public bool ExtractAlpha (SKBitmap destination, SKPaint paint)
		{
			return ExtractAlpha (destination, paint, out var offset);
		}

		/// <summary>
		/// Retrieve the alpha layer of this bitmap after applying the specified paint.
		/// </summary>
		/// <param name="destination">The bitmap to be filled with alpha layer.</param>
		/// <param name="paint">The paint to draw with.</param>
		/// <param name="offset">The top-left coordinate to position the retrieved bitmap so that it visually lines up with the original.</param>
		/// <returns>Returns true if the alpha layer was retrieved, false otherwise.</returns>
		public bool ExtractAlpha (SKBitmap destination, SKPaint paint, out SKPointI offset)
		{
			if (destination == null) {
				throw new ArgumentNullException (nameof (destination));
			}
			fixed (SKPointI* o = &offset) {
				return SkiaApi.sk_bitmap_extract_alpha (Handle, destination.Handle, paint == null ? IntPtr.Zero : paint.Handle, o);
			}
		}

		// properties

		/// <summary>
		/// Gets a value indicating whether or not the bitmap is valid enough to be drawn.
		/// </summary>
		public bool ReadyToDraw => SkiaApi.sk_bitmap_ready_to_draw (Handle);

		/// <summary>
		/// Gets an instance of <see cref="T:SkiaSharp.SKImageInfo" /> with all the properties of the bitmap.
		/// </summary>
		public SKImageInfo Info {
			get {
				SKImageInfoNative cinfo;
				SkiaApi.sk_bitmap_get_info (Handle, &cinfo);
				return SKImageInfoNative.ToManaged (ref cinfo);
			}
		}

		/// <summary>
		/// Gets the width of the bitmap.
		/// </summary>
		public int Width {
			get { return Info.Width; }
		}

		/// <summary>
		/// Gets the height of the bitmap.
		/// </summary>
		public int Height {
			get { return Info.Height; }
		}

		/// <summary>
		/// Gets the color type of the bitmap.
		/// </summary>
		public SKColorType ColorType {
			get { return Info.ColorType; }
		}

		/// <summary>
		/// Gets the configured <see cref="T:SkiaSharp.SKAlphaType" /> for the bitmap.
		/// </summary>
		/// <value>The configured <see cref="T:SkiaSharp.SKAlphaType" />.</value>
		/// <remarks>This determines the kind of encoding used for the alpha channel, opaque, premultiplied or unpremultiplied.</remarks>
		public SKAlphaType AlphaType {
			get { return Info.AlphaType; }
		}

		/// <summary>
		/// Gets the color space of the bitmap.
		/// </summary>
		public SKColorSpace ColorSpace {
			get { return Info.ColorSpace; }
		}

		/// <summary>
		/// Gets the number of bytes used per pixel.
		/// </summary>
		/// <remarks>This is calculated from the <see cref="P:SkiaSharp.SKBitmap.ColorType" />. If the color type is <see cref="F:SkiaSharp.SKColorType.Unknown" />, then the value will be 0.</remarks>
		public int BytesPerPixel {
			get { return Info.BytesPerPixel; }
		}

		/// <summary>
		/// The number of bytes per row.
		/// </summary>
		/// <remarks>The same as <see cref="P:SkiaSharp.SKImageInfo.RowBytes" />.</remarks>
		public int RowBytes {
			get { return (int)SkiaApi.sk_bitmap_get_row_bytes (Handle); }
		}

		/// <summary>
		/// Returns the byte size of the pixels, based on the <see cref="P:SkiaSharp.SKBitmap.Height" /> and <see cref="P:SkiaSharp.SKBitmap.RowBytes" />.
		/// </summary>
		/// <value>The byte size of the pixels.</value>
		/// <remarks>Note: this truncates the result to 32-bits.</remarks>
		public int ByteCount {
			get { return (int)SkiaApi.sk_bitmap_get_byte_count (Handle); }
		}

		// *Pixels*

		/// <summary>
		/// Returns the address of the pixels for this bitmap.
		/// </summary>
		/// <returns>Returns a pointer to the region that contains the pixel data for this bitmap. This might return <see langword="IntPtr.Zero" /> if there is no pixel buffer associated with this bitmap.</returns>
		public IntPtr GetPixels () =>
			GetPixels (out _);

		/// <summary>
		/// Returns a span that wraps the pixel data.
		/// </summary>
		/// <returns>Returns the span.</returns>
		/// <remarks>This span is only valid as long as the bitmap is valid</remarks>
		public Span<byte> GetPixelSpan () =>
			new Span<byte> ((void*)GetPixels (out var length), (int)length);

		public Span<byte> GetPixelSpan (int x, int y) =>
			GetPixelSpan ().Slice (Info.GetPixelBytesOffset (x, y));

		/// <summary>
		/// Returns the address of the pixels for this bitmap.
		/// </summary>
		/// <param name="length">The length of the pixel buffer of the bitmap.</param>
		/// <returns>Returns a pointer to the region that contains the pixel data for this bitmap. This might return <see langword="IntPtr.Zero" /> if there is no pixel buffer associated with this bitmap.</returns>
		public IntPtr GetPixels (out IntPtr length)
		{
			fixed (IntPtr* l = &length) {
				return (IntPtr)SkiaApi.sk_bitmap_get_pixels (Handle, l);
			}
		}

		/// <summary>
		/// Replaces the current pixel address for the bitmap.
		/// </summary>
		/// <param name="pixels">The new pixel address.</param>
		public void SetPixels (IntPtr pixels)
		{
			SkiaApi.sk_bitmap_set_pixels (Handle, (void*)pixels);
		}

		// more properties

		/// <summary>
		/// Gets a copy of all the pixel data as a byte array.
		/// </summary>
		/// <value>The pixel data.</value>
		public byte[] Bytes {
			get {
				var array = GetPixelSpan ().ToArray ();
				GC.KeepAlive (this);
				return array;
			}
		}

		/// <summary>
		/// Gets all the pixels as an array of colors.
		/// </summary>
		public SKColor[] Pixels {
			get {
				var info = Info;
				var pixels = new SKColor[info.Width * info.Height];
				fixed (SKColor* p = pixels) {
					SkiaApi.sk_bitmap_get_pixel_colors (Handle, (uint*)p);
				}
				return pixels;
			}
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				var info = Info;
				if (info.Width * info.Height != value.Length)
					throw new ArgumentException ($"The number of pixels must equal Width x Height, or {info.Width * info.Height}.", nameof (value));

				fixed (SKColor* v = value) {
					var tempInfo = new SKImageInfo (info.Width, info.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
					using var temp = new SKBitmap ();
					temp.InstallPixels (tempInfo, (IntPtr)v);

					using var shader = temp.ToShader ();

					using var canvas = new SKCanvas (this);
					using var paint = new SKPaint {
						Shader = shader,
						BlendMode = SKBlendMode.Src
					};
					canvas.DrawPaint (paint);
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the bitmap has empty dimensions.
		/// </summary>
		/// <remarks>In most cases, <see cref="P:SkiaSharp.SKBitmap.DrawsNothing" /> will return the desired result as it checks <see cref="P:SkiaSharp.SKBitmap.IsNull" /> as well.</remarks>
		public bool IsEmpty {
			get { return Info.IsEmpty; }
		}

		/// <summary>
		/// Gets a value indicating whether the bitmap has any pixelref.
		/// </summary>
		/// <remarks>This can return true even if the dimensions of the bitmap are not empty. In most cases, <see cref="P:SkiaSharp.SKBitmap.DrawsNothing" /> will return the desired result as it checks <see cref="P:SkiaSharp.SKBitmap.IsEmpty" /> as well.</remarks>
		public bool IsNull {
			get { return SkiaApi.sk_bitmap_is_null (Handle); }
		}

		/// <summary>
		/// Gets a value indicating whether drawing this bitmap has any effect.
		/// </summary>
		public bool DrawsNothing {
			get { return IsEmpty || IsNull; }
		}

		/// <summary>
		/// Indicates if the bitmap contents are immutable.
		/// </summary>
		/// <value>Returns <see langword="true" /> if it is immutable, <see langword="false" /> otherwise.</value>
		/// <remarks>Immutability means that the contents of its pixels will not change for the lifetime of the bitmap.</remarks>
		public bool IsImmutable {
			get { return SkiaApi.sk_bitmap_is_immutable (Handle); }
		}

		// DecodeBounds

		/// <summary>
		/// Decode the bitmap information using the specified stream.
		/// </summary>
		/// <param name="stream">The stream to decode.</param>
		/// <returns>The decoded bitmap information, or <see cref="F:SkiaSharp.SKImageInfo.Empty" /> if there was an error.</returns>
		public static SKImageInfo DecodeBounds (Stream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}
			using (var codec = SKCodec.Create (stream)) {
				return codec?.Info ?? SKImageInfo.Empty;
			}
		}

		/// <summary>
		/// Decode the bitmap information using the specified stream.
		/// </summary>
		/// <param name="stream">The stream to decode.</param>
		/// <returns>The decoded bitmap information, or <see cref="F:SkiaSharp.SKImageInfo.Empty" /> if there was an error.</returns>
		public static SKImageInfo DecodeBounds (SKStream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}
			using (var codec = SKCodec.Create (stream)) {
				return codec?.Info ?? SKImageInfo.Empty;
			}
		}

		/// <summary>
		/// Decode the bitmap information using the specified data.
		/// </summary>
		/// <param name="data">The data to decode.</param>
		/// <returns>The decoded bitmap information, or <see cref="F:SkiaSharp.SKImageInfo.Empty" /> if there was an error.</returns>
		public static SKImageInfo DecodeBounds (SKData data)
		{
			if (data == null) {
				throw new ArgumentNullException (nameof (data));
			}
			using (var codec = SKCodec.Create (data)) {
				return codec?.Info ?? SKImageInfo.Empty;
			}
		}

		/// <summary>
		/// Decode the bitmap information for the specified filename.
		/// </summary>
		/// <param name="filename">The filename of the bitmap to decode.</param>
		/// <returns>The decoded bitmap information, or <see cref="F:SkiaSharp.SKImageInfo.Empty" /> if there was an error.</returns>
		public static SKImageInfo DecodeBounds (string filename)
		{
			if (filename == null) {
				throw new ArgumentNullException (nameof (filename));
			}
			using (var codec = SKCodec.Create (filename)) {
				return codec?.Info ?? SKImageInfo.Empty;
			}
		}

		/// <summary>
		/// Decode the bitmap information using the specified byte buffer.
		/// </summary>
		/// <param name="buffer">The byte buffer to decode.</param>
		/// <returns>The decoded bitmap information, or <see cref="F:SkiaSharp.SKImageInfo.Empty" /> if there was an error.</returns>
		public static SKImageInfo DecodeBounds (byte[] buffer) =>
			DecodeBounds (buffer.AsSpan ());

		/// <param name="buffer"></param>
		public static SKImageInfo DecodeBounds (ReadOnlySpan<byte> buffer)
		{
			fixed (byte* b = buffer) {
				using var skdata = SKData.Create ((IntPtr)b, buffer.Length);
				using var codec = SKCodec.Create (skdata);
				return codec?.Info ?? SKImageInfo.Empty;
			}
		}

		// Decode

		/// <summary>
		/// Decode a bitmap using the specified codec.
		/// </summary>
		/// <param name="codec">The codec to decode.</param>
		/// <returns>The decoded bitmap, or <paramref name="null" /> on error.</returns>
		public static SKBitmap Decode (SKCodec codec)
		{
			if (codec == null) {
				throw new ArgumentNullException (nameof (codec));
			}
			var info = codec.Info;
			if (info.AlphaType == SKAlphaType.Unpremul) {
				info.AlphaType = SKAlphaType.Premul;
			}
			return Decode (codec, info);
		}

		/// <summary>
		/// Decode a bitmap using the specified codec and destination image information.
		/// </summary>
		/// <param name="codec">The codec to decode.</param>
		/// <param name="bitmapInfo">The destination image information.</param>
		/// <returns>The decoded bitmap, or <paramref name="null" /> on error.</returns>
		public static SKBitmap Decode (SKCodec codec, SKImageInfo bitmapInfo)
		{
			if (codec == null) {
				throw new ArgumentNullException (nameof (codec));
			}

			var bitmap = new SKBitmap (bitmapInfo);
			var result = codec.GetPixels (bitmapInfo, bitmap.GetPixels (out var length));
			if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput) {
				bitmap.Dispose ();
				bitmap = null;
			}
			return bitmap;
		}

		/// <summary>
		/// Decode a bitmap using the specified stream and destination image information.
		/// </summary>
		/// <param name="stream">The stream to decode.</param>
		/// <returns>The decoded bitmap, or <paramref name="null" /> on error.</returns>
		public static SKBitmap Decode (Stream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}
			using (var codec = SKCodec.Create (stream)) {
				if (codec == null) {
					return null;
				}
				return Decode (codec);
			}
		}

		/// <summary>
		/// Decode a bitmap using the specified stream and destination image information.
		/// </summary>
		/// <param name="stream">The stream to decode.</param>
		/// <param name="bitmapInfo">The destination image information.</param>
		/// <returns>The decoded bitmap, or <paramref name="null" /> on error.</returns>
		public static SKBitmap Decode (Stream stream, SKImageInfo bitmapInfo)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}
			using (var codec = SKCodec.Create (stream)) {
				if (codec == null) {
					return null;
				}
				return Decode (codec, bitmapInfo);
			}
		}

		/// <summary>
		/// Decode a bitmap using the specified stream.
		/// </summary>
		/// <param name="stream">The stream to decode.</param>
		/// <returns>The decoded bitmap, or <paramref name="null" /> on error.</returns>
		public static SKBitmap Decode (SKStream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}
			using (var codec = SKCodec.Create (stream)) {
				if (codec == null) {
					return null;
				}
				return Decode (codec);
			}
		}

		/// <summary>
		/// Decode a bitmap using the specified stream and destination image information.
		/// </summary>
		/// <param name="stream">The stream to decode.</param>
		/// <param name="bitmapInfo">The destination image information.</param>
		/// <returns>The decoded bitmap, or <paramref name="null" /> on error.</returns>
		public static SKBitmap Decode (SKStream stream, SKImageInfo bitmapInfo)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}
			using (var codec = SKCodec.Create (stream)) {
				if (codec == null) {
					return null;
				}
				return Decode (codec, bitmapInfo);
			}
		}

		/// <summary>
		/// Decode a bitmap using the specified data.
		/// </summary>
		/// <param name="data">The data to decode.</param>
		/// <returns>The decoded bitmap, or <paramref name="null" /> on error.</returns>
		public static SKBitmap Decode (SKData data)
		{
			if (data == null) {
				throw new ArgumentNullException (nameof (data));
			}
			using (var codec = SKCodec.Create (data)) {
				if (codec == null) {
					return null;
				}
				return Decode (codec);
			}
		}

		/// <summary>
		/// Decode a bitmap using the specified data and destination image information.
		/// </summary>
		/// <param name="data">The data to decode.</param>
		/// <param name="bitmapInfo">The destination image information.</param>
		/// <returns>The decoded bitmap, or <paramref name="null" /> on error.</returns>
		public static SKBitmap Decode (SKData data, SKImageInfo bitmapInfo)
		{
			if (data == null) {
				throw new ArgumentNullException (nameof (data));
			}
			using (var codec = SKCodec.Create (data)) {
				if (codec == null) {
					return null;
				}
				return Decode (codec, bitmapInfo);
			}
		}

		/// <summary>
		/// Decode a bitmap for the specified filename.
		/// </summary>
		/// <param name="filename">The filename of the bitmap to decode.</param>
		/// <returns>The decoded bitmap, or <paramref name="null" /> on error.</returns>
		public static SKBitmap Decode (string filename)
		{
			if (filename == null) {
				throw new ArgumentNullException (nameof (filename));
			}
			using (var codec = SKCodec.Create (filename)) {
				if (codec == null) {
					return null;
				}
				return Decode (codec);
			}
		}

		/// <summary>
		/// Decode a bitmap for the specified filename and destination image information.
		/// </summary>
		/// <param name="filename">The filename of the bitmap to decode.</param>
		/// <param name="bitmapInfo">The destination image information.</param>
		/// <returns>The decoded bitmap, or <paramref name="null" /> on error.</returns>
		public static SKBitmap Decode (string filename, SKImageInfo bitmapInfo)
		{
			if (filename == null) {
				throw new ArgumentNullException (nameof (filename));
			}
			using (var codec = SKCodec.Create (filename)) {
				if (codec == null) {
					return null;
				}
				return Decode (codec, bitmapInfo);
			}
		}

		/// <summary>
		/// Decode a bitmap using the specified byte buffer.
		/// </summary>
		/// <param name="buffer">The byte buffer to decode.</param>
		/// <returns>The decoded bitmap, or <paramref name="null" /> on error.</returns>
		public static SKBitmap Decode (byte[] buffer) =>
			Decode (buffer.AsSpan ());

		/// <summary>
		/// Decode a bitmap using the specified byte buffer and destination image information.
		/// </summary>
		/// <param name="buffer">The byte buffer to decode.</param>
		/// <param name="bitmapInfo">The destination image information.</param>
		/// <returns>The decoded bitmap, or <paramref name="null" /> on error.</returns>
		public static SKBitmap Decode (byte[] buffer, SKImageInfo bitmapInfo) =>
			Decode (buffer.AsSpan (), bitmapInfo);

		/// <param name="buffer"></param>
		public static SKBitmap Decode (ReadOnlySpan<byte> buffer)
		{
			fixed (byte* b = buffer) {
				using var skdata = SKData.Create ((IntPtr)b, buffer.Length);
				using var codec = SKCodec.Create (skdata);
				return Decode (codec);
			}
		}

		/// <param name="buffer"></param>
		/// <param name="bitmapInfo"></param>
		public static SKBitmap Decode (ReadOnlySpan<byte> buffer, SKImageInfo bitmapInfo)
		{
			fixed (byte* b = buffer) {
				using var skdata = SKData.Create ((IntPtr)b, buffer.Length);
				using var codec = SKCodec.Create (skdata);
				return Decode (codec, bitmapInfo);
			}
		}

		// InstallPixels

		/// <summary>
		/// Installs the specified pixels into the bitmap.
		/// </summary>
		/// <param name="info">The image information describing the pixels.</param>
		/// <param name="pixels">The pixels to install.</param>
		/// <returns>Returns true on success, or false on failure. If there was an error, the bitmap will be set to empty.</returns>
		public bool InstallPixels (SKImageInfo info, IntPtr pixels)
		{
			return InstallPixels (info, pixels, info.RowBytes, null, null);
		}

		/// <summary>
		/// Installs the specified pixels into the bitmap.
		/// </summary>
		/// <param name="info">The image information describing the pixels.</param>
		/// <param name="pixels">The pixels to install.</param>
		/// <param name="rowBytes">The stride of the pixels being installed.</param>
		/// <returns>Returns true on success, or false on failure. If there was an error, the bitmap will be set to empty.</returns>
		public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			return InstallPixels (info, pixels, rowBytes, null, null);
		}

		/// <summary>
		/// Installs the specified pixels into the bitmap.
		/// </summary>
		/// <param name="info">The image information describing the pixels.</param>
		/// <param name="pixels">The pixels to install.</param>
		/// <param name="rowBytes">The stride of the pixels being installed.</param>
		/// <param name="releaseProc">The delegate to invoke when the pixels are no longer referenced.</param>
		/// <returns>Returns true on success, or false on failure. If there was an error, the bitmap will be set to empty.</returns>
		public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKBitmapReleaseDelegate releaseProc)
		{
			return InstallPixels (info, pixels, rowBytes, releaseProc, null);
		}

		/// <summary>
		/// Installs the specified pixels into the bitmap.
		/// </summary>
		/// <param name="info">The image information describing the pixels.</param>
		/// <param name="pixels">The pixels to install.</param>
		/// <param name="rowBytes">The stride of the pixels being installed.</param>
		/// <param name="releaseProc">The delegate to invoke when the pixels are no longer referenced.</param>
		/// <param name="context">The user data to use when invoking the delegate.</param>
		/// <returns>Returns true on success, or false on failure. If there was an error, the bitmap will be set to empty.</returns>
		public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKBitmapReleaseDelegate releaseProc, object context)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			var del = releaseProc != null && context != null
				? new SKBitmapReleaseDelegate ((addr, _) => releaseProc (addr, context))
				: releaseProc;
			DelegateProxies.Create (del, out _, out var ctx);
			var proxy = del is not null ? DelegateProxies.SKBitmapReleaseProxy : null;
			return SkiaApi.sk_bitmap_install_pixels (Handle, &cinfo, (void*)pixels, (IntPtr)rowBytes, proxy, (void*)ctx);
		}

		/// <summary>
		/// Installs the specified pixels into the bitmap.
		/// </summary>
		/// <param name="pixmap">The pixels to install.</param>
		/// <returns>Returns true on success, or false on failure. If there was an error, the bitmap will be set to empty.</returns>
		public bool InstallPixels (SKPixmap pixmap)
		{
			return SkiaApi.sk_bitmap_install_pixels_with_pixmap (Handle, pixmap.Handle);
		}

		// NotifyPixelsChanged

		/// <summary>
		/// Indicates to consumers of the bitmap that the pixel data has changed.
		/// </summary>
		public void NotifyPixelsChanged ()
		{
			SkiaApi.sk_bitmap_notify_pixels_changed (Handle);
		}

		// PeekPixels

		/// <summary>
		/// Returns the pixels if they are available without having to lock the bitmap.
		/// </summary>
		/// <returns>Returns the pixels if they are available, otherwise <see langword="null" />.</returns>
		/// <remarks>If the pixels are available without locking, then the pixmap is only valid until the bitmap changes in any way, in which case the pixmap becomes invalid.</remarks>
		public SKPixmap PeekPixels ()
		{
			SKPixmap pixmap = new SKPixmap ();
			var result = PeekPixels (pixmap);
			if (result) {
				return pixmap;
			} else {
				pixmap.Dispose ();
				return null;
			}
		}

		/// <summary>
		/// Returns the pixmap of the bitmap.
		/// </summary>
		/// <param name="pixmap">The pixmap to receive the pixel information.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if the bitmap does not have access to pixel data.</returns>
		public bool PeekPixels (SKPixmap pixmap)
		{
			if (pixmap == null) {
				throw new ArgumentNullException (nameof (pixmap));
			}
			var result = SkiaApi.sk_bitmap_peek_pixels (Handle, pixmap.Handle);
			if (result)
				pixmap.pixelSource = this;
			return result;
		}

		// Resize

		/// <summary>
		/// Resizes the current bitmap using the specified quality filter.
		/// </summary>
		/// <param name="info">The image information of the desired bitmap.</param>
		/// <param name="quality">The level of quality to use when scaling the pixels.</param>
		/// <returns>Returns the resized bitmap if the resize operation could be performed, otherwise <see langword="null" />.</returns>
		[Obsolete ("Use Resize(SKImageInfo info, SKSamplingOptions sampling) instead.")]
		public SKBitmap Resize (SKImageInfo info, SKFilterQuality quality) =>
			Resize (info, quality.ToSamplingOptions ());

		/// <param name="size"></param>
		/// <param name="quality"></param>
		[Obsolete ("Use Resize(SKSizeI size, SKSamplingOptions sampling) instead.")]
		public SKBitmap Resize (SKSizeI size, SKFilterQuality quality) =>
			Resize (size, quality.ToSamplingOptions ());

		public SKBitmap Resize (SKImageInfo info, SKSamplingOptions sampling)
		{
			if (info.IsEmpty)
				return null;

			var dst = new SKBitmap (info);
			if (ScalePixels (dst, sampling)) {
				return dst;
			} else {
				dst.Dispose ();
				return null;
			}
		}

		public SKBitmap Resize (SKSizeI size, SKSamplingOptions sampling) =>
			Resize (Info.WithSize (size), sampling);

		// ScalePixels

		/// <summary>
		/// Copies this pixmap to the destination, scaling the pixels to fit the destination size and converting the pixels to match the color type and alpha type.
		/// </summary>
		/// <param name="destination">The bitmap to recieve the scaled and converted pixels.</param>
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
		[Obsolete ("Use ScalePixels(SKBitmap destination, SKSamplingOptions sampling) instead.")]
		public bool ScalePixels (SKBitmap destination, SKFilterQuality quality) =>
			ScalePixels (destination, quality.ToSamplingOptions ());

		/// <summary>
		/// Copies this pixmap to the destination, scaling the pixels to fit the destination size and converting the pixels to match the color type and alpha type.
		/// </summary>
		/// <param name="destination">The pixmap to recieve the scaled and converted pixels.</param>
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

		public bool ScalePixels (SKBitmap destination, SKSamplingOptions sampling)
		{
			if (destination == null) {
				throw new ArgumentNullException (nameof (destination));
			}

			using (var dstPix = destination.PeekPixels ()) {
				return ScalePixels (dstPix, sampling);
			}
		}

		public bool ScalePixels (SKPixmap destination, SKSamplingOptions sampling)
		{
			if (destination == null) {
				throw new ArgumentNullException (nameof (destination));
			}

			using (var srcPix = PeekPixels ()) {
				return srcPix.ScalePixels (destination, sampling);
			}
		}

		// From/ToImage

		/// <summary>
		/// Creates a new bitmap from a copy of the pixel data in the specified image.
		/// </summary>
		/// <param name="image">The image to use to create a bitmap.</param>
		/// <returns>Returns a new instance of <see cref="T:SkiaSharp.SKBitmap" />, or null if the bitmap could not be created.</returns>
		public static SKBitmap FromImage (SKImage image)
		{
			if (image == null) {
				throw new ArgumentNullException (nameof (image));
			}

			var info = new SKImageInfo (image.Width, image.Height, SKImageInfo.PlatformColorType, image.AlphaType);
			var bmp = new SKBitmap (info);
			if (!image.ReadPixels (info, bmp.GetPixels (), info.RowBytes, 0, 0)) {
				bmp.Dispose ();
				bmp = null;
			}
			return bmp;
		}

		// Encode

		/// <param name="format">The file format used to encode the image.</param>
		/// <param name="quality">The quality level to use for the image. Quality range from 0-100. Higher values correspond to improved visual quality, but less compression.</param>
		public SKData Encode (SKEncodedImageFormat format, int quality)
		{
			using var pixmap = PeekPixels ();
			return pixmap?.Encode (format, quality);
		}

		/// <param name="dst">The stream to write the encoded image to.</param>
		/// <param name="format">The file format used to encode the image.</param>
		/// <param name="quality">The quality level to use for the image. Quality range from 0-100. Higher values correspond to improved visual quality, but less compression.</param>
		public bool Encode (Stream dst, SKEncodedImageFormat format, int quality)
		{
			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, format, quality);
		}

		/// <summary>
		/// Encodes the image using the specified format.
		/// </summary>
		/// <param name="dst">The stream to write the encoded image to.</param>
		/// <param name="format">The file format used to encode the image.</param>
		/// <param name="quality">The quality level to use for the image. Quality range from 0-100. Higher values correspond to improved visual quality, but less compression.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if there was an error.</returns>
		public bool Encode (SKWStream dst, SKEncodedImageFormat format, int quality)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			using var pixmap = PeekPixels ();
			return pixmap?.Encode (dst, format, quality) ?? false;
		}

		// Swap

		private void Swap (SKBitmap other)
		{
			SkiaApi.sk_bitmap_swap (Handle, other.Handle);
		}

		// ToShader

		public SKShader ToShader () =>
			ToShader (SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, SKSamplingOptions.Default, null);
		/// <param name="tmx"></param>
		/// <param name="tmy"></param>
		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy) =>
			ToShader (tmx, tmy, SKSamplingOptions.Default, null);

		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling) =>
			ToShader (tmx, tmy, sampling, null);

		[Obsolete ("Use ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling) instead.")]
		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterQuality quality) =>
			ToShader (tmx, tmy, quality.ToSamplingOptions(), null);

		/// <param name="tmx"></param>
		/// <param name="tmy"></param>
		/// <param name="localMatrix"></param>
		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix) =>
			ToShader (tmx, tmy, SKSamplingOptions.Default, &localMatrix);

		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling, SKMatrix localMatrix) =>
			ToShader (tmx, tmy, sampling, &localMatrix);

		[Obsolete ("Use ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling, SKMatrix localMatrix) instead.")]
		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterQuality quality, SKMatrix localMatrix) =>
			ToShader (tmx, tmy, quality.ToSamplingOptions(), &localMatrix);

		private SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling, SKMatrix* localMatrix) =>
			SKShader.GetObject (SkiaApi.sk_bitmap_make_shader (Handle, tmx, tmy, &sampling, localMatrix));
	}
}

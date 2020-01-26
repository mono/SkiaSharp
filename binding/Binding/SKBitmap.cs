using System;
using System.IO;

namespace SkiaSharp
{
	// TODO: keep in mind SKBitmap may be going away (according to Google)
	// TODO: `ComputeIsOpaque` may be useful
	// TODO: `GenerationID` may be useful

	public unsafe class SKBitmap : SKObject
	{
		private const string UnableToAllocatePixelsMessage = "Unable to allocate pixels for the bitmap.";

		[Preserve]
		internal SKBitmap (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKBitmap ()
			: this (SkiaApi.sk_bitmap_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKBitmap instance.");
			}
		}

		public SKBitmap (int width, int height, bool isOpaque = false)
			: this (width, height, SKImageInfo.PlatformColorType, isOpaque ? SKAlphaType.Opaque : SKAlphaType.Premul)
		{
		}

		public SKBitmap (int width, int height, SKColorType colorType, SKAlphaType alphaType = SKAlphaType.Premul)
			: this (new SKImageInfo (width, height, colorType, alphaType))
		{
		}

		public SKBitmap (SKImageInfo info, int rowBytes = 0)
			: this ()
		{
			if (!TryAllocPixels (info, rowBytes == 0 ? info.RowBytes : rowBytes))
				throw new Exception (UnableToAllocatePixelsMessage);
		}

		public SKBitmap (SKImageInfo info, SKBitmapAllocFlags flags)
			: this ()
		{
			if (!TryAllocPixels (info, flags))
				throw new Exception (UnableToAllocatePixelsMessage);
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_bitmap_destructor (Handle);

		// properties

		public bool ReadyToDraw =>
			SkiaApi.sk_bitmap_ready_to_draw (Handle);

		public SKImageInfo Info {
			get {
				SKImageInfoNative cinfo;
				SkiaApi.sk_bitmap_get_info (Handle, &cinfo);
				return SKImageInfoNative.ToManaged (ref cinfo);
			}
		}

		public int Width => Info.Width;

		public int Height => Info.Height;

		public SKColorType ColorType => Info.ColorType;

		public SKAlphaType AlphaType => Info.AlphaType;

		public SKColorSpace ColorSpace => Info.ColorSpace;

		public int BytesPerPixel => Info.BytesPerPixel;

		public int RowBytes =>
			(int)SkiaApi.sk_bitmap_get_row_bytes (Handle);

		public int ByteCount =>
			(int)SkiaApi.sk_bitmap_get_byte_count (Handle);

		public bool IsEmpty => Info.IsEmpty;

		public bool IsNull => SkiaApi.sk_bitmap_is_null (Handle);

		public bool DrawsNothing => IsEmpty || IsNull;

		public bool IsVolatile {
			get => SkiaApi.sk_bitmap_is_volatile (Handle);
			set => SkiaApi.sk_bitmap_set_volatile (Handle, value);
		}

		// Immutable

		public bool IsImmutable => SkiaApi.sk_bitmap_is_immutable (Handle);

		public void SetImmutable () =>
			SkiaApi.sk_bitmap_set_immutable (Handle);

		// TryAllocPixels

		public bool TryAllocPixels (SKImageInfo info, int rowBytes = 0)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return SkiaApi.sk_bitmap_try_alloc_pixels (Handle, &cinfo, (IntPtr)(rowBytes == 0 ? info.RowBytes : rowBytes));
		}

		public bool TryAllocPixels (SKImageInfo info, SKBitmapAllocFlags flags)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return SkiaApi.sk_bitmap_try_alloc_pixels_with_flags (Handle, &cinfo, (uint)flags);
		}

		// Reset

		public void Reset () =>
			SkiaApi.sk_bitmap_reset (Handle);

		// Erase

		public void Erase (SKColor color) =>
			SkiaApi.sk_bitmap_erase (Handle, (uint)color);

		public void Erase (SKColor color, SKRectI rect) =>
			SkiaApi.sk_bitmap_erase_rect (Handle, (uint)color, &rect);

		// Pixels (color)

		public SKColor GetPixel (int x, int y) =>
			SkiaApi.sk_bitmap_get_pixel_color (Handle, x, y);

		public void SetPixel (int x, int y, SKColor color) =>
			SkiaApi.sk_bitmap_set_pixel_color (Handle, x, y, (uint)color);

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
				fixed (SKColor* v = value) {
					SkiaApi.sk_bitmap_set_pixel_colors (Handle, (uint*)v);
				}
			}
		}

		// Copy

		public SKBitmap Copy () =>
			Copy (ColorType);

		public SKBitmap Copy (SKColorType colorType)
		{
			var destination = new SKBitmap (Info.WithColorType (colorType));

			if (!CopyTo (destination)) {
				destination.Dispose ();
				destination = null;
			}

			return destination;
		}

		public bool CopyTo (SKBitmap destination)
		{
			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			using var destPixmap = destination.PeekPixels ();
			using var pixmap = PeekPixels ();
			return pixmap.ReadPixels (destPixmap);
		}

		// ExtractSubset

		public bool ExtractSubset (SKBitmap destination, SKRectI subset)
		{
			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			return SkiaApi.sk_bitmap_extract_subset (Handle, destination.Handle, &subset);
		}

		// ExtractAlpha

		public bool ExtractAlpha (SKBitmap destination) =>
			ExtractAlpha (destination, null, out _);

		public bool ExtractAlpha (SKBitmap destination, out SKPointI offset) =>
			ExtractAlpha (destination, null, out offset);

		public bool ExtractAlpha (SKBitmap destination, SKPaint paint) =>
			ExtractAlpha (destination, paint, out _);

		public bool ExtractAlpha (SKBitmap destination, SKPaint paint, out SKPointI offset)
		{
			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			fixed (SKPointI* o = &offset) {
				return SkiaApi.sk_bitmap_extract_alpha (Handle, destination.Handle, paint == null ? IntPtr.Zero : paint.Handle, o);
			}
		}

		// GetAddress

		public IntPtr GetAddress (int x, int y) =>
			(IntPtr)SkiaApi.sk_bitmap_get_addr (Handle, x, y);

		public IntPtr GetAddress8 (int x, int y) =>
			(IntPtr)SkiaApi.sk_bitmap_get_addr_8 (Handle, x, y);

		public IntPtr GetAddress16 (int x, int y) =>
			(IntPtr)SkiaApi.sk_bitmap_get_addr_16 (Handle, x, y);

		public IntPtr GetAddress32 (int x, int y) =>
			(IntPtr)SkiaApi.sk_bitmap_get_addr_32 (Handle, x, y);

		// Pixels (bytes)

		public IntPtr GetPixels () =>
			GetPixels (out _);

		public ReadOnlySpan<byte> GetPixelSpan () =>
			new ReadOnlySpan<byte> ((void*)GetPixels (out var length), (int)length);

		public IntPtr GetPixels (out IntPtr length)
		{
			fixed (IntPtr* l = &length) {
				return (IntPtr)SkiaApi.sk_bitmap_get_pixels (Handle, l);
			}
		}

		public void SetPixels (IntPtr pixels) =>
			SkiaApi.sk_bitmap_set_pixels (Handle, (void*)pixels);

		public byte[] Bytes {
			get {
				var array = GetPixelSpan ().ToArray ();
				GC.KeepAlive (this);
				return array;
			}
		}

		// DecodeBounds

		public static SKImageInfo DecodeBounds (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using var codec = SKCodec.Create (stream);
			return codec?.Info ?? SKImageInfo.Empty;
		}

		public static SKImageInfo DecodeBounds (SKStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using var codec = SKCodec.Create (stream);
			return codec?.Info ?? SKImageInfo.Empty;
		}

		public static SKImageInfo DecodeBounds (SKData data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			using var codec = SKCodec.Create (data);
			return codec?.Info ?? SKImageInfo.Empty;
		}

		public static SKImageInfo DecodeBounds (string filename)
		{
			if (filename == null)
				throw new ArgumentNullException (nameof (filename));

			using var codec = SKCodec.Create (filename);
			return codec?.Info ?? SKImageInfo.Empty;
		}

		public static SKImageInfo DecodeBounds (ReadOnlySpan<byte> buffer)
		{
			fixed (byte* b = buffer) {
				using var skdata = SKData.Create ((IntPtr)b, buffer.Length);
				using var codec = SKCodec.Create (skdata);
				return codec?.Info ?? SKImageInfo.Empty;
			}
		}

		// Decode

		public static SKBitmap Decode (SKCodec codec)
		{
			if (codec == null)
				throw new ArgumentNullException (nameof (codec));

			var info = codec.Info;
			if (info.AlphaType == SKAlphaType.Unpremul)
				info.AlphaType = SKAlphaType.Premul;

			// for backwards compatibility, remove the colorspace
			info.ColorSpace = null;
			return Decode (codec, info);
		}

		public static SKBitmap Decode (SKCodec codec, SKImageInfo bitmapInfo)
		{
			if (codec == null)
				throw new ArgumentNullException (nameof (codec));

			var bitmap = new SKBitmap (bitmapInfo);
			var result = codec.GetPixels (bitmapInfo, bitmap.GetPixels (out _));
			if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput) {
				bitmap.Dispose ();
				bitmap = null;
			}
			return bitmap;
		}

		public static SKBitmap Decode (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using var codec = SKCodec.Create (stream);
			if (codec == null)
				return null;
			return Decode (codec);
		}

		public static SKBitmap Decode (Stream stream, SKImageInfo bitmapInfo)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using var codec = SKCodec.Create (stream);
			if (codec == null)
				return null;
			return Decode (codec, bitmapInfo);
		}

		public static SKBitmap Decode (SKStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using var codec = SKCodec.Create (stream);
			if (codec == null)
				return null;
			return Decode (codec);
		}

		public static SKBitmap Decode (SKStream stream, SKImageInfo bitmapInfo)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using var codec = SKCodec.Create (stream);
			if (codec == null)
				return null;
			return Decode (codec, bitmapInfo);
		}

		public static SKBitmap Decode (SKData data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			using var codec = SKCodec.Create (data);
			if (codec == null)
				return null;
			return Decode (codec);
		}

		public static SKBitmap Decode (SKData data, SKImageInfo bitmapInfo)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			using var codec = SKCodec.Create (data);
			if (codec == null)
				return null;
			return Decode (codec, bitmapInfo);
		}

		public static SKBitmap Decode (string filename)
		{
			if (filename == null)
				throw new ArgumentNullException (nameof (filename));

			using var codec = SKCodec.Create (filename);
			if (codec == null)
				return null;
			return Decode (codec);
		}

		public static SKBitmap Decode (string filename, SKImageInfo bitmapInfo)
		{
			if (filename == null)
				throw new ArgumentNullException (nameof (filename));

			using var codec = SKCodec.Create (filename);
			if (codec == null)
				return null;
			return Decode (codec, bitmapInfo);
		}

		public static SKBitmap Decode (ReadOnlySpan<byte> buffer)
		{
			using var stream = new SKMemoryStream (buffer);
			return Decode (stream);
		}

		public static SKBitmap Decode (ReadOnlySpan<byte> buffer, SKImageInfo bitmapInfo)
		{
			fixed (byte* b = buffer) {
				using var skdata = SKData.Create ((IntPtr)b, buffer.Length);
				using var codec = SKCodec.Create (skdata);
				return Decode (codec, bitmapInfo);
			}
		}

		// InstallPixels

		public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes = 0, SKBitmapReleaseDelegate releaseProc = null, object context = null)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			var del = releaseProc != null && context != null
				? new SKBitmapReleaseDelegate ((addr, _) => releaseProc (addr, context))
				: releaseProc;
			var proxy = DelegateProxies.Create (del, DelegateProxies.SKBitmapReleaseDelegateProxy, out _, out var ctx);
			return SkiaApi.sk_bitmap_install_pixels (Handle, &cinfo, (void*)pixels, (IntPtr)(rowBytes == 0 ? info.RowBytes : rowBytes), proxy, (void*)ctx);
		}

		public bool InstallPixels (SKPixmap pixmap) =>
			SkiaApi.sk_bitmap_install_pixels_with_pixmap (Handle, pixmap.Handle);

		// InstallMaskPixels

		public bool InstallMaskPixels (SKMask mask) =>
			SkiaApi.sk_bitmap_install_mask_pixels (Handle, &mask);

		// NotifyPixelsChanged

		public void NotifyPixelsChanged () =>
			SkiaApi.sk_bitmap_notify_pixels_changed (Handle);

		// PeekPixels

		public SKPixmap PeekPixels ()
		{
			var pixmap = new SKPixmap ();
			if (!PeekPixels (pixmap)) {
				pixmap.Dispose ();
				pixmap = null;
			}
			return pixmap;
		}

		public bool PeekPixels (SKPixmap pixmap)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));

			return SkiaApi.sk_bitmap_peek_pixels (Handle, pixmap.Handle);
		}

		// Resize

		public SKBitmap Resize (SKImageInfo info, SKFilterQuality quality)
		{
			var dst = new SKBitmap (info);
			if (!ScalePixels (dst, quality)) {
				dst.Dispose ();
				dst = null;
			}
			return dst;
		}

		// ScalePixels

		public bool ScalePixels (SKBitmap destination, SKFilterQuality quality)
		{
			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			using var dstPix = destination.PeekPixels ();
			return ScalePixels (dstPix, quality);
		}

		public bool ScalePixels (SKPixmap destination, SKFilterQuality quality)
		{
			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			using var srcPix = PeekPixels ();
			return srcPix.ScalePixels (destination, quality);
		}

		// FromImage

		public static SKBitmap FromImage (SKImage image)
		{
			if (image == null)
				throw new ArgumentNullException (nameof (image));

			return image.ToBitmap ();
		}

		// Encode

		public SKData Encode (SKEncodedImageFormat format, int quality)
		{
			using var pixmap = PeekPixels ();
			return pixmap.Encode (format, quality);
		}

		public bool Encode (SKWStream dst, SKEncodedImageFormat format, int quality) =>
			SKPixmap.Encode (dst, this, format, quality);

		// Swap

		private void Swap (SKBitmap other) =>
			SkiaApi.sk_bitmap_swap (Handle, other.Handle);

		// ToShader

		public SKShader ToShader (SKShaderTileMode tmx = SKShaderTileMode.Clamp, SKShaderTileMode tmy = SKShaderTileMode.Clamp) =>
			GetObject<SKShader> (SkiaApi.sk_bitmap_make_shader (Handle, tmx, tmy, null));

		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, in SKMatrix localMatrix)
		{
			fixed (SKMatrix* m = &localMatrix) {
				return GetObject<SKShader> (SkiaApi.sk_bitmap_make_shader (Handle, tmx, tmy, m));
			}
		}
	}
}

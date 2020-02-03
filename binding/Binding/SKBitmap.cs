using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete]
	public enum SKBitmapResizeMethod
	{
		Box,
		Triangle,
		Lanczos3,
		Hamming,
		Mitchell
	}

	public static partial class SkiaExtensions
	{
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public static SKFilterQuality ToFilterQuality (this SKBitmapResizeMethod method)
		{
			switch (method) {
				case SKBitmapResizeMethod.Box:
				case SKBitmapResizeMethod.Triangle:
					return SKFilterQuality.Low;
				case SKBitmapResizeMethod.Lanczos3:
					return SKFilterQuality.Medium;
				case SKBitmapResizeMethod.Hamming:
				case SKBitmapResizeMethod.Mitchell:
					return SKFilterQuality.High;
				default:
					return SKFilterQuality.Medium;
			}
		}
	}

	// TODO: keep in mind SKBitmap may be going away (according to Google)
	// TODO: `ComputeIsOpaque` may be useful
	// TODO: `GenerationID` may be useful

	public unsafe class SKBitmap : SKObject
	{
		private const string UnsupportedColorTypeMessage = "Setting the ColorTable is only supported for bitmaps with ColorTypes of Index8.";
		private const string UnableToAllocatePixelsMessage = "Unable to allocate pixels for the bitmap.";

		[Preserve]
		internal SKBitmap (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKBitmap ()
			: this (SkiaApi.sk_bitmap_new (), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKBitmap instance.");
		}

		public SKBitmap (int width, int height)
			: this (new SKImageInfo (width, height))
		{
		}

		public SKBitmap (int width, int height, bool isOpaque)
			: this (new SKImageInfo (width, height, SKImageInfo.PlatformColorType, isOpaque ? SKAlphaType.Opaque : SKAlphaType.Premul))
		{
		}

		public SKBitmap (int width, int height, SKColorType colorType)
			: this (new SKImageInfo (width, height, colorType, null))
		{
		}

		public SKBitmap (int width, int height, SKColorType colorType, SKColorSpace colorspace)
			: this (new SKImageInfo (width, height, colorType, colorspace))
		{
		}

		public SKBitmap (int width, int height, SKColorType colorType, SKAlphaType alphaType)
			: this (new SKImageInfo (width, height, colorType, alphaType, null))
		{
		}

		public SKBitmap (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorspace)
			: this (new SKImageInfo (width, height, colorType, alphaType, colorspace))
		{
		}

		public SKBitmap (SKImageInfo info)
			: this (info, info.RowBytes)
		{
		}

		public SKBitmap (SKImageInfo info, int rowBytes)
			: this ()
		{
			if (!TryAllocPixels (info, rowBytes))
				throw new Exception (UnableToAllocatePixelsMessage);
		}

		public SKBitmap (SKImageInfo info, SKBitmapAllocFlags flags)
			: this ()
		{
			if (!TryAllocPixels (info, flags))
				throw new Exception (UnableToAllocatePixelsMessage);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use SKBitmap(SKImageInfo) instead.")]
		public SKBitmap (SKImageInfo info, SKColorTable ctable)
			: this (info)
		{
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use SKBitmap(SKImageInfo, SKBitmapAllocFlags) instead.")]
		public SKBitmap (SKImageInfo info, SKColorTable ctable, SKBitmapAllocFlags flags)
			: this (info, SKBitmapAllocFlags.None)
		{
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_bitmap_destructor (Handle);

		// properties

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported.")]
		public SKColorTable ColorTable => null;

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

		public bool TryAllocPixels (SKImageInfo info) =>
			TryAllocPixels (info, info.RowBytes);

		public bool TryAllocPixels (SKImageInfo info, int rowBytes)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return SkiaApi.sk_bitmap_try_alloc_pixels (Handle, &cinfo, (IntPtr)rowBytes);
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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetPixelColor(int, int) instead.")]
		public SKColor GetPixel (int x, int y) =>
			GetPixelColor (x, y);

		public SKColor GetPixelColor (int x, int y) =>
			SkiaApi.sk_bitmap_get_pixel_color (Handle, x, y);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixel(int, int) instead.")]
		public SKPMColor GetIndex8Color (int x, int y) =>
			(SKPMColor)GetPixel (x, y);

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

		public SKBitmap Copy () =>
			Copy (ColorType);

		public SKBitmap Copy (SKColorType colorType)
		{
			var destination = new SKBitmap ();
			if (!CopyTo (destination, colorType)) {
				destination.Dispose ();
				destination = null;
			}
			return destination;
		}

		public bool CopyTo (SKBitmap destination) =>
			CopyTo (destination, ColorType);

		public bool CopyTo (SKBitmap destination, SKColorType colorType)
		{
			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			using var srcPixmap = PeekPixels ();
			if (srcPixmap == null)
				return false;

			using var temp = new SKBitmap ();

			var dstInfo = srcPixmap.Info.WithColorType (colorType);
			if (!temp.TryAllocPixels (dstInfo))
				return false;

			using var tempPixmap = temp.PeekPixels ();
			if (tempPixmap == null)
				return false;

			if (!srcPixmap.ReadPixels (tempPixmap))
				return false;

			destination.Swap (temp);
			return true;
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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetAddress(int, int) instead.")]
		public IntPtr GetAddr (int x, int y) =>
			GetAddress (x, y);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public ushort GetAddr16 (int x, int y) =>
			*SkiaApi.sk_bitmap_get_addr_16 (Handle, x, y);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public uint GetAddr32 (int x, int y) =>
			*SkiaApi.sk_bitmap_get_addr_32 (Handle, x, y);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public byte GetAddr8 (int x, int y) =>
			*SkiaApi.sk_bitmap_get_addr_8 (Handle, x, y);

		// Pixels (bytes)

		public IntPtr GetPixels () =>
			GetPixels (out _);

		public Span<byte> GetPixelSpan () =>
			new Span<byte> ((void*)GetPixels (out var length), (int)length);

		public IntPtr GetPixels (out IntPtr length)
		{
			fixed (IntPtr* l = &length) {
				return (IntPtr)SkiaApi.sk_bitmap_get_pixels (Handle, l);
			}
		}

		public void SetPixels (IntPtr pixels) =>
			SkiaApi.sk_bitmap_set_pixels (Handle, (void*)pixels);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use SetPixels(IntPtr) instead.")]
		public void SetPixels (IntPtr pixels, SKColorTable ct) =>
			SetPixels (pixels);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported.")]
		public void SetColorTable (SKColorTable ct)
		{
			// no-op due to unsupperted action
		}

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

		public static SKImageInfo DecodeBounds (byte[] buffer) =>
			DecodeBounds (buffer.AsSpan ());

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

		public static SKBitmap Decode (byte[] buffer) =>
			Decode (buffer.AsSpan ());

		public static SKBitmap Decode (byte[] buffer, SKImageInfo bitmapInfo) =>
			Decode (buffer.AsSpan (), bitmapInfo);

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

		[Obsolete ("The Index8 color type and color table is no longer supported. Use InstallPixels(SKImageInfo, IntPtr, int) instead.")]
		public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable) =>
			InstallPixels (info, pixels, rowBytes, null, null);

		[Obsolete ("The Index8 color type and color table is no longer supported. Use InstallPixels(SKImageInfo, IntPtr, int, SKBitmapReleaseDelegate, object) instead.")]
		public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable, SKBitmapReleaseDelegate releaseProc, object context) =>
			InstallPixels (info, pixels, rowBytes, releaseProc, context);

		public bool InstallPixels (SKImageInfo info, IntPtr pixels) =>
			InstallPixels (info, pixels, info.RowBytes, null, null);

		public bool InstallPixels (SKImageInfo info, IntPtr pixels, SKBitmapReleaseDelegate releaseProc) =>
			InstallPixels (info, pixels, info.RowBytes, releaseProc, null);

		public bool InstallPixels (SKImageInfo info, IntPtr pixels, SKBitmapReleaseDelegate releaseProc, object context) =>
			InstallPixels (info, pixels, info.RowBytes, releaseProc, context);

		public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes) =>
			InstallPixels (info, pixels, rowBytes, null, null);

		public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKBitmapReleaseDelegate releaseProc) =>
			InstallPixels (info, pixels, rowBytes, releaseProc, null);

		public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKBitmapReleaseDelegate releaseProc, object context)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			var del = releaseProc != null && context != null
				? new SKBitmapReleaseDelegate ((addr, _) => releaseProc (addr, context))
				: releaseProc;
			var proxy = DelegateProxies.Create (del, DelegateProxies.SKBitmapReleaseDelegateProxy, out _, out var ctx);
			return SkiaApi.sk_bitmap_install_pixels (Handle, &cinfo, (void*)pixels, (IntPtr)rowBytes, proxy, (void*)ctx);
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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use Resize(SKImageInfo, SKFilterQuality) instead.")]
		public SKBitmap Resize (SKImageInfo info, SKBitmapResizeMethod method) =>
			Resize (info, method.ToFilterQuality ());

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use ScalePixels(SKBitmap, SKFilterQuality) instead.")]
		public bool Resize (SKBitmap dst, SKBitmapResizeMethod method) =>
			ScalePixels (dst, method.ToFilterQuality ());

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use ScalePixels(SKBitmap, SKFilterQuality) instead.")]
		public static bool Resize (SKBitmap dst, SKBitmap src, SKBitmapResizeMethod method) =>
			src.ScalePixels (dst, method.ToFilterQuality ());

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
			return pixmap?.Encode (format, quality);
		}

		public bool Encode (Stream dst, SKEncodedImageFormat format, int quality)
		{
			using var pixmap = new SKPixmap ();
			return PeekPixels (pixmap) && pixmap.Encode (dst, format, quality);
		}

		public bool Encode (SKWStream dst, SKEncodedImageFormat format, int quality)
		{
			using var pixmap = new SKPixmap ();
			return PeekPixels (pixmap) && pixmap.Encode (dst, format, quality);
		}

		// Swap

		private void Swap (SKBitmap other) =>
			SkiaApi.sk_bitmap_swap (Handle, other.Handle);

		// ToShader

		public SKShader ToShader () =>
			ToShader (SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);

		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy) =>
			GetObject<SKShader> (SkiaApi.sk_bitmap_make_shader (Handle, tmx, tmy, null));

		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix) =>
			GetObject<SKShader> (SkiaApi.sk_bitmap_make_shader (Handle, tmx, tmy, &localMatrix));
	}
}

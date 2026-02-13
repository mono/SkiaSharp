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

	public unsafe class SKBitmap : SKObject, ISKSkipObjectRegistration
	{
		private const string UnsupportedColorTypeMessage = "Setting the ColorTable is only supported for bitmaps with ColorTypes of Index8.";
		private const string UnableToAllocatePixelsMessage = "Unable to allocate pixels for the bitmap.";

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

		public SKBitmap (int width, int height, SKColorType colorType, SKAlphaType alphaType)
			: this (new SKImageInfo (width, height, colorType, alphaType))
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
			if (!TryAllocPixels (info, rowBytes)) {
				throw new Exception (UnableToAllocatePixelsMessage);
			}
		}

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

		public bool TryAllocPixels (SKImageInfo info)
		{
			return TryAllocPixels (info, info.RowBytes);
		}

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

		public void Reset ()
		{
			SkiaApi.sk_bitmap_reset (Handle);
		}

		// SetImmutable

		public void SetImmutable ()
		{
			SkiaApi.sk_bitmap_set_immutable (Handle);
		}

		// Erase

		public void Erase (SKColor color)
		{
			SkiaApi.sk_bitmap_erase (Handle, (uint)color);
		}

		public void Erase (SKColor color, SKRectI rect)
		{
			SkiaApi.sk_bitmap_erase_rect (Handle, (uint)color, &rect);
		}

		// GetAddress

		public IntPtr GetAddress (int x, int y) =>
			(IntPtr)SkiaApi.sk_bitmap_get_addr (Handle, x, y);

		// Pixels (color)

		public SKColor GetPixel (int x, int y)
		{
			return SkiaApi.sk_bitmap_get_pixel_color (Handle, x, y);
		}

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

		public SKBitmap Copy ()
		{
			return Copy (ColorType);
		}

		public SKBitmap Copy (SKColorType colorType)
		{
			var destination = new SKBitmap ();
			if (!CopyTo (destination, colorType)) {
				destination.Dispose ();
				destination = null;
			}
			return destination;
		}

		public bool CopyTo (SKBitmap destination)
		{
			if (destination == null) {
				throw new ArgumentNullException (nameof (destination));
			}
			return CopyTo (destination, ColorType);
		}

		public bool CopyTo (SKBitmap destination, SKColorType colorType)
		{
			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			if (colorType == SKColorType.Unknown)
				return false;

			if (Width == 0 || Height == 0)
				return false;

			// To check if we can do a direct copy
			// If a SKBitmap is created by ExtractSubset, its pixels may not be stored contiguously
			// In most cases, des and src have the same Size and ColorType
			int desWidth = destination.Width;
			int desHeight = destination.Height;
			if(desWidth==0||desHeight==0)
				return false;
			if (ColorType == destination.ColorType && ColorType == colorType) {
				if (Width * Height * BytesPerPixel == ByteCount &&
				    desWidth * desHeight * BytesPerPixel == destination.ByteCount) {
					if (Width == desWidth) {
						var des = destination.GetPixelSpan ();
						var src = GetPixelSpan ();
						if(Height<=desHeight) {
							return src.TryCopyTo (des);
						}
						return src.Slice (0, des.Length).TryCopyTo (des);
					}
				}
				// Copy row by row
				var rowBytes = Math.Min (RowBytes, destination.RowBytes);
				var srcAddress = GetAddress (0, 0);
				var desAddress = destination.GetAddress (0, 0);
				var srcOriginalRowBytes = GetAddress (0, 1) - srcAddress;
				var desOriginalRowBytes = destination.GetAddress (0, 1) - destination.GetAddress (0, 0);
				for(var row=0;row<Math.Min(Height, desHeight);row++,srcAddress+=srcOriginalRowBytes,desAddress+=desOriginalRowBytes) {
					var des =new Span<byte> (srcAddress.ToPointer(), rowBytes);
					var src =new Span<byte> (desAddress.ToPointer(), rowBytes);
					src.TryCopyTo(des);
				}
				return true;
			}

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

		public bool ExtractSubset (SKBitmap destination, SKRectI subset)
		{
			if (destination == null) {
				throw new ArgumentNullException (nameof (destination));
			}
			return SkiaApi.sk_bitmap_extract_subset (Handle, destination.Handle, &subset);
		}

		// ExtractAlpha

		public bool ExtractAlpha (SKBitmap destination)
		{
			return ExtractAlpha (destination, null, out var offset);
		}

		public bool ExtractAlpha (SKBitmap destination, out SKPointI offset)
		{
			return ExtractAlpha (destination, null, out offset);
		}

		public bool ExtractAlpha (SKBitmap destination, SKPaint paint)
		{
			return ExtractAlpha (destination, paint, out var offset);
		}

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

		public bool ReadyToDraw => SkiaApi.sk_bitmap_ready_to_draw (Handle);

		public SKImageInfo Info {
			get {
				SKImageInfoNative cinfo;
				SkiaApi.sk_bitmap_get_info (Handle, &cinfo);
				return SKImageInfoNative.ToManaged (ref cinfo);
			}
		}

		public int Width {
			get { return Info.Width; }
		}

		public int Height {
			get { return Info.Height; }
		}

		public SKColorType ColorType {
			get { return Info.ColorType; }
		}

		public SKAlphaType AlphaType {
			get { return Info.AlphaType; }
		}

		public SKColorSpace ColorSpace {
			get { return Info.ColorSpace; }
		}

		public int BytesPerPixel {
			get { return Info.BytesPerPixel; }
		}

		public int RowBytes {
			get { return (int)SkiaApi.sk_bitmap_get_row_bytes (Handle); }
		}

		public int ByteCount {
			get { return (int)SkiaApi.sk_bitmap_get_byte_count (Handle); }
		}

		// *Pixels*

		public IntPtr GetPixels () =>
			GetPixels (out _);

		public Span<byte> GetPixelSpan () =>
			new Span<byte> ((void*)GetPixels (out var length), (int)length);

		public Span<byte> GetPixelSpan (int x, int y) =>
			GetPixelSpan ().Slice (Info.GetPixelBytesOffset (x, y));

		public IntPtr GetPixels (out IntPtr length)
		{
			fixed (IntPtr* l = &length) {
				return (IntPtr)SkiaApi.sk_bitmap_get_pixels (Handle, l);
			}
		}

		public void SetPixels (IntPtr pixels)
		{
			SkiaApi.sk_bitmap_set_pixels (Handle, (void*)pixels);
		}

		// more properties

		public byte[] Bytes {
			get {
				var array = GetPixelSpan ().ToArray ();
				GC.KeepAlive (this);
				return array;
			}
		}

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

		public bool IsEmpty {
			get { return Info.IsEmpty; }
		}

		public bool IsNull {
			get { return SkiaApi.sk_bitmap_is_null (Handle); }
		}

		public bool DrawsNothing {
			get { return IsEmpty || IsNull; }
		}

		public bool IsImmutable {
			get { return SkiaApi.sk_bitmap_is_immutable (Handle); }
		}

		// DecodeBounds

		public static SKImageInfo DecodeBounds (Stream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}
			using (var codec = SKCodec.Create (stream)) {
				return codec?.Info ?? SKImageInfo.Empty;
			}
		}

		public static SKImageInfo DecodeBounds (SKStream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}
			using (var codec = SKCodec.Create (stream)) {
				return codec?.Info ?? SKImageInfo.Empty;
			}
		}

		public static SKImageInfo DecodeBounds (SKData data)
		{
			if (data == null) {
				throw new ArgumentNullException (nameof (data));
			}
			using (var codec = SKCodec.Create (data)) {
				return codec?.Info ?? SKImageInfo.Empty;
			}
		}

		public static SKImageInfo DecodeBounds (string filename)
		{
			if (filename == null) {
				throw new ArgumentNullException (nameof (filename));
			}
			using (var codec = SKCodec.Create (filename)) {
				return codec?.Info ?? SKImageInfo.Empty;
			}
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
			if (codec == null) {
				throw new ArgumentNullException (nameof (codec));
			}
			var info = codec.Info;
			if (info.AlphaType == SKAlphaType.Unpremul) {
				info.AlphaType = SKAlphaType.Premul;
			}
			return Decode (codec, info);
		}

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

		public static SKBitmap Decode (byte[] buffer) =>
			Decode (buffer.AsSpan ());

		public static SKBitmap Decode (byte[] buffer, SKImageInfo bitmapInfo) =>
			Decode (buffer.AsSpan (), bitmapInfo);

		public static SKBitmap Decode (ReadOnlySpan<byte> buffer)
		{
			fixed (byte* b = buffer) {
				using var skdata = SKData.Create ((IntPtr)b, buffer.Length);
				using var codec = SKCodec.Create (skdata);
				return Decode (codec);
			}
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

		public bool InstallPixels (SKImageInfo info, IntPtr pixels)
		{
			return InstallPixels (info, pixels, info.RowBytes, null, null);
		}

		public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			return InstallPixels (info, pixels, rowBytes, null, null);
		}

		public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKBitmapReleaseDelegate releaseProc)
		{
			return InstallPixels (info, pixels, rowBytes, releaseProc, null);
		}

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

		public bool InstallPixels (SKPixmap pixmap)
		{
			return SkiaApi.sk_bitmap_install_pixels_with_pixmap (Handle, pixmap.Handle);
		}

		// NotifyPixelsChanged

		public void NotifyPixelsChanged ()
		{
			SkiaApi.sk_bitmap_notify_pixels_changed (Handle);
		}

		// PeekPixels

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

		[Obsolete ("Use Resize(SKImageInfo info, SKSamplingOptions sampling) instead.")]
		public SKBitmap Resize (SKImageInfo info, SKFilterQuality quality) =>
			Resize (info, quality.ToSamplingOptions ());

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

		[Obsolete ("Use ScalePixels(SKBitmap destination, SKSamplingOptions sampling) instead.")]
		public bool ScalePixels (SKBitmap destination, SKFilterQuality quality) =>
			ScalePixels (destination, quality.ToSamplingOptions ());

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

		public SKData Encode (SKEncodedImageFormat format, int quality)
		{
			using var pixmap = PeekPixels ();
			return pixmap?.Encode (format, quality);
		}

		public bool Encode (Stream dst, SKEncodedImageFormat format, int quality)
		{
			using var wrapped = new SKManagedWStream (dst);
			return Encode (wrapped, format, quality);
		}

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
		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy) =>
			ToShader (tmx, tmy, SKSamplingOptions.Default, null);

		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling) =>
			ToShader (tmx, tmy, sampling, null);

		[Obsolete ("Use ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling) instead.")]
		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterQuality quality) =>
			ToShader (tmx, tmy, quality.ToSamplingOptions(), null);

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

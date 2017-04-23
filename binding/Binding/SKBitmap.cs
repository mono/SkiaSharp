//
// Bindings for SKBitmap
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	// public delegates
	public delegate void SKBitmapReleaseDelegate (IntPtr address, object context);

	// internal proxy delegates
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void SKBitmapReleaseDelegateInternal (IntPtr address, IntPtr context);

	public class SKBitmap : SKObject
	{
		private const string UnsupportedColorTypeMessage = "Setting the ColorTable is only supported for bitmaps with ColorTypes of Index8.";
		private const string UnableToAllocatePixelsMessage = "Unable to allocate pixels for the bitmap.";

		// so the GC doesn't collect the delegate
		private static readonly SKBitmapReleaseDelegateInternal releaseDelegateInternal;
		private static readonly IntPtr releaseDelegate;
		static SKBitmap ()
		{
			releaseDelegateInternal = new SKBitmapReleaseDelegateInternal (SKBitmapReleaseInternal);
			releaseDelegate = Marshal.GetFunctionPointerForDelegate (releaseDelegateInternal);
		}

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

		public SKBitmap (int width, int height, SKColorType colorType, SKAlphaType alphaType)
			: this (new SKImageInfo (width, height, colorType, alphaType))
		{
		}

		public SKBitmap (SKImageInfo info)
			: this (info, info.RowBytes)
		{
		}

		public SKBitmap (SKImageInfo info, int rowBytes)
			: this ()
		{
			if (!SkiaApi.sk_bitmap_try_alloc_pixels (Handle, ref info, (IntPtr)rowBytes)) {
				throw new Exception (UnableToAllocatePixelsMessage);
			}
		}

		public SKBitmap (SKImageInfo info, SKColorTable ctable)
			: this ()
		{
			if (!SkiaApi.sk_bitmap_try_alloc_pixels_with_color_table (Handle, ref info, IntPtr.Zero, ctable != null ? ctable.Handle : IntPtr.Zero)) {
				throw new Exception (UnableToAllocatePixelsMessage);
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_bitmap_destructor (Handle);
			}

			base.Dispose (disposing);
		}

		public void Reset ()
		{
			SkiaApi.sk_bitmap_reset (Handle);
		}

		public void SetImmutable ()
		{
			SkiaApi.sk_bitmap_set_immutable (Handle);
		}

		public void Erase (SKColor color)
		{
			SkiaApi.sk_bitmap_erase (Handle, color);
		}

		public void Erase (SKColor color, SKRectI rect)
		{
			SkiaApi.sk_bitmap_erase_rect (Handle, color, ref rect);
		}

		public byte GetAddr8(int x, int y) => SkiaApi.sk_bitmap_get_addr_8 (Handle, x, y);
		public UInt16 GetAddr16(int x, int y) => SkiaApi.sk_bitmap_get_addr_16 (Handle, x, y);
		public UInt32 GetAddr32(int x, int y) => SkiaApi.sk_bitmap_get_addr_32 (Handle, x, y);
		public IntPtr GetAddr(int x, int y) => SkiaApi.sk_bitmap_get_addr (Handle, x, y);

		public SKPMColor GetIndex8Color (int x, int y)
		{
			return SkiaApi.sk_bitmap_get_index8_color (Handle, x, y);
		}

		public SKColor GetPixel (int x, int y)
		{
			return SkiaApi.sk_bitmap_get_pixel_color (Handle, x, y);
		}

		public void SetPixel (int x, int y, SKColor color)
		{
			if (ColorType == SKColorType.Index8)
			{
				throw new NotSupportedException ("This method is not supported for bitmaps with ColorTypes of Index8.");
			}
			SkiaApi.sk_bitmap_set_pixel_color (Handle, x, y, color);
		}

		public bool CopyPixelsTo(IntPtr dst, int dstSize, int dstRowBytes = 0, bool preserveDstPad = false)
		{
			return SkiaApi.sk_bitmap_copy_pixels_to (Handle, dst, (IntPtr)dstSize, (IntPtr)dstRowBytes, preserveDstPad);
		}

		public bool CanCopyTo (SKColorType colorType)
		{
			return SkiaApi.sk_bitmap_can_copy_to (Handle, colorType);
		}

		public SKBitmap Copy ()
		{
			return Copy (ColorType);
		}

		public SKBitmap Copy (SKColorType colorType)
		{
			var destination = new SKBitmap ();
			if (!SkiaApi.sk_bitmap_copy (Handle, destination.Handle, colorType)) {
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
			return SkiaApi.sk_bitmap_copy (Handle, destination.Handle, ColorType);
		}

		public bool CopyTo (SKBitmap destination, SKColorType colorType)
		{
			if (destination == null) {
				throw new ArgumentNullException (nameof (destination));
			}
			return SkiaApi.sk_bitmap_copy (Handle, destination.Handle, colorType);
		}

		public bool ExtractSubset(SKBitmap destination, SKRectI subset)
		{
			if (destination == null) {
				throw new ArgumentNullException (nameof (destination));
			}
			return SkiaApi.sk_bitmap_extract_subset (Handle, destination.Handle, ref subset);
		}

		public bool ExtractAlpha(SKBitmap destination)
		{
			SKPointI offset;
			return ExtractAlpha (destination, null, out offset);
		}

		public bool ExtractAlpha(SKBitmap destination, out SKPointI offset)
		{
			return ExtractAlpha (destination, null, out offset);
		}

		public bool ExtractAlpha(SKBitmap destination, SKPaint paint)
		{
			SKPointI offset;
			return ExtractAlpha (destination, paint, out offset);
		}

		public bool ExtractAlpha(SKBitmap destination, SKPaint paint, out SKPointI offset)
		{
			if (destination == null) {
				throw new ArgumentNullException (nameof (destination));
			}
			return SkiaApi.sk_bitmap_extract_alpha (Handle, destination.Handle, paint == null ? IntPtr.Zero : paint.Handle, out offset);
		}

		public bool ReadyToDraw => SkiaApi.sk_bitmap_ready_to_draw (Handle); 

		public SKImageInfo Info {
			get {
				SKImageInfo info;
				SkiaApi.sk_bitmap_get_info (Handle, out info);
				return info;
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

		public int BytesPerPixel {
			get { return Info.BytesPerPixel; }
		}

		public int RowBytes {
			get { return (int)SkiaApi.sk_bitmap_get_row_bytes (Handle); }
		}

		public int ByteCount {
			get { return (int)SkiaApi.sk_bitmap_get_byte_count (Handle); }
		}

		public void LockPixels ()
		{
			SkiaApi.sk_bitmap_lock_pixels (Handle);
		}

		public void UnlockPixels ()
		{
			SkiaApi.sk_bitmap_unlock_pixels (Handle);
		}

		public IntPtr GetPixels ()
		{
			IntPtr length;
			return GetPixels (out length);
		}

		public IntPtr GetPixels (out IntPtr length)
		{
			return SkiaApi.sk_bitmap_get_pixels (Handle, out length);
		}

		public void SetPixels(IntPtr pixels)
		{
			SetPixels (pixels, ColorTable);
		}

		public void SetPixels(IntPtr pixels, SKColorTable ct)
		{
			SkiaApi.sk_bitmap_set_pixels (Handle, pixels, ct != null ? ct.Handle : IntPtr.Zero);
		}

		public void SetColorTable(SKColorTable ct)
		{
			SetPixels (GetPixels (), ct);
		}
		
		public byte[] Bytes {
			get { 
				using (new SKAutoLockPixels (this)) {
					IntPtr length;
					var pixelsPtr = GetPixels (out length);
					byte [] bytes = new byte [(int)length];
					Marshal.Copy (pixelsPtr, bytes, 0, (int)length);
					return bytes; 
				}
			}
		}

		public SKColor[] Pixels {
			get { 
				var info = Info;
				var pixels = new SKColor [info.Width * info.Height];
				SkiaApi.sk_bitmap_get_pixel_colors (Handle, pixels);
				return pixels;
			}
			set {
				SkiaApi.sk_bitmap_set_pixel_colors (Handle, value);
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

		public bool IsVolatile {
			get { return SkiaApi.sk_bitmap_is_volatile (Handle); }
			set { SkiaApi.sk_bitmap_set_volatile (Handle, value); }
		}

		public SKColorTable ColorTable {
			get { return GetObject<SKColorTable> (SkiaApi.sk_bitmap_get_colortable (Handle), false); }
		}

		public static SKImageInfo DecodeBounds (Stream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}
			return DecodeBounds (WrapManagedStream (stream));
		}

		public static SKImageInfo DecodeBounds (SKStream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}
			using (var codec = SKCodec.Create (stream)) {
				return codec.Info;
			}
		}

		public static SKImageInfo DecodeBounds (SKData data)
		{
			if (data == null) {
				throw new ArgumentNullException (nameof (data));
			}
			using (var codec = SKCodec.Create (data)) {
				return codec.Info;
			}
		}

		public static SKImageInfo DecodeBounds (string filename)
		{
			if (filename == null) {
				throw new ArgumentNullException (nameof (filename));
			}
			return DecodeBounds (new SKFileStream (filename));
		}

		public static SKImageInfo DecodeBounds (byte[] buffer)
		{
			if (buffer == null) {
				throw new ArgumentNullException (nameof (buffer));
			}
			return DecodeBounds (new SKMemoryStream (buffer));
		}

		public static SKBitmap Decode (SKCodec codec, SKImageInfo bitmapInfo)
		{
			if (codec == null) {
				throw new ArgumentNullException (nameof (codec));
			}

			// construct a color table for the decode if necessary
			SKColorTable colorTable = null;
			int colorCount = 0;
			if (bitmapInfo.ColorType == SKColorType.Index8)
			{
				colorTable = new SKColorTable ();
			}

			// read the pixels and color table
			var bitmap = new SKBitmap (bitmapInfo, colorTable);
			IntPtr length;
			var result = codec.GetPixels (bitmapInfo, bitmap.GetPixels (out length), colorTable, ref colorCount);
			if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput) {
				bitmap.Dispose ();
				bitmap = null;
			}
			return bitmap;
		}

		public static SKBitmap Decode (SKCodec codec)
		{
			if (codec == null) {
				throw new ArgumentNullException (nameof (codec));
			}
			return Decode (codec, codec.Info);
		}

		public static SKBitmap Decode (Stream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}
			return Decode (WrapManagedStream (stream));
		}

		public static SKBitmap Decode (Stream stream, SKImageInfo bitmapInfo)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}
			return Decode (WrapManagedStream (stream), bitmapInfo);
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
			return Decode (new SKFileStream (filename));
		}

		public static SKBitmap Decode (string filename, SKImageInfo bitmapInfo)
		{
			if (filename == null) {
				throw new ArgumentNullException (nameof (filename));
			}
			return Decode (new SKFileStream (filename), bitmapInfo);
		}

		public static SKBitmap Decode (byte[] buffer)
		{
			if (buffer == null) {
				throw new ArgumentNullException (nameof (buffer));
			}
			return Decode (new SKMemoryStream (buffer));
		}

		public static SKBitmap Decode (byte[] buffer, SKImageInfo bitmapInfo)
		{
			if (buffer == null) {
				throw new ArgumentNullException (nameof (buffer));
			}
			return Decode (new SKMemoryStream (buffer), bitmapInfo);
		}

		public bool InstallPixels (SKImageInfo info, IntPtr pixels)
		{
			return InstallPixels (info, pixels, info.RowBytes);
		}

		public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			return InstallPixels (info, pixels, rowBytes, null);
		}

		public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable)
		{
			return InstallPixels (info, pixels, rowBytes, ctable, null, null);
		}

		public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable, SKBitmapReleaseDelegate releaseProc, object context)
		{
			IntPtr ct = ctable == null ? IntPtr.Zero : ctable.Handle;
			if (releaseProc == null) {
				return SkiaApi.sk_bitmap_install_pixels (Handle, ref info, pixels, (IntPtr)rowBytes, ct, IntPtr.Zero, IntPtr.Zero);
			} else {
				var ctx = new NativeDelegateContext (context, releaseProc);
				return SkiaApi.sk_bitmap_install_pixels (Handle, ref info, pixels, (IntPtr)rowBytes, ct, releaseDelegate, ctx.NativeContext);
			}
		}

		public bool InstallPixels (SKPixmap pixmap)
		{
			return SkiaApi.sk_bitmap_install_pixels_with_pixmap (Handle, pixmap.Handle);
		}

		public bool InstallMaskPixels(SKMask mask)
		{
			return SkiaApi.sk_bitmap_install_mask_pixels(Handle, ref mask);
		}

		public void NotifyPixelsChanged()
		{
			SkiaApi.sk_bitmap_notify_pixels_changed(Handle);
		}

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
			return SkiaApi.sk_bitmap_peek_pixels (Handle, pixmap.Handle);
		}

		public SKBitmap Resize (SKImageInfo info, SKBitmapResizeMethod method)
		{
			var dst = new SKBitmap (info);
			var result = Resize (dst, this, method);
			if (result) {
				return dst;
			} else {
				dst.Dispose ();
				return null;
			}
		}

		public bool Resize (SKBitmap dst, SKBitmapResizeMethod method)
		{
			return Resize (dst, this, method);
		}

		public static bool Resize (SKBitmap dst, SKBitmap src, SKBitmapResizeMethod method)
		{
			using (new SKAutoLockPixels (src))
			using (var srcPix = src.PeekPixels ())
			using (var dstPix = dst.PeekPixels ()) {
				return SKPixmap.Resize (dstPix, srcPix, method);// && dst.InstallPixels (dstPix); 
			}
		}

		public static SKBitmap FromImage (SKImage image)
		{
			if (image == null) {
				throw new ArgumentNullException (nameof (image));
			}

			var info = new SKImageInfo (image.Width, image.Height, SKImageInfo.PlatformColorType, image.AlphaType);
			var bmp = new SKBitmap (info);
			if (!image.ReadPixels (info, bmp.GetPixels (), info.RowBytes, 0, 0))
			{
				bmp.Dispose ();
				bmp = null;
			}
			return bmp;
		}

		public bool Encode (SKWStream dst, SKEncodedImageFormat format, int quality)
		{
			using (new SKAutoLockPixels (this))
			using (var pixmap = new SKPixmap ()) {
				return PeekPixels (pixmap) && pixmap.Encode (dst, format, quality);
			}
		}

		private static SKStream WrapManagedStream (Stream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			// we will need a seekable stream, so buffer it if need be
			if (stream.CanSeek) {
				return new SKManagedStream (stream, true);
			} else {
				var buffered = new SKFrontBufferedStream (stream, SKCodec.MinBufferedBytesNeeded, true);
				return new SKManagedStream (buffered, true);
			}
		}


		// internal proxy
		#if __IOS__
		[ObjCRuntime.MonoPInvokeCallback (typeof (SKBitmapReleaseDelegateInternal))]
		#endif
		private static void SKBitmapReleaseInternal (IntPtr address, IntPtr context)
		{
			using (var ctx = NativeDelegateContext.Unwrap (context)) {
				ctx.GetDelegate<SKBitmapReleaseDelegate> () (address, ctx.ManagedContext);
			}
		}
	}

	public class SKAutoLockPixels : IDisposable
	{
		private SKBitmap bitmap;
		private readonly bool doLock;

		public SKAutoLockPixels (SKBitmap bitmap)
			: this (bitmap, true)
		{
		}

		public SKAutoLockPixels (SKBitmap bitmap, bool doLock)
		{
			this.bitmap = bitmap;
			this.doLock = doLock;

			if (bitmap != null && doLock) {
				bitmap.LockPixels ();
			}
		}

		public void Dispose ()
		{
			if (bitmap != null && doLock) {
				bitmap.UnlockPixels ();
			}
		}

		/// <summary>
		/// Perform the unlock now, instead of waiting for the Dispose.
		/// Will only do this once.
		/// </summary>
		public void Unlock ()
		{
			if (bitmap != null && doLock) {
				bitmap.UnlockPixels ();
				bitmap = null;
			}
		}
	}
}

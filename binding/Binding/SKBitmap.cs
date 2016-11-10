//
// Bindings for SKBitmap
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public class SKBitmap : SKObject
	{
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
				throw new Exception ("Unable to allocate pixels for the bitmap.");
			}
		}

		public SKBitmap (SKImageInfo info, SKColorTable ctable)
			: this ()
		{
			if (!SkiaApi.sk_bitmap_try_alloc_pixels_with_color_table (Handle, ref info, IntPtr.Zero, ctable != null ? ctable.Handle : IntPtr.Zero)) {
				throw new Exception ("Unable to allocate pixels for the bitmap.");
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

		public SKColor GetIndex8Color (int x, int y)
		{
			return SkiaApi.sk_bitmap_get_index8_color (Handle, x, y);
		}

		public SKColor GetPixel (int x, int y)
		{
			return SkiaApi.sk_bitmap_get_pixel_color (Handle, x, y);
		}

		public void SetPixel (int x, int y, SKColor color)
		{
			SkiaApi.sk_bitmap_set_pixel_color (Handle, x, y, color);
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

		public IntPtr GetPixels (out IntPtr length)
		{
			return SkiaApi.sk_bitmap_get_pixels (Handle, out length);
		}
		
		public byte[] Bytes {
			get { 
				LockPixels ();
				try {
					IntPtr length;
					var pixelsPtr = GetPixels (out length);
					byte[] bytes = new byte[(int)length];
					Marshal.Copy (pixelsPtr, bytes, 0, (int)length);
					return bytes; 
				} finally {
					UnlockPixels ();
				}
			}
		}

		public SKColor[] Pixels {
			get { 
				var info = Info;
				var pixels = new SKColor[info.Width * info.Height];
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
			get { return GetObject<SKColorTable> (SkiaApi.sk_bitmap_get_colortable (Handle)); }
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

		public static SKBitmap Decode (SKStream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}
			using (var codec = SKCodec.Create (stream)) {
				return Decode (codec);
			}
		}

		public static SKBitmap Decode (SKStream stream, SKImageInfo bitmapInfo)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}
			using (var codec = SKCodec.Create (stream)) {
				return Decode (codec, bitmapInfo);
			}
		}

		public static SKBitmap Decode (SKData data)
		{
			if (data == null) {
				throw new ArgumentNullException (nameof (data));
			}
			using (var codec = SKCodec.Create (data)) {
				return Decode (codec);
			}
		}

		public static SKBitmap Decode (SKData data, SKImageInfo bitmapInfo)
		{
			if (data == null) {
				throw new ArgumentNullException (nameof (data));
			}
			using (var codec = SKCodec.Create (data)) {
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
	}
}

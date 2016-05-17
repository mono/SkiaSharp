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
		internal SKBitmap (IntPtr handle)
			: base (handle)
		{
		}

		public SKBitmap ()
			: this (SkiaApi.sk_bitmap_new ())
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKBitmap instance.");
			}
		}

		public SKBitmap (int width, int height, bool isOpaque = false)
			: this (width, height, SKColorType.N_32, isOpaque ? SKAlphaType.Opaque : SKAlphaType.Premul)
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

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
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
			return SkiaApi.sk_bitmap_copy (Handle, destination.Handle, ColorType);
		}

		public bool CopyTo (SKBitmap destination, SKColorType colorType)
		{
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

		public static SKImageInfo DecodeBounds (SKStreamRewindable stream, SKColorType pref = SKColorType.Unknown)
		{
			SKImageInfo info;
			SKImageDecoder.DecodeStreamBounds (stream, out info, pref);
			return info;
		}

		public static SKImageInfo DecodeBounds (string filename, SKColorType pref = SKColorType.Unknown)
		{
			SKImageInfo info;
			SKImageDecoder.DecodeFileBounds (filename, out info, pref);
			return info;
		}

		public static SKImageInfo DecodeBounds (byte[] buffer, SKColorType pref = SKColorType.Unknown)
		{
			SKImageInfo info;
			SKImageDecoder.DecodeMemoryBounds (buffer, out info, pref);
			return info;
		}

		public static SKBitmap Decode (SKStreamRewindable stream, SKColorType pref = SKColorType.Unknown)
		{
			var bitmap = new SKBitmap ();
			if (!SKImageDecoder.DecodeStream (stream, bitmap, pref)) {
				bitmap.Dispose ();
				bitmap = null;
			}
			return bitmap;
		}

		public static SKBitmap Decode (string filename, SKColorType pref = SKColorType.Unknown)
		{
			var bitmap = new SKBitmap ();
			if (!SKImageDecoder.DecodeFile (filename, bitmap, pref)) {
				bitmap.Dispose();
				bitmap = null;
			}
			return bitmap;
		}

		public static SKBitmap Decode (byte[] buffer, SKColorType pref = SKColorType.Unknown)
		{
			var bitmap = new SKBitmap ();
			if (!SKImageDecoder.DecodeMemory (buffer, bitmap, pref)) {
				bitmap.Dispose ();
				bitmap = null;
			}
			return bitmap;
		}
	}
}

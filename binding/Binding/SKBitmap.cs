//
// Bindings for SKBitmap
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2015 Xamarin Inc
//

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public class SKBitmap : IDisposable
	{
		internal IntPtr handle;

		internal SKBitmap (IntPtr handle)
		{
			this.handle = handle;
		}

		public SKBitmap ()
		{
			handle = SkiaApi.sk_bitmap_new ();
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
			if (!SkiaApi.sk_bitmap_try_alloc_pixels (handle, ref info, (IntPtr)rowBytes)) {
				throw new Exception ("Unable to allocate pixels for the bitmap.");
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				SkiaApi.sk_bitmap_destructor (handle);
				handle = IntPtr.Zero;
			}
		}

		~SKBitmap ()
		{
			Dispose (false);
		}

		public void Reset ()
		{
			SkiaApi.sk_bitmap_reset (handle);
		}

		public void SetImmutable ()
		{
			SkiaApi.sk_bitmap_set_immutable (handle);
		}

		public void Erase (SKColor color)
		{
			SkiaApi.sk_bitmap_erase (handle, color);
		}

		public void Erase (SKColor color, SKRectI rect)
		{
			SkiaApi.sk_bitmap_erase_rect (handle, color, ref rect);
		}

		public SKColor GetPixel (int x, int y)
		{
			return SkiaApi.sk_bitmap_get_pixel_color (handle, x, y);
		}

		public void SetPixel (int x, int y, SKColor color)
		{
			SkiaApi.sk_bitmap_set_pixel_color (handle, x, y, color);
		}

		public bool CanCopyTo (SKColorType colorType)
		{
			return SkiaApi.sk_bitmap_can_copy_to (handle, colorType);
		}

		public SKBitmap Copy ()
		{
			return Copy (ColorType);
		}

		public SKBitmap Copy (SKColorType colorType)
		{
			var destination = new SKBitmap ();
			if (!SkiaApi.sk_bitmap_copy (handle, destination.handle, colorType)) {
				destination.Dispose ();
				destination = null;
			}
			return destination;
		}

		public bool CopyTo (SKBitmap destination)
		{
			return SkiaApi.sk_bitmap_copy (handle, destination.handle, ColorType);
		}

		public bool CopyTo (SKBitmap destination, SKColorType colorType)
		{
			return SkiaApi.sk_bitmap_copy (handle, destination.handle, colorType);
		}

		public SKImageInfo Info {
			get { return SkiaApi.sk_bitmap_get_info (handle); }
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
			get { return (int)SkiaApi.sk_bitmap_get_row_bytes (handle); }
		}

		public int ByteCount {
			get { return (int)SkiaApi.sk_bitmap_get_byte_count (handle); }
		}

		public byte[] Bytes {
			get { 
				SkiaApi.sk_bitmap_lock_pixels (handle);
				try {
					IntPtr length;
					var pixelsPtr = SkiaApi.sk_bitmap_get_pixels (handle, out length);
					byte[] bytes = new byte[(int)length];
					Marshal.Copy (pixelsPtr, bytes, 0, (int)length);
					return bytes; 
				} finally {
					SkiaApi.sk_bitmap_unlock_pixels (handle);
				}
			}
		}

		public SKColor[] Pixels {
			get { 
				var info = SkiaApi.sk_bitmap_get_info (handle);
				var pixels = new SKColor[info.Width * info.Height];
				SkiaApi.sk_bitmap_get_pixel_colors (handle, pixels);
				return pixels; 
			}
			set { 
				SkiaApi.sk_bitmap_set_pixel_colors (handle, value);
			}
		}

		public bool IsEmpty {
			get { return Info.IsEmpty; }
		}

		public bool IsNull {
			get { return SkiaApi.sk_bitmap_is_null (handle); }
		}

		public bool DrawsNothing { 
			get { return IsEmpty || IsNull; }
		}

		public bool IsImmutable {
			get { return SkiaApi.sk_bitmap_is_immutable (handle); }
		}

		public bool IsVolatile {
			get { return SkiaApi.sk_bitmap_is_volatile (handle); }
			set { SkiaApi.sk_bitmap_set_volatile (handle, value); }
		}

		public static SKImageInfo DecodeBounds (SKStreamRewindable stream, SKColorType pref = SKColorType.Unknown)
		{
			SKImageInfo info;
			SKImageDecoderFormat format = SKImageDecoderFormat.Unknown;
			SKImageDecoder.DecodeBounds (stream, pref, out info, ref format);
			return info;
		}

		public static SKBitmap Decode (SKStreamRewindable stream, SKColorType pref = SKColorType.Unknown)
		{
			var bitmap = new SKBitmap ();
			if (!SKImageDecoder.Decode (stream, bitmap, pref, SKImageDecoderMode.DecodePixels)) {
				bitmap.Dispose ();
				bitmap = null;
			}
			return bitmap;
		}
	}
}

using System;
// compile with: /unsafe

namespace SkiaSharp
{
	public class SKMask : SKObject
	{
		[Preserve]
		internal SKMask (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		internal SKMask(IntPtr pixels, SKMaskFormat format, uint rowBytes, SKRectI bounds)
		: this (SkiaApi.sk_mask_new(pixels, format, rowBytes, ref bounds), true)
		{
		}

		public static SKMask CreateMask(byte[] pixels, SKMaskFormat format, uint rowBytes, SKRectI bounds)
		{
			unsafe {
				fixed (byte* p = pixels) {
					IntPtr ptr = (IntPtr)p;
					return new SKMask(ptr, format, rowBytes, bounds);
				}
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_mask_destructor (Handle);
			}

			base.Dispose (disposing);
		}

		public SKColor GetPixelColor(int x, int y)
		{
			return new SKColor((uint)SkiaApi.sk_mask_get_pixel_color(Handle, x, y));
		}

		public UInt32 Size {
			get {
				return (UInt32)SkiaApi.sk_mask_get_image_size(Handle);
			}
		}

		public IntPtr Bytes { 
			get {
				return SkiaApi.sk_mask_get_image(Handle);
			}
		}

		public SKRectI Bounds {
			get {
				return SkiaApi.sk_mask_get_bounds(Handle);
			}
		}

		public UInt32 RowBytes {
			get {
				return SkiaApi.sk_mask_get_row_bytes(Handle);
			}
		}

		public SKMaskFormat Format {
			get {
				return SkiaApi.sk_mask_get_format(Handle);
			}
		}
	}
}
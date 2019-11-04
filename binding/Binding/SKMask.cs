using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public unsafe partial struct SKMask
	{
		public SKMask(IntPtr image, SKRectI bounds, UInt32 rowBytes, SKMaskFormat format)
		{
			this.fBounds = bounds;
			this.fRowBytes = rowBytes;
			this.fFormat = format;
			this.fImage = (byte*)image;
		}

		public SKMask(SKRectI bounds, UInt32 rowBytes, SKMaskFormat format)
		{
			this.fBounds = bounds;
			this.fRowBytes = rowBytes;
			this.fFormat = format;
			this.fImage = null;
		}

		public IntPtr Image {
			get => (IntPtr)fImage;
			set => fImage = (byte*)value;
		}
		public SKRectI Bounds {
			get => fBounds;
			set => fBounds = value;
		}
		public UInt32 RowBytes {
			get => fRowBytes;
			set => fRowBytes = value;
		}
		public SKMaskFormat Format {
			get => fFormat;
			set => fFormat = value;
		}
		public bool IsEmpty {
			get {
				fixed (SKMask* t = &this) {
					return SkiaApi.sk_mask_is_empty(t);
				}
			}
		}

		public long AllocateImage()
		{
			fixed (SKMask* t = &this) {
				var size = SkiaApi.sk_mask_compute_total_image_size(t);
				fImage = SkiaApi.sk_mask_alloc_image(size);
				return (long)size;
			}
		}
		public void FreeImage()
		{
			if (fImage != null) {
				SKMask.FreeImage((IntPtr)fImage);
				fImage = null;
			}
		}

		public long ComputeImageSize()
		{
			fixed (SKMask* t = &this) {
				return (long)SkiaApi.sk_mask_compute_image_size(t);
			}
		}

		public long ComputeTotalImageSize()
		{
			fixed (SKMask* t = &this) {
				return (long)SkiaApi.sk_mask_compute_total_image_size(t);
			}
		}

		public byte GetAddr1(int x, int y)
		{
			fixed (SKMask* t = &this) {
				return SkiaApi.sk_mask_get_addr_1(t, x, y);
			}
		}

		public byte GetAddr8(int x, int y)
		{
			fixed (SKMask* t = &this) {
				return SkiaApi.sk_mask_get_addr_8(t, x, y);
			}
		}

		public UInt16 GetAddr16(int x, int y)
		{
			fixed (SKMask* t = &this) {
				return SkiaApi.sk_mask_get_addr_lcd_16(t, x, y);
			}
		}

		public UInt32 GetAddr32 (int x, int y)
		{
			fixed (SKMask* t = &this) {
				return SkiaApi.sk_mask_get_addr_32 (t, x, y);
			}
		}

		public IntPtr GetAddr(int x, int y)
		{
			fixed (SKMask* t = &this) {
				return (IntPtr)SkiaApi.sk_mask_get_addr(t, x, y);
			}
		}

		public static IntPtr AllocateImage(long size) => (IntPtr)SkiaApi.sk_mask_alloc_image((IntPtr)size);
		public static void FreeImage(IntPtr image) => SkiaApi.sk_mask_free_image((byte*)image);

		public static SKMask Create(byte[] image, SKRectI bounds, UInt32 rowBytes, SKMaskFormat format)
		{
			// create the mask
			var mask = new SKMask(bounds, rowBytes, format);

			// is there the right amount of space in the mask
			if (image.Length != mask.ComputeTotalImageSize())
			{
				// Note: buffer.Length must match bounds.Height * rowBytes
				var expectedHeight = bounds.Height * rowBytes;
				var message = $"Length of image ({image.Length}) does not match the computed size of the mask ({expectedHeight}). Check the {nameof(bounds)} and {nameof(rowBytes)}.";
				throw new ArgumentException(message);
			}

			// copy the image data
			mask.AllocateImage();
			Marshal.Copy(image, 0, (IntPtr)mask.fImage, image.Length);

			// return the mask
			return mask;
		}
	}

	public class SKAutoMaskFreeImage : IDisposable
	{
		private IntPtr image;

		public SKAutoMaskFreeImage(IntPtr maskImage)
		{
			image = maskImage;
		}

		public void Dispose()
		{
			if (image != IntPtr.Zero)
			{
				SKMask.FreeImage(image);
				image = IntPtr.Zero;
			}
		}
	}
}

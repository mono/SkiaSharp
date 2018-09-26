using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SKMask
	{
		private IntPtr image;
		private SKRectI bounds;
		private UInt32 rowBytes;
		private SKMaskFormat format;

		public SKMask(IntPtr image, SKRectI bounds, UInt32 rowBytes, SKMaskFormat format)
		{
			this.bounds = bounds;
			this.rowBytes = rowBytes;
			this.format = format;
			this.image = image;
		}

		public SKMask(SKRectI bounds, UInt32 rowBytes, SKMaskFormat format)
		{
			this.bounds = bounds;
			this.rowBytes = rowBytes;
			this.format = format;
			this.image = IntPtr.Zero;
		}

		public IntPtr Image {
			get => image;
			set => image = value;
		}
		public SKRectI Bounds {
			get => bounds;
			set => bounds = value;
		}
		public UInt32 RowBytes {
			get => rowBytes;
			set => rowBytes = value;
		}
		public SKMaskFormat Format {
			get => format;
			set => format = value;
		}
		public bool IsEmpty => SkiaApi.sk_mask_is_empty(ref this);

		public long AllocateImage()
		{
			var size = ComputeTotalImageSize();
			image = SKMask.AllocateImage(size);
			return size;
		}
		public void FreeImage()
		{
			if (image != IntPtr.Zero)
			{
				SKMask.FreeImage(image);
				image = IntPtr.Zero;
			}
		}

		public long ComputeImageSize() => (long)SkiaApi.sk_mask_compute_image_size(ref this);
		public long ComputeTotalImageSize() => (long)SkiaApi.sk_mask_compute_total_image_size(ref this);
		public byte GetAddr1(int x, int y) => SkiaApi.sk_mask_get_addr_1(ref this, x, y);
		public byte GetAddr8(int x, int y) => SkiaApi.sk_mask_get_addr_8(ref this, x, y);
		public UInt16 GetAddr16(int x, int y) => SkiaApi.sk_mask_get_addr_lcd_16(ref this, x, y);
		public UInt32 GetAddr32(int x, int y) => SkiaApi.sk_mask_get_addr_32(ref this, x, y);
		public IntPtr GetAddr(int x, int y) => SkiaApi.sk_mask_get_addr(ref this, x, y);

		public static IntPtr AllocateImage(long size) => SkiaApi.sk_mask_alloc_image((IntPtr)size);
		public static void FreeImage(IntPtr image) => SkiaApi.sk_mask_free_image(image);

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
			Marshal.Copy(image, 0, mask.image, image.Length);

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

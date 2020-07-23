using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public unsafe partial struct SKMask
	{
		public SKMask (IntPtr image, SKRectI bounds, UInt32 rowBytes, SKMaskFormat format)
		{
			fBounds = bounds;
			fRowBytes = rowBytes;
			fFormat = format;
			fImage = (byte*)image;
		}

		public SKMask (SKRectI bounds, UInt32 rowBytes, SKMaskFormat format)
		{
			fBounds = bounds;
			fRowBytes = rowBytes;
			fFormat = format;
			fImage = null;
		}

		// properties

		public IntPtr Image {
			readonly get => (IntPtr)fImage;
			set => fImage = (byte*)value;
		}

		public Span<byte> GetImageSpan () =>
			new Span<byte> ((void*)Image, (int)ComputeTotalImageSize ());

		public SKRectI Bounds {
			readonly get => fBounds;
			set => fBounds = value;
		}

		public UInt32 RowBytes {
			readonly get => fRowBytes;
			set => fRowBytes = value;
		}

		public SKMaskFormat Format {
			readonly get => fFormat;
			set => fFormat = value;
		}

		public readonly bool IsEmpty {
			get {
				fixed (SKMask* t = &this) {
					return SkiaApi.sk_mask_is_empty (t);
				}
			}
		}

		// allocate / free

		public long AllocateImage ()
		{
			fixed (SKMask* t = &this) {
				var size = SkiaApi.sk_mask_compute_total_image_size (t);
				fImage = SkiaApi.sk_mask_alloc_image (size);
				return (long)size;
			}
		}

		public void FreeImage ()
		{
			if (fImage != null) {
				SKMask.FreeImage ((IntPtr)fImage);
				fImage = null;
			}
		}

		// Compute*

		public readonly long ComputeImageSize ()
		{
			fixed (SKMask* t = &this) {
				return (long)SkiaApi.sk_mask_compute_image_size (t);
			}
		}

		public readonly long ComputeTotalImageSize ()
		{
			fixed (SKMask* t = &this) {
				return (long)SkiaApi.sk_mask_compute_total_image_size (t);
			}
		}

		// GetAddr*

		public readonly byte GetAddr1 (int x, int y)
		{
			fixed (SKMask* t = &this) {
				return *SkiaApi.sk_mask_get_addr_1 (t, x, y);
			}
		}

		public readonly byte GetAddr8 (int x, int y)
		{
			fixed (SKMask* t = &this) {
				return *SkiaApi.sk_mask_get_addr_8 (t, x, y);
			}
		}

		public readonly UInt16 GetAddr16 (int x, int y)
		{
			fixed (SKMask* t = &this) {
				return *SkiaApi.sk_mask_get_addr_lcd_16 (t, x, y);
			}
		}

		public readonly UInt32 GetAddr32 (int x, int y)
		{
			fixed (SKMask* t = &this) {
				return *SkiaApi.sk_mask_get_addr_32 (t, x, y);
			}
		}

		public readonly IntPtr GetAddr (int x, int y)
		{
			fixed (SKMask* t = &this) {
				return (IntPtr)SkiaApi.sk_mask_get_addr (t, x, y);
			}
		}

		// statics

		public static IntPtr AllocateImage (long size) =>
			(IntPtr)SkiaApi.sk_mask_alloc_image ((IntPtr)size);

		public static void FreeImage (IntPtr image) =>
			SkiaApi.sk_mask_free_image ((byte*)image);

		public static SKMask Create (byte[] image, SKRectI bounds, UInt32 rowBytes, SKMaskFormat format) =>
			Create (image.AsSpan (), bounds, rowBytes, format);

		public static SKMask Create (ReadOnlySpan<byte> image, SKRectI bounds, UInt32 rowBytes, SKMaskFormat format)
		{
			// create the mask
			var mask = new SKMask (bounds, rowBytes, format);

			// calculate the size
			var imageSize = (int)mask.ComputeTotalImageSize ();

			// is there the right amount of space in the mask
			if (image.Length != imageSize) {
				// Note: buffer.Length must match bounds.Height * rowBytes
				var expectedSize = bounds.Height * rowBytes;
				var message = $"Length of image ({image.Length}) does not match the computed size of the mask ({expectedSize}). Check the {nameof (bounds)} and {nameof (rowBytes)}.";
				throw new ArgumentException (message);
			}

			// allocate and copy the image data
			mask.AllocateImage ();
			image.CopyTo (new Span<byte> ((void*)mask.Image, imageSize));

			// return the mask
			return mask;
		}
	}

	public class SKAutoMaskFreeImage : IDisposable
	{
		private IntPtr image;

		public SKAutoMaskFreeImage (IntPtr maskImage)
		{
			image = maskImage;
		}

		public void Dispose ()
		{
			if (image != IntPtr.Zero) {
				SKMask.FreeImage (image);
				image = IntPtr.Zero;
			}
		}
	}
}

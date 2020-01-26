using System;

namespace SkiaSharp
{
	public unsafe partial struct SKMask
	{
		public SKMask (IntPtr image, SKRectI bounds, uint rowBytes, SKMaskFormat format)
		{
			fBounds = bounds;
			fRowBytes = rowBytes;
			fFormat = format;
			fImage = (byte*)image;
		}

		public SKMask (SKRectI bounds, uint rowBytes, SKMaskFormat format)
		{
			fBounds = bounds;
			fRowBytes = rowBytes;
			fFormat = format;
			fImage = null;
		}

		public IntPtr Image {
			readonly get => (IntPtr)fImage;
			set => fImage = (byte*)value;
		}

		public SKRectI Bounds {
			readonly get => fBounds;
			set => fBounds = value;
		}

		public uint RowBytes {
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

		public readonly byte GetImage1 (int x, int y)
		{
			fixed (SKMask* t = &this) {
				return *SkiaApi.sk_mask_get_addr_1 (t, x, y);
			}
		}

		public readonly byte GetImage8 (int x, int y)
		{
			fixed (SKMask* t = &this) {
				return *SkiaApi.sk_mask_get_addr_8 (t, x, y);
			}
		}

		public readonly ushort GetImage16 (int x, int y)
		{
			fixed (SKMask* t = &this) {
				return *SkiaApi.sk_mask_get_addr_lcd_16 (t, x, y);
			}
		}

		public readonly uint GetImage32 (int x, int y)
		{
			fixed (SKMask* t = &this) {
				return *SkiaApi.sk_mask_get_addr_32 (t, x, y);
			}
		}

		public readonly IntPtr GetAddress (int x, int y)
		{
			fixed (SKMask* t = &this) {
				return (IntPtr)SkiaApi.sk_mask_get_addr (t, x, y);
			}
		}

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

		public static IntPtr AllocateImage (long size) =>
			(IntPtr)SkiaApi.sk_mask_alloc_image ((IntPtr)size);

		public static void FreeImage (IntPtr image) =>
			SkiaApi.sk_mask_free_image ((byte*)image);

		public static SKMask Create (ReadOnlySpan<byte> image, SKRectI bounds, uint rowBytes, SKMaskFormat format)
		{
			// create the mask
			var mask = new SKMask (bounds, rowBytes, format);

			// is there the right amount of space in the mask
			if (image.Length != mask.ComputeTotalImageSize ()) {
				// Note: buffer.Length must match bounds.Height * rowBytes
				var expectedHeight = bounds.Height * rowBytes;
				var message = $"Length of image ({image.Length}) does not match the computed size of the mask ({expectedHeight}). Check the {nameof (bounds)} and {nameof (rowBytes)}.";
				throw new ArgumentException (message);
			}

			// copy the image data
			mask.AllocateImage ();
			image.CopyTo (new Span<byte> (mask.fImage, image.Length));

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

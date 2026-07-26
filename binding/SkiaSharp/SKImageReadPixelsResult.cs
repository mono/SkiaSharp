#nullable disable

using System;

namespace SkiaSharp
{
	public sealed unsafe class SKImageReadPixelsResult : IDisposable
	{
		private IntPtr handle;
		private readonly SKImageInfo info;

		internal SKImageReadPixelsResult (IntPtr handle, SKImageInfo info)
		{
			this.handle = handle;
			this.info = info;
		}

		public int PlaneCount {
			get {
				ThrowIfDisposed ();
				return SkiaApi.sk_image_async_read_result_get_count (handle);
			}
		}

		public int GetPlaneRowBytes (int planeIndex)
		{
			ThrowIfDisposed ();
			if (planeIndex < 0 || planeIndex >= PlaneCount)
				throw new ArgumentOutOfRangeException (nameof (planeIndex));
			return (int)SkiaApi.sk_image_async_read_result_get_row_bytes (handle, planeIndex);
		}

		public ReadOnlySpan<byte> GetPlaneData (int planeIndex)
		{
			ThrowIfDisposed ();
			if (planeIndex < 0 || planeIndex >= PlaneCount)
				throw new ArgumentOutOfRangeException (nameof (planeIndex));

			var src = SkiaApi.sk_image_async_read_result_get_data (handle, planeIndex);
			if (src == null)
				return default;

			// The raw plane exactly as Skia laid it out, including any per-row padding
			// (rowBytes >= width * bytesPerPixel). Use GetPlaneRowBytes as the stride.
			var rowBytes = (int)SkiaApi.sk_image_async_read_result_get_row_bytes (handle, planeIndex);
			var length = checked (rowBytes * info.Height);
			return new ReadOnlySpan<byte> (src, length);
		}

		// Copies the plane into destination as tightly-packed pixels (any transfer-buffer row
		// padding is stripped, so the destination stride is info.RowBytes).
		public void CopyPlaneTo (int planeIndex, Span<byte> destination)
		{
			ThrowIfDisposed ();
			if (planeIndex < 0 || planeIndex >= PlaneCount)
				throw new ArgumentOutOfRangeException (nameof (planeIndex));

			var src = (byte*)SkiaApi.sk_image_async_read_result_get_data (handle, planeIndex);
			if (src == null)
				throw new InvalidOperationException ("Plane data is null.");

			var packedRowBytes = info.RowBytes;
			var height = info.Height;
			var required = checked (packedRowBytes * height);
			if (destination.Length < required)
				throw new ArgumentException (
					$"Destination must be at least {required} bytes ({height} rows × {packedRowBytes}); got {destination.Length}.",
					nameof (destination));

			var srcRowBytes = (int)SkiaApi.sk_image_async_read_result_get_row_bytes (handle, planeIndex);
			var copy = Math.Min (srcRowBytes, packedRowBytes);
			for (var y = 0; y < height; y++)
				new ReadOnlySpan<byte> (src + (y * srcRowBytes), copy).CopyTo (destination.Slice (y * packedRowBytes));
		}

		// Returns a tightly-packed copy of the plane that outlives the callback.
		public byte[] ToArray (int planeIndex = 0)
		{
			ThrowIfDisposed ();
			var pixels = new byte[info.BytesSize];
			CopyPlaneTo (planeIndex, pixels);
			return pixels;
		}

		// Materializes the whole (single-plane) result into an owned SKImage that outlives the callback.
		public SKImage ToImage ()
		{
			ThrowIfDisposed ();
			if (PlaneCount != 1)
				throw new InvalidOperationException ("ToImage is only supported for single-plane (interleaved) results.");

			var src = SkiaApi.sk_image_async_read_result_get_data (handle, 0);
			if (src == null)
				throw new InvalidOperationException ("Plane data is null.");

			var rowBytes = (int)SkiaApi.sk_image_async_read_result_get_row_bytes (handle, 0);
			return SKImage.FromPixelCopy (info, (IntPtr)src, rowBytes);
		}

		// Materializes the whole (single-plane) result into an owned SKBitmap that outlives the callback.
		public SKBitmap ToBitmap ()
		{
			ThrowIfDisposed ();
			if (PlaneCount != 1)
				throw new InvalidOperationException ("ToBitmap is only supported for single-plane (interleaved) results.");

			var bitmap = new SKBitmap (info);
			CopyPlaneTo (0, new Span<byte> ((void*)bitmap.GetPixels (), bitmap.ByteCount));
			return bitmap;
		}

		public void Dispose () => handle = IntPtr.Zero;

		private void ThrowIfDisposed ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException (nameof (SKImageReadPixelsResult),
					"The async read result is only valid for the duration of the callback.");
		}
	}
}

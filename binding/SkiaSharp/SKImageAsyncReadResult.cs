#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// The pixel data produced by <see cref="SKImage.RequestReadPixels(SKImageInfo, SKRectI, Action{SKImageAsyncReadResult})" />
	/// or <see cref="SKSurface.RequestReadPixels(SKImageInfo, SKRectI, Action{SKImageAsyncReadResult})" />.
	/// </summary>
	/// <remarks>
	/// This is a non-owning view over the native result. It is only valid for the duration of the
	/// callback it is delivered to; the underlying pixels are released as soon as the callback returns.
	/// Accessing any member after the callback has returned throws <see cref="ObjectDisposedException" />.
	/// </remarks>
	public sealed unsafe class SKImageAsyncReadResult : IDisposable
	{
		private IntPtr handle;

		internal SKImageAsyncReadResult (IntPtr handle)
		{
			this.handle = handle;
		}

		/// <summary>
		/// Gets the number of planes in the result. This is 1 for the standard (RGBA) read.
		/// </summary>
		public int PlaneCount {
			get {
				ThrowIfDisposed ();
				return SkiaApi.sk_image_async_read_result_get_count (handle);
			}
		}

		/// <summary>
		/// Gets a pointer to the pixel data for the specified plane. The pointer is only valid for the
		/// duration of the callback.
		/// </summary>
		/// <param name="planeIndex">The zero-based index of the plane.</param>
		public IntPtr GetPlaneData (int planeIndex)
		{
			ThrowIfDisposed ();
			if (planeIndex < 0 || planeIndex >= PlaneCount)
				throw new ArgumentOutOfRangeException (nameof (planeIndex));
			return (IntPtr)SkiaApi.sk_image_async_read_result_get_data (handle, planeIndex);
		}

		/// <summary>
		/// Gets the number of bytes in a row of the specified plane.
		/// </summary>
		/// <param name="planeIndex">The zero-based index of the plane.</param>
		public int GetPlaneRowBytes (int planeIndex)
		{
			ThrowIfDisposed ();
			if (planeIndex < 0 || planeIndex >= PlaneCount)
				throw new ArgumentOutOfRangeException (nameof (planeIndex));
			return (int)SkiaApi.sk_image_async_read_result_get_row_bytes (handle, planeIndex);
		}

		/// <summary>
		/// Copies <paramref name="rowCount" /> rows of the specified plane into <paramref name="destination" />.
		/// </summary>
		/// <param name="planeIndex">The zero-based index of the plane.</param>
		/// <param name="destination">The buffer to copy the pixel data into.</param>
		/// <param name="rowCount">The number of rows to copy (typically the destination image height).</param>
		public void CopyPlaneTo (int planeIndex, Span<byte> destination, int rowCount)
		{
			ThrowIfDisposed ();
			if (planeIndex < 0 || planeIndex >= PlaneCount)
				throw new ArgumentOutOfRangeException (nameof (planeIndex));
			if (rowCount < 0)
				throw new ArgumentOutOfRangeException (nameof (rowCount));

			var srcRowBytes = (int)SkiaApi.sk_image_async_read_result_get_row_bytes (handle, planeIndex);
			var required = checked (srcRowBytes * rowCount);
			if (destination.Length < required)
				throw new ArgumentException (
					$"Destination must be at least {required} bytes (rowCount={rowCount} × rowBytes={srcRowBytes}); got {destination.Length}.",
					nameof (destination));

			var src = SkiaApi.sk_image_async_read_result_get_data (handle, planeIndex);
			if (src == null)
				throw new InvalidOperationException ("Plane data is null.");
			new ReadOnlySpan<byte> (src, required).CopyTo (destination);
		}

		/// <summary>
		/// Invalidates this view. This is called automatically when the callback returns.
		/// </summary>
		public void Dispose () => handle = IntPtr.Zero;

		private void ThrowIfDisposed ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException (nameof (SKImageAsyncReadResult),
					"The async read result is only valid for the duration of the callback.");
		}
	}
}

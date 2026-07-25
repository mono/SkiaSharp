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

			// The (non-YUV) read produces a single plane whose height matches the requested info.
			var rowBytes = (int)SkiaApi.sk_image_async_read_result_get_row_bytes (handle, planeIndex);
			var length = checked (rowBytes * info.Height);
			return new ReadOnlySpan<byte> (src, length);
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

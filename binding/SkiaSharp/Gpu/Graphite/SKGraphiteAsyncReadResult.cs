#nullable disable

using System;

namespace SkiaSharp
{
	public sealed unsafe class SKGraphiteAsyncReadResult : IDisposable
	{
		private IntPtr handle;

		internal SKGraphiteAsyncReadResult (IntPtr handle)
		{
			this.handle = handle;
		}

		public int PlaneCount {
			get {
				ThrowIfDisposed ();
				return SkiaApi.sk_graphite_async_read_result_get_count (handle);
			}
		}

		public IntPtr GetPlaneData (int planeIndex)
		{
			ThrowIfDisposed ();
			if (planeIndex < 0 || planeIndex >= PlaneCount)
				throw new ArgumentOutOfRangeException (nameof (planeIndex));
			return (IntPtr)SkiaApi.sk_graphite_async_read_result_get_data (handle, planeIndex);
		}

		public int GetPlaneRowBytes (int planeIndex)
		{
			ThrowIfDisposed ();
			if (planeIndex < 0 || planeIndex >= PlaneCount)
				throw new ArgumentOutOfRangeException (nameof (planeIndex));
			return (int)SkiaApi.sk_graphite_async_read_result_get_row_bytes (handle, planeIndex);
		}

		public void CopyPlaneTo (int planeIndex, Span<byte> destination, int rowCount)
		{
			ThrowIfDisposed ();
			if (planeIndex < 0 || planeIndex >= PlaneCount)
				throw new ArgumentOutOfRangeException (nameof (planeIndex));
			if (rowCount < 0)
				throw new ArgumentOutOfRangeException (nameof (rowCount));

			int srcRowBytes = (int)SkiaApi.sk_graphite_async_read_result_get_row_bytes (handle, planeIndex);
			int required = checked (srcRowBytes * rowCount);
			if (destination.Length < required)
				throw new ArgumentException (
					$"Destination must be at least {required} bytes (rowCount={rowCount} × rowBytes={srcRowBytes}); got {destination.Length}.",
					nameof (destination));

			var src = SkiaApi.sk_graphite_async_read_result_get_data (handle, planeIndex);
			if (src == null) throw new InvalidOperationException ("Plane data is null.");
			new ReadOnlySpan<byte> (src, required).CopyTo (destination);
		}

		public void Dispose () => handle = IntPtr.Zero;

		private void ThrowIfDisposed ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException (nameof (SKGraphiteAsyncReadResult),
					"The async read result is only valid for the duration of the callback.");
		}
	}
}

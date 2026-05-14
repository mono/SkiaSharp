#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// Per-plane pixel data delivered to the callback passed to
	/// <see cref="SKGraphiteContext.RequestReadPixels"/>. Only valid for the duration
	/// of that callback — copy what you need before returning.
	/// </summary>
	public sealed unsafe class SKGraphiteAsyncReadResult : IDisposable
	{
		private IntPtr handle;

		internal SKGraphiteAsyncReadResult (IntPtr handle)
		{
			this.handle = handle;
		}

		/// <summary>Number of pixel planes (1 for RGBA, 2/3 for YUV variants).</summary>
		public int PlaneCount {
			get {
				ThrowIfDisposed ();
				return SkiaApi.sk_graphite_async_read_result_get_count (handle);
			}
		}

		/// <summary>Raw pointer to plane <paramref name="planeIndex"/>'s pixel data.</summary>
		public IntPtr GetPlaneData (int planeIndex)
		{
			ThrowIfDisposed ();
			if (planeIndex < 0 || planeIndex >= PlaneCount)
				throw new ArgumentOutOfRangeException (nameof (planeIndex));
			return (IntPtr)SkiaApi.sk_graphite_async_read_result_get_data (handle, planeIndex);
		}

		/// <summary>Stride in bytes for plane <paramref name="planeIndex"/>.</summary>
		public int GetPlaneRowBytes (int planeIndex)
		{
			ThrowIfDisposed ();
			if (planeIndex < 0 || planeIndex >= PlaneCount)
				throw new ArgumentOutOfRangeException (nameof (planeIndex));
			return (int)SkiaApi.sk_graphite_async_read_result_get_row_bytes (handle, planeIndex);
		}

		/// <summary>
		/// Copy <paramref name="rowCount"/> rows of plane <paramref name="planeIndex"/> verbatim
		/// (including any per-row padding) into <paramref name="destination"/>.
		/// </summary>
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

		/// <summary>Invalidates this wrapper; called automatically when the callback returns.</summary>
		public void Dispose () => handle = IntPtr.Zero;

		private void ThrowIfDisposed ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException (nameof (SKGraphiteAsyncReadResult),
					"The async read result is only valid for the duration of the callback.");
		}
	}
}

#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// The result handed to the callback passed to
	/// <see cref="SKGraphiteContext.RequestReadPixels"/>. Wraps the per-plane
	/// pixel data produced by Graphite's async readback.
	///
	/// LIFETIME: only valid for the duration of the callback. The underlying
	/// native buffer is freed by Skia immediately after the callback returns,
	/// at which point this wrapper is also disposed — don't capture it, copy
	/// what you need before exiting the callback. Accessing any member after
	/// disposal throws <see cref="ObjectDisposedException"/>.
	/// </summary>
	public sealed unsafe class SKGraphiteAsyncReadResult : IDisposable
	{
		private IntPtr handle;

		internal SKGraphiteAsyncReadResult (IntPtr handle)
		{
			this.handle = handle;
		}

		/// <summary>Number of pixel planes in the result (1 for RGBA, 2/3 for YUV variants).</summary>
		public int PlaneCount {
			get {
				ThrowIfDisposed ();
				return SkiaApi.sk_graphite_async_read_result_get_count (handle);
			}
		}

		/// <summary>
		/// Pointer to plane <paramref name="planeIndex"/>'s pixel data. Read-only;
		/// freed by Skia when the callback returns.
		/// </summary>
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
		/// (including any per-row padding the source carries) into <paramref name="destination"/>.
		/// The destination must be at least <c>rowCount × <see cref="GetPlaneRowBytes"/></c>
		/// bytes; otherwise <see cref="ArgumentException"/>.
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

		/// <summary>
		/// Detaches the wrapper from the underlying native result. Called automatically
		/// by <see cref="SKGraphiteContext.RequestReadPixels"/> after the user callback
		/// returns. Subsequent access to plane data throws <see cref="ObjectDisposedException"/>.
		/// </summary>
		public void Dispose () => handle = IntPtr.Zero;

		private void ThrowIfDisposed ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException (nameof (SKGraphiteAsyncReadResult),
					"The async read result is only valid for the duration of the callback.");
		}
	}
}

#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>Represents the pixel data returned by an asynchronous read-pixels request, valid only for the duration of the callback.</summary>
	/// <remarks>
	///       <format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// An instance is passed to the callback supplied to <xref:SkiaSharp.SKImage.RequestReadPixels(SkiaSharp.SKImageInfo,SkiaSharp.SKRectI,System.Action{SkiaSharp.SKImageReadPixelsResult})> or the equivalent method on <xref:SkiaSharp.SKSurface> and <xref:SkiaSharp.SKGraphiteContext>. The underlying pixels are only valid while the callback runs; to keep the data, copy it out with <xref:SkiaSharp.SKImageReadPixelsResult.ToArray(System.Int32)>, <xref:SkiaSharp.SKImageReadPixelsResult.ToBitmap>, or <xref:SkiaSharp.SKImageReadPixelsResult.ToImage>.
	///
	/// Results may be planar (for example, a YUV read has more than one plane); use <xref:SkiaSharp.SKImageReadPixelsResult.PlaneCount> to enumerate the planes.
	/// ]]></format>
	///     </remarks>
	public sealed unsafe class SKImageReadPixelsResult : IDisposable
	{
		private IntPtr handle;
		private readonly SKImageInfo info;

		internal SKImageReadPixelsResult (IntPtr handle, SKImageInfo info)
		{
			this.handle = handle;
			this.info = info;
		}

		/// <summary>Gets the number of pixel planes in the result.</summary>
		/// <value>The number of planes.</value>
		/// <remarks />
		public int PlaneCount {
			get {
				ThrowIfDisposed ();
				return SkiaApi.sk_image_async_read_result_get_count (handle);
			}
		}

		/// <param name="planeIndex">The zero-based index of the plane.</param>
		/// <summary>Gets the number of bytes per row of the specified plane, including any padding.</summary>
		/// <returns>The number of bytes per row of the plane.</returns>
		/// <remarks />
		public int GetPlaneRowBytes (int planeIndex)
		{
			ThrowIfDisposed ();
			if (planeIndex < 0 || planeIndex >= PlaneCount)
				throw new ArgumentOutOfRangeException (nameof (planeIndex));
			return (int)SkiaApi.sk_image_async_read_result_get_row_bytes (handle, planeIndex);
		}

		/// <param name="planeIndex">The zero-based index of the plane.</param>
		/// <summary>Gets a read-only view over the raw pixel data of the specified plane, including any per-row padding.</summary>
		/// <returns>A read-only span over the plane's pixel data, valid only for the duration of the callback.</returns>
		/// <remarks />
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
		/// <param name="planeIndex">The zero-based index of the plane to copy.</param>
		/// <param name="destination">The span to copy the tightly-packed pixels into.</param>
		/// <summary>Copies the specified plane into the destination as tightly-packed pixels, stripping any per-row padding.</summary>
		/// <remarks />
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
			var srcLength = checked (srcRowBytes * height);
			CopyRows (new ReadOnlySpan<byte> (src, srcLength), srcRowBytes, destination, packedRowBytes, height);
		}

		// Copies `height` rows from a strided source into a strided destination, taking the smaller of
		// the two strides per row (so source row padding is dropped when destination is tighter).
		// Internal + span-based so the padding-stripping path is unit-testable without a real GPU read.
		internal static void CopyRows (ReadOnlySpan<byte> source, int srcRowBytes, Span<byte> destination, int dstRowBytes, int height)
		{
			var copy = Math.Min (srcRowBytes, dstRowBytes);
			for (var y = 0; y < height; y++)
				source.Slice (y * srcRowBytes, copy).CopyTo (destination.Slice (y * dstRowBytes));
		}

		// Returns a tightly-packed copy of the plane that outlives the callback.
		/// <param name="planeIndex">The zero-based index of the plane to copy.</param>
		/// <summary>Copies the specified plane into a new tightly-packed byte array that outlives the callback.</summary>
		/// <returns>A new byte array containing the tightly-packed plane data.</returns>
		/// <remarks />
		public byte[] ToArray (int planeIndex = 0)
		{
			ThrowIfDisposed ();
			var pixels = new byte[info.BytesSize];
			CopyPlaneTo (planeIndex, pixels);
			return pixels;
		}

		// Materializes the whole (single-plane) result into an owned SKImage that outlives the callback.
		/// <summary>Copies the single-plane result into a new image that outlives the callback.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKImage" /> containing a copy of the result.</returns>
		/// <remarks />
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
		/// <summary>Copies the single-plane result into a new bitmap that outlives the callback.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKBitmap" /> containing a copy of the result.</returns>
		/// <remarks />
		public SKBitmap ToBitmap ()
		{
			ThrowIfDisposed ();
			if (PlaneCount != 1)
				throw new InvalidOperationException ("ToBitmap is only supported for single-plane (interleaved) results.");

			var bitmap = new SKBitmap (info);
			CopyPlaneTo (0, new Span<byte> ((void*)bitmap.GetPixels (), bitmap.ByteCount));
			return bitmap;
		}

		/// <summary>Invalidates the result so that its pixel data can no longer be accessed.</summary>
		/// <remarks />
		public void Dispose () => handle = IntPtr.Zero;

		private void ThrowIfDisposed ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException (nameof (SKImageReadPixelsResult),
					"The async read result is only valid for the duration of the callback.");
		}
	}
}

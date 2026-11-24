#nullable disable

using System;
using System.IO;

namespace SkiaSharp
{
	/// <summary>Wraps a <see cref="T:System.IO.Stream" /> into a <see cref="T:SkiaSharp.SKStreamAsset" /> (a seekable, rewindable Skia stream)</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// The following example shows how to wrap a <xref:System.IO.Stream> that
	/// represents a stream into an embedded resource in an assembly and use it with
	/// SkiaSharp APIs that use resources:
	///
	/// ## Examples
	///
	/// ```csharp
	/// public static void BitmapShader (SKCanvas canvas, int width, int height)
	/// {
	///     var assembly = typeof(Demos).GetTypeInfo ().Assembly;
	///
	///     // load the image from the embedded resource stream
	///     using (var resource = assembly.GetManifestResourceStream ("embedded.png"))
	///     using (var stream = new SKManagedStream(resource))
	///     using (var source = SKBitmap.Decode (stream)) {
	///         var matrix = SKMatrix.MakeRotation (30.0f);
	///         using (var shader = SKShader.CreateBitmap (source, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, matrix))
	///         using (var paint = new SKPaint ()) {
	///             paint.IsAntialias = true;
	///             paint.Shader = shader;
	///
	///             // tile the bitmap
	///             canvas.Clear (SKColors.White);
	///             canvas.DrawPaint (paint);
	///         }
	///     }
	/// }
	/// ```
	/// ]]></format></remarks>
	public class SKManagedStream : SKAbstractManagedStream
	{
		private Stream stream;

		private bool isAsEnd;
		private bool disposeStream;

		// Lazily-created snapshot for duplicate/fork. Stored in native memory
		// via SKData (ref-counted), so multiple duplicates share one native
		// byte buffer with no additional managed byte-buffer allocations per duplicate.
		private SKData snapshotData;

		/// <summary>Creates a new read-only stream from a <see cref="T:System.IO.Stream" />.</summary>
		/// <param name="managedStream">The managed stream.</param>
		/// <remarks>The underlying stream is not disposed when this object is disposed.</remarks>
		public SKManagedStream (Stream managedStream)
			: this (managedStream, false)
		{
		}

		/// <summary>Creates a new read-only stream from a <see cref="T:System.IO.Stream" />, can optionally dispose the provided stream when this stream is disposed.</summary>
		/// <param name="managedStream">The managed stream.</param>
		/// <param name="disposeManagedStream">If this is set to <see langword="true" />, the provided <see langword="managedStream" /> will be disposed when this instance is disposed.</param>
		/// <remarks />
		public SKManagedStream (Stream managedStream, bool disposeManagedStream)
			: base (true)
		{
			stream = managedStream ?? throw new ArgumentNullException (nameof (managedStream));
			disposeStream = disposeManagedStream;
		}

		/// <summary>Copy the contents of this stream into the destination stream.</summary>
		/// <param name="destination">The destination stream.</param>
		/// <returns>Returns the number of bytes that were copied.</returns>
		/// <remarks />
		public int CopyTo (SKWStream destination)
		{
			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			var total = 0;
			int len;
			using var buffer = Utils.RentArray<byte> (SKData.CopyBufferSize);
			while ((len = stream.Read ((byte[])buffer, 0, buffer.Length)) > 0) {
				destination.Write ((byte[])buffer, len);
				total += len;
			}
			destination.Flush ();
			return total;
		}

		/// <summary>Copies the contents of this stream into a new memory stream.</summary>
		/// <returns>Returns the new memory stream.</returns>
		/// <remarks />
		public SKStreamAsset ToMemoryStream ()
		{
			using var native = new SKDynamicMemoryWStream ();
			CopyTo (native);
			return native.DetachAsStream ();
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKManagedStream" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKManagedStream" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKNativeObject" /> types to destroy any managed objects.</summary>
		/// <remarks />
		protected override void DisposeManaged ()
		{
			if (snapshotData != null) {
				snapshotData.Dispose ();
				snapshotData = null;
			}

			if (disposeStream && stream != null) {
				stream.Dispose ();
				stream = null;
			}

			base.DisposeManaged ();
		}

		private IntPtr OnReadManagedStream (IntPtr buffer, IntPtr size)
		{
			if ((int)size < 0)
				throw new ArgumentOutOfRangeException (nameof (size));

			if (size == IntPtr.Zero)
				return IntPtr.Zero;

			// NOTE: some skips still requires a read as some streams cannot seek
			using var managedBuffer = Utils.RentArray<byte> ((int)size);
			var len = stream.Read (managedBuffer.Array, 0, managedBuffer.Length);

			if (buffer != IntPtr.Zero) {
				// read
				var src = managedBuffer.Span.Slice (0, len);
				var dst = buffer.AsSpan (managedBuffer.Length);
				src.CopyTo (dst);
			}

			if (!stream.CanSeek && (int)size > 0 && len <= (int)size)
				isAsEnd = true;

			return (IntPtr)len;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to copy the specified number of bytes into the specified buffer.</summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>Returns the number of bytes actually read.</returns>
		/// <remarks />
		protected internal override IntPtr OnRead (IntPtr buffer, IntPtr size)
		{
			return OnReadManagedStream (buffer, size);
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to copy the specified number of bytes into the specified buffer.</summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>Returns the number of bytes actually peeked/copied.</returns>
		/// <remarks>The stream's cursor must be returned to the position before this method was called.</remarks>
		protected internal override IntPtr OnPeek (IntPtr buffer, IntPtr size)
		{
			if (!stream.CanSeek) {
				return (IntPtr)0;
			}
			var oldPos = stream.Position;
			var result = OnReadManagedStream (buffer, size);
			stream.Position = oldPos;
			return result;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to indicate whether all the bytes in the stream have been read.</summary>
		/// <returns>Returns a value indicating whether all the bytes in the stream have been read.</returns>
		/// <remarks />
		protected internal override bool OnIsAtEnd ()
		{
			if (!stream.CanSeek) {
				return isAsEnd;
			}
			return stream.Position >= stream.Length;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to indicate whether this stream can report its current position.</summary>
		/// <returns>Returns a value indicating whether this stream can report its current position.</returns>
		/// <remarks />
		protected internal override bool OnHasPosition ()
		{
			return stream.CanSeek;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to indicate whether this stream can report its total length.</summary>
		/// <returns>Returns a value indicating whether this stream can report its total length.</returns>
		/// <remarks />
		protected internal override bool OnHasLength ()
		{
			return stream.CanSeek;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to rewind the current stream.</summary>
		/// <returns>Returns <see langword="true" /> if the stream is known to be at the beginning after this call returns.</returns>
		/// <remarks />
		protected internal override bool OnRewind ()
		{
			if (!stream.CanSeek) {
				return false;
			}
			stream.Position = 0;
			return true;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to get the current position in the stream.</summary>
		/// <returns>Returns the current position in the stream.</returns>
		/// <remarks />
		protected internal override IntPtr OnGetPosition ()
		{
			if (!stream.CanSeek) {
				return (IntPtr)0;
			}
			return (IntPtr)stream.Position;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to return the total length of the stream.</summary>
		/// <returns>Returns the total length of the stream.</returns>
		/// <remarks />
		protected internal override IntPtr OnGetLength ()
		{
			if (!stream.CanSeek) {
				return (IntPtr)0;
			}
			return (IntPtr)stream.Length;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to seek to an absolute position.</summary>
		/// <param name="position">The absolute position.</param>
		/// <returns>Returns <see langword="true" /> if seeking is supported and the seek was successful, otherwise <see langword="false" />.</returns>
		/// <remarks>If an attempt is made to move to a position outside the stream, the position must be set to the closest point within the stream (beginning or end).</remarks>
		protected internal override bool OnSeek (IntPtr position)
		{
			if (!stream.CanSeek) {
				return false;
			}
			stream.Position = (long)position;
			return true;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to seek to a relative offset.</summary>
		/// <param name="offset">The relative offset.</param>
		/// <returns>Returns <see langword="true" /> if seeking is supported and the seek was successful, otherwise <see langword="false" />.</returns>
		/// <remarks>If an attempt is made to move to a position outside the stream, the position must be set to the closest point within the stream (beginning or end).</remarks>
		protected internal override bool OnMove (int offset)
		{
			if (!stream.CanSeek) {
				return false;
			}
			stream.Position += offset;
			return true;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to copy the current stream.</summary>
		/// <returns>Returns a pointer to the new <see cref="T:SkiaSharp.SKStreamAsset" /> instance.</returns>
		/// <remarks />
		protected internal override IntPtr OnCreateNew ()
		{
			return IntPtr.Zero;
		}

		private SKData GetOrCreateSnapshot ()
		{
			if (snapshotData != null)
				return snapshotData;

			if (!stream.CanSeek)
				return null;

			var pos = stream.Position;
			try {
				stream.Position = 0;
				snapshotData = SKData.Create (stream, stream.Length);
			} finally {
				stream.Position = pos;
			}

			return snapshotData;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to duplicate the current stream.</summary>
		/// <returns>Returns a pointer to the new <see cref="T:SkiaSharp.SKStreamAsset" /> instance.</returns>
		/// <remarks>After the stream has been duplicated, the new stream must set its position to the start.</remarks>
		protected internal override IntPtr OnDuplicate ()
		{
			var data = GetOrCreateSnapshot ();
			if (data == null)
				return IntPtr.Zero;

			return SkiaApi.sk_memorystream_new_with_skdata (data.Handle);
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to fork the current stream.</summary>
		/// <returns>Returns a pointer to the new <see cref="T:SkiaSharp.SKStreamAsset" /> instance.</returns>
		/// <remarks>After the stream has been duplicated, the new stream must set its position to the same as this stream.</remarks>
		protected internal override IntPtr OnFork ()
		{
			var duplicate = OnDuplicate ();
			if (duplicate != IntPtr.Zero)
				SkiaApi.sk_stream_seek (duplicate, (IntPtr)stream.Position);
			return duplicate;
		}
	}
}

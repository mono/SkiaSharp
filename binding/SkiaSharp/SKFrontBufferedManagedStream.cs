#nullable disable

using System;
using System.IO;

namespace SkiaSharp
{
	/// <summary>A read-only stream that buffers the specified first chunk of bytes.</summary>
	/// <remarks>This is useful for decoding images using streams that are not seekable, since <see cref="T:SkiaSharp.SKCodec" /> needs to read the first few bytes to determine the codec to use.</remarks>
	public class SKFrontBufferedManagedStream : SKAbstractManagedStream
	{
		private SKStream stream;
		private bool disposeStream;

		private readonly bool hasLength;
		private readonly long streamLength;
		private readonly int bufferLength;
		private byte[] frontBuffer;
		private int bufferedSoFar;
		private long offset;
		private bool isUnderlyingAtEnd;

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKFrontBufferedStream" /> that wraps the specified stream.</summary>
		/// <param name="managedStream">The stream to buffer.</param>
		/// <param name="bufferSize">The number of bytes to buffer.</param>
		/// <remarks />
		public SKFrontBufferedManagedStream (Stream managedStream, int bufferSize)
			: this (managedStream, bufferSize, false)
		{
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKFrontBufferedStream" /> that wraps the specified stream.</summary>
		/// <param name="managedStream">The stream to buffer.</param>
		/// <param name="bufferSize">The number of bytes to buffer.</param>
		/// <param name="disposeUnderlyingStream"><see langword="true" /> to dispose the underlying stream when this stream is disposed; otherwise, <see langword="false" />.</param>
		/// <remarks />
		/// <exception cref="ArgumentNullException"><paramref name="managedStream" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize" /> is negative.</exception>
		public SKFrontBufferedManagedStream (Stream managedStream, int bufferSize, bool disposeUnderlyingStream)
			: this (CreateManagedStream (managedStream, bufferSize, disposeUnderlyingStream), bufferSize, true)
		{
		}

		private static SKManagedStream CreateManagedStream (Stream managedStream, int bufferSize, bool disposeUnderlyingStream)
		{
			if (managedStream == null)
				throw new ArgumentNullException (nameof (managedStream));
			if (bufferSize < 0)
				throw new ArgumentOutOfRangeException (nameof (bufferSize));

			return new SKManagedStream (managedStream, disposeUnderlyingStream);
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKFrontBufferedStream" /> that wraps the specified stream.</summary>
		/// <param name="nativeStream">The stream to buffer.</param>
		/// <param name="bufferSize">The number of bytes to buffer.</param>
		/// <remarks />
		public SKFrontBufferedManagedStream (SKStream nativeStream, int bufferSize)
			: this (nativeStream, bufferSize, false)
		{
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKFrontBufferedStream" /> that wraps the specified stream.</summary>
		/// <param name="nativeStream">The stream to buffer.</param>
		/// <param name="bufferSize">The number of bytes to buffer.</param>
		/// <param name="disposeUnderlyingStream"><see langword="true" /> to dispose the underlying stream when this stream is disposed; otherwise, <see langword="false" />.</param>
		/// <remarks />
		/// <exception cref="ArgumentNullException"><paramref name="nativeStream" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize" /> is negative.</exception>
		public SKFrontBufferedManagedStream (SKStream nativeStream, int bufferSize, bool disposeUnderlyingStream)
			: this (ValidateStream (nativeStream, bufferSize), new byte[bufferSize], disposeUnderlyingStream)
		{
		}

		private static SKStream ValidateStream (SKStream nativeStream, int bufferSize)
		{
			if (nativeStream == null)
				throw new ArgumentNullException (nameof (nativeStream));
			if (bufferSize < 0)
				throw new ArgumentOutOfRangeException (nameof (bufferSize));

			return nativeStream;
		}

		private SKFrontBufferedManagedStream (SKStream nativeStream, byte[] frontBuffer, bool disposeUnderlyingStream)
		{
			var hasPos = nativeStream.HasPosition;
			var hasLen = nativeStream.HasLength;
			var length = hasLen ? GetNativeSize (SkiaApi.sk_stream_get_length (nativeStream.Handle)) : 0;
			var position = hasPos ? GetNativeSize (SkiaApi.sk_stream_get_position (nativeStream.Handle)) : 0;
			GC.KeepAlive (nativeStream);

			disposeStream = disposeUnderlyingStream;
			stream = nativeStream;
			hasLength = hasPos && hasLen;
			streamLength = length >= position ? length - position : 0;
			offset = 0;
			bufferedSoFar = 0;
			bufferLength = frontBuffer.Length;
			this.frontBuffer = frontBuffer;
		}

		private static long GetNativeSize (IntPtr value) =>
			IntPtr.Size == 4 ? (uint)(int)value : (long)value;

		private static IntPtr ToNativeSize (long value) =>
			IntPtr.Size == 4 ? (IntPtr)unchecked((int)value) : (IntPtr)value;

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKFrontBufferedManagedStream" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKFrontBufferedManagedStream" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKNativeObject" /> types to destroy any managed objects.</summary>
		/// <remarks />
		protected override void DisposeManaged ()
		{
			if (disposeStream && stream != null) {
				stream.Dispose ();
			}
			stream = null;

			base.DisposeManaged ();
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to copy the specified number of bytes into the specified buffer.</summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>Returns the number of bytes actually read.</returns>
		/// <remarks />
		protected internal override IntPtr OnRead (IntPtr buffer, IntPtr size)
		{
			var remaining = GetNativeSize (size);
			if (remaining <= 0)
				return IntPtr.Zero;

			var start = offset;

			// Read managed children directly so this adapter does not add a nested native callback boundary.
			// 1. read any data that was previously buffered
			if (remaining > 0 && offset < bufferedSoFar)
			{
				var currentOffset = (int)offset;
				var copied = (int)Math.Min (remaining, bufferedSoFar - currentOffset);

				if (buffer != IntPtr.Zero)
				{
					frontBuffer.AsSpan (currentOffset, copied).CopyTo (buffer.AsSpan (copied));
					buffer += copied;
				}

				offset += copied;
				remaining -= copied;
			}

			// 2. buffer any more data, and then read it
			while (remaining > 0 && bufferedSoFar < bufferLength && !isUnderlyingAtEnd)
			{
				var currentOffset = (int)offset;
				var bytesToBuffer = (int)Math.Min (remaining, bufferLength - bufferedSoFar);

				int bytesRead;
				fixed (byte* p = &frontBuffer[currentOffset])
				{
					var read = Utils.FromNativeSize (stream is SKAbstractManagedStream managed
						? managed.OnRead ((IntPtr)p, (IntPtr)bytesToBuffer)
						: (IntPtr)stream.Read ((IntPtr)p, bytesToBuffer));
					GC.KeepAlive (stream);
					if (read > (ulong)bytesToBuffer)
						throw new IOException ("The stream returned an invalid byte count.");
					bytesRead = (int)read;
				}

				if (bytesRead <= 0)
				{
					isUnderlyingAtEnd = true;
					break;
				}

				bufferedSoFar += bytesRead;
				if (buffer != IntPtr.Zero)
				{
					frontBuffer.AsSpan (currentOffset, bytesRead).CopyTo (buffer.AsSpan (bytesRead));
					buffer += bytesRead;
				}

				offset += bytesRead;
				remaining -= bytesRead;
			}

			// 3. read the rest directly once the front buffer is full
			while (remaining > 0 && bufferedSoFar >= bufferLength && !isUnderlyingAtEnd)
			{
				var bytesToRead = (int)Math.Min (remaining, int.MaxValue);
				var read = Utils.FromNativeSize (stream is SKAbstractManagedStream managed
					? managed.OnRead (buffer, (IntPtr)bytesToRead)
					: (IntPtr)stream.Read (buffer, bytesToRead));
				GC.KeepAlive (stream);
				if (read > (ulong)bytesToRead)
					throw new IOException ("The stream returned an invalid byte count.");
				var bytesRead = (int)read;

				if (bytesRead <= 0)
				{
					isUnderlyingAtEnd = true;
					break;
				}

				// past the buffer, so dispose it
				frontBuffer = null;

				if (buffer != IntPtr.Zero)
					buffer += bytesRead;

				offset += bytesRead;
				remaining -= bytesRead;
			}

			return ToNativeSize (offset - start);
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to copy the specified number of bytes into the specified buffer.</summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>Returns the number of bytes actually peeked/copied.</returns>
		/// <remarks>The stream's cursor must be returned to the position before this method was called.</remarks>
		protected internal override IntPtr OnPeek (IntPtr buffer, IntPtr size)
		{
			var count = GetNativeSize (size);
			if (count <= 0 || frontBuffer == null || offset >= bufferLength)
			{
				// this stream is not able to buffer.
				return (IntPtr)0;
			}

			// keep track of the offset so we can return to it.
			var start = offset;
			try
			{
				var bytesToCopy = (int)Math.Min (count, bufferLength - offset);
				return OnRead (buffer, (IntPtr)bytesToCopy);
			}
			finally
			{
				// return to the original position
				offset = start;
			}
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to indicate whether all the bytes in the stream have been read.</summary>
		/// <returns>Returns a value indicating whether all the bytes in the stream have been read.</returns>
		/// <remarks />
		protected internal override bool OnIsAtEnd ()
		{
			if (offset < bufferedSoFar)
			{
				// even if the underlying stream is at the end, this stream has been
				// rewound after buffering, so it is not at the end.
				return false;
			}

			if (isUnderlyingAtEnd)
				return true;

			// Until #5464 lands, SKManagedStream reports EOF after any non-seekable read.
			// For that adapter, only a zero-byte read can establish EOF reliably.
			if (!hasLength && stream is SKManagedStream)
				return false;

			return stream.IsAtEnd;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to rewind the current stream.</summary>
		/// <returns>Returns <see langword="true" /> if the stream is known to be at the beginning after this call returns.</returns>
		/// <remarks />
		protected internal override bool OnRewind ()
		{
			// only allow a rewind if we have not exceeded the buffer.
			if (frontBuffer != null && offset <= bufferLength)
			{
				offset = 0;
				return true;
			}

			return false;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to indicate whether this stream can report its total length.</summary>
		/// <returns>Returns a value indicating whether this stream can report its total length.</returns>
		/// <remarks />
		protected internal override bool OnHasLength () => hasLength;

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to return the total length of the stream.</summary>
		/// <returns>Returns the total length of the stream.</returns>
		/// <remarks />
		protected internal override IntPtr OnGetLength () => ToNativeSize (streamLength);

		// seeking is not supported
		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to indicate whether this stream can report its current position.</summary>
		/// <returns>Returns a value indicating whether this stream can report its current position.</returns>
		/// <remarks />
		protected internal override bool OnHasPosition () => false;

		// seeking is not supported
		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to get the current position in the stream.</summary>
		/// <returns>Returns the current position in the stream.</returns>
		/// <remarks />
		protected internal override IntPtr OnGetPosition () => (IntPtr)0;

		// seeking is not supported
		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to seek to an absolute position.</summary>
		/// <param name="position">The absolute position.</param>
		/// <returns>Returns <see langword="true" /> if seeking is supported and the seek was successful, otherwise <see langword="false" />.</returns>
		/// <remarks>If an attempt is made to move to a position outside the stream, the position must be set to the closest point within the stream (beginning or end).</remarks>
		protected internal override bool OnSeek (IntPtr position) => false;

		// seeking is not supported
		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to seek to a relative offset.</summary>
		/// <param name="offset">The relative offset.</param>
		/// <returns>Returns <see langword="true" /> if seeking is supported and the seek was successful, otherwise <see langword="false" />.</returns>
		/// <remarks>If an attempt is made to move to a position outside the stream, the position must be set to the closest point within the stream (beginning or end).</remarks>
		protected internal override bool OnMove (int offset) => false;

		// duplicating or forking is not supported
		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to copy the current stream.</summary>
		/// <returns>Returns a pointer to the new <see cref="T:SkiaSharp.SKStreamAsset" /> instance.</returns>
		/// <remarks />
		protected internal override IntPtr OnCreateNew () => IntPtr.Zero;
	}
}

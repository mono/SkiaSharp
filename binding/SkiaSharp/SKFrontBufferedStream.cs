#nullable disable

using System;
using System.IO;

namespace SkiaSharp
{
	/// <summary>A read-only stream that buffers the specified first chunk of bytes.</summary>
	/// <remarks>This is useful for decoding images using streams that are not seekable, since <see cref="T:SkiaSharp.SKCodec" /> needs to read the first few bytes to determine the codec to use.</remarks>
	public class SKFrontBufferedStream : Stream
	{
		/// <summary>The default number of bytes to buffer (4096 bytes).</summary>
		/// <remarks />
		public const int DefaultBufferSize = 4096;

		private readonly long totalBufferSize;
		private readonly long totalLength;
		private readonly bool disposeStream;
		private Stream underlyingStream;
		
		private long currentOffset;
		private long bufferedSoFar;
		private byte[] internalBuffer;

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKFrontBufferedStream" /> that wraps the specified stream.</summary>
		/// <param name="stream">The stream to buffer.</param>
		/// <remarks />
		public SKFrontBufferedStream(Stream stream)
			: this(stream, DefaultBufferSize, false)
		{
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKFrontBufferedStream" /> that wraps the specified stream.</summary>
		/// <param name="stream">The stream to buffer.</param>
		/// <param name="bufferSize">The number of bytes to buffer.</param>
		/// <remarks />
		public SKFrontBufferedStream(Stream stream, long bufferSize)
			: this(stream, bufferSize, false)
		{
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKFrontBufferedStream" /> that wraps the specified stream.</summary>
		/// <param name="stream">The stream to buffer.</param>
		/// <param name="disposeUnderlyingStream"><see langword="true" /> to dispose the underlying stream when this stream is disposed; otherwise, <see langword="false" />.</param>
		/// <remarks />
		public SKFrontBufferedStream(Stream stream, bool disposeUnderlyingStream)
			: this(stream, DefaultBufferSize, disposeUnderlyingStream)
		{
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKFrontBufferedStream" /> that wraps the specified stream.</summary>
		/// <param name="stream">The stream to buffer.</param>
		/// <param name="bufferSize">The number of bytes to buffer, from zero to <see cref="int.MaxValue" />.</param>
		/// <param name="disposeUnderlyingStream"><see langword="true" /> to dispose the underlying stream when this stream is disposed; otherwise, <see langword="false" />.</param>
		/// <remarks />
		/// <exception cref="ArgumentNullException"><paramref name="stream" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize" /> is outside the supported range.</exception>
		public SKFrontBufferedStream(Stream stream, long bufferSize, bool disposeUnderlyingStream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (bufferSize < 0 || bufferSize > int.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(bufferSize));

			underlyingStream = stream;
			totalBufferSize = bufferSize;
			totalLength = stream.CanSeek && stream.Length >= 0
				? Math.Max(0, stream.Length - stream.Position)
				: -1;
			disposeStream = disposeUnderlyingStream;
		}

		/// <summary>Gets a value indicating whether the current stream supports reading.</summary>
		/// <value><see langword="true" /> if the stream supports reading; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public override bool CanRead => true;

		/// <summary>Gets a value indicating whether the current stream supports seeking.</summary>
		/// <value><see langword="true" /> if the stream supports seeking; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public override bool CanSeek => true; // we can seek if we are in the buffer

		/// <summary>Gets a value indicating whether the current stream supports writing.</summary>
		/// <value><see langword="true" /> if the stream supports writing; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public override bool CanWrite => false; // we don't write

		/// <summary>Gets the stream length in bytes.</summary>
		/// <value>The length of the stream in bytes.</value>
		/// <remarks />
		public override long Length => totalLength;

		/// <summary>Gets or sets the position within the current stream.</summary>
		/// <value>The current position within the stream.</value>
		/// <remarks />
		public override long Position
		{
			get { return currentOffset; }
			set { Seek(value, SeekOrigin.Begin); }
		}

		/// <summary>Clears all buffers for this stream and causes any buffered data to be written to the underlying device.</summary>
		/// <remarks />
		public override void Flush()
		{
			// we don't write
		}

		/// <summary>Copies bytes from the current buffered stream to an array.</summary>
		/// <param name="buffer">The buffer to which bytes are to be copied.</param>
		/// <param name="offset">The byte offset in the buffer at which to begin reading bytes.</param>
		/// <param name="count">The number of bytes to be read.</param>
		/// <returns>Returns the total number of bytes read into the buffer array.</returns>
		/// <remarks>Reads until <paramref name="count" /> bytes have been copied or the underlying stream returns zero bytes.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="buffer" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The buffer offset or byte count is outside the buffer.</exception>
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));
			if (count > buffer.Length - offset)
				throw new ArgumentOutOfRangeException(nameof(count));

			return ReadInternal(buffer, offset, count);
		}

		private int ReadInternal(byte[] buffer, int offset, int count)
		{
			if (count == 0)
				return 0;

			var start = currentOffset;

			if (internalBuffer == null && currentOffset < totalBufferSize)
			{
				// create the buffer now, since we are going to be writing to it
				internalBuffer = new byte[totalBufferSize];
			}

			// try read any data from the buffer
			if (currentOffset < bufferedSoFar)
			{
				var bytesCopied = ReadFromBuffer(buffer, offset, count);
				count -= bytesCopied;
				offset += bytesCopied;
			}

			// read from the stream and buffer that
			if (count > 0 && bufferedSoFar < totalBufferSize)
			{
				var buffered = BufferAndWriteTo(buffer, offset, count);
				count -= buffered;
				offset += buffered;
			}

			// just read from the stream once the front buffer is full
			if (count > 0 && bufferedSoFar >= totalBufferSize)
			{
				var direct = ReadDirectlyFromStream(buffer, offset, count);
				count -= direct;
				offset += direct;
			}

			return (int)(currentOffset - start);
		}

		/// <summary>Sets the position within the current buffered stream.</summary>
		/// <param name="offset">The byte offset relative to the specified origin.</param>
		/// <param name="origin">The reference point from which to obtain the new position.</param>
		/// <returns>Returns the new position within the current buffered stream.</returns>
		/// <remarks>Forward seeking advances through the underlying stream and stops at its end. Backward seeking is supported only until bytes beyond the front buffer have been consumed.</remarks>
		public override long Seek(long offset, SeekOrigin origin)
		{
			// find the absolute position
			var absolute = offset;
			if (origin == SeekOrigin.Current)
			{
				absolute = Position + offset;
			}
			else if (origin == SeekOrigin.End)
			{
				if (Length == -1)
				{
					throw new InvalidOperationException("Can't seek from end as the underlying stream is not seekable.");
				}
				absolute = Length + offset;
			}

			if (absolute < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), "Cannot seek before the beginning of the stream.");
			}

			if (currentOffset > totalBufferSize && absolute < currentOffset)
			{
				throw new InvalidOperationException("The position cannot be moved backward once the stream has moved past the buffer.");
			}

			// move to the position
			if (absolute <= currentOffset)
			{
				// we are moving back, so just move the local cursor
				currentOffset = absolute;
			}
			else
			{
				// we are moving forward, so we have to read into (or past) the buffer
				var toMove = absolute - currentOffset;
				while (toMove > 0)
				{
					var step = (int)Math.Min(toMove, SKData.CopyBufferSize);
					var moved = ReadInternal(null, 0, step);
					if (moved <= 0)
						break;
					toMove -= moved;
				}
			}

			return Position;
		}

		/// <summary>Sets the length of the buffered stream.</summary>
		/// <param name="value">An integer indicating the desired length of the current buffered stream in bytes.</param>
		/// <remarks />
		public override void SetLength(long value)
		{
			// we don't write
		}

		/// <summary>Copies bytes to the buffered stream and advances the current position within the buffered stream by the number of bytes written.</summary>
		/// <param name="buffer">The byte array from which to copy count bytes to the current buffered stream.</param>
		/// <param name="offset">The offset in the buffer at which to begin copying bytes to the current buffered stream.</param>
		/// <param name="count">The number of bytes to be written to the current buffered stream.</param>
		/// <remarks />
		public override void Write(byte[] buffer, int offset, int count)
		{
			// we don't write
		}

		private int ReadFromBuffer(byte[] dst, int offset, int size)
		{
			var bytesToCopy = Math.Min(size, (int)(bufferedSoFar - currentOffset));

			if (dst != null)
			{
				Buffer.BlockCopy(internalBuffer, (int)currentOffset, dst, offset, bytesToCopy);
			}

			currentOffset += bytesToCopy;

			return bytesToCopy;
		}

		private int BufferAndWriteTo(byte[] dst, int offset, int size)
		{
			var bytesToBuffer = Math.Min(size, (int)(totalBufferSize - bufferedSoFar));
			var totalBuffered = 0;

			while (totalBuffered < bytesToBuffer)
			{
				var read = underlyingStream.Read(internalBuffer, (int)bufferedSoFar, bytesToBuffer - totalBuffered);
				if (read <= 0)
					break;

				if (dst != null)
				{
					Buffer.BlockCopy(internalBuffer, (int)bufferedSoFar, dst, offset + totalBuffered, read);
				}

				bufferedSoFar += read;
				currentOffset = bufferedSoFar;
				totalBuffered += read;
			}

			return totalBuffered;
		}

		private int ReadDirectlyFromStream(byte[] dst, int offset, int size)
		{
			if (dst == null && underlyingStream.CanSeek && underlyingStream.Length >= 0)
			{
				var available = Math.Max(0, underlyingStream.Length - underlyingStream.Position);
				var skipped = (int)Math.Min(size, available);
				underlyingStream.Position += skipped;
				currentOffset += skipped;
				if (skipped > 0)
					internalBuffer = null;
				return skipped;
			}

			using var scratch = dst == null
				? Utils.RentArray<byte>(Math.Min(size, SKData.CopyBufferSize))
				: default;
			var buffer = dst ?? scratch.Array;
			var totalRead = 0;
			while (totalRead < size)
			{
				var toRead = dst == null ? Math.Min(size - totalRead, scratch.Length) : size - totalRead;
				var read = underlyingStream.Read(buffer, dst == null ? 0 : offset + totalRead, toRead);
				if (read <= 0)
					break;

				internalBuffer = null;
				currentOffset += read;
				totalRead += read;
			}

			return totalRead;
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKFrontBufferedStream" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKFrontBufferedStream" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			internalBuffer = null;
			if (disposeStream && underlyingStream != null)
			{
				underlyingStream.Dispose();
			}
			underlyingStream = null;
		}
	}
}

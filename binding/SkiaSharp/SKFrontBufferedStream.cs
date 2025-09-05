#nullable disable

using System;
using System.IO;

namespace SkiaSharp
{
	/// <summary>
	/// A read-only stream that buffers the specified first chunk of bytes.
	/// </summary>
	/// <remarks>This is useful for decoding images using streams that are not seekable, since <see cref="T:SkiaSharp.SKCodec" /> needs to read the first few bytes to determine the codec to use.</remarks>
	public class SKFrontBufferedStream : Stream
	{
		/// <summary>
		/// The default number of bytes to buffer (4096 bytes).
		/// </summary>
		public const int DefaultBufferSize = 4096;

		private readonly long totalBufferSize;
		private readonly long totalLength;
		private readonly bool disposeStream;
		private Stream underlyingStream;
		
		private long currentOffset;
		private long bufferedSoFar;
		private byte[] internalBuffer;

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKFrontBufferedStream" /> that wraps the specified stream.
		/// </summary>
		/// <param name="stream">The stream to buffer.</param>
		public SKFrontBufferedStream(Stream stream)
			: this(stream, DefaultBufferSize, false)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKFrontBufferedStream" /> that wraps the specified stream.
		/// </summary>
		/// <param name="stream">The stream to buffer.</param>
		/// <param name="bufferSize">The number of bytes to buffer.</param>
		public SKFrontBufferedStream(Stream stream, long bufferSize)
			: this(stream, bufferSize, false)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKFrontBufferedStream" /> that wraps the specified stream.
		/// </summary>
		/// <param name="stream">The stream to buffer.</param>
		/// <param name="disposeUnderlyingStream">Whether or not to dispose the underlying stream when this stream is disposed.</param>
		public SKFrontBufferedStream(Stream stream, bool disposeUnderlyingStream)
			: this(stream, DefaultBufferSize, disposeUnderlyingStream)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKFrontBufferedStream" /> that wraps the specified stream.
		/// </summary>
		/// <param name="stream">The stream to buffer.</param>
		/// <param name="bufferSize">The number of bytes to buffer.</param>
		/// <param name="disposeUnderlyingStream">Whether or not to dispose the underlying stream when this stream is disposed.</param>
		public SKFrontBufferedStream(Stream stream, long bufferSize, bool disposeUnderlyingStream)
		{
			underlyingStream = stream;
			totalBufferSize = bufferSize;
			totalLength = stream.CanSeek ? stream.Length : -1;
			disposeStream = disposeUnderlyingStream;
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports reading.
		/// </summary>
		public override bool CanRead => true;

		/// <summary>
		/// Gets a value indicating whether the current stream supports seeking.
		/// </summary>
		public override bool CanSeek => true; // we can seek if we are in the buffer

		/// <summary>
		/// Gets a value indicating whether the current stream supports writing.
		/// </summary>
		public override bool CanWrite => false; // we don't write

		/// <summary>
		/// Gets the stream length in bytes.
		/// </summary>
		public override long Length => totalLength;

		/// <summary>
		/// Gets the position within the current stream.
		/// </summary>
		public override long Position
		{
			get { return currentOffset; }
			set { Seek(value, SeekOrigin.Begin); }
		}

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		/// </summary>
		public override void Flush()
		{
			// we don't write
		}

		/// <summary>
		/// Copies bytes from the current buffered stream to an array.
		/// </summary>
		/// <param name="buffer">The buffer to which bytes are to be copied.</param>
		/// <param name="offset">The byte offset in the buffer at which to begin reading bytes.</param>
		/// <param name="count">The number of bytes to be read.</param>
		/// <returns>Returns the total number of bytes read into the buffer array.</returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
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

			// just read from the stream
			if (count > 0)
			{
				var direct = ReadDirectlyFromStream(buffer, offset, count);
				count -= direct;
				offset += direct;

				if (direct > 0)
				{
					// if we are here, we are past the buffer and can't go back
					internalBuffer = null;
				}
			}

			return (int)(currentOffset - start);
		}

		/// <summary>
		/// Sets the position within the current buffered stream.
		/// </summary>
		/// <param name="offset">The byte offset relative to the specified origin.</param>
		/// <param name="origin">The reference point from which to obtain the new position.</param>
		/// <returns>Returns the new position within the current buffered stream.</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			// we are outside the buffer, so throw
			if (currentOffset > totalBufferSize)
			{
				throw new InvalidOperationException("The position cannot be changed once the stream has moved past the buffer.");
			}

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

			// move to the position
			if (absolute <= currentOffset)
			{
				// we are moving back, so just move the local cursor
				currentOffset = absolute;
			}
			else
			{
				// we are moving forward, so we have to read into the buffer
				var toMove = absolute - currentOffset;
				currentOffset += Read(null, 0, (int)toMove);
			}

			return Position;
		}

		/// <summary>
		/// Sets the length of the buffered stream.
		/// </summary>
		/// <param name="value">An integer indicating the desired length of the current buffered stream in bytes.</param>
		public override void SetLength(long value)
		{
			// we don't write
		}

		/// <summary>
		/// Copies bytes to the buffered stream and advances the current position within the buffered stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">The byte array from which to copy count bytes to the current buffered stream.</param>
		/// <param name="offset">The offset in the buffer at which to begin copying bytes to the current buffered stream.</param>
		/// <param name="count">The number of bytes to be written to the current buffered stream.</param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			// we don't write
		}

		private int ReadFromBuffer(byte[] dst, int offset, int size)
		{
			var bytesToCopy = Math.Min(size, (int)(bufferedSoFar - currentOffset));

			if (dst != null && offset < dst.Length)
			{
				Buffer.BlockCopy(internalBuffer, (int)currentOffset, dst, offset, bytesToCopy);
			}

			currentOffset += bytesToCopy;

			return bytesToCopy;
		}

		private int BufferAndWriteTo(byte[] dst, int offset, int size)
		{
			var bytesToBuffer = Math.Min(size, (int)(totalBufferSize - bufferedSoFar));
			var buffered = underlyingStream.Read(internalBuffer, (int)currentOffset, bytesToBuffer);

			if (dst != null && offset < dst.Length)
			{
				Buffer.BlockCopy(internalBuffer, (int)currentOffset, dst, offset, buffered);
			}

			bufferedSoFar += buffered;
			currentOffset = bufferedSoFar;

			return buffered;
		}

		private int ReadDirectlyFromStream(byte[] dst, int offset, int size)
		{
			long bytesReadDirectly = 0;

			if (dst == null)
			{
				bytesReadDirectly = underlyingStream.Seek(size, SeekOrigin.Current);
			}
			else
			{
				bytesReadDirectly = underlyingStream.Read(dst, offset, size);
			}

			currentOffset += bytesReadDirectly;

			return (int)bytesReadDirectly;
		}

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

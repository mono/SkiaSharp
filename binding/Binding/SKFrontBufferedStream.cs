using System;
using System.IO;

namespace SkiaSharp
{
	public class SKFrontBufferedStream : Stream
	{
		public const int DefaultBufferSize = 4096;

		private readonly long totalBufferSize;
		private readonly long totalLength;
		private readonly bool disposeStream;
		private Stream underlyingStream;
		
		private long currentOffset;
		private long bufferedSoFar;
		private byte[] internalBuffer;

		public SKFrontBufferedStream(Stream stream)
			: this(stream, DefaultBufferSize, false)
		{
		}

		public SKFrontBufferedStream(Stream stream, long bufferSize)
			: this(stream, bufferSize, false)
		{
		}

		public SKFrontBufferedStream(Stream stream, bool disposeUnderlyingStream)
			: this(stream, DefaultBufferSize, disposeUnderlyingStream)
		{
		}

		public SKFrontBufferedStream(Stream stream, long bufferSize, bool disposeUnderlyingStream)
		{
			underlyingStream = stream;
			totalBufferSize = bufferSize;
			totalLength = stream.CanSeek ? stream.Length : -1;
			disposeStream = disposeUnderlyingStream;
		}

		public override bool CanRead => true;

		public override bool CanSeek => true; // we can seek if we are in the buffer

		public override bool CanWrite => false; // we don't write

		public override long Length => totalLength;

		public override long Position
		{
			get { return currentOffset; }
			set { Seek(value, SeekOrigin.Begin); }
		}

		public override void Flush()
		{
			// we don't write
		}

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

		public override void SetLength(long value)
		{
			// we don't write
		}

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

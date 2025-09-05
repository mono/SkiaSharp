#nullable disable

using System;
using System.Runtime.InteropServices;
using System.IO;

namespace SkiaSharp
{
	/// <summary>
	/// A read-only stream that buffers the specified first chunk of bytes.
	/// </summary>
	/// <remarks>This is useful for decoding images using streams that are not seekable, since <see cref="SKCodec" /> needs to read the first few bytes to determine the codec to use.</remarks>
	public class SKFrontBufferedManagedStream : SKAbstractManagedStream
	{
		private SKStream stream;
		private bool disposeStream;

		private readonly bool hasLength;
		private readonly int streamLength;
		private readonly int bufferLength;
		private byte[] frontBuffer;
		private int bufferedSoFar;
		private int offset;

		/// <summary>
		/// Creates a new instance of <see cref="SKFrontBufferedStream" /> that wraps the specified stream.
		/// </summary>
		/// <param name="managedStream">The stream to buffer.</param>
		/// <param name="bufferSize">The number of bytes to buffer.</param>
		public SKFrontBufferedManagedStream (Stream managedStream, int bufferSize)
			: this (managedStream, bufferSize, false)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="SKFrontBufferedStream" /> that wraps the specified stream.
		/// </summary>
		/// <param name="managedStream">The stream to buffer.</param>
		/// <param name="bufferSize">The number of bytes to buffer.</param>
		/// <param name="disposeUnderlyingStream">Whether or not to dispose the underlying stream when this stream is disposed.</param>
		public SKFrontBufferedManagedStream (Stream managedStream, int bufferSize, bool disposeUnderlyingStream)
			: this (new SKManagedStream (managedStream, disposeUnderlyingStream), bufferSize, true)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="SKFrontBufferedStream" /> that wraps the specified stream.
		/// </summary>
		/// <param name="nativeStream">The stream to buffer.</param>
		/// <param name="bufferSize">The number of bytes to buffer.</param>
		public SKFrontBufferedManagedStream (SKStream nativeStream, int bufferSize)
			: this (nativeStream, bufferSize, false)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="SKFrontBufferedStream" /> that wraps the specified stream.
		/// </summary>
		/// <param name="nativeStream">The stream to buffer.</param>
		/// <param name="bufferSize">The number of bytes to buffer.</param>
		/// <param name="disposeUnderlyingStream">Whether or not to dispose the underlying stream when this stream is disposed.</param>
		public SKFrontBufferedManagedStream (SKStream nativeStream, int bufferSize, bool disposeUnderlyingStream)
		{
			var length = nativeStream.HasLength ? nativeStream.Length : 0;
			var position = nativeStream.HasPosition ? nativeStream.Position : 0;

			disposeStream = disposeUnderlyingStream;
			stream = nativeStream;
			hasLength = nativeStream.HasPosition && nativeStream.HasLength;
			streamLength = length - position;
			offset = 0;
			bufferedSoFar = 0;
			bufferLength = bufferSize;
			frontBuffer = new byte[bufferSize];
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeManaged ()
		{
			if (disposeStream && stream != null) {
				stream.Dispose ();
				stream = null;
			}

			base.DisposeManaged ();
		}

		protected internal override IntPtr OnRead (IntPtr buffer, IntPtr size)
		{
			var start = offset;

			// 1. read any data that was previously buffered
			if ((int)size > 0 && offset < bufferedSoFar)
			{
				var copied = Math.Min ((int)size, bufferedSoFar - offset);

				if (buffer != IntPtr.Zero)
				{
					Marshal.Copy (frontBuffer, offset, buffer, copied);
					buffer += copied;
				}

				offset += copied;
				size -= copied;
			}

			bool isAtEnd = false;

			// 2. buffer any more data, and then read it
			if ((int)size > 0 && bufferedSoFar < bufferLength)
			{
				var bytesToBuffer = Math.Min ((int)size, bufferLength - bufferedSoFar);

				var tempBuffer = Marshal.AllocCoTaskMem (bytesToBuffer);
				var bytesRead = stream.Read (tempBuffer, bytesToBuffer);
				Marshal.Copy (tempBuffer, frontBuffer, offset, bytesRead);
				Marshal.FreeCoTaskMem (tempBuffer);

				isAtEnd = bytesRead < bytesToBuffer;

				bufferedSoFar += bytesRead;
				if (buffer != IntPtr.Zero)
				{
					Marshal.Copy (frontBuffer, offset, buffer, bytesRead);
					buffer += bytesRead;
				}

				offset += bytesRead;
				size -= bytesRead;
			}

			// 3. read the rest directly
			if ((int)size > 0 && !isAtEnd)
			{
				var bytesRead = stream.Read (buffer, (int)size);

				// past the buffer, so dispose it
				if (bytesRead > 0)
				{
					frontBuffer = null;
				}

				offset += bytesRead;
				size -= bytesRead;
			}

			return (IntPtr)(offset - start);
		}

		protected internal override IntPtr OnPeek (IntPtr buffer, IntPtr size)
		{
			if (offset >= bufferLength)
			{
				// this stream is not able to buffer.
				return (IntPtr)0;
			}

			// keep track of the offset so we can return to it.
			var start = offset;

			var bytesToCopy = Math.Min ((int)size, bufferLength - offset);
			var bytesRead = Read (buffer, bytesToCopy);

			// return to the original position
			offset = start;

			return (IntPtr)bytesRead;
		}

		protected internal override bool OnIsAtEnd ()
		{
			if (offset < bufferedSoFar)
			{
				// even if the underlying stream is at the end, this stream has been
				// rewound after buffering, so it is not at the end.
				return false;
			}

			return stream.IsAtEnd;
		}

		protected internal override bool OnRewind ()
		{
			// only allow a rewind if we have not exceeded the buffer.
			if (offset <= bufferLength)
			{
				offset = 0;
				return true;
			}

			return false;
		}

		protected internal override bool OnHasLength () => hasLength;

		protected internal override IntPtr OnGetLength () => (IntPtr)streamLength;

		// seeking is not supported
		protected internal override bool OnHasPosition () => false;

		// seeking is not supported
		protected internal override IntPtr OnGetPosition () => (IntPtr)0;

		// seeking is not supported
		protected internal override bool OnSeek (IntPtr position) => false;

		// seeking is not supported
		protected internal override bool OnMove (int offset) => false;

		// duplicating or forking is not supported
		protected internal override IntPtr OnCreateNew () => IntPtr.Zero;
	}
}

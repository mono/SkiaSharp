using System;
using System.Runtime.InteropServices;
using System.IO;

namespace SkiaSharp
{
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

		public SKFrontBufferedManagedStream (Stream managedStream, int bufferSize)
			: this (managedStream, bufferSize, false)
		{
		}

		public SKFrontBufferedManagedStream (Stream managedStream, int bufferSize, bool disposeUnderlyingStream)
			: this (new SKManagedStream (managedStream, disposeUnderlyingStream), bufferSize, true)
		{
		}

		public SKFrontBufferedManagedStream (SKStream nativeStream, int bufferSize)
			: this (nativeStream, bufferSize, false)
		{
		}

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

		protected override void Dispose (bool disposing)
		{
			if (disposing)
			{
				if (disposeStream && stream != null)
				{
					stream.Dispose ();
					stream = null;
				}
			}

			base.Dispose (disposing);
		}

		protected override IntPtr OnRead (IntPtr buffer, IntPtr size)
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

		protected override IntPtr OnPeek (IntPtr buffer, IntPtr size)
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

		protected override bool OnIsAtEnd ()
		{
			if (offset < bufferedSoFar)
			{
				// even if the underlying stream is at the end, this stream has been
				// rewound after buffering, so it is not at the end.
				return false;
			}

			return stream.IsAtEnd;
		}

		protected override bool OnRewind ()
		{
			// only allow a rewind if we have not exceeded the buffer.
			if (offset <= bufferLength)
			{
				offset = 0;
				return true;
			}

			return false;
		}

		protected override bool OnHasLength () => hasLength;

		protected override IntPtr OnGetLength () => (IntPtr)streamLength;

		// seeking is not supported
		protected override bool OnHasPosition () => false;

		// seeking is not supported
		protected override IntPtr OnGetPosition () => (IntPtr)0;

		// seeking is not supported
		protected override bool OnSeek (IntPtr position) => false;

		// seeking is not supported
		protected override bool OnMove (int offset) => false;

		// duplicating or forking is not supported
		protected override IntPtr OnCreateNew () => IntPtr.Zero;
	}
}

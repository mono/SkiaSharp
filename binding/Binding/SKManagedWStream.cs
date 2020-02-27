using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public class SKManagedWStream : SKAbstractManagedWStream
	{
		private Stream stream;
		private readonly bool disposeStream;

		public SKManagedWStream (Stream managedStream)
			: this (managedStream, false)
		{
		}

		public SKManagedWStream (Stream managedStream, bool disposeManagedStream)
			: this (managedStream, disposeManagedStream, true)
		{
		}

		private SKManagedWStream (Stream managedStream, bool disposeManagedStream, bool owns)
			: base (owns)
		{
			stream = managedStream;
			disposeStream = disposeManagedStream;
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

		protected override bool OnWrite (IntPtr buffer, IntPtr size)
		{
			var count = (int)size;
			var managedBuffer = ArrayPool<byte>.Shared.Rent (count);
			try {
				if (buffer != IntPtr.Zero) {
					Marshal.Copy (buffer, managedBuffer, 0, count);
				}
				stream.Write (managedBuffer, 0, count);
			} finally {
				ArrayPool<byte>.Shared.Return (managedBuffer);
			}
			return true;
		}

		protected override void OnFlush ()
		{
			stream.Flush ();
		}

		protected override IntPtr OnBytesWritten ()
		{
			return (IntPtr)stream.Position;
		}
	}
}

using System;
using System.Runtime.InteropServices;
using System.IO;

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

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (disposeStream && stream != null) {
					stream.Dispose ();
					stream = null;
				}
			}

			base.Dispose (disposing);
		}

		protected override bool OnWrite (IntPtr buffer, IntPtr size)
		{
			var count = (int)size;
			var managedBuffer = new byte[count];
			if (buffer != IntPtr.Zero) { 
				Marshal.Copy (buffer, managedBuffer, 0, count);
			}
			stream.Write (managedBuffer, 0, count);
			return true;
		}

		protected override void OnFlush ()
		{
			stream.Flush ();
		}

		protected override IntPtr OnBytesWritten()
		{
			return (IntPtr)stream.Position;
		}
	}
}

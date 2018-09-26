using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

namespace SkiaSharp
{
	public class SKManagedStream : SKAbstractManagedStream
	{
		private Stream stream;
		private bool isAsEnd;
		private readonly bool disposeStream;

		public SKManagedStream (Stream managedStream)
			: this (managedStream, false)
		{
		}

		public SKManagedStream (Stream managedStream, bool disposeManagedStream)
			: this (managedStream, disposeManagedStream, true)
		{
		}

		private SKManagedStream (Stream managedStream, bool disposeManagedStream, bool owns)
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

		private IntPtr OnReadManagedStream (IntPtr buffer, IntPtr size)
		{
			byte[] managedBuffer;
			using (var reader = new BinaryReader (stream, Encoding.UTF8, true)) {
				managedBuffer = reader.ReadBytes ((int)size);
			}
			var result = managedBuffer.Length;
			if (buffer != IntPtr.Zero) {
				Marshal.Copy (managedBuffer, 0, buffer, result);
			}
			if (!stream.CanSeek && (int)size > 0 && result <= (int)size) {
				isAsEnd = true;
			}
			return (IntPtr)result;
		}

		protected override IntPtr OnRead (IntPtr buffer, IntPtr size)
		{
			return OnReadManagedStream (buffer, size);
		}

		protected override IntPtr OnPeek (IntPtr buffer, IntPtr size)
		{
			if (!stream.CanSeek) {
				return (IntPtr)0;
			}
			var oldPos = stream.Position;
			var result = OnReadManagedStream (buffer, size);
			stream.Position = oldPos;
			return result;
		}

		protected override bool OnIsAtEnd ()
		{
			if (!stream.CanSeek) {
				return isAsEnd;
			}
			return stream.Position >= stream.Length;
		}

		protected override bool OnHasPosition ()
		{
			return stream.CanSeek;
		}

		protected override bool OnHasLength ()
		{
			return stream.CanSeek;
		}

		protected override bool OnRewind ()
		{
			if (!stream.CanSeek) {
				return false;
			}
			stream.Position = 0;
			return true;
		}

		protected override IntPtr OnGetPosition ()
		{
			if (!stream.CanSeek) {
				return (IntPtr)0;
			}
			return (IntPtr)stream.Position;
		}

		protected override IntPtr OnGetLength ()
		{
			if (!stream.CanSeek) {
				return (IntPtr)0;
			}
			return (IntPtr)stream.Length;
		}

		protected override bool OnSeek (IntPtr position)
		{
			if (!stream.CanSeek) {
				return false;
			}
			stream.Position = (long)position;
			return true;
		}

		protected override bool OnMove (int offset)
		{
			if (!stream.CanSeek) {
				return false;
			}
			stream.Position = stream.Position + offset;
			return true;
		}

		protected override IntPtr OnCreateNew ()
		{
			var newStream = new SKManagedStream (stream, false, false);
			return newStream.Handle;
		}
	}
}

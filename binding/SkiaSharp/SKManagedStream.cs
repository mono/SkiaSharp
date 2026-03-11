#nullable disable

using System;
using System.IO;

namespace SkiaSharp
{
	public class SKManagedStream : SKAbstractManagedStream
	{
		private Stream stream;

		private bool isAsEnd;
		private bool disposeStream;

		public SKManagedStream (Stream managedStream)
			: this (managedStream, false)
		{
		}

		public SKManagedStream (Stream managedStream, bool disposeManagedStream)
			: base (true)
		{
			stream = managedStream ?? throw new ArgumentNullException (nameof (managedStream));
			disposeStream = disposeManagedStream;
		}

		private SKManagedStream (Stream managedStream, bool disposeManagedStream, bool weak)
			: base (true, weak)
		{
			stream = managedStream ?? throw new ArgumentNullException (nameof (managedStream));
			disposeStream = disposeManagedStream;
		}

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

		public SKStreamAsset ToMemoryStream ()
		{
			using var native = new SKDynamicMemoryWStream ();
			CopyTo (native);
			return native.DetachAsStream ();
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

		protected internal override IntPtr OnRead (IntPtr buffer, IntPtr size)
		{
			return OnReadManagedStream (buffer, size);
		}

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

		protected internal override bool OnIsAtEnd ()
		{
			if (!stream.CanSeek) {
				return isAsEnd;
			}
			return stream.Position >= stream.Length;
		}

		protected internal override bool OnHasPosition ()
		{
			return stream.CanSeek;
		}

		protected internal override bool OnHasLength ()
		{
			return stream.CanSeek;
		}

		protected internal override bool OnRewind ()
		{
			if (!stream.CanSeek) {
				return false;
			}
			stream.Position = 0;
			return true;
		}

		protected internal override IntPtr OnGetPosition ()
		{
			if (!stream.CanSeek) {
				return (IntPtr)0;
			}
			return (IntPtr)stream.Position;
		}

		protected internal override IntPtr OnGetLength ()
		{
			if (!stream.CanSeek) {
				return (IntPtr)0;
			}
			return (IntPtr)stream.Length;
		}

		protected internal override bool OnSeek (IntPtr position)
		{
			if (!stream.CanSeek) {
				return false;
			}
			stream.Position = (long)position;
			return true;
		}

		protected internal override bool OnMove (int offset)
		{
			if (!stream.CanSeek) {
				return false;
			}
			stream.Position += offset;
			return true;
		}

		protected internal override IntPtr OnCreateNew ()
		{
			return IntPtr.Zero;
		}

		private byte[] ReadStreamFully ()
		{
			var pos = stream.Position;
			stream.Position = 0;

			byte[] data;
			if (stream is MemoryStream ms) {
				data = ms.ToArray ();
			} else {
				using var copy = new MemoryStream ();
				stream.CopyTo (copy);
				data = copy.ToArray ();
			}

			stream.Position = pos;
			return data;
		}

		protected internal override IntPtr OnDuplicate ()
		{
			if (!stream.CanSeek)
				return IntPtr.Zero;

			var data = ReadStreamFully ();
			var newManaged = new MemoryStream (data, 0, data.Length, false, true);
			newManaged.Position = 0;

			var newStream = new SKManagedStream (newManaged, true, weak: false);
			return newStream.Handle;
		}

		protected internal override IntPtr OnFork ()
		{
			if (!stream.CanSeek)
				return IntPtr.Zero;

			var pos = stream.Position;
			var data = ReadStreamFully ();
			var newManaged = new MemoryStream (data, 0, data.Length, false, true);
			newManaged.Position = pos;

			var newStream = new SKManagedStream (newManaged, true, weak: false);
			return newStream.Handle;
		}
	}
}

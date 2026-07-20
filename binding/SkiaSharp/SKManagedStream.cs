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

		// Lazily-created snapshot for duplicate/fork. Stored in native memory
		// via SKData (ref-counted), so multiple duplicates share one native
		// byte buffer with no additional managed byte-buffer allocations per duplicate.
		private SKData snapshotData;

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
			if (snapshotData != null) {
				snapshotData.Dispose ();
				snapshotData = null;
			}

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

		private SKData GetOrCreateSnapshot ()
		{
			if (snapshotData != null)
				return snapshotData;

			if (!stream.CanSeek)
				return null;

			var pos = stream.Position;
			try {
				stream.Position = 0;
				snapshotData = SKData.Create (stream, stream.Length);
			} finally {
				stream.Position = pos;
			}

			return snapshotData;
		}

		protected internal override IntPtr OnDuplicate ()
		{
			var data = GetOrCreateSnapshot ();
			if (data == null)
				return IntPtr.Zero;

			return SkiaApi.sk_memorystream_new_with_skdata (data.Handle);
		}

		protected internal override IntPtr OnFork ()
		{
			var duplicate = OnDuplicate ();
			if (duplicate != IntPtr.Zero)
				SkiaApi.sk_stream_seek (duplicate, (IntPtr)stream.Position);
			return duplicate;
		}
	}
}

using System;
using System.IO;

namespace SkiaSharp
{
	public class SKManagedStream : SKAbstractManagedStream
	{
		private Stream stream;

		private bool isAsEnd;
		private bool disposeStream;
		private bool wasCopied;

		private WeakReference parent;
		private WeakReference child;

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
			using (var native = new SKDynamicMemoryWStream ()) {
				CopyTo (native);
				return native.DetachAsStream ();
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeManaged ()
		{
			var childStream = child?.Target as SKManagedStream;
			var parentStream = parent?.Target as SKManagedStream;

			if (childStream != null && parentStream != null) {
				// remove this stream from the list by connecting the parent with the child
				childStream.parent = parent;
				parentStream.child = child;
			} else if (childStream != null) {
				// transfer ownership to child
				childStream.parent = null;
			} else if (parentStream != null) {
				// transfer ownership back to parent
				parentStream.child = null;
				parentStream.wasCopied = false;
				parentStream.disposeStream = disposeStream;

				disposeStream = false;
			}

			parent = null;
			child = null;

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

		protected override IntPtr OnRead (IntPtr buffer, IntPtr size)
		{
			VerifyOriginal ();

			return OnReadManagedStream (buffer, size);
		}

		protected override IntPtr OnPeek (IntPtr buffer, IntPtr size)
		{
			VerifyOriginal ();

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
			VerifyOriginal ();

			if (!stream.CanSeek) {
				return isAsEnd;
			}
			return stream.Position >= stream.Length;
		}

		protected override bool OnHasPosition ()
		{
			VerifyOriginal ();

			return stream.CanSeek;
		}

		protected override bool OnHasLength ()
		{
			VerifyOriginal ();

			return stream.CanSeek;
		}

		protected override bool OnRewind ()
		{
			VerifyOriginal ();

			if (!stream.CanSeek) {
				return false;
			}
			stream.Position = 0;
			return true;
		}

		protected override IntPtr OnGetPosition ()
		{
			VerifyOriginal ();

			if (!stream.CanSeek) {
				return (IntPtr)0;
			}
			return (IntPtr)stream.Position;
		}

		protected override IntPtr OnGetLength ()
		{
			VerifyOriginal ();

			if (!stream.CanSeek) {
				return (IntPtr)0;
			}
			return (IntPtr)stream.Length;
		}

		protected override bool OnSeek (IntPtr position)
		{
			VerifyOriginal ();

			if (!stream.CanSeek) {
				return false;
			}
			stream.Position = (long)position;
			return true;
		}

		protected override bool OnMove (int offset)
		{
			VerifyOriginal ();

			if (!stream.CanSeek) {
				return false;
			}
			stream.Position += offset;
			return true;
		}

		protected override IntPtr OnCreateNew ()
		{
			VerifyOriginal ();

			return IntPtr.Zero;
		}

		protected override IntPtr OnDuplicate ()
		{
			VerifyOriginal ();

			if (!stream.CanSeek)
				return IntPtr.Zero;

			var newStream = new SKManagedStream (stream, disposeStream);
			newStream.parent = new WeakReference (this);

			wasCopied = true;
			disposeStream = false;
			child = new WeakReference (newStream);

			stream.Position = 0;

			return newStream.Handle;
		}

		protected override IntPtr OnFork ()
		{
			VerifyOriginal ();

			var newStream = new SKManagedStream (stream, disposeStream);

			wasCopied = true;
			disposeStream = false;

			return newStream.Handle;
		}

		private void VerifyOriginal ()
		{
			if (wasCopied)
				throw new InvalidOperationException ("This stream was duplicated or forked and cannot be read anymore.");
		}
	}
}

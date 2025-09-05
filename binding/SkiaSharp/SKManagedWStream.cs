#nullable disable

using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	/// <summary>
	/// Wraps a <see cref="T:System.IO.Stream" /> into a <see cref="SKWStream" /> (a writeable Skia stream)
	/// </summary>
	public class SKManagedWStream : SKAbstractManagedWStream
	{
		private Stream stream;
		private readonly bool disposeStream;

		/// <summary>
		/// Creates a new writeable stream from a <see cref="T:System.IO.Stream" />.
		/// </summary>
		/// <param name="managedStream">The managed stream.</param>
		/// <remarks>The underlying stream is not disposed when this object is disposed.</remarks>
		public SKManagedWStream (Stream managedStream)
			: this (managedStream, false)
		{
		}

		/// <summary>
		/// Creates a new writeable stream from a <see cref="T:System.IO.Stream" />.
		/// </summary>
		/// <param name="managedStream">The managed stream.</param>
		/// <param name="disposeManagedStream">If this is set to <see langword="true" />, the provided <see langword="managedStream" /> will be disposed when this instance is disposed.</param>
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

		protected internal override bool OnWrite (IntPtr buffer, IntPtr size)
		{
			var count = (int)size;
			using var managedBuffer = Utils.RentArray<byte> (count);
			if (buffer != IntPtr.Zero) {
				Marshal.Copy (buffer, (byte[])managedBuffer, 0, count);
			}
			stream.Write ((byte[])managedBuffer, 0, count);
			return true;
		}

		protected internal override void OnFlush ()
		{
			stream.Flush ();
		}

		protected internal override IntPtr OnBytesWritten ()
		{
			return (IntPtr)stream.Position;
		}
	}
}

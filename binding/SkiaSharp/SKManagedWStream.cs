#nullable disable

using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	/// <summary>Wraps a <see cref="T:System.IO.Stream" /> into a <see cref="T:SkiaSharp.SKWStream" /> (a writeable Skia stream)</summary>
	/// <remarks />
	public class SKManagedWStream : SKAbstractManagedWStream
	{
		private Stream stream;
		private readonly bool disposeStream;

		/// <summary>Creates a new writeable stream from a <see cref="T:System.IO.Stream" />.</summary>
		/// <param name="managedStream">The managed stream.</param>
		/// <remarks>The underlying stream is not disposed when this object is disposed.</remarks>
		public SKManagedWStream (Stream managedStream)
			: this (managedStream, false)
		{
		}

		/// <summary>Creates a new writeable stream from a <see cref="T:System.IO.Stream" />.</summary>
		/// <param name="managedStream">The managed stream.</param>
		/// <param name="disposeManagedStream">If this is set to <see langword="true" />, the provided <see langword="managedStream" /> will be disposed when this instance is disposed.</param>
		/// <remarks />
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

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKManagedWStream" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKManagedWStream" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKNativeObject" /> types to destroy any managed objects.</summary>
		/// <remarks />
		protected override void DisposeManaged ()
		{
			if (disposeStream && stream != null) {
				stream.Dispose ();
				stream = null;
			}

			base.DisposeManaged ();
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedWStream" /> types to copy the specified number of bytes from the specified buffer into the underlying stream.</summary>
		/// <param name="buffer">The buffer to copy into the underlying stream.</param>
		/// <param name="size">The number of bytes to copy from the buffer.</param>
		/// <returns>Returns <see langword="true" /> on success, otherwise <see langword="false" />.</returns>
		/// <remarks />
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

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedWStream" /> types to flush the bytes to the underlying stream.</summary>
		/// <remarks />
		protected internal override void OnFlush ()
		{
			stream.Flush ();
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedWStream" /> types to specify the number of bytes currently written to the stream.</summary>
		/// <returns>Returns the number of bytes currently written to the stream.</returns>
		/// <remarks />
		protected internal override IntPtr OnBytesWritten ()
		{
			return (IntPtr)stream.Position;
		}
	}
}

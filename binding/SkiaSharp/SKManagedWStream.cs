#nullable disable

using System;
using System.IO;

namespace SkiaSharp
{
	/// <summary>Wraps a <see cref="T:System.IO.Stream" /> into a <see cref="T:SkiaSharp.SKWStream" /> (a writeable Skia stream)</summary>
	/// <remarks>Exceptions from the underlying stream's write and flush operations are caught rather than rethrown. After either operation throws, subsequent writes return <see langword="false" /> and subsequent flushes are ignored.</remarks>
	public class SKManagedWStream : SKAbstractManagedWStream
	{
		private Stream stream;
		private readonly bool disposeStream;
		private ulong bytesWritten;
		private bool hasFaulted;

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
			: this (managedStream ?? throw new ArgumentNullException (nameof (managedStream)), disposeManagedStream, true)
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
		/// <remarks>Managed exceptions raised while writing are caught and reported as <see langword="false" />. After a write or flush exception, this wrapper rejects subsequent writes.</remarks>
		protected internal override unsafe bool OnWrite (IntPtr buffer, IntPtr size)
		{
			if (hasFaulted || stream == null)
				return false;

			var count = Utils.FromNativeSize (size);
			if (count == 0)
				return true;

			// Native size_t can exceed int; transfer bounded chunks and count only completed writes.
			try {
				if (buffer != IntPtr.Zero) {
					var src = (byte*)buffer;
#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP2_1_OR_GREATER
					using var scratch = Utils.RentArray<byte> ((int)Math.Min (count, (ulong)SKData.CopyBufferSize));
#endif
					for (var left = count; left > 0;) {
						var toWrite = (int)Math.Min (left, (ulong)SKData.CopyBufferSize);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
						stream.Write (new ReadOnlySpan<byte> (src, toWrite));
#else
						new ReadOnlySpan<byte> (src, toWrite).CopyTo (scratch.Span);
						stream.Write (scratch.Array, 0, toWrite);
#endif
						bytesWritten += (ulong)toWrite;
						left -= (ulong)toWrite;
						src += toWrite;
					}
				} else {
					// A null native buffer means zero-fill; pooled storage must be cleared before writing.
					using var zeroes = Utils.RentArray<byte> ((int)Math.Min (count, (ulong)SKData.CopyBufferSize));
					zeroes.Span.Clear ();
					for (var left = count; left > 0;) {
						var toWrite = (int)Math.Min (left, (ulong)zeroes.Length);
						stream.Write (zeroes.Array, 0, toWrite);
						bytesWritten += (ulong)toWrite;
						left -= (ulong)toWrite;
					}
				}
				return true;
			} catch {
				hasFaulted = true;
				return false;
			}
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedWStream" /> types to flush the bytes to the underlying stream.</summary>
		/// <remarks>Exceptions from the underlying stream are caught and mark this wrapper as faulted. After a write or flush exception, further flushes are ignored and further writes return <see langword="false" />.</remarks>
		protected internal override void OnFlush ()
		{
			if (hasFaulted || stream == null)
				return;

			try {
				stream.Flush ();
			} catch {
				hasFaulted = true;
			}
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedWStream" /> types to specify the number of bytes currently written to the stream.</summary>
		/// <returns>Returns the number of bytes currently written to the stream.</returns>
		/// <remarks />
		protected internal override IntPtr OnBytesWritten ()
		{
			return Utils.ToNativeSize (bytesWritten);
		}
	}
}

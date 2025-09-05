#nullable disable

using System;
using System.IO;

namespace SkiaSharp
{
	/// <summary>
	/// Wraps a <see cref="T:System.IO.Stream" /> into a <see cref="T:SkiaSharp.SKStreamAsset" /> (a seekable, rewindable Skia stream)
	/// </summary>
	/// <remarks>The following example shows how to wrap a <see cref="System.IO.Stream" /> that
	/// represents a stream into an embedded resource in an assembly and use it with
	/// SkiaSharp APIs that use resources:
	/// ## Examples
	/// ```csharp
	/// public static void BitmapShader (SKCanvas canvas, int width, int height)
	/// {
	/// var assembly = typeof(Demos).GetTypeInfo ().Assembly;
	/// // load the image from the embedded resource stream
	/// using (var resource = assembly.GetManifestResourceStream ("embedded.png"))
	/// using (var stream = new SKManagedStream(resource))
	/// using (var source = SKBitmap.Decode (stream)) {
	/// var matrix = SKMatrix.MakeRotation (30.0f);
	/// using (var shader = SKShader.CreateBitmap (source, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, matrix))
	/// using (var paint = new SKPaint ()) {
	/// paint.IsAntialias = true;
	/// paint.Shader = shader;
	/// // tile the bitmap
	/// canvas.Clear (SKColors.White);
	/// canvas.DrawPaint (paint);
	/// }
	/// }
	/// }
	/// ```</remarks>
	public class SKManagedStream : SKAbstractManagedStream
	{
		private Stream stream;

		private bool isAsEnd;
		private bool disposeStream;
		private bool wasCopied;

		private WeakReference parent;
		private WeakReference child;

		/// <summary>
		/// Creates a new read-only stream from a <see cref="T:System.IO.Stream" />.
		/// </summary>
		/// <param name="managedStream">The managed stream.</param>
		/// <remarks>The underlying stream is not disposed when this object is disposed.</remarks>
		public SKManagedStream (Stream managedStream)
			: this (managedStream, false)
		{
		}

		/// <summary>
		/// Creates a new read-only stream from a <see cref="T:System.IO.Stream" />, can optionally dispose the provided stream when this stream is disposed.
		/// </summary>
		/// <param name="managedStream">The managed stream.</param>
		/// <param name="disposeManagedStream">If this is set to <see langword="true" />, the provided <see langword="managedStream" /> will be disposed when this instance is disposed.</param>
		public SKManagedStream (Stream managedStream, bool disposeManagedStream)
			: base (true)
		{
			stream = managedStream ?? throw new ArgumentNullException (nameof (managedStream));
			disposeStream = disposeManagedStream;
		}

		/// <summary>
		/// Copy the contents of this stream into the destination stream.
		/// </summary>
		/// <param name="destination">The destination stream.</param>
		/// <returns>Returns the number of bytes that were copied.</returns>
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

		/// <summary>
		/// Copies the contents of this stream into a new memory stream.
		/// </summary>
		/// <returns>Returns the new memory stream.</returns>
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

		protected internal override IntPtr OnRead (IntPtr buffer, IntPtr size)
		{
			VerifyOriginal ();

			return OnReadManagedStream (buffer, size);
		}

		protected internal override IntPtr OnPeek (IntPtr buffer, IntPtr size)
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

		protected internal override bool OnIsAtEnd ()
		{
			VerifyOriginal ();

			if (!stream.CanSeek) {
				return isAsEnd;
			}
			return stream.Position >= stream.Length;
		}

		protected internal override bool OnHasPosition ()
		{
			VerifyOriginal ();

			return stream.CanSeek;
		}

		protected internal override bool OnHasLength ()
		{
			VerifyOriginal ();

			return stream.CanSeek;
		}

		protected internal override bool OnRewind ()
		{
			VerifyOriginal ();

			if (!stream.CanSeek) {
				return false;
			}
			stream.Position = 0;
			return true;
		}

		protected internal override IntPtr OnGetPosition ()
		{
			VerifyOriginal ();

			if (!stream.CanSeek) {
				return (IntPtr)0;
			}
			return (IntPtr)stream.Position;
		}

		protected internal override IntPtr OnGetLength ()
		{
			VerifyOriginal ();

			if (!stream.CanSeek) {
				return (IntPtr)0;
			}
			return (IntPtr)stream.Length;
		}

		protected internal override bool OnSeek (IntPtr position)
		{
			VerifyOriginal ();

			if (!stream.CanSeek) {
				return false;
			}
			stream.Position = (long)position;
			return true;
		}

		protected internal override bool OnMove (int offset)
		{
			VerifyOriginal ();

			if (!stream.CanSeek) {
				return false;
			}
			stream.Position += offset;
			return true;
		}

		protected internal override IntPtr OnCreateNew ()
		{
			VerifyOriginal ();

			return IntPtr.Zero;
		}

		protected internal override IntPtr OnDuplicate ()
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

		protected internal override IntPtr OnFork ()
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

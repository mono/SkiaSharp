#nullable disable

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using SkiaSharp.Internals;

namespace SkiaSharp
{
	/// <summary>The <see cref="T:SkiaSharp.SKData" /> holds an immutable data buffer.</summary>
	/// <remarks><para>Not only is the data immutable, but the actual pointer that is returned by the <see cref="P:SkiaSharp.SKData.Data" /> property is guaranteed to always be the same for the life of this instance.</para><para>The <see cref="M:SkiaSharp.SKData.AsStream" /> method can be used to return a <see cref="T:System.IO.Stream" /> that wraps this <see cref="T:SkiaSharp.SKData" /> and allows for .NET APIs to scan the contents of the <see cref="T:SkiaSharp.SKData" /> as a stream.</para></remarks>
	public unsafe class SKData : SKObject, ISKNonVirtualReferenceCounted
	{
		// We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
		// The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
		// improvement in Copy performance.
		internal const int CopyBufferSize = 81920;

		private static SKData empty;
		private static bool emptyInitialized;
		private static object emptyLock = new object ();

		internal SKData (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		/// <summary>Releases the resources associated with the data.</summary>
		/// <param name="disposing">Specify true to release both managed and unmanaged resources, false to release only unmanaged resources.</param>
		/// <remarks />
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		void ISKNonVirtualReferenceCounted.ReferenceNative ()
		{
			SkiaApi.sk_data_ref (Handle);
			GC.KeepAlive (this);
		}

		void ISKNonVirtualReferenceCounted.UnreferenceNative ()
		{
			SkiaApi.sk_data_unref (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Gets a reference to the empty data instance.</summary>
		/// <value>The empty data instance.</value>
		/// <remarks />
		public static SKData Empty =>
			LazyInitializer.EnsureInitialized (
				ref empty, ref emptyInitialized, ref emptyLock,
				// Immortal Skia singleton (SkData::MakeEmpty's function-local static) — never unref it.
				// See SKColorFilter.GetDisposeProtectedObject for the full teardown-crash rationale.
				() => GetDisposeProtectedObject (SkiaApi.sk_data_new_empty (), owns: false, unrefExisting: false));

		// CreateCopy

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the provided byte buffer.</summary>
		/// <param name="bytes">The pointer to a buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data.</returns>
		/// <remarks />
		public static SKData CreateCopy (IntPtr bytes, int length) =>
			CreateCopy (bytes, (ulong)length);

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the provided byte buffer.</summary>
		/// <param name="bytes">The pointer to a buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data.</returns>
		/// <remarks />
		public static SKData CreateCopy (IntPtr bytes, long length) =>
			CreateCopy (bytes, (ulong)length);

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the provided byte buffer.</summary>
		/// <param name="bytes">The pointer to a buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data.</returns>
		/// <remarks />
		public static SKData CreateCopy (IntPtr bytes, ulong length)
		{
			if (!PlatformConfiguration.Is64Bit && length > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException (nameof (length), "The length exceeds the size of pointers.");
			return GetObject (SkiaApi.sk_data_new_with_copy ((void*)bytes, (IntPtr)length));
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the provided byte array.</summary>
		/// <param name="bytes">The array of bytes that will be copied.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data.</returns>
		/// <remarks />
		public static SKData CreateCopy (byte[] bytes) =>
			CreateCopy (bytes, (ulong)bytes.Length);

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the provided byte array.</summary>
		/// <param name="bytes">The array of bytes that will be copied.</param>
		/// <param name="length">The size of the buffer to create.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data.</returns>
		/// <remarks />
		public static SKData CreateCopy (byte[] bytes, ulong length)
		{
			fixed (byte* b = bytes) {
				return GetObject (SkiaApi.sk_data_new_with_copy (b, (IntPtr)length));
			}
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the provided byte span.</summary>
		/// <param name="bytes">The span of bytes that will be copied.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data.</returns>
		/// <remarks />
		public static SKData CreateCopy (ReadOnlySpan<byte> bytes)
		{
			fixed (byte* b = bytes) {
				return CreateCopy ((IntPtr)b, (ulong)bytes.Length);
			}
		}

		// Create

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with uninitialized data.</summary>
		/// <param name="size">The size of the data buffer to create.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks />
		public static SKData Create (int size) =>
			GetObject (SkiaApi.sk_data_new_uninitialized ((IntPtr)size));

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with uninitialized data.</summary>
		/// <param name="size">The size of the data buffer to create.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks />
		public static SKData Create (long size) =>
			GetObject (SkiaApi.sk_data_new_uninitialized ((IntPtr)size));

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with uninitialized data.</summary>
		/// <param name="size">The size of the data buffer to create.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks />
		public static SKData Create (ulong size)
		{
			if (!PlatformConfiguration.Is64Bit && size > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException (nameof (size), "The size exceeds the size of pointers.");

			return GetObject (SkiaApi.sk_data_new_uninitialized ((IntPtr)size));
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with the data from the file.</summary>
		/// <param name="filename">The file to open.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks />
		public static SKData Create (string filename)
		{
			if (string.IsNullOrEmpty (filename))
				throw new ArgumentException ("The filename cannot be empty.", nameof (filename));

			var utf8path = StringUtilities.GetEncodedText (filename, SKTextEncoding.Utf8, true);
			fixed (byte* u = utf8path) {
				return GetObject (SkiaApi.sk_data_new_from_file (u));
			}
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks />
		public static SKData Create (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			if (stream.CanSeek) {
				return Create (stream, stream.Length - stream.Position);
			} else {
				using var memory = new SKDynamicMemoryWStream ();
				using (var managed = new SKManagedStream (stream)) {
					managed.CopyTo (memory);
				}
				return memory.DetachAsData ();
			}
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="length">The amount of data to read.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks />
		public static SKData Create (Stream stream, int length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using (var managed = new SKManagedStream (stream))
				return Create (managed, length);
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="length">The amount of data to read.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks />
		public static SKData Create (Stream stream, ulong length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using (var managed = new SKManagedStream (stream))
				return Create (managed, length);
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="length">The amount of data to read.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks />
		public static SKData Create (Stream stream, long length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using (var managed = new SKManagedStream (stream))
				return Create (managed, length);
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks />
		public static SKData Create (SKStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return Create (stream, stream.Length);
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="length">The amount of data to read.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks />
		public static SKData Create (SKStream stream, int length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			try {
				return GetObject (SkiaApi.sk_data_new_from_stream (stream.Handle, (IntPtr)length));
			} finally {
				GC.KeepAlive(stream);
			}
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="length">The amount of data to read.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks />
		public static SKData Create (SKStream stream, ulong length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			try {
				return GetObject (SkiaApi.sk_data_new_from_stream (stream.Handle, (IntPtr)length));
			} finally {
				GC.KeepAlive(stream);
			}
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="length">The amount of data to read.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks />
		public static SKData Create (SKStream stream, long length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			try {
				return GetObject (SkiaApi.sk_data_new_from_stream (stream.Handle, (IntPtr)length));
			} finally {
				GC.KeepAlive(stream);
			}
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with reference to the specified data.</summary>
		/// <param name="address">The pointer to a buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance with reference to the specified data.</returns>
		/// <remarks>The caller is responsible for ensuring the data buffer lives as long as the <see cref="T:SkiaSharp.SKData" /> instance.</remarks>
		public static SKData Create (IntPtr address, int length)
		{
			return Create (address, length, null, null);
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with reference to the specified data.</summary>
		/// <param name="address">The pointer to a buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		/// <param name="releaseProc">The delegate to invoke when the <see cref="T:SkiaSharp.SKData" /> instance is ready to be discarded.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance with reference to the specified data.</returns>
		/// <remarks>The caller is responsible for ensuring the data buffer lives as long as the <see cref="T:SkiaSharp.SKData" /> instance.</remarks>
		public static SKData Create (IntPtr address, int length, SKDataReleaseDelegate releaseProc)
		{
			return Create (address, length, releaseProc, null);
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with reference to the specified data.</summary>
		/// <param name="address">The pointer to a buffer.</param>
		/// <param name="length">The length of the buffer.</param>
		/// <param name="releaseProc">The delegate to invoke when the <see cref="T:SkiaSharp.SKData" /> instance is ready to be discarded.</param>
		/// <param name="context">The user state to pass to the delegate when it is invoked.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance with reference to the specified data.</returns>
		/// <remarks>The caller is responsible for ensuring the data buffer lives as long as the <see cref="T:SkiaSharp.SKData" /> instance.</remarks>
		public static SKData Create (IntPtr address, int length, SKDataReleaseDelegate releaseProc, object context)
		{
			var del = releaseProc != null && context != null
				? new SKDataReleaseDelegate ((addr, _) => releaseProc (addr, context))
				: releaseProc;
			DelegateProxies.Create (del, out _, out var ctx);
			var proxy = del is not null ? DelegateProxies.SKDataReleaseProxy : null;
			return GetObject (SkiaApi.sk_data_new_with_proc ((void*)address, (IntPtr)length, proxy, (void*)ctx));
		}

		internal static SKData FromCString (string str)
		{
			var bytes = Encoding.ASCII.GetBytes (str ?? string.Empty);
			return SKData.CreateCopy (bytes, (ulong)(bytes.Length + 1)); // + 1 for the terminating char
		}

		// Subset

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKData" /> that points to a slice in this <see cref="T:SkiaSharp.SKData" />.</summary>
		/// <param name="offset">The offset of the data.</param>
		/// <param name="length">The length for the new <see cref="T:SkiaSharp.SKData" />.</param>
		/// <returns>A new data object representing the slice, or <see langword="null" /> if the slice is invalid.</returns>
		/// <remarks />
		public SKData Subset (ulong offset, ulong length)
		{
			if (!PlatformConfiguration.Is64Bit) {
				if (length > UInt32.MaxValue)
					throw new ArgumentOutOfRangeException (nameof (length), "The length exceeds the size of pointers.");
				if (offset > UInt32.MaxValue)
					throw new ArgumentOutOfRangeException (nameof (offset), "The offset exceeds the size of pointers.");
			}
			var result = GetObject (SkiaApi.sk_data_new_subset (Handle, (IntPtr)offset, (IntPtr)length));
			GC.KeepAlive (this);
			return result;
		}

		// ToArray

		/// <summary>Copies the data object into a byte array.</summary>
		/// <returns>Returns the byte array of the data.</returns>
		/// <remarks />
		public byte[] ToArray ()
		{
			var array = AsSpan ().ToArray ();
			GC.KeepAlive (this);
			return array;
		}

		// properties

		/// <summary>Gets a value indicating whether or not the data is empty.</summary>
		/// <value><see langword="true" /> if the data is empty; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsEmpty => Size == 0;

		/// <summary>Gets the size of this data object in bytes.</summary>
		/// <value>The size in bytes.</value>
		/// <remarks />
		public long Size {
			get {
				var result = (long)SkiaApi.sk_data_get_size (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets a pointer to the data wrapped by this <see cref="T:SkiaSharp.SKData" />.</summary>
		/// <value>A pointer to the data.</value>
		/// <remarks />
		public IntPtr Data {
			get {
				var result = (IntPtr)SkiaApi.sk_data_get_data (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets a writable span that wraps the underlying data.</summary>
		/// <value>A span over the data buffer.</value>
		/// <remarks>This span is only valid as long as the data is valid.</remarks>
		public Span<byte> Span => new Span<byte> ((void*)Data, (int)Size);

		// AsStream

		/// <summary>Wraps the <see cref="T:SkiaSharp.SKData" /> as a <see cref="T:System.IO.Stream" />.</summary>
		/// <returns>Returns the new <see cref="T:System.IO.Stream" />.</returns>
		/// <remarks />
		public Stream AsStream () =>
			new SKDataStream (this, false);

		/// <summary>Wraps the <see cref="T:SkiaSharp.SKData" /> as a <see cref="T:System.IO.Stream" />.</summary>
		/// <param name="streamDisposesData"><see langword="true" /> to dispose the data object when the stream is disposed; otherwise, <see langword="false" />.</param>
		/// <returns>Returns the new <see cref="T:System.IO.Stream" />.</returns>
		/// <remarks />
		public Stream AsStream (bool streamDisposesData) =>
			new SKDataStream (this, streamDisposesData);

		// AsSpan

		/// <summary>Returns a span that wraps the underlying data.</summary>
		/// <returns>Returns the data as a span.</returns>
		/// <remarks>This span is only valid as long as the data is valid.</remarks>
		public ReadOnlySpan<byte> AsSpan ()
		{
			return new ReadOnlySpan<byte> ((void*)Data, (int)Size);
		}

		// SaveTo

		/// <summary>Saves the buffer into the provided stream.</summary>
		/// <param name="target">The stream to save the data into.</param>
		/// <remarks />
		public void SaveTo (Stream target)
		{
			if (target == null)
				throw new ArgumentNullException (nameof (target));

			var ptr = Data;
			var total = Size;
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
			// Write native memory straight to the stream: no intermediate managed buffer and no
			// Marshal.Copy, so the pixel/encoded bytes are copied once instead of twice.
			var src = (byte*)ptr;
			for (var left = total; left > 0;) {
				var copyCount = (int)Math.Min (CopyBufferSize, left);
				target.Write (new ReadOnlySpan<byte> (src, copyCount));
				left -= copyCount;
				src += copyCount;
			}
#else
			// Stream.Write(ReadOnlySpan<byte>) is unavailable on netstandard2.0 / .NET Framework, so
			// fall back to copying through a pooled buffer.
			using var buffer = Utils.RentArray<byte> (CopyBufferSize);
			for (var left = total; left > 0;) {
				var copyCount = (int)Math.Min (CopyBufferSize, left);
				Marshal.Copy (ptr, (byte[])buffer, 0, copyCount);
				left -= copyCount;
				ptr += copyCount;
				target.Write ((byte[])buffer, 0, copyCount);
			}
#endif
			GC.KeepAlive (this);
		}

		internal static SKData GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKData (h, o));

		internal static SKData GetDisposeProtectedObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddDisposeProtectedObject (handle, owns, unrefExisting, (h, o) => new SKData (h, o));

		//

		private class SKDataStream : UnmanagedMemoryStream
		{
			private SKData host;
			private readonly bool disposeHost;

			public unsafe SKDataStream (SKData host, bool disposeHost = false)
				: base ((byte*)host.Data, host.Size, host.Size, FileAccess.ReadWrite)
			{
				this.host = host;
				this.disposeHost = disposeHost;
			}

			protected override void Dispose (bool disposing)
			{
				base.Dispose (disposing);

				if (disposeHost) {
					host?.Dispose ();
				}
				host = null;
			}
		}

	}
}

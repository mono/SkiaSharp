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
		// Preserve the 80 KiB transfer limit, matching Stream.CopyTo's default buffer request.
		// Unlike Stream.CopyTo, these loops do not use the full (potentially larger) rented array.
		internal const int CopyBufferSize = 80 * 1024;

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
			if (length > (ulong)(bytes?.LongLength ?? 0))
				throw new ArgumentOutOfRangeException (nameof (length), "The length exceeds the size of the array.");

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
		public static SKData Create (int size)
		{
			if (size < 0)
				throw new ArgumentOutOfRangeException (nameof (size), "The size cannot be negative.");

			return Create ((ulong)size);
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with uninitialized data.</summary>
		/// <param name="size">The size of the data buffer to create.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks />
		public static SKData Create (long size)
		{
			if (size < 0)
				throw new ArgumentOutOfRangeException (nameof (size), "The size cannot be negative.");

			return Create ((ulong)size);
		}

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
		/// <returns>Returns the resulting data, an empty instance for a successful zero-byte result, or <see langword="null" /> if the known remaining byte count cannot be read or the result cannot be created.</returns>
		/// <remarks>Reads from the current position. If the remaining length is known, an early end of the stream returns <see langword="null" />. Otherwise, reads until EOF. The input is not rewound after an incomplete read.</remarks>
		public static SKData Create (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (!stream.CanSeek)
				return CreateFromUnknownLengthStream (stream);

			var len = stream.Length;
			// SKFrontBufferedStream can report -1 for an unknown length despite CanSeek being true.
			if (len < 0)
				return CreateFromUnknownLengthStream (stream);

			var pos = stream.Position;
			if (pos >= len)
				return Empty;

			var remaining = len - pos;
			return CreateFromStream (stream, (ulong)remaining, checkRemainingLength: false);
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="length">The amount of data to read, in bytes.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance containing <paramref name="length" /> bytes, or <see langword="null" /> if the stream does not contain enough data.</returns>
		/// <remarks>Reads <paramref name="length" /> bytes from the current position in <paramref name="stream" />. If the stream contains at least <paramref name="length" /> bytes, a new <see cref="T:SkiaSharp.SKData" /> of that size is returned; if the end of the stream is reached before <paramref name="length" /> bytes are read, this method returns <see langword="null" />.</remarks>
		public static SKData Create (Stream stream, int length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			if (length < 0)
				throw new ArgumentOutOfRangeException (nameof (length), "The length cannot be negative.");

			return CreateFromStream (stream, (ulong)length, checkRemainingLength: true);
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="length">The amount of data to read, in bytes.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance containing <paramref name="length" /> bytes, or <see langword="null" /> if the stream does not contain enough data.</returns>
		/// <remarks>Reads <paramref name="length" /> bytes from the current position in <paramref name="stream" />. If the stream contains at least <paramref name="length" /> bytes, a new <see cref="T:SkiaSharp.SKData" /> of that size is returned; if the end of the stream is reached before <paramref name="length" /> bytes are read, this method returns <see langword="null" />.</remarks>
		public static SKData Create (Stream stream, ulong length) =>
			CreateFromStream (stream, length, checkRemainingLength: true);

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="length">The amount of data to read, in bytes.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance containing <paramref name="length" /> bytes, or <see langword="null" /> if the stream does not contain enough data.</returns>
		/// <remarks>Reads <paramref name="length" /> bytes from the current position in <paramref name="stream" />. If the stream contains at least <paramref name="length" /> bytes, a new <see cref="T:SkiaSharp.SKData" /> of that size is returned; if the end of the stream is reached before <paramref name="length" /> bytes are read, this method returns <see langword="null" />.</remarks>
		public static SKData Create (Stream stream, long length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			if (length < 0)
				throw new ArgumentOutOfRangeException (nameof (length), "The length cannot be negative.");

			return CreateFromStream (stream, (ulong)length, checkRemainingLength: true);
		}

		private static SKData CreateFromUnknownLengthStream (Stream stream)
		{
			// Stage unknown-length input in native blocks, then gather it into contiguous SKData.
			using var memory = new SKDynamicMemoryWStream ();
			using var buffer = Utils.RentArray<byte> (CopyBufferSize);

			ulong totalRead = 0;
			while (true) {
				var readLen = stream.Read (buffer.Array, 0, buffer.Length);
				if ((uint)readLen > (uint)buffer.Length)
					throw new IOException ("The stream returned an invalid byte count.");

				if (readLen == 0)
					break;

				totalRead = checked (totalRead + (ulong)readLen);
				if (!PlatformConfiguration.Is64Bit && totalRead > UInt32.MaxValue)
					throw new ArgumentOutOfRangeException ("length", "The length exceeds the size of pointers.");

				if (!memory.Write (buffer.Array, readLen))
					return null;
			}

			return memory.DetachAsData ();
		}

		private static SKData CreateFromStream (Stream stream, ulong length, bool checkRemainingLength)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			if (!PlatformConfiguration.Is64Bit && length > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException (nameof (length), "The length exceeds the size of pointers.");

			if (length == 0)
				return Empty;

			// Reject impossible explicit-length requests before allocating or consuming input.
			// Lengthless callers already calculated the available count and skip this metadata check.
			if (checkRemainingLength && stream.CanSeek) {
				var len = stream.Length;
				if (len >= 0) {
					var pos = stream.Position;
					var remaining = len - pos;
					if (remaining < 0 || (ulong)remaining < length)
						return null;
				}
			}

			var data = Create (length);
			if (data == null)
				return null;

			try {
				var dst = (byte*)data.Data;
				ulong totalRead = 0;
#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP2_1_OR_GREATER
				using var scratch = Utils.RentArray<byte> ((int)Math.Min (length, (ulong)CopyBufferSize));
#endif

				// Fill the final allocation in bounded reads; a positive short read is progress, not EOF.
				while (totalRead < length) {
					var toRead = (int)Math.Min (length - totalRead, (ulong)CopyBufferSize);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
					var len = stream.Read (new Span<byte> (dst + totalRead, toRead));
#else
					var len = stream.Read (scratch.Array, 0, toRead);
#endif
					if ((uint)len > (uint)toRead)
						throw new IOException ("The stream returned an invalid byte count.");
					if (len == 0) {
						data.Dispose ();
						return null;
					}

#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP2_1_OR_GREATER
					scratch.Span.Slice (0, len).CopyTo (new Span<byte> (dst + totalRead, len));
#endif
					totalRead += (ulong)len;
				}

				return data;
			} catch {
				data.Dispose ();
				throw;
			}
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <returns>Returns the resulting data, an empty instance for a successful zero-byte result, or <see langword="null" /> if the known remaining byte count cannot be read or the result cannot be created.</returns>
		/// <remarks>Reads from the current position. If the remaining length is known, an early end of the stream returns <see langword="null" />. Otherwise, reads until EOF. The input is not rewound after an incomplete read.</remarks>
		public static SKData Create (SKStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			// Choose one copy route; a known-length failure must not restart as an unknown-length read.
			try {
				if (stream is SKAbstractManagedStream managed) {
					// Keep managed overrides on the managed stack so their original exceptions can propagate.
					if (managed.OnHasLength () && managed.OnHasPosition ()) {
						var len = Utils.FromNativeSize (managed.OnGetLength ());
						var pos = Utils.FromNativeSize (managed.OnGetPosition ());

						if (pos >= len)
							return Empty;

						var remaining = len - pos;
						return CreateFromManagedStream (managed, remaining);
					}

					return CreateFromUnknownLengthManagedStream (managed);
				}

				if (stream.HasLength && stream.HasPosition) {
					var len = stream.Length64;
					var pos = stream.Position64;

					if (pos >= len)
						return Empty;

					var remaining = len - pos;
					return CreateFromStream (stream, (ulong)remaining);
				}

				return CreateFromUnknownLengthStream (stream);
			} finally {
				// Keep the source owner alive through every helper and its cleanup, including early exits.
				GC.KeepAlive (stream);
			}
		}

		private static SKData CreateFromManagedStream (SKAbstractManagedStream stream, ulong length)
		{
			// Fill one final allocation exactly; early EOF is failure rather than a partial result.
			var data = Create (length);
			if (data == null)
				return null;

			try {
				var dst = (byte*)data.Data;
				ulong totalRead = 0;
				while (totalRead < length) {
					var toRead = (int)Math.Min (length - totalRead, (ulong)CopyBufferSize);
					var read = Utils.FromNativeSize (stream.OnRead ((IntPtr)(dst + totalRead), (IntPtr)toRead));
					if (read > (ulong)toRead)
						throw new IOException ("The stream returned an invalid byte count.");
					if (read == 0)
						break;
					totalRead += read;
				}

				if (totalRead == length)
					return data;
			} catch {
				data.Dispose ();
				throw;
			}
			data.Dispose ();
			return null;
		}

		private static SKData CreateFromUnknownLengthManagedStream (SKAbstractManagedStream stream)
		{
			// No remaining-length snapshot is available, so accumulate bounded chunks until EOF.
			using var memory = new SKDynamicMemoryWStream ();
			using var buffer = Utils.RentArray<byte> (CopyBufferSize);
			fixed (byte* ptr = buffer.Array) {
				while (true) {
					var read = Utils.FromNativeSize (stream.OnRead ((IntPtr)ptr, (IntPtr)buffer.Length));
					if (read > (ulong)buffer.Length)
						throw new IOException ("The stream returned an invalid byte count.");
					if (read == 0)
						break;
					if (!memory.Write (buffer.Array, (int)read))
						return null;
				}
			}
			return memory.DetachAsData ();
		}

		private static SKData CreateFromUnknownLengthStream (SKStream stream)
		{
			// Stage native reads until EOF because the final allocation size is not known yet.
			using var memory = new SKDynamicMemoryWStream ();
			using var buffer = Utils.RentArray<byte> (CopyBufferSize);
			fixed (byte* ptr = buffer.Array) {
				while (true) {
					var read = stream.Read ((IntPtr)ptr, buffer.Length);
					if (read == 0)
						break;
					if (!memory.Write (buffer.Array, read))
						return null;
				}
			}
			return memory.DetachAsData ();
		}

		private static SKData CreateFromStream (SKStream stream, ulong length)
		{
			var data = Create (length);
			if (data == null)
				return null;

			try {
				var dst = (byte*)data.Data;
				ulong totalRead = 0;
				while (totalRead < length) {
					// Bound each native request while retaining the full-width total length.
					var toRead = (int)Math.Min (length - totalRead, (ulong)CopyBufferSize);
					var read = stream.Read ((IntPtr)(dst + totalRead), toRead);
					if (read == 0) {
						data.Dispose ();
						return null;
					}

					totalRead += (ulong)read;
				}

				return data;
			} catch {
				data.Dispose ();
				throw;
			}
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="length">The amount of data to read, in bytes.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance containing <paramref name="length" /> bytes, or <see langword="null" /> if the native factory cannot read the requested length.</returns>
		/// <remarks>The native factory makes at most one read from the current position and does not retry short reads. A short read returns <see langword="null" /> even if more bytes remain in <paramref name="stream" />.</remarks>
		public static SKData Create (SKStream stream, int length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			if (length < 0)
				throw new ArgumentOutOfRangeException (nameof (length), "The length cannot be negative.");

			return Create (stream, (ulong)length);
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="length">The amount of data to read, in bytes.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance containing <paramref name="length" /> bytes, or <see langword="null" /> if the native factory cannot read the requested length.</returns>
		/// <remarks>The native factory makes at most one read from the current position and does not retry short reads. A short read returns <see langword="null" /> even if more bytes remain in <paramref name="stream" />.</remarks>
		public static SKData Create (SKStream stream, ulong length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			if (!PlatformConfiguration.Is64Bit && length > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException (nameof (length), "The length exceeds the size of pointers.");

			try {
				return GetObject (SkiaApi.sk_data_new_from_stream (stream.Handle, (IntPtr)length));
			} finally {
				GC.KeepAlive(stream);
			}
		}

		/// <summary>Returns a new <see cref="T:SkiaSharp.SKData" /> instance with a copy of the data from the stream.</summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="length">The amount of data to read, in bytes.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKData" /> instance containing <paramref name="length" /> bytes, or <see langword="null" /> if the native factory cannot read the requested length.</returns>
		/// <remarks>The native factory makes at most one read from the current position and does not retry short reads. A short read returns <see langword="null" /> even if more bytes remain in <paramref name="stream" />.</remarks>
		public static SKData Create (SKStream stream, long length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			if (length < 0)
				throw new ArgumentOutOfRangeException (nameof (length), "The length cannot be negative.");

			return Create (stream, (ulong)length);
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
			var span = str.AsSpan ();
			var byteCount = Encoding.ASCII.GetByteCount (span);
			var data = SKData.Create (byteCount + 1);
			var bytes = data.Data.AsSpan (byteCount + 1);
			Encoding.ASCII.GetBytes (span, bytes);
			bytes[byteCount] = 0;
			return data;
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

			var src = (byte*)Data;
			var total = Size;
#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP2_1_OR_GREATER
			using var scratch = Utils.RentArray<byte> ((int)Math.Min (total, CopyBufferSize));
#endif
			for (var left = total; left > 0;) {
				var copyCount = (int)Math.Min (CopyBufferSize, left);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
				target.Write (new ReadOnlySpan<byte> (src, copyCount));
#else
				new ReadOnlySpan<byte> (src, copyCount).CopyTo (scratch.Span);
				target.Write (scratch.Array, 0, copyCount);
#endif
				left -= copyCount;
				src += copyCount;
			}
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

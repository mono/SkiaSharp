#nullable disable

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SkiaSharp.Internals;

namespace SkiaSharp
{
	public unsafe class SKData : SKObject, ISKNonVirtualReferenceCounted
	{
		// We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
		// The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
		// improvement in Copy performance.
		internal const int CopyBufferSize = 81920;

		private static readonly SKData empty;

		static SKData ()
		{
			empty = new SKDataStatic (SkiaApi.sk_data_new_empty ());
		}

		internal static void EnsureStaticInstanceAreInitialized ()
		{
			// IMPORTANT: do not remove to ensure that the static instances
			//            are initialized before any access is made to them
		}

		internal SKData (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		void ISKNonVirtualReferenceCounted.ReferenceNative () => SkiaApi.sk_data_ref (Handle);

		void ISKNonVirtualReferenceCounted.UnreferenceNative () => SkiaApi.sk_data_unref (Handle);

		public static SKData Empty => empty;

		// CreateCopy

		public static SKData CreateCopy (IntPtr bytes, int length) =>
			CreateCopy (bytes, (ulong)length);

		public static SKData CreateCopy (IntPtr bytes, long length) =>
			CreateCopy (bytes, (ulong)length);

		public static SKData CreateCopy (IntPtr bytes, ulong length)
		{
			if (!PlatformConfiguration.Is64Bit && length > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException (nameof (length), "The length exceeds the size of pointers.");
			return GetObject (SkiaApi.sk_data_new_with_copy ((void*)bytes, (IntPtr)length));
		}

		public static SKData CreateCopy (ReadOnlySpan<byte> bytes) =>
			CreateCopy (bytes, (ulong)bytes.Length);

		public static SKData CreateCopy (ReadOnlySpan<byte> bytes, ulong length)
		{
			fixed (byte* b = bytes) {
				return GetObject (SkiaApi.sk_data_new_with_copy (b, (IntPtr)length));
			}
		}

		// Create

		public static SKData Create (int size) =>
			GetObject (SkiaApi.sk_data_new_uninitialized ((IntPtr)size));

		public static SKData Create (long size) =>
			GetObject (SkiaApi.sk_data_new_uninitialized ((IntPtr)size));

		public static SKData Create (ulong size)
		{
			if (!PlatformConfiguration.Is64Bit && size > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException (nameof (size), "The size exceeds the size of pointers.");

			return GetObject (SkiaApi.sk_data_new_uninitialized ((IntPtr)size));
		}

		public static SKData Create (string filename)
		{
			if (string.IsNullOrEmpty (filename))
				throw new ArgumentException ("The filename cannot be empty.", nameof (filename));

			var utf8path = StringUtilities.GetEncodedText (filename, SKTextEncoding.Utf8, true);
			fixed (byte* u = utf8path) {
				return GetObject (SkiaApi.sk_data_new_from_file (u));
			}
		}

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

		public static SKData Create (Stream stream, int length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using (var managed = new SKManagedStream (stream))
				return Create (managed, length);
		}

		public static SKData Create (Stream stream, ulong length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using (var managed = new SKManagedStream (stream))
				return Create (managed, length);
		}

		public static SKData Create (Stream stream, long length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using (var managed = new SKManagedStream (stream))
				return Create (managed, length);
		}

		public static SKData Create (SKStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return Create (stream, stream.Length);
		}

		public static SKData Create (SKStream stream, int length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return GetObject (SkiaApi.sk_data_new_from_stream (stream.Handle, (IntPtr)length));
		}

		public static SKData Create (SKStream stream, ulong length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return GetObject (SkiaApi.sk_data_new_from_stream (stream.Handle, (IntPtr)length));
		}

		public static SKData Create (SKStream stream, long length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return GetObject (SkiaApi.sk_data_new_from_stream (stream.Handle, (IntPtr)length));
		}

		public static SKData Create (IntPtr address, int length)
		{
			return Create (address, length, null, null);
		}

		public static SKData Create (IntPtr address, int length, SKDataReleaseDelegate releaseProc)
		{
			return Create (address, length, releaseProc, null);
		}

		public static SKData Create (IntPtr address, int length, SKDataReleaseDelegate releaseProc, object context)
		{
			var del = releaseProc != null && context != null
				? new SKDataReleaseDelegate ((addr, _) => releaseProc (addr, context))
				: releaseProc;
			var proxy = DelegateProxies.Create (del, DelegateProxies.SKDataReleaseDelegateProxy, out _, out var ctx);
			return GetObject (SkiaApi.sk_data_new_with_proc ((void*)address, (IntPtr)length, proxy, (void*)ctx));
		}

		internal static SKData FromCString (string str)
		{
			var bytes = Encoding.ASCII.GetBytes (str ?? string.Empty);
			return SKData.CreateCopy (bytes, (ulong)(bytes.Length + 1)); // + 1 for the terminating char
		}

		// Subset

		public SKData Subset (ulong offset, ulong length)
		{
			if (!PlatformConfiguration.Is64Bit) {
				if (length > UInt32.MaxValue)
					throw new ArgumentOutOfRangeException (nameof (length), "The length exceeds the size of pointers.");
				if (offset > UInt32.MaxValue)
					throw new ArgumentOutOfRangeException (nameof (offset), "The offset exceeds the size of pointers.");
			}
			return GetObject (SkiaApi.sk_data_new_subset (Handle, (IntPtr)offset, (IntPtr)length));
		}

		// ToArray

		public byte[] ToArray ()
		{
			var array = AsSpan ().ToArray ();
			GC.KeepAlive (this);
			return array;
		}

		// properties

		public bool IsEmpty => Size == 0;

		public long Size => (long)SkiaApi.sk_data_get_size (Handle);

		public IntPtr Data => (IntPtr)SkiaApi.sk_data_get_data (Handle);

		public Span<byte> Span => new Span<byte> ((void*)Data, (int)Size);

		// AsStream

		public Stream AsStream () =>
			new SKDataStream (this, false);

		public Stream AsStream (bool streamDisposesData) =>
			new SKDataStream (this, streamDisposesData);

		// AsSpan

		public ReadOnlySpan<byte> AsSpan ()
		{
			return new ReadOnlySpan<byte> ((void*)Data, (int)Size);
		}

		// SaveTo

		public void SaveTo (Stream target)
		{
			if (target == null)
				throw new ArgumentNullException (nameof (target));

			var ptr = Data;
			var total = Size;
			using var buffer = Utils.RentArray<byte> (CopyBufferSize);
			for (var left = total; left > 0;) {
				var copyCount = (int)Math.Min (CopyBufferSize, left);
				Marshal.Copy (ptr, (byte[])buffer, 0, copyCount);
				left -= copyCount;
				ptr += copyCount;
				target.Write ((byte[])buffer, 0, copyCount);
			}
			GC.KeepAlive (this);
		}

		internal static SKData GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKData (h, o));

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

		//

		private sealed class SKDataStatic : SKData
		{
			internal SKDataStatic (IntPtr x)
				: base (x, false)
			{
			}

			protected override void Dispose (bool disposing) { }
		}
	}
}

using System;
using System.IO;
using System.Text;

namespace SkiaSharp
{
	public unsafe class SKData : SKObject, ISKNonVirtualReferenceCounted
	{
		private static readonly Lazy<SKData> empty;

		static SKData ()
		{
			empty = new Lazy<SKData> (() => new SKDataStatic (SkiaApi.sk_data_new_empty ()));
		}

		internal static void EnsureStaticInstanceAreInitialized ()
		{
			// IMPORTANT: do not remove to ensure that the static instances
			//            are initialized before any access is made to them
		}

		[Preserve]
		internal SKData (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		void ISKNonVirtualReferenceCounted.ReferenceNative () =>
			SkiaApi.sk_data_ref (Handle);

		void ISKNonVirtualReferenceCounted.UnreferenceNative () =>
			SkiaApi.sk_data_unref (Handle);

		public static SKData Empty =>
			empty.Value;

		public static SKData CreateCopy (IntPtr bytes, long length) =>
			GetObject<SKData> (SkiaApi.sk_data_new_with_copy ((void*)bytes, (IntPtr)length));

		public static SKData CreateCopy (ReadOnlySpan<byte> bytes) =>
			CreateCopy (bytes, bytes.Length);

		public static SKData CreateCopy (ReadOnlySpan<byte> bytes, long length)
		{
			fixed (byte* b = bytes) {
				return GetObject<SKData> (SkiaApi.sk_data_new_with_copy (b, (IntPtr)length));
			}
		}

		public static SKData Create (long size) =>
			GetObject<SKData> (SkiaApi.sk_data_new_uninitialized ((IntPtr)size));

		public static SKData Create (string filename)
		{
			if (string.IsNullOrEmpty (filename))
				throw new ArgumentException ("The filename cannot be empty.", nameof (filename));

			var utf8path = StringUtilities.GetEncodedText (filename, SKTextEncoding.Utf8);
			fixed (byte* u = utf8path) {
				return GetObject<SKData> (SkiaApi.sk_data_new_from_file (u));
			}
		}

		public static SKData Create (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return Create (stream, stream.Length);
		}

		public static SKData Create (Stream stream, long length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using var managed = new SKManagedStream (stream);
			return Create (managed, length);
		}

		public static SKData Create (SKStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return Create (stream, stream.Length);
		}

		public static SKData Create (SKStream stream, long length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return GetObject<SKData> (SkiaApi.sk_data_new_from_stream (stream.Handle, (IntPtr)length));
		}

		public static SKData Create (IntPtr address, long length) =>
			Create (address, length, null, null);

		public static SKData Create (IntPtr address, long length, SKDataReleaseDelegate releaseProc) =>
			Create (address, length, releaseProc, null);

		public static SKData Create (IntPtr address, long length, SKDataReleaseDelegate releaseProc, object context)
		{
			var del = releaseProc != null && context != null
				? new SKDataReleaseDelegate ((addr, _) => releaseProc (addr, context))
				: releaseProc;
			var proxy = DelegateProxies.Create (del, DelegateProxies.SKDataReleaseDelegateProxy, out _, out var ctx);
			return GetObject<SKData> (SkiaApi.sk_data_new_with_proc ((void*)address, (IntPtr)length, proxy, (void*)ctx));
		}

		internal static SKData FromCString (string str)
		{
			var bytes = Encoding.ASCII.GetBytes (str ?? string.Empty);
			return CreateCopy (bytes, bytes.Length + 1); // + 1 for the terminating char
		}

		public SKData Subset (long offset, long length) =>
			GetObject<SKData> (SkiaApi.sk_data_new_subset (Handle, (IntPtr)offset, (IntPtr)length));

		public byte[] ToArray ()
		{
			var array = AsSpan ().ToArray ();
			GC.KeepAlive (this);
			return array;
		}

		public bool IsEmpty =>
			Size == 0;

		public long Size =>
			(long)SkiaApi.sk_data_get_size (Handle);

		public IntPtr Data =>
			(IntPtr)SkiaApi.sk_data_get_data (Handle);

		public Stream AsStream () =>
			new SKDataStream (this, false);

		public Stream AsStream (bool streamDisposesData) =>
			new SKDataStream (this, streamDisposesData);

		public ReadOnlySpan<byte> AsSpan () =>
			new ReadOnlySpan<byte> ((void*)Data, (int)Size);

		public void SaveTo (Stream target)
		{
			if (target == null)
				throw new ArgumentNullException (nameof (target));

			AsStream ().CopyTo (target);
		}

		private class SKDataStream : UnmanagedMemoryStream
		{
			private SKData host;
			private readonly bool disposeHost;

			public unsafe SKDataStream (SKData host, bool disposeHost = false)
				: base ((byte*)host.Data, host.Size)
			{
				this.host = host;
				this.disposeHost = disposeHost;
			}

			protected override void Dispose (bool disposing)
			{
				base.Dispose (disposing);

				if (disposeHost && host != null)
					host.Dispose ();

				host = null;
			}
		}

		private sealed class SKDataStatic : SKData
		{
			internal SKDataStatic (IntPtr x)
				: base (x, false)
			{
				IgnorePublicDispose = true;
			}

			protected override void Dispose (bool disposing)
			{
				// do not dispose
			}
		}
	}
}

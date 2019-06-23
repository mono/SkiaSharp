﻿using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.ComponentModel;

namespace SkiaSharp
{
	public class SKData : SKObject
	{
		private const int CopyBufferSize = 8192;

		private static Lazy<SKData> empty;

		static SKData ()
		{
			empty = new Lazy<SKData> (() => GetObject<SKData> (SkiaApi.sk_data_new_empty ()));
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_data_unref (Handle);
			}

			base.Dispose (disposing);
		}

		[Preserve]
		internal SKData (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		public static SKData Empty => empty.Value;

		public static SKData CreateCopy (IntPtr bytes, ulong length)
		{
			if (SizeOf <IntPtr> () == 4 && length > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException (nameof (length), "The length exceeds the size of pointers.");
			return GetObject<SKData> (SkiaApi.sk_data_new_with_copy (bytes, (IntPtr) length));
		}

		public static SKData CreateCopy (byte[] bytes)
		{
			return CreateCopy (bytes, (ulong) bytes.Length);
		}

		public static SKData CreateCopy (byte[] bytes, ulong length)
		{
			return GetObject<SKData> (SkiaApi.sk_data_new_with_copy (bytes, (IntPtr) length));
		}

		public static SKData Create (int size)
		{
			return GetObject<SKData> (SkiaApi.sk_data_new_uninitialized ((IntPtr) size));
		}

		public static SKData Create (ulong size)
		{
			if (SizeOf <IntPtr> () == 4 && size > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException (nameof (size), "The size exceeds the size of pointers.");
				
			return GetObject<SKData> (SkiaApi.sk_data_new_uninitialized ((IntPtr) size));
		}

		public static SKData Create (string filename)
		{
			if (string.IsNullOrEmpty (filename))
				throw new ArgumentException ("The filename cannot be empty.", nameof (filename));

			var utf8path = StringUtilities.GetEncodedText (filename, SKEncoding.Utf8);
			return GetObject<SKData> (SkiaApi.sk_data_new_from_file(utf8path));
		}

		public static SKData Create (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			return Create (stream, stream.Length);
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

			return GetObject<SKData> (SkiaApi.sk_data_new_from_stream (stream.Handle, (IntPtr) length));
		}

		public static SKData Create (SKStream stream, ulong length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return GetObject<SKData> (SkiaApi.sk_data_new_from_stream (stream.Handle, (IntPtr) length));
		}

		public static SKData Create (SKStream stream, long length)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return GetObject<SKData> (SkiaApi.sk_data_new_from_stream (stream.Handle, (IntPtr) length));
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
			return GetObject<SKData> (SkiaApi.sk_data_new_with_proc (address, (IntPtr)length, proxy, ctx));
		}

		internal static SKData FromCString (string str)
		{
			var bytes = Encoding.ASCII.GetBytes (str ?? string.Empty);
			return SKData.CreateCopy (bytes, (ulong)(bytes.Length + 1)); // + 1 for the terminating char
		}

		public SKData Subset (ulong offset, ulong length)
		{
			if (SizeOf <IntPtr> () == 4) {
				if (length > UInt32.MaxValue)
					throw new ArgumentOutOfRangeException (nameof (length), "The length exceeds the size of pointers.");
				if (offset > UInt32.MaxValue)
					throw new ArgumentOutOfRangeException (nameof (offset), "The offset exceeds the size of pointers.");
			}
			return GetObject<SKData> (SkiaApi.sk_data_new_subset (Handle, (IntPtr) offset, (IntPtr) length));
		}

		public byte [] ToArray ()
		{
			var size = (int)Size;
			var bytes = new byte [size];

			if (size > 0) {
				Marshal.Copy (Data, bytes, 0, size);
			}

			return bytes;
		}

		public bool IsEmpty => Size == 0;

		public long Size => (long)SkiaApi.sk_data_get_size (Handle);

		public IntPtr Data => SkiaApi.sk_data_get_data (Handle);

		public Stream AsStream ()
		{
			return new SKDataStream (this, false);
		}

		public Stream AsStream (bool streamDisposesData)
		{
			return new SKDataStream (this, streamDisposesData);
		}

		public void SaveTo (Stream target)
		{
			if (target == null)
				throw new ArgumentNullException (nameof (target));

			var buffer = new byte [CopyBufferSize];
			var ptr = Data;
			var total = Size;

			for (var left = total; left > 0; ) {
				var copyCount = (int) Math.Min (CopyBufferSize, left);
				Marshal.Copy (ptr, buffer, 0, copyCount);
				left -= copyCount;
				ptr += copyCount;
				target.Write (buffer, 0, copyCount);
			}
		}

		private class SKDataStream : UnmanagedMemoryStream
		{
			private SKData host;
			private readonly bool disposeHost;

			public unsafe SKDataStream (SKData host, bool disposeHost = false)
				: base((byte *) host.Data, host.Size)
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


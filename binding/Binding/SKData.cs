//
// Bindings for SKData
//
// Author:
//   Miguel de Icaza
//
// Copyright 2016 Xamarin Inc
//

using System;
using System.Runtime.InteropServices;
using System.IO;

namespace SkiaSharp
{
	public class SKData : SKObject
	{
		private const int CopyBufferSize = 8192;

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

		public SKData ()
			: this (SkiaApi.sk_data_new_empty (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKData instance.");
			}
		}
			
		public SKData (IntPtr bytes, ulong length)
			: this (IntPtr.Zero, true)
		{
			if (Marshal.SizeOf (typeof(IntPtr)) == 4 && length > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException (nameof (length), "The length exceeds the size of pointers.");
			Handle = SkiaApi.sk_data_new_with_copy (bytes, (IntPtr) length);
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to copy the SKData instance.");
			}
		}

		public SKData (byte[] bytes)
			: this (SkiaApi.sk_data_new_with_copy (bytes, (IntPtr) bytes.Length), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to copy the SKData instance.");
			}
		}

		public static SKData FromMallocMemory (IntPtr bytes, ulong length)
		{
			if (Marshal.SizeOf (typeof(IntPtr)) == 4 && length > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException (nameof (length), "The length exceeds the size of pointers.");
			return GetObject<SKData> (SkiaApi.sk_data_new_from_malloc (bytes, (IntPtr) length));
		}

		public SKData Subset (ulong offset, ulong length)
		{
			if (Marshal.SizeOf (typeof(IntPtr)) == 4) {
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


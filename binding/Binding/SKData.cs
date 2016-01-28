//
// Bindings for SKData
//
// Author:
//   Miguel de Icaza
//
// Copyright 2015 Xamarin Inc
//

using System;
using System.Runtime.InteropServices;
using System.IO;

namespace SkiaSharp
{
	public class SKData : IDisposable
	{
		internal IntPtr handle;

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				SkiaApi.sk_data_unref (handle);
				handle = IntPtr.Zero;
			}
		}

		~SKData()
		{
			Dispose (false);
		}

		internal SKData (IntPtr x)
		{
			handle = x;
		}

		public SKData ()
		{
			handle = SkiaApi.sk_data_new_empty ();
		}
			
		public SKData (IntPtr bytes, ulong length)
		{
			if (Marshal.SizeOf<IntPtr> () == 4 && length > UInt32.MaxValue)
				throw new ArgumentException ("length", "The lenght exceeds the size of pointers");
			handle = SkiaApi.sk_data_new_with_copy (bytes, (IntPtr) length);
		}

		public SKData (byte[] bytes)
		{
			handle = SkiaApi.sk_data_new_with_copy (bytes, (IntPtr) bytes.Length);
		}

		public static SKData FromMallocMemory (IntPtr bytes, ulong length)
		{
			if (Marshal.SizeOf<IntPtr> () == 4 && length > UInt32.MaxValue)
				throw new ArgumentException ("length", "The length exceeds the size of pointers");
			return new SKData (SkiaApi.sk_data_new_from_malloc (bytes, (IntPtr) length));
		}

		public static SKData FromMallocMemory (byte[] bytes)
		{
			return new SKData (SkiaApi.sk_data_new_from_malloc (bytes, (IntPtr)bytes.Length));
		}

		public SKData Subset (ulong offset, ulong length)
		{
			if (Marshal.SizeOf<IntPtr> () == 4) {
				if (length > UInt32.MaxValue)
					throw new ArgumentException ("length", "The length exceeds the size of pointers");
				if (offset > UInt32.MaxValue)
					throw new ArgumentException ("offset", "The length exceeds the size of pointers");
			}
			return new SKData (SkiaApi.sk_data_new_subset (handle, (IntPtr) offset, (IntPtr) length));
		}

		public long Size => (long)SkiaApi.sk_data_get_size (handle);
		public IntPtr Data => SkiaApi.sk_data_get_data (handle);

		public void SaveTo (Stream target)
		{
			if (target == null)
				throw new ArgumentNullException ("target");
			var buffer = new byte [8192];
			var ptr = Data;
			var total = Size;

			for (var left = total; left > 0; ){
				var copyCount = (int) Math.Min (8192, left);
				Marshal.Copy (ptr, buffer, 0, copyCount);
				left -= copyCount;
				ptr += copyCount;
				target.Write (buffer, 0, copyCount);
			}
		}
		
	}
}


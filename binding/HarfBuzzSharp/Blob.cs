#nullable disable

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	public unsafe class Blob : NativeObject
	{
		private static readonly Lazy<Blob> emptyBlob = new Lazy<Blob> (() => new StaticBlob (HarfBuzzApi.hb_blob_get_empty ()));

		public static Blob Empty => emptyBlob.Value;

		internal Blob (IntPtr handle)
			: base (handle)
		{
		}

		public Blob (IntPtr data, int length, MemoryMode mode)
			: this (data, length, mode, null)
		{
		}

		public Blob (IntPtr data, int length, MemoryMode mode, ReleaseDelegate releaseDelegate)
			: this (Create (data, length, mode, releaseDelegate))
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeHandler ()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_blob_destroy (Handle);
			}
		}

		public int Length => (int)HarfBuzzApi.hb_blob_get_length (Handle);

		public int FaceCount => (int)HarfBuzzApi.hb_face_count (Handle);

		public bool IsImmutable => HarfBuzzApi.hb_blob_is_immutable (Handle);

		public void MakeImmutable () => HarfBuzzApi.hb_blob_make_immutable (Handle);

		public unsafe Stream AsStream ()
		{
			uint length;
			var dataPtr = HarfBuzzApi.hb_blob_get_data (Handle, &length);
			return new UnmanagedMemoryStream ((byte*)dataPtr, length);
		}

		public unsafe Span<byte> AsSpan ()
		{
			uint length;
			var dataPtr = HarfBuzzApi.hb_blob_get_data (Handle, &length);
			return new Span<byte> (dataPtr, (int)length);
		}

		public static Blob FromFile (string fileName)
		{
			if (!File.Exists (fileName)) {
				throw new FileNotFoundException ("Unable to find file.", fileName);
			}

			var blob = HarfBuzzApi.hb_blob_create_from_file (fileName);
			return new Blob (blob);
		}

		public static unsafe Blob FromStream (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			// For non-seekable streams, buffer into memory first
			if (!stream.CanSeek) {
				using var ms = new MemoryStream ();
				stream.CopyTo (ms);
				ms.Position = 0;
				return FromStream (ms);
			}

			var length = (int)(stream.Length - stream.Position);
			if (length == 0)
				return Empty;

			var dataPtr = Marshal.AllocCoTaskMem (length);
			try {
				using var ums = new UnmanagedMemoryStream ((byte*)dataPtr, length, length, FileAccess.ReadWrite);
				stream.CopyTo (ums);
				return new Blob (dataPtr, length, MemoryMode.ReadOnly, () => Marshal.FreeCoTaskMem (dataPtr));
			} catch {
				Marshal.FreeCoTaskMem (dataPtr);
				throw;
			}
		}

		private static IntPtr Create (IntPtr data, int length, MemoryMode mode, ReleaseDelegate releaseProc)
		{
			DelegateProxies.Create (releaseProc, out _, out var ctx);
			var proxy = releaseProc != null ? DelegateProxies.DestroyProxy : null;
			return HarfBuzzApi.hb_blob_create ((void*)data, (uint)length, mode, (void*)ctx, proxy);
		}

		private class StaticBlob : Blob
		{
			public StaticBlob (IntPtr handle)
				: base (handle)
			{
			}

			protected override void Dispose (bool disposing)
			{
				// do not dispose
			}
		}
	}
}

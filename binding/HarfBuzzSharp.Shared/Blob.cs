using System;
using System.IO;

namespace HarfBuzzSharp
{
	// public delegates
	//
	// bad choices.
	// this should not have had the "blob" prefix
	// it is a global dispose method, but we can't switch now
	// it is a breaking change since it will become ambiguous
	public delegate void BlobReleaseDelegate (object context);

	public class Blob : NativeObject
	{
		private static readonly Lazy<Blob> emptyBlob = new Lazy<Blob> (() => new StaticBlob (HarfBuzzApi.hb_blob_get_empty ()));

		public static Blob Empty => emptyBlob.Value;

		internal Blob (IntPtr handle)
			: base (handle)
		{
		}

		public Blob (IntPtr data, int length, MemoryMode mode, object userData, BlobReleaseDelegate releaseDelegate)
			: this (Create (data, length, mode, userData, new ReleaseDelegate (releaseDelegate)))
		{
		}

		public Blob (IntPtr data, int length, MemoryMode mode)
			: this (data, length, mode, null, null)
		{
		}

		public Blob (IntPtr data, uint length, MemoryMode mode, object userData, BlobReleaseDelegate releaseDelegate)
			: this (data, (int)length, mode, userData, releaseDelegate)
		{
		}

		protected override void DisposeHandler ()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_blob_destroy (Handle);
			}
		}

		public static Blob Empty => new Blob (HarfBuzzApi.hb_blob_get_empty ());

		public int Length => HarfBuzzApi.hb_blob_get_length (Handle);

		public int FaceCount => HarfBuzzApi.hb_face_count (Handle);

		public bool IsImmutable => HarfBuzzApi.hb_blob_is_immutable (Handle);

		public void MakeImmutable () => HarfBuzzApi.hb_blob_make_immutable (Handle);

		public unsafe Stream AsStream ()
		{
			var dataPtr = HarfBuzzApi.hb_blob_get_data (Handle, out var length);
			return new UnmanagedMemoryStream (dataPtr, length);
		}

		public unsafe ReadOnlySpan<byte> AsSpan ()
		{
			var dataPtr = HarfBuzzApi.hb_blob_get_data (Handle, out var length);
			return new ReadOnlySpan<byte> (dataPtr, length);
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
			// TODO: check to see if we can avoid the second copy (the ToArray)

			using (var ms = new MemoryStream ()) {
				stream.CopyTo (ms);
				var data = ms.ToArray ();

				fixed (byte* dataPtr = data) {
					return new Blob ((IntPtr)dataPtr, data.Length, MemoryMode.ReadOnly, null, _ => ms.Dispose ());
				}
			}
		}

		private static IntPtr Create (IntPtr data, int length, MemoryMode mode, object context, ReleaseDelegate releaseProc)
		{
			if (releaseProc == null) {
				return HarfBuzzApi.hb_blob_create (data, length, mode, IntPtr.Zero, IntPtr.Zero);
			} else {
				var ctx = new NativeDelegateContext (context, releaseProc);
				return HarfBuzzApi.hb_blob_create (data, length, mode, ctx.NativeContext, DestroyFunction.NativePointer);
			}
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

﻿using System;
using System.IO;

namespace HarfBuzzSharp
{
	public class Blob : NativeObject
	{
		private static readonly Lazy<Blob> emptyBlob = new Lazy<Blob> (() => new StaticBlob (HarfBuzzApi.hb_blob_get_empty ()));

		public static Blob Empty => emptyBlob.Value;

		internal Blob (IntPtr handle)
			: base (handle)
		{
		}

		[Obsolete ("Use Blob(IntPtr, int, MemoryMode, ReleaseDelegate releaseDelegate) instead.")]
		public Blob (IntPtr data, uint length, MemoryMode mode, object userData, BlobReleaseDelegate releaseDelegate)
			: this (data, (int)length, mode, () => releaseDelegate?.Invoke (userData))
		{
		}

		public Blob (IntPtr data, int length, MemoryMode mode)
			: this (data, length, mode, null)
		{
		}

		public Blob (IntPtr data, int length, MemoryMode mode, ReleaseDelegate releaseDelegate)
			: this (Create (data, length, mode, new ReleaseDelegate (releaseDelegate)))
		{
		}

		protected override void DisposeHandler ()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_blob_destroy (Handle);
			}
		}

		public int Length => HarfBuzzApi.hb_blob_get_length (Handle);

		public int FaceCount => HarfBuzzApi.hb_face_count (Handle);

		public bool IsImmutable => HarfBuzzApi.hb_blob_is_immutable (Handle);

		public void MakeImmutable () => HarfBuzzApi.hb_blob_make_immutable (Handle);

		public Stream AsStream ()
		{
			unsafe {
				var dataPtr = HarfBuzzApi.hb_blob_get_data (Handle, out var length);
				return new UnmanagedMemoryStream (dataPtr, length);
			}
		}

		public ReadOnlySpan<byte> AsSpan ()
		{
			unsafe {
				var dataPtr = HarfBuzzApi.hb_blob_get_data (Handle, out var length);
				return new ReadOnlySpan<byte> (dataPtr, length);
			}
		}

		public static Blob FromFile (string fileName)
		{
			if (!File.Exists (fileName)) {
				throw new FileNotFoundException ("Unable to find file.", fileName);
			}

			var blob = HarfBuzzApi.hb_blob_create_from_file (fileName);
			return new Blob (blob);
		}

		public static Blob FromStream (Stream stream)
		{
			// TODO: check to see if we can avoid the second copy (the ToArray)

			using (var ms = new MemoryStream ()) {
				stream.CopyTo (ms);
				var data = ms.ToArray ();

				unsafe {
					fixed (byte* dataPtr = data) {
						return new Blob ((IntPtr)dataPtr, data.Length, MemoryMode.ReadOnly, () => ms.Dispose ());
					}
				}
			}
		}

		private static IntPtr Create (IntPtr data, int length, MemoryMode mode, ReleaseDelegate releaseProc)
		{
			var proxy = DelegateProxies.Create (releaseProc, DelegateProxies.ReleaseDelegateProxy, out _, out var ctx);
			return HarfBuzzApi.hb_blob_create (data, length, mode, ctx, proxy);
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

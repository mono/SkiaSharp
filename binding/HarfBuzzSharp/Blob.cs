#nullable disable

using System;
using System.IO;

namespace HarfBuzzSharp
{
	/// <summary>
	/// Represents a blob of data in memory.
	/// </summary>
	public unsafe class Blob : NativeObject
	{
		private static readonly Lazy<Blob> emptyBlob = new Lazy<Blob> (() => new StaticBlob (HarfBuzzApi.hb_blob_get_empty ()));

		/// <summary>
		/// Gets a reference to the empty <see cref="Blob" /> instance.
		/// </summary>
		public static Blob Empty => emptyBlob.Value;

		internal Blob (IntPtr handle)
			: base (handle)
		{
		}

		/// <summary>
		/// Creates a new <see cref="Blob" /> instance, wrapping the specified data.
		/// </summary>
		/// <param name="data">The data to wrap.</param>
		/// <param name="length">The length of the data being wrapped.</param>
		/// <param name="mode">The memory mode to use.</param>
		/// <remarks>
		/// If there was a problem creating the blob, or if the data length was zero, then an empty blob will be created.
		/// </remarks>
		public Blob (IntPtr data, int length, MemoryMode mode)
			: this (data, length, mode, null)
		{
		}

		/// <summary>
		/// Creates a new <see cref="Blob" /> instance, wrapping the specified data.
		/// </summary>
		/// <param name="data">The data to wrap.</param>
		/// <param name="length">The length of the data being wrapped.</param>
		/// <param name="mode">The memory mode to use.</param>
		/// <param name="releaseDelegate">The delegate to invoke when the data is not needed anymore.</param>
		/// <remarks>
		/// If there was a problem creating the blob, or if the data length was zero, then an empty blob will be created.
		/// </remarks>
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

		/// <summary>
		/// Gets the length of blob data in bytes.
		/// </summary>
		public int Length => (int)HarfBuzzApi.hb_blob_get_length (Handle);

		/// <summary>
		/// Gets the number of faces in this blob.
		/// </summary>
		public int FaceCount => (int)HarfBuzzApi.hb_face_count (Handle);

		/// <summary>
		/// Gets the value indicating whether the blob is immutable.
		/// </summary>
		public bool IsImmutable => HarfBuzzApi.hb_blob_is_immutable (Handle);

		/// <summary>
		/// Makes the blob immutable.
		/// </summary>
		public void MakeImmutable () => HarfBuzzApi.hb_blob_make_immutable (Handle);

		/// <summary>
		/// Returns a stream that wraps the data.
		/// </summary>
		/// <returns>Returns the stream that wraps the data.</returns>
		/// <remarks>
		/// If the data is released, then the stream becomes invalid.
		/// </remarks>
		public unsafe Stream AsStream ()
		{
			uint length;
			var dataPtr = HarfBuzzApi.hb_blob_get_data (Handle, &length);
			return new UnmanagedMemoryStream ((byte*)dataPtr, length);
		}

		/// <summary>
		/// Returns a span that wraps the data.
		/// </summary>
		/// <returns>Returns the span that wraps the data.</returns>
		/// <remarks>
		/// If the data is released, then the span becomes invalid.
		/// </remarks>
		public unsafe Span<byte> AsSpan ()
		{
			uint length;
			var dataPtr = HarfBuzzApi.hb_blob_get_data (Handle, &length);
			return new Span<byte> (dataPtr, (int)length);
		}

		/// <summary>
		/// Creates a new <see cref="Blob" /> instance from the contents of the file.
		/// </summary>
		/// <param name="fileName">The path to the file to load.</param>
		/// <returns>Returns the new <see cref="Blob" /> instance.</returns>
		public static Blob FromFile (string fileName)
		{
			if (!File.Exists (fileName)) {
				throw new FileNotFoundException ("Unable to find file.", fileName);
			}

			var blob = HarfBuzzApi.hb_blob_create_from_file (fileName);
			return new Blob (blob);
		}

		/// <summary>
		/// Creates a new <see cref="Blob" /> instance from the contents of the stream.
		/// </summary>
		/// <param name="stream">The stream to use.</param>
		/// <returns>Returns the new <see cref="Blob" /> instance.</returns>
		public static unsafe Blob FromStream (Stream stream)
		{
			// TODO: check to see if we can avoid the second copy (the ToArray)

			using var ms = new MemoryStream ();
			stream.CopyTo (ms);
			var data = ms.ToArray ();

			fixed (byte* dataPtr = data) {
				return new Blob ((IntPtr)dataPtr, data.Length, MemoryMode.ReadOnly, () => ms.Dispose ());
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

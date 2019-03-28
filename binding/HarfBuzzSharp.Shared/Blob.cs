using System;
using System.IO;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	// public delegates
	public delegate void BlobReleaseDelegate (object context);

	// internal proxy delegates
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void hb_destroy_func_t (IntPtr context);

	public class Blob : NativeObject
	{
		// so the GC doesn't collect the delegate
		private static readonly hb_destroy_func_t destroy_funcInternal;
		private static readonly IntPtr destroy_func;

		static Blob ()
		{
			destroy_funcInternal = new hb_destroy_func_t (DestroyInternal);
			destroy_func = Marshal.GetFunctionPointerForDelegate (destroy_funcInternal);
		}

		internal Blob (IntPtr handle)
			: base (handle)
		{
		}

		public Blob (IntPtr data, int length, MemoryMode mode, object userData, BlobReleaseDelegate releaseDelegate)
			: this (Create (data, length, mode, userData, releaseDelegate))
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

		public int Length => HarfBuzzApi.hb_blob_get_length (Handle);

		public unsafe Stream Data {
			get {
				var dataPtr = HarfBuzzApi.hb_blob_get_data (Handle, out var length);
				return new UnmanagedMemoryStream (dataPtr, length);
			}
		}

		public int FaceCount => HarfBuzzApi.hb_face_count (Handle);

		public bool IsImmutable => HarfBuzzApi.hb_blob_is_immutable (Handle);

		public void MakeImmutable () => HarfBuzzApi.hb_blob_make_immutable (Handle);

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

		private static IntPtr Create (IntPtr data, int length, MemoryMode mode, object context, BlobReleaseDelegate releaseProc)
		{
			if (releaseProc == null) {
				return HarfBuzzApi.hb_blob_create (data, length, mode, IntPtr.Zero, IntPtr.Zero);
			} else {
				var ctx = new NativeDelegateContext (context, releaseProc);
				return HarfBuzzApi.hb_blob_create (data, length, mode, ctx.NativeContext, destroy_func);
			}
		}

		// internal proxy

		[MonoPInvokeCallback (typeof (hb_destroy_func_t))]
		private static void DestroyInternal (IntPtr context)
		{
			using (var ctx = NativeDelegateContext.Unwrap (context)) {
				ctx.GetDelegate<BlobReleaseDelegate> () (ctx.ManagedContext);
			}
		}
	}
}

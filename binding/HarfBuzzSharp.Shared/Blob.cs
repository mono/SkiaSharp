using System;
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

		public bool IsImmutable {
			get {
				return HarfBuzzApi.hb_blob_is_immutable (Handle);
			}
		}

		public int Length {
			get {
				return HarfBuzzApi.hb_blob_get_length (Handle);
			}
		}

		public void MakeImmutable () => HarfBuzzApi.hb_blob_make_immutable (Handle);

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_blob_destroy (Handle);
			}

			base.Dispose (disposing);
		}

		private static IntPtr Create (IntPtr data, int length, MemoryMode mode, object user_data, BlobReleaseDelegate releaseProc)
		{
			if (releaseProc == null) {
				return HarfBuzzApi.hb_blob_create (data, length, mode, IntPtr.Zero, IntPtr.Zero);
			} else {
				var ctx = new NativeDelegateContext (user_data, releaseProc);
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

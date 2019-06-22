using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	// public delegate
	public delegate void ReleaseDelegate (object context);

	// internal proxy delegate
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void hb_destroy_func_t (IntPtr context);

	internal static class DestroyFunction
	{
		// so the GC doesn't collect the delegate
		private static readonly hb_destroy_func_t managedReference;

		static DestroyFunction ()
		{
			managedReference = new hb_destroy_func_t (DestroyInternal);
			NativePointer = Marshal.GetFunctionPointerForDelegate (managedReference);
		}

		public static IntPtr NativePointer { get; }

		// internal proxy
		[MonoPInvokeCallback (typeof (hb_destroy_func_t))]
		private static void DestroyInternal (IntPtr context)
		{
			using (var ctx = NativeDelegateContext.Unwrap (context)) {
				ctx.GetDelegate<ReleaseDelegate> ()?.Invoke (ctx.ManagedContext);
			}
		}
	}
}

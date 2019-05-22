using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	// internal proxy delegates
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void hb_destroy_func_t (IntPtr context);

	// public delegates
	public delegate void ReleaseDelegate (object context);

	public class NativeObject : IDisposable
	{
		// so the GC doesn't collect the delegate
		private static readonly hb_destroy_func_t destroy_funcInternal;
		protected static readonly IntPtr destroy_func;

		private bool isDisposed;
		private readonly bool zero;

		static NativeObject ()
		{
			destroy_funcInternal = new hb_destroy_func_t (DestroyInternal);
			destroy_func = Marshal.GetFunctionPointerForDelegate (destroy_funcInternal);
		}

		internal NativeObject (IntPtr handle)
		{
			Handle = handle;
			zero = true;
		}

		internal NativeObject (IntPtr handle, bool zero)
		{
			Handle = handle;
			this.zero = zero;
		}

		~NativeObject ()
		{
			Dispose (false);
		}

		public virtual IntPtr Handle { get; protected set; }

		// Dispose method - always called
		private void Dispose (bool disposing)
		{
			if (isDisposed) {
				return;
			}

			isDisposed = true;

			if (!disposing) {
				return;
			}

			DisposeHandler ();

			if (zero) {
				Handle = IntPtr.Zero;
			}
		}

		// Intended to be overridden - always safe to use
		// since it will never be called unless applicable
		protected virtual void DisposeHandler ()
		{
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		internal static int SizeOf<T> ()
		{
#if WINDOWS_UWP || NET_STANDARD
			return Marshal.SizeOf<T> ();
#else
			return Marshal.SizeOf (typeof (T));
#endif
		}

		internal static IntPtr StructureArrayToPtr<T> (IReadOnlyList<T> items)
		{
			var size = SizeOf<T> ();
			var memory = Marshal.AllocCoTaskMem (size * items.Count);
			for (var i = 0; i < items.Count; i++) {
				var ptr = new IntPtr (memory.ToInt64 () + (i * size));
				Marshal.StructureToPtr (items[i], ptr, true);
			}
			return memory;
		}

		internal static IEnumerable<string> PtrToStringArray (IntPtr intPtr)
		{
			if (intPtr != IntPtr.Zero) {
				var ptr = Marshal.ReadIntPtr (intPtr);
				while (ptr != IntPtr.Zero) {
					var element = Marshal.PtrToStringAnsi (ptr);
					yield return element;
					intPtr = new IntPtr (intPtr.ToInt64 () + IntPtr.Size);
					ptr = Marshal.ReadIntPtr (intPtr);
				}
			}
		}

		// internal proxy

		[MonoPInvokeCallback (typeof (hb_destroy_func_t))]
		private static void DestroyInternal (IntPtr context)
		{
			using (var ctx = NativeDelegateContext.Unwrap (context)) {
				ctx.GetDelegate<ReleaseDelegate> () (ctx.ManagedContext);
			}
		}
	}
}

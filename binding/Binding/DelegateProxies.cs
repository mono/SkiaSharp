using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	// public delegates

	public delegate void SKDataReleaseDelegate (IntPtr address, object context);

	public delegate IntPtr GRGlGetProcDelegate (object context, string name);

	// internal proxy delegates

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate IntPtr GRGlGetProcDelegateProxyDelegate (IntPtr context, [MarshalAs(UnmanagedType.LPStr)] string name);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void SKDataReleaseDelegateProxyDelegate (IntPtr address, IntPtr context);

	internal static class DelegateProxies
	{
		// references to the proxy implementations
		public static SKDataReleaseDelegateProxyDelegate SKDataReleaseDelegateProxy { get; } = SKDataReleaseDelegateProxyImplementation;
		public static GRGlGetProcDelegateProxyDelegate GRGlGetProcDelegateProxy { get; } = GRGlGetProcDelegateProxyImplementation;

		// helper methods

		public static T Get<T> (Delegate managedDel, T nativeDel, out GCHandle gch, out IntPtr contextPtr)
			where T : Delegate
		{
			gch = GCHandle.Alloc (managedDel);

			contextPtr = managedDel != null
				? GCHandle.ToIntPtr (gch)
				: IntPtr.Zero;

			return managedDel != null ? nativeDel : null;
		}

		// internal proxy implementations

		[MonoPInvokeCallback (typeof (SKDataReleaseDelegateProxyDelegate))]
		private static void SKDataReleaseDelegateProxyImplementation (IntPtr address, IntPtr context)
		{
			var gch = GCHandle.FromIntPtr (context);
			try {
				var del = (SKDataReleaseDelegate)gch.Target;
				del.Invoke (address, null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (GRGlGetProcDelegateProxyDelegate))]
		private static IntPtr GRGlGetProcDelegateProxyImplementation (IntPtr context, string name)
		{
			var gch = GCHandle.FromIntPtr (context);
			var del = (GRGlGetProcDelegate)gch.Target;
			return del.Invoke (null, name);
		}
	}
}

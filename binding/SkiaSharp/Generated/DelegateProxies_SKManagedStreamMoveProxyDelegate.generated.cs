using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedstream_move_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <sk_stream_managedstream_t, void*, Int32, bool> SKManagedStreamMoveProxy = &SKManagedStreamMoveProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedStreamMoveProxyDelegate SKManagedStreamMoveProxy = SKManagedStreamMoveProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedStreamMoveProxyDelegate))]
#endif
	[return: MarshalAs (UnmanagedType.I1)]
	private static partial bool SKManagedStreamMoveProxyImplementation(sk_stream_managedstream_t s,void* context,Int32 offset);

	}
}

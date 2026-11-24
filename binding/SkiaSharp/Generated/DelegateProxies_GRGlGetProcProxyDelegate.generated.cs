using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for gr_gl_get_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, /* char */ void*, IntPtr> GRGlGetProcProxy = &GRGlGetProcProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly GRGlGetProcProxyDelegate GRGlGetProcProxy = GRGlGetProcProxyImplementation;
	[MonoPInvokeCallback (typeof (GRGlGetProcProxyDelegate))]
#endif
	private static partial IntPtr GRGlGetProcProxyImplementation(void* ctx,/* char */ void* name);

	}
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_graphite_shader_error_handler_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, /* char */ void*, /* char */ void*, bool, void> SKGraphiteShaderErrorHandlerProxy = &SKGraphiteShaderErrorHandlerProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKGraphiteShaderErrorHandlerProxyDelegate SKGraphiteShaderErrorHandlerProxy = SKGraphiteShaderErrorHandlerProxyImplementation;
	[MonoPInvokeCallback (typeof (SKGraphiteShaderErrorHandlerProxyDelegate))]
#endif
	private static partial void SKGraphiteShaderErrorHandlerProxyImplementation(void* userData,/* char */ void* shader,/* char */ void* errors,[MarshalAs (UnmanagedType.I1)] bool shaderWasCached);

	}
}

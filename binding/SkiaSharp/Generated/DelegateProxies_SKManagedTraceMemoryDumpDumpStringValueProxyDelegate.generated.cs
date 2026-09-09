using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedtraceMemoryDump_dumpStringValue_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <sk_managedtracememorydump_t, void*, /* char */ void*, /* char */ void*, /* char */ void*, void> SKManagedTraceMemoryDumpDumpStringValueProxy = &SKManagedTraceMemoryDumpDumpStringValueProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedTraceMemoryDumpDumpStringValueProxyDelegate SKManagedTraceMemoryDumpDumpStringValueProxy = SKManagedTraceMemoryDumpDumpStringValueProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedTraceMemoryDumpDumpStringValueProxyDelegate))]
#endif
	private static partial void SKManagedTraceMemoryDumpDumpStringValueProxyImplementation(sk_managedtracememorydump_t d,void* context,/* char */ void* dumpName,/* char */ void* valueName,/* char */ void* value);

	}
}

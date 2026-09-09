using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedtraceMemoryDump_dumpNumericValue_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <sk_managedtracememorydump_t, void*, /* char */ void*, /* char */ void*, /* char */ void*, UInt64, void> SKManagedTraceMemoryDumpDumpNumericValueProxy = &SKManagedTraceMemoryDumpDumpNumericValueProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedTraceMemoryDumpDumpNumericValueProxyDelegate SKManagedTraceMemoryDumpDumpNumericValueProxy = SKManagedTraceMemoryDumpDumpNumericValueProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedTraceMemoryDumpDumpNumericValueProxyDelegate))]
#endif
	private static partial void SKManagedTraceMemoryDumpDumpNumericValueProxyImplementation(sk_managedtracememorydump_t d,void* context,/* char */ void* dumpName,/* char */ void* valueName,/* char */ void* units,UInt64 value);

	}
}

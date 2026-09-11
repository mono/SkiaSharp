using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(sk_managedtracememorydump_t* d, void* context, const char* dumpName, const char* valueName, const char* units, uint64_t value)* sk_managedtraceMemoryDump_dumpNumericValue_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKManagedTraceMemoryDumpDumpNumericValueProxyDelegate(sk_managedtracememorydump_t d, void* context, /* char */ void* dumpName, /* char */ void* valueName, /* char */ void* units, UInt64 value);

}
#endif // !USE_LIBRARY_IMPORT

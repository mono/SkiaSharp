using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef size_t (*)(const sk_wstream_managedstream_t* s, void* context)* sk_managedwstream_bytesWritten_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate /* size_t */ IntPtr SKManagedWStreamBytesWrittenProxyDelegate(IntPtr s, void* context);

}
#endif // !USE_LIBRARY_IMPORT

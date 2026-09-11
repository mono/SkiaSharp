using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef size_t (*)(sk_stream_managedstream_t* s, void* context, void* buffer, size_t size)* sk_managedstream_read_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate /* size_t */ IntPtr SKManagedStreamReadProxyDelegate(sk_stream_managedstream_t s, void* context, void* buffer, /* size_t */ IntPtr size);

}
#endif // !USE_LIBRARY_IMPORT

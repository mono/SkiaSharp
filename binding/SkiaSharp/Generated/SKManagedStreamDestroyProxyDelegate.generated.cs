using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(sk_stream_managedstream_t* s, void* context)* sk_managedstream_destroy_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKManagedStreamDestroyProxyDelegate(sk_stream_managedstream_t s, void* context);

}
#endif // !USE_LIBRARY_IMPORT

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef size_t (*)(const sk_stream_managedstream_t* s, void* context)* sk_managedstream_getPosition_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate /* size_t */ IntPtr SKManagedStreamGetPositionProxyDelegate(IntPtr s, void* context);

}
#endif // !USE_LIBRARY_IMPORT

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef sk_stream_managedstream_t* (*)(const sk_stream_managedstream_t* s, void* context)* sk_managedstream_duplicate_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate IntPtr SKManagedStreamDuplicateProxyDelegate(IntPtr s, void* context);

}
#endif // !USE_LIBRARY_IMPORT

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef bool (*)(const sk_stream_managedstream_t* s, void* context)* sk_managedstream_hasLength_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool SKManagedStreamHasLengthProxyDelegate(sk_stream_managedstream_t s, void* context);

}
#endif // !USE_LIBRARY_IMPORT

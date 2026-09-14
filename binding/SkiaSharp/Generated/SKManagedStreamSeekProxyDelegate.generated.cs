using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef bool (*)(sk_stream_managedstream_t* s, void* context, size_t position)* sk_managedstream_seek_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool SKManagedStreamSeekProxyDelegate(sk_stream_managedstream_t s, void* context, /* size_t */ IntPtr position);

}
#endif // !USE_LIBRARY_IMPORT

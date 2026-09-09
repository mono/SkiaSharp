using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef bool (*)(sk_stream_managedstream_t* s, void* context)* sk_managedstream_rewind_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool SKManagedStreamRewindProxyDelegate(sk_stream_managedstream_t s, void* context);

}
#endif // !USE_LIBRARY_IMPORT

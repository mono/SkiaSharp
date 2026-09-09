using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef bool (*)(sk_stream_managedstream_t* s, void* context, int32_t offset)* sk_managedstream_move_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool SKManagedStreamMoveProxyDelegate(IntPtr s, void* context, Int32 offset);

}
#endif // !USE_LIBRARY_IMPORT

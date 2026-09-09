using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef bool (*)(sk_wstream_managedstream_t* s, void* context, const void* buffer, size_t size)* sk_managedwstream_write_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool SKManagedWStreamWriteProxyDelegate(IntPtr s, void* context, void* buffer, /* size_t */ IntPtr size);

}
#endif // !USE_LIBRARY_IMPORT

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(sk_wstream_managedstream_t* s, void* context)* sk_managedwstream_flush_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKManagedWStreamFlushProxyDelegate(IntPtr s, void* context);

}
#endif // !USE_LIBRARY_IMPORT

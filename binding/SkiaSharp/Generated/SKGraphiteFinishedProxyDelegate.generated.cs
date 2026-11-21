using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(void* context, bool success)* sk_graphite_finished_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKGraphiteFinishedProxyDelegate(void* context, Byte success);

}
#endif // !USE_LIBRARY_IMPORT

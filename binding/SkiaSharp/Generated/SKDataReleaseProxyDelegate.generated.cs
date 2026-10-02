using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(const void* ptr, void* context)* sk_data_release_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKDataReleaseProxyDelegate(void* ptr, void* context);

}
#endif // !USE_LIBRARY_IMPORT

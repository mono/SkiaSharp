using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(void* addr, void* context)* sk_bitmap_release_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKBitmapReleaseProxyDelegate(void* addr, void* context);

}
#endif // !USE_LIBRARY_IMPORT

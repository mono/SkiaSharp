using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(const void* addr, void* context)* sk_image_raster_release_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKImageRasterReleaseProxyDelegate(void* addr, void* context);

}
#endif // !USE_LIBRARY_IMPORT

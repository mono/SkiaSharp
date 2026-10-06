using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(void* addr, void* context)* sk_surface_raster_release_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKSurfaceRasterReleaseProxyDelegate(void* addr, void* context);

}
#endif // !USE_LIBRARY_IMPORT

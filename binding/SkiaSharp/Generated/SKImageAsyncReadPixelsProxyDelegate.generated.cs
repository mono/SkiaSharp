using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(void* context, const sk_image_async_read_result_t* result)* sk_image_async_read_pixels_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKImageAsyncReadPixelsProxyDelegate(void* context, IntPtr result);

}
#endif // !USE_LIBRARY_IMPORT

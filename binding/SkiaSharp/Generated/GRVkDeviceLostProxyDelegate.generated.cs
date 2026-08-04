using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(void* userData, const gr_vk_device_lost_info_t* info)* gr_vk_device_lost_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void GRVkDeviceLostProxyDelegate(void* userData, GRVkDeviceLostInfoNative* info);

}
#endif // !USE_LIBRARY_IMPORT

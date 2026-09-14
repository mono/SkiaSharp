using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef sk_graphite_vk_func_ptr (*)(void* userData, const char* name, vk_instance_t* instance, vk_device_t* device)* sk_graphite_vk_get_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate IntPtr SKGraphiteVkGetProxyDelegate(void* userData, /* char */ void* name, vk_instance_t instance, vk_device_t device);

}
#endif // !USE_LIBRARY_IMPORT

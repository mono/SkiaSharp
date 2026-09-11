using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef gr_vk_func_ptr (*)(void* ctx, const char* name, vk_instance_t* instance, vk_device_t* device)* gr_vk_get_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate IntPtr GRVkGetProcProxyDelegate(void* ctx, /* char */ void* name, vk_instance_t instance, vk_device_t device);

}
#endif // !USE_LIBRARY_IMPORT

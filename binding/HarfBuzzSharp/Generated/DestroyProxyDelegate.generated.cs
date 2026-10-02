using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace HarfBuzzSharp
{
	// typedef void (*)(void* user_data)* hb_destroy_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void DestroyProxyDelegate(void* user_data);

}
#endif // !USE_LIBRARY_IMPORT

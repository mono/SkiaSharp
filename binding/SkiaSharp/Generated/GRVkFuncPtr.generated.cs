using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)()* gr_vk_func_ptr
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void GRVkFuncPtr();

}
#endif // !USE_LIBRARY_IMPORT

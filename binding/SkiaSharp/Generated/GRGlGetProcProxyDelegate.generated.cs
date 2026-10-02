using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef gr_gl_func_ptr (*)(void* ctx, const char* name)* gr_gl_get_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate IntPtr GRGlGetProcProxyDelegate(void* ctx, /* char */ void* name);

}
#endif // !USE_LIBRARY_IMPORT

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)()* gr_gl_func_ptr
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void GRGlFuncPtr();

}
#endif // !USE_LIBRARY_IMPORT

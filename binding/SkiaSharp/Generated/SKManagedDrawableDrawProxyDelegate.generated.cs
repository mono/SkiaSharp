using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(sk_manageddrawable_t* d, void* context, sk_canvas_t* ccanvas)* sk_manageddrawable_draw_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKManagedDrawableDrawProxyDelegate(IntPtr d, void* context, IntPtr ccanvas);

}
#endif // !USE_LIBRARY_IMPORT

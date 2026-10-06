using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(sk_manageddrawable_t* d, void* context, sk_rect_t* rect)* sk_manageddrawable_getBounds_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKManagedDrawableGetBoundsProxyDelegate(sk_manageddrawable_t d, void* context, SKRect* rect);

}
#endif // !USE_LIBRARY_IMPORT

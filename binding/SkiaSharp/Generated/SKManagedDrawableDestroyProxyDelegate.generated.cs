using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(sk_manageddrawable_t* d, void* context)* sk_manageddrawable_destroy_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKManagedDrawableDestroyProxyDelegate(IntPtr d, void* context);

}
#endif // !USE_LIBRARY_IMPORT

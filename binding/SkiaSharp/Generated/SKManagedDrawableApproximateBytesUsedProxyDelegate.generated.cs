using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef size_t (*)(sk_manageddrawable_t* d, void* context)* sk_manageddrawable_approximateBytesUsed_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate /* size_t */ IntPtr SKManagedDrawableApproximateBytesUsedProxyDelegate(IntPtr d, void* context);

}
#endif // !USE_LIBRARY_IMPORT

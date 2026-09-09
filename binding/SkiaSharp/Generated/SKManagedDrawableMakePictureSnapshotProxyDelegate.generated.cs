using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef sk_picture_t* (*)(sk_manageddrawable_t* d, void* context)* sk_manageddrawable_makePictureSnapshot_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate IntPtr SKManagedDrawableMakePictureSnapshotProxyDelegate(IntPtr d, void* context);

}
#endif // !USE_LIBRARY_IMPORT

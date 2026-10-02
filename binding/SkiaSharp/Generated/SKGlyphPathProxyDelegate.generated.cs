using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(const sk_path_t* pathOrNull, const sk_matrix_t* matrix, void* context)* sk_glyph_path_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKGlyphPathProxyDelegate(sk_path_t pathOrNull, SKMatrix* matrix, void* context);

}
#endif // !USE_LIBRARY_IMPORT

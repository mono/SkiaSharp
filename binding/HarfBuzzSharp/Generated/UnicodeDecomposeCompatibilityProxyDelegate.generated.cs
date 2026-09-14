using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace HarfBuzzSharp
{
	// typedef unsigned int (*)(hb_unicode_funcs_t* ufuncs, hb_codepoint_t u, hb_codepoint_t* decomposed, void* user_data)* hb_unicode_decompose_compatibility_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate UInt32 UnicodeDecomposeCompatibilityProxyDelegate(hb_unicode_funcs_t ufuncs, UInt32 u, UInt32* decomposed, void* user_data);

}
#endif // !USE_LIBRARY_IMPORT

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace HarfBuzzSharp
{
	// typedef hb_codepoint_t (*)(hb_unicode_funcs_t* ufuncs, hb_codepoint_t unicode, void* user_data)* hb_unicode_mirroring_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate UInt32 UnicodeMirroringProxyDelegate(IntPtr ufuncs, UInt32 unicode, void* user_data);

}
#endif // !USE_LIBRARY_IMPORT

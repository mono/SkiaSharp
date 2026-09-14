using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace HarfBuzzSharp
{
	// typedef hb_bool_t (*)(hb_unicode_funcs_t* ufuncs, hb_codepoint_t a, hb_codepoint_t b, hb_codepoint_t* ab, void* user_data)* hb_unicode_compose_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool UnicodeComposeProxyDelegate(hb_unicode_funcs_t ufuncs, UInt32 a, UInt32 b, UInt32* ab, void* user_data);

}
#endif // !USE_LIBRARY_IMPORT

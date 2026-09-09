using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace HarfBuzzSharp
{
	// typedef hb_bool_t (*)(hb_buffer_t* buffer, hb_font_t* font, const char* message, void* user_data)* hb_buffer_message_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool BufferMessageProxyDelegate(IntPtr buffer, IntPtr font, /* char */ void* message, void* user_data);

}
#endif // !USE_LIBRARY_IMPORT

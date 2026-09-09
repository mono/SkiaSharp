using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace HarfBuzzSharp
{
	// typedef hb_blob_t* (*)(hb_face_t* face, hb_tag_t tag, void* user_data)* hb_reference_table_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate hb_blob_t ReferenceTableProxyDelegate(hb_face_t face, UInt32 tag, void* user_data);

}
#endif // !USE_LIBRARY_IMPORT

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef void (*)(void* userData, const char* shader, const char* errors, bool shaderWasCached)* sk_graphite_shader_error_handler_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void SKGraphiteShaderErrorHandlerProxyDelegate(void* userData, /* char */ void* shader, /* char */ void* errors, [MarshalAs (UnmanagedType.I1)] bool shaderWasCached);

}
#endif // !USE_LIBRARY_IMPORT

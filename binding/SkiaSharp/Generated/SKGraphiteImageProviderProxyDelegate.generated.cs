using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace SkiaSharp
{
	// typedef sk_image_t* (*)(void* userData, sk_graphite_recorder_t* recorder, const sk_image_t* image, bool mipmapped)* sk_graphite_image_provider_proc
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate IntPtr SKGraphiteImageProviderProxyDelegate(void* userData, IntPtr recorder, IntPtr image, [MarshalAs (UnmanagedType.I1)] bool mipmapped);

}
#endif // !USE_LIBRARY_IMPORT

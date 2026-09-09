using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region gr_context.h

		// void gr_backendrendertarget_delete(gr_backendrendertarget_t* rendertarget)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_backendrendertarget_delete (IntPtr rendertarget);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_backendrendertarget_delete (IntPtr rendertarget);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_backendrendertarget_delete (IntPtr rendertarget);
		}
		private static Delegates.gr_backendrendertarget_delete gr_backendrendertarget_delete_delegate;
		internal static void gr_backendrendertarget_delete (IntPtr rendertarget) =>
			(gr_backendrendertarget_delete_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_delete> ("gr_backendrendertarget_delete")).Invoke (rendertarget);
		#endif

		// gr_backend_t gr_backendrendertarget_get_backend(const gr_backendrendertarget_t* rendertarget)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial GRBackendNative gr_backendrendertarget_get_backend (IntPtr rendertarget);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GRBackendNative gr_backendrendertarget_get_backend (IntPtr rendertarget);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate GRBackendNative gr_backendrendertarget_get_backend (IntPtr rendertarget);
		}
		private static Delegates.gr_backendrendertarget_get_backend gr_backendrendertarget_get_backend_delegate;
		internal static GRBackendNative gr_backendrendertarget_get_backend (IntPtr rendertarget) =>
			(gr_backendrendertarget_get_backend_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_get_backend> ("gr_backendrendertarget_get_backend")).Invoke (rendertarget);
		#endif

		// bool gr_backendrendertarget_get_gl_framebufferinfo(const gr_backendrendertarget_t* rendertarget, gr_gl_framebufferinfo_t* glInfo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool gr_backendrendertarget_get_gl_framebufferinfo (IntPtr rendertarget, GRGlFramebufferInfo* glInfo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendrendertarget_get_gl_framebufferinfo (IntPtr rendertarget, GRGlFramebufferInfo* glInfo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_backendrendertarget_get_gl_framebufferinfo (IntPtr rendertarget, GRGlFramebufferInfo* glInfo);
		}
		private static Delegates.gr_backendrendertarget_get_gl_framebufferinfo gr_backendrendertarget_get_gl_framebufferinfo_delegate;
		internal static bool gr_backendrendertarget_get_gl_framebufferinfo (IntPtr rendertarget, GRGlFramebufferInfo* glInfo) =>
			(gr_backendrendertarget_get_gl_framebufferinfo_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_get_gl_framebufferinfo> ("gr_backendrendertarget_get_gl_framebufferinfo")).Invoke (rendertarget, glInfo);
		#endif

		// int gr_backendrendertarget_get_height(const gr_backendrendertarget_t* rendertarget)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 gr_backendrendertarget_get_height (IntPtr rendertarget);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendrendertarget_get_height (IntPtr rendertarget);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_backendrendertarget_get_height (IntPtr rendertarget);
		}
		private static Delegates.gr_backendrendertarget_get_height gr_backendrendertarget_get_height_delegate;
		internal static Int32 gr_backendrendertarget_get_height (IntPtr rendertarget) =>
			(gr_backendrendertarget_get_height_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_get_height> ("gr_backendrendertarget_get_height")).Invoke (rendertarget);
		#endif

		// int gr_backendrendertarget_get_samples(const gr_backendrendertarget_t* rendertarget)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 gr_backendrendertarget_get_samples (IntPtr rendertarget);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendrendertarget_get_samples (IntPtr rendertarget);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_backendrendertarget_get_samples (IntPtr rendertarget);
		}
		private static Delegates.gr_backendrendertarget_get_samples gr_backendrendertarget_get_samples_delegate;
		internal static Int32 gr_backendrendertarget_get_samples (IntPtr rendertarget) =>
			(gr_backendrendertarget_get_samples_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_get_samples> ("gr_backendrendertarget_get_samples")).Invoke (rendertarget);
		#endif

		// int gr_backendrendertarget_get_stencils(const gr_backendrendertarget_t* rendertarget)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 gr_backendrendertarget_get_stencils (IntPtr rendertarget);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendrendertarget_get_stencils (IntPtr rendertarget);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_backendrendertarget_get_stencils (IntPtr rendertarget);
		}
		private static Delegates.gr_backendrendertarget_get_stencils gr_backendrendertarget_get_stencils_delegate;
		internal static Int32 gr_backendrendertarget_get_stencils (IntPtr rendertarget) =>
			(gr_backendrendertarget_get_stencils_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_get_stencils> ("gr_backendrendertarget_get_stencils")).Invoke (rendertarget);
		#endif

		// int gr_backendrendertarget_get_width(const gr_backendrendertarget_t* rendertarget)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 gr_backendrendertarget_get_width (IntPtr rendertarget);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendrendertarget_get_width (IntPtr rendertarget);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_backendrendertarget_get_width (IntPtr rendertarget);
		}
		private static Delegates.gr_backendrendertarget_get_width gr_backendrendertarget_get_width_delegate;
		internal static Int32 gr_backendrendertarget_get_width (IntPtr rendertarget) =>
			(gr_backendrendertarget_get_width_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_get_width> ("gr_backendrendertarget_get_width")).Invoke (rendertarget);
		#endif

		// bool gr_backendrendertarget_is_valid(const gr_backendrendertarget_t* rendertarget)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool gr_backendrendertarget_is_valid (IntPtr rendertarget);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendrendertarget_is_valid (IntPtr rendertarget);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_backendrendertarget_is_valid (IntPtr rendertarget);
		}
		private static Delegates.gr_backendrendertarget_is_valid gr_backendrendertarget_is_valid_delegate;
		internal static bool gr_backendrendertarget_is_valid (IntPtr rendertarget) =>
			(gr_backendrendertarget_is_valid_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_is_valid> ("gr_backendrendertarget_is_valid")).Invoke (rendertarget);
		#endif

		// gr_backendrendertarget_t* gr_backendrendertarget_new_direct3d(int width, int height, const gr_d3d_textureresourceinfo_t* d3dInfo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_backendrendertarget_new_direct3d (Int32 width, Int32 height, GRD3DTextureResourceInfoNative* d3dInfo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_backendrendertarget_new_direct3d (Int32 width, Int32 height, GRD3DTextureResourceInfoNative* d3dInfo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_backendrendertarget_new_direct3d (Int32 width, Int32 height, GRD3DTextureResourceInfoNative* d3dInfo);
		}
		private static Delegates.gr_backendrendertarget_new_direct3d gr_backendrendertarget_new_direct3d_delegate;
		internal static IntPtr gr_backendrendertarget_new_direct3d (Int32 width, Int32 height, GRD3DTextureResourceInfoNative* d3dInfo) =>
			(gr_backendrendertarget_new_direct3d_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_new_direct3d> ("gr_backendrendertarget_new_direct3d")).Invoke (width, height, d3dInfo);
		#endif

		// gr_backendrendertarget_t* gr_backendrendertarget_new_gl(int width, int height, int samples, int stencils, const gr_gl_framebufferinfo_t* glInfo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_backendrendertarget_new_gl (Int32 width, Int32 height, Int32 samples, Int32 stencils, GRGlFramebufferInfo* glInfo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_backendrendertarget_new_gl (Int32 width, Int32 height, Int32 samples, Int32 stencils, GRGlFramebufferInfo* glInfo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_backendrendertarget_new_gl (Int32 width, Int32 height, Int32 samples, Int32 stencils, GRGlFramebufferInfo* glInfo);
		}
		private static Delegates.gr_backendrendertarget_new_gl gr_backendrendertarget_new_gl_delegate;
		internal static IntPtr gr_backendrendertarget_new_gl (Int32 width, Int32 height, Int32 samples, Int32 stencils, GRGlFramebufferInfo* glInfo) =>
			(gr_backendrendertarget_new_gl_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_new_gl> ("gr_backendrendertarget_new_gl")).Invoke (width, height, samples, stencils, glInfo);
		#endif

		// gr_backendrendertarget_t* gr_backendrendertarget_new_metal(int width, int height, const gr_mtl_textureinfo_t* mtlInfo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_backendrendertarget_new_metal (Int32 width, Int32 height, GRMtlTextureInfoNative* mtlInfo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_backendrendertarget_new_metal (Int32 width, Int32 height, GRMtlTextureInfoNative* mtlInfo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_backendrendertarget_new_metal (Int32 width, Int32 height, GRMtlTextureInfoNative* mtlInfo);
		}
		private static Delegates.gr_backendrendertarget_new_metal gr_backendrendertarget_new_metal_delegate;
		internal static IntPtr gr_backendrendertarget_new_metal (Int32 width, Int32 height, GRMtlTextureInfoNative* mtlInfo) =>
			(gr_backendrendertarget_new_metal_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_new_metal> ("gr_backendrendertarget_new_metal")).Invoke (width, height, mtlInfo);
		#endif

		// gr_backendrendertarget_t* gr_backendrendertarget_new_vulkan(int width, int height, const gr_vk_imageinfo_t* vkImageInfo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_backendrendertarget_new_vulkan (Int32 width, Int32 height, GRVkImageInfo* vkImageInfo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_backendrendertarget_new_vulkan (Int32 width, Int32 height, GRVkImageInfo* vkImageInfo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_backendrendertarget_new_vulkan (Int32 width, Int32 height, GRVkImageInfo* vkImageInfo);
		}
		private static Delegates.gr_backendrendertarget_new_vulkan gr_backendrendertarget_new_vulkan_delegate;
		internal static IntPtr gr_backendrendertarget_new_vulkan (Int32 width, Int32 height, GRVkImageInfo* vkImageInfo) =>
			(gr_backendrendertarget_new_vulkan_delegate ??= GetSymbol<Delegates.gr_backendrendertarget_new_vulkan> ("gr_backendrendertarget_new_vulkan")).Invoke (width, height, vkImageInfo);
		#endif

		// void gr_backendtexture_delete(gr_backendtexture_t* texture)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_backendtexture_delete (IntPtr texture);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_backendtexture_delete (IntPtr texture);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_backendtexture_delete (IntPtr texture);
		}
		private static Delegates.gr_backendtexture_delete gr_backendtexture_delete_delegate;
		internal static void gr_backendtexture_delete (IntPtr texture) =>
			(gr_backendtexture_delete_delegate ??= GetSymbol<Delegates.gr_backendtexture_delete> ("gr_backendtexture_delete")).Invoke (texture);
		#endif

		// gr_backend_t gr_backendtexture_get_backend(const gr_backendtexture_t* texture)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial GRBackendNative gr_backendtexture_get_backend (IntPtr texture);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GRBackendNative gr_backendtexture_get_backend (IntPtr texture);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate GRBackendNative gr_backendtexture_get_backend (IntPtr texture);
		}
		private static Delegates.gr_backendtexture_get_backend gr_backendtexture_get_backend_delegate;
		internal static GRBackendNative gr_backendtexture_get_backend (IntPtr texture) =>
			(gr_backendtexture_get_backend_delegate ??= GetSymbol<Delegates.gr_backendtexture_get_backend> ("gr_backendtexture_get_backend")).Invoke (texture);
		#endif

		// bool gr_backendtexture_get_gl_textureinfo(const gr_backendtexture_t* texture, gr_gl_textureinfo_t* glInfo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool gr_backendtexture_get_gl_textureinfo (IntPtr texture, GRGlTextureInfo* glInfo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendtexture_get_gl_textureinfo (IntPtr texture, GRGlTextureInfo* glInfo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_backendtexture_get_gl_textureinfo (IntPtr texture, GRGlTextureInfo* glInfo);
		}
		private static Delegates.gr_backendtexture_get_gl_textureinfo gr_backendtexture_get_gl_textureinfo_delegate;
		internal static bool gr_backendtexture_get_gl_textureinfo (IntPtr texture, GRGlTextureInfo* glInfo) =>
			(gr_backendtexture_get_gl_textureinfo_delegate ??= GetSymbol<Delegates.gr_backendtexture_get_gl_textureinfo> ("gr_backendtexture_get_gl_textureinfo")).Invoke (texture, glInfo);
		#endif

		// int gr_backendtexture_get_height(const gr_backendtexture_t* texture)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 gr_backendtexture_get_height (IntPtr texture);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendtexture_get_height (IntPtr texture);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_backendtexture_get_height (IntPtr texture);
		}
		private static Delegates.gr_backendtexture_get_height gr_backendtexture_get_height_delegate;
		internal static Int32 gr_backendtexture_get_height (IntPtr texture) =>
			(gr_backendtexture_get_height_delegate ??= GetSymbol<Delegates.gr_backendtexture_get_height> ("gr_backendtexture_get_height")).Invoke (texture);
		#endif

		// int gr_backendtexture_get_width(const gr_backendtexture_t* texture)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 gr_backendtexture_get_width (IntPtr texture);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_backendtexture_get_width (IntPtr texture);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_backendtexture_get_width (IntPtr texture);
		}
		private static Delegates.gr_backendtexture_get_width gr_backendtexture_get_width_delegate;
		internal static Int32 gr_backendtexture_get_width (IntPtr texture) =>
			(gr_backendtexture_get_width_delegate ??= GetSymbol<Delegates.gr_backendtexture_get_width> ("gr_backendtexture_get_width")).Invoke (texture);
		#endif

		// bool gr_backendtexture_has_mipmaps(const gr_backendtexture_t* texture)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool gr_backendtexture_has_mipmaps (IntPtr texture);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendtexture_has_mipmaps (IntPtr texture);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_backendtexture_has_mipmaps (IntPtr texture);
		}
		private static Delegates.gr_backendtexture_has_mipmaps gr_backendtexture_has_mipmaps_delegate;
		internal static bool gr_backendtexture_has_mipmaps (IntPtr texture) =>
			(gr_backendtexture_has_mipmaps_delegate ??= GetSymbol<Delegates.gr_backendtexture_has_mipmaps> ("gr_backendtexture_has_mipmaps")).Invoke (texture);
		#endif

		// bool gr_backendtexture_is_valid(const gr_backendtexture_t* texture)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool gr_backendtexture_is_valid (IntPtr texture);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_backendtexture_is_valid (IntPtr texture);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_backendtexture_is_valid (IntPtr texture);
		}
		private static Delegates.gr_backendtexture_is_valid gr_backendtexture_is_valid_delegate;
		internal static bool gr_backendtexture_is_valid (IntPtr texture) =>
			(gr_backendtexture_is_valid_delegate ??= GetSymbol<Delegates.gr_backendtexture_is_valid> ("gr_backendtexture_is_valid")).Invoke (texture);
		#endif

		// gr_backendtexture_t* gr_backendtexture_new_direct3d(int width, int height, const gr_d3d_textureresourceinfo_t* d3dInfo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_backendtexture_new_direct3d (Int32 width, Int32 height, GRD3DTextureResourceInfoNative* d3dInfo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_backendtexture_new_direct3d (Int32 width, Int32 height, GRD3DTextureResourceInfoNative* d3dInfo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_backendtexture_new_direct3d (Int32 width, Int32 height, GRD3DTextureResourceInfoNative* d3dInfo);
		}
		private static Delegates.gr_backendtexture_new_direct3d gr_backendtexture_new_direct3d_delegate;
		internal static IntPtr gr_backendtexture_new_direct3d (Int32 width, Int32 height, GRD3DTextureResourceInfoNative* d3dInfo) =>
			(gr_backendtexture_new_direct3d_delegate ??= GetSymbol<Delegates.gr_backendtexture_new_direct3d> ("gr_backendtexture_new_direct3d")).Invoke (width, height, d3dInfo);
		#endif

		// gr_backendtexture_t* gr_backendtexture_new_gl(int width, int height, bool mipmapped, const gr_gl_textureinfo_t* glInfo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_backendtexture_new_gl (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRGlTextureInfo* glInfo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_backendtexture_new_gl (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRGlTextureInfo* glInfo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_backendtexture_new_gl (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRGlTextureInfo* glInfo);
		}
		private static Delegates.gr_backendtexture_new_gl gr_backendtexture_new_gl_delegate;
		internal static IntPtr gr_backendtexture_new_gl (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRGlTextureInfo* glInfo) =>
			(gr_backendtexture_new_gl_delegate ??= GetSymbol<Delegates.gr_backendtexture_new_gl> ("gr_backendtexture_new_gl")).Invoke (width, height, mipmapped, glInfo);
		#endif

		// gr_backendtexture_t* gr_backendtexture_new_metal(int width, int height, bool mipmapped, const gr_mtl_textureinfo_t* mtlInfo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_backendtexture_new_metal (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRMtlTextureInfoNative* mtlInfo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_backendtexture_new_metal (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRMtlTextureInfoNative* mtlInfo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_backendtexture_new_metal (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRMtlTextureInfoNative* mtlInfo);
		}
		private static Delegates.gr_backendtexture_new_metal gr_backendtexture_new_metal_delegate;
		internal static IntPtr gr_backendtexture_new_metal (Int32 width, Int32 height, [MarshalAs (UnmanagedType.I1)] bool mipmapped, GRMtlTextureInfoNative* mtlInfo) =>
			(gr_backendtexture_new_metal_delegate ??= GetSymbol<Delegates.gr_backendtexture_new_metal> ("gr_backendtexture_new_metal")).Invoke (width, height, mipmapped, mtlInfo);
		#endif

		// gr_backendtexture_t* gr_backendtexture_new_vulkan(int width, int height, const gr_vk_imageinfo_t* vkInfo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_backendtexture_new_vulkan (Int32 width, Int32 height, GRVkImageInfo* vkInfo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_backendtexture_new_vulkan (Int32 width, Int32 height, GRVkImageInfo* vkInfo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_backendtexture_new_vulkan (Int32 width, Int32 height, GRVkImageInfo* vkInfo);
		}
		private static Delegates.gr_backendtexture_new_vulkan gr_backendtexture_new_vulkan_delegate;
		internal static IntPtr gr_backendtexture_new_vulkan (Int32 width, Int32 height, GRVkImageInfo* vkInfo) =>
			(gr_backendtexture_new_vulkan_delegate ??= GetSymbol<Delegates.gr_backendtexture_new_vulkan> ("gr_backendtexture_new_vulkan")).Invoke (width, height, vkInfo);
		#endif

		// void gr_direct_context_abandon_context(gr_direct_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_abandon_context (IntPtr context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_abandon_context (IntPtr context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_abandon_context (IntPtr context);
		}
		private static Delegates.gr_direct_context_abandon_context gr_direct_context_abandon_context_delegate;
		internal static void gr_direct_context_abandon_context (IntPtr context) =>
			(gr_direct_context_abandon_context_delegate ??= GetSymbol<Delegates.gr_direct_context_abandon_context> ("gr_direct_context_abandon_context")).Invoke (context);
		#endif

		// void gr_direct_context_check_async_work_completion(gr_direct_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_check_async_work_completion (IntPtr context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_check_async_work_completion (IntPtr context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_check_async_work_completion (IntPtr context);
		}
		private static Delegates.gr_direct_context_check_async_work_completion gr_direct_context_check_async_work_completion_delegate;
		internal static void gr_direct_context_check_async_work_completion (IntPtr context) =>
			(gr_direct_context_check_async_work_completion_delegate ??= GetSymbol<Delegates.gr_direct_context_check_async_work_completion> ("gr_direct_context_check_async_work_completion")).Invoke (context);
		#endif

		// void gr_direct_context_dump_memory_statistics(const gr_direct_context_t* context, sk_tracememorydump_t* dump)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_dump_memory_statistics (IntPtr context, IntPtr dump);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_dump_memory_statistics (IntPtr context, IntPtr dump);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_dump_memory_statistics (IntPtr context, IntPtr dump);
		}
		private static Delegates.gr_direct_context_dump_memory_statistics gr_direct_context_dump_memory_statistics_delegate;
		internal static void gr_direct_context_dump_memory_statistics (IntPtr context, IntPtr dump) =>
			(gr_direct_context_dump_memory_statistics_delegate ??= GetSymbol<Delegates.gr_direct_context_dump_memory_statistics> ("gr_direct_context_dump_memory_statistics")).Invoke (context, dump);
		#endif

		// void gr_direct_context_flush(gr_direct_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_flush (IntPtr context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_flush (IntPtr context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_flush (IntPtr context);
		}
		private static Delegates.gr_direct_context_flush gr_direct_context_flush_delegate;
		internal static void gr_direct_context_flush (IntPtr context) =>
			(gr_direct_context_flush_delegate ??= GetSymbol<Delegates.gr_direct_context_flush> ("gr_direct_context_flush")).Invoke (context);
		#endif

		// void gr_direct_context_flush_and_submit(gr_direct_context_t* context, bool syncCpu)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_flush_and_submit (IntPtr context, [MarshalAs (UnmanagedType.I1)] bool syncCpu);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_flush_and_submit (IntPtr context, [MarshalAs (UnmanagedType.I1)] bool syncCpu);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_flush_and_submit (IntPtr context, [MarshalAs (UnmanagedType.I1)] bool syncCpu);
		}
		private static Delegates.gr_direct_context_flush_and_submit gr_direct_context_flush_and_submit_delegate;
		internal static void gr_direct_context_flush_and_submit (IntPtr context, [MarshalAs (UnmanagedType.I1)] bool syncCpu) =>
			(gr_direct_context_flush_and_submit_delegate ??= GetSymbol<Delegates.gr_direct_context_flush_and_submit> ("gr_direct_context_flush_and_submit")).Invoke (context, syncCpu);
		#endif

		// void gr_direct_context_flush_image(gr_direct_context_t* context, const sk_image_t* image)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_flush_image (IntPtr context, IntPtr image);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_flush_image (IntPtr context, IntPtr image);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_flush_image (IntPtr context, IntPtr image);
		}
		private static Delegates.gr_direct_context_flush_image gr_direct_context_flush_image_delegate;
		internal static void gr_direct_context_flush_image (IntPtr context, IntPtr image) =>
			(gr_direct_context_flush_image_delegate ??= GetSymbol<Delegates.gr_direct_context_flush_image> ("gr_direct_context_flush_image")).Invoke (context, image);
		#endif

		// void gr_direct_context_flush_surface(gr_direct_context_t* context, sk_surface_t* surface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_flush_surface (IntPtr context, IntPtr surface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_flush_surface (IntPtr context, IntPtr surface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_flush_surface (IntPtr context, IntPtr surface);
		}
		private static Delegates.gr_direct_context_flush_surface gr_direct_context_flush_surface_delegate;
		internal static void gr_direct_context_flush_surface (IntPtr context, IntPtr surface) =>
			(gr_direct_context_flush_surface_delegate ??= GetSymbol<Delegates.gr_direct_context_flush_surface> ("gr_direct_context_flush_surface")).Invoke (context, surface);
		#endif

		// void gr_direct_context_free_gpu_resources(gr_direct_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_free_gpu_resources (IntPtr context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_free_gpu_resources (IntPtr context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_free_gpu_resources (IntPtr context);
		}
		private static Delegates.gr_direct_context_free_gpu_resources gr_direct_context_free_gpu_resources_delegate;
		internal static void gr_direct_context_free_gpu_resources (IntPtr context) =>
			(gr_direct_context_free_gpu_resources_delegate ??= GetSymbol<Delegates.gr_direct_context_free_gpu_resources> ("gr_direct_context_free_gpu_resources")).Invoke (context);
		#endif

		// size_t gr_direct_context_get_resource_cache_limit(gr_direct_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr gr_direct_context_get_resource_cache_limit (IntPtr context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr gr_direct_context_get_resource_cache_limit (IntPtr context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr gr_direct_context_get_resource_cache_limit (IntPtr context);
		}
		private static Delegates.gr_direct_context_get_resource_cache_limit gr_direct_context_get_resource_cache_limit_delegate;
		internal static /* size_t */ IntPtr gr_direct_context_get_resource_cache_limit (IntPtr context) =>
			(gr_direct_context_get_resource_cache_limit_delegate ??= GetSymbol<Delegates.gr_direct_context_get_resource_cache_limit> ("gr_direct_context_get_resource_cache_limit")).Invoke (context);
		#endif

		// void gr_direct_context_get_resource_cache_usage(gr_direct_context_t* context, int* maxResources, size_t* maxResourceBytes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_get_resource_cache_usage (IntPtr context, Int32* maxResources, /* size_t */ IntPtr* maxResourceBytes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_get_resource_cache_usage (IntPtr context, Int32* maxResources, /* size_t */ IntPtr* maxResourceBytes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_get_resource_cache_usage (IntPtr context, Int32* maxResources, /* size_t */ IntPtr* maxResourceBytes);
		}
		private static Delegates.gr_direct_context_get_resource_cache_usage gr_direct_context_get_resource_cache_usage_delegate;
		internal static void gr_direct_context_get_resource_cache_usage (IntPtr context, Int32* maxResources, /* size_t */ IntPtr* maxResourceBytes) =>
			(gr_direct_context_get_resource_cache_usage_delegate ??= GetSymbol<Delegates.gr_direct_context_get_resource_cache_usage> ("gr_direct_context_get_resource_cache_usage")).Invoke (context, maxResources, maxResourceBytes);
		#endif

		// bool gr_direct_context_is_abandoned(gr_direct_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool gr_direct_context_is_abandoned (IntPtr context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_direct_context_is_abandoned (IntPtr context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_direct_context_is_abandoned (IntPtr context);
		}
		private static Delegates.gr_direct_context_is_abandoned gr_direct_context_is_abandoned_delegate;
		internal static bool gr_direct_context_is_abandoned (IntPtr context) =>
			(gr_direct_context_is_abandoned_delegate ??= GetSymbol<Delegates.gr_direct_context_is_abandoned> ("gr_direct_context_is_abandoned")).Invoke (context);
		#endif

		// gr_direct_context_t* gr_direct_context_make_direct3d(const gr_d3d_backendcontext_t d3dBackendContext)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_direct_context_make_direct3d (GRD3DBackendContextNative d3dBackendContext);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_direct_context_make_direct3d (GRD3DBackendContextNative d3dBackendContext);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_direct_context_make_direct3d (GRD3DBackendContextNative d3dBackendContext);
		}
		private static Delegates.gr_direct_context_make_direct3d gr_direct_context_make_direct3d_delegate;
		internal static IntPtr gr_direct_context_make_direct3d (GRD3DBackendContextNative d3dBackendContext) =>
			(gr_direct_context_make_direct3d_delegate ??= GetSymbol<Delegates.gr_direct_context_make_direct3d> ("gr_direct_context_make_direct3d")).Invoke (d3dBackendContext);
		#endif

		// gr_direct_context_t* gr_direct_context_make_direct3d_with_options(const gr_d3d_backendcontext_t d3dBackendContext, const gr_context_options_t* options)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_direct_context_make_direct3d_with_options (GRD3DBackendContextNative d3dBackendContext, GRContextOptionsNative* options);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_direct_context_make_direct3d_with_options (GRD3DBackendContextNative d3dBackendContext, GRContextOptionsNative* options);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_direct_context_make_direct3d_with_options (GRD3DBackendContextNative d3dBackendContext, GRContextOptionsNative* options);
		}
		private static Delegates.gr_direct_context_make_direct3d_with_options gr_direct_context_make_direct3d_with_options_delegate;
		internal static IntPtr gr_direct_context_make_direct3d_with_options (GRD3DBackendContextNative d3dBackendContext, GRContextOptionsNative* options) =>
			(gr_direct_context_make_direct3d_with_options_delegate ??= GetSymbol<Delegates.gr_direct_context_make_direct3d_with_options> ("gr_direct_context_make_direct3d_with_options")).Invoke (d3dBackendContext, options);
		#endif

		// gr_direct_context_t* gr_direct_context_make_gl(const gr_glinterface_t* glInterface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_direct_context_make_gl (IntPtr glInterface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_direct_context_make_gl (IntPtr glInterface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_direct_context_make_gl (IntPtr glInterface);
		}
		private static Delegates.gr_direct_context_make_gl gr_direct_context_make_gl_delegate;
		internal static IntPtr gr_direct_context_make_gl (IntPtr glInterface) =>
			(gr_direct_context_make_gl_delegate ??= GetSymbol<Delegates.gr_direct_context_make_gl> ("gr_direct_context_make_gl")).Invoke (glInterface);
		#endif

		// gr_direct_context_t* gr_direct_context_make_gl_with_options(const gr_glinterface_t* glInterface, const gr_context_options_t* options)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_direct_context_make_gl_with_options (IntPtr glInterface, GRContextOptionsNative* options);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_direct_context_make_gl_with_options (IntPtr glInterface, GRContextOptionsNative* options);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_direct_context_make_gl_with_options (IntPtr glInterface, GRContextOptionsNative* options);
		}
		private static Delegates.gr_direct_context_make_gl_with_options gr_direct_context_make_gl_with_options_delegate;
		internal static IntPtr gr_direct_context_make_gl_with_options (IntPtr glInterface, GRContextOptionsNative* options) =>
			(gr_direct_context_make_gl_with_options_delegate ??= GetSymbol<Delegates.gr_direct_context_make_gl_with_options> ("gr_direct_context_make_gl_with_options")).Invoke (glInterface, options);
		#endif

		// gr_direct_context_t* gr_direct_context_make_metal(void* device, void* queue)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_direct_context_make_metal (void* device, void* queue);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_direct_context_make_metal (void* device, void* queue);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_direct_context_make_metal (void* device, void* queue);
		}
		private static Delegates.gr_direct_context_make_metal gr_direct_context_make_metal_delegate;
		internal static IntPtr gr_direct_context_make_metal (void* device, void* queue) =>
			(gr_direct_context_make_metal_delegate ??= GetSymbol<Delegates.gr_direct_context_make_metal> ("gr_direct_context_make_metal")).Invoke (device, queue);
		#endif

		// gr_direct_context_t* gr_direct_context_make_metal_with_options(void* device, void* queue, const gr_context_options_t* options)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_direct_context_make_metal_with_options (void* device, void* queue, GRContextOptionsNative* options);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_direct_context_make_metal_with_options (void* device, void* queue, GRContextOptionsNative* options);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_direct_context_make_metal_with_options (void* device, void* queue, GRContextOptionsNative* options);
		}
		private static Delegates.gr_direct_context_make_metal_with_options gr_direct_context_make_metal_with_options_delegate;
		internal static IntPtr gr_direct_context_make_metal_with_options (void* device, void* queue, GRContextOptionsNative* options) =>
			(gr_direct_context_make_metal_with_options_delegate ??= GetSymbol<Delegates.gr_direct_context_make_metal_with_options> ("gr_direct_context_make_metal_with_options")).Invoke (device, queue, options);
		#endif

		// gr_direct_context_t* gr_direct_context_make_vulkan(const gr_vk_backendcontext_t vkBackendContext)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_direct_context_make_vulkan (GRVkBackendContextNative vkBackendContext);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_direct_context_make_vulkan (GRVkBackendContextNative vkBackendContext);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_direct_context_make_vulkan (GRVkBackendContextNative vkBackendContext);
		}
		private static Delegates.gr_direct_context_make_vulkan gr_direct_context_make_vulkan_delegate;
		internal static IntPtr gr_direct_context_make_vulkan (GRVkBackendContextNative vkBackendContext) =>
			(gr_direct_context_make_vulkan_delegate ??= GetSymbol<Delegates.gr_direct_context_make_vulkan> ("gr_direct_context_make_vulkan")).Invoke (vkBackendContext);
		#endif

		// gr_direct_context_t* gr_direct_context_make_vulkan_with_options(const gr_vk_backendcontext_t vkBackendContext, const gr_context_options_t* options)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_direct_context_make_vulkan_with_options (GRVkBackendContextNative vkBackendContext, GRContextOptionsNative* options);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_direct_context_make_vulkan_with_options (GRVkBackendContextNative vkBackendContext, GRContextOptionsNative* options);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_direct_context_make_vulkan_with_options (GRVkBackendContextNative vkBackendContext, GRContextOptionsNative* options);
		}
		private static Delegates.gr_direct_context_make_vulkan_with_options gr_direct_context_make_vulkan_with_options_delegate;
		internal static IntPtr gr_direct_context_make_vulkan_with_options (GRVkBackendContextNative vkBackendContext, GRContextOptionsNative* options) =>
			(gr_direct_context_make_vulkan_with_options_delegate ??= GetSymbol<Delegates.gr_direct_context_make_vulkan_with_options> ("gr_direct_context_make_vulkan_with_options")).Invoke (vkBackendContext, options);
		#endif

		// void gr_direct_context_perform_deferred_cleanup(gr_direct_context_t* context, long long ms)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_perform_deferred_cleanup (IntPtr context, Int64 ms);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_perform_deferred_cleanup (IntPtr context, Int64 ms);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_perform_deferred_cleanup (IntPtr context, Int64 ms);
		}
		private static Delegates.gr_direct_context_perform_deferred_cleanup gr_direct_context_perform_deferred_cleanup_delegate;
		internal static void gr_direct_context_perform_deferred_cleanup (IntPtr context, Int64 ms) =>
			(gr_direct_context_perform_deferred_cleanup_delegate ??= GetSymbol<Delegates.gr_direct_context_perform_deferred_cleanup> ("gr_direct_context_perform_deferred_cleanup")).Invoke (context, ms);
		#endif

		// void gr_direct_context_purge_unlocked_resources(gr_direct_context_t* context, bool scratchResourcesOnly)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_purge_unlocked_resources (IntPtr context, [MarshalAs (UnmanagedType.I1)] bool scratchResourcesOnly);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_purge_unlocked_resources (IntPtr context, [MarshalAs (UnmanagedType.I1)] bool scratchResourcesOnly);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_purge_unlocked_resources (IntPtr context, [MarshalAs (UnmanagedType.I1)] bool scratchResourcesOnly);
		}
		private static Delegates.gr_direct_context_purge_unlocked_resources gr_direct_context_purge_unlocked_resources_delegate;
		internal static void gr_direct_context_purge_unlocked_resources (IntPtr context, [MarshalAs (UnmanagedType.I1)] bool scratchResourcesOnly) =>
			(gr_direct_context_purge_unlocked_resources_delegate ??= GetSymbol<Delegates.gr_direct_context_purge_unlocked_resources> ("gr_direct_context_purge_unlocked_resources")).Invoke (context, scratchResourcesOnly);
		#endif

		// void gr_direct_context_purge_unlocked_resources_bytes(gr_direct_context_t* context, size_t bytesToPurge, bool preferScratchResources)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_purge_unlocked_resources_bytes (IntPtr context, /* size_t */ IntPtr bytesToPurge, [MarshalAs (UnmanagedType.I1)] bool preferScratchResources);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_purge_unlocked_resources_bytes (IntPtr context, /* size_t */ IntPtr bytesToPurge, [MarshalAs (UnmanagedType.I1)] bool preferScratchResources);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_purge_unlocked_resources_bytes (IntPtr context, /* size_t */ IntPtr bytesToPurge, [MarshalAs (UnmanagedType.I1)] bool preferScratchResources);
		}
		private static Delegates.gr_direct_context_purge_unlocked_resources_bytes gr_direct_context_purge_unlocked_resources_bytes_delegate;
		internal static void gr_direct_context_purge_unlocked_resources_bytes (IntPtr context, /* size_t */ IntPtr bytesToPurge, [MarshalAs (UnmanagedType.I1)] bool preferScratchResources) =>
			(gr_direct_context_purge_unlocked_resources_bytes_delegate ??= GetSymbol<Delegates.gr_direct_context_purge_unlocked_resources_bytes> ("gr_direct_context_purge_unlocked_resources_bytes")).Invoke (context, bytesToPurge, preferScratchResources);
		#endif

		// void gr_direct_context_release_resources_and_abandon_context(gr_direct_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_release_resources_and_abandon_context (IntPtr context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_release_resources_and_abandon_context (IntPtr context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_release_resources_and_abandon_context (IntPtr context);
		}
		private static Delegates.gr_direct_context_release_resources_and_abandon_context gr_direct_context_release_resources_and_abandon_context_delegate;
		internal static void gr_direct_context_release_resources_and_abandon_context (IntPtr context) =>
			(gr_direct_context_release_resources_and_abandon_context_delegate ??= GetSymbol<Delegates.gr_direct_context_release_resources_and_abandon_context> ("gr_direct_context_release_resources_and_abandon_context")).Invoke (context);
		#endif

		// void gr_direct_context_reset_context(gr_direct_context_t* context, uint32_t state)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_reset_context (IntPtr context, UInt32 state);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_reset_context (IntPtr context, UInt32 state);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_reset_context (IntPtr context, UInt32 state);
		}
		private static Delegates.gr_direct_context_reset_context gr_direct_context_reset_context_delegate;
		internal static void gr_direct_context_reset_context (IntPtr context, UInt32 state) =>
			(gr_direct_context_reset_context_delegate ??= GetSymbol<Delegates.gr_direct_context_reset_context> ("gr_direct_context_reset_context")).Invoke (context, state);
		#endif

		// void gr_direct_context_set_resource_cache_limit(gr_direct_context_t* context, size_t maxResourceBytes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_direct_context_set_resource_cache_limit (IntPtr context, /* size_t */ IntPtr maxResourceBytes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_direct_context_set_resource_cache_limit (IntPtr context, /* size_t */ IntPtr maxResourceBytes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_direct_context_set_resource_cache_limit (IntPtr context, /* size_t */ IntPtr maxResourceBytes);
		}
		private static Delegates.gr_direct_context_set_resource_cache_limit gr_direct_context_set_resource_cache_limit_delegate;
		internal static void gr_direct_context_set_resource_cache_limit (IntPtr context, /* size_t */ IntPtr maxResourceBytes) =>
			(gr_direct_context_set_resource_cache_limit_delegate ??= GetSymbol<Delegates.gr_direct_context_set_resource_cache_limit> ("gr_direct_context_set_resource_cache_limit")).Invoke (context, maxResourceBytes);
		#endif

		// bool gr_direct_context_submit(gr_direct_context_t* context, bool syncCpu)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool gr_direct_context_submit (IntPtr context, [MarshalAs (UnmanagedType.I1)] bool syncCpu);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_direct_context_submit (IntPtr context, [MarshalAs (UnmanagedType.I1)] bool syncCpu);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_direct_context_submit (IntPtr context, [MarshalAs (UnmanagedType.I1)] bool syncCpu);
		}
		private static Delegates.gr_direct_context_submit gr_direct_context_submit_delegate;
		internal static bool gr_direct_context_submit (IntPtr context, [MarshalAs (UnmanagedType.I1)] bool syncCpu) =>
			(gr_direct_context_submit_delegate ??= GetSymbol<Delegates.gr_direct_context_submit> ("gr_direct_context_submit")).Invoke (context, syncCpu);
		#endif

		// const gr_glinterface_t* gr_glinterface_assemble_gl_interface(void* ctx, gr_gl_get_proc get)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_glinterface_assemble_gl_interface (void* ctx, void* get);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_glinterface_assemble_gl_interface (void* ctx, GRGlGetProcProxyDelegate get);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_glinterface_assemble_gl_interface (void* ctx, GRGlGetProcProxyDelegate get);
		}
		private static Delegates.gr_glinterface_assemble_gl_interface gr_glinterface_assemble_gl_interface_delegate;
		internal static IntPtr gr_glinterface_assemble_gl_interface (void* ctx, GRGlGetProcProxyDelegate get) =>
			(gr_glinterface_assemble_gl_interface_delegate ??= GetSymbol<Delegates.gr_glinterface_assemble_gl_interface> ("gr_glinterface_assemble_gl_interface")).Invoke (ctx, get);
		#endif

		// const gr_glinterface_t* gr_glinterface_assemble_gles_interface(void* ctx, gr_gl_get_proc get)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_glinterface_assemble_gles_interface (void* ctx, void* get);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_glinterface_assemble_gles_interface (void* ctx, GRGlGetProcProxyDelegate get);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_glinterface_assemble_gles_interface (void* ctx, GRGlGetProcProxyDelegate get);
		}
		private static Delegates.gr_glinterface_assemble_gles_interface gr_glinterface_assemble_gles_interface_delegate;
		internal static IntPtr gr_glinterface_assemble_gles_interface (void* ctx, GRGlGetProcProxyDelegate get) =>
			(gr_glinterface_assemble_gles_interface_delegate ??= GetSymbol<Delegates.gr_glinterface_assemble_gles_interface> ("gr_glinterface_assemble_gles_interface")).Invoke (ctx, get);
		#endif

		// const gr_glinterface_t* gr_glinterface_assemble_interface(void* ctx, gr_gl_get_proc get)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_glinterface_assemble_interface (void* ctx, void* get);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_glinterface_assemble_interface (void* ctx, GRGlGetProcProxyDelegate get);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_glinterface_assemble_interface (void* ctx, GRGlGetProcProxyDelegate get);
		}
		private static Delegates.gr_glinterface_assemble_interface gr_glinterface_assemble_interface_delegate;
		internal static IntPtr gr_glinterface_assemble_interface (void* ctx, GRGlGetProcProxyDelegate get) =>
			(gr_glinterface_assemble_interface_delegate ??= GetSymbol<Delegates.gr_glinterface_assemble_interface> ("gr_glinterface_assemble_interface")).Invoke (ctx, get);
		#endif

		// const gr_glinterface_t* gr_glinterface_assemble_webgl_interface(void* ctx, gr_gl_get_proc get)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_glinterface_assemble_webgl_interface (void* ctx, void* get);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_glinterface_assemble_webgl_interface (void* ctx, GRGlGetProcProxyDelegate get);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_glinterface_assemble_webgl_interface (void* ctx, GRGlGetProcProxyDelegate get);
		}
		private static Delegates.gr_glinterface_assemble_webgl_interface gr_glinterface_assemble_webgl_interface_delegate;
		internal static IntPtr gr_glinterface_assemble_webgl_interface (void* ctx, GRGlGetProcProxyDelegate get) =>
			(gr_glinterface_assemble_webgl_interface_delegate ??= GetSymbol<Delegates.gr_glinterface_assemble_webgl_interface> ("gr_glinterface_assemble_webgl_interface")).Invoke (ctx, get);
		#endif

		// const gr_glinterface_t* gr_glinterface_create_native_interface()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_glinterface_create_native_interface ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_glinterface_create_native_interface ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_glinterface_create_native_interface ();
		}
		private static Delegates.gr_glinterface_create_native_interface gr_glinterface_create_native_interface_delegate;
		internal static IntPtr gr_glinterface_create_native_interface () =>
			(gr_glinterface_create_native_interface_delegate ??= GetSymbol<Delegates.gr_glinterface_create_native_interface> ("gr_glinterface_create_native_interface")).Invoke ();
		#endif

		// bool gr_glinterface_has_extension(const gr_glinterface_t* glInterface, const char* extension)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool gr_glinterface_has_extension (IntPtr glInterface, [MarshalAs (UnmanagedType.LPStr)] String extension);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_glinterface_has_extension (IntPtr glInterface, [MarshalAs (UnmanagedType.LPStr)] String extension);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_glinterface_has_extension (IntPtr glInterface, [MarshalAs (UnmanagedType.LPStr)] String extension);
		}
		private static Delegates.gr_glinterface_has_extension gr_glinterface_has_extension_delegate;
		internal static bool gr_glinterface_has_extension (IntPtr glInterface, [MarshalAs (UnmanagedType.LPStr)] String extension) =>
			(gr_glinterface_has_extension_delegate ??= GetSymbol<Delegates.gr_glinterface_has_extension> ("gr_glinterface_has_extension")).Invoke (glInterface, extension);
		#endif

		// void gr_glinterface_unref(const gr_glinterface_t* glInterface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_glinterface_unref (IntPtr glInterface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_glinterface_unref (IntPtr glInterface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_glinterface_unref (IntPtr glInterface);
		}
		private static Delegates.gr_glinterface_unref gr_glinterface_unref_delegate;
		internal static void gr_glinterface_unref (IntPtr glInterface) =>
			(gr_glinterface_unref_delegate ??= GetSymbol<Delegates.gr_glinterface_unref> ("gr_glinterface_unref")).Invoke (glInterface);
		#endif

		// bool gr_glinterface_validate(const gr_glinterface_t* glInterface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool gr_glinterface_validate (IntPtr glInterface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_glinterface_validate (IntPtr glInterface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_glinterface_validate (IntPtr glInterface);
		}
		private static Delegates.gr_glinterface_validate gr_glinterface_validate_delegate;
		internal static bool gr_glinterface_validate (IntPtr glInterface) =>
			(gr_glinterface_validate_delegate ??= GetSymbol<Delegates.gr_glinterface_validate> ("gr_glinterface_validate")).Invoke (glInterface);
		#endif

		// gr_backend_t gr_recording_context_get_backend(gr_recording_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial GRBackendNative gr_recording_context_get_backend (IntPtr context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GRBackendNative gr_recording_context_get_backend (IntPtr context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate GRBackendNative gr_recording_context_get_backend (IntPtr context);
		}
		private static Delegates.gr_recording_context_get_backend gr_recording_context_get_backend_delegate;
		internal static GRBackendNative gr_recording_context_get_backend (IntPtr context) =>
			(gr_recording_context_get_backend_delegate ??= GetSymbol<Delegates.gr_recording_context_get_backend> ("gr_recording_context_get_backend")).Invoke (context);
		#endif

		// gr_direct_context_t* gr_recording_context_get_direct_context(gr_recording_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_recording_context_get_direct_context (IntPtr context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_recording_context_get_direct_context (IntPtr context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_recording_context_get_direct_context (IntPtr context);
		}
		private static Delegates.gr_recording_context_get_direct_context gr_recording_context_get_direct_context_delegate;
		internal static IntPtr gr_recording_context_get_direct_context (IntPtr context) =>
			(gr_recording_context_get_direct_context_delegate ??= GetSymbol<Delegates.gr_recording_context_get_direct_context> ("gr_recording_context_get_direct_context")).Invoke (context);
		#endif

		// int gr_recording_context_get_max_surface_sample_count_for_color_type(gr_recording_context_t* context, sk_colortype_t colorType)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 gr_recording_context_get_max_surface_sample_count_for_color_type (IntPtr context, SKColorTypeNative colorType);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_recording_context_get_max_surface_sample_count_for_color_type (IntPtr context, SKColorTypeNative colorType);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_recording_context_get_max_surface_sample_count_for_color_type (IntPtr context, SKColorTypeNative colorType);
		}
		private static Delegates.gr_recording_context_get_max_surface_sample_count_for_color_type gr_recording_context_get_max_surface_sample_count_for_color_type_delegate;
		internal static Int32 gr_recording_context_get_max_surface_sample_count_for_color_type (IntPtr context, SKColorTypeNative colorType) =>
			(gr_recording_context_get_max_surface_sample_count_for_color_type_delegate ??= GetSymbol<Delegates.gr_recording_context_get_max_surface_sample_count_for_color_type> ("gr_recording_context_get_max_surface_sample_count_for_color_type")).Invoke (context, colorType);
		#endif

		// bool gr_recording_context_is_abandoned(gr_recording_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool gr_recording_context_is_abandoned (IntPtr context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_recording_context_is_abandoned (IntPtr context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_recording_context_is_abandoned (IntPtr context);
		}
		private static Delegates.gr_recording_context_is_abandoned gr_recording_context_is_abandoned_delegate;
		internal static bool gr_recording_context_is_abandoned (IntPtr context) =>
			(gr_recording_context_is_abandoned_delegate ??= GetSymbol<Delegates.gr_recording_context_is_abandoned> ("gr_recording_context_is_abandoned")).Invoke (context);
		#endif

		// int gr_recording_context_max_render_target_size(gr_recording_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 gr_recording_context_max_render_target_size (IntPtr context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_recording_context_max_render_target_size (IntPtr context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_recording_context_max_render_target_size (IntPtr context);
		}
		private static Delegates.gr_recording_context_max_render_target_size gr_recording_context_max_render_target_size_delegate;
		internal static Int32 gr_recording_context_max_render_target_size (IntPtr context) =>
			(gr_recording_context_max_render_target_size_delegate ??= GetSymbol<Delegates.gr_recording_context_max_render_target_size> ("gr_recording_context_max_render_target_size")).Invoke (context);
		#endif

		// int gr_recording_context_max_texture_size(gr_recording_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 gr_recording_context_max_texture_size (IntPtr context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 gr_recording_context_max_texture_size (IntPtr context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 gr_recording_context_max_texture_size (IntPtr context);
		}
		private static Delegates.gr_recording_context_max_texture_size gr_recording_context_max_texture_size_delegate;
		internal static Int32 gr_recording_context_max_texture_size (IntPtr context) =>
			(gr_recording_context_max_texture_size_delegate ??= GetSymbol<Delegates.gr_recording_context_max_texture_size> ("gr_recording_context_max_texture_size")).Invoke (context);
		#endif

		// void gr_recording_context_unref(gr_recording_context_t* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_recording_context_unref (IntPtr context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_recording_context_unref (IntPtr context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_recording_context_unref (IntPtr context);
		}
		private static Delegates.gr_recording_context_unref gr_recording_context_unref_delegate;
		internal static void gr_recording_context_unref (IntPtr context) =>
			(gr_recording_context_unref_delegate ??= GetSymbol<Delegates.gr_recording_context_unref> ("gr_recording_context_unref")).Invoke (context);
		#endif

		// void gr_vk_extensions_delete(gr_vk_extensions_t* extensions)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_vk_extensions_delete (IntPtr extensions);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_vk_extensions_delete (IntPtr extensions);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_vk_extensions_delete (IntPtr extensions);
		}
		private static Delegates.gr_vk_extensions_delete gr_vk_extensions_delete_delegate;
		internal static void gr_vk_extensions_delete (IntPtr extensions) =>
			(gr_vk_extensions_delete_delegate ??= GetSymbol<Delegates.gr_vk_extensions_delete> ("gr_vk_extensions_delete")).Invoke (extensions);
		#endif

		// bool gr_vk_extensions_has_extension(gr_vk_extensions_t* extensions, const char* ext, uint32_t minVersion)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool gr_vk_extensions_has_extension (IntPtr extensions, [MarshalAs (UnmanagedType.LPStr)] String ext, UInt32 minVersion);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool gr_vk_extensions_has_extension (IntPtr extensions, [MarshalAs (UnmanagedType.LPStr)] String ext, UInt32 minVersion);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool gr_vk_extensions_has_extension (IntPtr extensions, [MarshalAs (UnmanagedType.LPStr)] String ext, UInt32 minVersion);
		}
		private static Delegates.gr_vk_extensions_has_extension gr_vk_extensions_has_extension_delegate;
		internal static bool gr_vk_extensions_has_extension (IntPtr extensions, [MarshalAs (UnmanagedType.LPStr)] String ext, UInt32 minVersion) =>
			(gr_vk_extensions_has_extension_delegate ??= GetSymbol<Delegates.gr_vk_extensions_has_extension> ("gr_vk_extensions_has_extension")).Invoke (extensions, ext, minVersion);
		#endif

		// void gr_vk_extensions_init(gr_vk_extensions_t* extensions, gr_vk_get_proc getProc, void* userData, vk_instance_t* instance, vk_physical_device_t* physDev, uint32_t instanceExtensionCount, const char** instanceExtensions, uint32_t deviceExtensionCount, const char** deviceExtensions)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_vk_extensions_init (IntPtr extensions, void* getProc, void* userData, IntPtr instance, IntPtr physDev, UInt32 instanceExtensionCount, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] instanceExtensions, UInt32 deviceExtensionCount, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] deviceExtensions);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_vk_extensions_init (IntPtr extensions, GRVkGetProcProxyDelegate getProc, void* userData, IntPtr instance, IntPtr physDev, UInt32 instanceExtensionCount, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] instanceExtensions, UInt32 deviceExtensionCount, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] deviceExtensions);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_vk_extensions_init (IntPtr extensions, GRVkGetProcProxyDelegate getProc, void* userData, IntPtr instance, IntPtr physDev, UInt32 instanceExtensionCount, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] instanceExtensions, UInt32 deviceExtensionCount, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] deviceExtensions);
		}
		private static Delegates.gr_vk_extensions_init gr_vk_extensions_init_delegate;
		internal static void gr_vk_extensions_init (IntPtr extensions, GRVkGetProcProxyDelegate getProc, void* userData, IntPtr instance, IntPtr physDev, UInt32 instanceExtensionCount, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] instanceExtensions, UInt32 deviceExtensionCount, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] deviceExtensions) =>
			(gr_vk_extensions_init_delegate ??= GetSymbol<Delegates.gr_vk_extensions_init> ("gr_vk_extensions_init")).Invoke (extensions, getProc, userData, instance, physDev, instanceExtensionCount, instanceExtensions, deviceExtensionCount, deviceExtensions);
		#endif

		// gr_vk_extensions_t* gr_vk_extensions_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr gr_vk_extensions_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gr_vk_extensions_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr gr_vk_extensions_new ();
		}
		private static Delegates.gr_vk_extensions_new gr_vk_extensions_new_delegate;
		internal static IntPtr gr_vk_extensions_new () =>
			(gr_vk_extensions_new_delegate ??= GetSymbol<Delegates.gr_vk_extensions_new> ("gr_vk_extensions_new")).Invoke ();
		#endif

		#endregion

	}
}

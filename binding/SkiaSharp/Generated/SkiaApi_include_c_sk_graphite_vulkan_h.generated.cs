using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_graphite_vulkan.h

		// sk_graphite_context_t* sk_graphite_context_make_vulkan(const sk_graphite_vk_backend_context_init_t init, const sk_graphite_context_options_t* opts)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_graphite_context_t sk_graphite_context_make_vulkan (SKGraphiteVkBackendContextNative init, SKGraphiteContextOptions* opts);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_graphite_context_t sk_graphite_context_make_vulkan (SKGraphiteVkBackendContextNative init, SKGraphiteContextOptions* opts);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_graphite_context_t sk_graphite_context_make_vulkan (SKGraphiteVkBackendContextNative init, SKGraphiteContextOptions* opts);
		}
		private static Delegates.sk_graphite_context_make_vulkan sk_graphite_context_make_vulkan_delegate;
		internal static sk_graphite_context_t sk_graphite_context_make_vulkan (SKGraphiteVkBackendContextNative init, SKGraphiteContextOptions* opts) =>
			(sk_graphite_context_make_vulkan_delegate ??= GetSymbol<Delegates.sk_graphite_context_make_vulkan> ("sk_graphite_context_make_vulkan")).Invoke (init, opts);
		#endif

		// sk_graphite_backend_semaphore_t* sk_graphite_vk_backend_semaphore_new(uint64_t vkSemaphore)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_graphite_backend_semaphore_t sk_graphite_vk_backend_semaphore_new (UInt64 vkSemaphore);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_graphite_backend_semaphore_t sk_graphite_vk_backend_semaphore_new (UInt64 vkSemaphore);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_graphite_backend_semaphore_t sk_graphite_vk_backend_semaphore_new (UInt64 vkSemaphore);
		}
		private static Delegates.sk_graphite_vk_backend_semaphore_new sk_graphite_vk_backend_semaphore_new_delegate;
		internal static sk_graphite_backend_semaphore_t sk_graphite_vk_backend_semaphore_new (UInt64 vkSemaphore) =>
			(sk_graphite_vk_backend_semaphore_new_delegate ??= GetSymbol<Delegates.sk_graphite_vk_backend_semaphore_new> ("sk_graphite_vk_backend_semaphore_new")).Invoke (vkSemaphore);
		#endif

		// sk_graphite_backend_texture_t* sk_graphite_vk_backend_texture_new(int32_t width, int32_t height, const sk_graphite_vk_texture_info_t* info, int32_t imageLayout, uint32_t queueFamilyIndex, void* vkImage)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_graphite_backend_texture_t sk_graphite_vk_backend_texture_new (Int32 width, Int32 height, SKGraphiteVkTextureInfo* info, Int32 imageLayout, UInt32 queueFamilyIndex, void* vkImage);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_graphite_backend_texture_t sk_graphite_vk_backend_texture_new (Int32 width, Int32 height, SKGraphiteVkTextureInfo* info, Int32 imageLayout, UInt32 queueFamilyIndex, void* vkImage);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_graphite_backend_texture_t sk_graphite_vk_backend_texture_new (Int32 width, Int32 height, SKGraphiteVkTextureInfo* info, Int32 imageLayout, UInt32 queueFamilyIndex, void* vkImage);
		}
		private static Delegates.sk_graphite_vk_backend_texture_new sk_graphite_vk_backend_texture_new_delegate;
		internal static sk_graphite_backend_texture_t sk_graphite_vk_backend_texture_new (Int32 width, Int32 height, SKGraphiteVkTextureInfo* info, Int32 imageLayout, UInt32 queueFamilyIndex, void* vkImage) =>
			(sk_graphite_vk_backend_texture_new_delegate ??= GetSymbol<Delegates.sk_graphite_vk_backend_texture_new> ("sk_graphite_vk_backend_texture_new")).Invoke (width, height, info, imageLayout, queueFamilyIndex, vkImage);
		#endif

		// sk_graphite_backend_texture_t* sk_graphite_vk_backend_texture_new_uint64(int32_t width, int32_t height, const sk_graphite_vk_texture_info_t* info, int32_t imageLayout, uint32_t queueFamilyIndex, uint64_t vkImage)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_graphite_backend_texture_t sk_graphite_vk_backend_texture_new_uint64 (Int32 width, Int32 height, SKGraphiteVkTextureInfo* info, Int32 imageLayout, UInt32 queueFamilyIndex, UInt64 vkImage);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_graphite_backend_texture_t sk_graphite_vk_backend_texture_new_uint64 (Int32 width, Int32 height, SKGraphiteVkTextureInfo* info, Int32 imageLayout, UInt32 queueFamilyIndex, UInt64 vkImage);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_graphite_backend_texture_t sk_graphite_vk_backend_texture_new_uint64 (Int32 width, Int32 height, SKGraphiteVkTextureInfo* info, Int32 imageLayout, UInt32 queueFamilyIndex, UInt64 vkImage);
		}
		private static Delegates.sk_graphite_vk_backend_texture_new_uint64 sk_graphite_vk_backend_texture_new_uint64_delegate;
		internal static sk_graphite_backend_texture_t sk_graphite_vk_backend_texture_new_uint64 (Int32 width, Int32 height, SKGraphiteVkTextureInfo* info, Int32 imageLayout, UInt32 queueFamilyIndex, UInt64 vkImage) =>
			(sk_graphite_vk_backend_texture_new_uint64_delegate ??= GetSymbol<Delegates.sk_graphite_vk_backend_texture_new_uint64> ("sk_graphite_vk_backend_texture_new_uint64")).Invoke (width, height, info, imageLayout, queueFamilyIndex, vkImage);
		#endif

		// sk_graphite_mutable_texture_state_t* sk_graphite_vk_mutable_texture_state_new(int32_t imageLayout, uint32_t queueFamilyIndex)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_graphite_mutable_texture_state_t sk_graphite_vk_mutable_texture_state_new (Int32 imageLayout, UInt32 queueFamilyIndex);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_graphite_mutable_texture_state_t sk_graphite_vk_mutable_texture_state_new (Int32 imageLayout, UInt32 queueFamilyIndex);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_graphite_mutable_texture_state_t sk_graphite_vk_mutable_texture_state_new (Int32 imageLayout, UInt32 queueFamilyIndex);
		}
		private static Delegates.sk_graphite_vk_mutable_texture_state_new sk_graphite_vk_mutable_texture_state_new_delegate;
		internal static sk_graphite_mutable_texture_state_t sk_graphite_vk_mutable_texture_state_new (Int32 imageLayout, UInt32 queueFamilyIndex) =>
			(sk_graphite_vk_mutable_texture_state_new_delegate ??= GetSymbol<Delegates.sk_graphite_vk_mutable_texture_state_new> ("sk_graphite_vk_mutable_texture_state_new")).Invoke (imageLayout, queueFamilyIndex);
		#endif

		// sk_graphite_texture_info_t* sk_graphite_vk_texture_info_new(const sk_graphite_vk_texture_info_t* info)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_graphite_texture_info_t sk_graphite_vk_texture_info_new (SKGraphiteVkTextureInfo* info);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_graphite_texture_info_t sk_graphite_vk_texture_info_new (SKGraphiteVkTextureInfo* info);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_graphite_texture_info_t sk_graphite_vk_texture_info_new (SKGraphiteVkTextureInfo* info);
		}
		private static Delegates.sk_graphite_vk_texture_info_new sk_graphite_vk_texture_info_new_delegate;
		internal static sk_graphite_texture_info_t sk_graphite_vk_texture_info_new (SKGraphiteVkTextureInfo* info) =>
			(sk_graphite_vk_texture_info_new_delegate ??= GetSymbol<Delegates.sk_graphite_vk_texture_info_new> ("sk_graphite_vk_texture_info_new")).Invoke (info);
		#endif

		#endregion

	}
}

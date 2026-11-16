using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region gr_vk_device_lost.h

		// gr_vk_device_lost_handler_t* gr_vk_device_lost_handler_new(gr_vk_device_lost_proc proc, void* userData)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial gr_vk_device_lost_handler_t gr_vk_device_lost_handler_new (void* proc, void* userData);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_vk_device_lost_handler_t gr_vk_device_lost_handler_new (GRVkDeviceLostProxyDelegate proc, void* userData);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_vk_device_lost_handler_t gr_vk_device_lost_handler_new (GRVkDeviceLostProxyDelegate proc, void* userData);
		}
		private static Delegates.gr_vk_device_lost_handler_new gr_vk_device_lost_handler_new_delegate;
		internal static gr_vk_device_lost_handler_t gr_vk_device_lost_handler_new (GRVkDeviceLostProxyDelegate proc, void* userData) =>
			(gr_vk_device_lost_handler_new_delegate ??= GetSymbol<Delegates.gr_vk_device_lost_handler_new> ("gr_vk_device_lost_handler_new")).Invoke (proc, userData);
		#endif

		#endregion

	}
}

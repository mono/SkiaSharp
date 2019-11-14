using System;

namespace SkiaSharp
{
	public class GRVkInterface : SKObject, ISKReferenceCounted
	{
		[Preserve]
		internal GRVkInterface (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		public static GRVkInterface Create (
			IntPtr vkGetInstanceProcAddr,
			IntPtr vkGetDeviceProcAddr,
			IntPtr vkInstance,
			IntPtr vkDevice,
			uint extensionFlags)
		{
			return GetObject<GRVkInterface> (
				SkiaApi.gr_vkinterface_make(
					vkGetInstanceProcAddr,
					vkGetDeviceProcAddr,
					vkInstance,
					vkDevice,
					extensionFlags));
		}

		public bool Validate (uint extensionFlags)
		{
			return SkiaApi.gr_vkinterface_validate (Handle, extensionFlags);
		}
	}
}

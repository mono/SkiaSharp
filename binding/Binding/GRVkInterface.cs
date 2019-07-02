using System;

namespace SkiaSharp
{
	public class GRVkInterface : SKObject
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

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.gr_vkinterface_unref (Handle);
			}

			base.Dispose (disposing);
		}
	}
}

using System;

namespace SkiaSharp
{
	public class GRVkBackendContext : SKObject, ISKReferenceCounted
	{
		[Preserve]
		internal GRVkBackendContext (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		public static GRVkBackendContext Assemble (
			IntPtr vkInstance,
			IntPtr vkPhysicalDevice,
			IntPtr vkDevice,
			IntPtr vkQueue,
			uint graphicsQueueIndex,
			uint minAPIVersion,
			uint extensions,
			uint features,
			GRVkInterface grVkInterface)
		{
			return GetObject<GRVkBackendContext> (
				SkiaApi.gr_vkbackendcontext_assemble (
					vkInstance,
					vkPhysicalDevice,
					vkDevice,
					vkQueue,
					graphicsQueueIndex,
					minAPIVersion,
					extensions,
					features,
					grVkInterface.Handle));
		}
	}
}

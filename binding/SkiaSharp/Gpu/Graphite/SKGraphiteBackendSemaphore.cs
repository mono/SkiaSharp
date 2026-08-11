#nullable disable

using System;

namespace SkiaSharp
{
	public unsafe class SKGraphiteBackendSemaphore : SKObject
	{
		internal SKGraphiteBackendSemaphore (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public static SKGraphiteBackendSemaphore CreateVulkan (ulong vkSemaphore)
		{
			if (vkSemaphore == 0)
				throw new ArgumentOutOfRangeException (
					nameof (vkSemaphore),
					vkSemaphore,
					"Must be non-zero.");

			IntPtr handle = SkiaApi.sk_graphite_vk_backend_semaphore_new (vkSemaphore);
			return handle == IntPtr.Zero ? null : new SKGraphiteBackendSemaphore (handle, true);
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_graphite_backend_semaphore_delete (Handle);
	}
}

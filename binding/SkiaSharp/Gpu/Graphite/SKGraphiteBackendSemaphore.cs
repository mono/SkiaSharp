#nullable enable

using System;

namespace SkiaSharp
{
	/// <summary>Wraps an external GPU semaphore for Graphite recording synchronization.</summary>
	/// <remarks>Disposing this descriptor does not destroy the caller-owned GPU semaphore.</remarks>
	public unsafe class SKGraphiteBackendSemaphore : SKObject
	{
		internal SKGraphiteBackendSemaphore (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Wraps a Vulkan semaphore while preserving its complete 64-bit handle.</summary>
		/// <param name="vkSemaphore">The nonzero Vulkan semaphore handle.</param>
		/// <returns>A semaphore descriptor, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks>The caller must keep the underlying semaphore alive until its GPU use has completed.</remarks>
		public static SKGraphiteBackendSemaphore? CreateVulkan (ulong vkSemaphore)
		{
			if (vkSemaphore == 0)
				throw new ArgumentOutOfRangeException (
					nameof (vkSemaphore),
					vkSemaphore,
					"Must be non-zero.");

			IntPtr handle = SkiaApi.sk_graphite_vk_backend_semaphore_new (vkSemaphore);
			return handle == IntPtr.Zero ? null : new SKGraphiteBackendSemaphore (handle, true);
		}

		/// <summary>Releases the native semaphore descriptor.</summary>
		/// <remarks>The underlying Vulkan semaphore is not destroyed.</remarks>
		protected override void DisposeNative () =>
			SkiaApi.sk_graphite_backend_semaphore_delete (Handle);
	}
}

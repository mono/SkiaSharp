#nullable enable

using System;

namespace SkiaSharp
{
	/// <summary>Describes a backend texture state requested after Graphite recording execution.</summary>
	/// <remarks>This descriptor does not own the texture whose state will be changed.</remarks>
	public unsafe class SKGraphiteMutableTextureState : SKObject
	{
		internal SKGraphiteMutableTextureState (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Creates a descriptor for a Vulkan image layout and queue-family ownership.</summary>
		/// <param name="imageLayout">The requested Vulkan image layout.</param>
		/// <param name="queueFamilyIndex">The requested queue-family index.</param>
		/// <returns>A state descriptor, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks>For presentation, request the presentation layout and the queue family that owns the swapchain image.</remarks>
		public static SKGraphiteMutableTextureState? CreateVulkan (
			int imageLayout,
			uint queueFamilyIndex)
		{
			IntPtr handle = SkiaApi.sk_graphite_vk_mutable_texture_state_new (
				imageLayout, queueFamilyIndex);
			return handle == IntPtr.Zero ? null : new SKGraphiteMutableTextureState (handle, true);
		}

		/// <summary>Releases the native state descriptor.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_graphite_mutable_texture_state_delete (Handle);
	}
}

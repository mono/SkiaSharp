#nullable disable

using System;

namespace SkiaSharp
{
	public unsafe class SKGraphiteMutableTextureState : SKObject
	{
		internal SKGraphiteMutableTextureState (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public static SKGraphiteMutableTextureState CreateVulkan (
			int imageLayout,
			uint queueFamilyIndex)
		{
			IntPtr handle = SkiaApi.sk_graphite_vk_mutable_texture_state_new (
				imageLayout, queueFamilyIndex);
			return handle == IntPtr.Zero ? null : new SKGraphiteMutableTextureState (handle, true);
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_graphite_mutable_texture_state_delete (Handle);
	}
}

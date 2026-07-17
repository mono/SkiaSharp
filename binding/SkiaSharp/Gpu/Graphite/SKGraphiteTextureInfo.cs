#nullable disable

using System;

namespace SkiaSharp
{
	public unsafe class SKGraphiteTextureInfo : SKObject
	{
		internal SKGraphiteTextureInfo (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public static SKGraphiteTextureInfo CreateVulkan (SKGraphiteVkTextureInfo info)
		{
			IntPtr handle = SkiaApi.sk_graphite_vk_texture_info_new (&info);
			return handle == IntPtr.Zero ? null : new SKGraphiteTextureInfo (handle, true);
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_graphite_texture_info_delete (Handle);

		public bool              IsValid     => SkiaApi.sk_graphite_texture_info_is_valid (Handle);
		public SKGraphiteBackend Backend     => SkiaApi.sk_graphite_texture_info_get_backend (Handle);
		public int               SampleCount => SkiaApi.sk_graphite_texture_info_get_sample_count (Handle);
		public bool              Mipmapped   => SkiaApi.sk_graphite_texture_info_get_mipmapped (Handle);
	}
}

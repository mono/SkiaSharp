#nullable disable

using System;

namespace SkiaSharp
{
	public unsafe class SKGraphiteBackendTexture : SKObject
	{
		internal SKGraphiteBackendTexture (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public static SKGraphiteBackendTexture CreateVulkan (
			int width, int height,
			SKGraphiteVkTextureInfo info,
			int imageLayout,
			uint queueFamilyIndex,
			IntPtr vkImage)
		{
			if (vkImage == IntPtr.Zero)
				throw new ArgumentNullException (nameof (vkImage));
			if (width <= 0)
				throw new ArgumentOutOfRangeException (nameof (width), width, "Must be positive.");
			if (height <= 0)
				throw new ArgumentOutOfRangeException (nameof (height), height, "Must be positive.");
			IntPtr handle = SkiaApi.sk_graphite_vk_backend_texture_new (
				width, height, &info, imageLayout, queueFamilyIndex, (void*)vkImage);
			return handle == IntPtr.Zero ? null : new SKGraphiteBackendTexture (handle, true);
		}

		public static SKGraphiteBackendTexture CreateMetal (
			int width, int height,
			IntPtr mtlTexture)
		{
			if (mtlTexture == IntPtr.Zero)
				throw new ArgumentNullException (nameof (mtlTexture));
			if (width <= 0)
				throw new ArgumentOutOfRangeException (nameof (width), width, "Must be positive.");
			if (height <= 0)
				throw new ArgumentOutOfRangeException (nameof (height), height, "Must be positive.");
			IntPtr handle = SkiaApi.sk_graphite_mtl_backend_texture_new (
				width, height, (void*)mtlTexture);
			return handle == IntPtr.Zero ? null : new SKGraphiteBackendTexture (handle, true);
		}

		public static SKGraphiteBackendTexture CreateDawn (IntPtr wgpuTexture)
		{
			if (wgpuTexture == IntPtr.Zero)
				throw new ArgumentNullException (nameof (wgpuTexture));
			IntPtr handle = SkiaApi.sk_graphite_dawn_backend_texture_new ((void*)wgpuTexture);
			return handle == IntPtr.Zero ? null : new SKGraphiteBackendTexture (handle, true);
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_graphite_backend_texture_delete (Handle);

		public bool              IsValid => SkiaApi.sk_graphite_backend_texture_is_valid (Handle);
		public SKGraphiteBackend Backend => SkiaApi.sk_graphite_backend_texture_get_backend (Handle);

		public SKSizeI Dimensions {
			get {
				int w, h;
				SkiaApi.sk_graphite_backend_texture_get_dimensions (Handle, &w, &h);
				return new SKSizeI (w, h);
			}
		}
	}
}

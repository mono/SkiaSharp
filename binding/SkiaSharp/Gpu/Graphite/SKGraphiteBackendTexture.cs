#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// Wraps a caller-allocated GPU texture (e.g. a <c>VkImage</c> from a
	/// swap chain) so Graphite can draw into it or sample from it. The
	/// underlying GPU object is NOT freed by <c>Dispose</c> — only this
	/// wrapper. Caller retains ownership of the GPU texture for its full
	/// Vulkan/Metal/Dawn lifetime.
	///
	/// Use with <see cref="SKSurface.Create(SKGraphiteRecorder, SKGraphiteBackendTexture, SKColorType)"/>
	/// to render into the wrapped texture.
	/// </summary>
	public unsafe class SKGraphiteBackendTexture : SKObject
	{
		internal SKGraphiteBackendTexture (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Wrap an externally-allocated <c>VkImage</c>. Returns null if the
		/// image is invalid or the format is not supported by Graphite. The
		/// VkImage's memory must already be bound; this wrapper does NOT
		/// allocate or bind memory.
		/// </summary>
		/// <param name="imageLayout">VkImageLayout the image is in when handed to Skia.</param>
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

		/// <summary>
		/// Wrap an externally-allocated id&lt;MTLTexture&gt;. The wrapper does
		/// NOT call retain or release on the passed-in texture — caller must
		/// keep it alive for the BackendTexture's lifetime.
		/// </summary>
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

		/// <summary>
		/// Wrap an externally-allocated <c>WGPUTexture</c>. Dimensions and format
		/// are queried directly from the texture. The wrapper does NOT retain
		/// the texture — caller must keep it alive for the BackendTexture's
		/// lifetime. Any SKSurface/SKImage built from this BackendTexture
		/// <em>does</em> retain it, at which point the caller may drop their reference.
		/// </summary>
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

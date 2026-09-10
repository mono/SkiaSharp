#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>Wraps an existing platform GPU texture so that it can be used as a Graphite render target or sampling source.</summary>
	/// <remarks>
	///       <format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// Create an instance from a platform texture handle using one of the backend-specific factory methods (<xref:SkiaSharp.SKGraphiteBackendTexture.CreateVulkan(System.Int32,System.Int32,SkiaSharp.SKGraphiteVkTextureInfo,System.Int32,System.UInt32,System.IntPtr)>, <xref:SkiaSharp.SKGraphiteBackendTexture.CreateMetal(System.Int32,System.Int32,System.IntPtr)>, or <xref:SkiaSharp.SKGraphiteBackendTexture.CreateDawn(System.IntPtr)>). The caller retains ownership of the underlying platform texture; disposing this wrapper does not destroy it.
	///
	/// This type wraps a native Skia resource and implements `IDisposable`. Dispose it when it is no longer needed.
	/// ]]></format>
	///     </remarks>
	public unsafe class SKGraphiteBackendTexture : SKObject
	{
		internal SKGraphiteBackendTexture (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <param name="width">The width of the texture, in pixels.</param>
		/// <param name="height">The height of the texture, in pixels.</param>
		/// <param name="info">The Vulkan texture descriptor.</param>
		/// <param name="imageLayout">The current Vulkan image layout of the texture.</param>
		/// <param name="queueFamilyIndex">The index of the Vulkan queue family that owns the image.</param>
		/// <param name="vkImage">A handle to the Vulkan image to wrap.</param>
		/// <summary>Wraps an existing Vulkan image as a Graphite backend texture.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKGraphiteBackendTexture" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
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

		/// <param name="width">The width of the texture, in pixels.</param>
		/// <param name="height">The height of the texture, in pixels.</param>
		/// <param name="mtlTexture">A handle to the Metal texture to wrap.</param>
		/// <summary>Wraps an existing Metal texture as a Graphite backend texture.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKGraphiteBackendTexture" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
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

		/// <param name="wgpuTexture">A handle to the WebGPU texture to wrap.</param>
		/// <summary>Wraps an existing Dawn (WebGPU) texture as a Graphite backend texture.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKGraphiteBackendTexture" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKGraphiteBackendTexture CreateDawn (IntPtr wgpuTexture)
		{
			if (wgpuTexture == IntPtr.Zero)
				throw new ArgumentNullException (nameof (wgpuTexture));
			IntPtr handle = SkiaApi.sk_graphite_dawn_backend_texture_new ((void*)wgpuTexture);
			return handle == IntPtr.Zero ? null : new SKGraphiteBackendTexture (handle, true);
		}

		/// <summary>Releases the native resources used by the backend texture wrapper.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_graphite_backend_texture_delete (Handle);

		/// <summary>Gets a value indicating whether the wrapper references a valid texture.</summary>
		/// <value>
		///           <see langword="true" /> if the backend texture is valid; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool              IsValid => SkiaApi.sk_graphite_backend_texture_is_valid (Handle);
		/// <summary>Gets the graphics backend that this texture belongs to.</summary>
		/// <value>One of the enumeration values that indicates the backend.</value>
		/// <remarks />
		public SKGraphiteBackend Backend => SkiaApi.sk_graphite_backend_texture_get_backend (Handle);

		/// <summary>Gets the dimensions of the texture, in pixels.</summary>
		/// <value>The width and height of the texture.</value>
		/// <remarks />
		public SKSizeI Dimensions {
			get {
				int w, h;
				SkiaApi.sk_graphite_backend_texture_get_dimensions (Handle, &w, &h);
				return new SKSizeI (w, h);
			}
		}
	}
}

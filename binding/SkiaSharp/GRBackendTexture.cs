#nullable disable

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	/// <summary>
	/// Wrap an existing texture created by the client in the 3D API.
	/// </summary>
	/// <remarks>The client is responsible for ensuring that the underlying 3D API object lives
	/// at least as long as the <see cref="SkiaSharp.GRBackendRenderTarget" /> object wrapping
	/// it.
	/// We require the client to explicitly provide information about the target, such
	/// as width, height, and pixel configuration, rather than querying the 3D API for
	/// these values. We expect these properties to be immutable even if the 3D API
	/// doesn't require this (eg: OpenGL).</remarks>
	public unsafe class GRBackendTexture : SKObject, ISKSkipObjectRegistration
	{
		internal GRBackendTexture (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates a new OpenGL <see cref="GRBackendTexture" /> with the specified properties and texture.
		/// </summary>
		/// <param name="width">The width of the render target.</param>
		/// <param name="height">The height of the render target.</param>
		/// <param name="mipmapped">Whether or not the texture is mipmapped.</param>
		/// <param name="glInfo">The OpenGL texture information.</param>
		public GRBackendTexture (int width, int height, bool mipmapped, GRGlTextureInfo glInfo)
			: this (IntPtr.Zero, true)
		{
			CreateGl (width, height, mipmapped, glInfo);
		}

		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="vkInfo"></param>
		public GRBackendTexture (int width, int height, GRVkImageInfo vkInfo)
			: this (IntPtr.Zero, true)
		{
			CreateVulkan (width, height, vkInfo);
		}

		public GRBackendTexture (int width, int height, GRD3DTextureResourceInfo d3dTextureInfo)
			: this (IntPtr.Zero, true)
		{
			CreateDirect3D (width, height, d3dTextureInfo);
		}

		public GRBackendTexture (int width, int height, bool mipmapped, GRMtlTextureInfo mtlInfo)
			: this (IntPtr.Zero, true)
		{
			var info = mtlInfo.ToNative ();
			Handle = SkiaApi.gr_backendtexture_new_metal (width, height, mipmapped, &info);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new GRBackendTexture instance.");
			}
		}

		private void CreateGl (int width, int height, bool mipmapped, GRGlTextureInfo glInfo)
		{
			Handle = SkiaApi.gr_backendtexture_new_gl (width, height, mipmapped, &glInfo);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new GRBackendTexture instance.");
			}
		}

		private void CreateVulkan (int width, int height, GRVkImageInfo vkInfo)
		{
			Handle = SkiaApi.gr_backendtexture_new_vulkan (width, height, &vkInfo);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new GRBackendTexture instance.");
			}
		}

		private void CreateDirect3D (int width, int height, GRD3DTextureResourceInfo d3dTextureInfo)
		{
			var native = d3dTextureInfo.ToNative ();
			Handle = SkiaApi.gr_backendtexture_new_direct3d (width, height, &native);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new GRBackendTexture instance.");
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.gr_backendtexture_delete (Handle);

		/// <summary>
		/// Gets a value indicating whether or not the <see cref="GRBackendTexture" /> was initialized.
		/// </summary>
		public bool IsValid => SkiaApi.gr_backendtexture_is_valid (Handle);
		/// <summary>
		/// Gets the width in pixels.
		/// </summary>
		public int Width => SkiaApi.gr_backendtexture_get_width (Handle);
		/// <summary>
		/// Gets the height in pixels.
		/// </summary>
		public int Height => SkiaApi.gr_backendtexture_get_height (Handle);
		/// <summary>
		/// Gets a value indicating whether this texture is mipmapped.
		/// </summary>
		public bool HasMipMaps => SkiaApi.gr_backendtexture_has_mipmaps (Handle);
		/// <summary>
		/// Gets the backend for this texture.
		/// </summary>
		public GRBackend Backend => SkiaApi.gr_backendtexture_get_backend (Handle).FromNative ();
		/// <summary>
		/// Gets the current size of the 3D API object.
		/// </summary>
		public SKSizeI Size => new SKSizeI (Width, Height);
		/// <summary>
		/// Gets a rectangle with the current width and height.
		/// </summary>
		public SKRectI Rect => new SKRectI (0, 0, Width, Height);

		/// <summary>
		/// Returns the texture info that this object wraps.
		/// </summary>
		/// <returns>Returns the texture info, if this object wraps an OpenGL texture, otherwise an empty info instance.</returns>
		public GRGlTextureInfo GetGlTextureInfo () =>
			GetGlTextureInfo (out var info) ? info : default;

		/// <summary>
		/// Returns the texture info that this object wraps.
		/// </summary>
		/// <param name="glInfo">The texture info, if this object wraps an OpenGL texture.</param>
		/// <returns>Returns true if this object wraps an OpenGL texture, otherwise false.</returns>
		public bool GetGlTextureInfo (out GRGlTextureInfo glInfo)
		{
			fixed (GRGlTextureInfo* g = &glInfo) {
				return SkiaApi.gr_backendtexture_get_gl_textureinfo (Handle, g);
			}
		}
	}
}

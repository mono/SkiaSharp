#nullable disable

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	/// <summary>Wrap an existing texture created by the client in the 3D API.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// The client is responsible for ensuring that the underlying 3D API object lives
	/// at least as long as the <xref:SkiaSharp.GRBackendRenderTarget> object wrapping
	/// it.
	///
	/// We require the client to explicitly provide information about the target, such
	/// as width, height, and pixel configuration, rather than querying the 3D API for
	/// these values. We expect these properties to be immutable even if the 3D API
	/// doesn't require this (eg: OpenGL).
	/// ]]></format></remarks>
	public unsafe class GRBackendTexture : SKObject, ISKSkipObjectRegistration
	{
		internal GRBackendTexture (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Creates a new OpenGL <see cref="T:SkiaSharp.GRBackendTexture" /> with the specified properties and texture.</summary>
		/// <param name="width">The width of the render target.</param>
		/// <param name="height">The height of the render target.</param>
		/// <param name="mipmapped"><see langword="true" /> if the texture is mipmapped; otherwise, <see langword="false" />.</param>
		/// <param name="glInfo">The OpenGL texture information.</param>
		/// <remarks />
		public GRBackendTexture (int width, int height, bool mipmapped, GRGlTextureInfo glInfo)
			: this (IntPtr.Zero, true)
		{
			CreateGl (width, height, mipmapped, glInfo);
		}

		/// <summary>Creates a new Vulkan <see cref="T:SkiaSharp.GRBackendTexture" /> with the specified properties and image.</summary>
		/// <param name="width">The width of the texture in pixels.</param>
		/// <param name="height">The height of the texture in pixels.</param>
		/// <param name="vkInfo">The Vulkan image information.</param>
		/// <remarks />
		public GRBackendTexture (int width, int height, GRVkImageInfo vkInfo)
			: this (IntPtr.Zero, true)
		{
			CreateVulkan (width, height, vkInfo);
		}

		/// <summary>Creates a new Direct3D <see cref="T:SkiaSharp.GRBackendTexture" /> with the specified properties and texture.</summary>
		/// <param name="width">The width of the texture in pixels.</param>
		/// <param name="height">The height of the texture in pixels.</param>
		/// <param name="d3dTextureInfo">The Direct3D texture resource information.</param>
		/// <remarks />
		public GRBackendTexture (int width, int height, GRD3DTextureResourceInfo d3dTextureInfo)
			: this (IntPtr.Zero, true)
		{
			CreateDirect3D (width, height, d3dTextureInfo);
		}

		/// <summary>Creates a new Metal <see cref="T:SkiaSharp.GRBackendTexture" /> with the specified properties and texture.</summary>
		/// <param name="width">The width of the texture in pixels.</param>
		/// <param name="height">The height of the texture in pixels.</param>
		/// <param name="mipmapped"><see langword="true" /> if the texture is mipmapped; otherwise, <see langword="false" />.</param>
		/// <param name="mtlInfo">The Metal texture information.</param>
		/// <remarks />
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

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.GRBackendTexture" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.GRBackendTexture" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.gr_backendtexture_delete (Handle);

		/// <summary>Gets a value indicating whether or not the <see cref="T:SkiaSharp.GRBackendTexture" /> was initialized.</summary>
		/// <value><see langword="true" /> if the <see cref="T:SkiaSharp.GRBackendTexture" /> was initialized; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsValid {
			get {
				var result = SkiaApi.gr_backendtexture_is_valid (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}
		/// <summary>Gets the width in pixels.</summary>
		/// <value>The width in pixels.</value>
		/// <remarks />
		public int Width {
			get {
				var result = SkiaApi.gr_backendtexture_get_width (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}
		/// <summary>Gets the height in pixels.</summary>
		/// <value>The height in pixels.</value>
		/// <remarks />
		public int Height {
			get {
				var result = SkiaApi.gr_backendtexture_get_height (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}
		/// <summary>Gets a value indicating whether this texture is mipmapped.</summary>
		/// <value><see langword="true" /> if this texture is mipmapped; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool HasMipMaps {
			get {
				var result = SkiaApi.gr_backendtexture_has_mipmaps (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}
		/// <summary>Gets the backend for this texture.</summary>
		/// <value>One of the enumeration values that specifies the backend for this texture.</value>
		/// <remarks />
		public GRBackend Backend {
			get {
				var result = SkiaApi.gr_backendtexture_get_backend (Handle).FromNative ();
				GC.KeepAlive (this);
				return result;
			}
		}
		/// <summary>Gets the current size of the 3D API object.</summary>
		/// <value>The current size of the 3D API object.</value>
		/// <remarks />
		public SKSizeI Size => new SKSizeI (Width, Height);
		/// <summary>Gets a rectangle with the current width and height.</summary>
		/// <value>The rectangle with the current width and height.</value>
		/// <remarks />
		public SKRectI Rect => new SKRectI (0, 0, Width, Height);

		/// <summary>Returns the texture info that this object wraps.</summary>
		/// <returns>Returns the texture info, if this object wraps an OpenGL texture, otherwise an empty info instance.</returns>
		/// <remarks />
		public GRGlTextureInfo GetGlTextureInfo () =>
			GetGlTextureInfo (out var info) ? info : default;

		/// <summary>Returns the texture info that this object wraps.</summary>
		/// <param name="glInfo">The texture info, if this object wraps an OpenGL texture.</param>
		/// <returns><see langword="true" /> if this object wraps an OpenGL texture; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool GetGlTextureInfo (out GRGlTextureInfo glInfo)
		{
			fixed (GRGlTextureInfo* g = &glInfo) {
				var result = SkiaApi.gr_backendtexture_get_gl_textureinfo (Handle, g);
				GC.KeepAlive (this);
				return result;
			}
		}
	}
}

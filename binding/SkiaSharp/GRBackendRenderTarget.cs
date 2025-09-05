#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>
	/// Wrap an existing render target created by the client in the 3D API.
	/// </summary>
	/// <remarks>
	/// The client is responsible for ensuring that the underlying 3D API object lives
	/// at least as long as the <see cref="SkiaSharp.GRBackendRenderTarget" /> object wrapping
	/// it.
	/// We require the client to explicitly provide information about the target, such
	/// as width, height, and pixel configuration, rather than querying the 3D API for
	/// these values. We expect these properties to be immutable even if the 3D API
	/// doesn't require this (eg: OpenGL).
	/// </remarks>
	public unsafe class GRBackendRenderTarget : SKObject, ISKSkipObjectRegistration
	{
		internal GRBackendRenderTarget (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates a new OpenGL <see cref="GRBackendRenderTarget" /> with the specified properties and framebuffer.
		/// </summary>
		/// <param name="width">The width of the render target.</param>
		/// <param name="height">The height of the render target.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="stencilBits">The number of bits of stencil per pixel.</param>
		/// <param name="glInfo">The OpenGL framebuffer information.</param>
		public GRBackendRenderTarget (int width, int height, int sampleCount, int stencilBits, GRGlFramebufferInfo glInfo)
			: this (IntPtr.Zero, true)
		{
			CreateGl (width, height, sampleCount, stencilBits, glInfo);
		}

		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="sampleCount"></param>
		/// <param name="vkImageInfo"></param>
		[Obsolete ("Use GRBackendRenderTarget(int width, int height, GRVkImageInfo vkImageInfo) instead.")]
		public GRBackendRenderTarget (int width, int height, int sampleCount, GRVkImageInfo vkImageInfo)
			: this (width, height, vkImageInfo)
		{
		}

		public GRBackendRenderTarget (int width, int height, GRVkImageInfo vkImageInfo)
			: this (IntPtr.Zero, true)
		{
			CreateVulkan (width, height, vkImageInfo);
		}

		public GRBackendRenderTarget (int width, int height, GRD3DTextureResourceInfo d3dTextureInfo)
			: this (IntPtr.Zero, true)
		{
			CreateDirect3D (width, height, d3dTextureInfo);
		}

		[Obsolete ("Use GRBackendRenderTarget(int width, int height, GRMtlTextureInfo mtlInfo) instead.")]
		public GRBackendRenderTarget (int width, int height, int sampleCount, GRMtlTextureInfo mtlInfo)
			: this (width, height, mtlInfo)
		{
		}

		public GRBackendRenderTarget (int width, int height, GRMtlTextureInfo mtlInfo)
			: this (IntPtr.Zero, true)
		{
			var info = mtlInfo.ToNative ();
			Handle = SkiaApi.gr_backendrendertarget_new_metal (width, height, &info);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new GRBackendRenderTarget instance.");
			}
		}

		private void CreateGl (int width, int height, int sampleCount, int stencilBits, GRGlFramebufferInfo glInfo)
		{
			Handle = SkiaApi.gr_backendrendertarget_new_gl (width, height, sampleCount, stencilBits, &glInfo);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new GRBackendRenderTarget instance.");
			}
		}

		private void CreateVulkan (int width, int height, GRVkImageInfo vkImageInfo)
		{
			Handle = SkiaApi.gr_backendrendertarget_new_vulkan (width, height, &vkImageInfo);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new GRBackendRenderTarget instance.");
			}
		}

		private void CreateDirect3D (int width, int height, GRD3DTextureResourceInfo d3dTextureInfo)
		{
			var native = d3dTextureInfo.ToNative ();
			Handle = SkiaApi.gr_backendrendertarget_new_direct3d (width, height, &native);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new GRBackendRenderTarget instance.");
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.gr_backendrendertarget_delete (Handle);

		/// <summary>
		/// Gets a value indicating whether or not the <see cref="GRBackendRenderTarget" /> was initialized.
		/// </summary>
		public bool IsValid => SkiaApi.gr_backendrendertarget_is_valid (Handle);
		/// <summary>
		/// Gets the width in pixels.
		/// </summary>
		public int Width => SkiaApi.gr_backendrendertarget_get_width (Handle);
		/// <summary>
		/// Gets the height in pixels.
		/// </summary>
		public int Height => SkiaApi.gr_backendrendertarget_get_height (Handle);
		/// <summary>
		/// Gets the number of samples per pixel.
		/// </summary>
		/// <remarks>
		/// This is used to influence decisions about applying other forms of anti-aliasing.
		/// </remarks>
		public int SampleCount => SkiaApi.gr_backendrendertarget_get_samples (Handle);
		/// <summary>
		/// Gets the number of bits of stencil per-pixel.
		/// </summary>
		public int StencilBits => SkiaApi.gr_backendrendertarget_get_stencils (Handle);
		/// <summary>
		/// Gets the backend for this render target.
		/// </summary>
		public GRBackend Backend => SkiaApi.gr_backendrendertarget_get_backend (Handle).FromNative ();
		/// <summary>
		/// Gets the current size of the 3D API object.
		/// </summary>
		public SKSizeI Size => new SKSizeI (Width, Height);
		/// <summary>
		/// Gets a rectangle with the current width and height.
		/// </summary>
		public SKRectI Rect => new SKRectI (0, 0, Width, Height);

		/// <summary>
		/// Returns the framebuffer info that this object wraps.
		/// </summary>
		/// <returns>Returns the framebuffer info, if this object wraps an OpenGL framebuffer, otherwise an empty info instance.</returns>
		public GRGlFramebufferInfo GetGlFramebufferInfo () =>
			GetGlFramebufferInfo (out var info) ? info : default;

		/// <summary>
		/// Returns the framebuffer info that this object wraps.
		/// </summary>
		/// <param name="glInfo">The framebuffer info, if this object wraps an OpenGL framebuffer.</param>
		/// <returns>Returns true if this object wraps an OpenGL framebuffer, otherwise false.</returns>
		public bool GetGlFramebufferInfo (out GRGlFramebufferInfo glInfo)
		{
			fixed (GRGlFramebufferInfo* g = &glInfo) {
				return SkiaApi.gr_backendrendertarget_get_gl_framebufferinfo (Handle, g);
			}
		}
	}
}

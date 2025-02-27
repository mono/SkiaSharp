#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	public unsafe class GRBackendRenderTarget : SKObject, ISKSkipObjectRegistration
	{
		internal GRBackendRenderTarget (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public GRBackendRenderTarget (int width, int height, int sampleCount, int stencilBits, GRGlFramebufferInfo glInfo)
			: this (IntPtr.Zero, true)
		{
			CreateGl (width, height, sampleCount, stencilBits, glInfo);
		}

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

#if __IOS__ || __MACOS__ || __TVOS__

		[Obsolete ("Use GRBackendRenderTarget(int width, int height, GRMtlTextureInfo mtlInfo) instead.")]
		public GRBackendRenderTarget (int width, int height, int sampleCount, GRMtlTextureInfo mtlInfo)
			: this (width, height, mtlInfo)
		{
		}

#endif

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

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.gr_backendrendertarget_delete (Handle);

		public bool IsValid => SkiaApi.gr_backendrendertarget_is_valid (Handle);
		public int Width => SkiaApi.gr_backendrendertarget_get_width (Handle);
		public int Height => SkiaApi.gr_backendrendertarget_get_height (Handle);
		public int SampleCount => SkiaApi.gr_backendrendertarget_get_samples (Handle);
		public int StencilBits => SkiaApi.gr_backendrendertarget_get_stencils (Handle);
		public GRBackend Backend => SkiaApi.gr_backendrendertarget_get_backend (Handle).FromNative ();
		public SKSizeI Size => new SKSizeI (Width, Height);
		public SKRectI Rect => new SKRectI (0, 0, Width, Height);

		public GRGlFramebufferInfo GetGlFramebufferInfo () =>
			GetGlFramebufferInfo (out var info) ? info : default;

		public bool GetGlFramebufferInfo (out GRGlFramebufferInfo glInfo)
		{
			fixed (GRGlFramebufferInfo* g = &glInfo) {
				return SkiaApi.gr_backendrendertarget_get_gl_framebufferinfo (Handle, g);
			}
		}
	}
}

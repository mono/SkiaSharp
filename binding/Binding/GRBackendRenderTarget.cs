using System;
using System.ComponentModel;

namespace SkiaSharp
{
	public unsafe class GRBackendRenderTarget : SKObject, ISKSkipObjectRegistration
	{
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GRBackendRenderTarget(int, int, int, int, GRGlFramebufferInfo) instead.")]
		public GRBackendRenderTarget (GRBackend backend, GRBackendRenderTargetDesc desc)
			: base (IntPtr.Zero, true, false)
		{
			if (backend != GRBackend.OpenGL)
				throw new ArgumentOutOfRangeException (nameof (backend));

			var glInfo = new GRGlFramebufferInfo ((uint)desc.RenderTargetHandle, desc.Config.ToGlSizedFormat ());
			RegisterHandle (SkiaApi.gr_backendrendertarget_new_gl (desc.Width, desc.Height, desc.SampleCount, desc.StencilBits, &glInfo));
		}

		public GRBackendRenderTarget (int width, int height, int sampleCount, int stencilBits, GRGlFramebufferInfo glInfo)
			: base (SkiaApi.gr_backendrendertarget_new_gl (width, height, sampleCount, stencilBits, &glInfo))
		{
		}

		public GRBackendRenderTarget (int width, int height, int sampleCount, GRVkImageInfo vkImageInfo)
			: base (SkiaApi.gr_backendrendertarget_new_vulkan (width, height, sampleCount, &vkImageInfo))
		{
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

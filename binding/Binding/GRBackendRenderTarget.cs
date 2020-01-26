using System;

namespace SkiaSharp
{
	public unsafe class GRBackendRenderTarget : SKObject
	{
		[Preserve]
		internal GRBackendRenderTarget (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public GRBackendRenderTarget (int width, int height, int sampleCount, int stencilBits, GRGlFramebufferInfo glInfo)
			: this (IntPtr.Zero, true)
		{
			Handle = SkiaApi.gr_backendrendertarget_new_gl (width, height, sampleCount, stencilBits, &glInfo);

			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new GRBackendRenderTarget instance.");
		}

		protected override void DisposeNative () =>
			SkiaApi.gr_backendrendertarget_delete (Handle);

		public bool IsValid => SkiaApi.gr_backendrendertarget_is_valid (Handle);

		public int Width => SkiaApi.gr_backendrendertarget_get_width (Handle);

		public int Height => SkiaApi.gr_backendrendertarget_get_height (Handle);

		public int SampleCount => SkiaApi.gr_backendrendertarget_get_samples (Handle);

		public int StencilBits => SkiaApi.gr_backendrendertarget_get_stencils (Handle);

		public GRBackend Backend => SkiaApi.gr_backendrendertarget_get_backend (Handle);

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

using System;

namespace SkiaSharp
{
	public class GRBackendRenderTarget : SKObject
	{
		[Preserve]
		internal GRBackendRenderTarget (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		[Obsolete ("Use GRBackendRenderTarget(int, int, int, int, GRGlFramebufferInfo) instead.")]
		public GRBackendRenderTarget (GRBackend backend, GRBackendRenderTargetDesc desc)
			: this (IntPtr.Zero, true)
		{
			switch (backend) {
				case GRBackend.Metal:
					throw new NotSupportedException ();
				case GRBackend.OpenGL:
					var glInfo = new GRGlFramebufferInfo ((uint)desc.RenderTargetHandle, desc.Config.ToGlSizedFormat ());
					CreateGl (desc.Width, desc.Height, desc.SampleCount, desc.StencilBits, glInfo);
					break;
				case GRBackend.Vulkan:
					throw new NotSupportedException ();
				default:
					throw new ArgumentOutOfRangeException (nameof (backend));
			}
		}

		public GRBackendRenderTarget (int width, int height, int sampleCount, int stencilBits, GRGlFramebufferInfo glInfo)
			: this (IntPtr.Zero, true)
		{
			CreateGl (width, height, sampleCount, stencilBits, glInfo);
		}

		private void CreateGl (int width, int height, int sampleCount, int stencilBits, GRGlFramebufferInfo glInfo)
		{
			Handle = SkiaApi.gr_backendrendertarget_new_gl (width, height, sampleCount, stencilBits, ref glInfo);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new GRBackendRenderTarget instance.");
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.gr_backendrendertarget_delete (Handle);
			}

			base.Dispose (disposing);
		}

		public bool IsValid => SkiaApi.gr_backendrendertarget_is_valid (Handle);
		public int Width => SkiaApi.gr_backendrendertarget_get_width (Handle);
		public int Height => SkiaApi.gr_backendrendertarget_get_height (Handle);
		public int SampleCount => SkiaApi.gr_backendrendertarget_get_samples (Handle);
		public int StencilBits => SkiaApi.gr_backendrendertarget_get_stencils (Handle);
		public GRBackend Backend => SkiaApi.gr_backendrendertarget_get_backend (Handle);
		public SKSizeI Size => new SKSizeI (Width, Height);
		public SKRectI Rect => new SKRectI (0, 0, Width, Height);

		public GRGlFramebufferInfo GetGlFramebufferInfo ()
		{
			if (GetGlFramebufferInfo (out var info))
				return info;
			return default (GRGlFramebufferInfo);
		}
		public bool GetGlFramebufferInfo (out GRGlFramebufferInfo glInfo)
		{
			return SkiaApi.gr_backendrendertarget_get_gl_framebufferinfo (Handle, out glInfo);
		}
	}
}

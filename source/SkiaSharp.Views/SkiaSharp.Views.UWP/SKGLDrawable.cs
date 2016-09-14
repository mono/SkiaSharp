using System;
using SkiaSharp.Views.GlesInterop;

namespace SkiaSharp.Views
{
	internal static class SKGLDrawable
	{
		public static GRBackendRenderTargetDesc CreateRenderTarget()
		{
			int framebuffer, stencil, samples;
			Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, out framebuffer);
			Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out stencil);
			Gles.glGetIntegerv(Gles.GL_SAMPLES, out samples);

			int bufferWidth = 0;
			int bufferHeight = 0;
			GlesContext.CurrentContext.GetSurfaceDimensions(out bufferWidth, out bufferHeight);

			return new GRBackendRenderTargetDesc
			{
				Width = bufferWidth,
				Height = bufferHeight,
				Config = GRPixelConfig.Rgba8888,
				Origin = GRSurfaceOrigin.TopLeft,
				SampleCount = samples,
				StencilBits = stencil,
				RenderTargetHandle = (IntPtr)framebuffer,
			};
		}
	}
}

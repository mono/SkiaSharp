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

			int bufferWidth, bufferHeight;
			Gles.glGetRenderbufferParameteriv(Gles.GL_RENDERBUFFER, Gles.GL_RENDERBUFFER_WIDTH, out bufferWidth);
			Gles.glGetRenderbufferParameteriv(Gles.GL_RENDERBUFFER, Gles.GL_RENDERBUFFER_HEIGHT, out bufferHeight);

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

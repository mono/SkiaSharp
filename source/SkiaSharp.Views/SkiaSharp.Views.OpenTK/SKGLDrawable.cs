using System;
#if __IOS__ || __TVOS__
using OpenTK.Graphics.ES20;
#elif __MACOS__ || __DESKTOP__
using OpenTK.Graphics.OpenGL;
#endif

namespace SkiaSharp.Views
{
	internal static class SKGLDrawable
	{
		public static GRBackendRenderTargetDesc CreateRenderTarget()
		{
			int framebuffer, stencil, samples;
			GL.GetInteger(GetPName.FramebufferBinding, out framebuffer);
			GL.GetInteger(GetPName.StencilBits, out stencil);
			GL.GetInteger(GetPName.Samples, out samples);

			int bufferWidth = 0;
			int bufferHeight = 0;
#if __IOS__ || __TVOS__
			GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferWidth, out bufferWidth);
			GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferHeight, out bufferHeight);
#endif

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

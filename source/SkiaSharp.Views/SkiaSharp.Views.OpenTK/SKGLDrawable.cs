using System;
#if __IOS__ || __TVOS__
using OpenTK.Graphics.ES20;
#elif __MACOS__ || __DESKTOP__
using OpenTK.Graphics.OpenGL;
#endif

#if __ANDROID__
namespace SkiaSharp.Views.Android
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif __DESKTOP__ || __WPF__
namespace SkiaSharp.Views.Desktop
#elif __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#endif
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

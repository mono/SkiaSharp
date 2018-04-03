#if !__WATCHOS__
using System;
using SkiaSharp.Views.GlesInterop;

#if __ANDROID__
namespace SkiaSharp.Views.Android
#elif __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif __DESKTOP__ || __WPF__ || __GTK__
namespace SkiaSharp.Views.Desktop
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#elif WINDOWS_UWP
namespace SkiaSharp.Views.UWP
#elif TIZEN4_0
namespace SkiaSharp.Views.Tizen
#endif
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
#if __IOS__ || __TVOS__
			Gles.glGetRenderbufferParameteriv(Gles.GL_RENDERBUFFER, Gles.GL_RENDERBUFFER_WIDTH, out bufferWidth);
			Gles.glGetRenderbufferParameteriv(Gles.GL_RENDERBUFFER, Gles.GL_RENDERBUFFER_HEIGHT, out bufferHeight);
#endif

			return new GRBackendRenderTargetDesc
			{
				Width = bufferWidth,
				Height = bufferHeight,
				Config = GRPixelConfig.Rgba8888,
				Origin = GRSurfaceOrigin.BottomLeft,
				SampleCount = samples,
				StencilBits = stencil,
				RenderTargetHandle = (IntPtr)framebuffer,
			};
		}
	}
}
#endif

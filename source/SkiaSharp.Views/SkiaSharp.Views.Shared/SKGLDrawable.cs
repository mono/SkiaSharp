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
#elif __TIZEN__
namespace SkiaSharp.Views.Tizen
#endif
{
	internal static class SKGLDrawable
	{
		public static GRBackendRenderTarget CreateRenderTarget(int bufferWidth, int bufferHeight)
		{
			Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, out var framebuffer);
			Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil);
			Gles.glGetIntegerv(Gles.GL_SAMPLES, out var samples);

#if __IOS__ || __TVOS__
			Gles.glGetRenderbufferParameteriv(Gles.GL_RENDERBUFFER, Gles.GL_RENDERBUFFER_WIDTH, out bufferWidth);
			Gles.glGetRenderbufferParameteriv(Gles.GL_RENDERBUFFER, Gles.GL_RENDERBUFFER_HEIGHT, out bufferHeight);
#endif

#if __TIZEN__
			var isBgra = SKImageInfo.PlatformColorType == SKColorType.Bgra8888;
			var config = isBgra ? GRPixelConfig.Bgra8888 : GRPixelConfig.Rgba8888;
#else
			var config = GRPixelConfig.Rgba8888;
#endif

			var glInfo = new GRGlFramebufferInfo((uint)framebuffer, config.ToSizedFormat());
			return new GRBackendRenderTarget(bufferWidth, bufferHeight, samples, stencil, glInfo);
		}
	}
}
#endif

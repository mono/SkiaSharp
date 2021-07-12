using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Hosting;
using CompatRegistrar = Microsoft.Maui.Controls.Internals.Registrar;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	public static class AppHostBuilderExtensions
	{
		public static IAppHostBuilder UseSkiaSharpCompatibilityRenderers(this IAppHostBuilder builder) =>
			builder
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddCompatibilityRenderer(typeof(SKCanvasView), typeof(SKCanvasViewRenderer));
#if !WINDOWS
					handlers.AddCompatibilityRenderer(typeof(SKGLView), typeof(SKGLViewRenderer));
#endif

					CompatRegistrar.Registered.Register(typeof(SKImageImageSource), typeof(SKImageSourceHandler));
					CompatRegistrar.Registered.Register(typeof(SKBitmapImageSource), typeof(SKImageSourceHandler));
					CompatRegistrar.Registered.Register(typeof(SKPixmapImageSource), typeof(SKImageSourceHandler));
					CompatRegistrar.Registered.Register(typeof(SKPictureImageSource), typeof(SKImageSourceHandler));
				});
	}
}

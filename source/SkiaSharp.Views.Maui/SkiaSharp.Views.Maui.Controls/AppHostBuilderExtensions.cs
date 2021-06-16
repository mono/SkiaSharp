using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Controls
{
	public static class AppHostBuilderExtensions
	{
		public static IAppHostBuilder UseSkiaSharpHandlers(this IAppHostBuilder builder) =>
			builder
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<SKCanvasView, SKCanvasViewHandler>();
				})
				.ConfigureImageSources(sources =>
				{
					sources.AddService<ISKImageImageSource, SKImageSourceService>();
					sources.AddService<ISKBitmapImageSource, SKImageSourceService>();
					sources.AddService<ISKPixmapImageSource, SKImageSourceService>();
					sources.AddService<ISKPictureImageSource, SKImageSourceService>();
				});
	}
}

using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Compatibility;
using SkiaSharp.Views.Maui.Controls.Hosting;
using CompatRegistrar = Microsoft.Maui.Controls.Internals.Registrar;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	[Obsolete("Use SkiaSharp.Views.Maui.Controls.Hosting instead.", true)]
	public static class AppHostBuilderExtensions
	{
		[Obsolete("Use SkiaSharp.Views.Maui.Controls.Hosting.UseSkiaSharp(bool, bool) instead.", true)]
		public static IAppHostBuilder UseSkiaSharpCompatibilityRenderers(this IAppHostBuilder builder) => builder.UseSkiaSharp(true);
	}
}

namespace SkiaSharp.Views.Maui.Controls.Hosting
{
	public static class AppHostBuilderExtensions
	{
		public static IAppHostBuilder UseSkiaSharp(this IAppHostBuilder builder, bool registerRenderers, bool replaceHandlers = false) =>
			builder
				.UseSkiaSharp()
				.ConfigureMauiHandlers(handlers =>
				{
#if !NETSTANDARD
					if (replaceHandlers)
						handlers.AddCompatibilityRenderer(typeof(SKCanvasView), typeof(SKCanvasViewRenderer));
					else
						handlers.TryAddCompatibilityRenderer(typeof(SKCanvasView), typeof(SKCanvasViewRenderer));

#if !WINDOWS
					handlers.AddCompatibilityRenderer(typeof(SKGLView), typeof(SKGLViewRenderer));
#endif

					CompatRegistrar.Registered.Register(typeof(SKImageImageSource), typeof(SKImageSourceHandler));
					CompatRegistrar.Registered.Register(typeof(SKBitmapImageSource), typeof(SKImageSourceHandler));
					CompatRegistrar.Registered.Register(typeof(SKPixmapImageSource), typeof(SKImageSourceHandler));
					CompatRegistrar.Registered.Register(typeof(SKPictureImageSource), typeof(SKImageSourceHandler));
#endif
				});
	}
}

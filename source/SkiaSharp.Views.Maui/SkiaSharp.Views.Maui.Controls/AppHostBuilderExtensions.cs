using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SkiaSharp.Views.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Controls
{
	[Obsolete("Use SkiaSharp.Views.Maui.Controls.Hosting instead.", true)]
	public static class AppHostBuilderExtensions
	{
		[Obsolete("Use SkiaSharp.Views.Maui.Controls.Hosting.UseSkiaSharp() instead.", true)]
		public static MauiAppBuilder UseSkiaSharpHandlers(this MauiAppBuilder builder) => builder.UseSkiaSharp();
	}
}

namespace SkiaSharp.Views.Maui.Controls.Hosting
{
	public static class AppHostBuilderExtensions
	{
		public static MauiAppBuilder UseSkiaSharp(this MauiAppBuilder builder) =>
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

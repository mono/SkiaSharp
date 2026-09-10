using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SkiaSharp.Views.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Controls.Hosting
{
	/// <summary>Provides extension methods for configuring SkiaSharp in a .NET MAUI application.</summary>
	/// <remarks>Call <see cref="M:SkiaSharp.Views.Maui.Controls.Hosting.AppHostBuilderExtensions.UseSkiaSharp(Microsoft.Maui.Hosting.MauiAppBuilder)" /> in your <c>MauiProgram.cs</c> to register SkiaSharp handlers and services.</remarks>
	public static class AppHostBuilderExtensions
	{
		/// <summary>Registers SkiaSharp handlers and services with the MAUI application.</summary>
		/// <param name="builder">The <see cref="T:Microsoft.Maui.Hosting.MauiAppBuilder" /> to configure.</param>
		/// <returns>The <see cref="T:Microsoft.Maui.Hosting.MauiAppBuilder" /> for method chaining.</returns>
		/// <remarks>This method registers handlers for <see cref="T:SkiaSharp.Views.Maui.Controls.SKCanvasView" /> and <see cref="T:SkiaSharp.Views.Maui.Controls.SKGLView" />, as well as image source services for SkiaSharp image types.</remarks>
		public static MauiAppBuilder UseSkiaSharp(this MauiAppBuilder builder) =>
			builder
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<SKCanvasView, SKCanvasViewHandler>();
					handlers.AddHandler<SKGLView, SKGLViewHandler>();
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

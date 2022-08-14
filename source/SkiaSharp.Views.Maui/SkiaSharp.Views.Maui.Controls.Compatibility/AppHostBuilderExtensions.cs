using System;
using Microsoft.Maui.Controls.Compatibility.Hosting;
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
		public static MauiAppBuilder UseSkiaSharpCompatibilityRenderers(this MauiAppBuilder builder) => builder.UseSkiaSharp(true);
	}
}

namespace SkiaSharp.Views.Maui.Controls.Hosting
{
	public static class AppHostBuilderExtensions
	{
		public static MauiAppBuilder UseSkiaSharp(this MauiAppBuilder builder, bool registerRenderers, bool replaceHandlers = false) =>
			builder
				.UseSkiaSharp()
				.ConfigureMauiHandlers(handlers =>
				{
#if !NETSTANDARD
					if (registerRenderers)
					{
#if __TIZEN__
#pragma warning disable CS0612 // Type or member is obsolete
						if (replaceHandlers)
							handlers.AddCompatibilityRenderer<SKCanvasView, SKCanvasViewRenderer>();
						else
							handlers.TryAddCompatibilityRenderer(typeof(SKCanvasView), typeof(SKCanvasViewRenderer));

						if (replaceHandlers)
							handlers.AddCompatibilityRenderer<SKGLView, SKGLViewRenderer>();
						else
							handlers.TryAddCompatibilityRenderer(typeof(SKGLView), typeof(SKGLViewRenderer));
#pragma warning restore CS0612 // Type or member is obsolete
#else
						if (replaceHandlers)
							handlers.AddHandler<SKCanvasView, SKCanvasViewRenderer>();
						else
							handlers.TryAddHandler<SKCanvasView, SKCanvasViewRenderer>();

#if !WINDOWS && !__MACCATALYST__
						if (replaceHandlers)
							handlers.AddHandler<SKGLView, SKGLViewRenderer>();
						else
							handlers.TryAddHandler<SKGLView, SKGLViewRenderer>();
#endif
#endif
					}

					CompatRegistrar.Registered.Register(typeof(SKImageImageSource), typeof(SKImageSourceHandler));
					CompatRegistrar.Registered.Register(typeof(SKBitmapImageSource), typeof(SKImageSourceHandler));
					CompatRegistrar.Registered.Register(typeof(SKPixmapImageSource), typeof(SKImageSourceHandler));
					CompatRegistrar.Registered.Register(typeof(SKPictureImageSource), typeof(SKImageSourceHandler));
#endif
				});
	}
}

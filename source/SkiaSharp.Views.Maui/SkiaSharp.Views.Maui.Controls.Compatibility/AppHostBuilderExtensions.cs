﻿using System;
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
#if !__TIZEN__
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

#if !__TIZEN__
					CompatRegistrar.Registered.Register(typeof(SKImageImageSource), typeof(SKImageSourceHandler));
					CompatRegistrar.Registered.Register(typeof(SKBitmapImageSource), typeof(SKImageSourceHandler));
					CompatRegistrar.Registered.Register(typeof(SKPixmapImageSource), typeof(SKImageSourceHandler));
					CompatRegistrar.Registered.Register(typeof(SKPictureImageSource), typeof(SKImageSourceHandler));
#endif
#endif
				});
	}
}

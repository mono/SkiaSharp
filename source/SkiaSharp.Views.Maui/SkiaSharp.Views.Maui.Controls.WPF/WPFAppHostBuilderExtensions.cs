using System;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Controls.Hosting;

/// <summary>Registers SkiaSharp's experimental native WPF handlers with a MAUI WPF application.</summary>
public static class WPFAppHostBuilderExtensions
{
	/// <summary>Registers the SkiaSharp canvas and GL view handlers for WPF.</summary>
	/// <param name="builder">The MAUI application builder configured with <c>UseMauiAppWPF</c>.</param>
	/// <returns>The application builder.</returns>
	/// <remarks>Call after <c>UseMauiAppWPF</c> and instead of <c>UseSkiaSharp</c>. Do not call <c>UseSkiaSharp</c> after this method, or it will replace the WPF handlers.</remarks>
	public static MauiAppBuilder UseSkiaSharpWPF(this MauiAppBuilder builder)
	{
		ArgumentNullException.ThrowIfNull(builder);

		return builder
			.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<SKCanvasView, WPFSKCanvasViewHandler>();
				handlers.AddHandler<SKGLView, WPFSKGLViewHandler>();
			});
	}
}

using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.WPF.Handlers;
using SkiaSharp.Views.Maui.Controls.WPF.Platform;

namespace SkiaSharp.Views.Maui.Controls.Hosting;

/// <summary>Registers SkiaSharp's experimental native WPF handlers with a MAUI WPF application.</summary>
public static class WpfAppHostBuilderExtensions
{
	/// <summary>Registers SkiaSharp services and replaces the default canvas handlers with WPF handlers.</summary>
	/// <param name="builder">The MAUI application builder configured with <c>UseMauiAppWPF</c>.</param>
	/// <returns>The application builder.</returns>
	/// <remarks>Call after <c>UseMauiAppWPF</c> and instead of <c>UseSkiaSharp</c>. Do not call <c>UseSkiaSharp</c> after this method, or it will replace the WPF handlers.</remarks>
	public static MauiAppBuilder UseSkiaSharpWPF(this MauiAppBuilder builder)
	{
		WpfSkiaImageMapper.Register();
		return builder
			.UseSkiaSharp()
			.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<SKCanvasView, WpfSKCanvasViewHandler>();
				handlers.AddHandler<SKGLView, WpfSKGLViewHandler>();
			});
	}
}

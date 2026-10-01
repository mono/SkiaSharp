using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Controls.Hosting
{
	/// <summary>Registers the experimental native AppKit SkiaSharp handlers.</summary>
	public static class AppKitHostBuilderExtensions
	{
		/// <summary>Registers the stable SkiaSharp controls with native AppKit CPU and Metal handlers.</summary>
		public static MauiAppBuilder UseSkiaSharpMacOS(this MauiAppBuilder builder)
		{
			MacImageSourceMapping.Register();
			return AppHostBuilderExtensions.UseSkiaSharp(builder).ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<SKCanvasView, AppKitSKCanvasViewHandler>();
				handlers.AddHandler<SKGLView, AppKitSKGLViewHandler>();
			});
		}
	}
}

using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Handlers.Gtk4;

namespace SkiaSharp.Views.Maui.Controls.Hosting;

/// <summary>Registers the experimental GTK4 SkiaSharp handlers.</summary>
public static class Gtk4AppHostBuilderExtensions
{
	/// <summary>Registers the GTK4 canvas and GPU view handlers.</summary>
	public static MauiAppBuilder UseSkiaSharpGtk4(this MauiAppBuilder builder) =>
		builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddHandler<SKCanvasView, SKCanvasViewHandler>();
			handlers.AddHandler<SKGLView, SKGLViewHandler>();
		});
}

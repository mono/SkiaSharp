using System.Runtime.Versioning;
using Microsoft.Maui.Hosting;

namespace SkiaSharp.Views.Maui.Controls.Hosting;

/// <summary>Registers the experimental Linux GTK4 SkiaSharp handlers.</summary>
public static class Gtk4AppHostBuilderExtensions
{
	/// <summary>Registers SkiaSharp image services and replaces its canvas and GPU handlers with GTK4 handlers.</summary>
	[SupportedOSPlatform("linux")]
	public static MauiAppBuilder UseSkiaSharpGtk4(this MauiAppBuilder builder) =>
		builder.UseSkiaSharp().ConfigureMauiHandlers(handlers =>
		{
			handlers.AddHandler<SKCanvasView, Gtk4.SKCanvasViewHandler>();
			handlers.AddHandler<SKGLView, Gtk4.SKGLViewHandler>();
		});
}

using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platforms.Linux.Gtk4.Essentials.Hosting;
using Microsoft.Maui.Platforms.Linux.Gtk4.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
#if DEBUG
using Microsoft.Maui.DevFlow.Agent.Gtk;
#endif

namespace SkiaSharpSample;

public static class MauiProgram
{
	[UnconditionalSuppressMessage("Interoperability", "CA1416",
		Justification = "Gtk4Runtime configures the GTK4 host for Windows and macOS.")]
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder()
			.UseMauiAppLinuxGtk4<App>()
			.AddLinuxGtk4Essentials()
			.ConfigureFonts(fonts => fonts.AddFont("bootstrap-icons.ttf", "BootstrapIcons"))
			.UseSkiaSharpGtk4();

#if DEBUG
		builder.AddMauiDevFlowAgent(options => options.EnableFileLogging = false);
#endif
		builder.Services.AddSingleton<Services.SampleService>();
		return builder.Build();
	}
}

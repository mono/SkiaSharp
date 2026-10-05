using System.Runtime.Versioning;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platforms.Linux.Gtk4.Essentials.Hosting;
using Microsoft.Maui.Platforms.Linux.Gtk4.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace SkiaSharpSample;

[SupportedOSPlatform("linux")]
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder()
			.UseMauiAppLinuxGtk4<App>()
			.AddLinuxGtk4Essentials()
			.ConfigureFonts(fonts => fonts.AddFont("bootstrap-icons.ttf", "BootstrapIcons"))
			.UseSkiaSharpGtk4();

		builder.Services.AddSingleton<Services.SampleService>();
		return builder.Build();
	}
}

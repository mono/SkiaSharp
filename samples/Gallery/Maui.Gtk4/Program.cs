using System.Runtime.Versioning;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platforms.Linux.Gtk4.Platform;

namespace SkiaSharpSample;

[SupportedOSPlatform("linux")]
public sealed class Program : GtkMauiApplication
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	public static void Main(string[] args) => new Program().Run(args);
}

using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platforms.Linux.Gtk4.Platform;
#if DEBUG
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DevFlow.Agent.Core;
#endif

namespace SkiaSharpSample;

[UnconditionalSuppressMessage("Interoperability", "CA1416",
	Justification = "Gtk4Runtime resolves the pinned backend's Linux library names on Windows and macOS.")]
public sealed class Program : GtkMauiApplication
{
	protected override bool CreateDesktopEntry => OperatingSystem.IsLinux();

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

#if DEBUG
	protected override void OnStarted()
	{
		var app = (App)Application;
		Services.GetRequiredService<MauiDevFlowAgentService>().Start(app, app.Dispatcher);
	}
#endif

	public static void Main(string[] args)
	{
		Gtk4Runtime.Initialize();
		new Program().Run(args);
	}
}

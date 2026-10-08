using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platforms.MacOS.Essentials;
using Microsoft.Maui.Platforms.MacOS.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace SkiaSharpSample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp() =>
		MauiApp.CreateBuilder()
			.UseMauiAppMacOS<App>()
			.AddMacOSEssentials()
			.UseSkiaSharpMacOS()
			.Build();
}

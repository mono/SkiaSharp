using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platforms.MacOS.Essentials;
using Microsoft.Maui.Platforms.MacOS.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SkiaSharpSample.Services;
#if DEBUG
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace SkiaSharpSample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiAppMacOS<App>()
            .AddMacOSEssentials()
            .ConfigureFonts(fonts => fonts.AddFont("bootstrap-icons.ttf", "BootstrapIcons"))
            .UseSkiaSharpMacOS();
        builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<Layout, GalleryLayoutHandler>());
#if DEBUG
        builder.AddMauiDevFlowAgent();
#endif
        builder.Services.AddSingleton<SampleService>();
        return builder.Build();
    }
}

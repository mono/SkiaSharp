using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
#if DEBUG
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace SkiaSharpSample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => fonts.AddFont("bootstrap-icons.ttf", "BootstrapIcons"))
            .UseSkiaSharp();
#if DEBUG
        builder.AddMauiDevFlowAgent();
#endif
        builder.Services.AddSingleton<Services.SampleService>();
        return builder.Build();
    }
}

using Microsoft.Maui.Controls.Hosting.WPF;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platforms.Windows.WPF.Essentials;
using SkiaSharp.Views.Maui.Controls.Hosting;
#if DEBUG
using Microsoft.Maui.DevFlow.Agent.WPF;
#endif

namespace SkiaSharpSample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiAppWPF<App>()
            .UseWPFEssentials()
            .ConfigureFonts(fonts => fonts.AddFont("bootstrap-icons.ttf", "BootstrapIcons"))
            .UseSkiaSharpWPF();

        WPFGalleryGlyphs.Register();
#if DEBUG
        // The preview agent shares one log file across apps.
        builder.AddMauiDevFlowAgent(options => options.EnableFileLogging = false);
#endif
        builder.Services.AddSingleton<Services.SampleService>();
        return builder.Build();
    }
}

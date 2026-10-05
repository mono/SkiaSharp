using Microsoft.Maui.Controls.Hosting.WPF;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platforms.Windows.WPF.Essentials;
using SkiaSharp.Views.Maui.Controls.Hosting;

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

        WpfGalleryGlyphs.Register();
        builder.Services.AddSingleton<Services.SampleService>();
        return builder.Build();
    }
}

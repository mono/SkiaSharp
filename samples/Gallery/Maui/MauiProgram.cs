using Microsoft.Maui.Hosting;
using Microsoft.Maui.Handlers;
using SkiaSharpSample.Controls;
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
        EntryHandler.Mapper.AppendToMapping("GalleryBorderlessSearch", (handler, view) =>
        {
            if (view is not GallerySearchEntry) return;
#if IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
            handler.PlatformView.Layer.BorderWidth = 0;
#endif
        });
#if DEBUG
        builder.AddMauiDevFlowAgent();
#endif
        builder.Services.AddSingleton<Services.SampleService>();
        return builder.Build();
    }
}

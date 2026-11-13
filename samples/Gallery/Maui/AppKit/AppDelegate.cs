using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Platforms.MacOS.Platform;

namespace SkiaSharpSample;

[Register("SkiaSharpGalleryAppKit")]
public sealed class AppDelegate : MacOSMauiApplication
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

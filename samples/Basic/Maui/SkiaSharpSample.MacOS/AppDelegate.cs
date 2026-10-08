using Foundation;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platforms.MacOS.Platform;

namespace SkiaSharpSample;

[Register("SkiaSharpBasicAppKit")]
public sealed class AppDelegate : MacOSMauiApplication
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

using Foundation;
using UIKit;
using Uno.UI.Hosting;
using Uno.UI.Runtime.AppleUIKit;

namespace SkiaSharpSample.iOS;

public class EntryPoint
{
    // This is the main entry point of the application.
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        var host = UnoPlatformHostBuilder.Create()
            .App(() => new App())
            .UseAppleUIKit()
            .Build();

        host.Run();
    }
}

[Register("SceneDelegate")]
public class SceneDelegate : UnoUISceneDelegate
{
}

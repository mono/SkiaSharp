using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platforms.Windows.WPF;

namespace SkiaSharpSample;

public sealed class WpfApp : MauiWPFApplication
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

public static class Program
{
    [STAThread]
    public static void Main() => new WpfApp().Run();
}

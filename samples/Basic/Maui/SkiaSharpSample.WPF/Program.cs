using Microsoft.Maui.Platforms.Windows.WPF;

namespace SkiaSharpSample;

public sealed class Program : MauiWPFApplication
{
	[STAThread]
	public static void Main() => new Program().Run();

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

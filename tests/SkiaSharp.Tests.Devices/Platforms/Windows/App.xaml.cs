using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml;

namespace SkiaSharp.Tests.WinUI
{
	public partial class App : MauiWinUIApplication
	{
		public App()
		{
			StartupDiagnostics.LogProcessStart();
			InitializeComponent();
		}

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

		protected override void OnLaunched(LaunchActivatedEventArgs args)
		{
			StartupDiagnostics.Write("startup", "OnLaunched");
			base.OnLaunched(args);
			StartupDiagnostics.Write("startup", "OnLaunched completed");
		}
	}
}

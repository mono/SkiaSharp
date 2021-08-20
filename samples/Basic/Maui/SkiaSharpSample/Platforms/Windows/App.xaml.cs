using Microsoft.Maui;
using Microsoft.UI.Xaml;

namespace SkiaSharpSample.WinUI
{
	public partial class App : MauiWinUIApplication
	{
		public App()
		{
			InitializeComponent();
		}

		protected override IStartup OnCreateStartup() => new Startup();

		protected override void OnLaunched(LaunchActivatedEventArgs args)
		{
			base.OnLaunched(args);

			Microsoft.Maui.Essentials.Platform.OnLaunched(args);
		}
	}
}

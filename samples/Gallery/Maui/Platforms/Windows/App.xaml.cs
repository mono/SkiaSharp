using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml;

namespace SkiaSharpSample.WinUI
{
	public partial class App : MauiWinUIApplication
	{
		public App()
		{
			InitializeComponent();
		}

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}

using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SkiaSharpSample
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
		}

		protected override Window CreateWindow(IActivationState activationState) =>
			new Window
			{
				Title = "SkiaSharp Sample",
				Page = new MainPage()
			};
	}
}

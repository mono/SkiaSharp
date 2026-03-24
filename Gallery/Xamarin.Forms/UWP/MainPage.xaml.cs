using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace SkiaSharpSample.Platform
{
	public sealed partial class MainPage
	{
		public MainPage()
		{
			if (ApiInformation.IsTypePresent(typeof(StatusBar).FullName))
			{
				var statusBar = StatusBar.GetForCurrentView();
				statusBar.BackgroundColor = Color.FromArgb(0xff, 0x34, 0x98, 0xdb);
				statusBar.ForegroundColor = Colors.White;
				statusBar.BackgroundOpacity = 1;
			}

			if (ApiInformation.IsTypePresent(typeof(ApplicationViewTitleBar).FullName))
			{
				var titlebar = ApplicationView.GetForCurrentView().TitleBar;
				titlebar.BackgroundColor = Color.FromArgb(0xff, 0x34, 0x98, 0xdb);
				titlebar.ForegroundColor = Colors.White;
				titlebar.ButtonBackgroundColor = Color.FromArgb(0xff, 0x34, 0x98, 0xdb);
				titlebar.ButtonForegroundColor = Colors.White;
				titlebar.ButtonHoverBackgroundColor = Color.FromArgb(0xff, 0x2c, 0x3e, 0x50);
				titlebar.ButtonHoverForegroundColor = Colors.White;
			}

			SamplesInitializer.Init();

			this.InitializeComponent();

			LoadApplication(new SkiaSharpSample.App());
		}
	}
}

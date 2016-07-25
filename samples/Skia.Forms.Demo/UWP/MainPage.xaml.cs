using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;

namespace Skia.Forms.Demo.UWP
{
    public sealed partial class MainPage
	{
		public MainPage ()
		{
			if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(typeof(Windows.UI.ViewManagement.StatusBar).FullName))
			{
				var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
				statusBar.BackgroundColor = Color.FromArgb(0xff, 0x34, 0x98, 0xdb);
				statusBar.ForegroundColor = Colors.White;
				statusBar.BackgroundOpacity = 1;
			}

			if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(typeof(Windows.UI.ViewManagement.ApplicationViewTitleBar).FullName))
			{
				var titlebar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
				titlebar.BackgroundColor = Color.FromArgb(0xff, 0x34, 0x98, 0xdb);
				titlebar.ForegroundColor = Colors.White;
				titlebar.ButtonBackgroundColor = Color.FromArgb(0xff, 0x34, 0x98, 0xdb);
				titlebar.ButtonForegroundColor = Colors.White;
				titlebar.ButtonHoverBackgroundColor = Color.FromArgb(0xff, 0x2c, 0x3e, 0x50);
				titlebar.ButtonHoverForegroundColor = Colors.White;
			}

			// set up resource paths
			string fontName = "content-font.ttf";
			SkiaSharp.Demos.CustomFontPath = Path.Combine (Package.Current.InstalledLocation.Path, "Assets", fontName);
			SkiaSharp.Demos.WorkingDirectory = ApplicationData.Current.LocalFolder.Path;
			SkiaSharp.Demos.OpenFileDelegate =
				async name =>
				{
					var file = await ApplicationData.Current.LocalFolder.GetFileAsync(name);
					await Windows.System.Launcher.LaunchFileAsync(file);
				};

			this.InitializeComponent ();

			LoadApplication (new Skia.Forms.Demo.App ());
		}
	}
}

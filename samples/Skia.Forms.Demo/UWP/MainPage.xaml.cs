using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Skia.Forms.Demo.UWP
{
    public sealed partial class MainPage
	{
		public MainPage ()
		{
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

using System.IO;
using Windows.ApplicationModel;

namespace Skia.Forms.Demo.UWP
{
    public sealed partial class MainPage
	{
		public MainPage ()
		{
			// set up resource paths
			string fontName = "content-font.ttf";
			SkiaSharp.Demos.CustomFontPath = Path.Combine (Package.Current.InstalledLocation.Path, "Assets", fontName);

			this.InitializeComponent ();

			LoadApplication (new Skia.Forms.Demo.App ());
		}
	}
}

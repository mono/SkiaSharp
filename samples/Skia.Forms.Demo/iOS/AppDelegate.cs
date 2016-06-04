using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using System.IO;

namespace Skia.Forms.Demo.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// set up resource paths
			string fontName = "content-font.ttf";
			SkiaSharp.Demos.CustomFontPath = NSBundle.MainBundle.PathForResource (Path.GetFileNameWithoutExtension (fontName), Path.GetExtension (fontName));
			var dir = Path.Combine (Path.GetTempPath (), "SkiaSharp.Demos", Path.GetRandomFileName ());
			if (!Directory.Exists (dir))
			{
				Directory.CreateDirectory (dir);
			}
			SkiaSharp.Demos.WorkingDirectory = dir;
			SkiaSharp.Demos.OpenFileDelegate = path =>
			{
				var nav = Xamarin.Forms.Platform.iOS.Platform.GetRenderer (Xamarin.Forms.Application.Current.MainPage) as UINavigationController;
				var vc = nav.VisibleViewController;
				var resourceToOpen = NSUrl.FromFilename (Path.Combine (dir, path));
				var controller = UIDocumentInteractionController.FromUrl (resourceToOpen);
				if (!controller.PresentOpenInMenu (vc.View.Bounds, vc.View, true))
				{
					new UIAlertView ("SkiaSharp", "Unable to open file.", null, "OK").Show ();
				}
			};

			global::Xamarin.Forms.Forms.Init ();

			LoadApplication (new App ());

			return base.FinishedLaunching (app, options);
		}
	}
}


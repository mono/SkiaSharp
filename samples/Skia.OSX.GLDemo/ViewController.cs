using System;

using AppKit;
using Foundation;
using System.IO;

namespace Skia.OSX.Demo
{
	public partial class ViewController : NSViewController
	{
		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

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
				if (!NSWorkspace.SharedWorkspace.OpenFile (Path.Combine (dir, path)))
				{
					NSAlert.WithMessage ("SkiaSharp", "OK", null, null, "Unable to open file.").RunSheetModal (View.Window);
				}
			};

			PopUpButton.AddItems (SkiaSharp.Demos.SamplesForPlatform (SkiaSharp.Demos.Platform.OSX | SkiaSharp.Demos.Platform.OpenGL));
			PopUpButton.SelectItem (0);
			SkiaView.Sample = SkiaSharp.Demos.GetSample (PopUpButton.SelectedItem.Title);
		}

		partial void PopUpButtonAction (NSObject sender)
		{
			SkiaView.Sample = SkiaSharp.Demos.GetSample (PopUpButton.SelectedItem.Title);
		}

		public override NSObject RepresentedObject {
			get {
				return base.RepresentedObject;
			}
			set {
				base.RepresentedObject = value;
				// Update the view, if already loaded.
			}
		}
	}
}

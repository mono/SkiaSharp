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

			PopUpButton.AddItems (SkiaSharp.Demos.SamplesForPlatform (SkiaSharp.Demos.Platform.OSX));
			PopUpButton.SelectItem (0);
			SkiaView.OnDrawCallback = SkiaSharp.Demos.MethodForSample (PopUpButton.SelectedItem.Title);
		}

		partial void PopUpButtonAction (NSObject sender)
		{
			SkiaView.OnDrawCallback = SkiaSharp.Demos.MethodForSample (PopUpButton.SelectedItem.Title);
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

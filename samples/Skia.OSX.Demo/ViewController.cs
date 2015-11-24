using System;

using AppKit;
using Foundation;

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
			PopUpButton.AddItems (new [] {"Xamagon", "Text", "Gradient"});
			PopUpButton.SelectItem (0);
		}

		partial void PopUpButtonAction (NSObject sender)
		{
			switch (PopUpButton.SelectedItem.Title) {
			case "Xamagon":
				SkiaView.OnDrawCallback = Skia.Forms.Demo.DrawHelpers.DrawXamagon;
				break;
			case "Text":
				SkiaView.OnDrawCallback = Skia.Forms.Demo.DrawHelpers.TextSample;
				break;
			case "Gradient":
				SkiaView.OnDrawCallback = Skia.Forms.Demo.DrawHelpers.DrawGradient;
				break;
			}
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

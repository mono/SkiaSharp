// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Skia.OSX.Demo
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSPopUpButton PopUpButton { get; set; }

		[Outlet]
		Skia.OSX.Demo.SkaiView SkiaView { get; set; }

		[Action ("PopUpButtonAction:")]
		partial void PopUpButtonAction (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (PopUpButton != null) {
				PopUpButton.Dispose ();
				PopUpButton = null;
			}

			if (SkiaView != null) {
				SkiaView.Dispose ();
				SkiaView = null;
			}
		}
	}
}

// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using AppKit;
using System.CodeDom.Compiler;

namespace SkiaSharpSample
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		SkiaSharp.Views.Mac.SKGLView skiaView { get; set; }

		[Action("OnComplexityChanged:")]
		partial void OnComplexityChanged(NSSlider sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (skiaView != null) {
				skiaView.Dispose ();
				skiaView = null;
			}
		}
	}
}

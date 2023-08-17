// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace SkiaSharpSample;

[Register ("ViewController")]
partial class ViewController
{
	[Outlet]
	[GeneratedCode ("iOS Designer", "1.0")]
	SkiaSharp.Views.tvOS.SKCanvasView skiaView { get; set; }

	void ReleaseDesignerOutlets ()
	{
		if (skiaView != null) {
			skiaView.Dispose ();
			skiaView = null;
		}
	}
}

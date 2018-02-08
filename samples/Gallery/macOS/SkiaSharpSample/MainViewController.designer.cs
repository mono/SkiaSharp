// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SkiaSharpSample
{
	[Register ("MainViewController")]
	partial class MainViewController
	{
		[Outlet]
		SkiaSharp.Views.Mac.SKCanvasView canvas { get; set; }

		[Outlet]
		SkiaSharp.Views.Mac.SKGLView glview { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (canvas != null) {
				canvas.Dispose ();
				canvas = null;
			}

			if (glview != null) {
				glview.Dispose ();
				glview = null;
			}
		}
	}
}

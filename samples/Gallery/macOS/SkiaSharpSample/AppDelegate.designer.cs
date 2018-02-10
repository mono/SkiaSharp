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
	partial class AppDelegate
	{
		[Outlet]
		AppKit.NSMenuItem samplesMenu { get; set; }

		[Action("OnBackendChanged:")]
		partial void OnBackendChanged(Foundation.NSObject sender);

		[Action("OnPlaySamples:")]
		partial void OnPlaySamples(Foundation.NSObject sender);

		void ReleaseDesignerOutlets()
		{
			if (samplesMenu != null)
			{
				samplesMenu.Dispose();
				samplesMenu = null;
			}
		}
	}
}

// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Skia.tvOS.Demo
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIScrollView scrollView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        Skia.tvOS.Demo.SkiaView skiaView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (scrollView != null) {
                scrollView.Dispose ();
                scrollView = null;
            }

            if (skiaView != null) {
                skiaView.Dispose ();
                skiaView = null;
            }
        }
    }
}
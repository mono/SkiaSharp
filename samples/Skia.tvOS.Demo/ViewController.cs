using System;
using System.IO;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Skia.tvOS.Demo
{
	public partial class ViewController : UIViewController
	{
		public ViewController (IntPtr handle)
			: base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// set up resource paths
			string fontName = "content-font.ttf";
			SkiaSharp.Demos.CustomFontPath = NSBundle.MainBundle.PathForResource (Path.GetFileNameWithoutExtension (fontName), Path.GetExtension (fontName));

			var demos = SkiaSharp.Demos.SamplesForPlatform (SkiaSharp.Demos.Platform.tvOS);
			var y = 100 + 12;
			foreach (var demo in demos) {
				var button = new UIButton (UIButtonType.RoundedRect);
				button.Frame = new CGRect (12, y, scrollView.Bounds.Width - 12 - 12, 100);
				y += 100 + 12;
				button.SetTitle (demo, UIControlState.Normal);
				button.PrimaryActionTriggered += ButtonTapped;

				if (skiaView.OnDrawCallback == null) {
					skiaView.OnDrawCallback = SkiaSharp.Demos.MethodForSample (demo);
				}

				scrollView.AddSubview (button);
			}

			scrollView.ContentSize = new CGSize (scrollView.Bounds.Width, y + 100 + 12);
		}

		private void ButtonTapped (object sender, EventArgs e)
		{
			var title = ((UIButton)sender).Title (UIControlState.Normal);
			skiaView.OnDrawCallback = SkiaSharp.Demos.MethodForSample (title);
		}
	}
}

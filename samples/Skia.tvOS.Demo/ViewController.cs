using System;
using System.IO;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Skia.tvOS.Demo
{
	public partial class ViewController : UIViewController
	{
		private UIButton tapButton;

		public ViewController(IntPtr handle)
			: base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			tapButton = new UIButton(UIButtonType.RoundedRect)
			{
				Frame = new CGRect(scrollView.Bounds.Right + 12, 0, 300, 100),
			};
			tapButton.SetTitle("Action", UIControlState.Normal);
			tapButton.PrimaryActionTriggered += OnTapped;
			View.AddSubview(tapButton);

			// set up resource paths
			string fontName = "content-font.ttf";
			SkiaSharp.Demos.CustomFontPath = NSBundle.MainBundle.PathForResource(Path.GetFileNameWithoutExtension(fontName), Path.GetExtension(fontName));
			var dir = Path.Combine(Path.GetTempPath(), "SkiaSharp.Demos", Path.GetRandomFileName());
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			SkiaSharp.Demos.WorkingDirectory = dir;
			SkiaSharp.Demos.OpenFileDelegate = path =>
			{
				var resourceToOpen = NSUrl.FromFilename(Path.Combine(dir, path));

				// TODO: try open file

				var alert = UIAlertController.Create("SkiaSharp", "Unable to open file.", UIAlertControllerStyle.Alert);
				alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
				PresentViewController(alert, true, null);
			};

			var demos = SkiaSharp.Demos.SamplesForPlatform(SkiaSharp.Demos.Platform.tvOS);
			var y = 100 + 12;
			foreach (var demo in demos)
			{
				var button = new UIButton(UIButtonType.RoundedRect);
				button.Frame = new CGRect(12, y, scrollView.Bounds.Width - 12 - 12, 100);
				y += 100 + 12;
				button.SetTitle(demo, UIControlState.Normal);
				button.PrimaryActionTriggered += ButtonTapped;

				if (skiaView.Sample == null)
				{
					// tap the first button so there is something on the screen
					ButtonTapped(button, EventArgs.Empty);
				}

				scrollView.AddSubview(button);
			}

			scrollView.ContentSize = new CGSize(scrollView.Bounds.Width, y + 100 + 12);
		}

		private void OnTapped(object sender, EventArgs e)
		{
			skiaView.Sample?.TapMethod?.Invoke();
		}

		private void ButtonTapped(object sender, EventArgs e)
		{
			var button = (UIButton)sender;
			var title = button.Title(UIControlState.Normal);
			skiaView.Sample = SkiaSharp.Demos.GetSample(title);

			var newBounds = tapButton.Frame;
			newBounds.Y = button.Frame.Top - scrollView.ContentOffset.Y;
			tapButton.Frame = newBounds;
			tapButton.Hidden = skiaView.Sample?.TapMethod == null;
		}
	}
}

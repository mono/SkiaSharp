using System;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;
using SkiaSharp;
using SkiaSharp.Views.tvOS;

namespace SkiaSharpSample
{
	public partial class ViewController : UIViewController
	{
		private const int ButtonHeight = 100;
		private const int Spacing = 12;
		private const int Padding = 100;

		private SampleBase currentSample;

		public ViewController(IntPtr handle)
			: base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			canvas.PaintSurface += OnPaintSample;

			// initialize all the samples
			SamplesInitializer.Init();

			// get our samples
			var samples = SamplesManager.GetSamples(SamplePlatforms.tvOS).ToArray();

			// the "moving" button frame
			var frame = new CGRect(
				Spacing, Padding + Spacing,
				scrollview.Bounds.Width - (Spacing * 2), ButtonHeight);

			foreach (var sample in samples)
			{
				// create and add the sample button
				var button = new UIButton(UIButtonType.RoundedRect);
				button.Frame = frame;
				button.SetTitle(sample.Title, UIControlState.Normal);
				button.PrimaryActionTriggered += (sender, e) => OnSampleSelected(sample);
				scrollview.AddSubview(button);

				// prepare for next button
				frame.Y += ButtonHeight + Spacing;
			}

			// set the scroll bounds
			frame.Y += Spacing + Padding;
			scrollview.ContentSize = new CGSize(scrollview.Bounds.Width, frame.Bottom);

			// select the "first" sample
			var showcase = samples.First(s => s.Category.HasFlag(SampleCategories.Showcases));
			OnSampleSelected(showcase);
		}

		private void OnPaintSample(object sender, SKPaintSurfaceEventArgs e)
		{
			// dar the sample on the canvas
			currentSample?.DrawSample(e.Surface.Canvas, e.Info.Width, e.Info.Height);
		}

		private void OnSampleSelected(SampleBase sample)
		{
			// clean up the old sample
			if (currentSample != null)
			{
				currentSample.RefreshRequested -= OnRefreshRequested;
				currentSample.Destroy();
			}

			// update the selected sample
			currentSample = sample;

			// prepare the sample
			if (sample != null)
			{
				sample.RefreshRequested += OnRefreshRequested;
				sample.Init();
			}

			// refresh the view
			OnRefreshRequested(null, null);
		}

		private void OnRefreshRequested(object sender, EventArgs e)
		{
			canvas.SetNeedsDisplay();
		}
	}
}

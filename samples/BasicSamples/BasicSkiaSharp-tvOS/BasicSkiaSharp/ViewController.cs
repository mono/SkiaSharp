using System;
using CoreGraphics;
using SkiaSharp;
using SkiaSharp.Views;
using UIKit;

namespace BasicSkiaSharp
{
	public partial class ViewController : UIViewController, ISKLayerDelegate, ISKGLLayerDelegate
	{
		private MySoftwareView softwareSkiaView;

		private SKLayer softwareSkiaLayer;
		private UIView softwareSkiaLayerView;

		private MyHardwareView hardwareSkiaView;

		private SKGLLayer hardwareSkiaLayer;
		private UIView hardwareSkiaLayerView;

		private UILabel softwareLabel;
		private UILabel hardwareLabel;
		private UILabel layerLabel;
		private UILabel viewLabel;

		public ViewController(IntPtr handle)
			: base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// create the labels
			var ninety = (float)Math.PI / -2f;
			var font = UIFont.BoldSystemFontOfSize(18);
			softwareLabel = new UILabel
			{
				Text = "Software",
				TextAlignment = UITextAlignment.Center,
				Font = font
			};
			View.AddSubview(softwareLabel);
			hardwareLabel = new UILabel
			{
				Text = "Hardware",
				TextAlignment = UITextAlignment.Center,
				Font = font
			};
			View.AddSubview(hardwareLabel);
			layerLabel = new UILabel
			{
				Text = "Layer",
				Transform = CGAffineTransform.MakeRotation(ninety),
				TextAlignment = UITextAlignment.Center,
				Font = font
			};
			View.AddSubview(layerLabel);
			viewLabel = new UILabel
			{
				Text = "View",
				Transform = CGAffineTransform.MakeRotation(ninety),
				TextAlignment = UITextAlignment.Center,
				Font = font
			};
			View.AddSubview(viewLabel);

			// create a custom software view
			softwareSkiaView = new MySoftwareView();
			softwareSkiaView.Opaque = false;
			View.AddSubview(softwareSkiaView);

			// create a custom hardware view
			hardwareSkiaView = new MyHardwareView();
			hardwareSkiaView.EnableSetNeedsDisplay = true;
			View.AddSubview(hardwareSkiaView);

			// add a software layer
			softwareSkiaLayerView = new UIView();
			View.AddSubview(softwareSkiaLayerView);
			softwareSkiaLayer = new SKLayer
			{
				ContentsScale = UIScreen.MainScreen.Scale,
				SKDelegate = this
			};
			softwareSkiaLayerView.Layer.AddSublayer(softwareSkiaLayer);

			// add a hardware layer
			hardwareSkiaLayerView = new UIView();
			View.AddSubview(hardwareSkiaLayerView);
			hardwareSkiaLayer = new SKGLLayer
			{
				ContentsScale = UIScreen.MainScreen.Scale,
				SKDelegate = this
			};
			hardwareSkiaLayerView.Layer.AddSublayer(hardwareSkiaLayer);
		}

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();

			var headerHeight = 32f;
			var topHeader = new CGRect(headerHeight, 0, View.Bounds.Width - headerHeight, headerHeight);
			var leftHeader = new CGRect(0, headerHeight, headerHeight, View.Bounds.Height - headerHeight);
			var availableSpace = CGRect.FromLTRB(leftHeader.Right, topHeader.Bottom, View.Bounds.Width, View.Bounds.Height);
			var colWidth = availableSpace.Width / 2f;
			var rowHeight = availableSpace.Height / 2f;
			var inset = 12;

			// layout the various views

			// the labels
			softwareLabel.Frame = new CGRect(topHeader.X, topHeader.Y, colWidth, topHeader.Height);
			hardwareLabel.Frame = new CGRect(topHeader.X + colWidth, topHeader.Y, colWidth, topHeader.Height);
			viewLabel.Frame = new CGRect(leftHeader.X, topHeader.Bottom, leftHeader.Width, rowHeight);
			layerLabel.Frame = new CGRect(leftHeader.X, topHeader.Bottom + rowHeight, leftHeader.Width, rowHeight);

			// the software view
			softwareSkiaView.Frame = new CGRect(availableSpace.X, availableSpace.Y, colWidth, rowHeight).Inset(inset, inset);

			// the software layer
			softwareSkiaLayerView.Frame = new CGRect(availableSpace.X, availableSpace.Y + rowHeight, colWidth, rowHeight).Inset(inset, inset);
			softwareSkiaLayer.Frame = softwareSkiaLayerView.Bounds;

			// the hardware view
			hardwareSkiaView.Frame = new CGRect(availableSpace.X + colWidth, availableSpace.Y, colWidth, rowHeight).Inset(inset, inset);

			// the hardware layer
			hardwareSkiaLayerView.Frame = new CGRect(availableSpace.X + colWidth, availableSpace.Y + rowHeight, colWidth, rowHeight).Inset(inset, inset);
			hardwareSkiaLayer.Frame = hardwareSkiaLayerView.Bounds;
		}

		// the real draw method
		private static void Draw(SKSurface surface, SKSize size)
		{
			const int stroke = 4;
			const int curve = 20;
			const int textSize = 60;
			const int shrink = stroke / -2;

			var canvas = surface.Canvas;

			canvas.Clear(SKColors.Transparent);

			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;
				paint.TextSize = textSize;

				paint.Color = SKColors.Orchid;
				var r = SKRect.Create(SKPoint.Empty, size);
				canvas.DrawRoundRect(r, curve, curve, paint);

				paint.Color = SKColors.GreenYellow;
				canvas.DrawText("Hello tvOS World!", 30, textSize + 10, paint);

				paint.Color = SKColors.Orange.WithAlpha(100);
				canvas.DrawOval(SKRect.Create(50, 50, 100, 100), paint);

				paint.IsStroke = true;
				paint.StrokeWidth = stroke;
				paint.Color = SKColors.Black;
				r.Inflate(shrink, shrink);
				canvas.DrawRoundRect(r, curve - stroke, curve - stroke, paint);
			}
		}

		// drawing for the software layer
		public void DrawInSurface(SKSurface surface, SKImageInfo info)
		{
			Draw(surface, info.Size);
		}

		// drawing for the hardware layer
		public void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
			Draw(surface, new SKSize(renderTarget.Width, renderTarget.Height));
		}

		// the custom view
		private class MySoftwareView : SKView
		{
			public override void Draw(SKSurface surface, SKImageInfo info)
			{
				base.Draw(surface, info);

				ViewController.Draw(surface, info.Size);
			}
		}

		// the custom view
		private class MyHardwareView : SKGLView
		{
			public override void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
			{
				base.DrawInSurface(surface, renderTarget);

				ViewController.Draw(surface, new SKSize(renderTarget.Width, renderTarget.Height));
			}
		}
	}
}


using System;
using System.Runtime.InteropServices;
using AppKit;
using CoreGraphics;
using Foundation;
using SkiaSharp;
using SkiaSharp.Views;

namespace BasicSkiaSharp
{
	public partial class MainWindow : NSWindow, ISKLayerDelegate, ISKGLLayerDelegate
	{
		private MySoftwareView softwareSkiaView;
		private NSView softwareSkiaLayerView;
		private MyHardwareView hardwareSkiaView;
		private NSView hardwareSkiaLayerView;

		private NSTextField softwareLabel;
		private NSTextField hardwareLabel;
		private NSTextField layerLabel;
		private NSTextField viewLabel;

		public MainWindow(IntPtr handle)
			: base(handle)
		{
		}

		[Export("initWithCoder:")]
		public MainWindow(NSCoder coder)
			: base(coder)
		{
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			// create the labels
			var font = NSFont.BoldSystemFontOfSize(18);
			softwareLabel = new NSTextField
			{
				StringValue = "Software",
				Alignment = NSTextAlignment.Center,
				Font = font,
				Bezeled = false,
				DrawsBackground = false,
				Editable = false,
				Selectable = false,
			};
			ContentView.AddSubview(softwareLabel);
			hardwareLabel = new NSTextField
			{
				StringValue = "Hardware",
				Alignment = NSTextAlignment.Center,
				Font = font,
				Bezeled = false,
				DrawsBackground = false,
				Editable = false,
				Selectable = false,
			};
			ContentView.AddSubview(hardwareLabel);
			layerLabel = new NSTextField
			{
				StringValue = "Layer",
				Alignment = NSTextAlignment.Center,
				Font = font,
				Bezeled = false,
				DrawsBackground = false,
				Editable = false,
				Selectable = false,
			};
			layerLabel.RotateByAngle(-90);
			ContentView.AddSubview(layerLabel);
			viewLabel = new NSTextField
			{
				StringValue = "View",
				Alignment = NSTextAlignment.Center,
				Font = font,
				Bezeled = false,
				DrawsBackground = false,
				Editable = false,
				Selectable = false,
			};
			viewLabel.RotateByAngle(-90);
			ContentView.AddSubview(viewLabel);

			// create a custom software view
			softwareSkiaView = new MySoftwareView();
			ContentView.AddSubview(softwareSkiaView);

			// create a custom hardware view
			hardwareSkiaView = new MyHardwareView();
			ContentView.AddSubview(hardwareSkiaView);

			// add a software layer
			softwareSkiaLayerView = new NSView();
			softwareSkiaLayerView.Layer = new SKLayer
			{
				ContentsScale = BackingScaleFactor,
				SKDelegate = this
			};
			softwareSkiaLayerView.WantsLayer = true;
			ContentView.AddSubview(softwareSkiaLayerView);

			// add a hardware layer
			hardwareSkiaLayerView = new NSView();
			hardwareSkiaLayerView.Layer = new SKGLLayer
			{
				ContentsScale = BackingScaleFactor,
				SKDelegate = this
			};
			hardwareSkiaLayerView.WantsLayer = true;
			ContentView.AddSubview(hardwareSkiaLayerView);

			DidResize += delegate { LayoutSubviews(); };
			LayoutSubviews();
		}

		private void LayoutSubviews()
		{
			var headerHeight = 32f;
			var topHeader = new CGRect(headerHeight, 0, ContentView.Bounds.Width - headerHeight, headerHeight);
			var leftHeader = new CGRect(0, headerHeight, headerHeight, ContentView.Bounds.Height - headerHeight);
			var availableSpace = CGRect.FromLTRB(leftHeader.Right, topHeader.Bottom, ContentView.Bounds.Width, ContentView.Bounds.Height);
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

			// the hardware view
			hardwareSkiaView.Frame = new CGRect(availableSpace.X + colWidth, availableSpace.Y, colWidth, rowHeight).Inset(inset, inset);

			// the hardware layer
			hardwareSkiaLayerView.Frame = new CGRect(availableSpace.X + colWidth, availableSpace.Y + rowHeight, colWidth, rowHeight).Inset(inset, inset);
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
				canvas.DrawText("Hello MacOS World!", 30, textSize + 10, paint);

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

				MainWindow.Draw(surface, info.Size);
			}
		}

		// the custom view
		private class MyHardwareView : SKGLView
		{
			public override void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
			{
				base.DrawInSurface(surface, renderTarget);

				MainWindow.Draw(surface, new SKSize(renderTarget.Width, renderTarget.Height));
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using UIKit;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas : FrameworkElement
	{
		private SKCGSurfaceFactory drawable;
		private bool designMode;

		public SKXamlCanvas()
		{
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
			SizeChanged += OnSizeChanged;

			RegisterPropertyChangedCallback(VisibilityProperty, (s, e) => OnVisibilityChanged(s));
			OnVisibilityChanged(this);
			Initialize();
		}

		private SKSize GetCanvasSize() => drawable?.Info.Size ?? SKSize.Empty;

		private static bool GetIsInitialized() => true;

		private void Initialize()
		{
			if (designMode)
				return;

			drawable = new SKCGSurfaceFactory();
		}

		private void OnDpiChanged(DisplayInformation sender, object args = null)
		{
			Dpi = sender.LogicalDpi / 96.0f;
			Invalidate();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			var display = DisplayInformation.GetForCurrentView();
			display.DpiChanged += OnDpiChanged;

			OnDpiChanged(display);
			Invalidate();
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			var display = DisplayInformation.GetForCurrentView();
			display.DpiChanged -= OnDpiChanged;
		}

		private void DoInvalidate()
			=> base.SetNeedsDisplay();

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);

			if (designMode || drawable == null)
				return;

			using (var ctx = UIGraphics.GetCurrentContext())
			{
				// create the skia context
				SKImageInfo info;
				using (var surface = drawable.CreateSurface(Bounds, IgnorePixelScaling ? 1 : ContentScaleFactor, out info))
				{
					// draw on the image using SKiaSharp
					OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));

					// draw the surface to the context
					drawable.DrawSurface(ctx, Bounds, info, surface);
				}
			}
		}
	}
}

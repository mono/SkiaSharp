﻿using UIKit;
using CoreGraphics;
using SkiaSharp.Views.iOS;
using Windows.UI.Xaml;

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas : FrameworkElement
	{
		private SKCGSurfaceFactory drawable;

		public SKXamlCanvas()
		{
			Initialize();
		}

		partial void DoLoaded() =>
			drawable = new SKCGSurfaceFactory();

		partial void DoUnloaded() =>
			drawable?.Dispose();

		private SKSize GetCanvasSize() =>
			drawable?.Info.Size ?? SKSize.Empty;

		private void DoInvalidate() =>
			SetNeedsDisplay();

		public override void Draw(CGRect dirtyRect)
		{
			base.Draw(dirtyRect);

			if (designMode || !isVisible || drawable == null)
				return;

			// create the skia context
			using var surface = drawable.CreateSurface(Bounds, IgnorePixelScaling ? 1 : ContentScaleFactor, out var info);
			if (info.Width == 0 || info.Height == 0)
				return;

			using var ctx = UIGraphics.GetCurrentContext();

			// draw on the image using SKiaSharp
			OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));

			// draw the surface to the context
			drawable.DrawSurface(ctx, Bounds, info, surface);
		}
	}
}

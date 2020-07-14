using AppKit;
using CoreGraphics;
using SkiaSharp.Views.Mac;
using Windows.UI.Xaml;

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas : FrameworkElement
	{
		private SKCGSurfaceFactory drawable;

		partial void DoLoaded() =>
			drawable = new SKCGSurfaceFactory();

		partial void DoUnloaded() =>
			drawable?.Dispose();

		private SKSize GetCanvasSize() =>
			drawable?.Info.Size ?? SKSize.Empty;

		private void DoInvalidate() =>
			NeedsDisplay = true;

		public override void DrawRect(CGRect dirtyRect)
		{
			base.DrawRect(dirtyRect);
			
			if (designMode || !isVisible || drawable == null)
				return;

			// create the skia context
			using var surface = drawable.CreateSurface(Bounds, IgnorePixelScaling ? 1 : Window.BackingScaleFactor, out var info);
			if (info.Width == 0 || info.Height == 0)
				return;

			using var ctx = NSGraphicsContext.CurrentContext.CGContext;

			// draw on the image using SKiaSharp
			OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));

			// draw the surface to the context
			drawable.DrawSurface(ctx, Bounds, info, surface);
		}
	}
}

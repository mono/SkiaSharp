using UIKit;
using CoreGraphics;
using SkiaSharp.Views.iOS;
#if WINUI
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

#if WINDOWS || WINUI
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
{
	public partial class SKXamlCanvas
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

		private void DoInvalidate() =>
			SetNeedsDisplay();

		public override void Draw(CGRect dirtyRect)
		{
			base.Draw(dirtyRect);

			if (designMode || !isVisible || drawable == null)
				return;

			// create the skia context
			using var surface = drawable.CreateSurface(Bounds, ContentScaleFactor, out var info);
			if (info.Width == 0 || info.Height == 0)
			{
				CanvasSize = SKSize.Empty;
				return;
			}

			var userVisibleSize = IgnorePixelScaling
				? new SKSizeI((int)Bounds.Width, (int)Bounds.Height)
				: info.Size;

			CanvasSize = userVisibleSize;

			if (IgnorePixelScaling)
			{
				var skiaCanvas = surface.Canvas;
				skiaCanvas.Scale((float)ContentScaleFactor);
				skiaCanvas.Save();
			}

			using var ctx = UIGraphics.GetCurrentContext();

			// draw on the image using SKiaSharp
			OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));

			// draw the surface to the context
			drawable.DrawSurface(ctx, Bounds, info, surface);
		}

		public override void WillMoveToWindow(UIWindow window)
		{
			if (drawable != null)
			{
				// release the memory if we are leaving the window
				if (window == null)
					drawable?.Dispose();
				else
					SetNeedsDisplay();
			}

			base.WillMoveToWindow(window);
		}
	}
}

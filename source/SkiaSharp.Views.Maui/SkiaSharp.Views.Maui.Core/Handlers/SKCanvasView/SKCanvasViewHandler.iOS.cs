using Microsoft.Maui.Handlers;
using SkiaSharp.Views.iOS;
using SkiaSharp.Views.Maui.Platform;
using UIKit;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKCanvasViewHandler : ViewHandler<ISKCanvasView, SKCanvasView>
	{
		private SKSizeI lastCanvasSize;
		private SKTouchHandler? touchHandler;

		protected override SKCanvasView CreatePlatformView() => new SKCanvasView { BackgroundColor = UIColor.Clear };

		protected override void ConnectHandler(SKCanvasView platformView)
		{
			platformView.PaintSurface += OnPaintSurface;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(SKCanvasView platformView)
		{
			touchHandler?.Detach(platformView);
			touchHandler = null;

			platformView.PaintSurface -= OnPaintSurface;

			base.DisconnectHandler(platformView);
		}

		// Mapper actions / properties

		public static void OnInvalidateSurface(SKCanvasViewHandler handler, ISKCanvasView canvasView, object? args)
		{
			handler.PlatformView?.SetNeedsDisplay();
		}

		public static void MapIgnorePixelScaling(SKCanvasViewHandler handler, ISKCanvasView canvasView)
		{
			handler.PlatformView?.UpdateIgnorePixelScaling(canvasView);
		}

		public static void MapEnableTouchEvents(SKCanvasViewHandler handler, ISKCanvasView canvasView)
		{
			if (handler.PlatformView == null)
				return;

			handler.touchHandler ??= new SKTouchHandler(
				args => canvasView.OnTouch(args),
				(x, y) => handler.OnGetScaledCoord(x, y));

			handler.touchHandler?.SetEnabled(handler.PlatformView, canvasView.EnableTouchEvents);
		}

		// helper methods

		private void OnPaintSurface(object? sender, iOS.SKPaintSurfaceEventArgs e)
		{
			var newCanvasSize = e.Info.Size;
			if (lastCanvasSize != newCanvasSize)
			{
				lastCanvasSize = newCanvasSize;
				VirtualView?.OnCanvasSizeChanged(newCanvasSize);
			}

			VirtualView?.OnPaintSurface(new SKPaintSurfaceEventArgs(e.Surface, e.Info, e.RawInfo));
		}

		private SKPoint OnGetScaledCoord(double x, double y)
		{
			if (VirtualView?.IgnorePixelScaling == false && PlatformView != null)
			{
				var scale = PlatformView.ContentScaleFactor;

				x *= scale;
				y *= scale;
			}

			return new SKPoint((float)x, (float)y);
		}
	}
}

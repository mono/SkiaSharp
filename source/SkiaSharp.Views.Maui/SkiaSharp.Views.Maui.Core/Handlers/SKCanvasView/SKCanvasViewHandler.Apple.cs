using Microsoft.Maui.Handlers;
using SkiaSharp.Views.iOS;
using SkiaSharp.Views.Maui.Platform;
using UIKit;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKCanvasViewHandler : ViewHandler<ISKCanvasView, SKCanvasView>
	{
		private PaintSurfaceProxy? paintSurfaceProxy;
		private SKTouchHandlerProxy? touchProxy;

		protected override SKCanvasView CreatePlatformView() => new SKCanvasView { BackgroundColor = UIColor.Clear };

		protected override void ConnectHandler(SKCanvasView platformView)
		{
			paintSurfaceProxy = new();
			paintSurfaceProxy.Connect(VirtualView, platformView);
			touchProxy = new();
			touchProxy.Connect(VirtualView, platformView);

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(SKCanvasView platformView)
		{
			paintSurfaceProxy?.Disconnect(platformView);
			paintSurfaceProxy = null;
			touchProxy?.Disconnect(platformView);
			touchProxy = null;

			base.DisconnectHandler(platformView);
		}

		// Mapper actions / properties

		public static void OnInvalidateSurface(SKCanvasViewHandler handler, ISKCanvasView canvasView, object? args)
		{
			if (handler?.PlatformView == null)
				return;

			handler.PlatformView.SetNeedsDisplay();
		}

		public static void MapIgnorePixelScaling(SKCanvasViewHandler handler, ISKCanvasView canvasView)
		{
			if (handler?.PlatformView == null)
				return;

			handler.PlatformView.IgnorePixelScaling = canvasView.IgnorePixelScaling;
		}

		public static void MapEnableTouchEvents(SKCanvasViewHandler handler, ISKCanvasView canvasView)
		{
			if (handler?.PlatformView == null)
				return;

			handler.touchProxy?.UpdateEnableTouchEvents(handler.PlatformView, canvasView.EnableTouchEvents);
		}

		// helper methods

		private class PaintSurfaceProxy : SKEventProxy<ISKCanvasView, SKCanvasView>
		{
			private SKSizeI lastCanvasSize;

			protected override void OnConnect(ISKCanvasView virtualView, SKCanvasView platformView) =>
				platformView.PaintSurface += OnPaintSurface;

			protected override void OnDisconnect(SKCanvasView platformView) =>
				platformView.PaintSurface -= OnPaintSurface;

			private void OnPaintSurface(object? sender, iOS.SKPaintSurfaceEventArgs e)
			{
				if (VirtualView is not {} view)
					return;

				var newCanvasSize = e.Info.Size;
				if (lastCanvasSize != newCanvasSize)
				{
					lastCanvasSize = newCanvasSize;
					view.OnCanvasSizeChanged(newCanvasSize);
				}

				view.OnPaintSurface(new SKPaintSurfaceEventArgs(e.Surface, e.Info, e.RawInfo));
			}
		}
	}
}

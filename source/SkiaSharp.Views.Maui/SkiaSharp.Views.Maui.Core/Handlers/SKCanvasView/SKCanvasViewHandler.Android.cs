using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using SkiaSharp.Views.Android;
using SkiaSharp.Views.Maui.Platform;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKCanvasViewHandler : ViewHandler<ISKCanvasView, SKCanvasView>
	{
		private SKSizeI lastCanvasSize;
		private SKTouchHandler? touchHandler;

		protected override SKCanvasView CreatePlatformView() => new SKCanvasView(Context);

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
			handler.PlatformView?.Invalidate();
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

		private void OnPaintSurface(object? sender, Android.SKPaintSurfaceEventArgs e)
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
			if (VirtualView?.IgnorePixelScaling == true && Context != null)
			{
				x = Context.FromPixels(x);
				y = Context.FromPixels(y);
			}

			return new SKPoint((float)x, (float)y);
		}
	}
}

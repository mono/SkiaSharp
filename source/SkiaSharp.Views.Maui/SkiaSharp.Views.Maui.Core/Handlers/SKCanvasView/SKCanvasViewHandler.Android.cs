using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Android;
using SkiaSharp.Views.Maui.Platform;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKCanvasViewHandler : ViewHandler<ISKCanvasView, SKCanvasView>
	{
		private SKSizeI lastCanvasSize;
		private SKTouchHandler? touchHandler;

		protected override SKCanvasView CreateNativeView() => new SKCanvasView(Context);

		protected override void ConnectHandler(SKCanvasView nativeView)
		{
			nativeView.PaintSurface += OnPaintSurface;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(SKCanvasView nativeView)
		{
			touchHandler?.Detach(nativeView);
			touchHandler = null;

			nativeView.PaintSurface -= OnPaintSurface;

			base.DisconnectHandler(nativeView);
		}

		// Mapper actions / properties

		public static void OnInvalidateSurface(SKCanvasViewHandler handler, ISKCanvasView canvasView)
		{
			handler.NativeView?.Invalidate();
		}

		public static void MapIgnorePixelScaling(SKCanvasViewHandler handler, ISKCanvasView canvasView)
		{
			handler.NativeView?.UpdateIgnorePixelScaling(canvasView);
		}

		public static void MapEnableTouchEvents(SKCanvasViewHandler handler, ISKCanvasView canvasView)
		{
			if (handler.NativeView == null)
				return;

			handler.touchHandler ??= new SKTouchHandler(
				args => canvasView.OnTouch(args),
				(x, y) => handler.OnGetScaledCoord(x, y));

			handler.touchHandler?.SetEnabled(handler.NativeView, canvasView.EnableTouchEvents);
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

			VirtualView?.OnPaintSurface(new SKPaintSurfaceEventArgs(e.Surface, e.Info));
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

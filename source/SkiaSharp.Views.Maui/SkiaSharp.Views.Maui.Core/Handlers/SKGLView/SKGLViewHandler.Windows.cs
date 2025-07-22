using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Maui.Platform;
using SkiaSharp.Views.Windows;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGLViewHandler : ViewHandler<ISKGLView, SKSwapChainPanel>
	{
		private SKSizeI lastCanvasSize;
		private GRContext? lastGRContext;
		private SKTouchHandler? touchHandler;

		protected override SKSwapChainPanel CreatePlatformView() => new MauiSKSwapChainPanel();

		protected override void ConnectHandler(SKSwapChainPanel platformView)
		{
			platformView.PaintSurface += OnPaintSurface;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(SKSwapChainPanel platformView)
		{
			touchHandler?.Detach(platformView);
			touchHandler = null;

			platformView.PaintSurface -= OnPaintSurface;

			base.DisconnectHandler(platformView);
		}

		// Mapper actions / properties

		public static void OnInvalidateSurface(SKGLViewHandler handler, ISKGLView view, object? args)
		{
			if (handler?.PlatformView == null)
				return;

			if (!handler.PlatformView.EnableRenderLoop)
				handler.PlatformView.Invalidate();
		}

		public static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view)
		{
			if (handler?.PlatformView is not MauiSKSwapChainPanel pv)
				return;

			pv.IgnorePixelScaling = view.IgnorePixelScaling;
			pv.Invalidate();
		}

		public static void MapHasRenderLoop(SKGLViewHandler handler, ISKGLView view)
		{
			if (handler?.PlatformView == null)
				return;

			handler.PlatformView.EnableRenderLoop = view.HasRenderLoop;
		}

		public static void MapEnableTouchEvents(SKGLViewHandler handler, ISKGLView view)
		{
			if (handler?.PlatformView == null)
				return;

			handler.touchHandler ??= new SKTouchHandler(
				args => view.OnTouch(args),
				(x, y) => handler.OnGetScaledCoord(x, y));

			handler.touchHandler?.SetEnabled(handler.PlatformView, view.EnableTouchEvents);
		}

		public static void MapBackground(SKGLViewHandler handler, ISKGLView view)
		{
			// WinUI 3 limitation:
			// Setting 'Background' property is not supported on SwapChainPanel.'.
		}

		// helper methods

		private void OnPaintSurface(object? sender, Windows.SKPaintGLSurfaceEventArgs e)
		{
			var newCanvasSize = e.Info.Size;
			if (lastCanvasSize != newCanvasSize)
			{
				lastCanvasSize = newCanvasSize;
				VirtualView?.OnCanvasSizeChanged(newCanvasSize);
			}
			if (sender is SKSwapChainPanel platformView)
			{
				var newGRContext = platformView.GRContext;
				if (lastGRContext != newGRContext)
				{
					lastGRContext = newGRContext;
					VirtualView?.OnGRContextChanged(newGRContext);
				}
			}

			VirtualView?.OnPaintSurface(new SKPaintGLSurfaceEventArgs(e.Surface, e.BackendRenderTarget, e.Origin, e.Info, e.RawInfo));
		}

		private SKPoint OnGetScaledCoord(double x, double y)
		{
			if (VirtualView?.IgnorePixelScaling == false && PlatformView != null)
			{
				var scale = PlatformView.ContentsScale;

				x *= scale;
				y *= scale;
			}

			return new SKPoint((float)x, (float)y);
		}

		private class MauiSKSwapChainPanel : SKSwapChainPanel
		{
			public bool IgnorePixelScaling { get; set; }

			protected override void OnPaintSurface(Windows.SKPaintGLSurfaceEventArgs e)
			{
				if (IgnorePixelScaling)
				{
					var density = (float)ContentsScale;
					var userVisibleSize = new SKSizeI((int)(e.Info.Width / density), (int)(e.Info.Height / density));
					var canvas = e.Surface.Canvas;
					canvas.Scale(density);
					canvas.Save();

					e = new Windows.SKPaintGLSurfaceEventArgs(e.Surface, e.BackendRenderTarget, e.Origin, e.Info.WithSize(userVisibleSize), e.Info);
				}

				base.OnPaintSurface(e);
			}
		}
	}
}

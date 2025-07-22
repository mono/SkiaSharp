using System;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.iOS;
using SkiaSharp.Views.Maui.Platform;
using UIKit;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGLViewHandler : ViewHandler<ISKGLView, SKMetalView>
	{
		private PaintSurfaceProxy? paintSurfaceProxy;
		private SKTouchHandlerProxy? touchProxy;

		protected override SKMetalView CreatePlatformView() =>
			new MauiSKMetalView
			{
				BackgroundColor = UIColor.Clear,
				Opaque = false,
			};

		protected override void ConnectHandler(SKMetalView platformView)
		{
			paintSurfaceProxy = new();
			paintSurfaceProxy.Connect(VirtualView, platformView);
			touchProxy = new();
			touchProxy.Connect(VirtualView, platformView);

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(SKMetalView platformView)
		{
			paintSurfaceProxy?.Disconnect(platformView);
			paintSurfaceProxy = null;
			touchProxy?.Disconnect(platformView);
			touchProxy = null;

			base.DisconnectHandler(platformView);
		}

		// Mapper actions / properties

		public static void OnInvalidateSurface(SKGLViewHandler handler, ISKGLView view, object? args)
		{
			if (handler?.PlatformView == null)
				return;

			if (handler.PlatformView.Paused && handler.PlatformView.EnableSetNeedsDisplay)
				handler.PlatformView.SetNeedsDisplay();
		}

		public static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view)
		{
			if (handler?.PlatformView is MauiSKMetalView pv)
			{
				pv.IgnorePixelScaling = view.IgnorePixelScaling;
				handler.PlatformView.SetNeedsDisplay();
			}
		}

		public static void MapHasRenderLoop(SKGLViewHandler handler, ISKGLView view)
		{
			if (handler?.PlatformView == null)
				return;

			handler.PlatformView.Paused = !view.HasRenderLoop;
			handler.PlatformView.EnableSetNeedsDisplay = !view.HasRenderLoop;
		}

		public static void MapEnableTouchEvents(SKGLViewHandler handler, ISKGLView view)
		{
			if (handler?.PlatformView == null)
				return;

			handler.touchProxy?.UpdateEnableTouchEvents(handler.PlatformView, view.EnableTouchEvents);
		}

		// helper methods

		private class MauiSKMetalView : SKMetalView
		{
			public bool IgnorePixelScaling { get; set; }

			protected override void OnPaintSurface(iOS.SKPaintMetalSurfaceEventArgs e)
			{
				if (IgnorePixelScaling)
				{
					var userVisibleSize = new SKSizeI((int)Bounds.Width, (int)Bounds.Height);
					var canvas = e.Surface.Canvas;
					canvas.Scale((float)ContentScaleFactor);
					canvas.Save();

					e = new iOS.SKPaintMetalSurfaceEventArgs(e.Surface, e.BackendRenderTarget, e.Origin, e.Info.WithSize(userVisibleSize), e.Info);
				}

				base.OnPaintSurface(e);
			}
		}

		private class PaintSurfaceProxy : SKEventProxy<ISKGLView, SKMetalView>
		{
			private SKSizeI lastCanvasSize;
			private GRContext? lastGRContext;

			protected override void OnConnect(ISKGLView virtualView, SKMetalView platformView) =>
				platformView.PaintSurface += OnPaintSurface;

			protected override void OnDisconnect(SKMetalView platformView) =>
				platformView.PaintSurface -= OnPaintSurface;

			private void OnPaintSurface(object? sender, iOS.SKPaintMetalSurfaceEventArgs e)
			{
				if (VirtualView is not {} view)
					return;

				var newCanvasSize = e.Info.Size;
				if (lastCanvasSize != newCanvasSize)
				{
					lastCanvasSize = newCanvasSize;
					view.OnCanvasSizeChanged(newCanvasSize);
				}
				if (sender is SKMetalView platformView)
				{
					var newGRContext = platformView.GRContext;
					if (lastGRContext != newGRContext)
					{
						lastGRContext = newGRContext;
						view.OnGRContextChanged(newGRContext);
					}
				}

				view.OnPaintSurface(new SKPaintGLSurfaceEventArgs(e.Surface, e.BackendRenderTarget, e.Origin, e.Info, e.RawInfo));
			}
		}
	}
}

using System;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.iOS;
using SkiaSharp.Views.Maui.Platform;
using UIKit;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGraphiteViewHandler :
		ViewHandler<ISKGraphiteView, SKGraphiteMetalView>
	{
		private PaintSurfaceProxy? paintSurfaceProxy;
		private SKTouchHandlerProxy? touchProxy;

		protected override SKGraphiteMetalView CreatePlatformView () =>
			new MauiSKGraphiteMetalView {
				BackgroundColor = UIColor.Clear,
				Opaque = false,
			};

		protected override void ConnectHandler (SKGraphiteMetalView platformView)
		{
			paintSurfaceProxy = new PaintSurfaceProxy ();
			paintSurfaceProxy.Connect (VirtualView, platformView);
			platformView.RenderFailed += OnRenderFailed;
			touchProxy = new SKTouchHandlerProxy ();
			touchProxy.Connect (VirtualView, platformView);
			base.ConnectHandler (platformView);
		}

		protected override void DisconnectHandler (SKGraphiteMetalView platformView)
		{
			platformView.Paused = true;
			paintSurfaceProxy?.Disconnect (platformView);
			paintSurfaceProxy = null;
			platformView.RenderFailed -= OnRenderFailed;
			touchProxy?.Disconnect (platformView);
			touchProxy = null;
			VirtualView?.OnGraphiteContextChanged (null);
			base.DisconnectHandler (platformView);
		}

		public static void OnInvalidateSurface (
			SKGraphiteViewHandler handler,
			ISKGraphiteView view,
			object? args)
		{
			if (handler.PlatformView is { Paused: true, EnableSetNeedsDisplay: true } platformView)
				platformView.SetNeedsDisplay ();
		}

		public static void MapIgnorePixelScaling (
			SKGraphiteViewHandler handler,
			ISKGraphiteView view)
		{
			if (handler.PlatformView is MauiSKGraphiteMetalView platformView) {
				platformView.IgnorePixelScaling = view.IgnorePixelScaling;
				platformView.SetNeedsDisplay ();
			}
		}

		public static void MapHasRenderLoop (
			SKGraphiteViewHandler handler,
			ISKGraphiteView view)
		{
			if (handler.PlatformView is not { } platformView)
				return;

			platformView.Paused = !view.HasRenderLoop;
			platformView.EnableSetNeedsDisplay = !view.HasRenderLoop;
		}

		public static void MapEnableTouchEvents (
			SKGraphiteViewHandler handler,
			ISKGraphiteView view)
		{
			if (handler.PlatformView is { } platformView)
				handler.touchProxy?.UpdateEnableTouchEvents (
					platformView, view.EnableTouchEvents);
		}

		private void OnRenderFailed (
			object? sender,
			iOS.SKGraphiteRenderFailedEventArgs e)
		{
			VirtualView?.OnRenderFailed (e.Exception);
		}

		private sealed class MauiSKGraphiteMetalView : SKGraphiteMetalView
		{
			public bool IgnorePixelScaling { get; set; }

			protected override void OnPaintSurface (
				iOS.SKPaintGraphiteSurfaceEventArgs e)
			{
				if (IgnorePixelScaling) {
					var userVisibleSize = new SKSizeI (
						(int)Bounds.Width,
						(int)Bounds.Height);
					var canvas = e.Surface.Canvas;
					canvas.Scale ((float)ContentScaleFactor);
					canvas.Save ();

					e = new iOS.SKPaintGraphiteSurfaceEventArgs (
						e.Surface,
						e.BackendTexture,
						e.Context,
						e.Info.WithSize (userVisibleSize),
						e.Info);
				}

				base.OnPaintSurface (e);
			}
		}

		private sealed class PaintSurfaceProxy :
			SKEventProxy<ISKGraphiteView, SKGraphiteMetalView>
		{
			private SKSizeI lastCanvasSize;
			private SKGraphiteContext? lastContext;

			protected override void OnConnect (
				ISKGraphiteView virtualView,
				SKGraphiteMetalView platformView) =>
				platformView.PaintSurface += OnPaintSurface;

			protected override void OnDisconnect (SKGraphiteMetalView platformView) =>
				platformView.PaintSurface -= OnPaintSurface;

			private void OnPaintSurface (
				object? sender,
				iOS.SKPaintGraphiteSurfaceEventArgs e)
			{
				if (VirtualView is not { } view)
					return;

				var newCanvasSize = e.Info.Size;
				if (lastCanvasSize != newCanvasSize) {
					lastCanvasSize = newCanvasSize;
					view.OnCanvasSizeChanged (newCanvasSize);
				}

				if (lastContext != e.Context) {
					lastContext = e.Context;
					view.OnGraphiteContextChanged (e.Context);
				}

				view.OnPaintSurface (new SKPaintGraphiteSurfaceEventArgs (
					e.Surface,
					e.BackendTexture,
					e.Context,
					e.Info,
					e.RawInfo));
			}
		}
	}
}

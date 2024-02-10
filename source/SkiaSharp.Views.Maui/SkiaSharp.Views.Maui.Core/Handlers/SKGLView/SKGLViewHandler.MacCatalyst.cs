using System;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.iOS;
using SkiaSharp.Views.Maui.Platform;
using UIKit;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGLViewHandler : ViewHandler<ISKGLView, SKMetalView>
	{
		private SKSizeI lastCanvasSize;
		private GRContext? lastGRContext;
		private SKTouchHandler? touchHandler;

		protected override SKMetalView CreatePlatformView() =>
			new MauiSKMetalView
			{
				BackgroundColor = UIColor.Clear,
				Opaque = false,
			};

		protected override void ConnectHandler(SKMetalView platformView)
		{
			platformView.PaintSurface += OnPaintSurface;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(SKMetalView platformView)
		{
			touchHandler?.Detach(platformView);
			touchHandler = null;

			platformView.PaintSurface -= OnPaintSurface;

			base.DisconnectHandler(platformView);
		}

		// Mapper actions / properties

		public static void OnInvalidateSurface(SKGLViewHandler handler, ISKGLView view, object? args)
		{
			if (handler.PlatformView.Paused && handler.PlatformView.EnableSetNeedsDisplay)
				handler.PlatformView.SetNeedsDisplay();
		}

		public static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view)
		{
			if (handler.PlatformView is MauiSKMetalView pv)
			{
				pv.IgnorePixelScaling = view.IgnorePixelScaling;
				handler.PlatformView.SetNeedsDisplay();
			}
		}

		public static void MapHasRenderLoop(SKGLViewHandler handler, ISKGLView view)
		{
			handler.PlatformView.Paused = !view.HasRenderLoop;
			handler.PlatformView.EnableSetNeedsDisplay = !view.HasRenderLoop;
		}

		public static void MapEnableTouchEvents(SKGLViewHandler handler, ISKGLView view)
		{
			handler.touchHandler ??= new SKTouchHandler(
				args => view.OnTouch(args),
				(x, y) => handler.OnGetScaledCoord(x, y));

			handler.touchHandler?.SetEnabled(handler.PlatformView, view.EnableTouchEvents);
		}

		// helper methods

		private void OnPaintSurface(object? sender, iOS.SKPaintMetalSurfaceEventArgs e)
		{
			var newCanvasSize = e.Info.Size;
			if (lastCanvasSize != newCanvasSize)
			{
				lastCanvasSize = newCanvasSize;
				VirtualView?.OnCanvasSizeChanged(newCanvasSize);
			}
			if (sender is SKMetalView platformView)
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
				var scale = PlatformView.ContentScaleFactor;

				x *= scale;
				y *= scale;
			}

			return new SKPoint((float)x, (float)y);
		}
	
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
	}
}

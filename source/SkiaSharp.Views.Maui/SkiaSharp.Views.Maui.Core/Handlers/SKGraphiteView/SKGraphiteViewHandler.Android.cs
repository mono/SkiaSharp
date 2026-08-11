using System;
using Android.Content;
using Android.Opengl;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using SkiaSharp.Views.Android;
using SkiaSharp.Views.Maui.Platform;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGraphiteViewHandler :
		ViewHandler<ISKGraphiteView, SKGraphiteVulkanView>
	{
		private SKSizeI lastCanvasSize;
		private SKGraphiteContext? lastContext;
		private SKTouchHandler? touchHandler;

		protected override SKGraphiteVulkanView CreatePlatformView ()
		{
			var view = new MauiSKGraphiteVulkanView (Context);
			view.SetOpaque (false);
			return view;
		}

		protected override void ConnectHandler (SKGraphiteVulkanView platformView)
		{
			platformView.PaintSurface += OnPaintSurface;
			platformView.RenderFailed += OnRenderFailed;
			platformView.OnResume ();
			base.ConnectHandler (platformView);
		}

		protected override void DisconnectHandler (SKGraphiteVulkanView platformView)
		{
			platformView.OnPause ();
			touchHandler?.Detach (platformView);
			touchHandler = null;
			platformView.PaintSurface -= OnPaintSurface;
			platformView.RenderFailed -= OnRenderFailed;
			lastCanvasSize = default;
			lastContext = null;
			VirtualView?.OnGraphiteContextChanged (null);
			base.DisconnectHandler (platformView);
		}

		public static void OnInvalidateSurface (
			SKGraphiteViewHandler handler,
			ISKGraphiteView view,
			object? args)
		{
			if (handler.PlatformView?.RenderMode == Rendermode.WhenDirty)
				handler.PlatformView.RequestRender ();
		}

		public static void MapIgnorePixelScaling (
			SKGraphiteViewHandler handler,
			ISKGraphiteView view)
		{
			if (handler.PlatformView is MauiSKGraphiteVulkanView platformView) {
				platformView.IgnorePixelScaling = view.IgnorePixelScaling;
				platformView.RequestRender ();
			}
		}

		public static void MapHasRenderLoop (
			SKGraphiteViewHandler handler,
			ISKGraphiteView view)
		{
			if (handler.PlatformView is { } platformView) {
				platformView.RenderMode = view.HasRenderLoop
					? Rendermode.Continuously
					: Rendermode.WhenDirty;
			}
		}

		public static void MapEnableTouchEvents (
			SKGraphiteViewHandler handler,
			ISKGraphiteView view)
		{
			if (handler.PlatformView is not { } platformView)
				return;

			handler.touchHandler ??= new SKTouchHandler (
				args => view.OnTouch (args),
				(x, y) => handler.OnGetScaledCoord (x, y));
			handler.touchHandler.SetEnabled (
				platformView, view.EnableTouchEvents);
		}

		private void OnPaintSurface (
			object? sender,
			Android.SKPaintGraphiteSurfaceEventArgs e)
		{
			var newCanvasSize = e.Info.Size;
			if (lastCanvasSize != newCanvasSize) {
				lastCanvasSize = newCanvasSize;
				VirtualView?.OnCanvasSizeChanged (newCanvasSize);
			}

			if (lastContext != e.Context) {
				lastContext = e.Context;
				VirtualView?.OnGraphiteContextChanged (e.Context);
			}

			VirtualView?.OnPaintSurface (new SKPaintGraphiteSurfaceEventArgs (
				e.Surface,
				e.BackendTexture,
				e.Context,
				e.Info,
				e.RawInfo));
		}

		private void OnRenderFailed (
			object? sender,
			Android.SKGraphiteRenderFailedEventArgs e)
		{
			lastCanvasSize = default;
			lastContext = null;
			VirtualView?.OnCanvasSizeChanged (default);
			VirtualView?.OnGraphiteContextChanged (null);
			VirtualView?.OnRenderFailed (e.Exception);
		}

		private SKPoint OnGetScaledCoord (double x, double y)
		{
			if (VirtualView?.IgnorePixelScaling == true && Context is not null) {
				x = Context.FromPixels (x);
				y = Context.FromPixels (y);
			}

			return new SKPoint ((float)x, (float)y);
		}

		private sealed class MauiSKGraphiteVulkanView : SKGraphiteVulkanView
		{
			private readonly float density;

			public MauiSKGraphiteVulkanView (Context context)
				: base (context)
			{
				density = Resources?.DisplayMetrics?.Density ?? 1;
			}

			public bool IgnorePixelScaling { get; set; }

			protected override void OnPaintSurface (
				Android.SKPaintGraphiteSurfaceEventArgs e)
			{
				if (IgnorePixelScaling) {
					var userVisibleSize = new SKSizeI (
						(int)(e.Info.Width / density),
						(int)(e.Info.Height / density));
					var canvas = e.Surface.Canvas;
					canvas.Scale (density);
					canvas.Save ();

					e = new Android.SKPaintGraphiteSurfaceEventArgs (
						e.Surface,
						e.BackendTexture,
						e.Context,
						e.Info.WithSize (userVisibleSize),
						e.Info);
				}

				base.OnPaintSurface (e);
			}
		}
	}
}

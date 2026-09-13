using System;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.iOS;
using SkiaSharp.Views.Maui.Platform;
using UIKit;

namespace SkiaSharp.Views.Maui.Handlers
{
	/// <summary>Handles <see cref="T:SkiaSharp.Views.Maui.ISKGLView" /> instances on Mac Catalyst.</summary>
	public partial class SKGLViewHandler : ViewHandler<ISKGLView, SKMetalView>
	{
		private PaintSurfaceProxy? paintSurfaceProxy;
		private SKTouchHandlerProxy? touchProxy;

		/// <summary>Creates the platform-specific GPU view for the current platform.</summary>
		/// <returns>The platform-specific GPU-backed view instance.</returns>
		/// <remarks>Returns an Android SKGLTextureView (OpenGL ES), Mac Catalyst SKMetalView (Metal), or Windows SKSwapChainPanel (DirectX via ANGLE) depending on the platform.</remarks>
		protected override SKMetalView CreatePlatformView() =>
			new MauiSKMetalView
			{
				BackgroundColor = UIColor.Clear,
				Opaque = false,
			};

		/// <summary>Connects the handler to the specified platform view.</summary>
		/// <param name="platformView">The platform view to connect.</param>
		protected override void ConnectHandler(SKMetalView platformView)
		{
			paintSurfaceProxy = new();
			paintSurfaceProxy.Connect(VirtualView, platformView);
			touchProxy = new();
			touchProxy.Connect(VirtualView, platformView);

			base.ConnectHandler(platformView);
		}

		/// <summary>Disconnects the handler from the specified platform view.</summary>
		/// <param name="platformView">The platform view to disconnect.</param>
		protected override void DisconnectHandler(SKMetalView platformView)
		{
			paintSurfaceProxy?.Disconnect(platformView);
			paintSurfaceProxy = null;
			touchProxy?.Disconnect(platformView);
			touchProxy = null;

			base.DisconnectHandler(platformView);
		}

		// Mapper actions / properties

		/// <summary>Handles the <see cref="M:SkiaSharp.Views.Maui.ISKGLView.InvalidateSurface" /> command to trigger a redraw.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view requesting invalidation.</param>
		/// <param name="args">Optional arguments (not used).</param>
		/// <remarks>This method is called when the cross-platform control requests a surface invalidation, causing the native GPU view to repaint.</remarks>
		public static void OnInvalidateSurface(SKGLViewHandler handler, ISKGLView view, object? args)
		{
			if (handler?.PlatformView == null)
				return;

			if (handler.PlatformView.Paused && handler.PlatformView.EnableSetNeedsDisplay)
				handler.PlatformView.SetNeedsDisplay();
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGLView.IgnorePixelScaling" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the IgnorePixelScaling property changes to update the native view's pixel scaling behavior.</remarks>
		public static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view)
		{
			if (handler?.PlatformView is MauiSKMetalView pv)
			{
				pv.IgnorePixelScaling = view.IgnorePixelScaling;
				handler.PlatformView.SetNeedsDisplay();
			}
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGLView.HasRenderLoop" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the HasRenderLoop property changes to enable or disable continuous rendering on the native view.</remarks>
		public static void MapHasRenderLoop(SKGLViewHandler handler, ISKGLView view)
		{
			if (handler?.PlatformView == null)
				return;

			handler.PlatformView.Paused = !view.HasRenderLoop;
			handler.PlatformView.EnableSetNeedsDisplay = !view.HasRenderLoop;
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGLView.EnableTouchEvents" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the EnableTouchEvents property changes to update the native view's touch handling.</remarks>
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

#nullable enable

using Microsoft.Maui.Handlers;
using SkiaSharp.Views.iOS;
using SkiaSharp.Views.Maui.Platform;
using UIKit;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGaneshViewHandler : ViewHandler<ISKGaneshView, SKGaneshView>
	{
		private PaintSurfaceProxy? paintSurfaceProxy;
		private SKTouchHandlerProxy? touchProxy;

		/// <summary>Creates the platform-specific Ganesh GPU view for the current platform.</summary>
		/// <returns>The platform-specific Ganesh GPU-backed view instance.</returns>
		/// <remarks />
		protected override SKGaneshView CreatePlatformView() =>
			new SKGaneshView
			{
				BackgroundColor = UIColor.Clear,
				Opaque = false,
			};

		/// <summary>Connects the handler to the specified platform view.</summary>
		/// <param name="platformView">The platform view to connect.</param>
		/// <remarks />
		protected override void ConnectHandler(SKGaneshView platformView)
		{
			paintSurfaceProxy = new PaintSurfaceProxy();
			paintSurfaceProxy.Connect(VirtualView, platformView);

			touchProxy = new SKTouchHandlerProxy();
			touchProxy.Connect(VirtualView, platformView);

			base.ConnectHandler(platformView);
		}

		/// <summary>Disconnects the handler from the specified platform view.</summary>
		/// <param name="platformView">The platform view to disconnect.</param>
		/// <remarks />
		protected override void DisconnectHandler(SKGaneshView platformView)
		{
			platformView.HasRenderLoop = false;

			paintSurfaceProxy?.Disconnect(platformView);
			paintSurfaceProxy = null;

			touchProxy?.Disconnect(platformView);
			touchProxy = null;

			VirtualView?.OnGRContextChanged(null);
			VirtualView?.OnCanvasSizeChanged(SKSizeI.Empty);

			base.DisconnectHandler(platformView);
		}

		/// <summary>Handles the <see cref="M:SkiaSharp.Views.Maui.ISKGaneshView.InvalidateSurface" /> command to trigger a redraw.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view requesting invalidation.</param>
		/// <param name="args">Optional arguments (not used).</param>
		/// <remarks>This method is called when the cross-platform control requests a surface invalidation, causing the native GPU view to repaint.</remarks>
		public static void OnInvalidateSurface(SKGaneshViewHandler handler, ISKGaneshView view, object? args)
		{
			if (handler?.PlatformView == null)
				return;

			handler.PlatformView.InvalidateSurface();
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGaneshView.IgnorePixelScaling" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the IgnorePixelScaling property changes to update the native view's pixel scaling behavior.</remarks>
		public static void MapIgnorePixelScaling(SKGaneshViewHandler handler, ISKGaneshView view)
		{
			if (handler?.PlatformView == null)
				return;

			handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGaneshView.HasRenderLoop" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the HasRenderLoop property changes to enable or disable continuous rendering on the native view.</remarks>
		public static void MapHasRenderLoop(SKGaneshViewHandler handler, ISKGaneshView view)
		{
			if (handler?.PlatformView == null)
				return;

			handler.PlatformView.HasRenderLoop = view.HasRenderLoop;
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGaneshView.EnableTouchEvents" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the EnableTouchEvents property changes to update the native view's touch handling.</remarks>
		public static void MapEnableTouchEvents(SKGaneshViewHandler handler, ISKGaneshView view)
		{
			if (handler?.PlatformView == null)
				return;

			handler.touchProxy?.UpdateEnableTouchEvents(handler.PlatformView, view.EnableTouchEvents);
		}

		private class PaintSurfaceProxy : SKEventProxy<ISKGaneshView, SKGaneshView>
		{
			private SKSizeI lastCanvasSize;
			private GRContext? lastGRContext;

			protected override void OnConnect(ISKGaneshView virtualView, SKGaneshView platformView)
			{
				platformView.PaintSurface += OnPaintSurface;
			}

			protected override void OnDisconnect(SKGaneshView platformView)
			{
				platformView.PaintSurface -= OnPaintSurface;
				lastCanvasSize = SKSizeI.Empty;
				lastGRContext = null;
			}

			private void OnPaintSurface(object? sender, iOS.SKPaintGaneshSurfaceEventArgs e)
			{
				if (VirtualView is not { } view)
					return;

				var newCanvasSize = e.Info.Size;
				if (lastCanvasSize != newCanvasSize)
				{
					lastCanvasSize = newCanvasSize;
					view.OnCanvasSizeChanged(newCanvasSize);
				}

				var newGRContext = e.Context;
				if (lastGRContext != newGRContext)
				{
					lastGRContext = newGRContext;
					view.OnGRContextChanged(newGRContext);
				}

				view.OnPaintSurface(new SKPaintGaneshSurfaceEventArgs(e.Surface, e.Context, e.BackendRenderTarget, e.Origin, e.Info, e.RawInfo));
			}
		}
	}
}

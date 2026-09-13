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

		/// <summary>Creates the platform-specific view for the current platform.</summary>
		/// <returns>The platform-specific canvas view instance.</returns>
		/// <remarks>Returns an Android SKCanvasView, iOS SKCanvasView, or Windows SKXamlCanvas depending on the platform.</remarks>
		protected override SKCanvasView CreatePlatformView() => new SKCanvasView { BackgroundColor = UIColor.Clear };

		/// <summary>Connects the handler to the specified platform view.</summary>
		/// <param name="platformView">The platform view to connect.</param>
		protected override void ConnectHandler(SKCanvasView platformView)
		{
			paintSurfaceProxy = new();
			paintSurfaceProxy.Connect(VirtualView, platformView);
			touchProxy = new();
			touchProxy.Connect(VirtualView, platformView);

			base.ConnectHandler(platformView);
		}

		/// <summary>Disconnects the handler from the specified platform view.</summary>
		/// <param name="platformView">The platform view to disconnect.</param>
		protected override void DisconnectHandler(SKCanvasView platformView)
		{
			paintSurfaceProxy?.Disconnect(platformView);
			paintSurfaceProxy = null;
			touchProxy?.Disconnect(platformView);
			touchProxy = null;

			base.DisconnectHandler(platformView);
		}

		// Mapper actions / properties

		/// <summary>Handles the <see cref="M:SkiaSharp.Views.Maui.ISKCanvasView.InvalidateSurface" /> command to trigger a redraw.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="canvasView">The canvas view requesting invalidation.</param>
		/// <param name="args">Optional arguments (not used).</param>
		/// <remarks>This method is called when the cross-platform control requests a surface invalidation, causing the native view to repaint.</remarks>
		public static void OnInvalidateSurface(SKCanvasViewHandler handler, ISKCanvasView canvasView, object? args)
		{
			if (handler?.PlatformView == null)
				return;

			handler.PlatformView.SetNeedsDisplay();
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKCanvasView.IgnorePixelScaling" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="canvasView">The canvas view whose property changed.</param>
		/// <remarks>This method is called when the IgnorePixelScaling property changes to update the native view's pixel scaling behavior.</remarks>
		public static void MapIgnorePixelScaling(SKCanvasViewHandler handler, ISKCanvasView canvasView)
		{
			if (handler?.PlatformView == null)
				return;

			handler.PlatformView.IgnorePixelScaling = canvasView.IgnorePixelScaling;
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKCanvasView.EnableTouchEvents" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="canvasView">The canvas view whose property changed.</param>
		/// <remarks>This method is called when the EnableTouchEvents property changes to update the native view's touch handling.</remarks>
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

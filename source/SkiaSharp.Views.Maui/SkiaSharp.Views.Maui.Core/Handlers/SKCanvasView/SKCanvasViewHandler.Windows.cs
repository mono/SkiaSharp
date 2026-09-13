using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using SkiaSharp.Views.Maui.Platform;
using SkiaSharp.Views.Windows;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKCanvasViewHandler : ViewHandler<ISKCanvasView, SKXamlCanvas>
	{
		private SKSizeI lastCanvasSize;
		private SKTouchHandler? touchHandler;

		/// <summary>Creates the platform-specific view for the current platform.</summary>
		/// <returns>The platform-specific canvas view instance.</returns>
		/// <remarks>Returns an Android SKCanvasView, iOS SKCanvasView, or Windows SKXamlCanvas depending on the platform.</remarks>
		protected override SKXamlCanvas CreatePlatformView() => new SKXamlCanvas();

		/// <summary>Connects the handler to the specified Windows canvas view.</summary>
		/// <param name="platformView">The Windows canvas view to connect.</param>
		/// <remarks />
		protected override void ConnectHandler(SKXamlCanvas platformView)
		{
			platformView.PaintSurface += OnPaintSurface;

			base.ConnectHandler(platformView);
		}

		/// <summary>Disconnects the handler from the specified Windows canvas view.</summary>
		/// <param name="platformView">The Windows canvas view to disconnect.</param>
		/// <remarks />
		protected override void DisconnectHandler(SKXamlCanvas platformView)
		{
			touchHandler?.Detach(platformView);
			touchHandler = null;

			platformView.PaintSurface -= OnPaintSurface;

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

			handler.PlatformView.Invalidate();
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

			handler.touchHandler ??= new SKTouchHandler(
				args => canvasView.OnTouch(args),
				(x, y) => handler.OnGetScaledCoord(x, y));

			handler.touchHandler?.SetEnabled(handler.PlatformView, canvasView.EnableTouchEvents);
		}

		// helper methods

		private void OnPaintSurface(object? sender, Windows.SKPaintSurfaceEventArgs e)
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
			if (VirtualView?.IgnorePixelScaling == false && PlatformView != null)
			{
				var scale = PlatformView.Dpi;

				x *= scale;
				y *= scale;
			}

			return new SKPoint((float)x, (float)y);
		}
	}
}

#nullable enable

using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using SkiaSharp.Views.Android;
using SkiaSharp.Views.Maui.Platform;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGraphiteViewHandler : ViewHandler<ISKGraphiteView, SKGraphiteView>
	{
		private SKSizeI lastCanvasSize;
		private SKGraphiteContext? lastGraphiteContext;
		private SKTouchHandler? touchHandler;

		/// <summary>Creates the platform-specific Graphite GPU view for the current platform.</summary>
		/// <returns>The platform-specific Graphite GPU-backed view instance.</returns>
		/// <remarks />
		protected override SKGraphiteView CreatePlatformView() =>
			new SKGraphiteView(Context);

		/// <summary>Connects the handler to the specified platform view.</summary>
		/// <param name="platformView">The platform view to connect.</param>
		/// <remarks />
		protected override void ConnectHandler(SKGraphiteView platformView)
		{
			platformView.PaintSurface += OnPaintSurface;

			base.ConnectHandler(platformView);
		}

		/// <summary>Disconnects the handler from the specified platform view.</summary>
		/// <param name="platformView">The platform view to disconnect.</param>
		/// <remarks />
		protected override void DisconnectHandler(SKGraphiteView platformView)
		{
			platformView.HasRenderLoop = false;

			touchHandler?.Detach(platformView);
			touchHandler = null;

			platformView.PaintSurface -= OnPaintSurface;

			lastCanvasSize = SKSizeI.Empty;
			lastGraphiteContext = null;

			VirtualView?.OnGraphiteContextChanged(null);
			VirtualView?.OnCanvasSizeChanged(SKSizeI.Empty);

			base.DisconnectHandler(platformView);
		}

		/// <summary>Handles the <see cref="M:SkiaSharp.Views.Maui.ISKGraphiteView.InvalidateSurface" /> command to trigger a redraw.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view requesting invalidation.</param>
		/// <param name="args">Optional arguments (not used).</param>
		/// <remarks>This method is called when the cross-platform control requests a surface invalidation, causing the native GPU view to repaint.</remarks>
		public static void OnInvalidateSurface(SKGraphiteViewHandler handler, ISKGraphiteView view, object? args)
		{
			if (handler?.PlatformView == null)
				return;

			handler.PlatformView.InvalidateSurface();
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGraphiteView.IgnorePixelScaling" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the IgnorePixelScaling property changes to update the native view's pixel scaling behavior.</remarks>
		public static void MapIgnorePixelScaling(SKGraphiteViewHandler handler, ISKGraphiteView view)
		{
			if (handler?.PlatformView == null)
				return;

			handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGraphiteView.HasRenderLoop" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the HasRenderLoop property changes to enable or disable continuous rendering on the native view.</remarks>
		public static void MapHasRenderLoop(SKGraphiteViewHandler handler, ISKGraphiteView view)
		{
			if (handler?.PlatformView == null)
				return;

			handler.PlatformView.HasRenderLoop = view.HasRenderLoop;
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGraphiteView.EnableTouchEvents" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the EnableTouchEvents property changes to update the native view's touch handling.</remarks>
		public static void MapEnableTouchEvents(SKGraphiteViewHandler handler, ISKGraphiteView view)
		{
			if (handler?.PlatformView == null)
				return;

			handler.touchHandler ??= new SKTouchHandler(
				args => view.OnTouch(args),
				(x, y) => handler.OnGetScaledCoord(x, y));

			handler.touchHandler?.SetEnabled(handler.PlatformView, view.EnableTouchEvents);
		}

		private void OnPaintSurface(object? sender, Android.SKPaintGraphiteSurfaceEventArgs e)
		{
			var newCanvasSize = e.Info.Size;
			if (lastCanvasSize != newCanvasSize)
			{
				lastCanvasSize = newCanvasSize;
				VirtualView?.OnCanvasSizeChanged(newCanvasSize);
			}

			var newGraphiteContext = e.Context;
			if (lastGraphiteContext != newGraphiteContext)
			{
				lastGraphiteContext = newGraphiteContext;
				VirtualView?.OnGraphiteContextChanged(newGraphiteContext);
			}

			VirtualView?.OnPaintSurface(new SKPaintGraphiteSurfaceEventArgs(e.Surface, e.Context, e.Recorder, e.BackendTexture, e.Info, e.RawInfo));
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

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

		/// <summary>Creates the platform-specific GPU view for the current platform.</summary>
		/// <returns>The platform-specific GPU-backed view instance.</returns>
		/// <remarks>Returns an Android SKGLTextureView (OpenGL ES), Mac Catalyst SKMetalView (Metal), or Windows SKSwapChainPanel (DirectX via ANGLE) depending on the platform.</remarks>
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

		/// <summary>Handles the <see cref="M:SkiaSharp.Views.Maui.ISKGLView.InvalidateSurface" /> command to trigger a redraw.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view requesting invalidation.</param>
		/// <param name="args">Optional arguments (not used).</param>
		/// <remarks>This method is called when the cross-platform control requests a surface invalidation, causing the native GPU view to repaint.</remarks>
		public static void OnInvalidateSurface(SKGLViewHandler handler, ISKGLView view, object? args)
		{
			if (handler?.PlatformView == null)
				return;

			if (!handler.PlatformView.EnableRenderLoop)
				handler.PlatformView.Invalidate();
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGLView.IgnorePixelScaling" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the IgnorePixelScaling property changes to update the native view's pixel scaling behavior.</remarks>
		public static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view)
		{
			if (handler?.PlatformView is not MauiSKSwapChainPanel pv)
				return;

			pv.IgnorePixelScaling = view.IgnorePixelScaling;
			pv.Invalidate();
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGLView.HasRenderLoop" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the HasRenderLoop property changes to enable or disable continuous rendering on the native view.</remarks>
		public static void MapHasRenderLoop(SKGLViewHandler handler, ISKGLView view)
		{
			if (handler?.PlatformView == null)
				return;

			handler.PlatformView.EnableRenderLoop = view.HasRenderLoop;
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGLView.EnableTouchEvents" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the EnableTouchEvents property changes to update the native view's touch handling.</remarks>
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

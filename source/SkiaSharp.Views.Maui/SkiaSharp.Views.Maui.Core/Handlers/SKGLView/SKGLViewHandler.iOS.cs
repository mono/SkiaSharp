using System;
using System.Runtime.Versioning;
using CoreAnimation;
using Foundation;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.iOS;
using SkiaSharp.Views.Maui.Platform;
using UIKit;

namespace SkiaSharp.Views.Maui.Handlers
{
	[ObsoletedOSPlatform("ios12.0", "Use 'Metal' instead.")]
	[ObsoletedOSPlatform("tvos12.0", "Use 'Metal' instead.")]
	[SupportedOSPlatform("ios")]
	[SupportedOSPlatform("tvos")]
	[UnsupportedOSPlatform("macos")]
	public partial class SKGLViewHandler : ViewHandler<ISKGLView, SKGLView>
	{
		private PaintSurfaceProxy? paintSurfaceProxy;
		private SKTouchHandlerProxy? touchProxy;
		private RenderLoopManager? renderLoopManager;

		/// <summary>Creates the platform-specific GPU view for the current platform.</summary>
		/// <returns>The platform-specific GPU-backed view instance.</returns>
		/// <remarks>Returns an Android SKGLTextureView (OpenGL ES), Mac Catalyst SKMetalView (Metal), or Windows SKSwapChainPanel (DirectX via ANGLE) depending on the platform.</remarks>
		protected override SKGLView CreatePlatformView() =>
			new MauiSKGLView
			{
				BackgroundColor = UIColor.Clear,
				Opaque = false,
			};

		/// <summary>Connects the handler to the specified platform view.</summary>
		/// <param name="platformView">The platform view to connect.</param>
		protected override void ConnectHandler(SKGLView platformView)
		{
			paintSurfaceProxy = new();
			paintSurfaceProxy.Connect(VirtualView, platformView);
			touchProxy = new();
			touchProxy.Connect(VirtualView, platformView);
			renderLoopManager = new RenderLoopManager(this);

			base.ConnectHandler(platformView);
		}

		/// <summary>Disconnects the handler from the specified platform view.</summary>
		/// <param name="platformView">The platform view to disconnect.</param>
		protected override void DisconnectHandler(SKGLView platformView)
		{
			paintSurfaceProxy?.Disconnect(platformView);
			paintSurfaceProxy = null;
			touchProxy?.Disconnect(platformView);
			touchProxy = null;
			renderLoopManager?.StopRenderLoop();

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

			handler.renderLoopManager?.RequestDisplay();
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGLView.IgnorePixelScaling" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the IgnorePixelScaling property changes to update the native view's pixel scaling behavior.</remarks>
		public static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view)
		{
			if (handler?.PlatformView is MauiSKGLView pv)
			{
				pv.IgnorePixelScaling = view.IgnorePixelScaling;
				handler.renderLoopManager?.RequestDisplay();
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

			if (view.HasRenderLoop)
				handler.renderLoopManager?.RequestRenderLoop();
			else
				handler.renderLoopManager?.StopRenderLoop();
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

		private class MauiSKGLView : SKGLView
		{
			public bool IgnorePixelScaling { get; set; }

			protected override void OnPaintSurface(iOS.SKPaintGLSurfaceEventArgs e)
			{
				if (IgnorePixelScaling)
				{
					var userVisibleSize = new SKSizeI((int)Bounds.Width, (int)Bounds.Height);
					var canvas = e.Surface.Canvas;
					canvas.Scale((float)ContentScaleFactor);
					canvas.Save();

					e = new iOS.SKPaintGLSurfaceEventArgs(e.Surface, e.BackendRenderTarget, e.Origin, e.Info.WithSize(userVisibleSize), e.Info);
				}

				base.OnPaintSurface(e);
			}
		}

		private class RenderLoopManager
		{
			private CADisplayLink? displayLink;
			private WeakReference<SKGLViewHandler> weakHandler;

			public RenderLoopManager(SKGLViewHandler handler)
			{
				weakHandler = new WeakReference<SKGLViewHandler>(handler);
			}

			public SKGLViewHandler? Handler
			{
				get
				{
					if (weakHandler.TryGetTarget(out var handler))
						return handler;
					return null;
				}
			}

			public SKGLView? PlatformView => Handler?.PlatformView;

			public ISKGLView? VirtualView => Handler?.VirtualView;

			public void RequestDisplay()
			{
				// skip if there is a render loop
				if (displayLink is not null)
					return;

				var nativeView = PlatformView;
				nativeView?.BeginInvokeOnMainThread(() =>
				{
					if (nativeView is not null && nativeView.Handle != IntPtr.Zero)
						nativeView.Display();
				});
			}

			public void RequestRenderLoop()
			{
				// skip if there is already a render loop
				if (displayLink is not null)
					return;

				// bail out if we are requesting something that the view doesn't want to
				if (VirtualView?.HasRenderLoop != true)
					return;

				displayLink = CADisplayLink.Create(() =>
				{
					var nativeView = PlatformView;
					var virtualView = VirtualView;

					// stop the render loop if the loop was disabled, or the views are disposed
					if (nativeView is null || virtualView is null || nativeView.Handle == IntPtr.Zero || !virtualView.HasRenderLoop)
					{
						StopRenderLoop();
						return;
					}

					// redraw the view
					nativeView.Display();
				});
				displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoopMode.Default);
			}

			public void StopRenderLoop()
			{
				// skip if there is no render loop
				if (displayLink is null)
					return;

				displayLink.Invalidate();
				displayLink.Dispose();
				displayLink = null;
			}
		}

		private class PaintSurfaceProxy : SKEventProxy<ISKGLView, SKGLView>
		{
			private SKSizeI lastCanvasSize;
			private GRContext? lastGRContext;

			protected override void OnConnect(ISKGLView virtualView, SKGLView platformView) =>
				platformView.PaintSurface += OnPaintSurface;

			protected override void OnDisconnect(SKGLView platformView) =>
				platformView.PaintSurface -= OnPaintSurface;

			private void OnPaintSurface(object? sender, iOS.SKPaintGLSurfaceEventArgs e)
			{
				if (VirtualView is not {} view)
					return;

				var newCanvasSize = e.Info.Size;
				if (lastCanvasSize != newCanvasSize)
				{
					lastCanvasSize = newCanvasSize;
					view.OnCanvasSizeChanged(newCanvasSize);
				}
				if (sender is SKGLView platformView)
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

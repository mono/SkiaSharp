using System;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Mac;
using Microsoft.Maui.Platforms.MacOS.Handlers;
using NativeMetalView = SkiaSharp.Views.Mac.SKMetalView;
using NativeMetalEventArgs = SkiaSharp.Views.Mac.SKPaintMetalSurfaceEventArgs;

namespace SkiaSharp.Views.Maui.Handlers
{
	/// <summary>Native AppKit Metal handler for the stable MAUI GPU canvas.</summary>
	public class AppKitSKGLViewHandler : MacOSViewHandler<ISKGLView, NativeMetalView>
	{
		/// <summary>Maps GPU canvas properties to the Metal view.</summary>
		public static readonly PropertyMapper<ISKGLView, AppKitSKGLViewHandler> Mapper =
			new(ViewHandler.ViewMapper)
			{
				[nameof(ISKGLView.IgnorePixelScaling)] = MapIgnorePixelScaling,
				[nameof(ISKGLView.EnableTouchEvents)] = MapEnableTouchEvents,
				[nameof(ISKGLView.HasRenderLoop)] = MapHasRenderLoop,
			};

		/// <summary>Maps GPU redraw commands to Metal.</summary>
		public static readonly CommandMapper<ISKGLView, AppKitSKGLViewHandler> CommandMapper =
			new(ViewHandler.ViewCommandMapper)
			{
				[nameof(ISKGLView.InvalidateSurface)] = OnInvalidateSurface,
			};

		private readonly MacTouchHandler touchHandler = new();
		private SKSizeI lastCanvasSize;
		private GRContext? lastContext;

		/// <summary>Creates the handler with the default AppKit mappings.</summary>
		public AppKitSKGLViewHandler() : base(Mapper, CommandMapper) { }

		/// <inheritdoc />
		protected override NativeMetalView CreatePlatformView()
		{
			var view = new MacMetalView(touchHandler);
			if (view.Device is null)
			{
				view.Dispose();
				throw new PlatformNotSupportedException("A Metal device is required for SKGLView on macOS.");
			}
			return view;
		}

		/// <inheritdoc />
		protected override void ConnectHandler(NativeMetalView platformView)
		{
			platformView.PaintSurface += OnPaintSurface;
			touchHandler.Connect(platformView, VirtualView);
			base.ConnectHandler(platformView);
		}

		/// <inheritdoc />
		protected override void DisconnectHandler(NativeMetalView platformView)
		{
			platformView.Paused = true;
			touchHandler.Disconnect();
			platformView.PaintSurface -= OnPaintSurface;
			VirtualView?.OnGRContextChanged(null);
			lastContext = null;
			lastCanvasSize = default;
			base.DisconnectHandler(platformView);
		}

		/// <summary>Enables or disables native mouse input.</summary>
		public static void MapEnableTouchEvents(AppKitSKGLViewHandler handler, ISKGLView view) =>
			handler.touchHandler.SetEnabled(view.EnableTouchEvents);

		/// <summary>Maps logical-pixel rendering to the Metal surface.</summary>
		public static void MapIgnorePixelScaling(AppKitSKGLViewHandler handler, ISKGLView view)
		{
			if (handler.PlatformView is MacMetalView nativeView)
			{
				nativeView.IgnorePixelScaling = view.IgnorePixelScaling;
				nativeView.NeedsDisplay = true;
			}
		}

		/// <summary>Starts or stops MTKView's continuous render loop.</summary>
		public static void MapHasRenderLoop(AppKitSKGLViewHandler handler, ISKGLView view)
		{
			handler.PlatformView.EnableSetNeedsDisplay = !view.HasRenderLoop;
			handler.PlatformView.Paused = !view.HasRenderLoop;
			if (!view.HasRenderLoop)
				handler.PlatformView.NeedsDisplay = true;
		}

		/// <summary>Requests an on-demand Metal frame.</summary>
		public static void OnInvalidateSurface(AppKitSKGLViewHandler handler, ISKGLView view, object? args)
		{
			if (handler.PlatformView.Paused)
				handler.PlatformView.NeedsDisplay = true;
		}

		private void OnPaintSurface(object? sender, NativeMetalEventArgs e)
		{
			var view = VirtualView;
			if (view is null)
				return;

			if (sender is NativeMetalView metalView && lastContext != metalView.GRContext)
			{
				lastContext = metalView.GRContext;
				view.OnGRContextChanged(lastContext);
			}
			if (lastCanvasSize != e.Info.Size)
			{
				lastCanvasSize = e.Info.Size;
				view.OnCanvasSizeChanged(lastCanvasSize);
			}
			view.OnPaintSurface(new SKPaintGLSurfaceEventArgs(e.Surface, e.BackendRenderTarget, e.Origin, e.Info, e.RawInfo));
		}
	}
}

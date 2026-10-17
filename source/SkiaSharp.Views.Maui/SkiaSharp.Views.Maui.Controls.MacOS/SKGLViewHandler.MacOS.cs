using System;
using AppKit;
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
			var view = new MauiSKMetalView(touchHandler);
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
			if (handler.PlatformView is MauiSKMetalView nativeView)
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

		private sealed class MauiSKMetalView : NativeMetalView
		{
			private readonly MacTouchHandler touch;
			private bool disposed;

			public MauiSKMetalView(MacTouchHandler touch) => this.touch = touch;

			public bool IgnorePixelScaling { get; set; }

			protected override void OnPaintSurface(NativeMetalEventArgs e)
			{
				using var restore = new SKAutoCanvasRestore(e.Surface.Canvas, true);
				if (IgnorePixelScaling)
				{
					var logical = new SKSizeI((int)Bounds.Width, (int)Bounds.Height);
					if (logical.Width > 0 && logical.Height > 0)
					{
						e.Surface.Canvas.Scale(e.Info.Width / (float)logical.Width, e.Info.Height / (float)logical.Height);
						e = new NativeMetalEventArgs(e.Surface, e.BackendRenderTarget,
							e.Origin, e.Info.WithSize(logical), e.Info);
					}
				}
				base.OnPaintSurface(e);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && !disposed)
				{
					disposed = true;
					Paused = true;
					GRContext?.Dispose();
				}
				base.Dispose(disposing);
			}

			public override void MouseDown(NSEvent e)
			{
				if (!touch.Handle(e, SKTouchAction.Pressed, SKMouseButton.Left))
					base.MouseDown(e);
			}

			public override void MouseDragged(NSEvent e)
			{
				if (!touch.Handle(e, SKTouchAction.Moved, SKMouseButton.Left))
					base.MouseDragged(e);
			}

			public override void MouseUp(NSEvent e)
			{
				if (!touch.Handle(e, SKTouchAction.Released, SKMouseButton.Left))
					base.MouseUp(e);
			}

			public override void RightMouseDown(NSEvent e)
			{
				if (!touch.Handle(e, SKTouchAction.Pressed, SKMouseButton.Right))
					base.RightMouseDown(e);
			}

			public override void RightMouseDragged(NSEvent e)
			{
				if (!touch.Handle(e, SKTouchAction.Moved, SKMouseButton.Right))
					base.RightMouseDragged(e);
			}

			public override void RightMouseUp(NSEvent e)
			{
				if (!touch.Handle(e, SKTouchAction.Released, SKMouseButton.Right))
					base.RightMouseUp(e);
			}

			public override void OtherMouseDown(NSEvent e)
			{
				if (!touch.Handle(e, SKTouchAction.Pressed, SKMouseButton.Middle))
					base.OtherMouseDown(e);
			}

			public override void OtherMouseDragged(NSEvent e)
			{
				if (!touch.Handle(e, SKTouchAction.Moved, SKMouseButton.Middle))
					base.OtherMouseDragged(e);
			}

			public override void OtherMouseUp(NSEvent e)
			{
				if (!touch.Handle(e, SKTouchAction.Released, SKMouseButton.Middle))
					base.OtherMouseUp(e);
			}

			public override void MouseMoved(NSEvent e)
			{
				if (!touch.Handle(e, SKTouchAction.Moved))
					base.MouseMoved(e);
			}

			public override void MouseEntered(NSEvent e)
			{
				if (!touch.Handle(e, SKTouchAction.Entered))
					base.MouseEntered(e);
			}

			public override void MouseExited(NSEvent e)
			{
				if (!touch.Handle(e, SKTouchAction.Exited))
					base.MouseExited(e);
			}

			public override void ScrollWheel(NSEvent e)
			{
				if (!touch.Handle(e, SKTouchAction.WheelChanged))
					base.ScrollWheel(e);
			}
		}
	}
}

using System;
using AppKit;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platforms.MacOS.Handlers;
using SkiaSharp.Views.Mac;
using NativeCanvasView = SkiaSharp.Views.Mac.SKCanvasView;
using NativePaintEventArgs = SkiaSharp.Views.Mac.SKPaintSurfaceEventArgs;

namespace SkiaSharp.Views.Maui.Handlers
{
	/// <summary>Native AppKit software-rendered handler for the stable MAUI canvas.</summary>
	public class AppKitSKCanvasViewHandler : MacOSViewHandler<ISKCanvasView, NativeCanvasView>
	{
		/// <summary>Maps canvas state to its AppKit view.</summary>
		public static readonly PropertyMapper<ISKCanvasView, AppKitSKCanvasViewHandler> Mapper =
			new(ViewHandler.ViewMapper)
			{
				[nameof(ISKCanvasView.IgnorePixelScaling)] = MapIgnorePixelScaling,
				[nameof(ISKCanvasView.EnableTouchEvents)] = MapEnableTouchEvents,
			};

		/// <summary>Maps canvas redraw commands to AppKit.</summary>
		public static readonly CommandMapper<ISKCanvasView, AppKitSKCanvasViewHandler> CommandMapper =
			new(ViewHandler.ViewCommandMapper)
			{
				[nameof(ISKCanvasView.InvalidateSurface)] = OnInvalidateSurface,
			};

		private SKSizeI lastCanvasSize;
		private readonly MacTouchHandler touchHandler = new();

		/// <summary>Creates the handler with the default AppKit mappings.</summary>
		public AppKitSKCanvasViewHandler() : base(Mapper, CommandMapper) { }

		/// <inheritdoc />
		protected override NativeCanvasView CreatePlatformView() => new MauiSKCanvasView(touchHandler);

		/// <inheritdoc />
		protected override void ConnectHandler(NativeCanvasView platformView)
		{
			platformView.PaintSurface += OnPaintSurface;
			touchHandler.Connect(platformView, VirtualView);
			base.ConnectHandler(platformView);
		}

		/// <inheritdoc />
		protected override void DisconnectHandler(NativeCanvasView platformView)
		{
			touchHandler.Disconnect();
			platformView.PaintSurface -= OnPaintSurface;
			lastCanvasSize = default;
			base.DisconnectHandler(platformView);
		}

		/// <summary>Maps the logical-pixel setting to the native AppKit canvas.</summary>
		public static void MapIgnorePixelScaling(AppKitSKCanvasViewHandler handler, ISKCanvasView view) =>
			handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;

		/// <summary>Enables or disables native mouse input.</summary>
		public static void MapEnableTouchEvents(AppKitSKCanvasViewHandler handler, ISKCanvasView view) =>
			handler.touchHandler.SetEnabled(view.EnableTouchEvents);

		/// <summary>Requests an AppKit redraw on the next display pass.</summary>
		public static void OnInvalidateSurface(AppKitSKCanvasViewHandler handler, ISKCanvasView view, object? args) =>
			handler.PlatformView.NeedsDisplay = true;

		private void OnPaintSurface(object? sender, NativePaintEventArgs e)
		{
			var view = VirtualView;
			if (view is null)
				return;

			if (lastCanvasSize != e.Info.Size)
			{
				lastCanvasSize = e.Info.Size;
				view.OnCanvasSizeChanged(lastCanvasSize);
			}

			view.OnPaintSurface(new SKPaintSurfaceEventArgs(e.Surface, e.Info, e.RawInfo));
		}

		private sealed class MauiSKCanvasView : NativeCanvasView
		{
			private readonly MacTouchHandler touch;

			public MauiSKCanvasView(MacTouchHandler touch) => this.touch = touch;

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

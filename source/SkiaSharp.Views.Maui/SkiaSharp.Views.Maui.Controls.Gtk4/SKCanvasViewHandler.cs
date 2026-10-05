using System.Runtime.Versioning;
using Microsoft.Maui;
using Microsoft.Maui.Platforms.Linux.Gtk4.Handlers;
using SkiaSharp.Views.Gtk;

namespace SkiaSharp.Views.Maui.Controls.Gtk4;

/// <summary>GTK4 software-backed handler for the MAUI SkiaSharp canvas.</summary>
[SupportedOSPlatform("linux")]
public sealed class SKCanvasViewHandler : GtkViewHandler<ISKCanvasView, SKDrawingArea>
{
	private SKSizeI lastCanvasSize;
	private SKTouchHandler? touchHandler;

	private static readonly PropertyMapper<ISKCanvasView, SKCanvasViewHandler> Mapper =
		new(GtkViewHandler<ISKCanvasView, SKDrawingArea>.ViewMapper)
		{
			[nameof(ISKCanvasView.EnableTouchEvents)] = MapEnableTouchEvents,
			[nameof(ISKCanvasView.IgnorePixelScaling)] = MapIgnorePixelScaling,
		};

	private static readonly CommandMapper<ISKCanvasView, SKCanvasViewHandler> Commands =
		new(GtkViewHandler<ISKCanvasView, SKDrawingArea>.ViewCommandMapper)
		{
			[nameof(ISKCanvasView.InvalidateSurface)] = OnInvalidateSurface,
		};

	/// <summary>Creates the GTK4 canvas handler.</summary>
	public SKCanvasViewHandler() : base(Mapper, Commands) { }

	/// <inheritdoc />
	protected override SKDrawingArea CreatePlatformView() => new();

	/// <inheritdoc />
	protected override void ConnectHandler(SKDrawingArea platformView)
	{
		platformView.PaintSurface += OnPaintSurface;
		base.ConnectHandler(platformView);
	}

	/// <inheritdoc />
	protected override void DisconnectHandler(SKDrawingArea platformView)
	{
		touchHandler?.Detach();
		touchHandler = null;
		platformView.PaintSurface -= OnPaintSurface;
		lastCanvasSize = default;
		VirtualView?.OnCanvasSizeChanged(default);
		base.DisconnectHandler(platformView);
	}

	private static void OnInvalidateSurface(SKCanvasViewHandler handler, ISKCanvasView view, object? args) =>
		handler.PlatformView?.QueueDraw();

	private static void MapIgnorePixelScaling(SKCanvasViewHandler handler, ISKCanvasView view)
	{
		if (handler.PlatformView is { } platformView)
			platformView.IgnorePixelScaling = view.IgnorePixelScaling;
	}

	private static void MapEnableTouchEvents(SKCanvasViewHandler handler, ISKCanvasView view)
	{
		if (handler.PlatformView is not { } platformView)
			return;

		handler.touchHandler ??= new SKTouchHandler(
			platformView,
			e => handler.VirtualView?.OnTouch(e),
			() => handler.VirtualView?.IgnorePixelScaling == true ? 1 : platformView.GetScaleFactor());
		handler.touchHandler.SetEnabled(view.EnableTouchEvents);
	}

	private void OnPaintSurface(object? sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
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
}

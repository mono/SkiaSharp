using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui;
using Microsoft.Maui.Platforms.Linux.Gtk4.Handlers;
using SkiaSharp.Views.Gtk;
using SkiaSharp.Views.Maui.Platform.Gtk4;

namespace SkiaSharp.Views.Maui.Handlers.Gtk4;

/// <summary>GTK4 software-backed handler for the MAUI SkiaSharp canvas.</summary>
[UnconditionalSuppressMessage("Interoperability", "CA1416",
	Justification = "GTK4/GirCore supports the host's native GTK runtime on Windows and macOS.")]
public class SKCanvasViewHandler : GtkViewHandler<ISKCanvasView, SKDrawingArea>
{
	private SKSizeI lastCanvasSize;
	private SKTouchHandler? touchHandler;

	/// <summary>Maps canvas properties; extend or replace this mapper to customize the handler.</summary>
	public static PropertyMapper<ISKCanvasView, SKCanvasViewHandler> SKCanvasViewMapper =
		new(GtkViewHandler<ISKCanvasView, SKDrawingArea>.ViewMapper)
		{
			[nameof(ISKCanvasView.EnableTouchEvents)] = MapEnableTouchEvents,
			[nameof(ISKCanvasView.IgnorePixelScaling)] = MapIgnorePixelScaling,
		};

	/// <summary>Maps canvas commands, including surface invalidation.</summary>
	public static CommandMapper<ISKCanvasView, SKCanvasViewHandler> SKCanvasViewCommandMapper =
		new(GtkViewHandler<ISKCanvasView, SKDrawingArea>.ViewCommandMapper)
		{
			[nameof(ISKCanvasView.InvalidateSurface)] = OnInvalidateSurface,
		};

	/// <summary>Creates the GTK4 canvas handler.</summary>
	public SKCanvasViewHandler() : this(null, null) { }

	/// <summary>Creates a handler with custom mappers, using the standard mapper for each null argument.</summary>
	public SKCanvasViewHandler(PropertyMapper? mapper, CommandMapper? commands)
		: base(mapper ?? SKCanvasViewMapper, commands ?? SKCanvasViewCommandMapper) { }

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
		var previousTouchHandler = touchHandler;
		touchHandler = null;
		platformView.PaintSurface -= OnPaintSurface;
		lastCanvasSize = default;
		try
		{
			previousTouchHandler?.Detach(platformView);
		}
		finally
		{
			try
			{
				VirtualView?.OnCanvasSizeChanged(default);
			}
			finally
			{
				base.DisconnectHandler(platformView);
			}
		}
	}

	/// <summary>Queues a new canvas frame.</summary>
	public static void OnInvalidateSurface(SKCanvasViewHandler handler, ISKCanvasView view, object? args) =>
		handler.PlatformView?.QueueDraw();

	/// <summary>Updates the native view's coordinate scaling.</summary>
	public static void MapIgnorePixelScaling(SKCanvasViewHandler handler, ISKCanvasView view)
	{
		if (handler.PlatformView is { } platformView)
			platformView.IgnorePixelScaling = view.IgnorePixelScaling;
	}

	/// <summary>Attaches or removes GTK pointer controllers.</summary>
	public static void MapEnableTouchEvents(SKCanvasViewHandler handler, ISKCanvasView view)
	{
		if (handler.PlatformView is not { } platformView)
			return;

		handler.touchHandler ??= new SKTouchHandler(
			e => handler.VirtualView?.OnTouch(e),
			handler.OnGetScaledCoord);
		handler.touchHandler.SetEnabled(platformView, view.EnableTouchEvents);
	}

	private SKPoint OnGetScaledCoord(double x, double y) =>
		SKTouchHandler.GetTouchLocation(x, y, PlatformView?.GetScaleFactor() ?? 1,
			VirtualView?.IgnorePixelScaling == true);

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

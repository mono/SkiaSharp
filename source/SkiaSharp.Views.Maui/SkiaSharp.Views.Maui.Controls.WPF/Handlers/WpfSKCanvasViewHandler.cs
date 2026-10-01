using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Maui.Controls.WPF.Platform;
using SkiaSharp.Views.WPF;
using WpfPaintSurfaceEventArgs = SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs;

namespace SkiaSharp.Views.Maui.Controls.WPF.Handlers;

/// <summary>Renders a MAUI canvas using WPF's software-backed <see cref="SKElement"/>.</summary>
public sealed class WpfSKCanvasViewHandler : WPFViewHandler<ISKCanvasView, SKElement>
{
	private SKSizeI lastCanvasSize;
	private WpfTouchHandler? touchHandler;

	/// <summary>Maps canvas properties to the native WPF view.</summary>
	public static readonly PropertyMapper<ISKCanvasView, WpfSKCanvasViewHandler> Mapper =
		new(ViewMapper)
		{
			[nameof(ISKCanvasView.IgnorePixelScaling)] = MapIgnorePixelScaling,
			[nameof(ISKCanvasView.EnableTouchEvents)] = MapEnableTouchEvents,
		};

	/// <summary>Maps canvas commands to WPF rendering.</summary>
	public static readonly CommandMapper<ISKCanvasView, WpfSKCanvasViewHandler> CommandMapper =
		new(ViewCommandMapper)
		{
			[nameof(ISKCanvasView.InvalidateSurface)] = MapInvalidateSurface,
		};

	/// <summary>Creates a WPF canvas handler.</summary>
	public WpfSKCanvasViewHandler() : base(Mapper, CommandMapper) { }

	/// <inheritdoc />
	protected override SKElement CreatePlatformView() => new();

	/// <inheritdoc />
	protected override void ConnectHandler(SKElement platformView)
	{
		platformView.PaintSurface += OnPaintSurface;
		base.ConnectHandler(platformView);
	}

	/// <inheritdoc />
	protected override void DisconnectHandler(SKElement platformView)
	{
		touchHandler?.Detach();
		touchHandler = null;
		platformView.PaintSurface -= OnPaintSurface;
		lastCanvasSize = default;
		base.DisconnectHandler(platformView);
	}

	/// <summary>Updates the native canvas scaling mode.</summary>
	public static void MapIgnorePixelScaling(WpfSKCanvasViewHandler handler, ISKCanvasView view) =>
		handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;

	/// <summary>Enables or disables native WPF pointer events.</summary>
	public static void MapEnableTouchEvents(WpfSKCanvasViewHandler handler, ISKCanvasView view)
	{
		handler.touchHandler ??= new WpfTouchHandler(handler.PlatformView, () => handler.VirtualView?.IgnorePixelScaling ?? false, e => handler.VirtualView?.OnTouch(e));
		handler.touchHandler.SetEnabled(view.EnableTouchEvents);
	}

	/// <summary>Requests a WPF redraw.</summary>
	public static void MapInvalidateSurface(WpfSKCanvasViewHandler handler, ISKCanvasView view, object? args) =>
		handler.PlatformView.InvalidateVisual();

	private void OnPaintSurface(object? sender, WpfPaintSurfaceEventArgs e)
	{
		var view = VirtualView;
		if (view is null)
			return;

		var size = e.Info.Size;
		if (lastCanvasSize != size)
		{
			lastCanvasSize = size;
			view.OnCanvasSizeChanged(size);
		}

		view.OnPaintSurface(new SKPaintSurfaceEventArgs(e.Surface, e.Info, e.RawInfo));
	}
}

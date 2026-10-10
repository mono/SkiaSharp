using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Maui.Platform;
using SkiaSharp.Views.WPF;
using WPFPaintSurfaceEventArgs = SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs;

namespace SkiaSharp.Views.Maui.Handlers;

/// <summary>Renders a MAUI canvas using WPF's software-backed <see cref="SKElement"/>.</summary>
public sealed class WPFSKCanvasViewHandler : WPFViewHandler<ISKCanvasView, SKElement>
{
	private SKSizeI lastCanvasSize;
	private WPFTouchHandler? touchHandler;

	/// <summary>Maps canvas properties to the native WPF view.</summary>
	public static PropertyMapper<ISKCanvasView, WPFSKCanvasViewHandler> SKCanvasViewMapper =
		new(ViewMapper)
		{
			[nameof(ISKCanvasView.IgnorePixelScaling)] = MapIgnorePixelScaling,
			[nameof(ISKCanvasView.EnableTouchEvents)] = MapEnableTouchEvents,
		};

	/// <summary>Maps canvas commands to WPF rendering.</summary>
	public static CommandMapper<ISKCanvasView, WPFSKCanvasViewHandler> SKCanvasViewCommandMapper =
		new(ViewCommandMapper)
		{
			[nameof(ISKCanvasView.InvalidateSurface)] = MapInvalidateSurface,
		};

	/// <summary>Creates a WPF canvas handler.</summary>
	public WPFSKCanvasViewHandler() : base(SKCanvasViewMapper, SKCanvasViewCommandMapper) { }

	/// <summary>Creates a WPF canvas handler with custom property mappings.</summary>
	/// <param name="mapper">The property mapper, or <see langword="null"/> to use the default mappings.</param>
	public WPFSKCanvasViewHandler(PropertyMapper? mapper) : this(mapper, null) { }

	/// <summary>Creates a WPF canvas handler with custom property and command mappings.</summary>
	/// <param name="mapper">The property mapper, or <see langword="null"/> to use the default mappings.</param>
	/// <param name="commandMapper">The command mapper, or <see langword="null"/> to use the default commands.</param>
	public WPFSKCanvasViewHandler(PropertyMapper? mapper, CommandMapper? commandMapper)
		: base(mapper ?? SKCanvasViewMapper, commandMapper ?? SKCanvasViewCommandMapper) { }

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
	/// <param name="handler">The WPF canvas handler.</param>
	/// <param name="view">The canvas whose scaling mode changed.</param>
	public static void MapIgnorePixelScaling(WPFSKCanvasViewHandler handler, ISKCanvasView view) =>
		handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;

	/// <summary>Enables or disables native WPF pointer events.</summary>
	/// <param name="handler">The WPF canvas handler.</param>
	/// <param name="view">The canvas whose touch setting changed.</param>
	public static void MapEnableTouchEvents(WPFSKCanvasViewHandler handler, ISKCanvasView view)
	{
		handler.touchHandler ??= new WPFTouchHandler(
			handler.PlatformView,
			() => handler.VirtualView?.IgnorePixelScaling ?? false,
			e => handler.VirtualView?.OnTouch(e));
		handler.touchHandler.SetEnabled(view.EnableTouchEvents);
	}

	/// <summary>Requests a WPF redraw.</summary>
	/// <param name="handler">The WPF canvas handler.</param>
	/// <param name="view">The canvas requesting a redraw.</param>
	/// <param name="args">The command arguments, which are not used.</param>
	public static void MapInvalidateSurface(WPFSKCanvasViewHandler handler, ISKCanvasView view, object? args) =>
		handler.PlatformView.InvalidateVisual();

	private void OnPaintSurface(object? sender, WPFPaintSurfaceEventArgs e)
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

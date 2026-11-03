using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui;
using Microsoft.Maui.Platforms.Linux.Gtk4.Handlers;
using SkiaSharp.Views.Gtk;
using SkiaSharp.Views.Maui.Platform.Gtk4;

namespace SkiaSharp.Views.Maui.Handlers.Gtk4;

/// <summary>GTK4 OpenGL handler for the MAUI SkiaSharp GPU view.</summary>
[UnconditionalSuppressMessage("Interoperability", "CA1416",
	Justification = "GTK4/GirCore supports the host's native GTK runtime on Windows and macOS.")]
public class SKGLViewHandler : GtkViewHandler<ISKGLView, SKGLArea>
{
	private SKSizeI lastCanvasSize;
	private GRContext? lastGRContext;
	private SKTouchHandler? touchHandler;

	/// <summary>Maps GPU view properties; extend or replace this mapper to customize the handler.</summary>
	public static PropertyMapper<ISKGLView, SKGLViewHandler> SKGLViewMapper =
		new(GtkViewHandler<ISKGLView, SKGLArea>.ViewMapper)
		{
			[nameof(ISKGLView.EnableTouchEvents)] = MapEnableTouchEvents,
			[nameof(ISKGLView.IgnorePixelScaling)] = MapIgnorePixelScaling,
			[nameof(ISKGLView.HasRenderLoop)] = MapHasRenderLoop,
		};

	/// <summary>Maps GPU view commands, including surface invalidation.</summary>
	public static CommandMapper<ISKGLView, SKGLViewHandler> SKGLViewCommandMapper =
		new(GtkViewHandler<ISKGLView, SKGLArea>.ViewCommandMapper)
		{
			[nameof(ISKGLView.InvalidateSurface)] = OnInvalidateSurface,
		};

	/// <summary>Creates the GTK4 GPU handler.</summary>
	public SKGLViewHandler() : this(null, null) { }

	/// <summary>Creates a handler with custom mappers, using the standard mapper for each null argument.</summary>
	public SKGLViewHandler(PropertyMapper? mapper, CommandMapper? commands)
		: base(mapper ?? SKGLViewMapper, commands ?? SKGLViewCommandMapper) { }

	/// <inheritdoc />
	protected override SKGLArea CreatePlatformView() => new();

	/// <inheritdoc />
	protected override void ConnectHandler(SKGLArea platformView)
	{
		platformView.OnUnrealize += OnUnrealize;
		platformView.PaintSurface += OnPaintSurface;
		base.ConnectHandler(platformView);
		platformView.QueueRender();
	}

	/// <inheritdoc />
	protected override void DisconnectHandler(SKGLArea platformView)
	{
		platformView.EnableRenderLoop = false;
		var previousTouchHandler = touchHandler;
		touchHandler = null;
		platformView.OnUnrealize -= OnUnrealize;
		platformView.PaintSurface -= OnPaintSurface;
		try
		{
			previousTouchHandler?.Detach(platformView);
		}
		finally
		{
			try
			{
				ResetGpuState();
			}
			finally
			{
				base.DisconnectHandler(platformView);
			}
		}
	}

	/// <summary>Queues a GPU frame when the continuous render loop is disabled.</summary>
	public static void OnInvalidateSurface(SKGLViewHandler handler, ISKGLView view, object? args)
	{
		if (handler.PlatformView is { } platformView && !view.HasRenderLoop)
			platformView.QueueRender();
	}

	/// <summary>Updates the native view's coordinate scaling.</summary>
	public static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view)
	{
		if (handler.PlatformView is { } platformView)
			platformView.IgnorePixelScaling = view.IgnorePixelScaling;
	}

	/// <summary>Starts or stops continuous GPU rendering.</summary>
	public static void MapHasRenderLoop(SKGLViewHandler handler, ISKGLView view)
	{
		if (handler.PlatformView is { } platformView)
			platformView.EnableRenderLoop = view.HasRenderLoop;
	}

	/// <summary>Attaches or removes GTK pointer controllers.</summary>
	public static void MapEnableTouchEvents(SKGLViewHandler handler, ISKGLView view)
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

	private void OnUnrealize(global::Gtk.Widget sender, EventArgs args) => ResetGpuState();

	private void ResetGpuState()
	{
		var hadContext = lastGRContext is not null;
		var hadSize = lastCanvasSize != default;
		lastGRContext = null;
		lastCanvasSize = default;
		try
		{
			if (hadContext)
				VirtualView?.OnGRContextChanged(null);
		}
		finally
		{
			if (hadSize)
				VirtualView?.OnCanvasSizeChanged(default);
		}
	}

	private void OnPaintSurface(object? sender, SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs e)
	{
		var view = VirtualView;
		if (view is null)
			return;

		var context = PlatformView?.GRContext
			?? throw new InvalidOperationException("GTK4 GPU paint requires a GPU context.");
		if (lastGRContext != context)
		{
			lastGRContext = context;
			view.OnGRContextChanged(context);
		}

		if (lastCanvasSize != e.Info.Size)
		{
			lastCanvasSize = e.Info.Size;
			view.OnCanvasSizeChanged(lastCanvasSize);
		}
		view.OnPaintSurface(new SKPaintGLSurfaceEventArgs(e.Surface, e.BackendRenderTarget,
			e.Origin, e.Info, e.RawInfo));
	}
}

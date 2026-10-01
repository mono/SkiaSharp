using System;
using System.Runtime.Versioning;
using Microsoft.Maui;
using Microsoft.Maui.Platforms.Linux.Gtk4.Handlers;
using GtkGLView = SkiaSharp.Views.Gtk.SKGLView;

namespace SkiaSharp.Views.Maui.Controls.Gtk4;

/// <summary>GTK4 OpenGL handler for the MAUI SkiaSharp GPU view.</summary>
[SupportedOSPlatform("linux")]
public sealed class SKGLViewHandler : GtkViewHandler<ISKGLView, GtkGLView>
{
	private SKSizeI lastCanvasSize;
	private GRContext? lastContext;
	private SKTouchHandler? touchHandler;

	private static readonly PropertyMapper<ISKGLView, SKGLViewHandler> Mapper =
		new(GtkViewHandler<ISKGLView, GtkGLView>.ViewMapper)
		{
			[nameof(ISKGLView.EnableTouchEvents)] = MapEnableTouchEvents,
			[nameof(ISKGLView.IgnorePixelScaling)] = MapIgnorePixelScaling,
			[nameof(ISKGLView.HasRenderLoop)] = MapHasRenderLoop,
		};

	private static readonly CommandMapper<ISKGLView, SKGLViewHandler> Commands =
		new(GtkViewHandler<ISKGLView, GtkGLView>.ViewCommandMapper)
		{
			[nameof(ISKGLView.InvalidateSurface)] = OnInvalidateSurface,
		};

	/// <summary>Creates the GTK4 GPU handler.</summary>
	public SKGLViewHandler() : base(Mapper, Commands) { }

	/// <inheritdoc />
	protected override GtkGLView CreatePlatformView() => new();

	/// <inheritdoc />
	protected override void ConnectHandler(GtkGLView platformView)
	{
		platformView.ContextChanged += OnContextChanged;
		platformView.PaintSurface += OnPaintSurface;
		base.ConnectHandler(platformView);
		platformView.QueueRender();
	}

	/// <inheritdoc />
	protected override void DisconnectHandler(GtkGLView platformView)
	{
		platformView.EnableRenderLoop = false;
		touchHandler?.Detach();
		touchHandler = null;
		platformView.ReleaseGlResources();
		platformView.ContextChanged -= OnContextChanged;
		platformView.PaintSurface -= OnPaintSurface;
		lastContext = null;
		lastCanvasSize = default;
		VirtualView?.OnGRContextChanged(null);
		VirtualView?.OnCanvasSizeChanged(default);
		base.DisconnectHandler(platformView);
	}

	private static void OnInvalidateSurface(SKGLViewHandler handler, ISKGLView view, object? args)
	{
		if (handler.PlatformView is { } platformView && !view.HasRenderLoop)
			platformView.QueueRender();
	}

	private static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view)
	{
		if (handler.PlatformView is { } platformView)
			platformView.IgnorePixelScaling = view.IgnorePixelScaling;
	}

	private static void MapHasRenderLoop(SKGLViewHandler handler, ISKGLView view)
	{
		if (handler.PlatformView is { } platformView)
			platformView.EnableRenderLoop = view.HasRenderLoop;
	}

	private static void MapEnableTouchEvents(SKGLViewHandler handler, ISKGLView view)
	{
		if (handler.PlatformView is not { } platformView)
			return;

		handler.touchHandler ??= new SKTouchHandler(
			platformView,
			e => handler.VirtualView?.OnTouch(e),
			() => handler.VirtualView?.IgnorePixelScaling == true ? 1 : platformView.GetScaleFactor());
		handler.touchHandler.SetEnabled(view.EnableTouchEvents);
	}

	private void OnContextChanged(object? sender, EventArgs args)
	{
		var context = PlatformView?.GRContext;
		if (lastContext == context)
			return;
		lastContext = context;
		VirtualView?.OnGRContextChanged(context);
		if (context is null)
		{
			lastCanvasSize = default;
			VirtualView?.OnCanvasSizeChanged(default);
		}
	}

	private void OnPaintSurface(object? sender, SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs e)
	{
		var view = VirtualView;
		if (view is null)
			return;

		if (lastCanvasSize != e.Info.Size)
		{
			lastCanvasSize = e.Info.Size;
			view.OnCanvasSizeChanged(lastCanvasSize);
		}
		view.OnPaintSurface(new SKPaintGLSurfaceEventArgs(e.Surface, e.BackendRenderTarget,
			e.Origin, e.Info, e.RawInfo));
	}
}

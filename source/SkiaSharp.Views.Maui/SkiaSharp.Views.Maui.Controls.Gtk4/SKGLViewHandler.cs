using System;
using System.Runtime.Versioning;
using Microsoft.Maui;
using Microsoft.Maui.Platforms.Linux.Gtk4.Handlers;
using SkiaSharp.Views.Gtk;

namespace SkiaSharp.Views.Maui.Controls.Gtk4;

/// <summary>GTK4 OpenGL handler for the MAUI SkiaSharp GPU view.</summary>
[SupportedOSPlatform("linux")]
public sealed class SKGLViewHandler : GtkViewHandler<ISKGLView, SKGLArea>
{
	private SKSizeI lastCanvasSize;
	private GRContext? lastContext;
	private SKTouchHandler? touchHandler;

	private static readonly PropertyMapper<ISKGLView, SKGLViewHandler> Mapper =
		new(GtkViewHandler<ISKGLView, SKGLArea>.ViewMapper)
		{
			[nameof(ISKGLView.EnableTouchEvents)] = MapEnableTouchEvents,
			[nameof(ISKGLView.IgnorePixelScaling)] = MapIgnorePixelScaling,
			[nameof(ISKGLView.HasRenderLoop)] = MapHasRenderLoop,
		};

	private static readonly CommandMapper<ISKGLView, SKGLViewHandler> Commands =
		new(GtkViewHandler<ISKGLView, SKGLArea>.ViewCommandMapper)
		{
			[nameof(ISKGLView.InvalidateSurface)] = OnInvalidateSurface,
		};

	/// <summary>Creates the GTK4 GPU handler.</summary>
	public SKGLViewHandler() : base(Mapper, Commands) { }

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
		touchHandler?.Detach();
		touchHandler = null;
		platformView.OnUnrealize -= OnUnrealize;
		platformView.PaintSurface -= OnPaintSurface;
		ResetGpuState();
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

	private void OnUnrealize(global::Gtk.Widget sender, EventArgs args) => ResetGpuState();

	private void ResetGpuState()
	{
		if (lastContext is not null)
		{
			lastContext = null;
			VirtualView?.OnGRContextChanged(null);
		}
		if (lastCanvasSize != default)
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

		var context = PlatformView?.GRContext
			?? throw new InvalidOperationException("GTK4 GPU paint requires a GPU context.");
		if (lastContext != context)
		{
			lastContext = context;
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

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Maui.Controls.WPF.Platform;
using SkiaSharp.Views.WPF;
using WpfPaintGLSurfaceEventArgs = SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs;

namespace SkiaSharp.Views.Maui.Controls.WPF.Handlers;

/// <summary>Renders a MAUI GPU canvas using WPF's OpenGL-backed <see cref="SKGLElement"/>.</summary>
public sealed class WpfSKGLViewHandler : WPFViewHandler<ISKGLView, WpfSKGLViewHandler.MauiSKGLElement>
{
	private SKSizeI lastCanvasSize;
	private GRContext? lastContext;
	private WpfTouchHandler? touchHandler;
	private DispatcherTimer? renderTimer;

	/// <summary>Maps GPU canvas properties to the native WPF view.</summary>
	public static readonly PropertyMapper<ISKGLView, WpfSKGLViewHandler> Mapper =
		new(ViewMapper)
		{
			[nameof(ISKGLView.IgnorePixelScaling)] = MapIgnorePixelScaling,
			[nameof(ISKGLView.EnableTouchEvents)] = MapEnableTouchEvents,
			[nameof(ISKGLView.HasRenderLoop)] = MapHasRenderLoop,
		};

	/// <summary>Maps GPU canvas commands to WPF rendering.</summary>
	public static readonly CommandMapper<ISKGLView, WpfSKGLViewHandler> CommandMapper =
		new(ViewCommandMapper)
		{
			[nameof(ISKGLView.InvalidateSurface)] = MapInvalidateSurface,
		};

	/// <summary>Creates a WPF GPU canvas handler.</summary>
	public WpfSKGLViewHandler() : base(Mapper, CommandMapper) { }

	/// <inheritdoc />
	protected override MauiSKGLElement CreatePlatformView() => new();

	/// <inheritdoc />
	protected override void ConnectHandler(MauiSKGLElement platformView)
	{
		platformView.PaintSurface += OnPaintSurface;
		platformView.Loaded += OnLoaded;
		platformView.Unloaded += OnUnloaded;
		base.ConnectHandler(platformView);
		UpdateRenderLoop();
	}

	/// <inheritdoc />
	protected override void DisconnectHandler(MauiSKGLElement platformView)
	{
		StopRenderLoop();
		touchHandler?.Detach();
		touchHandler = null;
		platformView.PaintSurface -= OnPaintSurface;
		platformView.Loaded -= OnLoaded;
		platformView.Unloaded -= OnUnloaded;
		NotifyContextLost();
		lastCanvasSize = default;
		platformView.Dispose();
		base.DisconnectHandler(platformView);
	}

	/// <summary>Updates pixel scaling and requests a redraw.</summary>
	public static void MapIgnorePixelScaling(WpfSKGLViewHandler handler, ISKGLView view)
	{
		handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;
		handler.PlatformView.InvalidateVisual();
	}

	/// <summary>Enables or disables native WPF pointer events.</summary>
	public static void MapEnableTouchEvents(WpfSKGLViewHandler handler, ISKGLView view)
	{
		handler.touchHandler ??= new WpfTouchHandler(handler.PlatformView, () => handler.VirtualView?.IgnorePixelScaling ?? false, e => handler.VirtualView?.OnTouch(e));
		handler.touchHandler.SetEnabled(view.EnableTouchEvents);
	}

	/// <summary>Starts or stops continuous rendering while the view is loaded.</summary>
	public static void MapHasRenderLoop(WpfSKGLViewHandler handler, ISKGLView view) =>
		handler.UpdateRenderLoop();

	/// <summary>Requests a frame when the continuous render loop is disabled.</summary>
	public static void MapInvalidateSurface(WpfSKGLViewHandler handler, ISKGLView view, object? args)
	{
		if (!view.HasRenderLoop)
			handler.PlatformView.InvalidateVisual();
	}

	private void OnLoaded(object sender, RoutedEventArgs e) => UpdateRenderLoop();

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		StopRenderLoop();
		NotifyContextLost();
	}

	private void UpdateRenderLoop()
	{
		if (PlatformView?.IsLoaded != true || VirtualView?.HasRenderLoop != true)
		{
			StopRenderLoop();
			return;
		}

		if (renderTimer is not null)
			return;

		renderTimer = new DispatcherTimer(DispatcherPriority.Render, PlatformView.Dispatcher)
		{
			Interval = TimeSpan.FromMilliseconds(16),
		};
		renderTimer.Tick += OnRenderTick;
		renderTimer.Start();
	}

	private void OnRenderTick(object? sender, EventArgs e) => PlatformView?.InvalidateVisual();

	private void StopRenderLoop()
	{
		if (renderTimer is null)
			return;

		renderTimer.Stop();
		renderTimer.Tick -= OnRenderTick;
		renderTimer = null;
	}

	private void NotifyContextLost()
	{
		if (lastContext is null)
			return;

		lastContext = null;
		VirtualView?.OnGRContextChanged(null);
	}

	private void OnPaintSurface(object? sender, WpfPaintGLSurfaceEventArgs e)
	{
		var view = VirtualView;
		if (view is null || sender is not SKGLElement element)
			return;

		var size = e.Info.Size;
		if (lastCanvasSize != size)
		{
			lastCanvasSize = size;
			view.OnCanvasSizeChanged(size);
		}

		var context = element.GRContext;
		if (lastContext != context)
		{
			lastContext = context;
			view.OnGRContextChanged(context);
		}

		view.OnPaintSurface(new SKPaintGLSurfaceEventArgs(e.Surface, e.BackendRenderTarget, e.Origin, e.Info, e.RawInfo));
	}

	/// <summary>A WPF GL control that supplies logical paint coordinates when pixel scaling is ignored.</summary>
	public sealed class MauiSKGLElement : SKGLElement
	{
		/// <summary>Gets or sets whether paint coordinates use WPF device-independent pixels.</summary>
		public bool IgnorePixelScaling { get; set; }

		/// <inheritdoc />
		protected override void OnPaintSurface(WpfPaintGLSurfaceEventArgs e)
		{
			if (!IgnorePixelScaling)
			{
				base.OnPaintSurface(e);
				return;
			}

			var source = PresentationSource.FromVisual(this);
			var transform = source?.CompositionTarget.TransformToDevice ?? Matrix.Identity;
			e.Surface.Canvas.Scale((float)transform.M11, (float)transform.M22);
			var logicalSize = WpfCanvasMetrics.GetLogicalSize(ActualWidth, ActualHeight);
			base.OnPaintSurface(new WpfPaintGLSurfaceEventArgs(e.Surface, e.BackendRenderTarget, e.Origin, e.Info.WithSize(logicalSize), e.Info));
		}
	}
}

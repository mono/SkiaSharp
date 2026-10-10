using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Maui.Platform;
using SkiaSharp.Views.WPF;
using WPFPaintGLSurfaceEventArgs = SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs;

namespace SkiaSharp.Views.Maui.Handlers;

/// <summary>Renders a MAUI GPU canvas using WPF's OpenGL-backed <see cref="SKGLElement"/>.</summary>
public sealed class WPFSKGLViewHandler : WPFViewHandler<ISKGLView, SKGLElement>
{
	private SKSizeI lastCanvasSize;
	private GRContext? lastContext;
	private WPFTouchHandler? touchHandler;

	/// <summary>Maps GPU canvas properties to the native WPF view.</summary>
	public static PropertyMapper<ISKGLView, WPFSKGLViewHandler> SKGLViewMapper =
		new(ViewMapper)
		{
			[nameof(ISKGLView.IgnorePixelScaling)] = MapIgnorePixelScaling,
			[nameof(ISKGLView.EnableTouchEvents)] = MapEnableTouchEvents,
			[nameof(ISKGLView.HasRenderLoop)] = MapHasRenderLoop,
		};

	/// <summary>Maps GPU canvas commands to WPF rendering.</summary>
	public static CommandMapper<ISKGLView, WPFSKGLViewHandler> SKGLViewCommandMapper =
		new(ViewCommandMapper)
		{
			[nameof(ISKGLView.InvalidateSurface)] = MapInvalidateSurface,
		};

	/// <summary>Creates a WPF GPU canvas handler.</summary>
	public WPFSKGLViewHandler() : base(SKGLViewMapper, SKGLViewCommandMapper) { }

	/// <summary>Creates a WPF GPU canvas handler with custom property mappings.</summary>
	/// <param name="mapper">The property mapper, or <see langword="null"/> to use the default mappings.</param>
	public WPFSKGLViewHandler(PropertyMapper? mapper) : this(mapper, null) { }

	/// <summary>Creates a WPF GPU canvas handler with custom property and command mappings.</summary>
	/// <param name="mapper">The property mapper, or <see langword="null"/> to use the default mappings.</param>
	/// <param name="commandMapper">The command mapper, or <see langword="null"/> to use the default commands.</param>
	public WPFSKGLViewHandler(PropertyMapper? mapper, CommandMapper? commandMapper)
		: base(mapper ?? SKGLViewMapper, commandMapper ?? SKGLViewCommandMapper) { }

	/// <inheritdoc />
	protected override SKGLElement CreatePlatformView() => new MauiSKGLElement();

	/// <inheritdoc />
	protected override void ConnectHandler(SKGLElement platformView)
	{
		platformView.PaintSurface += OnPaintSurface;
		platformView.Loaded += OnLoaded;
		platformView.Unloaded += OnUnloaded;
		base.ConnectHandler(platformView);
		UpdateRenderLoop();
	}

	/// <inheritdoc />
	protected override void DisconnectHandler(SKGLElement platformView)
	{
		platformView.RenderContinuously = false;
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
	/// <param name="handler">The WPF GPU canvas handler.</param>
	/// <param name="view">The canvas whose scaling mode changed.</param>
	public static void MapIgnorePixelScaling(WPFSKGLViewHandler handler, ISKGLView view)
	{
		if (((IElementHandler)handler).PlatformView is not MauiSKGLElement platformView)
			return;

		platformView.IgnorePixelScaling = view.IgnorePixelScaling;
		platformView.InvalidateVisual();
	}

	/// <summary>Enables or disables native WPF pointer events.</summary>
	/// <param name="handler">The WPF GPU canvas handler.</param>
	/// <param name="view">The canvas whose touch setting changed.</param>
	public static void MapEnableTouchEvents(WPFSKGLViewHandler handler, ISKGLView view)
	{
		handler.touchHandler ??= new WPFTouchHandler(
			handler.PlatformView,
			() => handler.VirtualView?.IgnorePixelScaling ?? false,
			e => handler.VirtualView?.OnTouch(e));
		handler.touchHandler.SetEnabled(view.EnableTouchEvents);
	}

	/// <summary>Starts or stops continuous rendering while the view is loaded.</summary>
	/// <param name="handler">The WPF GPU canvas handler.</param>
	/// <param name="view">The canvas whose render-loop setting changed.</param>
	public static void MapHasRenderLoop(WPFSKGLViewHandler handler, ISKGLView view) =>
		handler.UpdateRenderLoop();

	/// <summary>Requests a frame when the continuous render loop is disabled.</summary>
	/// <param name="handler">The WPF GPU canvas handler.</param>
	/// <param name="view">The canvas requesting a frame.</param>
	/// <param name="args">The command arguments, which are not used.</param>
	public static void MapInvalidateSurface(WPFSKGLViewHandler handler, ISKGLView view, object? args)
	{
		if (!view.HasRenderLoop)
			handler.PlatformView.InvalidateVisual();
	}

	private void OnLoaded(object sender, RoutedEventArgs e) => UpdateRenderLoop();

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		if (sender is SKGLElement platformView)
			platformView.RenderContinuously = false;
		NotifyContextLost();
	}

	private void UpdateRenderLoop()
	{
		if (((IElementHandler)this).PlatformView is SKGLElement platformView)
			platformView.RenderContinuously = platformView.IsLoaded && VirtualView?.HasRenderLoop == true;
	}

	private void NotifyContextLost()
	{
		if (lastContext is null)
			return;

		lastContext = null;
		VirtualView?.OnGRContextChanged(null);
	}

	private void OnPaintSurface(object? sender, WPFPaintGLSurfaceEventArgs e)
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
		protected override void OnPaintSurface(WPFPaintGLSurfaceEventArgs e)
		{
			if (!IgnorePixelScaling)
			{
				base.OnPaintSurface(e);
				return;
			}

			var source = PresentationSource.FromVisual(this);
			var transform = source?.CompositionTarget.TransformToDevice ?? Matrix.Identity;
			e.Surface.Canvas.Scale((float)transform.M11, (float)transform.M22);
			var logicalSize = new SKSizeI((int)ActualWidth, (int)ActualHeight);
			base.OnPaintSurface(new WPFPaintGLSurfaceEventArgs(e.Surface, e.BackendRenderTarget, e.Origin, e.Info.WithSize(logicalSize), e.Info));
		}
	}
}

using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal;

/// <summary>
/// The direct (Blazor WebAssembly) rendering strategy for <c>SKGLView</c>: SkiaSharp draws through
/// a real WebGL context in the browser, backed by a <see cref="GRContext"/> and a
/// <see cref="GRBackendRenderTarget"/> wrapping the canvas framebuffer. The render loop is driven
/// by the browser's <c>requestAnimationFrame</c>.
/// </summary>
[SupportedOSPlatform("browser")]
internal sealed class GLDirectRenderer : IRenderer
{
	private const int ResourceCacheBytes = 256 * 1024 * 1024; // 256 MB
	private const SKColorType ColorType = SKColorType.Rgba8888;
	private const GRSurfaceOrigin SurfaceOrigin = GRSurfaceOrigin.BottomLeft;

	private readonly IJSRuntime js;
	private readonly RenderPaintHandler paint;

	private SKHtmlCanvasInterop interop = null!;
	private SizeWatcherInterop sizeWatcher = null!;
	private DpiWatcherInterop dpiWatcher = null!;
	private SKHtmlCanvasInterop.GLInfo jsGLInfo = null!;
	private ElementReference htmlCanvas;

	private GRContext? context;
	private GRGlInterface? glInterface;
	private GRBackendRenderTarget? renderTarget;
	private SKSize renderTargetSize;
	private SKSurface? surface;
	private SKCanvas? canvas;
	private double dpi;
	private SKSize canvasSize;

	public GLDirectRenderer(IJSRuntime js, RenderPaintHandler paint)
	{
		this.js = js;
		this.paint = paint;
	}

	public bool EnableRenderLoop { get; set; }

	public bool IgnorePixelScaling { get; set; }

	// Transfer settings only apply to the bridged path; ignored here.
	public SKBlazorTransferFormat? TransferFormat { get; set; }

	public int? Quality { get; set; }

	public double Dpi => dpi;

	public async Task InitializeAsync(ElementReference canvas)
	{
		htmlCanvas = canvas;

		interop = await SKHtmlCanvasInterop.ImportAsync(js, htmlCanvas, OnRenderFrame);
		jsGLInfo = interop.InitGL();

		sizeWatcher = await SizeWatcherInterop.ImportAsync(js, htmlCanvas, OnSizeChanged);
		dpiWatcher = await DpiWatcherInterop.ImportAsync(js, OnDpiChanged);
	}

	public void Invalidate()
	{
		if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0 || jsGLInfo == null)
			return;

		interop.RequestAnimationFrame(EnableRenderLoop, (int)(canvasSize.Width * dpi), (int)(canvasSize.Height * dpi));
	}

	private void OnRenderFrame()
	{
		if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0 || jsGLInfo == null)
			return;

		// create the SkiaSharp context
		if (context == null)
		{
			glInterface = GRGlInterface.Create();
			context = GRContext.CreateGl(glInterface);

			// bump the default resource cache limit
			context.SetResourceCacheLimit(ResourceCacheBytes);
		}

		// get the new surface size
		var newSize = CreateSize(out var unscaledSize);
		var info = new SKImageInfo(newSize.Width, newSize.Height, ColorType);
		var userVisibleSize = IgnorePixelScaling ? unscaledSize : info.Size;

		// manage the drawing surface
		if (renderTarget == null || renderTargetSize != newSize || !renderTarget.IsValid)
		{
			// create or update the dimensions
			renderTargetSize = newSize;

			var glInfo = new GRGlFramebufferInfo(jsGLInfo.FboId, ColorType.ToGlSizedFormat());

			// destroy the old surface
			surface?.Dispose();
			surface = null;
			canvas = null;

			// re-create the render target
			renderTarget?.Dispose();
			renderTarget = new GRBackendRenderTarget(newSize.Width, newSize.Height, jsGLInfo.Samples, jsGLInfo.Stencils, glInfo);
		}

		// create the surface
		if (surface == null)
		{
			surface = SKSurface.Create(context, renderTarget, SurfaceOrigin, ColorType);
			canvas = surface.Canvas;
		}

		using (new SKAutoCanvasRestore(canvas, true))
		{
			if (IgnorePixelScaling)
			{
				canvas!.Scale((float)dpi);
				canvas.Save();
			}

			paint(surface, info.WithSize(userVisibleSize), info, renderTarget, SurfaceOrigin);
		}

		// update the control
		canvas?.Flush();
		context.Flush();
	}

	private SKSizeI CreateSize(out SKSizeI unscaledSize)
	{
		unscaledSize = SKSizeI.Empty;

		var w = canvasSize.Width;
		var h = canvasSize.Height;

		if (!IsPositive(w) || !IsPositive(h))
			return SKSizeI.Empty;

		unscaledSize = new SKSizeI((int)w, (int)h);
		return new SKSizeI((int)(w * dpi), (int)(h * dpi));

		static bool IsPositive(double value) =>
			!double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
	}

	private void OnDpiChanged(double newDpi)
	{
		dpi = newDpi;
		Invalidate();
	}

	private void OnSizeChanged(SKSize newSize)
	{
		canvasSize = newSize;
		Invalidate();
	}

	public ValueTask DisposeAsync()
	{
		dpiWatcher?.Unsubscribe(OnDpiChanged);
		sizeWatcher?.Dispose();
		interop?.Dispose();

		surface?.Dispose();
		renderTarget?.Dispose();
		context?.Dispose();
		glInterface?.Dispose();

		return ValueTask.CompletedTask;
	}
}

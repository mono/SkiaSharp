using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal;

/// <summary>
/// The direct (Blazor WebAssembly) rendering strategy for <c>SKCanvasView</c>: SkiaSharp draws in
/// the browser into a raster surface and the pixels are written straight into the HTML canvas via
/// <c>putImageData</c>. The render loop is driven by the browser's <c>requestAnimationFrame</c>.
/// </summary>
[SupportedOSPlatform("browser")]
internal sealed class CanvasDirectRenderer : IRenderer
{
	private readonly IJSRuntime js;
	private readonly RenderPaintHandler paint;

	private SKHtmlCanvasInterop interop = null!;
	private SizeWatcherInterop sizeWatcher = null!;
	private DpiWatcherInterop dpiWatcher = null!;
	private ElementReference htmlCanvas;

	private SKSizeI pixelSize;
	private byte[]? pixels;
	private GCHandle pixelsHandle;
	private double dpi;
	private SKSize canvasSize;

	public CanvasDirectRenderer(IJSRuntime js, RenderPaintHandler paint)
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
		interop.InitRaster();

		sizeWatcher = await SizeWatcherInterop.ImportAsync(js, htmlCanvas, OnSizeChanged);
		dpiWatcher = await DpiWatcherInterop.ImportAsync(js, OnDpiChanged);
	}

	public void Invalidate()
	{
		if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0)
			return;

		interop.RequestAnimationFrame(EnableRenderLoop, (int)(canvasSize.Width * dpi), (int)(canvasSize.Height * dpi));
	}

	private void OnRenderFrame()
	{
		if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0)
			return;

		var info = CreateBitmap(out var unscaledSize);
		var userVisibleSize = IgnorePixelScaling ? unscaledSize : info.Size;

		using (var surface = SKSurface.Create(info, pixelsHandle.AddrOfPinnedObject(), info.RowBytes))
		{
			if (IgnorePixelScaling)
			{
				var canvas = surface.Canvas;
				canvas.Scale((float)dpi);
				canvas.Save();
			}

			paint(surface, info.WithSize(userVisibleSize), info, null, default);
		}

		interop.PutImageData(pixelsHandle.AddrOfPinnedObject(), info.Size);
	}

	private SKImageInfo CreateBitmap(out SKSizeI unscaledSize)
	{
		var size = CreateSize(out unscaledSize);
		var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Opaque);

		if (pixels == null || pixelSize.Width != info.Width || pixelSize.Height != info.Height)
		{
			FreeBitmap();

			pixels = new byte[info.BytesSize];
			pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
			pixelSize = info.Size;
		}

		return info;
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

	private void FreeBitmap()
	{
		if (pixels != null)
		{
			pixelsHandle.Free();
			pixels = null;
		}
	}

	private void OnDpiChanged(double newDpi)
	{
		dpi = newDpi;
		Invalidate();
	}

	private void OnSizeChanged(SKSize newSize)
	{
		if ((int)(canvasSize.Width * dpi) == newSize.Width && (int)(canvasSize.Height * dpi) == newSize.Height)
			return;
		canvasSize = newSize;
		Invalidate();
	}

	public ValueTask DisposeAsync()
	{
		dpiWatcher?.Unsubscribe(OnDpiChanged);
		sizeWatcher?.Dispose();
		interop?.Dispose();
		FreeBitmap();
		return ValueTask.CompletedTask;
	}
}

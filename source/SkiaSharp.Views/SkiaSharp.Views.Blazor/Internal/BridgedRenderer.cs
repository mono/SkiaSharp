using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal;

/// <summary>
/// The bridged rendering strategy shared by <c>SKCanvasView</c> and <c>SKGLView</c> (Blazor
/// Server, Hybrid and static SSR): it renders on the .NET side into an RGBA raster surface,
/// produces the transfer payload, and presents it to the browser via
/// <see cref="SKHtmlCanvasBridgeInterop"/>.
/// </summary>
/// <remarks>
/// The render loop matches the WebAssembly semantics (driven by <c>EnableRenderLoop</c> and
/// <c>Invalidate()</c>) and is protected by backpressure: the next frame is only produced after
/// the previous transfer completes, byte-identical frames are suppressed, and each iteration
/// yields to the host dispatcher. Payload production runs off the dispatcher so it does not stall
/// the UI thread on Blazor Hybrid. This object is the target of the JavaScript
/// <c>DotNetObjectReference</c> for size/DPI callbacks.
/// </remarks>
internal sealed class BridgedRenderer : IRenderer
{
	private readonly IJSRuntime js;
	private readonly bool isGL;
	private readonly HostKind hostKind;
	private readonly SKBlazorOptions options;
	private readonly RenderPaintHandler paint;

	private SKHtmlCanvasBridgeInterop? bridge;
	private DotNetObjectReference<BridgedRenderer>? selfRef;

	private byte[]? pixels;
	private System.Runtime.InteropServices.GCHandle pixelsHandle;
	private SKSizeI pixelSize;
	private byte[]? lastFrame;

	private SKSize canvasSize;
	private double dpi;

	private bool initialized;
	private bool inFlight;
	private bool requested;
	private bool disposed;

	public BridgedRenderer(
		IJSRuntime js,
		bool isGL,
		HostKind hostKind,
		SKBlazorOptions options,
		RenderPaintHandler paint)
	{
		this.js = js ?? throw new ArgumentNullException(nameof(js));
		this.isGL = isGL;
		this.hostKind = hostKind;
		this.options = options ?? SKBlazorOptions.Default;
		this.paint = paint ?? throw new ArgumentNullException(nameof(paint));
	}

	public bool EnableRenderLoop { get; set; }

	public bool IgnorePixelScaling { get; set; }

	public SKBlazorTransferFormat? TransferFormat { get; set; }

	public int? Quality { get; set; }

	public double Dpi => dpi;

	public async Task InitializeAsync(ElementReference canvas)
	{
		bridge = new SKHtmlCanvasBridgeInterop(js, canvas);
		await bridge.ImportAsync();

		selfRef = DotNetObjectReference.Create(this);

		// Mark initialized before invoking the JS initializer: the JS side reports the initial
		// metrics synchronously during initialize(), and that OnMetricsChanged callback can race
		// back to .NET before this method returns. If we are not yet "initialized" the first
		// render would be dropped and nothing would re-trigger it.
		initialized = true;
		await bridge.InitializeAsync(selfRef, isGL);
	}

	/// <summary>Invoked by the bridge JavaScript when the element size or DPR changes.</summary>
	[JSInvokable]
	public Task OnMetricsChanged(double width, double height, double newDpi)
	{
		canvasSize = new SKSize((float)width, (float)height);
		dpi = newDpi > 0 ? newDpi : 1;
		Invalidate();
		return Task.CompletedTask;
	}

	public void Invalidate()
	{
		_ = RenderLoopAsync();
	}

	private async Task RenderLoopAsync()
	{
		if (!initialized || disposed)
			return;

		if (inFlight)
		{
			requested = true;
			return;
		}

		inFlight = true;
		try
		{
			do
			{
				requested = false;
				await RenderAndPresentAsync();

				// Always yield to the host dispatcher each iteration. Transferring a frame awaits
				// an interop round-trip, but a suppressed (byte-identical) frame performs no
				// transfer; without this yield a continuous render loop over a static scene would
				// spin synchronously and starve the dispatcher, preventing input, metrics and even
				// disposal from being processed (so the loop could not stop).
				await Task.Yield();
			}
			while ((EnableRenderLoop || requested) && !disposed && initialized);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine("SkiaSharp bridged render error: " + ex);
		}
		finally
		{
			inFlight = false;
		}
	}

	private async Task RenderAndPresentAsync()
	{
		if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0)
			return;

		var info = EnsureBuffer(out var unscaledSize);
		if (info.Width <= 0 || info.Height <= 0)
			return;

		var userVisibleSize = IgnorePixelScaling ? unscaledSize : info.Size;

		using (var surface = SKSurface.Create(info, pixelsHandle.AddrOfPinnedObject(), info.RowBytes))
		{
			if (surface is null)
				return;

			if (IgnorePixelScaling)
			{
				var canvas = surface.Canvas;
				canvas.Scale((float)dpi);
				canvas.Save();
			}

			PaintFrame(surface, info.WithSize(userVisibleSize), info);
		}

		var format = Host.ResolveTransferFormat(hostKind, TransferFormat, options);
		var quality = Quality ?? options.Quality;

		// Produce the transfer payload OFF the dispatcher. On Blazor Hybrid the render loop runs
		// on the UI thread, so encoding (or copying a raw buffer for Put) here would stutter the
		// app. The frame buffer is stable while we do this because backpressure prevents the next
		// frame from starting (and therefore from reallocating/overwriting the buffer) until the
		// present below completes.
		var bufferPtr = pixelsHandle.AddrOfPinnedObject();
		var frameInfo = info;
		var frame = await Task.Run(() =>
		{
			using var image = SKImage.FromPixels(frameInfo, bufferPtr, frameInfo.RowBytes);
			return FrameProducer.Produce(image, format, quality);
		});

		if (frame.Length == 0)
			return;

		if (lastFrame != null && lastFrame.AsSpan().SequenceEqual(frame))
			return;
		lastFrame = frame;

		if (bridge != null && !disposed)
			await bridge.PresentAsync(frame, info.Width, info.Height, FormatToken(format), isGL);
	}

	private void PaintFrame(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
	{
		if (!isGL)
		{
			paint(surface, info, rawInfo, null, default);
			return;
		}

		// Server-side drawing is CPU raster; present into the WebGL-backed canvas. The render
		// target is a lightweight placeholder so the GL event args shape is preserved.
		using var placeholder = new GRBackendRenderTarget(
			rawInfo.Width,
			rawInfo.Height,
			0,
			0,
			new GRGlFramebufferInfo(0, rawInfo.ColorType.ToGlSizedFormat()));

		paint(surface, info, rawInfo, placeholder, GRSurfaceOrigin.TopLeft);
	}

	private SKImageInfo EnsureBuffer(out SKSizeI unscaledSize)
	{
		unscaledSize = SKSizeI.Empty;

		var w = canvasSize.Width;
		var h = canvasSize.Height;
		if (!IsPositive(w) || !IsPositive(h))
			return SKImageInfo.Empty;

		unscaledSize = new SKSizeI((int)w, (int)h);
		var scaled = new SKSizeI((int)(w * dpi), (int)(h * dpi));
		var info = new SKImageInfo(scaled.Width, scaled.Height, FrameProducer.RgbaColorType, SKAlphaType.Premul);

		if (pixels == null || pixelSize.Width != info.Width || pixelSize.Height != info.Height)
		{
			FreeBuffer();
			pixels = new byte[info.BytesSize];
			pixelsHandle = System.Runtime.InteropServices.GCHandle.Alloc(pixels, System.Runtime.InteropServices.GCHandleType.Pinned);
			pixelSize = info.Size;
		}

		return info;

		static bool IsPositive(double value) =>
			!double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
	}

	private void FreeBuffer()
	{
		if (pixels != null)
		{
			pixelsHandle.Free();
			pixels = null;
		}
	}

	private static string FormatToken(SKBlazorTransferFormat format) => format switch
	{
		SKBlazorTransferFormat.Png => "png",
		SKBlazorTransferFormat.Jpeg => "jpeg",
		_ => "put",
	};

	public async ValueTask DisposeAsync()
	{
		disposed = true;

		if (bridge != null)
		{
			await bridge.DisposeAsync();
			bridge = null;
		}

		selfRef?.Dispose();
		selfRef = null;

		FreeBuffer();
	}
}

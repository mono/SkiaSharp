using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SkiaSharp.Views.Blazor.Internal;

namespace SkiaSharp.Views.Blazor
{
#if !NET9_0_OR_GREATER
	[SupportedOSPlatform("browser")]
#endif
	public partial class SKCanvasView : IDisposable
	{
		private SKHtmlCanvasInterop interop = null!;
		private SizeWatcherInterop sizeWatcher = null!;
		private DpiWatcherInterop dpiWatcher = null!;
		private ElementReference htmlCanvas;

		private SKSizeI pixelSize;
		private byte[]? pixels;
		private GCHandle pixelsHandle;
		private bool ignorePixelScaling;
		private double dpi;
		private SKSize canvasSize;
		private bool enableRenderLoop;

		[Inject]
		IJSRuntime JS { get; set; } = null!;

#if NET9_0_OR_GREATER
		[Inject]
		IServiceProvider Services { get; set; } = null!;

		private SKBlazorHostKind hostKind;
		private bool isBridged;
		private SKBlazorBridgedRenderer? renderer;

		/// <summary>
		/// The frame transfer format to use when this view runs in a bridged host (Blazor Server,
		/// Blazor Hybrid or static SSR). When <see langword="null"/> the global option or a host
		/// default is used. Ignored in Blazor WebAssembly.
		/// </summary>
		[Parameter]
		public SKBlazorTransferFormat? TransferFormat { get; set; }

		/// <summary>
		/// The JPEG quality (0-100) used when the resolved <see cref="TransferFormat"/> is
		/// <see cref="SKBlazorTransferFormat.Jpeg"/>. When <see langword="null"/> the global option
		/// value is used. Ignored in Blazor WebAssembly.
		/// </summary>
		[Parameter]
		public int? Quality { get; set; }
#endif

		[Parameter]
		public Action<SKPaintSurfaceEventArgs>? OnPaintSurface { get; set; }

		[Parameter]
		public bool EnableRenderLoop
		{
			get => enableRenderLoop;
			set
			{
				if (enableRenderLoop != value)
				{
					enableRenderLoop = value;
					Invalidate();
				}
			}
		}

		[Parameter]
		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				if (ignorePixelScaling != value)
				{
					ignorePixelScaling = value;
					Invalidate();
				}
			}
		}

		[Parameter(CaptureUnmatchedValues = true)]
		public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

		public double Dpi
		{
			get
			{
#if NET9_0_OR_GREATER
				if (isBridged && renderer != null)
					return renderer.Dpi;
#endif
				return dpi;
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (!firstRender)
				return;

#if NET9_0_OR_GREATER
			hostKind = SKBlazorHost.Resolve(RendererInfo.Name);
			isBridged = SKBlazorHost.IsBridged(hostKind);
			if (isBridged)
			{
				var options = (Services?.GetService(typeof(SKBlazorOptions)) as SKBlazorOptions) ?? SKBlazorOptions.Default;
				renderer = new SKBlazorBridgedRenderer(JS, isGL: false, hostKind, options, PaintBridgedFrame)
				{
					EnableRenderLoop = enableRenderLoop,
					IgnorePixelScaling = ignorePixelScaling,
					TransferFormat = TransferFormat,
					Quality = Quality,
				};
				await renderer.InitializeAsync(htmlCanvas);
				return;
			}
#endif

			if (OperatingSystem.IsBrowser())
			{
				interop = await SKHtmlCanvasInterop.ImportAsync(JS, htmlCanvas, OnRenderFrame);
				interop.InitRaster();

				sizeWatcher = await SizeWatcherInterop.ImportAsync(JS, htmlCanvas, OnSizeChanged);
				dpiWatcher = await DpiWatcherInterop.ImportAsync(JS, OnDpiChanged);
			}
		}

		public void Invalidate()
		{
#if NET9_0_OR_GREATER
			if (isBridged)
			{
				if (renderer != null)
				{
					renderer.EnableRenderLoop = enableRenderLoop;
					renderer.IgnorePixelScaling = ignorePixelScaling;
					renderer.TransferFormat = TransferFormat;
					renderer.Quality = Quality;
					renderer.Invalidate();
				}
				return;
			}
#endif

			if (!OperatingSystem.IsBrowser())
				return;

			if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0)
				return;

			interop.RequestAnimationFrame(EnableRenderLoop, (int)(canvasSize.Width * dpi), (int)(canvasSize.Height * dpi));
		}

#if NET9_0_OR_GREATER
		private void PaintBridgedFrame(SKSurface surface, SKImageInfo userInfo, SKImageInfo rawInfo) =>
			OnPaintSurface?.Invoke(new SKPaintSurfaceEventArgs(surface, userInfo, rawInfo));
#endif

		[SupportedOSPlatform("browser")]
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

				OnPaintSurface?.Invoke(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));
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

			static bool IsPositive(double value)
			{
				return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
			}
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

		public void Dispose()
		{
#if NET9_0_OR_GREATER
			if (renderer != null)
			{
				_ = renderer.DisposeAsync();
				renderer = null;
			}
#endif

			if (OperatingSystem.IsBrowser())
			{
				dpiWatcher?.Unsubscribe(OnDpiChanged);
				sizeWatcher?.Dispose();
				interop?.Dispose();
			}

			FreeBitmap();
		}
	}
}
